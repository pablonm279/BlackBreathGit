using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Collections.Generic;

public class UIEstadoCuadro : MonoBehaviour
{
  public int indexEstadoRepresentado;
  private Image Retrato;
  private static readonly Dictionary<string, Sprite> CacheSpritesFallback = new Dictionary<string, Sprite>();

  public GameObject goTooltip;
  public TextMeshProUGUI textTooltip;

  public TextMeshProUGUI textStacks;

  void Awake()
  {
    Retrato = gameObject.GetComponent<Image>();

  }

  private void AsegurarRetrato()
  {
    if (Retrato == null)
    {
      Retrato = gameObject.GetComponent<Image>();
    }
  }

  public Sprite imArdiendo;
  public Sprite imAturdido;
  public Sprite imAcido;
  public Sprite imCongelado;
  public Sprite ResRed;
  public Sprite ArmMod;
  public Sprite imSangrado;
  public Sprite imVeneno;
  public Sprite imApMod;
  public Sprite imBuff;
  public Sprite imDebuff;
  public Sprite imReaccion;
  public Sprite imMarca;
  public Sprite imRegvid;
  public Sprite imRegArm;
  public Sprite imEvasion;
  public Sprite imFlechas;
  public Sprite imBonusFuego;
  public Sprite imBonusArcano;
  public Sprite imBonusHielo;
  public Sprite imBonusRayo;
  public Sprite imBonusAcido;
  public Sprite imBonusNecro;
  public Sprite imBonusDivino;
  public Sprite imPurificadoraFervor;
  public Sprite imBarrera;
  public Sprite imResiduoTejido;
  public Sprite imEstaEscondido;
  public Sprite imCorrupto;
  public Sprite imVolador;
  public Sprite imCondena;
  public Sprite imEscudado;
  public Sprite imTierEnergia; //Canalizador
  public void RepresentarEstado(int index, int stacks, bool desdeBarraVida = false)
  {
    AsegurarRetrato();
    debarravida = desdeBarraVida;
    if (stacks == -1  || debarravida)
    {
      textStacks.text = "";
    }
    else
    {
      textStacks.text = "" + stacks;
    }



    //1 - Ardiendo
    //2 - Aturdido
    //3 - Acido
    //4 - Congelado
    //5 - Resistencias Reducidas
    //6 - Armadura Modificador
    //7 - AP Modificador
    //8 - Sangrado
    //9 - Veneno
    //10 - Regenera Vida
    //11 - Regenera Armadura
    //12 - Evasion
    //13 - Explorador Flechas
    //---Daño elemental
    //20 - Purificadora Fervor
    //21 - Bonus daño divino
    //22 - Barrera
    //23 - Residuo de Tejido
    //24 - Escondido Tier I
    //25 - Escondido Tier II
    //26 - Canalizador: Tier Energia
    //27 - Corrupto
    //28 - Volando
    //29 - Condena
    indexEstadoRepresentado = index;
    if (Retrato == null) { return; }
    if (textTooltip == null) { return; }
    switch (index)
    {
      case 1: Retrato.sprite = ResolverSprite(imArdiendo, "Imagenes/Estado_ardiendo"); textTooltip.text = TRADU.i.Traducir("Ardiendo: causa daño cada turno, se apaga con AP disponibles."); break;
      case 2: Retrato.sprite = ResolverSprite(imAturdido, "Imagenes/Estado_aturdido"); textTooltip.text = TRADU.i.Traducir("Aturdido: no puede actuar."); break;
      case 3: Retrato.sprite = ResolverSprite(imAcido, "Imagenes/Estado_acido"); textTooltip.text = TRADU.i.Traducir("Ácido: cada acumulación reduce en 1 la armadura."); break;
      case 4: Retrato.sprite = ResolverSprite(imCongelado, "Imagenes/Estado_congelado"); textTooltip.text = TRADU.i.Traducir("Congelado: reduce PA disponibles y aumenta armadura."); break;
      case 5: Retrato.sprite = ResolverSprite(ResRed, "Imagenes/Estado_resreducidas"); textTooltip.text = TRADU.i.Traducir("Resistencias Reducidas: reduce todas las resistencias 1 por acumulación."); break;
      case 6: Retrato.sprite = ResolverSprite(ArmMod, "Imagenes/Estado_armadurareducida"); textTooltip.text = TRADU.i.Traducir("Armadura Rota: reduce la armadura en 1 por acumulación."); break;
      // case 7: Retrato.sprite = imApMod;  break;
      case 8: Retrato.sprite = ResolverSprite(imSangrado, "Imagenes/Estado_sangrando"); textTooltip.text = TRADU.i.Traducir("Sangrado: cada acumulación resta 1 HP máxima por turno y previene 2 de curación."); break;
      case 9: Retrato.sprite = ResolverSprite(imVeneno, "Imagenes/Estado_veneno"); textTooltip.text = TRADU.i.Traducir("Veneno: provoca daño por turno, se debe hacer una tirada de salvación de Fortaleza cada turno para curarse, si falla se incrementa en 1."); break;
      case 10: Retrato.sprite = ResolverSprite(imRegvid, "Imagenes/Estado_regeneravida"); textTooltip.text = TRADU.i.Traducir("Regeneración: recupera vida cada turno."); break;
      case 11: Retrato.sprite = ResolverSprite(imRegArm, "Imagenes/Estado_regeneraAramdura"); textTooltip.text = TRADU.i.Traducir("Regeneración Armadura: recupera Armadura perdida cada turno."); break;
      case 12: Retrato.sprite = ResolverSprite(imEvasion, "Imagenes/Estado_EVASION"); textTooltip.text = TRADU.i.Traducir("Evasión: cada stack aumenta 1 la Defensa, se elimina al recibir daño."); break;
      case 13: Retrato.sprite = ResolverSprite(imFlechas, "Imagenes/Estado_flechas"); textTooltip.text = TRADU.i.Traducir("Flechas: Cantidad de flechas disponibles."); break;
      case 14: Retrato.sprite = ResolverSprite(imBonusAcido, "Imagenes/est_acido"); textTooltip.text = TRADU.i.Traducir("Bonus daño elemental Acido."); break;
      case 15: Retrato.sprite = ResolverSprite(imBonusArcano, null); textTooltip.text =  TRADU.i.Traducir("Bonus daño elemental Arcano."); break;
      case 16: Retrato.sprite = ResolverSprite(imBonusFuego, "Imagenes/est_fuego"); textTooltip.text = TRADU.i.Traducir("Bonus daño elemental Fuego."); break;
      case 17: Retrato.sprite = ResolverSprite(imBonusHielo, null); textTooltip.text =  TRADU.i.Traducir("Bonus daño elemental Hielo."); break;
      case 18: Retrato.sprite = ResolverSprite(imBonusNecro, null); textTooltip.text =  TRADU.i.Traducir("Bonus daño elemental Necro."); break;
      case 19: Retrato.sprite = ResolverSprite(imBonusRayo, "Imagenes/est_rayo"); textTooltip.text =  TRADU.i.Traducir("Bonus daño elemental Rayo."); break;
      case 20: Retrato.sprite = ResolverSprite(imPurificadoraFervor, "Imagenes/Estado_Fervor"); textTooltip.text = TRADU.i.Traducir("Fervor: Cantidad de Fervor que tiene la purificadora."); break;
      case 21: Retrato.sprite = ResolverSprite(imBonusDivino, null); textTooltip.text = TRADU.i.Traducir("Bonus daño elemental Divino."); break;
      case 22: Retrato.sprite = ResolverSprite(imBarrera, "Imagenes/Estado_Barrera"); textTooltip.text = TRADU.i.Traducir("Barrera: previene X cantidad de daño."); break;
      case 23: Retrato.sprite = ResolverSprite(imResiduoTejido, "Imagenes/Estado_residuocurativo"); textTooltip.text = TRADU.i.Traducir("Residuo de Tejido: se obtiene al recibir curación de origen mágico. Previene X puntos de curación."); break;
      case 24: Retrato.sprite = ResolverSprite(imEstaEscondido, "Imagenes/Estado_oculto"); textTooltip.text = TRADU.i.Traducir("Escondido I: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto se remueve al atacar o recibir daño."); break;
      case 25: Retrato.sprite = ResolverSprite(imEstaEscondido, "Imagenes/Estado_oculto"); textTooltip.text = TRADU.i.Traducir("Escondido II: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto no se remueve al recibir daño."); break;
      case 26: Retrato.sprite = ResolverSprite(imTierEnergia, "Imagenes/Estado_acumularenergia"); textTooltip.text = TRADU.i.Traducir("Energía: Nivel de Energía Acumulada por el Canalizador."); break;
      case 27: Retrato.sprite = ResolverSprite(imCorrupto, "Imagenes/Estado_Corrupto"); textTooltip.text = TRADU.i.Traducir("Corrupto: Recibe daño adicional de enemigos Corrompidos que además se curan al dañarlo. Si lo deja fuera de combate un enemigo corrompido, muere."); break;
      case 28: Retrato.sprite = ResolverSprite(imVolador, "Imagenes/estado_volando"); textTooltip.text = TRADU.i.Traducir("Volador: Esta unidad no puede ser alcanzada por ataques melee, puede perder el vuelo al ser dañado o fallar un ataque."); break;
      case 29: Retrato.sprite = ResolverSprite(imCondena, "Imagenes/Estado_condena"); textTooltip.text = TRADU.i.Traducir("Condena: En X cantidad de turnos recibirá daño verdadero igual al 10% de su vida máxima por turno con el efecto."); break;
      case 30: Retrato.sprite = ResolverSprite(imEscudado, "Imagenes/Estado_escudado"); textTooltip.text = TRADU.i.Traducir("Escudado: 10% chances por stack de evitar un ataque físico. Al evitar uno, pierde un stack."); break;

    }
  }

