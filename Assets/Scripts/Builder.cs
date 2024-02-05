using Firebase.Database;
using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Builder : MonoBehaviour
{
    public class Dataa
    {
        public float health;
        public float x;
        public float y;
        public float z;
        public float yRot;
        public string ownerId;
    }
    public class Data
    {
        public float health;
        public float x;
        public float y;
        public float z;
        public float yRot;
    }
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public GameObject doorWayPrefab;
    public GameObject doorPrefab;


    public GameObject floorObject;
    public GameObject wallObject;
    public GameObject roofObject;
    public GameObject doorWayObject;
    public GameObject doorObject;

    public LayerMask IgnoreMe;
    public LayerMask wallMask;
    public LayerMask roofMask;
    public LayerMask floorMask;
    public LayerMask doorMask;
    public float xOffset = 10f;
    public float yOffset = 1.0f;
    public float zOffset = 10f;
    public float maxDistance = 5f;
    public Material woodMat;
    public Material woodWallMat;
    public bool canPlace = false;
    public static bool clipping = false;
    public GameObject resourceNotification;
    public static Transform notifications;

    public Material woodMatError;
    public Material woodMatCanPlace;

    public Material woodWallMatError;
    public Material woodWallMatCanPlace;

    public float xFloor;
    public float yFloor;
    public float zFloor;

    public float xWall;
    public float yWall;
    public float zWall;

    public float xRoof;
    public float yRoof;
    public float zRoof;

    public float xObject;
    public float yObject;
    public float zObject;

    public float aa = 3f;
    public float aaa = 2.5f;

    bool clicked = false;
    int totalWood = 0;
    int totalWood2 = 0;
    string[] currentBuildings = { "floor", "wall", "roof", "doorway" };
    int currentBuildingsIndex = 0;

    public string currentBuilding = "floor";
    DatabaseReference dbReference;
    public GameObject currentBuildingText;
    void Start()
    {
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            this.enabled = false;
            return;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        if (!transform.parent.gameObject.transform.parent.GetComponent<NetworkObject>().IsOwner)
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (!clipping && !PublicVariables.inCupBoardArea)
        {
            canPlace = true;
        }
        else
        {
            canPlace = false;
        }
        totalWood2 = 0;
        foreach (var slot in Inventory.inventorySlots)
        {
            if (slot.GetComponent<InventoryObject>().icon != null)
            {
                if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                {
                    totalWood2 += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                }
            }
        }
        if (Inventory.inventoryShowing) return;
        if (Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(1))
        {
            currentBuildingsIndex++;
            currentBuildingsIndex = currentBuildingsIndex % 4;

            currentBuilding = currentBuildings[currentBuildingsIndex];
        }

        if (!floorObject)
        {
            floorObject = Instantiate(floorPrefab);

        }
        if (!wallObject)
        {
            wallObject = Instantiate(wallPrefab);
        }
        if (!roofObject)
        {
            roofObject = Instantiate(roofPrefab);
        }
        if (!doorWayObject)
        {
            doorWayObject = Instantiate(doorWayPrefab);
        }



        if (currentBuilding == "wall")
        {

            Wall();




        }
        else
        {
            wallObject.SetActive(false);
        }

        if (currentBuilding == "floor")
        {

            Floor();
        }
        else
        {
            floorObject.SetActive(false);
        }
        if (currentBuilding == "roof")
        {

            Roof();
        }
        else
        {
            roofObject.SetActive(false);
        }

        if (currentBuilding == "doorway")
        {
            Doorway();
        }
        else
        {
            doorWayObject.SetActive(false);
        }

        currentBuildingText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Current building: "+currentBuilding;




    }

    void Doorway()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, wallMask))
        {



            doorWayObject.SetActive(true);
            float x = hit.point.x / xOffset;
            int xx = Convert.ToInt32(x);
            float xxx = xx * xOffset;
            float xDistanceFromMid = Mathf.Abs(xxx) - Mathf.Abs(hit.point.x);


            float z = hit.point.z / zOffset;
            int zz = Convert.ToInt32(z);
            float zzz = zz * zOffset;


            float zDistanceFromMid = Mathf.Abs(zzz) - Mathf.Abs(hit.point.z);
            if (Mathf.Abs(zDistanceFromMid) > Mathf.Abs(xDistanceFromMid))
            {
                doorWayObject.transform.localEulerAngles = new Vector3(0, 90f, 0);
                if (Mathf.Sign(zzz) == 1)
                {
                    if (zzz > hit.point.z)
                    {
                        zzz = zzz - 2.5f;
                    }
                    else
                    {
                        zzz = zzz + 2.5f;
                    }
                }
                else
                {
                    if (zzz < hit.point.z)
                    {
                        zzz = zzz + 2.5f;
                    }
                    else
                    {
                        zzz = zzz - 2.5f;
                    }
                }
            }
            else
            {
                doorWayObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (Mathf.Sign(xxx) == 1)
                {
                    if (xxx > hit.point.x)
                    {
                        xxx = xxx - 2.5f;
                    }
                    else
                    {
                        xxx = xxx + 2.5f;
                    }
                }
                else
                {
                    if (xxx < hit.point.x)
                    {
                        xxx = xxx + 2.5f;
                    }
                    else
                    {
                        xxx = xxx - 2.5f;
                    }
                }

            }
            float y = hit.transform.position.y;
            float yyy = y + 2.5f;

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("floor"))
            {
                y = hit.transform.position.y;
                yyy = y + aa;
            }
            else
            {
                y = hit.transform.position.y;
                yyy = y + aaa;
            }





            doorWayObject.transform.position = new Vector3(xxx, yyy, zzz);
        }
        else
        {
            doorWayObject.SetActive(false);
            canPlace = false;
        }
        if (clipping)
        {

            canPlace = false;
        }

        if (canPlace)
        {
            Debug.Log("can place doorway");
            doorWayObject.SetActive(true);
            doorWayObject.transform.GetChild(2).GetComponent<MeshRenderer>().material = woodMatCanPlace;
            doorWayObject.transform.GetChild(3).GetComponent<MeshRenderer>().material = woodMatCanPlace;
            doorWayObject.transform.GetChild(4).GetComponent<MeshRenderer>().material = woodMatCanPlace;

        }
        else
        {
            Debug.Log("can not place doorway + " + doorWayObject.transform.GetChild(2).gameObject.name);
            doorWayObject.transform.GetChild(2).GetComponent<MeshRenderer>().material = woodMatError;
            doorWayObject.transform.GetChild(3).GetComponent<MeshRenderer>().material = woodMatError;
            doorWayObject.transform.GetChild(4).GetComponent<MeshRenderer>().material = woodMatError;

        }
        if (totalWood2 < 300)
        {
            doorWayObject.transform.GetChild(2).GetComponent<MeshRenderer>().material = woodMatError;
            doorWayObject.transform.GetChild(3).GetComponent<MeshRenderer>().material = woodMatError;
            doorWayObject.transform.GetChild(4).GetComponent<MeshRenderer>().material = woodMatError;
        }
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            StartCoroutine(DatabaseManager.GetWood((int woodCountDB) =>
            {
                GameObject noti = Instantiate(resourceNotification, notifications);
                Image image = noti.GetComponent<Image>();
                Color tempColor = Color.red;
                tempColor.a = 0.6f;
                image.color = tempColor;
                totalWood = 0;
                foreach (var slot in Inventory.inventorySlots)
                {
                    if (slot.GetComponent<InventoryObject>().icon != null)
                    {
                        if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                        {
                            totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                        }
                    }
                }
                if (totalWood >= 300)
                {
                    PublicVariables.myInventory.RemoveResource("Wood", 300);
                    totalWood -= 300;
                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "-300 (" + totalWood + ")";
                    PlaceDoorway();
                }
                else
                {

                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Not enough (" + totalWood + ")";
                }


            }));

        }
    }



    void Roof()
    {
        xRoof = roofObject.transform.position.x;
        yRoof = roofObject.transform.position.y;
        zRoof = roofObject.transform.position.z;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, roofMask))
        {

            roofObject.SetActive(true);


            roofObject.transform.position = hit.transform.position;
            xRoof = roofObject.transform.position.x;
            yRoof = roofObject.transform.position.y;
            zRoof = roofObject.transform.position.z;
        }
        else
        {
            roofObject.SetActive(false);
            canPlace = false;
        }
        if (clipping)
        {
            canPlace = false;
        }


        if (canPlace)
        {
            roofObject.SetActive(true);
            roofObject.GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            roofObject.GetComponent<MeshRenderer>().material = woodMatError;

        }
        if (totalWood2 < 200)
        {
            roofObject.GetComponent<MeshRenderer>().material = woodMatError;
        }
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            StartCoroutine(DatabaseManager.GetWood((int woodCountDB) =>
            {
                GameObject noti = Instantiate(resourceNotification, notifications);
                Image image = noti.GetComponent<Image>();
                Color tempColor = Color.red;
                tempColor.a = 0.6f;
                image.color = tempColor;
                totalWood = 0;
                foreach (var slot in Inventory.inventorySlots)
                {
                    if (slot.GetComponent<InventoryObject>().icon != null)
                    {
                        if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                        {
                            totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                        }
                    }
                }
                if (totalWood >= 200)
                {
                    PublicVariables.myInventory.RemoveResource("Wood", 200);
                    totalWood -= 200;
                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "-200 (" + totalWood + ")";
                    PlaceRoof();
                }
                else
                {

                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Not enough (" + totalWood + ")";
                }


            }));

        }
    }

    void Floor()
    {
        RaycastHit floorHit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out floorHit, maxDistance, floorMask))
        {
            if (floorHit.transform.gameObject.tag == "ground")
            {

                floorObject.SetActive(true);
                float x = floorHit.point.x / xOffset;
                int xx = Convert.ToInt32(x);
                float xxx = xx * xOffset;

                float y = floorHit.point.y / yOffset;
                int yy = Convert.ToInt32(y);
                float yyy = yy * yOffset;

                float z = floorHit.point.z / zOffset;
                int zz = Convert.ToInt32(z);
                float zzz = zz * zOffset;


                floorObject.transform.position = new Vector3(xxx, yyy, zzz);
                xFloor = floorObject.transform.position.x;
                yFloor = floorObject.transform.position.y;
                zFloor = floorObject.transform.position.z;



            }
            if (floorHit.transform.gameObject.tag == "tree")
            {
                canPlace = false;
                floorObject.SetActive(false);
            }

        }
        else
        {
            floorObject.SetActive(false);
            canPlace = false;
        }
        if (clipping)
        {
            canPlace = false;
        }

        if (canPlace)
        {
            floorObject.SetActive(true);
            floorObject.GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            floorObject.GetComponent<MeshRenderer>().material = woodMatError;

        }

        if (totalWood2 < 300)
        {
            floorObject.GetComponent<MeshRenderer>().material = woodMatError;

        }
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            StartCoroutine(DatabaseManager.GetWood((int woodCountDB) =>
            {
                GameObject noti = Instantiate(resourceNotification, notifications);
                Image image = noti.GetComponent<Image>();
                Color tempColor = Color.red;
                tempColor.a = 0.6f;
                image.color = tempColor;
                totalWood = 0;
                foreach (var slot in Inventory.inventorySlots)
                {
                    if (slot.GetComponent<InventoryObject>().icon != null)
                    {
                        if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                        {
                            totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                        }
                    }
                }
                if (totalWood >= 300)
                {
                    PublicVariables.myInventory.RemoveResource("Wood", 300);
                    totalWood -= 300;
                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "-300 (" + totalWood + ")";
                    PlaceFloor();
                }
                else
                {

                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Not enough (" + totalWood + ")";
                }


            }));

        }
    }

    void Wall()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, wallMask))
        {

            if (!clipping)
            {
                canPlace = true;
            }
            else
            {
                canPlace = false;
            }
            GetWallPosition(hit);






        }
        else
        {
            wallObject.SetActive(false);
            canPlace = false;
        }
        if (clipping)
        {
            canPlace = false;
        }

        if (canPlace)
        {
            wallObject.SetActive(true);
            wallObject.GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            wallObject.GetComponent<MeshRenderer>().material = woodMatError;

        }
        if (totalWood2 < 300)
        {
            wallObject.GetComponent<MeshRenderer>().material = woodMatError;
        }
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            StartCoroutine(DatabaseManager.GetWood((int woodCountDB) =>
            {
                GameObject noti = Instantiate(resourceNotification, notifications);
                Image image = noti.GetComponent<Image>();
                Color tempColor = Color.red;
                tempColor.a = 0.6f;
                image.color = tempColor;
                totalWood = 0;
                foreach (var slot in Inventory.inventorySlots)
                {
                    if (slot.GetComponent<InventoryObject>().icon != null)
                    {
                        if (slot.GetComponent<InventoryObject>().icon.name == "Wood")
                        {
                            totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                        }
                    }
                }
                if (totalWood >= 300)
                {
                    PublicVariables.myInventory.RemoveResource("Wood", 300);
                    totalWood -= 300;
                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "-300 (" + totalWood + ")";
                    PlaceWall();
                }
                else
                {

                    noti.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Not enough (" + totalWood + ")";
                }


            }));

        }
    }

    void GetWallPosition(RaycastHit wallHit)
    {

        wallObject.SetActive(true);
        float x = wallHit.point.x / xOffset;
        int xx = Convert.ToInt32(x);
        float xxx = xx * xOffset;
        float xDistanceFromMid = Mathf.Abs(xxx) - Mathf.Abs(wallHit.point.x);


        float z = wallHit.point.z / zOffset;
        int zz = Convert.ToInt32(z);
        float zzz = zz * zOffset;


        float zDistanceFromMid = Mathf.Abs(zzz) - Mathf.Abs(wallHit.point.z);
        if (Mathf.Abs(zDistanceFromMid) > Mathf.Abs(xDistanceFromMid))
        {
            wallObject.transform.localEulerAngles = new Vector3(0, 90f, 0);
            if (Mathf.Sign(zzz) == 1)
            {
                if (zzz > wallHit.point.z)
                {
                    zzz = zzz - 2.5f;
                }
                else
                {
                    zzz = zzz + 2.5f;
                }
            }
            else
            {
                if (zzz < wallHit.point.z)
                {
                    zzz = zzz + 2.5f;
                }
                else
                {
                    zzz = zzz - 2.5f;
                }
            }
        }
        else
        {
            wallObject.transform.localEulerAngles = new Vector3(0, 0, 0);
            if (Mathf.Sign(xxx) == 1)
            {
                if (xxx > wallHit.point.x)
                {
                    xxx = xxx - 2.5f;
                }
                else
                {
                    xxx = xxx + 2.5f;
                }
            }
            else
            {
                if (xxx < wallHit.point.x)
                {
                    xxx = xxx + 2.5f;
                }
                else
                {
                    xxx = xxx - 2.5f;
                }
            }

        }
        float y = wallHit.transform.position.y;
        float yyy = y + 2.5f;

        if (wallHit.transform.gameObject.layer == LayerMask.NameToLayer("floor"))
        {
            y = wallHit.transform.position.y;
            yyy = y + aa;
        }
        else
        {
            y = wallHit.transform.position.y;
            yyy = y + aaa;
        }





        wallObject.transform.position = new Vector3(xxx, yyy, zzz);

    }



    void PlaceFloor()
    {

        xFloor = floorObject.transform.position.x;
        yFloor = floorObject.transform.position.y;
        zFloor = floorObject.transform.position.z;
        int id = floorObject.GetComponent<building>().id;
        PublicVariables.buildingPositions.Add(floorObject.transform.position);

        PublicVariables.serverRpcs.PlaceBuildingServerRpc(PublicVariables.myOwnerClientId, "floor", floorObject.transform.position, id);
        Destroy(floorObject);

        floorObject = Instantiate(floorPrefab);

        writeNewBuilding(id, xFloor, yFloor, zFloor, "floors");
    }
    void PlaceWall()
    {
        float yRot = wallObject.transform.localEulerAngles.y;
        xWall = wallObject.transform.position.x;
        yWall = wallObject.transform.position.y;
        zWall = wallObject.transform.position.z;
        PublicVariables.buildingPositions.Add(wallObject.transform.position);

        int id = wallObject.GetComponent<building>().id;

        PublicVariables.serverRpcs.PlaceBuildingServerRpc(PublicVariables.myOwnerClientId, "wall", wallObject.transform.position, id, yRot);
        Destroy(wallObject);


        wallObject = Instantiate(wallPrefab);
        writeNewBuilding(id, xWall, yWall, zWall, "walls", yRot);


    }

    void PlaceDoorway()
    {
        float yRot = doorWayObject.transform.localEulerAngles.y;
        xObject = doorWayObject.transform.position.x;
        yObject = doorWayObject.transform.position.y;
        zObject = doorWayObject.transform.position.z;
        PublicVariables.buildingPositions.Add(doorWayObject.transform.position);

        int id = doorWayObject.GetComponent<building>().id;

        Destroy(doorWayObject);


        doorWayObject = Instantiate(doorWayPrefab);
        writeNewBuilding(id, xObject, yObject, zObject, "doorways", yRot);


    }

    void PlaceRoof()
    {
        xRoof = roofObject.transform.position.x;
        yRoof = roofObject.transform.position.y;
        zRoof = roofObject.transform.position.z;
        roofObject.GetComponent<BoxCollider>().isTrigger = false;
        roofObject.GetComponent<MeshRenderer>().material = woodWallMat;
        int id = roofObject.GetComponent<building>().id;
        PublicVariables.buildingPositions.Add(roofObject.transform.position);

        PublicVariables.serverRpcs.PlaceBuildingServerRpc(PublicVariables.myOwnerClientId, "roof", roofObject.transform.position, id);
        Destroy(roofObject);
        roofObject = Instantiate(roofPrefab);
        writeNewBuilding(id, xRoof, yRoof, zRoof, "roofs");
    }
    private void OnDisable()
    {
        if (floorObject)
        {
            floorObject.SetActive(false);
        }
        if (roofObject)
        {
            roofObject.SetActive(false);
        }
        if (wallObject)
        {
            wallObject.SetActive(false);
        }
        currentBuildingText.SetActive(false);


    }

    private async void writeNewBuilding(int id, float x, float y, float z, string type, float yRot = 0)
    {
        Dataa data = new Dataa
        {
            health = 500f,
            x = x,
            y = y,
            z = z,
            yRot = yRot,
            ownerId = SystemInfo.deviceUniqueIdentifier,

        };
        string json = JsonUtility.ToJson(data);
        string first = "server";
        string second = "buildings";
        string third = type;
        string fourth = id.ToString();
        Vector3 pos = new Vector3(x, y, z);


        await dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).SetRawJsonValueAsync(json);
        PublicVariables.serverRpcs.DataWrittenServerRpc(id, pos, type, yRot, SystemInfo.deviceUniqueIdentifier);
    }

    private void OnEnable()
    {
        currentBuildingText.SetActive(true);
    }
    


}
