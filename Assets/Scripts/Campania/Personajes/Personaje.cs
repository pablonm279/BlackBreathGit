using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour
{
    public const int CantidadMaximaRasgos = 300;
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
    public int Camp_Bendecido;
    public bool Camp_Herido;
    public int Camp_Enfermo; //es int porque al descender a 0 se va, -1 por viaje.
    public int Camp_Moral; //positiva buena, negativa mala tiende a cero cada dia
    public bool Camp_Avergonzado; //Se limpia al cambiar de zona.

    public bool Camp_Muerto;
    public bool Camp_Corrupto;
    public bool TraitHeroeLocalCivilesOtorgados;
    public bool TraitHeroeLocalPenalidadMuerteAplicada;
    public bool TraitEjemploASeguirAplicado;
    public bool TraitHerenciaItemOtorgado;
    [System.NonSerialized] public bool TraitDuroDeMatarActivadoEnCombate;
    [System.NonSerialized] public bool TraitPuertasDeLaMuerteActivadoEnCombate;
    [System.NonSerialized] public bool TraitVengativoActivadoEnCombate;
    [System.NonSerialized] public bool TraitColaborativoUsadoEnCombate;
    [System.NonSerialized] public bool TraitImpulsivoCansadoAplicadoEnCombate;
    [System.NonSerialized] public bool TraitPacienteAplicadoEnCombate;


  
    public int[] aRasgos = new int[CantidadMaximaRasgos];

  void Start()
  {
    AsegurarCapacidadRasgos();
    if (ActividadSeleccionada == 0 && PuedeRealizarActividades())
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

    public void AsegurarCapacidadRasgos()
    {
        if (aRasgos != null && aRasgos.Length >= CantidadMaximaRasgos)
        {
            return;
        }

        int[] rasgosNuevos = new int[CantidadMaximaRasgos];
        if (aRasgos != null)
        {
            System.Array.Copy(aRasgos, rasgosNuevos, Mathf.Min(aRasgos.Length, rasgosNuevos.Length));
        }

        aRasgos = rasgosNuevos;
    }

    public void LimpiarRasgos()
    {
        AsegurarCapacidadRasgos();
        System.Array.Clear(aRasgos, 0, aRasgos.Length);
    }

    public bool TieneRasgo(int rasgoId)
    {
        AsegurarCapacidadRasgos();
        return rasgoId > 0 && rasgoId < aRasgos.Length && aRasgos[rasgoId] == 1;
    }

    public bool AgregarRasgo(int rasgoId)
    {
        if (rasgoId <= 0 || rasgoId >= CantidadMaximaRasgos)
        {
            return false;
        }

        AsegurarCapacidadRasgos();
        foreach (int rasgoActivo in EnumerarRasgosActivos())
        {
            if (!PersonajeTraitCatalog.SonCompatibles(rasgoId, rasgoActivo))
            {
                return false;
            }
        }

        aRasgos[rasgoId] = 1;
        return true;
    }

    public IEnumerable<int> EnumerarRasgosActivos()
    {
        AsegurarCapacidadRasgos();
        for (int i = 1; i < aRasgos.Length; i++)
        {
            if (aRasgos[i] == 1)
            {
                yield return i;
            }
        }
    }

    public void ResetearEstadoTraitsCombate()
    {
        TraitDuroDeMatarActivadoEnCombate = false;
        TraitPuertasDeLaMuerteActivadoEnCombate = false;
        TraitVengativoActivadoEnCombate = false;
        TraitColaborativoUsadoEnCombate = false;
        TraitImpulsivoCansadoAplicadoEnCombate = false;
        TraitPacienteAplicadoEnCombate = false;
    }

    public bool PuedeRealizarActividades()
    {
        return /*!Camp_Fatigado && */!TieneRasgo(PersonajeTraitCatalog.TraitHolgazan);
    }

    public void SetCampBendecido(int dias)
    {
        Camp_Bendecido = Mathf.Max(0, dias);
    }

    public void AgregarCampBendecido(int dias)
    {
        if (dias <= 0)
        {
            return;
        }

        Camp_Bendecido = Mathf.Max(Camp_Bendecido, dias);
    }

    public void ReducirCampBendecido(int dias = 1)
    {
        if (Camp_Bendecido <= 0 || dias <= 0)
        {
            return;
        }

        Camp_Bendecido = Mathf.Max(0, Camp_Bendecido - dias);
    }

    public bool TieneCampBendecido()
    {
        return Camp_Bendecido > 0;
    }

    public void SetCampFatigado(bool fatigado)
    {
        Camp_Fatigado = fatigado;

        if (fatigado)
        {
            ActividadSeleccionada = 1;
        }
    }

    public float AplicarMultiplicadorExperienciaTraits(float cantidadBase)
    {
        float cantidadAjustada = cantidadBase;

        if (TieneRasgo(PersonajeTraitCatalog.TraitInteligente))
        {
            cantidadAjustada *= 1.15f;
        }

        if (TieneRasgo(PersonajeTraitCatalog.TraitIngenuo))
        {
            cantidadAjustada *= 0.85f;
        }

        return cantidadAjustada;
    }

    public float AplicarMultiplicadorCuracionCampaniaTraits(float cantidadBase)
    {
        float cantidadAjustada = cantidadBase;

        if (TieneRasgo(PersonajeTraitCatalog.TraitSano))
        {
            cantidadAjustada *= 1.15f;
        }

        if (TieneRasgo(PersonajeTraitCatalog.TraitEnfermizo))
        {
            cantidadAjustada *= 0.85f;
        }

        return cantidadAjustada;
    }

    public void AplicarTraitExpertoInicial()
    {
        if (!TieneRasgo(PersonajeTraitCatalog.TraitExperto))
        {
            return;
        }

        List<Habilidad> habilidadesCandidatas = new List<Habilidad>();
        foreach (Habilidad habilidad in GetComponents<Habilidad>())
        {
            if (habilidad == null
                || habilidad.NIVEL <= 0
                || habilidad.agregaDesdeArmaUI != null)
            {
                continue;
            }

            habilidadesCandidatas.Add(habilidad);
        }

        if (habilidadesCandidatas.Count == 0)
        {
            return;
        }

        Habilidad habilidadElegida = habilidadesCandidatas[UnityEngine.Random.Range(0, habilidadesCandidatas.Count)];
        if (habilidadElegida != null)
        {
            habilidadElegida.NIVEL = Mathf.Max(habilidadElegida.NIVEL, 3);
        }
    }

    public int ObtenerTSFortalezaTotalCampania()
    {
        int total = iTSFortaleza;

        if (itemArma != null) total += itemArma.buffTSFortaleza;
        if (itemArmadura != null) total += itemArmadura.buffTSFortaleza;
        if (Accesorio1 != null) total += Accesorio1.buffTSFortaleza;
        if (Accesorio2 != null) total += Accesorio2.buffTSFortaleza;
        if (TieneCampBendecido()) total += 3;

        return total;
    }

    public bool FalloTiradaSalvacionFortalezaCampania(int dc)
    {
        int tirada = UnityEngine.Random.Range(1, 21);
        return dc > tirada + ObtenerTSFortalezaTotalCampania();
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

      cant = AplicarMultiplicadorExperienciaTraits(cant);

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
        return;
      }

      if (IDClase == 6)
      {
        bool tienePresenciaProvocadora = GetComponent<PresenciaProvocadora>() != null;
        bool tieneDanzaDelEstoque = GetComponent<REPRESENTACIONDanzaDelEstoque>() != null;

        if (!tienePresenciaProvocadora && !tieneDanzaDelEstoque)
        {
          if (rand == 1)
          {
            Habilidad habilidad = gameObject.AddComponent<PresenciaProvocadora>();
            habilidad.NIVEL = 1;
            Habilidad_9 = 1;
            return;
          }

          Habilidad habilidadDanza = gameObject.AddComponent<REPRESENTACIONDanzaDelEstoque>();
          habilidadDanza.NIVEL = 1;
          Habilidad_10 = 1;
          return;
        }

        if (!tienePresenciaProvocadora)
        {
          Habilidad habilidad = gameObject.AddComponent<PresenciaProvocadora>();
          habilidad.NIVEL = 1;
          Habilidad_9 = 1;
          return;
        }

        if (!tieneDanzaDelEstoque)
        {
          Habilidad habilidad = gameObject.AddComponent<REPRESENTACIONDanzaDelEstoque>();
          habilidad.NIVEL = 1;
          Habilidad_10 = 1;
        }
      }
    }
}

