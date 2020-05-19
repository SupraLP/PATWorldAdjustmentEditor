public class PlanetProperties {
    public string name;
    public int mass;
    public int position_x;
    public int position_y;
    public int velocity_x;
    public int velocity_y;
    public int required_thrust_to_move;
    public bool starting_planet;
    public bool respawn;
    public int min_spawn_delay;
    public int max_spawn_delay;
    public PlanetCSG[] planetCSG;
    public MetalSpots[] metal_spots;
    public LandingZones landing_zones;
    public Planet planet;
}
