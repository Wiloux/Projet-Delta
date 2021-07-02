using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Florian;
using TMPro;
using System;
using ToolsBoxEngine;

namespace Florian
{
    public class MovementController : Character
    {
        public Movement physics;
        public CameraController cameraController;
        private VFXManager vfx;
        private FlanksAttack flanksAttack;
        private JumpingSheep jumpingSheep;
        private Gliding gliding;
        private Shark sharkAttack;
        private float sharkSide;
        private Fear fear;
        private PlayerTrigger playerTrigger = null;

        [Header("Bodys")]
        public Transform model;
        public Transform body;
        public SkinnedMeshRenderer bodyMount;
        public SkinnedMeshRenderer bodyDomi;
        public SkinnedMeshRenderer bodyLifeGuard;
        public List<Color> playerColors = new List<Color>();
        private Vector3 baseScale = Vector3.zero;
        private Coroutine scaleRoutine = null;

        [Header("Rewired")]
        public Rewired.Player player;
        public string playerName;

        [Header("Anims")]
        public Animator riderAnim;
        public Animator animalAnim;
        public Animator radiantAnim;
        [SerializeField] private float slopeRotateSpeed = 2f;

        [Header("Camera")]
        public Camera playerCamera;

        [Header("Laps")]
        public TextMeshProUGUI placementText = null;
        public TextMeshProUGUI lapsText = null;
        public int maxLaps = 2;

        [Header("Controls")]
        public bool lockMovements = false;
        public bool stopYou = false;
        public float weightDistributionSpeed = 3f;
        private Vector2 weightAxis = Vector2.zero;
        private Vector2 targetWeightAxis = Vector2.zero;

        [Header("Rebellion")]
        [SerializeField] private float forwardRebellionTime = 10f;
        [SerializeField] private float rebellionTime = 2f;
        private int rebellionSide = 0;
        private float rebellionHorizontalDirection = 0f;
        private int rebellionStacks = 0;
        private float emptyRebellionTimer = 0f;
        private float forwardTimer = 0f;

        public bool ghosted = false;

        public AudioSource AS;

        private Coroutine moveToRoutine;

        #region Properties

        #region Getters

        public float Speed
        {
            get { return physics.Speed; }
        }

        public float MaxSpeed
        {
            get { return physics.MaxSpeed; }
        }

        public bool IsMoving
        {
            get { return (physics.Speed != 0); }
        }

        public bool Accelerating
        {
            get { return physics.Accelerating; }
        }

        public bool Decelerating
        {
            get { return physics.Decelerating; }
        }

        public bool Turning
        {
            get { return physics.Turning; }
        }

        public bool Airborn
        {
            get { return physics.airborn; }
        }

        public bool Rebelling
        {
            get { return rebellionHorizontalDirection != 0f; }
        }

        #endregion

        public int Placement
        {
            set
            {
                switch (value)
                {

                    case 1:
                        placementText.text = value.ToString() + "st";
                        break;
                    case 2:
                        placementText.text = value.ToString() + "nd";
                        break;
                    case 3:
                        placementText.text = value.ToString() + "rd";
                        break;
                    case 4:
                        placementText.text = value.ToString() + "th";
                        break;
                }
            }


        }

        public int Laps
        {
            set { lapsText.text = "LAP  " + value.ToString() + "/" + maxLaps; }
        }

        public bool CanMove
        {
            get { return !(physics.stun || lockMovements || stopYou); }
        }

        private Animator AnimalAnim
        {
            get { return animalAnim; }
            set { animalAnim = value; radiantAnim = value; }
        }

        #endregion

        #region Unity Callbacks

