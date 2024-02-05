using UnityEngine;

public class CupBoard : MonoBehaviour
{
    public LayerMask m_LayerMask;
    public float distance;
    public bool clipping;
    public string owner;
    // Start is called before the first frame update
    void Start()
    {

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


        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distance, m_LayerMask);
        int i = 0;
        clipping = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].GetComponent<NotBuiltBuilding>() != null)
            {
                if (hitColliders[i].GetComponent<NotBuiltBuilding>().ownerId != owner)
                {
                    clipping = true;
                    PublicVariables.inCupBoardArea = clipping;
                    //Output all of the collider names

                    Debug.Log("clipping");
                    //Increase the number of Colliders in the array

                }

            }
            i++;
        }

        PublicVariables.inCupBoardArea = clipping;
    }
}
