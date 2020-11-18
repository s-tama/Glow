//
// Glow.shader
//

Shader "MyProject/Glow"
{
    Properties
    {
        [HideInInspector][NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		[HideInInspector][NoScaleOffset] _CompositeTex("Composite Texture", 2D) = "white" {}
		[HideInInspector] _Color("Brightness Color", Color) = (1, 1, 1, 1)
		[HideInInspector] _Threshold("Brightness Threshold", Float) = 0.7
		[HideInInspector] _Intensity("Brightness Intensity", Float) = 1.0
    }
    SubShader
    {
		CGINCLUDE
		#include "UnityCG.cginc"

		#define WEIGHT { 0.25641, 0.20000, 0.11025, 0.04615, 0.01538 }
		#define OFFSET { 0, 1, 2, 3, 4 }
		#define SAMPLE_NUM (5)

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		sampler2D _CompositeTex;
		fixed4 _Color;
		float _Threshold;	// 輝度と判断する閾値
		float _Intensity;	// 光の強度

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			return col;
		}
		
		fixed4 frag_brightness(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			return max(col - _Threshold, 0) * _Intensity;
		}

		fixed4 frag_blur_x(v2f i) : SV_Target
		{
			float weight[5] = WEIGHT;
			float offset[5] = OFFSET;
			int sampleNum = SAMPLE_NUM;
			float2 size = _MainTex_TexelSize;
			fixed4 col = tex2D(_MainTex, i.uv) * weight[0];

			for (int j = 1; j < sampleNum; j++)
			{
				col += tex2D(_MainTex, i.uv + float2(offset[j], 0) * size) * weight[j];
				col += tex2D(_MainTex, i.uv - float2(offset[j], 0) * size) * weight[j];
			}

			return col;
		}

		fixed4 frag_blur_y(v2f i) : SV_Target
		{
			float weight[5] = WEIGHT;
			float offset[5] = OFFSET;
			int sampleNum = SAMPLE_NUM;
			float2 size = _MainTex_TexelSize;
			fixed4 col = tex2D(_MainTex, i.uv) * weight[0];

			for (int j = 1; j < sampleNum; j++)
			{
				col += tex2D(_MainTex, i.uv + float2(0, offset[j]) * size) * weight[j];
				col += tex2D(_MainTex, i.uv - float2(0, offset[j]) * size) * weight[j];
			}

			return col;
		}

		fixed4 frag_composite(v2f i) : SV_Target
		{
			fixed4 main = tex2D(_MainTex, i.uv);
			fixed4 composite = tex2D(_CompositeTex, i.uv) * _Color;
			return saturate(main + composite);
		}
		ENDCG

        Cull Off 
		ZWrite Off 
		ZTest Always

		// 0. 通常のレンダリング
        Pass
        {
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            ENDCG
        }

		// 1. 輝度画像を返す
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_brightness
			ENDCG
		}

		// 2. ガウシアンブラーをかける（x）
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_blur_x
			ENDCG
		}

		// 3. ガウシアンブラーをかける（y）
		Pass
		{ 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_blur_y
			ENDCG
		}

		// 4. 画像を合成
		Pass
		{
			Blend OneMinusDstColor One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

		// 5. 画像を合成
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_composite
			ENDCG
		}
    }
}
