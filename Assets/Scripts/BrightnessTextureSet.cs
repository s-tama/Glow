//
// Shadetrに渡すグローバルプロパティ設定用
//

using UnityEngine;

namespace MyProject
{
    public class BrightnessTextureSet : MonoBehaviour
    {
        static readonly int SHADER_PROPERTY_ID_THRESHOLD = Shader.PropertyToID("_Threshold");
        static readonly int SHADER_PROPERTY_ID_INTENSITY = Shader.PropertyToID("_Intensity");

        [SerializeField] Shader _shader = null;
        Material _material;

        [SerializeField] int _devide = 2;   // RenderTextureの解像度を落とすために使用
        [SerializeField, Range(0, 1)] float _threshold = 0.7f;
        [SerializeField] float _intensity = 1f;

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // RenderTexture生成
            RenderTexture brightnessTex = CreateBrightness(source);

            // 最終出力
            Graphics.Blit(brightnessTex, destination);

            // RenderTexture解放
            RenderTexture.ReleaseTemporary(brightnessTex);
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

            _material.SetFloat(SHADER_PROPERTY_ID_THRESHOLD, _threshold);
            _material.SetFloat(SHADER_PROPERTY_ID_INTENSITY, _intensity);

            Graphics.Blit(source, brightnessTex, _material);

            return brightnessTex;
        }
    }
}
