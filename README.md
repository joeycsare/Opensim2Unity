# OpenSim -> Unity converter for "4DTrainer" Project on HKA.

### Slight Adaption of prnthp's repository "opensim-stl-export" to export OpenSim data as stl
https://github.com/prnthp/opensim-stl-export

## Dependencies
1. Python 2.7 Environment
1. [OpenSim Python Wrapper](https://simtk-confluence.stanford.edu/display/OpenSim/Scripting+in+Python)
1. numpy
1. numpy-stl
1. vtk *(for the vtp conversion)*

1. There are no dependencies for Unity. The importer consist of 5 scripts which can be found in the included project. The Unity Version is 2022.3 but conversion to any other version should be no problem 

## Setup
**I highly recommend you create a new Python env in conda because of the strict Python 2.7 requirement and OpenSim's setup process**
1. `conda create -n opensin python=2.7`
1. `conda activate opensin` or `activate opensin`
1.  Follow OpenSim 4.0+ installation [here](https://simtk-confluence.stanford.edu/display/OpenSim/Scripting+in+Python#ScriptinginPython-SettingupyourPythonscriptingenvironment) (YMMV for MacOS!)
1. `conda install numpy numpy-stl`
1. *Optional for vtp2stl.py* `conda install vtk`

### Tutorial
1. Convert all Geometry files to .obj as this is the easiest format to import in Unity. Either use your favorite program or if it's a VTP file, use the included converter. `python vtp_converter.py Geometry -o <output>` should work for most models.
1. run `osim2unity_by_json.py`. The default values can be found at the beginning of the script and are specially for this Unity project. Change them there or use --infile and --outdir. Use the debug -d and export -e flag for testing.
1. .obj files can be a bit tricky in Unity. If you want to be 100% safe, use Blender or something similar to export your files to .fbx, exctract the .mesh files from the fbx in the unity project tab and use the .mesh files as reference
1. The importer is located in the Unity menu bar in the `OpenSim` tab. Open the floating window and choose your options. You can only load the bones, rearange them with the joint info and activate dynamics to rotate the bones with the OpenSim coordinate info. Add your own paths and files if needed.
1. The Editor window creates a new GameObject with an `opensimimport` class. This class loads the json data, loads your bodys, adds the meshes at the right bodys and does the parenting with the joint info from OpenSim. 
1. Every GameObject contains a `SimPartStats` class with all infos about this Body and joint. You can rotate the bones here.

### Bugs
1. Loading of alternative file paths for the json TextAssets through the Editor GUI results in empty json data. For now the alternative paths of the Editor GUI are ignored in the `opensimimport` class and the default paths (`Resources/json`) are used.