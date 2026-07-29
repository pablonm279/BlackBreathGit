using UnityEngine;

public class disclaimerManager : MonoBehaviour
{
    public static bool DisclaimerCerradoEstaSesion { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEstadoSesion()
    {
        DisclaimerCerradoEstaSesion = false;
    }

    void Start()
    {
        if (DisclaimerCerradoEstaSesion)
        {
            gameObject.SetActive(false);
            return;
        }

        SeleccionarHijoSegunIdioma();
    }

    public void Continuar()
    {
        RuntimeAnalytics.SetTelemetryEnabled(true);
        CerrarDisclaimer();
    }

    public void ContinuarSinTelemetria()
    {
        RuntimeAnalytics.SetTelemetryEnabled(false);
        CerrarDisclaimer();
    }

    private void CerrarDisclaimer()
    {
        DisclaimerCerradoEstaSesion = true;
        gameObject.SetActive(false);
    }

    public void SeleccionarHijoSegunIdioma()
    {
        Transform disclaimerEsp = transform.Find("DisclaimerESP");
        Transform disclaimerEng = transform.Find("DisclaimerENG");
        Transform disclaimerPor = transform.Find("DisclaimerPOR");

        if (disclaimerEsp != null)
        {
            disclaimerEsp.gameObject.SetActive(false);
        }

        if (disclaimerEng != null)
        {
            disclaimerEng.gameObject.SetActive(false);
        }

        if (disclaimerPor != null)
        {
            disclaimerPor.gameObject.SetActive(false);
        }

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);

        switch (idioma)
        {
            case TRADU.IdiomaIngles:
                if (disclaimerEng != null)
                {
                    disclaimerEng.gameObject.SetActive(true);
                }
                break;
            case TRADU.IdiomaPortugues:
                if (disclaimerPor != null)
                {
                    disclaimerPor.gameObject.SetActive(true);
                }
                break;
            default:
                if (disclaimerEsp != null)
                {
                    disclaimerEsp.gameObject.SetActive(true);
                }
                break;
        }
    }
}
