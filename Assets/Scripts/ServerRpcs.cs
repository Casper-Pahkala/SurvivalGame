using Firebase.Database;
using Firebase.Extensions;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class ServerRpcs : NetworkBehaviour
{
    public GameObject chestPrefab;
    public string url;
    public string name;
    public static string savePath;
    public bool canPing = true;
    public GameObject videoObject;
    public GameObject doorPrefab;
    public GameObject doorwayPrefab;
    public GameObject axeWoodSound;
    public GameObject axeWooshSound;

    public RuntimeAnimatorController axeHitController;
    public GameObject sleepingbagPrefab;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public GameObject vineBoom;
    public GameObject videoPlayer;

    public VideoClip southPark1;

    bool videoSpawned = false;
    int playerCount = 0;
    int downloadedCount = 0;

    public GameObject playerLootBag;
    public LayerMask groundMask;
    public static Transform spawnPos;
    public GameObject fleshHitSound;
    public GameObject headshotHitSound;

    public GameObject m1911ShootingSound;
    public GameObject m1911EmptyClipSound;
    public GameObject m1911ReloadSound;

    void Start()
    {
        PublicVariables.serverRpcs = this;

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (IsHost)
            {
                foreach (Vector3 pos in PublicVariables.buildingPositions)
                {
                    UpdateBuildingsListClientRpc(pos);
                }

            }
        };
    }
    [Command]
    void kill(ulong clientid)
    {
        HitPlayerServerRpc(clientid, 100, true, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HitPlayerServerRpc(ulong clientId, int damage, bool head, bool soundOn = true)
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                if (soundOn)
                {
                    GameObject sound;
                    if (head)
                    {
                        sound = Instantiate(headshotHitSound);
                    }
                    else
                    {
                        sound = Instantiate(fleshHitSound);
                    }
                    sound.transform.position = player.transform.position;
                    sound.GetComponent<NetworkObject>().Spawn();
                }

                player.GetComponent<player>().health -= damage;

                if (player.GetComponent<player>().health <= 0)
                {


                    KillPlayer(player);

                }

                HitPlayerClientRpc(clientId, player.GetComponent<player>().health);

            }
        }
    }

    void KillPlayer(GameObject player)
    {
        player.GetComponent<player>().health = 100;
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, -Vector3.up, out hit, 10000f, groundMask))
        {

            GameObject prefab = Instantiate(playerLootBag);
            prefab.transform.position = hit.point;


            int id = UnityEngine.Random.Range(0, 99999999);
            SpawnObjectClientRpc(hit.point, "lootbag", id);
            prefab.GetComponent<lootbag>().lootBagId = id;
            float x = prefab.transform.position.x;
            float y = prefab.transform.position.y;
            float z = prefab.transform.position.z;
            DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            dbReference.Child("server").Child("lootbags").Child(id.ToString()).Child("pos").Child("x").SetValueAsync(x);
            dbReference.Child("server").Child("lootbags").Child(id.ToString()).Child("pos").Child("y").SetValueAsync(y);
            dbReference.Child("server").Child("lootbags").Child(id.ToString()).Child("pos").Child("z").SetValueAsync(z);
            dbReference.Child("server").Child("lootbags").Child(id.ToString()).Child("owner").SetValueAsync(player.GetComponent<player>().playerId);

            List<Inventory.ItemData> playerItems = player.GetComponent<player>().playerInventory.playerInventoryList;
            List<Inventory.ItemData> toDeleteList = new List<Inventory.ItemData>();
            foreach (var item in playerItems)
            {
                string json = JsonUtility.ToJson(item);
                dbReference.Child("server").Child("lootbags").Child(id.ToString()).Child("items").Child(item.slot.ToString() + "0").SetRawJsonValueAsync(json);
                toDeleteList.Add(item);


            }
            foreach (var item in toDeleteList)
            {
                UpdatePlayerItemsServerRpc(player.GetComponent<NetworkObject>().OwnerClientId, item.name, item.count, item.slot, false);

            }

        }
        else
        {
            Debug.Log("Raycast didn't hit groundmask and lootbag not spawned");
        }
        RemoveClientItemsClientRpc(player.GetComponent<NetworkObject>().OwnerClientId);

        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("sleepingbags").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                bool gotBag = false;
                DataSnapshot snapshot = task.Result;
                foreach (var child in snapshot.Children)
                {
                    if (child.Child("ownerId").Value.ToString() == SystemInfo.deviceUniqueIdentifier)
                    {
                        gotBag = true;
                        float x = float.Parse(child.Child("x").Value.ToString());
                        float y = float.Parse(child.Child("y").Value.ToString());
                        float z = float.Parse(child.Child("z").Value.ToString());
                        Vector3 pos = new Vector3(x, y + 2, z);
                        MovePlayerToPosClientRpc(player.GetComponent<NetworkObject>().OwnerClientId, pos);

                        break;
                    }
                }
                if (!gotBag)
                {
                    MovePlayerToPosClientRpc(player.GetComponent<NetworkObject>().OwnerClientId, spawnPos.position);

                }

            }
        });




    }
    [ClientRpc]
    void MovePlayerToPosClientRpc(ulong clientId, Vector3 position)
    {

        if (PublicVariables.myOwnerClientId == clientId)
        {
            GameObject player = PublicVariables.myPlayer;
            player.transform.root.GetComponent<CharacterController>().enabled = false;
            player.transform.root.position = position;
            player.transform.root.GetComponent<CharacterController>().enabled = true;
        }
    }

    [ClientRpc]
    void RemoveClientItemsClientRpc(ulong clientId)
    {
        if (PublicVariables.myOwnerClientId == clientId)
        {
            Inventory.RemoveEveryItem();
        }
    }
    [ClientRpc]
    void HitPlayerClientRpc(ulong clientId, int health)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {

                player.GetComponent<player>().health = health;

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayVineBoomServerRpc(ulong clientId)
    {
        PlayVineBoomClientRpc(clientId);
    }
    [ClientRpc]
    void PlayVineBoomClientRpc(ulong clientId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                GameObject prefab = Instantiate(vineBoom);
                prefab.transform.parent = player.transform;
                prefab.transform.localPosition = Vector3.zero;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayVideoServerRpc()
    {
        PlayVideoClientRpc();

    }
    [ClientRpc]
    void PlayVideoClientRpc()
    {
        PublicVariables.videoPlayer.SetActive(true);
        PublicVariables.videoPlayer.transform.GetChild(0).GetComponent<RawImage>().enabled = true;
        PublicVariables.videoPlayer.transform.GetChild(1).GetComponent<VideoPlayer>().enabled = true;
        PublicVariables.videoPlayer.transform.GetChild(1).GetComponent<VideoPlayer>().Play();

        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        yield return new WaitForSeconds(14);
        PublicVariables.videoPlayer.SetActive(false);
    }

    [ClientRpc]

    public void UpdateBuildingsListClientRpc(Vector3 pos)
    {

        PublicVariables.buildingPositions.Add(pos);
        Debug.Log("added building to pos list");

    }

    [ServerRpc(RequireOwnership = false)]
    public void HitWoodServerRpc(ulong clientId, string soundType, Vector3 position, int treeId = 0)
    {
        if (soundType == "axeWood")
        {
            GameObject sound = Instantiate(axeWoodSound, position, Quaternion.identity);
            sound.GetComponent<NetworkObject>().Spawn();
            sound.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            if (OwnerClientId == clientId)
            {
                //sound.GetComponent<AudioSource>().volume = 0f;
            }
            AxeHitAnimationClientRpc(clientId);
        }
        if (soundType == "axeWhoosh")
        {
            GameObject sound = Instantiate(axeWooshSound, position, Quaternion.identity);
            sound.GetComponent<NetworkObject>().Spawn();
            sound.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            if (OwnerClientId == clientId)
            {
                // sound.GetComponent<AudioSource>().volume = 0f;
            }
            AxeHitAnimationClientRpc(clientId);
        }
        /*
        if (treeId != 0)
        {
            GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
            foreach (GameObject tree in trees)
            {
                if (tree.GetComponent<tree>().id == treeId)
                {
                    tree.GetComponent<tree>().hitCount--;
                    if (tree.GetComponent<tree>().hitCount == 0)
                    {
                        tree.GetComponent<NetworkObject>().Despawn();
                        int id = tree.GetComponent<tree>().id;
                        deleteTree(id);
                    }
                }
            }
        }
        */


    }
    async void deleteTree(int id)
    {
        Debug.Log("Deleting " + id);
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        await dbReference.Child("server").Child("trees").Child(id.ToString()).RemoveValueAsync();
        Debug.Log("Deleted " + id);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DeleteObjectServerRpc(int id, string type, string tag)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in buildings)
        {
            if (obj.GetComponent<building>()!=null)
            {
                if (obj.GetComponent<building>().id == id)
                {
                    deleteBuilding(id, type);
                }
            }
            if (obj.GetComponent<Resource>() != null)
            {
                if (obj.GetComponent<Resource>().id == id)
                {
                    deleteBuilding(id, type);
                }
            }
        }
        DeleteBuildingClientRpc(id,tag);
    }
    [ClientRpc]
    void DeleteBuildingClientRpc(int id, string tag)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject building in buildings)
        {
            if (building.GetComponent<building>() != null)
            {
                if (building.GetComponent<building>().id == id)
                {
                    Destroy(building);
                }
            }
            if (building.GetComponent<Resource>() != null)
            {
                if (building.GetComponent<Resource>().id == id)
                {
                    Destroy(building);
                }
            }
        }
    }

    async void deleteBuilding(int id, string type)
    {
        Debug.Log("Deleting " + id);
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        if (type != null)
        {
            await dbReference.Child("server").Child(type).Child(id.ToString()).RemoveValueAsync();
            Debug.Log("Deleted " + id);

        }
    }

    [ClientRpc]
    public void AxeHitAnimationClientRpc(ulong clientId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                if (!player.GetComponent<NetworkObject>().IsOwner)
                {
                    player.transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = true;
                    player.transform.GetChild(0).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = axeHitController;
                    player.transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("hit");
                }
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void PlaceBuildingServerRpc(ulong clientId, string buildingType, Vector3 position, int buildingId, float rotation = 0f)
    {
        return;
        GameObject prefab;
        PublicVariables.buildingPositions.Add(position);
        UpdateBuildingsListClientRpc(position);
        if (buildingType == "floor")
        {
            prefab = Instantiate(floorPrefab);


        }
        else
        if (buildingType == "wall")
        {
            prefab = Instantiate(wallPrefab);
            prefab.transform.localEulerAngles = new Vector3(0, rotation, 0);


        }
        else
        if (buildingType == "roof")
        {
            prefab = Instantiate(roofPrefab);

        }
        else
        {
            prefab = Instantiate(floorPrefab);
        }
        prefab.GetComponent<building>().id = buildingId;
        prefab.transform.position = position;
        prefab.GetComponent<building>().placed = true;
        prefab.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowItemServerRpc(ulong clientId, string itemType)
    {
        if (OwnerClientId == clientId) return;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                if (p.transform.GetChild(0).Find(itemType) != null)
                {
                    p.transform.GetChild(0).Find(itemType).gameObject.SetActive(true);

                }


            }



        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DataWrittenServerRpc(int id, Vector3 pos, string type, float yRot = 0f, string ownerId = "")
    {
        if (type == "sleepingbags")
        {
            ObjectDataWrittenClientRpc(id, pos, type, ownerId, yRot);
        }else if(type == "chests")
        {
            ObjectDataWrittenClientRpc(id, pos, type, ownerId, yRot);

        }
        else
        {
            BuildingDataWrittenClientRpc(id, pos, type, ownerId, yRot);
        }


    }
    [ServerRpc(RequireOwnership = false)]
    public void TreeDataWrittenServerRpc(int id, int hitCount)
    {

        TreesDataWrittenClientRpc(id, hitCount);


    }
    [ClientRpc]
    public void TreesDataWrittenClientRpc(int id, int hitCount)
    {

        GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
        foreach (var tree in trees)
        {
            if (tree.GetComponent<tree>().id == id)
            {
                tree.GetComponent<tree>().hitCount = hitCount;
            }
        }


    }

    [ClientRpc]
    public void BuildingDataWrittenClientRpc(int id, Vector3 pos, string type, string ownerId, float yRot = 0f)
    {
        Debug.Log("Reached client");


        bool isInList = false;
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





            GameObject fab;
            if (type == "floors")
            {
                fab = floorPrefab;
            }
            else if (type == "walls")
            {
                fab = wallPrefab;


            }
            else if (type == "roofs")
            {
                fab = roofPrefab;
            }
            else if (type == "doorways")
            {
                fab = doorwayPrefab;

            }
            else if (type == "doors")
            {
                fab = doorPrefab;
            }
            else
            {
                fab = roofPrefab;
            }
            PublicVariables.Building data = new PublicVariables.Building
            {
                id = id,
                position = pos,
                prefab = fab,
                yRotation = yRot,
                health = 500,
            };
            PublicVariables.buildings.Add(data);
            if (PublicVariables.inGame)
            {
                GameObject prefab = Instantiate(fab);
                if (prefab.GetComponent<Door>() != null)
                {
                    prefab.GetComponent<Door>().ownerId = ownerId;
                }
                if (prefab.GetComponent<CupBoard>() != null)
                {
                    prefab.GetComponent<CupBoard>().owner = ownerId;
                }



                prefab.GetComponent<building>().id = id;
                prefab.GetComponent<building>().placed = true;
                prefab.GetComponent<ObjectHealth>().health = 500;



                prefab.transform.localEulerAngles = new Vector3(0, yRot, 0);
                prefab.transform.position = pos;





                Debug.Log("Spawned building to pos " + pos);
            }
        }


    }


    [ClientRpc]
    public void ObjectDataWrittenClientRpc(int id, Vector3 pos, string type, string ownerId, float yRot = 0f)
    {
        Debug.Log("Reached client");


        bool isInList = false;
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





            GameObject fab;
            if (type == "sleepingbags")
            {
                fab = sleepingbagPrefab;
            }
            else if(type =="chests")
            {
                fab = chestPrefab;

            }
            else
            {
                fab = roofPrefab;
            }
            PublicVariables.Building data = new PublicVariables.Building
            {
                id = id,
                position = pos,
                prefab = fab,
                yRotation = yRot,
                health = 500,
            };
            PublicVariables.buildings.Add(data);
            if (PublicVariables.inGame)
            {
                GameObject prefab = Instantiate(fab);
                if (prefab.GetComponent<Door>() != null)
                {
                    prefab.GetComponent<Door>().ownerId = ownerId;
                }
                if (prefab.GetComponent<CupBoard>() != null)
                {
                    prefab.GetComponent<CupBoard>().owner = ownerId;
                }


                if(prefab.GetComponent<lootbag>()!= null)
                {
                    prefab.GetComponent<lootbag>().lootBagId = id;
                }
                //prefab.GetComponent<building>().id = id;
                //prefab.GetComponent<building>().placed = true;
                //prefab.GetComponent<ObjectHealth>().health = 500;



                prefab.transform.localEulerAngles = new Vector3(prefab.transform.localEulerAngles.x, yRot, prefab.transform.localEulerAngles.z);
                prefab.transform.position = pos;





                Debug.Log("Spawned object to pos " + pos);
            }
        }


    }
    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc(int id)
    {
        OpenDoorClientRpc(id);
    }
    [ClientRpc]
    public void OpenDoorClientRpc(int id)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("building");
        foreach (GameObject building in buildings)
        {
            if (building.GetComponent<building>().id == id)
            {
                building.transform.GetChild(0).GetComponent<Door>().rotate();
                return;
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeHeldItemServerRpc(ulong clientId, string itemName)
    {
        ChangeHeldItemClientRpc(clientId, itemName);
    }
    [ClientRpc]
    public void ChangeHeldItemClientRpc(ulong clientId, string itemName)
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Changing item for player: " + clientId + " object: " + itemName);
        foreach (GameObject player in players)
        {

            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                int count = player.transform.GetChild(0).childCount;
                for (int i = 0; i < count; i++)
                {
                    player.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
                }
                if (itemName == "Axe")
                {
                    Debug.Log("equipped Axe for player: " + clientId);
                    player.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else if (itemName == "Builder")
                {
                    player.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);

                }
                else if (itemName == "m1911")
                {
                    player.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DamageBuildingServerRpc(int id, int damage, int layer)
    {
        DamageBuildingClientRpc(id, damage, layer);
    }
    [ClientRpc]
    public void DamageBuildingClientRpc(int id, int damage, int layer)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("building");
        foreach (var building in buildings)
        {
            if (building.GetComponent<building>().id == id)
            {
                if (building.layer == LayerMask.NameToLayer("doorway"))
                {
                    building.transform.parent.GetComponent<ObjectHealth>().health -= damage;
                }
                else
                {
                    building.GetComponent<ObjectHealth>().health -= damage;
                }
            }
        }
    }
    [Command]
    void playVideo(string name, int number)
    {
        PlayVideoServerRpc(name, number);
    }
    [ServerRpc(RequireOwnership = false)]
    void PlayVideoServerRpc(string name, int number)
    {
        playClientRpc(name, number);
    }
    [ClientRpc]
    void playClientRpc(string name, int number)
    {
        StorageManager.DownloadVideo(name + number);
        playerCount = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        playerCount = players.Length;
        Debug.Log(playerCount);

    }
    [ServerRpc(RequireOwnership = false)]
    public void videoDownloadedServerRpc(string name, ulong clientId)
    {
        playerCount = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        playerCount = players.Length;
        Debug.Log(name + " has downloaded video");
        downloadedCount++;
        Debug.Log(downloadedCount + " " + playerCount);
        if (playerCount <= downloadedCount)
        {
            Debug.Log("Everyone has video");
            playCurrentVideoClientRpc();
            playerCount = 0;

        }

    }
    [ClientRpc]
    void playCurrentVideoClientRpc()
    {
        PublicVariables.videoPlayer.transform.GetChild(0).GetComponent<VideoPlayer>().Play();
    }


    [Command]
    void downloadvideo(string name)
    {
        videoWithNameServerRpc(name);

    }
    [ServerRpc(RequireOwnership = false)]
    void videoWithNameServerRpc(string name)
    {
        videoWithNameClientRpc(name);
    }
    [ClientRpc]
    void videoWithNameClientRpc(string name)
    {
        StartCoroutine(DownloadVideo(name));
    }
    [Command]
    void downloadvideowithurl(string url)
    {
        StartCoroutine(DownloadVideo2(url));
    }

    IEnumerator DownloadVideo(string name)
    {

        UnityEngine.Debug.Log("started");
        savePath = string.Format("{0}/{1}.mp4", UnityEngine.Application.persistentDataPath, name);
        if (System.IO.File.Exists(savePath))
        {
            UnityEngine.Debug.Log("File exists");
            PublicVariables.theatrePlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);
            //videoPlayer.Play();
            //Enter serverrpc here to inform client has the file and comment play command
            yield break;
        }


        url = GetVideoLink(name);
        UnityWebRequest www = UnityWebRequest.Get(url);
        if (canPing)
        {
            canPing = false;
            StartCoroutine(ping());

        }

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {

            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            //Enter serverrpc here to inform client has the file and comment play command
            UnityEngine.Debug.Log("downloaded to " + savePath);
            PublicVariables.theatrePlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);

            //videoPlayer.Play();
        }
    }
    [Command]
    void sincity(string thesin)
    {
        if (thesin.StartsWith("http"))
        {
            DownloadVideoToScreenServerRpc(thesin);

        }
        else
        {
            Debug.Log("Sin city wasn't made for you");
        }
    }

    IEnumerator DownloadVideo2(string url)
    {
        int random = UnityEngine.Random.Range(0, 999999999);
        savePath = string.Format("{0}/{1}.mp4", UnityEngine.Application.persistentDataPath, random.ToString());

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {

            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            UnityEngine.Debug.Log("downloaded to " + savePath);
            PublicVariables.theatrePlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);

            //videoPlayer.Play();
            //Enter serverrpc here to inform client has the file and comment play command
        }
    }
    [ServerRpc(RequireOwnership =false)]
    void DownloadVideoToScreenServerRpc(string thesin)
    {
        DownloadVideoToScreenClientRpc(thesin);
    }
    [ClientRpc]
    void DownloadVideoToScreenClientRpc(string thesin)
    {
        StartCoroutine(DownloadVideoToScreen(thesin));
    }
    IEnumerator DownloadVideoToScreen(string url)
    {
        int random = UnityEngine.Random.Range(0, 9999999);
        savePath = string.Format("{0}/{1}.mp4", UnityEngine.Application.persistentDataPath, random.ToString()) ;
        if (System.IO.File.Exists(savePath))
        {
            UnityEngine.Debug.Log("File exists");
            PublicVariables.screenPlayer.GetComponent<RawImage>().enabled = true;
            PublicVariables.screenPlayer.GetComponent<VideoPlayer>().url = savePath;
            PublicVariables.screenPlayer.GetComponent<VideoPlayer>().Play();
            double videoLength = PublicVariables.screenPlayer.GetComponent<VideoPlayer>().clip.length;
            Debug.Log("Video length: " + videoLength);
            StartCoroutine(stopVideo(Convert.ToSingle(videoLength)));

            yield break;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {

            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            PublicVariables.screenPlayer.GetComponent<RawImage>().enabled = true;
            PublicVariables.screenPlayer.GetComponent<VideoPlayer>().url = savePath;
            PublicVariables.screenPlayer.GetComponent<VideoPlayer>().Play();
            double videoLength = PublicVariables.screenPlayer.GetComponent<VideoPlayer>().clip.length;
            StartCoroutine(stopVideo(Convert.ToSingle(videoLength)));
            //videoPlayer.Play();
            //Enter serverrpc here to inform client has the file and comment play command
        }
    }
    IEnumerator stopVideo(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PublicVariables.screenPlayer.GetComponent<RawImage>().enabled = false;
        PublicVariables.screenPlayer.GetComponent<VideoPlayer>().url = "";
        PublicVariables.screenPlayer.GetComponent<VideoPlayer>().Stop();
    }
    IEnumerator ping()
    {
        yield return new WaitForSeconds(2);
        UnityEngine.Debug.Log("downloading");
        canPing = true;
    }

    string GetVideoLink(string name)
    {
        if (name.Contains("spongebob"))
        {
            if (name.Contains("0000"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Squeaky-Boots.mp4";
            }
            if (name.Contains("0001"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Culture-Shock.mp4";
            }
            if (name.Contains("0002"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-MuscleBob-BuffPants.mp4";
            }
            if (name.Contains("0003"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Imitation-Krabs.mp4";
            }
            if (name.Contains("0004"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Something-Smells.mp4";
            }
            if (name.Contains("0005"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Your-Shoes-Untied.mp4";
            }
            if (name.Contains("0006"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Arrgh.mp4";
            }

            if (name.Contains("0007"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Life-of-Crime.mp4";
            }
            return "";


        }
        else if (name.Contains("southpark"))
        {
            UnityEngine.Debug.Log("southpark");
            if (name.Contains("1001"))
            {
                return "https://asmassets.mtvnservices.com/asm/mtv_international/alberto/2018/sp_1413_HD_intro_512x288_450.mp4";
            }
            if (name.Contains("1002"))
            {
                return "https://asmassets.mtvnservices.com/asm/mtv_international/alberto/2018/sp_1413_HD_intro_512x288_450.mp4";
            }
            if (name.Contains("1003"))
            {
                return "";
            }
            if (name.Contains("1004"))
            {
                return "";
            }
            if (name.Contains("1005"))
            {
                return "";
            }
            if (name.Contains("1006"))
            {
                return "";
            }
            if (name.Contains("1007"))
            {
                return "";
            }
            if (name.Contains("1008"))
            {
                return "";
            }
            if (name.Contains("1009"))
            {
                return "";
            }
            if (name.Contains("1010"))
            {
                return "";
            }
            //season 11

            if (name.Contains("1101"))
            {
                return "https://public.am.files.1drv.com/y4mkhEtT6H51wZOHIBHSbESNpS0kp6DDno2rTEm5XsOey9FSx9ryvErZjLAstXqih23-rQJ2T90ZfiaTg0MxChteIlpaIvE_5qoA9XOgvtqLcVDKAbvfg3WjvAuURykUqllcqlvP_5sH9I0brg-SegRvc1U8hQ1957UlwenmVdAoQeqm8v-Pc8I6IRtWbhE9FRMoePsmNZoCqp7TQBIqwWHJpjSffJaIssPAQXTkmBtwtc?AVOverride=1";
            }
            if (name.Contains("1102"))
            {
                return "";
            }
            if (name.Contains("1103"))
            {
                return "";
            }
            if (name.Contains("1104"))
            {
                return "";
            }
            if (name.Contains("1105"))
            {
                return "";
            }
            if (name.Contains("1106"))
            {
                return "";
            }
            if (name.Contains("1107"))
            {
                return "";
            }
            if (name.Contains("1108"))
            {
                return "";
            }
            if (name.Contains("1109"))
            {
                return "";
            }
            if (name.Contains("1110"))
            {
                return "";
            }

            //Season 15
            if (name.Contains("1501"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131375&authkey=ANNUp10NuDSFPWM";
            }

            if (name.Contains("1502"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131376&authkey=AJQGlhUt2sUWGfc";
            }

            if (name.Contains("1503"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131377&authkey=ALOlwmO1fVTSiDY";
            }
            if (name.Contains("1504"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131378&authkey=ANc3wnvsPDH3ELM";
            }
            if (name.Contains("1505"))
            {
                return "https://public.am.files.1drv.com/y4mLFWV35wSPRqTd4vH27RwKUbfnM6o3x7uREY4wzZ7G7pubqYxUPlabxpwufN_37nAsYtcfQX3gNclrJhJCx8OWDvVr4Dd3NacShDAxtmAn4QvDY8QXLhCAAOZNZhsQK2kZMZJvM5A0VLQZaVhrXc3g3XBZAr9OE1pq5ULEAzUbD4JFU3eR359HgUf2X5O4Yy0J13MxLiQjkqSQOCREi1k6JRi5Pj9-Xws221JWp2v6oM?AVOverride=1";
            }
            if (name.Contains("1506"))
            {
                return "https://public.am.files.1drv.com/y4m79klV2KFs0-Q6pFaCIb2TF1BWX_bdB1oM6FG4MdZlQpWw_dEjiUVUMUohle3IHkUlebe0Jo-xIeDERm9oBr4aFcaFzxRdHaWglTv3-nfDLFF2xb8gN0WvohU4wnt5wTM7_m_gfeInlkt0L7tWA1ye1NeMwlAn5J6nlMfY1zV8S6I84bQadpxgT3Ecld4RifdxLrqLf1Zg6H0pmQEEEDx_nSOXp7dqxESiorWwD_Mqlw?AVOverride=1";
            }
            if (name.Contains("1507"))
            {
                return "https://public.am.files.1drv.com/y4mNs_k0im_Qe8AKr7E6JLmINsHSd-KWc_r79yrM86RUqVV51leunhxuEAyvEy5CGW2w9PkHEACd4UQg0ckLv1fBRYm0x1p5iLuD1lSRJY0UR0lr5N-CrsMFW1iTG4UtjtMIAR2hBkRQWzGUDzPWmJZBmLKRREycIOqT7zk64axg7YWXwM-EbcUy-qAPjIb3ANbcMOKCF4fQQHlw5UWzdaE-LWwimphHvoyKMy0xxjyrx0?AVOverride=1";
            }
            if (name.Contains("1508"))
            {
                return "https://public.am.files.1drv.com/y4mLTW4aqSWOqDW4-oIPKZEgVG4dTb8MMNhoezdlW0qM1yfJd46MCbha8wNjVfssrbPHdzjCKn1IJBKk8S7-YNEgLR0hqdmKN1O7ZDEpOk5BmnkMI-ClTNVwJXV2eNFFHNu2iNoAsFj74rhffcNs_udQFKcRxmYNETfBg67u6ULUSI55BwbiTLxG99_HLNW7nvPfKbS-URI8slOeq1wcsLozKIGmJhg1G0Ar9cJxN7jS1k?AVOverride=1";
            }
            if (name.Contains("1509"))
            {
                return "https://public.am.files.1drv.com/y4m_WtNw6Bml10dIHxa1Ysr3Dm9yh488gqutR33KCuG3CE4V17pv_Ut245qTSnSEqTxp_tcMIJa0Lj7N5T1u8-DjlJ9mP68ZECKXnmafE3fXLpHGLNNxSnn8bEvIuxIMkkxGmL4QIf8qVKvAqm-_pCPFb0DG96Wh7mfKO4Whn6Mylv5r8KZ4l5QQmHDKyAR5UmmhBOnrfWHUhHL8TS2Rk4mgXj8K5FvEFEATh88ZGETFmU?AVOverride=1 ";
            }
            if (name.Contains("1510"))
            {
                return "https://public.am.files.1drv.com/y4m7xtsYQ2CNlseDrx49BIqTkOZ-TTSKrlEyzKX1TTjRvntf8wpbsaqIFz7Z3iK2TpPws-Tk6GVYapiaeM4wLZ93Lk3pRoSAmGI2vr4oos8vbYN0vc58bUzKafWvQXtnY8VRr8glRtAgzZ9pJP8ccGHb_iQCa_IBlRQbbZ1gTGJvrcwnlvfpN41lWXWilv3fKMo5zasHo4sP7B9BuZT11KcRwMBw1m95NqRD4-sZeL5lnk?AVOverride=1";
            }
            if (name.Contains("1511"))
            {
                return "https://public.am.files.1drv.com/y4mlbQIYXkzYueZhxwDlGod_KjjOuC14nXlTk4xZE3zwJDmUmCFAaQ51966K3xBA404ZBnYhskj3CDm7e6G2dSjai-3cc0CC_CcUqkty_4HwW4l3H6CparYF5SHPVDIdLxl620gqkWR_GvqxVoffJV8J5kJC0x6Xz0yklhzF5jgQd159VST3yF69IX_tvoP6gA5S-h5YpcFHHcqzkBdKkelV0PgROzAjyjuinORUkOXxKM?AVOverride=1";
            }
            if (name.Contains("1512"))
            {
                return "https://public.am.files.1drv.com/y4mdX0kfYJGgNQBbSMS-rnn3Wx-1rSYsu9Z6-WwLvtGJTYgeyHjx93NTh5Kr36_63xC8hhHAQS4nGQfk2VqeD-Xk0f8AepakQEKCUdYLO6Cyrz5I7PD7hFMqIjusSrtBZqNAQWGBfYkNqH-ArJQ3TMWv-I01ocwcllDolk_gxfzXtZj4W3iZ3Wj0DRNs5koMLWyhNtJJlKEcyVr0CxynNnseJ0Iv0D6O3vtbqGN3lxMKFM?AVOverride=1";
            }
            if (name.Contains("1513"))
            {
                return "https://public.am.files.1drv.com/y4mB_UD0W46x8U-3TWzhMcsGugSpeqQroQE9-wHFA4chysRDUIyNcrPHmEAtkFOEZ-VUGQt_hc9wQvu4YPttGDrHFCLAkMraAhcY3APG7w_C3rKLDadB0CH6dSySLXNgNaxNXvoIO-QztEli-lgfqu3qWgUR-jgJ8BzzGLrTTUmMEQhjeRl0T6aRs8asJGRnAfo31KQkA7wafYCQpi_9blS_bxfjvMwG4wNZVx9EEuc9v0?AVOverride=1";
            }
            if (name.Contains("1514"))
            {
                return "https://public.am.files.1drv.com/y4mkuwlq8_fiDC2aBrAbkuGDCCTye3Zbxy35DBbdg5bADBmxFhUx7ThvKmmkL0HjDOrZQAuJm1bz5qmfBM06jWWd3KoYe-KcbKlj5sKIIB0pktau8X_D6aaR3um0oh5G3kP3QC4FV63ofKl0ylp5ESHGspRISh0cZtEcQ_h1XWgq-NUHox_8IR9cNoxF-kJ5EapcdchjFS2gwlBTVBMeVvVSFDj9sv8YeQIRJpEP0eAlrk?AVOverride=1";
            }

        }
        return "";
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerIdServerRpc(ulong clientId, string playerId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                p.GetComponent<player>().playerId = playerId;
            }
            UpdatePlayerIdClientRpc(p.GetComponent<NetworkObject>().OwnerClientId, p.GetComponent<player>().playerId);
        }


    }
    [ClientRpc]
    public void UpdatePlayerIdClientRpc(ulong clientId, string playerId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                p.GetComponent<player>().playerId = playerId;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerItemsServerRpc(ulong clientId, string name, int count, int slot, bool add)
    {

        UpdatePlayerItemsClientRpc(clientId, name, count, slot, add);

    }
    [ClientRpc]

    public void UpdatePlayerItemsClientRpc(ulong clientId, string name, int count, int slot, bool add)
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (clientId == p.GetComponent<NetworkObject>().OwnerClientId)
            {
                Inventory.ItemData data = new Inventory.ItemData
                {
                    name = name,
                    count = count,
                    slot = slot,
                };
                if (add)
                {
                    foreach (var d in p.GetComponent<player>().playerInventory.playerInventoryList)
                    {
                        if (data.slot == d.slot)
                        {
                            p.GetComponent<player>().playerInventory.playerInventoryList.Remove(d);
                            break;
                        }
                    }
                    Debug.Log("Added: " + data.slot);
                    p.GetComponent<player>().playerInventory.playerInventoryList.Add(data);
                }
                else
                {
                    foreach (var d in p.GetComponent<player>().playerInventory.playerInventoryList)
                    {
                        if (data.slot == d.slot)
                        {
                            p.GetComponent<player>().playerInventory.playerInventoryList.Remove(d);
                            break;
                        }
                    }


                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(Vector3 position, string prefabName, int id = 0)
    {
        SpawnObjectClientRpc(position, prefabName, id);
    }

    [ClientRpc]
    void SpawnObjectClientRpc(Vector3 position, string prefabName, int id = 0)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject player = null;
        ulong clientId = Convert.ToUInt64(id);
        foreach (var p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player = p;
            }
        }
        GameObject prefab = null;
        if (prefabName == "lootbag")
        {
            prefab = Instantiate(playerLootBag);
            prefab.transform.position = position;
            prefab.GetComponent<lootbag>().lootBagId = id;
        }
        if (prefabName == "m1911ShootingSound")
        {
            if (PublicVariables.myOwnerClientId == clientId)
            {
                prefab = Instantiate(m1911ShootingSound);
                prefab.GetComponent<AudioSource>().spatialBlend = 0;
                prefab.transform.position = position;
            }
            else
            {
                prefab = Instantiate(m1911ShootingSound);
                prefab.GetComponent<AudioSource>().spatialBlend = 1;
                prefab.transform.position = position;
            }


        }
        if (prefabName == "m1911EmptyClipSound")
        {
            if (PublicVariables.myOwnerClientId == clientId)
            {
                prefab = Instantiate(m1911EmptyClipSound);
                prefab.GetComponent<AudioSource>().spatialBlend = 0;
                prefab.transform.position = position;
            }
            else
            {
                prefab = Instantiate(m1911EmptyClipSound);
                prefab.transform.position = position;
            }
        }
        if (prefabName == "m1911ReloadSound")
        {

            if (PublicVariables.myOwnerClientId == clientId)
            {
                prefab = Instantiate(m1911ReloadSound);
                prefab.GetComponent<AudioSource>().spatialBlend = 0;
                prefab.transform.position = position;
            }
            else
            {
                prefab = Instantiate(m1911ReloadSound);

                if (player)
                {
                    prefab.transform.parent = player.transform;

                }
                else
                {
                    Debug.Log("Error player null with reload sound");
                }
                prefab.transform.localPosition = Vector3.zero;
            }
        }

    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroyObjectsServerRpc(string tag)
    {
        DestroyObjectsClientRpc(tag);
    }
    [ClientRpc]
    public void DestroyObjectsClientRpc(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

}
