using UnityEngine;

public class AtaqueBaculoLlamaSacra : GolpeBaston
{
  private const int TipoDanioFuego = 4;
  private const int TipoDanioDivino = 11;
  private const int BonusAtaqueBase = 1;
  private const int DadosFuegoCantidad = 1;
  private const int DadosFuegoCaras = 8;
  private const int DadosDivinoCantidad = 1;
  private const int DadosDivinoCaras = 4;
  private const int DadosBonusHerejesCantidad = 1;
  private const int DadosBonusHerejesCaras = 6;

  public override void Awake()
  {
    nombre = "Llama Sacra";
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
    tipoPorcentaje = 3;

    imHab = Resources.Load<Sprite>("imHab/Purificadora_LlamaDivina");

    if (TRADU.i.nIdioma == 2)
    {
      nombre = "Sacred Flame";
      txtDescripcion = "<color=#5dade2><b>Sacred Flame</b></color>\n\n";
      txtDescripcion += "<i>The staff erupts in sacred fire, scorching the enemy with divine judgment.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8><b>MELEE</b> -Attack: <color=#ea0606>Power +1</color> - Damage: Fire 1d8 + Divine 1d4- </color>\n";
      txtDescripcion += "<color=#c8c8c8><b>Weapon Effect:</b> Burning 1. Against Ethereal, Undead, or Corrupted targets: +1d6 Divine damage.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valour Cost: {costoPM} </color>";
      return;
    }

    if (TRADU.i.nIdioma == 3)
    {
      nombre = "Chama Sagrada";
      txtDescripcion = "<color=#5dade2><b>Chama Sagrada</b></color>\n\n";
      txtDescripcion += "<i>O cajado irrompe em fogo sacro, queimando o inimigo com julgamento divino.</i>\n\n";
      txtDescripcion += "<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Poder +1</color> - Dano: Fogo 1d8 + Divino 1d4- </color>\n";
      txtDescripcion += "<color=#c8c8c8><b>Efeito da arma:</b> Ardendo 1. Contra Etereo, Nomuerto ou Corrupto: +1d6 de dano Divino.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Recarga: {cooldownMax} \n- Custo AP: {costoAP} \n- Custo Valentia: {costoPM} </color>";
      return;
    }

    txtDescripcion = "<color=#5dade2><b>Llama Sacra</b></color>\n\n";
    txtDescripcion += "<i>El báculo estalla en fuego sacro y abrasa al enemigo con juicio divino.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Poder +1</color> - Daño: Fuego 1d8 + Divino 1d4- </color>\n";
    txtDescripcion += "<color=#c8c8c8><b>Efecto del arma:</b> Ardiendo 1. Contra Etéreo, Nomuerto o Corrupto: +1d6 de daño Divino.</color>\n\n";
    txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Valentía: {costoPM} </color>";
    ActualizarDescripcion();
  }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    StatsDescripcionUI stats = ObtenerStatsDescripcionUI();
    int criticoMin = Mathf.Clamp(19 - stats.CriticoRango, 2, 20);
    int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
    string bonusTirada = FormatoModificadorDescripcion(stats.Ataque) + FormatoModificadorDescripcion(BonusAtaqueBase);
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string colorPoder = "#2aa6c8";
    string atributo = esIngles
      ? $"<color={colorPoder}>Power ({stats.Poder})</color>"
      : esPortugues
        ? $"<color={colorPoder}>Poder ({stats.Poder})</color>"
        : $"<color={colorPoder}>Poder ({stats.Poder})</color>";
    string titulo = esIngles ? "Sacred Flame" : esPortugues ? "Chama Sagrada" : "Llama Sacra";
    string subtitulo = esIngles
      ? "Melee staff attack with Fire and Divine damage."
      : esPortugues
        ? "Ataque corpo a corpo de cajado com dano de Fogo e Divino."
        : "Ataque melee de baculo con dano de Fuego y Divino.";
    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee attack</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in frontal melee range</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defense. Fumble: 5%. Crit: {criticoPorcentaje}%</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>1-8 Fire + {atributo}; 1-4 Divine + half {atributo}. Type: Fire/Divine</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Weapon effect:</b></color> <color={colorValor}>Burning 1. Against Ethereal, Undead, or Corrupted: +1-6 Divine</color>";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque corpo a corpo</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstaculo no alcance frontal corpo a corpo</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defesa. Falha critica: 5%. Critico: {criticoPorcentaje}%</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>1-8 Fogo + {atributo}; 1-4 Divino + metade de {atributo}. Tipo: Fogo/Divino</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Efeito da arma:</b></color> <color={colorValor}>Ardendo 1. Contra Etereo, Morto-vivo ou Corrupto: +1-6 Divino</color>";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstáculo en alcance melee frontal</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defensa. Pifia: 5%. Crítico: {criticoPorcentaje}%</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>1-8 Fuego + {atributo}; 1-4 Divino + mitad de {atributo}. Tipo: Fuego/Divino</color>\n";
      cuerpo += $"<color={colorEncabezado}><b>Efecto del arma:</b></color> <color={colorValor}>Ardiendo 1. Contra Etéreo, Nomuerto o Corrupto: +1-6 Divino</color>";
    }

    txtDescripcion = ConstruirDescripcionTooltipNueva(titulo, subtitulo, cuerpo);
  }

  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {
    if (obj is Unidad objetivo)
    {
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      float criticoRango = scEstaUnidad.mod_CriticoRangoDado;
      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, BonusAtaqueBase, criticoRango, objetivo, 0);

      if (resultadoTirada == -1)
      {
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        scEstaUnidad.EstablecerAPActualA(0);
        return;
      }

      if (resultadoTirada == 0)
      {
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        return;
      }

      bool esCritico = resultadoTirada == 3;
      bool esRoce = resultadoTirada == 1;

      float danioFuego = TiradaDeDados.TirarDados(DadosFuegoCantidad, DadosFuegoCaras) + scEstaUnidad.mod_CarPoder;
      float danioDivino = TiradaDeDados.TirarDados(DadosDivinoCantidad, DadosDivinoCaras) + Mathf.Max(1, Mathf.RoundToInt(scEstaUnidad.mod_CarPoder / 2f));

      danioFuego = danioFuego / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
      danioDivino = danioDivino / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      if (esRoce)
      {
        danioFuego -= danioFuego / 2f;
        danioDivino -= danioDivino / 2f;
      }

      objetivo.RecibirDanio(danioFuego, TipoDanioFuego, esCritico, scEstaUnidad);
      objetivo.RecibirDanioSinBonusElemental(danioDivino, TipoDanioDivino, esCritico, scEstaUnidad);

      if (EsObjetivoProfano(objetivo))
      {
        float danioBonus = TiradaDeDados.TirarDados(DadosBonusHerejesCantidad, DadosBonusHerejesCaras);
        if (esRoce)
        {
          danioBonus -= danioBonus / 2f;
        }

        objetivo.RecibirDanioSinBonusElemental(danioBonus, TipoDanioDivino, esCritico, scEstaUnidad);
      }

      Estados.Aplicar_Ardiendo(objetivo, esCritico ? 2 : 1, scEstaUnidad);
      objetivo.AplicarDebuffPorAtaquesreiterados(1);
      return;
    }

    if (obj is Obstaculo obstaculo)
    {
      float danio = TiradaDeDados.TirarDados(DadosFuegoCantidad, DadosFuegoCaras) + scEstaUnidad.mod_CarPoder;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
      obstaculo.RecibirDanio(danio, TipoDanioFuego, false, scEstaUnidad);
    }
  }

  private static bool EsObjetivoProfano(Unidad objetivo)
  {
    return objetivo.TieneTag("Etereo")
      || objetivo.TieneTag("Nomuerto")
      || objetivo.TieneTag("Corrupto");
  }
}
