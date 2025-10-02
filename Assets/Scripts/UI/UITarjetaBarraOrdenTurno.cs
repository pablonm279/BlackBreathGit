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
        
      scUInfochar = BattleManager.Instance.scUIInfoChar;
      if(scUInfochar != null)
      {


            if (BattleManager.Instance.unidadActiva == scUnidad)
            {
                if (gameObject != null)
                {
                    gameObject.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.75f);
                    gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                if (gameObject != null)
                {
                    if (scUnidad != null)
                    {
                        if (scUnidad.CasillaPosicion != null)
                        {
                            if (scUnidad.CasillaPosicion.lado == 1)
                            {
                                gameObject.GetComponent<Image>().color = new Color(0.4f, 0.1f, 0.1f);

                            }
                            else { if (gameObject != null) { gameObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.7f); } }
                        }
                    }
                }
                            gameObject.transform.GetChild(0).gameObject.SetActive(false);

       }
       

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
