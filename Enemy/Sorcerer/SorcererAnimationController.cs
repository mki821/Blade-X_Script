using UnityEngine;
using Swift_Blade.Combat.Caster;
using Swift_Blade.Pool;

namespace Swift_Blade.Enemy.Boss
{
    public class SorcererAnimationController : BaseEnemyAnimationController
    {
        [Header("Flameshrower")]
        [SerializeField] private SquareCaster[] _flameshrowerCasters;
        
        [Header("Suicide Bomb")]
        [SerializeField] private BaseEnemyCaster _suicideCaster;

        [Header("Close Explosion")]
        [SerializeField] private PoolPrefabMonoBehaviourSO _closeExplosion;
        [SerializeField] private Transform _closeExplosionSpawnTrm;
        [SerializeField] private BaseEnemyCaster _closeExplosionCaster;
        
        [Header("Fire Arrow")]
        [SerializeField] private FireArrow _fireArrowPrefab;
        [SerializeField] private Transform _fireArrowSpawnTrm;

        [Header("Fire Projectile")]
        [SerializeField] private PoolPrefabMonoBehaviourSO _fireProjectile;
        [SerializeField] private Transform _fireProjectileSpawnTrm;

        [Header("Explosion")]
        [SerializeField] private FireWall _fireWall;
        [SerializeField] private PoolPrefabMonoBehaviourSO _explosion;
        [SerializeField] private Transform _explosionSpawnTrm;
        [SerializeField] private BaseEnemyCaster _explosionCaster;

        [Header("Attack Range Visualize")]
        [SerializeField] private AttackRangeVisualizer _attackRangeVisualizer;
        [SerializeField] private float _duration = 0.5f;

        [Header("Enrage")]
        [SerializeField] private float _enrageAttackSpeed = 1.2f;

        private bool _isEnrage = false;
        
        protected void Start()
        {
            MonoGenericPool<CloseExplosionParticle>.Initialize(_closeExplosion);
            MonoGenericPool<FireProjectile>.Initialize(_fireProjectile);
            MonoGenericPool<ExplosionParticle>.Initialize(_explosion);
        }

        public void SetEnrage()
        {
            _isEnrage = true;

            Animator.SetBool("Enrage", _isEnrage);
            Animator.SetFloat("AttackSpeed", _enrageAttackSpeed);

            _closeExplosionCaster.SetCastInfo(3.75f, 3.75f, 7.5f);

        }

        private void AttackRangeVisualFlameShrower(int index)
        {
            SquareCaster temp = _flameshrowerCasters[index];

            _attackRangeVisualizer.SetRangeType(AttackRangeType.Square);

            Vector3 spawnPosition = temp.transform.position;
            spawnPosition.y = 0.0f;
            _attackRangeVisualizer.SetAttackRange(temp.CasterSize2D, spawnPosition, _duration, true, false);
        }

        private void CastFlameshrower(int index) => _flameshrowerCasters[index].Cast();
        private void SuicideBomb() => _suicideCaster.Cast();

        private void SpawnCloseExplosionEffect()
        {
            ExplosionParticle particle = MonoGenericPool<ExplosionParticle>.Pop();
            particle.transform.position = _closeExplosionSpawnTrm.position;
            particle.transform.localScale = Vector3.one * (_isEnrage ? 1.95f : 1.3f);

            _closeExplosionCaster.Cast();
        }

        private void SpawnFireArrow()
        {
            Instantiate(_fireArrowPrefab, _fireArrowSpawnTrm.position, _fireArrowSpawnTrm.rotation);
        }

        private void SpawnFireProjectile()
        {
            for(int i = 0; i < 10; ++i)
            {
                FireProjectile projectile = MonoGenericPool<FireProjectile>.Pop();
                projectile.transform.position = _fireProjectileSpawnTrm.position;

                projectile.SetAngle(36f * i);
            }
        }

        private void SetFireWall() => _fireWall.gameObject.SetActive(true);

        private void SpawnExplosionEffect()
        {
            ExplosionParticle particle = MonoGenericPool<ExplosionParticle>.Pop();
            particle.transform.position = _explosionSpawnTrm.position;
            particle.transform.localScale = Vector3.one * 2.3f;

            _explosionCaster.Cast();
        }
    }
}
