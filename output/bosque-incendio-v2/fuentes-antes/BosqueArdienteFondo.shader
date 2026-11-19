Shader "Campania/Bosque Ardiente/Fondo"
{
    Properties
    {
        _MainTex ("Arbolado existente", 2D) = "white" {}
        _Color ("Tinte", Color) = (.15,.13,.11,.25)
        _Modo ("0 arbolado, 1 humo", Float) = 0
        _Reloj ("Tiempo", Float) = 0
        _Dia ("Luz diurna", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent-40" "RenderType"="Transparent" }
        Blend One OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Color;
            float _Modo, _Reloj, _Dia;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 world : TEXCOORD1; };
            v2f vert(appdata_base v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.texcoord.xy;
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz; return o;
            }
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float ruido(float2 p)
            {
                float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            half4 frag(v2f i) : SV_Target
            {
                float alfa;
                half3 color = _Color.rgb * lerp(.6, 1, _Dia);
                if (_Modo < .5)
                {
                    alfa = tex2D(_MainTex, i.uv).a * _Color.a;
                    alfa *= smoothstep(0,.15,i.uv.y);
                }
                else
                {
                    float n = ruido(i.world.xz * .08 + float2(-_Reloj*.012, i.uv.y*3));
                    n = n*.7 + ruido(float2(i.world.x*.24-_Reloj*.022,i.uv.y*6))*.3;
                    float borde = smoothstep(0,.22,i.uv.y)*(1-smoothstep(.35,1,i.uv.y));
                    borde *= smoothstep(0,.12,i.uv.x)*(1-smoothstep(.88,1,i.uv.x));
                    alfa = borde * smoothstep(.23,.78,n) * _Color.a;
                    // El resplandor permanece apagado y localizado, sin una banda naranja uniforme.
                    color += half3(.10,.025,.003) * pow(saturate(n),4) * (1-i.uv.y);
                }
                return half4(color*alfa,alfa);
            }
            ENDCG
        }
    }
}
