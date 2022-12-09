Shader "Custom/SemanticOverlay"
{
    Properties {
        _MainTex ("Main Texture", 2D) = "" {}
        _Overlay ("Semantic Texture", 2D) = "black" {}
    }
 
    SubShader {
 
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
       
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Fog { Mode Off }
       
        Pass {  
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
 
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 color_uv : TEXCOORD0;
                float3 semantic_uv : TEXCOORD1;
            };
 
            sampler2D _MainTex;
            sampler2D _textureSemantic;
            
            float4x4 _semanticsTransform;
           
            v2f vert (appdata_t v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color_uv = v.uv;
                o.semantic_uv = mul(_semanticsTransform, float4(v.uv, 1.0f, 1.0f)).xyz;
                
                return o;
            }
 
            fixed4 frag (v2f i) : COLOR
            {
                // Get the raw texture value
                float4 texColor = tex2D(_MainTex, i.color_uv);

                // Get euclidean texture coordinates
                const float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);

                // Read the semantic texture pixel                     
                float3 semColor = tex2D(_textureSemantic, semanticUV).rgb;

                // Highlight pixel
                texColor.rgb = saturate(texColor.rgb * (1.0f - semColor.r) + semColor);

                return texColor;
            }
            
            ENDCG
        }
    }  
 
    Fallback off
}
