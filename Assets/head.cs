using UnityEngine;

public class head : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (hit.transform.gameObject.tag == "bottomroof")
        {
            if (GetComponent<PlayerMovement>().fromground)
            {
                GetComponent<PlayerMovement>().fromground = false;
                Debug.Log("hit roof 1");
                GetComponent<PlayerMovement>().hitRoof = true;
            }

        }
    }

}
