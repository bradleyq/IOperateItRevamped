using DriveIt.Settings;

namespace DriveIt.Vehicles
{
    public class VehicleBoat : VehicleGeneric
    {
        protected override float enginePower { get => ModSettings.BoatEnginePower; }
        protected override float brakingForce { get => ModSettings.BoatBrakingForce; }
        protected override float downForce { get => ModSettings.BoatDownForce; }
        protected override float driveBias { get => ModSettings.BoatDriveBias; }
        protected override float brakeBias { get => ModSettings.BoatBrakeBias; }
        protected override float springDamp { get => ModSettings.BoatSpringDamp; }
        protected override float springOffset { get => ModSettings.BoatSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.BoatMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.BoatMassCenterBias; }
    }
}
