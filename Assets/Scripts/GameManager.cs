using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Debug = UnityEngine.Debug;
using Slider = UnityEngine.UI.Slider;

public class GameManager : MonoBehaviour {
    
    [Header("File UI")]
    public Button saveWorldFile;
    
    [Header("Planet and adjustments")]
    public GameObject planet;
    public GameObject adjustmentObjectPrefab;
    public Material selectedMaterial;
    public Material sphereMaterial;
    
    [Header("Adjustment Edit UI")]
    public Slider radiusSlider;
    public InputField radiusInputText;
    public Slider heightSlider;
    public InputField heightInputText;
    public InputField adjustmentXInput;
    public InputField adjustmentYInput;
    public InputField adjustmentZInput;
    public Text jsonDataForCurrentObject;
    
    [Header("Various UI elements")]
    public InputField currentSystemText;
    public InputField currentPlanetText;

    public GameObject openDialog;

    public Image placeToolButton;
    public Image selectToolButton;
    public Image moveToolButton;
    public Image noToolButton;

    public Image camModeButton0;
    public Image camModeButton1;

    public RectTransform planetContainer;
    public GameObject planetButtonPrefab;
    
    [Header("Default Values and Settings (Get overwritten during runtime)")]
    public int defaultRadius;
    public float defaultHeight = 0;
    
    public int worldRadius = 500;
    public int diameter;
    
    private int dialogResponse = 1;
    
#pragma warning disable 108,114
    private GameObject camera;
#pragma warning restore 108,114
    private Vector3 lastMousePosition;
    private List<HeightAdjustmentObject> heightAdjustments;
    private int tool = 0;
    private int camMode = 1;
    private HeightAdjustmentObject activeObject;
    private PlanetProperties activePlanet;

    private List<GameObject> planetButtons = new List<GameObject>();

    private SolarSystem loadedSolarSystem;
    
    // Start is called before the first frame update
    void Start() {
        camera = Camera.main.gameObject;
        diameter = worldRadius * 2;
        defaultRadius = (int)Math.Floor(diameter * 0.1f);
        setWorldSize();
        heightAdjustments = new List<HeightAdjustmentObject>();
        SelectNoTool();
        SelectCamMode1();
        
        loadedSolarSystem = JsonUtility.FromJson<SolarSystem>(File.ReadAllText(Application.dataPath + "/sampleSystem.pas"));
        LoadSolarSystem();
        
        adjustmentXInput.onValueChanged.AddListener(delegate(string input) { activeObject.Pos = new Vector3(float.Parse(input), activeObject.Pos.y, activeObject.Pos.z); });
        adjustmentYInput.onValueChanged.AddListener(delegate(string input) { activeObject.Pos = new Vector3(activeObject.Pos.x, float.Parse(input), activeObject.Pos.z); });
        adjustmentZInput.onValueChanged.AddListener(delegate(string input) { activeObject.Pos = new Vector3(activeObject.Pos.x, activeObject.Pos.y, float.Parse(input)); });
    }

    public void SelectCamMode0() {
        camMode = 0;
        camModeButton0.color = Color.cyan;
        camModeButton1.color = Color.white;
    }

    public void SelectCamMode1() {
        camMode = 1;
        camModeButton0.color = Color.white;
        camModeButton1.color = Color.cyan;
    }

    public void SelectNoTool() {
        tool = 0;
        placeToolButton.color = Color.white;
        selectToolButton.color = Color.white;
        moveToolButton.color = Color.white;
        noToolButton.color = Color.cyan;
    }

    public void SelectPlaceTool() {
        tool = 1;
        planet.layer = LayerMask.NameToLayer("Default");
        foreach (var adjustment in heightAdjustments) {
            adjustment.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        placeToolButton.color = Color.cyan;
        selectToolButton.color = Color.white;
        moveToolButton.color = Color.white;
        noToolButton.color = Color.white;
    }

    public void SelectSelectTool() {
        tool = 2;
        planet.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (var adjustment in heightAdjustments) {
            adjustment.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        placeToolButton.color = Color.white;
        selectToolButton.color = Color.cyan;
        moveToolButton.color = Color.white;
        noToolButton.color = Color.white;
    }

    public void SelectMoveTool() {
        tool = 3;
        planet.layer = LayerMask.NameToLayer("Default");
        foreach (var adjustment in heightAdjustments) {
            adjustment.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        placeToolButton.color = Color.white;
        selectToolButton.color = Color.white;
        moveToolButton.color = Color.cyan;
        noToolButton.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetAxis("Fire2") >= 0.1f || Input.GetAxis("Fire3") >= 0.1f || (tool == 0 && Input.GetAxis("Fire1") >= 0.1f)) {
                if (camMode == 0) {
                    camera.transform.parent.Rotate(new Vector3(
                                                               lastMousePosition.y - Input.mousePosition.y, 
                                                               Input.mousePosition.x - lastMousePosition.x, 
                                                               Input.mousePosition.z - lastMousePosition.z), Space.Self);
                } else {
                    camera.transform.parent.RotateAround(Vector3.up, (float)((Input.mousePosition.x - lastMousePosition.x) * 0.01));
                    camera.transform.parent.RotateAround(camera.transform.parent.right, (float)((lastMousePosition.y - Input.mousePosition.y) * 0.01));
                }
            }
            lastMousePosition = Input.mousePosition;
        
            if (Input.GetMouseButtonDown(0)) {
                if (tool == 1) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    if (hit.collider.gameObject != null) {
                        var adjustment = Instantiate(adjustmentObjectPrefab, hit.point, new Quaternion()).GetComponent<HeightAdjustmentObject>();
                        adjustment.Pos = hit.point;
                        adjustment.Adjustment = defaultHeight;
                        adjustment.Radius = defaultRadius;
                        heightAdjustments.Add(adjustment);
                        SetActiveObject(adjustment);
                    }
                } else if (tool == 2) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    try {
                        SetActiveObject(hit.collider.gameObject.GetComponent<HeightAdjustmentObject>());
                    } catch (Exception e) {
                        Debug.Log("" + e);
                    }
                } 
            }
            if (Input.GetAxis("Fire1") >= 0.1f) {
                if (tool == 3) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    try {
                        activeObject.Pos = hit.point;
                        jsonDataForCurrentObject.text = WriteObjectToJson(activeObject);
                    } catch (Exception e) {
                        Debug.Log("" + e);
                    }
                }
            }

