using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
// using TMPro; // removido (no se usa)

public class AdministradorEscenas : MonoBehaviour
{

  public GameObject EscenaCampaign;
  public GameObject EscenaBatalla;
  public ContenedorPrefabs ContenedorPrefabsBatalla;

  public int escenaActual; //0 campaña 1 batalla


  public Personaje Personaje1;
  public Personaje Personaje2;
  public Personaje Personaje3;
  public Personaje Personaje4;

  public Unidad unidadPers1;
  public Unidad unidadPers2;
  public Unidad unidadPers3;
  public Unidad unidadPers4;

  // Personajes forzados a desplegar en Ataque a Caravana (se les aplica "Sorprendido" al inicio)
  public List<Personaje> PersonajesSorprendidosInicioCaravana = new List<Personaje>();


  public MeshRenderer mrFondoBatalla;

 public List<Material> listaFondosBosqueLamentos; //Para agregar fondos simplemente hay que agregarlos a la lista
 public List<Material> listaFondosSubterraneos; //Para agregar fondos simplemente hay que agregarlos a la lista

  [Header("UI")]
  [SerializeField] CanvasGroup fader;        // Imagen negra con CanvasGroup
  [SerializeField] float fadeTime = 0.4f;

  // Bloqueo para mantener el fader negro (evita fade-outs concurrentes)
  bool faderHold = false;

  // (removido) UI de carga sutil

    IEnumerator Start()
    {
        // Fade-in al entrar al menú
        yield return FadeTo(0f, fadeTime);
    }

