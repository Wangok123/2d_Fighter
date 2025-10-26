using Photon.Deterministic;
using Quantum.QuantumView.Base;

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
        
        private void PollInput(CallbackPollInput callback)
        {
            callback.SetInput(_input, DeterministicInputFlags.Repeatable);
        }
    }
}