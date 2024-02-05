using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Inventory;


public class DatabaseManager : MonoBehaviour
{
    public GameObject doorPrefab;
    public GameObject doorwayPrefab;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public List<int> _buildings;
    List<int> newBuildings;
    List<int> newTrees;
    public static bool playerJoined = false;
    public class Wood
    {
        public int woodCount;
        public Wood()
        {
        }

        public Wood(int woodCount)
        {

            this.woodCount = woodCount;

        }
    }
    DatabaseReference dbReference;
    string userID;
    public InputField textInput;
    FirebaseApp app;

    public static bool justNowCreated = true;


    void Start()
    {
        FirebaseDatabase.GetInstance("https://open-world-2d174-default-rtdb.europe-west1.firebasedatabase.app/").SetPersistenceEnabled(false);
        newBuildings = new List<int>();
        newTrees = new List<int>();
        _buildings = new List<int>();
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        userID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log(gameObject.name);

        StartCoroutine(GetName((string name) =>
        {
            textInput.text = name;
        }));
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("buildings").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadStartData(snapshot);
            }
        }
        );

        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("lootbags").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadLootBagsData(snapshot);
            }
        }
        );
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("chests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadChestsData(snapshot);
            }
        }
        );

        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("trees").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadTreesData(snapshot);
            }
        }
        );
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("hemps").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadHempsData(snapshot);
            }
        }
        );



        /*
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("buildings")
        .ValueChanged += HandleValueChanged;
        */
        /*
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("trees")
        .ValueChanged += HandleTreesChanged;
        */

        FirebaseDatabase.DefaultInstance
       .GetReference("users").Child(userID).Child("pos")
       .ValueChanged += HandleUserChanged;






    }
    void LoadHempsData(DataSnapshot snapshot)
    {

        foreach (var childsnapshot in snapshot.Children)
        {
            float x = float.Parse(childsnapshot.Child("x").Value.ToString());
            float y = float.Parse(childsnapshot.Child("y").Value.ToString());
            float z = float.Parse(childsnapshot.Child("z").Value.ToString());
            Vector3 position = new Vector3(x, y, z);
            int id = int.Parse(childsnapshot.Key.ToString());
            
            if (!PublicVariables.hempPositions.Contains(position))
            {
                PublicVariables.hempPositions.Add(position);
            }
        }
    }


    void LoadTreesData(DataSnapshot snapshot)
    {

        foreach (var childsnapshot in snapshot.Children)
        {
            float x = float.Parse(childsnapshot.Child("x").Value.ToString());
            float y = float.Parse(childsnapshot.Child("y").Value.ToString());
            float z = float.Parse(childsnapshot.Child("z").Value.ToString());
            int hitCount = int.Parse(childsnapshot.Child("hitCount").Value.ToString());
            string name = childsnapshot.Child("name").Value.ToString();
            Vector3 position = new Vector3(x, y, z);
            int id = int.Parse(childsnapshot.Key.ToString());
            PublicVariables.Tree data = new PublicVariables.Tree
            {
                id = id,
                position = position,
                hitCount = hitCount,
                name=name,

            };
            if (!PublicVariables.treePositions.Contains(data))
            {
                PublicVariables.treePositions.Add(data);
            }
        }
    }
    void LoadLootBagsData(DataSnapshot snapshot)
    {

        foreach (var childsnapshot in snapshot.Children)
        {
            Debug.Log("reached lootbag ");
            int lootBagId = int.Parse(childsnapshot.Key);
            DataSnapshot itemsSnapshot = childsnapshot.Child("items");
            foreach (var item in itemsSnapshot.Children)
            {
                int count = int.Parse(item.Child("count").Value.ToString());
                int slotIndex = int.Parse(item.Key);
                string name = item.Child("name").Value.ToString();

                Loot.lootBagitemData data = new Loot.lootBagitemData
                {
                    name = name,
                    count = count,
                    slot = slotIndex,
                    lootBagId = lootBagId,
                };
                if (!Loot.lootBagitems.Contains(data))
                {
                    Loot.lootBagitems.Add(data);

                }


            }
            Debug.Log("!!!");
            Vector3 position = Vector3.zero;
            float x = 0;
            float y = 0;
            float z = 0;
            DataSnapshot posSnapshot = childsnapshot.Child("pos");

            x = float.Parse(posSnapshot.Child("x").Value.ToString());
            y = float.Parse(posSnapshot.Child("y").Value.ToString());
            z = float.Parse(posSnapshot.Child("z").Value.ToString());
            position = new Vector3(x, y, z);



            PublicVariables.LootBag data2 = new PublicVariables.LootBag
            {
                position = position,
                id = lootBagId,
            };
            
               
            if(!PublicVariables.lootbags.Contains(data2))
            {
                Debug.Log("added lootbag " + lootBagId);
                PublicVariables.lootbags.Add(data2);
            }
                
            
            



        }
    }
    void LoadChestsData(DataSnapshot snapshot)
    {

        foreach (var childsnapshot in snapshot.Children)
        {
            Debug.Log("reached chest ");
            int lootBagId = int.Parse(childsnapshot.Key);
            DataSnapshot itemsSnapshot = childsnapshot.Child("items");
            foreach (var item in itemsSnapshot.Children)
            {
                int count = int.Parse(item.Child("count").Value.ToString());
                int slotIndex = int.Parse(item.Key);
                string name = item.Child("name").Value.ToString();

                Loot.lootBagitemData data = new Loot.lootBagitemData
                {
                    name = name,
                    count = count,
                    slot = slotIndex,
                    lootBagId = lootBagId,
                };
                if (!Loot.chestitems.Contains(data))
                {
                    Loot.chestitems.Add(data);

                }


            }
            Debug.Log("!!!");
            Vector3 position = Vector3.zero;
            float x = 0;
            float y = 0;
            float z = 0;

            x = float.Parse(childsnapshot.Child("x").Value.ToString());
            y = float.Parse(childsnapshot.Child("y").Value.ToString());
            z = float.Parse(childsnapshot.Child("z").Value.ToString());
            position = new Vector3(x, y, z);



            PublicVariables.LootBag data2 = new PublicVariables.LootBag
            {
                position = position,
                id = lootBagId,
            };
           
            
            if (!PublicVariables.chests.Contains(data2))
            {
                Debug.Log("added chest " + lootBagId);

                PublicVariables.chests.Add(data2);
            }

            




        }
    }
    void LoadStartData(DataSnapshot snapshot)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("building");
        string type;
        int id;
        newBuildings = new List<int>();
        foreach (var childsnapshot in snapshot.Children)
        {
            type = childsnapshot.Key.ToString();
            foreach (var child in childsnapshot.Children)
            {
                bool isInList = false;
                id = int.Parse(child.Key.ToString());

                foreach (var building in PublicVariables.buildings)
                {
                    int ID = building.id;
                    if (ID == id)
                    {
                        isInList = true;
                    }
                }
                if (!isInList)
                {
                    _buildings.Add(id);
                    float x = float.Parse(child.Child("x").Value.ToString());
                    float y = float.Parse(child.Child("y").Value.ToString());
                    float z = float.Parse(child.Child("z").Value.ToString());
                    string ownerId = "";
                    if (child.Child("ownerId").Exists)
                    {
                        ownerId = child.Child("ownerId").Value.ToString();
                    }
                    float yRot = 0f;

                    Vector3 position = new Vector3(x, y, z);
                    GameObject fab;
                    if (type == "floors")
                    {
                        fab = floorPrefab;
                    }
                    else if (type == "walls")
                    {
                        fab = wallPrefab;
                        yRot = float.Parse(child.Child("yRot").Value.ToString());

                    }
                    else if (type == "roofs")
                    {
                        fab = roofPrefab;
                    }
                    else if (type == "doorways")
                    {
                        fab = doorwayPrefab;
                        yRot = float.Parse(child.Child("yRot").Value.ToString());

                    }
                    else if (type == "doors")
                    {
                        fab = doorPrefab;
                        yRot = float.Parse(child.Child("yRot").Value.ToString());

                    }
                    else
                    {
                        fab = roofPrefab;
                    }
                    int health = int.Parse(child.Child("health").Value.ToString());
                    PublicVariables.Building data = new PublicVariables.Building();



                    data = new PublicVariables.Building
                    {
                        id = id,
                        position = position,
                        prefab = fab,
                        yRotation = yRot,
                        health = health,
                        ownerId = ownerId,
                    };




                    PublicVariables.buildings.Add(data);
                    if (PublicVariables.inGame)
                    {
                        GameObject prefab = Instantiate(fab);






                        prefab.transform.position = position;

                        prefab.transform.localEulerAngles = new Vector3(0, yRot, 0);



                        prefab.GetComponent<building>().id = id;
                        prefab.GetComponent<building>().placed = true;
                        prefab.GetComponent<ObjectHealth>().health = health;
                        if (prefab.GetComponent<Door>() != null)
                        {
                            prefab.GetComponent<Door>().ownerId = ownerId;
                        }

                        Debug.Log("Spawned building");
                    }


                }
                else
                {
                    foreach (var building in buildings)
                    {
                        if (building.GetComponent<building>().id == int.Parse(child.Key))
                        {

                            building.GetComponent<ObjectHealth>().health = int.Parse(child.Child("health").Value.ToString());
                            Debug.Log("changed health to: " + child.Child("health").Value.ToString());
                        }
                    }
                }

            }

        }
    }

    public static void CreateUser()
    {

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        string userID = SystemInfo.deviceUniqueIdentifier;
        reference.Child("users").Child(userID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {

                    SaveUserData();
                }
                else
                {
                    justNowCreated = false;
                }

            }
        });



    }
    public static async void SaveUserData()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        string userID = SystemInfo.deviceUniqueIdentifier;

        reference.Child("users").Child(userID).Child("username").SetValueAsync(PublicVariables.username);
        reference.Child("users").Child(userID).Child("userID").SetValueAsync(userID);

        ItemData data = new ItemData
        {
            name = "Axe",
            count = 1,
            slot = 51,
        };
        string json = JsonUtility.ToJson(data);
        await reference.Child("users").Child(userID).Child("items").Child("51").SetRawJsonValueAsync(json);
        ItemData data2 = new ItemData
        {
            name = "Builder",
            count = 1,
            slot = 52,
        };
        string json2 = JsonUtility.ToJson(data2);
        await reference.Child("users").Child(userID).Child("items").Child("52").SetRawJsonValueAsync(json2);

    }

    public static async void UpdateWood()
    {
        string userID = SystemInfo.deviceUniqueIdentifier;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        await reference.Child("users").Child(userID).Child("woodCount").SetValueAsync(PublicVariables.woodCount);
    }
    public static IEnumerator GetBuildingHealth(int id, string type, Action<int> onCallback)
    {

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var data = reference.Child("buildings").Child(type).Child(id.ToString()).Child("health").GetValueAsync();

        yield return new WaitUntil(predicate: () => data.IsCompleted);

        if (data != null)
        {
            DataSnapshot snapshot = data.Result;
            onCallback.Invoke(Convert.ToInt32(snapshot.Value));
        }

    }



    public static IEnumerator GetWood(Action<int> onCallback)
    {
        string userID = SystemInfo.deviceUniqueIdentifier;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var woodData = reference.Child("users").Child(userID).Child("woodCount").GetValueAsync();
        yield return new WaitUntil(predicate: () => woodData.IsCompleted);

        if (woodData != null)
        {
            DataSnapshot snapshot = woodData.Result;
            onCallback.Invoke(Convert.ToInt32(snapshot.Value));
        }
        else
        {
            onCallback.Invoke(-500);
        }

    }

    public static IEnumerator GetName(Action<string> onCallback)
    {
        string userID = SystemInfo.deviceUniqueIdentifier;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var nameData = reference.Child("users").Child(userID).Child("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => nameData.IsCompleted);

        if (nameData != null)
        {
            DataSnapshot snapshot = nameData.Result;
            try
            {
                onCallback.Invoke(snapshot.Value.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("No previus username");
            }
        }

    }




    void HandleUserChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            Debug.Log("here2");
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        float x = float.Parse(snapshot.Child("x").Value.ToString());
        float y = float.Parse(snapshot.Child("y").Value.ToString());
        float z = float.Parse(snapshot.Child("z").Value.ToString());
        PublicVariables.lastPlayerPos = new Vector3(x, y, z);
    }
    void HandleTreesChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            Debug.Log("here2");
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        foreach (var childsnapshot in snapshot.Children)
        {
            float x = float.Parse(childsnapshot.Child("x").Value.ToString());
            float y = float.Parse(childsnapshot.Child("y").Value.ToString());
            float z = float.Parse(childsnapshot.Child("z").Value.ToString());
            int hitCount = int.Parse(childsnapshot.Child("hitCount").Value.ToString());
            Vector3 position = new Vector3(x, y, z);
            int id = int.Parse(childsnapshot.Key.ToString());
            string name = childsnapshot.Child("name").Value.ToString();

            PublicVariables.Tree data = new PublicVariables.Tree
            {
                id = id,
                position = position,
                hitCount = hitCount,
                name = name,

            };

            if (!PublicVariables.treePositions.Contains(data))
            {

                PublicVariables.treePositions.Add(data);

            }


        }



    }
    public static void FetchInventoryData()
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        dbReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {

            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("got items");
                DataSnapshot snapshot = task.Result;
                if (justNowCreated && !snapshot.Exists)
                {
                    Debug.Log("item not yet saved");
                    FetchInventoryData();
                    return;
                }
                if (justNowCreated && snapshot.ChildrenCount < 2)
                {
                    Debug.Log("item children not yet saved");
                    FetchInventoryData();
                    return;
                }
                foreach (var child in snapshot.Children)
                {
                    foreach (var slot in Inventory.inventorySlots)
                    {
                        InventoryObject inventoryObject = slot.GetComponent<InventoryObject>();
                        if (int.Parse(child.Key) == inventoryObject.slotIndex)
                        {

                            string itemName = child.Child("name").Value.ToString();
                            GameObject prefab = (GameObject)Resources.Load("Icons/" + itemName, typeof(GameObject));
                            GameObject fab = Instantiate(prefab);

                            fab.transform.parent = inventoryObject.transform;
                            fab.transform.localPosition = Vector3.zero;
                            fab.transform.localScale = new Vector3(1, 1, 1);
                            int count = int.Parse(child.Child("count").Value.ToString());
                            fab.GetComponent<Item>().count = count;
                            fab.name = itemName;
                            PublicVariables.serverRpcs.UpdatePlayerItemsServerRpc(PublicVariables.myOwnerClientId, itemName, count, inventoryObject.slotIndex, true);
                            if (count > 1)
                            {
                                fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "x" + count;

                            }
                            inventoryObject.icon = fab;
                            if (child.Child("ammoCount").Exists)
                            {
                                int ammoCount = int.Parse(child.Child("ammoCount").Value.ToString());
                                fab.GetComponent<PistolAmmo>().ammoCount = ammoCount;
                                fab.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = ammoCount + "";
                            }




                        }
                    }

                }







            }
        });
    }

    private void Update()
    {
        if (playerJoined)
        {
            playerJoined = false;
            FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("buildings").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                LoadStartData(snapshot);
            }
        }
        );
        }
    }
}
