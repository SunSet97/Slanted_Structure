Shader "Custom/ToonFace"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _MainTex("Albedo(RGB)",2D) = "White"{}
        _BrightDark("Brightness$Darkness",Range(-1,1)) = 0
        _RampTex("Ramp",2D) = "White"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        cull back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon
        sampler2D _MainTex;
        float _CelShadingLevels;
        sampler2D _RampTex;
        float _BrightDark;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;//뷰벡터
        };
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
            o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;
        }
        //view벡터는 외각선 전용, Light벡터는 음영전용
        fixed4 LightingToon(SurfaceOutput s, float3 lightDir, float3 viewDir, float3 atten)//빛의 방향과 표면의 법선에 대한 내적 계산
        {  
            //Half-Lambert공식을 활용하여 음영을 부드럽게 만들어줌.하지만 너무 부드러워서 pow()함수를 이용하여 3제곱을 해준다.
            float ndotl = dot(s.Normal, lightDir) * 0.5 + 0.5;//빛의 방향과 표면의 법선에 대해 내적한 뒤, 전반적인 그래프의 값을 올려주고 모두 양수로 만든다.(그래프가 보다 더 완만해지게 됨)
            float4 Ramp = tex2D(_RampTex, fixed2(ndotl, 0.5));//NdotL을 램프 맵의 값으로 다시 매핑한다. 

            float rim = abs(dot(s.Normal, viewDir));//외각선 내각을 절대값으로 변경
            if (rim > 0.3) 
            {
                Ramp = Ramp * 1.1;
            }
            else
            {
                Ramp= -1;
            }
            
            float4 final;
            final.rgb = s.Albedo*Ramp.rgb* _BrightDark;
            final.a = s.Alpha;
            return final;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
