# Whisker Simulation
# 
# Separate_Metals_To_STL separates metals from non metal faces from the step file and converts to 2 stl files
# 6/14/2024
# Authors: Joseph Black
# For Use By UAH/MDA/NASA
import os
import shutil
import FreeCAD
import FreeCADGui
import ImportGui
import Part
import Mesh

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

def read_palette(file_path):
    palette = {}
    try:
        with open(file_path, 'r') as file:
            content = file.read()
            exec(content, {}, palette)
        # Validate the content
        if 'metal_colors' not in palette or 'copper_colors' not in palette:
            raise ValueError("Missing required dictionaries 'metal_colors' or 'copper_colors'.")
        if not palette['metal_colors'] or not palette['copper_colors']:
            raise ValueError("Dictionaries 'metal_colors' and 'copper_colors' cannot be empty.")
        for color in palette['metal_colors'] + palette['copper_colors']:
            if not isinstance(color, tuple) or len(color) != 3 or not all(isinstance(c, int) and 0 <= c <= 255 for c in color):
                raise ValueError("Each color must be a tuple of three integers between 0 and 255.")
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return None
    return palette

def setup_temp_folder(script_dir):
    parent_dir = os.path.dirname(script_dir)
    temp_folder = os.path.join(parent_dir, "temp").replace("\\", "/")
    if os.path.exists(temp_folder):
        shutil.rmtree(temp_folder)
    os.makedirs(temp_folder)
    return temp_folder

def is_color_in_list(color, color_list):
    r, g, b = color[:3]
    return any((abs(r - cr/255.0) <= 0.01 and abs(g - cg/255.0) <= 0.01 and abs(b - cb/255.0) <= 0.01) for cr, cg, cb in color_list)

def select_metal_copper_faces(doc, metal_colors, copper_colors):
    selection = FreeCADGui.Selection

    try:
        selection.clearSelection()
        for obj in doc.Objects:
            if hasattr(obj, "Shape") and hasattr(obj, "ViewObject"):
                shape = obj.Shape
                view_obj = obj.ViewObject

                for face_index, face in enumerate(shape.Faces):
                    color_found = False
                    if hasattr(view_obj, "DiffuseColor"):
                        colors = view_obj.DiffuseColor
                        if colors and len(colors) > 1:
                            if face_index < len(colors):
                                color = colors[face_index]
                                if isinstance(color, tuple) and len(color) >= 3:
                                    r, g, b = color[:3]
                                    if is_color_in_list((r, g, b), metal_colors) or is_color_in_list((r, g, b), copper_colors):
                                        selection.addSelection(obj, "Face{:d}".format(face_index + 1))
                                        color_found = True
                    if not color_found and hasattr(view_obj, "ShapeColor"):
                        color = view_obj.ShapeColor
                        if isinstance(color, tuple) and len(color) == 3:
                            r, g, b = color[:3]
                            if is_color_in_list((r, g, b), metal_colors) or is_color_in_list((r, g, b), copper_colors):
                                selection.addSelection(obj, "Face{:d}".format(face_index + 1))
                                color_found = True
        print("Selection of metal and copper faces complete.")
    except Exception as e:
        print(f"An error occurred during face selection: {e}")

def select_non_metal_faces(doc, metal_colors, copper_colors):
    selection = FreeCADGui.Selection

    try:
        selection.clearSelection()
        metal_copper_faces = set()
        all_faces = set()

        for obj in doc.Objects:
            if hasattr(obj, "Shape") and hasattr(obj, "ViewObject"):
                shape = obj.Shape
                view_obj = obj.ViewObject

                for face_index, face in enumerate(shape.Faces):
                    all_faces.add((obj, face_index + 1))
                    color_found = False
                    if hasattr(view_obj, "DiffuseColor"):
                        colors = view_obj.DiffuseColor
                        if colors and len(colors) > 1:
                            if face_index < len(colors):
                                color = colors[face_index]
                                if isinstance(color, tuple) and len(color) >= 3:
                                    r, g, b = color[:3]
                                    if is_color_in_list((r, g, b), metal_colors) or is_color_in_list((r, g, b), copper_colors):
                                        metal_copper_faces.add((obj, face_index + 1))
                                        color_found = True
                    if not color_found and hasattr(view_obj, "ShapeColor"):
                        color = view_obj.ShapeColor
                        if isinstance(color, tuple) and len(color) == 3:
                            r, g, b = color[:3]
                            if is_color_in_list((r, g, b), metal_colors) or is_color_in_list((r, g, b), copper_colors):
                                metal_copper_faces.add((obj, face_index + 1))
                                color_found = True

        non_metal_copper_faces = all_faces - metal_copper_faces

        for obj, face_index in non_metal_copper_faces:
            selection.addSelection(obj, "Face{:d}".format(face_index))

        print("Selection of non-metal and non-copper faces complete.")
    except Exception as e:
        print(f"An error occurred during face selection: {e}")

