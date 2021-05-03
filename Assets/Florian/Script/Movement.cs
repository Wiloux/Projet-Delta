using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;
using ToolsBoxEngine;


namespace Florian
{

    public class Movement : MonoBehaviour
    {
        private Rigidbody _rb;
        public GameObject model;
        public Camera playerCamera;
        public Transform body;

        [Header("Movement velocity")]
        public float speed;
        public float maxSpeed;
        public float turnSpeed;
        public float accelerationSpeed;

        [Header("Orientation")]
        public float maxTurnSpeed;
        public float minTurnSpeedPercentage;

        [Header("Rewired")]
        private Rewired.Player player;
        public string playerName;

        [Header("Air Detection")]
        public float gravity;
        public bool airborn;
        public LayerMask layerMask;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            SetController(playerName);
        }


        void Update()
        {
            turnSpeed = TurnSpeedHandler(speed);

            if (player.GetButton("Accelerate") && speed < maxSpeed)
            {
                speed += accelerationSpeed * Time.deltaTime;
            }
            else if (speed > 0)
            {
                speed -= accelerationSpeed * Time.deltaTime;
            }

            if (player.GetButton("Decelerate") && speed < maxSpeed)
            {
                speed += accelerationSpeed * Time.deltaTime;
            }

            if (player.GetAxis("Horizontal") != 0)
            {
                int direction = player.GetAxis("Horizontal") > 0 ? 1 : -1;
                transform.Rotate(new Vector2(0, 1) * direction * turnSpeed * Time.deltaTime, Space.Self);
                if (model.transform.localEulerAngles.y < 120f && model.transform.localEulerAngles.y > 60)
                {
                    model.transform.Rotate(new Vector2(0, 1) * direction * turnSpeed * 1.2f * Time.deltaTime, Space.Self);
                }
            }
            else
            {
                model.transform.DOLocalRotate(new Vector3(0, 90f, 0), 0.5f).SetEase(Ease.OutBack);
            }

            _rb.AddForce(transform.forward * speed, ForceMode.Acceleration);

            Gravity();
        }

        public void SetController(string name)
        {
            player = ReInput.players.GetPlayer(name);
            if (player != null) { Debug.Log("Controller found : " + player.name); } else { Debug.LogWarning("Controller not found"); return; }
            playerName = name;
        }

        public void SetController(string name, Controller controller)
        {
            player = ReInput.players.GetPlayer(name);
            player.controllers.ClearAllControllers();
            player.controllers.AddController(controller, true);
            if (player != null) { Debug.Log("Controller found : " + player.name); } else { Debug.LogWarning("Controller not found"); }
            playerName = name;
        }

        public float TurnSpeedHandler(float speed)
        {
            return Mathf.Lerp(maxTurnSpeed, maxTurnSpeed * minTurnSpeedPercentage, speed / maxSpeed);
        }

        public void Gravity()
        {
            _rb.AddForce(gravity * Vector3.down, ForceMode.Acceleration);
        }

        bool isGrounded()
        {
            RaycastHit hitFloor;
            if (Physics.Raycast(transform.position + (transform.up * 0.2f), Vector3.down, out hitFloor, 2.0f, layerMask))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetCamera(int playerId, int maxPlayer)
        {
            playerCamera.rect = Tools.GetPlayerRect(playerId, maxPlayer);
        }

        public void ChangeTexture(Material mat)
        {
            body.GetComponent<MeshRenderer>().material = mat;
        }


    }
}
