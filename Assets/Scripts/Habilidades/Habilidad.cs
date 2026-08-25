using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Reflection;
using System.Text.RegularExpressions;

public abstract class Habilidad : MonoBehaviour
{
  public string nombre;
  public int costoAP;

  public GameObject vfxImpacto;
  public GameObject vfxCasteo;

  public int requiereRecurso; // Habilidades que requieren un recurso externo para funcionar, ej flechas.

  public int costoPM;
  [System.NonSerialized] public bool sinCooldownDebug;
  private int cooldownMaxBase;
  private int cooldownActualBase;
  public int cooldownMax
  {
    get { return sinCooldownDebug ? 0 : cooldownMaxBase; }
    set { cooldownMaxBase = value; }
  }
  public int cooldownActual
  {
    get { return sinCooldownDebug ? 0 : cooldownActualBase; }
    set { cooldownActualBase = sinCooldownDebug ? 0 : value; }
  }
  public GameObject Usuario;
  public Unidad scEstaUnidad;

  public int NIVEL;
  public int IDenClase; //Del 1 al 10, que ID tiene esta habilidad en la clase

  public Item agregaDesdeArmaUI = null;
  public string txtDescripcion;
  public Sprite imHab;

  public List<Casilla> lCasillasafectadas = new List<Casilla>();
  public bool esZonal; //true si afecta a todas las unidades en el rango, false si es individual.

  //TARGETEO ESPECIAL - 1: Misma Fila  - 2: Misma Columna - 3: Dos Casillas (Vertical) - 4: Tres Casillas (Vertical) - 5: Dos Casillas (Atrás)
  public int targetEspecial = 0;  //1: Misma fila  2: Misma Columna 3: Dos Casillas (Vert) 4: Tres Casillas (Vert)5: Dos Casillas (Atras) 
                                  //6: 3 casillas Vertical y las de atras
                                  //10: V atras (dos diagonales)
  public int enArea = 0; //Si este valor es mayor a 0, permite tarjetear celdas, afectando a unidades alrededor. 1, cruz, 2 cuadrado, 3 todo
  public int esforzable; //Hasta cuantos AP de su costo permite "deber"
  public bool esCargable; //Si no alcanzan los AP del turno, se castea otro turno cuando se paguen todos.
  public bool esMelee;
  public bool bAfectaObstaculos;
  [Header("Animacion")]
  [SerializeField] public bool fuerzaPoseAtaque = false;
  [SerializeField] public bool forzarPoseHabilidad = false;
  [SerializeField] public bool omitirAnimacionDeUso = false;
  [Header("Camara")]
  [SerializeField, Tooltip("Si esta activo, esta habilidad no dispara el zoom/paneo de camara al resolverse.")]
  public bool omitirFocoCamara = false;
  [SerializeField, Range(0.15f, 2f), Tooltip("Multiplicador del foco de camara para esta habilidad. 1 usa el valor global; menor es mas sutil; mayor es mas dramatico.")]
  public float intensidadFocoCamara = 1f;

  public bool poneTrampas; //Si la habilidad pone trampas 
  public bool poneObstaculo; //Si la habilidad pone obstaculo 

  public bool esHostil; //Si es para enemigos o aliados
  public bool esDiscreta = false; //No quita sigilo

  [Tooltip("0=Sin mostrar probabilidad, 1=Ataque melee (Fuerza), 2=Ataque rango (Agilidad), 3=Ataque magia (Poder)")]
  public int tipoPorcentaje = 0;
  [Header("Ataque")]
  [Tooltip("Penetracion plana de armadura para esta habilidad. Solo aplica al resolver este ataque.")]
  public int penetracionArmadura = 0;

  protected const string ColorDescripcionMecanica = "#c8c8c8";
  protected const string ColorDescripcionCostos = "#44d3ec";
  protected const string ObjetivoMeleeUnitarioIngles = "1 target or obstacle in melee range";

  private const string ColorNumeroNeutro = "#e0b567";
  private const string ColorNumeroBeneficio = "#9fcf7a";
  private const string ColorNumeroPerjuicio = "#e0987a";
  private const string ColorConceptoJuego = "#b9c6cc";
  private const string ColorAtributoFuerza = "#d9822b";
  private const string ColorAtributoAgilidad = "#7fa35a";
  private const string ColorAtributoPoder = "#2aa6c8";
  private const string ColorSalvacionMental = "#c9a8e0";
  private const string ColorSalvacionFortaleza = "#e0c090";
  private const string ColorSalvacionReflejos = "#a3e0c2";
  private const string ColorPuntosAccion = "#c2387a";

  private static readonly string[] PalabrasClaveBeneficio =
  {
    "restaura", "restauran", "restore", "restores", "cura", "curan", "heal", "heals",
    "recupera", "recuperan", "recover", "recovers", "otorga", "otorgan", "concede", "conceden",
    "grant", "grants", "gana", "ganan", "gain", "gains", "regenera", "regeneran", "regenerate", "regenerates",
    "concedem", "ganha", "ganham"
  };

  private static readonly string[] PalabrasClavePerjuicio =
  {
    "sufre", "sufren", "suffer", "suffers", "pierde", "pierden", "lose", "loses",
    "daño", "daños", "damage", "dano", "danos", "sofre", "sofrem", "perde", "perdem"
  };

  // Orden: frases de varias palabras antes que sus palabras sueltas, para que la alternancia del
  // regex combinado consuma la frase completa como un solo match (evita doble coloreado anidado,
  // por ejemplo "Armor" dentro de "Armor Penetration").
  // NombreGrupo es el nombre del grupo del regex (debe ser un identificador valido, sin guiones).
  // IdTermino es el id canonico que se busca en CatalogoTerminosJuego (coincide con TerminoDescripcionId).
  private static readonly (string NombreGrupo, string Patron, string IdTermino, string ColorPorDefecto)[] TerminosColoreadosDescripcion =
  {
    ("armorPen", @"\bArmor Penetration\b|\bPenetraci[oó]n de Armadura\b|\bPenetra[cç][aã]o de Armadura\b", "armor-penetration", ColorConceptoJuego),
    ("trueDamage", @"\bTrue damage\b|\bDa[nñ]o Verdadero\b|\bDano Verdadeiro\b", "true-damage", ColorConceptoJuego),
    ("criticalHit", @"\bCritical Hit\b|\bGolpe Cr[ií]tico\b", "critical-hit", ColorConceptoJuego),
    ("fumble", @"\bFumble\b|\bPifia\b|\bFalha [Cc]r[ií]tica\b", "fumble", ColorConceptoJuego),
    ("strength", @"\bStrength\b|\bFuerza\b|\bForça\b", "strength", ColorAtributoFuerza),
    ("agility", @"\bAgility\b|\bAgilidad\b|\bAgilidade\b", "agility", ColorAtributoAgilidad),
    ("power", @"\bPower\b|\bPoder\b", "power", ColorAtributoPoder),
    ("salvacionMental", @"\bMental\b", "mental-save", ColorSalvacionMental),
    ("salvacionFortaleza", @"\bFortitude\b|\bFortaleza\b", "fortitude-save", ColorSalvacionFortaleza),
    ("salvacionReflejos", @"\bReflex\b|\bReflejos\b|\bReflexos\b", "reflex-save", ColorSalvacionReflejos),
    ("valour", @"\bValour\b|\bValent[ií]a\b", "valour", ColorConceptoJuego),
    ("defense", @"\bDefense\b|\bDefensa\b|\bDefesa\b", "defense", ColorConceptoJuego),
    ("attack", @"\bAttack\b|\bAtaque\b", "attack", ColorConceptoJuego),
    ("ap", @"\bAP\b", "ap", ColorPuntosAccion),
    ("barrier", @"\bBarrier\b|\bBarrera\b", "barrier", ColorConceptoJuego),
    ("fervor", @"\bFervor\b", "fervor", ColorConceptoJuego),
    ("evasion", @"\bEvasion\b|\bEvasi[oó]n\b|\bEvas[aã]o\b", "evasion", ColorConceptoJuego),
    ("weakness", @"\bWeakness\b|\bDebilidad\b|\bFraqueza\b", "weakness", ColorConceptoJuego),
    ("debuff", @"\bDebuff\b", "debuff", ColorConceptoJuego),
    ("armor", @"\bArmor\b|\bArmadura\b", "armor", ColorConceptoJuego),
    ("reaction", @"\bReaction\b|\bReacci[oó]n\b|\bRea[cç][aã]o\b", "reaction", ColorConceptoJuego),
    ("impulse", @"\bImpulse\b|\bImpulso\b", "impulse", ColorConceptoJuego),
    ("divine", @"\bDivine\b|\bDivino\b|\bDivina\b", "divine-damage", "#e8cf7a"),
    ("fire", @"\bFire\b|\bFuego\b|\bFogo\b", "fire-damage", "#e0987a"),
    ("cold", @"\bCold\b|\bFr[ií]o\b|\bFria\b", "cold-damage", "#8ecbe0"),
    ("lightning", @"\bLightning\b|\bRayo\b|\bRel[aâ]mpago\b", "lightning-damage", "#e0d67a"),
    ("acid", @"\bAcid\b|\b[AÁ]cido\b|\b[AÁ]cida\b", "acid-damage", "#a8d67a"),
    ("arcane", @"\bArcane\b|\bArcano\b|\bArcana\b", "arcane-damage", "#b98ee0"),
    ("slashing", @"\bSlashing\b|\bCortante\b", "slashing-damage", ColorConceptoJuego),
    ("piercing", @"\bPiercing\b|\bPerforante\b", "piercing-damage", ColorConceptoJuego),
    ("bludgeoning", @"\bBludgeoning\b|\bContundente\b", "bludgeoning-damage", ColorConceptoJuego),
    ("poison", @"\bPoison\b|\bVeneno\b", "poison", "#a8d67a"),
  };

  private static readonly Regex RegexTerminosCombinado = new Regex(
    string.Join("|", Array.ConvertAll(TerminosColoreadosDescripcion, t => $"(?<{t.NombreGrupo}>{t.Patron})")),
    RegexOptions.Compiled);

