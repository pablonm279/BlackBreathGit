using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.TerrainTools;

public class UITarjetaBarraOrdenTurno : MonoBehaviour
{


    public Slider BarraVida;
    public Image Retrato; 
    public Unidad scUnidad;
    public UIInfoChar scUInfochar;
    public GameObject seleccionado;
    public void Start()
    {
        
        Habilidad.OnUsarHabilidad += Habilidad_OnUsarHabilidad;
        BattleManager.Instance.OnTurnoNuevo += BattleManager_OnTurnoNuevo;

        scUInfochar = BattleManager.Instance.scUIInfoChar;

    }
    void Update()
    {
         if(scUInfochar.hayUnidadSeleccionadaParaInfo)
        {
           if(scUInfochar.unidadMostrada != null)
           {
            if(scUInfochar.unidadMostrada == scUnidad )
            {
                seleccionado.SetActive(true);

            }
            else
            { seleccionado.SetActive(false);}
           }
           else
           {seleccionado.SetActive(false);}
        }
        else{seleccionado.SetActive(false);}
       
    }
    public void ActualizarColores()
    {
        if (this == null || gameObject == null)
        {
            return;
        }

        var image = GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        GameObject marcadorSeleccion = null;
        if (transform != null && transform.childCount > 0)
        {
            marcadorSeleccion = transform.GetChild(0)?.gameObject;
        }

        var battleManager = BattleManager.Instance;
        if (battleManager == null)
        {
            return;
        }

        scUInfochar = battleManager.scUIInfoChar;
        if (scUInfochar == null)
        {
            return;
        }

        if (battleManager.unidadActiva == scUnidad)
        {
            image.color = new Color(0.95f, 0.95f, 0.75f);
            if (marcadorSeleccion != null)
            {
                marcadorSeleccion.SetActive(true);
            }
            return;
        }

        if (scUnidad?.CasillaPosicion != null)
        {
            image.color = scUnidad.CasillaPosicion.lado == 1
                ? new Color(0.4f, 0.1f, 0.1f)
                : new Color(0.2f, 0.2f, 0.7f);
        }

        if (marcadorSeleccion != null)
        {
            marcadorSeleccion.SetActive(false);
        }
    }
    public void ActualizarInfo()
    {
      
        BarraVida.value = scUnidad.HP_actual / scUnidad.mod_maxHP;

        Retrato.sprite = scUnidad.uRetrato;
     
        ActualizarColores();
    }

    private void Habilidad_OnUsarHabilidad(object sender, EventArgs empty)
    {
        ActualizarInfo(); 
    }

     private void BattleManager_OnTurnoNuevo(object sender, EventArgs empty)
    {
        MarcarTurnoActual();
         ActualizarColores();
    }

   public void MarcarTurnoActual()
    {
      if(gameObject== null)
      { return;}

      if(gameObject.GetComponent<Image>()== null)
      { return;}

    

    

    }
    Unidad anterior = null;
    public void MarcarUnidadRepresentada(int n) //1 es entra mouse, 0 es sale
    {
   
        if(n == 1) //entra mouse
        { 
          scUnidad.OnMouseEnter(); 
        }
        else if(n == 0) //sale mouse
        {
          scUnidad.OnMouseExit();  
        }
    }

    public void ClickMarcarUnidadRepresentada(int n)
    {     
       
          if(anterior == null)
          {
          anterior = scUnidad;  
          }
          else{anterior = null; }

          scUnidad.OnMouseDown();
         
    }


    private void OnDestroy()
    {
        
        BattleManager.Instance.OnTurnoNuevo -= BattleManager_OnTurnoNuevo;
        
    }
   
    

}
