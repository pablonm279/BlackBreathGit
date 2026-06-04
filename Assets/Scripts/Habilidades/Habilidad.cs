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
  public int cooldownMax;
  public int cooldownActual;
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
  private const string TooltipPifiaId = "combate_pifia";

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

  protected string CostoSuperiorDescripcion()
  {
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
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

    // En combate usar valores vivos con buffs/debuffs.
    if (!EsEscenaCampaña() && scEstaUnidad != null)
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
      if (personaje.Camp_Moral > 0)
      {
        stats.Ataque += 1;
      }
      else if (personaje.Camp_Moral < 0)
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

  // Método abstracto para activar la habilidad.
  public virtual async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {
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
    buffEsfuerzo.buffDescr = "La unidad se ha esforzado. -1 PA máximo y -2 Defensa por stack.";
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

    if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
    {
      iDadoSolo += 5;
      if(iDadoSolo > 20) { iDadoSolo = 20; }
    } //En tutorial no hay pifias

    if (iDadoSolo <= umbralPifia)//Pifia
    {
      if (personajeTraits != null)
      {
        TutorialTooltipManager.TryShow(TooltipPifiaId);
      }

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
          deltaClima));
      if (BattleManager.Instance != null && BattleManager.Instance.HabilidadActiva != null && BattleManager.Instance.HabilidadActiva.esMelee)
      {
        AjustesAudio.ReproducirClipEnPunto(BattleManager.Instance.contenedorPrefabs.sonidoErrar, transform.position);
      }
      if (esMelee && unidadAtacada != null)
      {
        unidadAtacada.AnticiparFalloAtaqueRecibido(scEstaUnidad, true);
      }
      unidadAtacada?.NotificarAtaqueRecibido();
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
          TRADU.i.Traducir("Impacto crítico")));
      unidadAtacada?.NotificarAtaqueRecibido();
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
        deltaClima));


    if (resultado < 2 && BattleManager.Instance.HabilidadActiva.esMelee)
    { AjustesAudio.ReproducirClipEnPunto(BattleManager.Instance.contenedorPrefabs.sonidoErrar, transform.position); }

    unidadAtacada?.NotificarAtaqueRecibido();
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

  /// <summary>
  /// Limpia las unidades marcadas en la ultima previsualizacion de la habilidad.
  /// </summary>
  public void LimpiarMarcasUnidadesPosibles()
  {
    if (unidadesMarcadasPrevisualizacion.Count == 0)
    {
      return;
    }

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



