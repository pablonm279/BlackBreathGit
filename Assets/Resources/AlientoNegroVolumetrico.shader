Shader "GDD/Aliento Negro Volumetrico"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _NoiseTex ("Ruido de volumen", 2D) = "gray" {}
        _TintColor ("Tinte", Color) = (0.08, 0.35, 0.26, 1)
        _Density ("Densidad", Range(0, 0.3)) = 0.055
        _NoiseScale ("Escala de ruido", Range(0.25, 8)) = 2.8
        _NoiseStrength ("Ruptura", Range(0, 1)) = 0.6
        _NoiseCutoff ("Umbral de ruido", Range(0, 1)) = 0.46
        _NoiseSoftness ("Suavidad de ruido", Range(0.01, 0.5)) = 0.2
        _NoiseSpeed ("Velocidad de ruido", Range(0, 0.25)) = 0.03
        _Distortion ("Distorsion UV", Range(0, 0.2)) = 0.04
        _VolumeLight ("Relieve", Range(0, 1)) = 0.5
        _CoreShadow ("Autosombra", Range(0, 1)) = 0.25
        _RimStrength ("Borde frio", Range(0, 1)) = 0.12
        _DarkCore ("Nucleo oscuro", Range(0, 1)) = 0.2
        _NecroPulse ("Pulso necrotico", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            fixed4 _TintColor;
            float _Density;
            float _NoiseScale;
            float _NoiseStrength;
            float _NoiseCutoff;
            float _NoiseSoftness;
            float _NoiseSpeed;
            float _Distortion;
            float _VolumeLight;
            float _CoreShadow;
            float _RimStrength;
            float _DarkCore;
            float _NecroPulse;
            float4x4 _WorldToAliento;
            float4 _DensityGradient;
            float4 _CaminoVisionPantalla;
            float _CaminoVisionActiva;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float2 shapeUv : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shapeUv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            // Unión suave de lóbulos: altura y pendiente de la superficie de nube.
            float3 Lobe(float2 p, float2 center, float radius)
            {
                float2 delta = p - center;
                return float3(delta, radius * radius - dot(delta, delta));
            }

            float3 MergeLobes(float3 a, float3 b)
            {
                float blend = saturate(0.5 + (a.z - b.z) / 0.18);
                float3 result = lerp(b, a, blend);
                result.z += 0.09 * blend * (1.0 - blend);
                return result;
            }

            float3 VolumeNoise(float3 p)
            {
                // Tres planos del mismo campo espacial; no depende del emisor ni
                // del tiling regional de la antigua textura plana.
                float3 xy = tex2D(_NoiseTex, p.xy).rgb;
                float3 yz = tex2D(_NoiseTex, p.yz + 0.31).rgb;
                float3 zx = tex2D(_NoiseTex, p.zx + 0.67).rgb;
                return saturate(((xy + yz + zx) / 3.0 - 0.5) * 2.2 + 0.5);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.shapeUv * 2.0 - 1.0;
                float3 surface = Lobe(p, float2(-0.12, -0.04), 0.73);
                surface = MergeLobes(surface, Lobe(p, float2(0.36, 0.18), 0.49));
                surface = MergeLobes(surface, Lobe(p, float2(-0.39, 0.31), 0.47));
                surface = MergeLobes(surface, Lobe(p, float2(0.09, -0.43), 0.48));
                // Salida temprana: fuera de los lobulos el espesor es 0 y el alpha final
                // tambien (1 - exp2(0) = 0), asi que el resultado visual es identico pero
                // se evitan ~7 lecturas de ruido y todo el sombreado en las esquinas del billboard.
                if (surface.z <= 0.0 || i.color.a <= 0.003)
                {
                    discard;
                }
                float domeHeight = sqrt(max(0.0, surface.z));

                // Base real del billboard: la luz sigue apuntando desde el mismo
                // lugar aunque cada partícula gire o cambie la cámara.
                float2 dxUv = ddx(i.shapeUv);
                float2 dyUv = ddy(i.shapeUv);
                float3 dxWorld = ddx(i.worldPos);
                float3 dyWorld = ddy(i.worldPos);
                float determinant = dxUv.x * dyUv.y - dxUv.y * dyUv.x;
                float inverseDet = (determinant < 0.0 ? -1.0 : 1.0)
                    / max(abs(determinant), 0.00000001);
                float3 right = (dxWorld * dyUv.y - dyWorld * dxUv.y) * inverseDet;
                float3 up = (dyWorld * dxUv.x - dxWorld * dyUv.x) * inverseDet;
                float3 facing = normalize(_WorldSpaceCameraPos - i.worldPos);
                float radiusWorld = (length(right) + length(up)) * 0.25;
                right = normalize(right);
                up = normalize(up);
                float3 volumePosition = i.worldPos + facing * domeHeight * radiusWorld;
                float3 volumeNormal = normalize(right * surface.x + up * surface.y
                    + facing * max(0.12, domeHeight));
                float3 lightDirection = normalize(float3(-0.48, 0.78, -0.4));
                float visualTime = _Time.y * 0.8;
                float flow = visualTime * _NoiseSpeed;
                float3 noisePosition = mul(_WorldToAliento, float4(volumePosition, 1)).xyz
                    * (_NoiseScale * 0.075) + float3(flow, -flow * 0.73, flow * 0.4);
                float3 noiseLow = VolumeNoise(noisePosition);
                float noiseA = saturate(noiseLow.r * 0.72 + noiseLow.g * 0.28);
                float noiseB = noiseLow.b;
                float2 warp = (float2(noiseA, noiseB) - 0.5) * _Distortion;
                fixed textureAlpha = tex2D(_MainTex, i.uv + warp).a;

                float noiseSoftness = max(0.001, _NoiseSoftness);
                float brokenVolume = smoothstep(
                    _NoiseCutoff - noiseSoftness,
                    _NoiseCutoff + noiseSoftness,
                    noiseA);
                float breakup = lerp(1.0, brokenVolume, _NoiseStrength);

                float edge = smoothstep(-0.025, 0.12,
                    surface.z + (noiseB - 0.5) * _Distortion);
                // El ruido nunca debe revelar el rectángulo del billboard.
                float2 border = min(i.shapeUv, 1.0 - i.shapeUv);
                edge *= smoothstep(0.0, 0.16, min(border.x, border.y));
                float thickness = domeHeight * breakup * lerp(0.72, 1.0, textureAlpha);
                float localX = mul(_WorldToAliento, float4(i.worldPos, 1)).x;
                float depth = smoothstep(0.0, 1.0,
                    (localX - _DensityGradient.x) / max(0.01, _DensityGradient.y));
                float densityRamp = lerp(0.34, 1.25, depth);
                float2 screenUv = i.screenPos.xy / max(0.0001, i.screenPos.w);
                float visionDistance = length((screenUv - _CaminoVisionPantalla.xy)
                    / max(_CaminoVisionPantalla.zw, float2(0.0001, 0.0001)));
                // Conserva una presencia tenue dentro del área jugable, con
                // transición amplia para no dibujar un agujero alrededor del carro.
                float readability = lerp(1.0, lerp(0.06, 1.0,
                    smoothstep(0.25, 1.35, visionDistance)), saturate(_CaminoVisionActiva));
                float alpha = (1.0 - exp2(-_Density * densityRamp * thickness * 6.0))
                    * i.color.a * edge * readability;
                float diffuse = saturate(dot(volumeNormal, lightDirection));

                float shadowNoise = VolumeNoise(noisePosition
                    + mul((float3x3)_WorldToAliento, lightDirection * radiusWorld * 0.35)
                    * (_NoiseScale * 0.075)).r;
                float densityPocket = smoothstep(
                    _NoiseCutoff - noiseSoftness,
                    _NoiseCutoff + noiseSoftness,
                    noiseA * 0.76 + noiseB * 0.24);
                float selfShadow = 1.0
                    - _CoreShadow
                    * densityPocket
                    * lerp(0.4, 1.0, shadowNoise) * domeHeight;
                float directionalLight = lerp(0.22, 3.8, pow(diffuse, 2.1));
                float volumeShape = lerp(
                    1.0,
                    directionalLight * selfShadow,
                    _VolumeLight);
                float coldRim = pow(saturate(1.0 - domeHeight), 2.4)
                    * _RimStrength
                    * (0.35 + noiseB * 0.65)
                    * brokenVolume;
                float darkHeart = densityPocket
                    * smoothstep(0.18, 0.92, domeHeight)
                    * (0.58 + shadowNoise * 0.42);
                float necroBeat = 0.5 + 0.5 * sin(
                    visualTime * 0.72
                    + noiseLow.g * 7.4
                    + noisePosition.x * 0.19
                    + noisePosition.z * 0.13);
                float necroVein = smoothstep(
                    0.54,
                    0.78,
                    noiseLow.g + (necroBeat - 0.5) * 0.24)
                    * densityPocket
                    * domeHeight;

                // Filamentos interiores lentos: la energía recorre el volumen
                // compartido, sin rayos, destellos rápidos ni nuevos billboards.
                float veinField = noiseLow.g - noiseLow.b
                    + sin(noisePosition.y * 2.1 + visualTime * 0.24) * 0.065;
                float filaments = (1.0 - smoothstep(0.012, 0.055, abs(veinField)))
                    * smoothstep(0.46, 0.72, noiseA)
                    * smoothstep(0.12, 0.48, domeHeight)
                    * (0.25 + 0.75 * necroBeat);

                fixed3 baseColor = lerp(i.color.rgb, fixed3(1, 1, 1), 0.45)
                    * _TintColor.rgb
                    * lerp(0.72, 1.12, noiseB);
                baseColor *= 1.0 - _DarkCore * darkHeart;
                fixed3 color = baseColor * max(0.16, volumeShape)
                    + _TintColor.rgb
                    * coldRim * textureAlpha
                    + float3(0.16, 0.85, 0.62)
                    * (necroVein * 0.65 + filaments * 0.85) * _NecroPulse;
                fixed4 result = fixed4(color, alpha);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, result, fixed4(0, 0, 0, 0));
                return result;
            }
            ENDCG
        }
    }

    Fallback Off
}
