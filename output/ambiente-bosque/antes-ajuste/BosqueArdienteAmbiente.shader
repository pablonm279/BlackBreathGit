Shader "Campania/Bosque Ardiente/Ambiente"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Modo ("0 llama, 1 humo, 2 brasa, 3 ceniza", Float) = 0
        _Espectral ("Fuego espectral", Float) = 0
        _Reloj ("Tiempo de campaña", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float fase : TEXCOORD1; };
            fixed4 _Color;
            float _Modo, _Espectral, _Reloj;
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float ruido(float2 p)
            {
                float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
                return lerp(lerp(hash21(a), hash21(a+float2(1,0)), f.x),
                    lerp(hash21(a+float2(0,1)), hash21(a+1), f.x), f.y);
            }
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.fase = dot(unity_ObjectToWorld._m03_m23, float2(1.73, 2.37));
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * 2 - 1;
                float t = _Reloj + i.fase;
                float alfa;
                half3 color = i.color.rgb;
                if (_Modo < .5)
                {
                    float y = i.uv.y;
                    float2 flujo = float2(i.uv.x * 5.8, y * 4.8 - t * 2.4);
                    float n = ruido(flujo) * .68 + ruido(flujo * 2.07 + 4.3) * .32;
                    float ondulacion = (ruido(float2(y * 3 - t * 1.3, t * .55)) - .5) * y * .62;
                    float ancho = .62 * sqrt(saturate(1-y)) + (n-.5)*.34;
                    float cuerpo = 1 - smoothstep(ancho * .08, ancho, abs(p.x - ondulacion));
                    float altura = 1 - smoothstep(.24 + n * .34, .95, y);
                    float detalle = smoothstep(.20 + y * .20, .74, n);
                    alfa = cuerpo * altura * detalle * smoothstep(0, .12, y) * i.color.a;
                    float calor = saturate(cuerpo * .66 + (1-y) * .55 - n * .22);
                    half3 exterior = lerp(half3(.85,.075,.008), half3(.025,.35,.27), _Espectral);
                    half3 medio = lerp(half3(1.45,.30,.012), half3(.065,.85,.48), _Espectral);
                    half3 centro = lerp(half3(1.75,.84,.18), half3(.42,1.2,.82), _Espectral);
                    color = lerp(exterior, medio, smoothstep(.10,.64,calor));
                    color = lerp(color, centro, smoothstep(.72,1,calor));
                }
                else if (_Modo < 1.5)
                {
                    float2 uv = i.uv * float2(4.2,3.1) + float2(-t*.075, -t*.11);
                    float n = ruido(uv) * .7 + ruido(uv*2.1+7.1)*.3;
                    float borde = saturate(1-dot(p,p));
                    alfa = borde*borde * smoothstep(.2,.78,n) * i.color.a;
                    color *= lerp(.72,1.12,n);
                }
                else if (_Modo < 2.5)
                {
                    alfa = pow(saturate(1-length(p)), 1.6) * i.color.a;
                    color *= 1.7;
                }
                else
                {
                    alfa = (1-smoothstep(.25,.9,abs(p.x)+abs(p.y))) * i.color.a;
                }
                return half4(color, saturate(alfa));
            }
            ENDCG
        }
    }
}
