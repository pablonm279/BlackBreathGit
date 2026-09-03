using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public enum FloatingTextContext
{
    Generic = 0,
    Damage = 1,
    CriticalDamage = 2,
    Block = 3,
    Heal = 4,
    Resist = 5,
    Miss = 6,
    BuffApply = 7,
    BuffEnd = 8,
    ValourGain = 9,
    ValourLoss = 10,
    ArmorAbsorb = 11
}

[Serializable]
public class FloatingTextProfile
{
    [Tooltip("Duracion total en segundos.")]
    public float lifetime = 1.6f;

    [Tooltip("Offset inicial sobre la posicion del texto (anchoredPosition).")]
    public Vector2 initialOffset = Vector2.zero;

    [Tooltip("Velocidad vertical (unidades de RectTransform por segundo).")]
    public float verticalSpeed = 15f;

    [Tooltip("Velocidad horizontal constante (usar 0 para solo movimiento vertical).")]
    public float horizontalSpeed = 0f;

    [Tooltip("Curva de escala (tiempo normalizado 0-1).")]
    public AnimationCurve scaleCurve = CreateDefaultScaleCurve();

    [Tooltip("Curva de alpha (0-1). Se multiplica por el color que se envia.")]
    public AnimationCurve alphaCurve = CreateDefaultAlphaCurve();

    [Tooltip("Aplicar gradiente para modificar el color base segun el tiempo.")]
    public bool useColorGradient = false;

    public Gradient colorGradient = CreateDefaultGradient();

    [Tooltip("Ruido en eje X basado en Perlin.")]
    public float noiseAmplitude = 0f;

    [Tooltip("Frecuencia del ruido Perlin.")]
    public float noiseFrequency = 1.5f;

    [Tooltip("Vibracion adicional (seno/coseno) aplicada a X/Y.")]
    public Vector2 vibrationAmplitude = Vector2.zero;

    [Tooltip("Frecuencia de la vibracion senoidal.")]
    public float vibrationFrequency = 12f;

    public void Validate()
    {
        if (lifetime < 0.1f) { lifetime = 0.1f; }
        if (scaleCurve == null || scaleCurve.length == 0) { scaleCurve = CreateDefaultScaleCurve(); }
        if (alphaCurve == null || alphaCurve.length == 0) { alphaCurve = CreateDefaultAlphaCurve(); }
        if (colorGradient == null) { colorGradient = CreateDefaultGradient(); }
        if (noiseFrequency <= 0f) { noiseFrequency = 0.1f; }
        if (vibrationFrequency <= 0f) { vibrationFrequency = 1f; }
    }

    public static AnimationCurve CreateDefaultScaleCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.25f, 1.05f),
            new Keyframe(1f, 0.95f));
    }

    public static AnimationCurve CreateDefaultAlphaCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 1f),
            new Keyframe(1f, 0f));
    }

    public static Gradient CreateDefaultGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return gradient;
    }

    public static FloatingTextProfile Clone(FloatingTextProfile source)
    {
        if (source == null) { return null; }
        return new FloatingTextProfile
        {
            lifetime = source.lifetime,
            initialOffset = source.initialOffset,
            verticalSpeed = source.verticalSpeed,
            horizontalSpeed = source.horizontalSpeed,
            scaleCurve = new AnimationCurve(source.scaleCurve.keys),
            alphaCurve = new AnimationCurve(source.alphaCurve.keys),
            useColorGradient = source.useColorGradient,
            colorGradient = source.colorGradient,
            noiseAmplitude = source.noiseAmplitude,
            noiseFrequency = source.noiseFrequency,
            vibrationAmplitude = source.vibrationAmplitude,
            vibrationFrequency = source.vibrationFrequency
        };
    }
}

public class FloatingTextAnimator : MonoBehaviour
{
    private static readonly Regex RegexTachadoSimple = new Regex(@"<s>(.*?)</s>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    [Header("Perfiles por contexto")]
    [SerializeField] private FloatingTextProfile genericProfile = CreateGenericProfile();
    [SerializeField] private FloatingTextProfile damageProfile = CreateDamageProfile();
    [SerializeField] private FloatingTextProfile criticalProfile = CreateCriticalProfile();
    [SerializeField] private FloatingTextProfile blockProfile = CreateBlockProfile();
    [SerializeField] private FloatingTextProfile armorAbsorbProfile = CreateArmorAbsorbProfile();
    [SerializeField] private FloatingTextProfile healProfile = CreateHealProfile();
    [SerializeField] private FloatingTextProfile resistProfile = CreateResistProfile();
    [SerializeField] private FloatingTextProfile missProfile = CreateMissProfile();
    [SerializeField] private FloatingTextProfile buffApplyProfile = CreateBuffApplyProfile();
    [SerializeField] private FloatingTextProfile buffEndProfile = CreateBuffEndProfile();
    [SerializeField] private FloatingTextProfile valourGainProfile = CreateValourGainProfile();
    [SerializeField] private FloatingTextProfile valourLossProfile = CreateValourLossProfile();

    private TextMeshProUGUI tmp;
    private RectTransform rect;
    private Coroutine runningRoutine;
    private float randomSeed;
    private Vector2 baseAnchoredPosition;
    private Vector2 initialAnchoredPosition;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        randomSeed = UnityEngine.Random.Range(0f, 1000f);
        initialAnchoredPosition = rect != null ? rect.anchoredPosition : Vector2.zero;
        baseAnchoredPosition = initialAnchoredPosition;

        genericProfile?.Validate();
        damageProfile?.Validate();
        criticalProfile?.Validate();
        blockProfile?.Validate();
        armorAbsorbProfile?.Validate();
        healProfile?.Validate();
        resistProfile?.Validate();
        missProfile?.Validate();
        buffApplyProfile?.Validate();
        buffEndProfile?.Validate();
        valourGainProfile?.Validate();
        valourLossProfile?.Validate();
    }