        void Start()
        {

            AS = GetComponent<AudioSource>();

            if (physics == null)
            {
                physics = GetComponent<Movement>();
            }

            physics.OnStun += OnStun;
            physics.OnFallingDeath += OnFallingDeath;
            physics.OnRespawn += OnRespawn;
            physics.OnBump += OnBump;

            vfx = GetComponent<VFXManager>();

            flanksAttack = GetComponent<FlanksAttack>();

            jumpingSheep = GetComponent<JumpingSheep>();

            gliding = GetComponent<Gliding>();

            sharkAttack = GetComponent<Shark>();

            fear = GetComponent<Fear>();

            playerTrigger = GetComponent<PlayerTrigger>();

            baseScale = model.localScale;

            if (jumpingSheep == null)
            {
                Material[] materials = bodyLifeGuard.materials;
                materials[0].SetColor("_newColor", playerColors[Int32.Parse(playerName[1].ToString()) - 1]);
                bodyLifeGuard.materials = materials;
            }
            else
            {
                Debug.Log(playerColors[Int32.Parse(playerName[1].ToString()) -1]);
                Material[] materials = bodyDomi.materials;
                materials[2].SetColor("_newColor", playerColors[Int32.Parse(playerName[1].ToString()) - 1]);
                materials[3].SetColor("_newColor", playerColors[Int32.Parse(playerName[1].ToString()) - 1]);
                bodyDomi.materials = materials;
            }
            //StartCoroutine(ScaleTo(Vector3.one * 5f, 3f));
        }

