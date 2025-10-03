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

      bonusAtaque = -2;
      if(NIVEL > 1){bonusAtaque++;}
      if(NIVEL == 5){bonusAtaque++;}
     
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 1; //Perforante
      criticoRangoHab = 0;

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.

      







      imHab = Resources.Load<Sprite>("imHab/Explorador_Rafaga");
     
       
    }

   
     public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Ráfaga I</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador ataca repetidamente con su arco al enemigo, y si muere, busca otro enemigo al azar.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Ataca hasta quedarse sin AP o sin flechas.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 1 por tiro \n-Enfriamiento: {cooldownMax} \n- Costo AP: 1 por tiro Esforzable \n- Costo Val: {costoPM} </color>\n\n";

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
      if(NIVEL== 2)
      {
        txtDescripcion = "<color=#5dade2><b>Ráfaga II</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador ataca repetidamente con su arco al enemigo, y si muere, busca otro enemigo al azar.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Ataca hasta quedarse sin AP o sin flechas.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque+1}</color> - Daño: Perforante 1d10- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 1 por tiro \n-Enfriamiento: {cooldownMax} \n- Costo AP: 1 por tiro \n- Costo Val: {costoPM} </color>\n\n";

    
       if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Cooldown</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
        txtDescripcion = "<color=#5dade2><b>Ráfaga III</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador ataca repetidamente con su arco al enemigo, y si muere, busca otro enemigo al azar.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Ataca hasta quedarse sin AP o sin flechas.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque+1}</color> - Daño: Perforante 1d10- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 1 por tiro \n-Enfriamiento: {cooldownMax-1} \n- Costo AP: 1 por tiro \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: -1 costo Valentía</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +2 Ataque</color>\n";
          }
          }
        }

      }
      if(NIVEL== 4)
      {
        txtDescripcion = "<color=#5dade2><b>Ráfaga IV a</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador ataca repetidamente con su arco al enemigo, y si muere, busca otro enemigo al azar.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Ataca hasta quedarse sin AP o sin flechas.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque+1}</color> - Daño: Perforante 1d10- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 1 por tiro \n-Enfriamiento: {cooldownMax-1} \n- Costo AP: 1 por tiro \n- Costo Val: {costoPM-1} </color>\n\n";
     }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Ráfaga IV b</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador ataca repetidamente con su arco al enemigo, y si muere, busca otro enemigo al azar.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Ataca hasta quedarse sin AP o sin flechas.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque+3}</color> - Daño: Perforante 1d10- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 1 por tiro \n-Enfriamiento: {cooldownMax-1} \n- Costo AP: 1 por tiro \n- Costo Val: {costoPM} </color>\n\n";
      }



    }


    Casilla Origen;
    public override void Activar()
    {
        if(Usuario.GetComponent<ClaseExplorador>().Cantidad_flechas > 0)
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
      while(scEstaUnidad.ObtenerAPActual() > 0 && scEstaUnidadExp.Cantidad_flechas > 0)
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
       
      Usuario.GetComponent<ClaseExplorador>().Cantidad_flechas--;
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
