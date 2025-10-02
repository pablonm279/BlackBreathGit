using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour
{
  public string buffNombre;
  public string buffDescr;
  // Si true, no genera texto flotante al aplicar/remover (útil para buffs iniciales de pelea, clima, etc.)
  public bool suprimeTextoFlotante = false;

  public bool boolfDebufftBuff; //Si se considera buff true, o debuff false
  public float percHPMax;
  public float cantHPMax;

  public float percIniciativa;
  public float cantIniciativa;

  public float percAPMax;
  public float cantAPMax;

  public float percPMMax;
  public float cantPMMax;

  public float percAtFue;
  public float cantAtFue;

  public float percAtAgi;
  public float cantAtAgi;

  public float percAtFPod;
  public float cantAtPod;

  public float percArmadura;
  public float cantArmadura;

  public float percResFue;
  public float cantResFue;
  public float percResHie;
  public float cantResHie;
  public float percResRay;
  public float cantResRay;
  public float percResAci;
  public float cantResAci;
  public float percResArc;
  public float cantResArc;
  public float percResNec;
  public float cantResNec;
  public float percResDiv;
  public float cantResDiv;
  public float cantBarrera;

  
  public int cantDamBonusElementalFue;
  public int cantDamBonusElementalHie;
  public int cantDamBonusElementalRay;
  public int cantDamBonusElementalAci;
  public int cantDamBonusElementalArc;
  public int cantDamBonusElementalNec;
  public int cantDamBonusElementalDiv;

  public float percDefensa;
  public float cantDefensa;

  public float percAtaque;
  public float cantAtaque;

  public float cantDanioPorcentaje; //Para buffs estilo +10% daño, no se usarían +2 de daño por ejemplo. Tiene mas sentido en porcentaje.

  public float cantCritDado;
  
  public float percCritDaño;
  public float cantCritDaño;

  public float percTsReflejos;
  public float cantTsReflejos;

  public float percTsFortaleza;
  public float cantTsFortaleza;

  public float percTsMental;
  public float cantTsMental;

  public int DuracionBuffRondas; // -1 para permanentes

  public GameObject goVFX;

  public bool esBuffVisibleUI = true;   //Si el buff se muestra en la UI de buffs del jugador, por defecto true.

  public bool esRemovible = true; //Si el buff se puede remover, o es permanente. Por defecto es true.
    public void AplicarBuff(Unidad unidad, Unidad Origen = null, bool removible = true)
    {
        esRemovible = removible; //Si se puede remover con habilidades etc.


        if (percHPMax != 0) { unidad.mod_maxHP *= 1 + percHPMax / 100; }
        if (cantHPMax != 0) { unidad.mod_maxHP += cantHPMax; }

        if (percIniciativa != 0) { unidad.mod_iniciativa *= 1 + percIniciativa / 100; }
        if (cantIniciativa != 0) { unidad.mod_iniciativa += cantIniciativa; }

        if (percAPMax != 0) { unidad.mod_maxAccionP *= 1 + percAPMax / 100; }
        if (cantAPMax != 0) { unidad.mod_maxAccionP += cantAPMax; }

        if (percPMMax != 0) { unidad.mod_maxValentiaP *= 1 + percPMMax / 100; }
        if (cantPMMax != 0) { unidad.mod_maxValentiaP += cantPMMax; }

        if (percAtFue != 0) { unidad.mod_CarFuerza *= 1 + percAtFue / 100; }
        if (cantAtFue != 0) { unidad.mod_CarFuerza += cantAtFue; }

        if (percAtAgi != 0) { unidad.mod_CarAgilidad *= 1 + percAtAgi / 100; }
        if (cantAtAgi != 0) { unidad.mod_CarAgilidad += cantAtAgi; }

        if (percAtFPod != 0) { unidad.mod_CarPoder *= 1 + percAtFPod / 100; }
        if (cantAtPod != 0) { unidad.mod_CarPoder += cantAtPod; }

        if (percArmadura != 0) { unidad.mod_Armadura *= 1 + percArmadura / 100; }
        if (cantArmadura != 0) { unidad.mod_Armadura += cantArmadura; }

        if (percResFue != 0) { unidad.mod_ResFuego *= 1 + percResFue / 100; }
        if (cantResFue != 0) { unidad.mod_ResFuego += cantResFue; }

        if (percResHie != 0) { unidad.mod_ResHielo *= 1 + percResHie / 100; }
        if (cantResHie != 0) { unidad.mod_ResHielo += cantResHie; }

        if (percResRay != 0) { unidad.mod_ResRayo *= 1 + percResRay / 100; }
        if (cantResRay != 0) { unidad.mod_ResRayo += cantResRay; }

        if (percResAci != 0) { unidad.mod_ResAcido *= 1 + percResAci / 100; }
        if (cantResAci != 0) { unidad.mod_ResAcido += cantResAci; }

        if (percResArc != 0) { unidad.mod_ResArcano *= 1 + percResArc / 100; }
        if (cantResArc != 0) { unidad.mod_ResArcano += cantResArc; }

        if (percResNec != 0) { unidad.mod_ResNecro *= 1 + percResNec / 100; }
        if (cantResNec != 0) { unidad.mod_ResNecro += cantResNec; }

        if (percResDiv != 0) { unidad.mod_ResDivino *= 1 + percResDiv / 100; }
        if (cantResDiv != 0) { unidad.mod_ResDivino += cantResDiv; }

        if (cantDamBonusElementalAci != 0) { unidad.bonusdam_acido += cantDamBonusElementalAci; }
        if (cantDamBonusElementalArc != 0) { unidad.bonusdam_arcano += cantDamBonusElementalArc; }
        if (cantDamBonusElementalFue != 0) { unidad.bonusdam_fuego += cantDamBonusElementalFue; }
        if (cantDamBonusElementalHie != 0) { unidad.bonusdam_hielo += cantDamBonusElementalHie; }
        if (cantDamBonusElementalNec != 0) { unidad.bonusdam_necro += cantDamBonusElementalNec; }
        if (cantDamBonusElementalRay != 0) { unidad.bonusdam_rayo += cantDamBonusElementalRay; }
        if (cantDamBonusElementalDiv != 0) { unidad.bonusdam_divino += cantDamBonusElementalDiv; }





        if (percDefensa != 0) { unidad.mod_Defensa *= 1 + percDefensa / 100; }
        if (cantDefensa != 0) { unidad.mod_Defensa += cantDefensa; }

        if (percAtaque != 0) { unidad.mod_Ataque *= 1 + percAtaque / 100; }
        if (cantAtaque != 0) { unidad.mod_Ataque += cantAtaque; }


        if (cantDanioPorcentaje != 0) { unidad.mod_DanioPorcentaje += cantDanioPorcentaje; }

        if (cantCritDado != 0) { unidad.mod_CriticoRangoDado += cantCritDado; }

        if (percCritDaño != 0) { unidad.mod_CriticoDañoBonus *= 1 + percCritDaño / 100; }
        if (cantCritDaño != 0) { unidad.mod_CriticoDañoBonus += cantCritDaño; }

        if (percTsReflejos != 0) { unidad.mod_TSReflejos *= 1 + percTsReflejos / 100; }
        if (cantTsReflejos != 0) { unidad.mod_TSReflejos += cantTsReflejos; }

        if (percTsFortaleza != 0) { unidad.mod_TSFortaleza *= 1 + percTsFortaleza / 100; }
        if (cantTsFortaleza != 0) { unidad.mod_TSFortaleza += cantTsFortaleza; }

        if (percTsMental != 0) { unidad.mod_TSMental *= 1 + percTsMental / 100; }
        if (cantTsMental != 0) { unidad.mod_TSMental += cantTsMental; }

        if (cantBarrera != 0) { unidad.barreraDeDanio += cantBarrera; }

        Color colorTexto = boolfDebufftBuff ? Color.cyan : Color.magenta;
        bool suprimirPorInicioCombate = (BattleManager.Instance != null && BattleManager.Instance.silenciarLogCombate);
        if (!suprimeTextoFlotante && !suprimirPorInicioCombate)
        {
            if (DuracionBuffRondas > 0)
            {
                unidad.GenerarTextoFlotante(buffNombre + " " + DuracionBuffRondas + "T", colorTexto);
            }
            else if (DuracionBuffRondas < 0)
            {
                unidad.GenerarTextoFlotante(buffNombre, colorTexto);
            }
        }

        string sBuff = $"{unidad.uNombre} recibe {buffNombre}.";

        if (boolfDebufftBuff)
        {
            sBuff = $"<color=#06b897>{sBuff}</color>";
        }
        else
        {
            sBuff = $"<color=#b83406>{sBuff}</color>";
        }

        BattleManager.Instance.EscribirLog(sBuff);
    
    // Repite el mismo patrón para otros atributos si es necesario...
    }


public void RemoverBuff(Unidad unidad)
{
     //Manejo VFX Buff
    if(goVFX != null)//Si se asignó un VFX al crear el efecto, se destruye.
    {
       Destroy(goVFX);

    }
 
    if (percHPMax != 0) { unidad.mod_maxHP /= 1 + percHPMax / 100; }
    if (cantHPMax != 0) { unidad.mod_maxHP -= cantHPMax; }

    if (percIniciativa != 0) { unidad.mod_iniciativa /= 1 + percIniciativa / 100; }
    if (cantIniciativa != 0) { unidad.mod_iniciativa -= cantIniciativa; }

    if (percAPMax != 0) { unidad.mod_maxAccionP /= 1 + percAPMax / 100; }
    if (cantAPMax != 0) { unidad.mod_maxAccionP -= cantAPMax; }

    if (percPMMax != 0) { unidad.mod_maxValentiaP /= 1 + percPMMax / 100; }
    if (cantPMMax != 0) { unidad.mod_maxValentiaP -= cantPMMax; }

    if (percAtFue != 0) { unidad.mod_CarFuerza /= 1 + percAtFue / 100; }
    if (cantAtFue != 0) { unidad.mod_CarFuerza -= cantAtFue; }

    if (percAtAgi != 0) { unidad.mod_CarAgilidad /= 1 + percAtAgi / 100; }
    if (cantAtAgi != 0) { unidad.mod_CarAgilidad -= cantAtAgi; }

    if (percAtFPod != 0) { unidad.mod_CarPoder /= 1 + percAtFPod / 100; }
    if (cantAtPod != 0) { unidad.mod_CarPoder -= cantAtPod; }

    if (percArmadura != 0) { unidad.mod_Armadura /= 1 + percArmadura / 100; }
    if (cantArmadura != 0) { unidad.mod_Armadura -= cantArmadura; }

    if (percResFue != 0) { unidad.mod_ResFuego /= 1 + percResFue / 100; }
    if (cantResFue != 0) { unidad.mod_ResFuego -= cantResFue; }

    if (percResHie != 0) { unidad.mod_ResHielo /= 1 + percResHie / 100; }
    if (cantResHie != 0) { unidad.mod_ResHielo -= cantResHie; }

    if (percResRay != 0) { unidad.mod_ResRayo /= 1 + percResRay / 100; }
    if (cantResRay != 0) { unidad.mod_ResRayo -= cantResRay; }

    if (percResAci != 0) { unidad.mod_ResAcido /= 1 + percResAci / 100; }
    if (cantResAci != 0) { unidad.mod_ResAcido -= cantResAci; }

    if (percResArc != 0) { unidad.mod_ResArcano /= 1 + percResArc / 100; }
    if (cantResArc != 0) { unidad.mod_ResArcano -= cantResArc; }

    if (percResNec != 0) { unidad.mod_ResNecro /= 1 + percResNec / 100; }
    if (cantResNec != 0) { unidad.mod_ResNecro -= cantResNec; }

    if (percResDiv != 0) { unidad.mod_ResDivino /= 1 + percResDiv / 100; }
    if (cantResDiv != 0) { unidad.mod_ResDivino -= cantResDiv; }

    if (percDefensa != 0) { unidad.mod_Defensa /= 1 + percDefensa / 100; }
    if (cantDefensa != 0) { unidad.mod_Defensa -= cantDefensa; }

    if (percAtaque != 0) { unidad.mod_Ataque /= 1 + percAtaque / 100; }
    if (cantAtaque != 0) { unidad.mod_Ataque -= cantAtaque; }

   
    if (cantDanioPorcentaje != 0) { unidad.mod_DanioPorcentaje -= cantDanioPorcentaje; }

    if (cantCritDado != 0) { unidad.mod_CriticoRangoDado -= cantCritDado; }
    
    if (percCritDaño != 0) { unidad.mod_CriticoDañoBonus /= 1 + percCritDaño / 100; }
    if (cantCritDaño != 0) { unidad.mod_CriticoDañoBonus -= cantCritDaño; }

    if (percTsReflejos != 0) { unidad.mod_TSReflejos /= 1 + percTsReflejos / 100; }
    if (cantTsReflejos != 0) { unidad.mod_TSReflejos -= cantTsReflejos;  }

    if (percTsFortaleza != 0) { unidad.mod_TSFortaleza /= 1 + percTsFortaleza / 100; }
    if (cantTsFortaleza != 0) { unidad.mod_TSFortaleza -= cantTsFortaleza; }

    if (percTsMental != 0) { unidad.mod_TSMental /= 1 + percTsMental / 100; }
    if (cantTsMental != 0) { unidad.mod_TSMental -= cantTsMental; }

    if (cantBarrera != 0) { unidad.barreraDeDanio -= cantBarrera; if (unidad.barreraDeDanio < 0) { unidad.barreraDeDanio = 0; } }

    if (cantDamBonusElementalAci != 0) { unidad.bonusdam_acido -= cantDamBonusElementalAci; }
    if (cantDamBonusElementalArc != 0) { unidad.bonusdam_arcano -= cantDamBonusElementalArc; }
    if (cantDamBonusElementalFue != 0) { unidad.bonusdam_fuego -= cantDamBonusElementalFue; }
    if (cantDamBonusElementalHie != 0) { unidad.bonusdam_hielo -= cantDamBonusElementalHie; }
    if (cantDamBonusElementalNec != 0) { unidad.bonusdam_necro -= cantDamBonusElementalNec; }
    if (cantDamBonusElementalRay != 0) { unidad.bonusdam_rayo -= cantDamBonusElementalRay; }
    if (cantDamBonusElementalDiv != 0) { unidad.bonusdam_divino -= cantDamBonusElementalDiv; }
    
    bool suprimirPorInicioCombate = (BattleManager.Instance != null && BattleManager.Instance.silenciarLogCombate);
    if (!suprimeTextoFlotante && !suprimirPorInicioCombate)
    {
        unidad.GenerarTextoFlotante("<s>" + buffNombre + "</s>", Color.cyan);
    }

    string sBuff = $"{unidad.uNombre} ya no tiene {buffNombre}.";

    if(boolfDebufftBuff)
    {
        sBuff = $"<color=#06b897>{sBuff}</color>";
    }
    else
    {
        sBuff = $"<color=#b83406>{sBuff}</color>";
    }


    if(CustomEffectInicioTurnoID == 1 && unidadOrigen != null)
    { //Al remover buff, fuego fatuo encarnando a la unidad se vuelve a ver
       gameObject.transform.GetChild(3).GetChild(1).gameObject.SetActive(true);
    }
    
    BattleManager.Instance.EscribirLog(sBuff);
    // Si se remueve "Acumulando" del Canalizador, liberar la pose fija de habilidad
    if (buffNombre == "Acumulando" && unidad is ClaseCanalizador)
    {
        var poseCtrl = unidad.GetComponent<UnidadPoseController>();
        if (poseCtrl != null) { poseCtrl.ExitPoseHold(); }
    }
    Destroy(this);

    // Repite el mismo patrón para otros atributos si es necesario...

    
}
  
//Efectos custom
public int CustomEffectInicioTurnoID;
public Unidad unidadOrigen;

public void activarCustomEffectInicioTurno()
{ //Efectos propios de un buff con efectos custom, que se llaman al iniciar el turno.
Unidad scEstaUnidad =  gameObject.GetComponent<Unidad>();

    if(CustomEffectInicioTurnoID == 1) //Encarnación Fuego Fatuo
    {
            scEstaUnidad.EstablecerAPActualA(0);
      BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
      scEstaUnidad.RecibirDanio(4,4,false,unidadOrigen);
      scEstaUnidad.RecibirDanio(4,9,false,unidadOrigen);
      unidadOrigen.RecibirCuracion(4, false);
    }
}

}
