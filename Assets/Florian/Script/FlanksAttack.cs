using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian {
    public class FlanksAttack : MonoBehaviour {
        [SerializeField] private float _pushForce;
        [SerializeField] private float _pushRadius;
        public float _cooldown;
        public float _timer;

        private void Update() {
            if (_timer > 0)
                _timer -= Time.deltaTime;

            if (_timer < 0)
                _timer = 0;
        }

        public void Push(float horizontal) {
            Colliders(horizontal);
        }

        //private void RightColliders() {
        //    Collider[] rightColliders = Physics.OverlapSphere(transform.right + transform.localPosition, _pushRadius);

        //    foreach (Collider pushedObject in rightColliders) {
        //        if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name) {
        //            Debug.Log(pushedObject.name);
        //            pushedObject.GetComponent<MovementController>().physics.AddVelocity(pushedObject.transform.worldToLocalMatrix.MultiplyVector(transform.right) * _pushForce);
        //            //pushedBody.AddForce(_pushForce * transform.right, ForceMode.Impulse);
        //            _timer = _cooldown;
        //        }
        //    }
        //}

        //private void LeftColliders() {
        //    Collider[] leftColliders = Physics.OverlapSphere(-transform.right + transform.localPosition, _pushRadius);

        //    foreach (Collider pushedObject in leftColliders) {
        //        if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name) {
        //            Debug.Log(pushedObject.name);
        //            Rigidbody pushedBody = pushedObject.GetComponent<Rigidbody>();
        //            pushedObject.GetComponent<MovementController>().physics.AddVelocity(pushedObject.transform.worldToLocalMatrix.MultiplyVector(-transform.right) * _pushForce);
        //            _timer = _cooldown;
        //        }
        //    }
        //}

        private void Colliders(float horizontal) {
            Collider[] leftColliders = Physics.OverlapSphere(transform.right * horizontal + transform.localPosition, _pushRadius);

            foreach (Collider pushedObject in leftColliders) {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name) {
                    Debug.Log(pushedObject.name);
                    pushedObject.GetComponent<MovementController>().physics.AddVelocity(
                        pushedObject.transform.worldToLocalMatrix.MultiplyVector(transform.right * horizontal) * _pushForce
                    );
                    _timer = _cooldown;
                }
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.right + transform.localPosition, _pushRadius);
            Gizmos.DrawSphere(-transform.right + transform.localPosition, _pushRadius);
        }
    }
}