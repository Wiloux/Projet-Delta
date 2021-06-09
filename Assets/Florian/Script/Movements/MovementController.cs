using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Florian;
using TMPro;
using ToolsBoxEngine;

namespace Florian {
    public class MovementController : Character {
        public Movement physics;
        private FlanksAttack flanksAttack;
        private JumpingSheep jumpingSheep;
        private MountThrowing mountThrowing;
        private Shark sharkAttack;
        private bool sharkSide;
        private Fear fear;

        [Header("Bodys")]
        public GameObject model;
        public Transform body;

        [Header("Rewired")]
        public Rewired.Player player;
        public string playerName;

        [Header("Anims")]
        public Animator riderAnim;
        public Animator animalAnim;

        [Header("Camera")]
        public Camera playerCamera;

        [Header("Laps")]
        public TextMeshProUGUI placementText = null;
        public TextMeshProUGUI lapsText = null;
        public int maxLaps = 2;

        [Header("Controls")]
        public bool lockMovements = false;

        #region Properties

        #region Getters

        public float Speed {
            get { return physics.Speed; }
        }

        public float MaxSpeed {
            get { return physics.maxSpeed; }
        }

        public bool IsMoving {
            get { return (physics.velocity.sqrMagnitude == 0); }
        }

        public bool Accelerating {
            get { return physics.Accelerating; }
        }

        public bool Decelerating {
            get { return physics.Decelerating; }
        }

        public bool Turning {
            get { return physics.Turning; }
        }

        public bool Airborn {
            get { return physics.airborn; }
        }

        #endregion

        public int Placement {
            set { placementText.text = value.ToString(); }
        }

        public int Laps {
            set { lapsText.text = value.ToString() + "/" + maxLaps; }
        }

        #endregion

        #region Unity Callbacks

        void Start() {
            if (physics == null) {
                physics = GetComponent<Movement>();
            }

            flanksAttack = GetComponent<FlanksAttack>();
            if (flanksAttack == null) { Debug.LogError("Flank attack not found on controller object"); }

            jumpingSheep = GetComponent<JumpingSheep>();
            if (jumpingSheep == null) { Debug.LogError("jumping Sheep not found on controller object"); }

            mountThrowing = GetComponent<MountThrowing>();
            if (mountThrowing == null) { Debug.LogError("mount Throwing not found on controller object"); }

            sharkAttack = GetComponent<Shark>();
            if (mountThrowing == null) { Debug.LogError("shark not found on controller object"); }

            fear = GetComponent<Fear>();
            if (fear == null) { Debug.LogError("fear not found on controller object"); }
        }

