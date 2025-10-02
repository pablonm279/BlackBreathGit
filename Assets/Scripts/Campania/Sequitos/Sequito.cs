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
