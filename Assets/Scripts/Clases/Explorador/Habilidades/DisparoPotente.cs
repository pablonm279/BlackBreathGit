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
      int bonoAtaqueNivel = bonusAtaque;
      int danioFijo = 2 + (NIVEL > 1 ? 2 : 0);

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

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (line)\n";
        cuerpo += "<b>Target:</b> Enemies on the same row\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Agility ({agilidadActual})</color>   + {bonoAtaqueNivel} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
        cuerpo += $"<b>Damage:</b> 1d10 + {danioFijo} + <color=#ea0606>Agility ({agilidadActual})</color> | <b>Type:</b> Slashing\n";
        cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Resource:</b> consumes 2 Arrows\n";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Distancia (linha)\n";
        cuerpo += "<b>Alvo:</b> Inimigos na mesma fila\n";
        cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Agilidade ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonoAtaqueNivel} vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
        cuerpo += $"<b>Dano:</b> 1d10 + {danioFijo} + <color=#ea0606>Agilidade ({agilidadActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Recurso:</b> consome 2 Flechas\n";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (linea)\n";
        cuerpo += "<b>Objetivo:</b> Enemigos en la misma fila\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Agilidad ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonoAtaqueNivel} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
        cuerpo += $"<b>Danio:</b> 1d10 + {danioFijo} + <color=#ea0606>Agilidad ({agilidadActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Recurso:</b> consume 2 Flechas\n";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A heavy line shot that pierces a full row at high AP cost."
          : esPortugues
            ? "Um disparo pesado em linha que atravessa a fila inteira com alto custo de AP."
          : "Un disparo de linea pesado que barre una fila entera con alto costo de AP.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+2 attack roll bonus).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (+2 no bonus de ataque).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de danio.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bono de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de Valentía) u Opcion B (+2 al bono de ataque).</color>"; }
      }

      if (CampaignManager.Instance != null && CampaignManager.Instance.gameObject != null && CampaignManager.Instance.gameObject.transform.parent != null && CampaignManager.Instance.gameObject.transform.parent.parent != null)
      {
        AdministradorEscenas admin = CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>();
        if (admin != null && admin.escenaActual == 1)
        {
          ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
          if (clase != null && clase.ObtenerCantidadFlechas() < 1)
          {
            txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
          }
        }
      }
    }
    Casilla Origen;
    private Task impactoFilaPendiente;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
        Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(-2);
      
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
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);
       
        //Chequear si tiene Marcar Presa
       if(ChequearTieneMarcarPresa(objetivo)) //Copiar este metodo, ver bien lo de danio marca, para próximas habilidades de daño del explorador
       {
         bonusAtaque += 4;
         criticoRango += 1;
         danioMarca += 15; //Esto se suma al porcentaje de daño solamente al ser golpe critico, ver mas abajo. Ya que esta marca agrega % daño crítico.

         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 1)
         {  danioMarca += 5;   }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 2)
         {  criticoRango += 1;  }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL == 4)
         {  bonusAtaque -= 2;  } //NV 4 Quita el debuff al marcar, entonces se resta los 2 que se ponia como compensacion
       }
       //----
     
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
         print("Critico");

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
            flight.Configure(transform, destino, 0.12f, 9.2f);
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











