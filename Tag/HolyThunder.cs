using UnityEngine;
using Swift_Blade.Pool;
using Swift_Blade.Combat.Caster;

namespace Swift_Blade
{
    public class HolyThunder : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _poolPushTime;
        public void OnPop()
        {
            Invoke(nameof(Push), _poolPushTime);
        }

        private void Push() => MonoGenericPool<HolyThunder>.Push(this);

        public void Cast(float skillDamage)
        {
            PlayerCasterAll caster = GetComponent<PlayerCasterAll>();
            caster.skillDamage = skillDamage;
            caster.Cast();
        }
    }
}
