using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TrampaMiradamasacre : Trampa
{
  
  public Unidad unidadCreadora;
  bool destruidaPorMuerteCreador;
  bool creadorAsignado;
  public void InicializarCreador(Unidad creadora)
  {
    unidadCreadora = creadora;
     Invoke("Activar", 0.5f);
    Inicializar();
    creadorAsignado = true;
    
  
  }

  public void Inicializar()
  {
    nombre = "Mirada de Masacre";
    intDificultadVer = 0;
    intUsos = 1;
    intDuracionTurnos = 1;
    esPersistente = false;
    ActivarVFXModeloTrampa();
   

  }

  bool activa = false;

  void Activar()
  { 
    activa = true;
  }

  void Update()
  {
    VerificarCreadorSigueActivo();
  }

  public async override void AplicarEfectosTrampa(Unidad objetivo)
  {
    if (activa != true)
    {
      return;
    }
    if (unidadCreadora != null)
    {


      await Task.Delay(200);

      if (objetivo.TiradaSalvacion(objetivo.mod_TSMental, 13))
      {
        objetivo.CambiarAPActual(-(int)objetivo.ObtenerAPActual());
        objetivo.GenerarTextoFlotante("Aterrado", Color.red);
        BattleManager.Instance.EscribirLog(objetivo.uNombre + TRADU.i.Traducir(" se aterra por Mirada de la Masacre y pierde el turno."));

        //---
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Por la masacre";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantDanioPorcentaje += 20;
        buff.cantAtaque += 2;
        buff.esStackeable = false;
        buff.AplicarBuff(unidadCreadora);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, unidadCreadora.gameObject);       

      }


     

      //--------------------------

      ReducirUsos();
    }

  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaMiradaMasacre;
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

  void VerificarCreadorSigueActivo()
  {
    if (destruidaPorMuerteCreador)
    {
      return;
    }

    if (!creadorAsignado)
    {
      return;
    }

    if (unidadCreadora == null)
    {
      DestruirTrampaPorMuerteCreador();
      return;
    }

    if (unidadCreadora.HP_actual <= 0 || !unidadCreadora.gameObject.activeInHierarchy)
    {
      DestruirTrampaPorMuerteCreador();
    }
  }

  void DestruirTrampaPorMuerteCreador()
  {
    if (destruidaPorMuerteCreador)
    {
      return;
    }

    destruidaPorMuerteCreador = true;
    DestruirTrampa();
  }

}
