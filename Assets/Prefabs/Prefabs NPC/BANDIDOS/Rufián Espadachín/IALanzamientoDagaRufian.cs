using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IALanzamientoDagaRufian : IAHabilidad
{
    private const int BonusAtaque = -1;
    private const int DadosDanio = 1;
    private const int CarasDanio = 6;
    private const int DanioPlano = 3;
    private const int TipoDanioCortante = 2;

    private void Awake()
    {
        nombre = "Daga Rasante";
        Usuario = gameObject;
        scEstaUnidad = GetComponent<Unidad>();
        hAncho = 0;
        hAlcance = 6;
        hCooldownMax = 3;
        hActualCooldown = 0;
        esMelee = false;
        esHostil = true;
        afectaObstaculos = true;
        costoAP = 3;
        prioridad = 3;
    }

    public override async Task ActivarHabilidad()
    {
        scEstaUnidad.CambiarAPActual(-costoAP);
        hActualCooldown = hCooldownMax;

        List<object> objetivos = objPosibles
            .Where(x => x is Unidad || x is Obstaculo)
            .OrderByDescending(ObtenerPosX)
            .ToList();

        int tiradaCompartida = Random.Range(1, 21);
        await PrepararInicioAnimacionConImpactoAsync(objetivos, null, async () =>
        {
            await LanzarDagaPorFila(objetivos);
            foreach (object objetivo in objetivos)
            {
                AplicarConTirada(objetivo, tiradaCompartida);
            }
        });
    }

    private async Task LanzarDagaPorFila(List<object> objetivos)
    {
        if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return;
        }

        int fila = objetivos.Select(ObtenerCasilla).Where(x => x != null).Select(x => x.posY).FirstOrDefault();
        List<Casilla> casillasFila = BattleManager.Instance.lCasillasTotal
            .Where(x => x != null && x.lado != scEstaUnidad.CasillaPosicion.lado && x.posY == fila)
            .OrderByDescending(x => x.posX)
            .ToList();

        if (casillasFila.Count == 0)
        {
            return;
        }

        Vector3 inicio = casillasFila[0].transform.position;
        Vector3 fin = casillasFila[casillasFila.Count - 1].transform.position;
        Vector3 direccion = (fin - inicio).normalized;
        inicio -= direccion * 1.35f;
        fin += direccion * 1.1f;

        GameObject proyectil = new GameObject("Proyectil Daga Rasante");
        DagaRufianVuelo vuelo = proyectil.AddComponent<DagaRufianVuelo>();
        vuelo.Configurar(inicio, fin);
        await vuelo.EsperarFinalAsync();
    }

    public override void AplicarEfectosHabilidad(object objetivo)
    {
        AplicarConTirada(objetivo, Random.Range(1, 21));
    }

    private void AplicarConTirada(object objetivo, int tiradaCompartida)
    {
        if (objetivo is Unidad unidadObjetivo)
        {
            int resultado = TiradaAtaque(
                unidadObjetivo.ObtenerdefensaActual(),
                scEstaUnidad.mod_CarAgilidad,
                BonusAtaque,
                scEstaUnidad.mod_CriticoRangoDado,
                unidadObjetivo,
                tiradaCompartida);

            if (resultado <= 0)
            {
                unidadObjetivo.FalloAtaqueRecibido(scEstaUnidad, false);
                if (resultado < 0) scEstaUnidad.EstablecerAPActualA(0);
                return;
            }

            float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioPlano;
            danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);
            if (resultado == 1) danio *= 0.5f;
            if (resultado == 3) danio += 2;

            unidadObjetivo.RecibirDanio(danio, TipoDanioCortante, resultado == 3, scEstaUnidad);
            unidadObjetivo.AplicarDebuffPorAtaquesreiterados(1);
            CrearVfx(unidadObjetivo.gameObject);
        }
        else if (objetivo is Obstaculo obstaculo)
        {
            float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioPlano;
            obstaculo.RecibirDanio(danio, TipoDanioCortante, false, scEstaUnidad);
        }
    }

    public override object EstablecerObjetivoPrioritario()
    {
        return objPosibles.OrderByDescending(ObtenerPosX).FirstOrDefault();
    }

    private static int ObtenerPosX(object objetivo)
    {
        if (objetivo is Unidad unidad && unidad.CasillaPosicion != null) return unidad.CasillaPosicion.posX;
        if (objetivo is Obstaculo obstaculo && obstaculo.CasillaPosicion != null) return obstaculo.CasillaPosicion.posX;
        return int.MinValue;
    }

    private static Casilla ObtenerCasilla(object objetivo)
    {
        if (objetivo is Unidad unidad) return unidad.CasillaPosicion;
        if (objetivo is Obstaculo obstaculo) return obstaculo.CasillaPosicion;
        return null;
    }

    private static void CrearVfx(GameObject objetivo)
    {
        GameObject prefab = Resources.Load<GameObject>("VFX/VFX_AtaqueDaga");
        if (prefab == null || objetivo == null) return;

        GameObject vfx = Object.Instantiate(prefab, objetivo.transform.position, Quaternion.identity, objetivo.transform);
        Canvas canvas = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvas, objetivo.transform, 5);
    }
}
