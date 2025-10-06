using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class EnemigoUnidadLadron : Unidad
{

   
    public override void ComienzoBatallaEnemigo()
    {
        base.ComienzoBatallaEnemigo();

        // Inicializa la clase Acechador

        GanarEscondido(1);
    }

    public override void GanarEscondido(int n)
    {
        base.GanarEscondido(n);
        // Obtener buff
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Al Acecho";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = -1;
        buff.cantAtaque += 1;
        buff.cantCritDado += 1;
        buff.cantDanioPorcentaje += 10;
        buff.AplicarBuff(this);

        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);


    }

    public override void PerderEscondido()
    {
        base.PerderEscondido();
        Invoke("RemoverAlAcechoConDelay", 1.2f); //Se remueve el buff "Al Acecho" después de 1.5 segundos para que el buff afecte la habilidad que lo removió

    }

    void RemoverAlAcechoConDelay()
    {
        RemoverBuffNombre("Al Acecho");
    }

 
  

}


