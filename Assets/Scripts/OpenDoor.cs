using Unity.Netcode;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public float distance = 5f;
    public LayerMask layerMask;
    public GameObject openDoorUI;
    public bool readyToOpen = false;
    GameObject door;
    public TMPro.TextMeshProUGUI doorUIText;

    // Start is called before the first frame update
    void Start()
    {
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            this.enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Camera.main == null) return;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, distance, layerMask))
        {
            readyToOpen = true;
            if (hit.transform.root.transform.gameObject.GetComponent<Door>().open)
            {
                doorUIText.text = "Close door";
            }
            else
            {
                doorUIText.text = "Open door";
            }

            openDoorUI.SetActive(true);
            door = hit.transform.parent.transform.gameObject;



        }
        else
        {
            readyToOpen = false;
            openDoorUI.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.E) && readyToOpen)
        {
            Open();
        }
    }

    void Open()
    {
        if (door.GetComponent<Door>().ownerId == SystemInfo.deviceUniqueIdentifier)
        {
            PublicVariables.serverRpcs.OpenDoorServerRpc(door.GetComponent<building>().id);


        }
    }



}
