Shader "Custom/Point"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:procedural

        #pragma target 4.5

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        float _Step;

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)

        StructuredBuffer<float3> _Positions;

        #endif

        void procedural()
        {
            #if defined (UNITY_PROCEDURAL_INSTANCING_ENABLED)
            
            float3 position = _Positions[unity_InstanceID];
            
            unity_ObjectToWorld = 0.0;
            unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
            unity_ObjectToWorld._m00_m11_m22 = _Step;
            
            #endif
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo.rg = saturate(IN.worldPos * 0.5 + 0.5);

            o.Smoothness = 0.5;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}