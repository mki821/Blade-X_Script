using UnityEngine;
using Swift_Blade.Audio;
using Swift_Blade.Feeling;
using DG.Tweening;

namespace Swift_Blade.Pool
{
    public class SwordAura : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed;
        [SerializeField] private LayerMask _whatIsEnemy;
        [SerializeField] private LayerMask _whatIsWall;

        [SerializeField] private float _poolPushTime = 1.5f;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        [SerializeField] private BaseAudioSO hitClip;
        
        private float _damage;

        public void OnPop()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).localScale = Vector3.one * 0.4f;
            }
            transform.localScale = Vector3.one * 0.4f;

            Invoke(nameof(Push), _poolPushTime);
        }

        private void Push()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).DOScale(0.0f, 0.5f);
            }
            transform.DOScale(0.0f, 0.5f).OnComplete(() => MonoGenericPool<SwordAura>.Push(this));
        }

        public void Initilize(float damage)
        {
            _damage = damage;

            bool isHit = Physics.BoxCast(transform.position, GetComponent<Collider>().bounds.size, transform.forward, out RaycastHit hit, transform.rotation, 20.0f, _whatIsEnemy);
            if (isHit)
            {
                Vector3 direction = hit.transform.position - transform.position;
                direction.y = 0.0f;
                direction.Normalize();

                transform.rotation = Quaternion.LookRotation(direction);
                transform.Rotate(0.0f, 0.0f, 90.0f);
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _whatIsEnemy.value) > 0)
            {
                if (other.TryGetComponent(out IHealth health))
                {
                    AudioManager.PlayWithInit(hitClip, true);

                    ActionData actionData = new ActionData
                    {
                        hitPoint = other.transform.position,
                        damageAmount = _damage
                    };
                    health.TakeDamage(actionData);

                    CameraShakeManager.Instance.DoShake(CameraShakeType.PlayerAttack);
                    CameraFocusManager.Instance.StartFocus(_focus);
                    HitStopManager.Instance.StartHitStop(_hitStop);
                }
            }
            else if (((1 << other.gameObject.layer) & _whatIsWall.value) > 0)
            {
                CancelInvoke(nameof(Push));
                Push();
            }
        }
    }
}
