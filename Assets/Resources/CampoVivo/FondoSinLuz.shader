Shader "CampoVivo/FondoSinLuz"
{
  // Fondo pintado que se muestra tal cual es la imagen: sin luces, reflejos ni ambiente.
  // Conserva _Color para que el tinte nocturno y el pulso de color del ambiente sigan funcionando.
  Properties
  {
    _MainTex ("Fondo", 2D) = "white" {}
    _Color ("Tinte", Color) = (1,1,1,1)
  }
  SubShader
  {
    Tags { "Queue"="Geometry" "RenderType"="Opaque" }
    Cull Off
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      sampler2D _MainTex;
      float4 _MainTex_ST;
      fixed4 _Color;
      struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
      struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
      v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o; }
      fixed4 frag(v2f i) : SV_Target { fixed4 c = tex2D(_MainTex, i.uv) * _Color; c.a = 1; return c; }
      ENDCG
    }
  }
}
