using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian
{
    public class JumpingSheep : MonoBehaviour
    {
        private MovementController _movementController;

        [Header("MegaJump")]
        public float _megaJumpForce = 100f;
        public float _megaAccelForce = 10f;

        [Header("Stomp")]
        public int _nbrStomp = 0;
        public float _stompRadius;
        public float _stompForce = 50f;


        private void Start()
        {
            _movementController = GetComponent<MovementController>();
        }

        private void Update()
        {
            //ChangeWoolColor();
        }

        public void MegaJump()
        {
            _movementController.physics.AddVelocity(new Vector3(0f, _megaJumpForce, _megaAccelForce));
        }

        public void Stomp()
        {
            _movementController.physics.AddVelocity(transform.worldToLocalMatrix.MultiplyVector(-transform.up) * _stompForce);

            Collider[] colliders = Physics.OverlapSphere(-transform.up + transform.localPosition, _stompRadius);

            foreach (Collider pushedObject in colliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    MovementController movementController = pushedObject.GetComponent<MovementController>();
                    movementController.physics.AddVelocity(
                        pushedObject.transform.worldToLocalMatrix.MultiplyVector(-transform.forward) * _stompForce
                    );
                    movementController.physics.TimedChange(ref movementController.physics.frictions.amplitude, "frictions.amplitude", movementController.physics.frictions.amplitude * 15f, 1f);
                    movementController.physics.TimedChange(ref movementController.physics.stun, "stun", true, 2.5f);
                }
            }

        }

        private void ChangeWoolColor()
        {
            if (_nbrStomp > 3)
            {
                //il devien noir
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(-transform.up + transform.localPosition, 5f);
        }
    }
}
