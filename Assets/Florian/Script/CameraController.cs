using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using ToolsBoxEngine;

namespace Florian {
    public class CameraController : MonoBehaviour {
        [Header("Camera")]
        [SerializeField] private Transform _target;
        [SerializeField] private MovementController movement;
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
        private bool lookingBack = false;

        [Header("Rewired")]
        public string playerName;
        private Player player;

        private void Start() {
            player = ReInput.players.GetPlayer(movement.playerName);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update() {
            if (player.GetButton("Look back")) {
                cam.CameraMove(lookBackPosition, lookBackRotation, 0f);
                lookingBack = true;
            } else {
                if(lookingBack) {
                    cam.CameraMove(cam._initPos, cam._initRot, 0f);
                    lookingBack = false;
                }

                float percentageSpeed = Mathf.Clamp01(movement.Speed / movement.MaxSpeed);
                float zPos = Mathf.Lerp(_minZ, _maxZ, percentageSpeed);
                if (movement.Speed == 0) { zPos = cam._initPos.z; }

                Debug.Log(movement.Accelerating);
                // Acceleration
                if (movement.Accelerating && movement.Speed < movement.MaxSpeed && !movement.Turning) {
                    //Vector3 accelpos = Vector3.Lerp(accelPosition, accelPosition + accelDelta, Mathf.Clamp01(movement.Speed / movement.maxSpeed));
                    if (zPos > cam._initPos.z) { zPos = Mathf.Lerp(cam._initPos.z, _maxZ, percentageSpeed); }

                    cam.CameraMovePosition(accelPosition.Override(zPos, Axis.Z), accelDuration);
                    _onJoyMode = false;
                }

                // Deceleration
                if (movement.Decelerating && movement.Speed > 0 && !movement.Turning) {
                    zPos = Mathf.Lerp(_minZ, _maxZ, Mathf.Clamp01(movement.Speed / (movement.MaxSpeed + decelerationOffset)));

                    cam.CameraMovePosition(decelPosition.Override(zPos, Axis.Z), decelDuration);
                }

                // Arrêt
                if (movement.Speed < 0.5f && !movement.Airborn) {
                    if (!_onJoyMode) {
                        _onJoyMode = true;
                        cam.ResetCameraJoystickPos(Vector3.zero);
                    }
                    cam.CameraJoystickRotation(player.GetAxis("CamHorizontal"), player.GetAxis("CamVertical"), _rotationSpeed, _target);
                }
                
                // Tourner
                if (movement.Turning && movement.Speed > 1f) {
                    cam.ResetTarget(accelDuration, _target);

                    Vector3 turnRotation = Vector3.Lerp(turnRotationMin, turnRotationMax, percentageSpeed);
                    cam.CameraMove(
                        new Vector3(turnPosition.x * movement.physics.HorizontalDirection, turnPosition.y, zPos),
                        turnRotation * movement.physics.HorizontalDirection,
                        turnDuration
                    );
                } else {
                    // Reset du non turn
                    cam.ResetTarget(accelDuration, _target);
                    cam.CameraMoveRotation(new Vector3(0, 0, 0), accelDuration);
                    if (!movement.Accelerating && !movement.Decelerating && movement.Speed > 0.5f)
                        cam.CameraMovePosition(cam._initPos, 1f);
                }

                // Airborn
                if (movement.Airborn)
                    cam.CameraMove(airPosition, airRotation, airDuration);
            }
        }
    }
}