def hide_unselected_faces():
    selection = FreeCADGui.Selection.getSelectionEx()
    
    if not selection:
        print("No faces selected.")
        return

    for sel in selection:
        obj = sel.Object
        selected_faces = set(int(face[4:]) - 1 for face in sel.SubElementNames if face.startswith("Face"))
        
        cumulative_placement = FreeCAD.Placement()
        
        # Calculate the cumulative placement from parent objects
        parent = obj
        while parent:
            cumulative_placement = parent.Placement.multiply(cumulative_placement)
            parent = parent.InList[0] if parent.InList else None
        
        all_faces = set(range(len(obj.Shape.Faces)))
        unselected_faces = all_faces - selected_faces
        
        for i, face_id in enumerate(unselected_faces):
            face = obj.Shape.Faces[face_id]
            
            # Create a new shape with the unselected face
            new_obj = Part.show(face)
            
            # Apply the cumulative placement to the new face
            new_obj.Placement = cumulative_placement
            
            # Assign a new name to the new shape
            new_obj.Label = f"nEW___shAPE_{i}"

        # Hide the original object
        obj.ViewObject.hide()

    print("Unselected faces have been hidden and transformed to their correct spots.")

def select_and_export_to_stl(step_name, output_file_path):
    doc = FreeCAD.ActiveDocument
    if not doc:
        print("No active document found.")
        return
    
    FreeCADGui.Selection.clearSelection()
    
    for obj in doc.Objects:
        if hasattr(obj, "Label") and "neW__FiXED_prt_" in obj.Label:
            FreeCADGui.Selection.addSelection(obj)
        elif obj.Name == step_name:
            FreeCADGui.Selection.addSelection(obj)
    
    doc.recompute()
    print(f"Parts with 'neW__FiXED_prt_' in their label and the assembly '{step_name}' have been selected.")
    
    # Export selected objects to STL
    selection = FreeCADGui.Selection.getSelection()
    if not selection:
        print("No objects selected for export.")
        return
    
    # Create a compound of all selected shapes
    shapes = [obj.Shape for obj in selection if hasattr(obj, "Shape")]
    if shapes:
        compound = shapes[0].multiFuse(shapes[1:]) if len(shapes) > 1 else shapes[0]
        Mesh.export([compound], output_file_path)
        print(f"Exported selected objects to {output_file_path}")
    else:
        print("No valid shapes found for export.")
        
def select_shapes_with_specific_string():
    doc = FreeCAD.ActiveDocument
    if not doc:
        print("No active document found.")
        return
    
    FreeCADGui.Selection.clearSelection()
    
    for obj in doc.Objects:
        if hasattr(obj, "Shape") and "nEW___shAPE" in obj.Label:
            FreeCADGui.Selection.addSelection(obj)
    
    doc.recompute()
    print("Shapes with 'nEW___shAPE' in their names have been selected.")
    
def group_touching_faces_from_selected():
    doc = FreeCAD.ActiveDocument
    if not doc:
        print("No active document found.")
        return
    
    selection = FreeCADGui.Selection.getSelection()
    if not selection:
        print("No shapes selected.")
        return
    
    all_faces = []
    selected_objects = set(selection)
    
    # Collect all faces from selected objects
    for obj in selection:
        if hasattr(obj, "Shape"):
            for face in obj.Shape.Faces:
                all_faces.append(face)
    
    grouped_faces = []
    used_faces = set()
    
    # Create parts from grouped faces
    for i, face in enumerate(all_faces):
        shapes = [Part.Face(face)]
        compound = Part.makeCompound(shapes)
        part = doc.addObject("Part::Feature", f"neW__FiXED_prt_{i}")
        part.Shape = compound
        grouped_faces.append(part)
    
    doc.recompute()
    print(f"Created {len(grouped_faces)} parts from touching faces in the selected shapes.")
    
    # Delete the selected objects
    for obj in selected_objects:
        doc.removeObject(obj.Name)
    
    doc.recompute()
    print("Selected shapes have been deleted.")
    
