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

        //public AmplitudeCurve acceleration = null;
        [HideInInspector] public float maxSpeed;
        public float backwardSpeed = 10f;
        [HideInInspector] public bool isAccelerate;

        private Vector3 offTrackVelocity = Vector3.zero;

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

        [HideInInspector] private bool needLastGroundPosUpdate;
        [HideInInspector] public Vector3 lastGroundPos;
        [HideInInspector] public float respawnOrientationFactor = 1f;

        [HideInInspector] public bool jumping = false;

        public float jumpForce;
        public LayerMask groundLayers;
        [HideInInspector] public bool stun;

        [Header("Collisions")]
        [SerializeField] private float stepHeight = 0.5f;
        [Range(0f, 90f), SerializeField] private float faceAngle = 50f;
        public float bounceForce = 20f;
        public float bounceStunTime = 2f;
        [Range(0f, 1f), SerializeField] private float offTrackMultiplier = 0.3f;
        [HideInInspector] public bool offTracking = false;

        [Header("Recovery")]
        [SerializeField] private float slowRecoveryTime = 2f;

        [HideInInspector] public Tools.BasicDelegate<float> OnStun;
        [HideInInspector] public Tools.BasicDelegate<float> OnFallingDeath;
        [HideInInspector] public Tools.BasicDelegate OnRespawn;
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
                horizontalDirection = Mathf.MoveTowards(horizontalDirection, targetHorizontalDirection, horizontalDirectionSpeed * Time.fixedDeltaTime);
            }

            if (horizontalDirection != 0f && !stun) {
                isTurn = true;
                Turn();
            } else {
                isTurn = false;
            }
            //Roll();

            float frictions = ComputeCurve(this.frictions);
            Vector3 frictionsMask = Vector3.one;

            if (isDecelerate && !stun) {
                float deceleration = ComputeCurve(this.deceleration);
                velocity += Vector3.forward * -deceleration * Time.fixedDeltaTime;
                if (offTrackVelocity.z > 0f) {
                    offTrackVelocity += -deceleration * Vector3.forward * Time.fixedDeltaTime * 2f;
                }

                if (velocity.z < -backwardSpeed) {
                    velocity = Vector3.forward * -backwardSpeed;
                }
            } else if (Speed < maxSpeed && !stun) {
                AccelerationUpdate();
                ResetDecelerateTimer();
                frictionsMask.z = 0f;
            }

            if (decelerateTimer > 0f) {
                frictionsMask.z = 0f;
            }

            if (frictions * Time.fixedDeltaTime > velocity.sqrMagnitude) {
                velocity = Vector3.zero.Override(velocity.y, Axis.Y);
            } else {
                velocity += -frictions * velocity.normalized.MultiplyIndividually(frictionsMask) * Time.fixedDeltaTime;
            }

            if (offTrackVelocity != Vector3.zero) {
                offTrackVelocity += -frictions * offTrackVelocity.normalized * Time.fixedDeltaTime * 2f;
                if (offTrackVelocity.z < 0f) {
                    offTrackVelocity.z = 0f;
                }
            }

            if (decelerateTimer > 0f) {
                decelerateTimer -= Time.fixedDeltaTime;
            }


            velocity += ComputeGravity() * Vector3.down;

            ApplySpeed();
            // SlopeTilt();
        }

        private void AccelerationUpdate() {
            if (currentAccelerationType == AccelerationType.NONE) { return; }

            AmplitudeCurve accelerationCurve = baseAcceleration;

            bool accelerationToOfftrack = false;
            switch (currentAccelerationType) {
                case AccelerationType.BASE:
                    accelerationCurve = baseAcceleration;
                    break;
                case AccelerationType.FORWARD:
                    accelerationCurve = forwardAcceleration;
                    break;
                case AccelerationType.WHIP:
                    accelerationCurve = whipAcceleration;
                    accelerationToOfftrack = true;
                    break;
            }

            float acceleration = ComputeCurve(accelerationCurve, maxSpeed);
            acceleration *= accelerationFactor;
            acceleration *= UnityEngine.Random.Range(1f - accelerationRandomRange, 1f + accelerationRandomRange);

            if (CurrentAccelerationType != AccelerationType.WHIP) {
                acceleration *= Time.fixedDeltaTime;
            }

            if (Speed + acceleration < maxSpeed) {
                velocity += Vector3.forward * acceleration;
                if (accelerationToOfftrack) {
                    offTrackVelocity += acceleration * Vector3.forward;
                }
            } else if (Speed < maxSpeed) {
                velocity.z = maxSpeed;
                if (accelerationToOfftrack) {
                    offTrackVelocity.z = maxSpeed;
                }
            }

            //if (CurrentAccelerationType == AccelerationType.WHIP) {
            //    CurrentAccelerationType = AccelerationType.BASE;
            //}
        }

        private float ComputeCurve(AmplitudeCurve curve) {
            return ComputeCurve(curve, maxSpeed);
        }

        private float ComputeCurve(AmplitudeCurve curve, float maxSpeed) {
            float perSpeed = Mathf.Clamp01(velocity.z / maxSpeed);
            perSpeed += Time.fixedDeltaTime;
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
            percentage += Time.fixedDeltaTime;
            float perCurve = gravityCurve.curve.Evaluate(percentage);
            return perCurve * gravityCurve.amplitude;
        }

        private void Turn() {
            float turnSpeed = turn.amplitude * turn.curve.Evaluate(SpeedRatio) * Time.fixedDeltaTime;
            //transform.Rotate(new Vector2(0, 1) * horizontalDirection * turnSpeed * Time.fixedDeltaTime, Space.Self);
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
            euler.z = Mathf.Lerp(Tools.AcuteAngle(euler.z), targetRoll, Time.fixedDeltaTime * 10f);
            transform.rotation = Quaternion.Euler(euler);
        }

        public Quaternion SlopeTilt() {
            RaycastHit hitFloor;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hitFloor, groundRayDistance, groundLayers)) {
                Vector3 forwardNoY = transform.forward;
                forwardNoY.y = 0f;
                forwardNoY.Normalize();
                Vector3 projectedForward = Vector3.ProjectOnPlane(forwardNoY, hitFloor.normal);
                //Debug.DrawRay(transform.position, transform.up * 10f, Color.green);
                Quaternion retour = Quaternion.LookRotation(projectedForward, hitFloor.normal);
                Debug.DrawRay(transform.position, retour * Vector3.up * 10f, Color.green);
                Debug.DrawRay(transform.position, hitFloor.normal * 10f, Color.red);
                return retour;
                //transform.rotation = Quaternion.LookRotation(projectedForward, hitFloor.normal);
                //rotation = Quaternion.FromToRotation(Vector3.up, hitFloor.normal) * Quaternion.LookRotation(forwardNoZ, hitFloor.normal);
                //transform.rotation = Quaternion.FromToRotation(Vector3.up, hitFloor.normal) * Quaternion.LookRotation(forwardNoY, Vector3.up);
                //transform.rotation = Quaternion.LookRotation(forwardNoY, hitFloor.normal);

            }
            return Quaternion.identity;
        }

        private void ApplySpeed() {
            if (this.velocity.y <= 0f)
                jumping = false;

            //float offTrackFactor = offTracking ? offTrackMultiplier : 1f;
            Vector3 velocity = this.velocity;
            if (offTracking) {
                velocity = (velocity - offTrackVelocity) * offTrackMultiplier + offTrackVelocity;
            }

            if (_rb != null) {
                _rb.velocity = RelativeDirection(velocity);
            } else {
                transform.position += RelativeDirection(velocity);
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
            if (accelerationType == AccelerationType.WHIP) {
                AccelerationUpdate();
                CurrentAccelerationType = AccelerationType.BASE;
            }
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

        public void Bump(Vector3 worldDirection, float force, SpeedStates state) {
            if (offTracking) {
                force *= 2f;
                Bump(worldDirection, force, 0f);
            } else {
                //Debug.Log(SpeedState);
                switch (state) {
                    case SpeedStates.STOPPED:
                        return;
                    case SpeedStates.CRUSADE:
                        force *= 0.8f;
                        Bump(worldDirection, force, 0f);
                        return;
                    case SpeedStates.HIGH:
                        Bump(worldDirection, force, bounceStunTime / 2f);
                        return;
                    case SpeedStates.OVER:
                        Bump(worldDirection, force, bounceStunTime);
                        return;
                }
            }
        }

        public void Bump(Vector3 worldDirection, float force, float stunTime) {
            NegateVelocity(Axis.Z);
            Vector3 direction = worldDirection.Redirect(transform.forward, Vector3.forward);
            AddVelocity(direction * force);
            Stun(stunTime);
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
            Vector3 direction = -groundCheck.transform.up;
            Debug.DrawRay(groundCheck.position, direction * groundRayDistance, Color.green);
            RaycastHit hitFloor;
            //if (Physics.Raycast(groundCheck.position, -transform.up, out hitFloor, groundRayDistance, layerMask)) {
            if (Physics.Raycast(groundCheck.position, direction, out hitFloor, groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore)) {
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

            if (/*!(groundLayers.Contains(collision.gameObject.layer)) &&*/
                Mathf.Abs(collision.contacts[0].point.y - groundCheck.position.y) > stepHeight &&
                Mathf.Lerp(90f, 0f, Vector3.Dot(collision.contacts[0].normal, -transform.forward)) < faceAngle
            ) {
                //Debug.LogWarning($"{gameObject.name} collided with : {collision.gameObject.name}");
                if (collision.gameObject.CompareTag("Player")) {
                    collision.gameObject.GetComponent<Movement>().Bump(transform.forward, bounceForce, SpeedState);
                }
                Bump(-transform.forward, bounceForce, SpeedState);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "DeathBox") {
                NegateVelocity(Axis.Z, Axis.X);
                float timeToRespawn = 1f;
                OnFallingDeath(timeToRespawn);
                StartCoroutine(Tools.Delay(Respawn, timeToRespawn));
                //NegateVelocity(Axis.Z, Axis.Y, Axis.X);
                //FallManager.instance.CheckPlayerBestCheckPoint(transform.gameObject, lastGroundPos);
            }
        }

        private void Respawn() {
            FallManager.instance.CheckPlayerBestCheckPoint(transform.gameObject, lastGroundPos, (respawnOrientationFactor == -1f));
            
            OnRespawn();
        }
    }
}
