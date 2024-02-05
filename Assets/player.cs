using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class player : MonoBehaviour
{
    public int health = 100;
    public string player_name;
    public GameObject healthBar;
    public GameObject healthText;
    public int previousHealth = 100;
    public string playerId;
    public Inventory playerInventory;
    public List<string> names;
    public bool addedInventoryData = false;
    void Start()
    {
        names = new List<string>();
    }
    public int count;

    // Update is called once per frame
    void Update()
    {
        count = playerInventory.playerInventoryList.Count;
        foreach (var inv in playerInventory.playerInventoryList)
        {
            if (!names.Contains(inv.name))
            {
                names.Add(inv.name);

            }
        }

        if (playerInventory.playerInventoryList.Count < names.Count)
        {
            names = new List<string>();
            foreach (var inv in playerInventory.playerInventoryList)
            {
                if (!names.Contains(inv.name))
                {
                    names.Add(inv.name);

                }
            }
        }
        if (!addedInventoryData && playerId != null)
        {
            addedInventoryData = true;

            FirebaseDatabase.DefaultInstance
            .GetReference("users").Child(playerId).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    DataSnapshot itemsSnapshot = snapshot.Child("items");
                    foreach (var item in itemsSnapshot.Children)
                    {
                        Inventory.ItemData data = new Inventory.ItemData
                        {
                            name = item.Child("name").Value.ToString(),
                            count = int.Parse(item.Child("count").Value.ToString()),
                            slot = int.Parse(item.Child("slot").Value.ToString()),

                        };
                        GetComponent<player>().playerInventory.playerInventoryList.Add(data);
                    }
                }
            });
        }
        if (previousHealth == health) return;
        if (!transform.root.GetComponent<NetworkObject>().IsOwner) return;

        float factor = ((float)health) / 100;
        healthText.GetComponent<TMPro.TextMeshProUGUI>().text = health + "";

        SetRight(healthBar.GetComponent<RectTransform>(), (230f * factor - 230));
        previousHealth = health;



    }

    public static void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(right, rt.offsetMax.y);
    }
}
