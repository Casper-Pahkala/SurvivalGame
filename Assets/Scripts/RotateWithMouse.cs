using QFSW.QC;
using UnityEngine;

public class RotateWithMouse : MonoBehaviour
{
    public GameObject player;
    float verticalRotation = 0f;
    float horizontalRotation = 0f;
    float yRotation;
    float xRotation;
    public GameObject head;
    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    [Range(0.1f, 400f)][SerializeField] float sensitivity = 2f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 43f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void LateUpdate()
    {
        if (QuantumConsole.gamePaused || Inventory.inventoryShowing || CraftingMenu.craftinShowing) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -yRotationLimit, yRotationLimit);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        head.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        player.transform.Rotate(Vector3.up * mouseX);

    }
}
