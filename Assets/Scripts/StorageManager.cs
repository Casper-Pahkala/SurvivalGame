using Firebase.Extensions;
using Firebase.Storage;
using QFSW.QC;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

public class StorageManager : MonoBehaviour
{
    FirebaseStorage storage;
    StorageReference pathReference;
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;

        pathReference = storage.GetReference("videos");
    }
    [Command]
    public static void DownloadVideo(string name)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        string localUrl = Application.persistentDataPath + name + ".mp4";
        VideoPlayer videoPlayer = PublicVariables.videoPlayer.transform.GetChild(0).GetComponent<VideoPlayer>();

        if (System.IO.File.Exists(localUrl))
        {
            Debug.Log("video already downloaded");
            videoPlayer.url = localUrl;
            //videoPlayer.Play();
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);
            return;
        }


        StorageReference pathReference = storage.GetReference("videos");
        // Start downloading a file
        pathReference.Child(name + ".mp4").GetFileAsync(localUrl,
            new StorageProgress<DownloadState>(state =>
            {
                // called periodically during the download
                Debug.Log(String.Format(
                    "Progress: {0} of {1} bytes transferred.",
                    state.BytesTransferred,
                    state.TotalByteCount
                ));

            }), CancellationToken.None).ContinueWithOnMainThread(resultTask =>
            {
                if (!resultTask.IsFaulted && !resultTask.IsCanceled)
                {
                    Debug.Log("Download finished.");

                    PublicVariables.videoPlayer.transform.GetChild(0).GetComponent<VideoPlayer>().url = localUrl;
                    PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);

                }
            });


    }
}
