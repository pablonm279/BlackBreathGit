using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;


public class Estados : MonoBehaviour
{
  private const int TurnosCondenadoParaEjecucion = 10;

  public async static void Efecto_Ardiendo(Unidad unidad)
  {
    unidad.RecibirDanio(2 * unidad.estado_ardiendo, 4, false, null, 400);

    Aplicar_Ardiendo(unidad, -1);
    if (unidad.estado_ardiendo < 0) { unidad.estado_ardiendo = 0; }
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    
       /* while(unidad.ObtenerAPActual() > 0 && unidad.estado_ardiendo > 0)
  {
      Aplicar_Ardiendo(unidad, -3);
     unidad.CambiarAPActual(-1);

      // Retraso de 1.15 segundos
      await BattleManager.DelayCombateAsync(1150);

      unidad.GenerarTextoFlotante(TRADU.i.Traducir("Apagando!"), Color.red);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" gasta 1 PA para apagar el fuego.")));



      if(  unidad.estado_ardiendo < 0) {  unidad.estado_ardiendo = 0;}
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }*/
  }

  public static void  Efecto_Congelado(Unidad unidad)
  {
     

     unidad.CambiarAPActual(-(int)unidad.estado_congelado);
     unidad.estado_congelado -= 1;
     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está congelado.")));


     if(unidad.estado_congelado < 0)
     {
       unidad.estado_congelado = 0;

       unidad.GenerarTextoFlotante(TRADU.i.Traducir("Descongelado!"), Color.red);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" se libró del congelamiento.")));


     }
     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Aturdido(Unidad unidad)
  {
     unidad.EstablecerAPActualA(0);
     unidad.estado_aturdido--;
    unidad.GenerarTextoFlotante(TRADU.i.Traducir("Aturdido!"), Color.yellow);
     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está aturdido.")));

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
   public static void  Efecto_RegeneraVida(Unidad unidad) //Regenera X Vida por turno
  {

    unidad.RecibirCuracion(unidad.estado_regeneravida, false);
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);

  }
  public static void Efecto_Condenado(Unidad unidad) //Cuando stacks llegue a 0 recibe 10% hpmax por turno activo (danio verdadero)
  {
    if (unidad == null || unidad.HP_actual <= 0)
    {
      return;
    }

    unidad.estado_CondenadoTurnosSeguidos++;

    if (unidad.estado_CondenadoTurnosSeguidos >= TurnosCondenadoParaEjecucion)
    {
      unidad.estado_Condenado = 0;
      float danioEjecucion = Mathf.Max(unidad.HP_actual + 1f, unidad.mod_maxHP * 10f);
      CondenaDanioImpactoFx.Crear(unidad);
      unidad.RecibirDanio(danioEjecucion, 10, false, null, 400);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" es ejecutado por la Condena.")));
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
      unidad.estado_CondenadoTurnosSeguidos = 0;
      return;
    }

    unidad.estado_Condenado--;
    if (unidad.estado_Condenado < 1)
    {
      float porcentajeAcumulado = 0.10f * unidad.estado_CondenadoTurnosSeguidos;
      CondenaDanioImpactoFx.Crear(unidad);
      unidad.RecibirDanio(unidad.mod_maxHP * porcentajeAcumulado, 10, false, null,400);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" es dañado por la Condena.")));
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);

      unidad.estado_Condenado = 0;
      unidad.estado_CondenadoTurnosSeguidos = 0;
    }

  }

  
   public static void Efecto_RegeneraArmadura(Unidad unidad) //Regenera X Armadura por turno (si perdió armadura al recibir daño)
  {

    if (unidad.estado_armaduraModificador > 0)
    {
      int armaduraRecuperada = Mathf.Min(unidad.estado_armaduraModificador, unidad.estado_regeneraarmadura);
      if (armaduraRecuperada > 0)
      {
        unidad.estado_armaduraModificador -= armaduraRecuperada;
        BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" regenera ") + armaduraRecuperada + TRADU.i.Traducir(" Armadura.")));
      }
    }

    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
  public static void  Efecto_Inmovil(Unidad unidad)
  {
     
     unidad.estado_inmovil--;

     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está inmovilizado.")));

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Sangrado(Unidad unidad)
  {
    
   
    unidad.mod_maxHP -= unidad.estado_sangrado * 2;
    if(unidad.HP_actual > unidad.mod_maxHP)  //Si al perder max HP, su vida actual es mayor a la amx, recibe daño verdadero para equiparar.
    {
       float cant = unidad.HP_actual - unidad.mod_maxHP ;
       unidad.RecibirDanio(cant, 10,false, null,400); 
    }
    unidad.estado_sangrado--;


    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Veneno(Unidad unidad)
  {
    unidad.RecibirDanio(1*unidad.estado_veneno, 10,false, null,400); 
    BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" recibe ") + (1*unidad.estado_veneno) + TRADU.i.Traducir(" daño veneno.")));


   bool noSeSalva = unidad.TiradaSalvacion(unidad.mod_TSFortaleza, 7+unidad.estado_veneno);
   if(!noSeSalva) //Cada turno se puede salvar del veneno; si falla, se suma 1 stack.
   {
     unidad.estado_veneno = 0; unidad.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Veneno") + "</s>", Color.green);
    BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" resiste totalmente al veneno.")));

   }
   else
   {
      Estados.Aplicar_Veneno(unidad, 1);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" falla su Tirada de salvación y el veneno empeora.")));

   }
   
    
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Aplicar_Ardiendo(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Ardiendo", origen))
     {
       return;
     }

     if(unidad.estado_ardiendo > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_ardiendo += stacks;

       if(stacks > 0)
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" arde"), Color.red);
      

       BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Veneno(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Veneno", origen))
     {
       return;
     }

     if(unidad.estado_veneno > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_veneno += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" veneno"), Color.green);
       BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Congelado(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Congelado", origen))
     {
       return;
     }

     if(unidad.estado_congelado > -1) //-1 Es si es inmune al estado.
     {
      unidad.estado_congelado += stacks;
      unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" frio"), Color.cyan);

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Aturdido(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Aturdido", origen))
     {
       return;
     }

     if(unidad.estado_aturdido > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_aturdido < stacks)
      {
        unidad.estado_aturdido = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" aturde"), Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }
  public static void  Aplicar_Inmovil(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Inmovil", origen))
     {
       return;
     }

     if(unidad.estado_inmovil > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_inmovil < stacks)
      {
        unidad.estado_inmovil = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" inmóvil"), Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }


  public static void  Aplicar_Sangrado(Unidad unidad, int stacks, Unidad origen = null)
  {
    if (stacks > 0 && unidad.IntentarResistenciaEstado("Sangrado", origen))
    {
      return;
    }

    if(unidad.estado_sangrado > -1) //-1 Es si es inmune al estado.
    {
       unidad.estado_sangrado += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" sangrado"), Color.red);
     

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Acido(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Acido", origen))
     {
       return;
     }

     if(unidad.estado_acido > -1) //-1 Es si es inmune al estado.
    {
     
       unidad.estado_acido += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" acido"), Color.green);

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }




  public static void Aplicar_MovimientoAbaratado(Unidad unidad, int stacks, Unidad origen = null, bool mostrarTextoFlotante = true)
  {
    if (unidad == null || stacks == 0)
    {
      return;
    }

    unidad.estado_MovimientoAbaratado += stacks;
    if (unidad.estado_MovimientoAbaratado < 0)
    {
      unidad.estado_MovimientoAbaratado = 0;
    }

    if (stacks > 0 && mostrarTextoFlotante)
    {
      unidad.GenerarTextoFlotante("+" + stacks + " " + TRADU.i.Traducir("impulso"), new Color(0.2f, 0.95f, 1f));
    }

    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
}

