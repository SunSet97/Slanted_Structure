Shader "Custom/Water_CelShader"
{
    Properties
    {
        _CubeMap("CubeMap",cube)=""{}
        _MainTex("MainTex",2D)="white"{}
        _CelShadingLevels("Levels",Range(0,1))=0.51
        _BumpMap ("Water Bump", 2D) = "bump" {}
        _BumpMap2 ("Water Bump2", 2D) = "bump" {}
        _SpacPow ("Specular",float)=2
        _WaveSpeed("Wave Speed", float) = 0.05
        _WavePower("Wave Power", float) = 0.2
        _WaveTilling("Wave Tilling", float) = 25
        _NormalTiling("Normal Tile", float) = 1

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

        Pass{
            cull back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata {
            float4 pos : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 posWorld : TEXCOORD1;
                float4 posObject : TEXCOORD2;
                float4 screenPos: TEXCOORD3;

                half3 tspace0 : TEXCOORD4; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD5; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD6; // tangent.z, bitangent.z, normal.z

                float3 viewDir : VIEW_DIR;
            };

            sampler2D _BumpMap,_BumpMap2;
            sampler2D _MainTex,_GrabTexture;
            float4 _MainTex_ST;
            samplerCUBE _CubeMap;
            float _NormalTiling;
            float _CelShadingLevels;
            float _SpacPow;
            float dotData;
            float _WaveSpeed;
            float _WavePower;
            float _WaveTilling;

            v2f vert (appdata v,float4 tangent : TANGENT)
            {
                
                v2f o;
                o.posObject=v.pos;
                o.posWorld=mul(unity_ObjectToWorld ,o.posObject);
                o.normal =UnityObjectToWorldNormal(v.normal);
                v.pos.y+=sin((abs(v.uv.x*2-1)*_WaveTilling)+sin(_Time.y*1))*_WavePower;//abs함수로 감싸면 -1~0~1이 1~0~1으로 바뀌면서 지그재그 됨.
                o.pos =  UnityObjectToClipPos(v.pos);

                half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(o.normal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, o.normal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, o.normal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, o.normal.z);

                o.viewDir= normalize(UnityWorldSpaceViewDir(o.posWorld));
                
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                
                o.screenPos=ComputeScreenPos(o.pos);    
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {  
                fixed4 _texColor;
                float4 fNormal1=tex2D(_BumpMap,_NormalTiling*(i.uv)+float2(_Time.y*_WaveSpeed,0.0f));
                float4 fNormal2=tex2D(_BumpMap2,_NormalTiling*(i.uv)-float2(_Time.y*_WaveSpeed,0.0f));
                half3 tnormal=UnpackNormal((fNormal1+fNormal2)/2);
                i.normal.x=dot(i.tspace0, tnormal);
                i.normal.y=dot(i.tspace1, tnormal);
                i.normal.z=dot(i.tspace2, tnormal);

                float attenuation;
                float3 lightDir;
                attenuation=1.0;
                lightDir=normalize(WorldSpaceLightDir(i.posObject));
               

                

                //불연속적인 명암 설정
                
                float NdotL = pow(dot(i.normal, lightDir) * 0.5 + 0.5,1.2);//pow(x,y)>x의 y승//0~1의 사이 값으로 바꾼 뒤 3승
                //빛의 방향과 표면의 법선에 대해 내적한 뒤, 전반적인 그래프의 값을 올려주고 모두 양수로 만든다.(그래프가 보다 더 완만해지게 됨)
                half cel = floor((NdotL * _CelShadingLevels) / (_CelShadingLevels-0.5));//ramp파일 대신 스냅
                
                
                
                half3 worldRefl = reflect(-i.viewDir, i.normal);

                // same as in previous shader
                half4 skyData = texCUBE(_CubeMap,worldRefl);//UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);


                //grab
                float4 fNoise=tex2D(_MainTex,i.uv+_Time.x);


                //light
                float3 refVec=i.normal*dot(i.normal,i.viewDir)*2-i.viewDir;
                refVec=normalize(refVec);
                dotData=pow(saturate(1-dot(i.normal,i.viewDir)),0.6);
                //rim
                float frim=saturate(dot(i.normal,i.viewDir));
                float frim1=pow((1-frim),50);
                float frim2=pow((1-frim),2);//프레넬 마스킹용(알파)
                float spcr=lerp(0,pow(saturate(dot(refVec,lightDir)),256),dotData)*_SpacPow;
                
                /* float3 halfwayDirection = normalize(lightDir + i.viewDir);
                float w = pow(1.0 - max(0.0, dot(halfwayDirection, i.viewDir)), 16.0);
                float3 specularReflection = float3(_LightColor0.rgb)* lerp(float3(_SpecColor.rgb),float3(1,1,1), w) 
                * pow(max(0.01, dot(reflect(-lightDir, i.normal), i.viewDir)), _SpacPow); */



                //refraction
                float3 fGrab=tex2D(_GrabTexture, (i.screenPos/(i.screenPos.a+0.0000001)).xy + i.normal.xy*0.1*fNoise);//(_GrabTexture,scrPos.xy+fNoise.r*0.05);
                
                float3 water=lerp(fGrab,skyColor*cel,dotData).rgb;//lerp(fGrab,fRefl,pow(dot(o.Normal,IN.viewDir),dotData)).rgb;

                _texColor.rgb =(water)*cel;
                _texColor.a=1;
                return _texColor+cel*float4((_LightColor0.rgb*spcr),frim1);
                //return _texColor+_LightSpec;
            }ENDCG
        }

        
    }
}