        public GameObject EndUI;
        void Update()
        {
            float horizontalDirection = 0f;
            bool emptyRebellion = true;


           

            if (stopYou)
            {
                if(!EndUI.activeSelf)
                EndUI.SetActive(true);

                if (physics.Speed > 0f)
                    physics.Decelerate();
                else
                {
                    physics.Accelerate(Movement.AccelerationType.NONE);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }
            }
            else
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (CanMove && !Airborn)
            {
                // Acceleration
                if (Rebelling)
                {
                    physics.Accelerate(Movement.AccelerationType.FORWARD, 1f);
                }
                else
                {
                    if (!Decelerating && player.GetButtonDown("Accelerate"))
                    {
                        physics.Accelerate(Movement.AccelerationType.WHIP);
                        AddRebellion();
                        emptyRebellion = false;
                    }
                    else if (weightAxis.y > 0.7f)
                    {
                        physics.Accelerate(Movement.AccelerationType.FORWARD, 1f);
                        forwardTimer += Time.deltaTime;
                        emptyRebellion = false;
                        if (forwardTimer >= forwardRebellionTime)
                        {
                            forwardTimer = 0f;
                            AddRebellion();
                        }
                    }
                    else
                    {
                        physics.Accelerate(Movement.AccelerationType.BASE);
                        if (forwardTimer > 0f)
                            forwardTimer -= Time.deltaTime;
                    }
                }

                // Deceleration
                if (weightAxis.y < -0.7f)
                {
                    physics.Decelerate();
                    riderAnim.SetFloat("Vertical", player.GetAxis("Vertical"));
                    if (physics.Speed <= 0f && Rebelling)
                    {
                        Unrebellion();
                    }
                }
            }

            // Mount throw controls
            //if (gliding != null && gliding._isThrowing) {
            //    if (player.GetAxis("Horizontal") != 0)
            //        horizontalDirection += player.GetAxis("Horizontal");
            //}

            if (CanMove)
            {
                // Flank Attack
                if (sharkAttack == null)
                {
                    if (player.GetAxisRaw("Attack") != 0f && flanksAttack._timer <= 0f)
                    {
                        AudioManager.Instance.Play3DSourceSFX(ClipsContainer.Instance.AllClips[0], AS, 1f);
                        flanksAttack.Push(Mathf.Sign(player.GetAxisRaw("Attack")));
                        flanksAttack._timer = flanksAttack._cooldown;
                        if (player.GetAxisRaw("Attack") > 0f)
                            riderAnim.SetTrigger("attackD");
                        else if (player.GetAxisRaw("Attack") < 0f)
                            riderAnim.SetTrigger("attackG");
                    }
                    // Shark Attack
                }
                else
                {
                    if (player.GetAxisRaw("Attack") != 0f && sharkAttack._timer <= 0f)
                    {
                        if (!vfx.sharkChargeFX.isPlaying)
                            vfx.sharkChargeFX.Play();
                        sharkAttack.pressTimer += Time.deltaTime;
                        vfx.SharkFX(sharkAttack.pressTimer);
                        if (player.GetAxisRaw("Attack") > 0f)
                        {
                            riderAnim.SetBool("chargingAttackD", true);
                        }
                        else
                        {
                            riderAnim.SetBool("chargingAttackG", true);
                        }
                        sharkSide = player.GetAxisRaw("Attack");
                    }
                    else if (player.GetAxisRaw("Attack") == 0f && sharkAttack.pressTimer != 0f)
                    {
                        vfx.sharkChargeFX.Stop();
                        sharkAttack.ComputeAttack(sharkSide);
                        AudioManager.Instance.Play3DSourceSFX(ClipsContainer.Instance.AllClips[8], AS, 1f);

                        sharkAttack.pressTimer = 0f;
                        sharkAttack._timer = sharkAttack._cooldown;

                        if (sharkSide > 0f)
                        {
                            riderAnim.SetBool("chargingAttackD", false);
                        }
                        else if (sharkSide < 0f)
                        {
                            riderAnim.SetBool("chargingAttackG", false);
                        }
                    }
                }

                // Mount throw
                if (gliding != null)
                {

                    if (player.GetButtonDown("Action") && Airborn)
                    {

                        AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[18], 1f);
                    }
                    if (player.GetButton("Action") && Airborn)
                    {

                        if (gliding.timer >= 0)
                        {

                            riderAnim.SetTrigger("plane");
                            riderAnim.SetBool("planeBool", true);
                            vfx.GlidFX(true);
                        }
                        else
                        {
                            riderAnim.SetBool("planeBool", false);
                            vfx.GlidFX(false);
                        }
                        gliding.isGliding = true;
                    }
                    else
                    {
                        riderAnim.SetBool("planeBool", false);
                        vfx.GlidFX(false);
                        gliding.isGliding = false;
                    }


                    if (!Airborn)
                    {
                        vfx.GlidFX(false);
                        riderAnim.SetBool("planeBool", false);
                    }
                }

                if (player.GetButtonDown("Jump"))
                {
                    // Jump sheep
                    if (jumpingSheep != null && jumpingSheep._nbrStomp > 0)
                    {
                        if (!Airborn && jumpingSheep._nbrStomp != 0)
                        {
                            AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[5], 1f);
                            jumpingSheep.MegaJump();
                        }
                        else if (Airborn && jumpingSheep.stompTimer <= 0)
                        {
                            jumpingSheep.Stomp();
                            jumpingSheep.stompTimer = jumpingSheep.stompDuration;
                        }
                        // Jump
                    }
                    else if (!Airborn)
                    {
                        AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[5], 1f);
                        physics.Jump();
                    }
                }

                // Fear
                if (fear != null)
                {
                    fear.AbilityUpdate();
                }

