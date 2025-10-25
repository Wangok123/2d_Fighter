using UnityCore.Extensions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.LoadWnd
{
    public class LoadPlayerItem : MonoBehaviour
    {
        [SerializeField] private LatImage loadImage;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text heroNameText;
        [SerializeField] private Text progressText;
        
        private const string Atlas = "heroload";
        
        public void SetData(string playerName, string heroName, string spriteName)
        {
            playerNameText.text = playerName;
            heroNameText.text = heroName;
            loadImage.LoadImage(Atlas, spriteName);
        }
        
        public void UpdateProgress(float progress)
        {
            progressText.text = $"{progress}%";
        }
    }
}