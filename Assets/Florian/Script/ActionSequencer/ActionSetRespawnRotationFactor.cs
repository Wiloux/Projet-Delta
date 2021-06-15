using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionSetRespawnRotationFactor : ActionGameObject {
        public float factor;

        protected override void OnStart() { }

        protected override void OnExecute() {
            Movement character = entity.GetComponent<Movement>();
            if (character == null) { return; }

            character.respawnOrientationFactor = factor;
        }

        public override bool IsActionEnded() {
            return true;
        }
    }
}