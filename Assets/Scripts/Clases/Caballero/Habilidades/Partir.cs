using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Partir : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de crpitico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ãcido - 8: Arcano

 

      public override void  Awake()
    {
      nombre = "Partir";
      IDenClase = 6;
      costoAP = 4;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 3;
      bAfectaObstaculos = true;

      bonusAtaque = 0; 
      XdDanio = 2;
      daniodX = 10; //2d10 +5
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;
       tipoPorcentaje = 1;
       imHab = Resources.Load<Sprite>("imHab/Caballero_Partir");
      
      ActualizarDescripcion();
    
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      var statsUI = ObtenerStatsDescripcionUI();

      int fuerzaActual = statsUI.Fuerza;
      int ataqueActual = statsUI.Ataque;
      int bonusAtaqueNivel = NIVEL > 2 ? 2 : 0;
      int bonusCritNivel = NIVEL == 5 ? 1 : 0;
      int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab + bonusCritNivel), 2, 20);
      int danioBaseFijo = 5 + (NIVEL > 1 ? 4 : 0);
      int dcMiedo = NIVEL == 4 ? 15 : 13;
      string bonusAtaqueTxt = bonusAtaqueNivel >= 0 ? $" + {bonusAtaqueNivel}" : $" - {Mathf.Abs(bonusAtaqueNivel)}";
      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Mental, dcMiedo);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Mental, dcMiedo);

      string tituloEs = "Partir I";
      string tituloEn = "Cleave I";
      if (NIVEL == 2) { tituloEs = "Partir II"; tituloEn = "Cleave II"; }
      if (NIVEL == 3) { tituloEs = "Partir III"; tituloEn = "Cleave III"; }
      if (NIVEL == 4) { tituloEs = "Partir IV a"; tituloEn = "Cleave IV a"; }
      if (NIVEL == 5) { tituloEs = "Partir IV b"; tituloEn = "Cleave IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Melee\n";
        cuerpo += "<b>Target:</b> 1 enemy in front range\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Strength ({fuerzaActual})</color>  {bonusAtaqueTxt} vs Defense. Fumble: 1-2. Crit: {criticoMin}-20\n";
        cuerpo += $"<b>Damage:</b> 2d10 + {danioBaseFijo} + <color=#ea0606>Strength ({fuerzaActual})</color> | <b>Type:</b> Slashing\n";
        cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
        cuerpo += "<b>On kill:</b> All enemies roll save\n";
        cuerpo += $"{lineaSalvacionEn}\n";
        cuerpo += "<b>On failed save:</b> Terrified for 2 turns (-2 Attack, -1 Max AP, -2 Mental Save)";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Melee\n";
        cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueTxt} vs Defensa. Pifia: 1-2. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Danio:</b> 2d10 + {danioBaseFijo} + <color=#ea0606>Fuerza ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Si mata al objetivo:</b> todos los enemigos hacen TS\n";
        cuerpo += $"{lineaSalvacionEs}\n";
        cuerpo += "<b>Si falla TS:</b> Aterrorizado por 2 turnos (-2 Ataque, -1 AP Max, -2 TS Mental)";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Val Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Val: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "A crushing execution cut that can panic the entire enemy side on kill."
          : "Un corte de ejecucion brutal que puede entrar en panico al lado enemigo al matar.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +4 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 attack roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 save DC on kill) or Option B (+1 crit range).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +4 de danio.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 al bono de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 DC del efecto al matar) u Opcion B (+1 rango critico).</color>"; }
      }
    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //AcÃ¡ van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       int danioMarca = 0;
       if(NIVEL > 2)
       {bonusAtaque += 2;}
       if(NIVEL == 5)
       {criticoRangoHab += 1;}
       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       if(ChequearTieneSiguesTu(objetivo))
       {
         bonusAtaque += 5;
         danioMarca = 8;
         Destroy(objetivo.GetComponent<MarcaSiguesTu>());

         if(gameObject.GetComponent<SiguesTu>().NIVEL > 1)
         { criticoRango +=2;    }
         if(gameObject.GetComponent<SiguesTu>().NIVEL > 2)
         { danioMarca +=2;    }
       }

      

      
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 1); // En habilidades caballero +1 a pifia, debilidad de Caballero
       print("Resultado tirada "+resultadoTirada);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
        
                 VFXAplicar(objetivo.gameObject);

      }
     
    

       fueElObjetivoAsesinado = objetivo;
      Invoke("ChequeoMuerteObjetivo", 3.0f); //Chequea si el objetivo muriÃ³, y aplica efectos de ser asÃ­.





        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //AcÃ¡ van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
     Unidad fueElObjetivoAsesinado;
    bool ChequearTieneSiguesTu(Unidad obj)
    { 
      if(obj.GetComponent<MarcaSiguesTu>() != null)
      {
        if(obj.GetComponent<MarcaSiguesTu>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
    }
    void VFXAplicar(GameObject objetivo)
    {
         VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Partir");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
     void ChequeoMuerteObjetivo()
  {
    bool aplicarEfectos = false;
    if (fueElObjetivoAsesinado == null)
    {
      aplicarEfectos = true; //Si no existe se asume que murio
    } //Si no habÃ­a objetivo, no hace nada
    else if (fueElObjetivoAsesinado.HP_actual < 1)
    {
      aplicarEfectos = true; //Si no tiene vida, murio
    }

    if (aplicarEfectos)
    { 
      PARTIREfectoAEnemigosPorMuerte();
    }
    fueElObjetivoAsesinado = null;
  }
    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si estÃ¡ en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
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
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras tambiÃ©n
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
    
       void VFXAplicarEnemigo(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoEnemigo");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---
  }
    private int AumentarRangoMelee() //aumenta el rango melee si no hay nada en frente ni filas adyacentes al origen de la habilidad
  {

    LadoManager scLado = Origen.ladoOpuesto.GetComponent<LadoManager>();

    int posYorigen = scEstaUnidad.CasillaPosicion.posY;


    List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
    List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

    foreach (Transform child in Origen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
    {
      Casilla cas = child.GetComponent<Casilla>();

      if (cas.posX == 3) //Columna 1 (frente)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna1.Add(cas);
        }
      }

      if (cas.posX == 2) //Columna 2 (medio)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna2.Add(cas);
        }
      }


    }

    //Se fija si las 3 casillas de la columna 1 estÃ¡n vacias
    foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
    {
      if (cas.bTieneUnidadoObstaculoParaMelee()) //si alguna de las 3 tiene algo, no aumenta el rango melee
      {
        return 0;
      }
    }
    foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
    { casOsc.ActivarCapaColorNegro(); }





    foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
    {
      if (cas.bTieneUnidadoObstaculoParaMelee()) //y si alguna de las 3 tiene algo, aumenta solo en 1 
      {
        return 1;
      }
    }
    foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna2) //si ninguna de las tres tiene algo, las oscurece
    { casOsc.ActivarCapaColorNegro(); }




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




    void PARTIREfectoAEnemigosPorMuerte()
    {
       
        List<Unidad> enemigos = new List<Unidad>();

        foreach(Casilla cas in scEstaUnidad.CasillaPosicion.ObtenerCasillasLadoOpuesto())
        {
           if(cas.Presente != null)
           {
             if(cas.Presente.GetComponent<Unidad>() != null)
             {
                Unidad uni = cas.Presente.GetComponent<Unidad>();
                int nDif = 13;
                if(NIVEL == 4){nDif += 2;}

                if(uni.TiradaSalvacion(uni.mod_TSMental, nDif))
                {
                    /////////////////////////////////////////////
                    //BUFF ---- AsÃ­ se aplica un buff/debuff
                    Buff buff = new Buff();
                    buff.buffNombre = "Aterrorizado";
                    buff.boolfDebufftBuff = false;
                    buff.DuracionBuffRondas = 2;
                    buff.cantAtaque -= 2;
                    buff.cantAPMax -= 1;
                    buff.cantTsMental -= 2;
                    buff.AplicarBuff(uni);
                    // Agrega el componente Buff al objeto objetivo y asigna la configuraciÃ³n del buff
                    Buff buffComponent = ComponentCopier.CopyComponent(buff, uni.gameObject);
                    
                    VFXAplicarEnemigo(uni.gameObject);
                 
                }
                else
                {
                   // uni.GenerarTextoFlotante("Resiste Aterrorizado", Color.cyan);
                }


             }



           }         



        }




    }
}




