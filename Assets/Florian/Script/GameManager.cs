using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ToolsBoxEngine;
using Rewired;

namespace Florian
{
    public class GameManager : MonoBehaviour
    {
        const int MAX_PLAYER = 4;
        public static GameManager instance;

        [Header("Rewired")]
        private List<int> assignedJoysticks = null;
        private Controller[] playersController = null;

        [Header("Race")]
        public RaceManager race;

        [Header("UIs")]
        public MultiplayerPanel multiplayerPanel = null;

        [Header("Players")]
        public GameObject kart = null;
        [HideInInspector] public List<MovementController> players = null;
        public Material[] playersColor = null;
        //public UnityEditor.Animations.AnimatorController lifeGuardAnim;
        //public UnityEditor.Animations.AnimatorController DomiAnim;
        public Mesh lifeGuardMesh;

        [Header("Other")]
        public Camera mainCamera = null;
        [Range(2f, 20f)] public float switchCameraInterval = 10f;
        private int actualCameraId;
        public miniMap miniMap;
        public GameObject valueRefs;
        public GameObject pausePanel;

        #region Properties

        private int NumberOfPlayerControllers
        {
            get
            {
                int num = 0;
                for (int i = 0; i < playersController.Length; i++)
                {
                    if (playersController[i] != null)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        private bool MultPanelActive
        {
            get { if (multiplayerPanel == null) { return false; } return multiplayerPanel.gameObject.activeSelf; }
        }

        #endregion

        #region Unity callbacks

        private void Awake()
        {
            instance = this;

            assignedJoysticks = new List<int>();
            ReInput.ControllerConnectedEvent += OnControllerConnected;
        }

        void Start()
        {
            ShowMultPanel(true);

            playersController = new Controller[MAX_PLAYER];

            AssignAllJoysticksToSystemPlayer(true);
        }

        void Update()
        {
            if (MultPanelActive)
            {
                UpdateMultPanel();
            }
            for (int i = 0; i < playersController.Length; i++)
            {
                if (playersController[i] != null)
                {
                    if (race.raceStarted && !race.startCountdownUI.activeSelf && (ReInput.players.GetPlayer(i).GetButtonDown("Start game")))
                    {
                        PauseGame(pausePanel);
                    }
                }
            }
        }

        #endregion

        #region MultiplayerPanel

        private void ShowMultPanel(bool state = true)
        {
            if (multiplayerPanel == null) { return; }
            multiplayerPanel.gameObject.SetActive(state);
        }

        private void UpdateMultPanel()
        {
            if (multiplayerPanel == null) { return; }

            // P1 Start game
            if (playersController[0] != null && ReInput.players.GetPlayer("P1").GetButtonDown("Start game"))
            {
                BeginGame();
            }

            // Change Portraits
            for (int i = 0; i < playersController.Length; i++)
            {
                if (playersController[i] != null)
                {
                    Player player = ReInput.players.GetPlayer("P" + (i + 1));
                    if (player.GetButtonDown("Leave game"))
                    {
                        RemoveController(i);
                        multiplayerPanel.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.SetActive(true);
                        multiplayerPanel.transform.GetChild(1).GetChild(i).GetChild(1).gameObject.SetActive(false);
                    }
                    else if (player.GetButtonDown("Left"))
                    {
                        multiplayerPanel.ChangePortraitSprite(i, -1);
                    }
                    else if (player.GetButtonDown("Right"))
                    {
                        multiplayerPanel.ChangePortraitSprite(i, 1);
                    }
                }
            }

            // Join game
            Controller controller = null;

            if (ReInput.players.GetSystemPlayer().GetButtonDown("Join game"))
            {
                controller = ReInput.players.GetSystemPlayer().controllers.GetLastActiveController();
            }

            if (controller == null) { return; }

            if (AddController(controller))
            {
                for (int i = 0; i < playersController.Length; i++)
                {
                    if (playersController[i] != null)
                    {
                        multiplayerPanel.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.SetActive(false);
                        multiplayerPanel.transform.GetChild(1).GetChild(i).GetChild(1).gameObject.SetActive(true);
                        multiplayerPanel.ChangePortraitSprite(i, 0);
                    }
                }
            }
        }

        private void BeginGame()
        {
            mainCamera.gameObject.SetActive(false);
            miniMap.map.gameObject.SetActive(true);
            int cameraNumber = 1;
            List<CharacterInfo> miniMapInfo = new List<CharacterInfo>();

            for (int i = 0; i < playersController.Length; i++)
            {
                if (playersController[i] != null)
                {
                    GameObject lastSpawned = SpawnKart(Vector3.zero, "P" + (i + 1));
                    GameObject lastSpawnedChild = lastSpawned.transform.GetChild(0).gameObject;
                    players.Add(lastSpawned.GetComponentInChildren<MovementController>());
                    players[players.Count - 1].SetCamera(cameraNumber, NumberOfPlayerControllers);
                    players[players.Count - 1].ChangeTexture(playersColor[multiplayerPanel.playerScreens[i].indexPortrait]);
                    cameraNumber++;

                    switch (multiplayerPanel.playerScreens[i].indexPortrait)
                    {
                        case 0:
                            lastSpawnedChild.GetComponent<MovementController>().riderAnim = lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Domi_anims").GetComponent<Animator>();
                            Fear lastFear = lastSpawnedChild.AddComponent<Fear>();
                            Fear refFear = valueRefs.GetComponent<Fear>();
                            lastFear.sphereGrowthCurve = refFear.sphereGrowthCurve.Clone();
                            lastFear.slowAmount = refFear.slowAmount;
                            lastFear.coolDownDuration = refFear.coolDownDuration;
                            lastFear.castingTime = refFear.castingTime;
                            JumpingSheep lastJumping = lastSpawnedChild.AddComponent<JumpingSheep>();
                            JumpingSheep refJumping = valueRefs.GetComponent<JumpingSheep>();
                            lastJumping._megaJumpForce = refJumping._megaJumpForce;
                            lastJumping.cooldown = refJumping.cooldown;
                            lastJumping._stompRadius = refJumping._stompRadius;
                            lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Lifeguard_anims").gameObject.SetActive(false);
                            lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Domi_anims").gameObject.SetActive(true);
                            break;
                        case 1:
                            lastSpawnedChild.GetComponent<MovementController>().riderAnim = lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Lifeguard_anims").GetComponent<Animator>();
                            Gliding refGliding = valueRefs.GetComponent<Gliding>();
                            Gliding lastGliding = lastSpawnedChild.AddComponent<Gliding>();
                            lastGliding.timerDur = refGliding.timerDur;
                            lastGliding.divideAmount = refGliding.divideAmount;

                            Shark refShark = valueRefs.GetComponent<Shark>();
                            Shark lastShark = lastSpawnedChild.AddComponent<Shark>();
                            lastShark.poweredPushForce = refShark.poweredPushForce;
                            lastShark.poweredPushRadius = refShark.poweredPushRadius;
                            lastShark.pressTimer = refShark.pressTimer;
                            lastShark._cooldown = refShark._cooldown;
                            lastShark.stunTime = refShark.stunTime;
                            lastShark._timer = refShark._timer;

                            //Debug.Log(lastSpawnedChild.transform.Find("Anims_Mouton/Bip002/Anims_Rider"));
                            lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Domi_anims").gameObject.SetActive(false);
                            lastSpawnedChild.transform.Find("Model/Mounton_Anims/Bip002/Lifeguard_anims").gameObject.SetActive(true);

                            break;
                    }
                }
            }

            //Check amount of players and calculate map position
            for (int i = 0; i < playersController.Length; i++)
            {
                if (playersController[i] != null)
                {
                    miniMap.CheckDisplayMode(players.Count);

                }
            }

            for (int j = 0; j < players.Count; j++)
            {
                miniMap.AddPlayer(players[j].gameObject.transform, multiplayerPanel.portraitSprites[multiplayerPanel.playerScreens[j].indexPortrait]);
            }

            // 3 cam = 4 random
            if (NumberOfPlayerControllers == 3)
            {
                mainCamera.gameObject.SetActive(true);
                mainCamera.rect = Tools.GetPlayerRect(4, 4);
                SwitchToNextCamera(0);
                StartCoroutine(Delay(switchCameraInterval, SwitchToNextCamera));
            }

            ShowMultPanel(false);

            race.StartRace(players.ToArray());
        }

        #endregion

        #region Controllers

        private void OnControllerConnected(ControllerStatusChangedEventArgs args)
        {
            if (args.controllerType != ControllerType.Joystick) { return; }
            if (assignedJoysticks.Contains(args.controllerId)) { return; }

            ReInput.players.GetSystemPlayer().controllers.AddController(args.controllerType, args.controllerId, true);
        }

        private void AssignAllJoysticksToSystemPlayer(bool removeFromOtherPlayers)
        {
            IList<Joystick> joysticks = ReInput.controllers.Joysticks;
            for (int i = 0; i < ReInput.controllers.joystickCount; i++)
            {
                ReInput.players.GetSystemPlayer().controllers.AddController(joysticks[i], removeFromOtherPlayers);
            }
        }

        private bool AddController(Controller controller)
        {
            for (int i = 0; i < playersController.Length; i++)
            {
                if (controller == playersController[i])
                {
                    return false;
                }
                else if (playersController[i] == null)
                {
                    playersController[i] = controller;

                    Player player = ReInput.players.GetPlayer("P" + (i + 1));
                    player.controllers.ClearAllControllers();
                    player.controllers.AddController(controller, true);

                    return true;
                }
            }
            return false;
        }

        private bool RemoveController(int index)
        {
            if (playersController[index] == null) { return false; }

            ReInput.players.GetSystemPlayer().controllers.AddController(playersController[index], true);
            playersController[index] = null;
            return true;
        }

        #endregion

        #region Menu

        public void ChangeScene(int sceneId)
        {
            AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[0]);
            SceneManager.LoadScene(sceneId);
        }

        public void ShowNewPanel(GameObject panel)
        {
            if (panel)
            {
                AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[0]);
                panel.SetActive(!panel.activeSelf);
            }
        }

        public void Quit()
        {
            AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[0]);
            Application.Quit();
        }

