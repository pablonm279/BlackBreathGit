using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AFondo : Habilidad
{
    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int bonusDanioPlano;
    [SerializeField] private int criticoRangoHab;
    [SerializeField] private int tipoDanio;
    [SerializeField] private int dcFortitud;
    [SerializeField] private int sangradoAplicado;

    private Casilla origen;
    private readonly List<Unidad> lObjetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "A Fondo";
        IDenClase = 5;
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
        cooldownMax = 2;
        bAfectaObstaculos = false;
        tipoPorcentaje = 1;
        targetEspecial = 10;

        bonusAtaque = 2;
        XdDanio = 1;
        daniodX = 10;
        bonusDanioPlano = NIVEL > 1 ? 5 : 3;
        criticoRangoHab = NIVEL == 5 ? 1 : 0;
        tipoDanio = 2; // Perforante
        dcFortitud = 12;
        sangradoAplicado = NIVEL == 4 ? 5 : 3;
        penetracionArmadura = NIVEL > 2 ? 2 : 1;

        imHab = Resources.Load<Sprite>("imHab/Duelista_A_Fondo");
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

        int atributoMixtoActual = ObtenerAtributoMixto(statsUI.Fuerza, statsUI.Agilidad);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

        string tituloEs = "A Fondo I";
        string tituloEn = "All In I";
        string tituloPt = "A Fondo I";
        if (NIVEL == 2) { tituloEs = "A Fondo II"; tituloEn = "All In II"; tituloPt = "A Fondo II"; }
        if (NIVEL == 3) { tituloEs = "A Fondo III"; tituloEn = "All In III"; tituloPt = "A Fondo III"; }
        if (NIVEL == 4) { tituloEs = "A Fondo IV a"; tituloEn = "All In IV a"; tituloPt = "A Fondo IV a"; }
        if (NIVEL == 5) { tituloEs = "A Fondo IV b"; tituloEn = "All In IV b"; tituloPt = "A Fondo IV b"; }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee\n";
            cuerpo += "<b>Target:</b> Selected enemy tile and the 2 rear diagonal tiles\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> + Attack ({ataqueActual}) + {bonusAtaque} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d10 + {bonusDanioPlano} | <b>Type:</b> Piercing\n";
            cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
            cuerpo += $"{ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcFortitud)}\n";
            cuerpo += $"<b>On failed save (if damaged):</b> +{sangradoAplicado} Bleed";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Corpo a corpo\n";
            cuerpo += "<b>Alvo:</b> Casa inimiga escolhida e as 2 diagonais de tras\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d10 + {bonusDanioPlano} | <b>Tipo:</b> Perfurante\n";
            cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"{ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcFortitud)}\n";
            cuerpo += $"<b>Se falhar na resistencia (se causar dano):</b> +{sangradoAplicado} Sangramento";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Objetivo:</b> Casilla enemiga elegida y las 2 diagonales de atras\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Danio:</b> 1d10 + {bonusDanioPlano} | <b>Tipo:</b> Perforante\n";
            cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"{ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcFortitud)}\n";
            cuerpo += $"<b>Si falla TS (si recibe danio):</b> +{sangradoAplicado} Sangrado";
        }

        string costos = esIngles
          ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
          : esPortugues
            ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
            : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
          esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
          esIngles
            ? "A committed thrust that pierces the target and threatens the rear diagonals."
            : esPortugues
              ? "Uma estocada comprometida que perfura o alvo e pressiona as diagonais de tras."
              : "Una estocada comprometida que perfora el objetivo y amenaza las diagonales traseras.",
          cuerpo,
          costos,
          "#5dade2");

        bool mostrarProximoNivel = CampaignManager.Instance != null
          && CampaignManager.Instance.scMenuPersonajes != null
          && CampaignManager.Instance.scMenuPersonajes.pSel != null
          && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 armor penetration.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Bleed) or Option B (+1 crit range).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 de penetracao de armadura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Sangramento) ou Opcao B (+1 faixa de critico).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de danio.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 de penetracion de armadura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 Sangrado) u Opcion B (+1 rango critico).</color>"; }
        }
    }

    public override void Activar()
    {
        origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
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
        if (obj is not Unidad objetivo)
        {
            return;
        }

        int atributoMixto = ObtenerAtributoMixtoActual();
        float defensaObjetivo = objetivo.ObtenerdefensaActual();
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
        int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, atributoMixto, bonusAtaque, criticoRango, objetivo, 0);

        if (resultadoTirada == -1)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            scEstaUnidad.EstablecerAPActualA(0);
        }
        else if (resultadoTirada == 0)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        }
        else
        {
            float hpAntes = objetivo.HP_actual;
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

            if (resultadoTirada == 1)
            {
                danio -= danio / 2f;
                objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            }
            else if (resultadoTirada == 2)
            {
                objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            }
            else if (resultadoTirada == 3)
            {
                objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
            }

            VFXAplicar(objetivo.gameObject);
            if (objetivo.HP_actual < hpAntes && objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, dcFortitud))
            {
                Estados.Aplicar_Sangrado(objetivo, sangradoAplicado, scEstaUnidad);
            }
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    private void ObtenerObjetivos()
    {
        lObjetivosPosibles.Clear();
        lCasillasafectadas.Clear();
        HashSet<Unidad> objetivosUnicos = new HashSet<Unidad>();

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

        List<Casilla> casillasPrimarias = origen.ObtenerCasillasRango(1 + rangoPlus, 1);
        foreach (Casilla c in casillasPrimarias)
        {
            c.ActivarCapaColorRojo();
            if (esMelee && c.transform.GetChild(2).gameObject.activeInHierarchy)
            {
                c.DesactivarCapaColorRojo();
            }

            lCasillasafectadas.Add(c);
            AgregarUnidadSiExiste(c, objetivosUnicos);

            int posXAtras = c.posX - 1;
            AgregarUnidadSiExiste(ObtenerCasillaPorPos(c.ladoGO, posXAtras, c.posY - 1), objetivosUnicos);
            AgregarUnidadSiExiste(ObtenerCasillaPorPos(c.ladoGO, posXAtras, c.posY + 1), objetivosUnicos);
        }

        lObjetivosPosibles.AddRange(objetivosUnicos);
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    }

    private void AgregarUnidadSiExiste(Casilla casilla, HashSet<Unidad> objetivosUnicos)
    {
        if (casilla == null || casilla.Presente == null)
        {
            return;
        }

        Unidad unidad = casilla.Presente.GetComponent<Unidad>();
        if (unidad == null || unidad.CasillaPosicion == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return;
        }

        if (unidad.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
        {
            return;
        }

        objetivosUnicos.Add(unidad);
    }

    private Casilla ObtenerCasillaPorPos(GameObject ladoGO, int posX, int posY)
    {
        if (ladoGO == null)
        {
            return null;
        }

        foreach (Transform child in ladoGO.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();
            if (cas != null && cas.posX == posX && cas.posY == posY)
            {
                return cas;
            }
        }

        return null;
    }

    private void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AtaqueDaga");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;
        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    private int AumentarRangoMelee()
    {
        int posYorigen = scEstaUnidad.CasillaPosicion.posY;
        List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
        List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

        foreach (Transform child in origen.ladoOpuesto.transform)
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

    private int TieneObstaculooUnidadAdelanteDeSuLado()
    {
        int orX = origen.posX;
        int orY = origen.posY;
        GameObject lado = origen.ladoGO;

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

            Obstaculo obstaculo = casillaRevisar.Presente.GetComponent<Obstaculo>();
            if (obstaculo != null && obstaculo.bPermiteAtacarDetras)
            {
                return 2;
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
