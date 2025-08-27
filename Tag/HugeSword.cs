using System.Collections;
using UnityEngine;
using Swift_Blade.Feeling;

namespace Swift_Blade.Pool
{
    public class HugeSword : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _castTime = 2.0f;
        [SerializeField] private float _fallingSpeed = 2.0f;

        private float _timer = 0.0f;

        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private MeshRenderer _meshRenderer;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        public void OnCreate()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void OnPop()
        {
            _meshRenderer.material.SetFloat("_Dissolve", 1.0f);

            _timer = 0.0f;
        }

        public void Cast(Vector3 startPosition, Vector3 endPosition)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;

            transform.position = _startPosition;

            Vector3 direction = _endPosition - _startPosition;
            transform.rotation = Quaternion.LookRotation(direction);

            StartCoroutine(CastCoroutine());
        }

        private IEnumerator CastCoroutine()
        {
            float percent = 0.0f;
            bool flag = false;
            Vector3 endPosition = _endPosition + (_endPosition - _startPosition).normalized * 5.0f;
            while (percent < 1.0f)
            {
                _timer += _fallingSpeed * Time.deltaTime;
                percent = _timer / _castTime;

                transform.position = Vector3.Lerp(_startPosition, endPosition, Mathf.Pow(percent, 4.0f));

                CameraShakeManager.Instance.DoShake(CameraShakeType.PlayerAttack);

                if (!flag && transform.position.y <= _endPosition.y + 1.7f)
                {
                    HugeSwordExplosion explosion = MonoGenericPool<HugeSwordExplosion>.Pop();
                    explosion.transform.position = _endPosition;
                    explosion.Cast();

                    GroundCrackParticle groundCrack = MonoGenericPool<GroundCrackParticle>.Pop();
                    groundCrack.transform.position = _endPosition;
                    groundCrack.transform.localScale = Vector3.one * 2.0f;                    

                    flag = true;
                }

                yield return null;
            }

            CameraShakeManager.Instance.DoShake(CameraShakeType.Strong);
            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            yield return new WaitForSeconds(1.0f);

            percent = 0.0f;
            while (percent < 1.0f)
            {
                percent += Time.deltaTime;

                _meshRenderer.material.SetFloat("_Dissolve", 1.0f - percent);

                yield return null;
            }

            MonoGenericPool<HugeSword>.Push(this);
        }
    }
}
