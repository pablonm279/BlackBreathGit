using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_VigilarDesdeLasSombras: Actividad
{
    void Awake()
    {
      IDActividad = 14; //

      desc =  TRADU.i.Traducir("<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.");

    }
}
