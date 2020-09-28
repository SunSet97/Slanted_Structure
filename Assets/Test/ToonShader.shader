Shader "ToonShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _ClothTex("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        cull front
        //1st pass

        CGPROGRAM
        #pragma surface surf Nolight vertex:vert noshadow noambient

        void vert(inout appdata_full v)
        {
            v.vertex.xyz = v.vertex.xyz + v.normal.xyz * 0.01;
        }

        struct Input
        {
            float4 color:Color;
        };


        void surf (Input IN, inout SurfaceOutput o)
        {
        }
        float4 LightingNolight(SurfaceOutput s, float3 lightDir, float atten) 
        {
            return float4(0, 0, 0, 1);

        }
        ENDCG

        cull back
        //2nd pass
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert 

        sampler2D _MainTex;
        sampler2D _ClothTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_ClothTex;
        };


        void surf (Input IN, inout SurfaceOutput o)
        {
          
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 d = tex2D(_ClothTex, IN.uv_ClothTex);
            o.Albedo = c.rgb + d.rgb;
            o.Alpha = c.a + d.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
