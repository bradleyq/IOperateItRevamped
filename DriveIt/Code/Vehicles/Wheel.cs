using DriveIt.Settings;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class Wheel : MonoBehaviour
    {
        private const float GRIP_SLIP_SPEED_LOW = 8.0f;
        private const float GRIP_SLIP_SPEED_HIGH = 40.0f;

        public const float GRIP_HIGH_SLIP = 0.5f;
        public const float GRIP_OPTIM_SLIP = 0.2f;
        public const float VALID_INCLINE = 0.5f;

        public bool isOnGround { get => onGround; }
        public MapUtils.COLLISION_TYPE wheelGroundType { get => groundType; }
        public Vector3 wheelGroundNormal { get => normal; }
        public Vector3 wheelGroundTangent { get => tangent; }
        public Vector3 wheelGroundBinormal { get => binormal; }
        public Vector3 wheelContactPoint { get => contactPoint; }
        public Vector3 wheelContactVelocity { get => contactVelocity; }
        public float wheelNormalImpulse { get => impulse.y; }
        public float wheelTangentImpulse { get => impulse.z; }
        public float wheelBinormalImpulse { get => impulse.x; }
        public float wheelTorqueFract { get => torqueFract; }
        public float wheelBrakeForce { get => brakeForce; }
        public float wheelHandbrakeForce { get => handbrakeForce; }
        public float wheelRadps { get => radps; }
        public float wheelSlip { get => slip; }
        public float wheelOptimSlip { get => slip - GRIP_OPTIM_SLIP; }
        public float wheelHighSlip { get => slip - GRIP_HIGH_SLIP; }
        public float wheelFrictionCoeffX { get => frictionCoeffX; }
        public float wheelFrictionCoeffZ { get => frictionCoeffZ; }
        public float wheelCompression { get => compression; }
        public Vector3 wheelOrigin { get => origin; }
        public float wheelRadius { get => radius; }
        public float wheelMoment { get => moment; }
        public bool isPowered { get => powered && (torqueFract > 0.0f); }
        public bool isSteerable { get => steerable; }
        public bool isInvertedSteer { get => inverted; }
        public bool isFront { get => front; }
        public bool isRight { get => right; }

        private VehicleGeneric vehicle;
        private MapUtils.COLLISION_TYPE groundType;
        private Vector3 origin;
        private Vector3 tangent;
        private Vector3 binormal;
        private Vector3 normal;
        private Vector3 heightSample;
        private Vector3 contactPoint;
        private Vector3 contactVelocity;
        private Vector3 impulse;
        private float radius;
        private float moment;
        private float radps;
        private float drag;
        private float torqueFract;
        private float brakeForce;
        private float handbrakeForce;
        private float compression;
        private float frictionCoeffX;
        private float frictionCoeffZ;
        private float slip;
        private bool onGround;
        private bool powered;
        private bool steerable;
        private bool inverted;
        private bool front;
        private bool right;

        public static Wheel InstanceWheel(VehicleGeneric parent, Vector3 localpos, float moment, float radius, bool isPowered = true, bool isSteerable = false, bool isInvertedSteer = false)
        {
            GameObject wheelObject = new GameObject("Wheel");
            Wheel w = wheelObject.AddComponent<Wheel>();
            wheelObject.transform.SetParent(parent.transform);
            wheelObject.transform.localPosition = localpos;
            w.vehicle = parent;
            w.tangent = Vector3.zero;
            w.binormal = Vector3.zero;
            w.normal = Vector3.zero;
            w.heightSample = Vector3.zero;
            w.contactPoint = Vector3.zero;
            w.contactVelocity = Vector3.zero;
            w.impulse = Vector3.zero;
            w.origin = localpos;
            w.moment = moment;
            w.radius = radius;
            w.radps = 0.0f;
            w.drag = 0.0f;
            w.torqueFract = 0.0f;
            w.brakeForce = 0.0f;
            w.handbrakeForce = 0.0f;
            w.compression = 0.0f;
            w.frictionCoeffX = ModSettings.GripCoeffK;
            w.frictionCoeffZ = ModSettings.GripCoeffK;
            w.slip = 0.0f;
            w.onGround = false;
            w.powered = isPowered;
            w.steerable = isSteerable;
            w.inverted = isInvertedSteer;
            w.front = localpos.z > 0.0f;
            w.right = localpos.x > 0.0f;

            w.vehicle.RegisterWheel(w);

            return w;
        }

        public void AdjustWheel(float torqueFract = 0.0f, float brakeForce = 0.0f, float handbrakeForce = 0.0f, float drag = 0.0f)
        {
            this.torqueFract = torqueFract;
            this.brakeForce = brakeForce;
            this.handbrakeForce = handbrakeForce;
            this.drag = drag;
        }

        public void ApplyDrag()
        {
            this.radps *= 1.0f - this.drag;
        }

        public void AddVelocity(float radps)
        {
            this.radps += radps;
        }

        public void SetVelocity(float radps)
        {
            this.radps = radps;
        }

        public void OnDestroy()
        {
            this.vehicle.DeRegisterWheel(this);
        }

        // Calculate the road tbn and height at the wheel position.
        public void CalcRoadState()
        {
            Vector3 pos = this.gameObject.transform.position;
            Vector3 xdisp = pos;
            Vector3 zdisp = pos;

            xdisp.x += 0.1f;
            zdisp.z += 0.1f;
            pos.y = MapUtils.CalculateHeight(pos, 0.0f, out this.groundType);
            xdisp.y = MapUtils.CalculateHeight(xdisp, 0.0f, out var _);
            zdisp.y = MapUtils.CalculateHeight(zdisp, 0.0f, out var _);

            this.heightSample = pos;
            this.normal = Vector3.Normalize(Vector3.Cross(zdisp - pos, xdisp - pos));
            this.binormal = Vector3.Normalize(Vector3.Cross(this.gameObject.transform.TransformDirection(Vector3.forward), this.normal));
            this.tangent = Vector3.Normalize(Vector3.Cross(this.normal, this.binormal));
        }

        // Adjust current wheel speed with previous tick sim and calculate new suspension position and state.
        public void CalcWheelState(Vector3 upVec)
        {
            if (this.onGround)
            {
                Vector3 prelimContactVel = vehicle.GetRigidbody().GetPointVelocity(this.contactPoint);
                float radDelta = Vector3.Dot(prelimContactVel, this.tangent) / this.radius - this.radps;
                this.radps += Mathf.Sign(radDelta) * Mathf.Min(Mathf.Abs(radDelta), this.impulse.y * this.radius * this.frictionCoeffZ / this.moment);
            }

            // calculate fist pass normal impulses. Update wheel suspension position.
            this.onGround = false;
            this.impulse = Vector3.zero;
            this.slip = 1.0f;
            float normDotUp = Vector3.Dot(this.normal, upVec);
            if (normDotUp > VALID_INCLINE)
            {
                Vector3 originWheelBottom = vehicle.GetRigidbody().transform.TransformPoint(this.origin + Vector3.down * this.radius);
                float compression = Mathf.Max(Vector3.Dot(this.heightSample - originWheelBottom, this.normal) / normDotUp, 0.0f);
                float springVel = (compression - this.compression) / Time.fixedDeltaTime;
                float deltaVel = -vehicle.springStiffness * Mathf.Exp(-vehicle.springStiffness * Time.fixedDeltaTime) * (compression + springVel * Time.fixedDeltaTime) 
                    + springVel * Mathf.Exp(-vehicle.springStiffness * Time.fixedDeltaTime) - springVel;

                this.gameObject.transform.localPosition = new Vector3(this.origin.x, this.origin.y + compression, this.origin.z);
                this.compression = compression;

                if (deltaVel < 0.0f)
                {
                    this.onGround = true;
                    this.impulse.y = (-deltaVel) * vehicle.GetRigidbody().mass / vehicle.wheelCount;
                    this.contactPoint = this.gameObject.transform.TransformPoint(new Vector3(0.0f, -this.radius, 0.0f));
                    this.contactVelocity = vehicle.GetRigidbody().GetPointVelocity(this.contactPoint);
                    Vector3 flatVel = this.contactVelocity - Vector3.Dot(this.contactVelocity, this.normal) * this.normal;
                    this.slip = Mathf.Clamp01(Vector3.Magnitude(flatVel - (this.radps * this.radius * this.tangent)) / Mathf.Clamp(flatVel.magnitude, GRIP_SLIP_SPEED_LOW, GRIP_SLIP_SPEED_HIGH));
                }
            }
            else
            {
                this.compression = 0.0f;
                this.gameObject.transform.localPosition = this.origin;
            }
        }

        public void SetFriction(float coeffX, float coeffZ)
        {
            this.frictionCoeffX = coeffX;
            this.frictionCoeffZ = coeffZ;
        }

        public void AddImpulses(float impulseX = 0.0f, float impulseY = 0.0f, float impulseZ = 0.0f)
        {
            this.impulse.x += impulseX;
            this.impulse.y = Mathf.Max(this.impulse.y + impulseY, 0.0f);
            this.impulse.z += impulseZ;
        }
    }
}