                if (player.GetButtonDown("Action"))
                {
                    if (fear != null && fear.castable)
                    {
                        AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[4], 1f);
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
            if (CanMove && !weightAxis.x.IsInside(-0.7f, 0.7f))
            {
                if (Rebelling)
                {
                    physics.SetHorizontalDirection(Mathf.Clamp(weightAxis.x + rebellionHorizontalDirection, -1f, 1f));
                }
                else
                {
                    physics.SetHorizontalDirection(weightAxis.x);
                }
            }
            else if (CanMove)
            {
                physics.SetHorizontalDirection(0f + rebellionHorizontalDirection);
            }
            else
            {
                physics.SetHorizontalDirection(0f);
            }

            riderAnim.SetFloat("Horizontal", weightAxis.x);
            riderAnim.SetFloat("Vertical", weightAxis.y);

            // Rebellion
            if (emptyRebellion)
            {
                if (rebellionStacks > 0)
                {
                    emptyRebellionTimer += Time.deltaTime;
                    if (emptyRebellionTimer > forwardRebellionTime)
                    {
                        emptyRebellionTimer = 0f;
                        vfx.AngerUnstack(rebellionSide);
                        rebellionStacks--;
                        Material[] materials = bodyMount.materials;
                        materials[1].SetColor("_newColor", Color.Lerp(Color.black, Color.red, rebellionStacks / 4f));
                        bodyMount.materials = materials;
                    }
                }
            }
            else
            {
                emptyRebellionTimer = 0f;
            }

            Quaternion slopeRotation = physics.SlopeTilt();
            if (slopeRotation != Quaternion.identity)
            {
                model.rotation = Quaternion.Slerp(model.rotation, slopeRotation, slopeRotateSpeed * Time.deltaTime);
            }
            else
            {
                model.localEulerAngles = Vector3.zero;
            }

            vfx.TrailsFX();
            UpdateAnims();
        }

        private void FixedUpdate()
        {
            physics.UpdateMovements();
        }

        private void UpdateAnims()
        {
            if (IsMoving && !physics.Decelerating)
            {
                animalAnim.SetBool("isMoving", true);
                radiantAnim.SetBool("isMoving", true);
                animalAnim.speed = Mathf.Lerp(0.75f, 2f, physics.Speed / physics.MaxSpeed);
                radiantAnim.speed = Mathf.Lerp(0.75f, 2f, physics.Speed / physics.MaxSpeed);
            }
            else
            {
                animalAnim.SetBool("isMoving", false);
                radiantAnim.SetBool("isMoving", false);
                animalAnim.speed = 1f;
                radiantAnim.speed = 1f;
            }

            if (player.GetButton("Accelerate"))
            {
                if (player.GetButtonDown("Accelerate"))
                {
                    animalAnim.SetTrigger("whipped");
                    radiantAnim.SetTrigger("whipped");
                    if (triggerUnstunned)
                    {
                        riderAnim.SetTrigger("whip");
                    }
                }
            }
            animalAnim.SetBool("stop", physics.Decelerating);
            radiantAnim.SetBool("stop", physics.Decelerating);
            animalAnim.SetFloat("velocity", physics.Speed / physics.MaxSpeed);
            radiantAnim.SetFloat("velocity", physics.Speed / physics.MaxSpeed);
            animalAnim.SetFloat("Horizontal", physics.HorizontalDirection);
        }

        #endregion

        public Coroutine GoToDestination(Vector3 position, float speed, float range = 5f)
        {
            lockMovements = true;
            physics.NegateVelocity(Axis.X, Axis.Y, Axis.Z);

            if (moveToRoutine != null) { StopCoroutine(moveToRoutine); }
            moveToRoutine = StartCoroutine(MoveTo(position, speed, range));
            return moveToRoutine;
        }

