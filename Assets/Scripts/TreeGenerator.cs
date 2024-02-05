using QFSW.QC;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public int distance;
    [Range(0.01f, 10f)]
    public float scale;

    public GameObject treePrefab;
    public GameObject hempPrefab;
    [Range(0.01f, 10f)]
    public float acceptancePoint;
    public int count = 100;

    public int numForests;
    public float forestSize;
    public float radius;

    void Start()
    {

    }
    [Command]
    public void Generate()
    {
        for (int i = 0; i < count; i++)
        {

            Vector2 randomPoint = Random.insideUnitCircle * distance;

            RaycastHit hit;
            LayerMask layerMask = 0;
            float yP = 0;
            if (Physics.Raycast(new Vector3(randomPoint.x, 100, randomPoint.y), -Vector3.up, out hit))
            {
                if (hit.transform.gameObject.tag == "ground")
                {
                    yP = hit.point.y;
                    int random = Random.Range(1, 16);
                    treePrefab = (GameObject)Resources.Load("Trees/Pine_1_" +random, typeof(GameObject));
                    Instantiate(treePrefab, new Vector3(randomPoint.x, yP, randomPoint.y), Quaternion.identity);
                }

            }



        }
    }
    [Command]
    void forests()
    {
        for (int i = 0; i < numForests; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            float size = Random.Range(0f, forestSize);
            Collider[] colliders = Physics.OverlapSphere(transform.position + pos, size);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("tree"))
                {
                    Destroy(collider.gameObject);
                }
            }
        }
    }

    [Command]
    public void Generatehemp()
    {
        for (int i = 0; i < count; i++)
        {

            Vector2 randomPoint = Random.insideUnitCircle * distance;

            RaycastHit hit;
            LayerMask layerMask = 0;
            float yP = 0;
            if (Physics.Raycast(new Vector3(randomPoint.x, 100, randomPoint.y), -Vector3.up, out hit))
            {
                if (hit.transform.gameObject.tag == "ground")
                {
                    yP = hit.point.y;
                    GameObject hemp = Instantiate(hempPrefab, new Vector3(randomPoint.x, yP, randomPoint.y), Quaternion.identity);
                    hemp.transform.localEulerAngles = new Vector3(-90f, 0, 0);
                    hemp.transform.position = new Vector3(hemp.transform.position.x, hemp.transform.position.y - 1, hemp.transform.position.z);
                }

            }



        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            //Generate();
        }
    }
}
