using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Estocada : Habilidad
{
    [SerializeField] protected GameObject VFXenObjetivo;
    [SerializeField] protected int bonusAtaque;
    [SerializeField] protected int XdDanio;
    [SerializeField] protected int daniodX;
    [SerializeField] protected int criticoRangoHab;
    [SerializeField] protected int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente

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
        int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
        string rangoDanio = FormatearRangoDados(1, 8);
        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string colorFuerza = "#d9822b";
        string colorAgilidad = "#7fa35a";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}";
        string atributo = esIngles
            ? $"<color={colorFuerza}>Strength</color>/<color={colorAgilidad}>Agility</color> ({atributoMixtoActual})"
            : esPortugues
                ? $"<color={colorFuerza}>Forca</color>/<color={colorAgilidad}>Agilidade</color> ({atributoMixtoActual})"
                : $"<color={colorFuerza}>Fuerza</color>/<color={colorAgilidad}>Agilidad</color> ({atributoMixtoActual})";
        string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy or obstacle in front range\n";
            cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
            cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
            cuerpo += $"<color={colorEncabezado}><b>Armor Penetration:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>Scaling:</b></color> half Strength + half Agility, rounded up\n";
            cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo ou obstaculo em alcance frontal\n";
            cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
            cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracao de armadura:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>Escala:</b></color> metade de Forca + metade de Agilidade, arredondando para cima\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo u obstáculo en alcance frontal\n";
            cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
            cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracion de armadura:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>Escala:</b></color> mitad Fuerza + mitad Agilidad, redondeando hacia arriba\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
        }

        string titulo = esIngles ? "Thrust" : esPortugues ? "Estocada" : "Estocada";
        string subtitulo = esIngles
            ? "Precise melee thrust with armor penetration."
            : esPortugues
                ? "Estocada melee precisa com penetracao de armadura."
                : "Estocada melee precisa con penetracion de armadura.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;
    }


    private string TextoModificadorDescripcion(int valor)
    {
        if (valor > 0) { return $" + {valor}"; }
        if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
        return "";
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
        return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return base.EsperarPostImpactoAsync(objetivos, casillaOrigenTrampas);
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
            print("Crítico");
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