  private static readonly Dictionary<string, (string IdTermino, string ColorPorDefecto)> DefinicionPorNombreGrupo = BuildDefinicionPorNombreGrupo();

  private static Dictionary<string, (string IdTermino, string ColorPorDefecto)> BuildDefinicionPorNombreGrupo()
  {
    var mapa = new Dictionary<string, (string, string)>();
    foreach ((string nombreGrupo, string _, string idTermino, string colorPorDefecto) in TerminosColoreadosDescripcion)
    {
      mapa[nombreGrupo] = (idTermino, colorPorDefecto);
    }

    return mapa;
  }

  private static string ObtenerColorTermino(string idTermino, string colorPorDefecto)
  {
    CatalogoTerminosJuego catalogo = CatalogoTerminosJuego.Instancia;
    TerminoJuegoDefinicion termino = catalogo != null ? catalogo.ObtenerPorId(idTermino) : null;
    return termino != null && !string.IsNullOrEmpty(termino.colorHex) ? termino.colorHex : colorPorDefecto;
  }

  private static readonly Regex RegexNumeroDescripcion = new Regex(
    @"(?<![\w.#])[+-]?\d+-\d+|(?<![\w.#])[+-]?\d+%|(?<![\w.#])[+-]?\d+(?!\w)",
    RegexOptions.Compiled);

  // Notacion de dados (ej: 1d20, 2d6). Se procesa aparte del resto de los numeros para
  // poder darle a "1d20" un hover propio (Ataques/Salvaciones) y a las demas combinaciones
  // una descripcion generica armada en el momento (ver TerminoHoverDetector.GenerarDescripcionDado).
  private static readonly Regex RegexDadoDescripcion = new Regex(@"(?<![\w.#])(\d+)d(\d+)\b", RegexOptions.Compiled);

  private static readonly Regex RegexClausulaDescripcion = new Regex(@"[^.;]+[.;]?", RegexOptions.Compiled);

  private static readonly Regex RegexEtiquetaTmp = new Regex(@"<[^>]*>", RegexOptions.Compiled);

  // Aplica una transformación solo al texto visible, dejando intactas las etiquetas TMP
  // (<color=...>, <sprite ...>, <link=...>, etc.) para no corromper su sintaxis.
  private static string TransformarTextoVisible(string valor, Func<string, string> transformador)
  {
    if (string.IsNullOrEmpty(valor))
    {
      return valor;
    }

    var resultado = new System.Text.StringBuilder();
    int posicion = 0;
    foreach (Match etiqueta in RegexEtiquetaTmp.Matches(valor))
    {
      if (etiqueta.Index > posicion)
      {
        resultado.Append(transformador(valor.Substring(posicion, etiqueta.Index - posicion)));
      }

      resultado.Append(etiqueta.Value);
      posicion = etiqueta.Index + etiqueta.Length;
    }

    if (posicion < valor.Length)
    {
      resultado.Append(transformador(valor.Substring(posicion)));
    }

    return resultado.ToString();
  }

  // Variante para resaltadores que crean links: conserva intacto el contenido de
  // cualquier <link> ya presente y asi evita anidarlos.
  private static string TransformarTextoVisibleFueraDeLinks(string valor, Func<string, string> transformador)
  {
    var resultado = new System.Text.StringBuilder();
    int posicion = 0;
    int profundidadLink = 0;
    foreach (Match etiqueta in RegexEtiquetaTmp.Matches(valor))
    {
      if (etiqueta.Index > posicion)
      {
        string segmento = valor.Substring(posicion, etiqueta.Index - posicion);
        resultado.Append(profundidadLink == 0 ? transformador(segmento) : segmento);
      }

      string textoEtiqueta = etiqueta.Value;
      if (textoEtiqueta.StartsWith("</link", StringComparison.OrdinalIgnoreCase))
      {
        profundidadLink = Math.Max(0, profundidadLink - 1);
      }

      resultado.Append(textoEtiqueta);
      if (textoEtiqueta.StartsWith("<link", StringComparison.OrdinalIgnoreCase))
      {
        profundidadLink++;
      }

      posicion = etiqueta.Index + etiqueta.Length;
    }

    if (posicion < valor.Length)
    {
      string segmento = valor.Substring(posicion);
      resultado.Append(profundidadLink == 0 ? transformador(segmento) : segmento);
    }

    return resultado.ToString();
  }

  // Busca el primer ':' que esté en texto visible, ignorando los que aparezcan dentro de
  // atributos de etiquetas TMP (por ejemplo el de <link="skill-term:armor-penetration">).
  private static int IndiceDePrimerDosPuntosVisible(string valor)
  {
    bool dentroDeEtiqueta = false;
    for (int i = 0; i < valor.Length; i++)
    {
      char c = valor[i];
      if (c == '<')
      {
        dentroDeEtiqueta = true;
      }
      else if (c == '>')
      {
        dentroDeEtiqueta = false;
      }
      else if (!dentroDeEtiqueta && c == ':')
      {
        return i;
      }
    }

    return -1;
  }

  private static string EliminarPrefijoRedundante(string etiqueta, string valor)
  {
    if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(etiqueta))
    {
      return valor;
    }

    int indiceDosPuntos = IndiceDePrimerDosPuntosVisible(valor);
    if (indiceDosPuntos <= 0)
    {
      return valor;
    }

    string prefijoCrudo = valor.Substring(0, indiceDosPuntos);
    string prefijoVisible = RegexEtiquetaTmp.Replace(prefijoCrudo, string.Empty).Trim();
    if (prefijoVisible.Length == 0 || prefijoVisible.Length > etiqueta.Length + 20)
    {
      return valor;
    }

    if (prefijoVisible.IndexOf(etiqueta, StringComparison.OrdinalIgnoreCase) < 0)
    {
      return valor;
    }

