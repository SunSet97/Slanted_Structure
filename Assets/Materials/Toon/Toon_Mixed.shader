﻿Shader "Custom/Toon_Mixed"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex1("1_Albedo(RGB)",2D) = "White"{}
        _MainTex2("2_Albedo(RGB)",2D) = "White"{}
        _CelShadingLevels("Levels",Range(0,1))=0.8
        _BrightDark("Brightness$Darkness",Range(-1,1))=0
        _lerpTest("Lerp",Range(0,1))=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        cull back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon
        sampler2D _MainTex1;
        sampler2D _MainTex2;
        float _CelShadingLevels;
        fixed4 _Color;
        float _BrightDark;
        float _lerpTest;
        
        struct Input
        {
            float2 uv_MainTex1;
            float2 uv_MainTex2;
        };
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex1, IN.uv_MainTex1);
            fixed4 d = tex2D(_MainTex2, IN.uv_MainTex2);
            o.Albedo = lerp(c.rgb,d.rgb,_lerpTest) * _Color;
            o.Alpha =c.a;
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
            final.rgb = s.Albedo * _LightColor0.rgb *(cel*atten)+_BrightDark;
            final.a = s.Alpha;
            return final;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