public sealed class PersonajeTraitDefinition
{
    public int Id { get; }
    public string Codigo { get; }
    public bool DisponibleAlCrear { get; }

    private readonly string nombreEs;
    private readonly string descripcionEs;
    private readonly string nombreEn;
    private readonly string descripcionEn;
    private readonly string nombrePtBr;
    private readonly string descripcionPtBr;
    private readonly HashSet<int> antagonicos;

    public PersonajeTraitDefinition(
        int id,
        string codigo,
        bool disponibleAlCrear,
        string nombreEs,
        string descripcionEs,
        string nombreEn,
        string descripcionEn,
        string nombrePtBr,
        string descripcionPtBr,
        params int[] antagonicos)
    {
        Id = id;
        Codigo = codigo;
        DisponibleAlCrear = disponibleAlCrear;
        this.nombreEs = nombreEs;
        this.descripcionEs = descripcionEs;
        this.nombreEn = nombreEn;
        this.descripcionEn = descripcionEn;
        this.nombrePtBr = nombrePtBr;
        this.descripcionPtBr = descripcionPtBr;
        this.antagonicos = antagonicos != null && antagonicos.Length > 0
            ? new HashSet<int>(antagonicos)
            : new HashSet<int>();
    }

    public string ObtenerNombre(int idioma)
    {
        return idioma switch
        {
            TRADU.IdiomaIngles => string.IsNullOrWhiteSpace(nombreEn) ? nombreEs : nombreEn,
            TRADU.IdiomaPortugues => string.IsNullOrWhiteSpace(nombrePtBr) ? nombreEs : nombrePtBr,
            _ => nombreEs
        };
    }

    public string ObtenerDescripcion(int idioma)
    {
        return idioma switch
        {
            TRADU.IdiomaIngles => string.IsNullOrWhiteSpace(descripcionEn) ? descripcionEs : descripcionEn,
            TRADU.IdiomaPortugues => string.IsNullOrWhiteSpace(descripcionPtBr) ? descripcionEs : descripcionPtBr,
            _ => descripcionEs
        };
    }

    public string ObtenerTextoCompleto(int idioma)
    {
        string nombre = ObtenerNombre(idioma);
        string descripcion = ObtenerDescripcion(idioma);
        return string.IsNullOrWhiteSpace(descripcion) ? nombre : nombre + ":       " + descripcion;
    }

    public bool EsAntagonicoCon(int traitId)
    {
        return antagonicos.Contains(traitId);
    }
}

public static class PersonajeTraitCatalog
{
    public const int TraitDulcesSuenos = 1;
    public const int TraitPesadillasRecurrentes = 2;
    public const int TraitValiente = 3;
    public const int TraitCobarde = 4;
    public const int TraitAvispado = 5;
    public const int TraitDistraido = 6;
    public const int TraitInteligente = 7;
    public const int TraitIngenuo = 8;
    public const int TraitIncansable = 9;
    public const int TraitFlojo = 10;
    public const int TraitOptimista = 11;
    public const int TraitPesimista = 12;
    public const int TraitCuidadoso = 13;
    public const int TraitRudioso = 14;
    public const int TraitResistente = 15;
    public const int TraitEndeble = 16;
    public const int TraitHombrosFirmes = 17;
    public const int TraitSano = 18;
    public const int TraitEnfermizo = 19;
    public const int TraitOdiaLaLluvia = 20;
    public const int TraitExperto = 21;
    public const int TraitDesganado = 22;
    public const int TraitOrganizado = 23;
    public const int TraitAlmaFuerte = 24;
    public const int TraitAlmaDebil = 25;
    public const int TraitTemeCorruptos = 26;
    public const int TraitTemeNomuertos = 27;
    public const int TraitTemeBestias = 28;
    public const int TraitTemeCriaturas = 29;
    public const int TraitTemeDemonios = 30;
    public const int TraitOdiaCorruptos = 31;
    public const int TraitOdiaNomuertos = 32;
    public const int TraitOdiaBestias = 33;
    public const int TraitOdiaCriaturas = 34;
    public const int TraitOdiaDemonios = 35;
    public const int TraitDuroDeMatar = 36;
    public const int TraitVengativo = 37;
    public const int TraitIndividualista = 38;
    public const int TraitColaborativo = 39;
    public const int TraitImpulsivo = 40;
    public const int TraitPaciente = 41;
    public const int TraitEspirituAlegre = 42;
    public const int TraitEspirituNegativo = 43;
    public const int TraitCoordinado = 44;
    public const int TraitTorpe = 45;
    public const int TraitBrutal = 46;
    public const int TraitClinico = 47;
    public const int TraitLoboSolitario = 48;
    public const int TraitTrabajoEnEquipo = 49;
    public const int TraitPuertasDeLaMuerte = 50;
    public const int TraitFragil = 51;
    public const int TraitProtector = 52;
    public const int TraitBajoPresion = 53;
    public const int TraitProtagonista = 54;
    public const int TraitPerfilBajo = 55;
    public const int TraitAhorrista = 56;
    public const int TraitDespilfarrador = 57;
    public const int TraitFeInquebrantable = 58;
    public const int TraitPagano = 59;
    public const int TraitAventurero = 60;
    public const int TraitArrastrado = 61;
    public const int TraitConoceBosqueArdiente = 62;
    public const int TraitDetestaBosqueArdiente = 63;
    public const int TraitConocePasoVientohelado = 64;
    public const int TraitDetestaPasoVientohelado = 65;
    public const int TraitConoceNedukazal = 66;
    public const int TraitDetestaNedukazal = 67;
    public const int TraitEsforzado = 68;
    public const int TraitMinimoEsfuerzo = 69;
    public const int TraitHolgazan = 70;
    public const int TraitTrabajador = 71;
    public const int TraitBeodo = 72;
    public const int TraitConvincente = 73;
    public const int TraitHeroeLocal = 74;
    public const int TraitAdmirado = 75;
    public const int TraitMalaReputacion = 76;
    public const int TraitBuenaReputacion = 77;
    public const int TraitHermitano = 78;
    public const int TraitCitadino = 79;
    public const int TraitEjemploASeguir = 80;
    public const int TraitFugaz = 81;
    public const int TraitTenaz = 82;
    public const int TraitTactico = 83;
    public const int TraitHerencia = 84;
    public const int TraitContrato = 85;