        void Update() {
            float horizontalDirection = 0f;

            if (mountThrowing != null && mountThrowing._isThrowing)
            {
                if (player.GetAxis("Horizontal") != 0)
                    horizontalDirection += player.GetAxis("Horizontal");
            }

            if (!lockMovements && !Airborn && !physics.stun) {
                bool resetDecelerateTimer = false;

                if (player.GetButtonDown("Cheat")) {
                    //physics.Accelerate();
                    //jumpingSheep.Stomp();
                }

                // Acceleration
                if (player.GetButtonDown("Accelerate")) {
                    physics.Accelerate(Movement.AccelerationType.WHIP);
                    resetDecelerateTimer = true;
                } else if (player.GetAxis("Vertical") == 1f) {
                    physics.Accelerate(Movement.AccelerationType.FORWARD, player.GetAxis("Vertical"));
                    riderAnim.SetFloat("Vertical", player.GetAxis("Vertical"));
                    resetDecelerateTimer = true;
                } else {
                    physics.Accelerate(Movement.AccelerationType.BASE);
                }

                //if (player.GetButton("Decelerate")) {
                //    physics.Decelerate();
                //}
                if(player.GetAxis("Vertical") == -1f) {
                    physics.Decelerate();
                    riderAnim.SetFloat("Vertical", player.GetAxis("Vertical"));
                }

                if (Mathf.Abs(player.GetAxis("Horizontal")) == 1f) {
                    horizontalDirection += player.GetAxis("Horizontal");
                    resetDecelerateTimer = false;
                }

                //if (player.GetButtonDown("Horizontal")) {
                //    resetDecelerateTimer = true;
                //}

                if (resetDecelerateTimer) {
                    //physics.ResetDecelerateTimer();
                }
            }

            if (!physics.stun && !lockMovements)
            {
                if (sharkAttack == null)
                {
                    if (player.GetAxisRaw("Attack") != 0f && flanksAttack._timer <= 0f)
                    {
                        flanksAttack.Push(Mathf.Sign(player.GetAxisRaw("Attack")));
                        flanksAttack._timer = flanksAttack._cooldown;
                        if (player.GetAxisRaw("Attack") > 0f)
                            riderAnim.SetTrigger("attackD");
                        else if (player.GetAxisRaw("Attack") < 0f)
                            riderAnim.SetTrigger("attackG");
                    }
                }
                else
                {
                    if (player.GetAxisRaw("Attack") != 0f && sharkAttack._timer <= 0f)
                    {
                        sharkAttack.pressTimer += Time.deltaTime;
                        if (player.GetAxisRaw("Attack") > 0f)
                        {
                            sharkSide = true;
                            riderAnim.SetBool("chargingAttackD", true);
                        }
                        else
                        {
                            sharkSide = false;
                            riderAnim.SetBool("chargingAttackG", true);
                        }
                    } else if (player.GetAxisRaw("Attack") == 0f && sharkAttack.pressTimer != 0.0f)
                    {
                        if (sharkSide)
                        sharkAttack.ComputeAttack(1);
                        else
                        sharkAttack.ComputeAttack(-1);

                        sharkAttack.pressTimer = 0.0f;
                        sharkAttack._timer = sharkAttack._cooldown;


                        if (sharkSide)
                        {
                            riderAnim.SetBool("chargingAttackD", false);
                        }
                        else if (!sharkSide)
                        {
                            riderAnim.SetBool("chargingAttackG", false);
                        }
                    }
                }

                if (mountThrowing != null) {
                    if (mountThrowing._isThrowing)
                        mountThrowing.MountThrowUpdate();
                    if (player.GetButtonDown("Action") && mountThrowing._timer == 0f)
                        mountThrowing.MountThrow();
                }

                if (player.GetButtonDown("Jump")) {
                    if (jumpingSheep != null && jumpingSheep._nbrStomp > 0) {
                        if (!Airborn && jumpingSheep._nbrStomp != 0)
                            jumpingSheep.MegaJump();
                        else if (Airborn && jumpingSheep._nbrStomp >= 2)
                            jumpingSheep.Stomp();
                    } else {
                        physics.Jump();
                    }
                }

                if (fear != null) {
                    fear.AbilityUpdate();
                }

                if (player.GetButtonDown("Action")) {
                    if (fear != null && fear.castable) {
                        fear.Activate();
                    }
                }
            }

            physics.SetHorizontalDirection(horizontalDirection);
            riderAnim.SetFloat("Horizontal", horizontalDirection);
            UpdateAnims();
        }

        private void FixedUpdate() {
            physics.UpdateMovements();
        }

        private void UpdateAnims() {
            if (IsMoving && !physics.Decelerating) {
                animalAnim.SetBool("isMoving", true);
                animalAnim.speed = Mathf.Lerp(0.75f, 1.5f, physics.Speed / physics.maxSpeed);
            } else {
                animalAnim.SetBool("isMoving", false);
                animalAnim.speed = 1f;
            }

            if (player.GetButton("Accelerate")) {
                if (player.GetButtonDown("Accelerate")) {
                    animalAnim.SetTrigger("whipped");
                    riderAnim.SetTrigger("whip");
                }
            }
            animalAnim.SetBool("stop", physics.Decelerating);
            animalAnim.SetFloat("velocity", physics.Speed / physics.maxSpeed);
        }

        #endregion

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
            MeshRenderer renderder = body.GetComponent<MeshRenderer>();
            if (renderder) {
                renderder.material = mat;
            }
        }

        #endregion
    }
}
