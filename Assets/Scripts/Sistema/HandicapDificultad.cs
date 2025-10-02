using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handicap global de dificultad por lado (Enemigos vs Aliados)
// Ajusta directamente los "mod_" de Unidad cuando una unidad aparece en una casilla.
// No agrega Buffs ni UI; efecto invisible.
namespace Sistema
{
  [AddComponentMenu("Sistema/Handicap Dificultad")]
  public class HandicapDificultad : MonoBehaviour
  {
    public static HandicapDificultad Instance { get; private set; }

    [Header("General")]
    public bool habilitado = true;
    public bool aplicarAAliados = true;
    public bool aplicarAEnemigos = true;
    [Tooltip("Evita re-aplicar al moverse entre casillas.")]
    public bool soloUnaVezPorUnidad = true;
    [Tooltip("Al aumentar el Max HP, ajusta HP actual para no mostrar herido.")]
    public bool ajustarHPActualConMax = true;
    [Tooltip("Si baja el Max AP, recorta AP actual si lo excede.")]
    public bool clipearAPActualAlMaxNuevo = true;

    [System.Serializable]
    public class Ajustes
    {
      [Header("Vida y Acción")]
      public float hpMaxPct;   // +% Max HP
      public float hpMaxFlat;  // + Max HP
      public float apMaxPct;   // +% Max AP
      public float apMaxFlat;  // + Max AP
      public float valentiaMaxPct; // +% Máx Valentía
      public float valentiaMaxFlat; // + Máx Valentía

      [Header("Básicos de Combate")]
      public float ataquePct;
      public float ataqueFlat;
      public float defensaPct;
      public float defensaFlat;
      public float armaduraPct;
      public float armaduraFlat;
      public float iniciativaPct;
      public float iniciativaFlat;

      [Header("Crítico")]
      public float critDanoPct;   // +% Daño crítico
      public float critDanoFlat;  // + Daño crítico
      public float critRangoFlat; // + Rango de crítico (plano)

      [Header("Resistencias (todas)")]
      public float resistAllPct;
      public float resistAllFlat;

      [Header("Tiradas de Salvación (todas)")]
      public float tsAllPct;
      public float tsAllFlat;

      [Header("Daño % global")]
      public float danioPorcPct;
      public float danioPorcFlat;
    }

    [Header("Aliados (lado = 2)")]
    public Ajustes ajustesAliados = new Ajustes();

    [Header("Enemigos (lado = 1)")]
    public Ajustes ajustesEnemigos = new Ajustes();

    public enum DificultadPreset { MuyFacil = 1, Facil = 2, Default = 3, Dificil = 4, MuyDificil = 5 }
    [Header("Preset de Dificultad (se suma a ajustes manuales de ENEMIGOS)")]
    public DificultadPreset presetSeleccionado = DificultadPreset.Default;
    [Tooltip("Si está activo, el preset se SUMA a los ajustes de enemigos al aplicar el handicap (no los sobreescribe). Aliados no son afectados por el preset.")]
    public bool usarPresets = true;

