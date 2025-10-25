# 帧定时（FrameTimer）

## 功能简介：（主要用于逻辑帧数的定时）
 
- 只可在单线程当中运行
- 只能由**外部循环驱动**计数
- 定时回调在**驱动线程中运行**

## 分析

这个代码定义了一个名为 `FrameTimer` 的类，它继承自抽象类 `Timer`，并实现了基于帧的定时器功能。`FrameTimer` 类位于命名空间 `LATTimer` 中。

在 `FrameTimer` 类中，定义了一些私有字段和常量，例如 `_currentFrame`、`FrameLock`、`_tasks` 和 `_tidList`。这些字段和常量用于管理定时任务和线程操作。

构造函数 `FrameTimer` 初始化了定时器的相关字段，并根据传入的 `frameId` 参数设置当前帧数：
```csharp
public FrameTimer(ulong frameId = 0)
```
该构造函数还初始化了任务字典 `_tasks` 和任务ID列表 `_tidList`。

`UpdateTasks` 方法用于更新任务状态。每次调用该方法时，当前帧数 `_currentFrame` 增加，并清空任务ID列表 `_tidList`。然后遍历所有任务，检查任务的目标帧是否小于等于当前帧。如果是，则调用任务的结束回调函数，并更新任务的目标帧和重复次数。如果任务的重复次数为0，则将任务ID添加到 `_tidList` 中：
```csharp
public void UpdateTasks()
```
在遍历完所有任务后，`UpdateTasks` 方法会从任务字典 `_tasks` 中移除已完成的任务，并调用日志回调函数记录任务完成情况。

`AddTask` 方法用于添加一个新的定时任务，返回生成的任务ID。该方法会创建一个新的 `FrameTask` 对象，并将其添加到任务字典 `_tasks` 中：
```csharp
public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
```

`DeleteTask` 方法用于删除一个定时任务，并调用取消回调函数。如果任务存在且成功移除，则返回 `true`，否则返回 `false`：
```csharp
public override bool DeleteTask(int taskId)
```

`Reset` 方法用于重置定时器，清空任务列表和任务ID列表，并将当前帧数重置为0：
```csharp
public override void Reset()
```

`GenerateTid` 方法生成一个新的任务ID，确保ID唯一。该方法使用 `lock` 关键字对 `FrameLock` 进行加锁，以确保线程安全：
```csharp
public override int GenerateTid()
```

此外，`FrameTimer` 类还定义了一个内部类 `FrameTask`，用于表示定时任务。`FrameTask` 类包含任务的详细信息，如任务ID、延迟时间、重复次数、目标帧、结束回调函数和取消回调函数：
```csharp
class FrameTask
{
    public int Tid;
    public uint Delay;
    public int Count;
    public ulong DestFrame;
    public Action<int> EndCallback;
    public Action<int> CancelCallback;

    public FrameTask(int tid, uint delay, int count, ulong destFrame, Action<int> endCallback, Action<int> cancelCallback)
    {
        Tid = tid;
        Delay = delay;
        Count = count;
        DestFrame = destFrame;
        EndCallback = endCallback;
        CancelCallback = cancelCallback;
    }
}
```