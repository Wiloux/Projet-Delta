using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Florian.ActionSequencer {
    public class ActionChangeMaterial : ActionGameObject {
        public Color colorTo = new Color(0, 0, 0, 1);
        public float transitionTime = 2f;
        private bool ended = false;

        protected override void OnStart() { }

        protected override void OnExecute() {
            MeshRenderer entityMaterial = entity.GetComponent<MeshRenderer>();
            if (entityMaterial == null) { return; }

            StartCoroutine(LerpMat(transitionTime, entityMaterial));
        }

        IEnumerator LerpMat(float time, MeshRenderer go) {
            Color baseColor = go.material.color;
            float timepassed = 0;

            while (timepassed < time) {
                Color color = Color.Lerp(baseColor, colorTo, timepassed / time);
                go.material.SetColor("_BaseColor", color);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            ended = true;
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
