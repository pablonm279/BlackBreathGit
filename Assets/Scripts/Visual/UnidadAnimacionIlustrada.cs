using UnityEngine;
using UnityEngine.UI;

// Solo modifica los vertices y el sprite de la ilustracion: no mueve la unidad,
// los puntos de impacto, el Animator, ni los indicadores del tutorial.
[DisallowMultipleComponent]
public sealed class UnidadAnimacionIlustrada : BaseMeshEffect
{
    private static PerfilAnimacionIlustrada[] perfiles;
    private UnidadPoseController controlador;
    private Unidad unidad;
    private Image imagen;
    private PerfilAnimacionIlustrada perfil;
    private UnidadPoseController.TipoPoseActual pose;
    private Sprite origen;
    private Sprite ultimoFrame;
    private float tiempo;
    private float? impactoHabilidad;
    private float tiempoRespiracion;
    private float faseReposo;
    private float recuperacionDash;
    private bool aplicando;
    private bool detenido;
    private bool esIA;
    private Animator animatorOriginal;
    private float progresoAnimator;

    public bool UsaSpritesOriginalesIA => esIA && perfil == null;
    public bool Disponible => !detenido && (!esIA || (unidad != null && !unidad.esInmobil))
        && (perfil != null ? perfil.Coincide(controlador) : esIA);
    public Sprite PosePreparacion => Disponible && perfil != null ? perfil.preparacion : null;

    public static UnidadAnimacionIlustrada IntentarCrear(UnidadPoseController destino)
    {
        if (destino.targetImage == null) return null;
        if (perfiles == null)
            perfiles = Resources.LoadAll<PerfilAnimacionIlustrada>("AnimacionesIlustradas");

        PerfilAnimacionIlustrada elegido = null;
        foreach (PerfilAnimacionIlustrada candidato in perfiles)
        {
            if (!candidato.EstaCompleto || !candidato.Coincide(destino)) continue;
            elegido = candidato;
            break;
        }
        Unidad unidadDestino = destino.GetComponent<Unidad>();
        bool ia = destino.GetComponent<IAUnidad>() != null;
        if (ia && (unidadDestino == null || unidadDestino.esInmobil)) return null;
        if (elegido == null && (!ia || unidadDestino == null)) return null;
        UnidadAnimacionIlustrada motor = CrearMotor(unidadDestino, destino.targetImage, ia);
        motor.controlador = destino;
        motor.perfil = elegido;
        motor.faseReposo = Mathf.Abs(destino.GetInstanceID() % 997) * 0.017f;
        return motor;
    }

    private static UnidadAnimacionIlustrada CrearMotor(Unidad destino, Image imagenDestino, bool ia)
    {
        UnidadAnimacionIlustrada motor = imagenDestino.GetComponent<UnidadAnimacionIlustrada>();
        if (motor == null) motor = imagenDestino.gameObject.AddComponent<UnidadAnimacionIlustrada>();
        motor.unidad = destino;
        motor.imagen = imagenDestino;
        motor.esIA = ia;
        // No consume Random del combate (iniciativa, ataques o tutorial).
        motor.faseReposo = Mathf.Abs(imagenDestino.GetInstanceID() % 997) * 0.017f;
        return motor;
    }

    public static void InicializarIASinControlador(Unidad destino)
    {
        if (destino == null || destino.esInmobil || destino.uImage == null || destino.GetComponent<IAUnidad>() == null
            || destino.GetComponent<UnidadPoseController>() != null) return;
        // Los prefabs antiguos conservan su Animator y todos sus sprites.
        UnidadAnimacionIlustrada motor = CrearMotor(destino, destino.uImage, true);
        motor.animatorOriginal = destino.GetComponent<Animator>();
        motor.Reproducir(UnidadPoseController.TipoPoseActual.Idle, destino.uImage.sprite, true);
    }

    public void Reproducir(UnidadPoseController.TipoPoseActual nuevaPose, Sprite spriteOrigen, bool reiniciar)
    {
        bool cambio = nuevaPose != pose || origen != spriteOrigen;
        if (pose == UnidadPoseController.TipoPoseActual.Mover && nuevaPose == UnidadPoseController.TipoPoseActual.Idle)
            recuperacionDash = 0.18f;
        else if (nuevaPose != UnidadPoseController.TipoPoseActual.Idle)
            recuperacionDash = 0f;
        pose = nuevaPose;
        origen = spriteOrigen;
        aplicando = Disponible;
        if (cambio || reiniciar) { tiempo = 0f; impactoHabilidad = null; }
        if (aplicando) AplicarFrame();
        else ultimoFrame = null;
        if (graphic != null) graphic.SetVerticesDirty();
    }

