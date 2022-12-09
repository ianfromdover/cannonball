// Copyright 2022 Niantic, Inc. All Rights Reserved.

Shader "LightshipHub/MeshingShaders/Gradient"
{
    Properties
    {
        _MainTex ("ColorRamp", 2D) = "white" {}
        _Start ("Start", float) = 0.1
        _Distance ("Range", float) = 5.0
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float depth: float;
            };

            float _Distance;
            float _Start;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.depth = abs(distance(_WorldSpaceCameraPos,worldPos))/_Distance;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float rampPosition = i.depth-_Start;
                fixed4 col = tex2D(_MainTex, float2(rampPosition,0.5f));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
