using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;

public class BotonHabilidad : MonoBehaviour
{

    public Habilidad HabilidadRepresentada;
    public Image HabilidadCooldownMuestra;
    public TextMeshProUGUI turnosCooldown;

    [SerializeField] private bool BotonActivo = false;
    [SerializeField] private GameObject goDesc;
    [SerializeField] private TextMeshProUGUI txtDescHab;
    [SerializeField] private TMP_SpriteAsset spriteAssetIconosCombate;
    [SerializeField] private float hoverDelay = 0.35f;

    [SerializeField] private GameObject prefabCirculoAccion;
    [SerializeField] private GameObject seleccionada;

    [SerializeField] private GameObject contenedorCirculosAccion;

    [SerializeField] private TextMeshProUGUI txtHotkey;

    public UIBotonesHabilidades scUiBotonesHabilidades;


    TextMeshProUGUI nombreHabilidad;
    private Coroutine hoverDescripcionRoutine;
    private bool hoverDescripcionActiva;
    private Sprite spriteCirculoAccionNormal;
    private Sprite spriteCirculoEsfuerzo;
    private static readonly Color colorCirculoDisponible = Color.white;
    private static readonly Color colorCirculoFaltante = new Color(0.5f, 0.5f, 0.5f, 1f);
    private void Awake()
    {
        scUiBotonesHabilidades = transform.parent.GetComponent<UIBotonesHabilidades>();
        nombreHabilidad = transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        if (seleccionada != null)
        {
            seleccionada.SetActive(false);
        }
    }

    void Start()
    {

        gameObject.GetComponent<Image>().sprite = HabilidadRepresentada.imHab;
        CachearSpritesCirculosAccion();



        // Instanciar tantos prefabCirculoAccion en contenedorCirculosAccion como el costo de AP de la habilidad
        if (contenedorCirculosAccion != null && prefabCirculoAccion != null && HabilidadRepresentada != null)
        {
            // Limpiar los círculos existentes primero
            foreach (Transform child in contenedorCirculosAccion.transform)
            {
                Destroy(child.gameObject);
            }

            int cantidad = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
            for (int i = 0; i < cantidad; i++)
            {
                Instantiate(prefabCirculoAccion, contenedorCirculosAccion.transform);
            }
        }

        ActualizarVisualCirculosAccion();
    }

    public void hoverDescripcion(int n)
    {
        if (n == 1)
        {
            hoverDescripcionActiva = true;
            if (hoverDescripcionRoutine != null)
            {
                StopCoroutine(hoverDescripcionRoutine);
                hoverDescripcionRoutine = null;
            }

            if (hoverDelay <= 0f)
            {
                MostrarDescripcion();
            }
            else
            {
                hoverDescripcionRoutine = StartCoroutine(HoverDescripcionConDelay());
            }
        }
        else
        {
            hoverDescripcionActiva = false;
            if (hoverDescripcionRoutine != null)
            {
                StopCoroutine(hoverDescripcionRoutine);
                hoverDescripcionRoutine = null;
            }
            goDesc.SetActive(false);
        }
    }

