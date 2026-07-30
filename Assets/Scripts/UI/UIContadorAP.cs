using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIContadorAP : MonoBehaviour
{

    public GameObject circuloAPprefab;
    public GameObject esfuerzoAPprefab;
    private Sprite spriteAPUsado;
    private Sprite spriteEsforzar;
    [SerializeField] private float duracionVaciadoAP = 0.16f;
    [SerializeField] private float demoraEntreVaciadosAP = 0.035f;
    private Unidad unidadRepresentada;
    private int apRepresentado = -1;
    private Coroutine corrutinaVaciadoAP;
 
   
   
    private void Start()
    {
       spriteAPUsado = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/ap_usado");
       spriteEsforzar = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/esforzar");
       ActualizarAPCirculos();

    }

 
    public void ActualizarAPCirculos()
    {
      if (!isActiveAndEnabled || !gameObject.scene.isLoaded)
      {
        return;
      }

      BattleManager battleManager = BattleManager.Instance;
      if (battleManager == null || !battleManager.isActiveAndEnabled)
      {
        return;
      }

      Unidad unidadSeleccionada = battleManager.unidadActiva;
      int apObjetivo = unidadSeleccionada != null
        ? Mathf.Max(0, (int)unidadSeleccionada.ObtenerAPActual())
        : 0;

      bool animarGasto = unidadSeleccionada != null
        && unidadRepresentada == unidadSeleccionada
        && apRepresentado > apObjetivo
        && transform.childCount >= apRepresentado;

      if (animarGasto)
      {
        if (corrutinaVaciadoAP == null)
        {
          corrutinaVaciadoAP = StartCoroutine(
            AnimarVaciadoAP(apObjetivo, apRepresentado - apObjetivo, unidadSeleccionada));
        }
      }
      else if (corrutinaVaciadoAP == null || unidadRepresentada != unidadSeleccionada || apRepresentado != apObjetivo)
      {
        CancelarVaciadoAP();
        ReconstruirCirculos(apObjetivo);
      }

      unidadRepresentada = unidadSeleccionada;
      apRepresentado = apObjetivo;

    // Luego de actualizar la UI, revisar si debe indicarse pasar turno
     battleManager.RevisarAPUnidadActiva();
     battleManager.ActualizarCasillasMelee();
    }

  private IEnumerator AnimarVaciadoAP(int apObjetivo, int cantidadGastada, Unidad unidadAlIniciar)
  {
    int primerIndice = Mathf.Clamp(apObjetivo, 0, transform.childCount);
    int cantidadAnimable = Mathf.Min(cantidadGastada, transform.childCount - primerIndice);
    List<Image> imagenes = new List<Image>();
    List<RectTransform> rects = new List<RectTransform>();
    List<Vector3> escalasBase = new List<Vector3>();

    for (int i = 0; i < cantidadAnimable; i++)
    {
      Transform circulo = transform.GetChild(primerIndice + i);
      Image imagen = circulo.GetComponent<Image>();
      RectTransform rect = circulo as RectTransform;
      if (imagen == null || rect == null)
      {
        continue;
      }

      imagen.type = Image.Type.Filled;
      imagen.fillMethod = Image.FillMethod.Radial360;
      imagen.fillOrigin = (int)Image.Origin360.Top;
      imagen.fillClockwise = false;
      imagen.fillAmount = 1f;
      imagenes.Add(imagen);
      rects.Add(rect);
      escalasBase.Add(rect.localScale);
    }

    float duracion = Mathf.Max(0.01f, duracionVaciadoAP);
    float demora = Mathf.Max(0f, demoraEntreVaciadosAP);
    float duracionTotal = duracion + demora * Mathf.Max(0, imagenes.Count - 1);
    float tiempo = 0f;

    while (tiempo < duracionTotal)
    {
      tiempo += Time.unscaledDeltaTime;
      for (int i = 0; i < imagenes.Count; i++)
      {
        if (imagenes[i] == null || rects[i] == null)
        {
          continue;
        }

        float progreso = Mathf.Clamp01((tiempo - demora * i) / duracion);
        float suavizado = Mathf.SmoothStep(0f, 1f, progreso);
        imagenes[i].fillAmount = 1f - suavizado;
        imagenes[i].color = new Color(1f, 1f, 1f, 1f - suavizado * 0.65f);
        float escala = Mathf.Lerp(1f, 0.72f, suavizado);
        rects[i].localScale = escalasBase[i] * escala;
      }
      yield return null;
    }

    corrutinaVaciadoAP = null;
    if (BattleManager.Instance != null && BattleManager.Instance.unidadActiva == unidadAlIniciar)
    {
      ReconstruirCirculos(apObjetivo);
      apRepresentado = apObjetivo;

      int apActual = Mathf.Max(0, (int)unidadAlIniciar.ObtenerAPActual());
      if (apActual != apObjetivo)
      {
        ActualizarAPCirculos();
      }
    }
  }

  private void CancelarVaciadoAP()
  {
    if (corrutinaVaciadoAP == null)
    {
      return;
    }

    StopCoroutine(corrutinaVaciadoAP);
    corrutinaVaciadoAP = null;
  }

  private void ReconstruirCirculos(int cantidad)
  {
    foreach (Transform circulo in transform)
    {
      Destroy(circulo.gameObject);
    }

    for (int i = 0; i < cantidad; i++)
    {
      Instantiate(circuloAPprefab, transform);
    }
  }

  public void MarcarCirculos(int n)
  {
    if (!isActiveAndEnabled || !gameObject.scene.isLoaded || BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
    {
      return;
    }

    // Obtén la cantidad total de elementos en el GridLayoutGroup
    int totalCirculos = transform.childCount;

    Unidad unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject.GetComponent<Unidad>();
    int apSinEsfuerzo = (int)unidadSeleccionada.ObtenerAPActual();

    // Asegúrate de que n está dentro de los límites y no sea mayor que apSinEsfuerzo
    n = Mathf.Clamp(n, 0, Mathf.Min(totalCirculos, apSinEsfuerzo));

    // Itera sobre los últimos N elementos y cambia su color a azul
    for (int i = totalCirculos - n; i < totalCirculos; i++)
    {
      Transform circuloTransform = transform.GetChild(i);
      Image circuloImage = circuloTransform.GetComponent<Image>();

      if (circuloImage == null)
      {
        continue;
      }

      circuloImage.color = Color.white;
      if (spriteAPUsado != null)
      {
        circuloImage.sprite = spriteAPUsado;
      }
    }
   
  }


  public void ResetearCirculos()
  {
    ActualizarAPCirculos();
    int totalCirculos = transform.childCount;

    for (int i = 0; i < totalCirculos; i++)
    {
        Transform circuloTransform = transform.GetChild(i);
        Image circuloImage = circuloTransform.GetComponent<Image>();
        if (circuloImage == null)
        {
          continue;
        }

        circuloImage.color = Color.white;
    }
  }

public void SeEsforzaria(int n)
{
    if(n > 0)
    {
      for (int i = 0; i < n; i++)
      {
        GameObject nuevoCirculo = Instantiate(esfuerzoAPprefab, transform);
        if (spriteEsforzar != null)
        {
          Image img = nuevoCirculo.GetComponent<Image>();
          if (img != null)
          {
            img.sprite = spriteEsforzar;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
          }
        }
      }
    }
}


}
