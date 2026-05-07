using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutodestruirDelay : MonoBehaviour
{
   
   [SerializeField] float delayautodestruir;

    void Start()
    {
        ReiniciarTemporizador();
    }

    public void SetDelay(float nuevoDelay)
    {
        delayautodestruir = Mathf.Max(0f, nuevoDelay);

        if (!isActiveAndEnabled)
        {
            return;
        }

        ReiniciarTemporizador();
    }

    private void ReiniciarTemporizador()
    {
        CancelInvoke(nameof(Destruir));
        Invoke(nameof(Destruir), delayautodestruir);
    }

    void Destruir()
    {
        Destroy(gameObject);
    }
}
