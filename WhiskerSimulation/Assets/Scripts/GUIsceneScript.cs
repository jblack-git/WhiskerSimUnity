// Whisker Simulation
// 
// GUIsceneScript handles the GUI and setting of values
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Text;
using System;
using System.Threading.Tasks;

// For browsing of folders
#if UNITY_STANDALONE_WIN
    using System.Windows.Forms;
#endif

public class GUIsceneScript : MonoBehaviour
{
    public static GUIsceneScript scene1;
    public InputField InputAmountWhiskers;
    public int whiskerAmount;
    public InputField InputLengthWhiskersMu1;
    public float whiskerLengthMu1;
    public InputField InputDiameterWhiskersMu2;
    public float whiskerDiameterMu2;
    public Toggle InputForceDrop;
    public Toggle InputForceShoot;
    public Toggle InputForceShake;
    public Toggle InputWhiskerInteraction;
    public Toggle InputDemo;
    public InputField InputGrouping;
    public InputField InputShakeFreq;
    public float ShakeFreq;
    public InputField InputGravity;
    public float Gravity;
    public InputField InputSimTime;
    public float SimTime;

    public InputField InputLinVelX;
    public float LinearVelX;
    public InputField InputLinVelY;
    public float LinearVelY;
    public InputField InputLinVelZ;
    public float LinearVelZ;
    public InputField InputAngVelX;
    public float AngularVelX;
    public InputField InputAngVelY;
    public float AngularVelY;
    public InputField InputAngVelZ;
    public float AngularVelZ;

    public float groupingAmount;
    public string selectedFolderPath;
    string stepFilePath;
    public Text progressText;

    private Light cameraLight;

    void Start()
    {
        if (scene1 == null)
        {
            scene1 = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Add a light to the main camera if not already added
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraLight = mainCamera.GetComponent<Light>();
            if (cameraLight == null)
            {
                cameraLight = mainCamera.gameObject.AddComponent<Light>();
            }
            cameraLight.type = LightType.Directional;
            cameraLight.intensity = 1.0f;
        }
    }

