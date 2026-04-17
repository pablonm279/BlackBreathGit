using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrampaEscape : Trampa
{
  private const float AlturaVfx = 0.035f;

  public void Inicializar()
  {
    nombre = "Via de Escape";
    intDificultadVer = 0;
    intUsos = 100;
    intDuracionTurnos = 100;
    esPersistente = false;
    esTrampaFavorable = true;

    ActivarVFXModeloTrampa();
    ReproducirVfxAparicion();
  }

  public override void AplicarEfectosTrampa(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    if (scBattleManager == null)
    {
      scBattleManager = BattleManager.Instance;
    }

    scBattleManager?.SincronizarHabilidadEscapar(unidad);
  }

  void ActivarVFXModeloTrampa()
  {
    if (scBattleManager == null || scBattleManager.contenedorPrefabs == null)
    {
      return;
    }

    prefabModelo = scBattleManager.contenedorPrefabs.TrampaEscape;
    if (prefabModelo == null)
    {
      return;
    }

    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation);

    Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += AlturaVfx;
    GOvfx.transform.position = newPosition;

    Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) - 2;
    }
  }

  void ReproducirVfxAparicion()
  {
    if (GOvfx == null)
    {
      return;
    }

    StartCoroutine(AnimarAparicionVfx());
  }

  IEnumerator AnimarAparicionVfx()
  {
    if (GOvfx == null)
    {
      yield break;
    }

    Transform raiz = GOvfx.transform;
    Vector3 escalaFinal = raiz.localScale;
    Vector3 escalaInicial = escalaFinal * 0.68f;
    Vector3 posicionFinal = raiz.position;
    Vector3 posicionInicial = posicionFinal + (Vector3.up * 0.08f);

    Graphic[] graficos = GOvfx.GetComponentsInChildren<Graphic>(true);
    Color[] coloresOriginales = new Color[graficos.Length];
    for (int i = 0; i < graficos.Length; i++)
    {
      coloresOriginales[i] = graficos[i].color;
      Color colorInicio = coloresOriginales[i];
      colorInicio.a *= 0.3f;
      graficos[i].color = colorInicio;
    }

    float duracion = 0.55f;
    float tiempo = 0f;

    while (tiempo < duracion)
    {
      if (raiz == null)
      {
        yield break;
      }

      tiempo += Time.deltaTime;
      float progreso = Mathf.Clamp01(tiempo / duracion);
      float progresoEscala = Mathf.SmoothStep(0f, 1f, progreso);
      float rebote = Mathf.Sin(progreso * Mathf.PI) * 0.14f;

      raiz.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, progresoEscala) * (1f + rebote);
      raiz.position = Vector3.Lerp(posicionInicial, posicionFinal, progresoEscala);

      for (int i = 0; i < graficos.Length; i++)
      {
        Color color = coloresOriginales[i];
        color.a *= Mathf.Lerp(0.3f, 1f, progresoEscala);
        graficos[i].color = color;
      }

      yield return null;
    }

    if (raiz != null)
    {
      raiz.localScale = escalaFinal;
      raiz.position = posicionFinal;
    }

    for (int i = 0; i < graficos.Length; i++)
    {
      graficos[i].color = coloresOriginales[i];
    }
  }
}
