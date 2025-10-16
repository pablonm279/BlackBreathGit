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







      imHab = Resources.Load<Sprite>("imHab/Acechador_CorteIncapacitante");

       
    }
    
   void Start()
   {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
   }

   public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Corte Incapacitante I</b></color>\n\n";
      txtDescripcion += "<i>Realiza un corte que incapacita los movimientos del enemigo temporalmente.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Fuerza</color><i> Daño Cortante: 2d6 + 3 + Fuerza. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 7 + Agilidad: Incapacitado: Inmóbil, -20% daño -2 Ataque. 2 Turnos</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Espada Corta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Corte Incapacitante II</b></color>\n\n";
      txtDescripcion += "<i>Realiza un corte que incapacita los movimientos del enemigo temporalmente.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Fuerza+1</color><i> Daño Cortante: 2d6 + 3 + Fuerza. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 7 + Agilidad: Incapacitado: Inmóbil, -20% daño -2 Ataque. 2 Turnos</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Espada Corta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Corte Incapacitante III</b></color>\n\n";
      txtDescripcion += "<i>Realiza un corte que incapacita los movimientos del enemigo temporalmente.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Fuerza+1</color><i> Daño Cortante: 2d6 + 3 + Fuerza. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 8 + Agilidad: Incapacitado: Inmóbil, -20% daño -2 Ataque. 2 Turnos</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Espada Corta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Aplica 3 Sangrado</color>\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: +1 Turno Duración Incapacitado</color>\n";
          }
        }
      }

    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Corte Incapacitante IVa</b></color>\n\n";
      txtDescripcion += "<i>Realiza un corte que incapacita los movimientos del enemigo temporalmente.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Fuerza+1</color><i> Daño Cortante: 2d6 + 3 + Fuerza. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 8 + Agilidad: Incapacitado: Inmóbil, -20% daño -2 Ataque, aplica 3 Sangrado. 2 Turnos</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Espada Corta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Corte Incapacitante IVb</b></color>\n\n";
      txtDescripcion += "<i>Realiza un corte que incapacita los movimientos del enemigo temporalmente.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Fuerza+1</color><i> Daño Cortante: 2d6 + 3 + Fuerza. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 8 + Agilidad: Incapacitado: Inmóbil, -20% daño -2 Ataque. 3 Turnos</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Espada Corta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    }
   
    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      if (NIVEL < 2)
      {
        txtDescripcion = "<color=#5dade2><b>Crippling Slash I</b></color>\n\n";
        txtDescripcion += "<i>Delivers a slash that temporarily cripples the enemy's movements.</i>\n\n";
        txtDescripcion += $"-Attack: <color=#ea0606>Strength</color><i> Slashing Damage: 2d6 + 3 + Strength. </i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>On hit - Fortitude Save DC 7 + Agility: Crippled: Immobile, -20% damage, -2 Attack. 2 Turns</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Uses Short Sword -Cooldown: {cooldownMax} \n- AP Cost: {costoAP} Effortable \n- Val Cost: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +1 Attack</color>\n\n";
            }
          }
        }
      }
      if (NIVEL == 2)
      {
        txtDescripcion = "<color=#5dade2><b>Crippling Slash II</b></color>\n\n";
        txtDescripcion += "<i>Delivers a slash that temporarily cripples the enemy's movements.</i>\n\n";
        txtDescripcion += $"-Attack: <color=#ea0606>Strength+1</color><i> Slashing Damage: 2d6 + 3 + Strength. </i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>On hit - Fortitude Save DC 7 + Agility: Crippled: Immobile, -20% damage, -2 Attack. 2 Turns</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Uses Short Sword -Cooldown: {cooldownMax} \n- AP Cost: {costoAP} Effortable \n- Val Cost: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +1 DC</color>\n\n";
            }
          }
        }
      }
      if (NIVEL == 3)
      {
        txtDescripcion = "<color=#5dade2><b>Crippling Slash III</b></color>\n\n";
        txtDescripcion += "<i>Delivers a slash that temporarily cripples the enemy's movements.</i>\n\n";
        txtDescripcion += $"-Attack: <color=#ea0606>Strength+1</color><i> Slashing Damage: 2d6 + 3 + Strength. </i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>On hit - Fortitude Save DC 8 + Agility: Crippled: Immobile, -20% damage, -2 Attack. 2 Turns</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Uses Short Sword -Cooldown: {cooldownMax} \n- AP Cost: {costoAP} Effortable \n- Val Cost: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Option A: Applies 3 Bleed</color>\n";
              txtDescripcion += $"<color=#dfea02>-Option B: +1 Turn Crippled Duration</color>\n";
            }
          }
        }
      }
      if (NIVEL == 4)
      {
        txtDescripcion = "<color=#5dade2><b>Crippling Slash IVa</b></color>\n\n";
        txtDescripcion += "<i>Delivers a slash that temporarily cripples the enemy's movements.</i>\n\n";
        txtDescripcion += $"-Attack: <color=#ea0606>Strength+1</color><i> Slashing Damage: 2d6 + 3 + Strength. </i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>On hit - Fortitude Save DC 8 + Agility: Crippled: Immobile, -20% damage, -2 Attack, applies 3 Bleed. 2 Turns</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Uses Short Sword -Cooldown: {cooldownMax} \n- AP Cost: {costoAP} Effortable \n- Val Cost: {costoPM} </color>\n\n";
      }
      if (NIVEL == 5)
      {
        txtDescripcion = "<color=#5dade2><b>Crippling Slash IVb</b></color>\n\n";
        txtDescripcion += "<i>Delivers a slash that temporarily cripples the enemy's movements.</i>\n\n";
        txtDescripcion += $"-Attack: <color=#ea0606>Strength+1</color><i> Slashing Damage: 2d6 + 3 + Strength. </i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>On hit - Fortitude Save DC 8 + Agility: Crippled: Immobile, -20% damage, -2 Attack. 3 Turns</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Uses Short Sword -Cooldown: {cooldownMax} \n- AP Cost: {costoAP} Effortable \n- Val Cost: {costoPM} </color>\n\n";
      }
    }
   
  if (SceneManager.GetActiveScene().name != "ES-Campaña")
    {
      int NivelMaestria = claseAcechador.PASIVA_MaestriaConBallestaMano;
      if (NivelMaestria == 1)
      {
        if (TRADU.i.nIdioma == 1)
        { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño.</i>\n\n"; }
        if (TRADU.i.nIdioma == 2)
        {
          txtDescripcion += "\n\n<i>Short Sword Mastery adds: +1 Attack +2 Damage.</i>\n\n";
        }
      }
      else if (NivelMaestria == 2)
      {
        if (TRADU.i.nIdioma == 1)
        { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n"; }
        if (TRADU.i.nIdioma == 2)
        {
          txtDescripcion += "\n\n<i>Short Sword Mastery adds: +1 Attack +2 Damage +1 Critical Range.</i>\n\n";
        }
      }
      else if (NivelMaestria == 3)
      {
        if (TRADU.i.nIdioma == 1)
        { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +1 Rango Crítico, -1 AP.</i>\n\n"; }
        if (TRADU.i.nIdioma == 2)
        {
          txtDescripcion += "\n\n<i>Short Sword Mastery adds: +1 Attack +2 Damage +1 Critical Range, -1 AP.</i>\n\n";
        }
      
      }
      else if (NivelMaestria == 4)
      {
         if (TRADU.i.nIdioma == 1)
        { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +4 Daño +2 Rango Crítico.</i>\n\n"; }
        if (TRADU.i.nIdioma == 2)
        {
          txtDescripcion += "\n\n<i>Short Sword Mastery adds: +1 Attack +4 Damage +2 Critical Range.</i>\n\n";
        }
     
      }
      else if (NivelMaestria == 5)
      {
         if (TRADU.i.nIdioma == 1)
        { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: Remueve Cooldown, +2 Ataque +4 Daño +1 Rango Crítico.</i>\n\n"; }
        if (TRADU.i.nIdioma == 2)
        {
          txtDescripcion += "\n\n<i>Short Sword Mastery adds: Removes Cooldown, +2 Attack +4 Damage +1 Critical Range.</i>\n\n";
        }
      
      }
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
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

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
          if(cas.bTieneUnidadoObstaculo()) //si alguna de las 3 tiene algo, no aumenta el rango melee
          {
            return 0;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }



       

       foreach(Casilla cas in casillasAdyacentesyFrenteColumna2) 
       {
          if(cas.bTieneUnidadoObstaculo()) //y si alguna de las 3 tiene algo, aumenta solo en 1 
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
