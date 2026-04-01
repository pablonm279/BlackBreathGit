using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Estocada : Habilidad
{
    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente

    Casilla Origen;
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    public override void Awake()
    {
        nombre = "Estocada";
        costoAP = 3;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 1;
        esCargable = false;
        esMelee = true;
        esHostil = true;
        cooldownMax = 0;
        bAfectaObstaculos = true;
        tipoPorcentaje = 1;
        bonusAtaque = 0;
        XdDanio = 1;
        daniodX = 8;
        tipoDanio = 2;
        criticoRangoHab = 0;
        penetracionArmadura = 2;

        imHab = Resources.Load<Sprite>("imHab/Duelista_Estocada");
        if (imHab == null)
        {
            imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        }

        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
        var statsUI = ObtenerStatsDescripcionUI();

        int fuerzaActual = statsUI.Fuerza;
        int agilidadActual = statsUI.Agilidad;
        int atributoMixtoActual = ObtenerAtributoMixto(fuerzaActual, agilidadActual);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee\n";
            cuerpo += "<b>Target:</b> 1 enemy in front range\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> + Attack ({ataqueActual}) + {bonusAtaque} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d8 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> | <b>Type:</b> Piercing\n";
            cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
            cuerpo += "<b>Scaling:</b> uses half Strength and half Agility, rounded up\n";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Alvo:</b> 1 inimigo em alcance frontal\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d8 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> | <b>Tipo:</b> Perfurante\n";
            cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
            cuerpo += "<b>Escala:</b> usa metade de Forca e metade de Agilidade, arredondando para cima\n";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Danio:</b> 1d8 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> | <b>Tipo:</b> Perforante\n";
            cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
            cuerpo += "<b>Escala:</b> usa mitad Fuerza y mitad Agilidad, redondeando hacia arriba\n";
        }

        string costos = esIngles
          ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
          : esPortugues
            ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
            : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
          esIngles ? "Thrust" : esPortugues ? "Estocada" : "Estocada",
          esIngles
            ? "A precise melee thrust that pierces armor, scales with both Strength and Agility."
            : esPortugues
              ? "Uma estocada corpo a corpo precisa que perfura armadura, escala com Forca e Agilidade."
              : "Una estocada melee precisa que perfora armadura, escala con Fuerza y Agilidad.",
          cuerpo,
          costos,
          "#5dade2");
    }

    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.5f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }
        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is Unidad)
        {
            Unidad objetivo = (Unidad)obj;
            EjecutarAtaqueEstocada(objetivo, tirada, bonusAtaque, 0, true);
        }
        else if (obj is Obstaculo)
        {
            int atributoMixto = ObtenerAtributoMixtoActual();
            Obstaculo objetivo = (Obstaculo)obj;
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

            objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        }
    }

    public void EjecutarRiposteContra(Unidad objetivo, int tirada, int bonusAtaqueAdicional, int bonusDanioPlano)
    {
        EjecutarAtaqueEstocada(objetivo, tirada, bonusAtaque + bonusAtaqueAdicional, bonusDanioPlano, false);
    }

    void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Estocada");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    private void ObtenerObjetivos()
    {
        lObjetivosPosibles.Clear();
        lObstaculosPosibles.Clear();

        int rangoPlus = 0;

        if (esMelee)
        {
            if (Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
            {
                rangoPlus = AumentarRangoMelee();
            }

            if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
            {
                rangoPlus++;
            }
        }

        List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(1 + rangoPlus, 1);

        foreach (Casilla c in lCasillasafectadas)
        {
            c.ActivarCapaColorRojo();
            if (esMelee && c.transform.GetChild(2).gameObject.activeInHierarchy)
            {
                c.DesactivarCapaColorRojo();
            }

            if (c.Presente == null)
            {
                continue;
            }

            if (c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
            {
                continue;
            }

            if (c.Presente.GetComponent<Unidad>() != null)
            {
                lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }

            if (c.Presente.GetComponent<Obstaculo>() != null)
            {
                lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());
            }
        }

        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
    }

    private int AumentarRangoMelee()
    {
        int posYorigen = scEstaUnidad.CasillaPosicion.posY;

        List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
        List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

        foreach (Transform child in Origen.ladoOpuesto.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();

            if (cas.posX == 3)
            {
                int calculo = Math.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna1.Add(cas);
                }
            }

            if (cas.posX == 2)
            {
                int calculo = Math.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna2.Add(cas);
                }
            }
        }

        foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
        {
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen))
            {
                return 0;
            }
        }
        foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna1)
        {
            casOsc.ActivarCapaColorNegro();
        }

        foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
        {
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen))
            {
                return 1;
            }
        }
        foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna2)
        {
            casOsc.ActivarCapaColorNegro();
        }

        return 2;
    }

    private void EjecutarAtaqueEstocada(Unidad objetivo, int tirada, int bonusAtaqueTotal, int bonusDanioPlano, bool gastarAPPorPifia)
    {
        if (objetivo == null)
        {
            return;
        }

        ClaseDuelista duelista = scEstaUnidad as ClaseDuelista;
        int bonusDanioPorcentajeDanza = duelista != null ? duelista.ObtenerBonusDanioPorcentajeDanzaDelEstoque(objetivo) : 0;
        int atributoMixto = ObtenerAtributoMixtoActual();
        float defensaObjetivo = objetivo.ObtenerdefensaActual();
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
        int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, atributoMixto, bonusAtaqueTotal, criticoRango, objetivo, 0);

        if (resultadoTirada == -1)
        {
            print("Pifia");
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            if (gastarAPPorPifia)
            {
                scEstaUnidad.EstablecerAPActualA(0);
            }
        }
        else if (resultadoTirada == 0)
        {
            print("Fallo");
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        }
        else if (resultadoTirada == 1)
        {
            print("Roce");
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);
            danio -= danio / 2;

            objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            VFXAplicar(objetivo.gameObject);
        }
        else if (resultadoTirada == 2)
        {
            print("Golpe");
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);

            objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            VFXAplicar(objetivo.gameObject);
        }
        else if (resultadoTirada == 3)
        {
            print("Critico");
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);

            objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
            VFXAplicar(objetivo.gameObject);
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    int TieneObstaculooUnidadAdelanteDeSuLado()
    {
        int orX = Origen.posX;
        int orY = Origen.posY;
        GameObject lado = Origen.ladoGO;

        if (orX != 2)
        {
            return 0;
        }

        Casilla casillaRevisar = null;
        foreach (Transform child in lado.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();
            if ((cas.posY == orY) && (cas.posX == orX + 1))
            {
                casillaRevisar = cas;
            }
        }

        if (casillaRevisar != null && casillaRevisar.Presente != null)
        {
            if (casillaRevisar.Presente.GetComponent<Unidad>() != null)
            {
                return 1;
            }

            if (casillaRevisar.Presente.GetComponent<Obstaculo>() != null)
            {
                if (casillaRevisar.Presente.GetComponent<Obstaculo>().bPermiteAtacarDetras)
                {
                    return 2;
                }
                return 0;
            }
        }
        return 0;
    }

    private int ObtenerAtributoMixtoActual()
    {
        return ObtenerAtributoMixto(scEstaUnidad.mod_CarFuerza, scEstaUnidad.mod_CarAgilidad);
    }

    private int ObtenerAtributoMixto(float fuerza, float agilidad)
    {
        return Mathf.CeilToInt((fuerza + agilidad) / 2f);
    }
}
