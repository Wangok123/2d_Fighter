using Photon.Deterministic;
using Quantum.QuantumView.Base;

namespace Quantum.QuantumView.Input
{
    public class LocalQuantumInputPoller : QuantumEntityViewComponent<CustomViewContext>
    {
        private SimpleInput _input;
        
        public override void OnActivate(Frame frame)
        {
            var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);

            if (Game.PlayerIsLocal(playerLink.Player))
            {
                QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback),
                    onlyIfActiveAndEnabled: true);
            }
        }
        
        private void PollInput(CallbackPollInput callback)
        {
            //callback.SetInput(_input, DeterministicInputFlags.Repeatable);
        }
    }
}