using UnityEngine;

public class Interact : MonoBehaviour
{
    public LayerMask layerMask;
    public float distance;
    public GameObject pickUpText;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Camera.main == null) return;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, distance, layerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("hemp"))
            {
                pickUpText.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickUpSomething("hemp", hit.transform.gameObject);
                }
            }
        }
        else
        {
            pickUpText.SetActive(false);
        }
    }

    void PickUpSomething(string name, GameObject prefab)
    {
        if (name == "hemp")
        {

            PublicVariables.serverRpcs.DeleteObjectServerRpc(prefab.GetComponent<Resource>().id, "hemps","hemp");
            PublicVariables.myInventory.AddItem(30, "Cloth");
            Destroy(prefab);
        }
    }
}
