// Whisker Simulation
// 
// FileLogger script handles writing out of logs to Application.persistentDataPath directory
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System;
using System.IO;
using UnityEngine;

public class FileLogger : MonoBehaviour
{
    private static FileLogger instance;
    private static StreamWriter logWriter;

    void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads

            // Initialize the logger
            string logFilePath = GetLogFilePath();
            logWriter = new StreamWriter(logFilePath, true);
            logWriter.AutoFlush = true;

            // Register the log callback
            Application.logMessageReceived += LogCallback;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    void OnDestroy()
    {
        // Only unregister and close if this is the instance being destroyed
        if (instance == this)
        {
            Application.logMessageReceived -= LogCallback;

            if (logWriter != null)
            {
                logWriter.Close();
            }
        }
    }

    private void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (logWriter != null)
        {
            logWriter.WriteLine($"{DateTime.Now} [{type}] {condition}");
            if (type == LogType.Exception)
            {
                logWriter.WriteLine(stackTrace);
            }
        }
    }

    private string GetLogFilePath()
    {
        // Get the directory where the executable is running
        string exeDirectory = Directory.GetParent(Application.dataPath).FullName;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string logFileName = $"debug_log_{timestamp}.txt";
        return Path.Combine(exeDirectory, "SimulationDir", "Logs",logFileName);
    }
}
