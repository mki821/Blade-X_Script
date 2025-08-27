using Swift_Blade.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace Swift_Blade.Combat.Caster
{
    public class SquareCaster : LayerCaster, ICasterAble
    {
        [SerializeField] private Vector3 _casterHalfSize;

        [Space(20)] public bool CanCurrentAttackParry = true;
        [Space(10)] public UnityEvent parryEvents;
        public UnityEvent unParriableAttack;

        public AudioCollectionSO fireSound;
        
        protected const float parryInterval = 0.5f;
        protected float lastParryTime;

        public Vector2 CasterSize2D { get => new Vector2(_casterHalfSize.x * 2.0f, _casterHalfSize.z * 2.0f); }

        private void Start()
        {
            OnCastEvent.AddListener(PlayAttackSound);
        }

        private void PlayAttackSound()
        {
            AudioManager.PlayWithInit(fireSound, true);
        }
        
        private void OnDestroy()
        {
            OnCastEvent.RemoveListener(PlayAttackSound);
        }
        
        public override bool Cast()
        {
            if (IsNotObstacleLine() == false)
            {
                return false;
            }

            OnCastEvent?.Invoke();
            
            Vector3 startPos = GetStartPosition();

            bool isHit = Physics.BoxCast(startPos, _casterHalfSize, transform.forward, out RaycastHit hit, transform.rotation, _castingRange, whatIsTarget);
            
            if (isHit && hit.collider.TryGetComponent(out IHealth health))
            {
                ActionData actionData = new ActionData(hit.point, hit.normal, 1, true);
                
                if (CanCurrentAttackParry && hit.collider.TryGetComponent(out PlayerParryController parryController))
                {
                    TryParry(hit, parryController, health, actionData);
                }
                else
                {
                    ApplyDamage(health, actionData);
                }
            }

            CanCurrentAttackParry = true;

            return isHit;
        }

        private void TryParry(RaycastHit hit, PlayerParryController parryController, IHealth health, ActionData actionData)
        {
            bool isLookingAtAttacker = IsFacingEachOther(hit.transform.GetComponentInParent<Player>().GetPlayerTransform , transform);
            bool canInterval = Time.time > lastParryTime + parryInterval;
            
            if (parryController.GetParry() && isLookingAtAttacker && canInterval)
            {
                parryEvents?.Invoke();//적 쪽
                parryController.ParryEvents?.Invoke();
                
                lastParryTime = Time.time;
            }
            else
            {
                ApplyDamage(health, actionData);
            }
        }
        
        protected bool IsFacingEachOther(Transform player, Transform enemy)
        {
            Vector3 playerToEnemy = (enemy.position - player.position).normalized;
            
            float playerDot = Vector3.Dot(player.forward, playerToEnemy);
            
            return playerDot > 0;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetStartPosition(), _casterHalfSize * 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(GetStartPosition() + transform.forward * _castingRange, _casterHalfSize * 2f);
            Gizmos.color = Color.white;
        }
        #endif
    }
}