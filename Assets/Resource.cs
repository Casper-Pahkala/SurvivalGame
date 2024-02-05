using UnityEngine;

public class Resource : MonoBehaviour
{
    public int id;
    // Start is called before the first frame update
    void Start()
    {
        if (id == 0)
        {
            id = Random.Range(0, 999999999);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
