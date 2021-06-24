using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Florian.ActionSequencer {
    public class ActionChangeMaterial : ActionGameObject {
        public Color colorTo = new Color(0,0,0,1);
        private bool ended = false;

        protected override void OnStart() { }

        protected override void OnExecute() {
            MeshRenderer entityMaterial = entity.GetComponent<MeshRenderer>();
            if (entityMaterial == null) { return; }

            StartCoroutine(LerpMat(5f, entityMaterial));
        }

        IEnumerator LerpMat(float time, MeshRenderer go)
        {
            Debug.Log(go.material.GetVector("_BaseColor"));


            float timepassed = 0;

            while(timepassed < time)
            {
                Vector4 color = Vector4.Lerp(go.material.color, colorTo, timepassed / time);
                go.material.SetVector("_BaseColor", color);
                timepassed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

        }

        public override bool IsActionEnded() {
            return ended;
        }

        public override void ResetAction() {
            base.ResetAction();
        }
    }
}
