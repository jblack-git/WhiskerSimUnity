// Whisker Simulation
// 
// ObjectShake handles a script to shake objects - Currently not in use
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Linq;

public class ObjectShake : MonoBehaviour
{
    float startingPosx = 0;
    float startingPosy = 0;
    float startingPosz = 0;
    
    void Start()
    {
        if (GUIsceneScript.scene1.InputForceShake.isOn)
        {
            //getting relative starting positions
            startingPosx = transform.position.x;
            startingPosy = transform.position.y;
            startingPosz = transform.position.z;
        }
    }

    void Update()
    {
        if (GUIsceneScript.scene1.InputForceShake.isOn)
        {
            //adding random position to mimic shaking
            float x = startingPosx + SimulationSceneScript.scene2.randomx;
            float y = startingPosy + SimulationSceneScript.scene2.randomy;
            float z = startingPosz + SimulationSceneScript.scene2.randomz;
            transform.position = new Vector3(x, y, z);
        }
    }
}
