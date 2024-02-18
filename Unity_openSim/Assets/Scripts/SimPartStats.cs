using System;
using System.Globalization;
using UnityEngine;

[ExecuteInEditMode]
public class SimPartStats : MonoBehaviour
{
    private void OnValidate()
    {
        DynamicUpdate();
    }
    private void Update()
    {
        if (runtimeEnabled)
        {
            DynamicUpdate();
        }
    }

    #region Declaration
    [HideInInspector] public OpenSimImport skeleton;
    public Osim_Joint thisJoint;
    public Osim_Body thisBody;
    [HideInInspector] public string folderName = "mesh";

    [Header("Move Bones in % of movement Range")]
    [Range(0, 100)]
    public float rotation = 0;
    [HideInInspector] public float startRotation = 0;
    [HideInInspector] public Quaternion startQuaternion = Quaternion.identity;

    [Header("Movement Options")]
    public bool resetThis = false;
    public bool resetAll = false;

    [Header("Loading Status of this Bone")]
    public bool bodyLoaded = false;
    public bool jointLoaded = false;
    public bool coordLoaded = false;
    public bool isDynamic = false;
    public bool runtimeEnabled = false;


    [HideInInspector] public Transform Iparent_sys;
    [HideInInspector] public Transform Ichild_sys;
    [HideInInspector] public Vector3 Icoord_axis = Vector3.zero;
    [HideInInspector] public int Imotion_type;
    [HideInInspector] public float Icoord_value = 0;
    [HideInInspector] public float Icoord_upper_bound = 0;
    [HideInInspector] public float Icoord_lower_bound = 0;
    [HideInInspector] public Matrix4x4 Itmatrix = Matrix4x4.identity;
    #endregion

    #region Initialization
    public void InitializeData()
    {
        if (thisBody.body_name != "no_name")
        {
            InitBody();
        }
        if (thisJoint.joint_name != "no_name")
        {
            InitJoint();
        }
    }
    private void InitBody()
    {
        if (thisBody.mesh != "none")
        {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.Load<Mesh>(folderName + "/" + thisBody.mesh.ToString().Split('.')[0]);

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = skeleton.boneMaterial1;
        }

        string[] rows = thisBody.tmatrix.Replace("[", string.Empty).Replace("]", string.Empty).Split(';');

        for (int j = 0; j < 4; j++)
        {
            string[] s_entrys = rows[j].Split(',');
            Itmatrix.SetRow(j, new Vector4(float.Parse(s_entrys[0], CultureInfo.InvariantCulture), float.Parse(s_entrys[1], CultureInfo.InvariantCulture), float.Parse(s_entrys[2], CultureInfo.InvariantCulture), float.Parse(s_entrys[3], CultureInfo.InvariantCulture)));
        }
        bodyLoaded = true;
    }
    private void InitJoint()
    {
        Ichild_sys = skeleton.transform.Find(thisJoint.child_sys);
        try
        {
            if (thisJoint.parent_sys != "ground")
            {
                Iparent_sys = skeleton.transform.Find(thisJoint.parent_sys);
            }
            else
            {
                Iparent_sys = skeleton.transform;
            }
        }
        catch
        {
            Debug.Log("Cant find " + thisJoint.parent_sys + " wit Data of Joint " + thisJoint.joint_name);
        }

        jointLoaded = true;

        if (thisJoint.coord_name != "no_coord")
        {
            char[] splitChars = { '[', ']' };
            string[] axisEntrys = thisJoint.coord_axis.Split(splitChars)[1].Split(',');
            Icoord_axis = new Vector3(float.Parse(axisEntrys[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(axisEntrys[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(axisEntrys[2], CultureInfo.InvariantCulture.NumberFormat));

            Imotion_type = int.Parse(thisJoint.motion_type, CultureInfo.InvariantCulture.NumberFormat);
            Icoord_value = float.Parse(thisJoint.coord_value, CultureInfo.InvariantCulture.NumberFormat);
            Icoord_upper_bound = float.Parse(thisJoint.coord_upper_bound, CultureInfo.InvariantCulture.NumberFormat);
            Icoord_lower_bound = float.Parse(thisJoint.coord_lower_bound, CultureInfo.InvariantCulture.NumberFormat);
            coordLoaded = true;
        }
    }
    public void InitDynamics()
    {
        if (coordLoaded)
        {
            isDynamic = true;
            startQuaternion = transform.localRotation;
            rotation = 100 * (Icoord_value - Icoord_lower_bound) / (Icoord_upper_bound - Icoord_lower_bound);
            startRotation = rotation;
        }
    }
    #endregion

    #region Calls
    public void SetAxisPercent(float m_rotation)
    {
        if (isDynamic)
        {
            rotation = Math.Clamp(m_rotation, 0, 100);
            DynamicUpdate();
        }
        else
            Debug.LogWarning("Body " + this.name + " is not dynamic");
    }
    public void SetAxisAngle(float m_angle)
    {
        if (isDynamic)
        {
            rotation = 100 * m_angle / (Icoord_upper_bound - Icoord_lower_bound);
            rotation = Math.Clamp(rotation, 0, 100);
            DynamicUpdate();
        }
        else
            Debug.LogWarning("Body " + this.name + " is not dynamic");
    }
    public void RotateAxisPercent(float m_rotation)
    {
        if (isDynamic)
        {
            rotation += m_rotation;
            rotation = Math.Clamp(rotation, 0, 100);
            DynamicUpdate();
        }
        else
            Debug.LogWarning("Body " + this.name + " is not dynamic");
    }
    public void RotateAxisAngle(float m_angle)
    {
        if (isDynamic)
        {
            rotation += 100 * m_angle / (Icoord_upper_bound - Icoord_lower_bound);
            rotation = Math.Clamp(rotation, 0, 100);
            DynamicUpdate();
        }
        else
            Debug.LogWarning("Body " + this.name + " is not dynamic");
    }
    #endregion

    #region Movement
    public void DynamicUpdate()
    {
        if (isDynamic)
        {
            transform.localRotation = startQuaternion;
            float angle = rotation / 100 * (Icoord_upper_bound - Icoord_lower_bound) + Icoord_lower_bound - Icoord_value;
            transform.Rotate(Icoord_axis, angle);

            if (resetThis)
            {
                resetThis = false;
                ResetDyn();
            }
            if (resetAll)
            {
                resetAll = false;
                skeleton.ResetAllDynamics();
            }
        }
    }
    public void ResetDyn()
    {
        if (isDynamic)
        {
            rotation = startRotation;
            DynamicUpdate();
        }
    }
    #endregion

    #region Transformating
    public void Transformating()
    {
        if (bodyLoaded)
        {
            transform.localScale = ExtractScale(Itmatrix);
            transform.localPosition = ExtractPosition(Itmatrix);
            transform.localRotation = ExtractRotation(Itmatrix);
        }
        if (jointLoaded)
        {
            Ichild_sys.parent = Iparent_sys;
        }
    }
    public Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03 * skeleton.scaler;
        position.y = matrix.m13 * skeleton.scaler;
        position.z = matrix.m23 * skeleton.scaler;
        return position;
    }

    public Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    #endregion
}