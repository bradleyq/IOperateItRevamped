using DriveIt.Settings;

namespace DriveIt.Vehicles
{
    public class VehiclePlane : VehicleGeneric
    {
        protected override float enginePower { get => ModSettings.PlaneEnginePower; }
        protected override float brakingForce { get => ModSettings.PlaneBrakingForce; }
        protected override float downForce { get => ModSettings.PlaneDownForce; }
        protected override float driveBias { get => 0.5f; }
        protected override float brakeBias { get => ModSettings.PlaneBrakeBias; }
        protected override float springDamp { get => ModSettings.PlaneSpringDamp; }
        protected override float springOffset { get => ModSettings.PlaneSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.PlaneMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.PlaneMassCenterBias; }
    }
}
