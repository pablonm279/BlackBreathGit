Shader "GDD/Campania/CaminoSueloRework"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 1)) = 0.22
        _EmissionMap ("Emission", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        _BiomeTint ("Biome Tint", Color) = (0.46, 0.35, 0.23, 1)
        _EdgeFeather ("Edge Feather", Range(0.08, 0.5)) = 0.28
        _EdgeBreakup ("Edge Breakup", Range(0, 0.5)) = 0.22
        _RutStrength ("Rut Strength", Range(0, 0.65)) = 0.16
        _MacroVariation ("Macro Variation", Range(0, 0.3)) = 0.12
        _Glossiness ("Smoothness", Range(0, 1)) = 0.035
        _Metallic ("Metallic", Range(0, 1)) = 0

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
            "Queue" = "Transparent-20"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 250
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade keepalpha noshadow noforwardadd
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _EmissionMap;
        fixed4 _Color;
        fixed4 _EmissionColor;
        fixed4 _BiomeTint;
        half _BumpScale;
        half _Glossiness;
        half _Metallic;
        half _EdgeFeather;
        half _EdgeBreakup;
        half _RutStrength;
        half _MacroVariation;
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
            float3 worldPos;
            float4 screenPos;
            float4 color : COLOR;
        };

        float HashSuave(float2 posicion)
        {
            float2 celda = floor(posicion);
            float2 fraccion = frac(posicion);
            fraccion = fraccion * fraccion * (3.0 - 2.0 * fraccion);

            float a = frac(sin(dot(celda, float2(127.1, 311.7))) * 43758.5453);
            float b = frac(sin(dot(celda + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
            float c = frac(sin(dot(celda + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
            float d = frac(sin(dot(celda + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);
            return lerp(lerp(a, b, fraccion.x), lerp(c, d, fraccion.x), fraccion.y);
        }

        void surf(Input entrada, inout SurfaceOutputStandard salida)
        {
            fixed4 textura = tex2D(_MainTex, entrada.uv_MainTex);
            float ruidoMacro = HashSuave(entrada.worldPos.xz * 0.72);
            float ruidoBorde = HashSuave(entrada.worldPos.xz * 3.1 + entrada.color.b * 7.3);

            // color.r vale 0 en los bordes y 1 en el centro de la calzada.
            float mascaraBorde = smoothstep(
                0.015,
                max(0.03, _EdgeFeather),
                entrada.color.r + (ruidoBorde - 0.52) * _EdgeBreakup);

            // Dos rodadas integradas en el material, sin mallas transparentes apiladas.
            float coordenadaAncho = entrada.color.g;
            float rodadaIzquierda = 1.0 - smoothstep(0.035, 0.105, abs(coordenadaAncho - 0.31));
            float rodadaDerecha = 1.0 - smoothstep(0.035, 0.105, abs(coordenadaAncho - 0.69));
            float rodadas = saturate(rodadaIzquierda + rodadaDerecha);
            float desgasteRecorrido = lerp(0.72, 1.0, saturate(_CaminoRecorrido));
            float oscurecimientoRodadas = 1.0 - rodadas * _RutStrength * desgasteRecorrido;

            fixed3 colorBase = textura.rgb * _Color.rgb;
            fixed3 colorBioma = colorBase * _BiomeTint.rgb * 1.62;
            colorBase = lerp(colorBase, colorBioma, 0.24);
            colorBase *= lerp(1.0 - _MacroVariation, 1.0 + _MacroVariation, ruidoMacro);
            colorBase *= oscurecimientoRodadas;

            // Un centro levemente compactado rompe la lectura de cinta uniforme.
            float centro = 1.0 - smoothstep(0.02, 0.34, abs(coordenadaAncho - 0.5));
            colorBase *= lerp(1.0, 0.94, centro * (0.35 + 0.35 * saturate(_CaminoRecorrido)));

            float2 uvPantalla = entrada.screenPos.xy / max(entrada.screenPos.w, 0.0001);
            float2 radios = max(_CaminoVisionPantalla.zw, float2(0.0001, 0.0001));
            float distanciaVision = length((uvPantalla - _CaminoVisionPantalla.xy) / radios);
            float suavizadoVision = clamp(_CaminoVisionSuavizado, 0.001, 0.45);
            float dentroVision = 1.0 - smoothstep(
                1.0 - suavizadoVision,
                1.0 + suavizadoVision,
                distanciaVision);

            float visible = max(dentroVision, saturate(_CaminoRecorrido));
            visible = lerp(visible, 1.0, saturate(_CaminoVisionDebug));
            visible = lerp(1.0, visible, saturate(_CaminoVisionActiva));
            clip(visible - 0.001);

            fixed3 normal = UnpackNormal(tex2D(_BumpMap, entrada.uv_BumpMap));
            normal.xy *= min(_BumpScale, 0.32);

            salida.Albedo = colorBase;
            salida.Normal = normalize(normal);
            salida.Metallic = min(_Metallic, 0.02);
            salida.Smoothness = min(_Glossiness, 0.055);
            salida.Occlusion = lerp(1.0, 0.82, rodadas * 0.28);
            salida.Emission = tex2D(_EmissionMap, entrada.uv_EmissionMap).rgb
                * _EmissionColor.rgb
                * lerp(0.72, 1.0, mascaraBorde);
            salida.Alpha = textura.a * _Color.a * mascaraBorde * visible * entrada.color.a;
        }
        ENDCG
    }

    Fallback Off
}