    private IEnumerator HoverDescripcionConDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        if (hoverDescripcionActiva)
        {
            MostrarDescripcion();
        }
        hoverDescripcionRoutine = null;
    }

    private void MostrarDescripcion()
    {
        HabilidadRepresentada.ActualizarDescripcion();
        TMP_SpriteAsset spriteAsset = ObtenerSpriteAssetIconosCombate();
        if (txtDescHab != null && spriteAsset != null)
        {
            txtDescHab.spriteAsset = spriteAsset;
        }

        string descripcion = Habilidad.LimpiarCostoValentiaDescripcion(HabilidadRepresentada.txtDescripcion);
        bool incluirIconos = spriteAsset != null;
        if (HabilidadRepresentada != null && HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
        {
            txtDescHab.text = TextoIconosCombate.FormatearIconosDespuesDelTitulo(descripcion, incluirIconos);
        }
        else
        {
            txtDescHab.text = TextoIconosCombate.FormatearIconosDesdeBloqueMecanico(descripcion, incluirIconos);
        }
        goDesc.SetActive(true);

        // Asegurarnos de que el goDesc (RectTransform) no salga de los margenes de la pantalla
        RectTransform descRect = goDesc.GetComponent<RectTransform>();

        // Obtener las dimensiones de la pantalla
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Obtener las coordenadas de la imagen en pantalla (anclada a su pivote)
        Vector3[] corners = new Vector3[4];
        descRect.GetWorldCorners(corners);

        // Comprobar si alguna esquina esta fuera de los margenes de la pantalla
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = corners[i];

            // Si alguna parte de la descripcion esta fuera del lado izquierdo de la pantalla
            if (corner.x < 0)
            {
                descRect.position += new Vector3(-corner.x, 0, 0); // Ajustar al margen izquierdo
            }

            // Si alguna parte de la descripcion esta fuera del lado derecho de la pantalla
            if (corner.x > screenSize.x)
            {
                descRect.position += new Vector3(screenSize.x - corner.x, 0, 0); // Ajustar al margen derecho
            }

            // Si alguna parte de la descripcion esta fuera de la parte inferior de la pantalla
            if (corner.y < 0)
            {
                descRect.position += new Vector3(0, -corner.y, 0); // Ajustar al margen inferior
            }

            // Si alguna parte de la descripcion esta fuera de la parte superior de la pantalla
            if (corner.y > screenSize.y)
            {
                descRect.position += new Vector3(0, screenSize.y - corner.y, 0); // Ajustar al margen superior
            }
        }
    }

    private TMP_SpriteAsset ObtenerSpriteAssetIconosCombate()
    {
        if (spriteAssetIconosCombate != null)
        {
            return spriteAssetIconosCombate;
        }

        return BattleManager.Instance != null ? BattleManager.Instance.SpriteAssetCombate : null;
    }

    private void CachearSpritesCirculosAccion()
    {
        if (spriteCirculoAccionNormal == null && prefabCirculoAccion != null)
        {
            Image imagenPrefab = prefabCirculoAccion.GetComponent<Image>();
            if (imagenPrefab != null)
            {
                spriteCirculoAccionNormal = imagenPrefab.sprite;
            }
        }

        if (spriteCirculoEsfuerzo == null)
        {
            spriteCirculoEsfuerzo = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/esforzar");
        }
    }

    private void ActualizarVisualCirculosAccion()
    {
        if (contenedorCirculosAccion == null || HabilidadRepresentada == null || BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
        {
            return;
        }

        CachearSpritesCirculosAccion();

        int apActual = Mathf.Max(0, Mathf.FloorToInt(BattleManager.Instance.unidadActiva.ObtenerAPActual()));
        int apNecesarios = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
        int faltantes = Mathf.Max(0, apNecesarios - apActual);
        int faltantesEsforzables = Mathf.Min(faltantes, Mathf.Max(0, HabilidadRepresentada.esforzable));
        int primerIndiceEsfuerzo = apNecesarios - faltantesEsforzables;

        for (int i = 0; i < contenedorCirculosAccion.transform.childCount; i++)
        {
            Image img = contenedorCirculosAccion.transform.GetChild(i).GetComponent<Image>();
            if (img == null)
            {
                continue;
            }

            bool circuloFaltante = i >= apActual && i < apNecesarios;
            bool mostrarEsfuerzo = circuloFaltante && faltantesEsforzables > 0 && i >= primerIndiceEsfuerzo;

            if (mostrarEsfuerzo && spriteCirculoEsfuerzo != null)
            {
                img.sprite = spriteCirculoEsfuerzo;
            }
            else if (spriteCirculoAccionNormal != null)
            {
                img.sprite = spriteCirculoAccionNormal;
            }

            img.color = circuloFaltante ? colorCirculoFaltante : colorCirculoDisponible;
        }
    }

    public void ActivarHabilidad(bool yaVienedeCargando)
    {
        if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
        {
            if(BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() < 4)
            {
                BattleManager.Instance.scTutorialCombate.SiguientePasoCombate();
            }
        }

        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"
        BattleManager.Instance.bOcupado = false;
        if (HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
        { return; } //Si se clickea el boton de una pasiva, no pasa nada

        if (HabilidadRepresentada.cooldownActual > 0)
        { return; } //Control extra para que no se puedan activar habilidades en cooldown

        if (HabilidadRepresentada is CargaDeEstoque cargaDeEstoque && !cargaDeEstoque.PuedeActivarseDesdePosicionActual(out string motivoCarga))
        {
            BattleManager.Instance.unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir(motivoCarga), Color.gray, FloatingTextContext.Generic);
            return;
        }

        if (HabilidadRepresentada is RecuperarAire recuperarAire && !recuperarAire.PuedeActivarseDesdePosicionActual(out string motivoRecuperarAire))
        {
            BattleManager.Instance.unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir(motivoRecuperarAire), Color.gray, FloatingTextContext.Generic);
            return;
        }

        if (HabilidadRepresentada.esMelee && !PuedeUsarHabilidadMelee())
        {
            Unidad unidadActiva = BattleManager.Instance.unidadActiva;
            if (unidadActiva != null && unidadActiva.estado_inmovil > 0)
            {
                unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("Inmóvil, Melee solo adyacente."), Color.gray, FloatingTextContext.Generic);
            }
            else
            {
                unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir("Adelántate para usarla"), Color.gray, FloatingTextContext.Generic);
            }
            return;
        }

        if (HabilidadRepresentada.requiereRecurso > 0) //de las habilidades que requieran un recurso, se fija una por una si lo cumplen
        {
            if (HabilidadRepresentada.nombre == "Tiro con Arco")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); return; }
            }
            if (HabilidadRepresentada.nombre == "Tiro Potente")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); return; }
            }
            if (HabilidadRepresentada.nombre == "Vigilancia")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); return; }
            }
            if (HabilidadRepresentada.nombre == "Enmendar")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClasePurificadora>().ObtenerFervor() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Asesinar")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseAcechador>().ObtenerEstaEscondido() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Descarga Desintegradora")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes energía"), Color.gray, FloatingTextContext.Resist); return; }
            }
            if (HabilidadRepresentada.nombre == "Manifestacion Arcana")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes energía"), Color.gray, FloatingTextContext.Resist); return; }
            }

        }

        if (!yaVienedeCargando)
        {
            if (ChequearCargaHabilidad())
            { return; }
        }


        BattleManager.Instance.LimpiarCapasCasillas();
        BattleManager.Instance.scUIContadorAP.ResetearCirculos();


        int esfuerzo;
        if (yaVienedeCargando)
        {
            ActivarBoton(0);


        }
        else if (BotonActivo == false && HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
            ActivarBoton(esfuerzo);
        }
        else if (HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
             if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
           {
            if(BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() < 4)
            {
                    return;
            }
           }
            DesactivarBoton();
        }


    }


    public void UpdateCooldownMuestra()
    {


        if (HabilidadRepresentada != null)
        {

            if (HabilidadRepresentada.cooldownMax > 0 && HabilidadRepresentada.cooldownActual > 0)
            {
                float nMax = HabilidadRepresentada.cooldownMax;
                float nCurr = HabilidadRepresentada.cooldownActual;
                float fillRes = nCurr / nMax;


                HabilidadCooldownMuestra.fillAmount = fillRes;

            }
            else
            {
                HabilidadCooldownMuestra.fillAmount = 0;
            }



        }
    }

    void ActivarBoton(int iEsfuerzo)
    {

        int esfuerzo = iEsfuerzo;

        scUiBotonesHabilidades.UIDesactivarHabilidades(HabilidadRepresentada.esHostil);
        HabilidadRepresentada.Activar();
        RuntimeAnalytics.TrackDesign("combat", "ability_selected", RuntimeAnalytics.AbilityToken(HabilidadRepresentada));
        VisualBotonActivo(1);
        // BattleManager.Instance.OpacarCasillasMelee();
        BotonActivo = true;
        if (BattleManager.Instance.unidadActiva.valorCargando > 0)
        {
            BattleManager.Instance.scUIContadorAP.MarcarCirculos(BattleManager.Instance.unidadActiva.valorCargando);
        }
        else
        {
            BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)HabilidadRepresentada.costoAP);
        }
        BattleManager.Instance.scUIContadorAP.SeEsforzaria(esfuerzo);



    }

    public void DesactivarBoton()
    {
        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"

        int esfuerzo = 0;
        if (HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
            VisualBotonActivo(0);
            BotonActivo = false;
            BattleManager.Instance.LimpiarCapasCasillas();
            BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
            BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
            BattleManager.Instance.SeleccionandoObjetivo = false;
            BattleManager.Instance.HabilidadActiva = null;

            BattleManager.Instance.scUIContadorAP.ResetearCirculos();


        }
    }





    public void DesactivarHabilidad(bool omitirTilteo = false)
    {
        BattleManager.Instance.DesmarcarTodasLasUnidades();
        if (BotonActivo == true)
        { VisualBotonActivo(0, omitirTilteo); }
        BotonActivo = false;
    }

    public GameObject Oscurecedor;
    public GameObject Melee;

    private void Update()
    {
        if (HabilidadRepresentada != null)
        {
            nombreHabilidad.text = HabilidadRepresentada.nombre;

            if (turnosCooldown != null)
            {
                turnosCooldown.text = HabilidadRepresentada.cooldownActual > 0
                    ? HabilidadRepresentada.cooldownActual.ToString()
                    : string.Empty;
            }

            // Actualiza el fill de cooldown en tiempo real para reflejar
            // inmediatamente los cambios al usar la habilidad o al cambiar de turno.
            UpdateCooldownMuestra();
        }
        else if (turnosCooldown != null)
        {
            turnosCooldown.text = string.Empty;
        }

        if (contenedorCirculosAccion != null && BattleManager.Instance != null && BattleManager.Instance.unidadActiva != null)
        {
            ActualizarVisualCirculosAccion();
        }

        // Mostrar/ocultar oscurecedor según si hay AP + esforzable suficientes
        if (Oscurecedor != null && HabilidadRepresentada != null && BattleManager.Instance != null && BattleManager.Instance.unidadActiva != null)
        {
            int apActualInt = Mathf.Max(0, (int)BattleManager.Instance.unidadActiva.ObtenerAPActual());
            int apNecesarios = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
            int disponible = apActualInt + HabilidadRepresentada.esforzable;
            bool bloqueoMelee = HabilidadRepresentada.esMelee && !PuedeUsarHabilidadMelee();

            Oscurecedor.SetActive(disponible < apNecesarios || bloqueoMelee);

            if (HabilidadRepresentada.esMelee)
            {
                Melee.SetActive(true);
            }
            else
            {
                Melee.SetActive(false);
            }
        }

    }


    private void VisualBotonActivo(int estado, bool omitirTilteo = false)
    {
        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"


        if (estado == 1)
        {
            if (BotonActivo == false)
            {
                if (HabilidadRepresentada.esHostil)
                {
                    BattleManager.Instance.TiltearCamaraLadoEnemigo(true);
                }
                gameObject.GetComponent<RectTransform>().localScale += new Vector3(0.2f, 0.2f, 0.2f);
            }

            if (seleccionada != null)
            {
                seleccionada.SetActive(true);
            }

        }
        else if (estado == 0)
        {

            if (BotonActivo == true)
            {
                BattleManager.Instance.DesmarcarTodasLasUnidades();
                if (HabilidadRepresentada.esHostil && !omitirTilteo)
                {
                    BattleManager.Instance.TiltearCamaraLadoEnemigo(false);
                }
                gameObject.GetComponent<RectTransform>().localScale -= new Vector3(0.2f, 0.2f, 0.2f);
            }

            if (seleccionada != null)
            {
                seleccionada.SetActive(false);
            }
        }
    }

    bool ChequearCargaHabilidad()
    {
        Unidad uActiva = BattleManager.Instance.unidadActiva;

        if (uActiva.ObtenerAPActual() < HabilidadRepresentada.costoAP && HabilidadRepresentada.esCargable)
        {
            uActiva.estaCargando = HabilidadRepresentada;
            uActiva.valorCargando = (int)(HabilidadRepresentada.costoAP - uActiva.ObtenerAPActual());

            BattleManager.Instance.TerminarTurno();



            return true;
        }
        else
        {
            return false;
        }
    }

    bool PuedeUsarHabilidadMelee()
    {
        if (HabilidadRepresentada == null || !HabilidadRepresentada.esMelee)
        {
            return true;
        }

        if (HabilidadRepresentada is CargaDeEstoque cargaDeEstoque)
        {
            return cargaDeEstoque.PuedeActivarseDesdePosicionActual(out _);
        }

        if (HabilidadRepresentada.nombre.Contains("Destruir Obstaculo"))
        {
            return true;

        }

        if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
        {
            return false;
        }

        Unidad unidad = BattleManager.Instance.unidadActiva;
        if (unidad.CasillaPosicion == null)
        {
            return false;
        }

        int posX = unidad.CasillaPosicion.posX;
        if (unidad.estado_inmovil > 0)
        {
            return posX == 3;
        }

        if (posX == 3)
        {
            return true;
        }

        if (posX == 2)
        {
            LadoManager lado = unidad.CasillaPosicion.ladoGO != null ? unidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>() : null;
            if (lado != null)
            {
                Casilla casillaDelantera = lado.ObtenerCasillaPorIndex(3, unidad.CasillaPosicion.posY);
                if (casillaDelantera != null && casillaDelantera.Presente != null)
                {
                    Unidad aliado = casillaDelantera.Presente.GetComponent<Unidad>();
                    if (aliado != null && aliado.CasillaPosicion != null && aliado.CasillaPosicion.lado == unidad.CasillaPosicion.lado)
                    {
                        return true;
                    }

                    Obstaculo obstaculo = casillaDelantera.Presente.GetComponent<Obstaculo>();
                    if (obstaculo != null && obstaculo.bPermiteAtacarDetras)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public MenuPersonajes scMenuPersonajes;
    public void SubirHabDeNivel(int n)
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
        bool nivelado = false;
        if (n == 0) //0 - Subida Normal
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL++;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }
        if (n == 4) //4 - Subida a 4a
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL = 4;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }
        if (n == 5) //5 - Subida a 4b
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL = 5;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }

        if (nivelado)
        {
            RuntimeAnalytics.TrackDesign(
                "characters",
                "skill_up",
                RuntimeAnalytics.AbilityToken(HabilidadRepresentada) + "_tier_" + HabilidadRepresentada.NIVEL);
        }
    }


    public void AgregarComoHabilidadNueva()
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
        // Verificamos si la habilidad representada no es nula
        if (HabilidadRepresentada != null)
        {
            // Obtenemos el tipo de la habilidad representada
            System.Type tipoHabilidad = HabilidadRepresentada.GetType();

            // Agregamos el componente del tipo de la habilidad al objeto scMenuPersonajes.pSel
            Habilidad nuevaHabilidad = (Habilidad)scMenuPersonajes.pSel.gameObject.AddComponent(tipoHabilidad);


            switch (nuevaHabilidad.IDenClase)
            {
                case 1: scMenuPersonajes.pSel.Habilidad_1 = 1; break;
                case 2: scMenuPersonajes.pSel.Habilidad_2 = 1; break;
                case 3: scMenuPersonajes.pSel.Habilidad_3 = 1; break;
                case 4: scMenuPersonajes.pSel.Habilidad_4 = 1; break;
                case 5: scMenuPersonajes.pSel.Habilidad_5 = 1; break;
                case 6: scMenuPersonajes.pSel.Habilidad_6 = 1; break;
                case 7: scMenuPersonajes.pSel.Habilidad_7 = 1; break;
                case 8: scMenuPersonajes.pSel.Habilidad_8 = 1; break;
                case 9: scMenuPersonajes.pSel.Habilidad_9 = 1; break;
                case 10: scMenuPersonajes.pSel.Habilidad_10 = 1; break;
            }
            nuevaHabilidad.NIVEL = 1;

            // Actualizamos la información y reducimos el nivel de la nueva habilidad base
            scMenuPersonajes.pSel.NivelNuevaHabilidadBase--;
            scMenuPersonajes.yaTiroHabRand = false;
            scMenuPersonajes.ActualizarInfo();
            scMenuPersonajes.LimpiarComponentesHab();
            RuntimeAnalytics.TrackDesign("characters", "new_skill", RuntimeAnalytics.AbilityToken(nuevaHabilidad));
        }
    }


    public void AsignarHotkey()
    {
        Invoke("ActualizarkotkeyDelay", 0.1f);
    }

    public void ActualizarkotkeyDelay()
    {
          if (HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
        { txtHotkey.text = "";  return; } //Si se clickea el boton de una pasiva, no pasa nada

        if (txtHotkey == null) return;
        int pos = transform.GetSiblingIndex();
        pos++; // Convertir de índice base 0 a base 1
        txtHotkey.text = pos.ToString();

    }
    
}



