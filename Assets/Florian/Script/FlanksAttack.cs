using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian {
    public class FlanksAttack : MonoBehaviour {
        private MovementController _movementController;

        public float _pushForce;
        public Vector3 _throwBoxDimension;
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
            Vector3 center = transform.right * horizontal * (_throwBoxDimension.x / 2f) + transform.position;
            Collider[] colliders = Physics.OverlapBox(center, _throwBoxDimension);

            foreach (Collider pushedObject in colliders) {
                MovementController hittedMvtController = pushedObject.GetComponent<MovementController>();
                if (hittedMvtController != null && hittedMvtController.playerName != _movementController.playerName) {
                    pushedObject.GetComponent<VFXManager>().AttackSkillFX(center, 2f);
                    pushedObject.GetComponent<VFXManager>().TextToon(center, 2f);
                    //hittedMvtController.physics.AddVelocity((transform.position - pushedObject.transform.position).normalized.Redirect(Vector3.forward, pushedObject.transform.forward) * -1f * _pushForce);
                    //hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.frictions.amplitude, "frictions.amplitude", hittedMvtController.physics.frictions.amplitude * 15f, 1f);
                    //hittedMvtController.physics.Bump((pushedObject.transform.position - transform.position).normalized, _pushForce, 0f);
                    hittedMvtController.physics.Bump(transform.right * horizontal, _pushForce, 0f);
                }
            }
        }

        private void OnDrawGizmos() {
            Color color = Color.blue;
            color.a = 0.2f;
            Gizmos.color = color;
            Gizmos.DrawCube(transform.right * (_throwBoxDimension.x / 2f) + transform.position, _throwBoxDimension);
            Gizmos.DrawCube(-transform.right * (_throwBoxDimension.x / 2f) + transform.position, _throwBoxDimension);
        }
    }
}