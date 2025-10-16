using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDesintegradora : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga Desintegradora";
      IDenClase = 9;
      costoAP = 6;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = true;

      targetEspecial = 9; //Piramide

      bonusAtaque = 5;
      XdDanio = 3;
      daniodX = 12; //3d12
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDesintegradora");
      

      requiereRecurso = 2; //Requiere tener 2 Tier energía 

    }

  public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#e67e22><b>Descarga Desintegradora I</b></color>\n\n";
      txtDescripcion += "<i>Sólo al alcanzar su máxima Energía, el Canalizador libera una devastadora explosión de pura desintegración arcana.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8>- Costo: 6 AP | 1 Valentía | Requiere Energía 2+</color>\n";
      txtDescripcion += "<color=#c8c8c8>- Alcance: en <b>Pirámide</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- En area | Daño: 3d12 + Poder | Dado Crítico: 2</color>\n";
      txtDescripcion += "<color=#c8c8c8>- TS Fortaleza 9: <b>Desintegra</b> (mata al objetivo)</color>\n";
      txtDescripcion += "<color=#c8c8c8>- <b>Reduce 1 Nivel de Energía</b> y el Canalizador queda <b>Aturdido 1 turno</b>.</color>\n\n";
      if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
        txtDescripcion += "<color=#dfea02>- Próximo Nivel: +5 Daño.</color>\n\n";
    }

    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#e67e22><b>Descarga Desintegradora II</b></color>\n\n";
      txtDescripcion += "<i>Sólo al alcanzar su máxima Energía, el Canalizador libera una devastadora explosión de pura desintegración arcana.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8>- Costo: 6 AP | 1 Valentía | Requiere Energía 2+</color>\n";
      txtDescripcion += "<color=#c8c8c8>- Alcance: en <b>Pirámide</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- En area | Daño: 3d12 + 5 + Poder | Dado Crítico: 2</color>\n";
      txtDescripcion += "<color=#c8c8c8>- TS Fortaleza 9: <b>Desintegra</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- <b>Reduce 1 Nivel de Energía</b> y el Canalizador queda <b>Aturdido 1 turno</b>.</color>\n\n";
      if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
        txtDescripcion += "<color=#dfea02>- Próximo Nivel: +1 DC.</color>\n\n";
    }

    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#e67e22><b>Descarga Desintegradora III</b></color>\n\n";
      txtDescripcion += "<i>Sólo al alcanzar su máxima Energía, el Canalizador libera una devastadora explosión de pura desintegración arcana.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8>- Costo: 6 AP | 1 Valentía | Requiere Energía 2+</color>\n";
      txtDescripcion += "<color=#c8c8c8>- Alcance: en <b>Pirámide</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- En area | Daño: 3d12 + 5 + Poder | Dado Crítico: 2</color>\n";
      txtDescripcion += "<color=#c8c8c8>- TS Fortaleza 10: <b>Desintegra</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- <b>Reduce 1 Energía</b> y el Canalizador queda <b>Aturdido 1 turno</b>.</color>\n\n";
      if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
        txtDescripcion += "<color=#dfea02>- Próximo Nivel:\nOpción A: No aturde.\nOpción B: No reduce Energía.</color>\n\n";
    }

    if (NIVEL == 4)
    {
      // Variante A: No aturde
      txtDescripcion = "<color=#e67e22><b>Descarga Desintegradora IV (A)</b></color>\n\n";
      txtDescripcion += "<i>Sólo al alcanzar su máxima Energía, el Canalizador libera una devastadora explosión de pura desintegración arcana.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8>- Costo: 6 AP | 1 Valentía | Requiere Energía 2+</color>\n";
      txtDescripcion += "<color=#c8c8c8>- Alcance: en <b>Pirámide</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- En area | Daño: 3d12 + 5 + Poder | Dado Crítico: 2</color>\n";
      txtDescripcion += "<color=#c8c8c8>- TS Fortaleza 10: <b>Desintegra</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- <b>Reduce 1 el Nivel de Energía</b>.</color>\n\n";
    }

    if (NIVEL == 5)
    {
      // Variante B: No reduce Energía
      txtDescripcion += "<color=#e67e22><b>Descarga Desintegradora IV (B)</b></color>\n\n";
      txtDescripcion += "<i>Sólo al alcanzar su máxima Energía, el Canalizador libera una devastadora explosión de pura desintegración arcana.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8>- Costo: 6 AP | 1 Valentía | Requiere Energía 2+</color>\n";
      txtDescripcion += "<color=#c8c8c8>- Alcance: en <b>Pirámide</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- En area | Daño: 3d12 + 5 + Poder | Dado Crítico: 2</color>\n";
      txtDescripcion += "<color=#c8c8c8>- TS Fortaleza 10: <b>Desintegra</b></color>\n";
      txtDescripcion += "<color=#c8c8c8>- <b>El canalizador queda <b>Aturdido 1 turno</b>.</color>\n\n";
    }
    
    if (TRADU.i.nIdioma == 2)  // English translation
    {
      if (NIVEL < 2)
      {
        txtDescripcion = "<color=#e67e22><b>Disintegrating Discharge I</b></color>\n\n";
        txtDescripcion += "<i>Only upon reaching maximum Energy, the Channeler unleashes a devastating explosion of pure arcane disintegration.</i>\n\n";
        txtDescripcion += "<color=#c8c8c8>- Cost: 6 AP | 1 Valor | Requires Energy 2+</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Range: in <b>Pyramid</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- Area | Damage: 3d12 + Power | Critical Die: 2</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Fortitude Save 9: <b>Disintegrates</b> (kills the target)</color>\n";
        txtDescripcion += "<color=#c8c8c8>- <b>Reduces 1 Energy Level</b> and the Channeler is <b>Stunned for 1 turn</b>.</color>\n\n";
        if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
          txtDescripcion += "<color=#dfea02>- Next Level: +5 Damage.</color>\n\n";
      }

      if (NIVEL == 2)
      {
        txtDescripcion = "<color=#e67e22><b>Disintegrating Discharge II</b></color>\n\n";
        txtDescripcion += "<i>Only upon reaching maximum Energy, the Channeler unleashes a devastating explosion of pure arcane disintegration.</i>\n\n";
        txtDescripcion += "<color=#c8c8c8>- Cost: 6 AP | 1 Valor | Requires Energy 2+</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Range: in <b>Pyramid</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- Area | Damage: 3d12 + 5 + Power | Critical Die: 2</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Fortitude Save 9: <b>Disintegrates</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- <b>Reduces 1 Energy Level</b> and the Channeler is <b>Stunned for 1 turn</b>.</color>\n\n";
        if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
          txtDescripcion += "<color=#dfea02>- Next Level: +1 DC.</color>\n\n";
      }

      if (NIVEL == 3)
      {
        txtDescripcion = "<color=#e67e22><b>Disintegrating Discharge III</b></color>\n\n";
        txtDescripcion += "<i>Only upon reaching maximum Energy, the Channeler unleashes a devastating explosion of pure arcane disintegration.</i>\n\n";
        txtDescripcion += "<color=#c8c8c8>- Cost: 6 AP | 1 Valor | Requires Energy 2+</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Range: in <b>Pyramid</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- Area | Damage: 3d12 + 5 + Power | Critical Die: 2</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Fortitude Save 10: <b>Disintegrates</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- <b>Reduces 1 Energy</b> and the Channeler is <b>Stunned for 1 turn</b>.</color>\n\n";
        if (EsEscenaCampaña() && CampaignManager.Instance.scMenuPersonajes.pSel?.NivelPuntoHabilidad > 0)
          txtDescripcion += "<color=#dfea02>- Next Level:\nOption A: Not stunned.\nOption B: Does not reduce Energy.</color>\n\n";
      }

      if (NIVEL == 4)
      {
        // Variant A: Not stunned
        txtDescripcion = "<color=#e67e22><b>Disintegrating Discharge IV (A)</b></color>\n\n";
        txtDescripcion += "<i>Only upon reaching maximum Energy, the Channeler unleashes a devastating explosion of pure arcane disintegration.</i>\n\n";
        txtDescripcion += "<color=#c8c8c8>- Cost: 6 AP | 1 Valor | Requires Energy 2+</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Range: in <b>Pyramid</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- Area | Damage: 3d12 + 5 + Power | Critical Die: 2</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Fortitude Save 10: <b>Disintegrates</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- <b>Reduces 1 Energy Level</b>.</color>\n\n";
      }

      if (NIVEL == 5)
      {
        // Variant B: Does not reduce Energy
        txtDescripcion += "<color=#e67e22><b>Disintegrating Discharge IV (B)</b></color>\n\n";
        txtDescripcion += "<i>Only upon reaching maximum Energy, the Channeler unleashes a devastating explosion of pure arcane disintegration.</i>\n\n";
        txtDescripcion += "<color=#c8c8c8>- Cost: 6 AP | 1 Valor | Requires Energy 2+</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Range: in <b>Pyramid</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- Area | Damage: 3d12 + 5 + Power | Critical Die: 2</color>\n";
        txtDescripcion += "<color=#c8c8c8>- Fortitude Save 10: <b>Disintegrates</b></color>\n";
        txtDescripcion += "<color=#c8c8c8>- <b>The channeler is <b>Stunned for 1 turn</b>.</color>\n\n";
      }
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
  await  base.Resolver(Objetivos);

    if (NIVEL != 4) { scEstaUnidad.estado_aturdido+=1; print(6565); }
    if(scEstaUnidad is ClaseCanalizador can){ if (NIVEL != 5) { can.CambiarEnergia(-1); } }
  }
    
    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.7f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseHabilidad;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay) * 1000f);
        return Task.Delay(ms);
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

      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
        scEstaUnidad.EstablecerAPActualA(0);
       VFXAplicar(objetivo.gameObject);
       }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
       VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioExtra);
        if (NIVEL > 1) { danio += 5; }

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


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    void EfectoAdicional(Unidad Objetivo)
    {
        int dc = 9;
        if (NIVEL > 2) { dc += 1; }
    if (Objetivo.TiradaSalvacion(Objetivo.mod_TSFortaleza, dc))
    {
      Objetivo.RecibirDanio(Objetivo.mod_maxHP, 10, false, scEstaUnidad);
      BattleManager.Instance.EscribirLog($"{Objetivo.uNombre} fue Desintegrado.");
    }
       
    }
  
    
    void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_DescargaDesintegradora");

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


