using UnityEngine;
using Swift_Blade.Pool;
using Swift_Blade.Inputs;
using System.Collections;
using Swift_Blade.Audio;
using Swift_Blade.Feeling;

namespace Swift_Blade
{
    public class KnightTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [SerializeField] private float _passiveDamage;
        [SerializeField] private PoolPrefabMonoBehaviourSO _swordAura;
        [SerializeField] private Vector3 _swordAuraSpawnOffset;

        [Header("Skill1")]
        [SerializeField] private float _skill1Duration;
        [SerializeField] private AudioSO _skill1Audio;
        
        [Header("Skill2")]
        [SerializeField] private PoolPrefabMonoBehaviourSO _hugeSword;
        [SerializeField] private PoolPrefabMonoBehaviourSO _hugeSwordExplosion;
        [SerializeField] private PoolPrefabMonoBehaviourSO _groundCrack;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        private bool _isUpgrade = false;

        private WaitForSeconds _wfs;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _wfs = new WaitForSeconds(_skill1Duration);

            MonoGenericPool<SwordAura>.Initialize(_swordAura);
            MonoGenericPool<HugeSword>.Initialize(_hugeSword);
            MonoGenericPool<HugeSwordExplosion>.Initialize(_hugeSwordExplosion);
            MonoGenericPool<GroundCrackParticle>.Initialize(_groundCrack);
        }

        protected override void TagEnableEffect()
        {
            _player.GetPlayerDamageCaster.OnCastEvent.AddListener(HandleAttack);
            
            _isUpgrade = false;
        }

        protected override void TagDisableEffect()
        {
            _player.GetPlayerDamageCaster.OnCastEvent.RemoveListener(HandleAttack);
        }

        private void HandleAttack()
        {
            AudioManager.PlayWithInit(_skill1Audio , true);
            
            SwordAura swordAura = MonoGenericPool<SwordAura>.Pop();

            Transform playerTransform = _player.transform.GetChild(0);
            Vector3 spawnPosition = playerTransform.position + _swordAuraSpawnOffset;
            Quaternion spawnRotation = Quaternion.Euler(0.0f, playerTransform.GetChild(0).eulerAngles.y, 90.0f);

            swordAura.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            if (_isUpgrade)
            {
                swordAura.transform.localScale = Vector3.one;
                swordAura.Initilize(_passiveDamage * 2.0f);
            }
            else
            {
                swordAura.transform.localScale = Vector3.one * 0.4f;
                swordAura.Initilize(_passiveDamage);
            }
        }

        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);

            _player.StartCoroutine(Skill1Coroutine());
        }

        private IEnumerator Skill1Coroutine()
        {
            _isUpgrade = true;

            Player.Instance.kinghtSkill1Source.time = 0.5f;
            Player.Instance.kinghtSkill1Source.Play();
            yield return _wfs;

            _isUpgrade = false;
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            Vector3 playerPosition = _player.GetPlayerTransform.position;
            Vector3 mousePosition = InputManager.Instance.MousePosWorld;

            Invoke(nameof(Skill2Sound), .5f);

            HugeSword sword = MonoGenericPool<HugeSword>.Pop();
            sword.Cast(playerPosition + Vector3.up * 30.0f, mousePosition);
        }

        private void Skill2Sound()
        {
            Player.Instance.knightSkill2Source?.Play();
        }
    }
}
