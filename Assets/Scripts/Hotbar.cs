using Unity.Netcode;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public static int currentHotBarPosition = 1;
    public GameObject item1, item2, item3, item4, item5, item6;
    public static GameObject currentHotbarItem;

    bool isOwner = false;
    void Start()
    {
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }
        if (transform.root.GetComponent<NetworkObject>().IsOwner)
        {
            isOwner = true;
            PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item1.GetComponent<HotBarItem>().currentItem);
            currentHotbarItem = item1;
        }

        ChangeToHotBarPosition(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOwner) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeToHotBarPosition(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeToHotBarPosition(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeToHotBarPosition(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeToHotBarPosition(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeToHotBarPosition(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ChangeToHotBarPosition(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ChangeToHotBarPosition(6);
        }
    }

    void ChangeToHotBarPosition(int position)
    {

        if (currentHotBarPosition == position)
        {

        }
        else
        {
            currentHotBarPosition = position;
            if (position == 1)
            {
                currentHotbarItem = item1;
                item1.GetComponent<HotBarItem>().isSelected = true;
                item2.GetComponent<HotBarItem>().isSelected = false;
                item3.GetComponent<HotBarItem>().isSelected = false;
                item4.GetComponent<HotBarItem>().isSelected = false;
                item5.GetComponent<HotBarItem>().isSelected = false;
                item6.GetComponent<HotBarItem>().isSelected = false;

                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item1.GetComponent<HotBarItem>().currentItem);
            }
            else if (position == 2)
            {
                currentHotbarItem = item2;
                item1.GetComponent<HotBarItem>().isSelected = false;
                item2.GetComponent<HotBarItem>().isSelected = true;
                item3.GetComponent<HotBarItem>().isSelected = false;
                item4.GetComponent<HotBarItem>().isSelected = false;
                item5.GetComponent<HotBarItem>().isSelected = false;
                item6.GetComponent<HotBarItem>().isSelected = false;
                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item2.GetComponent<HotBarItem>().currentItem);
            }
            else if (position == 3)
            {
                currentHotbarItem = item3;
                item1.GetComponent<HotBarItem>().isSelected = false;
                item2.GetComponent<HotBarItem>().isSelected = false;
                item3.GetComponent<HotBarItem>().isSelected = true;
                item4.GetComponent<HotBarItem>().isSelected = false;
                item5.GetComponent<HotBarItem>().isSelected = false;
                item6.GetComponent<HotBarItem>().isSelected = false;
                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item3.GetComponent<HotBarItem>().currentItem);

            }
            else if (position == 4)
            {
                currentHotbarItem = item4;
                item1.GetComponent<HotBarItem>().isSelected = false;
                item2.GetComponent<HotBarItem>().isSelected = false;
                item3.GetComponent<HotBarItem>().isSelected = false;
                item4.GetComponent<HotBarItem>().isSelected = true;
                item5.GetComponent<HotBarItem>().isSelected = false;
                item6.GetComponent<HotBarItem>().isSelected = false;
                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item4.GetComponent<HotBarItem>().currentItem);

            }
            else if (position == 5)
            {
                currentHotbarItem = item5;
                item1.GetComponent<HotBarItem>().isSelected = false;
                item2.GetComponent<HotBarItem>().isSelected = false;
                item3.GetComponent<HotBarItem>().isSelected = false;
                item4.GetComponent<HotBarItem>().isSelected = false;
                item5.GetComponent<HotBarItem>().isSelected = true;
                item6.GetComponent<HotBarItem>().isSelected = false;
                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item5.GetComponent<HotBarItem>().currentItem);

            }
            else if (position == 6)
            {
                currentHotbarItem = item6;
                item1.GetComponent<HotBarItem>().isSelected = false;
                item2.GetComponent<HotBarItem>().isSelected = false;
                item3.GetComponent<HotBarItem>().isSelected = false;
                item4.GetComponent<HotBarItem>().isSelected = false;
                item5.GetComponent<HotBarItem>().isSelected = false;
                item6.GetComponent<HotBarItem>().isSelected = true;
                PublicVariables.serverRpcs.ChangeHeldItemServerRpc(PublicVariables.myOwnerClientId, item6.GetComponent<HotBarItem>().currentItem);

            }
        }
    }
}
