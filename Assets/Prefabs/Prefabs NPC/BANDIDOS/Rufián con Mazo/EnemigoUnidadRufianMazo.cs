using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class EnemigoUnidadRufianMazo : Unidad
{
    bool yaEnfurecio = false;
    public async override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);

        Invoke("EnfurecerRufian", 1.5f);

    }
   
    void EnfurecerRufian()
    {
          if (HP_actual <= 20 && !yaEnfurecio)
        {
            yaEnfurecio = true;
            //Reacciona enfurecido

            // BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "<b>¡Enfurecido!</b>";
            buff.boolfDebufftBuff = true;
            buff.DuracionBuffRondas = 2;
            buff.cantDanioPorcentaje += 15;
            buff.cantDefensa -= 2;
            buff.cantTsMental += 2;
            buff.cantAPMax += 1;
            buff.AplicarBuff(this);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);

        }
    }

}


