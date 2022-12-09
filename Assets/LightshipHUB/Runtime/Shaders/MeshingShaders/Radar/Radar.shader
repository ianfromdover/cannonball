// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/MeshingShaders/Radar"
{
    Properties
    {
        _BaseColor ("Main Color", Color) = (0,1,1,.5)
        _Distance ("Range", float) = 5.0
        _Speed ("Speed", float) = 0.5
        _Width ("Width", Range(0.0, 1.0)) = 0.05
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #define TAU 3.14158*2.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float depth: float;
            };

            float4 _BaseColor;
            float _Distance;
            float _Speed;
            float _Width;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.depth = abs(distance(_WorldSpaceCameraPos,worldPos))/_Distance;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = i.depth;
                float cursor = _Time.y*_Speed%1.0f;
                float radar = abs(depth-cursor);
                radar *= 1.0f/_Width;
                radar %= 1.0f/_Width;
                radar = clamp(radar,0.0f,1.0f);
                radar *= radar;
                float4 col = _BaseColor * float4(1,1,1,1.0f-radar);

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