    private const string ColorNombre = "#f2e6c9";
    private const string ColorDescripcion = "#d7cfbf";

    private static readonly List<PersonajeTraitDefinition> definiciones = new List<PersonajeTraitDefinition>
    {
        new PersonajeTraitDefinition(
            TraitDulcesSuenos,
            "sweet_dreams",
            true,
            "Dulces Sueños",
            "Obtiene Alta Moral por dos días al descansar.",
            "Sweet Dreams",
            "Gains High Morale for two days when resting.",
            "Bons Sonhos",
            "Recebe Alta Moral por dois dias ao descansar.",
            TraitPesadillasRecurrentes),
        new PersonajeTraitDefinition(
            TraitPesadillasRecurrentes,
            "recurring_nightmares",
            true,
            "Pesadillas Recurrentes",
            "Obtiene Baja Moral por un día al descansar.",
            "Recurring Nightmares",
            "Gains Low Morale for one day when resting.",
            "Pesadelos Recorrentes",
            "Recebe Moral Baixa por um dia ao descansar.",
            TraitDulcesSuenos),
        new PersonajeTraitDefinition(
            TraitValiente,
            "brave",
            true,
            "Valiente",
            "Arranca los combates con +2 Val.",
            "Brave",
            "Starts battles with +2 Valour.",
            "Corajoso",
            "Começa os combates com +2 de Valentia.",
            TraitCobarde),
        new PersonajeTraitDefinition(
            TraitCobarde,
            "cowardly",
            true,
            "Cobarde",
            "Si falla TSMental (DC 12) al empezar combate, -2 Val.",
            "Cowardly",
            "If they fail a Mental Save (DC 12) at the start of combat, they get -2 Valour.",
            "Covarde",
            "Se falhar no Teste Mental (DC 12) no começo do combate, recebe -2 de Valentia.",
            TraitValiente),
        new PersonajeTraitDefinition(
            TraitAvispado,
            "sharp_witted",
            true,
            "Avispado",
            "No puede ser sorprendido por emboscadas enemigas.",
            "Sharp-Witted",
            "Cannot be surprised by enemy ambushes.",
            "Esperto",
            "Nao pode ser surpreendido por emboscadas inimigas.",
            TraitDistraido),
        new PersonajeTraitDefinition(
            TraitDistraido,
            "distracted",
            true,
            "Distraído",
            "Si falla TSMental (DC 11) al empezar combate, arranca Sorprendido.",
            "Distracted",
            "If they fail a Mental Save (DC 11) at the start of combat, they start Surprised.",
            "Distraído",
            "Se falhar no Teste Mental (DC 11) no começo do combate, começa Surpreendido.",
            TraitAvispado),
        new PersonajeTraitDefinition(
            TraitInteligente,
            "intelligent",
            true,
            "Inteligente",
            "+15% Experiencia obtenida.",
            "Intelligent",
            "+15% Experience gained.",
            "Inteligente",
            "+15% de Experiência obtida.",
            TraitIngenuo),
        new PersonajeTraitDefinition(
            TraitIngenuo,
            "naive",
            true,
            "Ingenuo",
            "-15% Experiencia obtenida.",
            "Naive",
            "-15% Experience gained.",
            "Ingênuo",
            "-15% de Experiência obtida.",
            TraitInteligente),
        new PersonajeTraitDefinition(
            TraitIncansable,
            "tireless",
            true,
            "Incansable",
            "No se Fatiga en combates largos.",
            "Tireless",
            "Does not become Fatigued in long battles.",
            "Incansável",
            "Nao fica Fatigado em combates longos.",
            TraitFlojo),
        new PersonajeTraitDefinition(
            TraitFlojo,
            "lazy",
            true,
            "Flojo",
            "Gana Fatiga cuando la caravana llega a Fatiga 4.",
            "Lazy",
            "Gains Fatigue when caravan fatigue reaches 4.",
            "Preguiçoso",
            "Recebe Fadiga quando a caravana chega à Fadiga 4.",
            TraitIncansable),
        new PersonajeTraitDefinition(
            TraitOptimista,
            "optimistic",
            true,
            "Optimista",
            "+5% chances de que los Eventos que toquen sean Positivos.",
            "Optimistic",
            "+5% chance for triggered Events to be Positive.",
            "Otimista",
            "+5% de chance de que os Eventos acionados sejam Positivos.",
            TraitPesimista),
        new PersonajeTraitDefinition(
            TraitPesimista,
            "pessimistic",
            true,
            "Pesimista",
            "+5% chances de que los Eventos que toquen sean Negativos.",
            "Pessimistic",
            "+5% chance for triggered Events to be Negative.",
            "Pessimista",
            "+5% de chance de que os Eventos acionados sejam Negativos.",
            TraitOptimista),
        new PersonajeTraitDefinition(
            TraitCuidadoso,
            "careful",
            true,
            "Cuidadoso",
            "-3% chances de emboscada.",
            "Careful",
            "-3% ambush chance.",
            "Cuidadoso",
            "-3% de chance de emboscada.",
            TraitRudioso),
        new PersonajeTraitDefinition(
            TraitRudioso,
            "noisy",
            true,
            "Rudioso",
            "+3% chances de emboscada.",
            "Noisy",
            "+3% ambush chance.",
            "Barulhento",
            "+3% de chance de emboscada.",
            TraitCuidadoso),
        new PersonajeTraitDefinition(
            TraitResistente,
            "resilient",
            true,
            "Resistente",
            "Si supera TSFortitud (DC 13) tras una pelea, se cura Herida.",
            "Resilient",
            "If they pass a Fortitude Save (DC 13) after a battle, they remove Wound.",
            "Resistente",
            "Se passar em um Teste de Fortitude (DC 13) após uma batalha, remove Ferida.",
            TraitEndeble),
        new PersonajeTraitDefinition(
            TraitEndeble,
            "frail",
            true,
            "Endeble",
            "Si falla TSFortitud (DC 11) al caer en combate, muere permanentemente.",
            "Frail",
            "If they fail a Fortitude Save (DC 11) when downed in combat, they die permanently.",
            "Frágil",
            "Se falhar em um Teste de Fortitude (DC 11) ao cair em combate, morre permanentemente.",
            TraitResistente),
        new PersonajeTraitDefinition(
            TraitHombrosFirmes,
            "strong_shoulders",
            true,
            "Hombros Firmes",
            "Aumenta 15 la capacidad de carga.",
            "Strong Shoulders",
            "Increases carrying capacity by 15.",
            "Ombros Firmes",
            "Aumenta a capacidade de carga em 15."),
        new PersonajeTraitDefinition(
            TraitSano,
            "healthy",
            true,
            "Sano",
            "Se cura un 15% más rápido en campaña.",
            "Healthy",
            "Heals 15% faster in campaign.",
            "Saudável",
            "Se cura 15% mais rápido na campanha.",
            TraitEnfermizo),
        new PersonajeTraitDefinition(
            TraitEnfermizo,
            "sickly",
            true,
            "Enfermizo",
            "Se cura un 15% más lento en campaña.",
            "Sickly",
            "Heals 15% slower in campaign.",
            "Enfermico",
            "Se cura 15% mais devagar na campanha.",
            TraitSano),
        new PersonajeTraitDefinition(
            TraitOdiaLaLluvia,
            "hates_rain",
            true,
            "Odia la Lluvia",
            "Obtiene Baja Moral mientras llueva o neve.",
            "Hates Rain",
            "Gains Low Morale while it rains or snows.",
            "Odeia a Chuva",
            "Recebe Moral Baixa enquanto chove ou neva."),
        new PersonajeTraitDefinition(
            TraitExperto,
            "expert",
            true,
            "Experto",
            "Maneja con maestría una de sus habilidades. Arranca nivel 3.",
            "Expert",
            "Masters one of their skills. Starts at level 3.",
            "Especialista",
            "Domina uma de suas habilidades. Começa no nível 3."),
        new PersonajeTraitDefinition(
            TraitDesganado,
            "listless",
            true,
            "Desganado",
            "Al cambiar de actividad obtiene Baja Moral por 2 días.",
            "Listless",
            "When changing activity, gains Low Morale for 2 days.",
            "Desanimado",
            "Ao mudar de atividade, recebe Moral Baixa por 2 dias."),
        new PersonajeTraitDefinition(
            TraitOrganizado,
            "organized",
            true,
            "Organizado",
            "Las mejoras de caravana valen un 5% menos.",
            "Organized",
            "Caravan upgrades cost 5% less.",
            "Organizado",
            "As melhorias da caravana custam 5% menos."),
        new PersonajeTraitDefinition(
            TraitAlmaFuerte,
            "strong_soul",
            true,
            "Alma Fuerte",
            "Si está dentro del Aliento Negro, se fortalece en batalla.",
            "Strong Soul",
            "If inside the Black Breath, becomes stronger in battle.",
            "Alma Forte",
            "Se estiver sob o Respiro Negro, fica mais forte em batalha.",
            TraitAlmaDebil),
        new PersonajeTraitDefinition(
            TraitAlmaDebil,
            "weak_soul",
            true,
            "Alma Débil",
            "Si está dentro del Aliento Negro, obtiene Baja Moral.",
            "Weak Soul",
            "If inside the Black Breath, gains Low Morale.",
            "Alma Fraca",
            "Se estiver sob o Respiro Negro, recebe Moral Baixa.",
            TraitAlmaFuerte),
        new PersonajeTraitDefinition(
            TraitTemeCorruptos,
            "fears_corrupted",
            true,
            "Teme a Corruptos",
            "Si hay Corruptos en el roster enemigo, arranca el combate con -2 Val.",
            "Fears the Corrupted",
            "If the enemy roster contains Corrupted foes, starts combat with -2 Valour.",
            "Teme os Corrompidos",
            "Se houver Corrompidos no elenco inimigo, começa o combate com -2 de Valentia.",
            TraitOdiaCorruptos),
        new PersonajeTraitDefinition(
            TraitTemeNomuertos,
            "fears_undead",
            true,
            "Teme a Nomuertos",
            "Si hay Nomuertos en el roster enemigo, arranca el combate con -2 Val.",
            "Fears the Undead",
            "If the enemy roster contains Undead foes, starts combat with -2 Valour.",
            "Teme os Mortos-vivos",
            "Se houver Mortos-vivos no elenco inimigo, começa o combate com -2 de Valentia.",
            TraitOdiaNomuertos),
        new PersonajeTraitDefinition(
            TraitTemeBestias,
            "fears_beasts",
            true,
            "Teme a Bestias",
            "Si hay Bestias en el roster enemigo, arranca el combate con -2 Val.",
            "Fears Beasts",
            "If the enemy roster contains Beasts, starts combat with -2 Valour.",
            "Teme as Bestas",
            "Se houver Bestas no elenco inimigo, começa o combate com -2 de Valentia.",
            TraitOdiaBestias),
        new PersonajeTraitDefinition(
            TraitTemeCriaturas,
            "fears_creatures",
            true,
            "Teme a Criaturas",
            "Si hay Criaturas en el roster enemigo, arranca el combate con -2 Val.",
            "Fears Creatures",
            "If the enemy roster contains Creatures, starts combat with -2 Valour.",
            "Teme as Criaturas",
            "Se houver Criaturas no elenco inimigo, começa o combate com -2 de Valentia.",
            TraitOdiaCriaturas),
        new PersonajeTraitDefinition(
            TraitTemeDemonios,
            "fears_demons",
            true,
            "Teme a Demonios",
            "Si hay Demonios en el roster enemigo, arranca el combate con -2 Val.",
            "Fears Demons",
            "If the enemy roster contains Demons, starts combat with -2 Valour.",
            "Teme os Demônios",
            "Se houver Demônios no elenco inimigo, começa o combate com -2 de Valentia.",
            TraitOdiaDemonios),
        new PersonajeTraitDefinition(
            TraitOdiaCorruptos,
            "hates_corrupted",
            true,
            "Odia a Corruptos",
            "Si hay Corruptos en el roster enemigo, obtiene +10% daño y +1 Ataque.",
            "Hates the Corrupted",
            "If the enemy roster contains Corrupted foes, gains +10% damage and +1 Attack.",
            "Odeia os Corrompidos",
            "Se houver Corrompidos no elenco inimigo, recebe +10% de dano e +1 de Ataque.",
            TraitTemeCorruptos),
        new PersonajeTraitDefinition(
            TraitOdiaNomuertos,
            "hates_undead",
            true,
            "Odia a Nomuertos",
            "Si hay Nomuertos en el roster enemigo, obtiene +10% daño y +1 Ataque.",
            "Hates the Undead",
            "If the enemy roster contains Undead foes, gains +10% damage and +1 Attack.",
            "Odeia os Mortos-vivos",
            "Se houver Mortos-vivos no elenco inimigo, recebe +10% de dano e +1 de Ataque.",
            TraitTemeNomuertos),
        new PersonajeTraitDefinition(
            TraitOdiaBestias,
            "hates_beasts",
            true,
            "Odia a Bestias",
            "Si hay Bestias en el roster enemigo, obtiene +10% daño y +1 Ataque.",
            "Hates Beasts",
            "If the enemy roster contains Beasts, gains +10% damage and +1 Attack.",
            "Odeia as Bestas",
            "Se houver Bestas no elenco inimigo, recebe +10% de dano e +1 de Ataque.",
            TraitTemeBestias),
        new PersonajeTraitDefinition(
            TraitOdiaCriaturas,
            "hates_creatures",
            true,
            "Odia a Criaturas",
            "Si hay Criaturas en el roster enemigo, obtiene +10% daño y +1 Ataque.",
            "Hates Creatures",
            "If the enemy roster contains Creatures, gains +10% damage and +1 Attack.",
            "Odeia as Criaturas",
            "Se houver Criaturas no elenco inimigo, recebe +10% de dano e +1 de Ataque.",
            TraitTemeCriaturas),
        new PersonajeTraitDefinition(
            TraitOdiaDemonios,
            "hates_demons",
            true,
            "Odia a Demonios",
            "Si hay Demonios en el roster enemigo, obtiene +10% daño y +1 Ataque.",
            "Hates Demons",
            "If the enemy roster contains Demons, gains +10% damage and +1 Attack.",
            "Odeia os Demônios",
            "Se houver Demônios no elenco inimigo, recebe +10% de dano e +1 de Ataque.",
            TraitTemeDemonios),
        new PersonajeTraitDefinition(
            TraitDuroDeMatar,
            "hard_to_kill",
            true,
            "Duro de Matar",
            "Si su vida baja a menos del 20%, obtiene +2 Defensa permanente en la pelea.",
            "Hard to Kill",
            "If Health drops below 20%, gains +2 Defense permanently for the battle.",
            "Duro de Matar",
            "Se a Vida cair abaixo de 20%, recebe +2 de Defesa permanente na batalha."),
        new PersonajeTraitDefinition(
            TraitVengativo,
            "vengeful",
            true,
            "Vengativo",
            "Si un aliado no IA cae en combate, obtiene Furia.",
            "Vengeful",
            "If a non-AI ally falls in combat, gains Fury.",
            "Vingativo",
            "Se um aliado sem IA cair em combate, recebe Fúria."),
        new PersonajeTraitDefinition(
            TraitIndividualista,
            "individualistic",
            true,
            "Individualista",
            "No permite intercambiar posiciones en combate.",
            "Individualistic",
            "Does not allow position swapping in combat.",
            "Individualista",
            "Nao permite trocar de posição em combate.",
            TraitColaborativo),
        new PersonajeTraitDefinition(
            TraitColaborativo,
            "collaborative",
            true,
            "Colaborativo",
            "Intercambiar posiciones con este personaje es gratis una vez por batalla.",
            "Collaborative",
            "Swapping positions with this character is free once per battle.",
            "Colaborativo",
            "Trocar de posição com este personagem é grátis uma vez por batalha.",
            TraitIndividualista),
        new PersonajeTraitDefinition(
            TraitImpulsivo,
            "impulsive",
            true,
            "Impuslivo",
            "Empieza fuerte, pero luego se cansa en combate.",
            "Impulsive",
            "Starts strong, but becomes tired later in combat.",
            "Impulsivo",
            "Começa forte, mas depois se cansa em combate.",
            TraitPaciente),
        new PersonajeTraitDefinition(
            TraitPaciente,
            "patient",
            true,
            "Paciente",
            "Obtiene bonificaciones de combate a partir de la ronda 3.",
            "Patient",
            "Gains combat bonuses starting on round 3.",
            "Paciente",
            "Recebe bônus de combate a partir da rodada 3.",
            TraitImpulsivo),
        new PersonajeTraitDefinition(
            TraitEspirituAlegre,
            "joyful_spirit",
            true,
            "Espíritu Alegre",
            "Si arranca combate con Alta Moral, se la contagia a aliados.",
            "Joyful Spirit",
            "If combat starts with High Morale, spreads it to allies.",
            "Espírito Alegre",
            "Se começar o combate com Moral Alta, espalha isso aos aliados.",
            TraitEspirituNegativo),
        new PersonajeTraitDefinition(
            TraitEspirituNegativo,
            "negative_spirit",
            true,
            "Espíritu Negativo",
            "Si arranca combate con Baja Moral, se la contagia a aliados.",
            "Negative Spirit",
            "If combat starts with Low Morale, spreads it to allies.",
            "Espírito Negativo",
            "Se começar o combate com Moral Baixa, espalha isso aos aliados.",
            TraitEspirituAlegre),
        new PersonajeTraitDefinition(
            TraitCoordinado,
            "coordinated",
            true,
            "Coordinado",
            "No puede Pifiar en batalla.",
            "Coordinated",
            "Cannot fumble in battle.",
            "Coordenado",
            "Nao pode sofrer Falha Crítica em batalha.",
            TraitTorpe),
        new PersonajeTraitDefinition(
            TraitTorpe,
            "clumsy",
            true,
            "Torpe",
            "+1 Rango de Pifia en batalla.",
            "Clumsy",
            "+1 Fumble range in battle.",
            "Desastrado",
            "+1 de faixa de Falha Crítica em batalha.",
            TraitCoordinado),
        new PersonajeTraitDefinition(
            TraitBrutal,
            "brutal",
            true,
            "Brutal",
            "-1 Ataque y +2 Rango crítico.",
            "Brutal",
            "-1 Attack and +2 Critical range.",
            "Brutal",
            "-1 de Ataque e +2 de faixa crítica.",
            TraitClinico),
        new PersonajeTraitDefinition(
            TraitClinico,
            "clinical",
            true,
            "Clínico",
            "+2 Ataque y -10% daño.",
            "Clinical",
            "+2 Attack and -10% damage.",
            "Clínico",
            "+2 de Ataque e -10% de dano.",
            TraitBrutal),
        new PersonajeTraitDefinition(
            TraitLoboSolitario,
            "lone_wolf",
            true,
            "Lobo Solitario",
            "Si está solo en combate obtiene bonificaciones y no lo afecta el valor grupal.",
            "Lone Wolf",
            "If alone in combat, gains bonuses and ignores group valour effects.",
            "Lobo Solitário",
            "Se estiver sozinho em combate, recebe bônus e ignora os efeitos de valentia do grupo.",
            TraitTrabajoEnEquipo),
        new PersonajeTraitDefinition(
            TraitTrabajoEnEquipo,
            "teamwork",
            true,
            "Trabajo en Equipo",
            "Obtiene Claridad: +1 TS Mental por cada aliado no IA vivo.",
            "Teamwork",
            "Gains Clarity: +1 Mental Save per living non-AI ally.",
            "Trabalho em Equipe",
            "Recebe Clareza: +1 Teste Mental por cada aliado sem IA vivo.",
            TraitLoboSolitario),
        new PersonajeTraitDefinition(
            TraitPuertasDeLaMuerte,
            "deaths_door",
            true,
            "Puertas de la Muerte",
            "Al recibir un golpe mortal, queda con 5% de vida e Invulnerable por 1 turno.",
            "Death's Door",
            "When taking a killing blow, remains at 5% Health and gains Invulnerable for 1 turn.",
            "Às Portas da Morte",
            "Ao receber um golpe mortal, fica com 5% de Vida e Invulnerável por 1 turno."),
        new PersonajeTraitDefinition(
            TraitFragil,
            "fragile",
            true,
            "Frágil",
            "+5% daño recibido.",
            "Fragile",
            "+5% damage taken.",
            "Frágil",
            "+5% de dano recebido."),
        new PersonajeTraitDefinition(
            TraitProtector,
            "protector",
            true,
            "Protector",
            "En ataques a la caravana obtiene bonificaciones de combate.",
            "Protector",
            "Gains combat bonuses during caravan defense battles.",
            "Protetor",
            "Recebe bônus de combate em defesas da caravana.",
            TraitBajoPresion),
        new PersonajeTraitDefinition(
            TraitBajoPresion,
            "under_pressure",
            true,
            "Bajo Presión",
            "En ataques a la caravana arranca con -2 Val.",
            "Under Pressure",
            "Starts caravan defense battles with -2 Valour.",
            "Sob Pressão",
            "Começa defesas da caravana com -2 de Valentia.",
            TraitProtector),
        new PersonajeTraitDefinition(
            TraitProtagonista,
            "protagonist",
            true,
            "Protagonista",
            "Mayor chance de participar en Eventos de personaje.",
            "Protagonist",
            "More likely to participate in character events.",
            "Protagonista",
            "Tem mais chance de participar de Eventos de personagem.",
            TraitPerfilBajo),
        new PersonajeTraitDefinition(
            TraitPerfilBajo,
            "low_profile",
            true,
            "Perfil Bajo",
            "Menor chance de participar en Eventos de personaje.",
            "Low Profile",
            "Less likely to participate in character events.",
            "Perfil Baixo",
            "Tem menos chance de participar de Eventos de personagem.",
            TraitProtagonista),
        new PersonajeTraitDefinition(
            TraitAhorrista,
            "frugal",
            true,
            "Ahorrista",
            "La caravana obtiene 50-100 Oro al visitar un Puesto Comercial.",
            "Frugal",
            "The caravan gains 50-100 Gold when visiting a Trading Post.",
            "Poupador",
            "A caravana recebe 50-100 de Ouro ao visitar um Posto Comercial.",
            TraitDespilfarrador),
        new PersonajeTraitDefinition(
            TraitDespilfarrador,
            "spendthrift",
            true,
            "Despilfarrador",
            "La caravana pierde 20-50 Oro al visitar un Puesto Comercial.",
            "Spendthrift",
            "The caravan loses 20-50 Gold when visiting a Trading Post.",
            "Esbanjador",
            "A caravana perde 20-50 de Ouro ao visitar um Posto Comercial.",
            TraitAhorrista),
        new PersonajeTraitDefinition(
            TraitFeInquebrantable,
            "unyielding_faith",
            true,
            "Fe Inquebrantable",
            "Obtiene Alta Moral por 4 días al visitar un Santuario.",
            "Unyielding Faith",
            "Gains High Morale for 4 days when visiting a Sanctuary.",
            "Fé Inabalável",
            "Recebe Moral Alta por 4 dias ao visitar um Santuário.",
            TraitPagano),
        new PersonajeTraitDefinition(
            TraitPagano,
            "pagan",
            true,
            "Pagano",
            "Obtiene Baja Moral por 3 días al visitar un Santuario.",
            "Pagan",
            "Gains Low Morale for 3 days when visiting a Sanctuary.",
            "Pagão",
            "Recebe Moral Baixa por 3 dias ao visitar um Santuário.",
            TraitFeInquebrantable),
        new PersonajeTraitDefinition(
            TraitAventurero,
            "adventurous",
            true,
            "Aventurero",
            "Obtiene Alta Moral por 4 días al comenzar una Zona nueva.",
            "Adventurous",
            "Gains High Morale for 4 days when a new Zone begins.",
            "Aventureiro",
            "Recebe Moral Alta por 4 dias ao começar uma nova Zona.",
            TraitArrastrado),
        new PersonajeTraitDefinition(
            TraitArrastrado,
            "dragged_along",
            true,
            "Arrastrado",
            "Obtiene Baja Moral por 3 días al comenzar una Zona nueva.",
            "Dragged Along",
            "Gains Low Morale for 3 days when a new Zone begins.",
            "Arrastado",
            "Recebe Moral Baixa por 3 dias ao começar uma nova Zona.",
            TraitAventurero),
        new PersonajeTraitDefinition(
            TraitConoceBosqueArdiente,
            "knows_burning_forest",
            true,
            "Conoce Bosque Ardiente",
            "+5% Exploración y -3% Emboscadas en Bosque Ardiente.",
            "Knows the Burning Forest",
            "+5% Exploration and -3% Ambush chance in the Burning Forest.",
            "Conhece a Floresta Ardente",
            "+5% de Exploração e -3% de Emboscadas na Floresta Ardente.",
            TraitDetestaBosqueArdiente),
        new PersonajeTraitDefinition(
            TraitDetestaBosqueArdiente,
            "hates_burning_forest",
            true,
            "Detesta Bosque Ardiente",
            "Obtiene Baja Moral por 6 días al comenzar Bosque Ardiente.",
            "Hates the Burning Forest",
            "Gains Low Morale for 6 days when the Burning Forest begins.",
            "Detesta a Floresta Ardente",
            "Recebe Moral Baixa por 6 dias ao começar a Floresta Ardente.",
            TraitConoceBosqueArdiente),
        new PersonajeTraitDefinition(
            TraitConocePasoVientohelado,
            "knows_frozen_pass",
            true,
            "Conoce Paso de Vientohelado",
            "+5% Exploración y -3% Emboscadas en Paso de Vientohelado.",
            "Knows the Windfrost Pass",
            "+5% Exploration and -3% Ambush chance in the Windfrost Pass.",
            "Conhece o Passo do Vento Gélido",
            "+5% de Exploração e -3% de Emboscadas no Passo do Vento Gélido.",
            TraitDetestaPasoVientohelado),
        new PersonajeTraitDefinition(
            TraitDetestaPasoVientohelado,
            "hates_frozen_pass",
            true,
            "Detesta Paso de Vientohelado",
            "Obtiene Baja Moral por 6 días al comenzar Paso de Vientohelado.",
            "Hates the Windfrost Pass",
            "Gains Low Morale for 6 days when the Windfrost Pass begins.",
            "Detesta o Passo do Vento Gélido",
            "Recebe Moral Baixa por 6 dias ao começar o Passo do Vento Gélido.",
            TraitConocePasoVientohelado),
        new PersonajeTraitDefinition(
            TraitConoceNedukazal,
            "knows_nedukazal",
            true,
            "Conoce Nedukazal",
            "+5% Exploración y -3% Emboscadas en Nedukazal.",
            "Knows Nedukazal",
            "+5% Exploration and -3% Ambush chance in Nedukazal.",
            "Conhece Nedukazal",
            "+5% de Exploração e -3% de Emboscadas em Nedukazal.",
            TraitDetestaNedukazal),
        new PersonajeTraitDefinition(
            TraitDetestaNedukazal,
            "hates_nedukazal",
            true,
            "Detesta Nedukazal",
            "Obtiene Baja Moral por 6 días al comenzar Nedukazal.",
            "Hates Nedukazal",
            "Gains Low Morale for 6 days when Nedukazal begins.",
            "Detesta Nedukazal",
            "Recebe Moral Baixa por 6 dias ao começar Nedukazal.",
            TraitConoceNedukazal),
        new PersonajeTraitDefinition(
            TraitEsforzado,
            "driven",
            true,
            "Esforzado",
            "Esforzarse en combate no le aplica el debuff.",
            "Driven",
            "Using Effort in battle does not apply its debuff.",
            "Esforçado",
            "Esforçar-se em combate nao aplica o debuff.",
            TraitMinimoEsfuerzo),
        new PersonajeTraitDefinition(
            TraitMinimoEsfuerzo,
            "minimal_effort",
            true,
            "Mínimo Esfuerzo",
            "No puede Esforzarse en combate.",
            "Minimal Effort",
            "Cannot use Effort in battle.",
            "Esforço Mínimo",
            "Nao pode se Esforçar em combate.",
            TraitEsforzado),
        new PersonajeTraitDefinition(
            TraitHolgazan,
            "slacker",
            true,
            "Holgazán",
            "No realiza Actividades de campaña.",
            "Slacker",
            "Does not perform campaign Activities.",
            "Preguiçoso",
            "Nao realiza Atividades de campanha."),
        new PersonajeTraitDefinition(
            TraitTrabajador,
            "hard_worker",
            true,
            "Trabajador",
            "Obtiene 150-200 Oro al llegar a un Asentamiento.",
            "Hard Worker",
            "Gains 150-200 Gold upon reaching a Settlement.",
            "Trabalhador",
            "Recebe 150-200 de Ouro ao chegar a um Assentamento.",
            TraitBeodo),
        new PersonajeTraitDefinition(
            TraitBeodo,
            "drunkard",
            true,
            "Beodo",
            "Al llegar a un Asentamiento obtiene Fatiga y gasta 10 Oro.",
            "Drunkard",
            "Upon reaching a Settlement gains Fatigue and spends 10 Gold.",
            "Bêbado",
            "Ao chegar a um Assentamento recebe Fadiga e gasta 10 de Ouro.",
            TraitTrabajador),
        new PersonajeTraitDefinition(
            TraitConvincente,
            "persuasive",
            true,
            "Convincente",
            "Obtiene 5 Civiles al llegar a un Asentamiento.",
            "Persuasive",
            "Gains 5 Civilians upon reaching a Settlement.",
            "Convincente",
            "Recebe 5 Civis ao chegar a um Assentamento."),
        new PersonajeTraitDefinition(
            TraitHeroeLocal,
            "local_hero",
            true,
            "Héroe Local",
            "Suma 15 Civiles a la Caravana. Si muere permanentemente, -20 Esperanza.",
            "Local Hero",
            "Adds 15 Civilians to the Caravan. If permanently killed, -20 Hope.",
            "Herói Local",
            "Adiciona 15 Civis à Caravana. Se morrer permanentemente, -20 Esperança.",
            TraitAdmirado),
        new PersonajeTraitDefinition(
            TraitAdmirado,
            "admired",
            true,
            "Admirado",
            "Obtiene +10 Esperanza al ganar un combate y -10 si cae en combate.",
            "Admired",
            "Gains +10 Hope upon winning a battle and -10 when falling in battle.",
            "Admirado",
            "Recebe +10 de Esperança ao vencer uma batalha e -10 ao cair em combate.",
            TraitHeroeLocal),
        new PersonajeTraitDefinition(
            TraitMalaReputacion,
            "bad_reputation",
            true,
            "Mala Reputación",
            "-2 Civiles reclutados en Asentamientos.",
            "Bad Reputation",
            "-2 Civilians recruited in Settlements.",
            "Má Reputação",
            "-2 Civis recrutados em Assentamentos.",
            TraitBuenaReputacion),
        new PersonajeTraitDefinition(
            TraitBuenaReputacion,
            "good_reputation",
            true,
            "Buena Reputación",
            "+2 Civiles reclutados en Asentamientos.",
            "Good Reputation",
            "+2 Civilians recruited in Settlements.",
            "Boa Reputação",
            "+2 Civis recrutados em Assentamentos.",
            TraitMalaReputacion),
        new PersonajeTraitDefinition(
            TraitHermitano,
            "hermit",
            true,
            "Hermitaño",
            "Obtiene Baja Moral por 2 días al llegar a un Asentamiento.",
            "Hermit",
            "Gains Low Morale for 2 days upon reaching a Settlement.",
            "Eremita",
            "Recebe Moral Baixa por 2 dias ao chegar a um Assentamento.",
            TraitCitadino),
        new PersonajeTraitDefinition(
            TraitCitadino,
            "city_dweller",
            true,
            "Citadino",
            "Obtiene Alta Moral por 3 días al llegar a un Asentamiento.",
            "City Dweller",
            "Gains High Morale for 3 days upon reaching a Settlement.",
            "Citadino",
            "Recebe Moral Alta por 3 dias ao chegar a um Assentamento.",
            TraitHermitano),
        new PersonajeTraitDefinition(
            TraitEjemploASeguir,
            "role_model",
            true,
            "Ejemplo a Seguir",
            "Al llegar al Puerto de Serria se obtienen +25 Valor de Trabajo.",
            "Role Model",
            "Upon reaching the Port of Serria, grants +25 Work Value.",
            "Exemplo a Seguir",
            "Ao chegar ao Porto de Serria, concede +25 de Valor de Trabalho."),
        new PersonajeTraitDefinition(
            TraitFugaz,
            "fugacious",
            true,
            "Fugaz",
            "Escapar solo consume 1 AP.",
            "Fugacious",
            "Fleeing only costs 1 AP.",
            "Fugaz",
            "Fugir consome apenas 1 AP.",
            TraitTenaz),
        new PersonajeTraitDefinition(
            TraitTenaz,
            "tenacious",
            true,
            "Tenaz",
            "Se rehusa a huir de batalla (no obtiene Escapar).",
            "Tenacious",
            "Refuses to flee from battle (does not gain Escape).",
            "Tenaz",
            "Recusa fugir da batalha (nao recebe Escapar).",
            TraitFugaz),
        new PersonajeTraitDefinition(
            TraitTactico,
            "tactical",
            true,
            "Táctico",
            "Las vías de escape llegan 1 turno antes y los refuerzos enemigos se retrasan 1 turno.",
            "Tactical",
            "Escape routes arrive 1 turn earlier and enemy reinforcements are delayed by 1 turn.",
            "Tático",
            "As vias de escape chegam 1 turno antes e os reforcos inimigos atrasam 1 turno."),
        new PersonajeTraitDefinition(
            TraitHerencia,
            "inheritance",
            true,
            "Herencia",
            "Nace con un arma o armadura de su clase, mejorada a +1 o +2. Si su clase no tiene, recibe un consumible aleatorio.",
            "Inheritance",
            "Starts with their class weapon or armor at +1 or +2. If their class has none, they receive a random consumable.",
            "Herança",
            "Começa com a arma ou armadura da sua classe em +1 ou +2. Se a classe não tiver, recebe um consumível aleatório."),
        new PersonajeTraitDefinition(
            TraitContrato,
            "contract",
            true,
            "Contrato",
            "Trabaja con la caravana por contrato, así que cobra. En cada descanso se le pagan 50 de Oro automáticamente. Si no es posible, obtiene Baja Moral por 3 días.",
            "Contract",
            "Works with the caravan on contract, so they get paid. Each rest, 50 Gold is paid automatically. If that is not possible, they gain Low Morale for 3 days.",
            "Contrato",
            "Trabalha com a caravana por contrato, portanto recebe pagamento. A cada descanso, 50 de Ouro são pagos automaticamente. Se não for possível, recebe Moral Baixa por 3 dias.")
    };

