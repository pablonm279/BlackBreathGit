Shader "GDD/Campania/NodoVisualForward"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows exclude_path:deferred
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _EmissionColor;
        half _Metallic;
        half _Glossiness;

        struct Input
        {
            float3 worldPos;
        };

        void surf(Input entrada, inout SurfaceOutputStandard salida)
        {
            salida.Albedo = _Color.rgb;
            salida.Metallic = _Metallic;
            salida.Smoothness = _Glossiness;
            salida.Emission = _EmissionColor.rgb;
            salida.Alpha = 1.0;
        }
        ENDCG
    }

    Fallback "Standard"
}
