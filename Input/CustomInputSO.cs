using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Swift_Blade.Inputs
{
    [CreateAssetMenu(fileName = "CustomInput", menuName = "SO/CustomInput")]
    public class CustomInputSO : ScriptableObject, CustomInput.IPlayerActions
    {
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

        public Vector2 Movement { get; private set; }
        public Vector2 MousePosition { get; private set; }

        private CustomInput _input;

        public CustomInput CustomInput => _input;

        private void OnEnable()
        {
            if(_input == null)
            {
                _input = new CustomInput();
                _input.Player.SetCallbacks(this);
            }

            _input.Player.Enable();
        }

        private void OnDisable()
        {
            _input.Player.SetCallbacks(null);
            _input.Player.Disable();
        }

        public void ResetInputs()
        {
            RollEvent = null;
            ParryEvent = null;
            InventoryEvent = null;
            ChangeQuickEvent = null;
            UseQuickEvent = null;
            Attack1Event = null;
            Attack2Event = null;
            Skill1Event = null;
            Skill2Event = null;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            Movement = context.ReadValue<Vector2>();
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            MousePosition = context.ReadValue<Vector2>();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if(context.performed) RollEvent?.Invoke();
        }

        public void OnParry(InputAction.CallbackContext context)
        {
            if(context.performed) ParryEvent?.Invoke();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if(context.performed) InventoryEvent?.Invoke();
        }

        public void OnChangeQuick(InputAction.CallbackContext context)
        {
            if(context.performed) ChangeQuickEvent?.Invoke();
        }

        public void OnUseQuick(InputAction.CallbackContext context)
        {
            if(context.performed) UseQuickEvent?.Invoke();
        }
        
        public void OnAttack1(InputAction.CallbackContext context)
        {
            if(context.performed) Attack1Event?.Invoke();
        }

        public void OnAttack2(InputAction.CallbackContext context)
        {
            if(context.performed) Attack2Event?.Invoke();
        }

        public void OnH(InputAction.CallbackContext context)
        {
            if(context.performed) HEvent?.Invoke();
        }

        public void OnP(InputAction.CallbackContext context)
        {
            if(context.performed) PEvent?.Invoke();
        }

        public void OnEsc(InputAction.CallbackContext context)
        {
            if(context.performed) EscEvent?.Invoke();
        }

        public void OnSkill1(InputAction.CallbackContext context)
        {
            if(context.performed) Skill1Event?.Invoke();
        }

        public void OnSkill2(InputAction.CallbackContext context)
        {
            if(context.performed) Skill2Event?.Invoke();
        }
    }
}
