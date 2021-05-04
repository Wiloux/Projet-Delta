using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionCheckpointReached : ActionGameObject {
        public RaceManager raceManager;
        public int checkpointIndex;

        protected override void OnStart() { }

        protected override void OnExecute() {
            Character character = entity.GetComponent<Character>();
            if (character == null) { return; }

            raceManager.CheckpointReached(raceManager.CharacterId(character), checkpointIndex);
        }

        public override bool IsActionEnded() {
            return true;
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
            this.raceManager = race;
            this.checkpointIndex = checkpointIndex;
        }

        #endregion
    }
}