    private static readonly Dictionary<int, PersonajeTraitDefinition> definicionesPorId = CrearIndicePorId();

    public static int ObtenerIdiomaActual()
    {
        return TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    }

    public static string FormatearParaUi(PersonajeTraitDefinition definicion, int idioma)
    {
        if (definicion == null)
        {
            return string.Empty;
        }

        string nombre = definicion.ObtenerNombre(idioma);
        string descripcion = definicion.ObtenerDescripcion(idioma);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return "<color=" + ColorNombre + "><b>" + nombre + ":</b></color>";
        }

        return "<color=" + ColorNombre + "><b>" + nombre + ":</b></color> <color=" + ColorDescripcion + "><size=85%>" + descripcion + "</size></color>";
    }

    public static bool TryGet(int id, out PersonajeTraitDefinition definicion)
    {
        return definicionesPorId.TryGetValue(id, out definicion);
    }

    public static bool SonCompatibles(int traitA, int traitB)
    {
        if (traitA <= 0 || traitB <= 0)
        {
            return true;
        }

        if (traitA == traitB)
        {
            return false;
        }

        bool existeA = TryGet(traitA, out PersonajeTraitDefinition definicionA);
        bool existeB = TryGet(traitB, out PersonajeTraitDefinition definicionB);
        if (!existeA || !existeB)
        {
            return true;
        }

        return !definicionA.EsAntagonicoCon(traitB) && !definicionB.EsAntagonicoCon(traitA);
    }

    public static List<PersonajeTraitDefinition> ObtenerTraitsDisponiblesAlCrear()
    {
        List<PersonajeTraitDefinition> disponibles = new List<PersonajeTraitDefinition>();
        for (int i = 0; i < definiciones.Count; i++)
        {
            if (definiciones[i].DisponibleAlCrear)
            {
                disponibles.Add(definiciones[i]);
            }
        }

        return disponibles;
    }

    private static Dictionary<int, PersonajeTraitDefinition> CrearIndicePorId()
    {
        Dictionary<int, PersonajeTraitDefinition> indice = new Dictionary<int, PersonajeTraitDefinition>();
        for (int i = 0; i < definiciones.Count; i++)
        {
            PersonajeTraitDefinition definicion = definiciones[i];
            if (definicion == null || indice.ContainsKey(definicion.Id))
            {
                continue;
            }

            indice.Add(definicion.Id, definicion);
        }

        return indice;
    }
}
