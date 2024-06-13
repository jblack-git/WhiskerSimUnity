// Whisker Simulation
// 
// PostBuildProcessor handles the placing of python scripts into the editor folder for builds
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostBuildProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Path to the build folder
        string buildFolder = Path.GetDirectoryName(pathToBuiltProject);

        // List of folders to copy to the build directory
        string[] foldersToCopy = new string[]
        {
            "Assets/SimulationDir"
        };

        foreach (string folder in foldersToCopy)
        {
            string destinationFolder = Path.Combine(buildFolder, Path.GetFileName(folder));
            CopyFolder(folder, destinationFolder);
        }

        Debug.Log("Folders have been copied to the build directory.");
    }

    private static void CopyFolder(string sourceFolder, string destFolder)
    {
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }

        foreach (string file in Directory.GetFiles(sourceFolder))
        {
            string dest = Path.Combine(destFolder, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }

        foreach (string folder in Directory.GetDirectories(sourceFolder))
        {
            string dest = Path.Combine(destFolder, Path.GetFileName(folder));
            CopyFolder(folder, dest);
        }
    }
}
