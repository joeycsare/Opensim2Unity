using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Osim_Joint
{
    public string joint_name = "no_name";
    public string parent_sys = "no_sys";
    public string child_sys = "no_sys";
    public string coord_name = "no_coord";
    public string coord_axis = "no_axis";
    public string motion_type = "no_type";
    public string coord_value = "0";
    public string coord_upper_bound = "0";
    public string coord_lower_bound = "0";
}
