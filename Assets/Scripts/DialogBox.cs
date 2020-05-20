using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogBox : MonoBehaviour {
    private int response;

    private void OnEnable() {
        response = 0;
    }

    public int CreateDialog() {
        while (true) {
            if (response != 0) {
                return response;
            }
        }
    }

    public void ResponseOne() {
        response = 1;
    }

    public void ResponseTwo() {
        response = 2;
    }

    public void ResponseThree() {
        response = 3;
    }
}
