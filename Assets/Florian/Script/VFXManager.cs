using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian
{
    public class VFXManager : MonoBehaviour
    {
        private MovementController _movementController = null;

        [SerializeField] private GameObject _stunFX = null;
        [SerializeField] private GameObject _jumpFX = null;
        [SerializeField] private GameObject _stompFX = null;
        [SerializeField] private GameObject _flanksAttackFX = null;
        [SerializeField] private GameObject _textFX = null;

        private void Start()
        {
            _movementController = GetComponent<MovementController>();
            if (_movementController == null) { Debug.LogError("Movement Controller not found on VFXManager"); }
        }

        public void Stunned(float stunTime)
        {
            StartCoroutine(PlayerVFX(_stunFX, stunTime));
        }

        public void TextToon(Vector3 position, float stunTime)
        {
            _textFX.transform.position = new Vector3(position.x, _textFX.transform.position.y, position.z);
            StartCoroutine(PlayerVFX(_textFX, stunTime));
        }

        public void JumpSkillFX(Vector3 position, float time)
        {
            GameObject ob = Instantiate(_jumpFX, position, Quaternion.Euler(-90f, 0f, 0f));
            Destroy(ob, time);
        }

        public void StompSkillFX(Vector3 position, float time)
        {
            GameObject ob = Instantiate(_stompFX, position, Quaternion.Euler(-90f, 0f, 0f));
            Destroy(ob, time);
        }

        public void AttackSkillFX(Vector3 position, float time)
        {
            _flanksAttackFX.transform.position = new Vector3(position.x, _flanksAttackFX.transform.position.y, position.z);
            StartCoroutine(PlayerVFX(_flanksAttackFX, time));
        }

        IEnumerator PlayerVFX(GameObject fx, float time)
        {
            fx.SetActive(true);
            yield return new WaitForSeconds(time);
            fx.SetActive(false);
        }
    }
}
