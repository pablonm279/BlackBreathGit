Shader "GDD/Campania/PasoVientoHeladoDetalles"
{
    Properties { _Glossiness ("Brillo", Range(0,1)) = 0.08 }
    SubShader
    {
        Tags { "Queue"="Transparent-25" "RenderType"="Transparent" }
        Cull Off ZWrite Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade keepalpha fullforwardshadows
        #pragma target 3.0
        half _Glossiness;
        struct Input { float4 color : COLOR; float3 worldPos; };
        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float grano = sin(i.worldPos.x * 13.7 + i.worldPos.z * 19.1) * .018;
            o.Albedo = saturate(i.color.rgb + grano);
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = i.color.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
