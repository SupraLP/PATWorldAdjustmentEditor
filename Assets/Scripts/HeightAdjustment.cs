using UnityEngine;

[System.Serializable]
public class HeightAdjustment : MonoBehaviour {
    
    public int adjustment;

    public int radius;
    public int Radius {
        get { return (int)transform.localScale.x; }
        set {
            transform.localScale = new Vector3(value, value, value);
            radius = value;
        }
    }

    public int[] pos;
    public int[] Pos {
        get {
            var position = transform.position;
            return new[]{ (int)position.x, (int)position.z, (int)position.y};
        }
        set {
            pos = new[]{ value[0], value[2], value[1]};
            transform.position = new Vector3(value[0], value[1], value[2]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        var camdir = Camera.main.transform.up;
        Debug.DrawRay(transform.position, new Vector3(camdir.x * adjustment, camdir.y * adjustment, camdir.z * adjustment), Color.red, 0.1f);
    }
}
