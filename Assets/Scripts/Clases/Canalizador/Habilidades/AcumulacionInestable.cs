using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class AcumulacionInestable : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
    public override void  Awake()
    {
      nombre = "Acumulación Inestable";
      IDenClase = 4; 
      costoAP = 1;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      if (NIVEL > 1 ) { cooldownMax--; }
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumulacionInestable");


    Invoke("PonerCD", 1.2f);
      
    
    }

  void PonerCD()
  { 
     if (NIVEL != 4 ) { cooldownActual = cooldownMax; }

  }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    string tituloEs = "Acumulacion Inestable I";
    string tituloEn = "Unstable Gathering I";
    string tituloPt = "Acumulacao Instavel I";
    if (NIVEL == 2) { tituloEs = "Acumulacion Inestable II"; tituloEn = "Unstable Gathering II"; }
    if (NIVEL == 3) { tituloEs = "Acumulacion Inestable III"; tituloEn = "Unstable Gathering III"; }
    if (NIVEL == 4) { tituloEs = "Acumulacion Inestable IV a"; tituloEn = "Unstable Gathering IV a"; }
    if (NIVEL == 5) { tituloEs = "Acumulacion Inestable IV b"; tituloEn = "Unstable Gathering IV b"; }
    if (NIVEL == 2) { tituloPt = "Acumulacao Instavel II"; }
    if (NIVEL == 3) { tituloPt = "Acumulacao Instavel III"; }
    if (NIVEL == 4) { tituloPt = "Acumulacao Instavel IV a"; }
    if (NIVEL == 5) { tituloPt = "Acumulacao Instavel IV b"; }

    int bonusDanioArcano = NIVEL > 2 ? 7 : 5;
    bool arrancaEnCooldown = NIVEL != 4;
    bool recibeDanioPropio = NIVEL != 5;

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Self\n";
      cuerpo += "<b>Target:</b> Self\n";
      cuerpo += "<b>Instant effect:</b> +1 Energy Tier\n";
      cuerpo += $"<b>Buff (this turn):</b> +{bonusDanioArcano} Arcane damage\n";
      cuerpo += recibeDanioPropio
        ? "<b>Backlash:</b> Takes 1d6 Arcane damage on cast"
        : "<b>Backlash:</b> No self damage on cast";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Propria\n";
      cuerpo += "<b>Alvo:</b> O proprio usuario\n";
      cuerpo += "<b>Efeito instantaneo:</b> +1 Nivel de Energia\n";
      cuerpo += $"<b>Buff (neste turno):</b> +{bonusDanioArcano} de dano Arcano\n";
      cuerpo += recibeDanioPropio
        ? "<b>Contragolpe:</b> Recebe 1d6 de dano Arcano ao usar"
        : "<b>Contragolpe:</b> Nao recebe dano ao usar";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Propia\n";
      cuerpo += "<b>Objetivo:</b> Propio usuario\n";
      cuerpo += "<b>Efecto instantaneo:</b> +1 Nivel de Energia\n";
      cuerpo += $"<b>Buff (este turno):</b> +{bonusDanioArcano} de daño Arcano\n";
      cuerpo += recibeDanioPropio
        ? "<b>Contragolpe:</b> Recibe 1-6 de daño Arcano al usarla"
        : "<b>Contragolpe:</b> No recibe daño al usarla";
    }

    string notaInicioCooldown = esIngles
      ? (arrancaEnCooldown ? "Starts on cooldown." : "Does not start on cooldown.")
      : esPortugues
        ? (arrancaEnCooldown ? "Comeca em recarga." : "Nao comeca em recarga.")
      : (arrancaEnCooldown ? "Arranca en enfriamiento." : "No arranca en enfriamiento.");

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- {notaInicioCooldown}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- {notaInicioCooldown}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- {notaInicioCooldown}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "The Channeler overloads their core to gain immediate power at a personal cost."
        : esPortugues
          ? "O Canalizador sobrecarrega o nucleo para ganhar poder imediato com custo pessoal."
        : "El Canalizador sobrecarga su nucleo para ganar poder inmediato a costa de su propio cuerpo.",
      cuerpo,
      costos,
      "#5dade2");

    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtituloFormato = esIngles
      ? "Gain +1 Energy now and Arcane bonus damage this turn."
      : esPortugues
        ? "Ganha +1 Energia agora e dano Arcano bonus neste turno."
        : "Gana +1 Energía ahora y daño Arcano bonus este turno.";
    string cuerpoFormato = "";
    if (esIngles)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Self buff</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Self</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{iconoEnergia} +1 Energy Tier</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>This turn:</b></color> <color={colorValor}>{iconoBuff} +{bonusDanioArcano} Arcane damage</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Backlash:</b></color> <color={colorValor}>{(recibeDanioPropio ? "Takes 1-6 Arcane damage" : "No self damage")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Start cooldown:</b></color> <color={colorValor}>{(arrancaEnCooldown ? "Yes" : "No")}</color>";
    }
    else if (esPortugues)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>O proprio usuario</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao usar:</b></color> <color={colorValor}>{iconoEnergia} +1 Nivel de Energia</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Neste turno:</b></color> <color={colorValor}>{iconoBuff} +{bonusDanioArcano} dano Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>{(recibeDanioPropio ? "Recebe 1-6 dano Arcano" : "Sem autodano")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Recarga inicial:</b></color> <color={colorValor}>{(arrancaEnCooldown ? "Sim" : "Nao")}</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Propio usuario</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al usar:</b></color> <color={colorValor}>{iconoEnergia} +1 Nivel de Energia</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Este turno:</b></color> <color={colorValor}>{iconoBuff} +{bonusDanioArcano} daño Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>{(recibeDanioPropio ? "Recibe 1-6 daño Arcano" : "Sin autodaño")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Enfriamiento inicial:</b></color> <color={colorValor}>{(arrancaEnCooldown ? "Si" : "No")}</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 Arcane bonus damage.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no starting cooldown) or Option B (no self damage).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de bonus de dano Arcano.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao comeca em recarga) ou Opcao B (sem autodano).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de bonus de daño Arcano.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (no arranca en enfriamiento) u Opción B (sin autodaño).</color>"; }
    }
  }

   
    public override Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        return base.Resolver(Objetivos, cas);
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

    if (obj is ClaseCanalizador scCana) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;
      VFXAplicar(objetivo.gameObject);
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Acumulacion Inestable";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 1;
      buff.cantDamBonusElementalArc = 5;
      if (NIVEL > 2 ) { buff.cantDamBonusElementalArc += 2; }
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
      objetivo.Marcar(0);


      scCana.CambiarEnergia(1);

      int rand = UnityEngine.Random.Range(1,6);
      if (NIVEL != 5 ) { scCana.RecibirDanio(rand, 8, false, null);  }
 

      }
    }
    
         void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AcumulacionInestable");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  
   AcumularEnergiaCanalizadorFx.CrearInestable(objetivo.GetComponent<Unidad>());

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






