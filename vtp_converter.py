#!/usr/bin/env python
import os
import vtk
import configargparse as argparse


def convertFile(filepath, outdir, format):
    if not os.path.isdir(outdir):
        os.makedirs(outdir)
    if os.path.isfile(filepath):
        basename = os.path.basename(filepath)
        print("Copying file:", basename)
        basename = os.path.splitext(basename)[0]
        outfile = os.path.join(outdir, basename + "." + format)
        reader = vtk.vtkXMLPolyDataReader()
        reader.SetFileName(filepath)
        reader.Update()
        if format == "obj":
            writer = vtk.vtkOBJWriter()
        elif format == "stl":
            writer = vtk.vtkOBJWriter()
        elif format == "gltf":
            writer = vtk.vtkGLTFWriter()
        writer.SetInputConnection(reader.GetOutputPort())
        writer.SetFileName(outfile)
        return writer.Write() == 1
    return False


def convertFiles(indir, outdir):
    files = os.listdir(indir)
    files = [os.path.join(indir, f) for f in files if f.endswith(".vtp")]
    ret = 0
    print("In:", indir)
    print("Out:", outdir)
    for f in files:
        ret += convertFile(f, outdir)
    print("Successfully converted %d out of %d files." % (ret, len(files)))

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="VTP converter")
    parser.add_argument("indir", default="Geometry", help="Path to input directory.")
    parser.add_argument("--format", "-f", default="obj", help="Choose target 3D file format (stl, obj, gltf)")
    parser.add_argument("--outdir", "-o", default="output", help="Path to output directory.")
    args = parser.parse_args()
    ret = args.func(args)
    convertFiles(args.indir, args.outdir, args.format)
