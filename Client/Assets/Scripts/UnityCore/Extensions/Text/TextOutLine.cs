using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.Extensions
{
    /// <summary>
    /// Text描边效果
    /// </summary>
    public class TextOutLine : BaseMeshEffect
    {
        private const float OutlineWidth = 10;

        private static List<UIVertex> m_VetexList = new List<UIVertex>();
        public Vector4 m_offset = Vector4.zero;
        
        private Text _text;

        public Text Text
        {
            get
            {
                if (_text == null)
                {
                    _text = GetComponent<Text>();
                }

                return _text;
            }
        }

        private Vector4 _deltaOffSet = Vector4.zero;
        private static int _mMinFont = 45;
        private static int _mMaxFont = 100;
        private static Vector4 _mMin = new Vector4(0.27f, 0.122f, 1.62f, 0);
        private static Vector4 _mMax = new Vector4(0.6f, 0.21f, 4f, 0);
        
        private bool _isRefresh;
        private bool _isSetting;

        protected override void Awake()
        {
            _text = GetComponent<Text>();
            if (Application.isPlaying)
            {
                if (graphic.material == Graphic.defaultGraphicMaterial)
                {
                    Debug.LogError(
                        $"{nameof(TextOutLine)}: Please set the material of the text to a material that supports outline effect.");
                }
            }

            _deltaOffSet = (_mMax - _mMin) / (_mMaxFont - _mMinFont);
        }
        
        protected override void OnEnable()
        {
            if (graphic) SetDirty();// 确保网格和描边效果的更新
        }

        // 监听UI布局变化 强制刷新网格重建
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (graphic) SetDirty();// 确保网格和描边效果的更新
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            //设置canvas通道和标记刷线数据
            if (!_isSetting)
            {
                SetCanvas();
                _isSetting = true;
            }

            vh.GetUIVertexStream(m_VetexList);

            // refreshOffset();
            //
            //
            // this._ProcessVertices();

            vh.Clear();
            vh.AddUIVertexTriangleStream(m_VetexList);
        }
        
        private void SetDirty()
        {
            graphic.SetAllDirty();
            graphic.SetMaterialDirty();
        }
        
        private void SetCanvas() 
        {
            if (graphic)
            {

                if (graphic.canvas)
                {
                    var v1 = graphic.canvas.additionalShaderChannels;
                    var v2 = AdditionalCanvasShaderChannels.TexCoord1;
                    if ((v1 & v2) != v2)
                    {
                        graphic.canvas.additionalShaderChannels |= v2;
                    }
                    v2 = AdditionalCanvasShaderChannels.TexCoord2;
                    if ((v1 & v2) != v2)
                    {
                        graphic.canvas.additionalShaderChannels |= v2;
                    }
                    v2 = AdditionalCanvasShaderChannels.TexCoord3;
                    if ((v1 & v2) != v2)
                    {
                        graphic.canvas.additionalShaderChannels |= v2;
                    }
                    v2 = AdditionalCanvasShaderChannels.Tangent;
                    if ((v1 & v2) != v2)
                    {
                        graphic.canvas.additionalShaderChannels |= v2;
                    }
                    v2 = AdditionalCanvasShaderChannels.Normal;
                    if ((v1 & v2) != v2)
                    {
                        graphic.canvas.additionalShaderChannels |= v2;
                    }
                }
            }

        }
        
        private void RefreshOffset()
        {
            float actualFontSize = Text.fontSize;
            if (Text.resizeTextForBestFit)
            {
                actualFontSize = Text.cachedTextGenerator.fontSizeUsedForBestFit;
            }

            // m_offset = _mMin + _deltaOffSet * (actualFontSize - m_minFont);
        }
    }
}