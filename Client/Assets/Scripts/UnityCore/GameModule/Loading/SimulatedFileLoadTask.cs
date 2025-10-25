using System.Collections;
using UnityEngine;

namespace UnityCore.GameModule.Loading
{
    public class SimulatedFileLoadTask: ILoadingTask
    {
        private float loadTime;
        private float progress = 0f;
        private bool isDone = false;

        public float Progress => Mathf.Clamp01(progress);
        public int Weight => 1; // Default weight, can be adjusted if needed
        public bool IsDone => isDone;

        public SimulatedFileLoadTask(float timeToLoad)
        {
            loadTime = timeToLoad;
        }

        public IEnumerator Execute()
        {
            float timer = 0f;
            while (timer < loadTime)
            {
                timer += Time.deltaTime;
                progress = timer / loadTime;
                yield return null;
            }
            progress = 1f;
            isDone = true;
        }
    }
}