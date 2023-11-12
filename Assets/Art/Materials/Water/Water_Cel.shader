Shader "Custom/Water1"
{
    Properties
    {
        _CubeMap("CubeMap",cube)=""{}
        _MainTex("MainTex",2D)="white"{}
        _BumpMap ("Water Bump", 2D) = "bump" {}
        _BumpMap2 ("Water Bump2", 2D) = "bump" {}
        _SpacPow ("Specular",float)=2
        _WaveSpeed("Wave Speed", float) = 0.05
        _WavePower("Wave Power", float) = 0.2
        _WaveTilling("Wave Tilling", float) = 25

    }
    CGINCLUDE
        #define _GLOSSYENV 1
        #define UNITY_SETUP_BRDF_INPUT SpecularSetup
    ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200

        GrabPass{}

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf WLight vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _BumpMap,_BumpMap2;
        sampler2D _MainTex,_GrabTexture;
        samplerCUBE _CubeMap;
        float _SpacPow;
        float dotData;
        float _WaveSpeed;
        float _WavePower;
        float _WaveTilling;
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_BumpMap2;
            float3 worldRefl;
            float3 viewDir;
            float4 screenPos;
            INTERNAL_DATA
        };

        void vert(inout appdata_full v)
        {
            v.vertex.y+=sin((abs(v.texcoord.x*2-1)*_WaveTilling)+sin(_Time.y*1))*_WavePower;//abs함수로 감싸면 -1~0~1이 1~0~1으로 바뀌면서 지그재그 됨.

        }
        

        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            //Normal
            float4 fNormal1=tex2D(_BumpMap,IN.uv_BumpMap+float2(_Time.y*_WaveSpeed,0.0f));
            float4 fNormal2=tex2D(_BumpMap,IN.uv_BumpMap-float2(_Time.y*_WaveSpeed,0.0f));
            o.Normal=UnpackNormal((fNormal1+fNormal2)/2);

            //불연속적인 명암설정
            /* float3 lightDir=normalize(WorldSpaceLightDir(i.posObject));
            float NdotL= pow(dot(o.normal, lightDir) * 0.5 + 0.5,1.2);
 */
            //Sky reflection
            float3 fRefl = texCUBE(_CubeMap,WorldReflectionVector(IN,o.Normal));//sky
            
            //grab
            float4 fNoise=tex2D(_MainTex,IN.uv_MainTex+_Time.x);

            //refraction
            float3 fGrab=tex2D(_GrabTexture, (IN.screenPos/(IN.screenPos.a+0.0000001)).xy + o.Normal.xy*0.03*fNoise);//(_GrabTexture,scrPos.xy+fNoise.r*0.05);
            dotData=pow(saturate(1-dot(o.Normal,IN.viewDir)),0.6);
            o.Gloss=1;
            float3 water=lerp(fGrab,fRefl,dotData).rgb;//lerp(fGrab,fRefl,pow(dot(o.Normal,IN.viewDir),dotData)).rgb;
            
            o.Albedo=water;

            
        }
        float4 LightingWLight(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
        {
            float3 refVec=s.Normal*dot(s.Normal,viewDir)*2-viewDir;
            refVec=normalize(refVec);
            
            //rim
            float frim=saturate(dot(s.Normal,viewDir));
            float frim1=pow((1-frim),20);
            float frim2=pow((1-frim),2);//프레넬 마스킹용(알파)

            float spcr=lerp(0,pow(saturate(dot(refVec,lightDir)),128),dotData)*_SpacPow;
            return float4(s.Albedo+spcr.rrr,frim2);


        }
        ENDCG
    }
    FallBack "Diffuse"
}
