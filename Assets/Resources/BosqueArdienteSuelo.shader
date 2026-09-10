Shader "Campania/Bosque Ardiente/Suelo"
{
    Properties
    {
        _MainTex ("Textura de suelo existente", 2D) = "white" {}
        _BumpMap ("Relieve de ceniza y corteza", 2D) = "bump" {}
        _Color ("Tinte de textura existente", Color) = (1,1,1,1)
        _UsarTextura ("Conservar textura procedural del inspector", Range(0,1)) = 0
        _Tierra ("Tierra tostada", Color) = (.025,.020,.017,1)
        _Ceniza ("Ceniza mineral", Color) = (.042,.037,.032,1)
        _Carbon ("Carbon", Color) = (.009,.008,.008,1)
        _OrigenTerreno ("Origen compartido del terreno", Vector) = (0,0,0,0)
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
        float4 _OrigenTerreno;
        struct Input { float2 uv_MainTex; float3 worldPos; };
        float2 gradiente(float2 p)
        {
            p = frac(p * float2(.1031,.11369));
            p += dot(p,p.yx + 19.19);
            return normalize(frac(float2((p.x+p.y)*p.y,(p.x+p.y)*p.x))*2-1 + .0001);
        }
        float ruido(float2 p)
        {
            float2 a = floor(p), f = frac(p);
            float2 u = f*f*f*(f*(f*6-15)+10);
            float n = lerp(lerp(dot(gradiente(a),f),dot(gradiente(a+float2(1,0)),f-float2(1,0)),u.x),
                lerp(dot(gradiente(a+float2(0,1)),f-float2(0,1)),dot(gradiente(a+1),f-1),u.x),u.y);
            return saturate(.5+n*.7);
        }
        float ruidoFiltrado(float2 p)
        {
            // El grano se funde a distancia para evitar cuadriculas y parpadeo al mover la camara.
            float huella = max(length(ddx(p)), length(ddy(p)));
            return lerp(ruido(p), .5, smoothstep(.35, 1.1, huella));
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            // Coordenadas de mundo: el detalle mantiene escala y continuidad en la extension.
            float2 p = IN.worldPos.xz - _OrigenTerreno.xz;
            p = mul(float2x2(.8,-.6,.6,.8),p);
            float macro = ruido(p*.16);
            float2 q = p + (float2(ruido(p*.23+3.7), ruido(p*.23+18.1))-.5)*2.6;
            float detalle = ruidoFiltrado(q*3.7+11.3);
            float grano = ruidoFiltrado(q*7.5);
            float polvo = ruidoFiltrado(mul(float2x2(.6,-.8,.8,.6),q)*19.3+37.1);
            float ceniza = smoothstep(.40,.85,ruido(q*.65+21)*.55+ruido(q*2.3)*.30+detalle*.15)*lerp(.45,1,macro);
            float carbon = smoothstep(.29,.85,ruido(q*.31-9)*.55+ruido(q*1.8)*.30+detalle*.15);
            float veta = ruido(q*float2(4.4,1.85));
            float borde = max(fwidth(veta)*.75,.003);
            float cicatriz = 1-smoothstep(.009,.045+borde,abs(veta-.5));
            cicatriz *= .036/(.036+borde);
            cicatriz *= smoothstep(.40,.67,carbon);
            half3 baseColor = lerp(_Tierra.rgb, _Ceniza.rgb, ceniza*.82);
            baseColor = lerp(baseColor, _Carbon.rgb, carbon*.82);
            // Dos orientaciones y escalas no coincidentes eliminan el mosaico del normal original.
            float2 uvDetalle = mul(float2x2(.6,-.8,.8,.6),p)*.119 + float2(3.17,7.43);
            half3 normalA = UnpackNormal(tex2D(_BumpMap,p*.085));
            half3 normalB = UnpackNormal(tex2D(_BumpMap,uvDetalle));
            float2 normalDetalle = normalA.xy*.55 + mul(float2x2(.6,.8,-.8,.6),normalB.xy)*.45;
            // Poros y fragmentos pequenos sobre el relieve amplio, sin aclarar la ceniza.
            half3 normalGrano = UnpackNormal(tex2D(_BumpMap,p*.43+float2(8.31,2.73)));
            normalDetalle = normalDetalle*.75 + normalGrano.xy*.35;
            baseColor *= lerp(.74,1.22,detalle)*lerp(.79,1.19,grano)*lerp(.90,1.10,polvo);
            baseColor = lerp(baseColor, tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb, _UsarTextura);
            o.Albedo = lerp(baseColor, _Carbon.rgb*.65, cicatriz*.18)*.432;
            o.Specular = 0;
            o.Gloss = 0;
            float calor = smoothstep(.70,.89,ruido(q*.48+73)) * smoothstep(.72,.88,grano);
            float pulso = .8+.2*sin(_Reloj*1.15+macro*19);
            o.Emission = half3(.38,.048,.003)*cicatriz*calor*pulso;
            // Microrrelieve optico; no modifica malla, colliders, rutas ni alturas de nodos.
            o.Normal = normalize(float3(normalDetalle*.12,1));
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