  private Sprite ResolverSprite(Sprite spriteSerializado, string rutaResources)
  {
    if (spriteSerializado != null)
    {
      return spriteSerializado;
    }

    if (string.IsNullOrEmpty(rutaResources))
    {
      return null;
    }

    if (!CacheSpritesFallback.TryGetValue(rutaResources, out Sprite spriteFallback) || spriteFallback == null)
    {
      spriteFallback = Resources.Load<Sprite>(rutaResources);
      CacheSpritesFallback[rutaResources] = spriteFallback;
    }

    return spriteFallback;
  }

  public TooltipBatalla scTooltipBatalla;
  void OnStart()
  {
    BuscartooltipBatallaTag();
  }

  void BuscartooltipBatallaTag()
  {
    if (TooltipBatalla.Instance != null)
    {
      scTooltipBatalla = TooltipBatalla.Instance;
      return;
    }

    GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
    foreach (var go in all)
    {
      if (go.CompareTag("TooltipBatalla"))
      {
        scTooltipBatalla = go.GetComponent<TooltipBatalla>();
        break;
      }
    }
  }
  public void RepresentarBuff(Buff buff, bool desdeBarraVida = false, int stackCount = 1)
  {
    AsegurarRetrato();
    debarravida = desdeBarraVida;
    if (textStacks != null)
    {
      textStacks.text = (!desdeBarraVida && stackCount > 1) ? $"x{stackCount}" : "";
    }

    textTooltip.text = GenerarDescripcionBuff(buff); //Efectos del buff
    if (Retrato != null)
    {
      Sprite sprite = null;
      if(buff.boolfDebufftBuff )
      {
        sprite = ResolverSprite(imBuff, "Imagenes/Estado_buff");
      }
       else
      {
        sprite = ResolverSprite(imDebuff, "Imagenes/Estado_debuff");
      }

     
        Retrato.sprite = sprite;
      
    }
  }

