# 2D Fighter API 文档

## API 概述

本文档提供2D Fighter项目中核心API的详细说明，包括客户端和服务器端的主要接口。

## 客户端 API

### Core 层 API

#### 1. ObjectPool API

**命名空间**：`Core.ObjectPool`

##### IObjectPoolManager

对象池管理器接口

```csharp
public interface IObjectPoolManager
{
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    T Spawn<T>() where T : ObjectBase, new();
    
    /// <summary>
    /// 从对象池获取对象（带参数）
    /// </summary>
    T Spawn<T>(string name) where T : ObjectBase;
    
    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    void Unspawn(ObjectBase obj);
    
    /// <summary>
    /// 清空对象池
    /// </summary>
    void Clear();
    
    /// <summary>
    /// 清空指定类型的对象池
    /// </summary>
    void Clear<T>() where T : ObjectBase;
}
```

**使用示例**：
```csharp
// 获取对象
var bullet = ObjectPoolManager.Instance.Spawn<Bullet>();
bullet.Initialize(position, direction);

// 使用对象
// ...

// 回收对象
ObjectPoolManager.Instance.Unspawn(bullet);
```

##### ObjectBase

池对象基类

```csharp
public abstract class ObjectBase
{
    /// <summary>
    /// 对象初始化（从池中取出时调用）
    /// </summary>
    public virtual void OnSpawn() { }
    
    /// <summary>
    /// 对象回收（放回池中时调用）
    /// </summary>
    public virtual void OnUnspawn() { }
    
    /// <summary>
    /// 对象销毁（池被清空时调用）
    /// </summary>
    public virtual void OnDestroy() { }
}
```

#### 2. EventSystem API

**命名空间**：`Core.EventSystem`

##### EventManager

事件管理器

```csharp
public class EventManager
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : IEvent;
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
    
    /// <summary>
    /// 发送事件
    /// </summary>
    public void Publish<T>(T eventData) where T : IEvent;
    
    /// <summary>
    /// 清空所有事件订阅
    /// </summary>
    public void Clear();
}
```

**使用示例**：
```csharp
// 订阅事件
EventManager.Instance.Subscribe<PlayerDeathEvent>(OnPlayerDeath);

// 发送事件
EventManager.Instance.Publish(new PlayerDeathEvent { PlayerId = 123 });

// 取消订阅
EventManager.Instance.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);

// 事件处理方法
void OnPlayerDeath(PlayerDeathEvent evt)
{
    Debug.Log($"Player {evt.PlayerId} died");
}
```

#### 3. StateMachine API

**命名空间**：`Core.StateMachine`

##### StateMachine

状态机

```csharp
public class StateMachine<T>
{
    /// <summary>
    /// 当前状态
    /// </summary>
    public IState<T> CurrentState { get; }
    
    /// <summary>
    /// 切换状态
    /// </summary>
    public void ChangeState(IState<T> newState);
    
    /// <summary>
    /// 更新状态机
    /// </summary>
    public void Update();
}
```

##### IState

状态接口

```csharp
public interface IState<T>
{
    /// <summary>
    /// 进入状态
    /// </summary>
    void OnEnter(T owner);
    
    /// <summary>
    /// 更新状态
    /// </summary>
    void OnUpdate(T owner);
    
    /// <summary>
    /// 退出状态
    /// </summary>
    void OnExit(T owner);
}
```

**使用示例**：
```csharp
// 定义状态
public class IdleState : IState<Player>
{
    public void OnEnter(Player player) 
    {
        Debug.Log("Enter Idle State");
    }
    
    public void OnUpdate(Player player)
    {
        if (player.InputMove)
            player.StateMachine.ChangeState(new MoveState());
    }
    
    public void OnExit(Player player)
    {
        Debug.Log("Exit Idle State");
    }
}

// 使用状态机
var stateMachine = new StateMachine<Player>();
stateMachine.ChangeState(new IdleState());
```

#### 4. Utils API

**命名空间**：`Core.Utils`

##### ListPool

列表对象池

```csharp
public static class ListPool<T>
{
    /// <summary>
    /// 从池中获取List
    /// </summary>
    public static List<T> Get();
    
    /// <summary>
    /// 归还List到池中
    /// </summary>
    public static void Release(List<T> list);
}
```

