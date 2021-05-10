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
        public CameraController cameraController;

        [Header("Movement velocity")]
        public float speed;
        public float maxSpeed;
        public float turnSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
        public bool isAccelerate;
        public bool isDecelerate;

        [Header("Orientation")]
        public float maxTurnSpeed;
        public float minTurnSpeedPercentage;
        public int direction;
        public bool isTurn;

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

        #endregion

        #region Unity callbacks

        void Start() {
            _rb = GetComponent<Rigidbody>();
            SetController(playerName);
        }

        void Update() {
            turnSpeed = TurnSpeedHandler(speed);
            isAccelerate = false;
            isDecelerate = false;
            isTurn = false;

            if (player.GetButton("Accelerate") && speed < maxSpeed) {
                isAccelerate = true;
                speed += accelerationSpeed * Time.deltaTime;
            } else if (speed > 0) {
                speed -= accelerationSpeed * Time.deltaTime;
            }

            if (player.GetButton("Decelerate") && speed > 0) {
                isDecelerate = true;
                speed -= decelerationSpeed * Time.deltaTime;
            }

            if (player.GetAxis("Horizontal") != 0) {
                isTurn = true;
                direction = player.GetAxis("Horizontal") > 0 ? 1 : -1;
                transform.Rotate(new Vector2(0, 1) * direction * turnSpeed * Time.deltaTime, Space.Self);
                //model.transform.Rotate(new Vector2(0, 1) * direction * turnSpeed * 1.2f * Time.deltaTime, Space.Self);
            } else {
                model.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutBack);
            }

            //_rb.AddForce(transform.forward * speed, ForceMode.Acceleration);
            _rb.velocity = (transform.forward * speed).Override(_rb.velocity.y, Axis.Y);

            airborn = isGrounded();
            if (player.GetButton("Jump") && airborn) {
                Jump();
            }
            Gravity();
        }

        #endregion

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

        public void SetCamera(int playerId, int maxPlayer) {
            playerCamera.rect = Tools.GetPlayerRect(playerId, maxPlayer);
        }

        public void ChangeTexture(Material mat) {
            body.GetComponent<MeshRenderer>().material = mat;
        }

        public void Jump() {
            _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }
}
