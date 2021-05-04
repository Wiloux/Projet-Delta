using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Florian.ActionSequencer {
    public abstract class Action : MonoBehaviour {
        public enum WaitType { NONE, TIME, ACTION_END, BOTH }

        public WaitType waitType = WaitType.NONE;
        protected bool timeWaited = false;
        protected bool actionEnded = false;
        public float timeToWait = 0f;

        public virtual void Start() {
            OnStart();
        }

        protected abstract void OnStart();

        public bool IsFinished() {
            switch (waitType) {
                case WaitType.NONE:
                    return true;
                case WaitType.TIME:
                    return timeWaited;
                case WaitType.ACTION_END:
                    return IsActionEnded();
                case WaitType.BOTH:
                    return (IsActionEnded() && timeWaited);
                default:
                    Debug.LogError("Unknown WaitType");
                    return true;
            }
        }

        protected IEnumerator Wait(float time) {
            yield return new WaitForSeconds(time);
            timeWaited = true;
        }

        public virtual void Execute() {
            if (waitType == WaitType.TIME || waitType == WaitType.BOTH) {
                StartCoroutine("Wait", timeToWait);
            }
            OnExecute();
        }

        protected abstract void OnExecute();

        public abstract bool IsActionEnded();

        public virtual void ResetAction() {
            actionEnded = false;
            timeWaited = false;
            StopAllCoroutines();
            OnResetAction();
        }

        public abstract void OnResetAction();

        public void SetAction(WaitType waitType, float timeToWait = 0f) {
            this.waitType = waitType;
            this.timeToWait = timeToWait;
        }
    }
}