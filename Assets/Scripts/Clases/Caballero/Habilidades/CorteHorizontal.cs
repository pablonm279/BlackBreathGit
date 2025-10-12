using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CorteHorizontal : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Corte Horizontal";
      IDenClase = 3;
      costoAP = 4;
      costoPM = 2;
      if(NIVEL == 4){costoPM -= 1;}
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 2;
      bAfectaObstaculos = true;

      targetEspecial = 4;

      bonusAtaque = -2;
      if(NIVEL > 2){bonusAtaque += 1;}
      XdDanio = 2;
      daniodX = 6; //2d6
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;

      imHab = Resources.Load<Sprite>("imHab/Caballero_CorteHorizontal");
      
      ActualizarDescripcion();
      
    }

    public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Horizontal I</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque horizontal que afecta a varios enemigos delante de él.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE - 3 de Ancho</b> -Ataque: <color=#ea0606>Fuerza {bonusAtaque}</color> - Daño: Cortante 2d6- </color>\n";
        txtDescripcion += $"Enemigos: TS Fortaleza vs 2. Aplica 3 Sangrado.\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño</color>\n\n";
          }
          }
        }
   
      }
      if(NIVEL== 2)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Horizontal II</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque horizontal que afecta a varios enemigos delante de él.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE - 3 de Ancho</b> -Ataque: <color=#ea0606>Fuerza {bonusAtaque}</color> - Daño: Cortante 2d6+2- </color>\n\n";
        txtDescripcion += $"Enemigos: TS Fortaleza vs 2. Aplica 3 Sangrado.\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Horizontal III</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque horizontal que afecta a varios enemigos delante de él.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE - 3 de Ancho</b> -Ataque: <color=#ea0606>Fuerza {bonusAtaque}</color> - Daño: Cortante 2d6+2- </color>\n\n";
        txtDescripcion += $"Enemigos: TS Fortaleza vs 2. Aplica 3 Sangrado.\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: -1 costo Valentía</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 TS Fortaleza +1 Sangrado Aplicado</color>\n";
          }
          }
        }

      }
      if(NIVEL== 4)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Horizontal IV a</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque horizontal que afecta a varios enemigos delante de él.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE - 3 de Ancho</b> -Ataque: <color=#ea0606>Fuerza {bonusAtaque}</color> - Daño: Cortante 2d6+2- </color>\n\n";
        txtDescripcion += $"Enemigos: TS Fortaleza vs 2. Aplica 3 Sangrado.\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>";
      }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Horizontal IV b</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque horizontal que afecta a varios enemigos delante de él.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE - 3 de Ancho</b> -Ataque: <color=#ea0606>Fuerza {bonusAtaque}</color> - Daño: Cortante 2d6+2- </color>\n\n";
        txtDescripcion += $"Enemigos: TS Fortaleza vs 3. Aplica 4 Sangrado.\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>";
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
    
    
   public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
   {
    // El log de uso ahora está centralizado en Habilidad.Resolver
     VFXAplicarOrigen(Usuario.gameObject);
   await base.Resolver(Objetivos);
   
   }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.6f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        return Task.Delay(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 1/*!!1 solo en caballero*/); // En habilidades caballero +1 a pifia, debilidad de Caballero
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);
      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
    void EfectoAdicional(Unidad Objetivo)
    {
       if(NIVEL !=5)
       {
        if(Objetivo.TiradaSalvacion(Objetivo.mod_TSFortaleza, 12))
        {
           Objetivo.estado_sangrado += 3;
            
        }
       }else //Si es nivel 4b, +1DC sangrado +1 stack
       {
         if(Objetivo.TiradaSalvacion(Objetivo.mod_TSFortaleza, 13))
        {
          Objetivo.estado_sangrado += 4;
            
        }
       }
    }
    void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteHorizontalImpacto");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---

  }
  
   void VFXAplicarOrigen(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteHorizontalOrigen");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---

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
      List<Casilla> alCasillasafectadas = Origen.ObtenerCasillasRango(2+rangoPlus,1);
    
      foreach(Casilla c in alCasillasafectadas)
      {
       lCasillasafectadas.Add(c);
       
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
