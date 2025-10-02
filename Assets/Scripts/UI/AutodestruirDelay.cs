using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutodestruirDelay : MonoBehaviour
{
   
   [SerializeField] float delayautodestruir;

    // Update is called once per frame
    void Start()
    {
        Invoke("Destruir", delayautodestruir);
    }

    void Destruir()
    {
        Destroy(gameObject);
    }
}
