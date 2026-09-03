using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_RitualDeLimpieza: Actividad
{
    void Awake()
    {
      IDActividad = 10; //

      desc = "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realiza rituales de protección para combatir el Aliento Negro.</i></color>\\n\\nEl Aliento Negro retrocede 4 h cada 24 horas activas y 5 h al descansar.";
    }

    public override string ObtenerDescripcion(Personaje personaje = null)
    {
      int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
      string descripcion = idioma switch
      {
        TRADU.IdiomaIngles => "<color=#0cca74><b>Cleansing Ritual: </b></color><color=#d3d3d3><i>The Purifier performs protection rites to combat the Black Breath.</i></color>\\n\\nThe Black Breath recedes by 4 h every 24 active hours, and by 5 h when resting.",
        TRADU.IdiomaPortugues => "<color=#0cca74><b>Ritual de Limpeza: </b></color><color=#d3d3d3><i>A Purificadora realiza ritos de proteção para combater o Sopro Negro.</i></color>\\n\\nO Sopro Negro recua 4 h a cada 24 horas ativas e 5 h ao descansar.",
        _ => desc
      };

      return AplicarColorDescripcionSuperior(descripcion);
    }
}



