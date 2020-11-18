using System;
using UnityEngine;

namespace MyProject
{
    public class Glow : MonoBehaviour
    {
        [Serializable]
        public struct ShaderProperty
        {
            public Color color;
            [Range(0, 1)] public float threshold;
            [Range(0, 10)]public float intensity;

            public ShaderProperty(Color color, float threshold, float intensity)
            {
                this.color = color;
                this.threshold = threshold;
                this.intensity = intensity;
            }
        }

        readonly int SHADER_PROPERTY_ID_COLOR = Shader.PropertyToID("_Color");
        readonly int SHADER_PROPERTY_ID_THRESHOLD = Shader.PropertyToID("_Threshold");
        readonly int SHADER_PROPERTY_ID_INTENSITY = Shader.PropertyToID("_Intensity");
        readonly int SHADER_PROPERTY_ID_COMPOSITE_TEX = Shader.PropertyToID("_CompositeTex");

        [SerializeField] Shader _shader = null;
        Material _material;

        [SerializeField] int _devide = 2;   // RenderTextureの解像度を落とすために使用
        [SerializeField, Range(0, 100)] int _iteration = 10;     // ぼかしをいれる回数

        [SerializeField] ShaderProperty _shaderProperty = new ShaderProperty(Color.white, 0.8f, 1.5f);

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // RenderTexture生成
            RenderTexture brightnessTex = CreateBrightness(source);
            RenderTexture blurTex0 = RenderTexture.GetTemporary(brightnessTex.descriptor);
            RenderTexture blurTex1 = RenderTexture.GetTemporary(brightnessTex.descriptor);
            RenderTexture compositeTex = RenderTexture.GetTemporary(brightnessTex.descriptor);

            // 生成した輝度画像にブラーをかける
            Graphics.Blit(brightnessTex, blurTex0);
            for (int i = 0; i < _iteration; i++)
            {
                Graphics.Blit(blurTex0, blurTex1, _material, 2);
                Graphics.Blit(blurTex1, blurTex0, _material, 3);

                Graphics.Blit(blurTex0, compositeTex, _material, 4);
            }

            // 最終出力
            _material.SetTexture(SHADER_PROPERTY_ID_COMPOSITE_TEX, compositeTex);
            _material.SetColor(SHADER_PROPERTY_ID_COLOR, _shaderProperty.color);
            Graphics.Blit(source, destination, _material, 5);

            // RenderTexture解放
            RenderTexture.ReleaseTemporary(brightnessTex);
            RenderTexture.ReleaseTemporary(blurTex0);
            RenderTexture.ReleaseTemporary(blurTex1);
            RenderTexture.ReleaseTemporary(compositeTex);
        }

        /// <summary>
        /// 輝度画像を生成する
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        RenderTexture CreateBrightness(RenderTexture source)
        {
            RenderTexture brightnessTex = RenderTexture.GetTemporary(
                source.width / _devide,
                source.height / _devide,
                source.depth,
                source.format);

            _material.SetFloat(SHADER_PROPERTY_ID_THRESHOLD, _shaderProperty.threshold);
            _material.SetFloat(SHADER_PROPERTY_ID_INTENSITY, _shaderProperty.intensity);

            Graphics.Blit(source, brightnessTex, _material, 1);

            return brightnessTex;
        }
    }
}
