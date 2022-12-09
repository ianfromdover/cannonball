// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/ObjectMasking"
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
                //fixed4 col = tex2D(_Tex, i.texcoord * 10.0f) * tex2D(_Mask, i.texcoord);
                //fixed4 maskInverse = 1 - tex2D(_Mask, i.texcoord);
                //fixed4 col = tex2D(_CameraTex, i.texcoord);
                fixed4 col = tex2D(_SegmentationCameraTex, i.texcoord) * tex2D(_Mask, i.texcoord);
                return col;
            }

            ENDCG
        }
    }
}



