Shader "Planet Maenad/GPUInstancer/GPU Healthbar Mesh" 
{
    Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Fill ("Fill", Range(0,1)) = 1
    }
    SubShader {
        Tags { "Queue"="Overlay" }
        LOD 100

        Pass 
		{
            ZTest Off
			Cull Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata 
			{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f 
			{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                // If you need instance data in the fragment shader, uncomment next line
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;

            UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v) 
			{
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                // If you need instance data in the fragment shader, uncomment next line
                // UNITY_TRANSFER_INSTANCE_ID(v, o);

                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // generate UVs from fill level (assumed texture is clamped)
                o.uv = v.uv;
                o.uv.x += 0.5 - fill;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {


                // Could access instanced data here too like:
                // UNITY_SETUP_INSTANCE_ID(i);
                // UNITY_ACCESS_INSTANCED_PROP(Props, _Foo);
                // But, remember to uncomment lines flagged above

                return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
}