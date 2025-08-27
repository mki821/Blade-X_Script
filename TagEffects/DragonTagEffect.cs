using UnityEngine;
using Swift_Blade.Pool;
using Swift_Blade.FSM;
using Swift_Blade.FSM.States;
using Swift_Blade.Feeling;

namespace Swift_Blade
{
    public class DragonTagEffect : TagEffectBase
    {
        [Header("Passive")]
        [SerializeField] private float _passiveDamage;
        [SerializeField] private PoolPrefabMonoBehaviourSO _dragonBall;

        [Header("Skill1")]
        [SerializeField] private float _skill1Damage;
        [SerializeField] private int _skill1Amount;
        [SerializeField] private PoolPrefabMonoBehaviourSO _dragonPulse;

        [Header("Hit effect")]
        [SerializeField] private CameraFocusSO _focus;
        [SerializeField] private HitStopSO _hitStop;

        private DragonBall[] _dragonBalls;

        private Transform _playerTransform;

        [Header("Skill2")]
        [SerializeField] private AnimationParameterSO _animationParamSO;

        public override void Initialize(Player player)
        {
            base.Initialize(player);

            _playerTransform = player.transform.GetChild(0);

            MonoGenericPool<DragonBall>.Initialize(_dragonBall);

            _dragonBalls = new DragonBall[3];
            for (int i = 0; i < 3; ++i)
            {
                DragonBall dragonBall = MonoGenericPool<DragonBall>.Pop();
                dragonBall.Initialize(_playerTransform, _passiveDamage, 120f * i);

                dragonBall.transform.SetParent(_playerTransform);
                dragonBall.transform.localPosition = Vector3.up;

                _dragonBalls[i] = dragonBall;
            }

            MonoGenericPool<DragonPulse>.Initialize(_dragonPulse);
        }

        protected override void TagEnableEffect()
        {
            for (int i = 0; i < 3; ++i)
            {
                _dragonBalls[i].Enable();
            }
        }

        protected override void TagDisableEffect()
        {
            for (int i = 0; i < 3; ++i)
            {
                _dragonBalls[i].Disable();
            }
        }

        public override void HandleSkill1()
        {
            if (!CanUseSkill1()) return;
            
            float angle = 360.0f / _skill1Amount;
            Vector3 spawnPosition = _player.transform.GetChild(0).position + Vector3.up * 0.3f;
            for (int i = 0; i < _skill1Amount; ++i)
            {
                float currentAngle = angle * i;
                DragonPulse dragonPulse = MonoGenericPool<DragonPulse>.Pop();

                dragonPulse.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0.0f, currentAngle, 0.0f));
                dragonPulse.SetDamage(_skill1Damage);
            }

            Player.Instance.dragonSkill1Source.Play();

            HitStopManager.Instance.StartHitStop(_hitStop);
            CameraFocusManager.Instance.StartFocus(_focus);
            CameraShakeManager.Instance.DoShake(CameraShakeType.PlayerParry);
        }

        public override void HandleSkill2()
        {
            if (!CanUseSkill2()) return;

            FiniteStateMachine<PlayerStateEnum> stateMachine = _player.GetStateMachine;
            PlayerCustomAnimatinoState customAnimationState = stateMachine.GetStateDictionary[PlayerStateEnum.Custom] as PlayerCustomAnimatinoState;
            customAnimationState.SetAnimationParam(_animationParamSO);
            customAnimationState.OnCustomAnimationEnd += static (PlayerCustomAnimatinoState state) => { state.D(false); };
            customAnimationState.OnCustomAnimationEnd += static (_) => { Player.Instance.dragonSkill2Source.Stop(); };
            stateMachine.ChangeState(PlayerStateEnum.Custom);

            Player.Instance.dragonSkill2Source.Play();
        }
    }
}
