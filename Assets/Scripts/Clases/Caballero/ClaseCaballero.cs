using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class AparienciaAlternativaCaballero
{
   public string nombre;
   public Sprite retrato;
   public Sprite poseIdle;
   public Sprite poseMover;
   public Sprite poseAtacar;
   public Sprite poseHabilidad;
   public Sprite poseRecibirDanio;
   public Sprite poseTurnoActivo;
   public Sprite posePosturaDefensiva;

   public bool TieneContenido()
   {
      return retrato != null || poseIdle != null || poseMover != null || poseAtacar != null || poseHabilidad != null || poseRecibirDanio != null || poseTurnoActivo != null || posePosturaDefensiva != null;
   }
}

public class ClaseCaballero : Unidad
{
   private const string BuffNombrePosturaDefensiva = "Postura Defensiva";
   
   public int PASIVA_Acorazado; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Determinacion;  //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Implacable;  //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Implacable_CARGAS; 
   private bool posePosturaDefensivaActiva;
   private Sprite poseIdleOriginal;
   private Sprite poseTurnoActivoOriginal;
   private Sprite posePosturaDefensivaAlternativaActiva;
   private UnidadPoseController poseControllerCaballero;
   

   public Sprite Pose_PosturaDefensiva;
   public List<AparienciaAlternativaCaballero> aparienciasAlternativas = new List<AparienciaAlternativaCaballero>();



  public override void SumarValentia(int cant, string motivo = null, bool mostrarTextoFlotante = true)
  {
    base.SumarValentia(cant, motivo, mostrarTextoFlotante); //hace todo lo mismo que el metodo original, y agrega lo de abajo al final
    AplicarPasivasValentiaCaballero();
  }

  public override void AjustarValentiaInicialSinLog(int cant, bool notificarValourGlobal = true)
  {
    base.AjustarValentiaInicialSinLog(cant, notificarValourGlobal);
    AplicarPasivasValentiaCaballero();
  }