    return valor.Substring(indiceDosPuntos + 1).TrimStart();
  }

  private static string ResaltarTerminosDescripcion(string valor)
  {
    if (string.IsNullOrEmpty(valor))
    {
      return valor;
    }

    // Los terminos creados con TerminoDescripcion ya traen su propio <link>. Evitar
    // insertar otro dentro: TMP no resuelve correctamente links anidados y puede
    // asociar el hover al termino anterior.
    return TransformarTextoVisibleFueraDeLinks(valor, segmento => RegexTerminosCombinado.Replace(segmento, m =>
    {
      foreach (Group grupo in m.Groups)
      {
        if (grupo.Success && DefinicionPorNombreGrupo.TryGetValue(grupo.Name, out (string IdTermino, string ColorPorDefecto) definicion))
        {
          string color = ObtenerColorTermino(definicion.IdTermino, definicion.ColorPorDefecto);
          // Se envuelve tambien en un <link="skill-term:id"> para que TerminoHoverDetector
          // pueda mostrar la definicion del termino al pasar el mouse (ver TerminoHoverPopup).
          return $"<link=\"skill-term:{definicion.IdTermino}\"><color={color}>{m.Value}</color></link>";
        }
      }

      return m.Value;
    }));
  }

  private static string ResaltarNumerosDescripcion(string valor)
  {
    if (string.IsNullOrEmpty(valor))
    {
      return valor;
    }

    return TransformarTextoVisible(valor, segmento =>
    {
      var resultado = new System.Text.StringBuilder();
      foreach (Match clausula in RegexClausulaDescripcion.Matches(segmento))
      {
        string texto = clausula.Value;
        string textoMinuscula = texto.ToLowerInvariant();
        string colorNumero = ColorNumeroNeutro;
        if (Array.Exists(PalabrasClaveBeneficio, palabra => textoMinuscula.Contains(palabra)))
        {
          colorNumero = ColorNumeroBeneficio;
        }
        else if (Array.Exists(PalabrasClavePerjuicio, palabra => textoMinuscula.Contains(palabra)))
        {
          colorNumero = ColorNumeroPerjuicio;
        }

        resultado.Append(RegexNumeroDescripcion.Replace(texto, m => $"<color={colorNumero}>{m.Value}</color>"));
      }

      return resultado.ToString();
    });
  }

  private static string ResaltarDados(string valor)
  {
    if (string.IsNullOrEmpty(valor))
    {
      return valor;
    }

    return TransformarTextoVisible(valor, segmento => RegexDadoDescripcion.Replace(segmento, m =>
    {
      string cantidad = m.Groups[1].Value;
      string caras = m.Groups[2].Value;
      string idTermino = cantidad == "1" && caras == "20" ? "d20-roll" : $"dice:{cantidad}:{caras}";
      return $"<link=\"skill-term:{idTermino}\"><color={ColorNumeroNeutro}>{m.Value}</color></link>";
    }));
  }

  private static string ResaltarValorDescripcion(string etiqueta, string valor)
  {
    string valorLimpio = EliminarPrefijoRedundante(etiqueta, valor);
    valorLimpio = ResaltarTerminosDescripcion(valorLimpio);
    valorLimpio = ResaltarDados(valorLimpio);
    valorLimpio = ResaltarNumerosDescripcion(valorLimpio);
    return valorLimpio;
  }

  public virtual void ActualizarPreviewCasilla(Casilla casilla)
  {
  }

  public virtual void LimpiarPreviewCasilla()
  {
  }

  private readonly HashSet<Unidad> unidadesPreviewDanio = new HashSet<Unidad>();
  private bool rangoPreviewBaseCacheValido;
  private float danioBasePreviewMinimo;
  private float danioBasePreviewMaximo;
  private int tipoDanioPreview;

  private bool TryObtenerRangoDanioPreview(Unidad objetivo, out int danioMinimo, out int danioMaximo)
  {
    danioMinimo = 0;
    danioMaximo = 0;

    if (!esHostil || scEstaUnidad == null)
    {
      return false;
    }

    if (!rangoPreviewBaseCacheValido)
    {
      if (!TryObtenerCampoEntero("XdDanio", out int cantidadDados)
        || !TryObtenerCampoEntero("daniodX", out int carasDado)
        || !TryObtenerCampoEntero("tipoDanio", out int tipoDanio)
        || cantidadDados <= 0
        || carasDado <= 0
        || tipoDanio <= 0)
      {
        return false;
      }

      ActualizarDescripcion();
      int baseMinimo = cantidadDados;
      int baseMaximo = cantidadDados * carasDado;
      string descripcionVisible = Regex.Replace(txtDescripcion ?? string.Empty, "<[^>]+>", string.Empty);
      Match rango = Regex.Match(descripcionVisible, @"(?<!\d)(-?\d+)\s*[-\u2013]\s*(-?\d+)(?!\d)");
      if (rango.Success
        && int.TryParse(rango.Groups[1].Value, out int rangoMinimo)
        && int.TryParse(rango.Groups[2].Value, out int rangoMaximo))
      {
        baseMinimo = rangoMinimo;
        baseMaximo = rangoMaximo;

        int finClausula = descripcionVisible.IndexOfAny(new[] { '.', '\n' }, rango.Index + rango.Length);
        if (finClausula < 0) { finClausula = descripcionVisible.Length; }
        string clausulaDanio = descripcionVisible.Substring(
          rango.Index + rango.Length,
          Mathf.Min(finClausula - rango.Index - rango.Length, 180));
        Match atributo = Regex.Match(clausulaDanio, @"\((-?\d+)\)");
        if (tipoPorcentaje > 0 && atributo.Success && int.TryParse(atributo.Groups[1].Value, out int valorAtributo))
        {
          baseMinimo += valorAtributo;
          baseMaximo += valorAtributo;
        }
      }

      float multiplicadorDanio = Mathf.Max(0f, 100f + scEstaUnidad.mod_DanioPorcentaje) / 100f;
      danioBasePreviewMinimo = baseMinimo * multiplicadorDanio;
      danioBasePreviewMaximo = baseMaximo * multiplicadorDanio;
      tipoDanioPreview = tipoDanio;
      rangoPreviewBaseCacheValido = true;
    }

    danioMinimo = objetivo.CalcularDanioFinalPreview(danioBasePreviewMinimo, tipoDanioPreview, scEstaUnidad, penetracionArmadura, false);
    danioMaximo = objetivo.CalcularDanioFinalPreview(danioBasePreviewMaximo, tipoDanioPreview, scEstaUnidad, penetracionArmadura, true);

    if (objetivo.estado_Escudado > 0)
    {
      danioMinimo = 0;
    }

    if (danioMaximo < danioMinimo)
    {
      (danioMinimo, danioMaximo) = (danioMaximo, danioMinimo);
    }

    return danioMaximo > 0;
  }

  private bool TryObtenerCampoEntero(string nombreCampo, out int valor)
  {
    Type tipoActual = GetType();
    while (tipoActual != null && tipoActual != typeof(MonoBehaviour))
    {
      FieldInfo campo = tipoActual.GetField(nombreCampo, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (campo != null)
      {
        object contenido = campo.GetValue(this);
        if (contenido is int entero)
        {
          valor = entero;
          return true;
        }
      }
      tipoActual = tipoActual.BaseType;
    }

    valor = 0;
    return false;
  }

  protected struct StatsDescripcionUI
  {
    public int Fuerza;
    public int Agilidad;
    public int Poder;
    public int Ataque;
    public int CriticoRango;
  }

  protected enum TipoSalvacionDescripcion
  {
    Fortaleza,
    Reflejos,
    Mental
  }

  protected static class TerminoDescripcionId
  {
    public const string MarcaReactiva = "reactive-mark";
    public const string Poder = "power";
    public const string Fuerza = "strength";
    public const string SalvacionMental = "mental-save";
    public const string SalvacionFortaleza = "fortitude-save";
    public const string SalvacionReflejos = "reflex-save";
    public const string Fervor = "fervor";
    public const string DanioDivino = "divine-damage";
    public const string DanioFuego = "fire-damage";
    public const string DanioContundente = "bludgeoning-damage";
    public const string Ardiendo = "burning";
    public const string Barrera = "barrier";
    public const string PuntosAccion = "ap";
    public const string Defensa = "defense";
    public const string Valentia = "valour";
    public const string Pasiva = "passive";
    public const string Intrinseca = "intrinsic";
    public const string Debilidad = "weakness";
    public const string TrampaProtectora = "ward-trap";
    public const string Debuff = "debuff";
    public const string Afliccion = "affliction";
    public const string AlientoNegro = "black-breath";
    public const string EcoDivino = "divine-echo";
    public const string Agilidad = "agility";
    public const string Flecha = "arrow";
    public const string DanioCortante = "slashing-damage";
    public const string DanioPerforante = "piercing-damage";
    public const string DanioAcido = "acid-damage";
    public const string Critico = "critical-hit";
    public const string PenetracionArmadura = "armor-penetration";
    public const string Evasion = "evasion";
    public const string Oculto = "hidden";
    public const string MarcaPresa = "prey-mark";
    public const string Ralentizado = "slowed";
    public const string Armadura = "armor";
    public const string Ataque = "attack";
    public const string Sangrado = "bleed";
    public const string Aterrorizado = "terrified";
    public const string DanioFrio = "cold-damage";
    public const string DanioArcano = "arcane-damage";
    public const string Invulnerable = "invulnerable";
    public const string Curacion = "healing";
    public const string CuracionMagica = "magical-healing";
    public const string Veneno = "poison";
    public const string Reaccion = "reaction";
    public const string MarcaSiguesTu = "you-are-next-mark";
    public const string Impulso = "impulse";
    public const string Provocado = "provoked";
    public const string Adolorido = "sore";
    public const string Tambaleando = "staggering";
    public const string VulnerabilidadExpuesta = "exposed-vulnerability";
    public const string Danzando = "dancing";
    public const string ResistenciaArcana = "arcane-resistance";
    public const string Energia = "energy-tier";
    public const string ResiduoEnergetico = "energy-residue";
    public const string Acumulando = "gathering";
    public const string DanioVerdadero = "true-damage";
    public const string Aturdido = "stunned";
    public const string Resistencias = "resistances";
    public const string ManifestacionArcana = "arcane-manifestation";
    public const string SifonArcano = "arcane-siphon";
  }

  protected readonly struct LineaDescripcionNormalizada
  {
    public readonly string Etiqueta;
    public readonly string Valor;
    public readonly int Nivel;

    public LineaDescripcionNormalizada(string etiqueta, string valor, int nivel)
    {
      Etiqueta = etiqueta;
      Valor = valor;
      Nivel = Mathf.Max(0, nivel);
    }
  }

  // Unidades resaltadas al previsualizar la habilidad (se limpia al cancelar/resolver)
  private readonly HashSet<Unidad> unidadesMarcadasPrevisualizacion = new HashSet<Unidad>();

  protected virtual int DelayPreImpactoMs => 1000;
  protected virtual int DelayPostImpactoMs => 700;
  protected virtual bool UsaTimingMeleeCentralizado => esHostil && esMelee && !omitirAnimacionDeUso;
  protected virtual int DelayPreImpactoMeleeMs => MeleeTimingUtility.CalcularPreImpactoMs(scEstaUnidad != null ? scEstaUnidad.GetComponent<UnidadPoseController>() : null);
  protected virtual int DelayPostImpactoMeleeMs => MeleeTimingUtility.CalcularPostImpactoMs(scEstaUnidad != null ? scEstaUnidad.GetComponent<UnidadPoseController>() : null);
  protected virtual bool UsaPoseAtaqueMeleeSostenida => esHostil && esMelee && !omitirAnimacionDeUso;

  protected virtual Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
  {
    if (UsaTimingMeleeCentralizado)
    {
      return DelayPreImpactoMeleeMs > 0 ? BattleManager.DelayCombateAsync(DelayPreImpactoMeleeMs) : Task.CompletedTask;
    }

    return DelayPreImpactoMs > 0 ? BattleManager.DelayCombateAsync(DelayPreImpactoMs) : Task.CompletedTask;
  }

  protected virtual Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
  {
    if (UsaTimingMeleeCentralizado)
    {
      return DelayPostImpactoMeleeMs > 0 ? BattleManager.DelayCombateAsync(DelayPostImpactoMeleeMs) : Task.CompletedTask;
    }

    return DelayPostImpactoMs > 0 ? BattleManager.DelayCombateAsync(DelayPostImpactoMs) : Task.CompletedTask;
  }

  protected string ConstruirDescripcionEstandar(string titulo, string subtitulo, string cuerpoMecanica, string bloqueCostos, string colorTitulo)
  {
    string desc = $"<color={colorTitulo}><b>{titulo}</b></color>\n\n";
    if (!string.IsNullOrEmpty(subtitulo))
    {
      desc += $"<i>{subtitulo}</i>\n\n";
    }

    if (!string.IsNullOrEmpty(cuerpoMecanica))
    {
      desc += $"<color={ColorDescripcionMecanica}>{cuerpoMecanica}</color>\n\n";
    }

    if (!string.IsNullOrEmpty(bloqueCostos))
    {
      desc += $"<color={ColorDescripcionCostos}>{bloqueCostos}</color>";
    }

    return desc;
  }

  protected string ConstruirLineaSalvacion(
    bool esIngles,
    TipoSalvacionDescripcion tipo,
    int dcBase,
    string atributoEscalaEs = null,
    string atributoEscalaEn = null,
    int valorAtributoEscala = 0,
    string atributoEscalaPt = null)
  {
    string tipoEs = "Fortaleza";
    string tipoEn = "Fortitude";
    string tipoPt = "Fortitude";
    if (tipo == TipoSalvacionDescripcion.Reflejos)
    {
      tipoEs = "Reflejos";
      tipoEn = "Reflex";
      tipoPt = "Reflexos";
    }
    else if (tipo == TipoSalvacionDescripcion.Mental)
    {
      tipoEs = "Mental";
      tipoEn = "Mental";
      tipoPt = "Mental";
    }

    bool esPortugues = !esIngles && TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
    bool usaEscalado = !string.IsNullOrEmpty(atributoEscalaEs) && !string.IsNullOrEmpty(atributoEscalaEn);
    if (usaEscalado)
    {
      int dcTotal = dcBase + valorAtributoEscala;
      if (esIngles)
      {
        return $"<b>Save:</b> {tipoEn}. Target rolls 1d20 + {tipoEn} vs DC {dcBase} + {atributoEscalaEn} ({valorAtributoEscala}) = {dcTotal}";
      }

      if (esPortugues)
      {
        string atributoPortugues = string.IsNullOrEmpty(atributoEscalaPt) ? atributoEscalaEs : atributoEscalaPt;
        return $"<b>Resistencia:</b> {tipoPt}. O alvo rola 1d20 + {tipoPt} vs CD {dcBase} + {atributoPortugues} ({valorAtributoEscala}) = {dcTotal}";
      }

      return $"<b>TS:</b> {tipoEs}. El objetivo tira 1d20 + {tipoEs} vs DC {dcBase} + {atributoEscalaEs} ({valorAtributoEscala}) = {dcTotal}";
    }

    if (esIngles)
    {
      return $"<b>Save:</b> {tipoEn}. Target rolls 1d20 + {tipoEn} vs DC {dcBase}";
    }

    if (esPortugues)
    {
      return $"<b>Resistencia:</b> {tipoPt}. O alvo rola 1d20 + {tipoPt} vs CD {dcBase}";
    }

    return $"<b>TS:</b> {tipoEs}. El objetivo tira 1d20 + {tipoEs} vs DC {dcBase}";
  }

  protected string FormatearRangoDados(int cantidadDados, int caras, int bonoFijo = 0)
  {
    int minimo = cantidadDados + bonoFijo;
    int maximo = (cantidadDados * caras) + bonoFijo;
    return $"{minimo}-{maximo}";
  }

  protected string ConstruirDescripcionTooltipNueva(string titulo, string subtitulo, string cuerpo, string costoSuperior = null)
  {
    string costo = costoSuperior == null ? CostoSuperiorDescripcion() : costoSuperior;
    string bloqueCosto = string.IsNullOrEmpty(costo) ? "" : $"<pos=74%><color=#c8c8c8>{costo}</color>";
    return $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>{bloqueCosto}\n\n" +
           $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
           "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
           cuerpo;
  }

  protected LineaDescripcionNormalizada LineaDescripcion(string etiqueta, string valor, int nivel = 0)
  {
    return new LineaDescripcionNormalizada(etiqueta, valor, nivel);
  }

  // Convención de descripciones normalizadas: "Target" solo indica qué puede seleccionarse.
  // El alcance se comunica mediante la previsualización de casillas al seleccionar la habilidad.
  // El daño base expresable como rango usa mínimo-máximo (por ejemplo, 7-22), no notación de dados.
  // Los atributos del usuario no llevan posesivo ("Power"); usar "Target's" solo para atributos del objetivo.
  // Las tiradas de ataque contra Defense se etiquetan "Attack Roll"; "Save" se reserva para salvaciones.
  // Los costos de 0 AP no se muestran; las pasivas ocultan por completo el bloque superior de costos.
  // Las habilidades revisadas no cuestan Valour; Valour solo se menciona cuando es efecto o recompensa.
  // Las habilidades melee normalizadas muestran el icono de espadas junto al titulo cuando se identifican como tales.
  // Las melee unitarias usan "Target: 1 target or obstacle in melee range".
  protected string ConstruirDescripcionNormalizadaIngles(
    string titulo,
    string resumen,
    IReadOnlyList<LineaDescripcionNormalizada> lineas,
    string proximaMejora = null,
    string costoSuperior = null,
    string colorTitulo = "#5dade2",
    bool mostrarIconoMelee = false)
  {
    bool esPasiva = !string.IsNullOrEmpty(resumen)
      && resumen.StartsWith("Passive:", System.StringComparison.OrdinalIgnoreCase);
    string costo = esPasiva
      ? string.Empty
      : costoSuperior == null ? CostoSuperiorDescripcion() : costoSuperior;
    string bloqueCosto = string.IsNullOrEmpty(costo) ? string.Empty : $"<pos=74%><color=#c8c8c8>{costo}</color>";
    string tituloVisible = mostrarIconoMelee ? $"{titulo} <sprite name=\"melee\">" : titulo;
    string descripcion = $"<size=115%><color={colorTitulo}><b>{tituloVisible}</b></color></size>{bloqueCosto}\n\n";

    if (!string.IsNullOrEmpty(resumen))
    {
      descripcion += $"<color=#8f8f8f><i>{resumen}</i></color>\n\n";
    }

    descripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>";
    if (lineas != null && lineas.Count > 0)
    {
      descripcion += "\n\n";
      for (int i = 0; i < lineas.Count; i++)
      {
        LineaDescripcionNormalizada linea = lineas[i];
        if (i > 0)
        {
          descripcion += "\n";
        }

        if (linea.Nivel > 0)
        {
          for (int nivel = 0; nivel < linea.Nivel; nivel++)
          {
            descripcion += "<space=1.15em>";
          }
        }

        string separadorEtiqueta = linea.Etiqueta.EndsWith(":") || linea.Etiqueta.EndsWith(":</color>") ? string.Empty : ":";
        string valorResaltado = ResaltarValorDescripcion(linea.Etiqueta, linea.Valor);
        descripcion += $"<color=#44d3ec><b>{linea.Etiqueta}{separadorEtiqueta}</b></color> <color=#ffffff>{valorResaltado}</color>";
      }
    }

    if (!string.IsNullOrEmpty(proximaMejora))
    {
      string bloqueProximaMejora = proximaMejora.StartsWith("Next Level:", System.StringComparison.OrdinalIgnoreCase)
        ? proximaMejora
        : $"Next Level: {proximaMejora}";
      descripcion += $"\n\n<color=#dfea02>{bloqueProximaMejora}</color>";
    }

    return descripcion;
  }

  protected string ConstruirDescripcionNormalizadaLocalizada(
    string titulo,
    string resumen,
    IReadOnlyList<LineaDescripcionNormalizada> lineas,
    string proximaMejora = null,
    string costoSuperior = null,
    string colorTitulo = "#5dade2",
    bool mostrarIconoMelee = false)
  {
    string costo = costoSuperior == null ? CostoSuperiorDescripcion() : costoSuperior;
    string bloqueCosto = string.IsNullOrEmpty(costo) ? string.Empty : $"<pos=74%><color=#c8c8c8>{costo}</color>";
    string tituloVisible = mostrarIconoMelee ? $"{titulo} <sprite name=\"melee\">" : titulo;
    string descripcion = $"<size=115%><color={colorTitulo}><b>{tituloVisible}</b></color></size>{bloqueCosto}\n\n";

    if (!string.IsNullOrEmpty(resumen))
    {
      descripcion += $"<color=#8f8f8f><i>{resumen}</i></color>\n\n";
    }

    descripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>";
    if (lineas != null && lineas.Count > 0)
    {
      descripcion += "\n\n";
      for (int i = 0; i < lineas.Count; i++)
      {
        LineaDescripcionNormalizada linea = lineas[i];
        if (i > 0)
        {
          descripcion += "\n";
        }

        for (int nivel = 0; nivel < linea.Nivel; nivel++)
        {
          descripcion += "<space=1.15em>";
        }

        string separadorEtiqueta = linea.Etiqueta.EndsWith(":") || linea.Etiqueta.EndsWith(":</color>") ? string.Empty : ":";
        string valorResaltado = ResaltarValorDescripcion(linea.Etiqueta, linea.Valor);
        descripcion += $"<color=#44d3ec><b>{linea.Etiqueta}{separadorEtiqueta}</b></color> <color=#ffffff>{valorResaltado}</color>";
      }
    }

    if (!string.IsNullOrEmpty(proximaMejora))
    {
      descripcion += $"\n\n<color=#dfea02>{proximaMejora}</color>";
    }

    return descripcion;
  }

  protected string TerminoDescripcion(string id, string texto, string spriteName = null, string tamanoIcono = "86%")
  {
    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(texto))
    {
      return texto;
    }

    string icono = string.IsNullOrEmpty(spriteName)
      ? string.Empty
      : $" <size={tamanoIcono}><sprite name=\"{spriteName}\"></size>";
    return $"<link=\"skill-term:{id}\">{texto}{icono}</link>";
  }

  protected bool DebeMostrarProximaMejoraDescripcion()
  {
    return EsEscenaCampaña()
      && CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      && CampaignManager.Instance.scMenuPersonajes.pSel != null
      && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
  }

  protected string CostoSuperiorDescripcion()
  {
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    if (costoAP <= 0)
    {
      return cooldownMax > 0 ? $"{cooldownMax} {iconoCooldown}" : string.Empty;
    }

    return cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
  }

  protected string FormatoModificadorDescripcion(int valor)
  {
    if (valor > 0) { return $" + {valor}"; }
    if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
    return "";
  }

  public static string LimpiarCostoValentiaDescripcion(string descripcion)
  {
    if (string.IsNullOrEmpty(descripcion))
    {
      return descripcion;
    }

    string texto = descripcion.Replace("\r\n", "\n");

    // Elimina lineas de costo de Valentia en ES/EN, preservando cierre de color cuando viene en la misma linea.
    texto = Regex.Replace(
      texto,
      @"\n?\s*-\s*(?:Val Cost|Valour Cost)\s*:[^\n]*(</color>)?",
      m => string.IsNullOrEmpty(m.Groups[1].Value) ? string.Empty : "\n" + m.Groups[1].Value,
      RegexOptions.IgnoreCase);

    texto = Regex.Replace(
      texto,
      @"\n?\s*-\s*Costo(?:\s+de)?\s+(?:Val|Valent(?:i|í)a)\s*:[^\n]*(</color>)?",
      m => string.IsNullOrEmpty(m.Groups[1].Value) ? string.Empty : "\n" + m.Groups[1].Value,
      RegexOptions.IgnoreCase);

    // Elimina hints de subida de nivel que solo hablaban de bajar costo de Valentia.
    texto = Regex.Replace(
      texto,
      @"\n?\s*<color=#dfea02>\s*-\s*(?:Next Level|Proximo Nivel):\s*-1\s*(?:Val(?:our)? cost|costo(?:\s+de)?\s+(?:Val|Valent(?:i|í)a))\.\s*</color>",
      string.Empty,
      RegexOptions.IgnoreCase);

    // Limpieza puntual de textos de opciones antiguas.
    texto = texto.Replace("Option A (-1 Val cost) or Option B", "Option A or Option B");
    texto = texto.Replace("Option A (-1 Valour cost) or Option B", "Option A or Option B");
    texto = texto.Replace("Opcion A (-1 costo Val) u Opcion B", "Opcion A u Opcion B");
    texto = texto.Replace("Opcion A (-1 costo de Val) u Opcion B", "Opcion A u Opcion B");
    texto = texto.Replace("Opcion A (-1 costo Valentia) u Opcion B", "Opcion A u Opcion B");
    texto = texto.Replace("Opcion A (-1 costo de Valentia) u Opcion B", "Opcion A u Opcion B");
    texto = texto.Replace("Opcion A (-1 costo Valentía) u Opcion B", "Opcion A u Opcion B");
    texto = texto.Replace("Opcion A (-1 costo de Valentía) u Opcion B", "Opcion A u Opcion B");

    texto = Regex.Replace(texto, @"\n{3,}", "\n\n");
    return texto;
  }

  protected StatsDescripcionUI ObtenerStatsDescripcionUI()
  {
    StatsDescripcionUI stats = new StatsDescripcionUI();

    // Una habilidad asociada a una unidad usa siempre sus valores vivos, incluso si el
    // combate se muestra dentro de la escena de campaña.
    if (scEstaUnidad != null)
    {
      stats.Fuerza = Mathf.RoundToInt(scEstaUnidad.mod_CarFuerza);
      stats.Agilidad = Mathf.RoundToInt(scEstaUnidad.mod_CarAgilidad);
      stats.Poder = Mathf.RoundToInt(scEstaUnidad.mod_CarPoder);
      stats.Ataque = Mathf.RoundToInt(scEstaUnidad.mod_Ataque);
      stats.CriticoRango = Mathf.RoundToInt(scEstaUnidad.mod_CriticoRangoDado);
      return stats;
    }

    MenuPersonajes menu = null;
    Personaje personaje = null;
    if (CampaignManager.Instance != null)
    {
      menu = CampaignManager.Instance.scMenuPersonajes;
      if (menu != null)
      {
        personaje = menu.pSel;
      }
    }

    if (personaje == null)
    {
      if (Usuario != null)
      {
        personaje = Usuario.GetComponent<Personaje>();
      }
      if (personaje == null)
      {
        personaje = GetComponent<Personaje>();
      }
    }

    if (personaje != null)
    {
      int buffFuerza;
      int buffAgi;
      int buffPoder;
      CalcularBuffsEquipoCampania(personaje, menu, out buffFuerza, out buffAgi, out buffPoder);

      stats.Fuerza = personaje.iFuerza + buffFuerza;
      stats.Agilidad = personaje.iAgi + buffAgi;
      stats.Poder = personaje.iPoder + buffPoder;
      stats.Ataque = Mathf.RoundToInt(personaje.fBonusAtaque);
      stats.CriticoRango = Mathf.RoundToInt(personaje.fCritRango);

      // Replica estados de campaña que alteran atributos/ataque en batalla.
      if (personaje.Camp_Fatigado || (personaje.PuedeRealizarActividades() && personaje.ActividadSeleccionada == 2))
      {
        stats.Fuerza -= 1;
        stats.Agilidad -= 1;
        stats.Poder -= 1;
      }
      if (personaje.Camp_Herido)
      {
        stats.Fuerza -= 1;
        stats.Agilidad -= 1;
        stats.Poder -= 1;
      }
      if (personaje.TieneMoralAlta())
      {
        stats.Ataque += 1;
      }
      else if (personaje.TieneMoralBaja())
      {
        stats.Ataque -= 1;
      }

      return stats;
    }

    // Fallback.
    if (scEstaUnidad != null)
    {
      stats.Fuerza = Mathf.RoundToInt(scEstaUnidad.mod_CarFuerza);
      stats.Agilidad = Mathf.RoundToInt(scEstaUnidad.mod_CarAgilidad);
      stats.Poder = Mathf.RoundToInt(scEstaUnidad.mod_CarPoder);
      stats.Ataque = Mathf.RoundToInt(scEstaUnidad.mod_Ataque);
      stats.CriticoRango = Mathf.RoundToInt(scEstaUnidad.mod_CriticoRangoDado);
    }

    return stats;
  }

  private void CalcularBuffsEquipoCampania(Personaje personaje, MenuPersonajes menu, out int buffFuerza, out int buffAgi, out int buffPoder)
  {
    buffFuerza = 0;
    buffAgi = 0;
    buffPoder = 0;

    if (menu != null && menu.scEquipo != null)
    {
      buffFuerza = menu.scEquipo.BuffTOTALEQUIPOFuerza;
      buffAgi = menu.scEquipo.BuffTOTALEQUIPOAgi;
      buffPoder = menu.scEquipo.BuffTOTALEQUIPOPoder;
      return;
    }

    if (personaje == null)
    {
      return;
    }

    if (personaje.itemArma != null)
    {
      buffFuerza += personaje.itemArma.buffFuerza;
      buffAgi += personaje.itemArma.buffAgi;
      buffPoder += personaje.itemArma.buffPoder;
    }
    if (personaje.itemArmadura != null)
    {
      buffFuerza += personaje.itemArmadura.buffFuerza;
      buffAgi += personaje.itemArmadura.buffAgi;
      buffPoder += personaje.itemArmadura.buffPoder;
    }
    if (personaje.Accesorio1 != null)
    {
      buffFuerza += personaje.Accesorio1.buffFuerza;
      buffAgi += personaje.Accesorio1.buffAgi;
      buffPoder += personaje.Accesorio1.buffPoder;
    }
    if (personaje.Accesorio2 != null)
    {
      buffFuerza += personaje.Accesorio2.buffFuerza;
      buffAgi += personaje.Accesorio2.buffAgi;
      buffPoder += personaje.Accesorio2.buffPoder;
    }
  }

  /// <summary>
  /// Permite disparar manualmente el flujo de pre-impacto (proyectiles, delays, etc.) sin pasar por Resolver.
  /// </summary>
  public Task PrepararImpactoManualAsync(List<object> objetivos, Casilla casillaOrigenTrampas = null)
  {
    return EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
  }

  /// <summary>
  /// Permite disparar manualmente el flujo de post-impacto (esperas, limpieza) sin pasar por Resolver.
  /// </summary>
  public Task FinalizarImpactoManualAsync(List<object> objetivos, Casilla casillaOrigenTrampas = null)
  {
    return EsperarPostImpactoAsync(objetivos, casillaOrigenTrampas);
  }

  public abstract void Activar();

  public static event EventHandler OnUsarHabilidad;

  public abstract void ActualizarDescripcion();
  public void ActualizarNivel()
  {
    Invoke("Awake", 0.5f);
  }
  public abstract void Awake();

  public Sprite ObtenerIconoUI()
  {
    if (imHab != null)
    {
      return imHab;
    }

    if (agregaDesdeArmaUI != null && agregaDesdeArmaUI.imItem != null)
    {
      return agregaDesdeArmaUI.imItem;
    }

    return null;
  }

  // Método abstracto para activar la habilidad.
  public virtual async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {
    BattleManager.Instance?.OcultarPanelDescripcionHabilidad(this, true);

    if (Objetivos != null)
    {
      HashSet<object> objetivosUnicos = new HashSet<object>();
      Objetivos.RemoveAll(objetivo => objetivo == null || !objetivosUnicos.Add(objetivo));
    }

    if (BattleManager.Instance != null)
    {
      if (!BattleManager.Instance.TryFiltrarObjetivosHostilesPorProvocacion(scEstaUnidad, esHostil, Objetivos, out List<object> objetivosFiltradosProvocacion))
      {
        scEstaUnidad?.GenerarTextoFlotante(TRADU.i.Traducir("Provocado: objetivo fuera de alcance."), Color.gray, FloatingTextContext.Generic);
        return;
      }

      Objetivos = objetivosFiltradosProvocacion;

      if (!BattleManager.Instance.TryFiltrarObjetivosMeleePorInmovilizacion(scEstaUnidad, esMelee, Objetivos, out List<object> objetivosFiltrados))
      {
        scEstaUnidad?.GenerarTextoFlotante(TRADU.i.Traducir("Inmóvil, Melee solo adyacente."), Color.gray, FloatingTextContext.Generic);
        return;
      }

      Objetivos = objetivosFiltrados;
    }

    // Objetivo/s ya confirmados para este ataque: si corresponde (misma regla que el hover:
    // habilidad hostil + objetivo IA + bando opuesto), sostener su pose de "objetivo hostil" hasta
    // el fin del ataque. Usa el mismo Enter/ExitPoseObjetivoHostil con contador que ya usa el hover,
    // asi que conviven sin conflicto sin modificar el mecanismo de hover existente.
    List<Unidad> unidadesConPoseObjetivoHostilSostenida = null;
    if (BattleManager.Instance != null && Objetivos != null)
    {
      foreach (var objetivo in Objetivos)
      {
        if (objetivo is Unidad unidadObjetivo && BattleManager.Instance.DebeMantenerPoseObjetivoHostilDuranteAtaque(unidadObjetivo))
        {
          UnidadPoseController poseObjetivo = unidadObjetivo.GetComponent<UnidadPoseController>();
          if (poseObjetivo != null && poseObjetivo.poseTurnoActivo != null)
          {
            poseObjetivo.EnterPoseObjetivoHostil();
            unidadesConPoseObjetivoHostilSostenida ??= new List<Unidad>();
            unidadesConPoseObjetivoHostilSostenida.Add(unidadObjetivo);
          }
        }
      }
    }

    RuntimeAnalytics.TrackAbilityUsed(this, scEstaUnidad);
    BattleManager.Instance.bOcupado = true;
    // Al confirmar la habilidad, limpiar marcas de previsualizacion en unidades.
    LimpiarMarcasUnidadesPosibles();
    MeleeApproachMover acercamientoMelee = MeleeApproachMover.ObtenerOCrear(scEstaUnidad);
    bool hizoAproximacion = acercamientoMelee != null && await acercamientoMelee.PrepararAproximacionJugadorAsync(this, Objetivos);
    bool usarTimingMeleeCentralizado = UsaTimingMeleeCentralizado;
    bool poseAtaqueSostenidaActiva = false;
    if (esHostil && !esDiscreta && scEstaUnidad != null && scEstaUnidad.ObtenerEstaEscondido() > 0)
    {
      scEstaUnidad.PerderEscondido();
    }
    // Animacion/pose:
    // - forzarPoseHabilidad: siempre usa pose de habilidad
    // - fuerzaPoseAtaque: siempre usa ataque
    // - Canalizador hostil: usa ataque
    // - Hostil melee: usa ataque
    // - Resto: usa pose de habilidad
    if (!omitirAnimacionDeUso)
    {
      bool usarAtaque = !forzarPoseHabilidad && (fuerzaPoseAtaque || (scEstaUnidad is ClaseCanalizador && esHostil) || (esHostil && esMelee));
      if (usarAtaque)
      {
        if (UsaPoseAtaqueMeleeSostenida)
        {
          scEstaUnidad.IniciarPoseAtaqueSostenida();
          poseAtaqueSostenidaActiva = true;
        }
        else
        {
          scEstaUnidad.ReproducirAnimacionAtaque();
        }
      }
      else
      {
        scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
      }
    }
    // Log de uso de habilidad
    if (BattleManager.Instance != null && scEstaUnidad != null)
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
    }

    List<Unidad> lUnidadesPosibles = new List<Unidad>(BattleManager.Instance.lUnidadesTotal);
    lUnidadesPosibles.Remove(scEstaUnidad);
    List<object> noParticipantes = null;

    if (Objetivos != null)
    {
      List<Unidad> lUnidadesObjetivos = new List<Unidad>(Objetivos.FindAll(x => x is Unidad).ConvertAll(x => (Unidad)x));
      foreach (Unidad unidad in lUnidadesObjetivos)
      {
        lUnidadesPosibles.Remove(unidad);
      }

      noParticipantes = new List<object>(lUnidadesPosibles.ConvertAll(x => (object)x));
      List<Obstaculo> obstaculosObjetivos = new List<Obstaculo>(Objetivos.FindAll(x => x is Obstaculo).ConvertAll(x => (Obstaculo)x));
      foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
      {
        Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
        if (obstaculo == null)
        {
          continue;
        }
        if (!obstaculosObjetivos.Contains(obstaculo))
        {
          noParticipantes.Add(obstaculo);
        }
      }

      BattleManager.Instance.SombrearANoParticipantesHabilidad(noParticipantes);
    }

    int penetracionAnteriorHabilidad = 0;
    bool restituirPenetracionHabilidad = false;
    bool focoCamaraAplicado = false;
    if (scEstaUnidad != null)
    {
      penetracionAnteriorHabilidad = scEstaUnidad.penetracionArmaduraHabilidadActual;
      scEstaUnidad.penetracionArmaduraHabilidadActual = Mathf.Max(0, penetracionArmadura);
      restituirPenetracionHabilidad = true;
    }

    try
    {
      if (!omitirFocoCamara && BattleManager.Instance != null)
      {
        bool focoEsArea = esZonal || enArea > 0 || (Objetivos != null && Objetivos.Count > 1);
        BattleManager.Instance.EnfocarCamaraHabilidad(scEstaUnidad, Objetivos, esHostil, false, intensidadFocoCamara, esMelee, focoEsArea);
        focoCamaraAplicado = true;
      }

      if (!usarTimingMeleeCentralizado)
      {
        await BattleManager.DelayCombateAsync(250);
      }

      await EsperarPreImpactoAsync(Objetivos, casillaOrigenTrampas);

      int tirada = UnityEngine.Random.Range(1, 21); //la tirada es la misma para toda la habilidad, no para cada objetivo

      if (Objetivos != null)
      {
        foreach (var objeto in Objetivos) //puede ser Unidad o Obstaculo
        {
          AplicarEfectosHabilidad(objeto, tirada, casillaOrigenTrampas);
        }
      }
      else
      {
        AplicarEfectosHabilidad(null, tirada, casillaOrigenTrampas);
      }

      await EsperarPostImpactoAsync(Objetivos, casillaOrigenTrampas);
      BanterBattleDirector.NotificarHabilidadAliada(scEstaUnidad, Objetivos, esHostil);
    }
    finally
    {
      if (restituirPenetracionHabilidad && scEstaUnidad != null)
      {
        scEstaUnidad.penetracionArmaduraHabilidadActual = penetracionAnteriorHabilidad;
      }

      if (focoCamaraAplicado && BattleManager.Instance != null)
      {
        BattleManager.Instance.RestaurarCamaraHabilidad();
      }

      if (unidadesConPoseObjetivoHostilSostenida != null)
      {
        foreach (Unidad unidadObjetivo in unidadesConPoseObjetivoHostilSostenida)
        {
          unidadObjetivo?.GetComponent<UnidadPoseController>()?.ExitPoseObjetivoHostil();
        }
      }
    }
    if (hizoAproximacion && acercamientoMelee != null)
    {
      await acercamientoMelee.VolverAPosicionInicialAsync();
    }
    else if (poseAtaqueSostenidaActiva && scEstaUnidad != null)
    {
      scEstaUnidad.FinalizarPoseAtaqueSostenida();
      poseAtaqueSostenidaActiva = false;
    }
    if (Objetivos != null)
    {
      BattleManager.Instance.DesombrearANoParticipantesHabilidad(noParticipantes ?? lUnidadesPosibles.ConvertAll(x => (object)x));
    }

    //Si la habilidad es hostil, no Discreta, y tiene Escondido tier 1, se revela

    if (esHostil && !esDiscreta && (scEstaUnidad.ObtenerEstaEscondido() > 0))
    {
      scEstaUnidad.PerderEscondido();
    }

    BattleManager.Instance.txtSeleccionaobj.SetActive(false);
    BattleManager.Instance.SeleccionandoObjetivo = false;
    BattleManager.Instance.LimpiarFadeHoverObjetivoHabilidad();
    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.scUIBotonesHab.UIDesactivarHabilidades();

    BattleManager.Instance.LimpiarCapasCasillas();

    if (scEstaUnidad.valorCargando > 0) //si está cargando y le alcanza que reste ap igual a lo que le faltaba
    {
      scEstaUnidad.CambiarAPActual(-scEstaUnidad.valorCargando);
      scEstaUnidad.valorCargando = 0;
      scEstaUnidad.estaCargando = null;
    }
    else
    {
      scEstaUnidad.CambiarAPActual(-costoAP);
    }
    if (seEsforzaria > 0)
    {
      AplicarDebuffEsfuerzo(seEsforzaria);
    }


    cooldownActual = cooldownMax;

    OnUsarHabilidad?.Invoke(this, EventArgs.Empty);



    Invoke("ActualizarCirculosDelay", 0.5f); //Para que se actualice el UI de AP luego de un delay, para que no se vea el cambio de golpe

    Invoke("desocuparDelay", 0.5f);
    BattleManager.Instance.bOcupado = false;
    
    if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
    {
            if(BattleManager.Instance.scTutorialCombate.ObtenerPasoActual()  == 4)
            {
               await BattleManager.DelayCombateAsync(1500);
                BattleManager.Instance.scTutorialCombate.SiguientePasoCombate();
            }
    }

    if (poseAtaqueSostenidaActiva && scEstaUnidad != null)
    {
      scEstaUnidad.FinalizarPoseAtaqueSostenida();
    }
  }

  void ActualizarCirculosDelay()
  {
    BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
  
  }

  void desocuparDelay()
  {
    BattleManager.Instance.bOcupado = false;

  }


  public int seEsforzaria;
  Personaje ObtenerPersonajeTraitsUsuario()
  {
    if (scEstaUnidad == null || CampaignManager.Instance == null || CampaignManager.Instance.scAdministradorEscenas == null)
    {
      return null;
    }

    return CampaignManager.Instance.scAdministradorEscenas.ObtenerPersonajeAliadoSeleccionadoPorUnidad(scEstaUnidad);
  }

  public bool tieneAPSuficientes(out int esforzo)
  {
    seEsforzaria = 0;
    esforzo = 0;
    int indexAPyEsfuerzo = (int)scEstaUnidad.ObtenerAPActual() + esforzable - costoAP;

    if ((int)scEstaUnidad.ObtenerAPActual() < 1) { return false; } //Si no tiene AP no puede esforzarse
    if (indexAPyEsfuerzo < 0) { return false; } //Devuelve false, como resultado de que no tiene AP suficiente

    if (indexAPyEsfuerzo < esforzable && esforzable > 0)
    { //Esto significa que para hacer la habilidad, se Esfuerza (debe AP para la siguiente ronda)
      Personaje personajeTraits = ObtenerPersonajeTraitsUsuario();
      if (personajeTraits != null && personajeTraits.TieneRasgo(PersonajeTraitCatalog.TraitMinimoEsfuerzo))
      {
        return false;
      }

      seEsforzaria = esforzable - indexAPyEsfuerzo;
      esforzo = seEsforzaria;


    }

    return true;


  }

  protected virtual void AplicarDebuffEsfuerzo(int esfuerzo)
  {
    if (scEstaUnidad == null || esfuerzo <= 0)
    {
      return;
    }

    Personaje personajeTraits = ObtenerPersonajeTraitsUsuario();
    if (personajeTraits != null && personajeTraits.TieneRasgo(PersonajeTraitCatalog.TraitEsforzado))
    {
      return;
    }

    Buff buffEsfuerzo = new Buff();
    buffEsfuerzo.buffNombre = "Esfuerzo";
    buffEsfuerzo.buffDescr = "La unidad se ha esforzado.";
    buffEsfuerzo.boolfDebufftBuff = false;
    buffEsfuerzo.DuracionBuffRondas = 2;
    buffEsfuerzo.suprimeTextoFlotante = true;
    buffEsfuerzo.cantAPMax -= esfuerzo;
    buffEsfuerzo.cantDefensa -= 2 * esfuerzo;
    buffEsfuerzo.esStackeable = true;
    buffEsfuerzo.AplicarBuff(scEstaUnidad);
    ComponentCopier.CopyComponent(buffEsfuerzo, scEstaUnidad.gameObject);
  }

  //Ataque vs Defensa convencional
  public int TiradaAtaque(int tirada, float defensaObjetivo, float atributoAtaca, float modificadorHabilidadaAtaque, float modificadorDadoCritico, Unidad unidadAtacada, int sumaPifia)
  {
    //Pifia = -1
    //Fallo = 0
    //Roce = 1
    //Golpe = 2
    //Crítico = 3

    int resultado = 0;
    Unidad objetivoProyectil = unidadAtacada;

    bool objetivoUnitario = !esZonal && enArea == 0 && targetEspecial == 0;
    if (unidadAtacada != null)
    {
      ReaccionRiposte.TryPrepararIntercepcion(scEstaUnidad, unidadAtacada, esMelee, objetivoUnitario, out Unidad objetivoIntercepcion, out float defensaIntercepcion);
      unidadAtacada = objetivoIntercepcion;
      defensaObjetivo = defensaIntercepcion;
    }

    float tiradaBase = tirada;
    float iTiradaAtaque = tirada;
    float iDadoSolo = tirada;
    float deltaClima = 0f;

    //Efectos de clima en Ataques
    if (CampaignManager.Instance.intTipoClima == 5) // Niebla -1 ataque rango
    {
      if (!esMelee)
      {
        iTiradaAtaque -= 1;
        deltaClima -= 1;
      }

    }


    string objetivoNombre = unidadAtacada != null ? unidadAtacada.uNombre : TRADU.i.Traducir("objetivo");
    CampaignManager campaignTraits = CampaignManager.Instance;
    AdministradorEscenas adminTraits = campaignTraits != null ? campaignTraits.scAdministradorEscenas : null;
    Personaje personajeTraits = adminTraits != null ? adminTraits.ObtenerPersonajeAliadoSeleccionadoPorUnidad(scEstaUnidad) : null;
    int extraPifiaTraits = personajeTraits != null && personajeTraits.TieneRasgo(PersonajeTraitCatalog.TraitTorpe) ? 1 : 0;
    bool noPuedePifiar = personajeTraits != null && personajeTraits.TieneRasgo(PersonajeTraitCatalog.TraitCoordinado);
    int umbralPifia = noPuedePifiar ? 0 : 1 + sumaPifia + extraPifiaTraits;
    float bonusCriticoRecibido = unidadAtacada != null ? unidadAtacada.bonusCritDadoRecibido : 0f;
    float umbralCritico = 19 - modificadorDadoCritico - bonusCriticoRecibido;
    string textoResultado;
    CombatLogFormatter.CombatOutcome outcome;
    bool ocultarBonificacionesBallestaTutorial = this is TiroBallestaDeMano
      && BattleManager.Instance.scTutorialCombate != null
      && BattleManager.Instance.scTutorialCombate.tutorialCombateActivo
      && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() == 4;

    if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
    {
      iDadoSolo += 5;
      if(iDadoSolo > 20) { iDadoSolo = 20; }
    } //En tutorial no hay pifias

    if (iDadoSolo <= umbralPifia)//Pifia
    {
      // Tooltip emergente de pifia desactivado (quedaba desactualizado respecto al
      // comportamiento actual: resta 1 PA pero ya no termina el turno).

      Unidad unidadTextoPifia = unidadAtacada != null ? unidadAtacada : scEstaUnidad;
      unidadTextoPifia?.GenerarTextoFlotante(TRADU.i.Traducir("<b>Pifia</b>"), Color.red, FloatingTextContext.Miss);
      scEstaUnidad?.ReproducirFlashPifiaLeveRapido();
      string nombreAtacante = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
      string nombreObjetivoValentia = unidadAtacada != null
        ? (TRADU.i != null ? TRADU.i.Traducir(unidadAtacada.uNombre) : unidadAtacada.uNombre)
        : (TRADU.i != null ? TRADU.i.Traducir("objetivo") : "objetivo");
      bool enInglesValentia = TRADU.i != null && TRADU.i.nIdioma == 2;
      string motivoPifiaValentia = enInglesValentia
        ? nombreAtacante + " fumbles against " + nombreObjetivoValentia
        : nombreAtacante + " pifia contra " + nombreObjetivoValentia;
      scEstaUnidad.SumarValentia(-1, motivoPifiaValentia);
      if (scEstaUnidad.ObtenerAPActual() > 0)
      {
        scEstaUnidad.CambiarAPActual(-1);
      }

      scEstaUnidad.IgnorarProximoSetAPCeroPorPifia();
      textoResultado = TRADU.i.Traducir("Pifia");
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          scEstaUnidad.mod_Ataque,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Pifia,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico),
          deltaClima,
          null,
          ocultarBonificacionesBallestaTutorial));
      if (BattleManager.Instance != null && BattleManager.Instance.HabilidadActiva != null && BattleManager.Instance.HabilidadActiva.esMelee)
      {
        AjustesAudio.ReproducirClipEnPunto(BattleManager.Instance.contenedorPrefabs.sonidoErrar, transform.position);
      }
      if (esMelee && unidadAtacada != null)
      {
        unidadAtacada.AnticiparFalloAtaqueRecibido(scEstaUnidad, true);
      }
      unidadAtacada?.NotificarAtaqueRecibido();
      BanterBattleDirector.NotificarResultadoAtaque(scEstaUnidad, unidadAtacada, -1);
      ArrowFlight.NotificarResultadoAtaque(scEstaUnidad, objetivoProyectil, -1);
      return -1;
    }

    if (iDadoSolo >= umbralCritico) //Golpe crítico
    {
      textoResultado = TRADU.i.Traducir("Crítico");
      ScreenFlash.FlashCritical();
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          scEstaUnidad.mod_Ataque,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Critico,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico),
          deltaClima,
          TRADU.i.Traducir("Impacto crítico"),
          ocultarBonificacionesBallestaTutorial));
      unidadAtacada?.NotificarAtaqueRecibido();
      BanterBattleDirector.NotificarResultadoAtaque(scEstaUnidad, unidadAtacada, 3);
      ArrowFlight.NotificarResultadoAtaque(scEstaUnidad, objetivoProyectil, 3);
      return 3;
    }

    float efectosAlAtaque = atributoAtaca + modificadorHabilidadaAtaque + scEstaUnidad.mod_Ataque;
    float iResultadoAtaque = iTiradaAtaque + efectosAlAtaque;

    if (iResultadoAtaque > defensaObjetivo)
    {
      resultado = 2; //Golpe
      outcome = CombatLogFormatter.CombatOutcome.Golpe;
      textoResultado = TRADU.i.Traducir("Golpe");
    }
    else if (Mathf.Approximately(iResultadoAtaque, defensaObjetivo))
    {
      resultado = 1; //Roce
      outcome = CombatLogFormatter.CombatOutcome.Roce;
      textoResultado = TRADU.i.Traducir("Roce");
    }
    else
    {
      resultado = 0; //Fallo
      outcome = CombatLogFormatter.CombatOutcome.Fallo;
      textoResultado = TRADU.i.Traducir("Fallo");
      unidadAtacada.GenerarTextoFlotante(TRADU.i.Traducir("Fallo"), new Color(0.8f, 0.8f, 0.8f), FloatingTextContext.Miss);
      if (esMelee && unidadAtacada != null)
      {
        unidadAtacada.AnticiparFalloAtaqueRecibido(scEstaUnidad, true);
      }
    }

    BattleManager.Instance.EscribirLog(
      CombatLogFormatter.FormatearAtaque(
        scEstaUnidad.uNombre,
        objetivoNombre,
        tiradaBase,
        iTiradaAtaque,
        atributoAtaca,
        modificadorHabilidadaAtaque,
        scEstaUnidad.mod_Ataque,
        defensaObjetivo,
        textoResultado,
        outcome,
        umbralPifia,
        Mathf.RoundToInt(umbralCritico),
        deltaClima,
        null,
        ocultarBonificacionesBallestaTutorial));


    if (resultado < 2 && BattleManager.Instance.HabilidadActiva.esMelee)
    { AjustesAudio.ReproducirClipEnPunto(BattleManager.Instance.contenedorPrefabs.sonidoErrar, transform.position); }

    unidadAtacada?.NotificarAtaqueRecibido();
    ArrowFlight.NotificarResultadoAtaque(scEstaUnidad, objetivoProyectil, resultado);
    return resultado;
  }

  //Tiradas Salvacion vs Atributo  boolean TRUE falla tirada  FALSE gana tirada.
  /*
  public bool TiradaSalvacion(float atributoDefiende, float atributoAtaca, float modificadorHabilidadaAtaque)
  {
    bool resultado = false;

    float iTiradaAtaque = UnityEngine.Random.Range(1,21);
    float iTiradaDefensa = UnityEngine.Random.Range(1,21);

    float iResultadoAtaque = iTiradaAtaque + atributoAtaca + modificadorHabilidadaAtaque;
    float iResultadoDefensa = iTiradaDefensa + atributoDefiende;


    resultado = iResultadoAtaque > iResultadoDefensa;

    if(resultado)
    {
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de Salvación: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: No se salva.");
    }
    else
    {
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de Salvación: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: Se salva.");
    }

    return resultado;
  }*/

    public abstract void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa); //la tirada se determina antes de entrar a cada objetivo, para que sea la misma

  public bool EsEscenaCampaña()
  {

    if (SceneManager.GetActiveScene().name == "ES-Campaña")
    {
      return true;
    }
    else { return false; }

  }

  /// <summary>
  /// Marca las unidades actualmente en rango segun lUnidadesPosiblesHabilidadActiva y limpia las que ya no aplican.
  /// </summary>
  public void SincronizarMarcasUnidadesPosibles()
  {
    SincronizarMarcasUnidades(BattleManager.Instance != null ? BattleManager.Instance.lUnidadesPosiblesHabilidadActiva : null);
  }

  public void SincronizarMarcasUnidadesPosibles(IEnumerable<Unidad> unidadesObjetivo)
  {
    SincronizarMarcasUnidades(unidadesObjetivo);
  }

  /// <summary>
  /// Limpia las unidades marcadas en la ultima previsualizacion de la habilidad.
  /// </summary>
  public void LimpiarMarcasUnidadesPosibles()
  {
    foreach (Unidad unidad in unidadesPreviewDanio)
    {
      unidad?.LimpiarPreviewDanio();
    }
    unidadesPreviewDanio.Clear();
    rangoPreviewBaseCacheValido = false;

    foreach (Unidad unidad in unidadesMarcadasPrevisualizacion)
    {
      unidad?.OcultarProbabilidad();
    }

    unidadesMarcadasPrevisualizacion.Clear();
  }

  private void SincronizarMarcasUnidades(IEnumerable<Unidad> unidadesObjetivo)
  {
    if (unidadesObjetivo == null)
    {
      LimpiarMarcasUnidadesPosibles();
      return;
    }

    HashSet<Unidad> nuevas = new HashSet<Unidad>();
    foreach (Unidad unidad in unidadesObjetivo)
    {
      if (unidad == null)
      {
        continue;
      }

      unidadesMarcadasPrevisualizacion.Add(unidad);

      if (DebeMostrarInvulnerableEnProbabilidad(unidad))
      {
        unidad.MostrarProbabilidad(0f, TRADU.i.Traducir("Invulnerable"));
        nuevas.Add(unidad);
        continue;
      }

      float? prob = CalcularProbabilidadSobreObjetivo(unidad);
      string textoProbabilidad = prob.HasValue ? ObtenerTextoProbabilidadSobreObjetivo(unidad, prob.Value) : null;
      unidad.MostrarProbabilidad(prob, textoProbabilidad);

      if (!unidadesPreviewDanio.Contains(unidad)
        && TryObtenerRangoDanioPreview(unidad, out int danioMinimo, out int danioMaximo))
      {
        unidad.MostrarPreviewDanio(danioMinimo, danioMaximo);
        unidadesPreviewDanio.Add(unidad);
      }

      nuevas.Add(unidad);
    }

    if (unidadesMarcadasPrevisualizacion.Count == 0)
    {
      return;
    }

    List<Unidad> paraRemover = new List<Unidad>();
    foreach (Unidad unidadMarcada in unidadesMarcadasPrevisualizacion)
    {
      if (!nuevas.Contains(unidadMarcada))
      {
        unidadMarcada?.OcultarProbabilidad();
        unidadMarcada?.LimpiarPreviewDanio();
        unidadesPreviewDanio.Remove(unidadMarcada);
        paraRemover.Add(unidadMarcada);
      }
    }

    foreach (Unidad unidad in paraRemover)
    {
      unidadesMarcadasPrevisualizacion.Remove(unidad);
    }
  }

  public float? CalcularProbabilidadSobreObjetivo(Unidad objetivo)
  {
    if (objetivo == null || scEstaUnidad == null)
    {
      return null;
    }

    float? probabilidadEspecial = CalcularProbabilidadEspecialSobreObjetivo(objetivo);
    if (probabilidadEspecial.HasValue)
    {
      return Mathf.Clamp01(probabilidadEspecial.Value);
    }

    switch (tipoPorcentaje)
    {
      case 1:
        return CalcularProbAtaque(objetivo, 1);
      case 2:
        return CalcularProbAtaque(objetivo, 2);
      case 3:
        return CalcularProbAtaque(objetivo, 3);
      default:
        return null;
    }
  }

  protected virtual float? CalcularProbabilidadEspecialSobreObjetivo(Unidad objetivo)
  {
    return null;
  }

  protected virtual string ObtenerTextoProbabilidadSobreObjetivo(Unidad objetivo, float probabilidad)
  {
    return null;
  }

  protected float CalcularProbabilidadFallarTS(float atributoDefiende, float dificultadSalvacion)
  {
    int exitos = 0;
    for (int dado = 1; dado <= 20; dado++)
    {
      if (dificultadSalvacion > dado + atributoDefiende)
      {
        exitos++;
      }
    }

    return exitos / 20f;
  }

  protected string FormatearTextoProbabilidadExito(float probabilidad)
  {
    int porcentaje = Mathf.RoundToInt(Mathf.Clamp01(probabilidad) * 100f);
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
    if (esIngles)
    {
      return $"{porcentaje}% success chance";
    }

    if (esPortugues)
    {
      return $"{porcentaje}% chance de sucesso";
    }

    return $"{porcentaje}% chances de \u00e9xito";
  }

  private bool DebeMostrarInvulnerableEnProbabilidad(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return false;
    }

    bool ataqueFisico = tipoPorcentaje == 1 || tipoPorcentaje == 2;
    IAUnidadEspectroBosque espectroBosque = objetivo.GetComponent<IAUnidadEspectroBosque>();
    return ataqueFisico && espectroBosque != null && espectroBosque.EstaEnPlanoEtereo();
  }

  private float CalcularProbAtaque(Unidad objetivo, int tipoAtaquePorcentaje)
  {
    float atributoAtacante = scEstaUnidad.mod_CarAgilidad;
    if (tipoAtaquePorcentaje == 1)
    {
      atributoAtacante = scEstaUnidad.mod_CarFuerza;
    }
    else if (tipoAtaquePorcentaje == 3)
    {
      atributoAtacante = scEstaUnidad.mod_CarPoder;
    }

    float ataqueTotal = scEstaUnidad.mod_Ataque + atributoAtacante + ObtenerBonusAtaque();
    if (tipoAtaquePorcentaje != 1 && CampaignManager.Instance != null)
    {
      // Penalidades de clima a ataques a distancia
      if (CampaignManager.Instance.intTipoClima == 5) // Niebla
      {
        ataqueTotal -= 1f;
      }
    }
    float defensaObjetivo = objetivo.ObtenerdefensaActual();

    int exitos = 0;
    for (int dado = 1; dado <= 20; dado++)
    {
      bool pifia = dado == 1;
      bool critico = dado >= 19;
      if (pifia)
      {
        continue;
      }

      if (critico)
      {
        exitos++;
        continue;
      }

      float resultado = dado + ataqueTotal;
      if (resultado > defensaObjetivo)
      {
        exitos++;
      }
    }

    return exitos / 20f;
  }

  protected virtual float ObtenerBonusAtaque()
  {
    // Busca un campo/propiedad llamado "bonusAtaque" en la habilidad concreta (serializado privado o público).
    BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    var campo = GetType().GetField("bonusAtaque", flags);
    if (campo != null && (campo.FieldType == typeof(int) || campo.FieldType == typeof(float)))
    {
      object val = campo.GetValue(this);
      if (val is int i) return i;
      if (val is float f) return f;
    }

    var prop = GetType().GetProperty("bonusAtaque", flags);
    if (prop != null && prop.CanRead)
    {
      object val = prop.GetValue(this, null);
      if (val is int i) return i;
      if (val is float f) return f;
    }

    return 0f;
  }

}

