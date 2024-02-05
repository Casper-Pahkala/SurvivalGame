using UnityEngine;

public class DestroyOnDisconnect : MonoBehaviour
{
    private void Update()
    {
        if (PublicVariables.disconnected)
        {
            Destroy(gameObject);
        }
    }
}
