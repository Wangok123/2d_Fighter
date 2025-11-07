namespace Quantum
{
    public class PlayerMovementData : AssetObject
    {
        public AssetRef<KCC2DConfig> DefaultKCC2DConfig;
        
        public unsafe void UpdateKCCSettings(Frame frame, EntityRef playerEntityRef)
        {
            CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(playerEntityRef);
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(playerEntityRef);
            KCC2D* kcc = frame.Unsafe.GetPointer<KCC2D>(playerEntityRef);

            KCC2DConfig config;
            
            {
                config = frame.FindAsset<KCC2DConfig>(DefaultKCC2DConfig.Id);
            }

            kcc->Config = config;
        }
    }
}