using System.Collections.Generic;
using UnityEngine;

public class PuntaHiriente : Habilidad
{
    private const string BuffNombreProvocado = "Provocado";
    private const string BuffNombreAdolorido = "Adolorido";

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;
    [SerializeField] private int tipoDanio;

    private Casilla origen;
    private readonly List<Unidad> lObjetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "Punta Hiriente";
        IDenClase = 7;
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
        cooldownMax = 5;
        bAfectaObstaculos = false;
        tipoPorcentaje = 1;

        bonusAtaque = 1;
        if (NIVEL > 1)
        {
            bonusAtaque++;
        }

        XdDanio = 1;
        daniodX = 10;
        criticoRangoHab = 0;
        tipoDanio = 2;
        penetracionArmadura = 5 + (NIVEL == 4 ? 3 : 0);

        imHab = Resources.Load<Sprite>("imHab/Duelista_PuntaHiriente");
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
        string nombreAdolorido = TRADU.i != null ? TRADU.i.Traducir(BuffNombreAdolorido) : BuffNombreAdolorido;
        var statsUI = ObtenerStatsDescripcionUI();

        int fuerzaActual = statsUI.Fuerza;
        int agilidadActual = statsUI.Agilidad;
        int atributoMixtoActual = ObtenerAtributoMixto(fuerzaActual, agilidadActual);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
        int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
        int duracionAdolorido = NIVEL == 5 ? 3 : 2;
        int dcBase = 7 + (NIVEL > 2 ? 1 : 0);
        int dcTotal = dcBase + agilidadActual;
        int penetracion = 5 + (NIVEL == 4 ? 3 : 0);
        string rangoDanio = FormatearRangoDados(1, 10, 2);

