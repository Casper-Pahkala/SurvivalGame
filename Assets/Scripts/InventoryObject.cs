using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryObject : MonoBehaviour, IPointerDownHandler
{
    public int slotIndex = 0;
    public TextMeshProUGUI itemCountText;
    public GameObject icon;
    public static GameObject draggingIcon;
    public bool dragging;
    public GameObject draggingObject;
    public float xOffset = 2f;
    public float yOffset = 2f;
    public static bool inPosition = false;
    public bool inPos = false;
    public static bool itemChanged = false;

    public static bool somethingDragging = false;
    public static GameObject iconToChange;

    public static int toSlotIndex;
    public static int fromSlotIndex;

    Button button;

    public bool a;
    public int aa;

    bool changed = false;
    void Start()
    {
        Inventory.inventorySlots.Add(gameObject);
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        a = inPosition;
        aa = fromSlotIndex;
        if (icon == null)
        {
            dragging = false;
        }
        else
        {
            if (GetComponent<HotBarItem>() != null)
            {
                GetComponent<HotBarItem>().currentItem = icon.name;
            }
        }
        if ((Input.mousePosition.x < transform.position.x + xOffset && Input.mousePosition.x > transform.position.x - xOffset) && (Input.mousePosition.y < transform.position.y + yOffset && Input.mousePosition.y > transform.position.y - yOffset))
        {
            if (somethingDragging)
            {
                inPos = true;
                if (icon != null)
                {
                    iconToChange = icon;
                }
                else
                {
                    iconToChange = null;

                }

            }
            else
            {
                if (inPos)
                {
                    inPos = false;
                    inPosition = false;
                    iconToChange = null;
                }


            }

        }
        else
        {
            if (inPos)
            {
                inPos = false;
                inPosition = false;
                iconToChange = null;
            }


        }
        if (inPos)
        {
            inPosition = true;
            toSlotIndex = slotIndex;
        }
        if (dragging)
        {
            icon.transform.position = Input.mousePosition;


        }


        if (Input.GetMouseButtonUp(0) && dragging)
        {
            if (inPos)
            {
                if (dragging)
                {
                    dragging = false;
                    icon.transform.parent = transform;
                    icon.transform.localPosition = new Vector3(0, 0, 0);
                }
                else
                {
                    //Item dragged to this gameobject


                    toSlotIndex = slotIndex;
                    itemChanged = true;
                    if (fromSlotIndex != 0)
                    {
                        changed = true;
                        PublicVariables.myInventory.ChangeItemPosition(fromSlotIndex, toSlotIndex);
                        itemChanged = true;
                        inPosition = false;
                        somethingDragging = false;
                    }
                    /*
                    icon = draggingIcon;
                    icon.transform.SetParent(transform, false);
                    icon.transform.localPosition = new Vector3(0, 0, 0);
                    itemChanged = true;
                    inPosition= false;
                    somethingDragging = false;
                    if (GetComponent<HotBarItem>() != null)
                    {
                        GetComponent<HotBarItem>().currentItem = icon.gameObject.name;
                    }
                    */
                }



            }
            else
            {
                if (dragging)
                {
                    dragging = false;
                    if (inPos)
                    {
                        somethingDragging = false;
                        icon.transform.parent = transform;
                        icon.transform.localPosition = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        fromSlotIndex = slotIndex;
                        if (inPosition)
                        {
                            //Item left this slot
                            //item is in another slot
                            if (!itemChanged)
                            {
                                //item hasnt changed yet
                                somethingDragging = false;
                                changed = true;
                                PublicVariables.myInventory.ChangeItemPosition(fromSlotIndex, toSlotIndex);
                            }





                        }
                        else
                        {

                            somethingDragging = false;
                            icon.transform.parent = transform;
                            icon.transform.localPosition = new Vector3(0, 0, 0);
                        }
                    }


                }
            }
            dragging = false;
            if (changed)
            {
                changed = false;
                toSlotIndex = 0;
                fromSlotIndex = 0;
            }




        }




    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == 0)
        {
            if (icon != null)
            {
                fromSlotIndex = slotIndex;
                toSlotIndex = 0;
                itemChanged = false;
                draggingIcon = icon;
                dragging = true;
                somethingDragging = true;
                icon.transform.parent = draggingObject.transform;
            }
            else
            {
                draggingIcon = null;
                dragging = false;
                somethingDragging = false;

            }
        }


    }
}
