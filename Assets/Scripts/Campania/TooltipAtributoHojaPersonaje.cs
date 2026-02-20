using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class TooltipAtributoHojaPersonaje : MonoBehaviour
{
  public enum TipoAtributo
  {
    Fuerza = 1,
    Agilidad = 2,
    Poder = 3,
    Iniciativa = 4,
    PA = 5,
    Valentia = 6,
    Armadura = 7,
    Defensa = 8,
    TSReflejos = 9,
    TSFortaleza = 10,
    TSMental = 11,
    ResFuego = 12,
    ResRayo = 13,
    ResHielo = 14,
    ResArcano = 15,
    ResAcido = 16,
    ResNecro = 17,
    ResDivino = 18,
    Vida = 19,
    Personalizado = 99
  }

  [Header("Configuracion")]
  public TipoAtributo atributo = TipoAtributo.Fuerza;
  [TextArea(3, 8)] public string textoPersonalizadoES = "";
  [TextArea(3, 8)] public string textoPersonalizadoEN = "";
  public bool usarPosicionMouse = true;

  public void Hover(int estado)
  {
    if (estado == 1) { MostrarTooltip(); }
    else { OcultarTooltip(); }
  }

  public void MostrarTooltip()
  {
    if (TooltipStats.Instance == null)
    {
      return;
    }

    string texto = ObtenerTextoTooltip();
    if (string.IsNullOrWhiteSpace(texto))
    {
      return;
    }

    Vector3 posicion = usarPosicionMouse ? Input.mousePosition : transform.position;
    TooltipStats.Instance.ShowTooltip(texto, posicion);
  }

  public void MostrarTooltip(BaseEventData _eventData)
  {
    MostrarTooltip();
  }

  public void OcultarTooltip()
  {
    if (TooltipStats.Instance == null)
    {
      return;
    }

    TooltipStats.Instance.HideTooltip();
  }

  public void OcultarTooltip(BaseEventData _eventData)
  {
    OcultarTooltip();
  }

  string ObtenerTextoTooltip()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;


    return esIngles ? textoPersonalizadoEN : textoPersonalizadoES;
  }
  


}
