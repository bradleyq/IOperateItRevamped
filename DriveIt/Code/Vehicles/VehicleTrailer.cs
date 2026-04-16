using DriveIt.Settings;

namespace DriveIt.Vehicles
{
    public class VehicleTrailer : VehicleGeneric
    {
        protected override float enginePower { get => 0.0f; }
        protected override float brakingForce { get => ModSettings.TrailerBrakingForce; }
        protected override float downForce { get => ModSettings.TrailerDownForce; }
        protected override float driveBias { get => 0.0f; }
        protected override float brakeBias { get => 0.0f; }
        protected override float springDamp { get => ModSettings.TrailerSpringDamp; }
        protected override float springOffset { get => ModSettings.TrailerSpringOffset; }
        protected override float springSwayBar { get => ModSettings.TrailerSpringSwayBar; }
        protected override float massCenterHeight { get => ModSettings.TrailerMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.TrailerMassCenterBias; }
    }
}
