// Whisker Simulation
// 
// SimulationSceneScript handles importing of files and setting up simulation
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Dummiesman; //asset for importing obj files at runtime

// Ensure ReportOutScript is present on the same GameObject
[RequireComponent(typeof(ReportOutScript))]
public class SimulationSceneScript : MonoBehaviour
{
    public static SimulationSceneScript scene2;
    public float randomx = 0;
    public float randomy = 0;
    public float randomz = 0;
    public List<float> numbers = new List<float>();
    private float shakeFrequency = 0.01f; // how fast it shakes
    public static float speed = 0.5f; // how much it shakes
    public static float shakeDistance = 0;
    public static int rCount = 0;
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public GameObject WhiskerSpawner;
    public GameObject container;

    public float timeLimit;
    private float elapsedTime = 0.0f;
    private bool physicsStopped = false;

    public Light camera1Light;
    public Light camera2Light;
    public Light camera3Light;
    private ReportOutScript reportOutScript;

    public bool camera3Enabled = false;

    void Start()
    {
        // singleton creation
        if (scene2 == null)
        {
            scene2 = this;
            DontDestroyOnLoad(gameObject);

            // Get the ReportOutScript component on this GameObject
            reportOutScript = GetComponent<ReportOutScript>();
            if (reportOutScript == null)
            {
                Debug.LogError("ReportOutScript component is not attached to the GameObject.");
                return;
            }

            // Demo check
            if (!GUIsceneScript.scene1.InputDemo.isOn)
            {
                // Find and disable all camera components in the scene
                Camera[] cameras = FindObjectsOfType<Camera>();
                foreach (Camera cam in cameras)
                {
                    cam.enabled = false;
                }
            }
            // Whisker interaction check
            if (!GUIsceneScript.scene1.InputWhiskerInteraction.isOn)
            {
                //Whisker layer is 6
                Physics.IgnoreLayerCollision(6, 6, true);
            }

            // Shake start
            if (GUIsceneScript.scene1.InputForceShake.isOn)
            {
                shakeDistance = 1;//(float)GUIsceneScript.scene1.ShakeFreq.value / 20; // distance between shake positions
                // Creates a list of random numbers that all the objects will use for shaking
                for (int i = 0; i < 1000; i++)
                {
                    System.Random r = new System.Random();
                    float d = RangedFloat(0, shakeDistance, r);
                    numbers.Add(d);
                }
                // Coroutine helps to make sure each object is synchronized
                StartCoroutine("UpdateInterval");
            }

            // Camera setup
            camera2.enabled = false;
            camera3.enabled = false;

            // Add lights to cameras if not already added
            camera1Light = camera1.GetComponent<Light>();
            if (camera1Light == null)
            {
                camera1Light = camera1.gameObject.AddComponent<Light>();
            }
            camera1Light.type = LightType.Directional;
            camera1Light.intensity = 1.0f;
            camera1Light.shadows = LightShadows.Soft;
            camera1Light.shadowStrength = 0.8f;

            camera2Light = camera2.GetComponent<Light>();
            if (camera2Light == null)
            {
                camera2Light = camera2.gameObject.AddComponent<Light>();
            }
            camera2Light.type = LightType.Directional;
            camera2Light.intensity = 1.0f;
            camera2Light.shadows = LightShadows.Soft;
            camera2Light.shadowStrength = 0.8f;
            camera2Light.enabled = false;

            camera3Light = camera3.GetComponent<Light>();
            if (camera3Light == null)
            {
                camera3Light = camera3.gameObject.AddComponent<Light>();
            }
            camera3Light.type = LightType.Directional;
            camera3Light.intensity = 1.0f;
            camera3Light.shadows = LightShadows.Soft;
            camera3Light.shadowStrength = 0.8f;
            camera3Light.enabled = false;

            // Create the container
            container = new GameObject("Container");

            // Import OBJ files and set up the scene
            ImportOBJFiles();

            // Create the container with flat planes
            CreateContainer();

            // Position the WhiskerSpawner
            PositionWhiskerSpawner();

            // Center the camera
            CenterCamera();

            // Start simulation time
            timeLimit = GUIsceneScript.scene1.SimTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
        {
            if (camera3Enabled)
            {
                // Cycle through the three cameras
                if (camera1.enabled)
                {
                    camera1.enabled = false;
                    camera2.enabled = true;
                    camera1Light.enabled = false;
                    camera2Light.enabled = true;
                    camera3Light.enabled = false;
                }
                else if (camera2.enabled)
                {
                    camera2.enabled = false;
                    camera3.enabled = true;
                    camera2Light.enabled = false;
                    camera3Light.enabled = true;
                    camera1Light.enabled = false;
                }
                else
                {
                    camera3.enabled = false;
                    camera1.enabled = true;
                    camera3Light.enabled = false;
                    camera1Light.enabled = true;
                    camera2Light.enabled = false;
                }
            }
            else
            {
                // Swap between camera1 and camera2
                camera1.enabled = !camera1.enabled;
                camera2.enabled = !camera2.enabled;
                camera1Light.enabled = camera1.enabled;
                camera2Light.enabled = camera2.enabled;
            }
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            if (reportOutScript != null)
            {
                Debug.LogError("Ending...");
                reportOutScript.StopAllSimulations();
            }
            else
            {
                Debug.LogError("ReportOutScript not found on reportOutManager.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Home))
        {
            if (reportOutScript != null)
            {
                reportOutScript.ResetCameraView();
            }
            else
            {
                Debug.LogError("ReportOutScript not found on reportOutManager.");
            }
        }

        if (camera3.enabled)
        {
            // Add user control for camera3 (fly-through)
            float moveSpeed = 100f * Time.deltaTime;
            float rotateSpeed = 200f * Time.deltaTime;

            if (Input.GetKey(KeyCode.Q)) camera3.transform.Translate(Vector3.forward * moveSpeed);
            if (Input.GetKey(KeyCode.E)) camera3.transform.Translate(-Vector3.forward * moveSpeed);
            if (Input.GetKey(KeyCode.A)) camera3.transform.Translate(-Vector3.right * moveSpeed);
            if (Input.GetKey(KeyCode.D)) camera3.transform.Translate(Vector3.right * moveSpeed);
            if (Input.GetKey(KeyCode.W)) camera3.transform.Translate(Vector3.up * moveSpeed);
            if (Input.GetKey(KeyCode.S)) camera3.transform.Translate(-Vector3.up * moveSpeed);

            float rotationX = camera3.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeed;
            float rotationY = camera3.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * rotateSpeed;
            camera3.transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        }
    }

    void FixedUpdate()
    {
        if (physicsStopped)
            return;

        // Increment elapsed time by the fixed delta time
        elapsedTime += Time.fixedDeltaTime;

        // Check if the elapsed time exceeds the time limit
        if (elapsedTime >= timeLimit)
        {
            if (reportOutScript != null)
            {
                physicsStopped = true;
                reportOutScript.StopAllSimulations();
            }
            else
            {
                Debug.LogError("ReportOutScript not found on reportOutManager for finish time.");
            }
        }
    }

    // Coroutine for calculating shake position change
    IEnumerator UpdateInterval()
    {
        for (; ; )
        {
            if (rCount >= 997)
                rCount = 0;
            randomx = numbers[rCount];
            randomy = numbers[rCount + 1];
            randomz = numbers[rCount + 2];
            rCount++;
            yield return new WaitForSeconds(shakeFrequency);
        }
    }

    public void EnableCamera3()
    {
        camera3Enabled = true;
    }

    // Function for getting a random value based on list numbers
    private static float RangedFloat(float min, float max, System.Random r)
    {
        return (System.Convert.ToSingle(r.NextDouble()) * (max - min) + min);
    }

    // Function to get the OBJ folder path from configuration
    private string GetOBJFolderPath()
    {
        string executableDirectory = Directory.GetParent(Application.dataPath).FullName;
        string path = Path.Combine(executableDirectory, "SimulationDir", "temp", "Meshes");

        return path;
    }

    // Function to import OBJ files from a directory
    private void ImportOBJFiles()
    {
        string objFolderPath = GetOBJFolderPath();
        string importFolderPath = "Assets/Board_Objects/ImportBoard/";

        // Delete all files in the import folder
        if (Directory.Exists(importFolderPath))
        {
            string[] existingFiles = Directory.GetFiles(importFolderPath);
            foreach (string file in existingFiles)
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(importFolderPath);
        }

        string[] objFiles = Directory.GetFiles(objFolderPath, "*.obj");

        foreach (string objFile in objFiles)
        {
            string assetPath = Path.Combine(importFolderPath, Path.GetFileName(objFile));
            ImportOBJ(assetPath, objFile);
        }
    }

    private void ImportOBJ(string assetPath, string objFile)
    {
        var importer = new Dummiesman.OBJLoader();
        GameObject loadedObject = importer.Load(objFile);

        if (loadedObject != null)
        {
            // Instantiate the loaded object
            GameObject instantiatedObject = Instantiate(loadedObject, container.transform);
            instantiatedObject.name = Path.GetFileNameWithoutExtension(objFile); // Ensure the name is the same as the loaded object
            instantiatedObject.transform.Rotate(0, 0, 0);
            instantiatedObject.transform.position = Vector3.zero;

            // Apply properties to the instantiated object
            ApplyPropertiesToInstantiatedObject(instantiatedObject);

            // Destroy the loadedObject to avoid duplicates
            Destroy(loadedObject);
        }
        else
        {
            Debug.LogError("Failed to load OBJ file: " + objFile);
        }
    }


    private void ApplyPropertiesToInstantiatedObject(GameObject instantiatedObject)
    {
        if (instantiatedObject.name.Contains("_Metal"))
        {
            instantiatedObject.layer = LayerMask.NameToLayer("Metal"); // Assign Metal layer
            ApplyLayerToAllChildren(instantiatedObject, "Metal"); // Apply Metal layer to all children
            var renderers = instantiatedObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.green;

                    // Ensure the renderer has the same name as its parent
                    renderer.gameObject.name = instantiatedObject.name;
                }
            }

            // Add Rigidbody to _Metal objects
            var rigidbody = instantiatedObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = instantiatedObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true; // Make it kinematic so it's affected by collisions but doesn't fall due to gravity
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // Set collision detection to Discrete
            }

            // Apply MeshCollider with convex set to true or false based on polygon count
            var meshFilters = instantiatedObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    var mesh = meshFilter.sharedMesh;
                    var meshCollider = instantiatedObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                    if (mesh.triangles.Length / 3 > 256)
                    {
                        Debug.LogWarning($"Mesh {mesh.name} exceeds 256 polygon limit. Using non-convex MeshCollider.");
                        meshCollider.convex = false; // Set convex to false
                    }
                    else
                    {
                        meshCollider.convex = true; // Set convex to true
                    }
                    meshCollider.isTrigger = false; // Ensure it's not a trigger
                }
            }
        }
        else if (instantiatedObject.name.Contains("_NonMetal"))
        {
            instantiatedObject.layer = LayerMask.NameToLayer("NonMetal"); // Assign NonMetal layer
            ApplyLayerToAllChildren(instantiatedObject, "NonMetal"); // Apply NonMetal layer to all children
            var renderers = instantiatedObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.blue; // Optional: Set color for _NonMetal objects

                    // Ensure the renderer has the same name as its parent
                    renderer.gameObject.name = instantiatedObject.name;
                }
            }

            // Add Rigidbody to _NonMetal objects
            var rigidbody = instantiatedObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = instantiatedObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true; // Make it kinematic so it's affected by collisions but doesn't fall due to gravity
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // Set collision detection to Discrete
            }

            // Apply MeshCollider with convex set to false
            var meshFilters = instantiatedObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    var meshCollider = instantiatedObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                    meshCollider.convex = false; // Set convex to false
                    meshCollider.isTrigger = false; // Ensure it's not a trigger
                }
            }
        }

        // Additional Debugging
        Debug.Log($"Rigidbody: {instantiatedObject.GetComponent<Rigidbody>() != null}");
        Debug.Log($"MeshCollider: {instantiatedObject.GetComponent<MeshCollider>() != null}");
    }





    private void ApplyLayerToAllChildren(GameObject parent, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in parent.transform)
        {
            child.gameObject.layer = layer;
        }
    }

    // Function to create a container with flat planes around the imported objects
    private void CreateContainer()
    {
        // Calculate the combined bounds of all OBJ objects
        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;
        Renderer[] renderers = container.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                combinedBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        // Calculate the dimensions and position of the container
        Vector3 containerCenter = combinedBounds.center;
        Vector3 containerSize = combinedBounds.size;

        float width = containerSize.x * 2; // Double the width
        float length = containerSize.z * 2; // Double the length
        float height = containerSize.y * 10; // 10x the height
        float yPosition = combinedBounds.min.y;

        // Create the flat planes adjacent to the surfaces of the container
        float thickness = 0.0001f;
        CreatePlane(container.transform, new Vector3(containerCenter.x, yPosition + height / 2, containerCenter.z + length / 2 + thickness / 2), new Vector3(width, height, thickness), "NonMetal", "BackPlane");
        CreatePlane(container.transform, new Vector3(containerCenter.x, yPosition + height / 2, containerCenter.z - length / 2 - thickness / 2), new Vector3(width, height, thickness), "NonMetal", "FrontPlane");
        CreatePlane(container.transform, new Vector3(containerCenter.x + width / 2 + thickness / 2, yPosition + height / 2, containerCenter.z), new Vector3(thickness, height, length), "NonMetal", "RightPlane");
        CreatePlane(container.transform, new Vector3(containerCenter.x - width / 2 - thickness / 2, yPosition + height / 2, containerCenter.z), new Vector3(thickness, height, length), "NonMetal", "LeftPlane");
        CreatePlane(container.transform, new Vector3(containerCenter.x, yPosition - thickness / 2, containerCenter.z), new Vector3(width, thickness, length), "NonMetal", "BottomPlane");
    }

    // Function to position the WhiskerSpawner object
    private void PositionWhiskerSpawner()
    {
        // Calculate the combined bounds of all OBJ objects
        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;
        Renderer[] renderers = container.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                combinedBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        // Calculate the center position and half the height of the container's walls
        Vector3 containerCenter = combinedBounds.center;
        float height = combinedBounds.size.y;
        float yPosition = combinedBounds.min.y;

        // Position the WhiskerSpawner halfway between the bottom and top of the container's walls
        WhiskerSpawner.transform.position = new Vector3(containerCenter.x, yPosition + height / 2, containerCenter.z);
    }

    // Helper function to create a plane
    private void CreatePlane(Transform parent, Vector3 position, Vector3 scale, string layerName, string planeName)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane.name = planeName; // Assign the unique name
        plane.transform.parent = parent;
        plane.transform.localPosition = position;
        plane.transform.localScale = scale;
        plane.layer = LayerMask.NameToLayer(layerName); // Set the layer
        plane.GetComponent<Renderer>().material.color = Color.gray;
    }

    // Function to center the camera to visualize all the OBJ objects
    private void CenterCamera()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<Renderer>() && obj.GetComponent<TextMesh>() == null)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = new Bounds(obj.GetComponent<Renderer>().bounds.center, obj.GetComponent<Renderer>().bounds.size);
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(obj.GetComponent<Renderer>().bounds);
                }
            }
        }

        if (boundsInitialized)
        {
            Vector3 centerPoint = combinedBounds.center;
            float distance = combinedBounds.size.magnitude / 2; // Reduce the distance to zoom in twice as much

            camera1.transform.position = centerPoint - camera1.transform.forward * distance;
            camera1.transform.LookAt(centerPoint);
        }
        else
        {
            Debug.LogWarning("No objects with Renderer found to center the camera on.");
        }
    }

    public GameObject GetContainer()
    {
        return container;
    }
}
