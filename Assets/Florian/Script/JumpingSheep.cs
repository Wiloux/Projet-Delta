using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian
{
    public class JumpingSheep : MonoBehaviour
    {
        [SerializeField] private Movement movement;
        private bool _jump = false;

        [Header("MegaJump")]
        [SerializeField] private float _megaJumpForce;

        [Header("Stomp")]
        [SerializeField] private int _nbrStomp = 0;
        [SerializeField] private float _megaGravity;
        [SerializeField] private float _stompRadius;

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
            movement.jumpForce = _megaJumpForce;
        }

        private void Stomp()
        {
            _nbrStomp++;
            movement.gravity = _megaGravity;

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
