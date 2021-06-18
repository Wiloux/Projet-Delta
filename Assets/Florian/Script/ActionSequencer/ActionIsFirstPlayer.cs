using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionIsFirstPlayer : ActionGameObject {
        public RaceManager raceManager;
        private bool ended = false;

        protected override void OnStart() {
            raceManager = Object.FindObjectOfType<RaceManager>();
        }

        protected override void OnExecute() {
            Character character = entity.GetComponent<Character>();
            if (character == null) { return; }

            int characterId = raceManager.CharacterId(character);
            if (characterId == -1) { return; }

            if (raceManager.placements[0] == characterId) {
                ended = true;
            }
        }

        public override bool IsActionEnded() {
            return ended;
        }

        public override void ResetAction() {
            base.ResetAction();
            ended = false;
        }
    }
}
