using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian
{
    public class JumpingSheep : MonoBehaviour
    {
        [SerializeField] private Movement movement;

        [Header("MegaJump")]
        [SerializeField] private float _megaJumpForce;
        [SerializeField] private bool useMegaJump;

        [Header("Stomp")]
        [SerializeField] private int _nbrStomp = 0;
        [SerializeField] private float _megaGravity;
        [SerializeField] private float _stompRadius;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private bool useStomp;

        private void Start()
        {
            movement = GetComponent<Movement>();
        }

        private void Update()
        {
            //if (movement.player.GetButton("Jump") /*&& movement.useJumpSkill*/)
            {
                if (!movement.airborn && !useMegaJump)
                    StartCoroutine(MegaJump());
                if (movement.airborn && !useStomp)
                    StartCoroutine(Stomp());
            }
            if (_nbrStomp == 3)
                ChangeWoolColor();
        }

        IEnumerator MegaJump()
        {
            useMegaJump = true;
            _nbrStomp++;
            //movement._rb.AddForce(_megaJumpForce * Vector3.up, ForceMode.Impulse);
            //movement._rb.velocity += _megaJumpForce * Vector3.up;
            Debug.Log("MegaJump !");
            yield return new WaitForSeconds(0.3f);
            useStomp = false;
        }

        IEnumerator Stomp()
        {
            useStomp = true;
            Debug.Log("Stomp !");
            _nbrStomp = 3;
            //movement.gravity = _megaGravity;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _stompRadius, layerMask);
            if (hitColliders.Length > 0)
            {
                Debug.Log(hitColliders[0]);
            }
            yield return new WaitForSeconds(0.3f);
            useMegaJump = false;
        }

        private void ChangeWoolColor()
        {
            Debug.Log("Black");
            _nbrStomp = 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawSphere(transform.position, _stompRadius);
        }

    }
}
