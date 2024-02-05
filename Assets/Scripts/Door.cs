using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{

    public int parentId = 0;
    public Vector3 pos;
    public Vector3 scale;
    public bool placed = false;
    public GameObject rotateAround;
    public float duration = 1f;
    public float factor = 100f;
    public bool open = false;
    public bool inAnimation = false;
    public float delay = 0.5f;
    public Vector3 rotOpen = Vector3.zero;
    public Vector3 rotClose = Vector3.zero;
    public float yRot;
    public string ownerId = "";
    float fac;


    // Start is called before the first frame update
    void Start()
    {
        yRot = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }
    public void rotate()
    {
        if (!inAnimation)
        {

            if (open)
            {
                fac = -1f;
            }
            else
            {
                fac = 1f;
            }
            //StartCoroutine(Delay());
            inAnimation = true;
            open = !open;
            StartCoroutine(RotateAround(gameObject, rotateAround.transform.position, Vector3.up * fac, 90f, 0.5f));

        }

    }

    IEnumerator RotateAround(GameObject gameobject, Vector3 point, Vector3 axis, float angle, float inTimeSecs)
    {
        float currentTime = 0.0f;
        float angleDelta = angle / inTimeSecs; //how many degress to rotate in one second
        float ourTimeDelta = 0;
        while (currentTime < inTimeSecs)
        {
            currentTime += Time.deltaTime;
            ourTimeDelta = Time.deltaTime;
            //Make sure we dont spin past the angle we want.
            if (currentTime > inTimeSecs)
                ourTimeDelta -= (currentTime - inTimeSecs);
            gameObject.transform.RotateAround(point, axis, angleDelta * ourTimeDelta);
            yield return null;
        }
        StartCoroutine(Delay());

    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);
        inAnimation = false;

    }
}
