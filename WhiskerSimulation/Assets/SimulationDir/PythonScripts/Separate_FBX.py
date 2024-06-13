# Whisker Simulation
# 
# Separate_FBX separates all the non touching metal objects in the metal mesh
# 6/14/2024
# Authors: Joseph Black
# For Use By UAH/MDA/NASA
import bpy
import sys
import os

def main():
    # Parse arguments passed to Blender after "--"
    argv = sys.argv
    argv = argv[argv.index("--") + 1:]  # get all args after "--"
    input_file = argv[0]
    output_directory = argv[1]

    # Clear existing mesh data
    bpy.ops.wm.read_factory_settings(use_empty=True)

    # Load the input file
    bpy.ops.import_scene.fbx(filepath=input_file)

    # Ensure the output directory exists
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    # Create a new empty object to act as the parent
    bpy.ops.object.empty_add(type='PLAIN_AXES')
    parent_obj = bpy.context.object
    parent_obj.name = "ParentObject"

    # Iterate over all objects in the scene and separate them if they are meshes
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH':
            # Select the object
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj

            # Enter edit mode and separate by loose parts
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.mesh.separate(type='LOOSE')
            bpy.ops.object.mode_set(mode='OBJECT')

            # Parent the mesh objects to the parent object
            for loose_obj in bpy.context.selected_objects:
                loose_obj.parent = parent_obj
                loose_obj.select_set(False)

    # Rotate the parent object by -90 degrees around the X-axis
    bpy.ops.object.select_all(action='DESELECT')
    bpy.context.view_layer.objects.active = parent_obj
    parent_obj.select_set(True)
    bpy.ops.transform.rotate(value=-1.5708, orient_axis='X')
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)

    # Ensure all separated parts are properly named and exported
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH' and obj.parent == parent_obj:
            # Deselect all objects
            bpy.ops.object.select_all(action='DESELECT')
            obj.select_set(True)
            parent_obj.select_set(True)

            # Generate a unique name for the object
            obj_name = bpy.path.clean_name(obj.name)

            # Export the object to a new file
            filename = obj_name + '.obj'
            filepath = os.path.join(output_directory, filename)
            bpy.ops.export_scene.obj(filepath=filepath, use_selection=True)

            # Deselect the object
            obj.select_set(False)

    # Load the corresponding NonMetalFaces file
    non_metal_file = input_file.replace("MetalFaces.fbx", "NonMetalFaces.fbx")
    if os.path.exists(non_metal_file):
        bpy.ops.import_scene.fbx(filepath=non_metal_file)
        for obj in bpy.context.scene.objects:
            if obj.type == 'MESH' and obj.parent is None:
                obj.parent = parent_obj
                obj.select_set(True)

        # Rotate the newly imported NonMetal objects by -90 degrees around the X-axis
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        bpy.ops.transform.rotate(value=-1.5708, orient_axis='X')
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)

        for obj in bpy.context.selected_objects:
            obj.select_set(False)

    # Ensure all objects are correctly oriented with Z up
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH':
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            bpy.ops.transform.rotate(value=1.5708, orient_axis='X')  # Apply rotation to set Z up
            bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
            obj.select_set(False)

    # Export all objects to individual files
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH' and obj.parent == parent_obj:
            obj.select_set(True)
            parent_obj.select_set(True)

            # Generate a unique name for the object
            obj_name = bpy.path.clean_name(obj.name)

            # Export the object to a new file
            filename = obj_name + '.obj'
            filepath = os.path.join(output_directory, filename)
            bpy.ops.export_scene.obj(filepath=filepath, use_selection=True)

            # Deselect the object
            obj.select_set(False)

    # Quit Blender
    bpy.ops.wm.quit_blender()

if __name__ == "__main__":
    main()
