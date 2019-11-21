Shader "Texture/BasicTextureShader" {
        Properties {
            _Tess ("Tessellation", Range(1,32)) = 4
            _maxDist ("Tess Fade Distance", Range(0, 500.0)) = 25.0
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _MOS ("Metallic (R), Occlussion (G), Smoothness (B)", 2D) = "white" {}
            _DispTex ("Disp Texture", 2D) = "gray" {}
            _NormalMap ("Normalmap", 2D) = "bump" {}
            _Displacement ("Displacement", Range(0, 1.0)) = 0.3
            _DispOffset ("Disp Offset", Range(0, 1)) = 0.5
            _Color ("Color", color) = (1,1,1,0)
            _Metallic ("Metallic", Range(0, 1)) = 0.5
            _Glossiness ("Smoothness", Range(0, 1)) = 0.5
			_HapticSmoothness ("Haptic Smoothness", Range(20,80)) = 100
			_HapticIntensityMinimum ("Haptic Intensity Minimum", Range(0.5, 1)) = 0.6
			_HapticIntensityMaximum ("Haptic Intensity Maximum", Range(0.5, 1)) = 1
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 300
            
            CGPROGRAM
            #pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tessDistance
            #pragma target 5.0
            #include "Tessellation.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };

            float _Tess;
            float _maxDist;

            float4 tessDistance (appdata v0, appdata v1, appdata v2) 
            {
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _maxDist * 0.2f, _maxDist * 1.2f, _Tess);
            }

            sampler2D _DispTex;
            sampler2D _MOS;
            uniform float4 _DispTex_ST;
            float _Displacement;
            float _DispOffset;
			float _HapticSmoothness;
			float _HapticIntensityMinimum;
			float _HapticIntensityMaximum;

            void disp (inout appdata v)
            {
           		const float fadeOut= saturate((_maxDist - distance(mul(unity_ObjectToWorld, v.vertex), _WorldSpaceCameraPos)) / (_maxDist * 0.7f));
                float d = tex2Dlod(_DispTex, float4(v.texcoord.xy * _DispTex_ST.xy + _DispTex_ST.zw,0,0)).r * _Displacement;
                d = d * 0.5 - 0.5 +_DispOffset;
                v.vertex.xyz += v.normal * d * fadeOut;
            }

            struct Input {
                float2 uv_MainTex;
                float2 uv_MOS;
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            fixed4 _Color;
            float _Metallic;
            float _Glossiness;

            void surf (Input IN, inout SurfaceOutputStandard o) {
                half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
                half4 mos = tex2D (_MOS, IN.uv_MOS);

                o.Albedo = c.rgb;
                o.Metallic = mos.r * _Metallic;
                o.Smoothness = mos.b *_Glossiness;
                o.Occlusion = mos.g;
                o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
            }
            ENDCG
        }
        FallBack "Standard"
    }