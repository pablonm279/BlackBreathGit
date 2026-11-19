Shader "Campania/Bosque Ardiente/Llamas"
{
    Properties
    {
        _Color ("Color original", Color) = (1,1,1,1)
        _MainTex ("Textura original", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = .5
        _Glossiness ("Brillo original", Range(0,1)) = .5
        _GlossMapScale ("Escala de brillo", Range(0,1)) = 1
        _SmoothnessTextureChannel ("Canal de brillo", Float) = 0
        _Metallic ("Metallic original", Range(0,1)) = 0
        _MetallicGlossMap ("Metallic", 2D) = "white" {}
        _SpecularHighlights ("Reflejos originales", Float) = 1
        _GlossyReflections ("Reflexiones originales", Float) = 1
        _BumpScale ("Relieve", Float) = 1
        _BumpMap ("Normal", 2D) = "bump" {}
        _Parallax ("Parallax", Range(.005,.08)) = .02
        _ParallaxMap ("Altura", 2D) = "black" {}
        _OcclusionStrength ("Oclusion", Range(0,1)) = 1
        _OcclusionMap ("Oclusion", 2D) = "white" {}
        [HDR] _EmissionColor ("Emision HDR original", Color) = (0,0,0,1)
        _EmissionMap ("Emision original", 2D) = "white" {}
        _DetailMask ("Mascara de detalle", 2D) = "white" {}
        _DetailAlbedoMap ("Detalle", 2D) = "grey" {}
        _DetailNormalMapScale ("Relieve de detalle", Float) = 1
        _DetailNormalMap ("Normal de detalle", 2D) = "bump" {}
        _UVSec ("UV de detalle", Float) = 0
        [HideInInspector] _Mode ("Mode", Float) = 2
        [HideInInspector] _SrcBlend ("Src", Float) = 5
        [HideInInspector] _DstBlend ("Dst", Float) = 10
        [HideInInspector] _ZWrite ("ZWrite", Float) = 0
        [HideInInspector] _UsarForma ("Forma refinada", Float) = 1
        _Reloj ("Tiempo de campaña", Float) = 0
    }
    CGINCLUDE
    // Mismo sombreado Standard de Unity que usan los materiales originales.
    #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    #include "UnityStandardCore.cginc"
    float _Reloj, _UsarForma;
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

    float FormaLlama(float2 uvTextura)
    {
        float2 uv=(uvTextura-_MainTex_ST.zw)/_MainTex_ST.xy;
                float fase=frac(dot(unity_ObjectToWorld._m03_m23,float2(.0173,.0237)))*23;
                // Mascara estable: el movimiento queda a cargo de las particulas originales.
                float y=saturate(uv.y), t=fase;
                float2 p=uv*2-1;
                float desvio=(ruido(float2(y*2.6-t*.8,fase))-.5)*y*.45;
                float2 flujo=float2((uv.x+desvio)*6.8,y*2.8-t*1.9);
                float n=ruido(flujo)*.62+ruido(flujo*2.03+7.3)*.26+ruido(flujo*4.07)*.12;
                float ancho=lerp(.94,.12,smoothstep(.1,1,y))+(n-.5)*.22;
                float cuerpo=1-smoothstep(ancho*.22,ancho,abs(p.x-desvio));
                float punta=1-smoothstep(.58+n*.27,1,y);
                float detalle=smoothstep(.20,.72,n);
                float mascara=sqrt(saturate(tex2D(_MainTex,TRANSFORM_TEX(uv,_MainTex)).a));

        return saturate(cuerpo*punta*detalle*mascara*smoothstep(0,.06,y)*_Color.a);
    }
    half4 fragLlamaBase(VertexOutputForwardBase i) : SV_Target
    {
        half4 color=fragForwardBaseInternal(i);
        color.a=lerp(color.a,FormaLlama(i.tex.xy),_UsarForma);
        return color;
    }
    half4 fragLlamaAdd(VertexOutputForwardAdd i) : SV_Target
    {
        half4 color=fragForwardAddInternal(i);
        color.a=lerp(color.a,FormaLlama(i.tex.xy),_UsarForma);
        return color;
    }
    ENDCG
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 300
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vertForwardBase
            #pragma fragment fragLlamaBase
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            ENDCG
        }
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode"="ForwardAdd" }
            Blend [_SrcBlend] One
            ZWrite Off
            ZTest LEqual
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vertForwardAdd
            #pragma fragment fragLlamaAdd
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            ENDCG
        }
        UsePass "Standard/SHADOWCASTER"
    }
}
