Shader "Campania/Bosque Ardiente/Madera"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0
        struct Input { float4 color : COLOR; float3 worldPos; };
        void surf(Input i, inout SurfaceOutput o)
        {
            float2 p = i.worldPos.xz;
            float veta = sin(p.x * 37 + sin(p.y * 5.7) * 1.8) * sin(p.y * 29.3 + p.x * 7.1);
            float huella = max(length(ddx(p * 37)), length(ddy(p * 37)));
            float detalle = lerp(veta, 0, smoothstep(.4, 1.5, huella));
            float grieta = pow(saturate(1-abs(veta)), 10) * (1-smoothstep(.4,1.5,huella));
            // El bosque tiene varios rellenos intensos: iguala la madera al carbón del suelo.
            o.Albedo = i.color.rgb * .12 * (1 + detalle * .16) * (1-grieta*.30);
            o.Gloss = 0; o.Specular = 0; o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