   public void PlayFadeInOut(float fadeDuration, float holdDuration)
{
    StartCoroutine(FadeInOut(fadeDuration, holdDuration));
}

private IEnumerator FadeInOut(float fadeDuration, float holdDuration)
{
    // Se oscurece (fade in)
    yield return StartCoroutine(FadeTo(1f, fadeDuration));

    // Mantiene la pantalla negra
    if (holdDuration > 0f)
        yield return new WaitForSecondsRealtime(holdDuration);

    // Se aclara (fade out)
    yield return StartCoroutine(FadeTo(0f, fadeDuration));
}

IEnumerator FadeTo(float target, float time)
{
    if (fader == null) yield break;
    // Si intentan hacer fade-out mientras está bloqueado, esperar hasta liberar
    if (faderHold && target < 1f)
    {
        while (faderHold) { yield return null; }
    }
    float start = fader.alpha;
    float t = 0f;
        while (t < time)
        {
            // Si durante la animación se activa un hold y el destino es fade-out, pausar hasta liberar y reanudar suave
            if (faderHold && target < 1f)
            {
                while (faderHold) { yield return null; }
                start = fader.alpha;
                t = 0f;
            }
            t += Time.unscaledDeltaTime;
            fader.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        fader.alpha = target;
    }

    // Expuestos para poder hacer yield desde otras clases (p.ej. AtributosZona)
    public IEnumerator FadeIn(float duration)
    {
        yield return FadeTo(1f, duration);
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return FadeTo(0f, duration);
    }

    // Forzar mantener pantalla negra y bloquear fade-outs hasta liberar
    public void SetFaderHold(bool hold)
    {
        faderHold = hold;
        if (fader != null && hold)
        {
            fader.alpha = 1f;
            fader.blocksRaycasts = true;
            fader.interactable = false;
        }
        else if (fader != null && !hold)
        {
            fader.blocksRaycasts = false;
        }
    }

  // (removido) Loading label y animación de puntos

  public async void CargarBatalla(int IDEncuentro, int esEmboscada = 0)
  {
    escenaActual = 1; //1 Batalla 0 Campaña
    CampaignManager.Instance.logDeCampania.LimpiarDesdeCampania();
    int idZona = CampaignManager.Instance.scAtributosZona.ID;
    MusicManager.Instance.PlayBatalla(idZona);
    CampaignManager.Instance.scAdministradorEscenas.PlayFadeInOut(0.5f, 1.6f);
    await Task.Delay(TimeSpan.FromSeconds(1.6f));
    EscenaCampaign.SetActive(false);
    EscenaBatalla.SetActive(true);
    // Silenciar logs de combate durante la preparación (buffs/estados iniciales)
    BattleManager.Instance.silenciarLogCombate = true; 
    bool bonusiniciativaTutorial = false;
    BattleManager.Instance.scTutorialCombate.IniciarPrimerCombate(); bonusiniciativaTutorial = true; 




    if (esEmboscada == 3) //Si es defensa de caravana pone defensas
    {
      CrearDefensasCaravana();
    }

    AdministrarFondos(IDEncuentro);

    if (Personaje1 != null)
    {
      ColocarPersonajecomoUnidad(Personaje1);
    }
    if (Personaje2 != null)
    {
      ColocarPersonajecomoUnidad(Personaje2);
    }
    if (Personaje3 != null)
    {
      ColocarPersonajecomoUnidad(Personaje3);
    }
    if (Personaje4 != null)
    {
      ColocarPersonajecomoUnidad(Personaje4);
    }

    //Inicializa ComienzoBatallaClase una vez asignados ya todos los aliados
    if (unidadPers1 != null)
    {
      unidadPers1.ComienzoBatallaClase();

      if (bonusiniciativaTutorial) { unidadPers1.mod_iniciativa += 20;  unidadPers1.iniciativa_actual += 20;}
    }
    if (unidadPers2 != null)
    {
      unidadPers2.ComienzoBatallaClase();
    }
    if (unidadPers3 != null)
    {
      unidadPers3.ComienzoBatallaClase();
    }
    if (unidadPers4 != null)
    {
      unidadPers4.ComienzoBatallaClase();
    }



    CrearEncuentroEnemigos(IDEncuentro);

    // Actualizar listas de unidades tras colocar aliados y enemigos
    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();
      BattleManager.Instance.ladoB.ActualizarListaDeUnidadesEnLado();
    }




    //Si es emboscada, se le da la iniciativa a los enemigos
    if (esEmboscada == 1)
    {
      LadoManager ladoEnemigo2 = BattleManager.Instance.ladoA; //Enemigos
      foreach (Unidad u in ladoEnemigo2.unidadesLado)
      {
        if (u == null) { continue; }
        u.mod_iniciativa += 3; //Aumenta la iniciativa de los enemigos
      }


      LadoManager ladoBueno = BattleManager.Instance.ladoB; //Buenos
      foreach (Unidad u in ladoBueno.unidadesLado)
      {
        if (u == null) { continue; }
        //Saca Escondido para los que arrancan combate escondidos
        u.PerderEscondido(); //Remueve el estado escondido de la unidad

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Sorprendido";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 1;
        buff.cantIniciativa -= 3;
        buff.cantAPMax -= 2;
        buff.cantDefensa -= 2;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

    }

    if (esEmboscada == 3)//Ataque a Caravana
    {
      // Evitar que los personajes forzados aparezcan tambin como refuerzos: marcar temporalmente como "Guardia"
      List<System.Tuple<Personaje,int>> actividadesPrevias = new List<System.Tuple<Personaje,int>>();
      if (PersonajesSorprendidosInicioCaravana != null && PersonajesSorprendidosInicioCaravana.Count > 0)
      {
        foreach (var p in PersonajesSorprendidosInicioCaravana)
        {
          if (p == null) continue;
          actividadesPrevias.Add(new System.Tuple<Personaje,int>(p, p.ActividadSeleccionada));
          p.ActividadSeleccionada = 3; // Guardia
        }
      }

      AsignarRefuerzosAliados();

      // Restaurar actividades previas
      if (actividadesPrevias.Count > 0)
      {
        foreach (var parAct in actividadesPrevias)
        {
          if (parAct.Item1 != null) parAct.Item1.ActividadSeleccionada = parAct.Item2;
        }
      }

      // Aplicar "Sorprendido" (2 rondas) solo a los que fueron forzados a desplegar
      if (PersonajesSorprendidosInicioCaravana != null && PersonajesSorprendidosInicioCaravana.Count > 0)
      {
        // Aplicar a cada uno de los 4 posibles participantes si corresponde
        void AplicarSorprendidoSiCorresponde(Personaje pers, Unidad uni)
        {
          if (pers == null || uni == null) return;
          if (!PersonajesSorprendidosInicioCaravana.Contains(pers)) return;

          // Remover ocultamiento si lo tuviera
          uni.PerderEscondido();

          // Debuff Sorprendido: mismos efectos que emboscada, pero 2 rondas
          Buff buff = new Buff();
          buff.buffNombre = "Sorprendido";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = 2;
          buff.cantIniciativa -= 3;
          buff.cantAPMax -= 2;
          buff.cantDefensa -= 2;
          buff.AplicarBuff(uni);
          Buff buffComponent = ComponentCopier.CopyComponent(buff, uni.gameObject);
        }

        AplicarSorprendidoSiCorresponde(Personaje1, unidadPers1);
        AplicarSorprendidoSiCorresponde(Personaje2, unidadPers2);
        AplicarSorprendidoSiCorresponde(Personaje3, unidadPers3);
        AplicarSorprendidoSiCorresponde(Personaje4, unidadPers4);

        // Limpiar la lista para evitar reutilizar en próximos combates
        PersonajesSorprendidosInicioCaravana.Clear();
      }
    }

    //Efectos ambientales de Batalla
    #region Clima en Batalla

    // Refrescar listas de unidades antes de aplicar clima
    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();
      BattleManager.Instance.ladoB.ActualizarListaDeUnidadesEnLado();
    }

    int climaCampaña = CampaignManager.Instance.intTipoClima;
    if (IDEncuentro > 400 && IDEncuentro < 450) { climaCampaña = 0; }//Si es encuentro subterraneo no hay clima

    if (climaCampaña == 1)//Normal (soleado)
    {
      //--- 
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_sol;
    }
    if (climaCampaña == 2)//Calor
    {
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acalorado";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie += 2;
        buff.percResFue -= 3;
        buff.AplicarBuff(u, null, false); //OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_calor;

    }
    if (climaCampaña == 3)//Lluvia
    {
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Mojado";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie -= 2;
        buff.cantResRay -= 4;
        buff.cantResFue += 3;
        buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_lluvia;
    }
    if (climaCampaña == 4)//Nieve
    {
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Frío";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie -= 3;
        buff.cantAPMax -= 1;
        buff.cantResFue += 2;
        buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_nieve;
    }
    if (climaCampaña == 5)//Niebla
    {
      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_niebla;
    }
    #endregion
    #region Corrupcion
    LadoManager ladodeEnemigo = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladodeEnemigo.unidadesLado)
    {
      if (u == null) { continue; }
      int tierAlientoNegro = (int)CampaignManager.Instance.GetTierAlientoNegro();

      if (u.TieneTag("Corrupto"))
      {
        if (tierAlientoNegro < 2)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Débil";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = -1;
          buff.cantDanioPorcentaje -= 5;
          buff.cantAtaque -= 1;
          buff.percTsMental -= 1;
          buff.percTsFortaleza -= 1;
          buff.cantResNec -= 5;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro == 2)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Presente";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 4;
          buff.percTsMental += 1;
          buff.percHPMax += 5;
          buff.percTsFortaleza += 1;
          u.estado_regeneravida = 1;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro == 3)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Fuerte";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 6;
          buff.cantAtaque += 1;
          buff.percTsMental += 2;
          buff.percHPMax += 10;
          buff.percTsFortaleza += 2;
          u.estado_regeneravida = 3;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro > 3)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Empoderante";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 8;
          buff.cantAtaque += 2;
          buff.percTsMental += 3;
          buff.percHPMax += 15;
          buff.percTsFortaleza += 3;
          buff.AplicarBuff(u);
          u.estado_regeneravida = 5;
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
      }
    }
    #endregion
    #region Noche
    int randomNoche = UnityEngine.Random.Range(1, 101); //1-100
    if (IDEncuentro > 400 && IDEncuentro < 450) { randomNoche = 0; }//Si es encuentro subterraneo siempre es "noche"

    if (randomNoche <= 25) //probabilidad noche 
    {
      BattleManager.Instance.nocheLienzo.SetActive(true);
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (!(u is ClaseAcechador) && !u.inmunidad_Oscuridad) //El acechador y criaturas inmunes no reciben debuff
        {
          //---
          //BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Oscuridad";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = -1;
          buff.cantPMMax -= 1;
          buff.cantTsMental -= 2;
          buff.cantAtaque -= 1;
          buff.cantResNec -= 2;
          buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
                                           // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
      }

    }
    else
    {
      BattleManager.Instance.nocheLienzo.SetActive(false);
    }
    #endregion




    //Forzar enemigos a tirar iniciativa
    LadoManager ladoEnemigo = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladoEnemigo.unidadesLado)
    {
      u.HP_actual = u.mod_maxHP;
      u.TirarIniciativa();
      u.ComienzoBatallaEnemigo();
    }
    // El inicio de la primera ronda y el primer turno ya los gestiona BattleManager.RondaNueva()
    // Asegurar que la primera ronda comience en 1.
    // RondaNueva() incrementa el contador, por eso se inicializa en 0 aquí.
    BattleManager.Instance.RondaNro = 0;
    BattleManager.Instance.RondaNueva();

  }

  void AdministrarFondos(int idEncuentro)
  {
    print("AdministrarFondos called with idEncuentro: " + idEncuentro + " IDZonaActual: " + CampaignManager.Instance.scAtributosZona.Nombre);
    if (idEncuentro > 400 && idEncuentro < 450) //Si es encuentro subterraneo
    {
              mrFondoBatalla.material = listaFondosSubterraneos[UnityEngine.Random.Range(0, listaFondosSubterraneos.Count)];
    }
    else
    {
      string IDZonaActual = CampaignManager.Instance.scAtributosZona.Nombre;

      if (IDZonaActual == "Bosque Angustiante") //FONDOS BOSQUE DE LOS LAMENTOS
      {
        mrFondoBatalla.material = listaFondosBosqueLamentos[UnityEngine.Random.Range(0, listaFondosBosqueLamentos.Count)];
      }


    }

    
   



 }
  public void ColocarPersonajecomoUnidad(Personaje pers, int indexRefuezo = 0)
  {

    GameObject prefabClase = null;
    switch (pers.IDClase)
    {
      case 1: prefabClase = BattleManager.Instance.prefabUnidadCaballero; break;
      case 2: prefabClase = BattleManager.Instance.prefabUnidadExplorador; break;
      case 3: prefabClase = BattleManager.Instance.prefabUnidadPurificadora; break;
      case 4: prefabClase = BattleManager.Instance.prefabUnidadAcechador; break;
      case 5: prefabClase = BattleManager.Instance.prefabUnidadCanalizador; break;
        //-----
    }


    GameObject persUnidad = Instantiate(prefabClase);
    switch (pers.IDClase) // Cuerpos Clase
    {
      case 1: persUnidad.GetComponent<Unidad>().uImage.sprite = ContenedorPrefabsBatalla.Heroe1; break;
      case 2: persUnidad.GetComponent<Unidad>().uImage.sprite = ContenedorPrefabsBatalla.ExploradorCuerpo1; break;
      case 3: persUnidad.GetComponent<Unidad>().uImage.sprite = ContenedorPrefabsBatalla.PurificadoraCuerpo1; break;
      case 4: persUnidad.GetComponent<Unidad>().uImage.sprite = ContenedorPrefabsBatalla.AcechadorCuerpo1; break;
      case 5: persUnidad.GetComponent<Unidad>().uImage.sprite = ContenedorPrefabsBatalla.CanalizadorCuerpo1; break;

        //-----
    }



    CampaignManager.Instance.scMenuPersonajes.scEquipo.ActualizarEquipo(pers);
    Equipo sEquipo = CampaignManager.Instance.scMenuPersonajes.scEquipo;
    sEquipo.ActualizarEquipo(pers); //Actualiza el equipo del personaje, para que tenga los buffs de la campaña

    int indexRetrato = pers.idRetrato;
    string stNombre = pers.sNombre;
    float maxHP = pers.fVidaMaxima + sEquipo.BuffTOTALEQUIPOhpMax;
    float fIniciativa = pers.iIniciativa + sEquipo.BuffTOTALEQUIPOIniciativa;
    float fApMax = pers.iApMax + sEquipo.BuffTOTALEQUIPOApMax;
    float fValMax = pers.iValMax + sEquipo.BuffTOTALEQUIPOValMax;
    float fFuerza = pers.iFuerza + sEquipo.BuffTOTALEQUIPOFuerza;
    float fAgilidad = pers.iAgi + sEquipo.BuffTOTALEQUIPOAgi;
    float fPoder = pers.iPoder + sEquipo.BuffTOTALEQUIPOPoder;
    float fArmadura = pers.iArmadura + sEquipo.BuffTOTALEQUIPOArmadura;
    float resNecro = pers.iResNecro + sEquipo.BuffTOTALEQUIPOResNecro;
    float ResDivino = pers.iResDivino + sEquipo.BuffTOTALEQUIPOResNecro;
    float resArcano = pers.iResArcano + sEquipo.BuffTOTALEQUIPOResArcano;
    float resFuego = pers.iResFuego + sEquipo.BuffTOTALEQUIPOResFuego;
    float resHielo = pers.iResHielo + sEquipo.BuffTOTALEQUIPOResHielo;
    float resRayo = pers.iResRayo + sEquipo.BuffTOTALEQUIPOResRayo;
    float fDefensa = pers.iDefensa + sEquipo.BuffTOTALEQUIPODefensa;
    float fCritRango = pers.fCritRango;
    float fCritDanio = pers.fCritDanio;
    float fAtaque = pers.fBonusAtaque;
    float fTSReflejos = pers.iTSReflejo + sEquipo.BuffTOTALEQUIPOTSReflejo;
    float fTSFortaleza = pers.iTSFortaleza + sEquipo.BuffTOTALEQUIPOTSFortaleza;
    float fTSMental = pers.iTSMental + sEquipo.BuffTOTALEQUIPOTSMental;


    persUnidad.GetComponent<Unidad>().CrearUnidad(indexRetrato, stNombre, maxHP, fIniciativa, fApMax, fValMax, fFuerza, fAgilidad, fPoder, fArmadura, resNecro, resArcano, resFuego, resHielo, resRayo, fDefensa, fCritRango, fCritDanio, fAtaque, fTSReflejos, fTSFortaleza, fTSMental, ResDivino); //Asi se determinan atributos



    //Habilidades
    CopiarHabilidades(pers.gameObject, persUnidad);

    //Vida
    var scUnidad = persUnidad.GetComponent<Unidad>();
    scUnidad.HP_actual = pers.fVidaActual;
    // Sincroniza barra de vida de la unidad con la vida actual al inicio de la batalla
    scUnidad.ActualizarBarraVidaPropia();

    //Consumibles
    persUnidad.GetComponent<Unidad>().ConsumibleA = pers.Consumible1;
    persUnidad.GetComponent<Unidad>().ConsumibleB = pers.Consumible2;


    //Ubicación
    if (indexRefuezo == 0) //Si no es refuerzo, se coloca en una casilla aleatoria
    { ColocarEnCasillaAleatoriaEnColumna(1,pers.iPuestoDeseado, persUnidad); }

    //Estados-Buffs
    AplicarEstadosCampaña(pers, persUnidad);

    //Aplicar efectos de Items específicos
    AplicarEfectosItemsEspecificos(pers, persUnidad);

    if (indexRefuezo > 0) //Si es refuerzo, se coloca en lista
    {
      BattleManager.Instance.aliadosRefuerzos.Add(persUnidad);
    }

    if (Personaje1 == pers)
    {
      unidadPers1 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje2 == pers)
    {
      unidadPers2 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje3 == pers)
    {
      unidadPers3 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje4 == pers)
    {
      unidadPers4 = persUnidad.GetComponent<Unidad>();
    }





  }

  void CrearDefensasCaravana()
  {
    if (CampaignManager.Instance.mejoraCaravanaDefensas == 1)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();

      //Nido
      TrampaNidoDefensivo tr5 = BattleManager.Instance.ladoB.c1x3.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr5.Inicializar();


    }
    else if (CampaignManager.Instance.mejoraCaravanaDefensas == 2)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada2 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada2, 3, 3);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();
      TrampaPinchos tr4 = BattleManager.Instance.ladoA.c2x1.gameObject.AddComponent<TrampaPinchos>();
      tr4.Inicializar();
      TrampaPinchos tr5 = BattleManager.Instance.ladoA.c2x5.gameObject.AddComponent<TrampaPinchos>();
      tr5.Inicializar();

      //Nido
      TrampaNidoDefensivo tr6 = BattleManager.Instance.ladoB.c1x3.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr6.Inicializar();

    }
    else if (CampaignManager.Instance.mejoraCaravanaDefensas == 3)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada2 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada2, 3, 2);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 4);
      GameObject barricada4 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada4, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();
      TrampaPinchos tr4 = BattleManager.Instance.ladoA.c2x1.gameObject.AddComponent<TrampaPinchos>();
      tr4.Inicializar();
      TrampaPinchos tr5 = BattleManager.Instance.ladoA.c2x5.gameObject.AddComponent<TrampaPinchos>();
      tr5.Inicializar();
      TrampaPinchos tr6 = BattleManager.Instance.ladoA.c3x3.gameObject.AddComponent<TrampaPinchos>();
      tr6.Inicializar();


      //Nido
      TrampaNidoDefensivo tr7 = BattleManager.Instance.ladoB.c1x2.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr7.Inicializar();
      TrampaNidoDefensivo tr8 = BattleManager.Instance.ladoB.c1x4.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr8.Inicializar();

    }



  }

  public GameObject CanvasUnidades;
  void CrearEncuentroEnemigos(int IDEncuentro)  //Agregar en MenuBatallas "EfectosDeBatallaEnCampaña" las recompensas/penalizaciones por ganar/perder de cada encuentro nuevo
  {

    BattleManager.Instance.RondaNro = 1;
    ///FASE 1 - BOSQUE ARDIENDO
    ///
    #region
    if (IDEncuentro == 1) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 4; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();


    }
    if (IDEncuentro == 2) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);

    }
    if (IDEncuentro == 3) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 4) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 5) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 6) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 7) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 8) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboAlfaEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 9) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 10) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 2; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 11) // FASE 1 - BOSQUE ARDIENDO - Jefe - Arbol Lamentos
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.JefeArbolLamentos);
      ColocarEnCasillaEspecifica(2, enemigo1, 2, 3);

      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo2, 3, 1);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo3, 3, 2);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo4, 3, 3);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo5, 3, 4);
      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo6, 3, 5);
    }
    if (IDEncuentro == 12) // FASE 1 - BOSQUE ARDIENDO - Jefe - PENDIENTE
    {
      //PRENDIENTE
    }
    if (IDEncuentro == 13) // FASE 1 - BOSQUE ARDIENDO - Ataque caravana 1
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.LoboAlfaEspectral);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 14) // FASE 1 - BOSQUE ARDIENDO - Ataque caravana 2
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);


      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 2; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    #endregion

    #region //Batallas Subterraneas ID Entre 400 y 450
    if (IDEncuentro == 400) // FASE 1 - Batalla Subterranea  
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 401) // FASE 1 - Batalla Subterranea
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 402) // FASE 1 - Batalla Subterranea
    {

      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo2);

    }

    /*Vacio*/if (IDEncuentro == 403) // FASE 2 - Batalla Subterranea
    {

    }
    /*Vacio*/if (IDEncuentro == 404) // FASE 2 - Batalla Subterranea
    {

    }
    /*Vacio*/if (IDEncuentro == 405) // FASE 2 - Batalla Subterranea
    {

    }

    /*Vacio*/ if (IDEncuentro == 406) // FASE 3 - Batalla Subterranea
    {

    }
    /*Vacio*/ if (IDEncuentro == 407) // FASE 3 - Batalla Subterranea
    {

    }
    /*Vacio*/ if (IDEncuentro == 408) // FASE 3 - Batalla Subterranea
    {

    }
    #endregion

    #region //Batallas Bandidos ID Entre 500 y 550
    if (IDEncuentro == 500) // FASE 1 - Batalla Bandidos I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 501) // FASE 1 - Batalla Bandidos II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 502) // FASE 1 - Batalla Bandidos III
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 503) // FASE 1 - Batalla Bandidos IV
    {

      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 2; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 504) // FASE 1 - Batalla Bandidos - Ataque a Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 505) // FASE 1 - Batalla Bandidos - Ataque a Caravana II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);


      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    #endregion

    #region //Batallas Curruptas ID Entre 600 y 700
    if (IDEncuentro == 600) // FASE 1 - Batalla Currupta I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 601) // FASE 1 - Batalla Currupta II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 2; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
    }
    if (IDEncuentro == 602) // FASE 1 - Batalla Currupta III
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 603) // FASE 1 - Batalla Currupta IV
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 604) // FASE 1 - Batalla Currupta V
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 605) // FASE 1 - Batalla Currupta - Ataque a la Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 606) // FASE 1 - Batalla Currupta - Ataque a la Caravana II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3; //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    #endregion


   
    Invoke ("ColocarunidadesEnCanvasUnidades", 0.5f);
  }

 void ColocarunidadesEnCanvasUnidades()
  {
    GameObject[] unidades = GameObject.FindGameObjectsWithTag("Unidad");
    foreach (GameObject unidad in unidades)
    {
        unidad.transform.SetParent(CanvasUnidades.transform, true); // Mantiene la escala
       // unidad.transform.localPosition = Vector3.zero; // O la posición relativa correcta
       // unidad.transform.localScale = Vector3.one;     // Asegura escala normal
    }
  }
  void ColocarEnCasillaAleatoria(int iLado, GameObject GO)
  {

    LadoManager lado = null;
    if (iLado == 1)
    {
      lado = BattleManager.Instance.ladoB; //Player
    }
    else { lado = BattleManager.Instance.ladoA; } //Enemigos

    int intentos = 0; // Para limitar los intentos de colocar la unidad
    bool colocado = false;

    while (!colocado && intentos < 100) // Limitar los intentos para evitar bucles infinitos
    {
      int rX = UnityEngine.Random.Range(1, 4);  // Rango de x
      int rY = UnityEngine.Random.Range(1, 6);  // Rango de y

      if (lado.ColocarEnCasilla(GO, rX, rY))
      {
        colocado = true; // Si fue colocado con éxito, salir del bucle
      }

      intentos++;
    }

    if (!colocado)
    {
      Debug.LogError("No se pudo colocar el objeto en ninguna casilla después de 100 intentos.");
    }
  }

    void ColocarEnCasillaAleatoriaEnColumna(int iLado, int columna, GameObject GO)
    {
      LadoManager lado = null;
      if (iLado == 1)
      {
        lado = BattleManager.Instance.ladoB; //Player
      }
      else
      {
        lado = BattleManager.Instance.ladoA; //Enemigos
      }

      int intentos = 0; // Para limitar los intentos de colocar la unidad
      bool colocado = false;

      // columna es la Y (de 1 a 5 normalmente)
      while (!colocado && intentos < 100) // Limitar los intentos para evitar bucles infinitos
      {
        int rX = UnityEngine.Random.Range(1, 6);  // Rango de x (filas) de 1 a 5 inclusive
        int rY = columna; // Usar la columna indicada

        if (lado.ColocarEnCasilla(GO, rY, rX))
        {
          colocado = true; // Si fue colocado con éxito, salir del bucle
        }

        intentos++;
      }

      if (!colocado)
      {
        Debug.LogError("No se pudo colocar el objeto en la columna " + columna + " después de 100 intentos.");
      }
    }

  void ColocarEnCasillaEspecifica(int iLado, GameObject GO, int X, int Y)
  {
    LadoManager lado = null;
    if (iLado == 1)
    {
      lado = BattleManager.Instance.ladoB; //Player
    }
    else { lado = BattleManager.Instance.ladoA; } //Enemigos

    lado.ColocarEnCasilla(GO, X, Y);

  }

  void AplicarEstadosCampaña(Personaje pers, GameObject GO)
  {
    Unidad unidad = GO.GetComponent<Unidad>();

    if (pers.Camp_Fatigado || pers.ActividadSeleccionada == 2) //FATIGADO
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Fatigado = new Buff();
      Fatigado.buffNombre = "Fatigado";
      Fatigado.boolfDebufftBuff = false;
      Fatigado.DuracionBuffRondas = -1;
      Fatigado.cantAPMax -= 1;
      Fatigado.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Fatigado, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Bendecido_SequitoClerigos) //BENDECIDO 
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Bendecido = new Buff();
      Bendecido.buffNombre = "Bendecido por Plegaria";
      Bendecido.boolfDebufftBuff = true;
      Bendecido.cantTsMental += 2;
      Bendecido.cantAtaque += 1;
      Bendecido.cantDefensa += 1;
      Bendecido.cantResNec += 5;
      Bendecido.DuracionBuffRondas = -1;
      Bendecido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Bendecido, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Herido) //Herido
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Herido";
      Herido.boolfDebufftBuff = false;
      Herido.DuracionBuffRondas = -1;
      Herido.cantAtFue -= 1;
      Herido.cantAtAgi -= 1;
      Herido.cantAtPod -= 1;
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Enfermo > 0) //Enfermo
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Enfermo = new Buff();
      Enfermo.buffNombre = "Enfermo";
      Enfermo.boolfDebufftBuff = false;
      Enfermo.DuracionBuffRondas = -1;
      Enfermo.cantTsFortaleza -= 3;
      Enfermo.cantDanioPorcentaje -= 15;
      Enfermo.cantAPMax -= 1;
      Enfermo.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Enfermo, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Moral < 0) //Baja Moral
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Bajamoral = new Buff();
      Bajamoral.buffNombre = "Baja Moral";
      Bajamoral.boolfDebufftBuff = false;
      Bajamoral.DuracionBuffRondas = -1;
      Bajamoral.cantTsMental -= 3;
      Bajamoral.cantDefensa -= 1;
      Bajamoral.cantAtaque -= 1;
      unidad.ValentiaP_actual -= 2;
      Bajamoral.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Bajamoral, GO.gameObject);
      //--------------------------------------

    }
     if (pers.Camp_Moral > 0) //Alta Moral
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff AltaMoral = new Buff();
      AltaMoral.buffNombre = "Alta Moral";
      AltaMoral.boolfDebufftBuff = true;
      AltaMoral.DuracionBuffRondas = -1;
      AltaMoral.cantTsMental += 2;
      AltaMoral.cantAtaque += 1;
      unidad.ValentiaP_actual += 2;
      AltaMoral.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(AltaMoral, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Corrupto) //Corrupto
    {
      unidad.estado_Corrupto = true;

    }
    if (pers.ActividadSeleccionada == 5) //Caballero: Mantener Armadura
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Armadura Cuidada";
      Herido.boolfDebufftBuff = true;
      Herido.DuracionBuffRondas = -1;
      Herido.cantArmadura += 3;
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------
    }
    if (pers.ActividadSeleccionada == 1) //Base: Descansar
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Fresco";
      Herido.boolfDebufftBuff = true;
      Herido.DuracionBuffRondas = 2;
      Herido.cantIniciativa += 3;
      Herido.cantAPMax += 1;
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------
    }
    if (pers.ActividadSeleccionada == 8) //Explorador: Preparar Flechas
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Flechas Preparadas";
      Herido.boolfDebufftBuff = true;
      Herido.cantDanioPorcentaje += 10;
      unidad.GetComponent<ClaseExplorador>().PrepararFlechasDelay();
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------
    }
    if (pers.ActividadSeleccionada == 9) //Explorador: Exploración
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Fatigado = new Buff();
      Fatigado.buffNombre = "Fatigado por Explorar";
      Fatigado.boolfDebufftBuff = false;
      Fatigado.DuracionBuffRondas = -1;
      Fatigado.cantAPMax -= 1;
      Fatigado.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Fatigado, GO.gameObject);
      //--------------------------------------
    }
    if (pers.ActividadSeleccionada == 13) //Acechador: Afilar Armas
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Arma Afilada";
      Herido.boolfDebufftBuff = true;
      Herido.DuracionBuffRondas = -1;
      Herido.cantDanioPorcentaje += 10;
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------
    }
    if (pers.ActividadSeleccionada == 11) //Purificadora: Ayudar a los Desamparados
    {
      ClasePurificadora clasePurificadora = unidad.GetComponent<ClasePurificadora>();
      clasePurificadora.CambiarFervor(1); //Aumenta fervor

    }
    if (pers.ActividadSeleccionada == 16) //Canalizador: Concentración Arcana
    {
      if (unidad is ClaseCanalizador cana)
      {
        cana.CambiarEnergia(1);
      }
    }

    //Aplicar efectos de items equipados
    if (pers.itemArma != null)  //ARMA
    {
      if (pers.itemArma.nivelMejora > 0)   //Cada +1 da +1Ataque y +5% daño.
      { 
        Buff buffArma = new Buff();
        buffArma.buffNombre = "Arma";
        buffArma.boolfDebufftBuff = true;
        buffArma.cantAtaque += pers.itemArma.nivelMejora; //+1 Ataque por cada nivel de mejora
        buffArma.cantDanioPorcentaje += 5* pers.itemArma.nivelMejora; //5% por cada nivel de mejora
        buffArma.esBuffVisibleUI = false; //No se muestra en la UI
        buffArma.DuracionBuffRondas = -1; //Duración indefinida
        buffArma.AplicarBuff(unidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buffArma, GO.gameObject);
      }
    }
   /* if (pers.itemArmadura != null)  //EN ARMADURAS SE PUEDE CAMBIAR DIRECTAMENTE EN EL ITEM, MEJOR ASI SE MUESTRA EN LA UI
    ------------- NO SE APLICA ARMADURA POR MEJORA
    {
      if (pers.itemArmadura.nivelMejora > 0)   //Cada +1 da +2 Armadura.
      {
        Buff buffArmadura = new Buff();
        buffArmadura.buffNombre = "Armadura";
        buffArmadura.boolfDebufftBuff = true;
        buffArmadura.cantArmadura += 2*pers.itemArmadura.nivelMejora; // +2 Armadura por nivel de mejora
        buffArmadura.esBuffVisibleUI = false; //No se muestra en la UI
        buffArmadura.DuracionBuffRondas = -1; //Duración indefinida
        buffArmadura.AplicarBuff(unidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buffArmadura, GO.gameObject);
      }
    }*/









    //---
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);

  }
  void AplicarEfectosItemsEspecificos(Personaje pers, GameObject GO)
  {
     Unidad unidad = GO.GetComponent<Unidad>();

    //Armaduras
    if (pers.itemArmadura != null)
    {
      //Armadura de Cuero Reforzado de Ligereza
      if (pers.itemArmadura.sNombreItem == "Armadura de Cuero Reforzado de Ligereza +1")
      {
        unidad.estado_evasion += 2; // Da +2 de evasión al comenzar el combate
      }
      //Armadura de Cuero Reforzado de Velo
      if (pers.itemArmadura.sNombreItem == "Armadura de Cuero Reforzado de Velo +2" && unidad is ClaseAcechador unidadAcechador)
      {
        unidadAcechador.tieneArmaduradeVelo = true; // Activa la armadura de velo
      }
      //Armadura de Coraza de Llamas +1
      if (pers.itemArmadura.sNombreItem == "Armadura de Coraza de Llamas +1" && unidad is ClaseCaballero unidadCaballero)
      {
        unidadCaballero.tieneCorazaDeLlamas = true; // Activa la armadura de coraza de llamas
      }
      if (pers.itemArmadura.sNombreItem == "Armadura de Cuero Borrosa +2")
      {
        unidad.estado_evasion += 3; // Da +3 de evasión al comenzar el combate
      }
    }

  }


  void CopiarHabilidades(GameObject pers, GameObject persUnidad)
  {
    Habilidad[] habilidades = pers.GetComponents<Habilidad>();


    foreach (Habilidad habilidad in habilidades)
    {

      if (habilidad.GetType().Name.Contains("REPRESENTACION")) //Si es una pasiva
      {
        //Aca se le dice a la Unidad creada qué pasivas tiene el personaje, y de que nivel
        //------------------------------------------------------------------
        //Pasivas de CABALLERO 
        if (persUnidad.GetComponent<ClaseCaballero>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONArmaduraLimitante"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONCorajeInquebrantable"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcorazado"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Acorazado = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONDeterminacion"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Determinacion = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONImplacable"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Implacable = habilidad.NIVEL;
          }
          else
          {
            print("ATENCIÓN: " + pers.name + " tiene una pasiva (" + habilidad.GetType().Name + ") que no ha sido agregada al control (arriba en codigo), revisar.");
          }
        }
        if (persUnidad.GetComponent<ClaseExplorador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONPasoCauteloso"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONVistaLejana"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_VistaLejana = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcrobatico"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_Acrobatico = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONReconocimiento"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_Reconocimiento = habilidad.NIVEL;
          }


        }
        if (persUnidad.GetComponent<ClasePurificadora>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONFervorConjunto"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAuraSagrada"))
          {
            persUnidad.GetComponent<ClasePurificadora>().PASIVA_AuraSagrada = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONEcosDivinos"))
          {
            persUnidad.GetComponent<ClasePurificadora>().PASIVA_EcosDivinos = habilidad.NIVEL;
          }
        }
        if (persUnidad.GetComponent<ClaseAcechador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONSigiloso"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMaestriaBallesta"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_MaestriaConBallestaMano = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMaestriaEspadaCorta"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_MaestriaConEspadacorta = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMasacre"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_Masacre = habilidad.NIVEL;
          }

        }
        if (persUnidad.GetComponent<ClaseCanalizador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONSobrecarga"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcumulacionProtegida"))
          {
            persUnidad.GetComponent<ClaseCanalizador>().PASIVA_AcumulacionProtegida = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONExcesoDePoder"))
          {
            persUnidad.GetComponent<ClaseCanalizador>().PASIVA_ExcesoDePoder = habilidad.NIVEL;
          }

        }
      }

      // Obtener el tipo de la habilidad
      System.Type tipoHabilidad = habilidad.GetType();

      // Agregar un nuevo componente del mismo tipo al objeto persUnidad
      Habilidad nuevaHabilidad = (Habilidad)persUnidad.AddComponent(tipoHabilidad);


      nuevaHabilidad.NIVEL = habilidad.NIVEL;
      nuevaHabilidad.ActualizarNivel(); //Esto es para que se llame awake de nuevo, luego de establecer el nivel, asi se aplican los efectos del nivel

    }
  }



  public async void FinDeBatalla(int resultado)
  {



    PlasmarEfectosBatallaEnPersonajes();
    CampaignManager.Instance.logDeCampania.LimpiarDesdeBatalla();
    CampaignManager.Instance.scMenuBatallas.EfectosDeBatallaEnCampaña(resultado);
    CampaignManager.Instance.scMenuBatallas.DejanEnListaParticipantesSolo();
    CampaignManager.Instance.scMapaManager.nodoActual.nodoDespejado = true;

    //Limpieza
    Personaje1 = null;
    Personaje2 = null;
    Personaje3 = null;
    Personaje4 = null;

    BattleManager.Instance.aliadosRefuerzos.Clear();
    BattleManager.Instance.enemigosRefuerzos.Clear();

    if (unidadPers1 != null)
    { Destroy(unidadPers1); }
    if (unidadPers2 != null)
    { Destroy(unidadPers2); }
    if (unidadPers3 != null)
    { Destroy(unidadPers3); }
    if (unidadPers4 != null)
    { Destroy(unidadPers4); }

    EliminarTodasLasUnidades();
    EliminarTodasLasTrampas();

    BattleManager.Instance.RondaNro = 1;
    BattleManager.Instance.unidadActiva = null;
    BattleManager.Instance.indexTurno = 0;

    CampaignManager.Instance.scAdministradorEscenas.PlayFadeInOut(0.7f, 1.6f);
    await Task.Delay(TimeSpan.FromSeconds(1.6f));

    EscenaCampaign.SetActive(true);
    MusicManager.Instance.VolverACampania();
    escenaActual = 0;
    EscenaBatalla.SetActive(false);





  }

  public void EliminarTodasLasUnidades()
  {
    // Buscar todos los objetos con el tag "Unidad"
    GameObject[] unidades = GameObject.FindGameObjectsWithTag("Unidad");

    // Recorrer el array y destruir cada objeto
    foreach (GameObject unidad in unidades)
    {
      Destroy(unidad);
    }
  }
  public void EliminarTodasLasTrampas()
  {
    
    // Elimina todos los componentes de tipo Trampa en cada casilla de ambos lados
    foreach (var casilla in BattleManager.Instance.ladoA.casillasLado)
    {
      var trampas = casilla.GetComponents<Trampa>();
      foreach (var trampa in trampas)
      {
        trampa.DestruirTrampa();
      }
    }
     foreach (var casilla in BattleManager.Instance.ladoA.casillasLado)
    {
      var trampas = casilla.GetComponents<Trampa>();
      foreach (var trampa in trampas)
      {
         trampa.DestruirTrampa();
      }
    }

  
  }
  void PlasmarEfectosBatallaEnPersonajes()
  {
    int longitudBatallaFatiga = 7;


    if (Personaje1 != null)
    {
      if (unidadPers1.HP_actual < Personaje1.fVidaActual)
      { Personaje1.fVidaActual = unidadPers1.HP_actual; } //No puede terminar con mas vida de la que empezó

      CanalizadorPasivaSobrecarga(Personaje1, unidadPers1);

      if (BattleManager.Instance.RondaNro > longitudBatallaFatiga) { Personaje1.Camp_Fatigado = true; } //Batalla de mas de 7 turnos, fatiga.
    }
    if (Personaje2 != null)
    {
      if (unidadPers2.HP_actual < Personaje2.fVidaActual)
      { Personaje2.fVidaActual = unidadPers2.HP_actual; } //No puede terminar con mas vida de la que empezó

      CanalizadorPasivaSobrecarga(Personaje2, unidadPers2);


      if (BattleManager.Instance.RondaNro > longitudBatallaFatiga) { Personaje2.Camp_Fatigado = true; } //Batalla de mas de 7 turnos, fatiga.
    }
    if (Personaje3 != null)
    {
      if (unidadPers3.HP_actual < Personaje3.fVidaActual)
      { Personaje3.fVidaActual = unidadPers3.HP_actual; } //No puede terminar con mas vida de la que empezó

      CanalizadorPasivaSobrecarga(Personaje3, unidadPers3);


      if (BattleManager.Instance.RondaNro > longitudBatallaFatiga) { Personaje3.Camp_Fatigado = true; } //Batalla de mas de 7 turnos, fatiga.
    }
    if (Personaje4 != null)
    {
      if (unidadPers4.HP_actual < Personaje4.fVidaActual)
      { Personaje4.fVidaActual = unidadPers4.HP_actual; } //No puede terminar con mas vida de la que empezó

      CanalizadorPasivaSobrecarga(Personaje4, unidadPers4);

      if (BattleManager.Instance.RondaNro > longitudBatallaFatiga) { Personaje4.Camp_Fatigado = true; } //Batalla de mas de 7 turnos, fatiga.
    }

  // Plasmar efectos de los refuerzos aliados (no milicianos) 
  //EN TEORIA ESTO NO ES NECESARIO YA QUE SI SE PIERDEN BATALLAS DE REFUERZOS SE PIERDE EL JUEGO
  List<Unidad> aliadosQueEntraron = new List<Unidad>();
  foreach (Unidad unidadRefuerzo in BattleManager.Instance.ladoB.unidadesLado)
  {
      if (unidadRefuerzo == null) continue;
      if (unidadRefuerzo.entroComoAliado)
      {
        aliadosQueEntraron.Add(unidadRefuerzo);
      }
    
     
  }
  foreach (Unidad unidadRefuerzo in aliadosQueEntraron)
  {
    
    // Buscar el personaje correspondiente al refuerzo (por nombre)
    bool encontrado = false;
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.sNombre == unidadRefuerzo.uNombre)
      {
        // Plasmar vida actual (no puede terminar con más vida de la que empezó)
        if (unidadRefuerzo.HP_actual < pers.fVidaActual)
        {
          pers.fVidaActual = unidadRefuerzo.HP_actual;
        }

        CanalizadorPasivaSobrecarga(pers, unidadRefuerzo);
        CampaignManager.Instance.scMenuBatallas.AdministrarHeridas(pers,unidadRefuerzo);

        if (BattleManager.Instance.RondaNro > longitudBatallaFatiga)
          {
            pers.Camp_Fatigado = true;
          }
        encontrado = true;
        break;
      }
    }
  }


  }

  void CanalizadorPasivaSobrecarga(Personaje pers, Unidad unidad)
  {
    if (unidad is ClaseCanalizador scCanalizador &&  pers.fVidaActual > 0)
    {

      float maxHP = pers.fVidaMaxima;
      float damPorEnergia = maxHP * 0.15f;
      int nEnergiaCombate = scCanalizador.ObtenerEnergia();

      pers.fVidaActual -= damPorEnergia * nEnergiaCombate;
      if (pers.fVidaActual < 1) { pers.fVidaActual = 1; }
      pers.fVidaMaxima -= 1 * nEnergiaCombate;

 //     print($"Al tener {nEnergiaCombate} Energia, {scCanalizador.uNombre} perdió {damPorEnergia * nEnergiaCombate} vida y {1 * nEnergiaCombate} mas hp.");

    }



  }
  
   void AsignarRefuerzosAliados()
    { 
        AdministradorEscenas scAdminEscenas = CampaignManager.Instance.scMenuBatallas.scAdministradorEscenas;

        int cantRefuerzoAliadoHeroe = 0;
        foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
        {
            if (!pers.Camp_Muerto && pers.fVidaActual > 1) //Si no está muerto y tiene vida
            {
                if (!(/*Base: Guardia*/pers.ActividadSeleccionada == 3) && !(/*Caballero: Vigilar*/pers.ActividadSeleccionada == 6))
                {   cantRefuerzoAliadoHeroe++;
                    //Si no está haciendo guardia, lo agrega como refuerzo, pero no lo muestra
                    scAdminEscenas.ColocarPersonajecomoUnidad(pers, cantRefuerzoAliadoHeroe);
                }
            }

        }
           //Agrega milicianos como refuerzos
            int cantidadMilicianos = (int)CampaignManager.Instance.GetMiliciasActual() / 10; //Cada 10 milicianos, uno se une como refuerzo
           if (cantidadMilicianos > 0)
          {
              bool tieneDesertores = CampaignManager.Instance.scMenuSequito.TieneSequito(6);
              var prefabs = scAdminEscenas.ContenedorPrefabsBatalla;

              for (int i = 0; i < cantidadMilicianos; i++)
              {
                  GameObject aliado;

                  if (tieneDesertores)
                      aliado = Instantiate(i % 2 == 0 ? prefabs.Desertor2 : prefabs.Desertor1);
                  else
                      aliado = Instantiate(i % 2 == 0 ? prefabs.Miliciano2 : prefabs.Miliciano1);

                  aliado.SetActive(false);
                  BattleManager.Instance.aliadosRefuerzos.Add(aliado);
              }
          }

      // Da vuelta la lista de refuerzos aliados
      BattleManager.Instance.aliadosRefuerzos.Reverse();

    }


}
