Shader "Campania/Bosque Ardiente/Llamas"
{
    Properties
    {
        _MainTex ("Mascara original", 2D) = "white" {}
        _Espectral ("Conservar fuego espectral", Range(0,1)) = 0
        _Reloj ("Tiempo de campaña", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Reloj, _Espectral;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float fase : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            float hash21(float2 p)
            {
                float3 q = frac(float3(p.xyx)*.1031);
                q += dot(q,q.yzx+33.33);
                return frac((q.x+q.y)*q.z);
            }
            float ruido(float2 p)
            {
                float2 a=floor(p), f=frac(p);
                float2 u=f*f*f*(f*(f*6-15)+10);
                return lerp(lerp(hash21(a),hash21(a+float2(1,0)),u.x),
                    lerp(hash21(a+float2(0,1)),hash21(a+1),u.x),u.y);
            }
            v2f vert(appdata v)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.color=v.color;
                o.fase=frac(dot(unity_ObjectToWorld._m03_m23,float2(.0173,.0237)))*23;
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float y=saturate(i.uv.y), t=_Reloj+i.fase;
                float2 p=i.uv*2-1;
                float desvio=(ruido(float2(y*2.6-t*.8,i.fase))-.5)*y*.45;
                float2 flujo=float2((i.uv.x+desvio)*6.8,y*2.8-t*1.9);
                float n=ruido(flujo)*.62+ruido(flujo*2.03+7.3)*.26+ruido(flujo*4.07)*.12;
                float ancho=lerp(.94,.12,smoothstep(.1,1,y))+(n-.5)*.22;
                float cuerpo=1-smoothstep(ancho*.22,ancho,abs(p.x-desvio));
                float punta=1-smoothstep(.58+n*.27,1,y);
                float detalle=smoothstep(.20,.72,n);
                float mascara=sqrt(saturate(tex2D(_MainTex,TRANSFORM_TEX(i.uv,_MainTex)).a));
                float alfa=saturate(cuerpo*punta*detalle*mascara*smoothstep(0,.06,y)*i.color.a);
                float calor=saturate((1-y)*.32+cuerpo*.55+n*.18);
                half3 borde=lerp(half3(.85,.045,.004),half3(.025,.30,.24),_Espectral);
                half3 medio=lerp(half3(2.3,.38,.015),half3(.055,.85,.56),_Espectral);
                half3 nucleo=lerp(half3(3.1,1.05,.14),half3(.35,1.25,.95),_Espectral);
                half3 color=lerp(borde,medio,smoothstep(.15,.65,calor));
                color=lerp(color,nucleo,smoothstep(.72,.97,calor));
                // Mantiene el desvanecimiento y la intensidad de la curva original del emisor.
                color*=saturate(max(i.color.r,max(i.color.g,i.color.b))*1.1);
                float halo=pow(saturate(1-dot(p,p)),2.3)*i.color.a*.025;
                half3 resplandor=lerp(half3(1,.16,.012),half3(.045,.4,.28),_Espectral)*halo;
                half4 salida=half4(color*alfa+resplandor,alfa);
                UNITY_APPLY_FOG_COLOR(i.fogCoord,salida,half4(0,0,0,0));
                return salida;
            }
            ENDCG
        }
    }
}
