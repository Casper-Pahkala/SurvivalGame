using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PublicVariables : MonoBehaviour
{
    public static TestLobby testlobby;
    public class Building
    {
        public int id;
        public GameObject prefab;
        public Vector3 position;
        public float yRotation;
        public int health;
        public int parentId = 0;
        public float xScale;
        public float yScale;
        public float zScale;
        public string ownerId;

    }
    public class Tree
    {
        public int id;
        public Vector3 position;
        public int hitCount;
        public string name;
    }
    public static VideoPlayer theatrePlayer;
    public static bool inCupBoardArea = false;
    public static Vector3 lastPlayerPos = Vector3.zero;
    public static int woodCount;
    public static VivoxVoiceManager VivoxVoiceManager;
    public static GameObject myPlayer;
    public static Animator myAxeAnimator;
    public static string myUniqueId;
    public static string username = "unnamed player";
    public static ulong myOwnerClientId;
    public static bool inGame = false;
    public static GameEngine gameEngine;
    public static bool isHost = false;
    public static bool disconnected = false;
    public static bool isHitting;
    public static float myX;
    public static float myY;
    public static float myZ;
    public static List<Vector3> hempPositions;
    public static List<Vector3> buildingPositions;
    public static List<Building> buildings;
    public static List<Tree> treePositions;
    public static ServerRpcs serverRpcs;
    public static PlayerNetworkHandler playerNetworkHandler;
    public static GameObject videoPlayer;
    public static Inventory myInventory;
    public Vector3 spawnPos;
    public static GameObject screenPlayer;
    public class LootBag
    {
        public int id;
        public Vector3 position;
        public string owner;
    }
    public static List<LootBag> lootbags;
    public static List<LootBag> chests;

    public static string Version = "1.4";
    // Start is called before the first frame update
    void Start()
    {
        hempPositions = new List<Vector3>();
        lastPlayerPos = spawnPos;
        lootbags = new List<LootBag>();
        chests = new List<LootBag>();
        VivoxVoiceManager = GetComponent<VivoxVoiceManager>();
        buildingPositions = new List<Vector3>();
        buildings = new List<Building> { };
        treePositions = new List<Tree> { };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
