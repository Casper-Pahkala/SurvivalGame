using Firebase.Database;
using Firebase.Extensions;
using QFSW.QC;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerNetworkHandler : NetworkBehaviour
{
    public AudioSource music;
    public PlayerMovement playerMovement;
    public RotateWithMouse rotateWithMouse;
    public Camera m_camera;
    public AudioListener audioListener;
    public GameObject playerModel;
    public Animator axeAnimator;
    public GameObject Axe;
    public GameObject errorCamera;
    public GameObject treePrefab;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public GameObject doorWayPrefab;
    public GameObject builder;
    public GameObject cameraObject;
    public GameObject healthUI;
    DatabaseReference dbReference;
    public LayerMask ignoreLayers;
    public LayerMask playerLayer;
    bool sceneLoaded;
    bool dataLoaded;
    public Inventory inventory;
    public GameObject lootBag;
    public GameObject lootScript;
    public Pistol playerPistol;
    public CraftingMenu craftingMenu;
    public GameObject chestPrefab;
    public GameObject hempPrefab;
    void Start()
    {

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        TestLobby.playing = true;
        if (IsOwner)
        {
            gameObject.layer = LayerMask.NameToLayer("myplayer");
            craftingMenu.enabled = true;
            playerPistol.enabled = true;
            PublicVariables.serverRpcs.UpdatePlayerIdServerRpc(OwnerClientId, SystemInfo.deviceUniqueIdentifier);
            PublicVariables.myInventory = inventory;
            GetComponent<OpenDoor>().enabled = true;
            cameraObject.SetActive(true);
            DatabaseManager.playerJoined = true;
            PublicVariables.playerNetworkHandler = this;
            StartCoroutine(WaitForSeconds(1));
            QuantumConsole.gamePaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            axeAnimator.enabled = true;
            playerMovement.enabled = true;
            rotateWithMouse.enabled = true;
            m_camera.enabled = true;
            m_camera.gameObject.GetComponent<RotateWithMouse>().enabled = true;
            audioListener.enabled = true;
            playerModel.SetActive(false);
            PublicVariables.myPlayer = gameObject;
            PublicVariables.myAxeAnimator = axeAnimator;
            PublicVariables.myOwnerClientId = OwnerClientId;
            PublicVariables.inGame = true;
            lootScript.SetActive(true);

            string userID = SystemInfo.deviceUniqueIdentifier;

            dbReference.Child("users").Child(userID).Child("pos").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("got pos");
                    DataSnapshot snapshot = task.Result;
                    float x = float.Parse(snapshot.Child("x").Value.ToString());
                    float y = float.Parse(snapshot.Child("y").Value.ToString());
                    float z = float.Parse(snapshot.Child("z").Value.ToString());

                    Vector3 pos = new Vector3(x, y, z);

                    //StartCoroutine(movePlayer(pos));


                }
            });


            if (IsHost)
            {
                StartCoroutine(sendPing());




            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (SceneManager.GetActiveScene().name == "OpenWorld")
            {
                LoadData();
            }

            healthUI.gameObject.SetActive(true);

        }
        else
        {

            this.enabled = false;

        }



    }
    void LoadData()
    {
        dataLoaded = true;
        //spawn buildings
        foreach (var building in PublicVariables.buildings)
        {
            GameObject prefab = Instantiate(building.prefab);

            prefab.transform.position = building.position;
            prefab.transform.localEulerAngles = new Vector3(0, building.yRotation, 0);




            prefab.GetComponent<building>().id = building.id;
            prefab.GetComponent<building>().placed = true;
            prefab.GetComponent<ObjectHealth>().health = building.health;
            if (prefab.GetComponent<Door>() != null)
            {
                prefab.GetComponent<Door>().ownerId = building.ownerId;
            }
            if (prefab.GetComponent<CupBoard>() != null)
            {
                prefab.GetComponent<CupBoard>().owner = building.ownerId;
            }

        }
        foreach (var tree in PublicVariables.treePositions)
        {
            GameObject prefab = (GameObject)Resources.Load("Trees/"+tree.name, typeof(GameObject));
            Instantiate(prefab);
            prefab.transform.position = tree.position;
            prefab.GetComponent<tree>().id = tree.id;
            prefab.GetComponent<tree>().hitCount = tree.hitCount;

        }
        foreach (var lootbag in PublicVariables.lootbags)
        {
            GameObject prefab = Instantiate(lootBag);
            prefab.transform.position = lootbag.position;
            prefab.GetComponent<lootbag>().lootBagId = lootbag.id;

        }
        foreach (var chest in PublicVariables.chests)
        {
            GameObject prefab = Instantiate(chestPrefab);
            prefab.transform.position = chest.position;
            prefab.GetComponent<lootbag>().lootBagId = chest.id;

        }
        foreach (var pos in PublicVariables.hempPositions)
        {
            GameObject prefab = Instantiate(hempPrefab);
            prefab.transform.position = pos;

        }
        //spawn player to position
        GetComponent<CharacterController>().enabled = false;
        transform.position = PublicVariables.lastPlayerPos;
        GetComponent<CharacterController>().enabled = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        sceneLoaded = true;
        //spawn buildings
        if (!dataLoaded) LoadData();
    }

    IEnumerator movePlayer(Vector3 pos)
    {
        yield return new WaitForSeconds(2);
        GetComponent<CharacterController>().enabled = false;
        transform.position = pos;
        GetComponent<CharacterController>().enabled = true;

        Debug.Log("moved to pos: " + pos);
    }

    IEnumerator spawnTree(Vector3 pos, int id)
    {
        yield return new WaitForSeconds(2);
        GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity);


        tree.GetComponent<tree>().id = id;
    }
    IEnumerator spawnFloor(Vector3 pos, int id, int health)
    {
        yield return new WaitForSeconds(2);

        GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity);
        floor.GetComponent<building>().id = id;
        floor.GetComponent<building>().placed = true;
        floor.GetComponent<NetworkObject>().Spawn();
        floor.GetComponent<ObjectHealth>().health = health;
        Debug.Log("Spawned floor");
    }
    IEnumerator spawnWall(Vector3 pos, int id, float yRot, int health)
    {
        yield return new WaitForSeconds(2);
        GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity);
        wall.GetComponent<building>().id = id;
        wall.GetComponent<building>().placed = true;
        wall.GetComponent<NetworkObject>().Spawn();
        wall.transform.localEulerAngles = new Vector3(0, yRot, 0);
        wall.GetComponent<ObjectHealth>().health = health;
        Debug.Log("Spawned wall");
    }
    IEnumerator spawnRoof(Vector3 pos, int id, int health)
    {
        yield return new WaitForSeconds(2);
        GameObject roof = Instantiate(roofPrefab, pos, Quaternion.identity);
        roof.GetComponent<building>().id = id;
        roof.GetComponent<building>().placed = true;
        roof.GetComponent<NetworkObject>().Spawn();
        roof.GetComponent<ObjectHealth>().health = health;
        Debug.Log("Spawned roof");
    }


    IEnumerator sendPing()
    {
        yield return new WaitForSeconds(1);
        if (IsHost)
        {
            int random = UnityEngine.Random.Range(0, 99999);
            dbReference.Child("server").Child("online").SetValueAsync(random);

            StartCoroutine(sendPing());

        }

    }
    [Command]
    void sendTreeData()
    {
        if (!IsHost) return;
        GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
        foreach (GameObject tree in trees)
        {
            int id = tree.GetComponent<tree>().id;
            string name = tree.name.Split('(')[0];
            Data data = new Data
            {
                hitCount = 20,
                x = tree.transform.position.x,
                y = tree.transform.position.y,
                z = tree.transform.position.z,
                name= name,
            };
            string json = JsonUtility.ToJson(data);

            dbReference.Child("server").Child("trees").Child(id.ToString()).SetRawJsonValueAsync(json);

        }

    }
    [Command]
    void sendHempData()
    {
        if (!IsHost) return;
        GameObject[] trees = GameObject.FindGameObjectsWithTag("hemp");
        foreach (GameObject tree in trees)
        {
            int id = tree.GetComponent<Resource>().id;
            Data data = new Data
            {
                x = tree.transform.position.x,
                y = tree.transform.position.y,
                z = tree.transform.position.z,

            };
            string json = JsonUtility.ToJson(data);

            dbReference.Child("server").Child("hemps").Child(id.ToString()).SetRawJsonValueAsync(json);

        }

    }
    public class User
    {
        public float x;
        public float y;
        public float z;
    }
    [Command]
    public void sendPlayerData()
    {
        string userID = SystemInfo.deviceUniqueIdentifier;
        User user = new User
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
        };
        string json = JsonUtility.ToJson(user);

        dbReference.Child("users").Child(userID).Child("pos").SetRawJsonValueAsync(json);

    }



    private void OnApplicationQuit()
    {
        sendPlayerData();
    }

    IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        sendPlayerData();
        StartCoroutine(WaitForSeconds(seconds));
    }
    public class Data
    {
        public float hitCount;
        public float x;
        public float y;
        public float z;
        public string name;
    }
    [Command]
    void muteMusic()
    {
        music.volume = 0;
    }
    [Command]
    void unmute()
    {
        music.volume = 1f;
    }
    [Command]
    void ebinjuttumage()
    {
        PublicVariables.serverRpcs.PlayVideoServerRpc();
    }


    public bool addedInventoryData = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift) && IsOwner)
        {
            PublicVariables.serverRpcs.PlayVineBoomServerRpc(OwnerClientId);
        }
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            // PublicVariables.serverRpcs.PlayVideoServerRpc();
        }




    }



}
