Shader "TablerosIntegrados/Suelo"
{
  Properties
  {
    _MainTex ("Microaltura de la textura", 2D) = "gray" {}
    _Color ("Intensidad por alfa (RGB reservado)", Color) = (1,1,1,1)
    _Interior ("Zona de relieve XY", Vector) = (.82,.78,0,0)
    _Opacidad ("Mezcla del microrelieve", Range(0,0.5)) = 0.23
    _FuerzaRelieve ("Fuerza de la microaltura", Range(0,2)) = 1
  }
  SubShader
  {
    Tags { "Queue"="Transparent-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
    // Modula el color que ya existe debajo; no pinta un albedo propio.
    Blend DstColor OneMinusSrcAlpha
    ZWrite Off
    Cull Off
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
      struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
      sampler2D _MainTex;
      fixed4 _Color;
      float4 _Interior, _MainTex_TexelSize;
      float _Opacidad, _FuerzaRelieve;
      v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
      float hash21(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
      float ruido(float2 p)
      {
        float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
        return lerp(lerp(hash21(a),hash21(a+float2(1,0)),f.x),lerp(hash21(a+float2(0,1)),hash21(a+1),f.x),f.y);
      }
      fixed4 frag(v2f i) : SV_Target
      {
        float2 borde = abs(i.uv*2-1);
        // Desvanecimiento amplio e irregular: ninguna placa ni contorno exterior.
        float2 caida = 1-smoothstep(_Interior.xy*.70,float2(1,1),borde);
        float mascara = caida.x*caida.y*lerp(.8,1,ruido(i.uv*5));
        float2 paso = _MainTex_TexelSize.xy*1.5;
        float3 pesos = float3(.299,.587,.114);
        float izquierda = dot(tex2D(_MainTex,i.uv-float2(paso.x,0)).rgb,pesos);
        float derecha = dot(tex2D(_MainTex,i.uv+float2(paso.x,0)).rgb,pesos);
        float abajo = dot(tex2D(_MainTex,i.uv-float2(0,paso.y)).rgb,pesos);
        float arriba = dot(tex2D(_MainTex,i.uv+float2(0,paso.y)).rgb,pesos);
        float2 pendiente = float2(derecha-izquierda,arriba-abajo)*24*_FuerzaRelieve;
        float3 normal = normalize(float3(-pendiente,1));
        float detalle = clamp(dot(normal.xy,float2(-.6,.8))*.55,-.35,.25);
        float alpha = mascara*min(_Opacidad*2.5,.65)*_Color.a;
        // Con relieve neutro el factor es 1: el fondo queda exactamente igual.
        // La mezcla conserva su tono, incluso al cambiar de fondo o de hora.
        return fixed4(float3(1,1,1)*(1+detalle)*alpha,alpha);
      }
      ENDCG
    }
  }
  Fallback Off
}
