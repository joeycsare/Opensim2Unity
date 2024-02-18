using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class OpenSimImport : MonoBehaviour
{
    public static List<Osim_Body> OS_Bodys;
    public static List<Osim_Joint> OS_Joints;

    public string bodyFilePath;
    public string jointFilePath;

    private TextAsset jointFile;
    private TextAsset bodyFile;

    public float scaler = 0.01f;
    public string meshFoldername = "mesh";
    private bool isAssembled = false;
    private bool isDynamic = false;
    private bool runtimeEnabled = false;
    public Material boneMaterial1;

    private void OnEnable()
    {
        if (isDynamic && runtimeEnabled)
        {
            //CreateDynamic();
        }
    }
    public void LoadData(string meshFolder, Material applyMaterialData, string applyjointFile, string applybodyFile, bool applyTransform, bool applyDynamic, bool applyRuntimeEnabled, float applyScaler)
    {
        boneMaterial1 = applyMaterialData;
        bodyFilePath = "jsons/body_data"; // applybodyFile;
        jointFilePath = "jsons/joint_data"; // applyjointFile;
        jointFile = Resources.Load<TextAsset>(jointFilePath);
        bodyFile = Resources.Load<TextAsset>(bodyFilePath);
        isDynamic = applyDynamic;
        runtimeEnabled = applyRuntimeEnabled;
        isAssembled = applyTransform;
        scaler = applyScaler;
        meshFoldername = meshFolder;


        if ((jointFile != null) && (bodyFile != null))
        {
            OS_Joints = JsonConvert.DeserializeObject<List<Osim_Joint>>(jointFile.text);
            OS_Bodys = JsonConvert.DeserializeObject<List<Osim_Body>>(bodyFile.text);
            Debug.Log("JSON Loaded");
        }
        else
        {
            Debug.Log("JSONs NOT AVAILABLE");
        }

        CreateModel();

        if (isAssembled)
        {
            Transformating();

            if (isDynamic)
            {
                CreateDynamic();
            }
        }
    }
    void CreateModel()
    {
        foreach (Osim_Body o in OS_Bodys)
        {
            // Create a new GameObject
            GameObject newObject = new GameObject(o.body_name);

            // Set the parent of the new GameObject
            newObject.transform.parent = this.transform;
            SimPartStats simPartStats = newObject.AddComponent<SimPartStats>();
            simPartStats.skeleton = this;
            simPartStats.thisBody = o;
            simPartStats.thisJoint = new Osim_Joint();
            simPartStats.folderName = meshFoldername;

            newObject.transform.position = Vector3.zero;
            newObject.transform.localScale = Vector3.one;
        }

        foreach (Osim_Joint j in OS_Joints)
        {
            Transform childSys = transform.Find(j.child_sys);
            childSys.GetComponent<SimPartStats>().thisJoint = j;
        }

        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.InitializeData();
        }
    }
    void CreateDynamic()
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            if (sP.thisJoint.coord_name != "no_coord")
            {
                sP.runtimeEnabled = runtimeEnabled;
                sP.InitDynamics();
            }
        }
    }
    void Transformating()
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.Transformating();
        }
    }
    public void ResetAllDynamics()
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.ResetDyn();
        }
    }
    public void Animate()
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.rotation = Random.value * 100;
            sP.DynamicUpdate();
        }
    }
}
