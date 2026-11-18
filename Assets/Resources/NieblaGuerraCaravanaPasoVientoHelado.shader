Shader "Hidden/GDD/Campania/NieblaGuerraCaravanaPasoVientoHelado"
{
    Properties
    {
        _MainTex ("Escena", 2D) = "white" {}
        _FogColor ("Color", Color) = (0.030, 0.052, 0.092, 0.72)
        _HistoryTex ("Historial mundial", 2D) = "black" {}
        _VisionScreen ("Centro UV y radios", Vector) = (0.5, 0.5, 0.2, 0.14)
        _VisionFeather ("Suavizado del borde", Float) = 0.12
        _HistoryBounds ("Origen XZ e inversa de tamaño", Vector) = (0, 0, 1, 1)
        _HistoryStrength ("Claridad del historial", Range(0, 1)) = 0.50
        _Opacity ("Opacidad", Range(0, 1)) = 1
        _EdgeParams ("Borde vivo", Vector) = (0.085, 0.045, 1, 0)
        _EdgeTime ("Tiempo del borde", Float) = 0
        _EdgeWave ("Onda de apertura", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            sampler2D _HistoryTex;
            fixed4 _FogColor;
            float _PasoLuzDiurna;
            float _PasoBrumaInicio, _PasoBrumaFin;
            float4 _VisionScreen;
            float _VisionFeather;
            float4 _HistoryBounds;
            float _HistoryStrength;
            float _Opacity;
            float4 _EdgeWave;
            float4x4 _InverseViewProjection;

            v2f vert(appdata entrada)
            {
                v2f salida;
                salida.vertex = UnityObjectToClipPos(entrada.vertex);
                salida.uv = entrada.uv;
                return salida;
            }

            fixed4 frag(v2f entrada) : SV_Target
            {
                float2 uv = entrada.uv;
#if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0.0)
                {
                    uv.y = 1.0 - uv.y;
                }
#endif

                fixed4 escena = tex2D(_MainTex, uv);
                float profundidad = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
#if defined(UNITY_REVERSED_Z)
                if (profundidad <= 0.00001)
#else
                if (profundidad >= 0.99999)
#endif
                {
                    return escena;
                }

#if !defined(UNITY_REVERSED_Z)
                profundidad = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, profundidad);
#endif
                float4 posicionClip = float4(uv * 2.0 - 1.0, profundidad, 1.0);
                float4 posicionMundo = mul(_InverseViewProjection, posicionClip);
                float2 mundoXZ = posicionMundo.xz / posicionMundo.w;

                float2 radiosPantalla = max(_VisionScreen.zw, float2(0.001, 0.001));
                float suavizado = clamp(_VisionFeather, 0.001, 0.45);
                float distancia = length((uv - _VisionScreen.xy) / radiosPantalla);
                float visibleAhora = 1.0 - smoothstep(
                    1.0 - suavizado * 1.30,
                    1.0 + suavizado,
                    distancia);

                float progresoOnda = saturate(_EdgeWave.x);
                float centroOnda = 1.0 + progresoOnda * 0.22;
                float anchoOnda = lerp(0.032, 0.065, progresoOnda);
                float onda = 1.0 - smoothstep(
                    anchoOnda,
                    anchoOnda * 2.15,
                    abs(distancia - centroOnda));
                visibleAhora = max(visibleAhora, onda * _EdgeWave.y * 0.36);

                float2 uvHistorial =
                    (mundoXZ - _HistoryBounds.xy) * _HistoryBounds.zw;
                float dentroHistorial =
                    step(0.0, uvHistorial.x) * step(uvHistorial.x, 1.0)
                    * step(0.0, uvHistorial.y) * step(uvHistorial.y, 1.0);
                float explorado = tex2D(_HistoryTex, saturate(uvHistorial)).r
                    * dentroHistorial
                    * _HistoryStrength;
                float cantidadNiebla = 1.0 - max(visibleAhora, explorado);

                float alpha = saturate(
                    _FogColor.a * _Opacity * cantidadNiebla) * .91;
                // Bruma azulada diurna; conserva exactamente las mascaras de vision e historial.
                float dia = saturate(_PasoLuzDiurna);
                fixed3 bruma = _FogColor.rgb;
                alpha *= lerp(1.0,.92,dia);
                // Solo la extension mas alla del terreno jugable se funde con el fondo.
                alpha *= 1-smoothstep(_PasoBrumaInicio,max(_PasoBrumaInicio+.01,_PasoBrumaFin),mundoXZ.y);
                return fixed4(lerp(escena.rgb, bruma, alpha), escena.a);
            }
            ENDCG
        }
    }

    Fallback Off
}
