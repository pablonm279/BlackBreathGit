Shader "GDD/Campania/CaminoVision"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.01
        _Glossiness ("Smoothness", Range(0, 1)) = 0.05
        _Metallic ("Metallic", Range(0, 1)) = 0
        _BumpScale ("Normal Scale", Float) = 1
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1
        _OcclusionMap ("Occlusion", 2D) = "white" {}

        [HideInInspector] _CaminoRecorrido ("Camino recorrido", Float) = 0
        [HideInInspector] _Mode ("Mode", Float) = 2
        [HideInInspector] _SrcBlend ("SrcBlend", Float) = 5
        [HideInInspector] _DstBlend ("DstBlend", Float) = 10
        [HideInInspector] _ZWrite ("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 200
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade keepalpha noshadow noforwardadd
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _EmissionMap;
        sampler2D _OcclusionMap;
        fixed4 _Color;
        fixed4 _EmissionColor;
        half _Glossiness;
        half _Metallic;
        half _BumpScale;
        half _OcclusionStrength;
        half _CaminoRecorrido;

        // Valores globales actualizados por NieblaGuerraCaravana.
        float4 _CaminoVisionPantalla;
        float _CaminoVisionSuavizado;
        float _CaminoVisionActiva;
        float _CaminoVisionDebug;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_EmissionMap;
            float2 uv_OcclusionMap;
            float4 screenPos;
        };

        void surf(Input entrada, inout SurfaceOutputStandard salida)
        {
            fixed4 albedo = tex2D(_MainTex, entrada.uv_MainTex) * _Color;
            float2 uvPantalla = entrada.screenPos.xy / max(entrada.screenPos.w, 0.0001);
            float2 radios = max(_CaminoVisionPantalla.zw, float2(0.0001, 0.0001));
            float distancia = length((uvPantalla - _CaminoVisionPantalla.xy) / radios);
            float suavizado = clamp(_CaminoVisionSuavizado, 0.001, 0.45);
            float dentroVision = 1.0 - smoothstep(
                1.0 - suavizado,
                1.0 + suavizado,
                distancia);

            float visible = max(dentroVision, saturate(_CaminoRecorrido));
            visible = lerp(visible, 1.0, saturate(_CaminoVisionDebug));
            visible = lerp(1.0, visible, saturate(_CaminoVisionActiva));

            // Sólo descarta la zona completamente exterior; conserva el feather.
            clip(visible - 0.001);

            fixed3 normal = UnpackNormal(tex2D(_BumpMap, entrada.uv_BumpMap));
            normal.xy *= _BumpScale;

            salida.Albedo = albedo.rgb;
            salida.Normal = normalize(normal);
            salida.Metallic = _Metallic;
            salida.Smoothness = _Glossiness;
            salida.Occlusion = lerp(
                1.0,
                tex2D(_OcclusionMap, entrada.uv_OcclusionMap).g,
                _OcclusionStrength);
            salida.Emission = tex2D(_EmissionMap, entrada.uv_EmissionMap).rgb
                * _EmissionColor.rgb;
            salida.Alpha = albedo.a * visible;
        }
        ENDCG
    }

    Fallback Off
}
