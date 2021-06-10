using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Florian
{
    public class Shark : MonoBehaviour
    {
        private MovementController _movementController;

        public float pushForce;
        public float pushRadius;
        public float poweredPushForce;
        public float poweredPushRadius;

        public float pressTimer;
        public float _cooldown;
        public float _timer;

        private void Start() {
            _movementController = GetComponent<MovementController>();
        }

        private void Update()
        {
            if (_timer > 0)
                _timer -= Time.deltaTime;
        }

        public void Push(float horizontal)
        {
            Colliders(horizontal, pushRadius, pushForce);
        }

        private void Colliders(float horizontal, float _pushRadius, float _pushForce)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.right * horizontal + transform.localPosition, _pushRadius);

            foreach (Collider pushedObject in colliders)
            {
                MovementController hittedMvtController = pushedObject.GetComponent<MovementController>();
                if (hittedMvtController != null && hittedMvtController.playerName != _movementController.playerName)
                {
                    hittedMvtController.physics.AddVelocity(Vector3.right * horizontal * _pushForce);
                    hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.frictions.amplitude, "frictions.amplitude", hittedMvtController.physics.frictions.amplitude * 15f, 1f);
                    hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.stun, "stun", true, 2.5f);
                }
            }

        }
        public void ComputeAttack(float horizontal)
        {
            if (pressTimer <= 1)
            {
                Colliders(horizontal, pushRadius, pushForce);
            }
            else if (pressTimer <= 2)
            {
                Colliders(horizontal, poweredPushRadius, pushForce);
            }
            else
            {
                Colliders(horizontal, poweredPushRadius, poweredPushForce);
            }
        }
    }
}