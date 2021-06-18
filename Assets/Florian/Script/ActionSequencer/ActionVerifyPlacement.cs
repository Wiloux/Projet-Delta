using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionVerifyPlacement : ActionGameObject {
        public enum MarkType {
            RELATIVE, ABSOLUTE
        }

        public enum Placement {
            FIRST, LAST, NOT_FIRST, NOT_LAST, NOT_FIRST_OR_LAST
        }

        public RaceManager raceManager;
        public MarkType markType = MarkType.RELATIVE;
        public int place = 1;
        public Placement placementType = Placement.FIRST;
        private bool ended = false;

        protected override void OnStart() {
            raceManager = Object.FindObjectOfType<RaceManager>();
            if (place <= 0) { place = 0; }
        }

        protected override void OnExecute() {
            Character character = entity.GetComponent<Character>();
            if (character == null) { return; }

            int characterId = raceManager.CharacterId(character);
            if (characterId == -1) { return; }

            int place = this.place;
            int placementsLength = raceManager.placements.Length;
            if (markType == MarkType.RELATIVE) {
                switch (placementType) {
                    case Placement.FIRST:
                        place = 1;
                        break;
                    case Placement.LAST:
                        place = placementsLength;
                        break;
                    case Placement.NOT_FIRST:
                        if (raceManager.placements[0] != characterId) {
                            ended = true;
                            return;
                        }
                        break;
                    case Placement.NOT_LAST:
                        if (raceManager.placements[placementsLength - 1] != characterId) {
                            ended = true;
                            return;
                        }
                        break;
                    case Placement.NOT_FIRST_OR_LAST:
                        if (raceManager.placements[0] != characterId && raceManager.placements[placementsLength - 1] != characterId) {
                            ended = true;
                            return;
                        }
                        break;
                }
            }
            
            if (place - 1 >= raceManager.placements.Length) { return; }
            if (raceManager.placements[place - 1] == characterId) {
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
