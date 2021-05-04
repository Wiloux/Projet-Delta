using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer {
    public abstract class ActionGameObject : Action {
        public enum TargetType { TARGET, TRIGGERING }

        protected GameObject entity = null;

        [SerializeField] private TargetType targetType = TargetType.TARGET;
        [SerializeField] private GameObject targetEntity;

        public virtual void Execute(GameObject entity) {
            switch (targetType) {
                case TargetType.TARGET:
                    this.entity = targetEntity;
                    break;
                case TargetType.TRIGGERING:
                    this.entity = entity;
                    break;
            }

            Execute();
        }

        public override void OnResetAction() {
            entity = null;
        }

        #region Setters

        public void SetAction(WaitType waitType, TargetType targetType, GameObject targetEntity = null) {
            base.SetAction(waitType, 0f);
            SetAction(targetType, targetEntity);
        }

        public void SetAction(WaitType waitType, float timeToWait, TargetType targetType, GameObject targetEntity = null) {
            base.SetAction(waitType, timeToWait);
            SetAction(targetType, targetEntity);
        }

        public void SetAction(TargetType targetType, GameObject targetEntity = null) {
            this.targetType = targetType;
            this.targetEntity = targetEntity;
        }

        #endregion
    }
}