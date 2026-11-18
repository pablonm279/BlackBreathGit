#ifndef PASO_VIENTO_HELADO_CIELO
#define PASO_VIENTO_HELADO_CIELO
float4 _PasoClimaVisual;
float4x4 _PasoRotacionCielo;
float4 _PasoProyeccionCielo;
float2 PasoUVCielo(float2 pantalla, float scroll)
{
    // Misma proyección para el fondo y la bruma que lo une al relieve.
    float2 apertura = max(_PasoProyeccionCielo.xy, .001);
    float3 rayo = mul((float3x3)_PasoRotacionCielo, float3((pantalla - .5) * apertura, 1));
    float2 vista = .5 + rayo.xy / (max(rayo.z, .001) * apertura * max(_PasoProyeccionCielo.z, .001));
    return float2(.05 + vista.x * .90 + scroll, saturate((vista.y - .48) / .52));
}
float PasoHashCielo(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
float PasoRuidoCielo(float2 p)
{
    float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(PasoHashCielo(a),PasoHashCielo(a+float2(1,0)),f.x),lerp(PasoHashCielo(a+float2(0,1)),PasoHashCielo(a+1),f.x),f.y);
}
fixed3 PasoCieloVelado(sampler2D imagen,float2 uv,float luz,fixed3 tinte)
{
    fixed3 c=tex2D(imagen,uv).rgb;
    // Nubes asimetricas interrumpen las cumbres repetidas sin deformar ni mover la cordillera.
    float nube=PasoRuidoCielo(uv*float2(5.1,8.7)+float2(17.3-_Time.y*.0015,43.1));
    float velo=smoothstep(.27,.73,nube)*.55+(1-smoothstep(.12,.70,uv.y))*.25;
    velo+=_PasoClimaVisual.x*.13+_PasoClimaVisual.y*.10*(1-smoothstep(.1,.65,uv.y));
    fixed3 bruma=fixed3(.31,.39,.49);
    float gris=dot(c,float3(.299,.587,.114));
    c=lerp(c,gris*fixed3(.91,.98,1.08),.22);
    c*=lerp(1,.83,saturate(_PasoClimaVisual.x));
    return lerp(c*.76,bruma,saturate(velo))*tinte*luz*.90;
}
#endif
