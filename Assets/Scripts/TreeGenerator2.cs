using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator2 : MonoBehaviour
{
    public GameObject treePrefab;
    public float radius;
    public float distance;
    public int numTrees;
    public int numForests;
    public float forestSize;

    void Start()
    {
        
    }
    [Command]
    void trees()
    {
        GenerateTrees();
        GenerateForests();
    }

    void GenerateTrees()
    {
        for (int i = 0; i < numTrees; i++)
        {
            float angle = i * Mathf.PI * 2f / numTrees;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + pos + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity))
            {
                int random = Random.Range(1, 16);
                treePrefab = (GameObject)Resources.Load("Trees/Pine_1_" + random, typeof(GameObject));
                GameObject tree = Instantiate(treePrefab, hit.point, Quaternion.identity);
                tree.transform.up = hit.normal;
                tree.transform.parent = transform;
            }
        }
    }

    void GenerateForests()
    {
        for (int i = 0; i < numForests; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            float size = Random.Range(0f, forestSize);
            Collider[] colliders = Physics.OverlapSphere(transform.position + pos, size);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("Tree"))
                {
                    Destroy(collider.gameObject);
                }
            }
        }
    }
}
