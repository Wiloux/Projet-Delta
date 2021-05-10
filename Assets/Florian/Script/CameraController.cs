using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Florian
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Transform _target;
        [SerializeField] private Movement movement;
        [SerializeField] private CameraManager cam;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private bool _onJoyMode;

        [Header("Acceleration")]
        [SerializeField] private Vector3 accelPosition;
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

        private void Start()
        {
            player = ReInput.players.GetPlayer(movement.playerName);
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void Update()
        {
            if (movement.isAccelerate && movement.speed != movement.maxSpeed && !movement.isTurn)
            {
                cam.CameraMovePosition(accelPosition, accelDuration);
                _onJoyMode = true;
            }

            if(movement.isDecelerate && movement.speed > 0 && !movement.isTurn)
                cam.CameraMovePosition(decelPosition, decelDuration);

            if (movement.speed < 0.5f && movement.airborn)
            {
                if (_onJoyMode)
                {
                    _onJoyMode = false;
                    cam.ResetTarget(0.2f, _target);
                }
                cam.CameraJoystickRotation(player.GetAxis("CamHorizontal"), player.GetAxis("CamVertical"), _rotationSpeed, _target);
            }
            
            if(movement.isTurn && movement.speed > 1f)
            {
                cam.ResetTarget(accelDuration, _target);
                cam.CameraMove(new Vector3((turnPosition.x * movement.direction), turnPosition.y, turnPosition.z), turnRotation * movement.direction, turnDuration);
            }
            else
            {
                cam.ResetTarget(accelDuration, _target);
                cam.CameraMoveRotation(new Vector3(0, 0, 0), accelDuration);
                if (!movement.isAccelerate && !movement.isDecelerate)
                    cam.CameraMovePosition(cam._initPos, 1f);
            }

            if (!movement.airborn)
                cam.CameraMove(airPosition, airRotation, airDuration);
        }
    }
}
