using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class AcumularEnergia : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acumular Energía";
      IDenClase = 0; // Intrínseca
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumularEnergia");

       
      ActualizarDescripcion();
    
    }

        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();
      int poderActual = statsUI.Poder;
      ClaseCanalizador canalizador = scEstaUnidad as ClaseCanalizador;
      int nivelAcumulacionProtegida = canalizador != null ? canalizador.PASIVA_AcumulacionProtegida : 0;
      int energiaActual = canalizador != null ? canalizador.ObtenerEnergia() : 0;
      int barreraProtegida = 1 + poderActual + 3 * energiaActual;
      if (nivelAcumulacionProtegida > 1) { barreraProtegida += 2; }
      if (nivelAcumulacionProtegida == 4) { barreraProtegida += 4; }
      int tsMentalProtegida = nivelAcumulacionProtegida > 2 ? 2 : 1;
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}";
      string dcConcentracion = "10 + damage / 3";
      string dcConcentracionPt = "10 + dano / 3";
      string dcConcentracionEs = "10 + daño / 3";

      string titulo = esIngles ? "Gather Energy" : esPortugues ? "Acumular Energia" : "Acumular Energia";
      string subtitulo = esIngles
        ? "The Channeler enters concentration to increase their Energy tier at the start of the next turn."
        : esPortugues
          ? "O Canalizador entra em concentracao para aumentar seu Nivel de Energia no inicio do proximo turno."
          : "El Canalizador entra en concentracion para aumentar su Nivel de Energia al inicio de su siguiente turno.";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Self\n";
        cuerpo += "<b>Target:</b> Self\n";
        cuerpo += "<b>Effect on cast:</b> Applies <b>Gathering</b> buff (2 rounds)\n";
        cuerpo += "<b>If concentration is maintained:</b> +1 Energy Tier on next turn\n";
        cuerpo += "<b>Energy I:</b> +10% Damage, +5% Critical\n";
        cuerpo += "<b>Energy II:</b> +15% Damage, +1 Max AP\n";
        cuerpo += "<b>Energy III:</b> +15% Damage, +1 Max AP, +5% Critical";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Propria\n";
        cuerpo += "<b>Alvo:</b> O proprio usuario\n";
        cuerpo += "<b>Efeito ao ativar:</b> Aplica buff <b>Acumulando</b> (2 rodadas)\n";
        cuerpo += "<b>Se mantiver a concentracao:</b> +1 Nivel de Energia no proximo turno\n";
        cuerpo += "<b>Energia I:</b> +10% Dano, +5% Critico\n";
        cuerpo += "<b>Energia II:</b> +15% Dano, +1 AP Maximo\n";
        cuerpo += "<b>Energia III:</b> +15% Dano, +1 AP Maximo, +5% Critico";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Propia\n";
        cuerpo += "<b>Objetivo:</b> Propio usuario\n";
        cuerpo += "<b>Efecto al activar:</b> Aplica buff <b>Acumulando</b> (2 rondas)\n";
        cuerpo += "<b>Si mantiene la concentracion:</b> +1 Nivel de Energia al siguiente turno\n";
        cuerpo += "<b>Energia I:</b> +10% Danio, +5% Critico\n";
        cuerpo += "<b>Energia II:</b> +15% Danio, +1 AP Maximo\n";
        cuerpo += "<b>Energia III:</b> +15% Danio, +1 AP Maximo, +5% Critico";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentia: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(titulo, subtitulo, cuerpo, costos, "#5dade2");
      string subtituloFormato = esIngles
        ? "Start gathering energy; if concentration holds, gain +1 Energy next turn."
        : esPortugues
          ? "Começa a acumular energia; se mantiver concentração, ganha +1 Energia no próximo turno."
          : "Empieza a acumular energía; si mantiene concentración, gana +1 Energía el próximo turno.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Self buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Self</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{iconoEnergia} Gathering for 2 rounds; ends turn</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>If maintained:</b></color> <color={colorValor}>{iconoEnergia} +1 Energy Tier next turn</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>When damaged:</b></color> <color={colorValor}>Mental Save vs DC {dcConcentracion}; on failed save loses Gathering</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy I:</b></color> <color={colorValor}>+10% Damage, +5% Critical</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy II:</b></color> <color={colorValor}>+15% Damage, +1 Max AP</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy III:</b></color> <color={colorValor}>+15% Damage, +1 Max AP, +5% Critical</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Protected Gathering:</b></color> <color={colorValor}>{barreraProtegida} Barrier, +{tsMentalProtegida} Mental Save{(nivelAcumulacionProtegida == 5 ? ", +1 Max AP" : "")}</color>";
        }
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>O próprio usuário</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao ativar:</b></color> <color={colorValor}>{iconoEnergia} Acumulando por 2 rodadas; termina o turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Se mantiver:</b></color> <color={colorValor}>{iconoEnergia} +1 Nível de Energia no próximo turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao receber dano:</b></color> <color={colorValor}>Resistência Mental vs CD {dcConcentracionPt}; se falhar, perde Acumulando</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia I:</b></color> <color={colorValor}>+10% Dano, +5% Crítico</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia II:</b></color> <color={colorValor}>+15% Dano, +1 AP Máximo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia III:</b></color> <color={colorValor}>+15% Dano, +1 AP Máximo, +5% Crítico</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Acumulação Protegida:</b></color> <color={colorValor}>{barreraProtegida} Barreira, +{tsMentalProtegida} Resistência Mental{(nivelAcumulacionProtegida == 5 ? ", +1 AP Max" : "")}</color>";
        }
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Propio usuario</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Al activar:</b></color> <color={colorValor}>{iconoEnergia} Acumulando por 2 rondas; termina el turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Si mantiene:</b></color> <color={colorValor}>{iconoEnergia} +1 Nivel de Energía el próximo turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Al recibir daño:</b></color> <color={colorValor}>TS Mental vs DC {dcConcentracionEs}; si falla, pierde Acumulando</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía I:</b></color> <color={colorValor}>+10% Daño, +5% Crítico</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía II:</b></color> <color={colorValor}>+15% Daño, +1 AP Máximo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía III:</b></color> <color={colorValor}>+15% Daño, +1 AP Máximo, +5% Crítico</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Acumulación Protegida:</b></color> <color={colorValor}>{barreraProtegida} Barrera, +{tsMentalProtegida} TS Mental{(nivelAcumulacionProtegida == 5 ? ", +1 AP Max" : "")}</color>";
        }
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoFormato;
    }

    public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
      await  base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

     ClaseCanalizador scClaseCana = (ClaseCanalizador)scEstaUnidad;
     int NivelAcumulacionProtegida = scClaseCana.PASIVA_AcumulacionProtegida;
    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acumulando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
      
       if (NivelAcumulacionProtegida > 0)
       {
      
        int factorBarrera = (int)(1 + scEstaUnidad.mod_CarPoder + 3 * scClaseCana.ObtenerEnergia());
        buff.cantBarrera += factorBarrera;
        if (NivelAcumulacionProtegida > 1) { buff.cantBarrera += 2; }
        if (NivelAcumulacionProtegida == 4) { buff.cantBarrera += 4; }
        if (NivelAcumulacionProtegida == 5) { buff.cantAPMax += 1; }


        buff.cantTsMental += 1;
        if (NivelAcumulacionProtegida > 2) {  buff.cantTsMental += 1; }

       }
       
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);
        // Mantener pose de habilidad mientras dura "Acumulando"
        var poseCtrl = objetivo.GetComponent<UnidadPoseController>();
        if (poseCtrl != null)
        {
            poseCtrl.EnterSkillPoseHold();
        }







      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();

       
      }
    }
    
         void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AcumularEnergia");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
        if(c.Presente == null)
        {
            continue;
        }
        
       
        if(c.Presente.GetComponent<Unidad>() == null)
        {
            continue;
        }
           
        if(c.Presente.GetComponent<Unidad>() != null)
        {
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }
 
}






