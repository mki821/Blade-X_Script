using UnityEngine;
using DG.Tweening;
using Swift_Blade.Feeling;

namespace Swift_Blade.Pool
{
    public class DragonBall : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _turnAmount = 360.0f;
        [SerializeField] private float _distance = 2.5f;
        [SerializeField] private LayerMask _whatIsEnemy;
        [SerializeField] private Transform _particle;

        [Header("Hit effect")]
        [SerializeField] private HitStopSO     _hitStop;
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioClip _clip;

        private float _damage;
        private float _originAngle;

        private SphereCollider _collider;

        private Sequence _sequence;

        public void Initialize(Transform player, float damage, float angle)
        {
            ParticleSystem.MainModule mainModule = _particle.GetComponent<ParticleSystem>().main;
            mainModule.customSimulationSpace = player;
            _damage = damage;
            _originAngle = angle;
            transform.localEulerAngles = Vector3.up * angle;
        }

        public void OnPop()
        {
            _collider = GetComponent<SphereCollider>();
            
            _sequence = DOTween.Sequence();
            
            _collider.enabled = false;
            _particle.gameObject.SetActive(false);
        }

        public void Enable()
        {
            transform.localEulerAngles = Vector3.up * _originAngle;
            
            _collider.enabled = true;
            _particle.gameObject.SetActive(true);
            
            _sequence = DOTween.Sequence();

            _sequence.Append(DOTween.To(() => _collider.center.z, x => _collider.center = Vector3.forward * x, _distance, 1f));
            _sequence.Join(_particle.DOLocalMoveZ(_distance, 1f));
            _sequence.Append(transform.DOLocalRotate(Vector3.up * (_originAngle + _turnAmount), 2.8f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(10, LoopType.Restart));

            _sequence.Append(DOTween.To(() => _collider.center.z, x => _collider.center = Vector3.forward * x, 0f, 1f));
            _sequence.Join(_particle.DOLocalMoveZ(0f, 1f));
            _sequence.AppendCallback(() => {
                _collider.enabled = false;
                _particle.gameObject.SetActive(false);
            });
            _sequence.AppendInterval(6f);
            _sequence.AppendCallback(Enable);

            // 이거 안하면 에러남..
            _sequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void Disable()
        {
            _sequence.Kill();
            
            _collider.enabled = false;
            _particle.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(((1 << other.gameObject.layer) & _whatIsEnemy.value) > 0)
            {
                if(other.TryGetComponent(out IHealth health))
                {
                    _source.PlayOneShot(_clip);

                    ActionData actionData = new ActionData(transform.position, Vector3.up, _damage, false)
                        {
                            hitPoint = other.transform.position,
                            textColor = new Color(243f/255f,156f/255f, 18f/255f,1)
                        };
                    health.TakeDamage(actionData);
                    HitStopManager.Instance.StartHitStop(_hitStop);
                    CameraFocusManager.Instance.StartFocus(_focus);
                    CameraShakeManager.Instance.DoShake(CameraShakeType.PlayerAttack);
                    //FloatingTextGenerator.Instance.GenerateText(Mathf.RoundToInt(_damage).ToString(), other.transform.position);
                }
            }
        }
    }
}
