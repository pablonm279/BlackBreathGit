using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class LuzCegadora : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Luz Cegadora";
      IDenClase = 4;
      costoAP = 4;
      costoPM = 1;
      if(NIVEL == 4){costoPM--;}
      
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 3; 
      bAfectaObstaculos = false;

      targetEspecial = 6; 

      bonusAtaque +=0; //0
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 11; //Divino
     

      imHab = Resources.Load<Sprite>("imHab/Purificadora_LuzCegadora");
      
     

      
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int dcBase = NIVEL > 1 ? 10 : 9;
      bool agregaD6Divino = NIVEL > 2;
      bool afectaOtrosEnemigos = NIVEL == 5;
      string rangoDanioBaseEs = FormatearRangoDados(1, 10, 1);
      string rangoDanioExtraEs = FormatearRangoDados(1, 6);

      string tituloEs = "Luz Cegadora I";
      string tituloEn = "Blinding Light I";
      string tituloPt = "Luz Cegante I";
      if (NIVEL == 2) { tituloEs = "Luz Cegadora II"; tituloEn = "Blinding Light II"; }
      if (NIVEL == 3) { tituloEs = "Luz Cegadora III"; tituloEn = "Blinding Light III"; }
      if (NIVEL == 4) { tituloEs = "Luz Cegadora IV a"; tituloEn = "Blinding Light IV a"; }
      if (NIVEL == 5) { tituloEs = "Luz Cegadora IV b"; tituloEn = "Blinding Light IV b"; }
      if (NIVEL == 2) { tituloPt = "Luz Cegante II"; }
      if (NIVEL == 3) { tituloPt = "Luz Cegante III"; }
      if (NIVEL == 4) { tituloPt = "Luz Cegante IV a"; }
      if (NIVEL == 5) { tituloPt = "Luz Cegante IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Reflejos, dcBase, "Pod", "Power", poderActual, "Poder");
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Reflejos, dcBase, "Poder", "Power", poderActual);

      string danioPrincipalEs = agregaD6Divino
        ? $"{rangoDanioBaseEs} + {rangoDanioExtraEs} + <color=#ea0606>Pod ({poderActual})</color> | <b>Tipo:</b> Divino"
        : $"{rangoDanioBaseEs} + <color=#ea0606>Pod ({poderActual})</color> | <b>Tipo:</b> Divino";
      string danioPrincipalEn = agregaD6Divino
        ? $"1d10 + 1 + 1d6 + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Divine"
        : $"1d10 + 1 + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Divine";
      string danioPrincipalPt = agregaD6Divino
        ? $"1d10 + 1 + 1d6 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Divino"
        : $"1d10 + 1 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Divino";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (3 range)\n";
        cuerpo += "<b>Target:</b> Frontal area (2 width)\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += "<b>On failed save and if not immune to Blind:</b> Blinded for 2 rounds (-3 Attack, -2 Defense, -1 Reflex)\n";
        cuerpo += $"<b>Damage vs Undead/Ethereal:</b> {danioPrincipalEn}";
        if (afectaOtrosEnemigos)
        {
          cuerpo += "\n<b>Other enemies:</b> receive 1/3 of the rolled Divine damage";
        }
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Alcance (3 de alcance)\n";
        cuerpo += "<b>Alvo:</b> Area frontal (2 de largura)\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += "<b>Se falhar na resistencia e nao for imune a Cegueira:</b> Cego por 2 rodadas (-3 Ataque, -2 Defesa, -1 Reflexos)\n";
        cuerpo += $"<b>Dano vs Morto-vivo/Etereo:</b> {danioPrincipalPt}";
        if (afectaOtrosEnemigos)
        {
          cuerpo += "\n<b>Outros inimigos:</b> recebem 1/3 do dano Divino rolado";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (3 alcance)\n";
        cuerpo += "<b>Objetivo:</b> Area frontal (2 ancho)\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += "<b>Si falla TS y no es inmune a Ceguera:</b> Ciego por 2 rondas (-3 Ataque, -2 Defensa, -1 Reflejos)\n";
        cuerpo += $"<b>Danio vs Nomuerto/Etereo:</b> {danioPrincipalEs}";
        if (afectaOtrosEnemigos)
        {
          cuerpo += "\n<b>Otros enemigos:</b> reciben 1/3 del danio Divino tirado";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Purifier unleashes divine radiance that hinders enemies and burns impure targets."
          : esPortugues
            ? "A Purificadora libera uma radiancia divina que enfraquece inimigos e queima alvos impuros."
          : "La Purificadora desata una radiancia divina que debilita enemigos y quema objetivos impuros.",
        cuerpo,
        costos,
        "#5dade2");

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Frontal flash that blinds enemies and damages impure targets."
        : esPortugues
          ? "Clarão frontal que cega inimigos e causa dano a alvos impuros."
          : "Destello frontal que ciega enemigos y dania objetivos impuros.";
      string danioVsImpuro = agregaD6Divino
        ? $"{rangoDanioBaseEs} + {rangoDanioExtraEs} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color>"
        : $"{rangoDanioBaseEs} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color>";
      string cuerpoNuevo = "";
      if (esIngles)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Frontal area (3 range, 2 width)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Enemies in area</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Reflex vs DC {dcBase} + <color={colorPoder}>Power ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Failed save:</b></color> <color={colorValor}>{iconoDebuff} Blinded 2 rounds: -3 Attack, -2 Defense, -1 Reflex</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Undead/Ethereal:</b></color> <color={colorValor}>{danioVsImpuro}. Type: Divine</color>";
        if (afectaOtrosEnemigos) { cuerpoNuevo += $"\n<color={colorEncabezado}><b>Other enemies:</b></color> <color={colorValor}>1/3 of rolled Divine damage.</color>"; }
      }
      else if (esPortugues)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Area frontal (3 de alcance, 2 de largura)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Inimigos na area</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Resistencia:</b></color> <color={colorValor}>Reflexos vs DC {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoDebuff} Cego 2 rodadas: -3 Ataque, -2 Defesa, -1 Reflexos</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Morto-vivo/Etereo:</b></color> <color={colorValor}>{danioVsImpuro}. Tipo: Divino</color>";
        if (afectaOtrosEnemigos) { cuerpoNuevo += $"\n<color={colorEncabezado}><b>Outros inimigos:</b></color> <color={colorValor}>1/3 do dano Divino rolado.</color>"; }
      }
      else
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Area frontal (3 alcance, 2 ancho)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Enemigos en el area</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Reflejos vs DC {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoDebuff} Ciego 2 rondas: -3 Ataque, -2 Defensa, -1 Reflejos</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Nomuerto/Etereo:</b></color> <color={colorValor}>{danioVsImpuro}. Tipo: Divino</color>";
        if (afectaOtrosEnemigos) { cuerpoNuevo += $"\n<color={colorEncabezado}><b>Otros enemigos:</b></color> <color={colorValor}>1/3 del danio Divino tirado.</color>"; }
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoNuevo;

      bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1d6 Divine damage vs Undead/Ethereal.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (1/3 damage to other enemies).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 CD de resistencia.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1d6 de dano Divino vs Morto-vivo/Etereo.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (1/3 de dano para outros inimigos).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1-6 de danio Divino vs Nomuerto/Etereo.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de Valentía) u Opcion B (1/3 de danio a otros enemigos).</color>"; }
      }
    }

    Casilla Origen;
    public override void Activar()
    { 
        seTiroFlechaVFX = false;
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    bool seTiroFlechaVFX = false;
    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;
        float dificultadAtributo = 9+scEstaUnidad.mod_CarPoder;
        if(NIVEL > 1){dificultadAtributo++;}
        VFXAplicar(objetivo.gameObject);
      if (objetivo.inmunidad_Ceguera)
      {
        objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.red);
      }
      else if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, dificultadAtributo)) //Si la tirada de salvacion es mayor a la tirada del usuario, no se aplica el efecto
      {

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Ciego";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 3;
        buff.cantDefensa -= 2;
        buff.cantTsReflejos -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);


      }

         float damDivino = UnityEngine.Random.Range(2, 12)+scEstaUnidad.mod_CarPoder;
         if(NIVEL > 2){damDivino += UnityEngine.Random.Range(1, 7);}

        if(objetivo.TieneTag("Nomuerto") || objetivo.TieneTag("Etereo"))
        {
           
            objetivo.RecibirDanio(damDivino,11, false,scEstaUnidad); 

        }
        else
        {
           if(NIVEL == 5)
           {
             objetivo.RecibirDanio(damDivino/3, tipoDanio, false,scEstaUnidad); 
           }
        }



     }
    }
  
  
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_LuzCegadora");
      if (VFXenObjetivo != null)
      {
        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
        vfx.transform.parent = objetivo.transform;
        VFXSoloSonido.OcultarVisuales(vfx);
      }

      LuzCegadoraDestelloVFX.Crear(objetivo);

    }


    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

   private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(3,2);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
       c.ActivarCapaColorRojo();
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
           }

        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

    
    
}

