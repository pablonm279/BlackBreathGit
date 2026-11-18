Shader "GDD/Campania/PasoVientoHeladoVegetacion"
{
    Properties
    {
        _MainTex ("Vegetacion", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Recorte", Range(0,1)) = .5
        _Glossiness ("Brillo", Range(0,1)) = .04
        _RespuestaViento ("Flexibilidad", Float) = 1
        _UsarColorVertices ("Color de las matas", Float) = 0
        _BrumaDistancia ("Bruma y niebla ambiental", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard alphatest:_Cutoff fullforwardshadows vertex:vert addshadow nofog finalcolor:AplicarNiebla
        #pragma multi_compile_fog
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        float _RespuestaViento;
        float _UsarColorVertices;
        half _BrumaDistancia;
        float4 _PasoVientoGlobal;
        float _PasoBrumaInicio, _PasoBrumaFin;
        struct Input { float2 uv_MainTex; float4 color:COLOR; float3 worldPos; };
        void vert(inout appdata_full v)
        {
            float3 mundo=mul(unity_ObjectToWorld,v.vertex).xyz;
            float altura=length(mul((float3x3)unity_ObjectToWorld,float3(0,1,0)));
            float peso=saturate(v.texcoord.y); peso*=peso;
            // Misma fase y pulso que VientoPasoVientoHelado.FuerzaEn, con propagacion espacial.
            float fase=_PasoVientoGlobal.w*.42-dot(mundo.xz,_PasoVientoGlobal.xy)*.09;
            float racha=_PasoVientoGlobal.z*(.20+.80*pow(.5+.5*sin(fase),3));
            float detalle=sin(_PasoVientoGlobal.w*1.8+mundo.x*.43+mundo.z*.21)*_PasoVientoGlobal.z*.006;
            mundo.xz+=_PasoVientoGlobal.xy*altura*peso*(racha*.035+detalle)*_RespuestaViento;
            v.vertex=mul(unity_WorldToObject,float4(mundo,1));
        }
        void surf(Input i, inout SurfaceOutputStandard o)
        {
            fixed4 color=tex2D(_MainTex,i.uv_MainTex)*_Color*lerp(fixed4(1,1,1,1),i.color,_UsarColorVertices);
            // Recupera el dibujo en los negros de las láminas sin agregar emisión ni perder la noche.
            float tono=dot(color.rgb,float3(.299,.587,.114));
            o.Albedo=lerp(color.rgb,fixed3(.18,.22,.26),.24*(1-saturate(tono*2)));
            float distancia=smoothstep(_PasoBrumaInicio-8,max(_PasoBrumaInicio+1,_PasoBrumaFin),i.worldPos.z);
            o.Albedo=lerp(o.Albedo,fixed3(.40,.48,.55),distancia*.40*_BrumaDistancia);
            o.Alpha=color.a;
            o.Metallic=0; o.Smoothness=min(_Glossiness,.08);
        }
        void AplicarNiebla(Input i, SurfaceOutputStandard o, inout fixed4 color)
        {
            #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
            UNITY_CALC_FOG_FACTOR(UnityWorldToClipPos(i.worldPos).z);
            #ifdef UNITY_PASS_FORWARDADD
                color.rgb=lerp(color.rgb,fixed3(0,0,0),(1-saturate(unityFogFactor))*_BrumaDistancia);
            #else
                color.rgb=lerp(color.rgb,unity_FogColor.rgb,(1-saturate(unityFogFactor))*_BrumaDistancia);
            #endif
            #endif
        }
        ENDCG
    }
    Fallback "Transparent/Cutout/Diffuse"
}
