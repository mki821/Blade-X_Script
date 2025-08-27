using System.Collections;
using UnityEngine;
using Swift_Blade.Combat.Health;
using Swift_Blade.Pool;
using Swift_Blade.Enemy;
using Swift_Blade.Feeling;

namespace Swift_Blade
{
    public class BerserkerTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [Range(0.1f, 10)][SerializeField] private float _attackIncreaseValue;
        [Range(0.1f, 10)][SerializeField] private float _attackSpeedIncreaseValue;
        [Range(0.1f, 10)][SerializeField] private float _moveSpeedIncreaseValue;
        [SerializeField] private PoolPrefabMonoBehaviourSO _lastChance;

        [Header("Skill1")]
        [SerializeField] private float _skill1Radius;
        [SerializeField] private float _skill1Duration = 10.0f;
        [SerializeField] private LayerMask _whatIsEnemy;
        [SerializeField] private PoolPrefabMonoBehaviourSO _arrowUp;
        [SerializeField] private float _particleDelay = 0.1f;

        [Header("Skill2")]
        [SerializeField] private PoolPrefabMonoBehaviourSO _swirl;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        private bool _isActive = false;

        private Collider[] _colliders;
        private Collider[] _colliders2;
        private WaitForSeconds _wfs;

        private PlayerHealth _health;
        private PlayerStatCompo _stat;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _health = _player.GetEntityComponent<PlayerHealth>();
            _stat = _player.GetEntityComponent<PlayerStatCompo>();

            _colliders = new Collider[15];
            _colliders2 = new Collider[15];
            _wfs = new WaitForSeconds(_particleDelay);

            MonoGenericPool<ArrowUpParticle>.Initialize(_arrowUp);
            MonoGenericPool<SwirlParticle>.Initialize(_swirl);
            MonoGenericPool<LastChanceParticle>.Initialize(_lastChance);
        }

        protected override void TagEnableEffect()
        {
            _health.OnHitEvent.AddListener(HandleHit);
            _health.OnHealEvent.AddListener(HandleHeal);

            HandleHit(default);
        }

        protected override void TagDisableEffect()
        {
            _health.OnHitEvent.RemoveListener(HandleHit);
            _health.OnHealEvent.RemoveListener(HandleHeal);

            if (_isActive)
            {
                _stat.RemoveModifier(StatType.DAMAGE, "Berserker");
                _stat.RemoveModifier(StatType.ATTACKSPEED, "Berserker");
                _stat.RemoveModifier(StatType.MOVESPEED, "Berserker");

                _isActive = false;
            }
        }

        private void HandleHit(ActionData actionData)
        {
            if (_health.GetCurrentHealth == 1)
            {
                _isActive = true;

                _stat.AddModifier(StatType.DAMAGE, "Berserker", _attackIncreaseValue);
                _stat.AddModifier(StatType.ATTACKSPEED, "Berserker", _attackSpeedIncreaseValue);
                _stat.AddModifier(StatType.MOVESPEED, "Berserker", _moveSpeedIncreaseValue);

                LastChanceParticle lastChance = MonoGenericPool<LastChanceParticle>.Pop();
                lastChance.transform.SetParent(_player.GetPlayerTransform);
                lastChance.transform.localPosition = Vector3.up * 2.0f;
            }
        }

        private void HandleHeal()
        {
            if (!_isActive) return;

            _isActive = false;

            _stat.RemoveModifier(StatType.DAMAGE, "Berserker");
            _stat.RemoveModifier(StatType.ATTACKSPEED, "Berserker");
            _stat.RemoveModifier(StatType.MOVESPEED, "Berserker");
        }
        
        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            Vector3 playerPosition = _player.GetPlayerTransform.position;
            int cnt = Physics.OverlapSphereNonAlloc(playerPosition, _skill1Radius, _colliders, _whatIsEnemy);

            float buffAmount = cnt * 10.0f;
            _stat.BuffToStat(StatType.DAMAGE, "Berserker_Skill1", _skill1Duration, buffAmount);
            _stat.BuffToStat(StatType.ATTACKSPEED, "Berserker_Skill1", _skill1Duration, buffAmount);
            _stat.BuffToStat(StatType.MOVESPEED, "Berserker_Skill1", _skill1Duration, buffAmount);
            SetBuffAmount(Inputs.InputType.Skill1, _skill1Duration);

            _player.StartCoroutine(ArrowUpCoroutine(Mathf.FloorToInt(cnt * 1.5f)));
        }

        private IEnumerator ArrowUpCoroutine(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Vector3 position = _player.GetPlayerTransform.position + new Vector3((Random.value - 0.5f) * 1.5f, Random.Range(1.0f, 1.5f), (Random.value - 0.5f) * 1.5f);
                Quaternion rotation = Quaternion.Euler(-90.0f, Random.Range(0.0f, 360.0f), 0.0f);

                ArrowUpParticle arrowUpParticle = MonoGenericPool<ArrowUpParticle>.Pop();
                arrowUpParticle.transform.SetPositionAndRotation(position, rotation);
                arrowUpParticle.SetFollowTransform(_player.GetPlayerTransform);

                yield return _wfs;
            }
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            Vector3 playerPosition = _player.GetPlayerTransform.position;
            int cnt = Physics.OverlapSphereNonAlloc(playerPosition, _skill1Radius, _colliders2, _whatIsEnemy);

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            for (int i = 0; i < cnt; ++i)
            {
                if (_colliders2[i].TryGetComponent(out BaseEnemy enemy))
                {
                    Vector3 direction = (playerPosition - enemy.transform.position).normalized;
                    ActionData actionData = new ActionData
                    {
                        stun = true,
                        knockbackDirection = direction,
                        knockbackForce = 6.0f
                    };

                    enemy.StartKnockback(actionData);
                }
            }

            SwirlParticle swirl = MonoGenericPool<SwirlParticle>.Pop();
            swirl.transform.position = _player.GetPlayerTransform.position + new Vector3(0, 0.5f, 0);
        }
    }
}
