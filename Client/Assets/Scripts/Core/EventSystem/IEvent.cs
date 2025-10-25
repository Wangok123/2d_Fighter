namespace Core.EventSystem
{
    public interface IEvent
    {
    }

    // 全局通道
    public class GlobalEvent : IEvent
    {
    }

    // UI通道
    public abstract class UIEvent : IEvent
    {
    }

    public class ButtonClickEvent : UIEvent
    {
    }

    // 游戏逻辑通道
    public abstract class GameEvent : IEvent
    {
    }

    public class EnemyDeathEvent : GameEvent
    {
    }
}