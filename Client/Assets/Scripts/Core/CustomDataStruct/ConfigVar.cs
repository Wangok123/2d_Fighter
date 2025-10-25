using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LATLog;

namespace Core
{
    public class ConfigVarAttribute : Attribute
    {
        public string Name = null;
        public string DefaultValue = "";
        public ConfigVar.Flags Flags = ConfigVar.Flags.None;
        public string Description = "";
    }

    public class ConfigVar
    {
        [Flags]
        public enum Flags
        {
            None = 0x0, // 无特性
            Save = 0x1, // 将配置变量保存到 settings.cfg 文件中
            Cheat = 0x2, // 将配置变量视为作弊变量。只能在启用作弊时设置
            ServerInfo = 0x4, // 当客户端连接或变量更改时，将这些变量发送给客户端
            ClientInfo = 0x8, // 当客户端连接或变量更改时，将这些变量发送给服务器
            User = 0x10, // 用户创建的变量
        }

        public static Dictionary<string, ConfigVar> ConfigVars;
        public static Flags DirtyFlags = Flags.None;
        private static bool s_Initialized = false;

        public readonly string Name;
        public readonly string Description;
        public readonly string DefaultValue;
        public readonly Flags Flag;
        public bool Changed;

        private string _stringValue;
        private float _floatValue;
        private int _intValue;

        public static void Init()
        {
            if (s_Initialized)
                return;

            ConfigVars = new Dictionary<string, ConfigVar>();
            InjectAttributeConfigVars();
            s_Initialized = true;
        }
        
        public virtual string Value
        {
            get => _stringValue;
            set
            {
                if (_stringValue == value)
                    return;
                DirtyFlags |= Flag;
                _stringValue = value;
                if (!int.TryParse(value, out _intValue))
                    _intValue = 0;
                if (!float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out _floatValue))
                    _floatValue = 0;
                Changed = true;
            }
        }
        
        public int IntValue => _intValue;

        public float FloatValue => _floatValue;

        public ConfigVar(string name, string description, string defaultValue, Flags flags = Flags.None)
        {
            this.Name = name;
            this.Flag = flags;
            this.Description = description;
            this.DefaultValue = defaultValue;
        }

        public static void ResetAllToDefault()
        {
            foreach (var v in ConfigVars)
            {
                v.Value.ResetToDefault();
            }
        }

        static void InjectAttributeConfigVars()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var _class in assembly.GetTypes())
                {
                    if (!_class.IsClass)
                        continue;
                    foreach (var field in _class.GetFields(System.Reflection.BindingFlags.Instance |
                                                           System.Reflection.BindingFlags.Static |
                                                           System.Reflection.BindingFlags.NonPublic |
                                                           System.Reflection.BindingFlags.Public))
                    {
                        if (!field.IsDefined(typeof(ConfigVarAttribute), false))
                            continue;
                        if (!field.IsStatic)
                        {
                            GameDebug.LogError("Cannot use ConfigVar attribute on non-static fields");
                            continue;
                        }

                        if (field.FieldType != typeof(ConfigVar))
                        {
                            GameDebug.LogError("Cannot use ConfigVar attribute on fields not of type ConfigVar");
                            continue;
                        }

                        var attr = field.GetCustomAttributes(typeof(ConfigVarAttribute),
                            false)[0] as ConfigVarAttribute;
                        var name = attr.Name != null ? attr.Name : _class.Name.ToLower() + "." + field.Name.ToLower();
                        var cvar = field.GetValue(null) as ConfigVar;
                        if (cvar != null)
                        {
                            GameDebug.LogError("ConfigVars (" + name +
                                               ") should not be initialized from code; just marked with attribute");
                            continue;
                        }

                        cvar = new ConfigVar(name, attr.Description, attr.DefaultValue, attr.Flags);
                        cvar.ResetToDefault();
                        RegisterConfigVar(cvar);
                        field.SetValue(null, cvar);
                    }
                }
            }

            // Clear dirty flags as default values shouldn't count as dirtying
            DirtyFlags = Flags.None;
        }

        private static Regex validateNameRe = new Regex(@"^[a-z_+-][a-z0-9_+.-]*$");
        public static void RegisterConfigVar(ConfigVar cvar)
        {
            if (ConfigVars.ContainsKey(cvar.Name))
            {
                GameDebug.LogError("Trying to register cvar " + cvar.Name + " twice");
                return;
            }
            if (!validateNameRe.IsMatch(cvar.Name))
            {
                GameDebug.LogError("Trying to register cvar with invalid name: " + cvar.Name);
                return;
            }
            ConfigVars.Add(cvar.Name, cvar);
        }
        
        private void ResetToDefault()
        {
            Value = DefaultValue;
        }
    }
}