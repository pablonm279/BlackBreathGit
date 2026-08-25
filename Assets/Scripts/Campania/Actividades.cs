using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Actividades : MonoBehaviour
{
  private static readonly Regex RichTextRegex = new Regex("<.*?>", RegexOptions.Compiled);
  private const string ColorTituloActividad = "#11d4a0";
  private const string ColorCuerpoPrincipalActividad = "#e9e4da";
  private const string ColorCuerpoSecundarioActividad = "#9c7c63";

  public MenuPersonajes scMenuPersonajes;
  public GameObject prefabBtnActividad;
  public Transform listaActividades;
  public TextMeshProUGUI textdesc;
  [SerializeField] private Image imagenActividadSeleccionada;

  public Sprite spriteActDescansar;
  public Sprite spriteActEntrenar;
  public Sprite spriteActGuardia;

  public Sprite spriteActCaballeroRelatosBatalla;
  public Sprite spriteActCaballeroMantenimientoArmadura;
  public Sprite spriteActCaballeroVigilar;
  public Sprite spriteActCazadorCazaNocturna;
  public Sprite spriteActPrepararFlechas;
  public Sprite spriteActExploracion;
  public Sprite spritePurificadoraRitualDeLimpieza;
  public Sprite spritePurificadoraAyudarDesamparados;
  public Sprite spritePurificadoraColaborarCuranderos;
  public Sprite spriteAfilarArmas;
  public Sprite spriteVigilarDesdeSombras;
  public Sprite spriteActCoercion;
  public Sprite spriteActConcArcana;
  public Sprite spriteActTelekinesis;
  public Sprite spriteActSimboloArcanoProt;
  public Sprite spriteActDuelistaSiempreAlerta;
  public Sprite spriteActDuelistaSocializar;
  public Sprite spriteActDuelistaConsuelo;

  void Awake()
  {
    AsegurarImagenActividadSeleccionada();

    if (spriteActDuelistaSiempreAlerta == null)
    {
      spriteActDuelistaSiempreAlerta = Resources.Load<Sprite>("Imagenes/actividades_siemprealerta");
    }

    if (spriteActDuelistaSocializar == null)
    {
      spriteActDuelistaSocializar = Resources.Load<Sprite>("Imagenes/actividades_socializar");
    }

    if (spriteActDuelistaConsuelo == null)
    {
      spriteActDuelistaConsuelo = Resources.Load<Sprite>("Imagenes/actividades_consuelo");
    }
  }

  public void ActualizarActividades()
  {
    DesactivarLayoutAutomatico();
    List<btnActividad> slots = ObtenerSlotsActividad();

    if (scMenuPersonajes == null || scMenuPersonajes.pSel == null)
    {
      LimpiarSlotsActividad(slots);
      ActualizarImagenActividadSeleccionada(null);
      return;
    }

    if (!scMenuPersonajes.pSel.PuedeRealizarActividades())
    {
      LimpiarSlotsActividad(slots);
      scMenuPersonajes.pSel.ActividadSeleccionada = 1;
      if (textdesc != null)
      {
        string textoBloqueado = TRADU.i != null
          ? TRADU.i.Traducir("Este personaje no puede realizar actividades ahora. Descansa.")
          : "Este personaje no puede realizar actividades ahora. Descansa.";
        textdesc.text = FormatearDescripcionPanel(textoBloqueado);
      }

      if (CampaignManager.Instance != null)
      {
        CampaignManager.Instance.GetCapacidadDeCargaActual();
        CampaignManager.Instance.CambiarBueyesActuales(0);
      }

      ActualizarImagenActividadSeleccionada(ObtenerSpriteActividad(1));
      return;
    }

    List<Actividad> actividadesOrdenadas = ObtenerActividadesOrdenadas(scMenuPersonajes.pSel);
    for (int i = 0; i < slots.Count; i++)
    {
      btnActividad slot = slots[i];
      if (slot == null)
      {
        continue;
      }

      if (i < actividadesOrdenadas.Count)
      {
        ConfigurarSlotActividad(slot, actividadesOrdenadas[i]);
      }
      else
      {
        LimpiarSlotActividad(slot);
      }
    }

    ActualizarRecuadros();
  }

  private void DesactivarLayoutAutomatico()
  {
    if (listaActividades == null)
    {
      return;
    }

    LayoutGroup layout = listaActividades.GetComponent<LayoutGroup>();
    if (layout != null)
    {
      layout.enabled = false;
    }

    ContentSizeFitter fitter = listaActividades.GetComponent<ContentSizeFitter>();
    if (fitter != null)
    {
      fitter.enabled = false;
    }
  }

  private void AsegurarImagenActividadSeleccionada()
  {
    if (imagenActividadSeleccionada != null)
    {
      return;
    }

    Transform raizBusqueda = listaActividades != null && listaActividades.parent != null
      ? listaActividades.parent
      : transform;

    if (raizBusqueda == null)
    {
      return;
    }

    Transform encontrada = raizBusqueda.Find("ActividadSeleccionada");
    if (encontrada == null)
    {
      encontrada = listaActividades != null ? listaActividades.Find("ActividadSeleccionada") : null;
    }

    if (encontrada == null)
    {
      encontrada = transform.Find("ActividadSeleccionada");
    }

    if (encontrada != null)
    {
      imagenActividadSeleccionada = encontrada.GetComponent<Image>();
    }
  }

  private void ActualizarImagenActividadSeleccionada(Sprite sprite)
  {
    AsegurarImagenActividadSeleccionada();
    if (imagenActividadSeleccionada == null)
    {
      return;
    }

    imagenActividadSeleccionada.sprite = sprite;
    imagenActividadSeleccionada.enabled = sprite != null;
  }

  private List<btnActividad> ObtenerSlotsActividad()
  {
    List<btnActividad> slots = new List<btnActividad>();
    if (listaActividades == null)
    {
      return slots;
    }

    foreach (Transform child in listaActividades)
    {
      if (child != null && child.name == "ActividadSeleccionada")
      {
        continue;
      }

      btnActividad slot = child.GetComponent<btnActividad>();
      if (slot == null)
      {
        slot = child.GetComponentInChildren<btnActividad>(true);
      }

      if (slot == null && prefabBtnActividad != null)
      {
        GameObject instancia = Instantiate(prefabBtnActividad, child, false);
        instancia.name = prefabBtnActividad.name;
        slot = instancia.GetComponent<btnActividad>();
      }

      if (slot != null)
      {
        slots.Add(slot);
      }
    }

    slots.Sort((a, b) =>
    {
      if (a == null && b == null)
      {
        return 0;
      }

      if (a == null)
      {
        return 1;
      }

      if (b == null)
      {
        return -1;
      }

      Vector3 posA = a.transform.position;
      Vector3 posB = b.transform.position;
      int compararY = posB.y.CompareTo(posA.y);
      return compararY != 0 ? compararY : posA.x.CompareTo(posB.x);
    });

    return slots;
  }

  private void LimpiarSlotsActividad(List<btnActividad> slots)
  {
    foreach (btnActividad slot in slots)
    {
      LimpiarSlotActividad(slot);
    }
  }

  private void LimpiarSlotActividad(btnActividad slot)
  {
    if (slot == null)
    {
      return;
    }

    slot.actividadRepresentada = null;
    slot.personajeSeleccionado = null;
    slot.scActividades = this;
    if (slot.actImage != null)
    {
      slot.actImage.sprite = null;
    }

    if (slot.Recuadro != null)
    {
      slot.Recuadro.SetActive(false);
    }

    slot.gameObject.SetActive(false);
  }

  private void ConfigurarSlotActividad(btnActividad slot, Actividad actividad)
  {
    if (slot == null)
    {
      return;
    }

    slot.gameObject.SetActive(true);
    slot.actividadRepresentada = actividad;
    slot.personajeSeleccionado = scMenuPersonajes != null ? scMenuPersonajes.pSel : null;
    slot.scActividades = this;
    if (slot.actImage != null)
    {
      slot.actImage.sprite = actividad != null ? ObtenerSpriteActividad(actividad.IDActividad) : null;
    }
  }

  private List<Actividad> ObtenerActividadesOrdenadas(Personaje personaje)
  {
    List<Actividad> generales = new List<Actividad>();
    List<Actividad> exclusivas = new List<Actividad>();

    foreach (Actividad act in personaje.gameObject.GetComponents<Actividad>())
    {
      if (act == null)
      {
        continue;
      }

      if (act.IDActividad >= 1 && act.IDActividad <= 3)
      {
        generales.Add(act);
      }
      else
      {
        exclusivas.Add(act);
      }
    }

    generales.Sort((a, b) => a.IDActividad.CompareTo(b.IDActividad));
    exclusivas.Sort((a, b) => a.IDActividad.CompareTo(b.IDActividad));

    List<Actividad> ordenadas = new List<Actividad>(5);
    ordenadas.AddRange(generales);
    ordenadas.AddRange(exclusivas);
    return ordenadas;
  }

  public void ActualizarRecuadros()
  {
    Sprite spriteSeleccionado = null;

    foreach (btnActividad btn in listaActividades.GetComponentsInChildren<btnActividad>(true))
    {
      if (btn == null || btn.actividadRepresentada == null || !btn.gameObject.activeInHierarchy)
      {
        continue;
      }

      if (scMenuPersonajes.pSel.ActividadSeleccionada == btn.actividadRepresentada.IDActividad)
      {
        btn.Recuadro.SetActive(true);
        spriteSeleccionado = btn.actImage != null ? btn.actImage.sprite : ObtenerSpriteActividad(btn.actividadRepresentada.IDActividad);
        if (textdesc != null)
        {
          textdesc.text = FormatearDescripcionPanel(
            btn.actividadRepresentada.ObtenerDescripcion(scMenuPersonajes.pSel));
        }
      }
      else
      {
        btn.Recuadro.SetActive(false);
      }

      CampaignManager.Instance.GetCapacidadDeCargaActual();
      CampaignManager.Instance.CambiarBueyesActuales(0);
    }

    if (spriteSeleccionado == null && scMenuPersonajes != null && scMenuPersonajes.pSel != null)
    {
      spriteSeleccionado = ObtenerSpriteActividad(scMenuPersonajes.pSel.ActividadSeleccionada);
    }

    ActualizarImagenActividadSeleccionada(spriteSeleccionado);
  }

  public Actividad ObtenerActividadActual(Personaje personaje)
  {
    if (personaje == null)
    {
      return null;
    }

    int idActividadBuscada = personaje.PuedeRealizarActividades() ? personaje.ActividadSeleccionada : 1;
    Actividad actividadFallback = null;

    foreach (Actividad act in personaje.gameObject.GetComponents<Actividad>())
    {
      if (act == null)
      {
        continue;
      }

      if (actividadFallback == null || act.IDActividad == 1)
      {
        actividadFallback = act;
      }

      if (act.IDActividad == idActividadBuscada)
      {
        return act;
      }
    }

    return actividadFallback;
  }

  public Sprite ObtenerSpriteActividad(int idActividad)
  {
    switch (idActividad)
    {
      case 1: return spriteActDescansar;
      case 2: return spriteActEntrenar;
      case 3: return spriteActGuardia;
      case 4: return spriteActCaballeroRelatosBatalla;
      case 5: return spriteActCaballeroMantenimientoArmadura;
      case 6: return spriteActCaballeroVigilar;
      case 7: return spriteActCazadorCazaNocturna;
      case 8: return spriteActPrepararFlechas;
      case 9: return spriteActExploracion;
      case 10: return spritePurificadoraRitualDeLimpieza;
      case 11: return spritePurificadoraAyudarDesamparados;
      case 12: return spritePurificadoraColaborarCuranderos;
      case 13: return spriteAfilarArmas;
      case 14: return spriteVigilarDesdeSombras;
      case 15: return spriteActCoercion;
      case 16: return spriteActConcArcana;
      case 17: return spriteActTelekinesis;
      case 18: return spriteActSimboloArcanoProt;
      case 19: return spriteActDuelistaSiempreAlerta ?? spriteActGuardia;
      case 20: return spriteActDuelistaSocializar ?? spriteActGuardia;
      case 21: return spriteActDuelistaConsuelo ?? spriteActGuardia;
      default: return null;
    }
  }

  public string FormatearDescripcionPanel(string descripcionOriginal)
  {
    if (string.IsNullOrWhiteSpace(descripcionOriginal))
    {
      return string.Empty;
    }

    string textoPlano = RichTextRegex.Replace(descripcionOriginal, string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(textoPlano))
    {
      return string.Empty;
    }

    int indiceSeparador = textoPlano.IndexOf(':');
    if (indiceSeparador < 0)
    {
      return textoPlano;
    }

    string titulo = textoPlano.Substring(0, indiceSeparador).Trim().ToUpperInvariant();
    string cuerpo = textoPlano.Substring(indiceSeparador + 1).Trim();

    if (string.IsNullOrWhiteSpace(cuerpo))
    {
      return "<color=" + ColorTituloActividad + "><b>" + titulo + "</b></color>";
    }

    string[] bloques = cuerpo.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
    string primerCuerpo = bloques.Length > 0 ? bloques[0].Trim() : string.Empty;
    string segundoCuerpo = bloques.Length > 1 ? string.Join("\n\n", bloques, 1, bloques.Length - 1).Trim() : string.Empty;

    string resultado = "<color=" + ColorTituloActividad + "><b>" + titulo + "</b></color>";

    if (!string.IsNullOrWhiteSpace(primerCuerpo))
    {
      resultado += "\n\n<color=" + ColorCuerpoPrincipalActividad + ">" + primerCuerpo + "</color>";
    }

    if (!string.IsNullOrWhiteSpace(segundoCuerpo))
    {
      resultado += "\n\n<color=" + ColorCuerpoSecundarioActividad + ">" + segundoCuerpo + "</color>";
    }

    return resultado;
  }

}
