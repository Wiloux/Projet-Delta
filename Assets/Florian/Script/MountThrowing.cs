using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian
{

    public class MountThrowing : MonoBehaviour
    {
        private MovementController _playerMovementController;
        [SerializeField] private float _throwVerticalForce;
        [SerializeField] private float _throwHorizontalForce;
        [SerializeField] private float _throwForce;
        [SerializeField] private Vector3 _throwBoxDimension;
        [SerializeField] private float _airbornTime = 0f;
        public float _cooldown;
        public float _timer;
        public bool _isThrowing;

        private void Start()
        {
            _playerMovementController = GetComponent<MovementController>();
        }

        public void MountThrow()
        {
            _playerMovementController.physics.AddVelocity(new Vector3(0f, _throwVerticalForce, _throwHorizontalForce));
            _playerMovementController.physics.TimedChange(ref _playerMovementController.physics.gravityCurve.amplitude, "gravityCurve.amplitude", _playerMovementController.physics.gravityCurve.amplitude * 0.01f, _airbornTime);
            _playerMovementController.physics.TimedChange(ref _playerMovementController.physics.turn.amplitude, "turn.amplitude", _playerMovementController.physics.turn.amplitude * 0.5f, _airbornTime * 2f);
            StartCoroutine(WaitNegateVelocity(_airbornTime));
            _isThrowing = true;
        }

        public void MountThrowUpdate()
        {
            if (!_playerMovementController.Airborn)
                _isThrowing = false;

            Collider[] colliders = Physics.OverlapBox(transform.forward + transform.localPosition, _throwBoxDimension);

            foreach (Collider pushedObject in colliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    _isThrowing = false;
                    Debug.Log("pushed");
                    MovementController pushedMovementController = pushedObject.GetComponent<MovementController>();
                    pushedMovementController.physics.AddVelocity(Vector3.right * _throwForce);
                    pushedMovementController.physics.TimedChange(ref pushedMovementController.physics.frictions.amplitude, "frictions.amplitude", pushedMovementController.physics.frictions.amplitude * 5f, 1f);
                    pushedMovementController.physics.TimedChange(ref pushedMovementController.physics.stun, "stun", true, 2.5f);

                    _playerMovementController.physics.NegateVelocity(Axis.Z);

                    _timer = _cooldown;
                    return;
                }
                else if (pushedObject.tag == "Obstacle" || pushedObject.tag == "Wall")
                {
                    _isThrowing = false;
                    _playerMovementController.physics.TimedChange(ref _playerMovementController.physics.stun, "stun", true, 2.5f);
                    _playerMovementController.physics.NegateVelocity(Axis.Z);
                    return;
                }
            }

            if (!_playerMovementController.Airborn)
            {
                Vector3 playerVelocity = _playerMovementController.physics.velocity;
                _playerMovementController.physics.NegateVelocity(Axis.Z);
                _playerMovementController.physics.AddVelocity(new Vector3(0f, 0f, playerVelocity.z / 2f));
            }
        }

        IEnumerator WaitNegateVelocity(float time)
        {
            yield return new WaitForSeconds(time);
            _playerMovementController.physics.NegateVelocity(Axis.Y);
        }
    }
}
