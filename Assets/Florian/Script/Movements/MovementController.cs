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
        public CameraController cameraController;
        private VFXManager vfx;
        private FlanksAttack flanksAttack;
        private JumpingSheep jumpingSheep;
        private Gliding gliding;
        private Shark sharkAttack;
        private bool sharkSide;
        private Fear fear;

        [Header("Bodys")]
        public Transform model;
        public Transform body;
        private Vector3 baseScale = Vector3.zero;

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
        public float weightDistributionSpeed = 3f;
        private Vector2 weightAxis = Vector2.zero;
        private Vector2 targetWeightAxis = Vector2.zero;

        [Header("Rebellion")]
        [SerializeField] private float forwardRebellionTime = 10f;
        [SerializeField] private float rebellionTime = 2f;
        private float rebellionHorizontalDirection = 0f;
        private int rebellionStacks = 0;
        private float emptyRebellionTimer = 0f;
        private float forwardTimer = 0f;

        #region Properties

        #region Getters

        public float Speed {
            get { return physics.Speed; }
        }

        public float MaxSpeed {
            get { return physics.MaxSpeed; }
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

        public bool Rebelling {
            get { return rebellionHorizontalDirection != 0f; }
        }

        #endregion

        public int Placement {
            set { placementText.text = value.ToString(); }
        }

        public int Laps {
            set { lapsText.text = value.ToString() + "/" + maxLaps; }
        }

        public bool CanMove {
            get { return !(physics.stun || lockMovements); }
        }

        #endregion

        #region Unity Callbacks

        void Start() {
            if (physics == null) {
                physics = GetComponent<Movement>();
            }

            physics.OnStun += OnStun;
            physics.OnFallingDeath += OnFallingDeath;
            physics.OnRespawn += OnRespawn;

            vfx = GetComponent<VFXManager>();

            flanksAttack = GetComponent<FlanksAttack>();
            if (flanksAttack == null) { Debug.LogError("Flank attack not found on controller object"); }

            jumpingSheep = GetComponent<JumpingSheep>();
            if (jumpingSheep == null) { Debug.LogError("jumping Sheep not found on controller object"); }

            gliding = GetComponent<Gliding>();
            if (gliding == null) { Debug.LogError("mount Throwing not found on controller object"); }

            sharkAttack = GetComponent<Shark>();
            if (gliding == null) { Debug.LogError("shark not found on controller object"); }

            fear = GetComponent<Fear>();
            if (fear == null) { Debug.LogError("fear not found on controller object"); }

            baseScale = model.localScale;
        }

        void Update() {
            float horizontalDirection = 0f;
            bool emptyRebellion = true;

            if (lockMovements) {
                if (physics.Speed > 0f)
                    physics.Decelerate();
                else
                    physics.Accelerate(Movement.AccelerationType.NONE);
            }

            if (CanMove && !Airborn) {
                // Acceleration
                if (Rebelling) {
                    physics.Accelerate(Movement.AccelerationType.FORWARD, 1f);
                } else {
                    if (!Decelerating && player.GetButtonDown("Accelerate")) {
                        physics.Accelerate(Movement.AccelerationType.WHIP);
                        AddRebellion();
                        emptyRebellion = false;
                    } else if (weightAxis.y > 0.7f) {
                        physics.Accelerate(Movement.AccelerationType.FORWARD, 1f);
                        forwardTimer += Time.deltaTime;
                        emptyRebellion = false;
                        if (forwardTimer >= forwardRebellionTime) {
                            forwardTimer = 0f;
                            AddRebellion();
                        }
                    } else {
                        physics.Accelerate(Movement.AccelerationType.BASE);
                        if (forwardTimer > 0f)
                            forwardTimer -= Time.deltaTime;
                    }
                }

                // Deceleration
                if (weightAxis.y < -0.7f) {
                    physics.Decelerate();
                    riderAnim.SetFloat("Vertical", player.GetAxis("Vertical"));
                }
            }

            // Mount throw controls
            //if (gliding != null && gliding._isThrowing) {
            //    if (player.GetAxis("Horizontal") != 0)
            //        horizontalDirection += player.GetAxis("Horizontal");
            //}

            if (CanMove) {
                // Flank Attack
                if (sharkAttack == null) {
                    if (player.GetAxisRaw("Attack") != 0f && flanksAttack._timer <= 0f) {
                        flanksAttack.Push(Mathf.Sign(player.GetAxisRaw("Attack")));
                        flanksAttack._timer = flanksAttack._cooldown;
                        if (player.GetAxisRaw("Attack") > 0f)
                            riderAnim.SetTrigger("attackD");
                        else if (player.GetAxisRaw("Attack") < 0f)
                            riderAnim.SetTrigger("attackG");
                    }
                    // Shark Attack
                } else {
                    if (player.GetAxisRaw("Attack") != 0f && sharkAttack._timer <= 0f) {
                        sharkAttack.pressTimer += Time.deltaTime;
                        if (player.GetAxisRaw("Attack") > 0f) {
                            sharkSide = true;
                            riderAnim.SetBool("chargingAttackD", true);
                        } else {
                            sharkSide = false;
                            riderAnim.SetBool("chargingAttackG", true);
                        }
                    } else if (player.GetAxisRaw("Attack") == 0f && sharkAttack.pressTimer != 0.0f) {
                        if (sharkSide)
                            sharkAttack.ComputeAttack(1);
                        else
                            sharkAttack.ComputeAttack(-1);

                        sharkAttack.pressTimer = 0.0f;
                        sharkAttack._timer = sharkAttack._cooldown;


                        if (sharkSide) {
                            riderAnim.SetBool("chargingAttackD", false);
                        } else if (!sharkSide) {
                            riderAnim.SetBool("chargingAttackG", false);
                        }
                    }
                }

                // Mount throw
                if (gliding != null) {
                    if (player.GetButton("Action") && Airborn) {

                        gliding.isGliding = true;
                    } else
                        gliding.isGliding = false;
                }

                if (player.GetButtonDown("Jump")) {
                    // Jump sheep
                    if (jumpingSheep != null && jumpingSheep._nbrStomp > 0) {
                        if (!Airborn && jumpingSheep._nbrStomp != 0)
                            jumpingSheep.MegaJump();
                        else if (Airborn && jumpingSheep._nbrStomp >= 2)
                            jumpingSheep.Stomp();
                        // Jump
                    } else if (!Airborn) {
                        physics.Jump();
                    }
                }

                // Fear
                if (fear != null) {
                    fear.AbilityUpdate();
                }

                if (player.GetButtonDown("Action")) {
                    if (fear != null && fear.castable) {
                        fear.Activate();
                    }
                }
            }

            // Tourner
            targetWeightAxis.Set(player.GetAxis("Horizontal"), player.GetAxis("Vertical"));
            float weightX = Mathf.MoveTowards(weightAxis.x, targetWeightAxis.x, weightDistributionSpeed * Time.deltaTime);
            float weightY = Mathf.MoveTowards(weightAxis.y, targetWeightAxis.y, weightDistributionSpeed * Time.deltaTime);
            weightAxis.Set(weightX, weightY);

            //if (CanMove && (weightAxis.x == -1 || weightAxis.x == 1))
            if (CanMove && !weightAxis.x.IsInside(-0.7f, 0.7f)) {
                if (Rebelling) {
                    physics.SetHorizontalDirection(Mathf.Clamp(weightAxis.x + rebellionHorizontalDirection, -1f, 1f));
                } else {
                    physics.SetHorizontalDirection(weightAxis.x);
                }
            } else {
                physics.SetHorizontalDirection(0f + rebellionHorizontalDirection);
            }

            riderAnim.SetFloat("Horizontal", weightAxis.x);
            riderAnim.SetFloat("Vertical", weightAxis.y);

            // Rebellion
            if (emptyRebellion) {
                if (rebellionStacks > 0) {
                    emptyRebellionTimer += Time.deltaTime;
                    if (emptyRebellionTimer > forwardRebellionTime) {
                        rebellionStacks--;
                    }
                }
            } else {
                emptyRebellionTimer = 0f;
            }

            Quaternion slopeRotation = physics.SlopeTilt();
            if (slopeRotation != Quaternion.identity) {
                model.transform.rotation = Quaternion.Slerp(model.transform.rotation, slopeRotation, 3f * Time.deltaTime);
            } else {
                model.transform.localEulerAngles = Vector3.zero;
            }

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

        public void OnStun(float time) {
            vfx.Stunned(time);
            riderAnim.SetTrigger("stunned");
            Unstunned(false);
            StartCoroutine(Tools.Delay(Unstunned, true, time));
        }

        private void Unstunned(bool value) {
            riderAnim.SetBool("unstunned", value);
        }

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

        #region Rebellion

        private void AddRebellion() {
            rebellionStacks++;

            int probability = 0;
            switch (rebellionStacks) {
                case 1:
                    probability = 15;
                    break;
                case 2:
                    probability = 30;
                    break;
                case 3:
                    probability = 50;
                    break;
                case 4:
                    probability = 100;
                    break;
            }
            int random = UnityEngine.Random.Range(0, 100);

            if (random < probability) {
                Rebellion();
            }
        }

        private void Rebellion() {
            //Debug.Log("REBELLION");
            //rebellionHorizontalDirection = UnityEngine.Random.Range(-0.5f, 0.5f);
            rebellionHorizontalDirection = Tools.RandomFloat(-0.5f, 0.5f);
            rebellionStacks = 0;
            StartCoroutine(Tools.Delay(Unrebellion, rebellionTime));
        }

        private void Unrebellion() {
            rebellionHorizontalDirection = 0f;
        }

        #endregion

        private void OnFallingDeath(float time) {
            animalAnim.SetTrigger("Falling_death");
            cameraController.followPlayer = false;
            //StartCoroutine(WaitFor(ScaleTo(Vector3.one * 5f, time * 0.4f), ScaleTo(Vector3.zero, time * 0.6f)));
        }

        private void OnRespawn() {
            cameraController.followPlayer = true;
            cameraController.ResetCamera();
            model.localScale = baseScale;
            physics.stun = false;
            Unstunned(true);
        }

        private IEnumerator ScaleTo(Vector3 scale, float time) {
            Vector3 baseScale = model.localScale;
            int framesNumber = Mathf.FloorToInt(60f * time);
            for (int i = 0; i < framesNumber; i++) {
                model.localScale = Vector3.Lerp(baseScale, scale, i / (float)framesNumber);
                yield return new WaitForSeconds(1f / 60f);
                Debug.Log($"// {i} + {time} + {framesNumber} + {1f / 60f}");
            }
        }

        private IEnumerator WaitFor(IEnumerator coroutine1, IEnumerator coroutine2) {
            yield return StartCoroutine(coroutine1);
            Debug.Log($"Started {coroutine1}");
            StartCoroutine(coroutine2);
        }

        //private void OnGUI() {
        //    GUILayout.Label(player.GetAxis("Horizontal").ToString());
        //    GUILayout.Label(player.GetAxis("Vertical").ToString());
        //}
    }
}
