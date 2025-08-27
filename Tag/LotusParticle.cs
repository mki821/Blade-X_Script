using UnityEngine;

namespace Swift_Blade.Pool
{
    public class LotusParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _rotationSpeed = 10.0f;

        private int _rotationDirection;

        public void OnPop()
        {
            _rotationDirection = Random.Range(0, 2) * 2 - 1;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * _rotationDirection * Time.deltaTime);
        }
    }
}
