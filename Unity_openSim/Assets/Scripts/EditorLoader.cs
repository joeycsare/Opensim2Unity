using System;
using UnityEditor;
using UnityEngine;

public class EditorLoader : EditorWindow
{
    bool assemblyEnabled;
    bool dynamicsEnabled;
    bool runtimeEnabled;
    bool meshEnabled;
    bool materialEnabled;
    bool bodyEnabled;
    bool jointEnabled;
    bool loadedState = false;
    private string oName = "Skeleton";
    private string meshFolderpath;
    private Material materialData;
    private string bodyFilepath;
    private string jointFilepath;
    private float scaler = 0.01f;
    private GameObject newObject;
    OpenSimImport osi;

    [MenuItem("OpenSim/Model Loader")]
    public static void ShowWindow()
    {
        EditorLoader wnd = GetWindow<EditorLoader>();
        wnd.titleContent = new GUIContent("Model Loader");
        wnd.maxSize = new Vector2(600, 400);
        wnd.minSize = new Vector2(500, 350);
    }

    void OnGUI()
    {
        if (!loadedState)
        {
            GUILayout.Space(10f);
            oName = EditorGUILayout.TextField("Object Name", oName);
            GUILayout.Space(3f);

            EditorGUILayout.LabelField("File Input", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            meshEnabled = EditorGUILayout.BeginToggleGroup("Use own mesh folder path", meshEnabled);
            meshFolderpath = EditorGUILayout.TextField("", meshFolderpath);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            bodyEnabled = EditorGUILayout.BeginToggleGroup("Use own body file path", bodyEnabled);
            bodyFilepath = EditorGUILayout.TextField("", bodyFilepath);

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            jointEnabled = EditorGUILayout.BeginToggleGroup("Use own joint file path", jointEnabled);
            jointFilepath = EditorGUILayout.TextField("", jointFilepath);

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            materialEnabled = EditorGUILayout.BeginToggleGroup("Use own bone material", materialEnabled);
            materialData = (Material)EditorGUILayout.ObjectField("", materialData, typeof(Material), false);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8f);

            EditorGUILayout.BeginHorizontal();
            assemblyEnabled = EditorGUILayout.BeginToggleGroup("Apply Assembly", assemblyEnabled);
            scaler = EditorGUILayout.FloatField("Bone size scaler", scaler);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            if (assemblyEnabled)
            {
                GUILayout.Space(8f);

                EditorGUILayout.BeginHorizontal();
                dynamicsEnabled = EditorGUILayout.BeginToggleGroup("Inspector Dynamics Enabled", dynamicsEnabled);
                runtimeEnabled = EditorGUILayout.Toggle("Update in Build", runtimeEnabled);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
            }
            GUILayout.Space(20f);

            if (GUILayout.Button("Start Loading"))
            {
                if (!meshEnabled)
                    meshFolderpath = "mesh";
                if (!materialEnabled)
                    materialData = Resources.Load<Material>("BoneMat_1");
                if (!bodyEnabled)
                    bodyFilepath = "jsons/body_data";
                if (!jointEnabled)
                    jointFilepath = "jsons/joint_data";

                newObject = new GameObject(oName);
                newObject.transform.position = Vector3.zero;
                newObject.transform.localScale = 100 * Vector3.one;
                osi = newObject.AddComponent<OpenSimImport>();
                osi.LoadData(meshFolderpath, materialData, bodyFilepath, jointFilepath, assemblyEnabled, dynamicsEnabled, runtimeEnabled, scaler);
                loadedState = true;
            }
        }
        else
        {
            GUILayout.Space(20f);
            if (dynamicsEnabled)
            {
                if (GUILayout.Button("Animate"))
                {
                    osi.Animate();
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("Reset Bones"))
                {
                    osi.ResetAllDynamics();
                }
                GUILayout.Space(5f);
            }

            if (GUILayout.Button("Delete Model"))
            {
                DestroyImmediate(newObject);
                loadedState = false;
            }
            GUILayout.Space(5f);
            if (GUILayout.Button("Add Model"))
            {
                loadedState = false;
            }
            GUILayout.Space(5f);

            if (GUILayout.Button("Safe and close"))
            {
                EditorLoader wnd = GetWindow<EditorLoader>();
                wnd.Close();
            }
        }
    }
}
