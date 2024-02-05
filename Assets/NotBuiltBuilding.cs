using UnityEngine;

public class NotBuiltBuilding : MonoBehaviour
{
    public string ownerId;
    void Start()
    {
        ownerId = SystemInfo.deviceUniqueIdentifier;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
