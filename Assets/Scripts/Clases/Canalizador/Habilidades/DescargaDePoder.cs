using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDePoder : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga De Poder";
      IDenClase = 2;
      costoAP = 4;
      if (NIVEL == 4) { costoAP--; }

      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      if (NIVEL == 5) { cooldownMax--; }
      bAfectaObstaculos = true;

      targetEspecial = 8; //T    
       tipoPorcentaje = 3;
      bonusAtaque = 0;
      if (NIVEL > 2) { bonusAtaque += 1; }
      XdDanio = 3;
      daniodX = 6; //3d6
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDePoder");
      

      
    }
   
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int ataqueActual = statsUI.Ataque;
      int criticoBonusUnidad = statsUI.CriticoRango;
      int bonusAtaqueNivel = NIVEL > 2 ? 1 : 0;
      int danioFijo = NIVEL > 1 ? 5 : 0;
      int criticoMin = Mathf.Clamp(19 - (criticoBonusUnidad + 2), 2, 20);

      string tituloEs = "Descarga de Poder I";
      string tituloEn = "Power Discharge I";
      string tituloPt = "Descarga de Poder I";
      if (NIVEL == 2) { tituloEs = "Descarga de Poder II"; tituloEn = "Power Discharge II"; }
      if (NIVEL == 3) { tituloEs = "Descarga de Poder III"; tituloEn = "Power Discharge III"; }
      if (NIVEL == 4) { tituloEs = "Descarga de Poder IV a"; tituloEn = "Power Discharge IV a"; }
      if (NIVEL == 5) { tituloEs = "Descarga de Poder IV b"; tituloEn = "Power Discharge IV b"; }
      if (NIVEL == 2) { tituloPt = "Descarga de Poder II"; }
      if (NIVEL == 3) { tituloPt = "Descarga de Poder III"; }
      if (NIVEL == 4) { tituloPt = "Descarga de Poder IV a"; }
      if (NIVEL == 5) { tituloPt = "Descarga de Poder IV b"; }

      string bonusAtaqueEs = bonusAtaqueNivel != 0 ? $" + {bonusAtaqueNivel}" : "";
      string bonusAtaqueEn = bonusAtaqueNivel != 0 ? $" + {bonusAtaqueNivel}" : "";

      string danioEs = danioFijo > 0
        ? $"3d6 + {danioFijo} + <color=#ea0606>Poder ({poderActual})</color>"
        : $"3d6 + <color=#ea0606>Poder ({poderActual})</color>";
      string danioEn = danioFijo > 0
        ? $"3d6 + {danioFijo} + <color=#ea0606>Power ({poderActual})</color>"
        : $"3d6 + <color=#ea0606>Power ({poderActual})</color>";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (5 range)\n";
        cuerpo += "<b>Target:</b> T area (3 horizontal + 2 at the far end)\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Power ({poderActual})</color>  {bonusAtaqueEn} vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
        cuerpo += $"<b>Damage:</b> {danioEn} | <b>Type:</b> Arcane";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Distancia (5 alcance)\n";
        cuerpo += "<b>Alvo:</b> Area em T (3 horizontal + 2 no fundo)\n";
        cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}){bonusAtaqueEs} vs Defesa. Falha critica: 1. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Dano:</b> {danioEs} | <b>Tipo:</b> Arcano";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
        cuerpo += "<b>Objetivo:</b> Area en T (3 horizontal + 2 al fondo)\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}){bonusAtaqueEs} vs Defensa. Pifia: 1. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Danio:</b> {danioEs} | <b>Tipo:</b> Arcano";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Channeler releases a concentrated shockwave that sweeps enemies in a T pattern."
          : esPortugues
            ? "O Canalizador libera uma descarga concentrada que varre inimigos em padrao de T."
          : "El Canalizador libera una descarga concentrada que barre enemigos en patron de T.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 AP) or Option B (-1 cooldown).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 AP) ou Opcao B (-1 recarga).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5 de danio.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bono de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 AP) u Opcion B (-1 enfriamiento).</color>"; }
      }
    }

    Casilla Origen;
    private Task preImpactoPendiente;

    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

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

        preImpactoPendiente = CrearProyectilFila(referencia);
        return preImpactoPendiente ?? Task.CompletedTask;
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
       
       int danioExtra = 0;
       if (NIVEL > 1) { danioExtra += 3; }

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);
       
      
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    
        

       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioExtra);
         if(NIVEL > 1){danio += 2;}
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
  
  
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
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
        int ladoRef = casillaClick.lado;
        List<Casilla> filaFull = new List<Casilla>();
        foreach (var c in BattleManager.Instance.lCasillasTotal)
        {
            if (c.lado == ladoRef && c.posY == filaY)
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
            foreach (var c in filaFull)
            {
                if (startCas == null || c.posX > startCas.posX)
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

        GameObject vfxPrefab = BattleManager.Instance.contenedorPrefabs.VFXDescargaDePoder_Fila;
        if (vfxPrefab == null)
        {
            return;
        }

        Quaternion rotacion = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
        GameObject vfx = Instantiate(vfxPrefab, spawnPos, rotacion);
        FlechaPotenteVuelo vuelo = vfx.GetComponent<FlechaPotenteVuelo>();
        if (vuelo != null)
        {
            vuelo.Configure(dir);
            await vuelo.EsperarFinalAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(400);
        }
    }

private void ObtenerObjetivos()
    {
      
     //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,0);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
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

    
}