        string tituloEs = "Punta Hiriente I";
        string tituloEn = "Wounding Thrust I";
        string tituloPt = "Ponta Feridora I";
        if (NIVEL == 2) { tituloEs = "Punta Hiriente II"; tituloEn = "Wounding Thrust II"; tituloPt = "Ponta Feridora II"; }
        if (NIVEL == 3) { tituloEs = "Punta Hiriente III"; tituloEn = "Wounding Thrust III"; tituloPt = "Ponta Feridora III"; }
        if (NIVEL == 4) { tituloEs = "Punta Hiriente IV a"; tituloEn = "Wounding Thrust IV a"; tituloPt = "Ponta Feridora IV a"; }
        if (NIVEL == 5) { tituloEs = "Punta Hiriente IV b"; tituloEn = "Wounding Thrust IV b"; tituloPt = "Ponta Feridora IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string colorFuerza = "#d9822b";
        string colorAgilidad = "#7fa35a";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
        string atributo = esIngles
            ? $"<color={colorFuerza}>Strength</color>/<color={colorAgilidad}>Agility</color> ({atributoMixtoActual})"
            : esPortugues
                ? $"<color={colorFuerza}>Forca</color>/<color={colorAgilidad}>Agilidade</color> ({atributoMixtoActual})"
                : $"<color={colorFuerza}>Fuerza</color>/<color={colorAgilidad}>Agilidad</color> ({atributoMixtoActual})";
        string agilidadDC = esIngles
            ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
            : esPortugues
                ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
                : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
        string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy in melee range\n";
            cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
            cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
            cuerpo += $"<color={colorEncabezado}><b>Armor Penetration:</b></color> {penetracion}\n";
            cuerpo += $"<color={colorEncabezado}><b>On hit:</b></color> applies Provoked for 2 turns\n";
            cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Fortitude vs DC {dcBase} + {agilidadDC} = {dcTotal}\n";
            cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> applies {nombreAdolorido} for {duracionAdolorido} turns: -10% Damage, -2";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo em alcance melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
            cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracao de armadura:</b></color> {penetracion}\n";
            cuerpo += $"<color={colorEncabezado}><b>Ao acertar:</b></color> aplica Provocado por 2 turnos\n";
            cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Fortitude vs CD {dcBase} + {agilidadDC} = {dcTotal}\n";
            cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> aplica {nombreAdolorido} por {duracionAdolorido} turnos: -10% Dano, -2";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo en alcance melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
            cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracion de armadura:</b></color> {penetracion}\n";
            cuerpo += $"<color={colorEncabezado}><b>Al impactar:</b></color> aplica Provocado por 2 turnos\n";
            cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Fortaleza vs DC {dcBase} + {agilidadDC} = {dcTotal}\n";
            cuerpo += $"<color={colorEncabezado}><b>Si falla:</b></color> aplica {nombreAdolorido} por {duracionAdolorido} turnos: -10% Daño, -2";
        }

        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtitulo = esIngles
            ? "Hurts an enemy, provoking it and reducing its offense."
            : esPortugues
                ? "Perfurar um inimigo, provocando-o e reduzindo sua ofensiva."
                : "Hiere a un enemigo, provocándolo y reduciendo su ofensiva.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 roll bonus.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+3 Armor Penetration) or Option B (+1 Adolorido duration).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de rolagem.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 na CD da resistencia.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+3 Penetracao de armadura) ou Opcao B (+1 turno de Adolorido).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bonus de tirada.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al DC de la TS.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+3 Penetracion de armadura) u Opción B (+1 turno de Adolorido).</color>"; }
        }
    }


    private string TextoModificadorDescripcion(int valor)
    {
        if (valor > 0) { return $" + {valor}"; }
        if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
        return "";
    }
    public override void Activar()
    {
        origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
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
            ClaseDuelista duelista = scEstaUnidad as ClaseDuelista;
            int bonusDanioPorcentajeDanza = duelista != null ? duelista.ObtenerBonusDanioPorcentajeDanzaDelEstoque(objetivo) : 0;
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);

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
            AplicarProvocado(objetivo);

            int dc = 7 + Mathf.RoundToInt(scEstaUnidad.mod_CarAgilidad);
            if (NIVEL > 2)
            {
                dc++;
            }

            if (objetivo.TiradaSalvacion(1, dc))
            {
                AplicarAdolorido(objetivo);
            }
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    private void AplicarProvocado(Unidad objetivo)
    {
        if (objetivo == null)
        {
            return;
        }

        objetivo.RemoverBuffNombre(BuffNombreProvocado);

         Buff buff = new Buff();
        buff.buffNombre = BuffNombreProvocado;
        buff.buffDescr = string.Empty;
        buff.boolfDebufftBuff = false;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 2;
        buff.AplicarBuff(objetivo, scEstaUnidad);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }

    private void AplicarAdolorido(Unidad objetivo)
    {
        if (objetivo == null)
        {
            return;
        }

        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

        objetivo.RemoverBuffNombre(BuffNombreAdolorido);

        Buff buff = new Buff();
        buff.buffNombre = BuffNombreAdolorido;
        buff.buffDescr = esIngles
            ? "Deals -10% damage and -2 Attack."
            : esPortugues
                ? "Causa -10% de dano e -2 Ataque."
                : "Hace -10% de daño y -2 Ataque.";
        buff.boolfDebufftBuff = false;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = NIVEL == 5 ? 3 : 2;
        buff.cantDanioPorcentaje -= 10;
        buff.cantAtaque -= 2;
        buff.AplicarBuff(objetivo, scEstaUnidad);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }

    private void ObtenerObjetivos()
    {
        lObjetivosPosibles.Clear();

        if (origen == null || scEstaUnidad == null || BattleManager.Instance == null)
        {
            return;
        }

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

            if (c.Presente == null)
            {
                continue;
            }

            Unidad unidad = c.Presente.GetComponent<Unidad>();
            if (unidad == null || unidad.CasillaPosicion == null || scEstaUnidad.CasillaPosicion == null)
            {
                continue;
            }

            if (unidad.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
            {
                continue;
            }

            objetivosUnicos.Add(unidad);
        }

        lObjetivosPosibles.AddRange(objetivosUnicos);
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    }

    private int AumentarRangoMelee()
    {
        if (origen == null || origen.ladoOpuesto == null)
        {
            return 0;
        }

        int posYorigen = scEstaUnidad.CasillaPosicion.posY;

        List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
        List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

        foreach (Transform child in origen.ladoOpuesto.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();

            if (cas.posX == 3)
            {
                int calculo = Mathf.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna1.Add(cas);
                }
            }

            if (cas.posX == 2)
            {
                int calculo = Mathf.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna2.Add(cas);
                }
            }
        }

        foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
        {
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad))
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
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad))
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

    private void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PuntaHiriente");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }
}
