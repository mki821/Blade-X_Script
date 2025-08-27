using UnityEngine;

namespace Swift_Blade.Combat.Caster
{
    public class BaseEnemyKnockbackCasterAll : BaseEnemyCaster
    {
        [SerializeField] private float _knockbackPower = 3200f;
        
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
                        Vector3 knockbackDirection = hit.transform.position - transform.position;
                        knockbackDirection.y = 0f;
                        knockbackDirection.Normalize();

                        ActionData actionData = new ActionData(hit.point, hit.normal, 1, true, knockbackDirection, _knockbackPower);
                        
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

        protected override void TryParry(RaycastHit hit, PlayerParryController parryController, IHealth health, ActionData actionData)
        {
            bool isLookingAtAttacker = IsFacingEachOther(hit.transform.GetComponentInParent<Player>().GetPlayerTransform , transform);
            bool canInterval = Time.time > lastParryTime + parryInterval;
            
            if (parryController.GetParry() && isLookingAtAttacker && canInterval)
            {
                parryEvents?.Invoke();//적 쪽
                parryController.ParryEvents?.Invoke();

                lastParryTime = Time.time;

                actionData.damageAmount = 0f;
            }
            
            ApplyDamage(health, actionData);
        }
    }
}
