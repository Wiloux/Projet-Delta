﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ToolsBoxEngine;
using Rewired;
using TMPro;

namespace Florian {
    public class MountKartingController : MonoBehaviour {
        [Header("Rewired")]
        public string playerName;
        private Rewired.Player player;

        [Header("Kart")]
        public Transform kartModel;
        public Transform kartNormal;
        public Rigidbody sphere;

        private float speed;
        private float currentSpeed;
        private float rotate;
        private float currentRotate;

        private int driftDirection;
        private int driftMode = 0;

        [Header("Bools")]
        public bool drifting;

        [Header("Boost")]
        public bool raphquidecide;

        [Header("Parameters")]
        public float acceleration = 30f;
        public float steering = 80f;
        public float gravity = 10f;
        public LayerMask layerMask;

        [Header("Model Parts")]
        public Transform frontWheels;
        public Transform backWheels;
        public Transform steeringWheel;
        public Transform body;
        public Camera playerCamera;

        public bool airBorn;

        [Header("Stunts")]
        public Vector3 rotationOnLeavingGround;
        public float rotateSpeed;
        public int nbOfLaps;
        public Vector2 lastDir;
        public bool startRot;
        public bool newAxis;
        public Transform parent;
        public TextMeshProUGUI comboMeter;

        private Vector2 stuntRotation = Vector2.zero;
        private Vector2 lastRot = Vector2.zero;
        private Nullable<Vector3> goToRotation = null;
        private Vector3 startRotation = Vector3.zero;
        private float goToRotationSteps = 0f;
        private float goToRotationTime = 0f;

        #region Unity callbacks

        private void Start() {
            SetController(playerName);
            //Debug.Log(player.controllers.GetFirstControllerWithTemplate<Rewired.IControllerTemplateAxisSource>());
            //Rewired.Controller controller = player.controllers;
            //for (int i = 0; i < controller.templateCount; i++) {
            //    Debug.Log(controller.name + " implements the " + controller.Templates[i].name + " Template.");
            //}
        }

        private void Update() {

            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                float time = Time.timeScale == 1 ? .2f : 1;
                Time.timeScale = time;
            }*/
            airBorn = !isGrounded();

            //Follow Collider
            transform.position = sphere.transform.position - new Vector3(0, 0.4f, 0);
            if (!airBorn) {
                //Accelerate
                if (player.GetButton("Accelerate"))
                    speed = acceleration;

                if (player.GetButton("Decelerate"))
                    speed -= acceleration * 0.2f;

                //Steer
                if (player.GetAxis("Horizontal") != 0) {
                    int dir = player.GetAxis("Horizontal") > 0 ? 1 : -1;
                    float amount = Mathf.Abs((player.GetAxis("Horizontal")));
                    Steer(dir, amount);
                }

                //Drift
                if (player.GetButtonDown("Drift") && !drifting && player.GetAxis("Horizontal") != 0) {
                    drifting = true;
                    driftDirection = player.GetAxis("Horizontal") > 0 ? 1 : -1;

                    //kartModel.parent.DOComplete();
                    //kartModel.parent.DOPunchPosition(transform.up * .2f, .3f, 5, 1);
                }

                if (drifting) {
                    float control = (driftDirection == 1) ? Tools.Remap(player.GetAxis("Horizontal"), -1, 1, 0, 2) : Tools.Remap(player.GetAxis("Horizontal"), -1, 1, 2, 0);
                    Steer(driftDirection, control);
                }

                if (player.GetButtonUp("Drift") && drifting) {
                    if (raphquidecide)
                        Boost();
                }

                currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f); speed = 0f;
                currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f); rotate = 0f;


