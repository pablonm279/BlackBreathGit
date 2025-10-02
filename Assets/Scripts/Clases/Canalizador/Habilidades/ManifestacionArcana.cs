using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ManifestacionArcana : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
    public override void  Awake()
    {
      nombre = "Manifestación Arcana";
      IDenClase = 10;
      costoAP = 7;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      bAfectaObstaculos = false;
      poneTrampas = false;
      poneObstaculo = true;
      
      requiereRecurso = 2; //Requiere tener 2 Tier energía 
      if (NIVEL == 5) { requiereRecurso--; }
      
      imHab = Resources.Load<Sprite>("imHab/Canalizador_ManifestacionArcana");
    }
    public override void ActualizarDescripcion()
    {
       if (NIVEL < 2)
{
    txtDescripcion = "<color=#ab47bc><b>Manifestación Arcana</b></color>\n\n";
    txtDescripcion += "<i>Canaliza energía pura para invocar una criatura semi-humanoide formada de residuos arcanos fluctuantes. Su fuerza aumenta según la energía acumulada en el campo.</i>\n\n";
    txtDescripcion += $"<color=#44d3ec>-Costo AP: 7 - Costo Val: 1 - Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>-Requiere Nivel de Energía: 2</color>\n\n";
    txtDescripcion += "<color=#c8c8c8><b>Efecto:</b></color> Invoca una <b>Manifestación Arcana</b>.\n";
    txtDescripcion += "Al ser invocada, absorbe todos los Residuos Energéticos del campo, ganando <color=#58d68d>+5% Daño</color> y <color=#58d68d>+6 Vida</color> por cada uno.\n\n";

    if (EsEscenaCampaña())
    {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Defensa</color>\n\n";
            }
        }
    }
}
if (NIVEL == 2)
{
    txtDescripcion = "<color=#ab47bc><b>Manifestación Arcana II</b></color>\n\n";
    txtDescripcion += "<i>Canaliza energía pura para invocar una criatura semi-humanoide formada de residuos arcanos fluctuantes. Su fuerza aumenta según la energía acumulada en el campo.</i>\n\n";
    txtDescripcion += $"<color=#44d3ec>-Costo AP: 7 - Costo Val: 1 - Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>-Requiere Nivel de Energía: 2</color>\n\n";
    txtDescripcion += "<color=#c8c8c8><b>Efecto:</b></color> Invoca una <b>Manifestación Arcana</b>, que absorbe todos los Residuos Energéticos del campo, ganando <color=#58d68d>+5% Daño</color> y <color=#58d68d>+6 Vida</color> por cada uno.\n";
    txtDescripcion += "<color=#58d68d>+1 Defensa base</color>\n\n";

    if (EsEscenaCampaña())
    {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
            }
        }
    }
}
if (NIVEL == 3)
{
    txtDescripcion = "<color=#ab47bc><b>Manifestación Arcana III</b></color>\n\n";
    txtDescripcion += "<i>Canaliza energía pura para invocar una criatura semi-humanoide formada de residuos arcanos fluctuantes. Su fuerza aumenta según la energía acumulada en el campo.</i>\n\n";
    txtDescripcion += $"<color=#44d3ec>-Costo AP: 7 - Costo Val: 1 - Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>-Requiere Nivel de Energía: 2</color>\n\n";
    txtDescripcion += "<color=#c8c8c8><b>Efecto:</b></color> Invoca una <b>Manifestación Arcana</b>, que absorbe todos los Residuos Energéticos del campo, ganando <color=#58d68d>+5% Daño</color> y <color=#58d68d>+6 Vida</color> por cada uno.\n";
    txtDescripcion += "<color=#58d68d>+1 Defensa base</color>\n";
    txtDescripcion += "<color=#58d68d>+1 Ataque base</color>\n\n";

    if (EsEscenaCampaña())
    {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel:\nA) +1 AP Máximo\nB) -1 Requisito Energía</color>\n\n";
            }
        }
    }
}
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#ab47bc><b>Manifestación Arcana IVa</b></color>\n\n";
      txtDescripcion += "<i>Canaliza energía pura para invocar una criatura semi-humanoide formada de residuos arcanos fluctuantes. Su fuerza aumenta según la energía acumulada en el campo.</i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Costo AP: 7 - Costo Val: 1 - Cooldown: {cooldownMax}</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Requiere Nivel de Energía: 2</color>\n\n";
      txtDescripcion += "<color=#c8c8c8><b>Efecto:</b></color> Invoca una <b>Manifestación Arcana</b>, que absorbe todos los Residuos Energéticos del campo, ganando <color=#58d68d>+5% Daño</color> y <color=#58d68d>+6 Vida</color> por cada uno.\n";
      txtDescripcion += "<color=#58d68d>+1 Defensa base</color>\n";
      txtDescripcion += "<color=#58d68d>+1 Ataque base</color>\n";
      txtDescripcion += "<color=#dfea02> +1 AP Máximo</color>\n";
    }
