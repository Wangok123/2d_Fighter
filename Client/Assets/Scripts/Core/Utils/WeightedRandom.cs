using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utils
{
    /// <summary>
    /// 权重计算器
    /// </summary>
    public class WeightedRandom<T>
    {
        private readonly Random _random = new Random();
        private readonly List<T> _items;
        private readonly List<int> _weights;
        private readonly int _totalWeight;

        public WeightedRandom(Dictionary<T, int> itemWeights)
        {
            _items = new List<T>(itemWeights.Keys);
            _weights = new List<int>(itemWeights.Values);
            _totalWeight = _weights.Sum();
        }

        public T GetRandomItem()
        {
            int randomNumber = _random.Next(0, _totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                currentWeight += _weights[i];
                if (randomNumber < currentWeight)
                {
                    return _items[i];
                }
            }

            return default; // 理论上不会执行到这里
        }
    }
}