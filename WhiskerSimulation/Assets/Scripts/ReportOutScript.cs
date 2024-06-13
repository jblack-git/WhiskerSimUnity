// Whisker Simulation
// 
// ReportOutScript handles the last section that shows the metal faces
// 6/14/2024
// Authors: Joseph Black
// For Use By UAH/MDA/NASA
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ReportOutScript : MonoBehaviour
{
    public InputField searchInputField;
    public GameObject scrollView;
    public GameObject scrollViewContent;
    public GameObject scrollViewItemPrefab; // Buttons to press in scrollview
    private Dictionary<string, GameObject> searchableObjects = new Dictionary<string, GameObject>();
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private SimulationSceneScript simulationSceneScript;
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public GameObject container;
    public bool camera3Enabled = false;

    void Start()
    {
        // Find the SimulationSceneScript instance
        simulationSceneScript = FindObjectOfType<SimulationSceneScript>();
        if (simulationSceneScript == null)
        {
            Debug.LogError("SimulationSceneScript not found in the scene.");
        }

        // Store original camera position and rotation
        originalCameraPosition = camera1.transform.position;
        originalCameraRotation = camera1.transform.rotation;

        // Hide the search bar initially
        searchInputField.gameObject.SetActive(false);
        searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        searchInputField.onEndEdit.AddListener(OnSearchSubmit);

        // Hide the scroll view initially
        scrollView.SetActive(false);
        scrollViewContent.gameObject.SetActive(false);

        // Set the search bar position to the top of the camera view
        RectTransform searchBarRect = searchInputField.GetComponent<RectTransform>();
        searchBarRect.anchorMin = new Vector2(0.5f, 1);
        searchBarRect.anchorMax = new Vector2(0.5f, 1);
        searchBarRect.anchoredPosition = new Vector2(0, -50); // Might adjust for different screen res

        // Add a light to camera2 if not already added
        simulationSceneScript.camera2Light = camera2.GetComponent<Light>();
        if (simulationSceneScript.camera2Light == null)
        {
            simulationSceneScript.camera2Light = camera2.gameObject.AddComponent<Light>();
        }
        simulationSceneScript.camera2Light.type = LightType.Directional;
        simulationSceneScript.camera2Light.intensity = 1.0f;
        simulationSceneScript.camera2Light.enabled = false; // Start with the light off

        // Add a light to camera3 if not already added
        simulationSceneScript.camera3Light = camera3.GetComponent<Light>();
        if (simulationSceneScript.camera3Light == null)
        {
            simulationSceneScript.camera3Light = camera3.gameObject.AddComponent<Light>();
        }
        simulationSceneScript.camera3Light.type = LightType.Directional;
        simulationSceneScript.camera3Light.intensity = 1.0f;
        camera3.nearClipPlane = 0.01f;
        simulationSceneScript.camera3Light.enabled = false; // Start with the light off

        camera3.enabled = false; // Start with camera3 off
    }

    public void StopAllSimulations()
    {
        // Ensure the SimulationSceneScript reference is valid
        if (simulationSceneScript == null)
        {
            Debug.LogError("SimulationSceneScript reference is not set.");
            return;
        }

        // Stop all coroutines
        simulationSceneScript.StopAllCoroutines();

        // Disable all rigidbodies to stop physics simulations
        Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.isKinematic = true;
        }

        // Store original camera position and rotation
        originalCameraPosition = camera1.transform.position;
        originalCameraRotation = camera1.transform.rotation;

        //enable cameras
        camera1.enabled = true;
        camera2.enabled = true;

        // Delete all whiskers and the container's walls and floor
        GameObject[] whiskers = GameObject.FindGameObjectsWithTag("whisker");
        foreach (GameObject whisker in whiskers)
        {
            Destroy(whisker);
        }

        container = simulationSceneScript.GetContainer();
        if (container != null)
        {
            Transform[] children = container.GetComponentsInChildren<Transform>();
            List<string> childNames = new List<string>();
            foreach (Transform child in children)
            {
                childNames.Add(child.name);
                if (child.name == "BackPlane" || child.name == "FrontPlane" || child.name == "RightPlane" || child.name == "LeftPlane" || child.name == "BottomPlane")
                {
                    Destroy(child.gameObject);
                }
            }
            Debug.Log("Container children: " + string.Join(", ", childNames));
        }
        else
        {
            Debug.LogError("Container not found for deleting walls and floor.");
        }

        // Populate searchable objects dictionary
        PopulateSearchableObjects();

        // Enable the search bar and scroll view
        searchInputField.gameObject.SetActive(true);
        scrollView.SetActive(true);
        scrollViewContent.gameObject.SetActive(true);

        // Set camera2 to camera1 and enable camera3 and its light
        camera2.transform.position = camera1.transform.position;
        camera2.transform.rotation = camera1.transform.rotation;
        camera3.transform.position = camera1.transform.position;
        camera3.transform.rotation = camera1.transform.rotation;
        camera3.enabled = false;
        simulationSceneScript.camera3Light.enabled = true;
        camera3Enabled = true;

        // Call EnableCamera3 on the simulation scene script
        simulationSceneScript.EnableCamera3();

        Debug.Log("All simulations stopped.");

        // Read JSON and populate scroll view
        ReadAndPopulateScrollView();
    }

    private void ReadAndPopulateScrollView()
    {
        string filePath = ScoreScript.reportFilePath;
        Dictionary<string, int> collisionCounts = new Dictionary<string, int>();

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JObject collisions = (JObject)json["collisions"];

            foreach (var item in collisions)
            {
                string objectName = item.Key;
                int collisionCount = ((JArray)item.Value).Count;
                collisionCounts[NormalizeName(objectName)] = collisionCount;
            }
        }
        else
        {
            Debug.LogError("JSON file not found: " + filePath);
        }

        // List to hold instantiated buttons
        List<GameObject> buttonObjects = new List<GameObject>();

        foreach (var kvp in searchableObjects)
        {
            string objectName = kvp.Key;
            int collisionCount = collisionCounts.ContainsKey(NormalizeName(objectName)) ? collisionCounts[NormalizeName(objectName)] : 0;
            string displayText = $"{objectName}: {collisionCount}";

            GameObject newItem = Instantiate(scrollViewItemPrefab, scrollViewContent.transform);
            Text textComponent = newItem.GetComponentInChildren<Text>();

            if (textComponent != null)
            {
                textComponent.text = displayText;

                // Add OnClick listener to the button
                Button buttonComponent = newItem.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.AddListener(() => OnButtonClicked(displayText));
                }

                // Adjust button width to fit text
                RectTransform buttonRect = newItem.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    float width = textComponent.preferredWidth + 20; // Add some padding
                    buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                }

                buttonObjects.Add(newItem);
            }
            else
            {
                Debug.LogError("ScrollViewItemPrefab does not have a Text component.");
            }
        }

        // Sort the button objects by text
        buttonObjects = buttonObjects.OrderBy(button => button.GetComponentInChildren<Text>().text).ToList();

        // Re-arrange buttons in the scroll view
        foreach (var button in buttonObjects)
        {
            button.transform.SetSiblingIndex(buttonObjects.IndexOf(button));
        }
    }

    // Probably don't need this function anymore
    private string NormalizeName(string name)
    {
        return name.Replace("_", "").Replace(".", "");
    }

    private void OnSearchValueChanged(string searchString)
    {
        foreach (var kvp in searchableObjects)
        {
            kvp.Value.SetActive(kvp.Key.Contains(searchString));
        }
        foreach (Transform item in scrollViewContent.transform)
        {
            item.gameObject.SetActive(item.GetComponentInChildren<Text>().text.Contains(searchString));
        }
    }

    private void PopulateSearchableObjects()
    {
        if (container != null)
        {
            Transform[] children = container.GetComponentsInChildren<Transform>();
            Debug.Log("Populating searchable objects with container children.");
            foreach (Transform child in children)
            {
                // Make sure the child does not have a Rigidbody and has a MeshRenderer
                if (child.name.Contains("_Metal") && child.GetComponent<Rigidbody>() == null && child.GetComponent<MeshRenderer>() != null)
                {
                    searchableObjects[child.name] = child.gameObject;
                    Debug.Log("Added to searchableObjects: " + child.name);
                }
            }
        }
        else
        {
            Debug.LogError("Container not found for populating searchable objects.");
        }
    }

    private void OnSearchSubmit(string searchString)
    {
        Debug.Log($"Search submitted: {searchString}");
        if (searchableObjects.ContainsKey(searchString))
        {
            Debug.Log($"Object found: {searchString}");
            OnObjectSelected(searchString);
        }
        else
        {
            Debug.Log($"No object found for: {searchString}");
            // Log all keys in the searchableObjects dictionary for debugging
            Debug.Log("Available keys in searchableObjects: " + string.Join(", ", searchableObjects.Keys));
        }
    }

    private void OnButtonClicked(string buttonText)
    {
        // Remove everything after the last colon to get rid of the collision number
        int lastColonIndex = buttonText.LastIndexOf(':');
        if (lastColonIndex != -1)
        {
            buttonText = buttonText.Substring(0, lastColonIndex);
        }

        searchInputField.text = buttonText;
        OnSearchValueChanged(buttonText);
        OnSearchSubmit(buttonText);

        // Deselect the search bar to lose focus
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnObjectSelected(string objectName)
    {
        if (searchableObjects.ContainsKey(objectName))
        {
            GameObject selectedObject = searchableObjects[objectName];
            Debug.Log($"Focusing on object: {objectName}");

            // Calculate bounds of the selected object
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Vector3 center = bounds.center;
                float heightAboveObject = bounds.extents.magnitude * 100.0f; // Might need to adjust for diff screen res

                camera2.transform.position = new Vector3(center.x, center.y + heightAboveObject, center.z);
                camera2.transform.rotation = Quaternion.Euler(80, 0, 0);
                camera2.transform.LookAt(center);

                // Switch to camera2 and enable the light
                camera1.enabled = false;
                simulationSceneScript.camera1Light.enabled = false;
                camera2.enabled = true;
                simulationSceneScript.camera2Light.enabled = true;
                camera3.enabled = false;
                simulationSceneScript.camera3Light.enabled = false;
            }
            else
            {
                Debug.LogError("Selected object does not have a renderer component.");
            }
        }
        else
        {
            Debug.Log($"Object not in dictionary: {objectName}");
        }
    }

    public void ResetCameraView()
    {
        camera1.transform.position = originalCameraPosition;
        camera1.transform.rotation = originalCameraRotation;
        searchInputField.text = string.Empty;

        // Switch back to camera1 and disable the light
        camera1.enabled = true;
        simulationSceneScript.camera1Light.enabled = true;
        camera2.enabled = false;
        simulationSceneScript.camera2Light.enabled = false;
        camera3.enabled = false;
        simulationSceneScript.camera3Light.enabled = false;

        // Reset all objects to be active
        foreach (var kvp in searchableObjects)
        {
            kvp.Value.SetActive(true);
        }
    }
}
