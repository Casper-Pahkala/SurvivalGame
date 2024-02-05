using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DestroyNetworkObject : NetworkBehaviour
{
    public float seconds;
    void Start()
    {
        StartCoroutine(Timer(seconds));
        if (IsSpawned && IsOwner)
        {
            GetComponent<AudioSource>().volume = 0;
        }
    }

    IEnumerator Timer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (IsServer && IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        if (!IsSpawned)
        {
            Destroy(gameObject);
        }
    }
}
