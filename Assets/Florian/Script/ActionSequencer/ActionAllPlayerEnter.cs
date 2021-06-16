using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionAllPlayerEnter : ActionGameObject {
        public RaceManager raceManager;
        private List<GameObject> playerList = new List<GameObject>();

        protected override void OnStart() {
            raceManager = Object.FindObjectOfType<RaceManager>();
        }

        protected override void OnExecute() {
            if (!playerList.Contains(entity)) {
                playerList.Add(entity);
            }
        }

        public override bool IsActionEnded() {
            if (playerList.Count >= raceManager.characters.Count) {
                return true;
            } else {
                return false;
            }
        }

        public override void ResetAction() {
            base.ResetAction();
            playerList.Clear();
        }

        #region Setters

        public void SetAction(WaitType waitType, TargetType targetType, RaceManager race, int checkpointIndex) {
            base.SetAction(waitType, 0f, targetType);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, float timeToWait, TargetType targetType, RaceManager race, int checkpointIndex) {
            base.SetAction(waitType, timeToWait, targetType);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, TargetType targetType, GameObject targetEntity, RaceManager race, int checkpointIndex) {
            base.SetAction(waitType, targetType, targetEntity);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, float timeToWait, TargetType targetType, GameObject targetEntity, RaceManager race, int checkpointIndex) {
            base.SetAction(waitType, timeToWait, targetType, targetEntity);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(RaceManager race, int checkpointIndex) {
            //this.raceManager = race;
            //this.checkpointIndex = checkpointIndex;
        }

        #endregion
    }
}
