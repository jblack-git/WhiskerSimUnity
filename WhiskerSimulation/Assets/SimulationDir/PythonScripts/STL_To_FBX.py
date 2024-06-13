# Whisker Simulation
# 
# STL_To_FBX simply converts an stl to fbx
# 6/14/2024
# Authors: Joseph Black
# For Use By UAH/MDA/NASA
import bpy
import sys

def main():
    # Parse arguments passed to Blender after "--"
    argv = sys.argv
    argv = argv[argv.index("--") + 1:]  # get all args after "--"
    input_file = argv[0]
    output_file = argv[1]

    # Clear existing mesh data
    bpy.ops.wm.read_factory_settings(use_empty=True)

    # Import the STL file
    bpy.ops.import_mesh.stl(filepath=input_file)

    # Export to FBX
    bpy.ops.export_scene.fbx(filepath=output_file)

if __name__ == "__main__":
    main()
