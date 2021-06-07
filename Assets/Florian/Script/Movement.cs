using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using ToolsBoxEngine;

namespace Florian {
    public class Movement : MonoBehaviour {
        private struct TimedChangeCoroutineStruct<T> {
            public Coroutine routine;
            public T value;

            public TimedChangeCoroutineStruct(Coroutine routine, T value) {
                this.routine = routine;
                this.value = value;
            }
        }

        private Rigidbody _rb;
        /*[HideInInspector]*/ public Vector3 velocity = Vector3.zero;
        private float horizontalDirection = 0f;

        [Header("Acceleration")]
        public AmplitudeCurve acceleration = null;
        public float maxSpeed;
        public float backwardSpeed = 10f;
        [HideInInspector] public bool isAccelerate;
        private int accelerationIteration = 0;

        [Header("Deceleration")]
        public AmplitudeCurve deceleration = null;
        [HideInInspector] public bool isDecelerate;

        [Header("Frictions")]
        public AmplitudeCurve frictions = null;
        [SerializeField] private float decelerateTime = 0.2f;
        private float decelerateTimer = 0.2f;

        [Header("Turn")]
        public AmplitudeCurve turn = null;
        [HideInInspector] public bool isTurn;

        [Header("Air Detection")]
        public AmplitudeCurve gravityCurve = null;
        [SerializeField] private Transform groundCheck = null;
        [SerializeField] private float groundRayDistance = 0.2f;
        [SerializeField] private float slopeRotateSpeed = 0.5f;
        [HideInInspector] public bool airborn;
        private bool jumping = false;

        public float jumpForce;
        public LayerMask layerMask;
        [HideInInspector] public bool stun;

        [Header("Recovery")]
        [SerializeField] private float slowRecoveryTime = 2f;
        private bool slowable = true;

        private Dictionary<string, TimedChangeCoroutineStruct<object>> timedChangedRoutines = null;

        #region Properties

        public float Speed {
            get { return velocity.magnitude * Mathf.Sign(velocity.z); }
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

        #endregion

        #region Unity callbacks

        void Start() {
            _rb = GetComponent<Rigidbody>();
            timedChangedRoutines = new Dictionary<string, TimedChangeCoroutineStruct<object>>();
        }

        void Update() {
            isAccelerate = decelerateTimer > 0f;
            isDecelerate = false;
            isTurn = false;
            airborn = !IsGrounded() || jumping;

            if (!airborn) {
                velocity.y = 0f;
            }
        }

        #endregion

        #region Update Movements

        public void UpdateMovements() {
            if (horizontalDirection != 0f && !stun) {
                isTurn = true;
                Turn();
            } else {
                isTurn = false;
            }

            if (isDecelerate && !stun) {
                float deceleration = ComputeCurve(this.deceleration);
                velocity += Vector3.forward * -deceleration * Time.deltaTime;

                if (velocity.z < -backwardSpeed) {
                    velocity = Vector3.forward * -backwardSpeed;
                }
            } else if (accelerationIteration > 0 && !stun) {
                for (int i = 0; i < accelerationIteration; i++) {
                    float acceleration = ComputeCurve(this.acceleration);
                    if (velocity.sqrMagnitude < maxSpeed * maxSpeed) {
                        velocity += Vector3.forward * acceleration * Time.deltaTime;
                    }
                }
            } else {
                if (decelerateTimer > 0) {
                    decelerateTimer -= Time.deltaTime;
                } else {
                    float frictions = ComputeCurve(this.frictions);
                    velocity += -frictions * velocity.normalized * Time.deltaTime;

                    if (Mathf.Clamp01(Mathf.Abs(Speed) / maxSpeed) <= 0.1f) {
                        //velocity = Vector3.zero;
                    }
                }
            }

            velocity += ComputeGravity() * Vector3.down;

            accelerationIteration = 0;
            ApplySpeed();
<<<<<<< HEAD:Assets/Florian/Script/Movement.cs
         // SlopeTilt();
=======
            SlopeTilt();
>>>>>>> parent of 680f88d (Nouveau 3C + shaders):Assets/Florian/Script/Movements/Movement.cs
        }

        private float ComputeCurve(AmplitudeCurve curve) {
            float curSpeed = velocity.magnitude;
            float perSpeed = Mathf.Clamp01(curSpeed / maxSpeed);
            float perCurve = curve.curve.Evaluate(perSpeed);
            return perCurve * curve.amplitude;
        }

        private float ComputeGravity() {
            float percentage = Mathf.Clamp01(velocity.y / gravityCurve.amplitude);
            float perCurve = gravityCurve.curve.Evaluate(percentage);
            return perCurve * gravityCurve.amplitude;
        }

        private void Turn() {
            float turnSpeed = turn.amplitude * turn.curve.Evaluate(Mathf.Clamp01(velocity.magnitude / maxSpeed));
            transform.Rotate(new Vector2(0, 1) * horizontalDirection * turnSpeed * Time.deltaTime, Space.Self);
        }

        private void SlopeTilt() {
            if (IsGrounded()) {
                RaycastHit hitFloor;
                if (Physics.Raycast(groundCheck.position, Vector3.down, out hitFloor, groundRayDistance, layerMask)) {
                    float angle = Vector3.SignedAngle(Vector3.up, hitFloor.normal, transform.right);
                    Vector3 from = Tools.AcuteAngle(transform.localEulerAngles);
                    Vector3 toRotate = from.Override(angle, Axis.X);
                    transform.localEulerAngles = Vector3.MoveTowards(from, toRotate, slopeRotateSpeed);
                    //transform.localEulerAngles = toRotate;
                }
            }
        }

        private void ApplySpeed() {
            if (velocity.y <= 0f)
                jumping = false;
            if (_rb != null) {
                _rb.velocity = RelativeDirection(velocity);
            } else {
                transform.position += velocity;
            }
        }

        #endregion

        #region Movements setters

        public void SetHorizontalDirection(float direction) {
            horizontalDirection = direction;
        }

        public void Decelerate() {
            isDecelerate = true;
        }

        public void Accelerate() {
            accelerationIteration++;
            ResetDecelerateTimer();
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
                case "deceleration.amplitude":
                    deceleration.amplitude = To(value, 0f);
                    break;
                case "frictions.amplitude":
                    frictions.amplitude = To(value, 0f);
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
            if (Physics.Raycast(groundCheck.position, -transform.up, out hitFloor, groundRayDistance, layerMask)) {
                return true;
            } else {
                return false;
            }
        }

        #endregion
    }
}
