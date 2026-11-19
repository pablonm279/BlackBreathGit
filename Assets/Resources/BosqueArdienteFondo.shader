Shader "Campania/Bosque Ardiente/Fondo"
{
    Properties
    {
        _Color ("Humo de incendio", Color) = (.07,.055,.045,.96)
        _Reloj ("Tiempo", Float) = 0
        _Dia ("Luz diurna", Range(0,1)) = 1
        _Capa ("Profundidad", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent-30" "RenderType"="Transparent" }
        Blend One OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float4 _Color;
            float _Reloj, _Dia, _Capa;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 world : TEXCOORD1; };
            v2f vert(appdata_base v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord.xy;
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float ruido(float2 p)
            {
                float2 a=floor(p),f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 p=i.world.xy*float2(.14,.21)+float2(-_Reloj*.025,-_Reloj*.045)+_Capa*17.3;
                float voluta=ruido(p+ruido(p*.52)*2.2);
                float n=voluta*.58+ruido(p*2.07+9.1)*.28+ruido(p*4.13+7.3)*.14;
                float borde=smoothstep(0,.09,i.uv.y)*(1-smoothstep(.88,1,i.uv.y));
                borde*=smoothstep(0,.05,i.uv.x)*(1-smoothstep(.95,1,i.uv.x));
                float alfa=_Color.a*lerp(.87,1,smoothstep(.25,.72,n))*borde;
                half3 color=_Color.rgb*lerp(.65,1,_Dia)*lerp(.35,1.65,n);
                float rescoldo=exp(-abs(i.uv.y-.11)*18)*smoothstep(.30,.78,ruido(p*float2(.6,1.4)+27));
                color+=half3(.028,.018,.008)*rescoldo;
                return half4(color*alfa,alfa);
            }
            ENDCG
        }
    }
}
