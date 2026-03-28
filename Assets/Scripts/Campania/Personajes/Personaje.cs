using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour
{
    [SerializeField] private string persistentId;
    private const float PorcentajeVidaPorPuntoFuerza = 0.03f;
    public int iDefensaBaseSinAgilidad;
    public int iAgiReferenciaDefensa;
    public bool escalaDefensaInicializada;
    public int iPoderReferenciaResElemental;
    public bool escalaResElementalPorPoderInicializada;
   
    public float fVidaActual;
    public float fVidaMaxima;
    public string sNombre;

    public int IDClase; //1: Caballero - 2: Explorador - 3: Purificadora  -  4: Acechador  -  5: Canalizador  - 6: Duelista

    public float fExperienciaActual;
    public float fNivelActual;

    public int iPuestoDeseado = 1; //1: Primera Columna - 2: Segunda Columna - 3: Tercera Columna

    public Sprite spRetrato;
    public int idRetrato;
   
    public int iFuerza;
    public int iAgi;
    public int iPoder;
    public int iIniciativa;
    public int iApMax;
    public int iValMax;
    public int iArmadura;
    public int iDefensa;
    public int iTSReflejo;
    public int iTSFortaleza;
    public int iTSMental;
    public int iResFuego;
    public int iResRayo;
    public int iResHielo;
    public int iResArcano;
    public int iResAcido;
    public int iResNecro;
    public int iResDivino;
    public float fCritRango;
    public float fCritDanio;
    public float fBonusAtaque;

    //Habilidades - Aca se aclara que Habilidades de la clase tiene 0 no 1 si NO EL NIVEL
    //(ver REPRESENTACIONCorajeInquebrantalbe, para que las PASIVAS aparezcan en lista habilidades)
    public int Habilidad_1;
    public int Habilidad_2;
    public int Habilidad_3;
    public int Habilidad_4;
    public int Habilidad_5;
    public int Habilidad_6;
    public int Habilidad_7;
    public int Habilidad_8;
    public int Habilidad_9;
    public int Habilidad_10;

    public int Actividad_1;
    public int Actividad_2;
    public int Actividad_3; 
    public int ActividadSeleccionada;

    public int NivelPuntoAtributo;
    public int NivelPuntoTS;
    public int NivelPuntoHabilidad;
    public int NivelNuevaHabilidadBase;
   

    //Inventario
    public Arma itemArma;
    public Armadura itemArmadura;
    public Accesorio Accesorio1;
    public Accesorio Accesorio2;
    public Consumible Consumible1;
    public Consumible Consumible2;


    //Estados de Campaña AGREGAR EN MenuPersonaje  "//Estados Campaña" para que se vean
    public bool Camp_Fatigado;
    public bool Camp_Bendecido_SequitoClerigos;
    public bool Camp_Herido;
    public int Camp_Enfermo; //es int porque al descender a 0 se va, -1 por viaje.
    public int Camp_Moral; //positiva buena, negativa mala tiende a cero cada dia
    public bool Camp_Avergonzado; //Se limpia al cambiar de zona.

    public bool Camp_Muerto;
    public bool Camp_Corrupto;
   


  
    public int[] aRasgos = new int[300];

  void Start()
  {
    if (ActividadSeleccionada == 0)
    {
      ActividadSeleccionada = 3; //Guardia
    }
  }

    public string GetPersistentId()
    {
        return persistentId;
    }

    public string EnsurePersistentId()
    {
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            persistentId = System.Guid.NewGuid().ToString("N");
        }

        return persistentId;
    }

    public void SetPersistentId(string id)
    {
        persistentId = id;
    }

    public void QuitarArma(Arma iArma)
  {
    // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] scripts = GetComponents<Habilidad>();

    // Recorre todos los scripts
    foreach (Habilidad script in scripts)
    {
      // Si la ahbilidad la agregó el arma que saca, quita la habilidad
      if (script.agregaDesdeArmaUI == iArma)
      {
        Destroy(script);
      }
    }
    itemArma = null;
  }

    public void QuitarArmadura(Armadura iArma)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

    // Recorre todos los scripts
    foreach (Habilidad script in hab)
    {
        // Si la ahbilidad la agregó el arma que saca, quita la habilidad
        if (script.agregaDesdeArmaUI== iArma)
        {
            Destroy(script);
        }
    }
        itemArmadura = null;
    }

    public void QuitarAccesorio1(Accesorio iAccesorio)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iAccesorio)
            {
                Destroy(script);
            }
        }
            Accesorio1 = null;
    }

    public void QuitarAccesorio2(Accesorio iAccesorio)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iAccesorio)
            {
                Destroy(script);
            }
        }
            Accesorio2 = null;
    }

    public void QuitarConsumible1(Consumible iCons)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iCons)
            {
                Destroy(script);
            }
        }
            Consumible1 = null;
    }
    public void QuitarConsumible2(Consumible iCons)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iCons)
            {
                Destroy(script);
            }
        }
            Consumible2 = null;
    }
    
    public void RecibirCuracion(float cant)
    {
        if(Camp_Herido)//Si esta herido, se cura la mitad
        { 
          cant = cant /2;
        }
        
        fVidaActual += cant;
        if(fVidaActual >= fVidaMaxima)
        {
            fVidaActual = fVidaMaxima;
            Camp_Herido = false; //si se sana del todo, se remueve la herida
        }
    }

    public float ObtenerVidaMaximaConFuerza(float bonusVidaMax = 0f, float bonusFuerza = 0f)
    {
      float vidaBase = Mathf.Max(1f, fVidaMaxima + bonusVidaMax);
      float fuerzaTotal = iFuerza + bonusFuerza;
      float bonusVida = vidaBase * PorcentajeVidaPorPuntoFuerza * fuerzaTotal;
      return Mathf.Max(1f, vidaBase + bonusVida);
    }

    public float ObtenerVidaActualConFuerza(float bonusVidaMax = 0f, float bonusFuerza = 0f)
    {
      float vidaMaxEscalada = ObtenerVidaMaximaConFuerza(bonusVidaMax, bonusFuerza);
      float vidaBase = Mathf.Max(1f, fVidaMaxima + bonusVidaMax);
      float bonusVida = vidaBase * PorcentajeVidaPorPuntoFuerza * (iFuerza + bonusFuerza);
      return Mathf.Clamp(fVidaActual + bonusVida, 0f, vidaMaxEscalada);
    }

    public void InicializarEscaladoDefensaPorAgilidadSiHaceFalta()
    {
      if (escalaDefensaInicializada)
      {
        return;
      }

      iAgiReferenciaDefensa = iAgi;
      iDefensaBaseSinAgilidad = iDefensa - iAgiReferenciaDefensa;
      escalaDefensaInicializada = true;
    }

    public float ObtenerDefensaTotalConAgilidad(float bonusDefensa = 0f, float bonusAgilidad = 0f)
    {
      InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
      return iDefensaBaseSinAgilidad + bonusDefensa + iAgi + bonusAgilidad;
    }

    public void InicializarEscaladoResElementalPorPoderSiHaceFalta()
    {
      if (escalaResElementalPorPoderInicializada)
      {
        return;
      }

      iPoderReferenciaResElemental = iPoder;
      escalaResElementalPorPoderInicializada = true;
    }

    public int ObtenerBonusResElementalPorPoder(float bonusPoder = 0f)
    {
      InicializarEscaladoResElementalPorPoderSiHaceFalta();
      float poderTotal = iPoder + bonusPoder;
      return Mathf.RoundToInt(poderTotal - iPoderReferenciaResElemental);
    }

    public int ObtenerResElementalConPoder(int resBase, float bonusRes = 0f, float bonusPoder = 0f)
    {
      return Mathf.RoundToInt(resBase + bonusRes + ObtenerBonusResElementalPorPoder(bonusPoder));
    }

    public float ObtenerExperienciaNecesariaParaProximoNivel()
    {
      return 100f + (fNivelActual * 50f);
    }

    public void RecibirExperiencia(float cant)
    {
      if (cant <= 0f)
      {
        return;
      }

      if (CampaignManager.Instance != null)
      {
        cant = CampaignManager.Instance.AplicarMultiplicadorExperienciaEstadosCaravana(cant);
      }

      fExperienciaActual += cant;

      while (fExperienciaActual >= ObtenerExperienciaNecesariaParaProximoNivel())
      {
        float experienciaNecesaria = ObtenerExperienciaNecesariaParaProximoNivel();
        fExperienciaActual -= experienciaNecesaria;
        fNivelActual++;

        float vidaAntesDeSubir = fVidaMaxima;
        AplicarRecompensasPorSubidaDeNivel();
        EscribirLogSubidaDeNivel(vidaAntesDeSubir);
      }

      NormalizarPuntosPendientesPorNivelActual();
    }

    public void NormalizarPuntosPendientesPorNivelActual()
    {
      int maxPuntosAtributo = 0;
      int maxPuntosTS = 0;
      int maxPuntosHabilidad = 0;
      int maxNuevasHabilidadesBase = 0;
      int nivelActualEntero = Mathf.Max(1, Mathf.RoundToInt(fNivelActual));

      for (int nivel = 2; nivel <= nivelActualEntero; nivel++)
      {
        switch (nivel)
        {
          case 2:
            maxPuntosHabilidad += 1;
            maxPuntosAtributo += 1;
            break;
          case 3:
            maxPuntosHabilidad += 1;
            maxNuevasHabilidadesBase += 1;
            break;
          case 4:
            maxPuntosHabilidad += 2;
            maxPuntosTS += 1;
            break;
          case 5:
            maxPuntosHabilidad += 1;
            maxPuntosAtributo += 1;
            break;
          case 6:
            maxPuntosHabilidad += 1;
            maxNuevasHabilidadesBase += 1;
            break;
          case 7:
            maxPuntosHabilidad += 1;
            break;
          case 8:
            maxPuntosHabilidad += 2;
            maxPuntosTS += 1;
            break;
          case 9:
            maxPuntosHabilidad += 1;
            maxPuntosAtributo += 1;
            break;
          case 10:
            maxPuntosHabilidad += 1;
            break;
          case 11:
            maxPuntosHabilidad += 1;
            maxPuntosTS += 1;
            break;
          case 12:
            maxPuntosHabilidad += 2;
            maxPuntosAtributo += 1;
            break;
          case 13:
            maxPuntosHabilidad += 1;
            break;
        }
      }

      NivelPuntoAtributo = Mathf.Clamp(NivelPuntoAtributo, 0, maxPuntosAtributo);
      NivelPuntoTS = Mathf.Clamp(NivelPuntoTS, 0, maxPuntosTS);
      NivelPuntoHabilidad = Mathf.Clamp(NivelPuntoHabilidad, 0, maxPuntosHabilidad);
      NivelNuevaHabilidadBase = Mathf.Clamp(NivelNuevaHabilidadBase, 0, maxNuevasHabilidadesBase);
    }

    private void AplicarRecompensasPorSubidaDeNivel()
    {
      switch (Mathf.RoundToInt(fNivelActual))
      {
        case 2:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad++;
          NivelPuntoAtributo++;
          break;
        case 3:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad++;
          NivelNuevaHabilidadBase++;
          iApMax++;
          break;
        case 4:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 2;
          NivelPuntoTS++;
          break;
        case 5:
          OtorgarVidaExtraPorDivisor(15f);
          NivelPuntoHabilidad += 1;
          NivelPuntoAtributo++;
          break;
        case 6:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 1;
          NivelNuevaHabilidadBase++;
          break;
        case 7:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 1;
          iApMax++;
          break;
        case 8:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 2;
          NivelPuntoTS++;
          break;
        case 9:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 1;
          NivelPuntoAtributo++;
          break;
        case 10:
          OtorgarVidaExtraPorDivisor(15f);
          NivelPuntoHabilidad += 1;
          AgregarHabilidadDefinitivaAleatoria();
          break;
        case 11:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 1;
          NivelPuntoTS++;
          break;
        case 12:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 2;
          NivelPuntoAtributo++;
          break;
        case 13:
          OtorgarVidaExtraPorDivisor(10f);
          NivelPuntoHabilidad += 1;
          break;
      }
    }

    private void OtorgarVidaExtraPorDivisor(float divisor)
    {
      if (divisor <= 0f)
      {
        return;
      }

      float vidaGanada = fVidaMaxima / divisor;
      fVidaMaxima += vidaGanada;
      fVidaActual += vidaGanada;
    }

    private void EscribirLogSubidaDeNivel(float vidaAntesDeSubir)
    {
      CampaignManager campaignManager = CampaignManager.Instance;
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      string nvMensaje = ConstruirMensajeSubidaDeNivel(vidaAntesDeSubir, esIngles);

      if (campaignManager != null && !string.IsNullOrWhiteSpace(nvMensaje))
      {
        campaignManager.EscribirLog("<Color=#F0CC39><b>" + nvMensaje + "</b></color>");
      }
    }

    private string ConstruirMensajeSubidaDeNivel(float vidaAntesDeSubir, bool esIngles)
    {
      int nivelActualEntero = Mathf.RoundToInt(fNivelActual);
      int vidaGanada10 = Mathf.FloorToInt(vidaAntesDeSubir / 10f);
      int vidaGanada15 = Mathf.FloorToInt(vidaAntesDeSubir / 15f);

      if (!esIngles)
      {
        string mensaje = $"{sNombre} ha subido a Nivel {nivelActualEntero} y obtuvo: ";
        switch (nivelActualEntero)
        {
          case 2: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 Punto Atributo";
          case 3: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 Habilidad Nueva +1 AP Máximo";
          case 4: return mensaje + $"{vidaGanada10} Vida +2 Puntos de Habilidad +1 Punto Salvación";
          case 5: return mensaje + $"{vidaGanada15} Vida +1 Punto de Habilidad +1 Punto Atributo";
          case 6: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 Habilidad Nueva";
          case 7: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 AP Máximo";
          case 8: return mensaje + $"{vidaGanada10} Vida +2 Puntos de Habilidad +1 Punto Salvación";
          case 9: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 Punto Atributo";
          case 10: return mensaje + $"{vidaGanada15} Vida +1 Punto de Habilidad +1 Habilidad Definitiva";
          case 11: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad +1 Punto Salvación";
          case 12: return mensaje + $"{vidaGanada10} Vida +2 Puntos de Habilidad +1 Punto Atributo";
          case 13: return mensaje + $"{vidaGanada10} Vida +1 Punto de Habilidad";
          default: return string.Empty;
        }
      }

      string mensajeEn = $"{sNombre} is now level {nivelActualEntero} and obtained: ";
      switch (nivelActualEntero)
      {
        case 2: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 Attribute Point";
        case 3: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 New Skill +1 Max AP";
        case 4: return mensajeEn + $"{vidaGanada10} Health +2 Skill Points +1 Saving Throw Point";
        case 5: return mensajeEn + $"{vidaGanada15} Health +1 Skill Point +1 Attribute Point";
        case 6: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 New Skill";
        case 7: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 Max AP";
        case 8: return mensajeEn + $"{vidaGanada10} Health +2 Skill Points +1 Saving Throw Point";
        case 9: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 Attribute Point";
        case 10: return mensajeEn + $"{vidaGanada15} Health +1 Skill Point +1 Ultimate Skill";
        case 11: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point +1 Saving Throw Point";
        case 12: return mensajeEn + $"{vidaGanada10} Health +2 Skill Points +1 Attribute Point";
        case 13: return mensajeEn + $"{vidaGanada10} Health +1 Skill Point";
        default: return string.Empty;
      }
    }

    private void AgregarHabilidadDefinitivaAleatoria()
    {
      int rand = UnityEngine.Random.Range(1, 3);

      if (IDClase == 1)
      {
        if (rand == 1 && GetComponent<REPRESENTACIONImplacable>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<REPRESENTACIONImplacable>();
          habilidad.NIVEL = 1;
          return;
        }

        if (GetComponent<HombroConHombro>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<HombroConHombro>();
          habilidad.NIVEL = 1;
        }
        return;
      }

      if (IDClase == 2)
      {
        if (rand == 1 && GetComponent<REPRESENTACIONReconocimiento>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<REPRESENTACIONReconocimiento>();
          habilidad.NIVEL = 1;
          return;
        }

        if (GetComponent<Rafaga>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<Rafaga>();
          habilidad.NIVEL = 1;
        }
        return;
      }

      if (IDClase == 3)
      {
        if (rand == 1 && GetComponent<Purificacion>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<Purificacion>();
          habilidad.NIVEL = 1;
          return;
        }

        if (GetComponent<EscudodeFe>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<EscudodeFe>();
          habilidad.NIVEL = 1;
        }
        return;
      }

      if (IDClase == 4)
      {
        if (rand == 1 && GetComponent<HaciaLasSombras>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<HaciaLasSombras>();
          habilidad.NIVEL = 1;
          return;
        }

        if (GetComponent<REPRESENTACIONMasacre>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<REPRESENTACIONMasacre>();
          habilidad.NIVEL = 1;
        }
        return;
      }

      if (IDClase == 5)
      {
        if (rand == 1 && GetComponent<DescargaDesintegradora>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<DescargaDesintegradora>();
          habilidad.NIVEL = 1;
          return;
        }

        if (GetComponent<ManifestacionArcana>() == null)
        {
          Habilidad habilidad = gameObject.AddComponent<ManifestacionArcana>();
          habilidad.NIVEL = 1;
        }
      }
    }
}












