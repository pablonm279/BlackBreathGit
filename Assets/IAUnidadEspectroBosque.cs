using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;
using UnityEngine.UI;
using System.Threading.Tasks;

public class IAUnidadEspectroBosque : Unidad
{

    public Sprite IDLEimagenEtereo;
    public Sprite IDLEimagenPlanofisico;

    public Sprite HABimagenEtereo;
    public Sprite HABimagenPlanofisico;
    
    public Sprite MOVimagenEtereo;
    public Sprite MOVimagenPlanofisico;

    public bool EstaEnPlanoEtereo()
    {
        return !TieneBuffNombre("En plano material");
    }

    public bool EsInvulnerableAFisico(int tipoDanio)
    {
        return EstaEnPlanoEtereo() && tipoDanio >= 1 && tipoDanio <= 3;
    }

    public override async void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        if (EsInvulnerableAFisico(tipoDanio) && estado_invulnerable == 0 && HP_actual > 0)
        {
            await BattleManager.DelayCombateAsync(delayEfectos);
            GenerarTextoFlotante(TRADU.i.Traducir("Invulnerable"), Color.gray, FloatingTextContext.Resist);
            return;
        }

        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);
    }

    public override void RecibirDanioBonusElemental(float Xddanio, int tipoDanio, Unidad uCausante)
    {
        if (EsInvulnerableAFisico(tipoDanio) && estado_invulnerable == 0 && HP_actual > 0)
        {
            GenerarTextoFlotante(TRADU.i.Traducir("Invulnerable"), Color.gray, FloatingTextContext.Resist);
            return;
        }

        base.RecibirDanioBonusElemental(Xddanio, tipoDanio, uCausante);
    }

}


