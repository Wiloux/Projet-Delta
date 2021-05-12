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
        [SerializeField] private bool _onJoyMode;

        [Header("Acceleration")]
        [SerializeField] private Vector3 accelPosition;
        [SerializeField] private Vector3 accelDelta;
        [SerializeField] private float accelDuration;

        [Header("Deceleration")]
        [SerializeField] private Vector3 decelPosition;
        [SerializeField] private float decelDuration;

        [Header("Turn")]
        [SerializeField] private Vector3 turnPosition;
        [SerializeField] private Vector3 turnRotation;
        [SerializeField] private float turnDuration;

        [Header("in the air")]
        [SerializeField] private Vector3 airPosition;
        [SerializeField] private Vector3 airRotation;
        [SerializeField] private float airDuration;

        [Header("Rewired")]
        public string playerName;
        private Player player;

        private void Start() {
            player = ReInput.players.GetPlayer(movement.playerName);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update() {
            // Acceleration
            if (movement.isAccelerate && movement.Speed < movement.maxSpeed && !movement.isTurn) {
                Vector3 accelpos = Vector3.Lerp(accelPosition, accelPosition + accelDelta, Mathf.Clamp01(movement.Speed / movement.maxSpeed));
                cam.CameraMovePosition(accelpos, accelDuration);
                _onJoyMode = true;
            }

            // Deceleration
            if (movement.isDecelerate && movement.Speed > 0 && !movement.isTurn)
                cam.CameraMovePosition(decelPosition, decelDuration);

            // Arrêt
            if (movement.Speed < 0.5f && !movement.airborn) {
                if (_onJoyMode) {
                    _onJoyMode = false;
                    cam.ResetCameraJoystickPos(_target);
                }
                cam.CameraJoystickRotation(player.GetAxis("CamHorizontal"), player.GetAxis("CamVertical"), _rotationSpeed, _target);
            }

            // Tourner
            if (movement.isTurn && movement.Speed > 1f) {
                cam.ResetTarget(accelDuration, _target);
                Vector3 pos = turnPosition;
                if (movement.isAccelerate) {
                    pos = Vector3.Lerp(accelPosition, accelPosition + accelDelta, Mathf.Clamp01(movement.Speed / movement.maxSpeed));
                } else if (movement.isDecelerate) {
                    pos = decelPosition;
                }

                cam.CameraMove(new Vector3(turnPosition.x * movement.HorizontalDirection, turnPosition.y, pos.z), turnRotation * movement.HorizontalDirection, turnDuration);
            } else {
                // Reset du non turn
                cam.ResetTarget(accelDuration, _target);
                cam.CameraMoveRotation(new Vector3(0, 0, 0), accelDuration);
                if (!movement.isAccelerate && !movement.isDecelerate)
                    cam.CameraMovePosition(cam._initPos, 1f);
            }

            // Airborn
            if (movement.airborn)
                cam.CameraMove(airPosition, airRotation, airDuration);
        }
    }
}
