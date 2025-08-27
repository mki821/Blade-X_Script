using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Swift_Blade.Combat.Health;
using Swift_Blade.Pool;
using Swift_Blade.Feeling;

namespace Swift_Blade
{
    public class BeastTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [SerializeField] private float _passiveAttackSpeedModifier = 1.0f;
        [SerializeField] private float _passiveMoveSpeedModifier = -5.0f;

        [SerializeField] private float _passiveDamage;
        [SerializeField] private PoolPrefabMonoBehaviourSO _impactDirt;

        [SerializeField] private float _activeSkillDuration = 5.0f;

        [Header("Skill1")]
        [SerializeField] private PoolPrefabMonoBehaviourSO _redWave;
        [SerializeField] private PoolPrefabMonoBehaviourSO _redCircle;

        [Header("Skill2")]
        [SerializeField] private float _attackSpeedModifier = 1.0f;
        [SerializeField] private float _damageModifier = -5.0f;
        [SerializeField] private PoolPrefabMonoBehaviourSO _circleWind;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        [Header("Sound Clips")]
        [SerializeField] private AudioClip _skill1Clip;
        [SerializeField] private AudioClip _skill2Clip;

        private WaitForSeconds _wfs;

        private PlayerHealth _health;
        private PlayerStatCompo _stat;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _wfs = new WaitForSeconds(_activeSkillDuration);

            _health = _player.GetEntityComponent<PlayerHealth>();
            _stat = _player.GetEntityComponent<PlayerStatCompo>();

            MonoGenericPool<ImpactDirtParticle>.Initialize(_impactDirt);
            MonoGenericPool<RedWaveParticle>.Initialize(_redWave);
            MonoGenericPool<RedCircleParticle>.Initialize(_redCircle);
            MonoGenericPool<CircleWindParticle>.Initialize(_circleWind);
        }

        protected override void TagEnableEffect()
        {
            _stat.AddModifier(StatType.ATTACKSPEED, "Beast", _passiveAttackSpeedModifier);
            _stat.AddModifier(StatType.MOVESPEED, "Beast", _passiveMoveSpeedModifier);

            _player.GetSkillController.skillEvents[SkillType.Attack] += HandleAttack;
        }

        protected override void TagDisableEffect()
        {
            _stat.RemoveModifier(StatType.ATTACKSPEED, "Beast");
            _stat.RemoveModifier(StatType.MOVESPEED, "Beast");

            _player.GetSkillController.skillEvents[SkillType.Attack] -= HandleAttack;
        }

        private void HandleAttack(Player player, IEnumerable<Transform> targets)
        {
            if (targets == null || !targets.Any()) return;
            if (player.GetPlayerHealth.IsFullHealth == false) return;

            MonoGenericPool<ImpactDirtParticle>.Pop().transform.position = targets.First().transform.position + new Vector3(0, 1, 0);

            foreach (var item in targets)
            {
                if (item.TryGetComponent(out BaseEnemyHealth enemyHealth))
                {
                    ActionData actionData = new ActionData
                    {
                        damageAmount = _passiveDamage,
                        stun = true
                    };

                    enemyHealth.TakeDamage(actionData);
                }
            }
        }

        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            RedCircleParticle redCircleParticle = MonoGenericPool<RedCircleParticle>.Pop();
            redCircleParticle.transform.SetParent(_player.GetPlayerTransform);
            redCircleParticle.transform.position = _player.GetPlayerTransform.position + new Vector3(0, 0.5f, 0);

            _player.StartCoroutine(Skill1Coroutine());
        }

        private IEnumerator Skill1Coroutine()
        {
            RedWaveParticle redWaveParticle = MonoGenericPool<RedWaveParticle>.Pop();
            redWaveParticle.transform.position = _player.GetPlayerTransform.position;

            _player.GetSkillController.skillEvents[SkillType.Attack] += Drain;
            SetBuffAmount(Inputs.InputType.Skill1, _activeSkillDuration);

            Player.Instance.beastSkill1Source.PlayOneShot(_skill1Clip);

            yield return _wfs;

            _player.GetSkillController.skillEvents[SkillType.Attack] -= Drain;
        }
        
        private void Drain(Player player, IEnumerable<Transform> targets)
        {
            if (targets.Any())
            {
                foreach (var item in targets)
                {
                    if (item.TryGetComponent(out BaseEnemyHealth health) && health.isDead)
                    {
                        RedCircleParticle redCircleParticle = MonoGenericPool<RedCircleParticle>.Pop();
                        redCircleParticle.transform.SetParent(_player.GetPlayerTransform);
                        redCircleParticle.transform.position = _player.GetPlayerTransform.position + new Vector3(0, 0.5f, 0);

                        _health.TakeHeal(1.0f);
                    }
                }
            }
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            _player.StartCoroutine(Skill2Coroutine());
        }

        private IEnumerator Skill2Coroutine()
        {
            _health.IsPlayerInvincible = true;
            _stat.BuffToStat(StatType.ATTACKSPEED, "BeastBuff", _activeSkillDuration, _attackSpeedModifier);
            _stat.BuffToStat(StatType.DAMAGE, "BeastBuff", _activeSkillDuration, _damageModifier);

            CircleWindParticle circleWind = MonoGenericPool<CircleWindParticle>.Pop();
            circleWind.transform.position = _player.GetPlayerTransform.position;

            Player.Instance.beastSkill2Source.PlayOneShot(_skill2Clip);
            SetBuffAmount(Inputs.InputType.Skill2, _activeSkillDuration);
            yield return _wfs;
            
            _health.IsPlayerInvincible = false;
        }
    }
}