public class CondenaDanioImpactoFx : MonoBehaviour
{
  private const float Duracion = 0.48f;
  private const int CantidadFragmentos = 5;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private RectTransform imagenUnidad;
  private Image auraExterior;
  private Image auraInterior;
  private Image anillo;
  private Image runaA;
  private Image runaB;
  private Image nucleo;
  private readonly Image[] fragmentos = new Image[CantidadFragmentos];
  private readonly float[] angulosFragmento = new float[CantidadFragmentos];
  private readonly float[] radiosFragmento = new float[CantidadFragmentos];
  private readonly float[] escalasFragmento = new float[CantidadFragmentos];
  private Vector2 tamanoBase;
  private Vector2 posicionImpacto;
  private float tiempo;

  private static Sprite spriteSuave;
  private static Sprite spriteAnillo;
  private static Sprite spriteFragmento;
  private static Texture2D texturaSuave;
  private static Texture2D texturaAnillo;
  private static Texture2D texturaFragmento;

  public static void Crear(Unidad unidad)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    RectTransform imagen = unidad.uImage.rectTransform;
    if (imagen == null || !(imagen.parent is RectTransform padre))
    {
      return;
    }

    GameObject go = new GameObject("CondenaDanioImpactoFx", typeof(RectTransform), typeof(CanvasGroup), typeof(CondenaDanioImpactoFx));
    CondenaDanioImpactoFx fx = go.GetComponent<CondenaDanioImpactoFx>();
    fx.Inicializar(padre, imagen);
  }

  private void Inicializar(RectTransform padre, RectTransform imagen)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    imagenUnidad = imagen;

    root.SetParent(padre, false);
    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);

    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;
    canvasGroup.alpha = 0f;

    tamanoBase = imagenUnidad.rect.size;
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = imagenUnidad.sizeDelta;
    }
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = new Vector2(34f, 40f);
    }

    posicionImpacto = new Vector2(0f, tamanoBase.y * 0.06f);
    root.anchoredPosition = imagenUnidad.anchoredPosition + posicionImpacto;
    root.localScale = Vector3.one;
    root.sizeDelta = tamanoBase * 0.74f;

    int targetSibling = Mathf.Min(padre.childCount - 1, imagenUnidad.GetSiblingIndex() + 1);
    root.SetSiblingIndex(targetSibling);

    auraExterior = CrearImagen("AuraExterior", ObtenerSpriteSuave(), root);
    auraInterior = CrearImagen("AuraInterior", ObtenerSpriteSuave(), root);
    anillo = CrearImagen("Anillo", ObtenerSpriteAnillo(), root);
    runaA = CrearImagen("RunaA", ObtenerSpriteFragmento(), root);
    runaB = CrearImagen("RunaB", ObtenerSpriteFragmento(), root);
    nucleo = CrearImagen("Nucleo", ObtenerSpriteSuave(), root);

    for (int i = 0; i < fragmentos.Length; i++)
    {
      fragmentos[i] = CrearImagen("Fragmento" + i, ObtenerSpriteFragmento(), root);
      angulosFragmento[i] = -72f + (i * (360f / fragmentos.Length)) + Random.Range(-12f, 12f);
      radiosFragmento[i] = Random.Range(0.16f, 0.28f);
      escalasFragmento[i] = Random.Range(0.78f, 1.14f);
    }

    ActualizarVisual(0f);
  }

  private static Image CrearImagen(string nombre, Sprite sprite, RectTransform padre)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float t = Mathf.Clamp01(tiempo / Duracion);
    ActualizarVisual(t);

    if (tiempo >= Duracion)
    {
      Destroy(gameObject);
    }
  }

  private void ActualizarVisual(float t)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.12f));
    float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.22f) / 0.78f));
    float intensidad = entrada * salida;
    float golpe = 1f - Mathf.SmoothStep(0f, 1f, t);
    float pulso = 0.5f + (0.5f * Mathf.Sin((t * 18f) + 0.55f));

    if (imagenUnidad != null)
    {
      root.anchoredPosition = imagenUnidad.anchoredPosition + posicionImpacto;
    }

    root.localScale = Vector3.one * Mathf.Lerp(0.86f, 1.08f, entrada);
    root.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-12f, 18f, t));
    canvasGroup.alpha = intensidad;

    Configurar(auraExterior, Vector2.zero, tamanoBase * (0.42f + (0.38f * t)), 0f, new Color(0.24f, 0.05f, 0.44f, 0.34f * intensidad));
    Configurar(auraInterior, Vector2.zero, tamanoBase * (0.24f + (0.18f * pulso)), 0f, new Color(0.74f, 0.28f, 0.96f, 0.42f * intensidad));
    Configurar(anillo, Vector2.zero, tamanoBase * (0.18f + (0.34f * t)), -95f + (t * 150f), new Color(0.9f, 0.46f, 1f, 0.52f * intensidad * golpe));
    Configurar(runaA, Vector2.zero, new Vector2(tamanoBase.x * 0.34f, tamanoBase.y * 0.06f), 45f - (t * 26f), new Color(0.92f, 0.66f, 1f, 0.54f * intensidad));
    Configurar(runaB, Vector2.zero, new Vector2(tamanoBase.x * 0.34f, tamanoBase.y * 0.06f), -45f + (t * 26f), new Color(0.66f, 0.32f, 0.96f, 0.44f * intensidad));
    Configurar(nucleo, Vector2.zero, tamanoBase * (0.11f + (0.05f * golpe)), 0f, new Color(1f, 0.84f, 1f, 0.62f * intensidad));

    for (int i = 0; i < fragmentos.Length; i++)
    {
      float angulo = angulosFragmento[i] + (t * 36f);
      float rad = angulo * Mathf.Deg2Rad;
      Vector2 direccion = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
      Vector2 posicion = direccion * (tamanoBase.x * Mathf.Lerp(0.04f, radiosFragmento[i], t));
      posicion += new Vector2(0f, Mathf.Sin((t * 11f) + i) * tamanoBase.y * 0.02f);
      Vector2 tamano = new Vector2(tamanoBase.x * 0.14f * escalasFragmento[i] * Mathf.Lerp(1f, 0.56f, t), tamanoBase.y * 0.045f);
      Color color = i % 2 == 0
        ? new Color(0.82f, 0.46f, 1f, 0.5f * intensidad * golpe)
        : new Color(0.58f, 0.24f, 0.94f, 0.44f * intensidad * golpe);
      Configurar(fragmentos[i], posicion, tamano, angulo + 90f, color);
    }
  }

  private static void Configurar(Image image, Vector2 posicion, Vector2 tamano, float rotacionZ, Color color)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacionZ);
    rect.localScale = Vector3.one;
    image.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 48;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "CondenaDanioSuaveRuntime";
    texturaSuave.wrapMode = TextureWrapMode.Clamp;
    texturaSuave.filterMode = FilterMode.Bilinear;
    texturaSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.5f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "CondenaDanioSuaveRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteAnillo()
  {
    if (spriteAnillo != null)
    {
      return spriteAnillo;
    }

    const int size = 72;
    texturaAnillo = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAnillo.name = "CondenaDanioAnilloRuntime";
    texturaAnillo.wrapMode = TextureWrapMode.Clamp;
    texturaAnillo.filterMode = FilterMode.Bilinear;
    texturaAnillo.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float borde = Mathf.Abs(distancia - 0.62f);
        float irregularidad = Mathf.Sin((Mathf.Atan2(dy, dx) * 6f) + 0.8f) * 0.03f;
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - ((borde + irregularidad) / 0.13f)), 1.8f) * Mathf.Clamp01(1f - distancia);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAnillo.SetPixels(pixels);
    texturaAnillo.Apply(false, true);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteAnillo.name = "CondenaDanioAnilloRuntime";
    return spriteAnillo;
  }

  private static Sprite ObtenerSpriteFragmento()
  {
    if (spriteFragmento != null)
    {
      return spriteFragmento;
    }

    const int width = 72;
    const int height = 18;
    texturaFragmento = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaFragmento.name = "CondenaDanioFragmentoRuntime";
    texturaFragmento.wrapMode = TextureWrapMode.Clamp;
    texturaFragmento.filterMode = FilterMode.Bilinear;
    texturaFragmento.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs(((x / (width - 1f)) * 2f) - 1f);
        float ny = Mathf.Abs((y - centroY) / centroY);
        float cuerpo = Mathf.Clamp01(1f - (nx * 1.2f) - (ny * 0.85f));
        float filo = Mathf.Clamp01(1f - (nx * 0.72f) - (ny * 1.65f));
        float alpha = Mathf.Pow(Mathf.Max(cuerpo, filo), 1.65f);
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaFragmento.SetPixels(pixels);
    texturaFragmento.Apply(false, true);
    spriteFragmento = Sprite.Create(texturaFragmento, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteFragmento.name = "CondenaDanioFragmentoRuntime";
    return spriteFragmento;
  }
}

