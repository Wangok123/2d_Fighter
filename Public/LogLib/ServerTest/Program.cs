using LATLog;
using LATMath;
using LATTimer;


GameDebug.InitSettings();
GameDebug.LogColor(LogColorType.Green, "Hello World!");
Example1();
Console.ReadKey();

static void Example1()
{
    TickTimer timer = new TickTimer(10, false)
    {
        LogCallback = GameDebug.Log,
        WarningCallback = GameDebug.LogWarning,
        ErrorCallback = GameDebug.LogError
    };

    uint interval = 66;
    int count = 50;
    int sum = 0;

    int taskID = 0;

    Task.Run(async () =>
    {
        await Task.Delay(2000);
        DateTime history = DateTime.UtcNow;
        taskID = timer.AddTask(interval, (int tid) =>
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now - history;
            history = now;
            int delta = (int)(ts.TotalMilliseconds - interval);
            GameDebug.LogColor(LogColorType.Yellow, $"间隔差: {delta}");
            sum += Math.Abs(delta);
            GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} work");
        }, (int tid) => { GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} cancel"); }, count);
    });

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "cal")
        {
            GameDebug.LogColor(LogColorType.Yellow, $"平均间隔 {sum * 1f / count}");
        }

        if (input == "del")
        {
            timer.DeleteTask(taskID);
        }
    }
}

static void Example3()
{
    AsyncTimer timer = new AsyncTimer(true)
    {
        LogCallback = GameDebug.Log,
        WarningCallback = GameDebug.LogWarning,
        ErrorCallback = GameDebug.LogError
    };

    uint interval = 66;
    int count = 50;
    int sum = 0;

    int taskID = 0;

    Task.Run(async () =>
    {
        await Task.Delay(2000);
        DateTime history = DateTime.UtcNow;
        taskID = timer.AddTask(interval, (int tid) =>
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now - history;
            history = now;
            int delta = (int)(ts.TotalMilliseconds - interval);
            GameDebug.LogColor(LogColorType.Yellow, $"间隔差: {delta}");
            sum += Math.Abs(delta);
            GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} work");
        }, (int tid) => { GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} cancel"); }, count);
    });

    Task.Run(async () =>
    {
        GameDebug.LogColor(LogColorType.Green, "TickTimer HandleTask");
        while (true)
        {
            timer.HandleTask();
            await Task.Delay(2);
        }
    });

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "cal")
        {
            GameDebug.LogColor(LogColorType.Yellow, $"平均间隔 {sum * 1f / count}");
        }

        if (input == "del")
        {
            timer.DeleteTask(taskID);
        }
    }
}

static void Example2()
{
    TickTimer timer = new TickTimer(0, false)
    {
        LogCallback = GameDebug.Log,
        WarningCallback = GameDebug.LogWarning,
        ErrorCallback = GameDebug.LogError
    };

    uint interval = 66;
    int count = 50;
    int sum = 0;

    int taskID = 0;

    Task.Run(async () =>
    {
        await Task.Delay(2000);
        DateTime history = DateTime.UtcNow;
        taskID = timer.AddTask(interval, (int tid) =>
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now - history;
            history = now;
            int delta = (int)(ts.TotalMilliseconds - interval);
            GameDebug.LogColor(LogColorType.Yellow, $"间隔差: {delta}");
            sum += Math.Abs(delta);
            GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} work");
        }, (int tid) => { GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} cancel"); }, count);
    });

    Task.Run(async () =>
    {
        GameDebug.LogColor(LogColorType.Green, "TickTimer HandleTask");
        while (true)
        {
            timer.UpdateTasks();
            //timer.HandleTask();
            //await Task.Delay(2);
        }
    });

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "cal")
        {
            GameDebug.LogColor(LogColorType.Yellow, $"平均间隔 {sum * 1f / count}");
        }

        if (input == "del")
        {
            timer.DeleteTask(taskID);
        }
    }
}

static void Example4()
{
    FrameTimer timer = new FrameTimer(100)
    {
        LogCallback = GameDebug.Log,
        WarningCallback = GameDebug.LogWarning,
        ErrorCallback = GameDebug.LogError
    };
    
    int count = 5;
    int sum = 0;

    int taskId = 0;

    Task.Run(async () =>
    {
        await Task.Delay(2000);
        taskId = timer.AddTask(10, (int tid) =>
        {
            GameDebug.LogColor(LogColorType.Blue, $"tid: {tid} work");
        }, (int tid) =>
        {
            GameDebug.LogColor(LogColorType.Magenta, $"tid: {tid} cancel");
        }, count);
    });

    Task.Run(async () =>
    {
        GameDebug.LogColor(LogColorType.Green, "TickTimer HandleTask");
        while (true)
        {
            timer.UpdateTasks();
            await Task.Delay(66);
        }
    });

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "cal")
        {
            GameDebug.LogColor(LogColorType.Yellow, $"平均间隔 {sum * 1f / count}");
        }

        if (input == "del")
        {
            timer.DeleteTask(taskId);
        }
    }
}

static void MathTest()
{
    for (int i = 0; i < 1025; i++)
    {
        GameDebug.Log(AcosTable.Table[i]);
    }
}