static class MeleeTimingUtility
{
  const float FraccionPosePreImpactoBase = 0.35f;
  const float FallbackPreImpactoSeg = 0.22f;
  const float MinPreImpactoSeg = 0.08f;
  const float MaxPreImpactoSeg = 0.42f;
  const float PostImpactoSegBase = 0.16f;
  const float MinPostImpactoSeg = 0.05f;
  const float MaxPostImpactoSeg = 0.3f;

  public static int CalcularPreImpactoMs(
    UnidadPoseController poseController,
    float fraccionPose = FraccionPosePreImpactoBase,
    float fallbackSeg = FallbackPreImpactoSeg,
    float minimoSeg = MinPreImpactoSeg,
    float maximoSeg = MaxPreImpactoSeg)
  {
    float fallback = poseController != null ? poseController.meleePreImpactoFallback : fallbackSeg;
    float duracionPose = poseController != null ? poseController.duracionPoseAtacar : fallback;
    float fraccion = poseController != null ? poseController.meleeFraccionImpacto : fraccionPose;
    float minimo = poseController != null ? poseController.meleePreImpactoMin : minimoSeg;
    float maximo = poseController != null ? poseController.meleePreImpactoMax : maximoSeg;
    float preImpactoSeg = duracionPose > 0.01f ? duracionPose * fraccion : fallback;
    preImpactoSeg = Mathf.Clamp(preImpactoSeg, Mathf.Min(minimo, maximo), Mathf.Max(minimo, maximo));
    return Mathf.Max(0, Mathf.RoundToInt(preImpactoSeg * 1000f));
  }

  public static int CalcularPostImpactoMs(
    UnidadPoseController poseController = null,
    float postImpactoSeg = PostImpactoSegBase,
    float minimoSeg = MinPostImpactoSeg,
    float maximoSeg = MaxPostImpactoSeg)
  {
    float postImpacto = poseController != null ? poseController.meleePostImpacto : postImpactoSeg;
    float minimo = poseController != null ? poseController.meleePostImpactoMin : minimoSeg;
    float maximo = poseController != null ? poseController.meleePostImpactoMax : maximoSeg;
    float clamped = Mathf.Clamp(postImpacto, Mathf.Min(minimo, maximo), Mathf.Max(minimo, maximo));
    return Mathf.Max(0, Mathf.RoundToInt(clamped * 1000f));
  }
}



