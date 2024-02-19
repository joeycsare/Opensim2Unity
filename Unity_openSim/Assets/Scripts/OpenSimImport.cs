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
    public Material boneMaterial1;
    private bool isAssembled = false;
    private bool isDynamic = false;
    private bool runtimeEnabled = false;
    

    public void LoadData(string meshFolder, Material applyMaterialData, string applyjointFile, string applybodyFile, bool applyTransform, bool applyDynamic, bool applyRuntimeEnabled, float applyScaler)
    {
        // Get info from Editoloader
        boneMaterial1 = applyMaterialData;
        bodyFilePath = "jsons/body_data"; // should use applybodyFile instead of this string. Same content but somehow doesnt work
        jointFilePath = "jsons/joint_data"; // applyjointFile;
        jointFile = Resources.Load<TextAsset>(jointFilePath);
        bodyFile = Resources.Load<TextAsset>(bodyFilePath);
        isDynamic = applyDynamic;
        runtimeEnabled = applyRuntimeEnabled;
        isAssembled = applyTransform;
        scaler = applyScaler;
        meshFoldername = meshFolder;

        // check if jsons are available and then load as classes
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
            // Create a new GameObject for every body
            GameObject newObject = new GameObject(o.body_name);

            // Put the new body as child of the OpenSimImport Object and add the data carrier class SimPartStats
            newObject.transform.parent = this.transform;
            SimPartStats simPartStats = newObject.AddComponent<SimPartStats>();
            simPartStats.skeleton = this;
            simPartStats.thisBody = o; // Assign the body to the carrier class
            simPartStats.thisJoint = new Osim_Joint(); 
            simPartStats.folderName = meshFoldername;

            newObject.transform.position = Vector3.zero; // make sure that the body is at the right position
            newObject.transform.localScale = Vector3.one;
        }

        foreach (Osim_Joint j in OS_Joints)
        {
            // Assigns the joint info to the Object with the same name as the child system of the OpenSim joint.
            // OpenSim joints track the movement of the child system compared to the parent system. Parenting comes later
            Transform childSys = transform.Find(j.child_sys); 
            childSys.GetComponent<SimPartStats>().thisJoint = j;
        }

        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.InitializeData(); // Calls all Init functions to load the assigned body and joint 
        }
    }
    void CreateDynamic() // make model moveable
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            if (sP.thisJoint.coord_name != "no_coord") // is there a coordinate attached to this joint/body
            {
                sP.runtimeEnabled = runtimeEnabled; // if you want the Editor handles to be useable in a build
                sP.InitDynamics();
            }
        }
    }
    void Transformating() // Parenting, positioning and rotation of all bodys 
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.Transformating();
        }
    }
    public void ResetAllDynamics() // Resets model in default state
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.ResetDyn();
        }
    }
    public void Animate() // makes a random pose with permitted values
    {
        foreach (SimPartStats sP in GetComponentsInChildren<SimPartStats>())
        {
            sP.rotation = Random.value * 100;
            sP.DynamicUpdate();
        }
    }
}
