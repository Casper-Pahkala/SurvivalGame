using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class tree : MonoBehaviour
{
    public int hitCount = 10;
    public int id;


    private void Start()
    {
        if (id == 0)
        {
            id = UnityEngine.Random.Range(0, 9999999);
        }
        if (hitCount <= 0)
        {
            deleteTree();
        }
    }
    private void Update()
    {
        if (hitCount <= 0)
        {
            deleteTree();
        }
    }
    void deleteTree()
    {
        Debug.Log("Deleting tree" + id);
        if (gameObject.activeSelf)
        {
            Destroy(gameObject);

        }
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance
        .GetReference("server").Child("trees").Child(id.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Delete();
                }
            }
        }
        );

    }
    async void Delete()
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        await dbReference.Child("server").Child("trees").Child(id.ToString()).RemoveValueAsync();
        Debug.Log("Deleted " + id);
    }
}
