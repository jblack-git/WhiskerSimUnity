// Whisker Simulation
// 
// ScoreScript handles Whisker and Metal collisions as well as updating the output JSON file
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ScoreScript : MonoBehaviour
{
    public static string reportFilePath;

    private static string reportFileName;
    private static bool fileCreated = false;
    private static Dictionary<string, List<string>> metalWhiskerCollisions = new Dictionary<string, List<string>>();

    void Start()
    {
        // Only create the file once per execution of Unity
        if (!fileCreated)
        {
            // Set up the JSON file path
            string executableDirectory = Directory.GetParent(Application.dataPath).FullName;
            string outputDirectory = Path.Combine(executableDirectory, "SimulationDir", "Output");
            string baseFileName = GetBaseFileNameForReport();
            reportFileName = GetUniqueFileName(outputDirectory, baseFileName, "json");
            reportFilePath = Path.Combine(outputDirectory, reportFileName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Initialize the JSON file
            InitializeJsonFile(reportFilePath);

            // Print the output file path
            UnityEngine.Debug.Log($"Output JSON file path: {reportFilePath}");

            // Set the flag to true so it doesn't create another file
            fileCreated = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        int whiskerLayer = LayerMask.NameToLayer("Whisker");
        int metalLayer = LayerMask.NameToLayer("Metal");

        int thisObjectLayer = gameObject.layer;
        int collidedObjectLayer = collision.collider.gameObject.layer;

        if ((thisObjectLayer == whiskerLayer && collidedObjectLayer == metalLayer) ||
            (thisObjectLayer == metalLayer && collidedObjectLayer == whiskerLayer))
        {
            string whiskerName = thisObjectLayer == whiskerLayer ? gameObject.name : collision.collider.gameObject.name;
            string metalName = thisObjectLayer == metalLayer ? gameObject.name : collision.collider.gameObject.name;

            if (!metalWhiskerCollisions.ContainsKey(metalName))
            {
                metalWhiskerCollisions[metalName] = new List<string>();
            }

            if (!metalWhiskerCollisions[metalName].Contains(whiskerName))
            {
                metalWhiskerCollisions[metalName].Add(whiskerName);
                UpdateJsonFile(reportFilePath);
            }

        }
        else
        {
            UnityEngine.Debug.Log("Collision did not match layers.");
        }
    }

    private string GetScriptLocationFromPaths(string pathsFilePath)
    {
        foreach (string line in File.ReadLines(pathsFilePath))
        {
            var parts = line.Split('=');
            if (parts.Length == 2 && parts[0].Trim() == "script_location")
            {
                return parts[1].Trim();
            }
        }
        return string.Empty;
    }

    private string GetBaseFileNameForReport()
    {
        string nonMetalObjectName = string.Empty;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("_NonMetal"))
            {
                nonMetalObjectName = obj.name.Split('_')[0];
                break;
            }
        }
        return $"{nonMetalObjectName}_Report";
    }

    private string GetUniqueFileName(string directory, string baseFileName, string extension)
    {
        string fileName = $"{baseFileName}.{extension}";
        int count = 1;

        while (File.Exists(Path.Combine(directory, fileName)))
        {
            fileName = $"{baseFileName}_{count}.{extension}";
            count++;
        }

        return fileName;
    }

    private void InitializeJsonFile(string filePath)
    {
        var initialData = new
        {
            collisions = new Dictionary<string, List<string>>()
        };
        string json = JsonConvert.SerializeObject(initialData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    private void UpdateJsonFile(string filePath)
    {
        var reportData = new { collisions = metalWhiskerCollisions };
        string updatedJson = JsonConvert.SerializeObject(reportData, Formatting.Indented);
        File.WriteAllText(filePath, updatedJson);
    }
}
