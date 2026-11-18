Shader "GDD/Campania/PasoVientoHeladoAmbiente"
{
    Properties
    {
        _Color ("Color", Color) = (0.8, 0.9, 1, 0.4)
        _Luz ("Luz ambiente", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Luz;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float radio=length(i.uv*2-1);
                float alpha=1-smoothstep(.12,1,radio);
                return fixed4(_Color.rgb*i.color.rgb*_Luz, _Color.a*i.color.a*alpha);
            }
            ENDCG
        }
    }
}