        #endregion

        public void PauseGame(GameObject pausePanel)
        {
            if (pausePanel.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                pausePanel.SetActive(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                pausePanel.SetActive(true);
            }
        }

        private GameObject SpawnKart(Vector3 pos)
        {
            GameObject insta = Instantiate(kart, pos, Quaternion.identity);
            insta.SetActive(true);
            return insta;
        }

        private GameObject SpawnKart(Vector3 pos, string playerName)
        {
            GameObject insta = SpawnKart(pos);
            insta.name = playerName;
            insta.GetComponentInChildren<MovementController>().SetController(playerName);
            return insta;
        }

        private GameObject SpawnKart(Vector3 pos, string playerName, Controller controller)
        {
            GameObject insta = SpawnKart(pos);
            insta.name = playerName;
            insta.GetComponentInChildren<MovementController>().SetController(playerName, controller);
            return insta;
        }

        private void SwitchToNextCamera()
        {
            actualCameraId++;
            SwitchToNextCamera(actualCameraId);
            StartCoroutine(Delay(switchCameraInterval, SwitchToNextCamera));
        }

        private void SwitchToNextCamera(int index)
        {
            actualCameraId = index;
            actualCameraId %= NumberOfPlayerControllers;
            mainCamera.transform.parent = players[actualCameraId].playerCamera.transform;
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        IEnumerator Delay(float time, Tools.BasicDelegate function)
        {
            yield return new WaitForSeconds(time);
            function();
        }

        public void RestartTheGame()
        {
            AudioManager.Instance.PlayMusic(null);
            AudioManager.Instance.PlayAmbiant(null, 0.3f);

            SceneManager.LoadScene(0);
        }
    }
}