##### WeightedRandom

权重随机

```csharp
public class WeightedRandom<T>
{
    /// <summary>
    /// 添加带权重的项
    /// </summary>
    public void Add(T item, float weight);
    
    /// <summary>
    /// 获取随机项
    /// </summary>
    public T GetRandom();
    
    /// <summary>
    /// 清空所有项
    /// </summary>
    public void Clear();
}
```

### UnityCore 层 API

#### 1. Network API

**命名空间**：`UnityCore.Network`

##### NetworkManager

网络管理器

```csharp
public class NetworkManager : MonoBehaviour
{
    /// <summary>
    /// 连接到服务器
    /// </summary>
    public void Connect(string host, int port);
    
    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect();
    
    /// <summary>
    /// 发送消息
    /// </summary>
    public void Send<T>(T message) where T : IMessage;
    
    /// <summary>
    /// 注册消息处理器
    /// </summary>
    public void RegisterHandler<T>(Action<T> handler) where T : IMessage;
    
    /// <summary>
    /// 注销消息处理器
    /// </summary>
    public void UnregisterHandler<T>(Action<T> handler) where T : IMessage;
}
```

**使用示例**：
```csharp
// 连接服务器
NetworkManager.Instance.Connect("127.0.0.1", 5000);

// 注册消息处理器
NetworkManager.Instance.RegisterHandler<LoginResponse>(OnLoginResponse);

// 发送消息
var request = new LoginRequest { Username = "player123" };
NetworkManager.Instance.Send(request);

// 断开连接
NetworkManager.Instance.Disconnect();
```

#### 2. ResourceSystem API

**命名空间**：`UnityCore.ResourceSystem`

##### ResourceManager

资源管理器

```csharp
public class ResourceManager : MonoBehaviour
{
    /// <summary>
    /// 异步加载资源
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
    
    /// <summary>
    /// 同步加载资源
    /// </summary>
    public T LoadAsset<T>(string path) where T : UnityEngine.Object;
    
    /// <summary>
    /// 卸载资源
    /// </summary>
    public void UnloadAsset(string path);
    
    /// <summary>
    /// 实例化对象
    /// </summary>
    public GameObject Instantiate(string path, Transform parent = null);
    
    /// <summary>
    /// 预加载资源
    /// </summary>
    public async UniTask PreloadAsync(string[] paths);
}
```

**使用示例**：
```csharp
// 异步加载资源
var prefab = await ResourceManager.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");

// 实例化对象
var player = ResourceManager.Instance.Instantiate("Assets/Prefabs/Player.prefab");

// 卸载资源
ResourceManager.Instance.UnloadAsset("Assets/Prefabs/Player.prefab");
```

#### 3. SceneManagement API

**命名空间**：`UnityCore.SceneManagement`

##### SceneManager

场景管理器

```csharp
public class SceneManager : MonoBehaviour
{
    /// <summary>
    /// 加载场景
    /// </summary>
    public async UniTask<Scene> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single);
    
    /// <summary>
    /// 卸载场景
    /// </summary>
    public async UniTask UnloadSceneAsync(string sceneName);
    
    /// <summary>
    /// 切换场景
    /// </summary>
    public async UniTask SwitchSceneAsync(string sceneName);
    
    /// <summary>
    /// 获取当前活动场景
    /// </summary>
    public Scene GetActiveScene();
}
```

#### 4. UI API

**命名空间**：`UnityCore.UI`

##### UIManager

UI管理器

```csharp
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 打开UI界面
    /// </summary>
    public async UniTask<T> OpenUIAsync<T>(string uiName) where T : UIBase;
    
    /// <summary>
    /// 关闭UI界面
    /// </summary>
    public void CloseUI(string uiName);
    
    /// <summary>
    /// 关闭所有UI
    /// </summary>
    public void CloseAllUI();
    
    /// <summary>
    /// 获取UI界面
    /// </summary>
    public T GetUI<T>(string uiName) where T : UIBase;
}
```

##### UIBase

UI基类

