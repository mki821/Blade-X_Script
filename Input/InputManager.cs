using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Swift_Blade.Inputs
{
    public enum InputType
    {
        Movement_Forward, Movement_Left, Movement_Back, Movement_Right, Roll, Parry, Inventory, ChangeQuick, UseQuick, Attack1, Attack2, H, P, Esc, Skill1, Skill2
    }

    [MonoSingletonUsage(MonoSingletonFlags.DontDestroyOnLoad)]
    [DefaultExecutionOrder(-200)]
    public class InputManager : MonoSingleton<InputManager>
    {
        public static event Action RebindEndEvent;

        public static event Action RollEvent;
        public static event Action ParryEvent;
        public static event Action InventoryEvent;
        public static event Action ChangeQuickEvent;
        public static event Action UseQuickEvent;
        public static event Action Attack1Event;
        public static event Action Attack2Event;
        public static event Action HEvent;
        public static event Action PEvent;
        public static event Action EscEvent;
        public static event Action Skill1Event;
        public static event Action Skill2Event;

        public static Dictionary<Action, bool> _skillBoundDictionary = new Dictionary<Action, bool>();

        [SerializeField] private CustomInputSO _input;

        public bool UseAttackToMouseDirection { get; set; } = true;
        public Vector2 InputDirection => _input.Movement;
        public Vector3 InputDirectionVector3 => new Vector3(InputDirection.x, 0, InputDirection.y);
        public Vector2 MousePos => _input.MousePosition;
        public Vector3 MousePosWorld
        {
            get
            {
                return Player.MousePosition.position;
            }
        }

        public void SetStopMing(bool isTurnOn)
        {
            if (_input == null)
            {
                Debug.LogWarning("[InputManager] InputSO is null.");
                return;
            }

            if(isTurnOn)
            {
                _input.CustomInput.Player.Enable();
            }
            else
            {
                _input.CustomInput.Player.Disable();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (_input == null)
                Debug.LogWarning("[InputManager] InputSO is null.");

            CustomInputSO.RollEvent += HandleRoll;
            CustomInputSO.ParryEvent += HandleParry;
            CustomInputSO.InventoryEvent += HandleInventory;
            CustomInputSO.ChangeQuickEvent += HandleChangeQuick;
            CustomInputSO.UseQuickEvent += HandleUseQuick;
            CustomInputSO.Attack1Event += HandleAttack1;
            CustomInputSO.Attack2Event += HandleAttack2;
            CustomInputSO.HEvent += HandleH;
            CustomInputSO.PEvent += HandleP;
            CustomInputSO.EscEvent += HandleEsc;
            CustomInputSO.Skill1Event += HandleSkill1;
            CustomInputSO.Skill2Event += HandleSkill2;

            SceneManager.sceneLoaded += SceneLoaded;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            CustomInputSO.RollEvent -= HandleRoll;
            CustomInputSO.ParryEvent -= HandleParry;
            CustomInputSO.InventoryEvent -= HandleInventory;
            CustomInputSO.ChangeQuickEvent -= HandleChangeQuick;
            CustomInputSO.UseQuickEvent -= HandleUseQuick;
            CustomInputSO.Attack1Event -= HandleAttack1;
            CustomInputSO.Attack2Event -= HandleAttack2;
            CustomInputSO.HEvent -= HandleH;
            CustomInputSO.PEvent -= HandleP;
            CustomInputSO.EscEvent -= HandleEsc;
            CustomInputSO.Skill1Event -= HandleSkill1;
            CustomInputSO.Skill2Event -= HandleSkill2;
        }

        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Skill1Event = null;
            Skill2Event = null;
            _skillBoundDictionary.Clear();
        }

        public void Rebind(InputType type, bool mouseEnable = true)
        {
            _input.CustomInput.Player.Disable();

            InputAction inputAction = InputTypeToInputAction(type);
            InputActionRebindingExtensions.RebindingOperation operation = inputAction.PerformInteractiveRebinding();

            if ((int)type < 4)
                operation.WithTargetBinding((int)type + 1).Start();

            if (mouseEnable)
                operation.WithControlsExcluding("Mouse");

            operation.WithCancelingThrough("<keyboard>/escape")
                .OnComplete(op =>
                {
                    op.Dispose();
                    RebindEndEvent?.Invoke();
                    _input.CustomInput.Player.Enable();
                }).OnCancel(op =>
                {
                    op.Dispose();
                    _input.CustomInput.Player.Enable();
                }).Start();
        }

        public string GetCurrentKeyByType(InputType type)
        {
            InputAction inputAction = InputTypeToInputAction(type);

            if ((int)type < 4)
                return inputAction.GetBindingDisplayString((int)type + 1);

            return inputAction.GetBindingDisplayString();
        }

        private InputAction InputTypeToInputAction(InputType type)
        {
            return type switch
            {
                InputType.Movement_Forward or InputType.Movement_Left or InputType.Movement_Back or InputType.Movement_Right => _input.CustomInput.Player.Movement,
                InputType.Roll => _input.CustomInput.Player.Roll,
                InputType.Parry => _input.CustomInput.Player.Parry,
                InputType.Inventory => _input.CustomInput.Player.Inventory,
                InputType.ChangeQuick => _input.CustomInput.Player.ChangeQuick,
                InputType.UseQuick => _input.CustomInput.Player.UseQuick,
                InputType.Attack1 => _input.CustomInput.Player.Attack1,
                InputType.Attack2 => _input.CustomInput.Player.Attack2,
                InputType.H => _input.CustomInput.Player.H,
                InputType.P => _input.CustomInput.Player.P,
                InputType.Esc => _input.CustomInput.Player.Esc,
                InputType.Skill1 => _input.CustomInput.Player.Skill1,
                InputType.Skill2 => _input.CustomInput.Player.Skill2,
                _ => default,
            };
        }

        public static void BindSkill(Action action, bool bind = true)
        {
            if (bind)
            {
                if (_skillBoundDictionary.Count == 0)
                {
                    Skill1Event += action;
                    _skillBoundDictionary.Add(action, true);
                }
                else
                {
                    Skill2Event += action;
                    _skillBoundDictionary.Add(action, false);
                }
            }
            else
            {
                if (_skillBoundDictionary.TryGetValue(action, out bool dir))
                {
                    if (dir)
                    {
                        Skill1Event -= action;
                    }
                    else
                    {
                        Skill2Event -= action;
                    }
                    _skillBoundDictionary.Remove(action);
                }
            }
        }

        #region Handle

        private void HandleRoll()
        {
            if (PopupManager.Instance.IsRemainPopup)
                return;

            RollEvent?.Invoke();
        }

        private void HandleParry()
        {
            if (PopupManager.Instance.IsRemainPopup)
                return;

            ParryEvent?.Invoke();
        }

        private void HandleInventory()
        {
            InventoryEvent?.Invoke();
        }

        private void HandleChangeQuick()
        {
            ChangeQuickEvent?.Invoke();
        }

        private void HandleUseQuick()
        {
            if (PopupManager.Instance.IsRemainPopup)
                return;

            UseQuickEvent?.Invoke();
        }

        private void HandleAttack1()
        {
            if (DialogueManager.Instance != null
                && DialogueManager.Instance.IsDialogueOpen)
                return;

            if (PopupManager.Instance != null
                && PopupManager.Instance.IsRemainPopup)
                return;

            Attack1Event?.Invoke();
        }

        private void HandleAttack2()
        {
            if (DialogueManager.Instance != null
                && DialogueManager.Instance.IsDialogueOpen)
                return;

            if (PopupManager.Instance != null
                && PopupManager.Instance.IsRemainPopup)
                return;

            Attack2Event?.Invoke();
        }

        private void HandleH()
        {
            if (PopupManager.Instance.IsRemainPopup)
                return;

            HEvent?.Invoke();
        }

        private void HandleP()
        {
            PEvent?.Invoke();
        }

        private void HandleEsc()
        {
            if (PopupManager.Instance.IsRemainPopup)
                return;

            EscEvent?.Invoke();
        }

        private void HandleSkill1()
        {
            if (DialogueManager.Instance != null
                && DialogueManager.Instance.IsDialogueOpen)
                return;

            if (PopupManager.Instance != null
                && PopupManager.Instance.IsRemainPopup)
                return;

            Skill1Event?.Invoke();
        }

        private void HandleSkill2()
        {
            if (DialogueManager.Instance != null
                && DialogueManager.Instance.IsDialogueOpen)
                return;

            if (PopupManager.Instance != null
                && PopupManager.Instance.IsRemainPopup)
                return;

            Skill2Event?.Invoke();
        }

        #endregion
    }
}
