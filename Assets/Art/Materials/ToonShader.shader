Shader "Custom/Toon"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex("Texture",2D)= "White"{}
        _RampTex("Ramp",2D) = "White"{}
        _AlphaValue("Alpha",Range(0,1))=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon
        sampler2D _MainTex;
        sampler2D _RampTex;
        fixed4 _Color;
        float _AlphaVlue;

        struct Input
        {
            float2 uv_MainTex;
        };
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb*_Color;
        }

        fixed4 LightingToon(SurfaceOutput s, fixed3 lightDir, fixed atten)//빛의 방향과 표면의 법선에 대한 내적 계산
        {
            half NdotL = dot(s.Normal, lightDir);//NdotL을 램프 맵의 값으로 다시 매핑
            NdotL = tex2D(_RampTex, fixed2(NdotL, 0.5));

            half4 color;//반환할 색
            color.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
            color.a = s.Alpha;

            return color;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
