Shader "Hidden/GDD/Campania/NieblaGuerraCaravana"
{
    Properties
    {
        _MainTex ("Escena", 2D) = "white" {}
        _FogColor ("Color", Color) = (0.0162, 0.0405, 0.0468, 0.6804)
        _HistoryTex ("Historial mundial", 2D) = "black" {}
        _VisionScreen ("Centro UV y radios", Vector) = (0.5, 0.5, 0.2, 0.14)
        _VisionFeather ("Suavizado del borde", Float) = 0.12
        _HistoryBounds ("Origen XZ e inversa de tamaño", Vector) = (0, 0, 1, 1)
        _HistoryStrength ("Claridad del historial", Range(0, 1)) = 0.50
        _Opacity ("Opacidad", Range(0, 1)) = 1
        _Band ("Franja jugable", Vector) = (0.12, 0.94, 0.075, 0)
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
            float4 _VisionScreen;
            float _VisionFeather;
            float4 _HistoryBounds;
            float _HistoryStrength;
            float _Opacity;
            float4 _Band;
            float4 _EdgeParams;
            float _EdgeTime;
            float4 _EdgeWave;
            float4x4 _InverseViewProjection;

            v2f vert(appdata entrada)
            {
                v2f salida;
                salida.vertex = UnityObjectToClipPos(entrada.vertex);
                salida.uv = entrada.uv;
                return salida;
            }

            float hash21(float2 punto)
            {
                punto = frac(punto * float2(123.34, 456.21));
                punto += dot(punto, punto + 45.32);
                return frac(punto.x * punto.y);
            }

            float ruido(float2 punto)
            {
                float2 celda = floor(punto);
                float2 local = frac(punto);
                local = local * local * (3.0 - 2.0 * local);

                float a = hash21(celda);
                float b = hash21(celda + float2(1.0, 0.0));
                float c = hash21(celda + float2(0.0, 1.0));
                float d = hash21(celda + float2(1.0, 1.0));
                return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
            }

            float nieblaFractal(float2 punto)
            {
                float valor = 0.0;
                valor += ruido(punto) * 0.55;
                valor += ruido(punto * 2.03 + 17.1) * 0.29;
                valor += ruido(punto * 4.01 - 9.7) * 0.16;
                return valor;
            }

            float3 ReconstruirPosicionMundo(float2 uv)
            {
                float profundidad = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
#if !defined(UNITY_REVERSED_Z)
                profundidad = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, profundidad);
#endif
                float4 posicionClip = float4(uv * 2.0 - 1.0, profundidad, 1.0);
                float4 posicionMundo = mul(_InverseViewProjection, posicionClip);
                return posicionMundo.xyz / posicionMundo.w;
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
                float3 posicionMundo = ReconstruirPosicionMundo(uv);

                float2 radiosPantalla = max(_VisionScreen.zw, float2(0.001, 0.001));
                float suavizado = clamp(_VisionFeather, 0.001, 0.45);
                float2 posicionElipse = (uv - _VisionScreen.xy) / radiosPantalla;
                float distancia = length(posicionElipse);

                // El borde comparte el ruido mundial de la bruma. Una banda
                // de baja frecuencia rompe la elipse y otra, casi constante
                // por dirección radial, forma lenguas finas hacia el exterior.
                float tiempo = _EdgeTime * 0.035 * max(0.05, _EdgeParams.z);
                float2 coordenadaRuido = posicionMundo.xz * 0.075
                    + float2(tiempo, -tiempo * 0.42);
                float nube = nieblaFractal(coordenadaRuido);
                float2 direccionElipse = posicionElipse / max(0.001, distancia);
                float lenguaRuido = ruido(
                    direccionElipse * 3.4
                    + posicionMundo.xz * 0.018
                    + float2(tiempo * 2.2, -tiempo * 1.3));
                float lengua = smoothstep(0.54, 0.88, lenguaRuido);
                lengua *= lengua;
                float desplazamientoBorde = (nube * 2.0 - 1.0) * _EdgeParams.x
                    - lengua * _EdgeParams.y;
                float distanciaViva = distancia + desplazamientoBorde;
                float visibleAhora = 1.0 - smoothstep(
                    1.0 - suavizado,
                    1.0 + suavizado,
                    distanciaViva);

                // Al aumentar el radio, una onda ligera se adelanta al nuevo
                // borde. No altera la visión lógica ni revela caminos.
                float progresoOnda = saturate(_EdgeWave.x);
                float centroOnda = 1.0 + progresoOnda * 0.22;
                float anchoOnda = lerp(0.032, 0.065, progresoOnda);
                float onda = 1.0 - smoothstep(
                    anchoOnda,
                    anchoOnda * 2.15,
                    abs(distancia - centroOnda));
                visibleAhora = max(visibleAhora, onda * _EdgeWave.y * 0.36);

                float2 uvHistorial =
                    (posicionMundo.xz - _HistoryBounds.xy) * _HistoryBounds.zw;
                float dentroHistorial =
                    step(0.0, uvHistorial.x) * step(uvHistorial.x, 1.0)
                    * step(0.0, uvHistorial.y) * step(uvHistorial.y, 1.0);
                float explorado = tex2D(_HistoryTex, saturate(uvHistorial)).r
                    * dentroHistorial
                    * _HistoryStrength;
                float cantidadNiebla = 1.0 - max(visibleAhora, explorado);

                // La niebla se desvanece antes de tocar el HUD superior.
                float franjaSuperior = 1.0 - smoothstep(_Band.y - _Band.z, _Band.y, uv.y);
                float mascaraFranja = franjaSuperior;

                // El ruido vive en el mundo, por lo que pan y zoom no hacen que
                // la bruma patine sobre caminos y adornos.
                float ondulacion = lerp(0.74, 1.04, smoothstep(0.16, 0.88, nube));

                float alpha = saturate(
                    _FogColor.a
                    * _Opacity
                    * cantidadNiebla
                    * mascaraFranja
                    * ondulacion);
                float3 colorNiebla = _FogColor.rgb * lerp(0.82, 1.12, nube);
                return fixed4(lerp(escena.rgb, colorNiebla, alpha), escena.a);
            }
            ENDCG
        }
    }

    Fallback Off
}
