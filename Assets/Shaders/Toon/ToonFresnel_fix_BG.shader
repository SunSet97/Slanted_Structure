Shader "Custom/ToonFresnel_fix_BG"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex("Albedo(RGB)",2D) = "White"{}
        _Shininess("Shininess",Range(0,128))=1
        _CelShadingLevels("Levels",Range(0,1))=0.51
        _Outline("Outline Width",Range(0,1))=0.3
        _BrightDark("Brightness$Darkness",Range(-1,1))=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
        LOD 100

        Pass{
            cull back

            CGPROGRAM
            #pragma multi_compile_fwdbase DIRECTIONAL POINT SPOT novertexlight
            #pragma vertex vert
            #pragma fragment frag
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
                SHADOW_COORDS(1)
                float3 normal : NORMAL;
                float4 posWorld : TEXCOORD2;
                float4 posObject : TEXCOORD3;
                float3 viewDir : VIEW_DIR;
                

            };

            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CelShadingLevels;
            fixed4 _Color;
            float _BrightDark;
            float _Shininess;
            float _Outline;



            v2f vert (appdata v)
            {
                v2f o;
                o.pos =  UnityObjectToClipPos(v.pos);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                TRANSFER_SHADOW(o);
                o.posWorld=mul(unity_ObjectToWorld ,v.pos);
                o.posObject=v.pos;
                
                o.viewDir= normalize(WorldSpaceViewDir(v.pos));
                o.normal =UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {  
                fixed4 _texColor=tex2D( _MainTex, i.uv)*_Color; 
                float attenuation;
                float3 lightDir;
                float3 lightDir_spotPoint;
                //Light의 종류 파악
                if(_WorldSpaceLightPos0.w==0 ){//Directional Light
                    attenuation=1.0;
                    lightDir=normalize(WorldSpaceLightDir(i.posObject));
                }
                else{//point or spot light
                    float3 vertexToLightSource=_WorldSpaceLightPos0.rgb-i.posWorld.rgb;
                    vertexToLightSource=normalize(vertexToLightSource);
                    float distance=length(vertexToLightSource.rgb);
                    attenuation=1.0/(0.0001+distance);
                    lightDir=vertexToLightSource;
                    
                }
                //Diffuse Reflection
                float3 diffuseRefleciton=_texColor.rgb*_LightColor0.rgb*attenuation;

                //SPECULAR Reflection
                float3 specularReflection;
                if(dot(i.normal,lightDir)<0){
                    specularReflection=float3(0,0,0);
                }
                else{
                    float3 halfwayDirection = normalize(lightDir + i.viewDir);
                    float w = pow(1.0 - max(0.0, dot(halfwayDirection, i.viewDir)), 5.0);
                    specularReflection = attenuation*float3(_LightColor0.rgb)* lerp(float3(_SpecColor.rgb),float3(1,1,1), w) 
                    * pow(max(0.01, dot(reflect(-lightDir, i.normal), i.viewDir)), _Shininess);

                }

                //불연속적인 명암 설정
                float3 WV_Normal=mul(UNITY_MATRIX_MV,i.normal);
                float3 WV_viewDir=mul(UNITY_MATRIX_MV,i.viewDir);
                float NdotL = pow(dot(i.normal, lightDir) * 0.5 + 0.5,1.2);//pow(x,y)>x의 y승//0~1의 사이 값으로 바꾼 뒤 3승
                //빛의 방향과 표면의 법선에 대해 내적한 뒤, 전반적인 그래프의 값을 올려주고 모두 양수로 만든다.(그래프가 보다 더 완만해지게 됨)
                half cel = floor((NdotL * _CelShadingLevels) / (_CelShadingLevels-0.5));//ramp파일 대신 스냅
                //Outline설정
                float rim = dot(WV_Normal, WV_viewDir);//외각선 내각을 절대값으로 변경
                if (rim > _Outline) 
                {
                }
                else//rim이 작으면 노말벡터와 외각선 벡터가 수직에 가까움
                {
                    cel = -1;//아웃라인
                }
                _texColor.rgb =(diffuseRefleciton+specularReflection+_BrightDark)*cel;
                _texColor.a=_texColor.a;
                _texColor.rgb*=SHADOW_ATTENUATION(i);
                return _texColor;
            }
        ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}
