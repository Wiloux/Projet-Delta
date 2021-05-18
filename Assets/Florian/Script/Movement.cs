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
        private Rigidbody _rb;
        [HideInInspector] public Vector3 velocity = Vector3.zero;
        private float horizontalDirection = 0f;

        [Header("Acceleration")]
        [SerializeField] private AmplitudeCurve acceleration = null;
        public float maxSpeed;
        [SerializeField] private float backwardSpeed = 10f;
        [HideInInspector] public bool isAccelerate;
        private int accelerationIteration = 0;

        [Header("Deceleration")]
        [SerializeField] private AmplitudeCurve deceleration = null;
        [HideInInspector] public bool isDecelerate;

        [Header("Frictions")]
        [SerializeField] private AmplitudeCurve frictions = null;
        [SerializeField] private float decelerateTime = 0.2f;
        private float decelerateTimer = 0.2f;

        [Header("Turn")]
        [SerializeField] private AmplitudeCurve turn = null;
        [HideInInspector] public bool isTurn;

        [Header("Air Detection")]
        [SerializeField] private AmplitudeCurve gravityCurve = null;
        [SerializeField] private Transform groundCheck = null;
        [HideInInspector] public bool airborn;

        public float jumpForce;
        public LayerMask layerMask;

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
        }

        void Update() {
            isAccelerate = decelerateTimer > 0f;
            isDecelerate = false;
            isTurn = false;
            airborn = !isGrounded();

            if (!airborn) {
                velocity.y = 0f;
            }
        }

        #endregion

        #region UpdateMovements

        public void UpdateMovements() {
            if (horizontalDirection != 0f) {
                isTurn = true;
                Turn();
            } else {
                isTurn = false;
            }

            if (isDecelerate) {
                float deceleration = ComputeCurve(this.deceleration);
                velocity += Vector3.forward * -deceleration * Time.deltaTime;

                if (velocity.z < -backwardSpeed) {
                    velocity = Vector3.forward * -backwardSpeed;
                }
            } else if (accelerationIteration > 0) {
                for (int i = 0; i < accelerationIteration; i++) {
                    float acceleration = ComputeCurve(this.acceleration);
                    velocity += Vector3.forward * acceleration * Time.deltaTime;

                    if (velocity.sqrMagnitude > maxSpeed * maxSpeed) {
                        velocity = Vector3.forward * maxSpeed;
                    }
                }
            } else {
                if (decelerateTimer > 0) {
                    decelerateTimer -= Time.deltaTime;
                } else {
                    float frictions = ComputeCurve(this.frictions);
                    velocity += -frictions * velocity.normalized * Time.deltaTime;

                    if (velocity.z <= 5f) {
                        velocity = Vector3.zero;
                    }
                }
            }

            velocity += ComputeGravity() * Vector3.down;

            accelerationIteration = 0;
            ApplySpeed();
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

        public void Jump() {
            _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }

        private void ApplySpeed() {
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

        bool isGrounded() {
            RaycastHit hitFloor;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hitFloor, 0.2f, layerMask)) {
                return true;
            } else {
                return false;
            }
        }

        #endregion
    }
}
