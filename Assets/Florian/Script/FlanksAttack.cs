using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian {
    public class FlanksAttack : MonoBehaviour {
        [SerializeField] private float _pushForce;
        [SerializeField] private Vector3 _throwBoxDimension;
        public float _cooldown;
        public float _timer;

        private void Update() {
            if (_timer > 0)
                _timer -= Time.deltaTime;
        }

        public void Push(float horizontal) {
            Colliders(horizontal);
        }

        private void Colliders(float horizontal) {
            Collider[] colliders = Physics.OverlapBox(transform.right * horizontal * 2f + transform.localPosition, _throwBoxDimension);

            foreach (Collider pushedObject in colliders) {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name) {
                    MovementController movementController = pushedObject.GetComponent<MovementController>();
                    movementController.physics.AddVelocity(Vector3.right * horizontal * _pushForce);
                    movementController.physics.TimedChange(ref movementController.physics.frictions.amplitude, "frictions.amplitude", movementController.physics.frictions.amplitude * 15f, 1f);
                    movementController.physics.TimedChange(ref movementController.physics.stun, "stun", true, 2.5f);
                }
            }

        }

        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.right * 2f + transform.localPosition, _throwBoxDimension);
            Gizmos.DrawCube(-transform.right * 2f + transform.localPosition, _throwBoxDimension);
        }*/
    }
}