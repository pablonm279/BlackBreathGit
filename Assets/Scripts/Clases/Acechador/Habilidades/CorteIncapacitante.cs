using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class CorteIncapacitante : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
     public override void  Awake()
    {

      
      nombre = "Corte Incapacitante";
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 4;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = true;

      bonusAtaque = 1;
      if(NIVEL > 1) { bonusAtaque++; } //Aumenta el ataque en 1 a partir del nivel 2
      XdDanio = 2;
      daniodX = 6; //2d6+3
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;



       tipoPorcentaje = 1;



      imHab = Resources.Load<Sprite>("imHab/Acechador_CorteIncapacitante");
      ActualizarDescripcion();
    }
    
   void Start()
   {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
   }

   public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    var statsUI = ObtenerStatsDescripcionUI();

    int fuerzaActual = statsUI.Fuerza;
    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int danioFijo = 3 + damExtra;
    int dcBase = NIVEL > 2 ? 8 : 7;
    int duracion = NIVEL == 5 ? 3 : 2;
    int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConEspadacorta : 0;

    string tituloEs = "Corte Incapacitante I";
    string tituloEn = "Crippling Slash I";
    if (NIVEL == 2) { tituloEs = "Corte Incapacitante II"; tituloEn = "Crippling Slash II"; }
    if (NIVEL == 3) { tituloEs = "Corte Incapacitante III"; tituloEn = "Crippling Slash III"; }
    if (NIVEL == 4) { tituloEs = "Corte Incapacitante IV a"; tituloEn = "Crippling Slash IV a"; }
    if (NIVEL == 5) { tituloEs = "Corte Incapacitante IV b"; tituloEn = "Crippling Slash IV b"; }

    string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Fortaleza, dcBase, "Agilidad", "Agility", agilidadActual);

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Melee\n";
      cuerpo += "<b>Target:</b> 1 enemy in front melee range\n";
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Strength ({fuerzaActual})</color>   + {bonusAtaque} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Damage:</b> 2d6 + {danioFijo} + <color=#ea0606>Strength ({fuerzaActual})</color> | <b>Type:</b> Slashing\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>On failed save:</b> Crippled ({duracion} turns): Immobile, -20% Damage, -2 Attack";
      if (NIVEL == 4)
      {
        cuerpo += ", +3 Bleed";
      }
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<b>Passive applied:</b> Short Sword Mastery (Tier {nivelMaestria})";
      }
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Melee\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance melee frontal\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza ({fuerzaActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Danio:</b> 2d6 + {danioFijo} + <color=#ea0606>Fuerza ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>Si falla TS:</b> Incapacitado ({duracion} turnos): Inmovil, -20% Danio, -2 Ataque";
      if (NIVEL == 4)
      {
        cuerpo += ", +3 Sangrado";
      }
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<b>Pasiva aplicada:</b> Maestria con Espada Corta (Tier {nivelMaestria})";
      }
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A control slash that can lock enemy movement after a save check."
        : "Un corte de control que puede bloquear movimiento enemigo tras TS.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack bonus.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+3 Bleed) or Option B (+1 turn duration).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bono de ataque.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC base de TS.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+3 Sangrado) u Opcion B (+1 turno de duracion).</color>"; }
    }
  }

  int damExtra;
      Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    void ChequearMaestria()
  {
    int NivelMaestria = claseAcechador.PASIVA_MaestriaConEspadacorta;

    if (NivelMaestria == 1)
    {
      bonusAtaque = 1;
      damExtra += 2;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño.</i>\n\n";

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +1 Rango Crítico, -1 AP.</i>\n\n";


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 4;
      criticoRangoHab = 2;
      costoAP -= 1; //costo AP -1
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +4 Daño +2 Rango Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 2;
      damExtra += 4;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: Remueve Cooldown, +2 Ataque +4 Daño +1 Rango Crítico.</i>\n\n";

    }

      ActualizarDescripcion();
    
  }
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;


      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 0);
      print("Resultado tirada " + resultadoTirada);


      if (resultadoTirada == -1)
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);
        EfectoAdicional(objetivo);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        VFXAplicar(objetivo.gameObject);
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        VFXAplicar(objetivo.gameObject);
        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---

      VFXAplicar(objetivo.gameObject);
      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteIncapacitante");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
    
  void EfectoAdicional(Unidad objetivo)
  {

    int DC = 7 + (int)scEstaUnidad.mod_CarAgilidad; //DC de la tirada de salvación

    if (NIVEL > 2) { DC++; }


    if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, DC))
    {
      int duracion = 2;
      if (NIVEL == 5) { duracion++; }
       /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
       buff.buffNombre = "Incapacitado";
       buff.buffDescr = "Inmóvil, Melee solo adyacente.";
       buff.boolfDebufftBuff = false;
       buff.DuracionBuffRondas = duracion;
       buff.cantDanioPorcentaje -= 20;
       if (NIVEL == 4) { objetivo.estado_sangrado += 3; }
       buff.cantAtAgi -= 2;
       buff.AplicarBuff(objetivo);
       objetivo.estado_inmovil = duracion;
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);



    }

  }
   
    //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      if(esMelee) 
      {
        if(Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
        {
           rangoPlus = AumentarRangoMelee();
        }

        if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
        {
          rangoPlus ++;
        }
      }
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(1+rangoPlus,1);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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

    private int AumentarRangoMelee() //aumenta el rango melee si no hay nada en frente ni filas adyacentes al origen de la habilidad
    {
     
      LadoManager scLado = Origen.ladoOpuesto.GetComponent<LadoManager>();

      int posYorigen = scEstaUnidad.CasillaPosicion.posY;
      

      List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
      List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();
    
      foreach(Transform child in Origen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
      {
          Casilla cas = child.GetComponent<Casilla>();

          if(cas.posX == 3) //Columna 1 (frente)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna1.Add(cas);
             }
          }

          if(cas.posX == 2) //Columna 2 (medio)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna2.Add(cas);
             }
          }

        
      }

       //Se fija si las 3 casillas de la columna 1 están vacias
       foreach(Casilla cas in casillasAdyacentesyFrenteColumna1)
       {
          if(cas.bTieneUnidadoObstaculoParaMelee()) //si alguna de las 3 tiene algo, no aumenta el rango melee
          {
            return 0;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }



       

       foreach(Casilla cas in casillasAdyacentesyFrenteColumna2) 
       {
          if(cas.bTieneUnidadoObstaculoParaMelee()) //y si alguna de las 3 tiene algo, aumenta solo en 1 
          {
            return 1;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna2) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }




       return 2; //si ninguna de las 2 columnas tiene algo, aumenta al maximo
    }

  int TieneObstaculooUnidadAdelanteDeSuLado()
    {
      int orX = Origen.posX;
      int orY = Origen.posY;
      GameObject lado = Origen.ladoGO;

      
      if(orX != 2) //Solamente util en la columna del medio
      {
         return 0;
      }
 
       Casilla casillaRevisar = null;
       foreach(Transform child in lado.transform)
       {
         Casilla cas = child.GetComponent<Casilla>();
         if((cas.posY == orY)&&(cas.posX == orX+1))
         {
          casillaRevisar = cas;
         }

       }

      if(casillaRevisar.Presente != null)
      {
        if(casillaRevisar.Presente.GetComponent<Unidad>() != null)
        {
          return 1; //Devuelve 1 si es unidad
        }

        if(casillaRevisar.Presente.GetComponent<Obstaculo>() != null)
        {
           if(casillaRevisar.Presente.GetComponent<Obstaculo>().bPermiteAtacarDetras)
          {
            return 2; //Devuelve 2 si es obstaculo
          }
          else{ return 0;}
        }
      }
      return 0; //Devuelve 0 si no hay nada 
    }
}