                //if (!drifting)
                //    kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (player.GetAxis("Horizontal") * 15), kartModel.localEulerAngles.z), .2f);
                //else
                //{
                //    float control = (driftDirection == 1) ? Tools.Remap(player.GetAxis("Horizontal"), -1, 1, .5f, 2) : Tools.Remap(player.GetAxis("Horizontal"), -1, 1, 2, .5f);
                //    kartModel.parent.localRotation = Quaternion.Euler(0, Mathf.LerpAngle(kartModel.parent.localEulerAngles.y, (control * 15) * driftDirection, .2f), 0);
                //}

                frontWheels.localEulerAngles = new Vector3(0, (player.GetAxis("Horizontal") * 15), frontWheels.localEulerAngles.z);
                frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
                backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);

                steeringWheel.localEulerAngles = new Vector3(-25, 90, ((player.GetAxis("Horizontal") * 45)));
            } else {

                //Air control
                Vector2 dir = Vector2.zero;
                bool H = false;
                if (player.GetAxisRaw("Horizontal") != 0) {
                    dir.x = player.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
                    parent.transform.Rotate(new Vector3(0, 1, 0) * rotateSpeed * Time.deltaTime * dir.x, Space.Self);
                    H = true;
                } else if (player.GetAxisRaw("Vertical") != 0) {
                    dir.y = player.GetAxisRaw("Vertical") > 0 ? 1 : -1;
                    parent.transform.Rotate(new Vector3(0, 0, 1) * rotateSpeed * Time.deltaTime * dir.y, Space.Self);
                    H = false;
                }
                if (dir != Vector2.zero) {
                    //CheckIfALapIsDone(rotationOnLeavingGround, dir, lastDir, H);

                    if ((dir.x != 0 && lastDir.y != 0) || (dir.y != 0 && lastDir.x != 0)) {
                        //parent.transform.localEulerAngles
                        RotateTo(rotationOnLeavingGround, 0.2f);
                    }

                    CheckIfALapIsDone(dir);
                }

                UpdateRotation();
            }
        }

        private void UpdateRotation() {
            if (goToRotation != null) {
                Debug.Log(startRotation + " .. " + goToRotation.Value + " .. " + Vector3.MoveTowards(startRotation, goToRotation, goToRotationSteps));
                parent.transform.localEulerAngles = Vector3.MoveTowards(startRotation, goToRotation, goToRotationSteps);
                goToRotationSteps += Time.deltaTime * rotateSpeed * 3f;
                if (parent.transform.localEulerAngles == goToRotation) {
                    goToRotation = null;
                    goToRotationSteps = 0f;
                }
            }
        }

        private void RotateTo(Vector3 angle, float time) {
            goToRotation = ChangeAngleInterval(angle);
            startRotation = ChangeAngleInterval(parent.transform.localEulerAngles);
            goToRotationTime = time;
            //StartCoroutine(Rotate(angle, time));
            //Debug.Log(" / " + goToRotation.Value + " .. " + angle + ".. " + startRotation);
        }

        //private IEnumerator Rotate(Vector3 angle, float time) {
        //    for (int i = 0; i < time; i++) {

        //    }
        //}

        private void CheckIfALapIsDone(Vector3 OriginalRot, Vector2 dir, Vector2 _lastdir, bool _newAxis) {

            if (dir == _lastdir && _newAxis == this.newAxis) {

                int DiffX = CalculateDiffToInt(parent.transform.localEulerAngles.z, OriginalRot.z, 360);
                int DiffY = CalculateDiffToInt(parent.transform.localEulerAngles.y, OriginalRot.y, 360);

                int Diff = DiffX + DiffY;
                Debug.Log("X : " + DiffX + " . Y : " + DiffY + " . Tot : " + Diff);

                if (Diff == 0 && startRot) {
                    nbOfLaps++;
                    UpdateComboMeter(comboMeter);
                    startRot = false;
                }

                if (Mathf.Abs(Diff) == 1 && !startRot) {
                    startRot = true;
                }

            } else {
                rotationOnLeavingGround = parent.transform.localEulerAngles;
                lastDir = dir;
                startRot = false;
                this.newAxis = _newAxis;
            }
        }

        private void CheckIfALapIsDone(Vector2 dir) {
            if (dir != lastDir) {
                if (dir.x != lastDir.x) {
                    stuntRotation.x = 0;
                }

                if (dir.y != lastDir.y) {
                    stuntRotation.y = 0;
                }
            }

            Vector2 actualRot = new Vector2(parent.transform.localEulerAngles.z, parent.transform.localEulerAngles.y);
            float diffX = Mathf.DeltaAngle(lastRot.x, actualRot.x);
            float diffY = Mathf.DeltaAngle(lastRot.y, actualRot.y);

            stuntRotation.x += diffX;
            stuntRotation.y += diffY;

            if (Mathf.Abs(stuntRotation.x) / 360f >= 1) {
                nbOfLaps += Mathf.FloorToInt(Mathf.Abs(stuntRotation.x) / 360f);
                stuntRotation.x %= 360f;
                UpdateComboMeter(comboMeter);
            }

            if (Mathf.Abs(stuntRotation.y) / 360f >= 1) {
                nbOfLaps += Mathf.FloorToInt(Mathf.Abs(stuntRotation.y) / 360f);
                stuntRotation.y %= 360f;
                UpdateComboMeter(comboMeter);
            }

            //Debug.Log("AccumRot : " + stuntRotation + " . rotDiff : " + diffX + " . " + diffY);

            lastRot = new Vector2(actualRot.x, actualRot.y);
            lastDir = new Vector2(dir.x, dir.y);
        }

        private void UpdateComboMeter(TextMeshProUGUI text) {
            text.text = "x" + nbOfLaps;
        }
        private int CalculateDiffToInt(float rotationA, float rotationB, float maxAngle) {
            float DiffY = Mathf.DeltaAngle(ChangeAngleInterval(rotationA), rotationB + maxAngle);
            return Mathf.RoundToInt(DiffY);
        }

        private void FixedUpdate() {
            //Forward Acceleration
            if (isGrounded()) {

                if (!drifting)
                    sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);
                else
                    sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);

                //Gravity
                //  sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

                //Steering
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);


                //RaycastHit hitOn;
                RaycastHit hitNear;

                //Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitOn, 1.1f, layerMask);
                Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitNear, 2.0f, layerMask);

                //Normal Rotation
                kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
                kartNormal.Rotate(0, transform.eulerAngles.y, 0);
            }
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

        public void SetCamera(int playerId, int maxPlayer) {
            playerCamera.rect = Tools.GetPlayerRect(playerId, maxPlayer);
        }

        public void ChangeTexture(Material mat) {
            body.GetComponent<MeshRenderer>().material = mat;
        }

        public void Boost() {
            drifting = false;

            if (driftMode > 0) {
                DOVirtual.Float(currentSpeed * 3, currentSpeed, .3f * driftMode, Speed);
            }

            driftMode = 0;
            kartModel.parent.DOLocalRotate(Vector3.zero, .5f).SetEase(Ease.OutBack);
        }

        public void Steer(int direction, float amount) {
            rotate = (steering * direction) * amount;
        }

        private void Speed(float x) {
            currentSpeed = x;
        }

        float ChangeAngleInterval(float angle) {
            if (angle > 180f) {
                angle = angle - 360f;
                return angle;
            } else {
                return angle;
            }
        }

        Vector3 ChangeAngleInterval(Vector3 angle) {
            return new Vector3(ChangeAngleInterval(angle.x), ChangeAngleInterval(angle.y), ChangeAngleInterval(angle.z));
        }

        bool isGrounded() {
            RaycastHit hitFloor;
            if (Physics.Raycast(transform.position + (transform.up * 0.2f), Vector3.down, out hitFloor, 2.0f, layerMask)) {
                nbOfLaps = 0;
                UpdateComboMeter(comboMeter);
                rotationOnLeavingGround = parent.transform.localEulerAngles;
                return true;
            } else {
                return false;
            }
        }

    }


}