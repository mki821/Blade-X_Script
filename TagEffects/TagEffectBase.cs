using UnityEngine;
using Swift_Blade.Inputs;

namespace Swift_Blade
{
    public abstract class TagEffectBase : MonoBehaviour
    {
        [SerializeField] private EquipmentTag _tagType;

        [SerializeField] private float _skill1Cooldown;
        [SerializeField] private float _skill2Cooldown;

        private float _skill1Timer;
        private float _skill2Timer;

        protected Player _player;
        protected PlayerTagCompo _tag;

        public float Skill1Cooldown => _skill1Cooldown;
        public float Skill2Cooldown => _skill2Cooldown;

        public virtual void Initialize(Player player)
        {
            _player = player;
            _tag = _player.GetEntityComponent<PlayerTagCompo>();
            _tag.updates.Add(HandleUpdate);
        }

        private void HandleUpdate()
        {
            if (_skill1Timer > 0.0f)
            {
                _skill1Timer -= Time.deltaTime;
            }

            if (_skill2Timer > 0.0f)
            {
                _skill2Timer -= Time.deltaTime;
            }
        }

        protected abstract void TagEnableEffect();
        protected abstract void TagDisableEffect();

        public virtual void TagEffect(int tagCount, bool isIncreasing)
        {
            if (isIncreasing)
            {
                switch (tagCount)
                {
                    case 1: TagEnableEffect(); break;
                    case 2: InputManager.BindSkill(HandleSkill1); break;
                    case 3: 
                        _tag.ActiveParticle(_tagType, true);
                        InputManager.BindSkill(HandleSkill2);
                        break;
                }
            }
            else
            {
                switch (tagCount)
                {
                    case 0: TagDisableEffect(); break;
                    case 1: InputManager.BindSkill(HandleSkill1, false); break;
                    case 2:
                        _tag.ActiveParticle(_tagType, false);
                        InputManager.BindSkill(HandleSkill2, false);
                        break;
                }
            }
        }

        public abstract void HandleSkill1();
        public abstract void HandleSkill2();

        protected bool CanUseSkill1()
        {
            if (_skill1Timer > 0.0f)
            {
                return false;
            }

            _skill1Timer = _skill1Cooldown;

            if (InputManager._skillBoundDictionary[HandleSkill1] == true)
            {
                TagInfoManager.Instance.SetCoolTime(InputType.Skill1, Skill1Cooldown);
            }
            else
            {
                TagInfoManager.Instance.SetCoolTime(InputType.Skill2, Skill2Cooldown);
            }

            return true;
        }

        protected bool CanUseSkill2()
        {
            if (_skill2Timer > 0.0f)
            {
                return false;
            }

            _skill2Timer = _skill2Cooldown;

            if (InputManager._skillBoundDictionary[HandleSkill2] == true)
            {
                TagInfoManager.Instance.SetCoolTime(InputType.Skill1, Skill1Cooldown);
            }
            else
            {
                TagInfoManager.Instance.SetCoolTime(InputType.Skill2, Skill2Cooldown);
            }

            return true;
        }

        protected void SetBuffAmount(InputType type, float duration)
        {
            //if (type == InputType.Skill1)
            //{
            //    if (InputManager._skillBoundDictionary[HandleSkill1] == true)
            //    {
            //        TagInfoManager.Instance.SetBuffAmount(InputType.Skill1, 5f);
            //    }
            //    else
            //    {
            //        TagInfoManager.Instance.SetBuffAmount(InputType.Skill2, 5f);
            //    }
            //}
            //else if (type == InputType.Skill2)
            //{
            //    if (InputManager._skillBoundDictionary[HandleSkill2] == true)
            //    {
            //        TagInfoManager.Instance.SetBuffAmount(InputType.Skill1, Skill1Cooldown);
            //    }
            //    else
            //    {
            //        TagInfoManager.Instance.SetBuffAmount(InputType.Skill2, Skill2Cooldown);
            //    }
            //}
        }
    }
}
