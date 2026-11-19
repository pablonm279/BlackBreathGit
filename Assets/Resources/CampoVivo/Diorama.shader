Shader "CampoVivo/Diorama"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _Modo ("0 deriva de suelo, 1 borde nevado, 2 niebla vertical, 3 rayos de luz, 4 nubes", Float) = 0
    _Reloj ("Reloj de combate", Float) = 0
    _Deriva ("Desplazamiento acumulado del viento", Float) = 0
    _Medidas ("Mitad lamina XY, mitad tablero XY", Vector) = (4,3,2,1.5)
    _Param ("Parametros segun modo", Vector) = (0.5,0.25,3.5,0)
    _Semilla ("Semilla", Float) = 0
    _Destino ("Blend destino: 10 normal, 1 aditivo", Float) = 10
    _Saturacion ("Saturacion del color en nieve y ventisca", Float) = 0.5
    _ColBrillo ("Color de los destellos del borde", Color) = (1,1,1,1)
    _Extra ("x: umbral de destellos (mas bajo = mas destellos)", Vector) = (0.996,0,0,0)
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    Blend SrcAlpha [_Destino]
    ZWrite Off
    ZTest LEqual
    Cull Off
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
      struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
      fixed4 _Color;
      float _Modo, _Reloj, _Deriva, _Semilla, _Saturacion;
      float4 _Medidas, _Param, _Extra;
      fixed4 _ColBrillo;
      v2f vert(appdata v)
      {
        v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
      }
      float hash21(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
      float ruido(float2 p)
      {
        float2 i = floor(p), f = frac(p); f = f*f*(3-2*f);
        return lerp(lerp(hash21(i),hash21(i+float2(1,0)),f.x),lerp(hash21(i+float2(0,1)),hash21(i+1),f.x),f.y);
      }
      float fbm(float2 p) { return ruido(p)*.5 + ruido(p*2.03+3.7)*.3 + ruido(p*4.1+9.1)*.2; }
      float sdCaja(float2 p, float2 b)
      {
        float2 d = abs(p) - b;
        return length(max(d,0)) + min(max(d.x,d.y),0);
      }
      // Espesor de los bancos de nieve del borde del tablero. Devuelve tambien la cercania al borde.
      float alturaRim(float2 p, float d, float2 s, float anchoFuera, float anchoDentro, out float prof)
      {
        // El borde ondula y el espesor del banco varia: nada queda como una banda recta.
        float dj = d + (fbm(p*3.3 + s + 5.1) - .5) * .45;
        float grosor = lerp(.55, 1.45, fbm(p*.7 + 11.3));
        prof = dj > 0 ? exp(-pow(dj/(anchoFuera*grosor),1.3)) : exp(-pow(-dj/anchoDentro,2));
        return fbm(p*2.4 + s)*.9 + prof*.42 - .27;
      }
      fixed4 frag(v2f i) : SV_Target
      {
        float t = _Reloj + _Semilla;
        float2 uv = i.uv;
        float2 e = abs(uv*2-1);
        float a = 0;
        float3 rgb = _Color.rgb;
        if (_Modo < 1.5)
        {
          // La nieve toma el tono neutro de la pintura: sin resplandor de color saturado.
          float lum = dot(rgb, float3(.3,.59,.11));
          rgb = lerp(float3(lum,lum,lum), rgb, _Saturacion);
        }
        if (_Modo < .5)
        {
          // Ventisca rasante sobre el terreno que rodea al tablero.
          float2 p = (uv*2-1) * _Medidas.xy;
          float d = sdCaja(p, _Medidas.zw);
          float dir = _Param.z == 0 ? 1 : _Param.z;   // sentido del viento: 1 derecha, -1 izquierda
          float2 q1 = float2(p.x*.1 - _Deriva*3.0*dir, p.y*2.6 + sin(p.x*.4 + t*.35)*.25);
          float2 q2 = float2(p.x*.22 - _Deriva*5.2*dir + 4.7, p.y*4.2 + 1.9);
          float n = fbm(q1)*.6 + ruido(q2)*.4;
          float rafaga = smoothstep(.52,.85,n);
          float fuera = smoothstep(-.15,1.3,d);
          float borde = (1 - smoothstep(.55,1,e.x)) * (1 - smoothstep(.3,.8,e.y));
          // Menos densa hacia el borde cercano del tablero: ahi se acumulaba una franja clara y nebulosa.
          float cercania = saturate(-p.y / _Medidas.y);
          a = rafaga * lerp(.1,1,fuera) * borde * lerp(1, .5, cercania);
        }
        else if (_Modo < 1.5)
        {
          if (_Extra.w > .5)
          {
            // Estilo viento (nieve de Paso Viento Helado): lenguetas finas y largas paralelas al borde, con puntas
            // afiladas y bordes suaves, como la nieve pintada del fondo. Sin relieve ni masas redondas.
            float2 pv = (uv*2-1) * _Medidas.xy;
            float dv = sdCaja(pv, _Medidas.zw);
            float ancho = _Param.x;
            if (dv > ancho*3.2 || dv < -_Param.y*2.5) return fixed4(rgb, 0);
            float2 sv = float2(_Semilla*7.3, _Semilla*3.1);
            // Vetas horizontales, como las lenguetas barridas por el viento de la pintura (sin pliegues en las esquinas).
            float2 qv = float2(pv.x*.2, pv.y*4.8) + sv;
            float racha = fbm(qv);
            float fina = ruido(float2(pv.x*.9, pv.y*12) + sv*1.7);
            // Donde la veta es fuerte hay una lengua; su alcance hacia afuera varia: unas cortas, otras largas.
            float lengua = smoothstep(.46,.66, racha + fina*.2);
            float alcance = ancho * (.2 + 1.3*lengua);
            float perfilV = dv > 0 ? exp(-pow(dv/alcance,2.2)) : exp(-pow(-dv/_Param.y,2));
            float nv = lengua * perfilV * .5;
            // Mas clara en el centro de cada veta y azulada hacia los bordes.
            float nucleo = smoothstep(.5,.8, racha + fina*.2);
            float3 colV = lerp(rgb, float3(.93,.97,1), nucleo*.7);
            float2 celdaV = floor(pv*60 + _Semilla);
            float brilloV = step(_Extra.x, hash21(celdaV)) * (.5 + .5*sin(t*1.7 + hash21(celdaV+3.1)*6.28));
            colV = lerp(colV, _ColBrillo.rgb, saturate(brilloV*perfilV*2));
            a = nv + brilloV*perfilV*.3;
            rgb = colV;
          }
          else
          {
          // Bancos de nieve con relieve (cresta iluminada desde el horizonte, sombra del lado cercano),
          // escarcha y sombra de contacto: disimulan el corte del tablero contra la pintura.
          float2 p = (uv*2-1) * _Medidas.xy;
          float d = sdCaja(p, _Medidas.zw);
          float anchoFuera = _Param.x, anchoDentro = _Param.y;
          if (d > anchoFuera*4.5 || d < -anchoDentro*3) return fixed4(rgb, 0);
          float2 s = float2(_Semilla*7.3,_Semilla*3.1);
          float prof, profL;
          float h = alturaRim(p, d, s, anchoFuera, anchoDentro, prof);
          float2 luz = normalize(float2(-.35,1)) * .07;
          float hL = alturaRim(p + luz, sdCaja(p + luz, _Medidas.zw), s, anchoFuera, anchoDentro, profL);
          float masa = smoothstep(.4,.6,h);
          float relieve = (smoothstep(.4,.6,h) - smoothstep(.4,.6,hL)) * 3.2;
          float nieve = prof*masa*.7;
          float sombra = (d > 0 ? exp(-pow(d/(anchoFuera*.7),1.2)) : 0) * .4;
          float2 celda = floor(p*60 + _Semilla);
          float brillo = step(_Extra.x,hash21(celda)) * (.5+.5*sin(t*1.7 + hash21(celda+3.1)*6.28));
          a = nieve + sombra*(1-nieve) + brillo*prof*.4;
          float3 colNieve = rgb * (1 + saturate(relieve)*.55 - saturate(-relieve)*.5);
          colNieve = lerp(colNieve, _ColBrillo.rgb, saturate(brillo*prof*2));
          // Brasas incandescentes dentro de la ceniza (solo si _Extra.z > 0): parches calientes que laten.
          float brasa = _Extra.z * smoothstep(.62,.8, fbm(p*3.6 + float2(t*.03,0) + s))
            * prof * (.6 + .4*sin(t*2.3 + hash21(floor(p*6))*6.28));
          nieve = max(nieve, brasa*.75);
          colNieve = lerp(colNieve, float3(1,.42,.1), saturate(brasa*1.4));
          a = nieve + sombra*(1-nieve) + brillo*prof*.4;
          float3 colSombra = float3(.03,.05,.09);
          rgb = (colNieve*nieve + colSombra*sombra*(1-nieve)) / max(nieve + sombra*(1-nieve), .0001);
          }
        }
        else if (_Modo < 2.5)
        {
          // Banco de niebla vertical entre la pintura y el tablero.
          float banda = exp(-pow((uv.y-_Param.x)/_Param.y,2));
          float2 q = float2(uv.x*_Param.z + _Deriva, uv.y*_Param.z*1.8 + sin(uv.x*3 + t*.1)*.12);
          float n = fbm(q);
          float borde = (1-smoothstep(.6,1,e.x)) * (1-smoothstep(.7,1,e.y));
          a = banda * smoothstep(.34,.8,n) * borde;
        }
        else if (_Modo < 3.5)
        {
          // Rayos de luz que nacen del brillo del horizonte (aditivo, muy tenues).
          float2 v = (uv - _Param.xy) * float2(1.6,1);
          float dist = length(v);
          // El patron se muestrea sobre la direccion (circulo unitario): continuo en todo el giro, sin corte angular.
          float2 dir = v / max(dist, .0001);
          float haces = ruido(dir*_Param.z + float2(_Deriva*.4, 3.3 + t*.03));
          float caida = exp(-dist*_Param.w);
          float borde = (1-smoothstep(.55,1,e.x)) * (1-smoothstep(.7,1,e.y));
          if (_Extra.y > .5)
          {
            // Version suave: muchos haces finos que se rompen con la distancia (cuevas y bosque).
            float h2 = ruido(dir*_Param.z*2.7 + float2(-t*.05, 7.1));
            haces = smoothstep(.3,.95, haces*.55 + h2*.45);
            haces *= .55 + .45*ruido(float2(dist*7 - t*.12, dir.x*3.0 + dir.y*2.0));
          }
          else
            haces = smoothstep(.4,.95,haces*.7 + ruido(dir*_Param.z*2.7 + float2(-t*.05, 7.1))*.3);
          a = haces * caida * borde * smoothstep(0,.12,dist);
        }
        else if (_Modo < 4.5)
        {
          // Nubes bajas que cruzan el cielo.
          float2 q = float2(uv.x*2.2 + _Deriva, uv.y*5.5 + sin(uv.x*2 + t*.07)*.2);
          float n = fbm(q);
          float cielo = smoothstep(.5,.78,uv.y);
          float borde = (1-smoothstep(.6,1,e.x)) * (1-smoothstep(.75,1,e.y));
          a = cielo * smoothstep(.42,.78,n) * borde;
        }
        else if (_Modo < 5.5)
        {
          // Placa del tablero: oscurece el suelo para leer unidades, con borde difuminado (sin corte recto)
          // y un poco mas densa hacia el centro, donde estan las unidades.
          float2 p = (uv*2-1) * _Medidas.xy;
          float d = sdCaja(p, _Medidas.zw);
          float suave = _Param.x;
          float borde = 1 - smoothstep(-suave, suave*.5, d);
          float2 r2 = abs(p) / _Medidas.zw;
          float r = max(r2.x, r2.y);
          a = borde * (1 - .3*r*r);
        }
        else if (_Modo < 6.5)
        {
          // Resplandor de fuegos lejanos (aditivo): focos irregulares que titilan en una banda del horizonte.
          float banda = exp(-pow((uv.y-_Param.x)/_Param.y,2));
          float focos = smoothstep(.45,.85, fbm(float2(uv.x*_Param.z + 3.1, 1.7 + t*.05)));
          float parpadeo = .65 + .35*sin(t*3.1 + uv.x*9) * (.6 + .4*sin(t*7.3 + uv.x*23));
          float borde = (1-smoothstep(.6,1,e.x)) * (1-smoothstep(.7,1,e.y));
          a = banda * focos * parpadeo * borde;
        }
        else if (_Modo < 7.5)
        {
          // Espectral (aditivo): volutas frias que aparecen y se desvanecen, con destellos sueltos.
          float2 q = float2(uv.x*3 + _Deriva*.5, uv.y*1.2 - t*.05);
          float n = fbm(q + float2(0, sin(t*.2 + uv.x*2)*.3));
          float vol = smoothstep(.5,.85,n) * (.5 + .5*sin(t*.35 + uv.x*4 + n*6));
          float2 g = uv*float2(110,62) + _Semilla;
          float2 c = floor(g);
          // Destellos como puntos suaves y redondos (antes salian cuadrados).
          float punto = saturate(1 - length(frac(g) - .5) * 2.4);
          float chispa = step(.992, hash21(c)) * pow(saturate(sin(t*1.3 + hash21(c+5.2)*6.28)), 3) * punto * punto;
          float borde = (1-smoothstep(.6,1,e.x)) * (1-smoothstep(.7,1,e.y));
          a = (vol*.45 + chispa*1.2) * borde * smoothstep(.15,.5,uv.y);
        }
        else if (_Modo < 8.5)
        {
          // Oscuridad de cueva: cierra suavemente las esquinas de la imagen.
          float2 pp = (uv*2-1) * float2(1,.85);
          a = smoothstep(.55,1.15,length(pp));
        }
        else
        {
          // Brasas sueltas que caen: se encienden de golpe, titilan y se apagan. Muy pequenas y escasas.
          // Sin viento: solo un leve vaiven. _Param: x columnas, y radio, z velocidad; _Extra.x aspecto de la capa.
          float cols = _Param.x, radio = _Param.y, vel = _Param.z, aspecto = max(_Extra.x, .1);
          float colBase = floor(uv.x * cols);
          float3 rgbAcum = float3(0,0,0);
          float pesoAcum = 0, alfaAcum = 0;
          for (int dc = -1; dc <= 1; dc++)
          {
            float c = colBase + dc;
            for (int k = 0; k < 2; k++)
            {
              float h1 = hash21(float2(c, k + 1.3));
              float h2 = hash21(float2(c + 7.1, k + 4.7));
              float h3 = hash21(float2(c + 13.7, k + 9.1));
              float ciclo = h1 + t * vel * (.05 + .05*h2);
              float f = frac(ciclo);
              // Cada cierto tiempo, muy pocas columnas sueltan una brasa.
              float activo = step(.76, hash21(float2(c*1.7 + k, floor(ciclo)*3.1 + 5.3)));
              float2 pos = float2((c + .15 + .7*h3 + sin(t*.7 + h2*6.28)*.2) / cols, 1 - f);
              float r = radio * (.75 + .5*h2);
              float cuerpo = smoothstep(0, .55, saturate(1 - length((uv - pos) * float2(aspecto, 1)) / r));
              // Se enciende rapido, se mantiene titilando y se apaga antes de llegar abajo.
              float vida = smoothstep(0, .1, f) * (1 - smoothstep(.45, .9, f));
              float titileo = .65 + .35*sin(t*11 + h3*30);
              float al = cuerpo * activo * vida * titileo;
              // Rojo brasa a naranja encendido: saturado, sin llegar a blanco.
              float3 col = lerp(float3(.95,.16,.08), float3(1,.42,.08), frac(h2*3.7 + sin(t*2 + h1*9)*.3));
              rgbAcum += col * al;
              pesoAcum += al; alfaAcum += al;
            }
          }
          rgb = rgbAcum / max(pesoAcum, .0001);
          a = saturate(alfaAcum * 1.6);
        }
        return fixed4(rgb, _Color.a * a);
      }
      ENDCG
    }
  }
}
