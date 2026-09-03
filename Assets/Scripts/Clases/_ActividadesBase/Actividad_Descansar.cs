using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Descansar : Actividad
{
    void Awake()
    {
      IDActividad = 1; //Descansar

      desc = "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nMejora la velocidad de recuperación pasiva del personaje.\\nSi se produce un combate, lo arrancará Fresco.";

    }

    public override string ObtenerDescripcion(Personaje personaje = null)
    {
      float porcentajePorHora = CampaignManager.Instance != null
        ? CampaignManager.Instance.ObtenerPorcentajeCuracionPasivaPorHora(personaje, true, true)
        : 4f;
      string etiqueta = TRADU.i != null
        ? TRADU.i.Traducir("Vida recuperada por hora: ")
        : "Vida recuperada por hora: ";

      return base.ObtenerDescripcion(personaje)
        + "\\n"
        + etiqueta
        + porcentajePorHora.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
        + "%.";
    }
}



