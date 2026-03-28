using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIBarraOrdenTurno : MonoBehaviour
{
    public GameObject prefabTarjetaBarraOrdenTurno;
    public List<Unidad> lUnidades = new List<Unidad>();

  
    public void ActualizarBarraOrdenTurno()
    {
       
        foreach (Transform buttonTransform in transform)//Esto remueve los retratos anteriores antes de recalcular que retratos corresponden
        {
            Destroy(buttonTransform.gameObject);
        }

        lUnidades.Clear();
        lUnidades = BattleManager.Instance.lUnidadesTotal.ToList(); //Copia los valores, sin el "tolist" copiaba la referencia a la otra lista, peligroso



        foreach (Unidad unidad in lUnidades)
        {
            GameObject GTarjeta = Instantiate(prefabTarjetaBarraOrdenTurno, transform);
            UITarjetaBarraOrdenTurno scTarjeta = GTarjeta.transform.GetChild(3).gameObject.GetComponent<UITarjetaBarraOrdenTurno>();

            scTarjeta.scUnidad = unidad;
            scTarjeta.ActualizarInfo();
            scTarjeta.MarcarTurnoActual();

            int indexunidad = BattleManager.Instance.lUnidadesTotal.IndexOf(unidad);
            int indexTurno = BattleManager.Instance.indexTurno;

            if (indexunidad < indexTurno)
            {
              scTarjeta.oscurecedor.SetActive(true);
            }else
            {
              scTarjeta.oscurecedor.SetActive(false);
            }

        }


    }

}
