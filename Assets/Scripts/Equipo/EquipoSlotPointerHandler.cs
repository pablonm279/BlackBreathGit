using UnityEngine;
using UnityEngine.EventSystems;

public class EquipoSlotPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
  public enum TipoSlot
  {
    Arma,
    Armadura,
    Accesorio1,
    Accesorio2,
    Consumible1,
    Consumible2
  }

  private MenuPersonajes menuPersonajes;
  private TipoSlot tipoSlot;

  public void Configurar(MenuPersonajes menu, TipoSlot slot)
  {
    menuPersonajes = menu;
    tipoSlot = slot;
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if (eventData.button == PointerEventData.InputButton.Right && menuPersonajes != null)
    {
      menuPersonajes.RegistrarClickDerechoEnSlotEquipo();
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Right || menuPersonajes == null)
    {
      return;
    }

    switch (tipoSlot)
    {
      case TipoSlot.Arma:
        menuPersonajes.OnRightClickArma();
        break;
      case TipoSlot.Armadura:
        menuPersonajes.OnRightClickArmadura();
        break;
      case TipoSlot.Accesorio1:
        menuPersonajes.OnRightClickAccesorio1();
        break;
      case TipoSlot.Accesorio2:
        menuPersonajes.OnRightClickAccesorio2();
        break;
      case TipoSlot.Consumible1:
        menuPersonajes.OnRightClickConsumible1();
        break;
      case TipoSlot.Consumible2:
        menuPersonajes.OnRightClickConsumible2();
        break;
    }

    menuPersonajes.LimpiarBloqueoClickDerechoEnSlotEquipo();
  }
}
