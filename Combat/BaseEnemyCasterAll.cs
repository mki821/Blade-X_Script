using UnityEngine;

namespace Swift_Blade.Combat.Caster
{
    public class BaseEnemyCasterAll : BaseEnemyCaster
    {
        [SerializeField] private int _maxCastCount = 10;
        
        private RaycastHit[] _hits;

        private void Awake()
        {
            _hits = new RaycastHit[_maxCastCount];
        }

        public override bool Cast()
        {
            if (IsNotObstacleLine() == false)
            {
                return false;
            }
            
            OnCastEvent?.Invoke();
            
            Vector3 startPos = GetStartPosition();
            
            int cnt = Physics.SphereCastNonAlloc(startPos, _casterRadius, transform.forward, _hits, _castingRange, whatIsTarget);
            
            if (cnt > 0)
            {
                for(int i = 0; i < cnt; ++i)
                {
                    RaycastHit hit = _hits[i];

                    if(hit.collider.TryGetComponent(out IHealth health))
                    {
                        ActionData actionData = new ActionData(hit.point, hit.normal, 1, true);
                        
                        if (CanCurrentAttackParry && hit.collider.TryGetComponent(out PlayerParryController parryController))
                        {
                            TryParry(hit, parryController, health, actionData);
                        }
                        else
                        {
                            ApplyDamage(health,actionData);
                        }
                    }
                }
            }

            CanCurrentAttackParry = true;

            return cnt > 0;
        }
    }
}
