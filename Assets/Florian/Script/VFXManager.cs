using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Florian
{
    public class VFXManager : MonoBehaviour
    {
        private MovementController _movementController = null;
        private RaceManager raceManager = null;

        [Header("VFX")]
        [SerializeField] private ParticleSystem _stunFX = null;
        [SerializeField] private ParticleSystem _jumpFX = null;
        [SerializeField] private ParticleSystem _stompFX = null;
        [SerializeField] private ParticleSystem _flanksAttackFX = null;
        [SerializeField] private ParticleSystem _textFX = null;
        [SerializeField] private ParticleSystem _basicTrailsFX = null;
        [SerializeField] private ParticleSystem _offTrackTrailsFX = null;
        [SerializeField] private ParticleSystem _bumpFX = null;
        [SerializeField] private AngerParticles angerFX = null;
        [SerializeField] private ParticleSystem _waveFX = null;
        [SerializeField] private ParticleSystem _splashFX = null;
        public ParticleSystem sharkChargeFX = null;

        [Header("Set Teleporter Material")]
        public Image _fadeImg;

        private void Start()
        {
            _movementController = GetComponent<MovementController>();
            if (_movementController == null) { Debug.LogError("Movement Controller not found on VFXManager"); }

            _fadeImg = GameObject.Find(GetComponent<MovementController>().playerName + "/PlayerRoot/HUD/TP Fade").GetComponent<Image>();
            _fadeImg.material = Instantiate(_fadeImg.material);

            raceManager = GameObject.Find("----------- Utilities ----------/Utilities/RaceManager").GetComponent<RaceManager>();
            UpdateFadeUI();
        }

        [SerializeField] private ParticleSystem _slowFX = null;

        public void Slowed()
        {
            _slowFX.Play();
        }
        public void SharkFX(float timer)
        {
            ParticleSystem.MainModule mainSharkFX = sharkChargeFX.main;
            if (timer <= 0.3f)
            {
                mainSharkFX.startColor = new ParticleSystem.MinMaxGradient(Color.blue);
                mainSharkFX.startSpeed = new ParticleSystem.MinMaxCurve(-5f, -2f);
            }
            else if (timer <= 1f)
            {
                mainSharkFX.startColor = new ParticleSystem.MinMaxGradient(Color.green);
                mainSharkFX.startSpeed = new ParticleSystem.MinMaxCurve(-6f, -3f);
            }
            else if (timer <= 2f)
            {
                mainSharkFX.startColor = new ParticleSystem.MinMaxGradient(Color.yellow);
                mainSharkFX.startSpeed = new ParticleSystem.MinMaxCurve(-7f, -4f);
            }
            else
            {
                mainSharkFX.startColor = new ParticleSystem.MinMaxGradient(Color.red);
                mainSharkFX.startSpeed = new ParticleSystem.MinMaxCurve(-9f, -5f);
            }
        }

        public void GlidFX(bool active)
        {

            _waveFX.gameObject.SetActive(active);
            if (_waveFX.isPlaying && !active)
            {
                _splashFX.Play();
                _waveFX.Stop();
            }
            else if (!_waveFX.isPlaying && active)
            {
                AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[7], 1f);
                
                _splashFX.Play();
                _waveFX.Play();
            }
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
            ParticleSystem ob = Instantiate(_jumpFX, position, Quaternion.Euler(-90f, 0f, 0f));
            ob.Play();
            Destroy(ob.gameObject, time);
        }

        public void StompSkillFX(Vector3 position, float time)
        {
            ParticleSystem ob = Instantiate(_stompFX, position, Quaternion.Euler(-90f, 0f, 0f));
            ob.Play();
            Destroy(ob.gameObject, time);
        }

        public void AttackSkillFX(Vector3 position, float time)
        {
            _flanksAttackFX.transform.position = new Vector3(position.x, _flanksAttackFX.transform.position.y, position.z);
            StartCoroutine(PlayerVFX(_flanksAttackFX, time));
        }

        public void BumpFX(Vector3 position, float time)
        {
            _bumpFX.transform.position = new Vector3(position.x, _bumpFX.transform.position.y, position.z);
            StartCoroutine(PlayerVFX(_bumpFX, time));
        }

        public void TrailsFX()
        {

            if (!_movementController.Airborn)
            {
                if (_movementController.physics.offTracking)
                {
                    if (!_basicTrailsFX.isPlaying)
                    {
                        _basicTrailsFX.Play();
                        _offTrackTrailsFX.Stop();
                    }
                }
                else
                {
                    if (!_offTrackTrailsFX.isPlaying)
                    {
                        _basicTrailsFX.Stop();
                        _offTrackTrailsFX.Play();
                    }
                }
            }
            else
            {
                _offTrackTrailsFX.Stop();
                _basicTrailsFX.Stop();
            }
        }

        public void AngerStack(int side)
        {
            angerFX.Create(side);
        }

        public void AngerUnstack(int side)
        {
            angerFX.Unstack(side);
        }

        public void AngerClear(int side)
        {
            angerFX.ClearParticles(side);
        }

        public void AngerRadiant() {
            angerFX.PlayRadiant();
        }

        IEnumerator PlayerVFX(ParticleSystem fx, float time)
        {
            fx.Play();
            yield return new WaitForSeconds(time);
            fx.Stop();
        }

        private void UpdateFadeUI()
        {
            if (raceManager.characters.Count == 2)
            {
                _fadeImg.gameObject.transform.localPosition += new Vector3(0f, 0f, -20f);
            }

        }
    }
}
