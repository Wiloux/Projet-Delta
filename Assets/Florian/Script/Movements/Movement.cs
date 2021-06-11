using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using ToolsBoxEngine;

namespace Florian {
    [SelectionBase]
    public class Movement : MonoBehaviour {
        private struct TimedChangeCoroutineStruct<T> {
            public Coroutine routine;
            public T value;

            public TimedChangeCoroutineStruct(Coroutine routine, T value) {
                this.routine = routine;
                this.value = value;
            }
        }

        public enum SpeedStates { STOPPED, CRUSADE, HIGH, OVER }
        public enum AccelerationType { NONE, BASE, FORWARD, WHIP }

        private Rigidbody _rb;
        /*[HideInInspector]*/
        public Vector3 velocity = Vector3.zero;

        [Header("MaxSpeeds")]
        public float crusadeSpeed = 20f;
        public float highSpeed = 40f;
        public float overSpeed = 50f;

        [Header("Accelerations")]
        public AmplitudeCurve baseAcceleration;
        public AmplitudeCurve forwardAcceleration;
        public AmplitudeCurve whipAcceleration;
        [Range(0f, 1f)] public float accelerationRandomRange = 0.1f;
        private AccelerationType currentAccelerationType = AccelerationType.NONE;
        private float accelerationFactor = 1f;

        private int rebellionStacks = 0;

        [Header("Acceleration")]
        //public AmplitudeCurve acceleration = null;
        [HideInInspector] public float maxSpeed;
        public float backwardSpeed = 10f;
        [HideInInspector] public bool isAccelerate;

        [Header("Deceleration")]
        public AmplitudeCurve deceleration = null;
        [HideInInspector] public bool isDecelerate;

        [Header("Frictions")]
        public AmplitudeCurve frictions = null;
        [SerializeField] private float decelerateTime = 0.2f;
        private float decelerateTimer = 0.2f;

        [Header("Turn")]
        [SerializeField] private float horizontalDirectionSpeed = 10f;
        private float targetHorizontalDirection = 0f;
        private float horizontalDirection = 0f;
        public AmplitudeCurve turn = null;
        [HideInInspector] public bool isTurn = false;
        [SerializeField] private float rollAngle = 10f;

        [Header("Air Detection")]
        public AmplitudeCurve gravityCurve = null;
        [SerializeField] private Transform groundCheck = null;
        [SerializeField] private float groundRayDistance = 0.2f;
        [SerializeField] private float slopeRotateSpeed = 0.5f;
        [HideInInspector] public bool airborn;
        private bool needLastGroundPosUpdate;
        public Vector3 lastGroundPos;
        [HideInInspector] public bool jumping = false;

        public float jumpForce;
        public LayerMask groundLayers;
        [HideInInspector] public bool stun;

        [Header("Collisions")]
        [Range(0f, 90f), SerializeField] private float faceAngle = 50f;
        public float bounceForce = 20f;
        public float bounceStunTime = 2f;
        [Range(0f, 1f), SerializeField] private float offTrackMultiplier = 0.3f;
        private bool offTracking = false;

        [Header("Recovery")]
        [SerializeField] private float slowRecoveryTime = 2f;

        [HideInInspector] public Tools.BasicDelegate<float> OnStun;
        private bool slowable = true;

        private Dictionary<string, TimedChangeCoroutineStruct<object>> timedChangedRoutines = null;

        #region Properties

        public float Speed {
            //get { return velocity.magnitude * Mathf.Sign(velocity.z); }
            get { return velocity.z; }
        }

        public float HorizontalDirection {
            get { return horizontalDirection; }
        }

        public bool Decelerating {
            get { return isDecelerate; }
        }

        public bool Accelerating {
            get { return isAccelerate; }
        }

        public bool Turning {
            get { return isTurn; }
        }

        public float SpeedRatio {
            get { return Mathf.Clamp01(velocity.z / overSpeed); }
        }

        public float MaxSpeed {
            get { return overSpeed; }
        }

