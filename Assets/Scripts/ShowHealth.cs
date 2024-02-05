using Unity.Netcode;
using UnityEngine;

public class ShowHealth : MonoBehaviour
{
    public float distance = 5f;
    public LayerMask layerMask;
    public GameObject healthUI;
    public GameObject healthText;
    public GameObject healthBar;
    void Start()
    {
        if (!transform.root.transform.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            gameObject.SetActive(false);
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

            int health = 0;
            int maxHealth = 0;
            if (hit.transform.GetComponent<ObjectHealth>() != null)
            {
                health = hit.transform.GetComponent<ObjectHealth>().health;
                maxHealth = hit.transform.GetComponent<ObjectHealth>().maxHealth;
            }
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("doorway"))
            {
                health = hit.transform.gameObject.transform.parent.GetComponent<ObjectHealth>().health;
                maxHealth = hit.transform.gameObject.transform.parent.GetComponent<ObjectHealth>().maxHealth;
            }
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("door"))
            {
                health = hit.transform.parent.GetComponent<ObjectHealth>().health;
                maxHealth = hit.transform.parent.GetComponent<ObjectHealth>().maxHealth;
            }


            float factor = ((float)health) / maxHealth;
            healthUI.SetActive(true);
            healthText.GetComponent<TMPro.TextMeshProUGUI>().text = health + "/" + maxHealth;

            SetRight(healthBar.GetComponent<RectTransform>(), (200f * factor - 200));

        }
        else
        {
            healthUI.SetActive(false);
        }

    }

    public static void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(right, rt.offsetMax.y);
    }
}
