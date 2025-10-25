using Core.StateMachine;
using LATLog;
using UnityCore.EventDefine;
using YooAsset;

namespace UnityCore.ResourceSystem.FsmNode
{
    public class FsmCreateDownloader : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("创建资源下载器！");
            CreateDownloader();
        }

        public void OnExit()
        {
        }

        public void OnUpdate()
        {
        }
        
        void CreateDownloader()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            _machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                GameDebug.Log("Not found any download files !");
                _machine.ChangeState<FsmStartGame>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            }
        }
    }
}