        private AccelerationType CurrentAccelerationType {
            get { return currentAccelerationType; }
            set {
                currentAccelerationType = value;
                switch (currentAccelerationType) {
                    case AccelerationType.BASE:
                        maxSpeed = crusadeSpeed;
                        break;
                    case AccelerationType.FORWARD:
                        maxSpeed = highSpeed;
                        break;
                    case AccelerationType.WHIP:
                        maxSpeed = overSpeed;
                        break;
                }
            }
        }

        public SpeedStates SpeedState {
            get {
                if (velocity.z < 5f)
                    return SpeedStates.STOPPED;
                else if (velocity.z < crusadeSpeed)
                    return SpeedStates.CRUSADE;
                else if (velocity.z < highSpeed)
                    return SpeedStates.HIGH;
                else
                    return SpeedStates.OVER;
            }
        }

        #endregion

        #region Unity callbacks

        void Start() {
            _rb = GetComponent<Rigidbody>();
            timedChangedRoutines = new Dictionary<string, TimedChangeCoroutineStruct<object>>();
            maxSpeed = crusadeSpeed;
        }

        void Update() {
            //isAccelerate = decelerateTimer > 0f;
            isDecelerate = false;
            //isTurn = false;
            airborn = !IsGrounded() || jumping;

            if (!IsGrounded() && needLastGroundPosUpdate) {
                needLastGroundPosUpdate = false;
                lastGroundPos = transform.position;
            }

            if (IsGrounded()) {
                needLastGroundPosUpdate = true;
            }
            //airborn = false;

            if (!airborn) {
                velocity.y = 0f;
            }
            //if (IsGrounded()) {
            //    velocity.y = 0f;
            //}
        }

        #endregion

        #region Update Movements

        public void UpdateMovements() {
            if (velocity.z > crusadeSpeed)
                isAccelerate = true;
            else
                isAccelerate = false;

            if (horizontalDirection != targetHorizontalDirection) {
                horizontalDirection = Mathf.MoveTowards(horizontalDirection, targetHorizontalDirection, horizontalDirectionSpeed * Time.deltaTime);
            }

            if (horizontalDirection != 0f && !stun) {
                isTurn = true;
                Turn();
            } else {
                isTurn = false;
            }
            //Roll();

            if (isDecelerate && !stun) {
                float deceleration = ComputeCurve(this.deceleration);
                velocity += Vector3.forward * -deceleration * Time.deltaTime;

                if (velocity.z < -backwardSpeed) {
                    velocity = Vector3.forward * -backwardSpeed;
                }
            } else if (Speed < maxSpeed && !stun) {
                AccelerationUpdate();
                ResetDecelerateTimer();
            } else {
                if (decelerateTimer <= 0f) {
                    float frictions = ComputeCurve(this.frictions);
                    if (frictions * velocity.normalized.sqrMagnitude * Time.deltaTime > velocity.sqrMagnitude) {
                        velocity = Vector3.zero;
                    } else {
                        velocity += -frictions * velocity.normalized * Time.deltaTime;
                    }

                    //if (Mathf.Clamp01(Mathf.Abs(Speed) / maxSpeed) <= 0.1f) {
                    //    //velocity = Vector3.zero;
                    //}
                }
            }

            if (decelerateTimer > 0f) {
                decelerateTimer -= Time.deltaTime;
            }

            velocity += ComputeGravity() * Vector3.down;

            ApplySpeed();
            // SlopeTilt();
        }

        private void AccelerationUpdate() {
            if (currentAccelerationType == AccelerationType.NONE) { return; }

            AmplitudeCurve accelerationCurve = baseAcceleration;

            switch (currentAccelerationType) {
                case AccelerationType.BASE:
                    accelerationCurve = baseAcceleration;
                    break;
                case AccelerationType.FORWARD:
                    accelerationCurve = forwardAcceleration;
                    break;
                case AccelerationType.WHIP:
                    accelerationCurve = whipAcceleration;
                    rebellionStacks++;
                    break;
            }

            float acceleration = ComputeCurve(accelerationCurve, maxSpeed);
            acceleration *= accelerationFactor;
            acceleration *= UnityEngine.Random.Range(1f - accelerationRandomRange, 1f + accelerationRandomRange);

            if (CurrentAccelerationType != AccelerationType.WHIP) {
                acceleration *= Time.deltaTime;
            }

            if (Speed + acceleration < maxSpeed) {
                velocity += Vector3.forward * acceleration;
            } else if (Speed < maxSpeed) {
                velocity.z = maxSpeed;
            }

            if (CurrentAccelerationType == AccelerationType.WHIP) {
                CurrentAccelerationType = AccelerationType.BASE;
            }
        }

