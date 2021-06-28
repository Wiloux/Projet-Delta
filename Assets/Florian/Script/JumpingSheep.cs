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
            _woolMaterial = GameObject.Find(_movementController.playerName + "/PlayerRoot/Model/Mounton_Anims/Mouton_Mesh").GetComponent<SkinnedMeshRenderer>().material;
        }

        public void MegaJump()
        {
            Vector3 jumpFxPos = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
            GetComponent<VFXManager>().JumpSkillFX(jumpFxPos, 4f);
            _movementController.physics.AddVelocity(new Vector3(0f, _megaJumpForce, _megaAccelForce));
            //Debug.Log(_movementController.physics.velocity);
            _nbrStomp--;
            RetrieveACharge();
        }

        public void Stomp()
        {
            //_movementController.physics.AddVelocity(transform.worldToLocalMatrix.MultiplyVector(-transform.up) * _stompSpeed);
            _movementController.physics.AddVelocity(Vector3.down * _stompSpeed);
            //Movement physics = _movementController.physics;
            //physics.TimedChange(ref physics.gravityCurve.curve, "gravityCurve.curve", AnimationCurve.Constant(0f, 1f, 1f), 1f);
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
            Vector3 stompFxPos = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
            GetComponent<VFXManager>().StompSkillFX(stompFxPos, 5f);
            Bump();
        }

        private void Bump()
        {
            Collider[] colliders = Physics.OverlapSphere(-transform.up + transform.localPosition, _stompRadius);

            foreach (Collider pushedObject in colliders)
            {
                MovementController hittedMvtController = pushedObject.GetComponent<MovementController>();
                if (hittedMvtController != null && hittedMvtController.playerName != _movementController.playerName)
                {
                    Vector3 directionBtwnPlayers = (transform.position - pushedObject.transform.position).normalized;
                    hittedMvtController.physics.AddVelocity(directionBtwnPlayers.Redirect(Vector3.forward, pushedObject.transform.forward) * _stompForce);
                    hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.frictions.amplitude, "frictions.amplitude", hittedMvtController.physics.frictions.amplitude * 5f, 1f);
                    hittedMvtController.physics.TimedChange(ref hittedMvtController.physics.stun, "stun", true, stunTime);
                    pushedObject.GetComponent<VFXManager>().Stunned(stunTime);
                }
            }
        }
    }
}

