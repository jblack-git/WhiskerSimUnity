// Whisker Simulation
// 
// WhiskerSpawnerScript handles the forces applied to the whiskers as well as creation
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class WhiskerSpawnerScript : MonoBehaviour
{
    public GameObject whiskerPrefab;
    private int whiskerCounter = 0;
    private System.Random rnd = new System.Random();

    void Start()
    {
        float spreadRadius = GUIsceneScript.scene1.groupingAmount / 2;
        string mystring;

        print("whiskers spawning...\n");

        List<double> whiskerLengths = GenerateLogNormalDistribution(GUIsceneScript.scene1.whiskerLengthMu1, 1.15, GUIsceneScript.scene1.whiskerAmount);
        List<double> whiskerDiameters = GenerateLogNormalDistribution(GUIsceneScript.scene1.whiskerDiameterMu2, 0.67, GUIsceneScript.scene1.whiskerAmount);

        for (int i = 0; i < GUIsceneScript.scene1.whiskerAmount; i++)
        {
            mystring = i.ToString() + "," + whiskerLengths[i].ToString() + "," + whiskerDiameters[i].ToString();

            // Set the initial rotation to 90 degrees around the z-axis
            Quaternion initialRotation = Quaternion.Euler(0, 0, 90);

            GameObject newObject = Instantiate(whiskerPrefab, transform.position, initialRotation);
            newObject.name = "Whisker_" + whiskerCounter++;
            // Dividing by 1000 because the output is in microns
            newObject.transform.localScale = new Vector3(newObject.transform.localScale.x * (float)whiskerLengths[i] / 1000.0f, newObject.transform.localScale.y * (float)whiskerDiameters[i] / 1000.0f, newObject.transform.localScale.z * (float)whiskerDiameters[i] / 1000.0f);
            newObject.transform.position = new Vector3(newObject.transform.position.x + (float)(rnd.NextDouble() * (spreadRadius * 2) - spreadRadius), newObject.transform.position.y, newObject.transform.position.z + (float)(rnd.NextDouble() * (spreadRadius * 2) - spreadRadius));

            if (GUIsceneScript.scene1.InputForceShoot.isOn)
            {
                Rigidbody rb = newObject.GetComponent<Rigidbody>();

                // Apply linear velocity
                Vector3 linearVelocity = new Vector3(
                    GUIsceneScript.scene1.LinearVelX,
                    GUIsceneScript.scene1.LinearVelY,
                    GUIsceneScript.scene1.LinearVelZ
                );
                rb.velocity = linearVelocity;

                // Apply angular velocity
                Vector3 angularVelocity = new Vector3(
                    GUIsceneScript.scene1.AngularVelX,
                    GUIsceneScript.scene1.AngularVelY,
                    GUIsceneScript.scene1.AngularVelZ
                );
                rb.angularVelocity = angularVelocity;
            }
        }
    }

    //https://nepp.nasa.gov/whisker/reference/2009Panashchenko_Log_Normal_L_and_T.pdf
    private List<double> GenerateLogNormalDistribution(double mu, double sigma, int count)
    {
        List<double> values = new List<double>();
        for (int i = 0; i < count; i++)
        {
            double standardNormal = Math.Sqrt(-2.0 * Math.Log(rnd.NextDouble())) * Math.Sin(2.0 * Math.PI * rnd.NextDouble());
            double logNormalValue = Math.Exp(mu + sigma * standardNormal);
            values.Add(logNormalValue);
        }
        return values;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
