using System;
using UnityCore.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityCore.Input.Module
{
    public class PlayerInputModule : IInputModule
    {
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputComponent InputComponent => Game.Input;

        public event Action<Vector2> onMove;
        public event Action onJump;

        public void Initialize()
        {
            var playerMap = InputComponent.GetActionMap("Player");
            moveAction = playerMap.FindAction("Movement");
            jumpAction = playerMap.FindAction("Jump");

            moveAction.performed += OnMovement;
            moveAction.canceled += OnCancelMovement;

            jumpAction.performed += OnJump;
        }

        public void Cleanup()
        {
        }

        public void Enable()
        {
            moveAction.Enable();
            jumpAction.Enable();
        }

        public void Disable()
        {
            moveAction.Disable();
            jumpAction.Disable();
        }

        private void OnMovement(InputAction.CallbackContext context)
        {
            var moveInput = context.ReadValue<Vector2>();
            onMove?.Invoke(moveInput);
        }

        private void OnCancelMovement(InputAction.CallbackContext context)
        {
            var moveInput = Vector2.zero;
            onMove?.Invoke(moveInput);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            onJump?.Invoke();
        }
    }
}