using UnityEngine;

public class doorwayclipping : MonoBehaviour
{
    public LayerMask m_LayerMask;
    bool m_Started;
    public bool clipping = false;
    void Start()
    {
        m_Started = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        MyCollisions();
    }

    void MyCollisions()
    {
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.lossyScale / 2.5f, transform.rotation, m_LayerMask);
        int i = 0;
        clipping = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            clipping = true;
            Builder.clipping = true;
            //Output all of the collider names


            //Increase the number of Colliders in the array
            i++;

        }
        Builder.clipping = clipping;
    }

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

}
