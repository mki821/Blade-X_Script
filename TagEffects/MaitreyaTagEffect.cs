using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swift_Blade.Combat.Health;
using Swift_Blade.Pool;

namespace Swift_Blade
{
    public class MaitreyaTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [SerializeField] private int _passivePercent;
        [SerializeField] private PoolPrefabMonoBehaviourSO _shield;

        [Header("Skill1")]
        [SerializeField] private float _shieldDuration;

        [Header("Skill2")]
        [SerializeField] private float _skill2Duration;
        [SerializeField] private float _damageMultiplier = 10.0f;
        [SerializeField] private PoolPrefabMonoBehaviourSO _lotus;
        [SerializeField] private PoolPrefabMonoBehaviourSO _lotusParticle;
        [SerializeField] private PoolPrefabMonoBehaviourSO _blossomParticle;

        [SerializeField] private AudioClip _clip;

        private int _hitCount;

        private WaitForSeconds _wfs;

        private List<LotusParticle> _lotusList;

        private PlayerHealth _health;
        private PlayerStatCompo _stat;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _wfs = new WaitForSeconds(_skill2Duration);

            _health = _player.GetEntityComponent<PlayerHealth>();
            _stat = _player.GetEntityComponent<PlayerStatCompo>();

            _lotusList = new List<LotusParticle>();

            MonoGenericPool<ShieldParticle>.Initialize(_shield);
            MonoGenericPool<Lotus>.Initialize(_lotus);
            MonoGenericPool<LotusParticle>.Initialize(_lotusParticle);
            MonoGenericPool<BlossomParticle>.Initialize(_blossomParticle);
        }

        protected override void TagEnableEffect()
        {
            _player.GetSkillController.skillEvents[SkillType.Hit] += HandleHit;
        }

        protected override void TagDisableEffect()
        {
            _player.GetSkillController.skillEvents[SkillType.Hit] -= HandleHit;
        }

        private void HandleHit(Player player, IEnumerable<Transform> targets)
        {
            if (Random.Range(0, 100) < _passivePercent)
            {
                ShieldParticle shield = MonoGenericPool<ShieldParticle>.Pop();
                shield.transform.position = _player.GetPlayerTransform.position + Vector3.up * 2.0f;

                _health.TakeHeal(1.0f);
            }
        }

        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;

            if (_health.GetCurrentHealth > 1)
            {
                _health.DecreaseHealth(1);
                _player.GetSkillController.UseSkill(SkillType.Shield);
                _stat.BuffToStat(StatType.HEALTH, "Maitreya", _shieldDuration, 2f);
            }
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            _player.StartCoroutine(Skill2Coroutine());
        }

        private IEnumerator Skill2Coroutine()
        {
            _hitCount = 0;

            _player.GetSkillController.skillEvents[SkillType.Hit] += HandleHitCharge;

            LotusParticle lotusParticle = MonoGenericPool<LotusParticle>.Pop();
            lotusParticle.transform.SetParent(_player.GetPlayerTransform.parent);
            lotusParticle.transform.localPosition = Vector3.down * 9.0f;
            lotusParticle.transform.localScale = new Vector3(9.0f, 5.0f, 9.0f);

            _lotusList.Add(lotusParticle);

            BlossomParticle blossom = MonoGenericPool<BlossomParticle>.Pop();
            blossom.transform.SetParent(_player.GetPlayerTransform.parent);
            blossom.transform.localPosition = Vector3.zero;
            Player.Instance.matSkill2Source.PlayOneShot(_clip);

            yield return _wfs;

            _player.GetSkillController.skillEvents[SkillType.Hit] -= HandleHitCharge;

            _health.TakeHeal(_hitCount);

            for (int i = 0; i < _lotusList.Count; ++i)
            {
                MonoGenericPool<LotusParticle>.Push(_lotusList[i]);
            }
            _lotusList.Clear();

            Lotus lotus = MonoGenericPool<Lotus>.Pop();
            lotus.transform.position = _player.GetPlayerTransform.position;
            lotus.Cast((_hitCount + 1) * _damageMultiplier);
        }

        private void HandleHitCharge(Player player, IEnumerable<Transform> targets)
        {
            ++_hitCount;

            LotusParticle lotus = MonoGenericPool<LotusParticle>.Pop();
           lotus.transform.SetParent(_player.GetPlayerTransform.parent);

            Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(1.0f, 0.8f), Random.Range(-0.4f, 0.4f));
            lotus.transform.localPosition = offset;

            _lotusList.Add(lotus);
        }
    }
}