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
        nombre = TextoPorIdioma("Herido", "Injured", "Ferido");
        descripcion = TextoPorIdioma(
          "-1 Atributos. Si cae en combate muere. Curación diaria reducida.",
          "-1 Attributes. If falls in combat, dies. Reduced daily healing.",
          "-1 Atributos. Se cair em combate, morre. Cura diária reduzida.");
        break;

      case TipoEstadoCampania.Corrupto:
        nombre = TextoPorIdioma("Corrupto", "Corrupted", "Corrompido");
        descripcion = TextoPorIdioma(
          "Recibe daño adicional de enemigos Corrompidos. Si un Corrompido lo deja fuera de combate, muere.",
          "Takes extra damage from Corrupted enemies. If a Corrupted enemy defeats them, they die.",
          "Recebe dano extra de inimigos Corrompidos. Se um Corrompido o derrotar, ele morre.");
        break;

      case TipoEstadoCampania.Enfermo:
        nombre = TextoPorIdioma("Enfermo", "Sick", "Doente");
        descripcion = TextoPorIdioma(
          "En batalla: -3 TS Fortaleza, -15% daño y -1 PA máximo.",
          "In battle: -3 Fortitude Save, -15% damage and -1 max AP.",
          "Em batalha: -3 TS Fortaleza, -15% dano e -1 PA máximo.");
        if (personaje != null && personaje.Camp_Enfermo > 0)
        {
          detalle = "\n" + TextoPorIdioma("Duración restante: ", "Remaining duration: ", "Duração restante: ") + personaje.Camp_Enfermo;
        }
        break;

      case TipoEstadoCampania.BajaMoral:
        nombre = TextoPorIdioma("Baja Moral", "Low Morale", "Moral Baixa");
        descripcion = TextoPorIdioma(
          "En batalla: -3 TS Mental, -1 Defensa, -1 Ataque y -2 Valentía.",
          "In battle: -3 Mental Save, -1 Defense, -1 Attack and -2 Valour.",
          "Em batalha: -3 TS Mental, -1 Defesa, -1 Ataque e -2 Valentia.");
        break;

      case TipoEstadoCampania.AltaMoral:
        nombre = TextoPorIdioma("Alta Moral", "High Morale", "Moral Alta");
        descripcion = TextoPorIdioma(
          "En batalla: +2 TS Mental, +1 Ataque y +2 Valentía.",
          "In battle: +2 Mental Save, +1 Attack and +2 Valour.",
          "Em batalha: +2 TS Mental, +1 Ataque e +2 Valentia.");
        break;

      case TipoEstadoCampania.Fatigado:
        nombre = TextoPorIdioma("Fatigado", "Fatigued", "Fatigado");
        descripcion = TextoPorIdioma(
          "-1 Atributos. Se limpia al descansar.",
          "-1 Attributes. Removed by resting.",
          "-1 Atributos. É removido ao descansar.");
        break;

      case TipoEstadoCampania.Bendecido:
        nombre = TextoPorIdioma("Bendecido", "Blessed", "Abençoado");
        descripcion = TextoPorIdioma(
          "En batalla: +3 a todas las TS y +5 Resistencia Necrótica.",
          "In battle: +3 to all Saves and +5 Necrotic Resistance.",
          "Em batalha: +3 em todos os TS e +5 Resistência Necrótica.");
        if (personaje != null && personaje.Camp_Bendecido > 0)
        {
          detalle = "\n" + TextoPorIdioma("Duración restante: ", "Remaining duration: ", "Duração restante: ") + personaje.Camp_Bendecido;
        }
        break;

      case TipoEstadoCampania.Avergonzado:
        nombre = TextoPorIdioma("Avergonzado", "Ashamed", "Envergonhado");
        descripcion = TextoPorIdioma(
          "En batalla: -2 TS Mental y -2 Val.",
          "In battle: -2 Mental Save and -2 Valour.",
          "Em batalha: -2 TS Mental e -2 Valentia.");
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

  private static string TextoPorIdioma(string textoEs, string textoEn, string textoPt)
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

    return textoEs;
  }
}
