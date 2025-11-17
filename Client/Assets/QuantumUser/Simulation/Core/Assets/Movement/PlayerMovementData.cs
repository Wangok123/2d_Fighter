namespace Quantum
{
    public class PlayerMovementData : MovementData
    {
        public AssetRef<KCC2DConfig> DefaultKCC2DConfig;
        
        public override unsafe void UpdateKCCSettings(Frame frame, EntityRef playerEntityRef)
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