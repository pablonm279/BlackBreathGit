using System.Collections;
using UnityEngine;

public class DesapareceBloqueoCargar : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DestruirDespuesDeDosSegundos());
    }

    IEnumerator DestruirDespuesDeDosSegundos()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
