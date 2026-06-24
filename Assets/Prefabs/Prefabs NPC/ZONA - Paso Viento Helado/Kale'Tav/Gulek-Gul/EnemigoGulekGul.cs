using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class EnemigoGulekGul : Unidad
{
    private UnidadPoseController poseControllerGulek;
    private bool poseMartilloLevantadoActiva;
    private Sprite poseIdleBaseGulek;
    private Sprite poseIdleMartilloGulek;

    void LateUpdate()
    {
        ActualizarPoseMartillo();
    }

    private void ActualizarPoseMartillo(bool forzar = false)
    {
        if (poseControllerGulek == null)
        {
            poseControllerGulek = GetComponent<UnidadPoseController>();
        }

        if (poseControllerGulek == null)
        {
            return;
        }

        if (poseIdleBaseGulek == null)
        {
            poseIdleBaseGulek = poseControllerGulek.poseIdle;
        }

        if (poseIdleMartilloGulek == null)
        {
            poseIdleMartilloGulek = poseControllerGulek.poseHabilidad;
        }

        bool debeMantenerPoseMartilloLevantado = TieneBuffNombre("Martillo Listo");
        if (!forzar && poseMartilloLevantadoActiva == debeMantenerPoseMartilloLevantado)
        {
            return;
        }

        poseMartilloLevantadoActiva = debeMantenerPoseMartilloLevantado;
        Sprite idleActual = poseMartilloLevantadoActiva && poseIdleMartilloGulek != null
            ? poseIdleMartilloGulek
            : poseIdleBaseGulek;

        poseControllerGulek.poseIdle = idleActual;
        poseControllerGulek.poseHabilidad = idleActual;
        poseControllerGulek.RefrescarPoseActual();
    }

    public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0, bool ignorarEscudo = false)
    {
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos, ignorarEscudo);


        if (TieneBuffNombre("Martillo Listo"))
        {

            if (TiradaSalvacion(1, (danio / 2) + 2))
            {
                //Salvación exitosa

                RemoverBuffNombre("Martillo Listo");
                gameObject.GetComponent<IALevantarMartillo>().hActualCooldown = 0;
                gameObject.GetComponent<IAMartilloPesado>().hActualCooldown = 1;
                ActualizarPoseMartillo(true);
                BattleManager.Instance.EscribirLog("Gulek Gul pierde el buff 'Martillo Listo' tras recibir daño y no podrá utilizarlo.");

            }

        }


    }
    
    public override bool TiradaSalvacion(int tipoSalvacion, float dificultadHabilidada, bool porValourGlobal = false) //TRUE no se salva FALSE se salva (xd)
  {
     bool resultado = false;
        int intentos = tipoSalvacion == 3 ? 2 : 1;

     for (int i = 0; i < intentos; i++)
     {
        resultado = base.TiradaSalvacion(tipoSalvacion, dificultadHabilidada, porValourGlobal);

        if (!resultado)
        {
            return resultado;
        }

        if (tipoSalvacion == 3 && i == 0)
        {
            BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" obtiene un intento adicional de Tirada de Salvación."));
        }
     }
        if (tipoSalvacion == 3 && resultado)
        {
            RemoverBuffNombre("Martillo Listo");
            ActualizarPoseMartillo(true);
        } //Si pierde una tirada de voluntad, deja el martillo
     return resultado;
  }
  

}




