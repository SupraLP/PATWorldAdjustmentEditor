[System.Serializable]
public class Planet {
    public int seed = 0;
    public int radius = 500;
    public int heightRange = 35;
    public int waterHeight = 34;
    public int waterDepth = 100;
    public int temperature = 50;
    public int metalDensity = 50;
    public int metalClusters = 50;
    public int metalSpotLimit = -1;
    public int biomeScale = 50;
    public string biome = "earth";
    public bool symmetricalMetal = false;
    public bool symmetricalStarts = false;
    public int numArmies = 2;
    public int landingZonesPerArmy = 0;
    public int landingZoneSize = 0;
    public HeightAdjustment[] heightAdjustments = new HeightAdjustment[]{};
}
