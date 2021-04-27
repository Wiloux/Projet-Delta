using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Florian {
    public class GameManager : MonoBehaviour {
        const int MAX_PLAYER = 4;

        public static GameManager instance;

        public Transform multiplayerPanel = null;
        private Controller[] playersController = null;
        public List<MountKartingController> players = null;

        public GameObject kart = null;

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

        #endregion

        private void Awake() {
            instance = this;
        }

        void Start() {
            ShowMultPanel(true);

            playersController = new Controller[MAX_PLAYER];
        }

        void Update() {
            UpdateMultPanel();
        }

        private void ShowMultPanel(bool state = true) {
            multiplayerPanel.gameObject.SetActive(state);
        }

        private void UpdateMultPanel() {
            Controller controller = ReInput.controllers.GetLastActiveController();
            if (controller == ReInput.controllers.Keyboard || controller == ReInput.controllers.Mouse) { return; }

            if (AddController(controller)) {
                for (int i = 0; i < playersController.Length; i++) {
                    if (playersController[i] != null) {
                        multiplayerPanel.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        multiplayerPanel.GetChild(i).GetChild(1).gameObject.SetActive(true);
                    }
                }

                if (playersController[playersController.Length - 1] != null || NumberOfPlayerControllers == ReInput.controllers.controllerCount - 2) {
                    for (int i = 0; i < NumberOfPlayerControllers; i++) {
                        GameObject lastSpawn = SpawnKart(Vector3.zero);
                        lastSpawn.SetActive(true);
                        lastSpawn.GetComponentInChildren<MountKartingController>().SetController("P" + (i + 1), playersController[i]);
                        Debug.Log("P" + (i + 1));
                    }
                    ShowMultPanel(false);
                }
            }
        }

        private bool AddController(Controller controller) {
            for (int i = 0; i < playersController.Length; i++) {
                if (controller == playersController[i]) {
                    return false;
                } else if (playersController[i] == null) {
                    playersController[i] = controller;
                    return true;
                }
            }
            return false;
        }

        private GameObject SpawnKart(Vector3 pos) {
            return Instantiate(kart, pos, Quaternion.identity);
        }
    }
}