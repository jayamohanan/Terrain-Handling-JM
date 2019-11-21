Shader "Custom/terrain_Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_TextureScale("Texture Scale", Range(0,50)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

	    

		float maxHeight;
		float minHeight;
		const static int maxSize = 8;
		const static float epsilon = 1E-4;
		int  layerCount;
		float startHeight[maxSize];
		half3 tintColor[maxSize];
		float tintStrength[maxSize];
		float blendStrength[maxSize];
		UNITY_DECLARE_TEX2DARRAY(textureArray);

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
        };

		sampler2D _MainTex;
		float _TextureScale;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

		float InverseLerp(float value, float min, float max)
		{
			return saturate((value - min) / (max-min));
		}


		float3 triplanarProjection(float3 worldPos, float3 worldNormal, int index) {
			float3 worldPosScaled = worldPos / _TextureScale;
			float3 normalAbs = abs(worldNormal);
			normalAbs = normalAbs / (normalAbs.x + normalAbs.y + normalAbs.z);
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(textureArray, float3(worldPosScaled.y, worldPosScaled.z, index)) * normalAbs.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(textureArray, float3(worldPosScaled.x, worldPosScaled.z, index)) * normalAbs.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(textureArray, float3(worldPosScaled.x, worldPosScaled.y, index)) * normalAbs.z;
			return  xProjection + yProjection + zProjection;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float heightPercent = InverseLerp(IN.worldPos.y, minHeight, maxHeight);
			float3 baseColor;
			for (int i = 0; i < layerCount; i++)
			{
				float blendValue = InverseLerp(heightPercent - startHeight[i], -blendStrength[i]/2 - epsilon, blendStrength[i] / 2);
				float3 textureColor = triplanarProjection(IN.worldPos, IN.worldNormal, i);
				float3 totalColor = textureColor*(1-tintStrength[i]) + tintColor[i]* tintStrength[i];
				
				baseColor = (1 - blendValue) * baseColor + blendValue * totalColor;
			}
			o.Albedo = baseColor;
		}
        ENDCG
    }
    FallBack "Diffuse"
}
