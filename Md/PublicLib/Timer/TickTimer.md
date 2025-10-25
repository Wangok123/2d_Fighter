# 时间定时（TickTimer）
 
## 功能简介：（主要用于高频高精度的毫秒级定时）
 
- 可使用`外部循环驱动`计时，也可使用`单独线程驱动`计时，支持多线程
- 定时回调可以在`驱动线程中运行`，也可`指定Handle线程运行`
 
## 分析

这个代码定义了一个名为 `TickTimer` 的类，它继承自抽象类 `Timer`，并实现了高频高精度的毫秒定时器功能。`TickTimer` 类位于命名空间 `LATTimer` 中。

在 `TickTimer` 类中，定义了一些私有字段和常量，例如 `_startTime`、`_tasks`、`_taskPackQueue`、`_setHandler` 和 `TIDLOCK`。这些字段和常量用于管理定时任务和线程操作。

构造函数 `TickTimer` 初始化了定时器的相关字段，并根据传入的 `interval` 参数决定是否启动一个新的线程来处理定时任务：
```csharp
public TickTimer(int interval = 0, bool setHandler = true)
```
如果 `interval` 不为0，则启动一个新的线程，该线程会循环调用 `UpdateTasks` 方法来更新任务状态。

`UpdateTasks` 方法遍历所有任务，并检查当前时间是否超过任务的目标时间。如果超过，则更新任务的状态并调用相应的回调函数：
```csharp
public void UpdateTasks()
```

`HandleTask` 方法处理任务队列中的任务包，并调用相应的回调函数：
```csharp
public void HandleTask()
```

`AddTask` 方法用于添加一个新的定时任务，返回生成的任务ID：
```csharp
public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
```

`DeleteTask` 方法用于删除一个定时任务，并调用取消回调函数：
```csharp
public override bool DeleteTask(int taskId)
```

`Reset` 方法用于重置定时器，清空任务列表并中止定时器线程：
```csharp
public override void Reset()
```

`GenerateTid` 方法生成一个新的任务ID，确保ID唯一：
```csharp
public override int GenerateTid()
```

此外，`TickTimer` 类还定义了两个内部类 `TickTask` 和 `TickTaskPack`，分别用于表示定时任务和任务包。`TickTask` 类包含任务的详细信息，如任务ID、延迟时间、重复次数、开始时间、目标时间、循环索引以及回调函数。`TickTaskPack` 类包含任务ID和回调函数，用于任务队列的处理。
