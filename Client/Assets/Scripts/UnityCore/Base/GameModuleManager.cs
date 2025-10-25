using System;
using System.Collections.Generic;
using Core;
using Core.ReferencePool;

namespace UnityCore.Base
{
    public static class GameModuleManager
    {
        private static readonly LatLinkedList<CoreModule> s_GameFrameworkModules = new LatLinkedList<CoreModule>();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (CoreModule module in s_GameFrameworkModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }
        
        public static void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (CoreModule module in s_GameFrameworkModules)
            {
                module.FixedUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (LinkedListNode<CoreModule> current = s_GameFrameworkModules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            s_GameFrameworkModules.Clear();
            ReferencePool.ClearAll();
        }
        
        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);

            if (interfaceType.FullName == null)
            {
                throw new Exception($"{interfaceType} is not a Game Framework module. Please use the correct interface type.");
            }

            string moduleName = interfaceType.FullName;
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new Exception($"Can not find Game Framework module type '{moduleName}'.");
            }

            return GetModule(moduleType) as T;
        }
        
        public static bool RemoveModule<T>() where T : CoreModule
        {
            Type moduleType = typeof(T);
            CoreModule module = GetModule(moduleType);
            if (module == null)
            {
                return false;
            }

            s_GameFrameworkModules.Remove(module);
            module.Shutdown();
            return true;
        }
        
        private static CoreModule GetModule(Type moduleType)
        {
            foreach (CoreModule module in s_GameFrameworkModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }
        
        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static CoreModule CreateModule(Type moduleType)
        {
            CoreModule module = (CoreModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new Exception($"Can not create module '{moduleType.FullName}'.");
            }

            LinkedListNode<CoreModule> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                s_GameFrameworkModules.AddBefore(current, module);
            }
            else
            {
                s_GameFrameworkModules.AddLast(module);
            }

            return module;
        }
    }
}