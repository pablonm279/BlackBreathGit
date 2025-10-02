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

      targetEspecial = 1; //Misma fila

      bonusAtaque = -1;
      if(NIVEL > 2){bonusAtaque += 1;}
      if(NIVEL == 5){bonusAtaque += 2;}
      XdDanio = 1;
      daniodX = 10; //1d10+3
      tipoDanio = 1; //Cortante
      criticoRangoHab = 0;

      imHab = Resources.Load<Sprite>("imHab/Explorador_TiroPotente");
      

      requiereRecurso = 2; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
      
    }

    public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Tiro Potente I</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando toda la fuerza de su arco, el Explorador dispara una flecha que atraviesa enemigos en la misma fila.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Afecta a toda la fila.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10+3- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 2 -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

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
        txtDescripcion = "<color=#5dade2><b>Tiro Potente II</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando toda la fuerza de su arco, el Explorador dispara una flecha que atraviesa enemigos en la misma fila.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Afecta a toda la fila.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10+5- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 2 - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    
    
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
        txtDescripcion = "<color=#5dade2><b>Tiro Potente III</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando toda la fuerza de su arco, el Explorador dispara una flecha que atraviesa enemigos en la misma fila.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Afecta a toda la fila.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10+5- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 2 - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

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
        txtDescripcion = "<color=#5dade2><b>Tiro Potente IV a</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando toda la fuerza de su arco, el Explorador dispara una flecha que atraviesa enemigos en la misma fila.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Afecta a toda la fila.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10+5- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 2 - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
     }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Tiro Potente IV b</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando toda la fuerza de su arco, el Explorador dispara una flecha que atraviesa enemigos en la misma fila.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Afecta a toda la fila.</b> -Ataque: <color=#ea0606>Agilidad {bonusAtaque}</color> - Daño: Perforante 1d10+5- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Flechas: 2 - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
      }



    }

    Casilla Origen;
    public override void Activar()
    { 
        seTiroFlechaVFX = false;
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
        Usuario.GetComponent<ClaseExplorador>().Cantidad_flechas -= 2;
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    bool seTiroFlechaVFX = false;
    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;
       if(seTiroFlechaVFX == false)
       {
         seTiroFlechaVFX = true;
         // Dispara un proyectil que recorra toda la fila (3 casillas) y se pierda más allá
         // usando la casilla clickeada como referencia de dirección.
         CrearProyectilFila(objetivo.CasillaPosicion);
       }
       
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
  
    async Task CrearProyectil(object Objetivo)
   {
      await Task.Delay(80);
      GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.Flecha;
      GameObject Proyectil = Instantiate(flechaPrefab);
      Proyectil.GetComponent<ArrowFlight>().startMarker = transform;
      Proyectil.GetComponent<ArrowFlight>().parabola = 0.12f;
      Proyectil.GetComponent<ArrowFlight>().velocidad = 9.2f;
    
    
      if(Objetivo != null)
      {
      
        if(Objetivo is Unidad)
        { 
          Unidad obj = (Unidad)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
        }
        else if(Objetivo is Obstaculo)
        {
          Obstaculo obj = (Obstaculo)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
        }
         else if(Objetivo is Casilla)
        {
         Casilla obj = (Casilla)Objetivo;
         Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
        }
     }
     
   }

    async Task CrearProyectilFila(Casilla casillaClick)
    {
      await Task.Delay(10);
      // Spawn simple: flecha nace detrás de X=3 de la fila enemiga (misma Y) y no se mueve aquí
      int filaY = (casillaClick != null) ? casillaClick.posY : Origen.posY;
      List<Casilla> filaFull = new List<Casilla>();
      foreach (var c in BattleManager.Instance.lCasillasTotal)
      {
        if (c.lado != Origen.lado && c.posY == filaY)
        {
          filaFull.Add(c);
        }
      }
      if (filaFull.Count == 0) { return; }
      Casilla startCas = null;
      foreach (var c in filaFull) { if (c.posX == 3) { startCas = c; break; } }
      if (startCas == null)
      {
        foreach (var c in filaFull) { if (startCas == null || c.posX > startCas.posX) startCas = c; }
      }
      Casilla endCas = startCas;
      foreach (var c in filaFull) { if (c.posX < endCas.posX) endCas = c; }
      Vector3 dir = (endCas.transform.position - startCas.transform.position).normalized;
      float offsetBehind = 2.2f;
      Vector3 spawnPos = startCas.transform.position - dir * offsetBehind;
      GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.FlechaPotente;
      Instantiate(flechaPrefab, spawnPos, Quaternion.identity);
 

     
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
