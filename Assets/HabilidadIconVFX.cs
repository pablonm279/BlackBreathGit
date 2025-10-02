using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class HabilidadIconVFX : MonoBehaviour
{
    // ============================
    //        AJUSTES BÁSICOS
    // ============================
    [Header("▶ Disparo")]
    [Tooltip("Si está activo, el efecto se reproduce automáticamente al habilitarse el objeto.")]
    public bool playOnEnable = true;

    [Tooltip("Si está activo y hay un Button en el mismo objeto, reproducirá el efecto al hacer click.")]
    public bool playOnClick = false;

    [Tooltip("Retraso (segundos) antes de iniciar el efecto.")]
    [Range(0f, 3f)] public float startDelay = 0f;

    [Header("▶ Duración y Curva")]
    [Tooltip("Duración total (segundos) de UNA pasada del efecto.")]
    [Range(0.05f, 5f)] public float duration = 0.6f;

    [Tooltip("Curva de progresión del efecto (0→1).")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public enum LoopMode { None, Loop, PingPong }
    [Tooltip("Cómo repetir el efecto. PingPong invierte al volver.")]
    public LoopMode loopMode = LoopMode.None;

    [Tooltip("Número de repeticiones si hay Loop (0 = infinito).")]
    [Min(0)] public int loopCount = 0;

    // ============================
    //          EFECTO BASE
    // ============================
    public enum BaseEffect
    {
        None,
        FadeIn,
        FadeOut,
        WipeTopToBottom,
        WipeBottomToTop,
        WipeLeftToRight,
        WipeRightToLeft
    }

    [Header("▶ Efecto Base (uno principal)")]
    [Tooltip("Efecto visual principal que se aplicará a la imagen.")]
    public BaseEffect baseEffect = BaseEffect.WipeTopToBottom;

    [Tooltip("Si está activo, forzará el tipo Filled del Image para los Wipes.")]
    public bool forceFilledForWipe = true;

    // ============================
    //         EFECTOS EXTRA
    // ============================
    [Header("▶ Extras (opcionales)")]
    [Tooltip("Activa un pulso de brillo/escala mientras corre el efecto.")]
    public bool extraPulseGlow = true;

    [Tooltip("Color objetivo del pulso.")]
    public Color glowColor = new Color(1f, 0.25f, 0.25f, 1f);

    [Tooltip("Intensidad del pulso (0 = sin cambio de color).")]
    [Range(0f, 1f)] public float glowIntensity = 0.35f;

    [Tooltip("Escala adicional en el pico del pulso (1 = sin cambio).")]
    [Range(1f, 1.5f)] public float glowScaleMax = 1.06f;

    [Space(6)]
    [Tooltip("Agrega un leve shake (temblor) mientras dura el efecto.")]
    public bool extraShake = false;

    [Tooltip("Amplitud del shake (UI units).")]
    [Range(0f, 10f)] public float shakeAmplitude = 3f;

    [Tooltip("Frecuencia del shake (ciclos/seg).")]
    [Range(0f, 30f)] public float shakeFrequency = 12f;

    // ============================
    //            AUDIO
    // ============================
    [Header("▶ Audio (opcional)")]
    public bool playSfx = false;
    public AudioClip sfxClip;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 2f)] public float sfxDelay = 0.05f;

    // ============================
    //            EVENTS
    // ============================
    [Header("▶ Eventos (opcional)")]
    public UnityEvent onPlay;
    public UnityEvent onComplete;

    // ============================
    //        TIEMPO (extra)
    // ============================
    [Header("▶ Tiempo (extra)")]
    [Tooltip("Usar Time.deltaTime (true) o Time.unscaledDeltaTime (false).")]
    public bool useScaledTime = false;

    // ============================
    //     TAMAÑO DINÁMICO
    // ============================
    [Header("▶ Tamaño dinámico")]
    [Tooltip("Escala por eje controlada por curvas (se suma a la escala original).")]
    public bool sizeOverTime = false;

    [Tooltip("Curva de escala X. 0=sin cambio; 1/−1 según multiplicador.")]
    public AnimationCurve sizeXCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(-2f, 2f)] public float sizeXMultiplier = 0.2f;

    [Tooltip("Curva de escala Y.")]
    public AnimationCurve sizeYCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(-2f, 2f)] public float sizeYMultiplier = 0.2f;

    // ============================
    //   MOVIMIENTO / VELOCIDAD
    // ============================
    [Header("▶ Movimiento por velocidad")]
    [Tooltip("Desplaza integrando velocidad por eje.")]
    public bool moveOverTime = false;

    public float velX = 0f;
    public AnimationCurve velXCurve = AnimationCurve.Linear(0, 1, 1, 1);

    public float velY = 0f;
    public AnimationCurve velYCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Tooltip("Mover usando anchoredPosition (UI recomendado). Si lo desactivás usa localPosition.")]
    public bool useAnchoredPosition = true;

    // ============================
    //        ROTACIÓN Z
    // ============================
    [Header("▶ Rotación Z")]
    public bool rotateOverTime = false;
    public float rotDegrees = 15f;
    public AnimationCurve rotCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ============================
    //   COLOR / ALPHA / MATERIAL
    // ============================
    [Header("▶ Color / Alpha / Material")]
    [Tooltip("Interpola color usando un gradiente a lo largo del efecto.")]
    public bool colorOverTime = false;
    public Gradient colorGradient;

    [Tooltip("Multiplica alpha del Image por una curva (0→1).")]
    public bool extraAlphaCurve = false;
    public AnimationCurve alphaMulCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Tooltip("Anima una propiedad float del material (p. ej. _Glow).")]
    public bool materialFloatOverTime = false;
    public string materialFloatName = "_Glow";
    public AnimationCurve materialFloatCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float materialFloatMultiplier = 1f;

    // ============================
    //        INTERNOS / CACHE
    // ============================
    private Image _img;
    private RectTransform _rt;
    private CanvasGroup _cg;
    private Button _btn;
    private AudioSource _audio;
    private Coroutine _routine;

    private Color _origColor;
    private Vector3 _origScale;
    private Vector3 _origLocalPos;
    private Vector2 _origAnchoredPos;
    private Image.Type _origType;
    private Image.FillMethod _origFillMethod;
    private int _origFillOrigin;
    private float _origFillAmount;

    private Vector2 _moveAccum;      // desplazamiento acumulado (UI)
    private Material _matInst;

    void Reset()
    {
        playOnEnable = true;
        baseEffect = BaseEffect.WipeTopToBottom;
        forceFilledForWipe = true;
        extraPulseGlow = true;
        glowColor = new Color(1f, 0.25f, 0.25f, 1f);
        glowIntensity = 0.35f;
        glowScaleMax = 1.06f;
        curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        duration = 0.6f;

        colorGradient = new Gradient
        {
            colorKeys = new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            alphaKeys = new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };
    }

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        _btn = GetComponent<Button>();

        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

        if (playSfx)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }

        if (materialFloatOverTime && _img.material != null)
        {
            _matInst = new Material(_img.material);
            _img.material = _matInst;
        }

        CacheOriginal();
        PrepareForBaseEffect();
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
        if (playOnClick && _btn != null) _btn.onClick.AddListener(Play);
    }

    void OnDisable()
    {
        if (playOnClick && _btn != null) _btn.onClick.RemoveListener(Play);
        StopCurrent();
        RestoreOriginalImmediate();
    }

    // -----------------------------
    //  API PÚBLICA
    // -----------------------------
    [ContextMenu("Play")]
    public void Play()
    {
        // si ahora activaste materialFloat, asegurar instancia
        if (materialFloatOverTime && _img != null && _img.material != null && _matInst == null)
        {
            _matInst = new Material(_img.material);
            _img.material = _matInst;
        }
        StopCurrent();
        _routine = StartCoroutine(Co_Play());
    }

    [ContextMenu("Stop")]
    public void StopCurrent()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    // -----------------------------
    //  CORE
    // -----------------------------
    private IEnumerator Co_Play()
    {
        onPlay?.Invoke();

        if (playSfx && sfxClip != null)
            StartCoroutine(Co_PlaySfxDelayed());

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        int loopsDone = 0;
        bool forward = true;

        do
        {
            yield return StartCoroutine(Co_RunOnce(forward));

            if (loopMode == LoopMode.PingPong) forward = !forward;

            if (loopMode == LoopMode.None) break;
            if (loopCount > 0 && ++loopsDone >= loopCount) break;

        } while (loopMode != LoopMode.None);

        onComplete?.Invoke();
        _routine = null;
    }

    private IEnumerator Co_RunOnce(bool forward)
    {
        PrepareForBaseEffect(forward);

        float t = 0f;
        while (t < duration)
        {
            float dt = useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            float k = (curve != null) ? curve.Evaluate(n) : n;
            float dirk = forward ? k : (1f - k);

            ApplyBaseEffect(dirk);
            ApplyExtras(k, dt);

            t += dt;
            yield return null;
        }

        ApplyBaseEffect(forward ? 1f : 0f);
        ApplyExtras(1f, 0f);
    }

    private IEnumerator Co_PlaySfxDelayed()
    {
        if (sfxDelay > 0f) yield return new WaitForSeconds(sfxDelay);
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }
        _audio.volume = sfxVolume;
        _audio.PlayOneShot(sfxClip);
    }

    // -----------------------------
    //  BASE EFFECT & EXTRAS
    // -----------------------------
    private void PrepareForBaseEffect(bool forward = true)
    {
        bool isWipe =
            baseEffect == BaseEffect.WipeTopToBottom ||
            baseEffect == BaseEffect.WipeBottomToTop ||
            baseEffect == BaseEffect.WipeLeftToRight ||
            baseEffect == BaseEffect.WipeRightToLeft;

        if (isWipe && forceFilledForWipe)
        {
            _img.type = Image.Type.Filled;
            if (baseEffect == BaseEffect.WipeTopToBottom || baseEffect == BaseEffect.WipeBottomToTop)
            {
                _img.fillMethod = Image.FillMethod.Vertical;
                _img.fillOrigin = (baseEffect == BaseEffect.WipeTopToBottom) ? 1 : 0; // 1=Top, 0=Bottom
            }
            else
            {
                _img.fillMethod = Image.FillMethod.Horizontal;
                _img.fillOrigin = (baseEffect == BaseEffect.WipeLeftToRight) ? 0 : 1; // 0=Left, 1=Right
            }
            _img.fillAmount = forward ? 0f : 1f;
        }

        if (baseEffect == BaseEffect.FadeIn)  _cg.alpha = forward ? 0f : 1f;
        if (baseEffect == BaseEffect.FadeOut) _cg.alpha = forward ? 1f : 0f;

        _img.color = _origColor;
        transform.localScale = _origScale;

        _moveAccum = Vector2.zero;
        if (_rt != null)
        {
            if (useAnchoredPosition) _rt.anchoredPosition = _origAnchoredPos;
            else _rt.localPosition = _origLocalPos;
        }
    }

    private void ApplyBaseEffect(float k)
    {
        switch (baseEffect)
        {
            case BaseEffect.None: break;
            case BaseEffect.FadeIn:  _cg.alpha = Mathf.Clamp01(k); break;
            case BaseEffect.FadeOut: _cg.alpha = Mathf.Clamp01(1f - k); break;
            default:
                _img.fillAmount = Mathf.Clamp01(k);
                break;
        }
    }

    private void ApplyExtras(float k, float dt)
    {
        // ---- Escala “sizeOverTime” + pulso
        Vector3 scaleNow = _origScale;
        if (sizeOverTime)
        {
            float dx = (sizeXCurve != null ? sizeXCurve.Evaluate(k) : 0f) * sizeXMultiplier;
            float dy = (sizeYCurve != null ? sizeYCurve.Evaluate(k) : 0f) * sizeYMultiplier;
            scaleNow = new Vector3(_origScale.x * (1f + dx),
                                   _origScale.y * (1f + dy),
                                   _origScale.z);
        }
        if (extraPulseGlow && glowIntensity > 0f)
        {
            float s = Mathf.Lerp(1f, glowScaleMax, Mathf.Sin(k * Mathf.PI));
            scaleNow *= s;
        }
        transform.localScale = scaleNow;

        // ---- Movimiento (anchoredPosition por defecto)
        Vector2 posNow = useAnchoredPosition ? _origAnchoredPos : (Vector2)_origLocalPos;
        if (moveOverTime)
        {
            float vx = velX * (velXCurve != null ? velXCurve.Evaluate(k) : 1f);
            float vy = velY * (velYCurve != null ? velYCurve.Evaluate(k) : 1f);
            _moveAccum += new Vector2(vx, vy) * dt;
            posNow += _moveAccum;
        }

        // ---- Shake (mismo tiempo que elegiste)
        if (extraShake && shakeAmplitude > 0f && shakeFrequency > 0f)
        {
            float timeRef = useScaledTime ? Time.time : Time.unscaledTime;
            float sx = (Mathf.PerlinNoise(timeRef * shakeFrequency, 0.123f) - 0.5f) * 2f;
            float sy = (Mathf.PerlinNoise(0.456f, timeRef * shakeFrequency) - 0.5f) * 2f;
            Vector2 offs = new Vector2(sx, sy) * shakeAmplitude * (1f - Mathf.Abs(1f - 2f * k));
            posNow += offs;
        }

        if (_rt != null)
        {
            if (useAnchoredPosition) _rt.anchoredPosition = posNow;
            else _rt.localPosition = new Vector3(posNow.x, posNow.y, _origLocalPos.z);
        }

        // ---- Rotación Z
        if (rotateOverTime && Mathf.Abs(rotDegrees) > 0.0001f)
        {
            float r01 = (rotCurve != null ? rotCurve.Evaluate(k) : k);
            var e = transform.localEulerAngles;
            e.z = rotDegrees * r01;
            transform.localEulerAngles = e;
        }

        // ---- Color (gradiente → luego glow mezcla)
        Color col = _origColor;
        if (colorOverTime && colorGradient != null)
            col = colorGradient.Evaluate(k);

        if (extraPulseGlow && glowIntensity > 0f)
        {
            float tGlow = Mathf.Sin(k * Mathf.PI) * glowIntensity; // 0→1
            col = Color.Lerp(col, glowColor, Mathf.Clamp01(tGlow));
        }

        // mantiene alpha del Image; CanvasGroup gobierna visibilidad global
        col.a = _img.color.a;
        _img.color = col;

        // ---- Alpha multiplicador adicional
        if (extraAlphaCurve && alphaMulCurve != null)
        {
            float mul = Mathf.Clamp01(alphaMulCurve.Evaluate(k));
            var c = _img.color; c.a = Mathf.Clamp01(c.a * mul); _img.color = c;
        }

        // ---- Material float
        if (materialFloatOverTime && _img.material != null && !string.IsNullOrEmpty(materialFloatName))
        {
            if (_matInst == null) { _matInst = new Material(_img.material); _img.material = _matInst; }
            float v = (materialFloatCurve != null ? materialFloatCurve.Evaluate(k) : k) * materialFloatMultiplier;
            _img.material.SetFloat(materialFloatName, v);
        }
    }

    // -----------------------------
    //  CACHE / RESTORE
    // -----------------------------
    private void CacheOriginal()
    {
        _origColor = _img.color;
        _origScale = transform.localScale;
        _origLocalPos = transform.localPosition;
        _origAnchoredPos = (_rt != null) ? _rt.anchoredPosition : Vector2.zero;

        _origType = _img.type;
        _origFillMethod = _img.fillMethod;
        _origFillOrigin = _img.fillOrigin;
        _origFillAmount = _img.fillAmount;
    }

    private void RestoreOriginalImmediate()
    {
        _img.color = _origColor;
        transform.localScale = _origScale;

        if (_rt != null)
        {
            if (useAnchoredPosition) _rt.anchoredPosition = _origAnchoredPos;
            else _rt.localPosition = _origLocalPos;
        }

        _cg.alpha = 1f;

        _img.type = _origType;
        _img.fillMethod = _origFillMethod;
        _img.fillOrigin = _origFillOrigin;
        _img.fillAmount = _origFillAmount;

        if (_matInst != null && materialFloatOverTime && !string.IsNullOrEmpty(materialFloatName))
            _matInst.SetFloat(materialFloatName, 0f);
    }

    // Helpers de Curva
    [ContextMenu("Curve/Set EaseInOut Default")]
    private void Curve_SetDefault() => curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [ContextMenu("Curve/Set Linear 0→1")]
    private void Curve_SetLinear() => curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
