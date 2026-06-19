using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DisparoPotente : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
   public override void  Awake()
    {
      nombre = "Tiro Potente";
      IDenClase = 5;
      costoAP = 4;
      costoPM = 1;
      if(NIVEL == 4){costoPM -= 1;}
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 3;
      bAfectaObstaculos = true;
       tipoPorcentaje = 2;
      targetEspecial = 1; //Misma fila

      bonusAtaque = -1;
      if(NIVEL > 2){bonusAtaque += 1;}
      if(NIVEL == 5){bonusAtaque += 2;}
      XdDanio = 1;
      daniodX = 10; //1d10+3
      tipoDanio = 2; //Perforante
      criticoRangoHab = 0;
      
     penetracionArmadura = 2;

      imHab = Resources.Load<Sprite>("imHab/Explorador_TiroPotente");
      

      requiereRecurso = 2; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
      ActualizarDescripcion();

    }

        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int agilidadActual = statsUI.Agilidad;
      int ataqueActual = statsUI.Ataque;
      int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
      int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
      int bonoAtaqueNivel = bonusAtaque;
      int danioFijo = 2 + (NIVEL > 1 ? 2 : 0);
      string rangoDanio = FormatearRangoDados(1, 10, danioFijo);

      string tituloEs = "Tiro Potente I";
      string tituloEn = "Powerful Shot I";
      string tituloPt = "Tiro Potente I";
      if (NIVEL == 2) { tituloEs = "Tiro Potente II"; tituloEn = "Powerful Shot II"; }
      if (NIVEL == 3) { tituloEs = "Tiro Potente III"; tituloEn = "Powerful Shot III"; }
      if (NIVEL == 4) { tituloEs = "Tiro Potente IV a"; tituloEn = "Powerful Shot IV a"; }
      if (NIVEL == 5) { tituloEs = "Tiro Potente IV b"; tituloEn = "Powerful Shot IV b"; }
      if (NIVEL == 2) { tituloPt = "Tiro Potente II"; }
      if (NIVEL == 3) { tituloPt = "Tiro Potente III"; }
      if (NIVEL == 4) { tituloPt = "Tiro Potente IV a"; }
      if (NIVEL == 5) { tituloPt = "Tiro Potente IV b"; }

      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";
      string colorAgilidad = "#7fa35a";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
      string atributo = esIngles
        ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
        : esPortugues
          ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
          : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
      string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonoAtaqueNivel);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged line attack\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> enemies and obstacles on the same row\n";
        cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> {costoPM} Valour; consumes 1 Arrow \n";
        cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
        cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
        cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Slashing\n";
        cuerpo += $"<color={colorEncabezado}><b>Armor Penetration:</b></color> {penetracionArmadura}\n";
        cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia em linha\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> inimigos e obstaculos na mesma fila\n";
        cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> {costoPM} Valentia; consome 1 Flecha\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
        cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
        cuerpo += $"<color={colorEncabezado}><b>Penetracao de armadura:</b></color> {penetracionArmadura}\n";
        cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia en linea\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> enemigos y obstáculos en la misma fila\n";
        cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> {costoPM} Valentía; consume 1 Flecha\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
        cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
        cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
        cuerpo += $"<color={colorEncabezado}><b>Penetracion de armadura:</b></color> {penetracionArmadura}\n";
        cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
      }

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Heavy line shot with armor penetration."
        : esPortugues
          ? "Disparo pesado em linha com penetracao de armadura."
          : "Disparo pesado en linea con penetracion de armadura.";

      txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
      txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
      txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
      txtDescripcion += cuerpo;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel) { return; }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+2 roll bonus).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de rolagem.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (+2 no bonus de rolagem).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de daño.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bonus de tirada.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (-1 costo de Valentía) u Opción B (+2 al bonus de tirada).</color>"; }
      }

      if (CampaignManager.Instance != null && CampaignManager.Instance.gameObject != null && CampaignManager.Instance.gameObject.transform.parent != null && CampaignManager.Instance.gameObject.transform.parent.parent != null)
      {
        AdministradorEscenas admin = CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>();
        if (admin != null && admin.escenaActual == 1)
        {
          ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
          if (clase != null && clase.ObtenerCantidadFlechas() < requiereRecurso)
          {
            txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
          }
        }
      }
    }
    private string TextoModificadorDescripcion(int valor)
    {
      if (valor > 0) { return $" + {valor}"; }
      if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
      return "";
    }
    Casilla Origen;
    private Task impactoFilaPendiente;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
        Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(-1);
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        Casilla referencia = casillaOrigenTrampas;

        if (objetivos != null && objetivos.Count > 0)
        {
            if (objetivos[0] is Unidad unidadObjetivo)
            {
                referencia = unidadObjetivo.CasillaPosicion;
            }
            else if (objetivos[0] is Obstaculo obstaculoObjetivo)
            {
                referencia = obstaculoObjetivo.CasillaPosicion;
            }
        }

        if (referencia == null && BattleManager.Instance.casillaClickHabilidad != null)
        {
            referencia = BattleManager.Instance.casillaClickHabilidad;
        }

        if (referencia == null)
        {
            referencia = Origen;
        }

        impactoFilaPendiente = CrearProyectilFila(referencia);
        if (impactoFilaPendiente == null)
        {
            return BattleManager.DelayCombateAsync(300);
        }

        return impactoFilaPendiente;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;
       
       int danioMarca = 0;

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int bonusAtaqueTotal = bonusAtaque;
       int resultadoTirada;
       
        //Chequear si tiene Marcar Presa
       if(ChequearTieneMarcarPresa(objetivo)) //Copiar este metodo, ver bien lo de danio marca, para próximas habilidades de daño del explorador
       {
         bonusAtaqueTotal += 4;
         criticoRango += 1;
         danioMarca += 15; //Esto se suma al porcentaje de daño solamente al ser golpe critico, ver mas abajo. Ya que esta marca agrega % daño crítico.

         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 1)
         {  danioMarca += 5;   }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 2)
         {  criticoRango += 1;  }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL == 4)
         {  bonusAtaqueTotal -= 2;  } //NV 4 Quita el debuff al marcar, entonces se resta los 2 que se ponia como compensacion
       }
       //----
       resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaqueTotal, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);
     
       if(resultadoTirada == -1)
       {//PIFIA 
         print("Pifia");
         objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
         //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
       }
       else if (resultadoTirada == 0)
       {//FALLO
         print("Fallo");
         objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

       }
       else if (resultadoTirada == 1)
       {//ROCE
         print("Roce");
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarAgilidad+2;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    
        

       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarAgilidad+2;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Crítico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarAgilidad+2;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
         if(NIVEL > 1){danio += 2;}
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarAgilidad+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    private Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private async Task LanzarProyectilAsync(object objetivo)
    {
        await BattleManager.DelayCombateAsync(80);

        GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.Flecha;
        if (flechaPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(flechaPrefab);
        ArrowFlight flight = proyectil.GetComponent<ArrowFlight>();

        Transform destino = null;
        if (objetivo is Unidad unidadObjetivo)
        {
            destino = unidadObjetivo.transform;
        }
        else if (objetivo is Obstaculo obstaculoObjetivo)
        {
            destino = obstaculoObjetivo.transform;
        }
        else if (objetivo is Casilla casillaObjetivo)
        {
            destino = casillaObjetivo.transform;
        }

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.22f, 7.2f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(200);
        }
    }
    private Task CrearProyectilFila(Casilla casillaClick)
    {
        if (casillaClick == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilFilaAsync(casillaClick);
    }

    private async Task LanzarProyectilFilaAsync(Casilla casillaClick)
    {
        await BattleManager.DelayCombateAsync(10);

        int filaY = casillaClick.posY;
        List<Casilla> filaFull = new List<Casilla>();
        foreach (var c in BattleManager.Instance.lCasillasTotal)
        {
            if (c.lado != Origen.lado && c.posY == filaY)
            {
                filaFull.Add(c);
            }
        }
        if (filaFull.Count == 0)
        {
            return;
        }

        Casilla startCas = null;
        foreach (var c in filaFull)
        {
            if (c.posX == 3)
            {
                startCas = c;
                break;
            }
        }
        if (startCas == null)
        {
            startCas = filaFull[0];
            foreach (var c in filaFull)
            {
                if (c.posX > startCas.posX)
                {
                    startCas = c;
                }
            }
        }

        Casilla endCas = startCas;
        foreach (var c in filaFull)
        {
            if (c.posX < endCas.posX)
            {
                endCas = c;
            }
        }

        Vector3 dir = (endCas.transform.position - startCas.transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector3.right;
        }

        float offsetBehind = 2.2f;
        Vector3 spawnPos = startCas.transform.position - dir * offsetBehind;

        GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.FlechaPotente;
        if (flechaPrefab == null)
        {
            return;
        }

        Quaternion rotacion = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
        GameObject flecha = Instantiate(flechaPrefab, spawnPos, rotacion);
        FlechaPotenteVuelo vuelo = flecha.GetComponent<FlechaPotenteVuelo>();
        if (vuelo != null)
        {
            vuelo.Configure(dir);
            await vuelo.EsperarFinalAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(450);
        }
    }
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
    
      lObjetivosPosibles.Clear();
      
      lCasillasafectadas.Clear();
      List<Casilla> alCasillasafectadas = Origen.ObtenerCasillasRango(6,0);
    
      foreach(Casilla c in alCasillasafectadas)
      {
       
        lCasillasafectadas.Add(c);
       c.ActivarCapaColorRojo();
      
        if(c.Presente == null)
        {
            continue;
        }
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }
             if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
           }

        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

     bool ChequearTieneMarcarPresa(Unidad obj)
    { 
      if(obj.GetComponent<MarcaMarcarPresa>() != null)
      {
        if(obj.GetComponent<MarcaMarcarPresa>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
    }

    
}











