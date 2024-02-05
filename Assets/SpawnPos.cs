using UnityEngine;

public class SpawnPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ServerRpcs.spawnPos = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
