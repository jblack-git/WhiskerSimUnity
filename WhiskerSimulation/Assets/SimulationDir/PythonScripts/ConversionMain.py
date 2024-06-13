# Whisker Simulation
# 
# ConversionMain runs all three conversion scripts to turn a pcb step into unique obj files for unity
# 6/14/2024
# Authors: Joseph Black
# For Use By UAH/MDA/NASA
import os
import sys
import subprocess

def read_paths(file_path):
    paths = {}
    try:
        with open(file_path, 'r') as file:
            for line in file:
                if '=' not in line:
                    raise ValueError("Invalid format in Paths.txt. Each line must contain a key and a value separated by '='.")
                key, value = line.strip().split('=')
                if not key or not value:
                    raise ValueError("Invalid format in Paths.txt. Keys and values cannot be empty.")
                paths[key] = value.replace("\\", "/")
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return None
    return paths

def main(paths_file):
    script_dir = os.path.dirname(__file__).replace("\\", "/")
    paths = read_paths(paths_file)
    if paths is None:
        print("Failed to read Paths.txt. Exiting.")
        return
    
    blender_executable = paths.get('blender_executable')
    freecad_path = paths.get('freecad_path')

    if not blender_executable or not freecad_path:
        print("Paths for blender_executable or freecad_path are missing in Paths.txt. Exiting.")
        return

    freecad_path = freecad_path.replace("\\", "/")
    blender_executable = blender_executable.replace("\\", "/")

    # Call Separate_Metals_To_STL.py
    separate_metals_script = os.path.join(script_dir, "Separate_Metals_To_STL.py").replace("\\", "/")
    subprocess.run([freecad_path, "--hidden", separate_metals_script])

    # Ensure temp folder exists
    parent_dir = os.path.dirname(script_dir)
    temp_folder = os.path.join(parent_dir, "temp").replace("\\", "/")
    if not os.path.exists(temp_folder):
        print(f"Temp folder not found at {temp_folder}. Exiting.")
        return
    
    # Convert STL to FBX
    for stl_file in os.listdir(temp_folder):
        if stl_file.endswith(".stl"):
            stl_path = os.path.join(temp_folder, stl_file).replace("\\", "/")
            fbx_path = stl_path.replace(".stl", ".fbx")
            subprocess.run([blender_executable, "--background", "--python", os.path.join(script_dir, "STL_To_FBX.py"), "--", stl_path, fbx_path])

    # Create Meshes folder
    meshes_folder = os.path.join(temp_folder, "Meshes").replace("\\", "/")
    if not os.path.exists(meshes_folder):
        os.makedirs(meshes_folder)

    # Separate FBX
    for fbx_file in os.listdir(temp_folder):
        if fbx_file.endswith("_MetalFaces.fbx"):
            fbx_path = os.path.join(temp_folder, fbx_file).replace("\\", "/")
            subprocess.run([blender_executable, "--background", "--python", os.path.join(script_dir, "Separate_FBX.py"), "--", fbx_path, meshes_folder])

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python script_name.py <path_to_Paths.txt>")
    else:
        main(sys.argv[1])
