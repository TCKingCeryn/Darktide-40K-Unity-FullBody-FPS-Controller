// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Lava Flowing Shader/Unlit/Simple" 
{
Properties {
	//_Color ("Main Color", Color) = (1,1,1)
	_MainTex ("_MainTex RGBA", 2D) = "white" {}
	_LavaTex ("_LavaTex RGB", 2D) = "white" {}
}

Category {
	Tags { "RenderType"="Opaque" }

	Lighting Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _LavaTex;
			
			struct appdata_t {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
			};

			struct v2f {
				fixed4 vertex : SV_POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed2 texcoord1 : TEXCOORD1;
			};
			
			fixed4 _MainTex_ST;
			fixed4 _LavaTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord,_LavaTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.texcoord);
				fixed4 tex2 = tex2D(_LavaTex, i.texcoord1);
				
				tex = lerp(tex2,tex,tex.a);
				
				return tex;
			}
			ENDCG 
		}
	}	
}
}
