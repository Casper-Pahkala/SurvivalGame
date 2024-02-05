using QFSW.QC;
using UnityEngine;

public class CraftingMenu : MonoBehaviour
{
    public GameObject craftingUI;
    public static bool craftinShowing = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (QuantumConsole.gamePaused)
        {
            craftingUI.SetActive(false);
            craftinShowing = false;
            return;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            craftinShowing = !craftinShowing;
            if (craftinShowing)
            {
                Inventory.inventoryShowing = false;
                craftingUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
            else
            {
                craftingUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

        }




    }
    public void OnClick(string name)
    {
        int cost = 300;
        if (name == "Door")
        {
            cost = 300;
        }
        if (name == "Axe")
        {
            cost = 0;
        }
        if (name == "Builder")
        {
            cost = 1000;
        }
        if (name == "m1911")
        {
            cost = 5000;
        }
        if (name == "PistolAmmo")
        {
            cost = 100;
        }
        string type = "Wood";
        if (name == "SleepingBag")
        {
            type = "Cloth";
            cost = 30;
        }
        if (name == "Chest")
        {
            
            cost = 500;
        }
        CraftWithWood(name, cost, type);
    }

    public void CraftWithWood(string name, int cost, string type)
    {
        Debug.Log("Craft pressed");
        int totalWood = 0;
        foreach (var slot in Inventory.inventorySlots)
        {
            if (slot.GetComponent<InventoryObject>().icon != null)
            {
                if (slot.GetComponent<InventoryObject>().icon.name == type)
                {
                    totalWood += slot.GetComponent<InventoryObject>().icon.GetComponent<Item>().count;
                }
            }
        }
        if (totalWood >= cost)
        {
            Debug.Log("bought " + name);

            PublicVariables.myInventory.AddItem(1, name);
            PublicVariables.myInventory.RemoveResource(type, cost);
        }
        else
        {

        }


    }
}
