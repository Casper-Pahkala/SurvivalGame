using UnityEngine;

public class ShootingSound : MonoBehaviour
{
    public Vector3 followPosition;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (followPosition != null)
        {
            transform.position = followPosition;
        }
    }
}
