using Swift_Blade.Feeling;
using Swift_Blade.Pool;
using UnityEngine;

namespace Swift_Blade
{
    public class DragonPulse : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 10.0f;
        [SerializeField] private LayerMask _whatIsEnemy;

        [SerializeField] private float _poolPushTime = 3.0f;

        private float _damage;

        public void OnPop()
        {
            Invoke(nameof(Push), _poolPushTime);
        }

        private void Push() => MonoGenericPool<DragonPulse>.Push(this);

        public void SetDamage(float damage) => _damage = damage;

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
                    ActionData actionData = new ActionData
                    {
                        hitPoint = other.transform.position,
                        damageAmount = _damage
                    };

                    health.TakeDamage(actionData);
                }
            }
        }
    }
}