public static class VFXSoloSonido
{
  public static void OcultarVisuales(GameObject vfx)
  {
    if (vfx == null)
    {
      return;
    }

    foreach (Canvas canvas in vfx.GetComponentsInChildren<Canvas>(true))
    {
      canvas.enabled = false;
    }

    foreach (Graphic graphic in vfx.GetComponentsInChildren<Graphic>(true))
    {
      graphic.enabled = false;
    }

    foreach (Renderer renderer in vfx.GetComponentsInChildren<Renderer>(true))
    {
      renderer.enabled = false;
    }

    foreach (Light luz in vfx.GetComponentsInChildren<Light>(true))
    {
      luz.enabled = false;
    }
  }
}

public class LuzCegadoraDestelloVFX : MonoBehaviour
{
  private const float Duracion = 1.35f;
  private const float EscalaGlobal = 0.18f;
  private const float OffsetY = 8f;
  private const float TamanoMinimo = 18f;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private Image haloExterior;
  private Image haloInterior;
  private Image rayoHorizontal;
  private Image rayoVertical;
  private Image brilloCentral;
  private float tiempo;
  private Vector2 tamanoBase;

  private static Sprite spriteSuave;
  private static Sprite spriteRayo;
  private static Texture2D texturaSuave;
  private static Texture2D texturaRayo;

