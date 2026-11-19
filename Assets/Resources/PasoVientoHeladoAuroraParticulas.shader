Shader "GDD/Campania/PasoVientoHeladoAuroraParticulas"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (0.1,1,0.6,0.15)
        _Fase ("Fase de la cortina", Float) = 0
        [HideInInspector] _SrcBlend ("Origen", Float) = 5
        [HideInInspector] _DstBlend ("Destino", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            half4 _Color;
            float _Fase;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float4 pantalla:TEXCOORD1; float fase:TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color;
                o.pantalla=ComputeScreenPos(o.pos); COMPUTE_EYEDEPTH(o.pantalla.z);
                float3 mundo=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.fase=_Fase+dot(mundo.xz,float2(.013,.009));
                return o;
            }
            half4 frag(v2f i):SV_Target
            {
                float y=i.uv.y, t=_Time.y;
                float curva=sin(y*4.2+t*.16+i.fase)*.11+sin(y*9.0-t*.09+i.fase)*.035;
                float x=i.uv.x-.5-curva;
                float ancho=.31+.05*sin(y*3.3+i.fase+t*.11);
                float velo=exp2(-x*x/(ancho*ancho)*2.2);
                float filamento=.78+.22*sin(x*48+sin(y*6+t*.10+i.fase)*1.4);
                float extremos=pow(saturate(sin(y*3.141593)),.65);
                float borde=smoothstep(0,.10,i.uv.x)*(1-smoothstep(.90,1,i.uv.x));
                float textura=tex2D(_MainTex,float2(saturate(x+.5),y)).a;
                float escena=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.pantalla)));
                float contacto=saturate((escena-i.pantalla.z)/1.5);
                half3 tono=lerp(half3(.88,1,.94),half3(.82,.94,1.12),smoothstep(.25,.95,y));
                return half4(_Color.rgb*i.color.rgb*tono,saturate(_Color.a*i.color.a*5)*velo*filamento*extremos*borde*textura*contacto);
            }
            ENDCG
        }
    }
    Fallback Off
}
