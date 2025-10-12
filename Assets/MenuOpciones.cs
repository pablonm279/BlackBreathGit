using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOpciones : MonoBehaviour
{
    
    [SerializeField] GameObject menuOpciones;


    public void abrirMenu()
    {
        menuOpciones.SetActive(true);
        Time.timeScale = 0f;
    }
    public void cerrarMenu()
    {
        menuOpciones.SetActive(false);
        Time.timeScale = 1f;
    }

    public void salirdeljuego()
    {
        Application.Quit();
    }
}
