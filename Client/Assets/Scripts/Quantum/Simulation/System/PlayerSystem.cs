namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var data = f.GetPlayerData(player);
            if (data.PlayerAvatar != null)
            {
                SetPlayerCharacter(f, player, data.PlayerAvatar);
            }
            else
            {
                Log.Error(
                    "Character prototype is null on RuntimePlayer, check QuantumMenuConnectionBehaviourSDK to prevent adding player automatically!");
            }
        }
        
        private void SetPlayerCharacter(Frame frame, PlayerRef player, AssetRef<EntityPrototype> prototypeAsset)
        {
            var characterEntity = frame.Create(prototypeAsset);

            var playerLink = frame.Unsafe.GetPointer<PlayerLink>(characterEntity);
            playerLink->Player = player;
        }
    }
}