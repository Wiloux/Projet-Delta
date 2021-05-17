using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using ToolsBoxEngine;

namespace Florian {
    public class Movement : Character {
        private Rigidbody _rb;
        public GameObject model;
        public Camera playerCamera;
        public Transform body;

        [Header("Movement velocity")]
        public float speed;
        public float maxSpeed;
        [SerializeField] private float backwardSpeed = 10f;
        public float turnSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
        public bool isAccelerate;
        public bool isTurn;
        public bool isDecelerate;
        public float direction;

        [Header("Orientation")]
        public float maxTurnSpeed;
        public float minTurnSpeedPercentage;

        [Header("Rewired")]
        private Rewired.Player player;
        public string playerName;

        [Header("Air Detection")]
        public float gravity;
        public float jumpForce;
        public bool airborn;
        public LayerMask layerMask;

        [Header("Other")]
        public TextMeshProUGUI placementText = null;
        public TextMeshProUGUI lapsText = null;
        public int maxLaps = 2;

        [Header("Rework")]
        [SerializeField] private AmplitudeCurve acceleration = null;
        [SerializeField] private AmplitudeCurve deceleration = null;
        [SerializeField] private AmplitudeCurve frictions = null;
        [SerializeField] private float decelerateTime = 0.2f;
        private float decelerateTimer = 0.2f;
        [SerializeField] private AmplitudeCurve turn = null;

        private Vector3 velocity = Vector3.zero;
        private float horizontalDirection = 0f;
        private int accelerationIteration = 0;

        #region Properties

        public int Placement {
            set {
                placementText.text = value.ToString();
            }
        }

        public int Laps {
            set {
                lapsText.text = value.ToString() + "/" + maxLaps;
            }
        }

        public float Speed {
            get { return velocity.magnitude; }
        }

        public float HorizontalDirection {
            get { return horizontalDirection; }
        }

        #endregion

        #region Unity callbacks

        void Start() {
            _rb = GetComponent<Rigidbody>();
            SetController(playerName);
        }

        void Update() {
            isAccelerate = decelerateTimer > 0f;
            isDecelerate = false;
            isTurn = false;
            airborn = !isGrounded();

            float horizontalDirection = 0f;

            if (player.GetButton("Cheat")) {
                //accelerationIteration++;
                decelerateTimer = decelerateTime;
            }

            if (player.GetButton("Accelerate")) {
                if (player.GetButtonDown("Accelerate")) {
                    accelerationIteration++;
                }
                decelerateTimer = decelerateTime;
            }

            if (player.GetButton("Decelerate")) {
                isDecelerate = true;
            }

            if (player.GetAxis("Horizontal") != 0) {
                horizontalDirection += player.GetAxis("Horizontal");
            }

            //float angle = Vector3.Angle(Vector3.forward, transform.forward);
            //movementDirection = Quaternion.AngleAxis(angle, Vector3.up) * direction.normalized;
            this.horizontalDirection = horizontalDirection;
            UpdateMovements();
            Gravity();
            //UpdateRotation();
            Debug.Log("Vel : " + velocity);
        }

        #endregion

        private void UpdateMovements() {
            bool isMoving = (horizontalDirection != 0f || accelerationIteration > 0);

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

                    if (Mathf.Abs(frictions) <= 0f || Mathf.Abs(frictions) >= this.frictions.amplitude) {
                        velocity = Vector3.zero;
                    }
                }
            }

            accelerationIteration = 0;
            ApplySpeed();
        }

        private float ComputeCurve(AmplitudeCurve curve) {
            float curSpeed = velocity.magnitude;
            float perSpeed = Mathf.Clamp01(curSpeed / maxSpeed);
            float perCurve = curve.curve.Evaluate(perSpeed);
            return perCurve * curve.amplitude;
        }

        private void Turn() {
            float turnSpeed = turn.amplitude * turn.curve.Evaluate(Mathf.Clamp01(velocity.magnitude / maxSpeed));
            transform.Rotate(new Vector2(0, 1) * horizontalDirection * turnSpeed * Time.deltaTime, Space.Self);
        }


        private void ApplySpeed() {
            if (_rb != null) {
                _rb.velocity = RelativeDirection(velocity);
            } else {
                transform.position += velocity;
            }
        }

        #region Setters

        public void SetController(string name) {
            player = ReInput.players.GetPlayer(name);
            if (player != null) { Debug.Log("Controller found : " + player.name); } else { Debug.LogWarning("Controller not found"); return; }
            playerName = name;
        }

        public void SetController(string name, Controller controller) {
            player = ReInput.players.GetPlayer(name);
            player.controllers.ClearAllControllers();
            player.controllers.AddController(controller, true);
            if (player != null) { Debug.Log("Controller found : " + player.name); } else { Debug.LogWarning("Controller not found"); }
            playerName = name;
        }

        public void SetCamera(int playerId, int maxPlayer) {
            playerCamera.rect = Tools.GetPlayerRect(playerId, maxPlayer);
        }

        public void ChangeTexture(Material mat) {
            body.GetComponent<MeshRenderer>().material = mat;
        }

        #endregion

        private Vector3 RelativeDirection(Vector3 vector) {
            float angle = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
            return Quaternion.AngleAxis(angle, Vector3.up) * vector;
        }

        public float TurnSpeedHandler(float speed) {
            return Mathf.Lerp(maxTurnSpeed, maxTurnSpeed * minTurnSpeedPercentage, speed / maxSpeed);
        }

        public void Gravity() {
            _rb.AddForce(gravity * Vector3.down, ForceMode.Acceleration);
        }

        bool isGrounded() {
            RaycastHit hitFloor;
            if (Physics.Raycast(transform.position + (transform.up * 0.2f), Vector3.down, out hitFloor, 2.0f, layerMask)) {
                return true;
            } else {
                return false;
            }
        }

        public void Jump() {
            _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }
}