```csharp
public abstract class UIBase : MonoBehaviour
{
    /// <summary>
    /// UI初始化
    /// </summary>
    public virtual void OnInit() { }
    
    /// <summary>
    /// UI打开
    /// </summary>
    public virtual void OnOpen() { }
    
    /// <summary>
    /// UI关闭
    /// </summary>
    public virtual void OnClose() { }
    
    /// <summary>
    /// UI销毁
    /// </summary>
    public virtual void OnDestroy() { }
}
```

#### 5. Audio API

**命名空间**：`UnityCore.Audio`

##### AudioManager

音频管理器

```csharp
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM(string clipName, bool loop = true);
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM();
    
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(string clipName, Vector3 position = default);
    
    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetBGMVolume(float volume);
    
    /// <summary>
    /// 设置SFX音量
    /// </summary>
    public void SetSFXVolume(float volume);
}
```

#### 6. Input API

**命名空间**：`UnityCore.Input`

##### InputManager

输入管理器

```csharp
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// 获取移动输入
    /// </summary>
    public Vector2 GetMoveInput();
    
    /// <summary>
    /// 获取按键按下
    /// </summary>
    public bool GetKeyDown(string actionName);
    
    /// <summary>
    /// 获取按键抬起
    /// </summary>
    public bool GetKeyUp(string actionName);
    
    /// <summary>
    /// 获取按键持续按下
    /// </summary>
    public bool GetKey(string actionName);
}
```

#### 7. Combat API

**命名空间**：`UnityCore.Combat`

##### CombatManager

战斗管理器

```csharp
public class CombatManager : MonoBehaviour
{
    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle(BattleConfig config);
    
    /// <summary>
    /// 结束战斗
    /// </summary>
    public void EndBattle(BattleResult result);
    
    /// <summary>
    /// 处理伤害
    /// </summary>
    public void ApplyDamage(Entity attacker, Entity target, float damage);
    
    /// <summary>
    /// 注册实体
    /// </summary>
    public void RegisterEntity(Entity entity);
    
    /// <summary>
    /// 注销实体
    /// </summary>
    public void UnregisterEntity(Entity entity);
}
```

## 服务器 API

### 网络层 API

#### KCPNet

**命名空间**：`LatNet`

```csharp
public class KCPNet
{
    /// <summary>
    /// 启动服务器
    /// </summary>
    public void Start(string host, int port);
    
    /// <summary>
    /// 更新网络（主循环调用）
    /// </summary>
    public void Update();
    
    /// <summary>
    /// 关闭服务器
    /// </summary>
    public void Shutdown();
    
    /// <summary>
    /// 获取所有会话
    /// </summary>
    public List<KCPSession> GetAllSessions();
}
```

#### KCPSession

**命名空间**：`LatNet`

```csharp
public class KCPSession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public uint SessionId { get; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }
    
    /// <summary>
    /// 发送数据
    /// </summary>
    public void Send(byte[] data);
    
    /// <summary>
    /// 接收数据
    /// </summary>
    public byte[] Receive();
    
    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect();
    
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; }
}
```

### 协议层 API

#### ProtocolManager

**命名空间**：`LatProtocol`

```csharp
public class ProtocolManager
{
    /// <summary>
    /// 注册协议处理器
    /// </summary>
    public void RegisterHandler<T>(Action<KCPSession, T> handler) where T : IMessage;
    
    /// <summary>
    /// 处理消息
    /// </summary>
    public void HandleMessage(KCPSession session, byte[] data);
    
    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage<T>(KCPSession session, T message) where T : IMessage;
    
    /// <summary>
    /// 广播消息
    /// </summary>
    public void BroadcastMessage<T>(T message) where T : IMessage;
}
```

#### ProtobufHelper

**命名空间**：`LatProtocol`

```csharp
public static class ProtobufHelper
{
    /// <summary>
    /// 序列化消息
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : IMessage;
    
    /// <summary>
    /// 反序列化消息
    /// </summary>
    public static T Deserialize<T>(byte[] data) where T : IMessage, new();
}
```

### 业务层 API

#### LoginService

**命名空间**：`LatServer.Service`

```csharp
public class LoginService
{
    /// <summary>
    /// 处理登录请求
    /// </summary>
    public void HandleLogin(KCPSession session, LoginRequest request);
    
    /// <summary>
    /// 验证用户
    /// </summary>
    public bool ValidateUser(string username);
    
    /// <summary>
    /// 创建用户
    /// </summary>
    public ulong CreateUser(string username);
}
```

