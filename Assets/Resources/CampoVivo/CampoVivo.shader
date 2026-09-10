Shader "CampoVivo/Atmosfera"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _Modo ("0 bruma, 1 mota, 2 fragmento, 3 onda, 4 suelo", Float) = 0
    _Reloj ("Reloj de combate", Float) = 0
    _Viento ("Viento", Vector) = (0.05,0.01,0,0)
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
    ZTest LEqual
    Cull Off
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
      struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
      fixed4 _Color;
      float _Modo, _Reloj;
      float4 _Viento;
      v2f vert(appdata v)
      {
        v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o;
      }
      float hash21(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
      float ruido(float2 p)
      {
        float2 i = floor(p), f = frac(p); f = f*f*(3-2*f);
        return lerp(lerp(hash21(i),hash21(i+float2(1,0)),f.x),lerp(hash21(i+float2(0,1)),hash21(i+1),f.x),f.y);
      }
      fixed4 frag(v2f i) : SV_Target
      {
        float2 p = i.uv * 2 - 1;
        float r = length(p);
        float a;
        if (_Modo < .5 || _Modo > 3.5)
        {
          float2 uv = i.uv * float2(5,3) + _Viento.xy * _Reloj;
          float n = ruido(uv) * .65 + ruido(uv * 2.13 + 7.4) * .35;
          float borde = saturate(1-dot(p,p));
          a = borde*borde * smoothstep(.22,.8,n);
        }
        else if (_Modo < 1.5)
          a = pow(saturate(1-r), 1.6);
        else if (_Modo < 2.5)
          a = 1-smoothstep(.35,1,abs(p.x)+abs(p.y));
        else
          a = (1-smoothstep(.015,.10,abs(r-.72))) * saturate(1-r);
        return fixed4(i.color.rgb, i.color.a * a);
      }
      ENDCG
    }
  }
}
