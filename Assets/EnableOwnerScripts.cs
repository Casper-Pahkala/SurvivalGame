using Unity.Netcode;

public class EnableOwnerScripts : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        if (IsOwner)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (i != 1)
                {
                    transform.GetChild(i).gameObject.SetActive(true);

                }

            }
            GetComponent<PlayerNetworkHandler>().enabled = true;
            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<OpenDoor>().enabled = true;
            GetComponent<head>().enabled = true;


        }
        else
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
