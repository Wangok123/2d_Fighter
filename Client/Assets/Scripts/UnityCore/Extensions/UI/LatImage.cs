using System.Collections;
using LATLog;
using UnityCore.Base;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UnityCore.Extensions.UI
{
    [RequireComponent(typeof(Image))]
    public class LatImage : MonoBehaviour
    {
        [SerializeField] private string atlasName;
        [SerializeField] private string spriteName;

        public Image Image => _image;

        private SpriteAtlas _cachedAtlas;
        private Sprite _cachedSprite;
        private Image _image;
        private Coroutine _loadCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(atlasName) && !string.IsNullOrEmpty(spriteName))
            {
                LoadImage(atlasName, spriteName);
            }
        }

        private void OnDestroy()
        {
            Unload();
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
                _loadCoroutine = null;
            }
        }

        /// <summary>
        /// 设置图集和精灵，加载图集并缓存精灵
        /// </summary>
        public void LoadImage(string atlas, string sprite)
        {
            if (string.IsNullOrEmpty(atlas) || string.IsNullOrEmpty(sprite))
            {
                GameDebug.LogError("[LatImage] 图集或精灵名称不能为空");
                return;
            }

            atlasName = atlas.ToLower();
            spriteName = sprite;
            // 如果已经有缓存的精灵，直接使用
            if (_cachedSprite != null && _cachedSprite.name == sprite)
            {
                _image.sprite = _cachedSprite;
                return;
            }

            // 否则加载图集
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
            }

            _loadCoroutine = StartCoroutine(LoadAtlasAsync(atlasName, spriteName));
        }

        /// <summary>
        /// 异步加载图集
        /// </summary>
        private IEnumerator LoadAtlasAsync(string atlas, string sprite)
        {
            yield return Game.YooAsset.LoadAtlasAsset(atlas, spriteAtlas =>
            {
                _cachedAtlas = spriteAtlas;
                // 缓存图集
                LoadSpriteFromAtlas(spriteAtlas, sprite);
            });
        }

        /// <summary>
        /// 从图集中加载精灵
        /// </summary>
        private void LoadSpriteFromAtlas(SpriteAtlas spriteAtlas, string spName)
        {
            if (spriteAtlas != null)
            {
                _cachedSprite = spriteAtlas.GetSprite(spName);
                if (_cachedSprite != null)
                {
                    _image.sprite = _cachedSprite;
                }
                else
                {
                    GameDebug.LogError($"[LatImage] 图集没有找到精灵: {spName}");
                }
            }
        }

        /// <summary>
        /// 卸载图集并清除缓存
        /// </summary>
        private void Unload()
        {
            if (_cachedAtlas != null)
            {
                Game.YooAsset.UnloadAsset(atlasName);
            }

            _cachedSprite = null;
        }
    }
}