  private void AplicarPasivasValentiaCaballero()
  {
    if(ValentiaP_actual < 0) //Pasiva - "Coraje Inquebrantable: Sus puntos de valentía no pueden ser negativos."
    {
      ValentiaP_actual = 0;
    }

    //PASIVA_Implacable-------------------------------------
    if(PASIVA_Implacable > 0 && ValentiaP_actual == mod_maxValentiaP && PASIVA_Implacable_CARGAS > 0) //Pasiva -Aumenta stats si Valentía al máximo, por 2 Turnos, 1 vez. 
    {
      bool yaTieneElBuff = false;
      Buff[] buffs = gameObject.GetComponents<Buff>();
      foreach(Buff b in buffs)
      {
        if(b.buffNombre == "Implacable"){  yaTieneElBuff = true;}
      }

      if(!yaTieneElBuff)
      {
        PASIVA_Implacable_CARGAS--;
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Implacable";
        buff.suprimeTextoFlotante = true;
        buff.ocultarEnBarraVida = true;
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtFue += 3;
        buff.cantAPMax += 2;
        buff.cantDanioPorcentaje += 20;
        buff.cantTsMental += 3;
        if(PASIVA_Implacable > 1){ buff.cantAtFue += 1;  buff.cantTsFortaleza += 2;}
        if(PASIVA_Implacable > 2){ buff.cantCritDado += 1; }
        if(PASIVA_Implacable == 5){ buff.DuracionBuffRondas += 1; }
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject);
      }
    }
  }

  public override void ReducirArmaduraPorGolpe(float danioFinal)
  {

   switch(PASIVA_Acorazado)
   {
      case 0: if(danioFinal > 0){estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break; //No tiene la pasiva
      //-----------------------------------------------------//
      case 1: if(danioFinal > 5){estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break; //Nv 1  6 daño o mas para reducir armadura
      case 2: if(danioFinal > 6){estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break; //Nv 2  7 daño o mas para reducir armadura
      case 3: if(danioFinal > 7){estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break; //Nv 3  8 daño o mas para reducir armadura
      case 4: if(danioFinal > 9){estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break; //Nv 4a  10 daño o mas para reducir armadura
      case 5: if(danioFinal > 7 && (ObtenerArmaduraActual() > mod_Armadura/2)) //Nv 4b 8 daño o mas, y no se puede reducir a menos de la mitad de la armadura inicial.
      {estado_armaduraModificador++; scTextoArmaduraFlash.Flash();} break;
   }


  }

  public override void ActualizarClaseComienzoTurno()
  {
    ChequearBuffPASIVADeterminacion();
    SincronizarPosturaDefensivaSegunBuffActual();
  }

  public override void AplicarAparienciaAlternativaAleatoria()
  {
    AplicarAparienciaAlternativaPorIndice(ElegirIndiceAparienciaAlternativaAleatoria());
  }

  public override void AplicarAparienciaAlternativaPorIndice(int indiceApariencia)
  {
    if (poseControllerCaballero == null)
    {
      poseControllerCaballero = GetComponent<UnidadPoseController>();
    }

    if (poseControllerCaballero == null)
    {
      return;
    }

    AparienciaAlternativaCaballero aparienciaElegida = ObtenerAparienciaAlternativaCaballero(indiceApariencia);
    posePosturaDefensivaAlternativaActiva = aparienciaElegida != null ? aparienciaElegida.posePosturaDefensiva : null;
    if (aparienciaElegida == null)
    {
      poseControllerCaballero.RestaurarPosesBase();
      return;
    }

    Sprite poseIdleBase = poseControllerCaballero.ObtenerPoseIdleBase() != null ? poseControllerCaballero.ObtenerPoseIdleBase() : (uImage != null ? uImage.sprite : null);
    Sprite poseMoverBase = poseControllerCaballero.ObtenerPoseMoverBase() != null ? poseControllerCaballero.ObtenerPoseMoverBase() : poseIdleBase;
    Sprite poseAtacarBase = poseControllerCaballero.ObtenerPoseAtacarBase() != null ? poseControllerCaballero.ObtenerPoseAtacarBase() : poseIdleBase;
    Sprite poseHabilidadBase = poseControllerCaballero.ObtenerPoseHabilidadBase() != null ? poseControllerCaballero.ObtenerPoseHabilidadBase() : poseIdleBase;
    Sprite poseRecibirDanioBase = poseControllerCaballero.ObtenerPoseRecibirDanioBase();
    Sprite poseTurnoActivoBase = poseControllerCaballero.ObtenerPoseTurnoActivoBase();

    Sprite poseIdle = aparienciaElegida.poseIdle != null ? aparienciaElegida.poseIdle : poseIdleBase;
    Sprite poseMover = aparienciaElegida.poseMover != null ? aparienciaElegida.poseMover : poseMoverBase;
    Sprite poseAtacar = aparienciaElegida.poseAtacar != null ? aparienciaElegida.poseAtacar : poseAtacarBase;
    Sprite poseHabilidad = aparienciaElegida.poseHabilidad != null ? aparienciaElegida.poseHabilidad : poseHabilidadBase;
    Sprite poseRecibirDanio = aparienciaElegida.poseRecibirDanio != null ? aparienciaElegida.poseRecibirDanio : poseRecibirDanioBase;
    Sprite poseTurnoActivo = aparienciaElegida.poseTurnoActivo != null ? aparienciaElegida.poseTurnoActivo : poseTurnoActivoBase;

    poseControllerCaballero.ConfigurarPoses(poseIdle, poseMover, poseAtacar, poseHabilidad, poseRecibirDanio, poseTurnoActivo);
  }

  public override int ObtenerCantidadAparienciasAlternativas()
  {
    return 1 + ObtenerAparienciasAlternativasCaballeroValidas().Count;
  }

  public override bool EsIndiceAparienciaAlternativaValido(int indiceApariencia)
  {
    return indiceApariencia == Personaje.IndiceAparienciaBase || ObtenerAparienciaAlternativaCaballero(indiceApariencia) != null;
  }

  public override Sprite ObtenerRetratoAparienciaAlternativa(int indiceApariencia)
  {
    AparienciaAlternativaCaballero apariencia = ObtenerAparienciaAlternativaCaballero(indiceApariencia);
    return apariencia != null ? apariencia.retrato : null;
  }

  public override List<int> ObtenerIndicesAparienciasAlternativasDisponibles()
  {
    List<int> indicesDisponibles = new List<int> { Personaje.IndiceAparienciaBase };
    if (aparienciasAlternativas == null || aparienciasAlternativas.Count == 0)
    {
      return indicesDisponibles;
    }

    for (int i = 0; i < aparienciasAlternativas.Count; i++)
    {
      AparienciaAlternativaCaballero apariencia = aparienciasAlternativas[i];
      if (apariencia != null && apariencia.TieneContenido())
      {
        indicesDisponibles.Add(i);
      }
    }

    return indicesDisponibles;
  }

  AparienciaAlternativaCaballero ObtenerAparienciaAlternativaCaballero(int indiceApariencia)
  {
    if (aparienciasAlternativas == null || indiceApariencia < 0 || indiceApariencia >= aparienciasAlternativas.Count)
    {
      return null;
    }

    AparienciaAlternativaCaballero apariencia = aparienciasAlternativas[indiceApariencia];
    return apariencia != null && apariencia.TieneContenido() ? apariencia : null;
  }

  List<AparienciaAlternativaCaballero> ObtenerAparienciasAlternativasCaballeroValidas()
  {
    List<AparienciaAlternativaCaballero> aparienciasValidas = new List<AparienciaAlternativaCaballero>();
    if (aparienciasAlternativas == null || aparienciasAlternativas.Count == 0)
    {
      return aparienciasValidas;
    }

    for (int i = 0; i < aparienciasAlternativas.Count; i++)
    {
      AparienciaAlternativaCaballero apariencia = aparienciasAlternativas[i];
      if (apariencia != null && apariencia.TieneContenido())
      {
        aparienciasValidas.Add(apariencia);
      }
    }

    return aparienciasValidas;
  }

  public override int ElegirIndiceAparienciaAlternativaAleatoria()
  {
    List<AparienciaAlternativaCaballero> aparienciasValidas = ObtenerAparienciasAlternativasCaballeroValidas();
    if (aparienciasValidas.Count == 0)
    {
      return Personaje.IndiceAparienciaBase;
    }

    int opcionElegida = UnityEngine.Random.Range(0, aparienciasValidas.Count + 1);
    if (opcionElegida == 0)
    {
      return Personaje.IndiceAparienciaBase;
    }

    AparienciaAlternativaCaballero aparienciaElegida = aparienciasValidas[opcionElegida - 1];
    return aparienciasAlternativas.IndexOf(aparienciaElegida);
  }

    void ChequearBuffPASIVADeterminacion()
    {
      if(PASIVA_Determinacion > 0 && ValentiaP_actual > 0)
      {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff buffDet = new Buff();
       buffDet.buffNombre = "Determinación "+ValentiaP_actual;
       buffDet.suprimeTextoFlotante = true;
       buffDet.ocultarEnBarraVida = false;
       buffDet.boolfDebufftBuff = true;
       buffDet.DuracionBuffRondas = 1; 
       buffDet.cantDanioPorcentaje = 5*ValentiaP_actual;
       if(PASIVA_Determinacion == 5) //PASIVA_Determinacion Nv 4b +2% daño por Valentía
       {
         buffDet.cantDanioPorcentaje += 2*ValentiaP_actual;
       }
       buffDet.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
     
       Buff buffComponent = ComponentCopier.CopyComponent(buffDet, gameObject);
       

      }
    }

    public override void AplicarMotivado()
    {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Motivado";
       motivado.suprimeTextoFlotante = true;
       motivado.ocultarEnBarraVida = true;
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantTsMental += 2;
       if(PASIVA_Determinacion > 1) //Determinacion nv 2 o +
       {
         motivado.cantTsMental += 1;
         motivado.cantTsFortaleza += 1;
         motivado.cantTsReflejos += 1;
       }
       motivado.cantTsFortaleza += 1;
       motivado.cantTsReflejos += 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
    }
    public override void AplicarEuforico()
   {
   /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Euforia";
       motivado.suprimeTextoFlotante = true;
       motivado.ocultarEnBarraVida = true;
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantAtFue += 1;
       motivado.cantAtPod += 1;
       motivado.cantAtAgi += 1;
      if(PASIVA_Determinacion > 2) //Determinacion nv 3 o +
       {
         motivado.cantAtaque += 1;
       }
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
   }
    

    public override void ComienzoBatallaClase()
    {
       base.ComienzoBatallaClase();
       SincronizarPosturaDefensivaSegunBuffActual();


       //PASIVA_Determinacion Nv 4a
       if(PASIVA_Determinacion == 4)
       {
         ValentiaP_actual = 5;
         AplicarMotivado();
         AplicarEuforico(); //Se llaman desde aca porque si no no se agregan, al no detectar el valor anterior por que es turno 1

       }

       //PASIVA_Implacable 
       if(PASIVA_Implacable > 0 && PASIVA_Implacable != 4)
       {
         PASIVA_Implacable_CARGAS = 1;
         mod_maxValentiaP += 2;
       }
       else if(PASIVA_Implacable > 0 && PASIVA_Implacable == 4)
       {
         PASIVA_Implacable_CARGAS = 2;
         mod_maxValentiaP += 1;
       }

    }

    public void NotificarInicioPosturaDefensiva()
    {
      ActualizarPosePosturaDefensiva(true);
    }

    public void NotificarFinPosturaDefensiva()
    {
      ActualizarPosePosturaDefensiva(false);
    }

    private void SincronizarPosturaDefensivaSegunBuffActual()
    {
      if (TieneBuffNombre(BuffNombrePosturaDefensiva))
      {
        NotificarInicioPosturaDefensiva();
        return;
      }

      NotificarFinPosturaDefensiva();
    }

    private void ActualizarPosePosturaDefensiva(bool activar)
    {
      if (poseControllerCaballero == null)
      {
        poseControllerCaballero = GetComponent<UnidadPoseController>();
      }

      if (poseControllerCaballero == null)
      {
        return;
      }

      Sprite posePosturaDefensiva = posePosturaDefensivaAlternativaActiva != null ? posePosturaDefensivaAlternativaActiva : Pose_PosturaDefensiva;
      if (posePosturaDefensiva == null)
      {
        return;
      }

      if (activar)
      {
        if (!posePosturaDefensivaActiva)
        {
          poseIdleOriginal = poseControllerCaballero.poseIdle;
          poseTurnoActivoOriginal = poseControllerCaballero.poseTurnoActivo;
        }

        poseControllerCaballero.poseIdle = posePosturaDefensiva;
        poseControllerCaballero.poseTurnoActivo = posePosturaDefensiva;
        posePosturaDefensivaActiva = true;
        poseControllerCaballero.SetIdle();
        return;
      }

      if (posePosturaDefensivaActiva)
      {
        poseControllerCaballero.poseIdle = poseIdleOriginal;
        poseControllerCaballero.poseTurnoActivo = poseTurnoActivoOriginal;
        poseControllerCaballero.RefrescarPoseActual();
      }

      posePosturaDefensivaActiva = false;
    }


    public bool tieneCorazaDeLlamas; //esto se pone TRUE al inicio del combate en AdministradorEscenas "AplicarEfectosItemsEspecificos"

  public async override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayefectos = 0, bool ignorarEscudo = false)
  {
    base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayefectos, ignorarEscudo);
    //------------------------------------------------------------

    //Reaccion Postura Defensiva
    if (gameObject != null)
    {
      if (gameObject.GetComponent<ReaccionPosturaDefensiva>() != null)
      {
        ReaccionPosturaDefensiva reacc = gameObject.GetComponent<ReaccionPosturaDefensiva>();
        if (reacc.NIVEL != 4) //Si tiene la reaccion activa y no es nivel 4a, la remueve
        {
          Destroy(reacc);
          await gameObject.GetComponent<Unidad>().GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Postura Defensiva") + "</s>", Color.blue);


        }

      }
    }

    //Armadura de Coraza de Llamas +1
    if (tieneCorazaDeLlamas && uCausante.CasillaPosicion.posX == 3)
    {
      if (uCausante != null)
      {
        if (uCausante.TiradaSalvacion(2, 10))
        {
          Estados.Aplicar_Ardiendo(uCausante, 1, this);

        }


      }




    }
  
    }

}