        private float ComputeCurve(AmplitudeCurve curve) {
            return ComputeCurve(curve, maxSpeed);
        }

        private float ComputeCurve(AmplitudeCurve curve, float maxSpeed) {
            float perSpeed = Mathf.Clamp01(velocity.z / maxSpeed);
            perSpeed += Time.deltaTime;
            float perCurve = curve.curve.Evaluate(perSpeed);
            return perCurve * curve.amplitude;
        }

        private float ComputeGravity() {
            if (jumping) {
                if (velocity.y - gravityCurve.amplitude < 0) {
                    return velocity.y;
                }
                return gravityCurve.amplitude;
            }

            float percentage = Mathf.Clamp01(Mathf.Abs(velocity.y) / gravityCurve.amplitude);
            percentage += Time.deltaTime;
            float perCurve = gravityCurve.curve.Evaluate(percentage);
            return perCurve * gravityCurve.amplitude;
        }

        private void Turn() {
            float turnSpeed = turn.amplitude * turn.curve.Evaluate(SpeedRatio) * Time.deltaTime;
            //transform.Rotate(new Vector2(0, 1) * horizontalDirection * turnSpeed * Time.deltaTime, Space.Self);
            Vector3 forwardNoY = transform.forward;
            forwardNoY.y = 0f;
            forwardNoY.Normalize();
            Vector3 newForward = Quaternion.Euler(0, turnSpeed * horizontalDirection, 0) * forwardNoY;
            transform.rotation = Quaternion.LookRotation(newForward, transform.up);
            //Vector3 euler = transform.rotation.eulerAngles;
            //euler.y += turnSpeed * horizontalDirection;
            //transform.rotation = Quaternion.Euler(euler);
            //transform.rotation *= Quaternion.Euler(0, 1f * turnSpeed * horizontalDirection, 0);
        }

        private void Roll() {
            Vector3 euler = transform.rotation.eulerAngles;
            float targetRoll = 0f;
            if (horizontalDirection != 0) {
                float rollSpeed = this.rollAngle * SpeedRatio;
                targetRoll = rollSpeed * -Mathf.Sign(horizontalDirection);
            }
            euler.z = Mathf.Lerp(Tools.AcuteAngle(euler.z), targetRoll, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Euler(euler);
        }

        public void SlopeTilt() {
            RaycastHit hitFloor;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hitFloor, groundRayDistance * 10f, groundLayers)) {
                Vector3 forwardNoY = transform.forward;
                forwardNoY.y = 0f;
                forwardNoY.Normalize();
                Vector3 projectedForward = Vector3.ProjectOnPlane(forwardNoY, hitFloor.normal);
                transform.rotation = Quaternion.LookRotation(projectedForward, hitFloor.normal);
                //rotation = Quaternion.FromToRotation(Vector3.up, hitFloor.normal) * Quaternion.LookRotation(forwardNoZ, hitFloor.normal);
                //transform.rotation = Quaternion.FromToRotation(Vector3.up, hitFloor.normal) * Quaternion.LookRotation(forwardNoY, Vector3.up);
                //transform.rotation = Quaternion.LookRotation(forwardNoY, hitFloor.normal);

                Debug.DrawRay(transform.position, transform.up * 10f, Color.green);
                Debug.DrawRay(transform.position, hitFloor.normal * 10f, Color.red);
            }
        }