    public void setAmount()
    {
        if (!string.IsNullOrWhiteSpace(InputAmountWhiskers.text))
        {
            whiskerAmount = int.Parse(InputAmountWhiskers.text);
        }
        else
        {
            whiskerAmount = 10;
        }
        if (!string.IsNullOrWhiteSpace(InputLengthWhiskersMu1.text))
        {
            whiskerLengthMu1 = float.Parse(InputLengthWhiskersMu1.text);
        }
        else
        {
            whiskerLengthMu1 = 7.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputDiameterWhiskersMu2.text))
        {
            whiskerDiameterMu2 = float.Parse(InputDiameterWhiskersMu2.text);
        }
        else
        {
            whiskerDiameterMu2 = 1.17f;
        }
        if (!string.IsNullOrWhiteSpace(InputGrouping.text))
        {
            groupingAmount = float.Parse(InputGrouping.text);
        }
        else
        {
            groupingAmount = 5.0f;
        }
        /*if (!string.IsNullOrWhiteSpace(InputShakeFreq.text))
        {
            ShakeFreq = float.Parse(InputShakeFreq.text);
        }
        else
        {
            ShakeFreq = 1.0f;
        }*/
        if (!string.IsNullOrWhiteSpace(InputGravity.text))
        {
            Gravity = float.Parse(InputGravity.text) * -1000.0f;
            Physics.gravity = new Vector3(0, Gravity, 0);
        }
        else
        {
            Gravity = 9.81f * -1000.0f;
            Physics.gravity = new Vector3(0, Gravity, 0);
        }
        if (!string.IsNullOrWhiteSpace(InputSimTime.text))
        {
            SimTime = float.Parse(InputSimTime.text);
        }
        else
        {
            SimTime = 10.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputLinVelX.text))
        {
            LinearVelX = float.Parse(InputLinVelX.text) / 1000.0f;
        }
        else
        {
            LinearVelX = 0.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputLinVelY.text))
        {
            LinearVelY = float.Parse(InputLinVelY.text) / 1000.0f;
        }
        else
        {
            LinearVelY = 0.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputLinVelZ.text))
        {
            LinearVelZ = float.Parse(InputLinVelZ.text) / 1000.0f;
        }
        else
        {
            LinearVelZ = 0.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputAngVelX.text))
        {
            AngularVelX = float.Parse(InputAngVelX.text) / 1000.0f;
        }
        else
        {
            AngularVelX = 0.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputAngVelY.text))
        {
            AngularVelY = float.Parse(InputAngVelY.text) / 1000.0f;
        }
        else
        {
            AngularVelY = 0.0f;
        }
        if (!string.IsNullOrWhiteSpace(InputAngVelZ.text))
        {
            AngularVelZ = float.Parse(InputAngVelZ.text) / 1000.0f;
        }
        else
        {
            AngularVelZ = 0.0f;
        }

        //Everything is set so start
        UpdateProgress("Importing board objects and starting simulation...");
        SceneManager.LoadSceneAsync("SimulationScene");
    }

    public async void loadModel()
    {
        // Path to the Paths.txt file
        string executableDirectory = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
        string txtFilePath = Path.Combine(executableDirectory, "SimulationDir", "Config", "Paths.txt");
        string blenderExecutable = "";
        string scriptLocation = "";
        string pythonPath = "";

        try
        {
            // Read the paths from the text file
            var paths = ReadPathsFile(txtFilePath);
            blenderExecutable = paths["blender_executable"];
            scriptLocation = Path.Combine(executableDirectory, "SimulationDir", "PythonScripts");
            pythonPath = paths["python_path"];

            // Update the Paths.txt file with the new step file path
            paths["step_file"] = stepFilePath;
            WritePathsFile(txtFilePath, paths);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error reading or writing paths file: {ex.Message}");
            return;
        }

        // Ensure the Python script and paths file exist
        string scriptPath = Path.Combine(scriptLocation, "ConversionMain.py");
        if (File.Exists(scriptPath) && File.Exists(txtFilePath))
        {
            // Start the Python process asynchronously
            UpdateProgress("Starting ConversionMain.py...");
            UnityEngine.Debug.Log("Starting ConversionMain.py...");
            UnityEngine.Debug.Log("python path: " + pythonPath);
            UnityEngine.Debug.Log("python scripts path: " + scriptPath);
            UnityEngine.Debug.Log("Paths.txt path: "+ txtFilePath);
            await RunPythonScriptAsync(pythonPath, scriptPath, txtFilePath);
            UnityEngine.Debug.Log("ConversionMain.py finished.");
            UpdateProgress("ConversionMain.py finished.");
        }
        else
        {
            UnityEngine.Debug.LogError("ConversionMain.py or Paths.txt file does not exist.");
        }
    }

    private void WritePathsFile(string filePath, Dictionary<string, string> paths)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var kvp in paths)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }
    }

    private Dictionary<string, string> ReadPathsFile(string filePath)
    {
        var paths = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
            {
                paths[parts[0].Trim()] = parts[1].Trim();
            }
        }
        return paths;
    }

    private async Task RunPythonScriptAsync(string pythonPath, string scriptPath, string txtFilePath)
    {
        await Task.Run(() =>
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{txtFilePath}\"",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        });
    }

    private void UpdateProgress(string message)
    {
        if (progressText != null)
        {
            progressText.text = message;
        }
    }



    public void Browse()
    {
#if UNITY_EDITOR
        stepFilePath = EditorUtility.OpenFilePanel("Select STEP file", "", "step");
#elif UNITY_STANDALONE_WIN
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "STEP files (*.step)|*.step";
            openFileDialog.Title = "Select STEP file";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                stepFilePath = openFileDialog.FileName;
            }
        }
#endif

        if (!string.IsNullOrEmpty(stepFilePath))
        {
            UnityEngine.Debug.Log("Selected file: " + stepFilePath);
        }
        else
        {
            UnityEngine.Debug.Log("No file selected.");
        }

        loadModel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.Application.Quit();
        }
    }
}
