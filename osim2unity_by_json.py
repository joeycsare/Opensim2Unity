import opensim as osim
import numpy as np
import configargparse as argparse
import os
import json

default_model = "das3_model.osim"
unity_outdir = "Unity_openSim/Assets/Resources/jsons"

class Body_Geometry: # Class to safe OpenSim Body data
    def __init__(self, body_name, unique_name, mesh, t):
        self.unique_name = unique_name # Identifier of this bodymesh if Body has multiple meshes
        self.body_name = body_name # Name of body in OpenSim
        self.mesh = mesh #mesh name
        self.t = t # Homogen Transform matrix

    def nparray_to_json(self, nparray): # Serializes the t matrix to string 
        datastring = (
            "["
            + "["+ str(nparray[0, 0])+ ","+ str(nparray[0, 1])+ ","+ str(nparray[0, 2])+ ","+ str(nparray[0, 3])+ "];"
            + "["+ str(nparray[1, 0])+ ","+ str(nparray[1, 1])+ ","+ str(nparray[1, 2])+ ","+ str(nparray[1, 3])+ "];"
            + "["+ str(nparray[2, 0])+ ","+ str(nparray[2, 1])+ ","+ str(nparray[2, 2])+ ","+ str(nparray[2, 3])+ "];"
            + "[0, 0, 0, 1]"
            + "]"
        )
        return datastring

    def to_json(self): # serializes class to string
        return {
            "unique_name": str(self.unique_name),
            "body_name": str(self.body_name),
            "mesh": str(self.mesh),
            "tmatrix": self.nparray_to_json(self.t),
        }

class Joint_Connection: # same as body
    def __init__(
        self,
        joint_name, # name of joint
        parent_sys, # parent coordinate system
        child_sys, # child coordinate system
        coord_name, # name of joint coordinate if existent
        coord_axis, # coordinate transformation axis
        motion_type, # style of transformation, 1 rotation, 0 translation
        coord_value, # default value of coordinate 
        coord_upper_bound, # bounts of joint coordinate
        coord_lower_bound,
    ):
        self.joint_name = joint_name
        self.parent_sys = parent_sys
        self.child_sys = child_sys
        self.coord_name = coord_name
        self.coord_axis = coord_axis
        self.motion_type = motion_type
        self.coord_value = coord_value
        self.coord_upper_bound = coord_upper_bound
        self.coord_lower_bound = coord_lower_bound

    def to_json(self):
        return {
            "joint_name": str(self.joint_name),
            "parent_sys": str(self.parent_sys),
            "child_sys": str(self.child_sys),
            "coord_name": str(self.coord_name),
            "coord_axis": str(self.coord_axis),
            "motion_type": str(self.motion_type),
            "coord_value": str(self.coord_value),
            "coord_upper_bound": str(self.coord_upper_bound),
            "coord_lower_bound": str(self.coord_lower_bound),
        }

def appendBodys(body, unique_name, mesh_file_name, initSys, selfSys): # adds body mesh to body list
    body_name = body.getName() 
    p = selfSys.getPositionInGround(initSys) # gets position relative to ground
    r = selfSys.getTransformInGround(initSys).R() # gets rotation as rotationmatrix relative to ground
    alpha = np.pi / 2
    
    
    t_norm = np.array(  # initial Matrix. combines position and rotation in 4x4 hom. matrix
        [
            [r.get(0, 0), r.get(0, 1), r.get(0, 2), p.get(0)],
            [r.get(1, 0), r.get(1, 1), r.get(1, 2), p.get(1)],
            [r.get(2, 0), r.get(2, 1), r.get(2, 2), p.get(2)],
            [0, 0, 0, 1],
        ]
    )
    # rotating and mirroring operations of homogen coordinate transformation. can be queued
    rot = np.array(  # rotating 
        [
            [np.cos(alpha), 0, np.sin(alpha), 0],
            [0, 1, 0, 0],
            [-np.sin(alpha), 0, np.cos(alpha), 0],
            [0, 0, 0, 1],
        ]
    )
    mir = np.array(  # mirroring on x axis
        [
            [-1, 0, 0, 0],
            [0, 1, 0, 0],
            [0, 0, 1, 0],
            [0, 0, 0, 1],
        ]
    )
    t_switchxz_col = np.array(  # initial Matrix with switched colums 1 & 3. mimics transformation from left in right orientated coordinate system 
        [
            [r.get(0, 2), r.get(0, 1), r.get(0, 0), p.get(0)],
            [r.get(1, 2), r.get(1, 1), r.get(1, 0), p.get(1)],
            [r.get(2, 2), r.get(2, 1), r.get(2, 0), p.get(2)],
            [0, 0, 0, 1],
        ]
    )
    t_switchxz_row = np.array(  # initial Matrix with switched rows 1 & 3.
        [
            [r.get(2, 0), r.get(2, 1), r.get(2, 2), p.get(2)],
            [r.get(1, 0), r.get(1, 1), r.get(1, 2), p.get(1)],
            [r.get(0, 0), r.get(0, 1), r.get(0, 2), p.get(0)],
            [0, 0, 0, 1],
        ]
    )

    # t = np.matmul(t, rot)
    # t = np.matmul(t, mir)
    # t = np.round(t_switchxz_col,6)
    # t = np.round(t_switchxz_row,6)
    t = np.round(t_switchxz_row, 6) # OpenSim uses left oriented system, Unity right oriented

    return Body_Geometry(body_name, unique_name, mesh_file_name, t)

