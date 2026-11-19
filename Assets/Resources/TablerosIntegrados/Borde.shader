Shader "TablerosIntegrados/Borde"
{
  Properties
  {
    _Color ("Color funcional", Color) = (1,1,1,1)
    _BaseColor ("Compatibilidad con hover", Color) = (1,1,1,1)
    _EmissionColor ("Pulso del estado actual", Color) = (0,0,0,1)
    _UsarColorVertice ("Color por vertice en barras", Float) = 0
    _DibujarCasilla ("Marco desgastado", Float) = 0
    _Grosor ("Grosor del marco", Float) = .006
    _Desgaste ("Desgaste del contorno", Float) = .55
    _Relleno ("Sombra interior", Float) = .045
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
    Cull Off
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
      struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float2 semilla : TEXCOORD1; };
      fixed4 _Color;
      half4 _EmissionColor;
      float _UsarColorVertice, _DibujarCasilla, _Grosor, _Desgaste, _Relleno;
      v2f vert(appdata v)
      {
        v2f o; o.pos = UnityObjectToClipPos(v.vertex);
        o.color = lerp(fixed4(1,1,1,1),v.color,_UsarColorVertice);
        o.uv = v.uv;
        o.semilla = frac(float2(unity_ObjectToWorld._m03,unity_ObjectToWorld._m23)*.37)*29;
        return o;
      }
      float hash21(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
      float ruido(float2 p)
      {
        float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
        return lerp(lerp(hash21(a),hash21(a+float2(1,0)),f.x),lerp(hash21(a+float2(0,1)),hash21(a+1),f.x),f.y);
      }
      fixed4 frag(v2f i) : SV_Target
      {
        fixed4 color = fixed4(saturate(_Color.rgb+_EmissionColor.rgb*.16),_Color.a)*i.color;
        if (_DibujarCasilla < .5) return color;
        float2 q = abs(i.uv-.5)-.452;
        float d = length(max(q,0))+min(max(q.x,q.y),0)-.012;
        float aa = max(fwidth(d),.0005);
        float n = .5, grano = .8;
        if (abs(d) < .035)
        {
          n = ruido(i.uv*67+i.semilla);
          grano = ruido(i.uv*173+i.semilla*1.7);
        }
        float linea = 1-smoothstep(_Grosor-aa,_Grosor+aa,abs(d+(n-.5)*.004));
        float erosion = lerp(1,lerp(.30,1,smoothstep(.15,.82,grano)),_Desgaste);
        // Remates biselados: una pequena cuna con muesca diagonal en cada esquina.
        float2 esquina = .5-abs(i.uv-.5);
        float triangulo = min(min(esquina.x-.030,esquina.y-.030),(.105-esquina.x-esquina.y)*.7071);
        float remate = smoothstep(-aa,aa,triangulo);
        float bisel = 1-smoothstep(.001,.004,abs(min(esquina.x,esquina.y)-.032));
        float muesca = 1-smoothstep(.001,.0035,abs(esquina.x+esquina.y-.091));
        remate *= (.78+.22*bisel)*(1-.62*muesca)*lerp(.86,1,grano);
        float tinta = max(linea*erosion,remate)*color.a;
        float interior = (1-smoothstep(-aa,aa,d))*_Relleno;
        float contacto = exp(-pow((d-.006)/.012,2))*.09;
        float fondo = saturate(interior+contacto)*(1-tinta);
        float alpha = tinta+fondo;
        return fixed4(color.rgb*tinta/max(alpha,.0001),alpha);
      }
      ENDCG
    }
  }
  Fallback Off
}
