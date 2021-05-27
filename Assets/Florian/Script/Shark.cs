using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Florian
{
    public class Shark : MonoBehaviour
    {
        public float pushForce;
        public float pushRadius;
        public float poweredPushForce;
        public float poweredPushRadius;

        public float pressTimer;
        public float _cooldown;
        public float _timer;


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
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    MovementController movementController = pushedObject.GetComponent<MovementController>();
                    movementController.physics.AddVelocity(Vector3.right * horizontal * _pushForce);
                    movementController.physics.TimedChange(ref movementController.physics.frictions.amplitude, "frictions.amplitude", movementController.physics.frictions.amplitude * 15f, 1f);
                    movementController.physics.TimedChange(ref movementController.physics.stun, "stun", true, 2.5f);
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