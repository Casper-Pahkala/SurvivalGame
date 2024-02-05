using System.Collections;
using UnityEngine;

public class DestroyAfterXSeconds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
