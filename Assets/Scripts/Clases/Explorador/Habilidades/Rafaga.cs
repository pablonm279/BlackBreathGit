using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class Rafaga : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private int hAlcance = 7;
    private int hAncho = 3; //1 - adyancentes también
      public override void  Awake()
    {
      nombre = "Ráfaga";
      IDenClase = 10; // Termina turno
      costoAP = 0;
      costoPM = 2;
      if(NIVEL == 4){costoPM--;}

      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
       tipoPorcentaje = 2;
      bonusAtaque = -2;
      if(NIVEL > 1){bonusAtaque++;}
      if(NIVEL == 5){bonusAtaque++;}
     
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 1; //Perforante
      criticoRangoHab = 0;

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.

      







      imHab = Resources.Load<Sprite>("imHab/Explorador_Rafaga");
      ActualizarDescripcion();
    }


    public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    string tituloEs = "Rafaga I";
    string tituloEn = "Barrage I";
    if (NIVEL == 2) { tituloEs = "Rafaga II"; tituloEn = "Barrage II"; }
    if (NIVEL == 3) { tituloEs = "Rafaga III"; tituloEn = "Barrage III"; }
    if (NIVEL == 4) { tituloEs = "Rafaga IV a"; tituloEn = "Barrage IV a"; }
    if (NIVEL == 5) { tituloEs = "Rafaga IV b"; tituloEn = "Barrage IV b"; }

    int bonusAtaqueNivel = bonusAtaque;

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<b>Type:</b> Ranged ({hAlcance} range)\n";
      cuerpo += "<b>Target:</b> 1 enemy in wide range (3-width). If it dies, continues on the next enemy in list\n";
      cuerpo += "<b>Loop:</b> repeats shots until current AP reaches 0 or arrows reach 0\n";
      cuerpo += $"<b>Roll (per shot):</b> 1d20 + <color=#ea0606>Agility ({agilidadActual})</color>   + {bonusAtaqueNivel} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Damage (per shot):</b> 1d10 + 1 + <color=#ea0606>Agility ({agilidadActual})</color> | <b>Type:</b> Piercing\n";
      cuerpo += "<b>Resource:</b> consumes 1 Arrow and 1 AP per shot\n";
      cuerpo += "<b>Turn flow:</b> using this skill ends your turn";
    }
    else
    {
      cuerpo += $"<b>Tipo:</b> Rango ({hAlcance} alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo en rango amplio (ancho 3). Si muere, continua sobre el siguiente enemigo de la lista\n";
      cuerpo += "<b>Bucle:</b> repite disparos hasta que tus AP actuales lleguen a 0 o te quedes sin flechas\n";
      cuerpo += $"<b>Tirada (por disparo):</b> 1d20 + <color=#ea0606>Agilidad ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonusAtaqueNivel} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Danio (por disparo):</b> 1d10 + 1 + <color=#ea0606>Agilidad ({agilidadActual})</color> | <b>Tipo:</b> Perforante\n";
      cuerpo += "<b>Recurso:</b> consume 1 Flecha y 1 AP por disparo\n";
      cuerpo += "<b>Flujo de turno:</b> usar esta habilidad termina tu turno";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: variable (1 per shot)\n- Val Cost: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: variable (1 por disparo)\n- Costo Val: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A sustained arrow sequence that drains your current action economy."
        : "Una secuencia sostenida de flechas que vacia tu economia de acciones actual.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (mostrarProximoNivel)
    {
      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack bonus.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Val cost) or Option B (+2 attack bonus).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bono de ataque.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo Val) u Opcion B (+2 al bono de ataque).</color>"; }
      }
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
    public override void Activar()
    {
        if(Usuario.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() > 0)
        {
          Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
          ObtenerObjetivos();

        
          BattleManager.Instance.SeleccionandoObjetivo = true;
          BattleManager.Instance.HabilidadActiva = this;
        }

        
    }
    
      

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    {
      Unidad objetivo = (Unidad)obj;
      ClaseExplorador scEstaUnidadExp = Usuario.GetComponent<ClaseExplorador>();
      while(scEstaUnidad.ObtenerAPActual() > 0 && scEstaUnidadExp.ObtenerCantidadFlechas() > 0)
      {

          BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(scEstaUnidad);
          BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

      scEstaUnidad.CambiarAPActual(-1);  //Gasta 1 AP por cada ataque
          int tir = UnityEngine.Random.Range(1,21); 
          await Atacar(objetivo, tir);
          await Task.Delay(800);

          if(objetivo.HP_actual < 1)
          {
            List<Unidad> lEnemigos = new List<Unidad>();
            lEnemigos = objetivo.ObtenerListaAliados(false);
            if(lEnemigos.Count > 0)
            {
              objetivo = lEnemigos[0]; //Ataca al siguiente enemigo en la lista
            }
            else
            {

              break;

            }


          }

                
      }
      
      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();
           
    }

    async Task Atacar(object obj, int tirada)
    {
      
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;
       
      Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(-1);
      Task impacto = CrearProyectil(objetivo);
      scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

      if (impacto != null)
      {
        await impacto;
      }
      else
      {
        await Task.Delay(200);
      }
      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       
       //Chequear si tiene Marcar Presa
       if(ChequearTieneMarcarPresa(objetivo)) //Copiar este metodo, ver bien lo de danio marca, para próximas habilidades de daño del explorador
       {
         bonusAtaque += 4;
         criticoRango += 1;
         danioMarca += 15; //Esto se suma al porcentaje de daño solamente al ser golpe critico, ver mas abajo. Ya que esta amrca agrega % daño crítico.

         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 1)
         {  danioMarca += 5;   }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 2)
         {  criticoRango += 1;  }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL == 4)
         {  bonusAtaque -= 2;  } //NV 4 Quita el debuff al marcar, entonces se resta los 2 que se ponia como compensacion

         
        
       }
       //----

       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0); 
            
     
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
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
        await Task.Delay(200);

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

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.7f, 4.9f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await Task.Delay(150);
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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance,hAncho);
    
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


