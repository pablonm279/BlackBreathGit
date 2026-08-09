using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBotonesHabilidades : MonoBehaviour
{

    public GameObject actionButtonPrefab;

   
    // Start is called before the first frame update

    private List<BotonHabilidad> listaBotonesHabilidad;
    

    private void Awake()
    {
        listaBotonesHabilidad = new List<BotonHabilidad>();
    }
    
    private void Start()
    {
       ActualizarBotonesHabilidad();
    }
    public void DeseleccionarTodas()
    {
        foreach (BotonHabilidad boton in GetComponentsInChildren<BotonHabilidad>())
        {
            boton.DesactivarHabilidad();
        }
    }

    public UIbotonesPasivas botonesPasivas;
    public void ActualizarBotonesHabilidad()
    {  
        // Las habilidades contextuales de combate se renderizan al final del listado.
        botonesPasivas.ActualizarBotonesPasivas();
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
            Destroy(buttonTransform.gameObject);
        }

       if(listaBotonesHabilidad != null)
       {
        listaBotonesHabilidad.Clear();
       }

      
      if(BattleManager.Instance.unidadActiva != null)
      {
        GameObject unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject;

        List<Habilidad> noHostilNoMelee = new List<Habilidad>();
        List<Habilidad> noHostilMelee = new List<Habilidad>();
        List<Habilidad> hostilNoMelee = new List<Habilidad>();
        List<Habilidad> hostilMelee = new List<Habilidad>();
        List<Habilidad> contextualesCombate = new List<Habilidad>();

        foreach (Habilidad habilidad in unidadSeleccionada.GetComponents<Habilidad>())
        {
            if (habilidad is RetrasarTurno retrasar /*&& retrasar.yaRetraso*/) //Se desactiva retrasar TURNO siempre por ahora
            {
                continue;
            }

            if(habilidad.GetType().Name.Contains("REPRESENTACION"))
            {
                continue;
            }

            if (EsHabilidadContextualDeCombate(habilidad))
            {
                contextualesCombate.Add(habilidad);
                continue;
            }

            if (habilidad.esHostil)
            {
                if (habilidad.esMelee)
                {
                    hostilMelee.Add(habilidad);
                }
                else
                {
                    hostilNoMelee.Add(habilidad);
                }
            }
            else
            {
                if (habilidad.esMelee)
                {
                    noHostilMelee.Add(habilidad);
                }
                else
                {
                    noHostilNoMelee.Add(habilidad);
                }
            }
        }

        foreach (Habilidad habilidad in noHostilNoMelee.OrderBy(h => h.costoAP))
        {
            CrearBotonHabilidad(habilidad);
        }

        foreach (Habilidad habilidad in noHostilMelee.OrderBy(h => h.costoAP))
        {
            CrearBotonHabilidad(habilidad);
        }

        foreach (Habilidad habilidad in hostilNoMelee.OrderBy(h => h.costoAP))
        {
            CrearBotonHabilidad(habilidad);
        }

        foreach (Habilidad habilidad in hostilMelee.OrderBy(h => h.costoAP))
        {
            CrearBotonHabilidad(habilidad);
        }

        foreach (Habilidad habilidad in contextualesCombate.OrderBy(h => h.costoAP))
        {
            CrearBotonHabilidad(habilidad);
        }
      }
    }




    public void UIDesactivarHabilidades(bool omitirTilteo = false)
    {
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
           buttonTransform.GetComponent<BotonHabilidad>().DesactivarHabilidad(omitirTilteo);
        }
    }

    public void UIDesactivarBotones()
    {  
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
          buttonTransform.GetComponent<BotonHabilidad>().DesactivarBoton();
        }
    }

    public void MostrarDisponibilidadTrasMovimiento(int costoMovimiento)
    {
        foreach (BotonHabilidad boton in listaBotonesHabilidad)
        {
            if (boton != null)
            {
                boton.MostrarDisponibilidadTrasMovimiento(costoMovimiento);
            }
        }
    }

    public void LimpiarDisponibilidadTrasMovimiento()
    {
        foreach (BotonHabilidad boton in listaBotonesHabilidad)
        {
            if (boton != null)
            {
                boton.LimpiarDisponibilidadTrasMovimiento();
            }
        }
    }

    bool EsHabilidadContextualDeCombate(Habilidad habilidad)
    {
        return habilidad is DestruirObstaculo || habilidad is Escapar;
    }

    void CrearBotonHabilidad(Habilidad habilidad)
    {
        GameObject actionButtonTransform = Instantiate(actionButtonPrefab, transform);
        BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
        habilidadBotonUI.HabilidadRepresentada = habilidad;
        habilidadBotonUI.UpdateCooldownMuestra();

        if (listaBotonesHabilidad != null)
        {
            listaBotonesHabilidad.Add(habilidadBotonUI);
            habilidadBotonUI.AsignarHotkey();
        }
    }

    public bool ActivarHabilidadPorHotkeyIndex(int index)
    {
        if (listaBotonesHabilidad == null || index < 0)
        {
            return false;
        }

        int indiceFiltrado = 0;
        foreach (BotonHabilidad boton in listaBotonesHabilidad)
        {
            if (boton == null || boton.HabilidadRepresentada == null)
            {
                continue;
            }

            if (boton.HabilidadRepresentada is RetrasarTurno)
            {
                continue;
            }

            if (indiceFiltrado == index)
            {
                if (BattleManager.Instance != null
                    && BattleManager.Instance.HabilidadActiva == boton.HabilidadRepresentada
                    && BattleManager.Instance.TryConfirmarHabilidadAutoObjetivo())
                {
                    return true;
                }

                boton.ActivarHabilidad(false);
                return true;
            }

            indiceFiltrado++;
        }

        return false;
    }
}
