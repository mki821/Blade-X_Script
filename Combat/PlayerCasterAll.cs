using UnityEngine;

namespace Swift_Blade.Combat.Caster
{
    public class PlayerCasterAll : LayerCaster
    {
        public float skillDamage = 10;

        [SerializeField] private int _maxCastCount = 10;
        
        private RaycastHit[] _hits;

        private Color damageTextColor;
        
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
                for (int i = 0; i < cnt; ++i)
                {
                    RaycastHit hit = _hits[i];

                    if (hit.collider.TryGetComponent(out IHealth health))
                    {
                        ActionData actionData = new ActionData(hit.collider.transform.localPosition, hit.normal, skillDamage,true)
                            {
                                textColor = damageTextColor
                            };
                        
                        ApplyDamage(health, actionData);
                        
                        SetDamageTextColor(Color.white);
                    }
                }
            }

            return cnt > 0;
        }

        public void SetDamageTextColor(Color color)
        {
            damageTextColor = color;
        }
        
    }
}
