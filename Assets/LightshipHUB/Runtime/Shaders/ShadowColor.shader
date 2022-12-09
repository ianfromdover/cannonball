// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LighshipHub/ShadowColor"
{
    Properties
    {
        _Color ("Main Color", Color) = (1.000000,1.000000,1.000000,1.000000)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "Queue" = "Transparent" }
       
        ZWrite Off
        Blend One OneMinusSrcAlpha
        ColorMask RGB
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x,v.vertex.y,v.vertex.z*0.001,v.vertex.w));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
