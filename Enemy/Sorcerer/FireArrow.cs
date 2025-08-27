using UnityEngine;

namespace Swift_Blade.Pool
{
    public class FireArrow : MonoBehaviour
    {
        [SerializeField] private LayerMask _whatIsTarget;
        
        [SerializeField] private float _speed;
        [SerializeField] private float _pushTime;
        private float _pushTimer;
        
        private Rigidbody _rigidbody;
        
        private bool _isDead;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            Shot();
        }
        
        private void Update()
        {
            _pushTimer += Time.deltaTime;
            if (_pushTimer > _pushTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((_whatIsTarget & (1 << other.gameObject.layer)) != 0 && _isDead == false)
            {
                if (other.gameObject.TryGetComponent(out IHealth health))
                {
                    Hit(health);
                }
            }
        }

        private void Hit(IHealth health)
        {
            _isDead = true;
            health.TakeDamage(new ActionData() { damageAmount = 1, stun = true });
        }

        public void Shot()
        {
            Vector3 velocity = transform.forward;
            _rigidbody.linearVelocity = velocity * _speed;
        }
    }
}
