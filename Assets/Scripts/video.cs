using UnityEngine;
using UnityEngine.Video;

public class video : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        PublicVariables.videoPlayer = gameObject;
        PublicVariables.theatrePlayer = transform.GetChild(0).gameObject.GetComponent<VideoPlayer>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