def process_files(infile, outdir, debug, export_JSON):
    if not os.path.exists(outdir): # assert output directory
        os.makedirs(outdir)
        if(debug):
            print("Create dir: " + outdir)

    model = osim.Model(infile)
    s = model.initSystem()
    bodies = []
    joints = []

    for body in model.getBodySet():
        check_geom_string = body.getPropertyByName("attached_geometry").toString()

        if check_geom_string == "(Mesh)": # checks if body has a mesh attached directly 
            geom = body.get_attached_geometry(0)
            mesh_file_name = geom.getPropertyByName("mesh_file").toString()
            unique_name = body.getName() + "_1"
            bodies.append(appendBodys(body, unique_name, mesh_file_name, s, body))

        elif check_geom_string == "(No Objects)": # if not attached directly, gets all attached components
            compList = body.getComponentsList()
            if sum(1 for _ in compList) < 3: #if no components, body has no mesh
                mesh_file_name = "none"
                unique_name = body.getName() + "_1"
                bodies.append(appendBodys(body, unique_name, mesh_file_name, s, body))
            else:
                index = 0
                for comp in compList:
                    if comp.hasProperty("attached_geometry"): # checks if found component has mesh
                        if (comp.getPropertyByName("attached_geometry").toString()== "(Mesh)"): # if yes, appends mesh to body list
                            index = index + 1
                            geom = comp.get_attached_geometry(0)
                            mesh_file_name = geom.getPropertyByName("mesh_file").toString()
                            unique_name = body.getName() + "_" + str(index)
                            selfSys = geom.getFrame() 
                            bodies.append(
                                appendBodys(
                                    body, unique_name, mesh_file_name, s, selfSys
                                )
                            )

    if debug: # prints all bodys with data 
        print("--------------------------- Body Data --------------------------------")
        for bod in bodies:
            if bod.mesh != "none":
                print("Name: " + bod.unique_name)
                print("Mesh: " + bod.mesh)
                print("matrix:")
                print(bod.t)
                print("-----------------------------------------------")

    for i, joint in enumerate(model.getJointList()): # gets all joints
        joint_name = joint.getName()

        parent_sys = joint.getParentFrame().findBaseFrame().getName()
        child_sys = joint.getChildFrame().findBaseFrame().getName()

        if isinstance(joint, osim.CustomJoint): # checks if joint is custom an loads coodinate data from joint
            for k in range(0, 2):
                st = joint.get_SpatialTransform().getTransformAxis(k)
                if st.getCoordinateNamesInArray().getSize() > 0:
                    coord_axis = st.getAxis()
                    break
            coord = joint.get_coordinates(0)
            coord_name = coord.getName()
            motion_type = coord.getMotionType()
            coord_value = np.round(np.rad2deg(coord.getValue(s)), 5)
            coord_upper_bound = np.round(np.rad2deg(coord.getRangeMax()), 5)
            coord_lower_bound = np.round(np.rad2deg(coord.getRangeMin()), 5)
        else: 
            coord_name = "no_coord"
            coord_axis = "no_coord"
            motion_type = "no_coord"
            coord_value = 0
            coord_upper_bound = 0
            coord_lower_bound = 0

        joints.append(
            Joint_Connection(
                joint_name,
                parent_sys,
                child_sys,
                coord_name,
                coord_axis,
                motion_type,
                coord_value,
                coord_upper_bound,
                coord_lower_bound,
            )
        )

    if debug:
        print("--------------------------- Joint Data --------------------------------")
        for j in joints:
            print("Joint name: " + j.joint_name)
            print("Attached Coordinate: " + j.coord_name)
            print("Connects system {0} and {1}".format(j.parent_sys, j.child_sys))
            print("-----------------------------------------------------------")
    
    ground = model.get_ground() # Get ground object
    ground.getPropertyByName("attached_geometry").toString()
    if check_geom_string == "(Mesh)":
        geom = ground.get_attached_geometry(0)

    if export_JSON: # exports class lists as jsons
        file_path_body_data = "body_data.json"
        file_path_joint_data = "joint_data.json"

        body_data = []
        joint_data = []

        for body_geometry in bodies:
            body_data.append(body_geometry.to_json())
        for joint_connection in joints:
            joint_data.append(joint_connection.to_json())

        with open(outdir + "/" + file_path_body_data, "w") as file:
            json.dump(body_data, file)

        with open(outdir + "/" + file_path_joint_data, "w") as file:
            json.dump(joint_data, file)

        print("JSONs exported to directory " + outdir)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="OpenSim STL Exporter")
    parser.add_argument("--infile", "-i", default=default_model, help="Path to input file.")
    parser.add_argument("--outdir", "-o", default=unity_outdir, help="Path to output directory.")
    parser.add_argument("--debug", "-d", default=False, action="store_true", help="Print Log Messages")
    parser.add_argument("--export", "-e", default=True, action="store_false", help="Export new JSON Files")
    args = parser.parse_args()
    process_files(args.infile, args.outdir, args.debug, args.export)