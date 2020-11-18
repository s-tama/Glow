using System;
using UnityEngine;

namespace MyProject
{
    public class Brightness : MonoBehaviour
    {
        [Serializable]
        public struct ShaderProperty
        {
            [Range(0, 1)] public float threshold;
            [Range(0, 10)] public float intensity;

            public ShaderProperty(float threshold, float intensity)
            {
                this.threshold = threshold;
                this.intensity = intensity;
            }
        }

        readonly int SHADER_PROPERTY_ID_THRESHOLD = Shader.PropertyToID("_Threshold");
        readonly int SHADER_PROPERTY_ID_INTENSITY = Shader.PropertyToID("_Intensity");

        [SerializeField] Shader _shader = null;
        Material _material;

        [SerializeField] int _devide = 2;   // RenderTextureの解像度を落とすために使用
        [SerializeField, Range(0, 100)] int _iteration = 10;     // ぼかしをいれる回数

        [SerializeField] ShaderProperty _shaderProperty = new ShaderProperty(0.8f, 1.5f);

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // 輝度情報を格納するためのテクスチャを生成
            RenderTexture brightnessTex = RenderTexture.GetTemporary(
                source.width / _devide,
                source.height / _devide,
                source.depth,
                source.format);

            _material.SetFloat(SHADER_PROPERTY_ID_THRESHOLD, _shaderProperty.threshold);
            _material.SetFloat(SHADER_PROPERTY_ID_INTENSITY, _shaderProperty.intensity);

            // 輝度情報を抽出してテクスチャに格納
            // 'Glow.shader'のパス1を指定
            Graphics.Blit(source, brightnessTex, _material, 1);

            // ブラーを適用するためのテクスチャを生成
            RenderTexture blurTex0 = RenderTexture.GetTemporary(brightnessTex.descriptor);
            RenderTexture blurTex1 = RenderTexture.GetTemporary(brightnessTex.descriptor);
            RenderTexture compositeTex = RenderTexture.GetTemporary(brightnessTex.descriptor);

            Graphics.Blit(brightnessTex, blurTex0);
            for (int i = 0; i < _iteration; i++)
            {
                // xとyに交互にブラーを適用していく
                // 'Glow.shader'のパス2, 3を使用
                Graphics.Blit(blurTex0, blurTex1, _material, 2);
                Graphics.Blit(blurTex1, blurTex0, _material, 3);

                // 最終的にできたぼかし画像を加算合成
                // 'Glow.shader'のパス4を使用
                Graphics.Blit(blurTex0, compositeTex, _material, 4);
            }

            Graphics.Blit(compositeTex, destination);

            RenderTexture.ReleaseTemporary(brightnessTex);
            RenderTexture.ReleaseTemporary(blurTex0);
            RenderTexture.ReleaseTemporary(blurTex1);
            RenderTexture.ReleaseTemporary(compositeTex);
        }
    }
}
