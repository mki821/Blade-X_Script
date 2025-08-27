using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Swift_Blade
{
    public class SpecialCooldown : MonoBehaviour
    {
        [SerializeField] private Image _progress;
        [SerializeField] private TextMeshProUGUI _cooldownText;

        private static bool _isCooldown = false;
        private static float _cooldown = 0.0f;
        private static float _remainCooldown = 0.0f;

        private void Awake()
        {
            if(_isCooldown)
            {
                StartCoroutine(Coroutine());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetCooldown()
        {
            if(_isCooldown)
            {
                return;
            }

            gameObject.SetActive(true);

            _isCooldown = true;
            _cooldown = PlayerWeaponManager.CurrentWeapon.GetSpecialDelay;
            _remainCooldown = _cooldown;

            _cooldownText.text = Mathf.CeilToInt(_cooldown).ToString();
            _progress.fillAmount = 1.0f;

            StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            while(_remainCooldown > 0.0f)
            {
                _remainCooldown -= Time.deltaTime;

                _cooldownText.text = Mathf.CeilToInt(_remainCooldown).ToString();
                _progress.fillAmount = _remainCooldown / _cooldown;

                yield return null;
            }
            
            _progress.fillAmount = 0.0f;
            _isCooldown = false;
            
            gameObject.SetActive(false);
        }
    }
}