#### MatchService

**命名空间**：`LatServer.Service`

```csharp
public class MatchService
{
    /// <summary>
    /// 请求匹配
    /// </summary>
    public void RequestMatch(KCPSession session, MatchRequest request);
    
    /// <summary>
    /// 取消匹配
    /// </summary>
    public void CancelMatch(ulong userId);
    
    /// <summary>
    /// 更新匹配队列
    /// </summary>
    public void UpdateMatchQueue();
}
```

#### BattleService

**命名空间**：`LatServer.Service`

```csharp
public class BattleService
{
    /// <summary>
    /// 创建战斗房间
    /// </summary>
    public BattleRoom CreateRoom(List<ulong> userIds);
    
    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle(uint roomId);
    
    /// <summary>
    /// 处理操作
    /// </summary>
    public void HandleOperation(uint roomId, ulong userId, int operationKey);
    
    /// <summary>
    /// 结束战斗
    /// </summary>
    public void EndBattle(uint roomId, BattleResult result);
}
```

## 数据传输对象（DTO）

### UserDto

用户数据传输对象

```csharp
public class UserDto
{
    public ulong UserId { get; set; }      // 用户ID
    public string Username { get; set; }   // 用户名
    public int Level { get; set; }         // 等级
    public uint Exp { get; set; }          // 经验值
}
```

### HeroDto

英雄数据传输对象

```csharp
public class HeroDto
{
    public int HeroId { get; set; }        // 英雄ID
}
```

### BattleHeroDto

战斗英雄数据传输对象

```csharp
public class BattleHeroDto
{
    public string UserName { get; set; }   // 用户名
    public int HeroId { get; set; }        // 英雄ID
}
```

## 错误码

### ErrorCodeID

错误码枚举

```csharp
public enum ErrorCodeID
{
    Success = 0,               // 成功
    
    // 网络错误 1000-1999
    NetworkError = 1000,       // 网络错误
    ConnectionFailed = 1001,   // 连接失败
    Timeout = 1002,            // 超时
    
    // 登录错误 2000-2999
    LoginFailed = 2000,        // 登录失败
    InvalidUsername = 2001,    // 无效用户名
    
    // 匹配错误 3000-3999
    MatchFailed = 3000,        // 匹配失败
    MatchTimeout = 3001,       // 匹配超时
    
    // 战斗错误 4000-4999
    BattleError = 4000,        // 战斗错误
    InvalidOperation = 4001,   // 无效操作
}
```

## API 使用最佳实践

### 1. 异步操作
```csharp
// 推荐：使用UniTask
var asset = await ResourceManager.Instance.LoadAssetAsync<GameObject>("path");

// 避免：使用协程（除非必要）
StartCoroutine(LoadAssetCoroutine());
```

### 2. 资源管理
```csharp
// 加载资源
var prefab = await ResourceManager.Instance.LoadAssetAsync<GameObject>("path");

// 使用资源
var instance = Instantiate(prefab);

// 使用完毕后卸载
ResourceManager.Instance.UnloadAsset("path");
```

### 3. 事件订阅
```csharp
// 订阅事件
void OnEnable()
{
    EventManager.Instance.Subscribe<GameEvent>(OnGameEvent);
}

// 取消订阅
void OnDisable()
{
    EventManager.Instance.Unsubscribe<GameEvent>(OnGameEvent);
}
```

### 4. 对象池使用
```csharp
// 从池中获取
var obj = ObjectPoolManager.Instance.Spawn<MyObject>();

// 初始化对象
obj.Initialize();

// 使用完毕回收
ObjectPoolManager.Instance.Unspawn(obj);
```

### 5. 网络消息
```csharp
// 注册处理器
NetworkManager.Instance.RegisterHandler<MessageType>(HandleMessage);

// 发送消息
NetworkManager.Instance.Send(new MessageType { Data = data });

// 记得注销
NetworkManager.Instance.UnregisterHandler<MessageType>(HandleMessage);
```

## 相关文档

- [项目总览](01-项目总览.md)
- [技术栈详解](02-技术栈.md)
- [客户端架构](03-客户端架构.md)
- [服务端架构](04-服务端架构.md)
- [网络通信协议](05-网络通信协议.md)
