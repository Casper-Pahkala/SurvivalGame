using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class HotBarItem : MonoBehaviour
{
    public bool isSelected = false;
    Color originalColor;
    float originalAlpha;
    Sprite itemSprite;
    public string currentItem = "Axe";
    public GameObject mainCamera;
    public GameObject Axe;
    public GameObject Builder;

    public GameObject doorPrefab;
    public GameObject doorObject;
    public LayerMask doorMask;
    public LayerMask objectMask;
    public float xObject;
    public float yObject;
    public float zObject;
    public bool canPlace = false;
    public static bool clipping = false;
    public float maxDistance = 5f;
    public Material woodMatError;
    public Material woodMatCanPlace;
    public Material doorMat;
    public Material handleMat;
    public int parentId;
    public int ammo;
    public GameObject sleepingBagPrefab;
    public GameObject sleepingBagObject;

    public GameObject chestPrefab;
    public GameObject chestObject;



    public GameObject m1911;
    public class Dataa
    {
        public float health;
        public float x;
        public float y;
        public float z;
        public float yRot;
        public string ownerId;

    }
    void Start()
    {
        originalColor = GetComponent<Image>().color;


    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            GetComponent<Image>().color = Color.white;
            ShowItem();
        }
        else
        {

            var image = GetComponent<Image>();
            image.color = originalColor;
            if (doorObject)
            {
                doorObject.SetActive(false);

            }

        }
        if (currentItem == "Door" && isSelected)
        {

            Door();
        }
        if (currentItem == "SleepingBag" && isSelected)
        {
            SleepingBag();
        }
        if (currentItem == "Chest" && isSelected)
        {
            Chest();
        }

    }


    void ShowItem()
    {
        if (currentItem == "Axe")
        {
            Axe.SetActive(true);
        }
        else
        {
            Axe.SetActive(false);
        }
        if (currentItem == "Builder")
        {
            Builder.SetActive(true);
        }
        else
        {
            Builder.SetActive(false);
        }
        if (currentItem == "m1911")
        {
            m1911.SetActive(true);
        }
        else
        {
            m1911.SetActive(false);
        }
        

    }
    void UpdateSprite()
    {
        itemSprite = Resources.Load("/Resources/" + currentItem) as Sprite;
        transform.GetChild(0).GetComponent<Image>().sprite = itemSprite;
    }
    void Chest()
    {
        if (!chestObject)
        {
            chestObject = Instantiate(chestPrefab);
            chestObject.transform.parent = transform.root.transform;
            chestObject.transform.localEulerAngles = new Vector3 (0f, 0f, 0f);
        }
        xObject = chestObject.transform.position.x;
        yObject = chestObject.transform.position.y;
        zObject = chestObject.transform.position.z;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, objectMask))
        {
            canPlace = true;
            chestObject.transform.position = hit.point;
            chestObject.SetActive(true);

            xObject = chestObject.transform.position.x;
            yObject = chestObject.transform.position.y;
            zObject = chestObject.transform.position.z;
        }
        else
        {
            chestObject.SetActive(false);
            canPlace = false;
        }
        if (clipping || PublicVariables.inCupBoardArea)
        {
            canPlace = false;
        }
        if (canPlace)
        {
            chestObject.SetActive(true);
            chestObject.GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            chestObject.GetComponent<MeshRenderer>().material = woodMatError;

        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            canPlace = false;
            PlaceChest(hit);
        }
    }
    void SleepingBag()
    {
        if (!sleepingBagObject)
        {
            sleepingBagObject = Instantiate(sleepingBagPrefab);
            sleepingBagObject.transform.parent = transform.root.transform;
        }
        xObject = sleepingBagObject.transform.position.x;
        yObject = sleepingBagObject.transform.position.y;
        zObject = sleepingBagObject.transform.position.z;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, objectMask))
        {
            canPlace = true;
            sleepingBagObject.transform.position = hit.point;
            sleepingBagObject.SetActive(true);

            xObject = sleepingBagObject.transform.position.x;
            yObject = sleepingBagObject.transform.position.y;
            zObject = sleepingBagObject.transform.position.z;

        }
        else
        {
            sleepingBagObject.SetActive(false);
            canPlace = false;
        }
        if (clipping || PublicVariables.inCupBoardArea)
        {
            canPlace = false;
        }
        if (canPlace)
        {
            sleepingBagObject.SetActive(true);
            sleepingBagObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            sleepingBagObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = woodMatError;

        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            canPlace = false;
            PlaceSleepingBag(hit);
        }
    }

    void Door()
    {
        if (!doorObject)
        {
            doorObject = Instantiate(doorPrefab);
        }
        xObject = doorObject.transform.position.x;
        yObject = doorObject.transform.position.y;
        zObject = doorObject.transform.position.z;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, doorMask))
        {
            canPlace = true;
            doorObject.transform.parent = hit.transform.parent;
            doorObject.transform.localPosition = hit.transform.localPosition;
            doorObject.transform.localScale = hit.transform.localScale;
            doorObject.transform.localRotation = hit.transform.localRotation;
            parentId = hit.transform.parent.GetComponent<building>().id;

            doorObject.SetActive(true);


            xObject = doorObject.transform.position.x;
            yObject = doorObject.transform.position.y;
            zObject = doorObject.transform.position.z;
        }
        else
        {
            doorObject.SetActive(false);
            canPlace = false;
        }
        if (clipping || PublicVariables.inCupBoardArea)
        {
            canPlace = false;
        }


        if (canPlace)
        {
            doorObject.SetActive(true);
            doorObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = woodMatCanPlace;
            doorObject.transform.GetChild(1).GetComponent<MeshRenderer>().material = woodMatCanPlace;


        }
        else
        {
            doorObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = woodMatError;
            doorObject.transform.GetChild(1).GetComponent<MeshRenderer>().material = woodMatError;

        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            canPlace = false;
            PlaceDoor(hit);
        }
    }

    void PlaceDoor(RaycastHit hit)
    {
        currentItem = "";
        xObject = doorObject.transform.position.x;
        yObject = doorObject.transform.position.y;
        zObject = doorObject.transform.position.z;
        float yRot = doorObject.transform.rotation.eulerAngles.y;
        doorObject.GetComponent<BoxCollider>().isTrigger = false;
        doorObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = doorMat;
        doorObject.transform.GetChild(1).GetComponent<MeshRenderer>().material = handleMat;
        PublicVariables.buildingPositions.Add(doorObject.transform.position);

        
        int id = Random.Range(0, 9999999);



        writeNewBuilding(id, xObject, yObject, zObject, "doors", yRot);
        int slotIndex = GetComponent<InventoryObject>().slotIndex;
        PublicVariables.myInventory.RemoveSingularItem(slotIndex);
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(slotIndex.ToString()).RemoveValueAsync();
    }
    void PlaceSleepingBag(RaycastHit hit)
    {
        currentItem = "";
        xObject = sleepingBagObject.transform.position.x;
        yObject = sleepingBagObject.transform.position.y;
        zObject = sleepingBagObject.transform.position.z;
        sleepingBagObject.SetActive(false);
        int id = Random.Range(0, 9999999);



        writeNewObject(id, xObject, yObject, zObject, "sleepingbags");
        int slotIndex = GetComponent<InventoryObject>().slotIndex;
        PublicVariables.myInventory.RemoveSingularItem(slotIndex);
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(slotIndex.ToString()).RemoveValueAsync();
    }

    void PlaceChest(RaycastHit hit)
    {
        currentItem = "";
        xObject = chestObject.transform.position.x;
        yObject = chestObject.transform.position.y;
        zObject = chestObject.transform.position.z;
        chestObject.SetActive(false);
        int id = Random.Range(0, 9999999);


        writeNewObject(id, xObject, yObject, zObject, "chests",chestObject.transform.localEulerAngles.y);
        int slotIndex = GetComponent<InventoryObject>().slotIndex;
        PublicVariables.myInventory.RemoveSingularItem(slotIndex);
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(SystemInfo.deviceUniqueIdentifier).Child("items").Child(slotIndex.ToString()).RemoveValueAsync();
    }

    private async void writeNewBuilding(int id, float x, float y, float z, string type, float yRot = 0f)
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
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

        Vector3 pos = new Vector3(x, y, z);

        await dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).SetRawJsonValueAsync(json);
        PublicVariables.serverRpcs.DataWrittenServerRpc(id, pos, type, yRot, SystemInfo.deviceUniqueIdentifier);
    }

    private async void writeNewObject(int id, float x, float y, float z, string type, float yRot = 0f)
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
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

        Vector3 pos = new Vector3(x, y, z);

        await dbReference.Child("server").Child(type).Child(id.ToString()).SetRawJsonValueAsync(json);
        PublicVariables.serverRpcs.DataWrittenServerRpc(id, pos, type, yRot, SystemInfo.deviceUniqueIdentifier);
    }
    private void OnDisable()
    {
        if (doorObject)
        {
            doorObject.SetActive(false);
        }
        if (sleepingBagObject)
        {
            sleepingBagObject.SetActive(false);
        }
        if (chestObject)
        {
            chestObject.SetActive(false);
        }
    }
}
