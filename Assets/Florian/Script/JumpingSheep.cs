using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian
{
    public class JumpingSheep : MonoBehaviour
    {
        private MovementController _movementController;
        private Coroutine charging = null;
        private Material _woolMaterial = null;

        [Header("MegaJump")]
        public float _megaJumpForce = 100f;
        public float _megaAccelForce = 10f;

        [Header("Stomp")]
        public int _nbrStomp = 3;
        public float _stompRadius = 40f;
        public float _stompForce = 10f;
        public float _stompSpeed = 50f;
        public float cooldown = 5f;
        public float stunTime = 1.5f;

        private void Start()
        {
            _movementController = GetComponent<MovementController>();
            _woolMaterial = GameObject.Find(_movementController.playerName + "/mouton anilm/mouton pour test unity003").GetComponent<SkinnedMeshRenderer>().material;
        }

        public void MegaJump()
        {
            _movementController.physics.AddVelocity(new Vector3(0f, _megaJumpForce, _megaAccelForce));
            _nbrStomp--;
            RetrieveACharge();
        }

        public void Stomp()
        {
            _movementController.physics.AddVelocity(transform.worldToLocalMatrix.MultiplyVector(-transform.up) * _stompSpeed);
            StartCoroutine(CheckGrounded());
            _nbrStomp = 0;
            if(charging != null)
            {
                StopCoroutine(charging);
                charging = null;
            }
            RetrieveACharge();
        }

        private void RetrieveACharge()
        {
            if (charging != null)
                return;
            charging = StartCoroutine(ChargeRetrieve(cooldown));
        }

        IEnumerator ChargeRetrieve( float time)
        {
            for (int i = 0; i < time * 60f; i++)
            {
                float lerp = i / (time * 60f);
                Color woolColor = Color.Lerp(Color.black, Color.white, Mathf.Lerp(0, 1 / 3f, lerp) + (_nbrStomp / 3f));
                _woolMaterial.color = woolColor;
                yield return new WaitForSeconds(1 / 60f);
            }
            _nbrStomp++;
            charging = null;
            if (_nbrStomp < 3)
                RetrieveACharge();
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
                    movementController.physics.AddVelocity(Vector3.back * _stompForce);
                    movementController.physics.TimedChange(ref movementController.physics.frictions.amplitude, "frictions.amplitude", movementController.physics.frictions.amplitude * 5f, 1f);
                    movementController.physics.TimedChange(ref movementController.physics.stun, "stun", true, stunTime);
                }
            }
        }
    }
}