  public static void Crear(GameObject objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    Unidad unidad = objetivo.GetComponent<Unidad>();
    RectTransform imagenBase = unidad != null && unidad.uImage != null
      ? unidad.uImage.rectTransform
      : null;
    Canvas canvas = imagenBase != null
      ? imagenBase.GetComponentInParent<Canvas>(true)
      : objetivo.GetComponentInChildren<Canvas>(true);
    if (canvas == null)
    {
      return;
    }

    if (imagenBase == null)
    {
      imagenBase = ObtenerImagenBase(canvas.transform);
    }

    RectTransform padre = imagenBase != null && imagenBase.parent is RectTransform
      ? (RectTransform)imagenBase.parent
      : canvas.transform as RectTransform;
    if (padre == null)
    {
      return;
    }

    GameObject go = new GameObject("DestelloLuzCegadora", typeof(RectTransform), typeof(CanvasGroup), typeof(LuzCegadoraDestelloVFX));
    LuzCegadoraDestelloVFX destello = go.GetComponent<LuzCegadoraDestelloVFX>();
    destello.Inicializar(padre, imagenBase);
    RenderOrderHelper.OrdenarCanvasEncima(canvas, objetivo.transform, 8);
  }

  private static RectTransform ObtenerImagenBase(Transform canvas)
  {
    if (canvas == null)
    {
      return null;
    }

    Transform directa = canvas.Find("Image");
    if (directa != null && directa.TryGetComponent(out Image imageDirecta))
    {
      return imageDirecta.rectTransform;
    }

    foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
    {
      if (image != null && image.sprite != null)
      {
        return image.rectTransform;
      }
    }

    return null;
  }

  private void Inicializar(RectTransform padre, RectTransform imagenBase)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    root.SetParent(padre, false);
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    Vector2 posicion = Vector2.zero;
    Vector3 escala = Vector3.one;
    if (imagenBase != null)
    {
      posicion = imagenBase.anchoredPosition;
      escala = imagenBase.localScale;
      tamanoBase = imagenBase.rect.size;
      if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
      {
        tamanoBase = imagenBase.sizeDelta;
      }
    }

