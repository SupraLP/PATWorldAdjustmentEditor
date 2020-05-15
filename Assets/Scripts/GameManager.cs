using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
    public Text radiusInputText;
    public Slider heightSlider;
    public Text heightInputText;

    public Text jsonDataForCurrentObject;
    
#pragma warning disable 108,114
    private GameObject camera;
#pragma warning restore 108,114
    public GameObject radiusPrefab;
    private Vector3 lastMousePosition;
    private List<GameObject> spheres;
    private int tool = 0;
    private GameObject activeObject;
    
    // Start is called before the first frame update
    void Start() {
        camera = Camera.main.gameObject;
        diameter = worldSize * 2;
        setWorldSize();
        spheres = new List<GameObject>();
    }

    public void SelectNoTool() {
        tool = 0;
    }

    public void SelectPlaceTool() {
        tool = 1;
        planet.layer = LayerMask.NameToLayer("Default");
        foreach (var sphere in spheres) {
            sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    public void SelectSelectTool() {
        tool = 2;
        planet.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (var sphere in spheres) {
            sphere.layer = LayerMask.NameToLayer("Default");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetAxis("Fire2") >= 0.1f || Input.GetAxis("Fire3") >= 0.1f || (tool == 0 && Input.GetAxis("Fire1") >= 0.1f)) {
                camera.transform.parent.Rotate(new Vector3(
                                                           lastMousePosition.y - Input.mousePosition.y, 
                                                           Input.mousePosition.x - lastMousePosition.x, 
                                                           Input.mousePosition.z - lastMousePosition.z), Space.Self);
            }
            lastMousePosition = Input.mousePosition;
        
            if (Input.GetMouseButtonDown(0)) {
                if (tool == 1) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    if (hit.collider.gameObject != null) {
                        GameObject newSphere = Instantiate(radiusPrefab, hit.point, new Quaternion());
                        spheres.Add(newSphere);
                        SetActiveObject(newSphere);
                        SetRadiusForActiveObject((int)Math.Floor(diameter * 0.1f));
                    }
                } else if (tool == 2) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    try {
                        SetActiveObject(hit.collider.gameObject);
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

    private void SetActiveObject(GameObject active) {
        if (activeObject != null) {
            activeObject.GetComponent<MeshRenderer>().material = sphereMaterial;
        }
        activeObject = active;
        activeObject.GetComponent<MeshRenderer>().material = selectedMaterial;
        WriteObjectToJson(activeObject);
        SetRadiusForActiveObject((int)activeObject.transform.localScale.x);
        SetHeightForActiveObject(activeObject.GetComponent<HeightContainer>().height);
    }

    private void SetRadiusForActiveObject(int value) {
        activeObject.transform.localScale = new Vector3(value, value, value);
        WriteObjectToJson(activeObject);
        radiusSlider.SetValueWithoutNotify(value);
        radiusInputText.text = value.ToString();
    }

    private void SetHeightForActiveObject(int value) {
        activeObject.GetComponent<HeightContainer>().height = value;
        WriteObjectToJson(activeObject);
        heightSlider.SetValueWithoutNotify(value);
        heightInputText.text = value.ToString();
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

    public void SaveInfoToFile() {
        var outString = "";
        foreach (var sphere in spheres) {
            outString += WriteObjectToJson(sphere);
        }
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
    }

    private String WriteObjectToJson(GameObject sphere) {
        var position = sphere.transform.position;
        String outString = "{\n" +
                           "\t\"adjustment\": " + sphere.GetComponent<HeightContainer>().height + ",\n" +
                           "\t\"radius\": " + sphere.transform.localScale.x + ",\n" +
                           "\t\"pos\": [\n" +
                           "\t\t" + position.x + ",\n" +
                           "\t\t" + position.y + ",\n" +
                           "\t\t" + position.z + "\n" +
                           "\t]\n" +
                           "}\n";
        jsonDataForCurrentObject.text = outString;
        return outString;
    }
}