        private IEnumerator MoveTo(Vector3 position, float speed, float range)
        {
            while ((position - transform.position).sqrMagnitude > range * range)
            {
                position = position.Override(transform.position.y, Axis.Y);
                Quaternion rotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);
                physics.velocity = Vector3.forward * speed;
                yield return new WaitForEndOfFrame();
            }
            physics.NegateVelocity(Axis.X, Axis.Y, Axis.Z);
        }

        public void StopDestinationRoutine()
        {
            StopCoroutine(moveToRoutine);
        }

        #region Setters

        public void OnStun(float time)
        {
            if (gliding != null)
            {
                AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[3], 1f);
            }
            else
            {
                AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[2], 1f);
            }
            vfx.Stunned(time);
            riderAnim.SetTrigger("stunned");
            Unstunned(false);
            StartCoroutine(Tools.Delay(Unstunned, true, time));
        }

        private void OnBump()
        {
            AddRebellion();
        }

        bool triggerUnstunned = false;

        private void Unstunned(bool value)
        {
            riderAnim.SetBool("unstunned", value);
            triggerUnstunned = value;
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

        public void SetCamera(int playerId, int maxPlayer)
        {
            playerCamera.rect = Tools.GetPlayerRect(playerId, maxPlayer);
        }

        public void ChangeTexture(Material mat)
        {
            MeshRenderer renderder = body.GetComponent<MeshRenderer>();
            if (renderder)
            {
                renderder.material = mat;
            }
        }

        #endregion

        #region Rebellion

        private void AddRebellion()
        {
            if (Rebelling) { return; }

            if (rebellionStacks == 0)
            {
                rebellionSide = (int)(Tools.RandomFloat(-1f, 1f));
            }

            rebellionStacks++;
            Material[] materials = bodyMount.materials;
            materials[1].SetColor("_newColor", Color.Lerp(Color.black, Color.red, rebellionStacks / 4f));
            bodyMount.materials = materials;

            vfx.AngerStack(rebellionSide);


            if (rebellionStacks >= 4)
            {
                Rebellion();
            }
        }

        private void Rebellion()
        {
            AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[13], 1f);
            vfx.AngerRadiant();
            //Debug.Log("REBELLION");
            //rebellionHorizontalDirection = UnityEngine.Random.Range(-0.5f, 0.5f);
            rebellionHorizontalDirection = 0.5f * rebellionSide;
            forwardTimer = 0f;
            emptyRebellionTimer = 0f;
            rebellionStacks = 0;
            StartCoroutine(Tools.Delay(Unrebellion, rebellionTime));
        }

        private void Unrebellion()
        {
            vfx.AngerClear(rebellionSide);
            rebellionSide = 0;
            rebellionHorizontalDirection = 0f;
            forwardTimer = 0f;
            emptyRebellionTimer = 0f;
            rebellionStacks = 0;
            Material[] materials = bodyMount.materials;
            materials[1].SetColor("_newColor", Color.Lerp(Color.black, Color.red, rebellionStacks / 4f));
            bodyMount.materials = materials;
        }

        #endregion

        private void OnFallingDeath(float time)
        {
            //animalAnim.SetTrigger("Falling_death");
            //cameraController.followPlayer = false;
            cameraController.cameraState = CameraController.CameraState.LOOKAT;

            if (scaleRoutine != null) { StopCoroutine(scaleRoutine); }
            scaleRoutine = StartCoroutine(ScaleTo(Vector3.zero, time * 0.5f));
        }

        private void OnRespawn()
        {
            if (scaleRoutine != null) { StopCoroutine(scaleRoutine); }
            model.localScale = baseScale;

            //cameraController.followPlayer = true;
            cameraController.cameraState = CameraController.CameraState.FOLLOW;
            cameraController.ResetCamera();
            physics.stun = false;

            if (jumpingSheep != null)
            {
                jumpingSheep.StopStomp();
            }

            Unrebellion();
            Unstunned(true);
        }

        private IEnumerator ScaleTo(Vector3 scale, float time)
        {
            Vector3 baseScale = model.localScale;
            int framesNumber = Mathf.FloorToInt(60f * time);
            for (int i = 0; i < framesNumber; i++)
            {
                model.localScale = Vector3.Lerp(baseScale, scale, i / (float)framesNumber);
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        private IEnumerator WaitFor<T>(IEnumerator coroutine1, Tools.BasicDelegate<T> function, T arg)
        {
            yield return StartCoroutine(coroutine1);
            Debug.Log($"Started {coroutine1}");
            function(arg);
        }

        //public void ResetPlayerTriggerList()
        //{
        //    playerTrigger.ResetList();
        //}

        //private void OnGUI() {
        //    GUILayout.Label(player.GetAxis("Horizontal").ToString());
        //    GUILayout.Label(player.GetAxis("Vertical").ToString());
        //}

        public void Ghost(bool state)
        {
            GetComponent<CapsuleCollider>().enabled = !state;
            ghosted = state;
        }
    }
}
