// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/MeshingShaders/Waves"
{
    Properties
    {
        _BaseColor ("Main Color", Color) = (1,1,1,.5)
        _Distance ("Range", float) = 5.0
        _Speed ("Speed", float) = 0.5
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
                float3 worldPos: float3;
            };

            float4 _BaseColor;
            float _Distance;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.depth = abs(distance(_WorldSpaceCameraPos,worldPos))/_Distance;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = i.depth;
                float cursor = _Time.y*_Speed%1.0f;
                float noise = sin((i.worldPos.x)*3.14158*2.0);
                noise *= cos((i.worldPos.y*.2)*3.14158*2.0);
                noise *= cos((i.worldPos.z*.5)*3.14158*2.0);
                float wave = noise*.5+.5;
                cursor += wave*.2;

                float waveRange = abs(depth-cursor);
                waveRange *= 1.0f/_Distance;
                waveRange %= 1.0f/_Distance;
                waveRange *= 2;
                waveRange -= 1;
                waveRange = 1-abs(waveRange);
                waveRange = smoothstep(0+wave*.3,1-wave*.2,waveRange);
                float4 col = _BaseColor * float4(1,1,1,(1.0f-waveRange)*pow(wave,2));

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
