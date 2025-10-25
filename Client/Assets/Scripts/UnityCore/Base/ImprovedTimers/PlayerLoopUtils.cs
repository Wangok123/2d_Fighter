using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UnityCore.Base.ImprovedTimers
{
    public static class PlayerLoopUtils
    {
        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.type != typeof(T)) return HandleSubSystemLoop<T>(ref loop, in systemToInsert, index);

            var playerLoopSystem = new List<PlayerLoopSystem>();
            if (loop.subSystemList != null)
                playerLoopSystem.AddRange(loop.subSystemList);
            
            playerLoopSystem.Insert(index, systemToInsert);
            loop.subSystemList = playerLoopSystem.ToArray();
            return true;
        }

        private static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.subSystemList == null) return false;
            
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index)) continue;
                return true;
            } 
            
            return false;
        }

        public static void PrintPlayerLoop(PlayerLoopSystem loop)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Player Loop Structure:");
            foreach (var subSystem in loop.subSystemList) 
            {
                PrintSubSystem(subSystem, sb, 0);
            }
            
            Debug.Log(sb.ToString());
        }

        static void PrintSubSystem(PlayerLoopSystem system, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2).AppendLine(system.type.Name);
            if (system.subSystemList == null || system.subSystemList.Length == 0)
                return;
            
            foreach (var subSystem in system.subSystemList)
            {
                PrintSubSystem(subSystem, sb, level + 1);
            }
        }
    }
}