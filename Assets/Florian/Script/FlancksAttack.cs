using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian
{
    public class FlancksAttack : MonoBehaviour
    {
        [SerializeField] private float _pushForce;
        [SerializeField] private float _pushRadius;
        [SerializeField] private int _cooldown;
        private float _timer = 0f;

        private void Update()
        {
             _timer += Time.deltaTime;
            if (_timer > _cooldown)
                _timer = 0;
        }

        private void Push(float horizontal)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _pushRadius);

            foreach (Collider pushedObject in colliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    Rigidbody pushedBody = pushedObject.GetComponent<Rigidbody>();
                    pushedBody.AddForce(_pushForce * transform.right * horizontal, ForceMode.Impulse);
                }
            }
        }
    }
}