    float ancho = Mathf.Max(TamanoMinimo, tamanoBase.x * 2.4f * EscalaGlobal);
    float alto = Mathf.Max(TamanoMinimo, tamanoBase.y * 2.4f * EscalaGlobal);
    tamanoBase = new Vector2(ancho, alto);

    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.anchoredPosition = posicion + new Vector2(0f, OffsetY);
    root.localScale = escala;
    root.sizeDelta = tamanoBase;
    root.SetAsLastSibling();

    haloExterior = CrearImagen("HaloExterior", ObtenerSpriteSuave());
    haloInterior = CrearImagen("HaloInterior", ObtenerSpriteSuave());
    rayoHorizontal = CrearImagen("RayoHorizontal", ObtenerSpriteRayo());
    rayoVertical = CrearImagen("RayoVertical", ObtenerSpriteRayo());
    brilloCentral = CrearImagen("BrilloCentral", ObtenerSpriteSuave());

    rayoVertical.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
    ActualizarVisual(0f);
  }

  private Image CrearImagen(string nombre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(root, false);
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

    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.24f));
    float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.45f) / 0.55f));
    float intensidad = entrada * salida;
    float pulsoCentro = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.08f) / 0.22f)) * salida;
    float pulsoRayos = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.16f) / 0.28f)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.62f) / 0.38f)));
    float escala = Mathf.Lerp(0.58f, 0.88f, Mathf.SmoothStep(0f, 1f, t));

    canvasGroup.alpha = intensidad;
    root.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-3f, 5f, t));

    ConfigurarImagen(haloExterior, tamanoBase * (2.15f * escala), new Color(1f, 0.93f, 0.38f, 0.36f * intensidad));
    ConfigurarImagen(haloInterior, tamanoBase * (1.24f * escala), new Color(1f, 0.98f, 0.76f, 0.56f * intensidad));
    ConfigurarImagen(brilloCentral, tamanoBase * (0.52f + (0.28f * escala)), new Color(1f, 1f, 0.9f, 0.68f * pulsoCentro));
    ConfigurarImagen(rayoHorizontal, new Vector2(tamanoBase.x * 2.25f * escala, tamanoBase.y * 0.22f), new Color(1f, 0.96f, 0.58f, 0.54f * pulsoRayos));
    ConfigurarImagen(rayoVertical, new Vector2(tamanoBase.x * 1.85f * escala, tamanoBase.y * 0.16f), new Color(1f, 1f, 0.78f, 0.42f * pulsoRayos));
  }

  private static void ConfigurarImagen(Image image, Vector2 tamano, Color color)
  {
    if (image == null)
    {
      return;
    }

    image.rectTransform.anchoredPosition = Vector2.zero;
    image.rectTransform.sizeDelta = tamano;
    image.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 64;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "LuzCegadoraSoftGlowRuntime";
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
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.1f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "LuzCegadoraSoftGlowRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteRayo()
  {
    if (spriteRayo != null)
    {
      return spriteRayo;
    }

    const int width = 128;
    const int height = 32;
    texturaRayo = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaRayo.name = "LuzCegadoraRayGlowRuntime";
    texturaRayo.wrapMode = TextureWrapMode.Clamp;
    texturaRayo.filterMode = FilterMode.Bilinear;
    texturaRayo.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float vertical = 1f - Mathf.Clamp01(Mathf.Abs(y - centroY) / centroY);
      vertical = Mathf.Pow(vertical, 1.8f);
      for (int x = 0; x < width; x++)
      {
        float horizontal = 1f - Mathf.Clamp01(Mathf.Abs(x - centroX) / centroX);
        float alpha = Mathf.Pow(horizontal, 0.58f) * vertical;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaRayo.SetPixels(pixels);
    texturaRayo.Apply(false, true);
    spriteRayo = Sprite.Create(texturaRayo, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteRayo.name = "LuzCegadoraRayGlowRuntime";
    return spriteRayo;
  }
}




