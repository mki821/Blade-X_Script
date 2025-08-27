using UnityEngine;
using DG.Tweening;
using Swift_Blade.Combat.Caster;
using Swift_Blade.Feeling;

namespace Swift_Blade.Pool
{
    public class Lotus : MonoBehaviour, IPoolable
    {
        private Sequence _sequence;

        private MeshRenderer _meshRenderer;
        private PlayerCasterAll _caster;

        private readonly int PercentHash = Shader.PropertyToID("_Percent");

        public void OnCreate()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _caster = GetComponent<PlayerCasterAll>();
        }

        public void OnPop()
        {
            transform.localScale = Vector3.zero;
            _meshRenderer.material.SetFloat(PercentHash, transform.position.y);

            _sequence = DOTween.Sequence();
        }

        public void Cast(float damage)
        {
            _caster.skillDamage = damage;
            _caster.Cast();

            CameraShakeManager.Instance.DoShake(CameraShakeType.Strong);

            _sequence.Append(transform.DOScale(Vector3.one, 0.5f));
            _sequence.Join(_meshRenderer.material.DOFloat(transform.position.y + 5.0f, PercentHash, 0.5f).OnComplete(() =>
            {
                _caster.Cast();
                CameraShakeManager.Instance.DoShake(CameraShakeType.Strong);
            }));
            _sequence.AppendInterval(0.5f);
            _sequence.Append(_meshRenderer.material.DOFloat(transform.position.y, PercentHash, 0.3f).OnComplete(() =>
            {
                MonoGenericPool<Lotus>.Push(this);
            }));
        }
    }
}
