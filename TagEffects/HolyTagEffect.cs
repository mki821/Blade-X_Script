using System.Collections;
using UnityEngine;
using Swift_Blade.Combat.Health;
using Swift_Blade.Pool;
using Swift_Blade.Feeling;

namespace Swift_Blade
{
    public class HolyTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [SerializeField] private float _damage;
        [SerializeField] private float _radius;
        [SerializeField] private LayerMask _whatIsEnemy;
        [SerializeField] private PoolPrefabMonoBehaviourSO _shockWave;

        [Header("Skill1")]
        [SerializeField] private float _shieldDuration = 30f;

        [Header("Skill2")]
        [SerializeField] private float _skillDamage;
        [SerializeField] private int _skillAmount = 10;
        [SerializeField] private float _skillDelay = 0.1f;
        [SerializeField] private PoolPrefabMonoBehaviourSO _holyThunder;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        [SerializeField] private AudioClip _clip;

        private Collider[] _colliders;
        private WaitForSeconds _wfs;

        private PlayerHealth _health;
        private PlayerStatCompo _stat;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _health = _player.GetEntityComponent<PlayerHealth>();
            _stat = _player.GetEntityComponent<PlayerStatCompo>();

            _colliders = new Collider[10];
            _wfs = new WaitForSeconds(_skillDelay);

            MonoGenericPool<ShockWaveParticle>.Initialize(_shockWave);
            MonoGenericPool<HolyThunder>.Initialize(_holyThunder);
        }

        protected override void TagEnableEffect()
        {
            _health.OnHitEvent.AddListener(HandleHit);
        }

        protected override void TagDisableEffect()
        {
            _health.OnHitEvent.RemoveListener(HandleHit);
        }

        private void HandleHit(ActionData _)
        {
            ActionData actionData = new ActionData
            {
                damageAmount = _damage
            };

            Vector3 playerPosition = _player.GetPlayerTransform.position;
            int cnt = Physics.OverlapSphereNonAlloc(playerPosition, _radius, _colliders, _whatIsEnemy);
            for (int i = 0; i < cnt; ++i)
            {
                if (_colliders[i].TryGetComponent(out IHealth health))
                {
                    actionData.hitPoint = _colliders[i].transform.position;
                    health.TakeDamage(actionData);
                }
            }

            ShockWaveParticle shockWave = MonoGenericPool<ShockWaveParticle>.Pop();
            shockWave.transform.position = playerPosition + new Vector3(0, 0.5f, 0);
        }

        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;

            _player.GetSkillController.UseSkill(SkillType.Shield);
            _stat.BuffToStat(StatType.HEALTH, $"Holy_{Time.time}", _shieldDuration, 2f);
            SetBuffAmount(Inputs.InputType.Skill1, 2f);

            CameraFocusManager.Instance.StartFocus(_focus);
            HitStopManager.Instance.StartHitStop(_hitStop);
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            SetBuffAmount(Inputs.InputType.Skill1, _skillDelay);
            _player.StartCoroutine(Skill2Coroutine());
        }

        private IEnumerator Skill2Coroutine()
        {
            Vector3 playerPosition = _player.transform.GetChild(0).position;

            for (int i = 0; i < _skillAmount; ++i)
            {
                Vector3 spawnPosition = playerPosition + new Vector3(Random.Range(-8.0f, 8.0f), 0.0f, Random.Range(-8.0f, 8.0f));
                
                HolyThunder holyThunder = MonoGenericPool<HolyThunder>.Pop();
                holyThunder.transform.position = spawnPosition;
                holyThunder.Cast(_skillDamage);

                HitStopManager.Instance.StartHitStop(_hitStop);
                CameraFocusManager.Instance.StartFocus(_focus);
                CameraShakeManager.Instance.DoShake(CameraShakeType.PlayerAttack);

                Player.Instance.holySkill1Source.PlayOneShot(_clip);

                yield return _wfs;
            }
        }
    }
}
