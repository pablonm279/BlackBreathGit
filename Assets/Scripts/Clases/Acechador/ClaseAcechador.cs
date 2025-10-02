using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class ClaseAcechador : Unidad
{

    public int PASIVA_MaestriaConBallestaMano; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
    public int PASIVA_MaestriaConEspadacorta; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
    public int PASIVA_Masacre; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b

    public override void ComienzoBatallaClase()
    {
        base.ComienzoBatallaClase();

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
        buff.cantAtaque += 2;
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

    public override void AcabaDeMatarUnidad(Unidad uVictima)
    {
        base.AcabaDeMatarUnidad(uVictima);
        // Aquí puedes agregar lógica adicional que quieras ejecutar cuando el Acechador acaba de matar a una unidad

        if (PASIVA_Masacre > 0) // Si tiene la pasiva Masacre
        {

           CambiarAPActual(+2); // Gana 1 Acción de ataque adicional
            if (PASIVA_Masacre == 5) { CambiarAPActual(+1); }



            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Masacre";
            buff.boolfDebufftBuff = true;
            buff.DuracionBuffRondas = 1;
            buff.cantDanioPorcentaje += 10;
            if (PASIVA_Masacre > 2) { buff.cantDanioPorcentaje += 5; } // Aumenta el daño si es nivel 3 o más
            buff.AplicarBuff(this);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);

            int DC = 5; // Valor de dificultad base para la tirada de salvación
            if (PASIVA_Masacre > 1) { DC += 1; }
            foreach (Unidad uni in ObtenerTodosEnemigos())
            {
                if (uni.TiradaSalvacion(uni.mod_TSMental, DC + mod_CarAgilidad) && !uni.TieneBuffNombre("Aterrorizado"))
                {
                    /////////////////////////////////////////////
                    //BUFF ---- Así se aplica un buff/debuff
                    Buff buff2 = new Buff();
                    buff2.buffNombre = "Aterrorizado";
                    buff2.boolfDebufftBuff = false;
                    buff2.DuracionBuffRondas = 2;
                    buff2.cantAtaque -= 2;
                    buff2.cantAPMax -= 1;
                    if (PASIVA_Masacre == 4) { buff2.cantAPMax--; }
                    buff2.cantTsMental -= 2;
                    buff2.AplicarBuff(uni);
                    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
                    Buff buffComponent2 = ComponentCopier.CopyComponent(buff2, uni.gameObject);
                }
            }

         Invoke("ActualizarCirculosDelay", 0.5f); //Para que se actualice el UI de AP luego de un delay, para que no se vea el cambio de golpe


        }
    }
    

    void ActualizarCirculosDelay()
    {
        BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
    }


    public bool tieneArmaduradeVelo; //esto se pone TRUE al inicio del combate en AdministradorEscenas "AplicarEfectosItemsEspecificos"
    public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);
        // Aquí puedes agregar lógica adicional que quieras ejecutar cuando el Acechador recibe daño


        //Armadura de Cuero Reforzado de Velo
        if (tieneArmaduradeVelo && esCritico)
        {
            uCausante.EstablecerAPActualA(0); //El atacante se queda sin AP asi no lo sigue atacando
            GanarEscondido(1);
            BattleManager.Instance.EscribirLog($"{uNombre} es escondido en las sombras tras recibir un ataque crítico por su Armadura de Velo.");
        }
    }

}


