using System;
using System.Collections.Generic;
using Core;
using LATLog;
using UnityCore.UI.Core;

namespace UnityCore.Base
{
    public static class GameEntry
    {
        private static readonly LatLinkedList<LatComponent> s_Components = new LatLinkedList<LatComponent>();
        
        public static T GetComponent<T>() where T : LatComponent
        {
            return (T)GetComponent(typeof(T));
        }
        
        public static LatComponent GetComponent(Type type)
        {
            LinkedListNode<LatComponent> current = s_Components.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }
        
        public static LatComponent GetComponent(string typeName)
        {
            LinkedListNode<LatComponent> current = s_Components.First;
            while (current != null)
            {
                Type type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }
        
        public static void RegisterComponent(LatComponent component)
        {
            if (component == null)
            {
                GameDebug.LogError("Component is invalid.");
                return;
            }

            Type type = component.GetType();

            LinkedListNode<LatComponent> current = s_Components.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    GameDebug.LogError($"Component type '{type.FullName}' is already exist.");
                    return;
                }

                current = current.Next;
            }

            s_Components.AddLast(component);
        }
    }
}