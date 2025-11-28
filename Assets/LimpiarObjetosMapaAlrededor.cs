using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimpiarObjetosMapaAlrededor : MonoBehaviour
{
    [SerializeField] private float radioLimpieza = 10f;
    private const string TagMapaObjeto = "MapaObjeto";

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        LimpiarObjetosCercanos();
    }

    private void LimpiarObjetosCercanos()
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(TagMapaObjeto);

        foreach (GameObject objeto in objetos)
        {
            if (objeto == null || objeto == gameObject)
            {
                continue;
            }

            if (Vector3.Distance(transform.position, objeto.transform.position) <= radioLimpieza)
            {
                Destroy(objeto);
            }
        }
    }
}
