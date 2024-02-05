using UnityEngine;

public class notifications : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PublicVariables.gameEngine.notifications = transform;
        Axe.notifications = transform;
        Builder.notifications = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
