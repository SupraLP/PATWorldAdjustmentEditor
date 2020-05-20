using UnityEngine;

[System.Serializable]
public class HeightAdjustmentObject : MonoBehaviour {
    
    private HeightAdjustment _heightAdjustment = new HeightAdjustment();
    
    public float Adjustment {
        get { return _heightAdjustment.adjustment; }
        set { _heightAdjustment.adjustment = value; }
    }
    
    public int Radius {
        get { return (int)transform.localScale.x; }
        set {
            transform.localScale = new Vector3(value, value, value);
            _heightAdjustment.radius = value;
        }
    }

    public Vector3 Pos {
        get { return transform.position; }
        set {
            _heightAdjustment.pos = new[]{ value.x, value.z, value.y};
            Debug.Log(value);
            Debug.Log(_heightAdjustment.pos[0] + " " + _heightAdjustment.pos[1] + " " + _heightAdjustment.pos[2]);
            transform.position = value;
        }
    }

    public HeightAdjustment GetHeightAdjustment() {
        return _heightAdjustment;
    }

    public void LoadHeightAdjustment(HeightAdjustment heightAdjustment) {
        this._heightAdjustment = heightAdjustment;
        Pos = new Vector3(heightAdjustment.pos[0], heightAdjustment.pos[2], heightAdjustment.pos[1]);
        Radius = heightAdjustment.radius;
        Adjustment = heightAdjustment.adjustment;
    }
    
    /*void Update() {
        var camdir = Camera.main.transform.up;
        Debug.DrawRay(transform.position, new Vector3(camdir.x * Adjustment, camdir.y * Adjustment, camdir.z * Adjustment), Color.red, 0.1f);
    }*/
}
