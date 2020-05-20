[System.Serializable]
public class PlanetProperties {
    public string name = "-- empty planet --";
    public int mass = 20000;
    public float position_x = 39800;
    public float position_y = 0;
    public float velocity_x = 0;
    public float velocity_y = -112;
    public int required_thrust_to_move = 0;
    public bool starting_planet = true;
    public bool respawn = false;
    public bool start_destroyed = false;
    public int min_spawn_delay = 0;
    public int max_spawn_delay = 0;
    public PlanetCSG[] planetCSG;
    public MetalSpots[] metal_spots;
    public LandingZones landing_zones;
    public Planet planet = new Planet();
}
