using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class EscudoEnergetico : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
     public override void  Awake()
    {
      nombre = "Escudo Energético";
      IDenClase = 6;
      costoAP = 2; //Termina turno
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_EscudoEnergetico");
    }

   
    public override void ActualizarDescripcion()
    {
      if(NIVEL < 2)
{
    txtDescripcion = "<color=#5dade2><b>Escudo Energético I</b></color>\n\n";
    txtDescripcion += "<i>Canaliza una barrera de energía que refuerza su defensa y devuelve proyectiles en forma de descarga arcana.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Propio.</b> - Defensa: 1 + Nivel Energía - Proyectiles fallidos: Descarga Arcana al atacante y genera Residuo Energético en casilla adyacente.</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Máximo usos: 3 por turno\n- Costo AP: 2 (termina turno)\n- Cooldown: {cooldownMax} \n- Costo Val: 0</color>\n";
    txtDescripcion += $"<color=#ea0606>- Se cancela si recibe daño</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 Defensa</color>\n\n";
            }
        }
    }
}

if(NIVEL == 2)
{
    txtDescripcion = "<color=#5dade2><b>Escudo Energético II</b></color>\n\n";
    txtDescripcion += "<i>Canaliza una barrera de energía que refuerza su defensa y devuelve proyectiles en forma de descarga arcana.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Propio.</b> - Defensa: 2 + Nivel Energía - Proyectiles fallidos: Descarga Arcana al atacante y genera Residuo Energético en casilla adyacente.</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Máximo usos: 3 por turno\n- Costo AP: 2 (termina turno)\n- Cooldown: {cooldownMax} \n- Costo Val: 0</color>\n";
    txtDescripcion += $"<color=#ea0606>- Se cancela si recibe daño</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 Tirada Ataque en Descarga</color>\n\n";
            }
        }
    }
}

if(NIVEL == 3)
{
    txtDescripcion = "<color=#5dade2><b>Escudo Energético III</b></color>\n\n";
    txtDescripcion += "<i>Canaliza una barrera de energía que refuerza su defensa y devuelve proyectiles en forma de descarga arcana.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Propio.</b> - Defensa: 2 + Nivel Energía - Proyectiles fallidos: Descarga Arcana (+1 Ataque) al atacante y genera Residuo Energético en casilla adyacente.</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Máximo usos: 3 por turno\n- Costo AP: 2 (termina turno)\n- Cooldown: {cooldownMax} \n- Costo Val: 0</color>\n";
    txtDescripcion += $"<color=#ea0606>- Se cancela si recibe daño</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel:\nOpción A: No se cancela al ser golpeado\nOpción B: +1 uso por turno</color>\n\n";
            }
        }
    }
}

if(NIVEL == 4)
{
    // Variante A
    txtDescripcion = "<color=#5dade2><b>Escudo Energético IV a</b></color>\n\n";
    txtDescripcion += "<i>Canaliza una barrera de energía que refuerza su defensa y devuelve proyectiles en forma de descarga arcana.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Propio.</b> - Defensa: 2 + Nivel Energía - Proyectiles fallidos: Descarga Arcana (+1 Ataque) al atacante y genera Residuo Energético en casilla adyacente.</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Máximo usos: 3 por turno\n- Costo AP: 2 (termina turno)\n- Cooldown: {cooldownMax} \n- Costo Val: 0</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Ya no se cancela al recibir daño</color>\n\n";
}

if(NIVEL == 5)
{
    // Variante B
    txtDescripcion = "<color=#5dade2><b>Escudo Energético IV b</b></color>\n\n";
    txtDescripcion += "<i>Canaliza una barrera de energía que refuerza su defensa y devuelve proyectiles en forma de descarga arcana.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Propio.</b> - Defensa: 2 + Nivel Energía - Proyectiles fallidos: Descarga Arcana (+1 Ataque) al atacante y genera Residuo Energético en casilla adyacente.</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Máximo usos: 4 por turno\n- Costo AP: 2 (termina turno)\n- Cooldown: {cooldownMax} \n- Costo Val: 0</color>\n";
    txtDescripcion += $"<color=#ea0606>- Se cancela si recibe daño</color>\n\n";
}


    }
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

       Unidad objetivo = (Unidad)obj;
         VFXAplicar(objetivo.gameObject);
      ClaseCanalizador scCana = (ClaseCanalizador)objetivo;
      float defensa = 10*scCana.ObtenerEnergia(); 
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Escudo Energético";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantDefensa += 1+defensa;
       if (NIVEL > 1) { buff.cantDefensa += 1; }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacción 
       ReaccionEscudoEnergetico reaccion = new ReaccionEscudoEnergetico();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Escudo Energético";
       ReaccionEscudoEnergetico reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       //Usarla termina el turno
       BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_EscudoEnergetico");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder = 200;  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
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
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }

   
 
}
