using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightContainer : MonoBehaviour {
    public int height;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        var camdir = Camera.main.transform.up;
        Debug.DrawRay(transform.position, new Vector3(camdir.x * height, camdir.y * height, camdir.z * height), Color.red, 0.1f);
    }
}
