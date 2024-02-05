using Firebase.Database;
using UnityEngine;

public class lootbag : MonoBehaviour
{
    public int lootBagId;
    public bool bagEmpty = false;
    void Start()
    {
        if (lootBagId == 0)
        {
            lootBagId = Random.Range(0, 999999999);
        }
        FirebaseDatabase.DefaultInstance
       .GetReference("server").Child("lootbags").Child(lootBagId.ToString()).Child("items")
       .ValueChanged += HandleLootChanged;

    }

    void HandleLootChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        int count = 0;
        foreach (var c in snapshot.Children)
        {
            count++;
        }
        if (count == 0)
        {
            bagEmpty = true;

            FirebaseDatabase.DefaultInstance
            .GetReference("server").Child("lootbags").Child(lootBagId.ToString()).RemoveValueAsync();
        }
        else
        {
            bagEmpty = false;
        }
    }
}
