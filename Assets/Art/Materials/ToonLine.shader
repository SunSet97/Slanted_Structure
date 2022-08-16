Shader "Custom/ToonLine"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex("Albedo(RGB)",2D)= "White"{}
        _RampTex("Ramp",2D) = "White"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        cull front

        LOD 200
        CGPROGRAM
        #pragma surface surf Nolight vertex:vert noshadow noambient
        void vert(inout appdata_full v)
        {
            v.vertex.xyz = v.vertex.xyz + v.normal.xyz * 0.015;
        }
        struct Input 
        {
            float4 color:COLOR;
        };
        
        void surf(Input IN, inout SurfaceOutput o){}
        float4 LightingNolight(SurfaceOutput s, float3 lightDir, float atten)
        { 
            return float4(0, 0, 0, 1);
        }
        ENDCG

        cull back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon
        sampler2D _MainTex;
        sampler2D _RampTex;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb*_Color;
            o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;
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
