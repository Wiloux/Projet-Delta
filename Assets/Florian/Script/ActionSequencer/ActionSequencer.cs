using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class Entity { }

    public class ActionSequencer : MonoBehaviour {
        [HideInInspector] public Action[] actions;
        public bool launch = false;
        public bool oneTimeUse = false;

        private int actionIndex = -1;
        private bool running = false;

        [HideInInspector] public GameObject triggeringEntity = null;

        #region Properties

        public bool IsRunning {
            get { return running; }
        }

        public bool HasActions {
            get { return actions.Length > 0; }
        }

        #endregion

        #region Unity callbacks

        void Awake() {
            actions = GetComponentsInChildren<Action>();
        }

        void Update() {
            if (launch && HasActions) {
                UpdateAction();
            }
        }

        #endregion

        void UpdateAction() {
            running = true;

            if (actionIndex == -1) {
                actionIndex++;
                ExecuteAction(actions[actionIndex]);
            }

            while (!(actionIndex == actions.Length - 1) && actions[actionIndex].IsFinished()) {
                actionIndex++;
                ExecuteAction(actions[actionIndex]);
            }

            if (actions[actions.Length - 1].IsFinished()) {
                ResetActions();
                if (oneTimeUse) {
                    gameObject.SetActive(false);
                }
            }
        }

        public void ResetActions() {
            if (HasActions) {
                for (int i = 0; i < actions.Length; i++) {
                    actions[i].ResetAction();
                }
            }

            actionIndex = -1;
            running = false;
            launch = false;
        }

        public void Launch(GameObject entity = null) {
            launch = true;

            if (entity != null) { triggeringEntity = entity; }
        }

        private void ExecuteAction(Action action) {
            if (action is ActionGameObject actionGameObject) {
                actionGameObject.Execute(triggeringEntity);
            } else {
                action.Execute();
            }
        }
    }
}