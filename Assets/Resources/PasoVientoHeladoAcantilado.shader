Shader "GDD/Campania/PasoVientoHeladoAcantilado"
{
    // Acantilado del Paso: la foto original solo aporta el detalle (luminancia); el color sale de la paleta del terreno
    // (roca de pizarra azulada + nieve), con la base oscurecida y estratos suaves. Es opaco y con luz, asi la niebla
    // de guerra y la hora del dia lo tratan igual que el suelo.
    Properties
    {
        _MainTex ("Detalle (foto original)", 2D) = "white" {}
        _Color ("Tinte global (igual al del suelo)", Color) = (0.76, 0.82, 0.89, 1)
        _RocaOscura ("Roca oscura", Color) = (0.20, 0.25, 0.32, 1)
        _RocaClara ("Roca clara", Color) = (0.38, 0.44, 0.52, 1)
        _Nieve ("Nieve", Color) = (0.60, 0.67, 0.76, 1)
        _OscurecerBase ("Oscurecimiento en la base", Range(0.2, 1)) = 0.50
        _BaseY ("Altura de la base (m)", Float) = 18
        _Altura ("Altura del muro (m)", Float) = 9
        _Glossiness ("Brillo", Range(0,1)) = 0.04
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 300
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color, _RocaOscura, _RocaClara, _Nieve;
        half _OscurecerBase, _Glossiness;
        float _BaseY, _Altura;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        float Hash(float2 p)
        {
            return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
        }

        float Ruido(float2 p)
        {
            float2 celda = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = Hash(celda);
            float b = Hash(celda + float2(1, 0));
            float c = Hash(celda + float2(0, 1));
            float d = Hash(celda + float2(1, 1));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float2 plano = float2(i.worldPos.x + i.worldPos.z, i.worldPos.y);
            float h = saturate((i.worldPos.y - _BaseY) / max(_Altura, 0.01));
            float bloques = Ruido(plano * float2(.72,.36));
            float veta = Ruido(float2(plano.x * .28, plano.y * 2.8 + bloques * 1.3));
            float grieta = smoothstep(.29,.36,Ruido(plano * float2(2.4,.22)));
            float detalle = Ruido(plano * 7.5);
            fixed3 roca = lerp(fixed3(.24,.29,.35), fixed3(.43,.48,.54), bloques);
            roca *= (.72 + veta * .35) * lerp(.62,1,grieta);
            fixed3 foto = tex2D(_MainTex,float2(i.uv_MainTex.x,saturate((i.uv_MainTex.y-.60)/.40))).rgb;
            fixed3 c = lerp(foto,roca,.20);
            c = lerp(c,fixed3(.66,.73,.80),smoothstep(.97,1,i.uv_MainTex.y));
            c *= .96 + detalle*.08;
            c *= lerp(.65,1,smoothstep(0,.55,h));
            o.Albedo = c * _Color.rgb;
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
