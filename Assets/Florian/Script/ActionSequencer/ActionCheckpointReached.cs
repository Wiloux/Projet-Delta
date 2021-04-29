using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionCheckpointReached : ActionGameObject {
        public RaceManager raceManager;
        public int checkpointIndex;

        protected override void OnStart() { }

        protected override void OnExecute() {
            raceManager.CheckpointReached(entity.GetComponent<Character>(), checkpointIndex);
        }

        public override bool IsActionEnded() {
            return true;
        }
    }
}