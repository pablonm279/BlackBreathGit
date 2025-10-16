using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Sequito : MonoBehaviour
{
    
    public int ID;
    
    public int intRepresentacionciviles; //-1 para séquitos que no se pueden eliminar (iniciales por ejemplo)

    public TextMeshProUGUI txtNombre;
    public Image imSplashart;
    public TextMeshProUGUI txtdesc;
    public TextMeshProUGUI txtmecanicas;


   /* void Start()
    {
       Invoke("actualizarTradu",0.5f);
    }
    private void OnEnable() { 
       Invoke("actualizarTradu",0.5f);
    }

    
    void actualizarTradu()
    {
        txtNombre.text = TRADU.i.Traducir(txtNombre.text);
        txtdesc.text = TRADU.i.Traducir(txtdesc.text);
        txtmecanicas.text = TRADU.i.Traducir(txtmecanicas.text);
    }*/

    private GameObject placeholderLugarContenido;
    public void clickRepresentar()
    {
     
     DesactivarContenidoTodosSequitos();
     placeholderLugarContenido = transform.parent.parent.GetChild(0).gameObject;

     GameObject contenido = transform.GetChild(1).gameObject;
     contenido.SetActive(true);

     contenido.transform.position = placeholderLugarContenido.transform.position;

     if(gameObject.GetComponent<SequitoMercaderes>() != null)
     {
       gameObject.GetComponent<SequitoMercaderes>().MostrarInventarioVenta();
       gameObject.GetComponent<SequitoMercaderes>().Actualizar();
     }

    }


    void DesactivarContenidoTodosSequitos()
    {   
            Transform parent = transform.parent;

            foreach(Transform child in parent)
            {
                child.GetChild(1).gameObject.SetActive(false);
            }



    }







}
