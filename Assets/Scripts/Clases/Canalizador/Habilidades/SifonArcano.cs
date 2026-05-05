using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SifonArcano : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Sifón Arcano";
      IDenClase = 7;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_SifonArcano");
      ActualizarDescripcion();
     
    }
  public override void ActualizarDescripcion()
  {
    var statsUI = ObtenerStatsDescripcionUI();
    int poderActual = statsUI.Poder;
    int bonusDCNivel = NIVEL > 2 ? 1 : 0;
    int dcBase = 10 + bonusDCNivel;
    int duracionTurnos = NIVEL == 5 ? 4 : 3;
    int bonusDanioBase = NIVEL > 1 ? 5 : 3;
    string rangoDanioEs = FormatearRangoDados(1, 10, bonusDanioBase);
    string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcBase, "Pod", "Power", poderActual, "Poder");
    string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcBase, "Pod", "Power", poderActual, "Poder");

    string tituloEs = "Sifon Arcano I";
    string tituloEn = "Arcane Siphon I";
    if (NIVEL == 2) { tituloEs = "Sifon Arcano II"; tituloEn = "Arcane Siphon II"; }
    if (NIVEL == 3) { tituloEs = "Sifon Arcano III"; tituloEn = "Arcane Siphon III"; }
    if (NIVEL == 4) { tituloEs = "Sifon Arcano IV a"; tituloEn = "Arcane Siphon IV a"; }
    if (NIVEL == 5) { tituloEs = "Sifon Arcano IV b"; tituloEn = "Arcane Siphon IV b"; }

    string lineaDanioEs = bonusDanioBase > 0
      ? $"<b>Danio por turno:</b> ({rangoDanioEs}) x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano"
      : "<b>Danio por turno:</b> 1-10 x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano";
    string lineaDanioEn = bonusDanioBase > 0
      ? $"<b>Turn Damage:</b> (1d10 + {bonusDanioBase}) x (1 + Energy Residues) | <b>Type:</b> Arcane"
      : "<b>Turn Damage:</b> 1d10 x (1 + Energy Residues) | <b>Type:</b> Arcane";

    bool esInglesFormato = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortuguesFormato = TRADU.i != null && TRADU.i.nIdioma == 3;
    string tituloPtFormato = "Sifao Arcano I";
    if (NIVEL == 2) { tituloPtFormato = "Sifao Arcano II"; }
    if (NIVEL == 3) { tituloPtFormato = "Sifao Arcano III"; }
    if (NIVEL == 4) { tituloPtFormato = "Sifao Arcano IV a"; }
    if (NIVEL == 5) { tituloPtFormato = "Sifao Arcano IV b"; }
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string colorPoder = "#2aa6c8";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
    string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string tituloFormato = esInglesFormato ? tituloEn : esPortuguesFormato ? tituloPtFormato : tituloEs;
    string subtituloFormato = esInglesFormato
      ? "Applies a turn-damage siphon; damage scales with Energy Residues."
      : esPortuguesFormato
        ? "Aplica dano por turno; o dano escala com Residuos Energeticos."
        : "Aplica daño por turno; el daño escala con Residuos Energeticos.";
    string cuerpoFormato = "";
    if (esInglesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged debuff (5 range)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy on the opposite side</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Fortitude vs DC {dcBase} + <color={colorPoder}>Power ({poderActual})</color></color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On failed save:</b></color> <color={colorValor}>{iconoDebuff} Arcane Siphon for {duracionTurnos} turns</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Turn damage:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Energy Residues). Type: Arcane</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On kill:</b></color> <color={colorValor}>{iconoBuff} +1 permanent Max AP, +10% Damage, {iconoEnergia} +1 Energy</color>";
    }
    else if (esPortuguesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Debuff a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo no lado oposto</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Resistência:</b></color> <color={colorValor}>Fortitude vs CD {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoDebuff} Sifao Arcano por {duracionTurnos} turnos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Dano por turno:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Residuos Energeticos). Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao matar:</b></color> <color={colorValor}>{iconoBuff} +1 AP Max permanente, +10% Dano, {iconoEnergia} +1 Energia</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Debuff a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo del lado opuesto</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Fortaleza vs DC {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoDebuff} Sifon Arcano por {duracionTurnos} turnos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño por turno:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Residuos Energeticos). Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al matar:</b></color> <color={colorValor}>{iconoBuff} +1 AP Max permanente, +10% Daño, {iconoEnergia} +1 Energia</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{tituloFormato}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
    {
      if (esInglesFormato)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base turn damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A or B (IV upgrades).</color>"; }
      }
      else if (esPortuguesFormato)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no dano base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 CD.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A ou Opcao B (melhorias IV).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al danio base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A u Opcion B (mejoras IV).</color>"; }
      }
    }
    if (!string.IsNullOrEmpty(txtDescripcion))
    {
      return;
    }

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string cuerpo = "";
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy unit on the opposite side\n";
      cuerpo += lineaSalvacionEn + "\n";
      cuerpo += $"<b>On failed save:</b> Applies Arcane Siphon for {duracionTurnos} turns\n";
      cuerpo += lineaDanioEn + "\n";
      cuerpo += "<b>On kill by this effect:</b> +1 permanent AP max, +10% Damage and +1 Energy";

      string costos = $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEn,
        "Marks the target with an unstable link that drains vitality over time, amplified by Energy Residues.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base turn damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A or B (IV upgrades).</color>"; }
      }
      return;
    }
    if (TRADU.i != null && TRADU.i.nIdioma == 3)
    {
      string corpo = "";
      corpo += "<b>Tipo:</b> Distancia (5 alcance)\n";
      corpo += "<b>Alvo:</b> 1 unidade inimiga do lado oposto\n";
      corpo += $"{ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcBase, "Poder", "Power", poderActual)}\n";
      corpo += $"<b>Se falhar TS:</b> aplica Sifao Arcano por {duracionTurnos} turnos\n";
      corpo += (bonusDanioBase > 0
        ? $"<b>Dano por turno:</b> (1d10 + {bonusDanioBase}) x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano"
        : "<b>Dano por turno:</b> 1d10 x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano");
      corpo += "\n";
      corpo += "<b>Se matar com este efeito:</b> +1 AP max permanente, +10% Dano e +1 Energia";

      string costos = $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        "Sifao Arcano",
        "Marca o alvo com um vinculo instavel que drena vitalidade por turnos, amplificado por Residuos Energeticos.",
        corpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no dano base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 CD.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A ou Opcao B (melhorias IV).</color>"; }
      }
      return;
    }

    {
      string cuerpo = "";
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 unidad enemiga del lado opuesto\n";
      cuerpo += lineaSalvacionEs + "\n";
      cuerpo += $"<b>Si falla TS:</b> aplica Sifon Arcano por {duracionTurnos} turnos\n";
      cuerpo += lineaDanioEs + "\n";
      cuerpo += "<b>Si mata con este efecto:</b> +1 AP max permanente, +10% Danio y +1 Energia";

      string costos = $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEs,
        "Marca al objetivo con un vinculo inestable que drena vitalidad por turnos, amplificado por Residuos Energeticos.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al danio base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A u Opcion B (mejoras IV).</color>"; }
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if (obj is Unidad uni) //Acá van los efectos a Unidades.
    {
      float dc = 8 + scEstaUnidad.mod_CarPoder; 
      VFXAplicar(uni.gameObject);
      if (NIVEL > 2) { dc += 1; }

      if (uni.TiradaSalvacion(uni.mod_TSFortaleza, dc))
      {

        //Agrega la reacción 
        ReaccionSifonArcano reaccion = new ReaccionSifonArcano();
        reaccion.variableUnidad = scEstaUnidad;
        reaccion.NIVEL = NIVEL;
        reaccion.nombre = "Sifón Arcano";
        reaccion.variableUnidad = scEstaUnidad;
        ReaccionSifonArcano reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, uni.gameObject);


      }
                        
      }
     
    }

  protected override float? CalcularProbabilidadEspecialSobreObjetivo(Unidad objetivo)
  {
    if (objetivo == null || scEstaUnidad == null)
    {
      return null;
    }

    float dc = 8 + scEstaUnidad.mod_CarPoder;
    if (NIVEL > 2)
    {
      dc += 1;
    }

    return CalcularProbabilidadFallarTS(objetivo.mod_TSFortaleza, dc);
  }

  protected override string ObtenerTextoProbabilidadSobreObjetivo(Unidad objetivo, float probabilidad)
  {
    return FormatearTextoProbabilidadExito(probabilidad);
  }
    
       void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SifonArcano");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

  }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
     
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
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

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          c.ActivarCapaColorRojo();
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

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}




