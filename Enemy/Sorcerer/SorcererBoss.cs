using UnityEngine;
using Swift_Blade.FSM.States;

namespace Swift_Blade.Enemy.Boss
{
    public class SorcererBoss : BaseEnemy
    {
        [SerializeField] private float _enragePercent = 0.3f;

        protected override void Start()
        {
            base.Start();

            baseHealth.OnChangeHealthEvent += HandleChangeHealth;
        }

        private void HandleChangeHealth(float percent)
        {
            if (percent <= _enragePercent)
            {
                ((SorcererAnimationController)baseAnimationController).SetEnrage();
                baseHealth.OnChangeHealthEvent -= HandleChangeHealth;
                btAgent.SetVariableValue("Enrage", true);
            }
        }

        public void ResetParry()
        {
            BasePlayerState.parryable = true;
        }
    }
}
