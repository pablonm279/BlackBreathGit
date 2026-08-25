using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)]
public sealed class BattleWheelPerspective : MonoBehaviour
{
  [Header("Perspectiva con rueda")]
  [SerializeField] private float pasoAcercamiento = 0.12f;
  [SerializeField] private float pasoAlejamiento = 0.16f;
  [SerializeField] private float alejamientoMaximo = 0.18f;
  [SerializeField] private float suavizado = 0.28f;
  [SerializeField] private float inclinacionMaxima = 1.6f;
  [SerializeField] private float avanceMaximo = 0.38f;
  [SerializeField] private float descensoMaximo = 0.08f;

  private BattleManager battleManager;
  private Transform camara;
  private Vector3 posicionLocalBaseRig;
  private float perspectivaObjetivo;
  private float perspectivaActual;
  private float velocidadPerspectiva;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void RegistrarInstalacion()
  {
    SceneManager.sceneLoaded -= InstalarEnEscena;
    SceneManager.sceneLoaded += InstalarEnEscena;
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void InstalarEnEscenaActual()
  {
    InstalarEnEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
  }

  private static void InstalarEnEscena(Scene escena, LoadSceneMode modo)
  {
    BattleManager[] managers = Resources.FindObjectsOfTypeAll<BattleManager>();
    foreach (BattleManager manager in managers)
    {
      if (manager == null || manager.gameObject.scene != escena || manager.goCamara == null)
      {
        continue;
      }

      Transform camara = manager.goCamara.transform;
      if (camara.parent != null && camara.parent.GetComponent<BattleWheelPerspective>() != null)
      {
        continue;
      }

      CrearRig(manager, camara);
    }
  }

  private static void CrearRig(BattleManager manager, Transform camara)
  {
    Transform padreOriginal = camara.parent;
    int indiceOriginal = camara.GetSiblingIndex();
    Vector3 posicionLocalOriginal = camara.localPosition;
    Quaternion rotacionLocalOriginal = camara.localRotation;
    Vector3 escalaLocalOriginal = camara.localScale;

    GameObject rig = new GameObject("Battle Wheel Perspective Rig");
    SceneManager.MoveGameObjectToScene(rig, camara.gameObject.scene);
    rig.transform.SetParent(padreOriginal, false);
    rig.transform.SetSiblingIndex(indiceOriginal);
    rig.transform.localPosition = posicionLocalOriginal;
    rig.transform.localRotation = Quaternion.identity;
    rig.transform.localScale = Vector3.one;

    camara.SetParent(rig.transform, false);
    camara.localPosition = Vector3.zero;
    camara.localRotation = rotacionLocalOriginal;
    camara.localScale = escalaLocalOriginal;

    BattleWheelPerspective control = rig.AddComponent<BattleWheelPerspective>();
    control.battleManager = manager;
    control.camara = camara;
    control.posicionLocalBaseRig = posicionLocalOriginal;
  }

  private void Update()
  {
    RestaurarRigBase();

    if (PuedeLeerRueda())
    {
      float rueda = Input.mouseScrollDelta.y;
      if (Mathf.Abs(rueda) > 0.001f)
      {
        float paso = rueda > 0f ? pasoAcercamiento : pasoAlejamiento;
        perspectivaObjetivo = Mathf.Clamp(
          perspectivaObjetivo + (Mathf.Sign(rueda) * paso),
          -Mathf.Max(0f, alejamientoMaximo),
          1f);
      }
    }

    perspectivaActual = Mathf.SmoothDamp(
      perspectivaActual,
      perspectivaObjetivo,
      ref velocidadPerspectiva,
      Mathf.Max(0.01f, suavizado));
  }

  private void LateUpdate()
  {
    if (camara == null)
    {
      return;
    }

    float nivelVisual = perspectivaActual >= 0f
      ? Mathf.SmoothStep(0f, 1f, perspectivaActual)
      : -Mathf.SmoothStep(0f, 1f, Mathf.Abs(perspectivaActual) / Mathf.Max(0.001f, alejamientoMaximo)) * alejamientoMaximo;

    Quaternion rotacionCamaraLocal = camara.localRotation;
    Quaternion inclinacionLocalCamara = Quaternion.Euler(-nivelVisual * inclinacionMaxima, 0f, 0f);
    transform.localRotation = rotacionCamaraLocal * inclinacionLocalCamara * Quaternion.Inverse(rotacionCamaraLocal);

    Vector3 avance = rotacionCamaraLocal * Vector3.forward * (nivelVisual * avanceMaximo);
    Vector3 descenso = Vector3.down * (nivelVisual * descensoMaximo);
    transform.localPosition = posicionLocalBaseRig + avance + descenso;
  }

  private void OnDisable()
  {
    RestaurarRigBase();
  }

  private void RestaurarRigBase()
  {
    transform.localPosition = posicionLocalBaseRig;
    transform.localRotation = Quaternion.identity;
  }

  private bool PuedeLeerRueda()
  {
    if (camara == null || !camara.gameObject.activeInHierarchy || battleManager == null)
    {
      return false;
    }

    if (battleManager.EntradaBatallaBloqueadaPorUI)
    {
      return false;
    }

    if (battleManager.scTutorialCombate != null && battleManager.scTutorialCombate.tutorialCombateActivo)
    {
      return false;
    }

    return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
  }
}
