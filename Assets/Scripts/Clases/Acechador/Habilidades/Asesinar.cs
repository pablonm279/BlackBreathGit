using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Asesinar : Habilidad
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


      nombre = "Asesinar";
      costoAP = 3; 
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 6;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4; 
      bAfectaObstaculos = false;

      bonusAtaque = 0;
    
      XdDanio = 2;
      daniodX = 8; //2d8+2
      tipoDanio = 1; //Cortante
      criticoRangoHab = 0;




      requiereRecurso = 1; //No requiere recurso


      imHab = Resources.Load<Sprite>("imHab/Acechador_Asesinar");

       
    }
    
   void Start()
   {
  
   }

   public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Asesinar I</b></color>\n\n";
      txtDescripcion += "<i>Ataca a un objetivo desde las sombras, infiriéndole grandes daños si el enemigo no tiene aliados alrededor.</i>\n\n";
      txtDescripcion += "<i>Al matar: reduce el Enfriamiento a 1 y el Acechador obtiene Escondido I.</i>\n\n";
      txtDescripcion += "<i><color=#ea0606>--Requiere estar Escondido para usar--</color></i>\n\n";
      txtDescripcion += $"-Ataque:<color=#ea0606>Agilidad</color><i> Daño Perforante: 2d8 + 2 + Agilidad. +4 a Humanoides. Si el objetivo está Aislado, +2 Ataque y duplica daño.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño.</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Asesinar II</b></color>\n\n";
      txtDescripcion += "<i>Ataca a un objetivo desde las sombras, infiriéndole grandes daños si el enemigo no tiene aliados alrededor.</i>\n\n";
      txtDescripcion += "<i>Al matar: reduce el Enfriamiento a 1 y el Acechador obtiene Escondido I.</i>\n\n";
      txtDescripcion += "<i><color=#ea0606>--Requiere estar Escondido para usar--</color></i>\n\n";
      txtDescripcion += $"-Ataque:<color=#ea0606>Agilidad</color><i> Daño Perforante: 2d8 + 4 + Agilidad. +4 a Humanoides. Si el objetivo está Aislado, +2 Ataque y duplica daño.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque si está aislado.</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Asesinar III</b></color>\n\n";
      txtDescripcion += "<i>Ataca a un objetivo desde las sombras, infiriéndole grandes daños si el enemigo no tiene aliados alrededor.</i>\n\n";
      txtDescripcion += "<i>Al matar: reduce el Enfriamiento a 1 y el Acechador obtiene Escondido I.</i>\n\n";
      txtDescripcion += "<i><color=#ea0606>--Requiere estar Escondido para usar--</color></i>\n\n";
      txtDescripcion += $"-Ataque:<color=#ea0606>Agilidad</color><i> Daño Perforante: 2d8 + 4 + Agilidad. +4 a Humanoides. Si el objetivo está Aislado, +3 Ataque y duplica daño.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Al matar, +1 Valentía.</color>\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: +3 Daño.</color>\n";
          }
        }
      }

    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Asesinar IVa</b></color>\n\n";
      txtDescripcion += "<i>Ataca a un objetivo desde las sombras, infiriéndole grandes daños si el enemigo no tiene aliados alrededor.</i>\n\n";
      txtDescripcion += "<i>Al matar: reduce el Enfriamiento a 1 y el Acechador obtiene Escondido I y +1 Valentía.</i>\n\n";
      txtDescripcion += "<i><color=#ea0606>--Requiere estar Escondido para usar--</color></i>\n\n";
      txtDescripcion += $"-Ataque:<color=#ea0606>Agilidad</color><i> Daño Perforante: 2d8 + 4 + Agilidad. +4 a Humanoides. Si el objetivo está Aislado, +3 Ataque y duplica daño.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Asesinar IVb</b></color>\n\n";
      txtDescripcion += "<i>Ataca a un objetivo desde las sombras, infiriéndole grandes daños si el enemigo no tiene aliados alrededor.</i>\n\n";
      txtDescripcion += "<i>Al matar: reduce el Enfriamiento a 1 y el Acechador obtiene Escondido I.</i>\n\n";
      txtDescripcion += "<i><color=#ea0606>--Requiere estar Escondido para usar--</color></i>\n\n";
      txtDescripcion += $"-Ataque:<color=#ea0606>Agilidad</color><i> Daño Perforante: 2d8 + 7 + Agilidad. +4 a Humanoides. Si el objetivo está Aislado, +3 Ataque y duplica daño.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
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
        return Task.Delay(250);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;

      if (NIVEL > 1) { damExtra += 2; } //A partir del nivel 2, +2 de daño extra
      if (NIVEL == 5) { damExtra += 3; } //A Nv 5, +3 de daño extra

      if (objetivo.ChequearEstaAislado(2))
      {
        bonusAtaque += 2; //Si está aislado, +2 Ataque
        if (NIVEL > 2) { bonusAtaque++; } //A partir del nivel 3, +3 Ataque si está aislado
      }

      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0);
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }

         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }
        
         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      

      }
      else if (resultadoTirada == 3)
      {//CRITICO
                print("Critico");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        } 
        
        if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }


        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
      }

      if (objetivo.HP_actual < 1 && resultadoTirada > 0)
      {
        scEstaUnidad.GanarEscondido(1);
        cooldownActual = 1; //Si mata, reduce el cooldown a 1 turno.

        if (NIVEL == 4) { scEstaUnidad.SumarValentia(2); }
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
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ASesinar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

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
   
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(4,0);
    
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

   
 
}





