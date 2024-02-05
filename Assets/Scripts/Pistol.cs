using Firebase.Database;
using QFSW.QC;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    public static bool canShoot = true;
    public GameObject shootingSound;
    public LayerMask hitMask;
    public float distance = 200f;
    public Animator animator;
    public bool canAim = true;
    public Animator shootingAnimator;
    public Transform barrelLocation;
    public float shootDelay = 0.3f;
    public int totalAmmo = 0;
    public int clipAmmo = 0;
    public float reloadTime = 2f;
    public int clipMaxAmmo = 12;
    public bool reloading = false;
    bool firstTimeFetched = false;
    private void Start()
    {
        // transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = clipAmmo + " / " + clipMaxAmmo;
    }
    private void Update()
    {
        if (!firstTimeFetched)
        {
            if (Hotbar.currentHotbarItem.GetComponent<InventoryObject>().icon != null)
            {
                firstTimeFetched = true;
                clipAmmo = Hotbar.currentHotbarItem.GetComponent<InventoryObject>().icon.GetComponent<PistolAmmo>().ammoCount;

            }
        }
        if (QuantumConsole.gamePaused || Loot.lootOpen || CraftingMenu.craftinShowing || Inventory.inventoryShowing)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {

            if (canShoot)
            {
                if (clipAmmo > 0)
                {
                    Shoot();
                }
                else
                {
                    ShootEmptyClip();
                }
            }
        }
        if (Input.GetMouseButton(1))
        {
            if (canAim)
            {
                Aim();
            }
        }
        else
        {
            animator.SetBool("isAiming", false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CheckAmmo();
            if (totalAmmo > 0 && clipAmmo!=clipMaxAmmo)
            {
                if (!reloading)
                {
                    Reload();

                }
            }
        }
    }
    void Reload()
    {
        
        reloading = true;

        StartCoroutine(Reloaded());
        Debug.Log("Reloading");
        PublicVariables.serverRpcs.SpawnObjectServerRpc(transform.position, "m1911ReloadSound", Convert.ToInt32(PublicVariables.myOwnerClientId));
        //animator.SetTrigger("reload");

    }
    IEnumerator Reloaded()
    {
        yield return new WaitForSeconds(reloadTime);
        int ammoToReload = 0;
        ammoToReload = clipMaxAmmo - clipAmmo;
        if (totalAmmo >= ammoToReload)
        {
            
        }
        else
        {
            ammoToReload = totalAmmo;
        }
        clipAmmo += ammoToReload;
        PublicVariables.myInventory.RemoveResource("PistolAmmo", ammoToReload);
        canShoot = true;

        Debug.Log("Reloaded");
        reloading = false;
        SetAmmo();
    }
    void SetAmmo()
    {
        Hotbar.currentHotbarItem.transform.GetChild(0).transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = clipAmmo + "";
        Hotbar.currentHotbarItem.GetComponent<InventoryObject>().icon.GetComponent<PistolAmmo>().ammoCount = clipAmmo;
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        string hotbarSlotIndex = Hotbar.currentHotbarItem.GetComponent<InventoryObject>().slotIndex.ToString();
        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(hotbarSlotIndex).Child("ammoCount").SetValueAsync(clipAmmo);
    }
    void ShootEmptyClip()
    {
        canShoot = false;
        StartCoroutine(Delay());
        PublicVariables.serverRpcs.SpawnObjectServerRpc(transform.position, "m1911EmptyClipSound", Convert.ToInt32(PublicVariables.myOwnerClientId));
        PublicVariables.serverRpcs.DestroyObjectsServerRpc("reloadsound");
        if (reloading)
        {
            StopAllCoroutines();
            Debug.Log("stopped reloading");
            reloading = false;
        }

    }

    void Aim()
    {
        animator.SetBool("isAiming", true);
    }

    void CheckAmmo()
    {
        totalAmmo = 0;
        foreach (var slot in Inventory.inventorySlots)
        {
            if (slot.GetComponent<InventoryObject>().icon != null)
            {

                if (slot.GetComponent<InventoryObject>().icon.name == "PistolAmmo")
                {
                    Debug.Log(slot.GetComponent<InventoryObject>().slotIndex + " slot pos has pistol ammo");
                    totalAmmo += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                }
            }
        }
    }

    void Shoot()
    {
        if (reloading)
        {
            StopAllCoroutines();
            Debug.Log("stopped reloading");
            reloading = false;
        }
        StartCoroutine(Delay());
        canShoot = false;
        clipAmmo--;
        PublicVariables.serverRpcs.DestroyObjectsServerRpc("reloadsound");
        PublicVariables.serverRpcs.SpawnObjectServerRpc(transform.position, "m1911ShootingSound", Convert.ToInt32(PublicVariables.myOwnerClientId));
        SetAmmo();
        CheckIfHitSomething();
        shootingAnimator.SetTrigger("shoot");

    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    void CheckIfHitSomething()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, distance, hitMask))
        {
            Debug.Log("Hit " + hit.transform.gameObject.name);
            if (hitMask == LayerMask.GetMask("player"))
            {
                
                    if (hit.transform.gameObject.tag == "head")
                    {
                        Debug.Log("hit player head");
                        PublicVariables.serverRpcs.HitPlayerServerRpc(hit.transform.root.gameObject.GetComponent<NetworkObject>().OwnerClientId, 55, true);

                    }
                    else
                    {
                        Debug.Log("hit player");
                        PublicVariables.serverRpcs.HitPlayerServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().OwnerClientId, 24, false);
                    }
                
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        reloading = false;
    }
    private void OnEnable()
    {
        if (Hotbar.currentHotbarItem)
        {
            clipAmmo = Hotbar.currentHotbarItem.GetComponent<InventoryObject>().icon.GetComponent<PistolAmmo>().ammoCount;

        }
        StartCoroutine(Delay());
        reloading = false;

    }
}
