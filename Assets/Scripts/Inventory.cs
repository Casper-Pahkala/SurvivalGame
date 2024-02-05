using Firebase.Database;
using QFSW.QC;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryObject;
    public static bool inventoryShowing = false;
    public static List<GameObject> inventorySlots = new List<GameObject>();
    public bool dataFetched = false;

    public GameObject line1;
    public GameObject line2;
    public GameObject line3;
    public GameObject line4;
    public GameObject background;

    public bool closedLoot = false;

    public GameObject lootInventory;
    bool lootOpen = false;

    public class ItemData
    {
        public string name;
        public int count;
        public int slot;

    }
    public List<ItemData> playerInventoryList = new List<ItemData>();
    void Start()
    {
        playerInventoryList = new List<ItemData>();
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void OpenLoot()
    {
        lootOpen = !lootOpen;
        lootInventory.SetActive(lootOpen);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            OpenLoot();
        }
        if (inventorySlots.Count > 29 && !dataFetched)
        {

            dataFetched = true;
            DatabaseManager.FetchInventoryData();
        }
        if (inventorySlots.Count > 59 && !closedLoot)
        {

            closedLoot = true;
            lootInventory.SetActive(false);
        }
        if (QuantumConsole.gamePaused)
        {
            line1.SetActive(false);
            line2.SetActive(false);
            line3.SetActive(false);
            line4.SetActive(false);
            background.SetActive(false);
            inventoryShowing = false;
            return;
        }


        if (Input.GetKeyDown(KeyCode.Tab) && !CraftingMenu.craftinShowing)
        {

            ToggleInventory();
        }
        if (!inventoryShowing)
        {
            line1.SetActive(false);
            line2.SetActive(false);
            line3.SetActive(false);
            line4.SetActive(false);
            background.SetActive(false);
            transform.GetChild(0).GetChild(1).GetComponent<VerticalLayoutGroup>().padding.top = 500;
            transform.GetChild(0).GetChild(1).GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.LowerCenter;
        }
        else
        {
            line1.SetActive(true);
            line2.SetActive(true);
            line3.SetActive(true);
            line4.SetActive(true);
            background.SetActive(true);
            transform.GetChild(0).GetChild(1).GetComponent<VerticalLayoutGroup>().padding.top = 0;
            transform.GetChild(0).GetChild(1).GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        }





    }

    void ToggleInventory()
    {
        inventoryShowing = !inventoryShowing;
        if (inventoryShowing)
        {
            line1.SetActive(true);
            line2.SetActive(true);
            line3.SetActive(true);
            line4.SetActive(true);
            background.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            line1.SetActive(false);
            line2.SetActive(false);
            line3.SetActive(false);
            line4.SetActive(false);
            background.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    [Command]
    public void AddItem(int itemCount, string itemName)
    {

        inventorySlots.Sort((p1, p2) => p1.GetComponent<InventoryObject>().slotIndex.CompareTo(p2.GetComponent<InventoryObject>().slotIndex));
        GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
        if (prefab == null)
        {
            Debug.Log("Item null in path: " + "Assets/Icons/" + itemName + ".prefab");
            return;
        }
        bool added = false;
        int leftToAddCount;
        leftToAddCount = itemCount;
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        foreach (var slot in inventorySlots)
        {
            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
            if (inventoryObject.icon != null && !added)
            {
                int currentCount = inventoryObject.icon.GetComponent<Item>().count;
                int maxCount = inventoryObject.icon.GetComponent<Item>().maxCount;

                if (inventoryObject.icon.name == itemName && !added)
                {
                    if (leftToAddCount + currentCount > maxCount)
                    {
                        int addCount = maxCount - currentCount;
                        leftToAddCount -= addCount;
                        inventoryObject.icon.GetComponent<Item>().count = maxCount;

                        inventoryObject.icon.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + maxCount;


                        ItemData data = new ItemData
                        {
                            name = itemName,
                            count = maxCount,
                            slot = inventoryObject.slotIndex,
                        };
                        string json = JsonUtility.ToJson(data);

                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);
                        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).SetRawJsonValueAsync(json);
                        if (leftToAddCount <= 0)
                        {
                            added = true;
                        }



                    }
                    else
                    {
                        added = true;

                        int addCount = leftToAddCount + currentCount;
                        inventoryObject.icon.GetComponent<Item>().count = addCount;

                        inventoryObject.icon.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + addCount;


                        ItemData data = new ItemData
                        {
                            name = itemName,
                            count = addCount,
                            slot = inventoryObject.slotIndex,
                        };

                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);

                        string json = JsonUtility.ToJson(data);
                        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).SetRawJsonValueAsync(json);

                    }


                }
            }
            else if (!added)
            {
                added = true;
                GameObject iconPrefab = Instantiate(prefab);
                iconPrefab.transform.parent = slot.transform;
                iconPrefab.name = itemName;
                iconPrefab.transform.localPosition = Vector3.zero;
                iconPrefab.transform.localScale = new Vector3(1, 1, 1);
                iconPrefab.GetComponent<Item>().count = leftToAddCount;
                if (itemCount > 1)
                {
                    iconPrefab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + leftToAddCount;
                }
                inventoryObject.icon = iconPrefab;

                ItemData data = new ItemData
                {
                    name = itemName,
                    count = leftToAddCount,
                    slot = inventoryObject.slotIndex,
                };
                string json = JsonUtility.ToJson(data);

                PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);

                dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).SetRawJsonValueAsync(json);
            }
        }
    }

    public void ChangeItemPosition(int firstIndex, int secondIndex)
    {

        inventorySlots.Sort((p1, p2) => p1.GetComponent<InventoryObject>().slotIndex.CompareTo(p2.GetComponent<InventoryObject>().slotIndex));
        InventoryObject first = null;
        InventoryObject second = null;
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        string firstName = "";
        string secondName = "asd";
        GameObject firstSlot = null;
        GameObject secondSlot = null;
        foreach (var slot in inventorySlots)
        {
            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();

            if (firstIndex == inventoryObject.slotIndex)
            {
                first = inventoryObject;
                if (inventoryObject.icon != null)
                {
                    firstName = inventoryObject.icon.name;
                }
                firstSlot = slot;
            }
            if (secondIndex == inventoryObject.slotIndex)
            {
                second = inventoryObject;
                if (inventoryObject.icon != null)
                {
                    secondName = inventoryObject.icon.name;
                }
                secondSlot = slot;
            }
        }
        if (first == null) return;
        if (second == null) return;

        if (firstName == secondName)
        {

            bool added = false;


            int itemCount = first.icon.GetComponent<Item>().count;
            int leftToAddCount = itemCount;
            int currentCount = second.icon.GetComponent<Item>().count;
            int maxCount = second.icon.GetComponent<Item>().maxCount;
            if (maxCount < 2)
            {
                first.icon.transform.parent = first.transform;
                first.icon.transform.localPosition = Vector3.zero;
                return;

            }

            if (itemCount + currentCount > maxCount)
            {
                int addCount = maxCount - currentCount;
                leftToAddCount -= addCount;
                second.icon.GetComponent<Item>().count = maxCount;
                if (itemCount > 1)
                {
                    second.icon.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + maxCount;
                }

                ItemData data = new ItemData
                {
                    name = secondName,
                    count = maxCount,
                    slot = second.slotIndex,
                };
                string json = JsonUtility.ToJson(data);
                Destroy(first.icon);
                first.icon = null;
                if (first.slotIndex > 100 && first.slotIndex < 1000)
                {

                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }else if (first.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                else
                {
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                if (second.slotIndex > 100 && second.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else if (second.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else
                {

                    PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);
                }
                foreach (var itemData in playerInventoryList)
                {
                    if (itemData.slot == first.slotIndex)
                    {

                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, itemData.name, itemData.count, itemData.slot, false);
                        playerInventoryList.Remove(itemData);
                        break;
                    }
                }


                foreach (var slot in inventorySlots)
                {
                    InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                    if (inventoryObject.icon == null && !added)
                    {


                        added = true;
                        GameObject prefab = (GameObject)Resources.Load("Icons/" + secondName, typeof(GameObject));
                        GameObject fab = Instantiate(prefab);
                        fab.transform.parent = inventoryObject.transform;
                        fab.transform.localPosition = Vector3.zero;
                        fab.transform.localScale = new Vector3(1, 1, 1);
                        fab.name = secondName;
                        inventoryObject.icon = fab;
                        inventoryObject.icon.GetComponent<Item>().count = leftToAddCount;
                        if (leftToAddCount > 1)
                        {
                            inventoryObject.icon.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + leftToAddCount;
                        }
                        ItemData dataa = new ItemData
                        {
                            name = secondName,
                            count = leftToAddCount,
                            slot = inventoryObject.slotIndex,
                        };
                        string jsonn = JsonUtility.ToJson(dataa);

                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, dataa.name, dataa.count, dataa.slot, true);
                        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).SetRawJsonValueAsync(jsonn);
                    }
                }
            }
            else
            {

                int addCount = itemCount + currentCount;
                second.icon.GetComponent<Item>().count = addCount;
                if (addCount > 1)
                {
                    second.icon.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + addCount;
                }
                ItemData data = new ItemData
                {
                    name = secondName,
                    count = addCount,
                    slot = second.slotIndex,
                };
                string json = JsonUtility.ToJson(data);
                Destroy(first.icon);
                first.icon = null;
                if (first.slotIndex > 100 && first.slotIndex < 1000)
                {

                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                else if (first.slotIndex > 1000)
                {

                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                else
                {
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                if (second.slotIndex > 100 && second.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else if (second.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else
                {

                    PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);
                }

                bool removed = false;
                foreach (var itemData in playerInventoryList)
                {
                    if (itemData.slot == first.slotIndex && !removed)
                    {
                        removed = true;

                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, itemData.name, itemData.count, itemData.slot, false);

                        break;
                    }
                }


            }



        }
        else
        {

            if (first.icon != null && second.icon != null)
            {
                GameObject firstItem = first.icon;
                GameObject secondItem = second.icon;
                first.icon = secondItem;
                second.icon = firstItem;


                firstItem.transform.parent = secondSlot.transform;
                firstItem.transform.localPosition = Vector3.zero;

                secondItem.transform.parent = firstSlot.transform;
                secondItem.transform.localPosition = Vector3.zero;
                ItemData data = new ItemData
                {
                    name = secondName,
                    count = secondItem.GetComponent<Item>().count,
                    slot = first.slotIndex,
                };
                string json = JsonUtility.ToJson(data);

                ItemData data2 = new ItemData
                {
                    name = firstName,
                    count = firstItem.GetComponent<Item>().count,
                    slot = second.slotIndex,
                };
                string json2 = JsonUtility.ToJson(data2);
                if (first.slotIndex > 100 && first.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else if (first.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else
                {
                    PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(first.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                if (second.slotIndex > 100 && second.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json2);

                }
                if (second.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json2);

                }
                else
                {
                    PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data2.name, data2.count, data2.slot, true);
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json2);
                }





            }
            else if (second.icon == null && first.icon != null)
            {
                Debug.Log("second null but not first ");
                GameObject firstItem = first.icon;
                second.icon = firstItem;

                first.icon = null;

                firstItem.transform.parent = secondSlot.transform;
                firstItem.transform.localPosition = Vector3.zero;

                ItemData data = new ItemData
                {
                    name = firstName,
                    count = firstItem.GetComponent<Item>().count,
                    slot = second.slotIndex,
                };
                string json = JsonUtility.ToJson(data);
                
                if (second.slotIndex > 100 && second.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else if (second.slotIndex > 1000)
                {
                    Debug.Log("added to "+ Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString());
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                else
                {

                    PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, data.name, data.count, data.slot, true);
                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(second.slotIndex.ToString()).SetRawJsonValueAsync(json);

                }
                if (first.slotIndex > 100 && first.slotIndex < 1000)
                {
                    dbReference.Child("server").Child("lootbags").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                else if (first.slotIndex > 1000)
                {
                    dbReference.Child("server").Child("chests").Child(Loot.currentLootBag.GetComponent<lootbag>().lootBagId.ToString()).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();

                }
                else
                {

                    dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(first.slotIndex.ToString()).RemoveValueAsync();
                }

                bool removed = false;
                foreach (var itemData in playerInventoryList)
                {

                    if (itemData.slot == first.slotIndex && !removed)
                    {
                        Debug.Log("removed value" + itemData.name);
                        removed = true;
                        PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, itemData.name, itemData.count, itemData.slot, false);


                        break;
                    }
                }


            }


        }
        if (firstSlot.GetComponent<HotBarItem>() != null)
        {
            firstSlot.GetComponent<HotBarItem>().currentItem = secondName;
        }
        if (secondSlot.GetComponent<HotBarItem>() != null)
        {
            secondSlot.GetComponent<HotBarItem>().currentItem = firstName;
        }


    }

    public void RemoveResource(string itemName, int Count)
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        int leftToRemoveCount = Count;
        bool removed = false;
        foreach (var slot in inventorySlots)
        {
            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
            if (inventoryObject.icon != null)
            {
                Item item = inventoryObject.icon.GetComponent<Item>();
                string resourceName = item.gameObject.name;
                if (resourceName == itemName && !removed)
                {

                    if (item.count - leftToRemoveCount > 0)
                    {

                        removed = true;
                        item.count -= leftToRemoveCount;
                        item.gameObject.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + item.count.ToString();
                        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).Child("count").SetValueAsync(item.count.ToString());
                        foreach (var itemData in playerInventoryList)
                        {
                            if (itemData.slot == inventoryObject.slotIndex)
                            {
                                itemData.count = item.count;
                            }
                        }
                    }
                    else
                    {
                        Destroy(item.gameObject);
                        inventoryObject.icon = null;
                        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(inventoryObject.slotIndex.ToString()).RemoveValueAsync();
                        leftToRemoveCount -= item.count;
                        bool removedd = false;
                        foreach (var itemData in playerInventoryList)
                        {
                            if (itemData.slot == inventoryObject.slotIndex && !removedd)
                            {
                                removedd = true;

                                PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, itemData.name, itemData.count, itemData.slot, false);

                                break;

                            }
                        }
                    }

                }

            }

        }
    }

    public void RemoveSingularItem(int slotIndex)
    {
        bool removed = false;
        foreach (var slot in inventorySlots)
        {

            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
            if (inventoryObject.slotIndex == slotIndex)
            {
                Item item = inventoryObject.icon.GetComponent<Item>();
                Destroy(item.gameObject);
                inventoryObject.icon = null;
            }
        }
    }

    public static void RemoveEveryItem()
    {
        Debug.Log("removed every item");
        foreach (var slot in inventorySlots)
        {

            InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
            if (inventoryObject.icon != null)
            {
                Item item = inventoryObject.icon.GetComponent<Item>();
                Destroy(item.gameObject);
                inventoryObject.icon = null;
                if (slot.GetComponent<HotBarItem>() != null)
                {
                    slot.GetComponent<HotBarItem>().currentItem = "";


                }
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").RemoveValueAsync();
            }


        }
    }

    void AddItemToList(ItemData data)
    {
        foreach (var d in playerInventoryList)
        {
            if (data.slot == d.slot)
            {
                playerInventoryList.Remove(d);
            }
        }
        playerInventoryList.Add(data);
    }


}
