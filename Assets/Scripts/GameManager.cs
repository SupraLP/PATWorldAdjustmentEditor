using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
    
    public Button saveWorldFile;

    public int worldSize = 500; // overwritten by file

    public GameObject planet;
    public Material selectedMaterial;
    public Material sphereMaterial;

    public Slider radiusSlider;
    public Text radiusInputText;
    public Slider heightSlider;
    public Text heightInputText;

    public Text jsonDataForCurrentObject;
    
    private GameObject camera;
    public GameObject radiusPrefab;
    private Vector3 lastMousePosition;
    private List<GameObject> spheres;
    private int tool = 0;
    private GameObject activeObject;
    
    // Start is called before the first frame update
    void Start() {
        camera = Camera.main.gameObject;
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
        
            if (Input.GetAxis("Fire1") >= 0.1f) {
                if (tool == 1) {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity);
                    if (hit.collider.gameObject != null) {
                        GameObject newSphere = Instantiate(radiusPrefab, hit.point, new Quaternion());
                        spheres.Add(newSphere);
                        SetActiveObject(newSphere);
                        SetRadiusForActiveObject((int)Math.Floor(worldSize * 0.1f));
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
    }

    private void SetRadiusForActiveObject(int value) {
        activeObject.transform.localScale = new Vector3(value, value, value);
        WriteObjectToJson(activeObject);
    }

    private void SetHeightForActiveObject(int value) {
        activeObject.GetComponent<HeightContainer>().height = value;
        WriteObjectToJson(activeObject);
    }

    public void SetHeightInputFromSlider(float value) {
        heightInputText.text = value.ToString();
        SetHeightForActiveObject((int)value);
    }

    public void SetHeightSliderFromText(String value) {
        try {
            heightSlider.SetValueWithoutNotify(int.Parse(value));
            SetHeightForActiveObject(int.Parse(value));
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
    }

    public void SetRadiusInputFromSlider(float value) {
        int radius = (int)value;
        radiusInputText.text = radius.ToString();
        SetRadiusForActiveObject(radius);
    }

    public void SetRadiusSliderFromText(String value) {
        try {
            int radius = int.Parse(value);
            radiusSlider.SetValueWithoutNotify(radius);
            SetRadiusForActiveObject(radius);
        } catch (Exception e) {
            Debug.Log("The String was not valid:\n" + e);
        }
    }

    public void setWorldSize() {
        planet.transform.localScale = new Vector3(worldSize, worldSize, worldSize);
        camera.transform.position = new Vector3(0, 0, (float)(-worldSize*1.2));
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
