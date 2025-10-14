using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnidadCanvas : MonoBehaviour
{

    public GameObject unidadCanvas;
    public TextMeshProUGUI txtDaño;
    public TextMeshProUGUI txtArmadura;
    public GameObject PrefabtxtDaño;
    public GameObject PrefabtxtValentia;
    public GameObject imMarcador;

    public TextMeshProUGUI txtVida;


    public RectTransform barraVida;
    private Vector3 escalaBase;

    void Start()
    {
        if(barraVida != null)
        {
            escalaBase = barraVida.localScale;   // guardás la escala original
        }
    }

    void Update()
    {
        Unidad unidad = GetComponentInParent<Unidad>();


        if (unidad == null) { return; }

        if (txtArmadura != null) { txtArmadura.text = unidad.ObtenerArmaduraActual().ToString(); }

        if (txtVida != null)
        {

            txtVida.text = ((int)unidad.HP_actual).ToString();
            txtVida.text += "/" + ((int)unidad.mod_maxHP).ToString();

            // Mostrar barrera de daño en azul si existe
            if (unidad.barreraDeDanio > 0)
            {
                txtVida.text += $" <color=#0074A6>({unidad.barreraDeDanio})</color>";
            }
        }
        if (barraVida != null)
        {
            barraVida.gameObject.GetComponent<Slider>().value = unidad.HP_actual / unidad.mod_maxHP * 100;
            
            int posEstaUnidad = BattleManager.Instance.lUnidadesTotal.IndexOf(unidad);
            int indexTurno = BattleManager.Instance.indexTurno;
            bool yapasosuturno = indexTurno > posEstaUnidad+1;


            Image barraFillImage = barraVida.gameObject.transform.GetChild(1).GetChild(0).GetComponentInChildren<Image>();
            if (yapasosuturno)
            {
                barraFillImage.color = new Color(barraFillImage.color.r, barraFillImage.color.g, barraFillImage.color.b, 0.25f); // oscurecer (alpha 0.5)
            }
            else
            {
                barraFillImage.color = new Color(barraFillImage.color.r, barraFillImage.color.g, barraFillImage.color.b, 1f); // normal (alpha 1)
            }
        }
        ActualizarEscalaBarra(unidad);

        ActualizarEstadosIconos();
    }


    [SerializeField] private GameObject casillaEstadoPrefab;
    [SerializeField] private GameObject contenedorCasillasEstados;
    void ActualizarEscalaBarra(Unidad unidad)
    {
        Casilla casilla = unidad.CasillaPosicion;
        if (casilla == null) return;

        // filas X = 1..5
        float minScale = 1.4f;   // 100%
        float maxScale = 2.0f;   // 120%
        float t = Mathf.InverseLerp(1f, 5f, Mathf.Clamp(casilla.posY, 1f, 5f));
        float factor = Mathf.Lerp(minScale, maxScale, t);

        if (barraVida == null) return;
        // Escala uniforme: mismo factor en X e Y (Z queda en 1)
        barraVida.localScale = new Vector3(
        escalaBase.x * factor,
        escalaBase.y * factor,
        1f
    );
    }


    void ActualizarEstadosIconos()
    {
        if (contenedorCasillasEstados == null) return;
        Unidad scUnidadMostrada = GetComponentInParent<Unidad>();
        if (scUnidadMostrada == null) return;

        // Limpiar los iconos previos antes de agregar los nuevos
        foreach (Transform child in contenedorCasillasEstados.transform)
        {
            Destroy(child.gameObject);
        }

        // Mostrar Buffs
        foreach (Buff buff in scUnidadMostrada.gameObject.GetComponents<Buff>())
        {
            if (buff.DuracionBuffRondas != 0 && buff.esBuffVisibleUI)
            {
                if (buff.esRemovible)
                {
                    GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
                    buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarBuff(buff, true);
                }
            }
        }

        // Mostrar Reacciones
        foreach (Reaccion buff in scUnidadMostrada.gameObject.GetComponents<Reaccion>())
        {
            GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarReaccion(buff, true);
        }

        // Mostrar Marcas
        foreach (Marca buff in scUnidadMostrada.gameObject.GetComponents<Marca>())
        {
            GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarMarca(buff, true);
        }

        MostrarEstados(scUnidadMostrada);
    }

   void MostrarEstados(Unidad scUnidadMostrada)
    {
       if(scUnidadMostrada.estado_ardiendo > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(1,-1,true);
     }
     if(scUnidadMostrada.estado_aturdido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(2,-1,true);
     }
     if(scUnidadMostrada.estado_acido > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(3,-1,true);
     }
     if(scUnidadMostrada.estado_congelado > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(4,-1,true);
     }
     if(scUnidadMostrada.estado_ResistenciasReducidas > 0)
     {
       GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
       GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(5,-1,true);
     }
     /*if(scUnidadMostrada.estado_armaduraModificador > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(6,-1,true);
     }*/
     if(scUnidadMostrada.estado_sangrado > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(8,-1,true);
     }
     if(scUnidadMostrada.estado_veneno > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(9,-1,true);
     }
     if(scUnidadMostrada.estado_APModificador > 0)
     {
        /*GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(7,scUnidadMostrada.estado_APModificador); */
     }
     if(scUnidadMostrada.estado_regeneravida > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(10, -1,true);
     }
     if(scUnidadMostrada.estado_regeneraarmadura > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(11,-1,true);
     }
      if(scUnidadMostrada.estado_evasion > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(12,-1,true);
     }
    
   /*  if (scUnidadMostrada is ClaseExplorador)
     {
            ClaseExplorador exp = (ClaseExplorador)scUnidadMostrada;
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(13, -1,true);
     }*/
     if(scUnidadMostrada.bonusdam_acido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(14,-1,true);
     }
       if(scUnidadMostrada.bonusdam_arcano > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(15, -1,true);
     }
       if(scUnidadMostrada.bonusdam_fuego > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(16, -1,true);
     }
       if(scUnidadMostrada.bonusdam_hielo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(17,-1,true);
     }
       if(scUnidadMostrada.bonusdam_necro > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(18,-1,true);
     }
       if(scUnidadMostrada.bonusdam_rayo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(19, -1,true);
     }
    /*  if(scUnidadMostrada is ClasePurificadora)
     { 
        ClasePurificadora exp = (ClasePurificadora)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(20,-1,true);
     }*/
     if (scUnidadMostrada.bonusdam_divino > 0)
     {
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(21, -1,true);
     }
    /*  if(scUnidadMostrada.barreraDeDanio > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(22,-1,true);
     }*/
      if(scUnidadMostrada.tejidoCuracMagica > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(23,-1,true);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 1)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(24,-1,true);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 2)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(25,-1,true);
     }
     /* if(scUnidadMostrada is ClaseCanalizador)
     { 
        ClaseCanalizador exp = (ClaseCanalizador)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(26,-1,true);
     }*/
      if(scUnidadMostrada.estado_Corrupto)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(27,-1,true);
     }
    }

}
