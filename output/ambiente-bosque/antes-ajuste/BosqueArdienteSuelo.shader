Shader "Campania/Bosque Ardiente/Suelo"
{
    Properties
    {
        _MainTex ("Textura de suelo existente", 2D) = "white" {}
        _BumpMap ("Relieve de ceniza y corteza", 2D) = "bump" {}
        _Color ("Tinte de textura existente", Color) = (1,1,1,1)
        _UsarTextura ("Conservar textura procedural del inspector", Range(0,1)) = 0
        _Tierra ("Tierra tostada", Color) = (.025,.020,.017,1)
        _Ceniza ("Ceniza mineral", Color) = (.075,.072,.067,1)
        _Carbon ("Carbon", Color) = (.009,.008,.008,1)
        _Reloj ("Tiempo de campaña", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex, _BumpMap;
        fixed4 _Color, _Tierra, _Ceniza, _Carbon;
        float _UsarTextura, _Reloj;
        struct Input { float2 uv_MainTex; float3 worldPos; };
        float hash21(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
        float ruido(float2 p)
        {
            float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
            return lerp(lerp(hash21(a),hash21(a+float2(1,0)),f.x),
                lerp(hash21(a+float2(0,1)),hash21(a+1),f.x),f.y);
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            // Coordenadas de mundo: el detalle mantiene escala y continuidad en la extension.
            float2 p = IN.worldPos.xz;
            float macro = ruido(p*.16);
            float2 q = p + (float2(ruido(p*.23+3.7), ruido(p*.23+18.1))-.5)*2.6;
            float ceniza = smoothstep(.46,.8,ruido(q*.65+21)*.65+ruido(q*2.3)*.35)*lerp(.45,1,macro);
            float carbon = smoothstep(.34,.8,ruido(q*.31-9)*.65+ruido(q*1.8)*.35);
            float grano = ruido(q*7.5);
            float veta = ruido(q*float2(4.4,1.85));
            float cicatriz = 1-smoothstep(.009,.045,abs(veta-.5));
            cicatriz *= smoothstep(.40,.67,carbon);
            half3 baseColor = lerp(_Tierra.rgb, _Ceniza.rgb, ceniza*.82);
            baseColor = lerp(baseColor, _Carbon.rgb, carbon*.82);
            half3 normalDetalle = UnpackNormal(tex2D(_BumpMap,p*.12));
            baseColor *= lerp(.68,1.18,grano) * lerp(.82,1.1,normalDetalle.z);
            baseColor = lerp(baseColor, tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb, _UsarTextura);
            o.Albedo = lerp(baseColor, _Carbon.rgb*.65, cicatriz*.25);
            o.Specular = 0;
            o.Gloss = 0;
            float calor = smoothstep(.70,.89,ruido(q*.48+73)) * smoothstep(.72,.88,grano);
            float pulso = .8+.2*sin(_Reloj*1.15+macro*19);
            o.Emission = half3(.38,.048,.003)*cicatriz*calor*pulso;
            // Microrrelieve optico; no modifica malla, colliders, rutas ni alturas de nodos.
            o.Normal = normalize(float3(normalDetalle.xy*.42 + float2(grano-.5,veta-.5)*.12,1));
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