def select_specific_parts_and_assembly():
    doc = FreeCAD.ActiveDocument
    if not doc:
        print("No active document found.")
        return
    
    FreeCADGui.Selection.clearSelection()
    
    for obj in doc.Objects:
        if hasattr(obj, "Label") and "neW__FiXED_prt_" in obj.Label:
            FreeCADGui.Selection.addSelection(obj)
        elif obj.Name == "Motor":
            FreeCADGui.Selection.addSelection(obj)
    
    doc.recompute()
    print("Parts with 'neW__FiXED_prt_' in their label and the assembly 'Motor' have been selected.")

def main():
    script_dir = os.path.dirname(__file__).replace("\\", "/")
    paths_file = os.path.join(script_dir, "../Config/Paths.txt").replace("\\", "/")
    metal_palette_file = os.path.join(script_dir, "../Config/Metal_Palette.txt").replace("\\", "/")

    if not os.path.exists(paths_file):
        print(f"Paths.txt not found at {paths_file}. Exiting.")
        return
    
    if not os.path.exists(metal_palette_file):
        print(f"Metal_Palette.txt not found at {metal_palette_file}. Exiting.")
        return

    paths = read_paths(paths_file)
    if paths is None:
        print("Failed to read Paths.txt. Exiting.")
        return

    palette = read_palette(metal_palette_file)
    if palette is None:
        print("Failed to read Metal_Palette.txt. Exiting.")
        return

    step_file = paths['step_file']

    metal_colors = palette['metal_colors']
    copper_colors = palette['copper_colors']

    temp_folder = setup_temp_folder(script_dir)

    step_file_name = os.path.splitext(os.path.basename(step_file))[0]

    # Open the STEP file
    print("Opening STEP file")
    ImportGui.open(step_file)
    FreeCADGui.SendMsgToActiveView("ViewFit")
    doc = FreeCAD.activeDocument()
    print("STEP file opened")

    # Select metal faces
    select_non_metal_faces(doc, metal_colors, copper_colors)

    # Hide selected faces and create faces from full objs
    hide_unselected_faces()

    # Clear current selection
    FreeCADGui.Selection.clearSelection()

    # Create shapes 
    select_shapes_with_specific_string()
    
    #Create parts from shapes
    group_touching_faces_from_selected()

    # Select everything else and export metal faces
    select_specific_parts_and_assembly()
    output_filename = os.path.join(temp_folder, step_file_name + "_MetalFaces.stl").replace("\\", "/")
    Mesh.export(FreeCADGui.Selection.getSelection(), output_filename)
    print("Metal Face STL exported")

    # Close current STEP file
    while doc:
        print("Closing document...")
        FreeCAD.closeDocument(doc.Name)
        doc = FreeCAD.activeDocument()
    print("Document closed")

    # Open the STEP file for second type of file
    print("Opening STEP file")
    ImportGui.open(step_file)
    FreeCADGui.SendMsgToActiveView("ViewFit")
    doc = FreeCAD.activeDocument()
    print("STEP file opened")

    # Select metal faces
    select_metal_copper_faces(doc, metal_colors, copper_colors)

    # Hide selected faces and create faces from full objs
    hide_unselected_faces()

    # Clear current selection
    FreeCADGui.Selection.clearSelection()

    # Create shapes 
    select_shapes_with_specific_string()
    
    #Create parts from shapes
    group_touching_faces_from_selected()

    # Select everything else and export non-metal faces
    select_specific_parts_and_assembly()
    output_filename = os.path.join(temp_folder, step_file_name + "_NonMetalFaces.stl").replace("\\", "/")
    Mesh.export(FreeCADGui.Selection.getSelection(), output_filename)
    print("Non Metal Faces STL exported")

    # Close current STEP file
    while doc:
        print("Closing document...")
        FreeCAD.closeDocument(doc.Name)
        doc = FreeCAD.activeDocument()
    print("Document closed")

    # Close FreeCAD
    FreeCADGui.getMainWindow().close()

main()
