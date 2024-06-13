// Whisker Simulation
// 
// PostBuildProcessor handles the placing of python scripts into the editor folder for builds
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA

Pre Reqs For Running:
1440p monitor
Windows OS
Blender 3.4
Python 3.8
Unity 2021.3.10f1 for development
FreeCAD 0.21.2

Pre Reqs For Simulation:
Step file of pcb
SimulationDir/Config/Paths.txt including all paths listed
	All paths are supposed to be executables
	Leave step_file empty since that will be set by the simulation
Metal_Palette.txt of colors of Metal and Copper colors


Info:
After launch of WhiskerSimulation.exe, browse for your step file and select it. After loading, the message window
	on the bottom will say "ConversionMain.py has finished...". Input all desired inputs and Press to continue.
	Having Demo Mode checked will determine if the simulation will run headless or not. After the simulation,
	a JSON will be created in the output folder showing which whiskers collided with metal faces. The user
	can select which metal faces to focus on in the scroll view or can type the metal face name in the search
	bar. After clicking on the metal face button or pressing enter after typing a metal face name will zoom
	in and isolate the metal face from the rest of them. Press "Page Up" or "Page Down" to rotate camera views.
	One of the Three cameras will have flying capabilities. The controls are W,A,S,D for moving your current
	plane in direct relation to the center of the screen. Q and E control zooming in and out. Use the mouse to
	rotate the view. Press "Home" to reset the view. Press "esc" to exit out of the report screen.