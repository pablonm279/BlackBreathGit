Shader "Campania/Bosque Ardiente/Fuego"
{
    Properties
    {
        _Color ("Opacidad", Color) = (1,1,1,1)
        _Reloj ("Tiempo", Float) = 0
        _Intensidad ("Incandescencia", Range(.1,3)) = .9
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
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _Color;
            float _Reloj, _Intensidad;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float fase : TEXCOORD1; float4 pantalla : TEXCOORD2; };
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float ruido(float2 p)
            {
                float2 a=floor(p),f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color;
                o.pantalla=ComputeScreenPos(o.pos); COMPUTE_EYEDEPTH(o.pantalla.z);
                float3 mundo=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.fase=dot(mundo.xz,float2(.31,.47)); return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float y=saturate(i.uv.y), x=i.uv.x*2-1;
                float t=_Reloj*1.05+i.fase;
                float2 flujo=float2(x*2.7,y*3.1-t*1.4);
                float giro=ruido(flujo*.73+19.1)-.5;
                flujo.x+=giro*y*1.8;
                float n=ruido(flujo)*.55+ruido(flujo*2.03+7.3)*.30+ruido(flujo*4.07+13.1)*.15;
                float desvio=sin(y*5.7-t*1.6)*y*.16;
                // Densidad turbulenta: las puntas aparecen y se rompen, sin una silueta de cono fija.
                float campo=(1-y)*.73+n*.72-pow(abs(x-desvio),1.25)*.85;
                float borde=max(fwidth(campo)*1.15,.035);
                float cuerpo=smoothstep(.39-borde,.61+borde,campo);
                float base=smoothstep(0,.23,y);
                float techo=1-smoothstep(.83,1,y);
                float raw=SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.pantalla));
                float contacto=raw>.000001 ? saturate((LinearEyeDepth(raw)-i.pantalla.z)*5) : 1;
                float alfa=cuerpo*base*techo*i.color.a*saturate(_Color.a)*contacto;
                float temperatura=saturate((campo-.43)*3.7);
                half3 color=lerp(half3(.90,.38,.09),half3(2.10,1.40,.48),temperatura*temperatura);
                float halo=exp(-(x*x*8+pow((y-.16)*7,2)))*.06*i.color.a*contacto;
                return half4(color*_Intensidad*alfa+half3(.28,.19,.08)*halo,saturate(alfa+halo));
            }
            ENDCG
        }
    }
}
