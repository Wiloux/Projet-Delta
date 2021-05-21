using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian
{
    public class JumpingSheep : MonoBehaviour
    {
        [SerializeField] private MovementController movement;
        private bool _jump = false;

        [Header("MegaJump")]
        public float _megaJumpForce;

        [Header("Stomp")]
        public int _nbrStomp = 0;
        public float _megaGravity;
        public float _stompRadius;

        private void Update()
        {
            /*if (_jump)
            {
                if (movement.airborn)
                {
                    MegaJump();
                }
                else
                {
                    Stomp();
                }
            }
            ChangeWoolColor();*/
        }

        private void MegaJump()
        {
            movement.physics.jumpForce = _megaJumpForce;
        }

        private void Stomp()
        {
            _nbrStomp++;
            //movement.gravity = _megaGravity;

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
            Gizmos.DrawSphere(transform.position, 5f);
        }
    }
}
