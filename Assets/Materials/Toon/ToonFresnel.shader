﻿Shader "Custom/ToonFresnel"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _BumpMap("NormalMap",2D) = "bump"{}
        _MainTex("Albedo(RGB)",2D) = "White"{}
        _RampTex("Ramp",2D) = "White"{}
        _CelShadingLevels("Levels",Range(0,1))=0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        cull back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _RampTex;
        float _CelShadingLevels;
        fixed4 _Color;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;//뷰벡터
        };
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb*_Color;
            o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;
        }
        //view벡터는 외각선 전용, Light벡터는 음영전용
        fixed4 LightingToon(SurfaceOutput s, float3 lightDir, float3 viewDir, float3 atten)//빛의 방향과 표면의 법선에 대한 내적 계산
        {  
            float NdotL = pow(dot(s.Normal, lightDir) * 0.5 + 0.5,3);
            //빛의 방향과 표면의 법선에 대해 내적한 뒤, 전반적인 그래프의 값을 올려주고 모두 양수로 만든다.(그래프가 보다 더 완만해지게 됨)
            half cel = floor((NdotL * _CelShadingLevels) / (_CelShadingLevels - 0.5));//ramp파일 대신 스냅

            float rim = abs(dot(s.Normal, viewDir));//외각선 내각을 절대값으로 변경
            if (rim > 0.3) 
            {
            }
            else
            {
                cel = -1;
            }
            float4 final;
            final.rgb = s.Albedo * _LightColor0.rgb *(cel*atten);
            final.a = s.Alpha;
            return final;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