            if (activeObject != null) {
                if (Input.GetButton("Delete")) {
                    Destroy(activeObject);
                }
            }
        
            camera.transform.Translate(Vector3.forward * (Input.mouseScrollDelta.y * 10f), Space.Self);
        }
    }

    private void SetActiveObject(HeightAdjustmentObject active) {
        if (activeObject != null) {
            activeObject.GetComponent<MeshRenderer>().material = sphereMaterial;
        }
        activeObject = active;
        activeObject.GetComponent<MeshRenderer>().material = selectedMaterial;
        jsonDataForCurrentObject.text = WriteObjectToJson(activeObject);
        SetRadiusForActiveObject(activeObject.Radius);
        SetHeightForActiveObject(activeObject.Adjustment);
        adjustmentXInput.text = activeObject.Pos.x.ToString();
        adjustmentYInput.text = activeObject.Pos.y.ToString();
        adjustmentZInput.text = activeObject.Pos.z.ToString();
    }

    private void SetRadiusForActiveObject(int value) {
        activeObject.Radius = value;
        jsonDataForCurrentObject.text = WriteObjectToJson(activeObject);
        radiusSlider.SetValueWithoutNotify(value);
        radiusInputText.text = value.ToString();
        defaultRadius = value;
    }

    private void SetHeightForActiveObject(float value) {
        activeObject.Adjustment = value;
        jsonDataForCurrentObject.text = WriteObjectToJson(activeObject);
        heightSlider.SetValueWithoutNotify(value);
        heightInputText.text = value.ToString();
        defaultHeight = value;
    }

    public void SetHeightInputFromSlider(float value) {
        SetHeightForActiveObject((int)value);
    }

    public void SetHeightSliderFromText(String value) {
        try {
            SetHeightForActiveObject(int.Parse(value));
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
    }

    public void SetRadiusInputFromSlider(float value) {
        SetRadiusForActiveObject((int)value);
    }

    public void SetRadiusSliderFromText(String value) {
        try {
            SetRadiusForActiveObject(int.Parse(value));
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
    }

    public void SetPlanetSizeFromText(String value) {
        try {
            worldRadius = int.Parse(value);
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
        diameter = worldRadius * 2;
        defaultRadius = (int)Math.Floor(diameter * 0.1f);
        setWorldSize();
    }

    public void OpenSolarSystemFile() {
        string loadedFile = "";
        StandaloneFileBrowser.OpenFilePanelAsync("Open World File", 
                                                 "", 
                                                 "", 
                                                 false,
                                                 paths => {
                                                     if (!string.IsNullOrEmpty(paths[0])) {
                                                         loadedFile = File.ReadAllText(paths[0]);
                                                     }
                                                 });
        loadedSolarSystem = ValidateSolarSystem(loadedFile);
        if (loadedSolarSystem != null) {
            openDialog.SetActive(true);
        }
    }

    private SolarSystem ValidateSolarSystem(string jsonString) {
        try {
            var tempSolarSystem = JsonUtility.FromJson<SolarSystem>(jsonString);
            Debug.Log("Json File Loaded:\n" + jsonString);
            return tempSolarSystem;
        } catch (Exception e) {
            Debug.Log(e);
            return null;
        }
    }

    private void LoadSolarSystem() {
        openDialog.SetActive(false);
        
        if (dialogResponse == 1) {
            currentSystemText.text = loadedSolarSystem.name;
            foreach (var planetButton in planetButtons) {
                Destroy(planetButton);
            }
            for (int i = 0; i < loadedSolarSystem.planets.Length; i++) {
                var planetButton = Instantiate(planetButtonPrefab, planetContainer);
                planetButton.GetComponentInChildren<Text>().text = loadedSolarSystem.planets[i].name;
                planetButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -4 - 34*i);
                var i1 = i;
                planetButton.GetComponent<Button>().onClick.AddListener(delegate { SelectPlanet(loadedSolarSystem.planets[i1], i1); }) ;
                planetButtons.Add(planetButton);
            }
            planetContainer.sizeDelta = new Vector2(planetContainer.sizeDelta.x, 4 + loadedSolarSystem.planets.Length * 34);
            
            SelectPlanet(loadedSolarSystem.planets[0], 0);
        }

        dialogResponse = 0;
    }

    private void SelectPlanet(PlanetProperties planet, int planetId) {
        // set UI button color for inactive / active button
        foreach (var planetButton2 in planetButtons) {
            planetButton2.GetComponent<Image>().color = planetButtonPrefab.GetComponent<Image>().color;
            planetButton2.GetComponentInChildren<Text>().color = Color.white;
        }
        planetButtons[planetId].GetComponent<Image>().color = Color.cyan;
        planetButtons[planetId].GetComponentInChildren<Text>().color = Color.black;
        
        // transfer currently loaded data into json objects
        if (activePlanet != null) {
            List<HeightAdjustment> heightAdjustmentList = new List<HeightAdjustment>();
            foreach (var heightAdjustment in heightAdjustments) {
                heightAdjustmentList.Add(heightAdjustment.GetHeightAdjustment());
            }
            activePlanet.planet.heightAdjustments = heightAdjustmentList.ToArray();
        }
        
        //delete current objects
        foreach (var heightAdjustment in heightAdjustments) {
            Destroy(heightAdjustment.gameObject);
        }
        heightAdjustments.Clear();
        
        // set new planet
        activePlanet = planet;
        currentPlanetText.text = activePlanet.name;
        var heightAdjustmentArray = loadedSolarSystem.planets[planetId].planet.heightAdjustments;
        Debug.Log("Setting new height Adjustments:\n" + JsonUtility.ToJson(heightAdjustmentArray, true));
        foreach (var heightAdjustment in heightAdjustmentArray) {
            var heightAdjustmentObject = Instantiate(adjustmentObjectPrefab).GetComponent<HeightAdjustmentObject>();
            heightAdjustmentObject.LoadHeightAdjustment(heightAdjustment);
            heightAdjustments.Add(heightAdjustmentObject);
        }
        diameter = activePlanet.planet.radius * 2;
        defaultRadius = (int)Math.Floor(diameter * 0.1f);
        setWorldSize();
    }

    public void ResponseOne() {
        dialogResponse = 1;
        LoadSolarSystem();
    }

    public void ResponseTwo() {
        dialogResponse = 2;
        LoadSolarSystem();
    }

    public void SaveInfoToFile() {
        /*var outString = "				\"heightAdjustments\": [\n				    ";
        for (var index = 0; index < heightAdjustments.Count; index++) {
            var adjustment = heightAdjustments[index];
            outString += Regex.Replace(WriteObjectToJson(adjustment) + (heightAdjustments.Count-index == 1 ? "" : ",\n"), "\n", "\n					");
        }

        outString += "\n				]";*/
        List<HeightAdjustment> heightAdjustmentList = new List<HeightAdjustment>();
        foreach (var heightAdjustment in heightAdjustments) {
            heightAdjustmentList.Add(heightAdjustment.GetHeightAdjustment());
        }
        loadedSolarSystem.planets[0].planet.heightAdjustments = heightAdjustmentList.ToArray();
        var outString = JsonUtility.ToJson(loadedSolarSystem, true);

        StandaloneFileBrowser.SaveFilePanelAsync("Save File", 
                                                 "", 
                                                 "outFile.json", 
                                                 "", 
                                                 (string path) => { 
                                                     if (!string.IsNullOrEmpty(path)) {
                                                         File.WriteAllText(path, outString);
                                                     }  
                                                 });
    }

    public void setWorldSize() {
        planet.transform.localScale = new Vector3(diameter, diameter, diameter);
        camera.transform.rotation = new Quaternion();
        camera.transform.parent.rotation = new Quaternion();
        camera.transform.position = new Vector3(0, 0, (float)(-diameter*1.2));
    }

    private String WriteObjectToJson(HeightAdjustmentObject heightAdjustmentObject) {
        /*String outString = "{\n" +
                           "    \"adjustment\": " + heightAdjustmentObject.GetHeightAdjustment().adjustment + ",\n" +
                           "    \"radius\": " + heightAdjustmentObject.GetHeightAdjustment().radius + ",\n" +
                           "    \"pos\": [\n" +
                           "        " + heightAdjustmentObject.GetHeightAdjustment().pos[0] + ",\n" +
                           "        " + heightAdjustmentObject.GetHeightAdjustment().pos[1] + ",\n" +
                           "        " + heightAdjustmentObject.GetHeightAdjustment().pos[2] + "\n" +
                           "    ]\n" +
                           "}";*/
        String outString = JsonUtility.ToJson(heightAdjustmentObject.GetHeightAdjustment(), true);
        return outString;
    }
}
