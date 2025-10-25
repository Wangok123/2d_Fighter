# ConfigVar 类

> 代码结构和目的:
 该代码定义了一个 ConfigVar 类及其相关的属性和方法，用于管理游戏配置变量。配置变量可以保存、加载和重置，并且可以通过标志来控制其行为。 

 ## ConfigVarAttribute 类
 
 是一个自定义属性类，用于标记配置变量。它包含变量的名称、默认值、标志和描述：

 ```C#
 public class ConfigVarAttribute : Attribute
{
    public string Name = null;
    public string DefaultValue = "";
    public ConfigVar.Flags Flags = ConfigVar.Flags.None;
    public string Description = "";
}
 ```

## ConfigVar 类
ConfigVar 类是核心类，包含静态字段和方法来管理所有配置变量。它使用一个字典来存储所有注册的配置变量：

```C#
public static Dictionary<string, ConfigVar> ConfigVars;
```

## 初始化和重置
Init 方法用于初始化配置变量字典，并调用 InjectAttributeConfigVars 方法来注入带有 ConfigVarAttribute 属性的配置变量：

```C#
public static void Init()
{
    if (s_Initialized)
        return;

    ConfigVars = new Dictionary<string, ConfigVar>();
    InjectAttributeConfigVars();
    s_Initialized = true;
}
```

## InjectAttributeConfigVars 方法

作用是通过反射机制扫描当前应用程序域中的所有程序集，查找带有 ConfigVarAttribute 特性的静态字段，并将这些字段注册为 ConfigVar 实例。具体步骤如下：  
- 遍历当前应用程序域中的所有程序集。
- 遍历每个程序集中的所有类型。
- 检查每个类型是否为类。
- 遍历每个类中的所有字段。
- 检查每个字段是否带有 ConfigVarAttribute 特性。
- 检查字段是否为静态字段。
- 检查字段类型是否为 ConfigVar。
- 根据特性信息创建新的 ConfigVar 实例。
- 将新的 ConfigVar 实例注册到 ConfigVars 字典中。
- 将字段的值设置为新的 ConfigVar 实例。
- 清除脏标志，因为默认值不应计为脏。
- 该方法的主要目的是自动化配置变量的注册过程，避免手动注册每个配置变量

## 总结

该代码通过定义 ConfigVar 类及其相关方法，提供了一个灵活的框架来管理游戏配置变量。配置变量可以通过自定义属性进行标记，并支持保存、加载和重置操作。