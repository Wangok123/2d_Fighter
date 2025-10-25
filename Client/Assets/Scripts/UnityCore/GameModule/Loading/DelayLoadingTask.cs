using System.Collections;
using UnityEngine;

namespace UnityCore.GameModule.Loading
{
    public class DelayLoadingTask: ILoadingTask
    {
        private float duration;
        private float elapsedTime = 0f;
        public bool IsDone { get; private set; } = false;
        public float Progress => Mathf.Clamp01(elapsedTime / duration);
        public int Weight => 1; // Default weight, can be adjusted if needed

        public DelayLoadingTask(float duration)
        {
            this.duration = duration;
        }

        public IEnumerator Execute()
        {
            elapsedTime = 0f;
            IsDone = false;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            IsDone = true;
        }
    }
}