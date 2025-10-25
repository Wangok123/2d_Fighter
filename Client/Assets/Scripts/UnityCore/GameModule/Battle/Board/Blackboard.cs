using System.Collections.Generic;

namespace UnityCore.GameModule.Battle.Board
{
    public class Blackboard
    {
        private readonly Dictionary<IKey, LatValue> dic = new();

        // 泛型接口
        public bool Get<T>(LatKey<T> key, out T value)
        {
            if (dic.TryGetValue(key, out LatValue latValue))
            {
                value = key.Unbox(latValue);
                return true;
            }

            value = default;
            return false;
        }

        public void Set<T>(LatKey<T> key, T value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = key.Box(value);
            }
            else
            {
                dic.Add(key, key.Box(value));
            }
        }

        public bool Remove<T>(LatKey<T> key, out T value)
        {
            if (dic.TryGetValue(key, out LatValue latValue))
            {
                value = key.Unbox(latValue);
                dic.Remove(key);
                return true;
            }

            value = default;
            return false;
        }

        // Nullable支持 -- 重载方法
        public void Set<T>(LatKey<T> key, T? value) where T : struct
        {
            if (value.HasValue)
            {
                Set(key, value.Value);
            }
            else
            {
                dic.Remove(key);
            }
        }

        public bool Get<T>(LatKey<T> key, out T? value) where T : struct
        {
            if (dic.TryGetValue(key, out LatValue latValue))
            {
                value = key.Unbox(latValue);
                return true;
            }

            value = default;
            return false;
        }

        public bool Remove<T>(LatKey<T> key, out T? value) where T : struct
        {
            if (dic.TryGetValue(key, out LatValue latValue))
            {
                value = key.Unbox(latValue);
                dic.Remove(key);
                return true;
            }

            value = default;
            return false;
        }
    }
}