    private readonly HashSet<int> _aplicadosAliados = new HashSet<int>();
    private readonly HashSet<int> _aplicadosEnemigos = new HashSet<int>();

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(this);
        return;
      }
      Instance = this;
    }

    private void Start()
    {
      // Barrido inicial para unidades ya colocadas en escena
      StartCoroutine(ApplyInitialSweep());
    }

    private IEnumerator ApplyInitialSweep()
    {
      // Espera un frame para permitir que BattleManager/escena acomoden referencias
      yield return null;
      yield return null;

      if (!habilitado) yield break;

      var unidades = FindObjectsOfType<Unidad>();
      foreach (var u in unidades)
      {
        if (u != null && u.CasillaPosicion != null)
        {
          int lado = u.CasillaPosicion.lado;
          AplicarSiCorresponde(u, lado);
        }
      }
    }

    public void AplicarSiCorresponde(Unidad u, int lado)
    {
      if (!habilitado || u == null) return;

      bool esEnemigo = (lado == 1);
      if (esEnemigo && !aplicarAEnemigos) return;
      if (!esEnemigo && !aplicarAAliados) return;

      if (soloUnaVezPorUnidad)
      {
        int id = u.GetInstanceID();
        var set = esEnemigo ? _aplicadosEnemigos : _aplicadosAliados;
        if (set.Contains(id)) return;
        set.Add(id);
      }

      Ajustes aBase = esEnemigo ? ajustesEnemigos : ajustesAliados;
      if (esEnemigo && usarPresets)
      {
        var aPreset = ObtenerAjustesPreset(presetSeleccionado);
        var aSum = SumarAjustes(aBase, aPreset);
        AplicarAjustes(u, aSum);
      }
      else
      {
        AplicarAjustes(u, aBase);
      }
    }

    private static float Ajuste(float baseVal, float pct, float flat)
    {
      return baseVal * (1f + pct / 100f) + flat;
    }

    private void AplicarAjustes(Unidad u, Ajustes a)
    {
      // Max HP y HP actual
      float oldMaxHP = u.mod_maxHP;
      u.mod_maxHP = Mathf.Max(1f, Ajuste(u.mod_maxHP, a.hpMaxPct, a.hpMaxFlat));
      if (ajustarHPActualConMax)
      {
        float delta = u.mod_maxHP - oldMaxHP;
        u.HP_actual = Mathf.Clamp(u.HP_actual + delta, 0f, u.mod_maxHP);
      }
      else
      {
        u.HP_actual = Mathf.Min(u.HP_actual, u.mod_maxHP);
      }

      // Max AP y AP actual (clamp opcional)
      float newMaxAP = Mathf.Max(0f, Ajuste(u.mod_maxAccionP, a.apMaxPct, a.apMaxFlat));
      u.mod_maxAccionP = newMaxAP;
      if (clipearAPActualAlMaxNuevo)
      {
        int apActualInt = Mathf.FloorToInt(u.ObtenerAPActual());
        if (apActualInt > newMaxAP)
        {
          u.EstablecerAPActualA(Mathf.FloorToInt(newMaxAP));
        }
      }

      // Valentía (PM)
      u.mod_maxValentiaP = Ajuste(u.mod_maxValentiaP, a.valentiaMaxPct, a.valentiaMaxFlat);
      u.ValentiaP_actual = Mathf.Min(u.ValentiaP_actual, u.mod_maxValentiaP);

      // Iniciativa
      u.mod_iniciativa = Ajuste(u.mod_iniciativa, a.iniciativaPct, a.iniciativaFlat);
      u.iniciativa_actual = u.mod_iniciativa;

      // Básicos
      u.mod_Ataque = Ajuste(u.mod_Ataque, a.ataquePct, a.ataqueFlat);
      u.mod_Defensa = Ajuste(u.mod_Defensa, a.defensaPct, a.defensaFlat);
      u.mod_Armadura = Ajuste(u.mod_Armadura, a.armaduraPct, a.armaduraFlat);

      // Crítico
      u.mod_CriticoDañoBonus = Ajuste(u.mod_CriticoDañoBonus, a.critDanoPct, a.critDanoFlat);
      u.mod_CriticoRangoDado += a.critRangoFlat;

      // Resistencias (todas)
      u.mod_ResFuego = Ajuste(u.mod_ResFuego, a.resistAllPct, a.resistAllFlat);
      u.mod_ResHielo = Ajuste(u.mod_ResHielo, a.resistAllPct, a.resistAllFlat);
      u.mod_ResRayo = Ajuste(u.mod_ResRayo, a.resistAllPct, a.resistAllFlat);
      u.mod_ResAcido = Ajuste(u.mod_ResAcido, a.resistAllPct, a.resistAllFlat);
      u.mod_ResArcano = Ajuste(u.mod_ResArcano, a.resistAllPct, a.resistAllFlat);
      u.mod_ResNecro = Ajuste(u.mod_ResNecro, a.resistAllPct, a.resistAllFlat);
      u.mod_ResDivino = Ajuste(u.mod_ResDivino, a.resistAllPct, a.resistAllFlat);

      // TS (todas)
      u.mod_TSReflejos = Ajuste(u.mod_TSReflejos, a.tsAllPct, a.tsAllFlat);
      u.mod_TSFortaleza = Ajuste(u.mod_TSFortaleza, a.tsAllPct, a.tsAllFlat);
      u.mod_TSMental = Ajuste(u.mod_TSMental, a.tsAllPct, a.tsAllFlat);

      // Daño % global
      u.mod_DanioPorcentaje = Ajuste(u.mod_DanioPorcentaje, a.danioPorcPct, a.danioPorcFlat);
    }

    public void EstablecerDificultadCombate(int nivel)
    {
      if (nivel < 1) nivel = 1; if (nivel > 5) nivel = 5;
      EstablecerDificultadCombate((DificultadPreset)nivel);
    }

    public void EstablecerDificultadCombate(DificultadPreset preset)
    {
      presetSeleccionado = preset;
    }

    private Ajustes ObtenerAjustesPreset(DificultadPreset preset)
    {
      // Solo tocamos: HP Max, Ataque, Daño% y TS (todas).
      var a = new Ajustes();
      switch (preset)
      {
        case DificultadPreset.MuyFacil:
          a.hpMaxPct = -20f;
          a.ataqueFlat = -2f;
          a.danioPorcPct = -15f;
          a.tsAllFlat = -2f;
          break;
        case DificultadPreset.Facil:
          a.hpMaxPct = -10f;
          a.ataqueFlat = -1f;
          a.danioPorcPct = -7f;
          a.tsAllFlat = -1f;
          break;
        case DificultadPreset.Default:
          break;
        case DificultadPreset.Dificil:
          a.hpMaxPct = 10f;
          a.ataqueFlat = 1f;
          a.danioPorcPct = 7f;
          a.tsAllFlat = 1f;
          break;
        case DificultadPreset.MuyDificil:
          a.hpMaxPct = 20f;
          a.ataqueFlat = 2f;
          a.danioPorcPct = 15f;
          a.tsAllFlat = 2f;
          break;
      }
      return a;
    }

    private Ajustes SumarAjustes(Ajustes a, Ajustes b)
    {
      var r = new Ajustes();
      // Vida/Acción
      r.hpMaxPct = a.hpMaxPct + b.hpMaxPct; r.hpMaxFlat = a.hpMaxFlat + b.hpMaxFlat;
      r.apMaxPct = a.apMaxPct + b.apMaxPct; r.apMaxFlat = a.apMaxFlat + b.apMaxFlat;
      r.valentiaMaxPct = a.valentiaMaxPct + b.valentiaMaxPct; r.valentiaMaxFlat = a.valentiaMaxFlat + b.valentiaMaxFlat;
      // Básicos
      r.ataquePct = a.ataquePct + b.ataquePct; r.ataqueFlat = a.ataqueFlat + b.ataqueFlat;
      r.defensaPct = a.defensaPct + b.defensaPct; r.defensaFlat = a.defensaFlat + b.defensaFlat;
      r.armaduraPct = a.armaduraPct + b.armaduraPct; r.armaduraFlat = a.armaduraFlat + b.armaduraFlat;
      r.iniciativaPct = a.iniciativaPct + b.iniciativaPct; r.iniciativaFlat = a.iniciativaFlat + b.iniciativaFlat;
      // Crítico
      r.critDanoPct = a.critDanoPct + b.critDanoPct; r.critDanoFlat = a.critDanoFlat + b.critDanoFlat;
      r.critRangoFlat = a.critRangoFlat + b.critRangoFlat;
      // Resistencias
      r.resistAllPct = a.resistAllPct + b.resistAllPct; r.resistAllFlat = a.resistAllFlat + b.resistAllFlat;
      // TS
      r.tsAllPct = a.tsAllPct + b.tsAllPct; r.tsAllFlat = a.tsAllFlat + b.tsAllFlat;
      // Daño%
      r.danioPorcPct = a.danioPorcPct + b.danioPorcPct; r.danioPorcFlat = a.danioPorcFlat + b.danioPorcFlat;
      return r;
    }
  }
}
