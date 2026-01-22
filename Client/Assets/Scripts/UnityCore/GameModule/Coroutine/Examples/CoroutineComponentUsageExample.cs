using UnityCore.Base;
using UnityEngine;
using Wjybxx.BigCat.Co;
using Wjybxx.Commons.Concurrent;

namespace UnityCore.GameModule.Coroutine.Examples
{
    public class CoroutineComponentUsageExample : MonoBehaviour
    {
        private CoroutineComponent _coroutineComponent;
        private long _taskId;

        private void Start()
        {
            _coroutineComponent = Game.Coroutine;

            ExampleDelayedCall();
            // ExampleRepeatCall();
            // ExampleCoroutine();
            // ExampleTimeScale();
        }

        private void ExampleDelayedCall()
        {
            Debug.Log("[Example] DelayedCall - 延迟调用示例");

            var future = _coroutineComponent.DelayedCall(() =>
            {
                Debug.Log($"[Example] 延迟2秒后执行 at {Time.time}");
            }, 2.0);

            _taskId = future.TaskId;
        }

        private void ExampleRepeatCall()
        {
            Debug.Log("[Example] RepeatCall - 周期调用示例");

            var future = _coroutineComponent.RepeatCall(() =>
            {
                Debug.Log($"[Example] 每秒重复执行 at {Time.time}");
            }, 1.0, 1.0);

            Invoke(nameof(CancelRepeatTask), 5.0f);
        }

        private void CancelRepeatTask()
        {
            Debug.Log("[Example] 取消周期任务");
            _coroutineComponent.CancelTask(_taskId);
        }

        private void ExampleCoroutine()
        {
            Debug.Log("[Example] StartCoroutine - 协程示例");

            var context = _coroutineComponent.StartCoroutine(async ctx =>
            {
                Debug.Log($"[Example] 协程开始 at {Time.time}");

                await ctx.Sleep(1.0, timingType: TimingType.Time);
                Debug.Log($"[Example] 等待1秒后 at {Time.time}");

                await ctx.Sleep(2.0, timingType: TimingType.Time);
                Debug.Log($"[Example] 等待3秒总计 at {Time.time}");
            });

            Debug.Log($"[Example] 协程ID: {context.CoroutineId}");
        }

        private void ExampleTimeScale()
        {
            Debug.Log("[Example] TimeScale - 时间缩放示例");

            _coroutineComponent.DelayedCall(() =>
            {
                Debug.Log("[Example] 设置时间缩放为 0.5");
                _coroutineComponent.SetTimeScale(0.5f);
            }, 3.0);

            _coroutineComponent.DelayedCall(() =>
            {
                Debug.Log("[Example] 恢复时间缩放为 1.0");
                _coroutineComponent.SetTimeScale(1.0f);
            }, 6.0);
        }
    }
}
