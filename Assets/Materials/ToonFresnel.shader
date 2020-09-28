Shader "Custom/ToonFresnel"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex("Albedo(RGB)",2D) = "White"{}
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

        fixed4 LightingToon(SurfaceOutput s, float3 lightDir, float3 viewDir, float3 atten)//빛의 방향과 표면의 법선에 대한 내적 계산
        {            
            float ndotl = dot(s.Normal, lightDir) * 0.5 + 0.5;
            ndotl= tex2D(_RampTex, fixed2(ndotl, 0.5));
            if (ndotl > 0.7) 
            {
                ndotl = 1;
            }
            else
            {
                ndotl = 0.3;
            }
            float rim = abs(dot(s.Normal, viewDir));
            if (rim > 0.3) 
            {
                rim = 1;
            }
            else
            {
                rim = -1;
            }
            float4 final;
            final.rgb = s.Albedo * ndotl * _LightColor0.rgb * rim;
            final.a = s.Alpha;
            return final;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
