using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using ToolsBoxEngine;

namespace Florian {
    public class CameraController : MonoBehaviour {
        [Header("Camera")]
        [SerializeField] private Transform _target;
        [SerializeField] private Movement movement;
        [SerializeField] private CameraManager cam;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _minZ;
        [SerializeField] private float _maxZ;
        private bool _onJoyMode;

        [Header("Acceleration")]
        [SerializeField] private Vector3 accelPosition;
        [SerializeField] private Vector3 accelDelta;
        [SerializeField] private float accelDuration;

        [Header("Deceleration")]
        [SerializeField] private Vector3 decelPosition;
        [SerializeField] private float decelDuration;
        [SerializeField] private float decelerationOffset;

        [Header("Turn")]
        [SerializeField] private Vector3 turnPosition;
        [SerializeField] private Vector3 turnRotationMin;
        [SerializeField] private Vector3 turnRotationMax;
        [SerializeField] private float turnDuration;

        [Header("In the air")]
        [SerializeField] private Vector3 airPosition;
        [SerializeField] private Vector3 airRotation;
        [SerializeField] private float airDuration;

        [Header("Look back")]
        [SerializeField] private Vector3 lookBackPosition;
        private Vector3 lookBackRotation = new Vector3(0, -180f, 0);

        [Header("Rewired")]
        public string playerName;
        private Player player;

        private Coroutine cameraMoving;

        private void Start() {
            player = ReInput.players.GetPlayer(movement.playerName);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update() {
            if (player.GetButton("Look back")) {
                cam.CameraMove(lookBackPosition, lookBackRotation, 0.1f);
            } else {
                float percentageSpeed = Mathf.Clamp01(movement.Speed / movement.maxSpeed);
                float zPos = Mathf.Lerp(_minZ, _maxZ, percentageSpeed);
                if (movement.Speed == 0) { zPos = cam._initPos.z; }

                // Acceleration
                if (movement.isAccelerate && movement.Speed < movement.maxSpeed && !movement.isTurn) {
                    //Vector3 accelpos = Vector3.Lerp(accelPosition, accelPosition + accelDelta, Mathf.Clamp01(movement.Speed / movement.maxSpeed));
                    if (zPos > cam._initPos.z) { zPos = Mathf.Lerp(cam._initPos.z, _maxZ, percentageSpeed); }

                    cam.CameraMovePosition(accelPosition.Override(zPos, Axis.Z), accelDuration);
                    _onJoyMode = false;
                }

                // Deceleration
                if (movement.isDecelerate && movement.Speed > 0 && !movement.isTurn) {
                    zPos = Mathf.Lerp(_minZ, _maxZ, Mathf.Clamp01(movement.Speed / (movement.maxSpeed + decelerationOffset)));

                    cam.CameraMovePosition(decelPosition.Override(zPos, Axis.Z), decelDuration);
                }

                // Arrêt
                if (movement.Speed < 0.5f && !movement.airborn) {
                    if (!_onJoyMode) {
                        _onJoyMode = true;
                        cam.ResetCameraJoystickPos(Vector3.zero);
                        //cameraMoving = cam.ResetTransform(0.1f, _target);
                    }
                    Debug.Log("// " + cam._rotationX + " .. " + cam._rotationY);
                    cam.CameraJoystickRotation(player.GetAxis("CamHorizontal"), player.GetAxis("CamVertical"), _rotationSpeed, _target);
                }

                // Tourner
                if (movement.isTurn && movement.Speed > 1f) {
                    cam.ResetTarget(accelDuration, _target);

                    Vector3 turnRotation = Vector3.Lerp(turnRotationMin, turnRotationMax, percentageSpeed);
                    cam.CameraMove(
                        new Vector3(turnPosition.x * movement.HorizontalDirection, turnPosition.y, zPos),
                        turnRotation * movement.HorizontalDirection,
                        turnDuration
                    );
                } else {
                    // Reset du non turn
                    cam.ResetTarget(accelDuration, _target);
                    cam.CameraMoveRotation(new Vector3(0, 0, 0), accelDuration);
                    if (!movement.isAccelerate && !movement.isDecelerate)
                        cam.CameraMovePosition(cam._initPos.Override(zPos, Axis.Z), 1f);
                }

                // Airborn
                if (movement.airborn)
                    cam.CameraMove(airPosition, airRotation, airDuration);
            }
        }
    }
}