        private void ApplySpeed() {
            if (velocity.y <= 0f)
                jumping = false;

            float offTrackFactor = offTracking == true ? offTrackMultiplier : 1f;

            if (_rb != null) {
                _rb.velocity = RelativeDirection(velocity) * offTrackFactor;
            } else {
                transform.position += velocity * offTrackFactor;
            }
        }

        #endregion

        #region Movements setters

        public void Stun(float time) {
            TimedChange(ref frictions.curve, "frictions.curve", AnimationCurve.Constant(0f, 1f, 1f), time);
            TimedChange(ref frictions.amplitude, "frictions.amplitude", frictions.amplitude * 5f, time);
            TimedChange(ref stun, "stun", true, time);

            if (OnStun != null) {
                OnStun(time);
            }
        }

        public void SetHorizontalDirection(float direction) {
            targetHorizontalDirection = direction;
        }

        public void Decelerate() {
            isDecelerate = true;
        }

        public void Accelerate(AccelerationType accelerationType, float accelerationFactor = 1f) {
            CurrentAccelerationType = accelerationType;
            this.accelerationFactor = accelerationFactor;
        }

        public void AddVelocity(Vector3 velocity) {
            if (velocity.y > 0f)
                jumping = true;

            this.velocity += velocity;
        }

        public void Slow(float slow) {
            if (!slowable) { return; }

            if (velocity.z > 0f) {
                AddVelocity(Vector3.back * Mathf.Abs(slow));
                if (velocity.z < 0f) {
                    velocity.z = 0f;
                }
            } else if (velocity.z < 0f) {
                AddVelocity(Vector3.forward * Mathf.Abs(slow));
                if (velocity.z > 0f) {
                    velocity.z = 0f;
                }
            }

            slowable = false;
            StartCoroutine(Delay((string s, bool b) => ModifyValue(s, b), "slowable", true, slowRecoveryTime));
        }

        public void NegateVelocity(params Axis[] axis) {
            for (int i = 0; i < axis.Length; i++) {
                switch (axis[i]) {
                    case Axis.X:
                        velocity.x = 0;
                        break;
                    case Axis.Y:
                        velocity.y = 0;
                        break;
                    case Axis.Z:
                        velocity.z = 0;
                        break;
                }
            }
        }

        public void Jump(float force = 0f) {
            if (force == 0f) { force = jumpForce; }
            AddVelocity(Vector3.zero.Override(force, Axis.Y));
        }

        public void Bump(Vector3 worldDirection, float force, bool dependingSpeedState = true) {
            if (dependingSpeedState) {
                if (offTracking) {
                    // Offtrack !
                } else {
                    Debug.Log(SpeedState);
                    switch (SpeedState) {
                        case SpeedStates.STOPPED:
                            force = 0f;
                            break;
                        case SpeedStates.CRUSADE:
                            force *= 0.8f;
                            break;
                        case SpeedStates.HIGH:
                            Stun(bounceStunTime / 2f);
                            break;
                        case SpeedStates.OVER:
                            Stun(bounceStunTime);
                            break;
                    }
                }
            }

            NegateVelocity(Axis.Z);
            Vector3 direction = worldDirection.Redirect(transform.forward, Vector3.forward);
            AddVelocity(direction * force);
        }

        #region TimedChanged

        public void TimedChange<T>(ref T variable, string variableName, T value, float time) {
            T baseValue = variable;
            variable = value;

            if (timedChangedRoutines.ContainsKey(variableName)) {
                baseValue = (T)timedChangedRoutines[variableName].value;
                StopCoroutine(timedChangedRoutines[variableName].routine);
                timedChangedRoutines.Remove(variableName);
            }

            Coroutine routine = StartCoroutine(Delay((string s, T t) => ModifyValue(s, t), variableName, baseValue, time));
            timedChangedRoutines.Add(variableName, new TimedChangeCoroutineStruct<object>(routine, baseValue));
        }

        private IEnumerator Delay<T1, T2>(Tools.BasicDelegate<T1, T2> function, T1 arg1, T2 arg2, float time) {
            yield return new WaitForSeconds(time);
            function(arg1, arg2);
        }

