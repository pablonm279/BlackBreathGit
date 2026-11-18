using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIEstadoPersonajeCamp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  public enum TipoEstadoCampania
  {
    Herido = 0,
    Corrupto = 1,
    Enfermo = 2,
    BajaMoral = 3,
    AltaMoral = 4,
    Fatigado = 5,
    Bendecido = 6,
    Avergonzado = 7
  }

  [SerializeField] private Image iconoEstado;
  [Header("Sprites por estado")]
  [SerializeField] private Sprite spriteHerido;
  [SerializeField] private Sprite spriteCorrupto;
  [SerializeField] private Sprite spriteEnfermo;
  [SerializeField] private Sprite spriteBajaMoral;
  [SerializeField] private Sprite spriteAltaMoral;
  [SerializeField] private Sprite spriteFatigado;
  [SerializeField] private Sprite spriteBendecido;
  [SerializeField] private Sprite spriteAvergonzado;
  [SerializeField] private TipoEstadoCampania tipoEstado;
  [TextArea(2, 6)] [SerializeField] private string tooltipActual;

  private void Awake()
  {
    AsegurarReferencias();
  }

  public void Representar(TipoEstadoCampania tipo, Personaje personaje)
  {
    AsegurarReferencias();
    tipoEstado = tipo;
    tooltipActual = ConstruirTooltip(tipo, personaje);

    if (iconoEstado != null)
    {
      Sprite sprite = ObtenerSprite(tipo);
      iconoEstado.sprite = sprite;
      iconoEstado.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.2f);
      iconoEstado.preserveAspect = true;
      iconoEstado.raycastTarget = true;
    }

    gameObject.name = ObtenerIdEstado(tipo);
  }

  public void Hover()
  {
    MostrarTooltip();
  }

  public void Hover(BaseEventData _eventData)
  {
    MostrarTooltip();
  }

  public void MouseExit()
  {
    OcultarTooltip();
  }

  public void MouseExit(BaseEventData _eventData)
  {
    OcultarTooltip();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    MostrarTooltip();
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    OcultarTooltip();
  }

  private void OnDisable()
  {
    OcultarTooltip();
  }

  private void AsegurarReferencias()
  {
    if (iconoEstado == null)
    {
      iconoEstado = GetComponent<Image>();
    }
  }

  private void MostrarTooltip()
  {
    if (TooltipStats.Instance == null || string.IsNullOrWhiteSpace(tooltipActual))
    {
      return;
    }

    TooltipStats.Instance.ShowTooltipRaw(tooltipActual, Input.mousePosition);
  }

  private void OcultarTooltip()
  {
    if (TooltipStats.Instance != null)
    {
      TooltipStats.Instance.HideTooltip();
    }
  }

  private string ConstruirTooltip(TipoEstadoCampania tipo, Personaje personaje)
  {
    string nombre;
    string descripcion;
    string detalle = string.Empty;

    switch (tipo)
    {
      case TipoEstadoCampania.Herido:
        nombre = TextoPorIdioma("Herido", "Injured", "Ferido", "受伤");
        descripcion = TextoPorIdioma(
          "-1 Atributos. Si cae en combate muere. Curación por hora reducida.",
          "-1 Attributes. If falls in combat, dies. Reduced hourly healing.",
          "-1 Atributos. Se cair em combate, morre. Cura por hora reduzida.",
          "-1 属性。若在战斗中倒下则死亡。每小时治疗量降低。");
        break;

      case TipoEstadoCampania.Corrupto:
        nombre = TextoPorIdioma("Corrupto", "Corrupted", "Corrompido", "腐化");
        descripcion = TextoPorIdioma(
          "Recibe daño adicional de enemigos Corrompidos. Si un Corrompido lo deja fuera de combate, muere.",
          "Takes extra damage from Corrupted enemies. If a Corrupted enemy defeats them, they die.",
          "Recebe dano extra de inimigos Corrompidos. Se um Corrompido o derrotar, ele morre.",
          "受到腐化敌人的额外伤害。若被腐化者击倒，则死亡。");
        break;

      case TipoEstadoCampania.Enfermo:
        nombre = TextoPorIdioma("Enfermo", "Sick", "Doente", "患病");
        descripcion = TextoPorIdioma(
          "En batalla: -3 TS Fortaleza, -15% daño y -1 PA máximo.",
          "In battle: -3 Fortitude Save, -15% damage and -1 max AP.",
          "Em batalha: -3 TS Fortaleza, -15% dano e -1 PA máximo.",
          "战斗中：坚韧豁免 -3，伤害 -15%，最大行动点 -1。");
        if (personaje != null && personaje.EstaEnfermo())
        {
          detalle = ConstruirDetalleDuracion(personaje.ObtenerHorasRestantesEnfermo());
        }
        break;

      case TipoEstadoCampania.BajaMoral:
        nombre = TextoPorIdioma("Baja Moral", "Low Morale", "Moral Baixa", "士气低落");
        descripcion = TextoPorIdioma(
          "En batalla: -3 TS Mental, -1 Defensa, -1 Ataque y -2 Valentía.",
          "In battle: -3 Mental Save, -1 Defense, -1 Attack and -2 Valour.",
          "Em batalha: -3 TS Mental, -1 Defesa, -1 Ataque e -2 Valentia.",
          "战斗中：心智豁免 -3，防御 -1，攻击 -1，勇气 -2。");
        if (personaje != null)
        {
          detalle = ConstruirDetalleDuracion(personaje.ObtenerHorasRestantesMoral());
        }
        break;

      case TipoEstadoCampania.AltaMoral:
        nombre = TextoPorIdioma("Alta Moral", "High Morale", "Moral Alta", "士气高涨");
        descripcion = TextoPorIdioma(
          "En batalla: +2 TS Mental, +1 Ataque y +2 Valentía.",
          "In battle: +2 Mental Save, +1 Attack and +2 Valour.",
          "Em batalha: +2 TS Mental, +1 Ataque e +2 Valentia.",
          "战斗中：心智豁免 +2，攻击 +1，勇气 +2。");
        if (personaje != null)
        {
          detalle = ConstruirDetalleDuracion(personaje.ObtenerHorasRestantesMoral());
        }
        break;

      case TipoEstadoCampania.Fatigado:
        nombre = TextoPorIdioma("Fatigado", "Fatigued", "Fatigado", "疲劳");
        descripcion = TextoPorIdioma(
          "-1 Atributos. Se limpia al acampar.",
          "-1 Attributes. Removed by camping.",
          "-1 Atributos. É removido ao descansar.",
          "-1 属性。扎营时消除。");
        break;

      case TipoEstadoCampania.Bendecido:
        nombre = TextoPorIdioma("Bendecido", "Blessed", "Abençoado", "祝福");
        descripcion = TextoPorIdioma(
          "En batalla: +3 a todas las TS y +5 Resistencia Necrótica.",
          "In battle: +3 to all Saves and +5 Necrotic Resistance.",
          "Em batalha: +3 em todos os TS e +5 Resistência Necrótica.",
          "战斗中：所有豁免 +3，死灵抗性 +5。");
        if (personaje != null && personaje.EstaBendecido())
        {
          detalle = ConstruirDetalleDuracion(personaje.ObtenerHorasRestantesBendecido());
        }
        break;

      case TipoEstadoCampania.Avergonzado:
        nombre = TextoPorIdioma("Avergonzado", "Ashamed", "Envergonhado", "羞愧");
        descripcion = TextoPorIdioma(
          "En batalla: -2 TS Mental y -2 Val.",
          "In battle: -2 Mental Save and -2 Valour.",
          "Em batalha: -2 TS Mental e -2 Valentia.",
          "战斗中：心智豁免 -2，勇气 -2。");
        break;

      default:
        nombre = tipo.ToString();
        descripcion = string.Empty;
        break;
    }

    return "<b>" + nombre + "</b>\n" + descripcion + detalle;
  }

  private static string ObtenerIdEstado(TipoEstadoCampania tipo)
  {
    switch (tipo)
    {
      case TipoEstadoCampania.Herido: return "camp_herido";
      case TipoEstadoCampania.Corrupto: return "camp_corrupto";
      case TipoEstadoCampania.Enfermo: return "camp_enfermo";
      case TipoEstadoCampania.BajaMoral: return "camp_baja_moral";
      case TipoEstadoCampania.AltaMoral: return "camp_alta_moral";
      case TipoEstadoCampania.Fatigado: return "camp_fatigado";
      case TipoEstadoCampania.Bendecido: return "camp_bendecido";
      case TipoEstadoCampania.Avergonzado: return "camp_avergonzado";
      default: return "camp_estado";
    }
  }

  private static string ConstruirDetalleDuracion(float horas)
  {
    string duracion = CampaignManager.Instance != null
      ? CampaignManager.Instance.FormatearDuracionHoras(horas, true)
      : Mathf.CeilToInt(Mathf.Max(0f, horas)) + " h";
    return "\n" + TextoPorIdioma("Duración restante: ", "Remaining duration: ", "Duração restante: ", "剩余持续时间：") + duracion;
  }

  private Sprite ObtenerSprite(TipoEstadoCampania tipo)
  {
    switch (tipo)
    {
      case TipoEstadoCampania.Herido: return spriteHerido;
      case TipoEstadoCampania.Corrupto: return spriteCorrupto;
      case TipoEstadoCampania.Enfermo: return spriteEnfermo;
      case TipoEstadoCampania.BajaMoral: return spriteBajaMoral;
      case TipoEstadoCampania.AltaMoral: return spriteAltaMoral;
      case TipoEstadoCampania.Fatigado: return spriteFatigado;
      case TipoEstadoCampania.Bendecido: return spriteBendecido;
      case TipoEstadoCampania.Avergonzado: return spriteAvergonzado;
      default: return null;
    }
  }

  private static string TextoPorIdioma(string textoEs, string textoEn, string textoPt, string textoZh = null)
  {
    if (TRADU.i == null)
    {
      return textoEs;
    }

    if (TRADU.i.nIdioma == TRADU.IdiomaIngles)
    {
      return textoEn;
    }

    if (TRADU.i.nIdioma == TRADU.IdiomaPortugues)
    {
      return textoPt;
    }

    if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
    {
      return textoZh ?? textoEn;
    }

    return textoEs;
  }
}
