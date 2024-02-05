using Firebase.Database;
using UnityEngine;

public class ObjectHealth : MonoBehaviour
{
    public int health = 500;
    public int maxHealth = 500;

    private void Start()
    {

    }

    void Update()
    {
        if (health <= 0)
        {
            deleteBuilding();
        }
    }

    async void deleteBuilding()
    {
        int id = GetComponent<building>().id;
        string type = GetComponent<building>().buildingType;
        Debug.Log("Deleting " + id);
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        if (type != null)
        {
            await dbReference.Child("server").Child("buildings").Child(type).Child(id.ToString()).RemoveValueAsync();
            Debug.Log("Deleted " + id);
        }
        if (gameObject.activeSelf)
        {
            Destroy(gameObject);

        }
    }

}