  public void RepresentarReaccion(Reaccion buff, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    textStacks.text = "" + buff.usos;

     if (desdeBarraVida) { textStacks.text = ""; }

    textTooltip.text = buff.descripcion; //Efectos del buff
    Sprite spriteReaccion = ResolverSprite(imReaccion, "Imagenes/Estado_reaccion");
    if (Retrato != null && spriteReaccion != null)
    {
      Retrato.sprite = spriteReaccion;
    }
  }

  public void RepresentarMarca(Marca buff, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    textStacks.text = "" + buff.duracion;
    

    if (desdeBarraVida) { textStacks.text = ""; }

    textTooltip.text = buff.descripcion; //Efectos del buff

     Sprite spriteMarca = ResolverSprite(imMarca, "Imagenes/Estado_Marcado");
     if (Retrato != null && spriteMarca != null)
    {
      Retrato.sprite = spriteMarca;
    }
  }

  private string GenerarDescripcionBuff(Buff buff)
  {
    string descripcion = "" + TRADU.i.Traducir(buff.buffNombre) + "\n" + TRADU.i.Traducir(buff.buffDescr) + "\n";

    if (buff.percHPMax != 0) descripcion += TRADU.i.Traducir("HP Máximo: ") + "<color=" + (buff.percHPMax > 0 ? "green" : "red") + ">" + buff.percHPMax + "%" + "</color>\n";
    if (buff.cantHPMax != 0) descripcion += TRADU.i.Traducir("HP Máximo: ") + "<color=" + (buff.cantHPMax > 0 ? "green" : "red") + ">" + buff.cantHPMax + "</color>\n";

    if (buff.percIniciativa != 0) descripcion += TRADU.i.Traducir("Iniciativa: ") + "<color=" + (buff.percIniciativa > 0 ? "green" : "red") + ">" + buff.percIniciativa + "%" + "</color>\n";
    if (buff.cantIniciativa != 0) descripcion += TRADU.i.Traducir("Iniciativa: ") + "<color=" + (buff.cantIniciativa > 0 ? "green" : "red") + ">" + buff.cantIniciativa + "</color>\n";

    if (buff.percAPMax != 0) descripcion += TRADU.i.Traducir("PA Máximo: ") + "<color=" + (buff.percAPMax > 0 ? "green" : "red") + ">" + buff.percAPMax + "%" + "</color>\n";
    if (buff.cantAPMax != 0) descripcion += TRADU.i.Traducir("PA Máximo: ") + "<color=" + (buff.cantAPMax > 0 ? "green" : "red") + ">" + buff.cantAPMax + "</color>\n";

    if (buff.percPMMax != 0) descripcion += TRADU.i.Traducir("PM Máximo: ") + "<color=" + (buff.percPMMax > 0 ? "green" : "red") + ">" + buff.percPMMax + "%" + "</color>\n";
    if (buff.cantPMMax != 0) descripcion += TRADU.i.Traducir("PM Máximo: ") + "<color=" + (buff.cantPMMax > 0 ? "green" : "red") + ">" + buff.cantPMMax + "</color>\n";

    if (buff.percAtFue != 0) descripcion += TRADU.i.Traducir("Fuerza: ") + "<color=" + (buff.percAtFue > 0 ? "green" : "red") + ">" + buff.percAtFue + "%" + "</color>\n";
    if (buff.cantAtFue != 0) descripcion += TRADU.i.Traducir("Fuerza: ") + "<color=" + (buff.cantAtFue > 0 ? "green" : "red") + ">" + buff.cantAtFue + "</color>\n";

    if (buff.percAtAgi != 0) descripcion += TRADU.i.Traducir("Agilidad: ") + "<color=" + (buff.percAtAgi > 0 ? "green" : "red") + ">" + buff.percAtAgi + "%" + "</color>\n";
    if (buff.cantAtAgi != 0) descripcion += TRADU.i.Traducir("Agilidad: ") + "<color=" + (buff.cantAtAgi > 0 ? "green" : "red") + ">" + buff.cantAtAgi + "</color>\n";

    if (buff.percAtFPod != 0) descripcion += TRADU.i.Traducir("Poder: ") + "<color=" + (buff.percAtFPod > 0 ? "green" : "red") + ">" + buff.percAtFPod + "%" + "</color>\n";
    if (buff.cantAtPod != 0) descripcion += TRADU.i.Traducir("Poder: ") + "<color=" + (buff.cantAtPod > 0 ? "green" : "red") + ">" + buff.cantAtPod + "</color>\n";

    if (buff.percArmadura != 0) descripcion += TRADU.i.Traducir("Armadura: ") + "<color=" + (buff.percArmadura > 0 ? "green" : "red") + ">" + buff.percArmadura + "%" + "</color>\n";
    if (buff.cantArmadura != 0) descripcion += TRADU.i.Traducir("Armadura: ") + "<color=" + (buff.cantArmadura > 0 ? "green" : "red") + ">" + buff.cantArmadura + "</color>\n";

    if (buff.percResFue != 0) descripcion += TRADU.i.Traducir("Resistencia Fuego: ") + "<color=" + (buff.percResFue > 0 ? "green" : "red") + ">" + buff.percResFue + "%" + "</color>\n";
    if (buff.cantResFue != 0) descripcion += TRADU.i.Traducir("Resistencia Fuego: ") + "<color=" + (buff.cantResFue > 0 ? "green" : "red") + ">" + buff.cantResFue + "</color>\n";

    if (buff.percResHie != 0) descripcion += TRADU.i.Traducir("Resistencia Hielo: ") + "<color=" + (buff.percResHie > 0 ? "green" : "red") + ">" + buff.percResHie + "%" + "</color>\n";
    if (buff.cantResHie != 0) descripcion += TRADU.i.Traducir("Resistencia Hielo: ") + "<color=" + (buff.cantResHie > 0 ? "green" : "red") + ">" + buff.cantResHie + "</color>\n";

    if (buff.percResRay != 0) descripcion += TRADU.i.Traducir("Resistencia Rayo: ") + "<color=" + (buff.percResRay > 0 ? "green" : "red") + ">" + buff.percResRay + "%" + "</color>\n";
    if (buff.cantResRay != 0) descripcion += TRADU.i.Traducir("Resistencia Rayo: ") + "<color=" + (buff.cantResRay > 0 ? "green" : "red") + ">" + buff.cantResRay + "</color>\n";

    if (buff.percResAci != 0) descripcion += TRADU.i.Traducir("Resistencia Ácido: ") + "<color=" + (buff.percResAci > 0 ? "green" : "red") + ">" + buff.percResAci + "%" + "</color>\n";
    if (buff.cantResAci != 0) descripcion += TRADU.i.Traducir("Resistencia Ácido: ") + "<color=" + (buff.cantResAci > 0 ? "green" : "red") + ">" + buff.cantResAci + "</color>\n";

    if (buff.percResArc != 0) descripcion += TRADU.i.Traducir("Resistencia Arcano: ") + "<color=" + (buff.percResArc > 0 ? "green" : "red") + ">" + buff.percResArc + "%" + "</color>\n";
    if (buff.cantResArc != 0) descripcion += TRADU.i.Traducir("Resistencia Arcano: ") + "<color=" + (buff.cantResArc > 0 ? "green" : "red") + ">" + buff.cantResArc + "</color>\n";

    if (buff.percResNec != 0) descripcion += TRADU.i.Traducir("Resistencia Necrótica: ") + "<color=" + (buff.percResNec > 0 ? "green" : "red") + ">" + buff.percResNec + "%" + "</color>\n";
    if (buff.cantResNec != 0) descripcion += TRADU.i.Traducir("Resistencia Necrótica: ") + "<color=" + (buff.cantResNec > 0 ? "green" : "red") + ">" + buff.cantResNec + "</color>\n";

    if (buff.percDefensa != 0) descripcion += TRADU.i.Traducir("Defensa: ") + "<color=" + (buff.percDefensa > 0 ? "green" : "red") + ">" + buff.percDefensa + "%" + "</color>\n";
    if (buff.cantDefensa != 0) descripcion += TRADU.i.Traducir("Defensa: ") + "<color=" + (buff.cantDefensa > 0 ? "green" : "red") + ">" + buff.cantDefensa + "</color>\n";

    if (buff.percAtaque != 0) descripcion += TRADU.i.Traducir("Ataque: ") + "<color=" + (buff.percAtaque > 0 ? "green" : "red") + ">" + buff.percAtaque + "%" + "</color>\n";
    if (buff.cantAtaque != 0) descripcion += TRADU.i.Traducir("Ataque: ") + "<color=" + (buff.cantAtaque > 0 ? "green" : "red") + ">" + buff.cantAtaque + "</color>\n";

    if (buff.cantDanioPorcentaje != 0) descripcion += TRADU.i.Traducir("Daño: ") + "<color=" + (buff.cantDanioPorcentaje > 0 ? "green" : "red") + ">" + buff.cantDanioPorcentaje + "%" + "</color>\n";

    if (buff.cantCritDado != 0) descripcion += TRADU.i.Traducir("Crítico Dado: ") + "<color=" + (buff.cantCritDado > 0 ? "green" : "red") + ">" + buff.cantCritDado + "</color>\n";
    if (buff.cantPenetracionArmadura != 0) descripcion += EtiquetaBilingue("Penetracion armadura: ", "Armor Penetration: ") + "<color=" + (buff.cantPenetracionArmadura > 0 ? "green" : "red") + ">" + buff.cantPenetracionArmadura + "</color>\n";
    if (buff.cantReduccionDanioRecibidoPorcentaje != 0) descripcion += EtiquetaBilingue("Reduccion dano recibido: ", "Damage reduction: ") + "<color=" + (buff.cantReduccionDanioRecibidoPorcentaje > 0 ? "green" : "red") + ">" + buff.cantReduccionDanioRecibidoPorcentaje + "%" + "</color>\n";
    if (buff.cantReduccionDanioCriticoRecibidoPorcentaje != 0) descripcion += EtiquetaBilingue("Reduccion dano critico recibido: ", "Critical damage reduction: ") + "<color=" + (buff.cantReduccionDanioCriticoRecibidoPorcentaje > 0 ? "green" : "red") + ">" + buff.cantReduccionDanioCriticoRecibidoPorcentaje + "%" + "</color>\n";
    if (buff.cantResistenciaEstadosPorcentaje != 0) descripcion += EtiquetaBilingue("Resistencia estados: ", "Status resistance: ") + "<color=" + (buff.cantResistenciaEstadosPorcentaje > 0 ? "green" : "red") + ">" + buff.cantResistenciaEstadosPorcentaje + "%" + "</color>\n";
    if (buff.cantEspinasDanioPlano != 0) descripcion += EtiquetaBilingue("Espinas dano plano: ", "Flat thorns damage: ") + "<color=" + (buff.cantEspinasDanioPlano > 0 ? "green" : "red") + ">" + buff.cantEspinasDanioPlano + "</color>\n";
    if (buff.cantEspinasDanioPorcentaje != 0) descripcion += EtiquetaBilingue("Espinas dano %: ", "Thorns damage %: ") + "<color=" + (buff.cantEspinasDanioPorcentaje > 0 ? "green" : "red") + ">" + buff.cantEspinasDanioPorcentaje + "%" + "</color>\n";

    if (buff.percCritDaño != 0) descripcion += TRADU.i.Traducir("Daño Crítico: ") + "<color=" + (buff.percCritDaño > 0 ? "green" : "red") + ">" + buff.percCritDaño + "%" + "</color>\n";
    if (buff.cantCritDaño != 0) descripcion += TRADU.i.Traducir("Daño Crítico: ") + "<color=" + (buff.cantCritDaño > 0 ? "green" : "red") + ">" + buff.cantCritDaño + "</color>\n";

    if (buff.percTsReflejos != 0) descripcion += TRADU.i.Traducir("TS Reflejos: ") + "<color=" + (buff.percTsReflejos > 0 ? "green" : "red") + ">" + buff.percTsReflejos + "%" + "</color>\n";
    if (buff.cantTsReflejos != 0) descripcion += TRADU.i.Traducir("TS Reflejos: ") + "<color=" + (buff.cantTsReflejos > 0 ? "green" : "red") + ">" + buff.cantTsReflejos + "</color>\n";

    if (buff.percTsFortaleza != 0) descripcion += TRADU.i.Traducir("TS Fortaleza: ") + "<color=" + (buff.percTsFortaleza > 0 ? "green" : "red") + ">" + buff.percTsFortaleza + "%" + "</color>\n";
    if (buff.cantTsFortaleza != 0) descripcion += TRADU.i.Traducir("TS Fortaleza: ") + "<color=" + (buff.cantTsFortaleza > 0 ? "green" : "red") + ">" + buff.cantTsFortaleza + "</color>\n";

    if (buff.percTsMental != 0) descripcion += TRADU.i.Traducir("TS Mental: ") + "<color=" + (buff.percTsMental > 0 ? "green" : "red") + ">" + buff.percTsMental + "%" + "</color>\n";
    if (buff.cantTsMental != 0) descripcion += TRADU.i.Traducir("TS Mental: ") + "<color=" + (buff.cantTsMental > 0 ? "green" : "red") + ">" + buff.cantTsMental + "</color>\n";

    if (buff.cantDamBonusElementalAci != 0) descripcion += TRADU.i.Traducir("Bonus daño Ácido: ") + "<color=" + (buff.cantDamBonusElementalAci > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalAci + "</color>\n";
    if (buff.cantDamBonusElementalArc != 0) descripcion += TRADU.i.Traducir("Bonus daño arcano: ") + "<color=" + (buff.cantDamBonusElementalArc > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalArc + "</color>\n";
    if (buff.cantDamBonusElementalFue != 0) descripcion += TRADU.i.Traducir("Bonus daño fuego: ") + "<color=" + (buff.cantDamBonusElementalFue > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalFue + "</color>\n";
    if (buff.cantDamBonusElementalHie != 0) descripcion += TRADU.i.Traducir("Bonus daño hielo: ") + "<color=" + (buff.cantDamBonusElementalHie > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalHie + "</color>\n";
    if (buff.cantDamBonusElementalNec != 0) descripcion += TRADU.i.Traducir("Bonus daño necro: ") + "<color=" + (buff.cantDamBonusElementalNec > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalNec + "</color>\n";
    if (buff.cantDamBonusElementalRay != 0) descripcion += TRADU.i.Traducir("Bonus daño rayo: ") + "<color=" + (buff.cantDamBonusElementalRay > 0 ? "green" : "red") + ">" + buff.cantDamBonusElementalRay + "</color>\n";

    if (buff.DuracionBuffRondas > 0) descripcion += TRADU.i.Traducir("Duración: ") + buff.DuracionBuffRondas + TRADU.i.Traducir(" rondas\n");
    else if (buff.DuracionBuffRondas < 0) descripcion += TRADU.i.Traducir("Duración: Permanente\n");
    if (buff.seConsumeAlRecibirAtaque) descripcion += TRADU.i.Traducir("Se consume al recibir el próximo ataque.") + "\n";

    return descripcion;
  }

  private string EtiquetaBilingue(string textoEs, string textoEn)
  {
    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      return textoEn;
    }

    return textoEs;
  }

  public void ActivarTooltip()
  {
    if (!debarravida)
      goTooltip.SetActive(true);


    if (debarravida)
    {
      BuscartooltipBatallaTag();
      scTooltipBatalla.ShowTooltipText(textTooltip.text);

     

    }
  }

  public void DesactivarTooltip()
  {
    
    if (!debarravida)
      goTooltip.SetActive(false);


    if (debarravida)
    {
      BuscartooltipBatallaTag();

      if (scTooltipBatalla != null)
      {
        scTooltipBatalla.HideTooltipSinAnim();
      }
    }
  }

  public bool debarravida = false;

 
 
}



