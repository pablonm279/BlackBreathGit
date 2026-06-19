using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipConsumibleBatalla : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] private int slotConsumible = 1;
  private bool tooltipConsumibleVisible;

  public void ConfigurarSlot(int slot)
  {
    slotConsumible = slot;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    Unidad unidad = BattleManager.Instance != null ? BattleManager.Instance.unidadActiva : null;
    if (unidad == null)
    {
      return;
    }

    Consumible consumible = slotConsumible == 1 ? unidad.ConsumibleA : unidad.ConsumibleB;
    if (consumible == null)
    {
      return;
    }

    string texto = ItemTooltipFormatter.ConstruirTooltipSoloEfectos(consumible);
    if (string.IsNullOrWhiteSpace(texto))
    {
      return;
    }

    TooltipBatalla.Instance?.ShowTooltipTextSinAnimDirecto(texto);
    tooltipConsumibleVisible = true;
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    OcultarTooltipConsumible();
  }

  private void OnDisable()
  {
    OcultarTooltipConsumible();
  }

  private void OcultarTooltipConsumible()
  {
    if (!tooltipConsumibleVisible)
    {
      return;
    }

    TooltipBatalla.Instance?.HideTooltipSinAnim();
    tooltipConsumibleVisible = false;
  }
}
