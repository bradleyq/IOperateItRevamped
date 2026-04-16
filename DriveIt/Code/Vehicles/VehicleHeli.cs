using DriveIt.Settings;

namespace DriveIt.Vehicles
{
    public class VehicleHeli : VehicleGeneric
    {
        protected override float enginePower { get => ModSettings.HeliEnginePower; }
        protected override float brakingForce { get => 0.0f; }
        protected override float downForce { get => 0.0f; }
        protected override float driveBias { get => 0.5f; }
        protected override float brakeBias { get => 0.5f; }
        protected override float springDamp { get => ModSettings.HeliSpringDamp; }
        protected override float springOffset { get => ModSettings.HeliSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.HeliMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.HeliMassCenterBias; }
    }
}
