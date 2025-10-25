namespace UnityCore.GameModule.Loading
{
    public interface ILoadingUI
    {
        void OnLoadingStarted();
        void SetProgress(float progress); // 0~1
        void OnLoadingComplete();
    }
}