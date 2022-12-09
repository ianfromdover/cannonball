// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/MeshingShaders/Grid"
{
    Properties
    {
        _BaseColor ("Main Color", Color) = (0,1,1,.5)
        _GridSize ("Grid Size", float) = 5.0
        _Speed ("Speed", float) = 0.5
        _Width ("Line Width", Range(0.0, 1.0)) = 0.05
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
                float3 worldPos: float3;
            };

            float4 _BaseColor;
            float _Speed;
            float _GridSize;
            float _Width;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float cursor = (_Time.y*_Speed)%1.0f;
                i.worldPos-=cursor*.1;
                i.worldPos*=10;
                float gridSize = _GridSize;

                float x = 1-abs(abs(i.worldPos.x%gridSize)*2-1); 
                float y = 1-abs(abs(i.worldPos.y%gridSize)*2-1); 
                float z = 1-abs(abs(i.worldPos.z%gridSize)*2-1);

                float grid = max(max(x,y),z);
                grid = smoothstep(1-_Width,1-_Width/2,grid);
                float4 col = float4(1,1,1,grid);
                col *= _BaseColor;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
