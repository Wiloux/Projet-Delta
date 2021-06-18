using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionSetActiveColliders : Action {
        public GameObject colliderObject;
        public bool active = false;

        protected override void OnStart() {  }

        protected override void OnExecute() {
            Collider[] colliders = gameObject.GetComponents<Collider>();
            if(colliders == null || colliders.Length <= 0) { return; }

            for (int i = 0; i < colliders.Length; i++) {
                colliders[i].enabled = active;
            }
        }

        public override bool IsActionEnded() {
            return true;
        }

        public override void OnResetAction() {

        }
    }
}

