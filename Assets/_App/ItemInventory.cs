using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Examples;
using Microsoft.MixedReality.WorldLocking.Tools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.OpenVR.Headers;

public class ItemInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] invetoryPool;

    [SerializeField] private Transform inventroyParent;

    [SerializeField] private string fileName = "visuals";

    [SerializeField] private List<PresistentObject> pool = new List<PresistentObject>();
    private List<GameObject> instances = new List<GameObject>();

    [SerializeField] private TextMesh pathText;

    private void Start()
    {
        instances = new List<GameObject>();
        pool = new List<PresistentObject>();

        Load();
    }

    private void Load()
    {
        CheckJsonFile();

        foreach (var item in pool)
        {
            RestoreItem(item.objectIndex, item.pose);
        }
    }

    private void CheckJsonFile()
    {
        string json = GetFile();
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        JSONArray jsonArray = JSON.Parse(json).AsArray;

        // Clear the existing pool
        pool.Clear();

        // Iterate through each JSON object in the array and convert it back to PersistentObject
        foreach (JSONNode jsonObj in jsonArray)
        {
            int objectIndex = jsonObj["objectIndex"];

            Vector3 position = new Vector3(
                jsonObj["position"]["x"].AsFloat, 
                jsonObj["position"]["y"].AsFloat, 
                jsonObj["position"]["z"].AsFloat);

            Quaternion rotation = new Quaternion(
                jsonObj["rotation"]["x"].AsFloat, 
                jsonObj["rotation"]["y"].AsFloat, 
                jsonObj["rotation"]["z"].AsFloat, 
                jsonObj["rotation"]["w"].AsFloat);

            Pose pose = new Pose(position, rotation);
            PresistentObject obj = new PresistentObject(objectIndex, pose);

            pool.Add(obj);
        }
    }
    private string GetFile()
    {
        //Load text from a JSON file (Assets/Resources/Data/visuals.json)
        var targetFile = Resources.Load<TextAsset>("Data/" + fileName);

        if (targetFile == null)
        {
            Debug.Log("File is null");
            pathText.text = "File is null";
            return "";
        }
        pathText.text = Path.Combine(Application.persistentDataPath, fileName + ".json");
        return targetFile.text;
    }


    private Item CreateObject(int index)
    {
        var newItem = Instantiate(invetoryPool[index]);
        newItem.transform.parent = inventroyParent;
        instances.Add(newItem);

        Item itm = newItem.AddComponent<Item>(); 

        return itm;
    }

    public GameObject SpawnItem(int index, Pose worldPose)
    {
        var newItem = CreateObject(index);

        newItem.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);

        Pose localPose = new Pose(newItem.transform.localPosition, newItem.transform.localRotation);

        PresistentObject presistentObject = new PresistentObject(index, localPose);

        newItem.presistentObject = presistentObject;

        pool.Add(presistentObject);
        SaveInventory();

        return newItem.gameObject;
    }

    public GameObject RestoreItem(int index, Pose localPose)
    {
        var newItem = CreateObject(index);

        newItem.transform.SetLocalPose(localPose);

        return newItem.gameObject;
    }

    private string GetPoolJson()
    {
        // Create a JSON array to hold the pool objects
        JSONArray jsonArray = new JSONArray();

        // Iterate over each object in the pool and add its position and rotation to the JSON array
        foreach (var obj in pool)
        {
            JSONObject jsonObj = new JSONObject();

            jsonObj["objectIndex"].AsInt = obj.objectIndex;

            jsonObj["position"]["x"].AsFloat = obj.pose.position.x;
            jsonObj["position"]["y"].AsFloat = obj.pose.position.y;
            jsonObj["position"]["z"].AsFloat = obj.pose.position.z;

            jsonObj["rotation"]["x"].AsFloat = obj.pose.rotation.x;
            jsonObj["rotation"]["y"].AsFloat = obj.pose.rotation.y;
            jsonObj["rotation"]["z"].AsFloat = obj.pose.rotation.z;
            jsonObj["rotation"]["w"].AsFloat = obj.pose.rotation.w;

            jsonArray.Add(jsonObj);
        }

        // Convert the JSON array to a string and print it
        string jsonString = jsonArray.ToString();
        return jsonString;
    }

    public void SaveInventory()
    {
        string json = GetPoolJson();


        // Define the file path differently based on whether it's in the editor or in a build
        string path;
#if UNITY_EDITOR
        // In the editor, save the file to the Assets folder
        path = Path.Combine(Application.dataPath, "Resources/Data/" + fileName + ".json");
#else
    // In a build, save the file to the persistent data path
    path = Path.Combine(Application.persistentDataPath, fileName + ".json");
#endif

        File.WriteAllText(path, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void Clear()
    {
        foreach(GameObject item in instances)
        {
            Destroy(item);
        }
        instances.Clear();

        pool.Clear();
        SaveInventory();
    }

    public void Destroy(GameObject hitObject)
    {
        Item itm = hitObject.GetComponent<Item>();
        if(itm == null)
        {
            return;
        }

        if (pool.Contains(itm.presistentObject))
        {
            int index = pool.IndexOf(itm.presistentObject);
            pool.RemoveAt(index);

            DestroyImmediate(hitObject);

            SaveInventory();
        }
    }
}


[System.Serializable]
public class PresistentObject
{
    public int objectIndex;
    public Pose pose;

    public PresistentObject(int objectIndex, Pose pose)
    {
        this.objectIndex = objectIndex;
        this.pose = pose;
    }
}