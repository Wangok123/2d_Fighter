using LATLog;
using UnityCore.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.Extensions.UI
{
    [RequireComponent(typeof(RawImage))]
    public class LatRawImage : MonoBehaviour
    {
        [SerializeField] private string textureName;

        private RawImage _rawImage;
        private Coroutine _loadCoroutine;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(textureName))
            {
                LoadTexture(textureName);
            }
        }

        private void OnDisable()
        {
            Unload();
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
                _loadCoroutine = null;
            }
        }

        /// <summary>
        /// 设置纹理名称并加载纹理
        /// </summary>
        public void LoadTexture(string texture)
        {
            if (string.IsNullOrEmpty(texture))
            {
                GameDebug.LogError("[LatRawImage] 纹理名称不能为空");
                return;
            }

            textureName = texture;
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
                _loadCoroutine = null;
            }
            _loadCoroutine = StartCoroutine(Game.YooAsset.LoadTextureAsset(texture, OnLoadSuccess));
        }

        private void OnLoadSuccess(Texture texture)
        {
            _rawImage.texture = texture;
        }

        private void Unload()
        {
            if (!string.IsNullOrEmpty(textureName))
            {
                Game.YooAsset.UnloadAsset(textureName);
            }
        }
    }
}