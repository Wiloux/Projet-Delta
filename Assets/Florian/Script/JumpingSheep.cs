using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

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
        public float _stompRadius = 40f;
        public float _stompForce = 10f;
        public float _stompSpeed = 50f;
        public float cooldown = 10f;
        private bool _isBlack = false;


        private void Start()
        {
            _movementController = GetComponent<MovementController>();
        }

        private void Update()
        {
            if (!_isBlack)
                ChangeWoolColor();
            else
                StartCoroutine(CooldownWoolColor());
        }

        public void MegaJump()
        {
            _movementController.physics.AddVelocity(new Vector3(0f, _megaJumpForce, _megaAccelForce));
            //test changer gravité
            _nbrStomp++;
        }

        public void Stomp()
        {
            _movementController.physics.AddVelocity(transform.worldToLocalMatrix.MultiplyVector(-transform.up) * _stompSpeed);
            StartCoroutine(CheckGrounded());
            _nbrStomp = 3;
        }

        private void ChangeWoolColor()
        {
            if (_nbrStomp >= 3)
            {
                _isBlack = true;
            }
        }

        IEnumerator CooldownWoolColor()
        {
            _isBlack = false;
            yield return new WaitForSeconds(cooldown);
            _nbrStomp = 0;
        }

        IEnumerator CheckGrounded()
        {
            while (_movementController.Airborn)
            {
                yield return new WaitForEndOfFrame();
            }
            Bump();
        }

        private void Bump()
        {
            Collider[] colliders = Physics.OverlapSphere(-transform.up + transform.localPosition, _stompRadius);

            foreach (Collider pushedObject in colliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    MovementController movementController = pushedObject.GetComponent<MovementController>();
                    //Vector3 direction = (pushedObject.transform.position - transform.position).normalized;
                    movementController.physics.AddVelocity(Vector3.back * _stompForce);
                    movementController.physics.TimedChange(ref movementController.physics.frictions.amplitude, "frictions.amplitude", movementController.physics.frictions.amplitude * 5f, 1f);
                    movementController.physics.TimedChange(ref movementController.physics.stun, "stun", true, 2.5f);
                }
            }
        }
    }
}

