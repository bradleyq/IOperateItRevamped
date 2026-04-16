using DriveIt.Settings;

namespace DriveIt.Vehicles
{
    public class VehicleTrain : VehicleGeneric
    {
        protected override float enginePower { get => ModSettings.TrainEnginePower; }
        protected override float brakingForce { get => ModSettings.TrainBrakingForce; }
        protected override float downForce { get => ModSettings.TrainDownForce; }
        protected override float driveBias { get => ModSettings.TrainDriveBias; }
        protected override float brakeBias { get => ModSettings.TrainBrakeBias; }
        protected override float springDamp { get => ModSettings.TrainSpringDamp; }
        protected override float springOffset { get => ModSettings.TrainSpringOffset; }
        protected override float springSwayBar { get => ModSettings.TrainSpringSwayBar; }
        protected override float massCenterHeight { get => ModSettings.TrainMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.TrainMassCenterBias; }
    }
}
