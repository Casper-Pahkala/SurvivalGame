using UnityEngine;

public class doorsnap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.root.GetComponent<building>() != null)
        {
            if (transform.root.GetComponent<building>().placed)
            {
                gameObject.layer = LayerMask.GetMask("door");
            }
        }
    }
}
