namespace Quantum
{
    public class CharacterMovementData : AssetObject
    {
        public AssetRef<KCC2DConfig> DefaultKCC2DConfig;
        
        public unsafe void UpdateKCCSettings(Frame frame, EntityRef playerEntityRef)
        {
            KCC2D* kcc = frame.Unsafe.GetPointer<KCC2D>(playerEntityRef);

            KCC2DConfig config;
            {
                config = frame.FindAsset<KCC2DConfig>(DefaultKCC2DConfig.Id);
            }

            kcc->Config = config;
        }
    }
}