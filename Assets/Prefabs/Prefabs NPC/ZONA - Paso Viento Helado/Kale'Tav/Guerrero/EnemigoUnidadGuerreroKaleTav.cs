using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class EnemigoUnidadGuerreroKaleTav : Unidad
{

    public override void RecibirHerida()
    {
        if (TieneBuffNombre("Furioso por Herida"))
        {
            return; // Ya tiene la herida, no hacer nada
        }
        //BUFF ---- Así se aplica un buff/debuff
        Buff Herida = new Buff();
        Herida.buffNombre = "Furioso por Herida";
        Herida.boolfDebufftBuff = true;
        Herida.DuracionBuffRondas = -1;
        Herida.cantAtFue += 1;
        Herida.cantAtAgi += 1;
       
        Herida.cantAPMax += 1;
        Herida.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(Herida, gameObject);
        //--------------------------------------

    }

    public override void AcabaDeMatarUnidad(Unidad Victima)
    { 

        //BUFF ---- Así se aplica un buff/debuff
        Buff Herida = new Buff();
        Herida.buffNombre = "Regocijo Asesino";
        Herida.boolfDebufftBuff = true;
        Herida.DuracionBuffRondas = -1;
        Herida.percCritDaño += 10;
        Herida.cantArmadura += 4;
        Herida.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(Herida, gameObject);
        //--------------------------------------



    }

}


