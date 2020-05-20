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
    
    public Button saveWorldFile;

    public int worldSize = 500; // overwritten by file
    private int diameter;

    public GameObject planet;
    public Material selectedMaterial;
    public Material sphereMaterial;

    public Slider radiusSlider;
    public InputField radiusInputText;
    public Slider heightSlider;
    public InputField heightInputText;

    public Text jsonDataForCurrentObject;
    
    public Text currentSystemText;
    public Text currentPlanetText;

    public GameObject openDialog;
    public GameObject radiusPrefab;
    
    public int defaultRadius;
    public float defaultHeight = 0;

    public Image placeToolButton;
    public Image selectToolButton;
    public Image moveToolButton;
    public Image noToolButton;

    public Image camModeButton0;
    public Image camModeButton1;
    
    private int dialogResponse = 0;
    
#pragma warning disable 108,114
    private GameObject camera;
#pragma warning restore 108,114
    private Vector3 lastMousePosition;
    private List<HeightAdjustmentObject> heightAdjustments;
    private int tool = 0;
    private int camMode = 1;
    private HeightAdjustmentObject activeObject;

    private SolarSystem loadedSolarSystem;
    
    // Start is called before the first frame update
    void Start() {
        camera = Camera.main.gameObject;
        diameter = worldSize * 2;
        defaultRadius = (int)Math.Floor(diameter * 0.1f);
        setWorldSize();
        heightAdjustments = new List<HeightAdjustmentObject>();
        SelectNoTool();
        SelectCamMode1();
        loadedSolarSystem = new SolarSystem();
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
                        var adjustment = Instantiate(radiusPrefab, hit.point, new Quaternion()).GetComponent<HeightAdjustmentObject>();
                        adjustment.LoadHeightAdjustment(new HeightAdjustment());
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
        SetRadiusForActiveObject(activeObject.Radius);
        SetHeightForActiveObject(activeObject.Adjustment);
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
            worldSize = int.Parse(value);
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
        diameter = worldSize * 2;
        defaultRadius = (int)Math.Floor(diameter * 0.1f);
        setWorldSize();
    }

    public void OpenWorldFile() {
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
        loadedSolarSystem = JsonUtility.FromJson<SolarSystem>(loadedFile);
        Debug.Log(loadedFile);
        Debug.Log(JsonUtility.ToJson(loadedSolarSystem, true));
        var heightAdjustmentArray = loadedSolarSystem.planets[0].planet.heightAdjustments;
        if (heightAdjustmentArray != null) {
            openDialog.SetActive(true);
        }
    }

    private void KeepLoadingWorld() {
        openDialog.SetActive(false);
        var heightAdjustmentArray = loadedSolarSystem.planets[0].planet.heightAdjustments;
        Debug.Log(JsonUtility.ToJson(heightAdjustmentArray, true));
        currentSystemText.text = loadedSolarSystem.name;
        currentPlanetText.text = loadedSolarSystem.planets[0].name;
        if (dialogResponse == 1) {
            foreach (var heightAdjustment in heightAdjustments) {
                heightAdjustments.Remove(heightAdjustment);
                Destroy(heightAdjustment.gameObject);
            }
            foreach (var heightAdjustment in heightAdjustmentArray) {
                var heightAdjustmentObject = Instantiate(radiusPrefab).GetComponent<HeightAdjustmentObject>();
                heightAdjustmentObject.LoadHeightAdjustment(heightAdjustment);
                heightAdjustments.Add(heightAdjustmentObject);
            }
        } else if (dialogResponse == 2) {
            foreach (var heightAdjustment in heightAdjustmentArray) {
                var heightAdjustmentObject = Instantiate(radiusPrefab).GetComponent<HeightAdjustmentObject>();
                heightAdjustmentObject.LoadHeightAdjustment(heightAdjustment);
                heightAdjustments.Add(heightAdjustmentObject);
            }
        }
    }

    public void ResponseOne() {
        dialogResponse = 1;
        KeepLoadingWorld();
    }

    public void ResponseTwo() {
        dialogResponse = 2;
        KeepLoadingWorld();
    }

    public void ResponseThree() {
        dialogResponse = 3;
        KeepLoadingWorld();
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
        camera.transform.position = new Vector3(0, 0, (float)(-diameter*1.2));
        camera.transform.rotation = new Quaternion();
        camera.transform.parent.position = new Vector3();
        camera.transform.parent.rotation = new Quaternion();
    }

    private String WriteObjectToJson(HeightAdjustmentObject heightAdjustmentObject) {
        /*String outString = "{\n" +
                           "    \"adjustment\": " + heightAdjustment.adjustment + ",\n" +
                           "    \"radius\": " + heightAdjustment.radius + ",\n" +
                           "    \"pos\": [\n" +
                           "        " + heightAdjustment.pos[0] + ",\n" +
                           "        " + heightAdjustment.pos[1] + ",\n" +
                           "        " + heightAdjustment.pos[2] + "\n" +
                           "    ]\n" +
                           "}";*/
        String outString = JsonUtility.ToJson(heightAdjustmentObject.GetHeightAdjustment(), true);
        return outString;
    }
}