    public void SetBasePosition(Vector2 anchoredPosition)
    {
        baseAnchoredPosition = anchoredPosition;
    }

    public Vector2 GetInitialBasePosition()
    {
        return initialAnchoredPosition;
    }

    public float GetLifetime(FloatingTextContext context)
    {
        FloatingTextProfile profile = ResolveProfile(context);
        if (profile == null)
        {
            return 0f;
        }

        profile.Validate();
        return profile.lifetime;
    }

    public Coroutine Play(string text, Color color, FloatingTextContext context)
    {
        if (tmp == null || rect == null)
        {
            return null;
        }

        text = NormalizarTextoRichText(text);

        if (!isActiveAndEnabled)
        {
            tmp.richText = true;
            tmp.text = text;
            tmp.color = color;
            rect.anchoredPosition = baseAnchoredPosition;
            rect.localScale = Vector3.one;
            return null;
        }

        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
        }

        runningRoutine = StartCoroutine(PlayRoutine(text, color, context));
        return runningRoutine;
    }

    public IEnumerator PlayRoutine(string text, Color color, FloatingTextContext context)
    {
        if (tmp == null || rect == null)
        {
            yield break;
        }

        text = NormalizarTextoRichText(text);

        FloatingTextProfile profile = ResolveProfile(context);
        profile.Validate();

        tmp.richText = true;
        tmp.text = text;
        rect.anchoredPosition = baseAnchoredPosition;
        rect.localScale = Vector3.one;

        float elapsed = 0f;
        float lifetime = profile.lifetime;

        while (elapsed < lifetime)
        {
            float t = Mathf.Clamp01(elapsed / lifetime);

            Vector2 offset = profile.initialOffset;
            offset += new Vector2(profile.horizontalSpeed * elapsed, profile.verticalSpeed * elapsed);

            if (profile.noiseAmplitude > 0f)
            {
                float noise = (Mathf.PerlinNoise(randomSeed, elapsed * profile.noiseFrequency) - 0.5f) * 2f;
                offset.x += noise * profile.noiseAmplitude;
            }

            if (profile.vibrationAmplitude.sqrMagnitude > 0f)
            {
                offset.x += Mathf.Sin((elapsed + randomSeed) * profile.vibrationFrequency) * profile.vibrationAmplitude.x;
                offset.y += Mathf.Cos((elapsed + randomSeed) * profile.vibrationFrequency) * profile.vibrationAmplitude.y;
            }

            rect.anchoredPosition = baseAnchoredPosition + offset;

            float scaleValue = Mathf.Max(0.01f, profile.scaleCurve.Evaluate(t));
            rect.localScale = Vector3.one * scaleValue;

            Color tinted = color;
            if (profile.useColorGradient)
            {
                Color gradientColor = profile.colorGradient.Evaluate(t);
                tinted = MultiplyColors(color, gradientColor);
            }

            float alpha = Mathf.Clamp01(profile.alphaCurve.Evaluate(t));
            tinted.a *= alpha;
            tmp.color = tinted;

            elapsed += Time.deltaTime;
            yield return null;
        }

        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 0f);
        rect.localScale = Vector3.zero;
        runningRoutine = null;
    }

    private FloatingTextProfile ResolveProfile(FloatingTextContext context)
    {
        switch (context)
        {
            case FloatingTextContext.Damage:
                return damageProfile != null ? FloatingTextProfile.Clone(damageProfile) : FloatingTextProfile.Clone(genericProfile);
            case FloatingTextContext.CriticalDamage:
                if (criticalProfile != null) { return FloatingTextProfile.Clone(criticalProfile); }
                if (damageProfile != null) { return FloatingTextProfile.Clone(damageProfile); }
                break;
            case FloatingTextContext.Block:
                if (blockProfile != null) { return FloatingTextProfile.Clone(blockProfile); }
                break;
            case FloatingTextContext.ArmorAbsorb:
                if (armorAbsorbProfile != null) { return FloatingTextProfile.Clone(armorAbsorbProfile); }
                if (blockProfile != null) { return FloatingTextProfile.Clone(blockProfile); }
                break;
            case FloatingTextContext.Heal:
                if (healProfile != null) { return FloatingTextProfile.Clone(healProfile); }
                break;
            case FloatingTextContext.Resist:
                if (resistProfile != null) { return FloatingTextProfile.Clone(resistProfile); }
                break;
            case FloatingTextContext.Miss:
                if (missProfile != null) { return FloatingTextProfile.Clone(missProfile); }
                break;
            case FloatingTextContext.BuffApply:
                if (buffApplyProfile != null) { return FloatingTextProfile.Clone(buffApplyProfile); }
                break;
            case FloatingTextContext.BuffEnd:
                if (buffEndProfile != null) { return FloatingTextProfile.Clone(buffEndProfile); }
                break;
            case FloatingTextContext.ValourGain:
                if (valourGainProfile != null) { return FloatingTextProfile.Clone(valourGainProfile); }
                if (buffApplyProfile != null) { return FloatingTextProfile.Clone(buffApplyProfile); }
                break;
            case FloatingTextContext.ValourLoss:
                if (valourLossProfile != null) { return FloatingTextProfile.Clone(valourLossProfile); }
                if (buffEndProfile != null) { return FloatingTextProfile.Clone(buffEndProfile); }
                break;
        }

        return FloatingTextProfile.Clone(genericProfile);
    }

    private static Color MultiplyColors(Color a, Color b)
    {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    public static string NormalizarTextoRichText(string texto)
    {
        if (string.IsNullOrEmpty(texto) || texto.IndexOf("<s>", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return texto;
        }

        return RegexTachadoSimple.Replace(texto, match => "- " + match.Groups[1].Value);
    }

    private static FloatingTextProfile CreateGenericProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 22f,
            horizontalSpeed = 0f,
            noiseAmplitude = 0f,
            vibrationAmplitude = Vector2.zero,
            vibrationFrequency = 10f
        };
    }

    private static FloatingTextProfile CreateDamageProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.6f,
            verticalSpeed = 24f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.2f, 1.07f),
                new Keyframe(1f, 0.96f))
        };
    }

    private static FloatingTextProfile CreateCriticalProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.7f,
            verticalSpeed = 26f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1.05f),
                new Keyframe(0.25f, 1.12f),
                new Keyframe(1f, 0.98f))
        };
    }

    private static FloatingTextProfile CreateBlockProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.4f,
            verticalSpeed = 20f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.15f, 1.04f),
                new Keyframe(1f, 0.97f))
        };
    }

    private static FloatingTextProfile CreateArmorAbsorbProfile()
    {
        return CreateBlockProfile();
    }

    private static FloatingTextProfile CreateHealProfile()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.7f, 1f, 0.75f), 0f),
                new GradientColorKey(new Color(0.3f, 0.85f, 0.6f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });

        return new FloatingTextProfile
        {
            lifetime = 1.6f,
            verticalSpeed = 21f,
            horizontalSpeed = 0f,
            useColorGradient = true,
            colorGradient = gradient,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.98f),
                new Keyframe(0.3f, 1.05f),
                new Keyframe(1f, 1f))
        };
    }

    private static FloatingTextProfile CreateResistProfile()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.75f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.55f, 0.8f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });

        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 20f,
            horizontalSpeed = 0f,
            useColorGradient = true,
            colorGradient = gradient,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.99f),
                new Keyframe(0.25f, 1.04f),
                new Keyframe(1f, 0.98f))
        };
    }

    private static FloatingTextProfile CreateMissProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 19f,
            horizontalSpeed = 0f,
            alphaCurve = new AnimationCurve(
                new Keyframe(0f, 0.15f),
                new Keyframe(0.25f, 0.55f),
                new Keyframe(0.7f, 0.65f),
                new Keyframe(1f, 0.35f)),
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.96f),
                new Keyframe(0.3f, 1.02f),
                new Keyframe(1f, 0.96f))
        };
    }

    private static FloatingTextProfile CreateBuffApplyProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.45f,
            verticalSpeed = 18f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.98f),
                new Keyframe(0.25f, 1.03f),
                new Keyframe(1f, 0.98f))
        };
    }

    private static FloatingTextProfile CreateBuffEndProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 16f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1.02f),
                new Keyframe(0.2f, 1.06f),
                new Keyframe(1f, 0.97f))
        };
    }

    private static FloatingTextProfile CreateValourGainProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 20f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.98f),
                new Keyframe(0.24f, 1.06f),
                new Keyframe(1f, 0.98f))
        };
    }

    private static FloatingTextProfile CreateValourLossProfile()
    {
        return new FloatingTextProfile
        {
            lifetime = 1.5f,
            verticalSpeed = 18f,
            horizontalSpeed = 0f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1.01f),
                new Keyframe(0.22f, 1.07f),
                new Keyframe(1f, 0.97f))
        };
    }
}