if (NIVEL == 5)
{
    txtDescripcion = "<color=#ab47bc><b>Manifestación Arcana IVb</b></color>\n\n";
    txtDescripcion += "<i>Canaliza energía pura para invocar una criatura semi-humanoide formada de residuos arcanos fluctuantes. Su fuerza aumenta según la energía acumulada en el campo.</i>\n\n";
    txtDescripcion += $"<color=#44d3ec>-Costo AP: 7 - Costo Val: 1 - Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>-Requiere Nivel de Energía: 1</color>\n\n";
    txtDescripcion += "<color=#c8c8c8><b>Efecto:</b></color> Invoca una <b>Manifestación Arcana</b>, que absorbe todos los Residuos Energéticos del campo, ganando <color=#58d68d>+5% Daño</color> y <color=#58d68d>+6 Vida</color> por cada uno.\n";
    txtDescripcion += "<color=#58d68d>+1 Defensa base</color>\n";
    txtDescripcion += "<color=#58d68d>+1 Ataque base</color>\n";

}

    }
    void Start()
    {
      
    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
      
     GameObject manifestacion = Instantiate(BattleManager.Instance.contenedorPrefabs.Canalizador_ManifestacionArcana);
     Unidad uMani = manifestacion.GetComponent<Unidad>();
     
       int cantidadResiduos = 0;
      // Buscar en todas las casillas de ambos lados
      List<Casilla> todasLasCasillas = BattleManager.Instance.lCasillasTotal;

      foreach (var casilla in todasLasCasillas)
      {
        Trampa trampa = casilla.GetComponent<Trampa>();
        if (trampa != null && trampa.nombre == "Residuo Energetico")
        {
          trampa.DestruirTrampa();

           /////////////////////////////////////////////
          //BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Energía Absorbida";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDanioPorcentaje += 5;
          buff.cantHPMax += 6;
          buff.AplicarBuff(uMani);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, uMani.gameObject);
          uMani.HP_actual = uMani.mod_maxHP;


          cantidadResiduos++;
        }
      }

    if (NIVEL > 1)
    { uMani.mod_Ataque += 1;}
    if (NIVEL > 2)
    { uMani.mod_Defensa += 1;}
    if (NIVEL == 4)
    { uMani.mod_maxAccionP += 1; uMani.CambiarAPActual((int)uMani.ObtenerAPActual()); }


    uMani.TirarIniciativa();



    

     manifestacion.SetActive(true);
     cas.PonerObjetoEnCasilla(manifestacion);
     invocado = manifestacion;
     invocado.transform.rotation = Quaternion.Euler(0, 180, 0);
     BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
     manifestacion.GetComponent<Unidad>().EstablecerAPActualA(0);
    

     
       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
    }
  GameObject invocado;
  void rotarInvocado()
  { 
    invocado.transform.rotation = Quaternion.Euler(0, 180, 0);

  }
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
     
     
      
      //Casillas Alrededor al origen
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente == null)
        {
            continue;
        }
        
      
        if(c.Presente.GetComponent<Unidad>() == null)
        {
        continue;
        }
          if(c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
        }

      }
    
         
    }

   
    

 
}