    public void Detener()
    {
        detenido = true;
        aplicando = false;
        if (graphic != null) graphic.SetVerticesDirty();
    }

    public void EsperarImpactoHabilidad()
    {
        if (Disponible && pose == UnidadPoseController.TipoPoseActual.Atacar)
            impactoHabilidad = float.PositiveInfinity;
    }

    public void ConfirmarImpactoHabilidad()
    {
        if (!Disponible || pose != UnidadPoseController.TipoPoseActual.Atacar || !impactoHabilidad.HasValue) return;
        impactoHabilidad = tiempo;
        AplicarFrame();
        if (graphic != null) graphic.SetVerticesDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        detenido = false;
        tiempo = 0f;
        impactoHabilidad = null;
        tiempoRespiracion = 0f;
        recuperacionDash = 0f;
    }

    protected override void OnDisable()
    {
        if (aplicando && imagen != null && imagen.sprite == ultimoFrame && origen != null)
            imagen.sprite = origen;
        aplicando = false;
        base.OnDisable();
    }

    private void Update()
    {
        if (UsaSpritesOriginalesIA && controlador == null && Disponible && imagen != null)
            SincronizarAnimatorOriginal();
        if (UsaSpritesOriginalesIA && !Disponible && aplicando)
        {
            aplicando = false;
            if (graphic != null) graphic.SetVerticesDirty();
        }
        if (!aplicando || !Disponible || imagen == null || !imagen.isActiveAndEnabled) return;
        // Respeta una sustitucion externa de sprite y los estados que inmovilizan.
        if (imagen.sprite != ultimoFrame) { aplicando = false; graphic.SetVerticesDirty(); return; }
        if (unidad != null && (unidad.HP_actual <= 0f || unidad.estado_congelado > 0 || unidad.estado_aturdido > 0)) return;
        if (Time.deltaTime <= 0f) return;
        tiempo += Time.deltaTime;
        // Reloj independiente: cambiar de turno no salta de fase ni demora ataques.
        tiempoRespiracion += Time.deltaTime * UnidadAmbientMotionFx.ObtenerFactorRespiracion(unidad);
        recuperacionDash = Mathf.Max(0f, recuperacionDash - Time.deltaTime);
        AplicarFrame();
        graphic.SetVerticesDirty();
    }

    private void AplicarFrame()
    {
        if (UsaSpritesOriginalesIA) { ultimoFrame = imagen.sprite; return; }
        ultimoFrame = EvaluarFrame(perfil, pose, tiempo, controlador, origen, impactoHabilidad);
        if (imagen.sprite != ultimoFrame) imagen.sprite = ultimoFrame;
    }

    private void SincronizarAnimatorOriginal()
    {
        UnidadPoseController.TipoPoseActual actual = UnidadPoseController.TipoPoseActual.Idle;
        bool reiniciar = false;
        if (animatorOriginal != null && animatorOriginal.isActiveAndEnabled
            && animatorOriginal.runtimeAnimatorController != null)
        {
            AnimatorStateInfo estado = animatorOriginal.GetCurrentAnimatorStateInfo(0);
            if (estado.IsName("Animacion_Ataque"))
            {
                actual = UnidadPoseController.TipoPoseActual.Atacar;
                reiniciar = estado.normalizedTime < progresoAnimator;
            }
            else if (!estado.IsName("Animacion_idle") && !estado.IsName("New State")
                && !estado.IsName("Animacion_TurnoNuevo"))
                actual = UnidadPoseController.TipoPoseActual.Habilidad;
            progresoAnimator = estado.normalizedTime;
        }
        if (unidad.movimientoEnCurso) actual = UnidadPoseController.TipoPoseActual.Mover;
        Reproducir(actual, imagen.sprite, reiniciar);
    }

