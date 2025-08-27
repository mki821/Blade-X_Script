using UnityEngine;

namespace Swift_Blade.Combat.Caster
{
    public class BaseEnemyKnockbackCaster : BaseEnemyCaster
    {
        [SerializeField] private float _knockbackPower = 3200f;

        public override bool Cast()
        {
            if (IsNotObstacleLine() == false)
            {
                return false;
            }
            
            OnCastEvent?.Invoke();
            
            Vector3 startPos = GetStartPosition();
            
            bool isHit = Physics.SphereCast(
                startPos,
                _casterRadius,
                transform.forward,
                out RaycastHit hit,
                _castingRange, whatIsTarget);
            
            if (isHit && hit.collider.TryGetComponent(out IHealth health))
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

            CanCurrentAttackParry = true;

            return isHit;
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
                return;
            }
            
            ApplyDamage(health, actionData);
        }
    }
}
