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
    [SerializeField] GameObject contenidoPanel;
    [SerializeField] TextMeshProUGUI txtBotonEchar;

    bool contenidoPreparado;

    private void Start()
    {
        if (txtBotonEchar == null)
        {
            return;
        }

        txtBotonEchar.text = TRADU.i != null ? TRADU.i.Traducir("Echar") : "Echar";
    }


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
    public void clickRepresentar()
    {
        MenuSequitos menuSequitos = GetComponentInParent<MenuSequitos>();
        if (menuSequitos != null)
        {
            menuSequitos.MostrarContenido(this);
        }
        else
        {
            MostrarContenido();
        }

        SequitoMercaderes mercaderes = GetComponent<SequitoMercaderes>();
        if (mercaderes != null)
        {
            mercaderes.MostrarInventarioVenta();
            mercaderes.Actualizar();
        }
    }

    public void OcultarContenido()
    {
        if (contenidoPanel == null)
        {
            contenidoPanel = BuscarContenidoPanel();
        }

        if (contenidoPanel != null)
        {
            contenidoPanel.SetActive(false);
        }
    }

    public void PrepararContenido(Transform placeholder)
    {
        if (contenidoPanel == null)
        {
            contenidoPanel = BuscarContenidoPanel();
        }

        if (contenidoPanel == null)
        {
            Debug.LogWarning($"[Sequito] {name} no tiene contenidoPanel asignado.", this);
            return;
        }

        if (placeholder == null)
        {
            Debug.LogWarning($"[Sequito] No hay placeholder de contenido para {name}.", this);
            return;
        }

        if (contenidoPreparado && contenidoPanel.transform.parent == placeholder)
        {
            return;
        }

        contenidoPanel.SetActive(false);

        RectTransform rectContenido = contenidoPanel.transform as RectTransform;
        if (rectContenido == null)
        {
            return;
        }

        rectContenido.SetParent(placeholder, false);
        SequitoPanelVisual.Aplicar(this, rectContenido);
        contenidoPreparado = true;
    }

    public void MostrarContenido()
    {
        if (contenidoPanel == null)
        {
            contenidoPanel = BuscarContenidoPanel();
        }

        if (contenidoPanel == null)
        {
            Debug.LogWarning($"[Sequito] {name} no tiene contenidoPanel asignado.", this);
            return;
        }

        contenidoPanel.SetActive(true);
        SequitoPanelVisual.ReiniciarScroll(contenidoPanel);
    }

    GameObject BuscarContenidoPanel()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Contenido")
            {
                return child.gameObject;
            }
        }

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Contenido"))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (contenidoPanel != null && !contenidoPanel.transform.IsChildOf(transform))
        {
            Destroy(contenidoPanel);
        }

        MenuSequitos menuSequitos = GetComponentInParent<MenuSequitos>();
        if (menuSequitos != null)
        {
            menuSequitos.DesregistrarInstancia(this);
        }
    }
}