    public static Sprite EvaluarFrame(PerfilAnimacionIlustrada datos, UnidadPoseController.TipoPoseActual estado,
        float segundos, UnidadPoseController destino, Sprite spriteOrigen = null, float? impactoHabilidad = null)
    {
        switch (estado)
        {
            case UnidadPoseController.TipoPoseActual.Mover:
                return destino.poseMover != null ? destino.poseMover : datos.origenMover;
            case UnidadPoseController.TipoPoseActual.Atacar:
                // Usa el mismo instante de impacto que las habilidades existentes.
                float impactoSeg = impactoHabilidad ?? MeleeTimingUtility.CalcularPreImpactoMs(destino) / 1000f;
                if (segundos < impactoSeg) return datos.anticipacion;
                if (segundos < impactoSeg + 0.18f) return datos.impacto;
                return ResolverReposo(datos, destino);
            case UnidadPoseController.TipoPoseActual.Habilidad:
                return spriteOrigen != null ? spriteOrigen : destino.poseHabilidad;
            default:
                Sprite reposoActual = spriteOrigen != null ? spriteOrigen : destino.ObtenerPoseReposoActual();
                return reposoActual == datos.origenIdle ? datos.reposo : reposoActual;
        }
    }

    private static Sprite ResolverReposo(PerfilAnimacionIlustrada datos, UnidadPoseController destino)
    {
        Sprite reposoActual = destino.ObtenerPoseReposoActual();
        return reposoActual == datos.origenIdle ? datos.reposo : reposoActual;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || (perfil == null && !esIA) || imagen == null || ultimoFrame == null
            || imagen.sprite != ultimoFrame || vh.currentVertCount == 0) return;
        Rect rect = graphic.rectTransform.rect;
        Vector2 apoyo = rect.min + Vector2.Scale(rect.size, perfil != null ? perfil.apoyo : new Vector2(0.5f, 0f));
        if (UsaSpritesOriginalesIA)
        {
            // Usa la base de la geometria real, incluso con preserveAspect o malla ajustada.
            UIVertex baseVertice = default;
            apoyo.y = float.PositiveInfinity;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref baseVertice, i);
                apoyo.y = Mathf.Min(apoyo.y, baseVertice.position.y);
            }
        }
        bool frameNuevo = perfil != null && (ultimoFrame == perfil.reposo || ultimoFrame == perfil.anticipacion
            || ultimoFrame == perfil.impacto || ultimoFrame == perfil.preparacion);
        // Las poses originales ya tienen el encuadre del prefab.
        if (!frameNuevo && perfil != null) apoyo.x += rect.width * perfil.desplazamientoHorizontal;
        float estirar = 0f;
        float inclinar = 0f;
        bool inmovil = !aplicando || !Disponible || (unidad != null
            && (unidad.HP_actual <= 0f || unidad.estado_congelado > 0 || unidad.estado_aturdido > 0));
        if (!inmovil && pose == UnidadPoseController.TipoPoseActual.Idle)
        {
            float onda = Mathf.Sin(tiempoRespiracion * 2.1f + faseReposo);
            estirar = onda * (perfil != null ? perfil.respiracion : 0.006f);
            inclinar = Mathf.Sin(tiempoRespiracion * 1.05f + faseReposo) * 0.0025f;
            inclinar -= 0.025f * Mathf.SmoothStep(0f, 1f, recuperacionDash / 0.18f);
        }
        else if (!inmovil && pose == UnidadPoseController.TipoPoseActual.Mover)
        {
            // Impulso unico y asentamiento: conserva el dash, sin pasos ni rebote ciclico.
            estirar = -0.009f * Mathf.Sin(Mathf.Clamp01(tiempo / 0.24f) * Mathf.PI);
            inclinar = -0.025f * (1f - Mathf.Exp(-tiempo / 0.07f))
                + 0.004f * Mathf.Sin(tiempo * 12f) * Mathf.Exp(-tiempo / 0.28f);
        }
        else if (!inmovil && pose == UnidadPoseController.TipoPoseActual.Atacar)
        {
            float impactoSeg = impactoHabilidad ?? MeleeTimingUtility.CalcularPreImpactoMs(controlador) / 1000f;
            inclinar = tiempo < impactoSeg
                ? 0.014f * Mathf.Sin(Mathf.Clamp01(tiempo / Mathf.Max(0.01f, impactoSeg)) * Mathf.PI * 0.5f)
                : -0.022f * Mathf.Clamp01(1f - (tiempo - impactoSeg) / 0.22f);
        }

        UIVertex vertice = default;
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertice, i);
            float altura = vertice.position.y - apoyo.y;
            if (frameNuevo)
                vertice.position.x = apoyo.x + (vertice.position.x - apoyo.x) * perfil.escalaHorizontal
                    + rect.width * perfil.desplazamientoHorizontal;
            vertice.position.y += altura * estirar;
            vertice.position.x += altura * inclinar;
            vh.SetUIVertex(vertice, i);
        }
    }
}
