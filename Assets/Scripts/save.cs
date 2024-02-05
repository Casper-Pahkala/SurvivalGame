using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;



public class save : MonoBehaviour
{

    [System.Serializable]
    public class GameData
    {
        public string deviceId;

        public GameData(string Id)
        {
            deviceId = Id;

        }
    }

    string deviceId;

    void Start()
    {
        SaveFile();
        LoadFile();
    }

    public void SaveFile()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        deviceId = SystemInfo.deviceUniqueIdentifier;
        GameData data = new GameData(deviceId);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public void LoadFile()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        GameData data = (GameData)bf.Deserialize(file);
        file.Close();

        deviceId = data.deviceId;
        PublicVariables.myUniqueId = data.deviceId;


        Debug.Log("My unique ID: " + data.deviceId);

    }

}