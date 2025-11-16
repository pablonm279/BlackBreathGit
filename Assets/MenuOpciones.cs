using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOpciones : MonoBehaviour
{
    
    [SerializeField] GameObject menuOpciones;


    public void abrirMenu()
    {
        menuOpciones.SetActive(!menuOpciones.activeInHierarchy);
       
    }
    public void cerrarMenu()
    {
        menuOpciones.SetActive(false);
        
    }

    public void salirdeljuego()
    {
        Application.Quit();
    }
}
