using Photon.Deterministic;
using Quantum.QuantumView.Base;
using UnityEngine;

namespace Quantum.QuantumView.Input
{
    public class LocalQuantumInputPoller : QuantumEntityViewComponent<CustomViewContext>
    {
        private SimpleInput2D _input;
        private InputActions _inputActions;

        private void OnEnable()
        {
            // InputActions class is auto-generated from the InputSystem_Actions asset
            _inputActions ??= new InputActions();
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        public override void OnActivate(Frame frame)
        {
            var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);

            if (!Game.PlayerIsLocal(playerLink.Player))
            {
                enabled = false;
                return;
            }

            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback),
                onlyIfActiveAndEnabled: true);
        }

        public override void OnUpdateView()
        {
            UpdateLastDirectionInContext();
        }

        private void UpdateLastDirectionInContext()
        {
            var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);
            if (Game.PlayerIsLocal(playerLink.Player))
            {
                ViewContext.LocalCharacterLastDirection = GetAimDirection();
            }
        }

        private void PollInput(CallbackPollInput callback)
        {
            UpdateInputActions();
            callback.SetInput(_input, DeterministicInputFlags.Repeatable);
        }
        
        private void UpdateInputActions()
        {
            var moveValue = _inputActions.Player.Movement.ReadValue<Vector2>().ToFPVector2();
            _input.Up = moveValue.Y > 0;
            _input.Down = moveValue.Y < 0;
            _input.Left = moveValue.X < 0;
            _input.Right = moveValue.X > 0;

            _input.LP = _inputActions.Player.Light_Attack.IsPressed();
            _input.HP = _inputActions.Player.Heavy_Attack.IsPressed();
            _input.Jump = _inputActions.Player.Jump.IsPressed();
            _input.Dash = _inputActions.Player.Sprint.IsPressed();
            _input.Use = _inputActions.Player.Interact.IsPressed();
        }

        // 在预测帧里获得角色朝向
        private FPVector2 GetAimDirection()
        {
            Frame frame = PredictedFrame;
            
            
            
            return FPVector2.Zero;
        }
    }
}