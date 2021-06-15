using System;
using UnityEngine;

namespace Florian.ActionSequencer {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [ExecuteInEditMode]
    public class AreaTrigger : MonoBehaviour {
        public enum TriggerType { ON_ENTER, ON_EXIT }
        public enum TriggerEntity { EVERYTHING, PLAYERS }

        public TriggerEntity triggerEntity;

        private ActionSequencer enterActions;
        private ActionSequencer exitActions;

        public bool alwaysExitActions = false;

        public Color gizmosColor = Color.red;

        private Rigidbody rigidBody;
        private BoxCollider[] boxCollider;

        private void Awake() {
            rigidBody = GetComponent<Rigidbody>();
            boxCollider = GetComponents<BoxCollider>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
            for (int i = 0; i < boxCollider.Length; i++) {
                boxCollider[i].isTrigger = true;
            }

            foreach (ActionSequencer sequencer in GetComponentsInChildren<ActionSequencer>()) {
                if (sequencer.name == "OnEnter") {
                    enterActions = sequencer;
                } else if (sequencer.name == "OnExit") {
                    exitActions = sequencer;
                }
            }

            if (null == enterActions) {
                GameObject enterActionGo = new GameObject("OnEnter");
                enterActionGo.transform.SetParent(transform);
                enterActions = enterActionGo.AddComponent<ActionSequencer>();
            }

            if (null == exitActions) {
                GameObject exitActionGo = new GameObject("OnExit");
                exitActionGo.transform.SetParent(transform);
                exitActions = exitActionGo.AddComponent<ActionSequencer>();
            }

            if (gameObject.name.Equals("GameObject")) { gameObject.name = "AreaTrigger"; }
        }

        public void OnAreaEnter(GameObject entity = null) {
            enterActions.Launch(entity);
        }

        public void OnAreaExit(GameObject entity = null) {
            exitActions.Launch(entity);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject == null) return;
            if (triggerEntity == TriggerEntity.PLAYERS && other.GetComponent<Movement>() == null) return;

            OnAreaEnter(other.gameObject);
        }

        private void OnTriggerExit(Collider other) {
            if (other.gameObject == null) return;
            if (triggerEntity == TriggerEntity.PLAYERS && other.GetComponent<Movement>() == null) return;

            OnAreaExit(other.gameObject);
        }

        private void OnDrawGizmos() {
            Matrix4x4 baseMatrix = Gizmos.matrix;

            Color color = gizmosColor;
            Gizmos.color = color;
            Matrix4x4 matrix = Gizmos.matrix;
            matrix.SetTRS(transform.position, transform.localRotation, Vector3.one);
            Gizmos.matrix = matrix;
            for (int i = 0; i < boxCollider.Length; i++) {
                Gizmos.DrawCube(Vector3.zero + boxCollider[i].center, boxCollider[i].size);
            }

            Gizmos.matrix = baseMatrix;
        }

        public void SetCollider(Vector3 size) {
            for (int i = 0; i < boxCollider.Length; i++) {
                boxCollider[i].size = size;
            }
        }

        public T AddAction<T>(TriggerType triggerType) where T : Action {
            switch (triggerType) {
                case TriggerType.ON_ENTER:
                    return enterActions.AddAction<T>();
                case TriggerType.ON_EXIT:
                    return exitActions.AddAction<T>();
            }

            return null;
        }
    }
}