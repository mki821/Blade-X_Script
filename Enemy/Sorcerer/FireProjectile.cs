using System.Collections;
using UnityEngine;

namespace Swift_Blade.Pool
{
    public class FireProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private LayerMask _whatIsTarget;
        
        [SerializeField] private float _originSpeed;
        [SerializeField] private float _pushTime;

        private float _speed;
        private float _startAngle;
        private float _pushTimer;

        private ParticleSystem _particleSystem;

        public void OnCreate()
        {
            _particleSystem = GetComponentInChildren<ParticleSystem>();
        }

        public void OnPop()
        {
            _speed = _originSpeed;
            _pushTimer = 0f;
            
            _particleSystem.Clear();
        }

        public void SetAngle(float angle)
        {
            transform.eulerAngles = new Vector3(0f, angle, 0f);
            _startAngle = angle;
            
            _particleSystem.Play();

            StartCoroutine(MoveCoroutine());
        }

        private IEnumerator MoveCoroutine()
        {
            float percent;
            float currentAngle;

            while(_pushTimer < _pushTime)
            {
                _pushTimer += Time.deltaTime;
                percent = _pushTimer / _pushTime;

                currentAngle = Mathf.Lerp(0f, 360f, percent);
                transform.eulerAngles = new Vector3(0f, _startAngle + currentAngle, 0f);

                _speed += 10f * Time.deltaTime;
                transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
                
                yield return null;
            }
            
            MonoGenericPool<FireProjectile>.Push(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((_whatIsTarget & (1 << other.gameObject.layer)) != 0)
            {
                if (other.gameObject.TryGetComponent(out IHealth health))
                {
                    health.TakeDamage(new ActionData() { damageAmount = 1, stun = true });
                }
            }
        }
    }
}
