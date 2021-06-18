using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public class ActionChangeMaterial : ActionGameObject {
        public Material material;

        protected override void OnStart() { }

        protected override void OnExecute() {
            MeshRenderer entityMaterial = entity.GetComponent<MeshRenderer>();
            if (entityMaterial == null) { return; }

            entityMaterial.material = material;
        }

        public override bool IsActionEnded() {
            return true;
        }

        public override void ResetAction() {
            base.ResetAction();
        }
    }
}
