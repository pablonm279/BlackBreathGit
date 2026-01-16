using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desactivarsizonaincorrecta : MonoBehaviour
{
    public int IDzonaCorrecta;

    void OnEnable()
    {
        print("fuego activado");
        if (CampaignManager.Instance.scAtributosZona.ID != IDzonaCorrecta)
        {
            print("Desactivado nodo por zona incorrecta");
            gameObject.SetActive(false);


        }

    }

    void OnDisable()
    {
        print("fuego desactivado");
    }
}
