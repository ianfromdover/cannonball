// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/OptimizedObjectMasking"
{
    Properties 
    {
        _MainTex("_MainTex", 2D) = "white" {}
        _CameraTex("_CameraTex", 2D) = "white" {}
        _SegmentationCameraTex("_SegmentationCameraTex", 2D) = "white" {}
        _Mask("_Mask", 2D) = "white" {}
    }

    SubShader 
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
       
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
       
        Pass 
        {
            CGPROGRAM

            #pragma vertex vert alpha
            #pragma fragment frag alpha

            #include "UnityCG.cginc"

            struct appdata_t 
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 vertex  : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _CameraTex;
            sampler2D _SegmentationCameraTex;
            sampler2D _Mask;

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;

                o.vertex     = UnityObjectToClipPos(v.vertex);
                o.texcoord   = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            
                float invAspect = 1080.0f / 1920.0f;
                float mask = 0;
                float _BlurSize = .001f;
                int count = 8;

                for(float index = 0; index < count; index++){
                    mask += tex2D(_Mask, i.texcoord + float2(_BlurSize*index, 0)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(-_BlurSize*index, 0)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(0,_BlurSize*index)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(0,-_BlurSize*index)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(_BlurSize*index,_BlurSize*index)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(-_BlurSize*index,-_BlurSize*index)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(_BlurSize*index,-_BlurSize*index)).a;
                    mask += tex2D(_Mask, i.texcoord + float2(-_BlurSize*index,_BlurSize*index)).a;
                }

                float original = tex2D(_Mask, i.texcoord).a;
                mask = mask / (8*count+0)*2.;

                if(original>0.0f)
                {
                    mask = mask*mask;
                }
                
                fixed4 col = tex2D(_SegmentationCameraTex, i.texcoord) * float4(1,1,1,mask);
                return col;
            }

            ENDCG
        }
    }
}



