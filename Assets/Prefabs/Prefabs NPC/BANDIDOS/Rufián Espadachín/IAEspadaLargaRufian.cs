using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAEspadaLargaRufian : IAHabilidad
{
    private const int BonusAtaque = 1;
    private const int DadosDanio = 2;
    private const int CarasDanio = 8;
    private const int DanioPlano = 3;
    private const int TipoDanioCortante = 2;

    private void Awake()
    {
        nombre = "Tajo de Espada Larga";
        Usuario = gameObject;
        scEstaUnidad = GetComponent<Unidad>();
        hAncho = 1;
        hAlcance = 1;
        hCooldownMax = 0;
        hActualCooldown = 0;
        esMelee = true;
        esHostil = true;
        afectaObstaculos = true;
        costoAP = 3;
        prioridad = 1;
    }

    public override async Task ActivarHabilidad()
    {
        scEstaUnidad.CambiarAPActual(-costoAP);
        object objetivo = EstablecerObjetivoPrioritario();
        await PrepararInicioAnimacionConImpactoAsync(objetivo);
    }

    public override void AplicarEfectosHabilidad(object objetivo)
    {
        AplicarConTirada(objetivo, -1);
    }

    public void EjecutarContraataque(Unidad objetivo, int tiradaCompartida)
    {
        AplicarConTirada(objetivo, tiradaCompartida);
    }

    private void AplicarConTirada(object objetivo, int tiradaCompartida)
    {
        if (objetivo is Unidad unidadObjetivo)
        {
            int resultado = TiradaAtaque(
                unidadObjetivo.ObtenerdefensaActual(),
                scEstaUnidad.mod_CarFuerza,
                BonusAtaque,
                scEstaUnidad.mod_CriticoRangoDado,
                unidadObjetivo,
                tiradaCompartida);

            if (resultado <= 0)
            {
                unidadObjetivo.FalloAtaqueRecibido(scEstaUnidad, true);
                if (resultado < 0)
                {
                    scEstaUnidad.EstablecerAPActualA(0);
                }
                return;
            }

            float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioPlano;
            danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);
            if (resultado == 1) danio *= 0.5f;
            if (resultado == 3) danio += DanioPlano;

            unidadObjetivo.RecibirDanio(danio, TipoDanioCortante, resultado == 3, scEstaUnidad);
            unidadObjetivo.AplicarDebuffPorAtaquesreiterados(1);
            CrearVfx(unidadObjetivo.gameObject, "VFX/VFX_AtaqueEspadaCorta");
        }
        else if (objetivo is Obstaculo obstaculo)
        {
            float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioPlano;
            obstaculo.RecibirDanio(danio, TipoDanioCortante, false, scEstaUnidad);
        }
    }

    public override object EstablecerObjetivoPrioritario()
    {
        Unidad unidad = objPosibles.OfType<Unidad>()
            .OrderByDescending(x => x.CasillaPosicion.posX)
            .ThenBy(x => Mathf.Abs(x.CasillaPosicion.posY - scEstaUnidad.CasillaPosicion.posY))
            .FirstOrDefault();

        return unidad != null ? (object)unidad : objPosibles.OfType<Obstaculo>().FirstOrDefault();
    }

    private static void CrearVfx(GameObject objetivo, string ruta)
    {
        GameObject prefab = Resources.Load<GameObject>(ruta);
        if (prefab == null || objetivo == null) return;

        GameObject vfx = Object.Instantiate(prefab, objetivo.transform.position, Quaternion.identity, objetivo.transform);
        Canvas canvas = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvas, objetivo.transform, 5);
    }
}
