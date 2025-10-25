using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.Tools
{
    // 根据滑动列表，输出一个长截图
    public class LongScreenShot : MonoBehaviour
    {
        public ScrollRect scrollView; // ScrollView 组件
        public Camera screenshotCamera; // 截图用的摄像机
        public int screenshotWidth = 1920; // 截图宽度
        public int screenshotHeight = 1080; // 截图高度
        private int _totalHeight;

        void Start()
        {
            StartCoroutine(CaptureLongScreenshot());
        }

        IEnumerator CaptureLongScreenshot()
        {
            yield return new WaitForEndOfFrame();

            // 获取 ScrollView 内容的总高度
            _totalHeight = Mathf.FloorToInt(scrollView.content.rect.height);

            // 创建 RenderTexture 用于捕获图像
            RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
            screenshotCamera.targetTexture = rt;

            // 创建最终的长截图
            Texture2D longScreenshot = new Texture2D(screenshotWidth,
                _totalHeight, TextureFormat.RGB24, false);

            //先缓存content的初始位置
            var anpos = scrollView.content.anchoredPosition;
            scrollView.content.anchoredPosition = new Vector2(anpos.x, _totalHeight - screenshotHeight);
            yield return new WaitForEndOfFrame();

            // 循环滚动并截图
            for (int i = 0; i < Mathf.CeilToInt((float)_totalHeight / screenshotHeight); i++)
            {
                float tmpScreenShotHeight = screenshotHeight;

                var remainingHeight = _totalHeight - (i * screenshotHeight);
                // 如果剩余高度小于截图高度，则调整截图高度
                if (remainingHeight < screenshotHeight)
                {
                    tmpScreenShotHeight = remainingHeight;
                    var pos = scrollView.content.anchoredPosition;
                    scrollView.content.anchoredPosition = new Vector2(pos.x, 0);
                }
                else
                {
                    var pos = scrollView.content.anchoredPosition;
                    float yOffset = i == 0 ? 0 : screenshotHeight;
                    scrollView.content.anchoredPosition = new Vector2(pos.x, pos.y - yOffset);
                }

                // 等待一帧以确保 ScrollView 更新
                yield return new WaitForEndOfFrame();

                // 手动渲染
                screenshotCamera.Render();

                // 将 RenderTexture 内容读取到 Texture2D
                RenderTexture.active = rt;
                Texture2D sectionScreenshot =
                    new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
                sectionScreenshot.ReadPixels(
                    new Rect(0, screenshotHeight - tmpScreenShotHeight, screenshotWidth, tmpScreenShotHeight), 0, 0);
                sectionScreenshot.Apply();

                // 将当前截图部分合并到长图中
                longScreenshot.SetPixels(0, i * screenshotHeight, screenshotWidth, Mathf.CeilToInt(tmpScreenShotHeight),
                    sectionScreenshot.GetPixels());
                yield return new WaitForEndOfFrame();
            }

            // 应用拼接的结果
            longScreenshot.Apply();

            // 保存为 PNG 图片
            byte[] bytes = longScreenshot.EncodeToPNG();
            var path = Path.Combine(Application.persistentDataPath, "LongScreenShot");
            var filePath = Path.Combine(path, $"LongScreenShot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllBytes(filePath, bytes);

            // 清理
            screenshotCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(longScreenshot);
            yield return null;

            //todo: 释放
        }
    }
}