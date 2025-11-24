
namespace Quantum.Core.Utils
{
    public static class GameSettingsHelper
    {
        public static GameSettingsData Get(Frame frame)
        {
            return frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);
        }
    }
}