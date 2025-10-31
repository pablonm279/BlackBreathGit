using UnityEngine;

public class RevisarEncarnado : MonoBehaviour
{
    [SerializeField] private GameObject graficoFuegoFatuo;
    private Unidad unidad;

    void Awake()
    {
        unidad = GetComponent<Unidad>();

        if (graficoFuegoFatuo == null)
        {
            graficoFuegoFatuo = BuscarGraficoPorDefecto();
        }
    }

    void Update()
    {
        if (unidad == null || graficoFuegoFatuo == null)
        {
            return;
        }

        if (unidad.estado_aturdido <= 0 && !graficoFuegoFatuo.activeSelf)
        {
            graficoFuegoFatuo.SetActive(true);
        }
    }

    private GameObject BuscarGraficoPorDefecto()
    {
        if (transform.childCount <= 3)
        {
            return null;
        }

        Transform contenedorVisual = transform.GetChild(3);

        if (contenedorVisual.childCount <= 1)
        {
            return null;
        }

        return contenedorVisual.GetChild(1).gameObject;
    }
}
