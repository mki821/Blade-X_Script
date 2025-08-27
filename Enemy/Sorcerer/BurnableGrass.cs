using UnityEngine;

namespace Swift_Blade.Enemy.Boss
{
    public class BurnableGrass : MonoBehaviour, IHealth
    {
        [SerializeField] private float _burnTime = 3f;
        [SerializeField] private Material _burnMaterial;
        [SerializeField] private ParticleSystem _burnParticle;

        [SerializeField] private LayerMask _whatIsTarget;
        
        public bool IsDead => false;
        private bool _isBurn = false;
        private float _timer = 0f;

        private void Update()
        {
            if(!_isBurn) return;

            _timer += Time.deltaTime;

            if(_timer >= _burnTime)
            {
                _burnParticle.Stop();
                GetComponent<Collider>().enabled = false;
                enabled = false;
            }
        }

        

        public void TakeDamage(ActionData actionData)
        {
            _isBurn = true;
            _burnParticle.Play();
            GetComponent<MeshRenderer>().material = _burnMaterial;
            GetComponent<Collider>().excludeLayers = 0;
        }

        public void TakeHeal(float amount) { }
        public void Dead() { }

        private void OnTriggerEnter(Collider other) {
            if(!_isBurn) return;

            if(((1 << other.gameObject.layer) & _whatIsTarget.value) > 0)
            {
                if(other.TryGetComponent(out IHealth health))
                {
                    ActionData actionData = new ActionData(transform.position, Vector3.up, 1f, false);
                    health.TakeDamage(actionData);
                }
            }
        }
    }
}
