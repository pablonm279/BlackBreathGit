using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class ClaseCanalizador : Unidad
{

    [SerializeField] private int TierEnergia = 0;
    public int PASIVA_AcumulacionProtegida; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
    public int PASIVA_ExcesoDePoder; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b


    public async override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);


        ChequearResisteAcumulandoEnergia(danio);
        
        //Remueve reaccion escudo energético al ser dañado si no es NV 4
      if(gameObject != null)
      {
      if(gameObject.GetComponent<ReaccionEscudoEnergetico>() != null)
      {
        ReaccionEscudoEnergetico reacc = gameObject.GetComponent<ReaccionEscudoEnergetico>();
        if(reacc.NIVEL != 4) //Si tiene la reaccion activa y no es nivel 4a, la remueve
        {
          Destroy(reacc);
          await gameObject.GetComponent<Unidad>().GenerarTextoFlotante("<s>" + "Escudo Energético" + "</s>", Color.blue);
          

        }

      }
     }
    

    }

    public override void OcasionoDanioaEnemigo(Unidad victima, int tipoDanio, bool esCritico, float danio)
    {
        base.OcasionoDanioaEnemigo(victima, tipoDanio, esCritico, danio);

        if (PASIVA_ExcesoDePoder > 0 && esCritico)
        {
            int dam =UnityEngine.Random.Range(1,5);
            if (PASIVA_ExcesoDePoder > 1) { dam--; }
            if (PASIVA_ExcesoDePoder > 2) { dam--; }

            RecibirDanio(dam, 8, false, this);
            //--Genera Residuo energetico
            List<Casilla> casillasAlrededor = CasillaPosicion.ObtenerCasillasAlrededor(2);
            List<Casilla> casillasDesocupadas = casillasAlrededor.FindAll(c => c.Presente == null);
            if (casillasDesocupadas.Count > 0)
            {
                Casilla casillaAlAzar = casillasDesocupadas[UnityEngine.Random.Range(0, casillasDesocupadas.Count)];
                // Aquí puedes usar casillaAlAzar según lo que necesites hacer
                casillaAlAzar.AddComponent<ResiduoEnergetico>();
                casillaAlAzar.GetComponent<ResiduoEnergetico>().InicializarCreador(this, PASIVA_ExcesoDePoder);
            }

            if (PASIVA_ExcesoDePoder == 4)
            {
                 //--Genera Residuo energetico
                List<Casilla> casillasAlrededor2 = CasillaPosicion.ObtenerCasillasAlrededor(2);
                List<Casilla> casillasDesocupadas2 = casillasAlrededor.FindAll(c => c.Presente == null);
                if (casillasDesocupadas.Count > 0)
                {
                    Casilla casillaAlAzar2 = casillasDesocupadas2[UnityEngine.Random.Range(0, casillasDesocupadas2.Count)];
                    // Aquí puedes usar casillaAlAzar según lo que necesites hacer
                    casillaAlAzar2.AddComponent<ResiduoEnergetico>();
                    casillaAlAzar2.GetComponent<ResiduoEnergetico>().InicializarCreador(this, PASIVA_ExcesoDePoder);
                }

            }

        }
    }

    public override void ComienzoBatallaClase()
    {
        base.ComienzoBatallaClase();



        if (PASIVA_ExcesoDePoder > 0)
        {
            mod_CriticoRangoDado += 1; 
            if (PASIVA_ExcesoDePoder == 5)
            { 
              mod_CriticoRangoDado += 1; 
            }

        }
     


    }

    public override void RemoverBuffNombre(string nombreBuff)
    {
        base.RemoverBuffNombre(nombreBuff);
         // Si el Canalizador pierde "Acumulando", vuelve a la pose normal
        if (nombreBuff == "Acumulando")
        {
        var poseCtrl = GetComponent<UnidadPoseController>();
        if (poseCtrl != null) { poseCtrl.ExitPoseHold(); }
        }
    }
    void ChequearResisteAcumulandoEnergia(float danio)
    {

        if (TieneBuffNombre("Acumulando")) //Si está acumulando, hace TS mental (concentracion) para ver si se interrumpe.
        {

            int DC = (int)(8 + danio / 4);
            if (TiradaSalvacion(mod_TSMental, 10 + danio / 3))
            {
                RemoverBuffNombre("Acumulando");
                BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" falló la Tirada de Concentración y ya no acumula energía."));
            }
        }



    }

    public override void ActualizarClaseComienzoTurno()
    {
        if (TieneBuffNombre("Acumulando"))
        {
            CambiarEnergia(1);
            RemoverBuffNombre("Acumulando");
        }


    }

    public void CambiarEnergia(int n)
    {
        TierEnergia += n;

        if (TierEnergia < 0) { TierEnergia = 0; }
        if (TierEnergia > 3) { TierEnergia = 3; }

        BattleManager.Instance.EscribirLog($"-{uNombre} ahora Maneja un Nivel {TierEnergia} de Energía.");


        RemoverBuffNombre("Energizado"); //Remueve el buff y lo actualiza segun nivel nuevo de energía
        if (TierEnergia == 1)
        {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Energizado";
            buff.boolfDebufftBuff = true;
            buff.cantDanioPorcentaje += 10;
            buff.cantCritDado += 1;
            buff.cantResArc -= 1;
            buff.AplicarBuff(this);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
        }
        else if (TierEnergia == 2)
        {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Energizado";
            buff.boolfDebufftBuff = true;
            buff.cantDanioPorcentaje += 25;
            buff.cantCritDado += 1;
            buff.cantAPMax += 1;
            buff.cantResArc -= 6;
            buff.AplicarBuff(this);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
        }
        else if(TierEnergia == 3)
        { 
              /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Energizado";
            buff.boolfDebufftBuff = true;
            buff.cantDanioPorcentaje += 40;
            buff.cantCritDado += 2;
            buff.cantAPMax += 2;
            buff.cantResArc -= 14;
            buff.AplicarBuff(this);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
        }


    }
    public int ObtenerEnergia()
    {
        return TierEnergia;
    }
}


