using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian
{
    public class FlancksAttack : MonoBehaviour
    {
        [SerializeField] private float _pushForce;
        [SerializeField] private float _pushRadius;
        public float _cooldown;
        public float _timer;

        private void Update()
        {
            if (_timer > 0)
                _timer -= Time.deltaTime;

            if (_timer < 0)
                _timer = 0;
        }

        public void Push(float horizontal)
        {
            if (horizontal > 0)
                StartCoroutine(RightColliders());
            else if (horizontal < 0)
                StartCoroutine(LeftColliders());
        }

        IEnumerator RightColliders()
        {
            Collider[] rightColliders = Physics.OverlapSphere(transform.right + transform.localPosition, _pushRadius);

            foreach (Collider pushedObject in rightColliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    Debug.Log(pushedObject.name);
                    Rigidbody pushedBody = pushedObject.GetComponent<Rigidbody>();
                    pushedObject.GetComponent<Movement>().AddVelocity(pushedObject.transform.worldToLocalMatrix.MultiplyVector(transform.right) * _pushForce);
                    //pushedBody.AddForce(_pushForce * transform.right, ForceMode.Impulse);
                    _timer = _cooldown;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        IEnumerator LeftColliders()
        {
            Collider[] leftColliders = Physics.OverlapSphere(-transform.right + transform.localPosition, _pushRadius);

            foreach (Collider pushedObject in leftColliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    Debug.Log(pushedObject.name);
                    Rigidbody pushedBody = pushedObject.GetComponent<Rigidbody>();
                    pushedObject.GetComponent<Movement>().AddVelocity(pushedObject.transform.worldToLocalMatrix.MultiplyVector(-transform.right) * _pushForce);
                    //pushedBody.AddForce(_pushForce * -transform.right, ForceMode.Impulse);
                    _timer = _cooldown;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.right + transform.localPosition, _pushRadius);
            Gizmos.DrawSphere(-transform.right + transform.localPosition, _pushRadius);
        }

    }
}