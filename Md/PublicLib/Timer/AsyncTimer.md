# 多线程定时（AsyncTimer）

## 功能简介：（主要用于大量并发任务的定时）
 
- 使用**async await异步语法驱动**计时，运行在**线程池**中，支持多线程
- 定时回调可以在**驱动线程中运行**，也可**指定Handle线程运行**

## 分析

这个代码定义了一个名为 `AsyncTimer` 的类，它继承自抽象类 `Timer`，并实现了异步定时器的功能。`AsyncTimer` 类位于命名空间 `LATTimer` 中。

在 `AsyncTimer` 类中，定义了一些私有字段和常量，例如 `AsyncLock`、`_tasks`、`_taskPackQueue` 和 `_setHandler`。这些字段和常量用于管理定时任务和线程操作。

构造函数 `AsyncTimer` 初始化了定时器的相关字段，并根据传入的 `setHandle` 参数决定是否初始化任务包队列：
```csharp
public AsyncTimer(bool setHandle)
```
如果 `setHandle` 为 `true`，则初始化 `_taskPackQueue`。

`HandleTask` 方法处理任务队列中的任务包，并调用相应的回调函数：
```csharp
public void HandleTask()
```

`AddTask` 方法用于添加一个新的定时任务，返回生成的任务ID：
```csharp
public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
```
该方法会创建一个新的 `AsyncTask` 对象，并将其添加到任务字典 `_tasks` 中。

`RunTaskInPool` 方法在任务池中运行任务，使用 `Task.Run` 启动异步任务，并根据任务的延迟时间和重复次数执行回调函数：
```csharp
private void RunTaskInPool(AsyncTask task)
```

`CallbackTaskCB` 方法处理任务的回调逻辑，根据 `_setHandler` 的值决定是直接调用回调函数还是将任务包加入队列：
```csharp
private void CallbackTaskCB(AsyncTask task)
```

`DeleteTask` 方法用于删除一个定时任务，并调用取消回调函数：
```csharp
public override bool DeleteTask(int taskId)
```

`Reset` 方法用于重置定时器，清空任务列表并重置任务ID：
```csharp
public override void Reset()
```

`GenerateTid` 方法生成一个新的任务ID，确保ID唯一：
```csharp
public override int GenerateTid()
```

此外，`AsyncTimer` 类还定义了两个内部类 `AsyncTaskPack` 和 `AsyncTask`，分别用于表示任务包和异步任务。`AsyncTask` 类包含任务的详细信息，如任务ID、延迟时间、重复次数、开始时间、循环索引以及回调函数。`AsyncTaskPack` 类包含任务ID和回调函数，用于任务队列的处理。