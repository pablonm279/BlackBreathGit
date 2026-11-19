Shader "Campania/Bosque Ardiente/Fuego"
{
    Properties
    {
        _Color ("Opacidad", Color) = (1,1,1,1)
        _Reloj ("Tiempo", Float) = 0
        _Intensidad ("Incandescencia", Range(.1,3)) = 1.15
    }
    SubShader
    {
        Tags { "Queue"="Transparent+2" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
            float _Reloj, _Intensidad;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float fase : TEXCOORD1; };
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float ruido(float2 p)
            {
                float2 a=floor(p),f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color;
                float3 mundo=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.fase=dot(mundo.xz,float2(.31,.47)); return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float y=saturate(i.uv.y), x=i.uv.x*2-1;
                float t=_Reloj*1.45+i.fase;
                float n=ruido(float2(x*3.2,y*3.6-t*1.7));
                n=n*.7+ruido(float2(x*7.1+3.7,y*7.3-t*2.5))*.3;
                float desvio=(sin(y*5.3-t*2.1)*.19+sin(y*10.7+t*.8)*.09)*y;
                float ancho=pow(saturate(1-y),.78)*.74+(n-.5)*.32;
                float distancia=abs(x-desvio);
                float borde=max(fwidth(distancia)*1.1,.015);
                float cuerpo=1-smoothstep(ancho*.60,ancho+borde,distancia);
                float lengua=(1-smoothstep(.05,.18,abs(x+.24-sin(y*8-t)*.09)))
                    *(1-smoothstep(.38,.66+sin(t*.9)*.1,y));
                cuerpo=max(cuerpo,lengua*.8);
                float punta=1-smoothstep(.62+n*.24,.98,y);
                float huecos=lerp(.48,1,smoothstep(.23,.67,n));
                float alfa=cuerpo*punta*huecos*smoothstep(0,.055,y)*i.color.a*saturate(_Color.a);
                float nucleo=(1-smoothstep(.08,.40,distancia))*(1-smoothstep(.1,.62,y));
                half3 color=lerp(half3(1.25,.25,.018),half3(.72,.045,.002),smoothstep(.32,.95,y));
                color=lerp(color,half3(2.1,1.15,.28),nucleo*.85);
                return half4(color*_Intensidad*alfa,alfa);
            }
            ENDCG
        }
    }
}
