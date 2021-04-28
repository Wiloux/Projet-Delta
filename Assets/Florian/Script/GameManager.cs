using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ToolsBoxEngine;
using Rewired;

namespace Florian {
    public class GameManager : MonoBehaviour {
        const int MAX_PLAYER = 4;

        public static GameManager instance;

        [Header("Rewired")]
        private List<int> assignedJoysticks = null;
        private Controller[] playersController = null;

        [Header("UIs")]
        public MultiplayerPanel multiplayerPanel = null;

        [Header("Players")]
        public GameObject kart = null;
        [HideInInspector] public List<MountKartingController> players = null;
        [SerializeField] private Material[] playersColor = null;

        [Header("Other")]
        public Camera mainCamera = null;
        [Range(2f, 20f)] public float switchCameraInterval = 10f;
        private int actualCameraId;

        #region Properties

        private int NumberOfPlayerControllers {
            get {
                int num = 0;
                for (int i = 0; i < playersController.Length; i++) {
                    if (playersController[i] != null) {
                        num++;
                    }
                }
                return num;
            }
        }

        private bool MultPanelActive {
            get { if (multiplayerPanel == null) { return false; } return multiplayerPanel.gameObject.activeSelf; }
        }

        #endregion

        #region Unity callbacks

        private void Awake() {
            instance = this;

            assignedJoysticks = new List<int>();
            ReInput.ControllerConnectedEvent += OnControllerConnected;
        }

        void Start() {
            ShowMultPanel(true);

            playersController = new Controller[MAX_PLAYER];

            AssignAllJoysticksToSystemPlayer(true);
        }

        void Update() {
            if (MultPanelActive) {
                UpdateMultPanel();
            }
        }

        #endregion

        #region MultiplayerPanel

        private void ShowMultPanel(bool state = true) {
            if(multiplayerPanel == null) { return; }
            multiplayerPanel.gameObject.SetActive(state);
        }

        private void UpdateMultPanel() {
            if (multiplayerPanel == null) { return; }

            // P1 Start game
            if (playersController[0] != null && ReInput.players.GetPlayer("P1").GetButtonDown("Start game")) {
                Debug.Log("Started \\o/");
                BeginGame();
            }

            // Change Portraits
            for (int i = 0; i < playersController.Length; i++) {
                if (playersController[i] != null) {
                    Player player = ReInput.players.GetPlayer("P" + (i + 1));
                    if (player.GetButtonDown("Leave game")) {
                        RemoveController(i);
                        multiplayerPanel.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        multiplayerPanel.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
                    } else if (player.GetButtonDown("Left")) {
                        multiplayerPanel.ChangePortraitSprite(i, -1);
                    } else if (player.GetButtonDown("Right")) {
                        multiplayerPanel.ChangePortraitSprite(i, 1);
                    }
                }
            }

            // Join game
            Controller controller = null;

            if (ReInput.players.GetSystemPlayer().GetButtonDown("Join game")) {
                controller = ReInput.players.GetSystemPlayer().controllers.GetLastActiveController();
            }

            if (controller == null) { return; }

            if (AddController(controller)) {
                for (int i = 0; i < playersController.Length; i++) {
                    if (playersController[i] != null) {
                        multiplayerPanel.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        multiplayerPanel.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                        multiplayerPanel.ChangePortraitSprite(i, 0);
                    }
                }
            }
        }

        private void BeginGame() {
            mainCamera.gameObject.SetActive(false);
            int cameraNumber = 1;
            for (int i = 0; i < playersController.Length; i++) {
                if (playersController[i] != null) {
                    GameObject lastSpawned = SpawnKart(Vector3.zero, "P" + (i + 1));
                    players.Add(lastSpawned.GetComponentInChildren<MountKartingController>());
                    players[players.Count - 1].SetCamera(cameraNumber, NumberOfPlayerControllers);
                    players[players.Count - 1].ChangeTexture(playersColor[multiplayerPanel.playerScreens[i].indexPortrait]);
                    cameraNumber++;
                }
            }

            // 3 cam = 4 random
            if (NumberOfPlayerControllers == 3) {
                mainCamera.gameObject.SetActive(true);
                mainCamera.rect = Tools.GetPlayerRect(4, 4);
                SwitchToNextCamera(0);
                StartCoroutine(Delay(switchCameraInterval, SwitchToNextCamera));
            }

            ShowMultPanel(false);
        }

        #endregion

        #region Controllers

        private void OnControllerConnected(ControllerStatusChangedEventArgs args) {
            if (args.controllerType != ControllerType.Joystick) { return; }
            if (assignedJoysticks.Contains(args.controllerId)) { return; }

            ReInput.players.GetSystemPlayer().controllers.AddController(args.controllerType, args.controllerId, true);
        }

        private void AssignAllJoysticksToSystemPlayer(bool removeFromOtherPlayers) {
            IList<Joystick> joysticks = ReInput.controllers.Joysticks;
            for (int i = 0; i < ReInput.controllers.joystickCount; i++) {
                ReInput.players.GetSystemPlayer().controllers.AddController(joysticks[i], removeFromOtherPlayers);
            }
        }

        private bool AddController(Controller controller) {
            for (int i = 0; i < playersController.Length; i++) {
                if (controller == playersController[i]) {
                    return false;
                } else if (playersController[i] == null) {
                    playersController[i] = controller;

                    Player player = ReInput.players.GetPlayer("P" + (i + 1));
                    player.controllers.ClearAllControllers();
                    player.controllers.AddController(controller, true);

                    return true;
                }
            }
            return false;
        }

        private bool RemoveController(int index) {
            if (playersController[index] == null) { return false; }

            ReInput.players.GetSystemPlayer().controllers.AddController(playersController[index], true);
            playersController[index] = null;
            return true;
        }

        #endregion

        #region Menu

        public void ChangeScene(int sceneId) {
            SceneManager.LoadScene(sceneId);
        }

        public void Quit() {
            Application.Quit();
        }

        #endregion

        private GameObject SpawnKart(Vector3 pos) {
            GameObject insta = Instantiate(kart, pos, Quaternion.identity);
            insta.SetActive(true);
            return insta;
        }

        private GameObject SpawnKart(Vector3 pos, string playerName) {
            GameObject insta = SpawnKart(pos);
            insta.GetComponentInChildren<MountKartingController>().SetController(playerName);
            return insta;
        }

        private GameObject SpawnKart(Vector3 pos, string playerName, Controller controller) {
            GameObject insta = SpawnKart(pos);
            insta.GetComponentInChildren<MountKartingController>().SetController(playerName, controller);
            return insta;
        }

        private void SwitchToNextCamera() {
            actualCameraId++;
            SwitchToNextCamera(actualCameraId);
            StartCoroutine(Delay(switchCameraInterval, SwitchToNextCamera));
        }

        private void SwitchToNextCamera(int index) {
            actualCameraId = index;
            actualCameraId %= NumberOfPlayerControllers;
            mainCamera.transform.parent = players[actualCameraId].playerCamera.transform;
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        IEnumerator Delay(float time, Tools.BasicDelegate function) {
            yield return new WaitForSeconds(time);
            function();
        }
    }
}