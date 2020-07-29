// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RiverMaterial"
{
    Properties
    {
        _Color ("색상", Color) = (1,1,1,1)
        _MainTex ("텍스쳐", 2D) = "white" {}
        _Multiple("강 속도 배율",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
    
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Multiple;
    
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f{
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target{
                float depth = i.uv.x;
                fixed4 col = tex2D(_MainTex, i.uv - float2(0,_Time.y * _Multiple));
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                if(depth >= 0.5f){
                    return col - float4(0,0,0,0.5f) + _Color * (depth - 0.5f) / 2;
                }else{
                    return col - float4(0,0,0,0.5f) + _Color * (0.5f - depth) / 2;
                }                 
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
