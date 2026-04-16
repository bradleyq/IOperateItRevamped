using DriveIt.Settings;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleBike : VehicleGeneric
    {
        private float m_tilt = 0.0f;
        private class TiltKalmanFilter
        {
            // Process noise covariance (how much we expect the true value to change)
            private static float q = 0.2f;

            // Measurement noise covariance (how noisy the measurements are)
            private static float r = 1000.0f;

            // Estimate error covariance
            private static float p = 1.0f;

            // Current estimate
            private static float x = 0.0f;

            // Kalman gain
            private static float k = 0.0f;

            public static float Update(float measurement)
            {
                p = p + q;
                k = p / (p + r);
                x = x + k * (measurement - x);
                p = (1f - k) * p;
                return x;
            }
            public static float CurrentEstimate => x;
        }

        protected override float enginePower { get => ModSettings.BikeEnginePower; }
        protected override float brakingForce { get => ModSettings.BikeBrakingForce; }
        protected override float downForce { get => ModSettings.BikeDownForce; }
        protected override float driveBias { get => ModSettings.BikeDriveBias; }
        protected override float brakeBias { get => ModSettings.BikeBrakeBias; }
        protected override float springDamp { get => ModSettings.BikeSpringDamp; }
        protected override float springOffset { get => ModSettings.BikeSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.BikeMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.BikeMassCenterBias; }

        //        if (m_isConstrainedZ && !w.isFront)
        //{
        //    w.frictionCoeffX *= CONSTRAINEDZ_STAB_BOOST;
        //}

        //        if (m_isConstrainedZ)
        //{
        //    frictionScaleLong = Mathf.Min(w.normalImpulse* w.frictionCoeffZ, Mathf.Abs(flatImpulses.y)) / Mathf.Max(Mathf.Abs(flatImpulses.y), DriveCommon.FLOAT_ERROR);
        //    frictionScaleLat = Mathf.Min(w.normalImpulse* w.frictionCoeffX, Mathf.Abs(flatImpulses.x)) / Mathf.Max(Mathf.Abs(flatImpulses.x), DriveCommon.FLOAT_ERROR);
        //}

        //            if (m_isConstrainedZ)
        //    {
        //        Vector3 sideVec = Vector3.Cross(m_vehicleRigidBody.transform.forward, Vector3.up).normalized;
        //float tilt = Mathf.Atan(Vector3.Dot(netNetImpulse, sideVec) / (m_vehicleRigidBody.mass * ACCEL_G * Time.fixedDeltaTime)) * 180.0f / Mathf.PI * STEER_TILT_SCALE;
        //TiltKalmanFilter.Update(tilt);
        //        m_tilt = TiltKalmanFilter.CurrentEstimate;
        //        Quaternion rot = Quaternion.AngleAxis(TiltKalmanFilter.CurrentEstimate, m_vehicleRigidBody.transform.forward) * Quaternion.LookRotation(m_vehicleRigidBody.transform.forward);
        //m_vehicleRigidBody.transform.rotation = rot;
        //    }


    }
}
