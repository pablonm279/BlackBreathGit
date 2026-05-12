using UnityEngine;
using UnityEngine.EventSystems;

public class EquipoSlotPointerHandler : MonoBehaviour
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
  private bool callbacksRegistrados;

  public void Configurar(MenuPersonajes menu, TipoSlot slot)
  {
    menuPersonajes = menu;
    tipoSlot = slot;
    RegistrarCallbacksSiHaceFalta();
  }

  public void ManejarPointerDown(BaseEventData eventDataBase)
  {
    PointerEventData eventData = eventDataBase as PointerEventData;
    if (eventData != null && eventData.button == PointerEventData.InputButton.Right && menuPersonajes != null)
    {
      menuPersonajes.RegistrarClickDerechoEnSlotEquipo();
    }
  }

  public void ManejarPointerClick(BaseEventData eventDataBase)
  {
    PointerEventData eventData = eventDataBase as PointerEventData;
    if (eventData == null || eventData.button != PointerEventData.InputButton.Right || menuPersonajes == null)
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

  private void RegistrarCallbacksSiHaceFalta()
  {
    if (callbacksRegistrados)
    {
      return;
    }

    EventTrigger trigger = GetComponent<EventTrigger>();
    if (trigger == null)
    {
      trigger = gameObject.AddComponent<EventTrigger>();
    }

    RegistrarEntrada(trigger, EventTriggerType.PointerDown, ManejarPointerDown);
    RegistrarEntrada(trigger, EventTriggerType.PointerClick, ManejarPointerClick);
    callbacksRegistrados = true;
  }

  private static void RegistrarEntrada(EventTrigger trigger, EventTriggerType tipo, UnityEngine.Events.UnityAction<BaseEventData> accion)
  {
    if (trigger == null)
    {
      return;
    }

    EventTrigger.Entry entrada = new EventTrigger.Entry();
    entrada.eventID = tipo;
    entrada.callback.AddListener(accion);
    trigger.triggers.Add(entrada);
  }
}
