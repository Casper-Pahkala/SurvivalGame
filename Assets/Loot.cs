using Firebase.Database;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public GameObject lootbagUI;
    public GameObject lootchestUI;
    public static bool lootOpen = false;
    public GameObject lootBag;
    public static GameObject currentLootBag;
    public float distance = 5f;
    public LayerMask lootMask;
    public bool canLoot = false;
    public bool spawned = false;
    public GameObject lootText;
    public class lootBagitemData
    {
        public string name;
        public int count;
        public int slot;
        public int lootBagId;
    }
    public static List<lootBagitemData> lootBagitems = new List<lootBagitemData>();
    public static List<lootBagitemData> chestitems = new List<lootBagitemData>();

    void Start()
    {

        FirebaseDatabase.DefaultInstance
       .GetReference("server").Child("lootbags")
       .ValueChanged += HandleLootbagsData;

        FirebaseDatabase.DefaultInstance
       .GetReference("server").Child("chests")
       .ValueChanged += HandleChestsData;


        lootchestUI.SetActive(false);
    }
    void HandleLootbagsData(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        lootBagitems = new List<lootBagitemData>();
        foreach (var child in snapshot.Children)
        {
            int lootBagId = int.Parse(child.Key);
            DataSnapshot itemsSnapshot = child.Child("items");
            foreach (var item in itemsSnapshot.Children)
            {
                int count = int.Parse(item.Child("count").Value.ToString());
                int slotIndex = int.Parse(item.Key);
                string name = item.Child("name").Value.ToString();

                lootBagitemData data = new lootBagitemData
                {
                    name = name,
                    count = count,
                    slot = slotIndex,
                    lootBagId = lootBagId,
                };
                lootBagitems.Add(data);
            }


        }

        if (lootOpen)
        {
            EmptyEverySlotInLoot();
            foreach (var item in lootBagitems)
            {
                if (item.lootBagId == currentLootBag.GetComponent<lootbag>().lootBagId)
                {
                    foreach (var slot in Inventory.inventorySlots)
                    {
                        InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                        if (item.slot == inventoryObject.slotIndex && inventoryObject.icon == null)
                        {

                            string itemName = item.name;
                            GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
                            GameObject fab = Instantiate(prefab);

                            fab.transform.parent = inventoryObject.transform;
                            fab.transform.localPosition = Vector3.zero;
                            fab.transform.localScale = new Vector3(1, 1, 1);
                            int count = item.count;
                            fab.GetComponent<Item>().count = count;
                            fab.name = itemName;
                            if (count > 1)
                            {
                                fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + count;
                            }
                            inventoryObject.icon = fab;




                        }
                    }
                }


            }
        }


    }
    void HandleChestsData(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        chestitems = new List<lootBagitemData>();
        foreach (var child in snapshot.Children)
        {
            int lootBagId = int.Parse(child.Key);
            DataSnapshot itemsSnapshot = child.Child("items");
            foreach (var item in itemsSnapshot.Children)
            {
                int count = int.Parse(item.Child("count").Value.ToString());
                int slotIndex = int.Parse(item.Key);
                string name = item.Child("name").Value.ToString();

                lootBagitemData data = new lootBagitemData
                {
                    name = name,
                    count = count,
                    slot = slotIndex,
                    lootBagId = lootBagId,
                };
                chestitems.Add(data);
            }


        }

        if (lootOpen)
        {
            EmptyEverySlotInLoot();
            foreach (var item in chestitems)
            {
                if (item.lootBagId == currentLootBag.GetComponent<lootbag>().lootBagId)
                {
                    foreach (var slot in Inventory.inventorySlots)
                    {
                        InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                        if (item.slot == inventoryObject.slotIndex && inventoryObject.icon == null)
                        {

                            string itemName = item.name;
                            GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
                            GameObject fab = Instantiate(prefab);

                            fab.transform.parent = inventoryObject.transform;
                            fab.transform.localPosition = Vector3.zero;
                            fab.transform.localScale = new Vector3(1, 1, 1);
                            int count = item.count;
                            fab.GetComponent<Item>().count = count;
                            fab.name = itemName;
                            if (count > 1)
                            {
                                fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + count;
                            }
                            inventoryObject.icon = fab;




                        }
                    }
                }


            }
        }


    }


    void Update()
    {

        if (lootOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            lootOpen = false;
            lootbagUI.SetActive(false);
            Debug.Log("closed");
            lootchestUI.SetActive(false);
            Inventory.inventoryShowing = false;
            if (currentLootBag.GetComponent<lootbag>().bagEmpty)
            {
                if (!currentLootBag.IsDestroyed())
                {
                    Destroy(currentLootBag);

                }
            }

        }
        if (lootOpen && Input.GetKeyDown(KeyCode.Tab))
        {
            lootOpen = false;
            lootbagUI.SetActive(false);
            Debug.Log("closed");
            lootchestUI.SetActive(false);

            Inventory.inventoryShowing = false;
            if (currentLootBag.GetComponent<lootbag>().bagEmpty)
            {
                if (!currentLootBag.IsDestroyed())
                {
                    Destroy(currentLootBag);

                }
            }
        }
        RaycastHit hit;
        if (Camera.main == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {

            if (lootOpen)
            {

                Debug.Log("chest closed");
                lootOpen = false;
                lootbagUI.SetActive(false);
                lootchestUI.SetActive(false);

                Inventory.inventoryShowing = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (currentLootBag.GetComponent<lootbag>().bagEmpty)
                {
                    if (!currentLootBag.IsDestroyed())
                    {
                        Destroy(currentLootBag);

                    }
                }
                return;
            }
        }
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, distance, lootMask))
        {
            lootText.SetActive(true);
            canLoot = true;
            if (Input.GetKeyDown(KeyCode.E))
            {
                
                lootOpen = !lootOpen;
                currentLootBag = hit.transform.gameObject;
                if (lootOpen)
                {
                    EmptyEverySlotInLoot();
                    foreach (var item in lootBagitems)
                    {
                        if (item.lootBagId == hit.transform.gameObject.GetComponent<lootbag>().lootBagId)
                        {
                            foreach (var slot in Inventory.inventorySlots)
                            {
                                InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                                Debug.Log("every: " + inventoryObject.slotIndex);
                                if (item.slot == inventoryObject.slotIndex && inventoryObject.icon == null)
                                {

                                    Debug.Log("this " + inventoryObject.slotIndex);
                                    string itemName = item.name;
                                    GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
                                    GameObject fab = Instantiate(prefab);

                                    fab.transform.parent = inventoryObject.transform;
                                    fab.transform.localPosition = Vector3.zero;
                                    fab.transform.localScale = new Vector3(1, 1, 1);
                                    int count = item.count;
                                    fab.GetComponent<Item>().count = count;
                                    fab.name = itemName;
                                    if (count > 1)
                                    {
                                        fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + count;
                                    }
                                    inventoryObject.icon = fab;
                                }
                            }
                        }
                    }

                    foreach (var item in chestitems)
                    {
                        if (item.lootBagId == hit.transform.gameObject.GetComponent<lootbag>().lootBagId)
                        {
                            foreach (var slot in Inventory.inventorySlots)
                            {
                                InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                                Debug.Log("every: " + inventoryObject.slotIndex);
                                if (item.slot == inventoryObject.slotIndex && inventoryObject.icon == null)
                                {

                                    Debug.Log("this " + inventoryObject.slotIndex);
                                    string itemName = item.name;
                                    GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
                                    GameObject fab = Instantiate(prefab);

                                    fab.transform.parent = inventoryObject.transform;
                                    fab.transform.localPosition = Vector3.zero;
                                    fab.transform.localScale = new Vector3(1, 1, 1);
                                    int count = item.count;
                                    fab.GetComponent<Item>().count = count;
                                    fab.name = itemName;
                                    if (count > 1)
                                    {
                                        fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + count;
                                    }
                                    inventoryObject.icon = fab;
                                }
                            }
                        }
                    }
                    Debug.Log("5");

                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("lootbag")){
                        lootbagUI.SetActive(true);
                        Debug.Log("active");

                    }
                    if(hit.transform.gameObject.layer == 19)
                    {
                        Debug.Log("chest open");
                        lootchestUI.SetActive(true);

                    }
                    Inventory.inventoryShowing = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                }
                
            }
        }
        else
        {
            canLoot = false;
            lootText.SetActive(false);

        }
    }
    void EmptyEverySlotInLoot()
    {
        foreach (var slot in Inventory.inventorySlots)
        {

            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
            if (inventoryObject.slotIndex > 100)
            {
                if (inventoryObject.icon != null)
                {

                    Item item = inventoryObject.icon.GetComponent<Item>();
                    Destroy(item.gameObject);
                    inventoryObject.icon = null;

                }
            }



        }
    }




}
