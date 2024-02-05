using UnityEngine;

public class building : MonoBehaviour
{
    public int hitCount = 10;
    public int id;
    bool clipped;
    public bool placed = false;
    bool addedToList = false;
    public string buildingType;
    bool clipping = false;
    public LayerMask m_LayerMask;
    bool m_Started;

    private void Start()
    {
        m_Started = true;


        if (id == 0)
        {
            id = UnityEngine.Random.Range(0, 9999999);
        }


        StartCoroutine(DatabaseManager.GetBuildingHealth(id, buildingType, (int health) =>
        {
            if (health > 0)
            {
                GetComponent<ObjectHealth>().health = health;
            }


        }));

    }

    private void Update()
    {


    }

    private void OnTriggerEnter(Collider other)
    {
        Builder.clipping = true;
    }

    void FixedUpdate()
    {
        if (buildingType == "doorways" || placed == true) return;
        MyCollisions();
    }

    void MyCollisions()
    {
        if (placed) return;
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        float factor = 2.5f;
        if (buildingType == "doors")
        {
            factor = 2f;
        }
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.lossyScale / factor, transform.rotation, m_LayerMask);
        int i = 0;
        clipping = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            clipping = true;
            Builder.clipping = true;
            HotBarItem.clipping = true;
            //Output all of the collider names


            //Increase the number of Colliders in the array
            i++;

        }

        Builder.clipping = clipping;
        HotBarItem.clipping = clipping;
    }

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
