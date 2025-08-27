using UnityEngine;

namespace Swift_Blade.Enemy.Boss
{
    public class FireWall : MonoBehaviour
    {
        [SerializeField] private float _knockbackForce = 4000.0f;
        [SerializeField] private LayerMask _whatIsEnemy;

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _whatIsEnemy.value) > 0)
            {
                if (other.TryGetComponent(out IHealth health))
                {
                    Vector3 direction = other.transform.position - transform.position;
                    direction.y = 1.0f;
                    direction.Normalize();

                    ActionData actionData = new ActionData
                    {
                        knockbackDirection = direction,
                        knockbackForce = _knockbackForce
                    };
                    health.TakeDamage(actionData);
                }
            }
        }
    }
}
