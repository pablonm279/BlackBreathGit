Shader "GDD/Aliento Negro Contacto Suelo"
{
    Properties
    {
        _NoiseTex ("Ruido compartido", 2D) = "gray" {}
        _TintColor ("Matiz regional", Color) = (0.03, 0.3, 0.29, 1)
        _Footprint ("Centro XZ y radios", Vector) = (0, 0, 10, 20)
        _Strength ("Contacto", Range(0, 0.2)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Geometry+12" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend DstColor Zero
        ZWrite Off
        Offset -1, -1
        ColorMask RGB
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _NoiseTex;
            float4x4 _WorldToAliento;
            float4 _Footprint, _TintColor;
            float _Strength;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 vertex:SV_POSITION; float3 worldPos:TEXCOORD0; float3 normal:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 p = mul(_WorldToAliento, float4(i.worldPos, 1)).xyz;
                float2 q = (p.xz - _Footprint.xy) / max(_Footprint.zw, 0.001);
                float flow = _Time.y * 0.0224;
                float noise = tex2D(_NoiseTex, p.xz * 0.1725 + float2(flow, flow * 0.4)).r;
                float mask = 1 - smoothstep(0.32, 1.08, length(q) + (noise - 0.5) * 0.08);
                mask *= lerp(0.75, 1, noise) * smoothstep(0.25, 0.75, i.normal.y);
                float3 tint = _TintColor.rgb / max(0.001, max(_TintColor.r, max(_TintColor.g, _TintColor.b)));
                return fixed4(1 - _Strength * mask * (1 - tint * 0.55), 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
