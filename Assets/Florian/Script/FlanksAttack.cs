using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian {
    public class FlanksAttack : MonoBehaviour {
        private MovementController _movementController;

        [SerializeField] private float _pushForce;
        [SerializeField] private Vector3 _throwBoxDimension;
        public float _cooldown;
        public float _timer;

        private void Start() {
            _movementController = GetComponent<MovementController>();
        }

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
                MovementController hittedMvtController = pushedObject.GetComponent<MovementController>();
                if (hittedMvtController != null && hittedMvtController.playerName != _movementController.playerName) {
                    pushedObject.GetComponent<VFXManager>().AttackSkillFX(transform.right * horizontal * 2f + transform.localPosition, 2f);
                    pushedObject.GetComponent<VFXManager>().TextToon(transform.right * horizontal * 2f + transform.localPosition, 2f);
                    hittedMvtController.physics.AddVelocity((transform.position - pushedObject.transform.position).normalized.Redirect(Vector3.forward, pushedObject.transform.forward) * -1f * _pushForce);
                    hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.frictions.amplitude, "frictions.amplitude", hittedMvtController.physics.frictions.amplitude * 15f, 1f);
                }
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.right * 2f + transform.localPosition, _throwBoxDimension);
            Gizmos.DrawCube(-transform.right * 2f + transform.localPosition, _throwBoxDimension);
        }
    }
}