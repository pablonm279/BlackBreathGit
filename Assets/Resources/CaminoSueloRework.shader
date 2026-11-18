Shader "GDD/Campania/CaminoSueloRework"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 1)) = 0.22
        _EmissionMap ("Emission", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        _BiomeTint ("Biome Tint", Color) = (0.46, 0.35, 0.23, 1)
        _EdgeFeather ("Edge Feather", Range(0.08, 0.5)) = 0.28
        _EdgeBreakup ("Edge Breakup", Range(0, 0.5)) = 0.22
        _RutStrength ("Rut Strength", Range(0, 0.65)) = 0.16
        _MacroVariation ("Macro Variation", Range(0, 0.3)) = 0.12
        _Glossiness ("Smoothness", Range(0, 1)) = 0.035
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SnowAmount ("Snow shoulders", Range(0, 1)) = 0
        _RoadStructure ("Road structure (berms, ruts, hard edge)", Range(0, 1)) = 0
        _PasoRusticRoad ("Huella rustica del Paso", Float) = 0
        _SettlementRoad ("Piedra de asentamiento", Float) = 0

        [HideInInspector] _CaminoRecorrido ("Camino recorrido", Float) = 0
        [HideInInspector] _Mode ("Mode", Float) = 2
        [HideInInspector] _SrcBlend ("SrcBlend", Float) = 5
        [HideInInspector] _DstBlend ("DstBlend", Float) = 10
        [HideInInspector] _ZWrite ("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent-20"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 250
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade keepalpha noshadow noforwardadd
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _EmissionMap;
        sampler2D _PasoNieve;
        fixed4 _Color;
        fixed4 _EmissionColor;
        fixed4 _BiomeTint;
        half _BumpScale;
        half _Glossiness;
        half _Metallic;
        half _EdgeFeather;
        half _EdgeBreakup;
        half _RutStrength;
        half _MacroVariation;
        half _CaminoRecorrido;
        half _SnowAmount;
        half _RoadStructure;
        half _PasoRusticRoad, _SettlementRoad;

        // Valores globales actualizados por NieblaGuerraCaravana.
        float4 _CaminoVisionPantalla;
        float _CaminoVisionSuavizado;
        float _CaminoVisionActiva;
        float _CaminoVisionDebug;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_EmissionMap;
            float3 worldPos;
            float4 screenPos;
            float4 color : COLOR;
        };

        float HashSuave(float2 posicion)
        {
            float2 celda = floor(posicion);
            float2 fraccion = frac(posicion);
            fraccion = fraccion * fraccion * (3.0 - 2.0 * fraccion);

            float a = frac(sin(dot(celda, float2(127.1, 311.7))) * 43758.5453);
            float b = frac(sin(dot(celda + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
            float c = frac(sin(dot(celda + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
            float d = frac(sin(dot(celda + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);
            return lerp(lerp(a, b, fraccion.x), lerp(c, d, fraccion.x), fraccion.y);
        }

        float HashPiedra(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
        fixed3 PiedraIrregular(float2 uv, fixed3 nieve, float escarcha)
        {
            float2 celda=floor(uv),f=frac(uv),identidad=0;
            float primera=100,segunda=100;
            [unroll] for(int y=-1;y<=1;y++)
            [unroll] for(int x=-1;x<=1;x++)
            {
                float2 vecino=float2(x,y),id=celda+vecino;
                float2 centro=.20+.60*float2(HashPiedra(id),HashPiedra(id+19.7));
                float2 delta=vecino+centro-f;float d=dot(delta,delta);
                if(d<primera){segunda=primera;primera=d;identidad=id;}else segunda=min(segunda,d);
            }
            float junta=max(1-smoothstep(.018,.075,segunda-primera),smoothstep(.16,.34,primera)*.90);
            float variacion=HashPiedra(identidad+31.1);
            fixed3 piedra=lerp(fixed3(.23,.26,.28),fixed3(.43,.45,.41),variacion);
            piedra*=.86+HashSuave(uv*17.3)*.23;
            piedra*=lerp(.84,1.03,saturate((segunda-primera)*3));
            piedra=lerp(piedra,fixed3(.13,.15,.16),junta*.78);
            return lerp(piedra,nieve,escarcha);
        }

        void surf(Input entrada, inout SurfaceOutputStandard salida)
        {
            fixed4 textura = tex2D(_MainTex, entrada.uv_MainTex);
            float ruidoMacro = HashSuave(entrada.worldPos.xz * 0.72);
            float ruidoBorde = HashSuave(entrada.worldPos.xz * 3.1 + entrada.color.b * 7.3);

            // color.r vale 0 en los bordes y 1 en el centro de la calzada.
            float mascaraBorde = smoothstep(
                0.015,
                max(0.03, _EdgeFeather),
                entrada.color.r + (ruidoBorde - 0.52) * _EdgeBreakup);

            // Dos rodadas integradas en el material, sin mallas transparentes apiladas.
            float coordenadaAncho = entrada.color.g;
            float rodadaIzquierda = 1.0 - smoothstep(0.035, 0.105, abs(coordenadaAncho - 0.31));
            float rodadaDerecha = 1.0 - smoothstep(0.035, 0.105, abs(coordenadaAncho - 0.69));
            float rodadas = saturate(rodadaIzquierda + rodadaDerecha);
            float desgasteRecorrido = lerp(0.72, 1.0, saturate(_CaminoRecorrido));
            float oscurecimientoRodadas = 1.0 - rodadas * _RutStrength * desgasteRecorrido;

            fixed3 colorBase = textura.rgb * _Color.rgb;
            fixed3 colorBioma = colorBase * _BiomeTint.rgb * 1.62;
            colorBase = lerp(colorBase, colorBioma, 0.24);
            colorBase *= lerp(1.0 - _MacroVariation, 1.0 + _MacroVariation, ruidoMacro);
            colorBase *= oscurecimientoRodadas;

            // Un centro levemente compactado rompe la lectura de cinta uniforme.
            float centro = 1.0 - smoothstep(0.02, 0.34, abs(coordenadaAncho - 0.5));
            colorBase *= lerp(1.0, 0.94, centro * (0.35 + 0.35 * saturate(_CaminoRecorrido)));

            // Nieve acumulada en los hombros y hielo compacto entre las rodadas.
            float hombros = smoothstep(0.16, 0.43, abs(coordenadaAncho - 0.5));
            float granoNieve = HashSuave(entrada.worldPos.xz * 14.0);
            fixed3 nieve = lerp(fixed3(0.49, 0.57, 0.64), fixed3(0.66, 0.73, 0.79), hombros);
            nieve *= lerp(0.94, 1.04, granoNieve) * _Color.rgb;
            nieve *= 1.0 - rodadas * _RutStrength * desgasteRecorrido;
            colorBase = lerp(colorBase, nieve, _SnowAmount);

            // Calzada armada: perfil constante (berma clara, labio oscuro, centro compactado, rodadas).
            float estructura = saturate(_RoadStructure);
            float distBorde = abs(coordenadaAncho - 0.5) * 2.0;
            float distRuidosa = distBorde + (ruidoBorde - 0.5) * 0.07;
            float cuerpoCalzada = 1.0 - smoothstep(0.80, 0.97, distRuidosa);
            float berma = smoothstep(0.52, 0.72, distRuidosa) * (1.0 - smoothstep(0.84, 0.96, distRuidosa));
            float labio = smoothstep(0.86, 0.97, distRuidosa);
            float cresta = 1.0 - smoothstep(0.0, 0.16, distBorde);
            float lumaTextura = dot(textura.rgb, float3(0.299, 0.587, 0.114));
            fixed3 calzada = fixed3(0.37, 0.43, 0.50);
            calzada = lerp(calzada, fixed3(0.60, 0.66, 0.73), berma);
            calzada = lerp(calzada, fixed3(0.20, 0.26, 0.33), labio * 0.70);
            calzada *= 1.0 - rodadas * saturate(_RutStrength * 1.6) * desgasteRecorrido;
            calzada *= lerp(1.0, 1.07, cresta);
            calzada *= lerp(0.9, 1.1, saturate(lumaTextura * 1.6)) * lerp(0.95, 1.05, granoNieve) * _Color.rgb;
            colorBase = lerp(colorBase, calzada, estructura);

            float formaRustica=1;
            if(_PasoRusticRoad>.5)
            {
                float grano=HashSuave(entrada.worldPos.xz*5.3);
                float irregular=HashSuave(entrada.worldPos.xz*1.7);
                fixed3 nievePisada=tex2D(_PasoNieve,entrada.worldPos.xz*.065).rgb*lerp(.78,.88,grano);
                fixed3 tierra=lerp(fixed3(.28,.23,.19),fixed3(.39,.34,.28),ruidoMacro);
                float desgaste=(.76+rodadas*.24)*lerp(.86,1,irregular);
                colorBase=lerp(nievePisada,tierra,desgaste);
                colorBase*=lerp(.93,1.02,grano);
                float nieveCentro=smoothstep(.38,.74,irregular)*(.10+hombros*.20);
                colorBase=lerp(colorBase,nievePisada,nieveCentro);
                if(_SettlementRoad>.5)
                {
                    float escarcha=saturate(hombros*.28+smoothstep(.48,.75,irregular)*.16);
                    colorBase=PiedraIrregular(float2(entrada.uv_MainTex.x*4.2,coordenadaAncho*5.1),nievePisada,escarcha);
                }
                colorBase*=_Color.rgb;
                float bordeOrganico=distBorde+(irregular-.5)*.30;
                formaRustica=(1-smoothstep(.57,.98,bordeOrganico))*mascaraBorde;
                if(_SettlementRoad<.5) formaRustica*=lerp(.86,1,saturate(rodadas*.75+centro*.45))*lerp(.94,1,grano);
            }

            float2 uvPantalla = entrada.screenPos.xy / max(entrada.screenPos.w, 0.0001);
            float2 radios = max(_CaminoVisionPantalla.zw, float2(0.0001, 0.0001));
            float distanciaVision = length((uvPantalla - _CaminoVisionPantalla.xy) / radios);
            float suavizadoVision = clamp(_CaminoVisionSuavizado, 0.001, 0.45);
            float dentroVision = 1.0 - smoothstep(
                1.0 - suavizadoVision,
                1.0 + suavizadoVision,
                distanciaVision);

            float visible = max(dentroVision, saturate(_CaminoRecorrido));
            visible = lerp(visible, 1.0, saturate(_CaminoVisionDebug));
            visible = lerp(1.0, visible, saturate(_CaminoVisionActiva));
            clip(visible - 0.001);

            fixed3 normal = UnpackNormal(tex2D(_BumpMap, entrada.uv_BumpMap));
            normal.xy *= min(_BumpScale, 0.32);

            salida.Albedo = colorBase;
            salida.Normal = normalize(normal);
            salida.Metallic = min(_Metallic, 0.02);
            salida.Smoothness = _PasoRusticRoad>.5 ? 0 : min(_Glossiness, 0.055);
            salida.Occlusion = lerp(1.0, 0.82, rodadas * 0.28);
            salida.Emission = tex2D(_EmissionMap, entrada.uv_EmissionMap).rgb
                * _EmissionColor.rgb
                * lerp(0.72, 1.0, mascaraBorde);
            // En modo estructurado la forma la define el perfil (no el alpha desflecado de la textura).
            float alfaForma = lerp(textura.a * mascaraBorde, cuerpoCalzada * 0.94, estructura);
            if(_PasoRusticRoad>.5) alfaForma=formaRustica;
            salida.Alpha = alfaForma * _Color.a * visible * entrada.color.a;
        }
        ENDCG
    }

    Fallback Off
}
