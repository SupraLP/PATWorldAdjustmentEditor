[System.Serializable]
public class HeightAdjustment {
    public float adjustment;
    public int radius;
    public float[] pos;

    public HeightAdjustment() {
        pos = new[] {0f,0f,0f};
        radius = 100;
        adjustment = 0;
    }
}
