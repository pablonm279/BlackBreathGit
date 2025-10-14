using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class UIEstadoCuadro : MonoBehaviour
{

  public int indexEstadoRepresentado;
  private Image Retrato;

  public GameObject goTooltip;
  public TextMeshProUGUI textTooltip;

  public TextMeshProUGUI textStacks;

  void Awake()
  {
    Retrato = gameObject.GetComponent<Image>();

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
  public Sprite imTierEnergia; //Canalizador
  public void RepresentarEstado(int index, int stacks, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    if (stacks == -1)
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
    if (Retrato == null) { return; }
    if (textTooltip == null) { return; }
    switch (index)
    {
      case 1: Retrato.sprite = imArdiendo; textTooltip.text = "Ardiendo: causa daño cada turno, se apaga con AP disponibles."; break;
      case 2: Retrato.sprite = imAturdido; textTooltip.text = "Aturdido: no puede actuar."; break;
      case 3: Retrato.sprite = imAcido; textTooltip.text = "Ácido: cada acumulación reduce en 1 la armadura."; break;
      case 4: Retrato.sprite = imCongelado; textTooltip.text = "Congelado: reduce AP disponibles y aumenta armadura."; break;
      case 5: Retrato.sprite = ResRed; textTooltip.text = "Resistencias Reducidas: reduce todas las resistencias 1 por acumulación."; break;
      case 6: Retrato.sprite = ArmMod; textTooltip.text = "Armadura Rota: reduce la armadura en 1 por acumulación."; break;
      // case 7: Retrato.sprite = imApMod;  break;
      case 8: Retrato.sprite = imSangrado; textTooltip.text = "Sangrado: cada acumulación resta 1 HP máxima por turno y previene 2 de curación."; break;
      case 9: Retrato.sprite = imVeneno; textTooltip.text = "Veneno: provoca daño por turno, se debe hacer una tirada de salvación de Fortaleza cada turno para curarse, si falla se incrementa en 1."; break;
      case 10: Retrato.sprite = imRegvid; textTooltip.text = "Regeneración: recupera vida cada turno."; break;
      case 11: Retrato.sprite = imRegArm; textTooltip.text = "Regeneración Armadura: recupera Armadura perdida cada turno."; break;
      case 12: Retrato.sprite = imEvasion; textTooltip.text = "Evasión: cada stack aumenta 1 la Defensa, se elimina al recibir daño."; break;
      case 13: Retrato.sprite = imFlechas; textTooltip.text = "Flechas: Cantidad de flechas disponibles."; break;
      case 14: Retrato.sprite = imBonusAcido; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_acido}  Bonus de daño elemental de Acido ."; break;
      case 15: Retrato.sprite = imBonusArcano; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_arcano} Bonus de daño elemental de Arcano."; break;
      case 16: Retrato.sprite = imBonusFuego; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_fuego}  Bonus de daño elemental de Fuego."; break;
      case 17: Retrato.sprite = imBonusHielo; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_hielo}  Bonus de daño elemental de Hielo."; break;
      case 18: Retrato.sprite = imBonusNecro; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_necro}  Bonus de daño elemental de Necro."; break;
      case 19: Retrato.sprite = imBonusRayo; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_rayo}  Bonus daño elemental de Rayo."; break;
      case 20: Retrato.sprite = imPurificadoraFervor; textTooltip.text = "Fervor: Cantidad de Fervor que tiene la purificadora."; break;
      case 21: Retrato.sprite = imBonusDivino; textTooltip.text = $"+1-{BattleManager.Instance.scUIInfoChar.unidadMostrada.bonusdam_divino}  Bonus de daño elemental de Divino."; break;
      case 22: Retrato.sprite = imBarrera; textTooltip.text = "Barrera: previene X cantidad de daño."; break;
      case 23: Retrato.sprite = imResiduoTejido; textTooltip.text = "Residuo de Tejido: se obtiene al recibir curación de origen mágico. Previene X puntos de curación."; break;
      case 24: Retrato.sprite = imEstaEscondido; textTooltip.text = "Escondido I: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto se remueve al atacar o recibir daño."; break;
      case 25: Retrato.sprite = imEstaEscondido; textTooltip.text = "Escondido II: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto no se remueve al recibir daño."; break;
      case 26: Retrato.sprite = imTierEnergia; textTooltip.text = "Energía: Nivel de Energía Acumulada por el Canalizador."; break;
      case 27: Retrato.sprite = imCorrupto; textTooltip.text = "Corrupto: Recibe daño adicional de enemigos Corrompidos que además se curan al dañarlo. Si lo deja fuera de combate un enemigo corrompido, muere."; break;
    }
  }

  public TooltipBatalla scTooltipBatalla;
  void OnStart()
  {
    BuscartooltipBatallaTag();
  }

  void BuscartooltipBatallaTag()
  {
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
  public void RepresentarBuff(Buff buff, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    textStacks.text = "";
    textTooltip.text = GenerarDescripcionBuff(buff); //Efectos del buff
    if (Retrato != null && imBuff != null && imDebuff != null)
    {
      if (buff.boolfDebufftBuff) { Retrato.sprite = imBuff; } else { Retrato.sprite = imDebuff; }
    }
  }

  public void RepresentarReaccion(Reaccion buff, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    textStacks.text = "" + buff.usos;

     if (desdeBarraVida) { textStacks.text = ""; }

    textTooltip.text = buff.descripcion; //Efectos del buff
    if (Retrato != null && imReaccion != null)
    {
      Retrato.sprite = imReaccion;
    }
  }

  public void RepresentarMarca(Marca buff, bool desdeBarraVida = false)
  {
    debarravida = desdeBarraVida;
    textStacks.text = "" + buff.duracion;
    

    if (desdeBarraVida) { textStacks.text = ""; }

    textTooltip.text = buff.descripcion; //Efectos del buff

     if (Retrato != null && imMarca != null)
    {
      Retrato.sprite = imMarca;
    }
  }

  private string GenerarDescripcionBuff(Buff buff)
  {
    string descripcion = "" + buff.buffNombre + "\n" + buff.buffDescr + "\n";


    if (buff.percHPMax != 0) descripcion += $"HP Máximo: <color={(buff.percHPMax > 0 ? "green" : "red")}>{buff.percHPMax}%</color>\n";
    if (buff.cantHPMax != 0) descripcion += $"HP Máximo: <color={(buff.cantHPMax > 0 ? "green" : "red")}>{buff.cantHPMax}</color>\n";

    if (buff.percIniciativa != 0) descripcion += $"Iniciativa: <color={(buff.percIniciativa > 0 ? "green" : "red")}>{buff.percIniciativa}%</color>\n";
    if (buff.cantIniciativa != 0) descripcion += $"Iniciativa: <color={(buff.cantIniciativa > 0 ? "green" : "red")}>{buff.cantIniciativa}</color>\n";

    if (buff.percAPMax != 0) descripcion += $"AP Máximo: <color={(buff.percAPMax > 0 ? "green" : "red")}>{buff.percAPMax}%</color>\n";
    if (buff.cantAPMax != 0) descripcion += $"AP Máximo: <color={(buff.cantAPMax > 0 ? "green" : "red")}>{buff.cantAPMax}</color>\n";

    if (buff.percPMMax != 0) descripcion += $"PM Máximo: <color={(buff.percPMMax > 0 ? "green" : "red")}>{buff.percPMMax}%</color>\n";
    if (buff.cantPMMax != 0) descripcion += $"PM Máximo: <color={(buff.cantPMMax > 0 ? "green" : "red")}>{buff.cantPMMax}</color>\n";

    if (buff.percAtFue != 0) descripcion += $"Ataque Fuerza: <color={(buff.percAtFue > 0 ? "green" : "red")}>{buff.percAtFue}%</color>\n";
    if (buff.cantAtFue != 0) descripcion += $"Ataque Fuerza: <color={(buff.cantAtFue > 0 ? "green" : "red")}>{buff.cantAtFue}</color>\n";

    if (buff.percAtAgi != 0) descripcion += $"Ataque Agilidad: <color={(buff.percAtAgi > 0 ? "green" : "red")}>{buff.percAtAgi}%</color>\n";
    if (buff.cantAtAgi != 0) descripcion += $"Ataque Agilidad: <color={(buff.cantAtAgi > 0 ? "green" : "red")}>{buff.cantAtAgi}</color>\n";

    if (buff.percAtFPod != 0) descripcion += $"Ataque Poder: <color={(buff.percAtFPod > 0 ? "green" : "red")}>{buff.percAtFPod}%</color>\n";
    if (buff.cantAtPod != 0) descripcion += $"Ataque Poder: <color={(buff.cantAtPod > 0 ? "green" : "red")}>{buff.cantAtPod}</color>\n";

    if (buff.percArmadura != 0) descripcion += $"Armadura: <color={(buff.percArmadura > 0 ? "green" : "red")}>{buff.percArmadura}%</color>\n";
    if (buff.cantArmadura != 0) descripcion += $"Armadura: <color={(buff.cantArmadura > 0 ? "green" : "red")}>{buff.cantArmadura}</color>\n";

    if (buff.percResFue != 0) descripcion += $"Resistencia Fuego: <color={(buff.percResFue > 0 ? "green" : "red")}>{buff.percResFue}%</color>\n";
    if (buff.cantResFue != 0) descripcion += $"Resistencia Fuego: <color={(buff.cantResFue > 0 ? "green" : "red")}>{buff.cantResFue}</color>\n";

    if (buff.percResHie != 0) descripcion += $"Resistencia Hielo: <color={(buff.percResHie > 0 ? "green" : "red")}>{buff.percResHie}%</color>\n";
    if (buff.cantResHie != 0) descripcion += $"Resistencia Hielo: <color={(buff.cantResHie > 0 ? "green" : "red")}>{buff.cantResHie}</color>\n";

    if (buff.percResRay != 0) descripcion += $"Resistencia Rayo: <color={(buff.percResRay > 0 ? "green" : "red")}>{buff.percResRay}%</color>\n";
    if (buff.cantResRay != 0) descripcion += $"Resistencia Rayo: <color={(buff.cantResRay > 0 ? "green" : "red")}>{buff.cantResRay}</color>\n";

    if (buff.percResAci != 0) descripcion += $"Resistencia Ácido: <color={(buff.percResAci > 0 ? "green" : "red")}>{buff.percResAci}%</color>\n";
    if (buff.cantResAci != 0) descripcion += $"Resistencia Ácido: <color={(buff.cantResAci > 0 ? "green" : "red")}>{buff.cantResAci}</color>\n";

    if (buff.percResArc != 0) descripcion += $"Resistencia Arcano: <color={(buff.percResArc > 0 ? "green" : "red")}>{buff.percResArc}%</color>\n";
    if (buff.cantResArc != 0) descripcion += $"Resistencia Arcano: <color={(buff.cantResArc > 0 ? "green" : "red")}>{buff.cantResArc}</color>\n";

    if (buff.percResNec != 0) descripcion += $"Resistencia Necrótica: <color={(buff.percResNec > 0 ? "green" : "red")}>{buff.percResNec}%</color>\n";
    if (buff.cantResNec != 0) descripcion += $"Resistencia Necrótica: <color={(buff.cantResNec > 0 ? "green" : "red")}>{buff.cantResNec}</color>\n";

    if (buff.percDefensa != 0) descripcion += $"Defensa: <color={(buff.percDefensa > 0 ? "green" : "red")}>{buff.percDefensa}%</color>\n";
    if (buff.cantDefensa != 0) descripcion += $"Defensa: <color={(buff.cantDefensa > 0 ? "green" : "red")}>{buff.cantDefensa}</color>\n";

    if (buff.percAtaque != 0) descripcion += $"Ataque: <color={(buff.percAtaque > 0 ? "green" : "red")}>{buff.percAtaque}%</color>\n";
    if (buff.cantAtaque != 0) descripcion += $"Ataque: <color={(buff.cantAtaque > 0 ? "green" : "red")}>{buff.cantAtaque}</color>\n";

    if (buff.cantDanioPorcentaje != 0) descripcion += $"Daño: <color={(buff.cantDanioPorcentaje > 0 ? "green" : "red")}>{buff.cantDanioPorcentaje}%</color>\n";

    if (buff.cantCritDado != 0) descripcion += $"Crítico Dado: <color={(buff.cantCritDado > 0 ? "green" : "red")}>{buff.cantCritDado}</color>\n";

    if (buff.percCritDaño != 0) descripcion += $"Daño Crítico: <color={(buff.percCritDaño > 0 ? "green" : "red")}>{buff.percCritDaño}%</color>\n";
    if (buff.cantCritDaño != 0) descripcion += $"Daño Crítico: <color={(buff.cantCritDaño > 0 ? "green" : "red")}>{buff.cantCritDaño}</color>\n";

    if (buff.percTsReflejos != 0) descripcion += $"TS Reflejos: <color={(buff.percTsReflejos > 0 ? "green" : "red")}>{buff.percTsReflejos}%</color>\n";
    if (buff.cantTsReflejos != 0) descripcion += $"TS Reflejos: <color={(buff.cantTsReflejos > 0 ? "green" : "red")}>{buff.cantTsReflejos}</color>\n";

    if (buff.percTsFortaleza != 0) descripcion += $"TS Fortaleza: <color={(buff.percTsFortaleza > 0 ? "green" : "red")}>{buff.percTsFortaleza}%</color>\n";
    if (buff.cantTsFortaleza != 0) descripcion += $"TS Fortaleza: <color={(buff.cantTsFortaleza > 0 ? "green" : "red")}>{buff.cantTsFortaleza}</color>\n";

    if (buff.percTsMental != 0) descripcion += $"TS Mental: <color={(buff.percTsMental > 0 ? "green" : "red")}>{buff.percTsMental}%</color>\n";
    if (buff.cantTsMental != 0) descripcion += $"TS Mental: <color={(buff.cantTsMental > 0 ? "green" : "red")}>{buff.cantTsMental}</color>\n";

    if (buff.cantDamBonusElementalAci != 0) descripcion += $"Bonus daño ácido: <color={(buff.cantDamBonusElementalAci > 0 ? "green" : "red")}>{buff.cantDamBonusElementalAci}</color>\n";
    if (buff.cantDamBonusElementalArc != 0) descripcion += $"Bonus daño arcano: <color={(buff.cantDamBonusElementalArc > 0 ? "green" : "red")}>{buff.cantDamBonusElementalArc}</color>\n";
    if (buff.cantDamBonusElementalFue != 0) descripcion += $"Bonus daño fuego: <color={(buff.cantDamBonusElementalFue > 0 ? "green" : "red")}>{buff.cantDamBonusElementalFue}</color>\n";
    if (buff.cantDamBonusElementalHie != 0) descripcion += $"Bonus daño hielo: <color={(buff.cantDamBonusElementalHie > 0 ? "green" : "red")}>{buff.cantDamBonusElementalHie}</color>\n";
    if (buff.cantDamBonusElementalNec != 0) descripcion += $"Bonus daño necro: <color={(buff.cantDamBonusElementalNec > 0 ? "green" : "red")}>{buff.cantDamBonusElementalNec}</color>\n";
    if (buff.cantDamBonusElementalRay != 0) descripcion += $"Bonus daño rayo: <color={(buff.cantDamBonusElementalRay > 0 ? "green" : "red")}>{buff.cantDamBonusElementalRay}</color>\n";

    if (buff.DuracionBuffRondas > 0) descripcion += $"Duración: {buff.DuracionBuffRondas} rondas\n";
    else if (buff.DuracionBuffRondas < 0) descripcion += "Duración: Permanente\n";

    return descripcion;
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
    
      scTooltipBatalla.tooltipObject.SetActive(false);
    }
  }

  public bool debarravida = false;

 
 
}
