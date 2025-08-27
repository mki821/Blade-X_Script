using Swift_Blade.Combat.Caster;
using UnityEngine;

namespace Swift_Blade.Pool
{
    public class HugeSwordExplosion : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _poolPushTime;

        public void OnPop() { Invoke(nameof(Push), _poolPushTime); }

        private void Push() => MonoGenericPool<HugeSwordExplosion>.Push(this);

        public void Cast()
        {
            var cast = GetComponent<PlayerCasterAll>();
            cast.SetDamageTextColor(new Color(155/255f, 89/255f, 182f/255f,1));
            cast.Cast();
        }
    }
}
