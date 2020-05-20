using UnityEngine;

[System.Serializable]
public class HeightAdjustmentObject : MonoBehaviour {
    
    public HeightAdjustment heightAdjustment;
    
    public int Adjustment {
        get { return heightAdjustment.adjustment; }
        set { heightAdjustment.adjustment = value; }
    }
    
    public int Radius {
        get { return (int)transform.localScale.x; }
        set {
            transform.localScale = new Vector3(value, value, value);
            heightAdjustment.radius = value;
        }
    }

    public int[] Pos {
        get {
            var position = transform.position;
            return new[]{ (int)position.x, (int)position.z, (int)position.y};
        }
        set {
            heightAdjustment.pos = new[]{ value[0], value[2], value[1]};
            transform.position = new Vector3(value[0], value[1], value[2]);
        }
    }

    public void LoadHeightAdjustment(HeightAdjustment heightAdjustment) {
        this.heightAdjustment = heightAdjustment;
        Pos = heightAdjustment.pos;
        Radius = heightAdjustment.radius;
    }
    
    void Update() {
        var camdir = Camera.main.transform.up;
        Debug.DrawRay(transform.position, new Vector3(camdir.x * Adjustment, camdir.y * Adjustment, camdir.z * Adjustment), Color.red, 0.1f);
    }
}
