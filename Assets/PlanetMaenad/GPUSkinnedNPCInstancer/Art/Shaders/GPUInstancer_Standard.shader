Shader "Planet Maenad/GPUInstancer/GPU Instance Standard" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.1

		_BumpAmount("BumpAmount", Range(0,1)) = 1
		_BumpMap ("BumpMap", 2D) = "bump" {}

		_EmissionColor("EmissionColor", Color) = (0,0,0)
        _EmissionTex("Emission", 2D) = "white" {}
		_EmissionAmount("EmissionAmount", Range(0,25)) = 0.0


		[NoScaleOffset] _AnimTex("Animation Texture", 2D) = "white" {}

		[HideInInspector] [PerRendererData] _StartFrame("", Int) = 0
		[HideInInspector] [PerRendererData] _EndFrame("", Int) = 0
		[HideInInspector] [PerRendererData] _FrameCount("", Int) = 1
		[HideInInspector] [PerRendererData] _OffsetSeconds("", Float) = 0
		[HideInInspector] _PixelCountPerFrame("", Int) = 0
	}

	SubShader
	{
		Tags {"LightMode"="Deferred"}
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow 

		#include "UnityCG.cginc"

		#pragma multi_compile_instancing
		#pragma target 4.5
		#pragma shader_feature _EMISSION
		#pragma shader_feature_local _NORMALMAP

		struct Input 		
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_EmissionTex;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		sampler2D _MainTex;

	    sampler2D _BumpMap;
		float _BumpAmount;

		sampler2D _EmissionTex;
		float _EmissionAmount;

		sampler2D _AnimTex;
		float4 _AnimTex_TexelSize;

		half _Glossiness;
		half _Metallic;

		int _PixelCountPerFrame;

		UNITY_INSTANCING_BUFFER_START(Props)

			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			#define _Color_arr Props		

			UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
			#define _EmissionColor_arr Props


			UNITY_DEFINE_INSTANCED_PROP(int, _StartFrame)
			#define _StartFrame_arr Props
			UNITY_DEFINE_INSTANCED_PROP(int, _EndFrame)
			#define _EndFrame_arr Props
			UNITY_DEFINE_INSTANCED_PROP(int, _FrameCount)
			#define _FrameCount_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _OffsetSeconds)
			#define _OffsetSeconds_arr Props

		UNITY_INSTANCING_BUFFER_END(Props)
		


		float4 GetUV(int index)		
		{
			int row = index / (int)_AnimTex_TexelSize.z;
			int col = index % (int)_AnimTex_TexelSize.z;

			return float4(col / _AnimTex_TexelSize.z, row / _AnimTex_TexelSize.w, 0, 0);
		}
		
		float4x4 GetMatrix(int startIndex, float boneIndex)		
		{
			int matrixIndex = startIndex + boneIndex * 3;

			float4 row0 = tex2Dlod(_AnimTex, GetUV(matrixIndex));
			float4 row1 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 1));
			float4 row2 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 2));
			float4 row3 = float4(0, 0, 0, 1);

			return float4x4(row0, row1, row2, row3);
		}

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			half4 boneIndex : TEXCOORD2;
			fixed4 boneWeight : TEXCOORD3;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		void vert(inout appdata v, out Input o) 
		{
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			UNITY_INITIALIZE_OUTPUT(Input, o);

			int startFrame = UNITY_ACCESS_INSTANCED_PROP(_StartFrame_arr, _StartFrame);
			int endFrame = UNITY_ACCESS_INSTANCED_PROP(_EndFrame_arr, _EndFrame);
			int frameCount = UNITY_ACCESS_INSTANCED_PROP(_FrameCount_arr, _FrameCount);
			float offsetSeconds = UNITY_ACCESS_INSTANCED_PROP(_OffsetSeconds_arr, _OffsetSeconds);

			int offsetFrame = (int)((_Time.y + offsetSeconds) * 30);
			int currentFrame = startFrame + offsetFrame % frameCount;
			
			int clampedIndex = currentFrame * _PixelCountPerFrame;
			
			float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x);
			float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y);
			float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z);
			float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w);

			float4 pos = mul(bone1Matrix, v.vertex) * v.boneWeight.x + mul(bone2Matrix, v.vertex) * v.boneWeight.y + mul(bone3Matrix, v.vertex) * v.boneWeight.z + mul(bone4Matrix, v.vertex) * v.boneWeight.w;
			float4 normal = mul(bone1Matrix, v.normal) * v.boneWeight.x + mul(bone2Matrix, v.normal) * v.boneWeight.y + mul(bone3Matrix, v.normal) * v.boneWeight.z + mul(bone4Matrix, v.normal) * v.boneWeight.w;

			v.vertex = pos;
			v.normal = normal;
		}


		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			fixed4 e = tex2D(_EmissionTex, IN.uv_EmissionTex);

		    o.Albedo = c.rgb;		
			o.Alpha = c.a;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) * _BumpAmount;

			o.Emission = e.rgb * UNITY_ACCESS_INSTANCED_PROP(_EmissionColor_arr, _EmissionColor) * _EmissionAmount;
			
		}

		ENDCG
	}

	FallBack "Diffuse"
}
