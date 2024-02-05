using Firebase.Database;
using Firebase.Extensions;
using QFSW.QC;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Axe : MonoBehaviour
{
    public static Vector3 position;
    public LayerMask playerMask;
    public GameObject axeAirHitSound;
    public GameObject axeWoodHitSound;
    public GameObject resourceNotification;
    bool readyToHit = false;
    public float offset;
    public float distance;
    bool soundPlayed = false;
    public float liftedFloat;
    public ServerRpcs serverRpcs;
    public static Transform notifications;
    public float hitDistance = 5f;
    public LayerMask hitMask;
    public LayerMask treeMask;
    DatabaseReference dbReference;
    RaycastHit hit;
    public float cameraOffset = 1f;
    
    void Start()
    {
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            this.enabled = false;
            return;
        }
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        serverRpcs = GameObject.Find("rpcs").GetComponent<ServerRpcs>();

        position = transform.position;
        if (!transform.parent.gameObject.transform.parent.GetComponent<NetworkObject>().IsOwner)
        {
            gameObject.GetComponent<Axe>().enabled = false;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(HitTimer());
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0) && !QuantumConsole.gamePaused && PublicVariables.inGame && readyToHit && !Inventory.inventoryShowing && !CraftingMenu.craftinShowing)
        {

            HitAxe();
            Debug.Log("Hit with axe: " + transform.parent.gameObject.transform.parent.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    void CheckIfHitSomething()
    {

        if (Physics.Raycast(Camera.main.transform.position - Camera.main.transform.forward * cameraOffset, Camera.main.transform.forward, out hit, hitDistance, hitMask))
        {
            Debug.Log("Hit " + hit.transform.gameObject.layer);
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("doorway"))
            {

                int id = hit.transform.gameObject.transform.parent.GetComponent<building>().id;
                string type = hit.transform.gameObject.transform.parent.GetComponent<building>().buildingType;

                dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogException(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        int health = int.Parse(snapshot.Child("health").Value.ToString());
                        Damage(health, id, type);

                    }
                });
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("door"))
            {
                int id = hit.transform.parent.GetComponent<building>().id;
                string type = hit.transform.parent.GetComponent<building>().buildingType;

                dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogException(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        int health = int.Parse(snapshot.Child("health").Value.ToString());
                        Damage(health, id, type);

                    }
                });
            }
            else
            {
                int id = hit.transform.gameObject.GetComponent<building>().id;
                string type = hit.transform.gameObject.GetComponent<building>().buildingType;
                dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogException(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        int health = int.Parse(snapshot.Child("health").Value.ToString());
                        Damage(health, id, type);

                    }
                });
            }

        }
    }
    void CheckIfHitPlayer()
    {
        RaycastHit playerHit;
        if (Physics.Raycast(Camera.main.transform.position - Camera.main.transform.forward * cameraOffset, Camera.main.transform.forward, out playerHit, hitDistance, playerMask))
        {
            if (!playerHit.transform.root.gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                if (playerHit.transform.gameObject.tag == "head")
                {
                    Debug.Log("hit player head");
                    PublicVariables.serverRpcs.HitPlayerServerRpc(playerHit.transform.root.gameObject.GetComponent<NetworkObject>().OwnerClientId, 40, true);

                }
                else
                {
                    Debug.Log("hit player");
                    PublicVariables.serverRpcs.HitPlayerServerRpc(playerHit.transform.gameObject.GetComponent<NetworkObject>().OwnerClientId, 15, false);
                }

            }



        }
    }
    void Damage(int health, int id, string type)
    {
        health -= 10;
        /*
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("doorway"))
        {
            hit.transform.gameObject.transform.parent.GetComponent<ObjectHealth>().health = health;
        }
        else
        {
             hit.transform.gameObject.GetComponent<ObjectHealth>().health = health;
        }
        */
        PublicVariables.serverRpcs.DamageBuildingServerRpc(id, 10, hit.transform.gameObject.layer);
        dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).Child("health").SetValueAsync(health);
        if (!hit.transform.gameObject.IsDestroyed())
        {
            Debug.Log("Hit " + hit.transform.gameObject.name + " to health: " + health);

        }

    }
    private void HitAxe()
    {
        if (readyToHit)
        {
            RaycastHit hitt;
            CheckIfHitSomething();
            CheckIfHitPlayer();
            PublicVariables.isHitting = true;
            PublicVariables.myAxeAnimator.SetTrigger("Hit");
            readyToHit = false;
            if (Physics.Raycast(Camera.main.transform.position - Camera.main.transform.forward * cameraOffset, Camera.main.transform.forward, out hitt, hitDistance, treeMask))
            {
                GameObject tree = hitt.transform.gameObject;
                HitWood(tree.GetComponent<tree>().id, tree);

                PublicVariables.serverRpcs.HitWoodServerRpc(PublicVariables.myOwnerClientId, "axeWood", transform.position);

            }
            else
            {
                PublicVariables.serverRpcs.HitWoodServerRpc(PublicVariables.myOwnerClientId, "axeWhoosh", transform.position);
            }









            StartCoroutine(HitTimer());
        }
    }
    void HitWood(int id, GameObject tree)
    {
        dbReference.Child("server").Child("trees").Child(id.ToString()).Child("hitCount").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("tree hitcount fetced");

                int hitCount = int.Parse(snapshot.Value.ToString());
                hitCount -= 1;
                SetHitCount(id, tree, hitCount);


            }
        });

    }
    void SetHitCount(int id, GameObject tree, int hitCount)
    {
        dbReference.Child("server").Child("trees").Child(id.ToString()).Child("hitCount").SetValueAsync(hitCount);
        PublicVariables.serverRpcs.TreeDataWrittenServerRpc(id, hitCount);

        tree.GetComponent<tree>().hitCount = hitCount;
        //Hit(hitCount,tree, id);
        UpdateWood();
    }
    void FetchWood()
    {
        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("woodCount").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {


                //UpdateWood(task.Result);
            }
        });
    }
    void UpdateWood()
    {
        int totalWood = 0;
        int chopAmount = UnityEngine.Random.Range(25, 40);
        //int woodCount = int.Parse(snapshot.Value.ToString());
        // woodCount += chopAmount;



        //await dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("woodCount").SetValueAsync(woodCount);
        // PublicVariables.woodCount = woodCount;
        PublicVariables.myInventory.AddItem(chopAmount, "Wood");
        GameObject sound = Instantiate(axeWoodHitSound);
        sound.GetComponent<AudioSource>().spatialBlend = 0f;


        GameObject noti = Instantiate(resourceNotification, notifications);

        foreach (var slot in Inventory.inventorySlots)
        {
            if (slot.GetComponent<InventoryObject>().icon != null)
            {
                Debug.Log(slot.GetComponent<InventoryObject>().slotIndex + " slot pos icon not null");

                if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                {
                    Debug.Log(slot.GetComponent<InventoryObject>().slotIndex + " slot pos has wood");
                    totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                }
            }
        }
        noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "+" + chopAmount + " (" + totalWood + ")";
    }
    void Hit(int hitCount, GameObject tree, int id)
    {
        hitCount--;
        dbReference.Child("server").Child("trees").Child(id.ToString()).Child("hitCount").SetValueAsync(hitCount);
        GameObject sound = Instantiate(axeWoodHitSound);
        sound.GetComponent<AudioSource>().spatialBlend = 0f;
        int chopAmount = UnityEngine.Random.Range(25, 40);
        int wood = PublicVariables.woodCount;
        wood += chopAmount;
        PublicVariables.woodCount = wood;
        GameObject noti = Instantiate(resourceNotification, notifications);
        noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "+" + chopAmount + " (" + PublicVariables.woodCount + ")";

        tree.GetComponent<tree>().hitCount = hitCount;

    }


    IEnumerator HitTimer()
    {
        yield return new WaitForSeconds(0.6f);
        StopAllCoroutines();
        readyToHit = true;
        soundPlayed = false;
        PublicVariables.isHitting = false;

    }
}
