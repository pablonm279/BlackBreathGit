Shader "GDD/Campania/PasoVientoHeladoTerrenoExtension"
{
    // Igual que el suelo mate del Paso, pero empujado hacia atras en profundidad (a lo largo del rayo de la camara):
    // donde la malla principal esta casi a la misma altura siempre gana, sin lineas de interseccion entre ambas mallas.
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.08
        _Metallic ("Metallic", Range(0,1)) = 0
        _EmpujeProfundidad ("Empuje en profundidad (m)", Float) = 1.0
        _ApoyoAldea ("Centro y radios del suelo de aldea", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow nofog finalcolor:AplicarNiebla
        #pragma multi_compile_fog
        #pragma target 3.0
        #include "PasoVientoHeladoCielo.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _BumpScale;
        half _Glossiness;
        half _Metallic;
        float _EmpujeProfundidad;
        float4 _ApoyoAldea;
        float4 _PasoCornisaFrontal;
        sampler2D _PasoCordillera;
        sampler2D _PasoNieve, _PasoPizarra;
        float _PasoBrumaInicio, _PasoBrumaFin, _PasoScrollCielo, _PasoLuzCielo;
        fixed4 _PasoTinteCielo;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
            float3 worldNormal;
            float3 terrenoPos;
            float4 screenPos;
            INTERNAL_DATA
        };

        float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
        float ruido(float2 p)
        {
            float2 a=floor(p), f=frac(p); f=f*f*(3-2*f);
            return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
        }

        fixed3 MuestraPizarra(float2 uv)
        {
            // La imagen tiene nieve en el borde superior: repetir solo su interior,
            // con reflejo continuo y una deformacion leve, evita las franjas horizontales.
            uv.y+=(ruido(uv*float2(.7,.45)+19)-.5)*.22;
            uv=1-abs(frac(uv*.5)*2-1);
            uv=float2(.025,.10)+uv*float2(.95,.68);
            return tex2D(_PasoPizarra,uv).rgb;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            float3 mundo = mul(unity_ObjectToWorld, v.vertex).xyz;
            // El empuje solo resuelve profundidad; ambas mallas comparten coordenadas de textura.
            o.terrenoPos = mundo;
            #ifndef UNITY_PASS_SHADOWCASTER
            float3 direccion = normalize(mundo - _WorldSpaceCameraPos);
            float empuje = _EmpujeProfundidad;
            if (_ApoyoAldea.z > 0 && _ApoyoAldea.w > 0)
                empuje *= smoothstep(.7,1.1,length((mundo.xz-_ApoyoAldea.xy)/_ApoyoAldea.zw));
            mundo += direccion * empuje;
            v.vertex = mul(unity_WorldToObject, float4(mundo, 1.0));
            #endif
        }

        float BrumaFueraAldea(float3 p)
        {
            if (_ApoyoAldea.z <= 0 || _ApoyoAldea.w <= 0) return 1;
            return smoothstep(.7,1.1,length((p.xz-_ApoyoAldea.xy)/_ApoyoAldea.zw));
        }

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float3 superficie = i.terrenoPos;
            float2 p = superficie.xz;
            float macro = ruido(p * .13) * .65 + ruido(p * .31 + 17) * .35;
            float fino = ruido(float2(p.x * .8 + p.y * .25,p.y * 2.4));
            fixed3 original = tex2D(_MainTex, i.uv_MainTex).rgb;
            float2 nieveUV = p*.065;
            float3 normalMundo = WorldNormalVector(i,float3(0,0,1));
            float3 normalGeometrica = cross(ddx(superficie),ddy(superficie));
            normalGeometrica *= rsqrt(max(.000001,dot(normalGeometrica,normalGeometrica)));
            float3 pesos = pow(abs(normalGeometrica),4);
            pesos /= max(.001,pesos.x+pesos.y+pesos.z);
            fixed3 nieve = tex2D(_PasoNieve,superficie.zy*.065).rgb * pesos.x
                + tex2D(_PasoNieve,nieveUV).rgb * pesos.y
                + tex2D(_PasoNieve,superficie.xy*.065).rgb * pesos.z;
            nieve = (nieve-.5)*.82+.49;
            nieve *= .90 + macro * .14;
            // Las pendientes escarpadas descubren pizarra, las planas conservan nieve limpia.
            // Cobertura continua en las caras: el ruido tambien varia con la altura,
            // evitando las islas cuadradas que producia la proyeccion horizontal.
            float variacionRoca=ruido(superficie.xy*.29+ruido(superficie.zy*.17)*1.7)*.55
                +ruido(superficie.zy*.23+13.7)*.45;
            float roca=(1-smoothstep(.50,.90,abs(normalMundo.y)))
                *lerp(.76,.96,smoothstep(.20,.80,variacionRoca));
            fixed3 pizarra = MuestraPizarra(superficie.zy*float2(.04,.16)) * pesos.x
                + MuestraPizarra(p*float2(.04,.16)) * pesos.y
                + MuestraPizarra(superficie.xy*float2(.04,.16)) * pesos.z;
            pizarra=(pizarra-.35)*.72+.35;
            fixed3 albedo = lerp(nieve, pizarra,roca*.88);
            // Islas estables de tundra, con borde roto y restos de nieve; no consumen RNG de campaña.
            float2 q=float2(p.x*.81-p.y*.59,p.x*.59+p.y*.81);
            float2 deformacion=float2(ruido(q*.19+17.3),ruido(q*.23-29.1))-.5;
            float manchas=ruido(q*.115+deformacion*1.8+13.7)*.65+ruido(q*.27+deformacion*.7-21.3)*.35;
            float tundra=smoothstep(.52,.76,manchas)*smoothstep(.58,.94,abs(normalMundo.y));
            fixed3 tierra=lerp(fixed3(.27,.30,.32),fixed3(.34,.36,.36),ruido(q*.85));
            tierra*=.96+ruido(q*5.7)*.08;
            albedo=lerp(albedo,tierra,tundra*.48);
            float frontal=smoothstep(-.6,.8,(superficie.z-_PasoCornisaFrontal.x)*_PasoCornisaFrontal.y)
                *abs(_PasoCornisaFrontal.y)*(1-smoothstep(.45,.9,abs(normalMundo.y)));
            float2 cara=superficie.xy;
            float2 desvio=float2(ruido(cara*.19+17),ruido(cara*.23-31))-.5;
            float regiones=ruido(cara*float2(.11,.18)+desvio*1.9)*.70
                +ruido(cara*.37+23)*.30;
            float tierraRoca=smoothstep(.48,.70,regiones)*frontal;
            albedo=lerp(albedo,albedo*fixed3(.72,.75,.79),tierraRoca*.45);
            float detalleRegional=smoothstep(.56,.78,ruido(cara*.14+desvio*1.8+37));
            // Fisuras finas, curvadas y discontinuas; variacion anclada a la roca, sin RNG.
            float veta=(cara.x+ruido(float2(cara.y*.45,cara.x*.07))*1.6)*.55;
            float corte=1-smoothstep(.012,.045,abs(frac(veta)-.5));
            corte*=smoothstep(.53,.76,ruido(float2(floor(veta)*1.91,cara.y*.17+31)));
            albedo*=1-corte*frontal*(.18+detalleRegional*.15);
            albedo*=1+(ruido(cara*3.1+desvio)-.5)*frontal*(.06+detalleRegional*.07);
            albedo*=1+(ruido(cara*1.3+desvio*2)-.5)*frontal*detalleRegional*.14;
            o.Albedo = albedo * _Color.rgb;
            // Relieve fino contenido: evita las bandas azules del normal anterior.
            float altura = tex2D(_PasoNieve,nieveUV).r;
            float dx = tex2D(_PasoNieve,nieveUV+float2(.002,0)).r-altura;
            float dy = tex2D(_PasoNieve,nieveUV+float2(0,.002)).r-altura;
            o.Normal = normalize(float3(-float2(dx,dy)*2.8,1));
            o.Normal=normalize(o.Normal+float3(float2(ruido(cara*2.2)-.5,ruido(cara*3.7+13)-.5)*frontal*.08,0));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            float bruma = smoothstep(_PasoBrumaInicio, max(_PasoBrumaInicio+.01,_PasoBrumaFin),i.worldPos.z) * BrumaFueraAldea(i.worldPos);
            float3 vista = mul(UNITY_MATRIX_V,float4(i.worldPos,1)).xyz;
            float2 pantalla = .5 + .5 * vista.xy * float2(abs(UNITY_MATRIX_P._m00),abs(UNITY_MATRIX_P._m11)) / max(.001,-vista.z);
            fixed3 cielo = PasoCieloVelado(_PasoCordillera,PasoUVCielo(pantalla,_PasoScrollCielo),_PasoLuzCielo,_PasoTinteCielo.rgb);
            o.Albedo *= 1-bruma;
            o.Emission = cielo * bruma;
            o.Alpha = 1;
        }
        void AplicarNiebla(Input i, SurfaceOutputStandard o, inout fixed4 color)
        {
            #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
            UNITY_CALC_FOG_FACTOR(UnityWorldToClipPos(i.worldPos).z);
            float fuerza=(1-saturate(unityFogFactor))*BrumaFueraAldea(i.worldPos);
            #ifdef UNITY_PASS_FORWARDADD
                color.rgb=lerp(color.rgb,fixed3(0,0,0),fuerza);
            #else
                color.rgb=lerp(color.rgb,unity_FogColor.rgb,fuerza);
            #endif
            #endif
        }
        ENDCG
    }
    Fallback "Diffuse"
}
