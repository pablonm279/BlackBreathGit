Shader "GDD/Campania/PasoVientoHeladoRelieveFondo"
{
    Properties { _InicioBruma ("Inicio bruma", Float) = 100 _FinBruma ("Fin bruma", Float) = 230 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        #include "PasoVientoHeladoCielo.cginc"
        sampler2D _PasoNieve, _PasoPizarra, _PasoCordillera;
        float _InicioBruma, _FinBruma, _PasoScrollCielo, _PasoLuzCielo;
        fixed4 _PasoTinteCielo;
        struct Input { float3 worldPos; float3 worldNormal; };
        void vert(inout appdata_full v)
        {
            float3 mundo=mul(unity_ObjectToWorld,v.vertex).xyz;
            // Solo enterrar el borde abierto delantero; el resto del relieve conserva su forma.
            mundo.y-=12*(1-smoothstep(_InicioBruma-22,_InicioBruma-16,mundo.z));
            v.vertex=mul(unity_WorldToObject,float4(mundo,1));
        }
        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float3 pesos = pow(abs(i.worldNormal),4); pesos /= max(.001,pesos.x+pesos.y+pesos.z);
            fixed3 roca = tex2D(_PasoPizarra,i.worldPos.zy*float2(.035,.12)).rgb*pesos.x
                + tex2D(_PasoPizarra,i.worldPos.xy*float2(.035,.12)).rgb*pesos.z
                + tex2D(_PasoPizarra,i.worldPos.xz*.035).rgb*pesos.y;
            fixed3 nieve = tex2D(_PasoNieve,i.worldPos.xz*.055).rgb;
            float cubierta = smoothstep(.42,.88,i.worldNormal.y);
            fixed3 albedo = lerp(roca*fixed3(.64,.76,.90),nieve*fixed3(.69,.78,.88),cubierta);
            albedo = lerp(fixed3(.25,.34,.44),albedo,.76);
            // Un poco mas de bruma distante, conservando el volumen y la iluminacion originales.
            float distancia = smoothstep(_InicioBruma-6,_FinBruma,i.worldPos.z)*.88;
            float3 vista =mul(UNITY_MATRIX_V,float4(i.worldPos,1)).xyz;
            float2 pantalla=.5+.5*vista.xy*float2(abs(UNITY_MATRIX_P._m00),abs(UNITY_MATRIX_P._m11))/max(.001,-vista.z);
            fixed3 fondo=PasoCieloVelado(_PasoCordillera,PasoUVCielo(pantalla,_PasoScrollCielo),_PasoLuzCielo,_PasoTinteCielo.rgb);
            o.Albedo=albedo*(1-distancia); o.Emission=fondo*distancia;
            o.Metallic=0; o.Smoothness=.035; o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
