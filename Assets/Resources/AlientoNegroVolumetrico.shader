Shader "GDD/Aliento Negro Volumetrico"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _NoiseTex ("Ruido de volumen", 2D) = "gray" {}
        _TintColor ("Tinte", Color) = (0.08, 0.35, 0.26, 1)
        _Density ("Densidad", Range(0, 0.05)) = 0.008
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
                float3 localPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
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
                o.localPos = v.vertex.xyz;

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float flow = _Time.y * _NoiseSpeed;
                float2 noiseUv = i.uv * _NoiseScale
                    + i.localPos.xz * 0.11
                    + float2(flow, -flow * 0.73);

                fixed4 noiseLow = tex2D(_NoiseTex, noiseUv);
                fixed4 noiseHigh = tex2D(_NoiseTex, noiseUv * 1.71 + 0.37);
                float noiseA = saturate(noiseLow.r * 0.68 + noiseHigh.g * 0.32);
                float noiseB = saturate(noiseLow.g * 0.62 + noiseHigh.b * 0.38);
                float2 warp = (float2(noiseA, noiseB) - 0.5) * _Distortion;
                fixed textureAlpha = tex2D(_MainTex, i.uv + warp).a;

                float noiseSoftness = max(0.001, _NoiseSoftness);
                float brokenVolume = smoothstep(
                    _NoiseCutoff - noiseSoftness,
                    _NoiseCutoff + noiseSoftness,
                    noiseA);
                float breakup = lerp(1.0, brokenVolume, _NoiseStrength);

                float alpha = textureAlpha * i.color.a * _Density * breakup;

                float2 puffPosition = i.uv * 2.0 - 1.0;
                float domeHeight = sqrt(saturate(1.0 - dot(puffPosition, puffPosition)));
                float3 volumeNormal = normalize(float3(
                    -puffPosition.x,
                    puffPosition.y,
                    max(0.14, domeHeight)));
                float3 lightDirection = normalize(float3(-0.48, 0.64, 0.92));
                float diffuse = saturate(dot(volumeNormal, lightDirection));

                float shadowNoise = tex2D(
                    _NoiseTex,
                    noiseUv + float2(-0.11, 0.13)).r;
                float densityPocket = smoothstep(
                    _NoiseCutoff - noiseSoftness,
                    _NoiseCutoff + noiseSoftness,
                    noiseA * 0.76 + noiseB * 0.24);
                float selfShadow = 1.0
                    - _CoreShadow
                    * densityPocket
                    * lerp(0.62, 1.0, shadowNoise);
                float directionalLight = lerp(0.64, 1.28, diffuse);
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
                    _Time.y * 0.72
                    + noiseHigh.b * 7.4
                    + i.localPos.x * 0.19
                    + i.localPos.z * 0.13);
                float necroVein = smoothstep(
                    0.7,
                    0.92,
                    noiseHigh.b + (necroBeat - 0.5) * 0.24)
                    * densityPocket
                    * domeHeight;

                fixed3 baseColor = i.color.rgb
                    * _TintColor.rgb
                    * lerp(0.72, 1.12, noiseB);
                baseColor *= 1.0 - _DarkCore * darkHeart;
                fixed3 color = baseColor * max(0.24, volumeShape)
                    + _TintColor.rgb
                    * (coldRim + necroVein * _NecroPulse)
                    * textureAlpha;
                fixed4 result = fixed4(color, alpha);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, result, fixed4(0, 0, 0, 0));
                return result;
            }
            ENDCG
        }
    }

    Fallback Off
}
