using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Florian {
    public class Shark : MonoBehaviour {
        private MovementController _movementController;
        private FlanksAttack flanksAttack;

        //public float pushForce;
        //public float pushRadius;
        public float poweredPushForce = 2f;
        public float poweredPushRadius = 2f;
        public float stunTime = 1f;

        public float pressTimer;
        public float _cooldown;
        public float _timer;

        private void Start() {
            _movementController = GetComponent<MovementController>();
            flanksAttack = GetComponent<FlanksAttack>();
        }

        private void Update() {
            if (_timer > 0)
                _timer -= Time.deltaTime;
        }

        public void Push(float horizontal) {
            Colliders(horizontal, flanksAttack._throwBoxDimension, flanksAttack._pushForce, false);
        }

        private void Colliders(float horizontal, Vector3 _pushSize, float _pushForce, bool stun) {
            Vector3 center = transform.right * horizontal * (_pushSize.x / 2f) + transform.position;
            Collider[] colliders = Physics.OverlapBox(center, _pushSize);

            foreach (Collider pushedObject in colliders) {
                MovementController hittedMvtController = pushedObject.GetComponent<MovementController>();
                if (hittedMvtController != null && hittedMvtController.playerName != _movementController.playerName) {
                    float stunTime = 0f;
                    if (stun) { stunTime = this.stunTime; }
                    //hittedMvtController.physics.Bump((pushedObject.transform.position - transform.position).normalized, _pushForce, stunTime);
                    hittedMvtController.physics.Bump(transform.right * horizontal, _pushForce, stunTime);
                    //hittedMvtController.physics.AddVelocity(Vector3.right * horizontal * _pushForce);
                    //hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.frictions.amplitude, "frictions.amplitude", hittedMvtController.physics.frictions.amplitude * 15f, 1f);
                    //hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.stun, "stun", true, 2.5f);
                }
            }
        }

        public void ComputeAttack(float horizontal) {
            if (pressTimer <= 0.3f) {
                Colliders(horizontal, flanksAttack._throwBoxDimension, flanksAttack._pushForce, false);
            } else if (pressTimer <= 1f) {
                Colliders(horizontal, flanksAttack._throwBoxDimension * poweredPushRadius, flanksAttack._pushForce, false);
            } else if (pressTimer <= 2f) {
                Colliders(horizontal, flanksAttack._throwBoxDimension * poweredPushRadius, flanksAttack._pushForce * poweredPushForce, false);
            } else {
                Colliders(horizontal, flanksAttack._throwBoxDimension * poweredPushRadius, flanksAttack._pushForce * poweredPushForce, true);
            }
        }
    }
}