        private void ModifyValue<T>(string variableName, T value) {
            //(float)Convert.ChangeType(value, typeof(float));
            switch (variableName) {
                case "maxSpeed":
                    maxSpeed = To(value, 0f);
                    break;
                case "gravityCurve.amplitude":
                    gravityCurve.amplitude = To(value, 0f);
                    break;
                case "gravityCurve.curve":
                    gravityCurve.curve = To(value, new AnimationCurve());
                    break;
                case "deceleration.amplitude":
                    deceleration.amplitude = To(value, 0f);
                    break;
                case "frictions.amplitude":
                    frictions.amplitude = To(value, 0f);
                    break;
                case "frictions.curve":
                    frictions.curve = To(value, new AnimationCurve());
                    break;
                case "stun":
                    stun = To(value, false);
                    break;
                case "turn.amplitude":
                    turn.amplitude = To(value, 0f);
                    break;
                case "slowable":
                    slowable = To(value, false);
                    break;
                default:
                    Debug.LogWarning("Value not known : " + variableName);
                    break;
            }
        }

        private static T To<T>(object input, T value) {
            T result = value;
            try {
                if (input == null || input == DBNull.Value) { return result; }

                result = (T)Convert.ChangeType(input, typeof(T));
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return result;
        }

        #endregion

        #endregion

        #region Setters

        public void ResetDecelerateTimer() {
            decelerateTimer = decelerateTime;
        }

        #endregion

        #region Getters

        private Vector3 RelativeDirection(Vector3 vector) {
            float angle = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
            return Quaternion.AngleAxis(angle, Vector3.up) * vector;
        }

        private bool IsGrounded() {
            RaycastHit hitFloor;
            //if (Physics.Raycast(groundCheck.position, -transform.up, out hitFloor, groundRayDistance, layerMask)) {
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hitFloor, groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore)) {
                Debug.Log(hitFloor.collider.tag);
                if (hitFloor.collider.gameObject.tag == "OffTrack") {
                    offTracking = true;
                } else {
                    offTracking = false;
                }
                return true;
            }
            return false;
        }

        //private bool IsGrounded() {
        //    RaycastHit[] hitFloor = Physics.RaycastAll(groundCheck.position, Vector3.down, groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore);
        //    bool grounded = false;
        //    //if (Physics.Raycast(groundCheck.position, -transform.up, out hitFloor, groundRayDistance, layerMask)) {
        //    //if (Physics.RaycastAll(groundCheck.position, Vector3.down, out hitFloor, groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore)) {
        //    if (hitFloor.Length > 0) {
        //        grounded = true;
        //        for (int i = 0; i < hitFloor.Length; i++) {
        //            Debug.Log(hitFloor[i].collider.tag);
        //            if (hitFloor[i].collider.gameObject.tag == "OffTrack") {
        //                offTracking = true;
        //            } else {
        //                offTracking = false;
        //            }

        //        }
        //    }
        //    return grounded;
        //}

        #endregion

        private void OnCollisionEnter(Collision collision) {
            float dot = Vector3.Dot(collision.contacts[0].normal, -transform.forward);
            if (dot != 0f) {
                Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal * 20f, Color.green, 20f);
                Debug.DrawRay(collision.contacts[0].point, -transform.forward * 20f, Color.red, 20f);
            }
            if (/*!(groundLayers.Contains(collision.gameObject.layer)) &&*/
                Mathf.Abs(collision.contacts[0].point.y - transform.position.y) < 0.5f &&
                Mathf.Lerp(90f, 0f, Vector3.Dot(collision.contacts[0].normal, -transform.forward)) < faceAngle
            ) {
                Debug.Log($"{gameObject.name} collided with : {collision.gameObject.name}");
                Bump(-transform.forward, bounceForce);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "DeathBox") {
                NegateVelocity(Axis.Z, Axis.Y, Axis.X);
                FallManager.instance.CheckPlayerBestCheckPoint(transform.gameObject, lastGroundPos);
            }
        }
    }
}
