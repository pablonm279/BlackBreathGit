using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VigilanciaTrampa : Trampa
{
  
  TiroconArco tiroArco;
  public void InicializarCreador(Unidad creadora)
  {
    AsignarCreador(creadora);
    Inicializar();
  }
  
  public void Inicializar()
  {
     nombre = "Vigilancia";
     intDificultadVer = 0;   
     intUsos = 1;
     intDuracionTurnos = 1;
     esPersistente = false;
     tiroArco =  unidadCreadora.GetComponent<TiroconArco>();
     ActivarVFXModeloTrampa();

  }



  public async override void AplicarEfectosTrampa(Unidad objetivo)
  {

    Vigilancia vigilancia = unidadCreadora.GetComponent<Vigilancia>();
    if (vigilancia == null)
    {
      Debug.LogWarning("VigilanciaTrampa no encontró la habilidad Vigilancia en la unidad creadora.");
      return;
    }
    if (tiroArco == null)
    {
      Debug.LogWarning("VigilanciaTrampa no encontró TiroconArco en la unidad creadora.");
      return;
    }

    ClaseExplorador claseExplorador = unidadCreadora.GetComponent<ClaseExplorador>();
    if (claseExplorador == null)
    {
      Debug.LogWarning("VigilanciaTrampa no encontró ClaseExplorador en la unidad creadora.");
      return;
    }

    if (vigilancia.disparosEsteTurno > 0)
    {
      if (claseExplorador.ObtenerCantidadFlechas() < 1)
      {
        string mensajeSinFlechas = TRADU.i != null && TRADU.i.nIdioma == 2
          ? "No more arrows"
          : TRADU.i != null && TRADU.i.nIdioma == 3
            ? "Não há mais flechas"
            : "No hay más flechas";
        _ = unidadCreadora.GenerarTextoFlotante(mensajeSinFlechas, Color.gray, FloatingTextContext.Resist);
        if (BattleManager.Instance != null)
        {
          BattleManager.Instance.EscribirLog(mensajeSinFlechas + ".");
        }
        ReducirUsos();
        return;
      }

      vigilancia.disparosEsteTurno--;
      //objetivo.AccionP_actual = 0; //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona

      unidadCreadora.ReproducirAnimacionHabilidadNoHostil();

      List<object> objetivos = objetivo != null ? new List<object> { objetivo } : null;
      if (objetivos != null)
      {
        await tiroArco.PrepararImpactoManualAsync(objetivos, unidadCreadora.CasillaPosicion);
      }

      int tirada = UnityEngine.Random.Range(1, 21);

      if (vigilancia.NIVEL > 1)
      {
        tirada += 1;
      }
      if (vigilancia.NIVEL > 2)
      {
        tirada += 1;
      }

      tiroArco.AplicarEfectosHabilidadConTipoDanio(objetivo, tirada, vigilancia.TipoDanioReaccion, null);

      if (objetivos != null)
      {
        await tiroArco.FinalizarImpactoManualAsync(objetivos, unidadCreadora.CasillaPosicion);
      }

      string unidadNombre = TRADU.i != null ? TRADU.i.Traducir(unidadCreadora.uNombre) : unidadCreadora.uNombre;
      string verboReacciona = TRADU.i != null ? TRADU.i.Traducir("reacciona con ") : "reacciona con ";
      string nombreHab = TRADU.i != null ? TRADU.i.Traducir(nombre) : nombre;
      BattleManager.Instance.EscribirLog(unidadNombre + " " + verboReacciona + nombreHab + ".");

      //--------------------------
    }
      ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaVigilancia;
    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation) as GameObject;
    // Ajusta la posición en el eje Y
    Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += 0.015f;
    GOvfx.transform.position = newPosition;

     Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) -2;
    }
  }

}


