using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class HombroConHombro : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
      public override void  Awake()
    {
    

      nombre = "HombroConHombro";
      IDenClase = 10;
      costoAP = 2;
      costoPM = 2;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;
      

       imHab = Resources.Load<Sprite>("imHab/Caballero_HombroconHombro");

        ActualizarDescripcion();
    
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int bonoDefensa = 2 + (NIVEL > 1 ? 1 : 0);
      int bonoAtaque = 2 + (NIVEL > 2 ? 1 : 0);
      bool daInvulnerable = NIVEL == 4;
      bool daApMax = NIVEL == 5;

      string tituloEs = "Hombro con Hombro I";
      string tituloEn = "Shoulder to Shoulder I";
      string tituloPt = "Ombro a Ombro I";
      if (NIVEL == 2) { tituloEs = "Hombro con Hombro II"; tituloEn = "Shoulder to Shoulder II"; }
      if (NIVEL == 3) { tituloEs = "Hombro con Hombro III"; tituloEn = "Shoulder to Shoulder III"; }
      if (NIVEL == 4) { tituloEs = "Hombro con Hombro IV a"; tituloEn = "Shoulder to Shoulder IV a"; }
      if (NIVEL == 5) { tituloEs = "Hombro con Hombro IV b"; tituloEn = "Shoulder to Shoulder IV b"; }
      if (NIVEL == 2) { tituloPt = "Ombro a Ombro II"; }
      if (NIVEL == 3) { tituloPt = "Ombro a Ombro III"; }
      if (NIVEL == 4) { tituloPt = "Ombro a Ombro IV a"; }
      if (NIVEL == 5) { tituloPt = "Ombro a Ombro IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Support\n";
        cuerpo += "<b>Target:</b> Self and adjacent allies in your column\n";
        cuerpo += $"<b>Buff (3 turns):</b> +{bonoDefensa} Defense, +{bonoAtaque} Attack\n";
        cuerpo += "<b>Per affected ally:</b> +1 Valour\n";
        if (daInvulnerable)
        {
          cuerpo += "<b>Additional:</b> Invulnerable for 1 turn\n";
        }
        if (daApMax)
        {
          cuerpo += "<b>Additional:</b> +1 Max AP for 3 turns\n";
        }
        cuerpo += "<b>Save:</b> None";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Suporte\n";
        cuerpo += "<b>Alvo:</b> O usuario e aliados adjacentes na mesma coluna\n";
        cuerpo += $"<b>Buff (3 turnos):</b> +{bonoDefensa} Defesa, +{bonoAtaque} Ataque\n";
        cuerpo += "<b>Por aliado afetado:</b> +1 Valentia\n";
        if (daInvulnerable)
        {
          cuerpo += "<b>Adicional:</b> Invulneravel por 1 turno\n";
        }
        if (daApMax)
        {
          cuerpo += "<b>Adicional:</b> +1 AP Max por 3 turnos\n";
        }
        cuerpo += "<b>Resistencia:</b> Nao se aplica";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Soporte\n";
        cuerpo += "<b>Objetivo:</b> El usuario y aliados adyacentes en su columna\n";
        cuerpo += $"<b>Buff (3 turnos):</b> +{bonoDefensa} Defensa, +{bonoAtaque} Ataque\n";
        cuerpo += "<b>Por cada aliado afectado:</b> +1 Valentía\n";
        if (daInvulnerable)
        {
          cuerpo += "<b>Adicional:</b> Invulnerable por 1 turno\n";
        }
        if (daApMax)
        {
          cuerpo += "<b>Adicional:</b> +1 AP Max por 3 turnos\n";
        }
        cuerpo += "<b>TS:</b> No aplica";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Knight forms a compact frontline stance and boosts nearby allies."
          : esPortugues
            ? "O Cavaleiro forma uma linha compacta e fortalece aliados proximos."
          : "El Caballero forma una linea cerrada y potencia a sus aliados cercanos.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense buff.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Attack buff.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (Invulnerable 1 turn) or Option B (+1 Max AP).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no buff de Defesa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no buff de Ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (Invulneravel 1 turno) ou Opcao B (+1 AP Max).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de Defensa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de Ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (Invulnerable 1 turno) u Opcion B (+1 AP Max).</color>"; }
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
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Hombro Con Hombro";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDefensa += 2;
       buff.cantAtaque += 2;
       if(NIVEL > 1){  buff.cantDefensa += 1;}
       if(NIVEL > 2){  buff.cantAtaque += 1;}
       if(NIVEL == 4){  objetivo.estado_invulnerable += 1;}
       if(NIVEL == 5){  buff.cantAPMax += 1;}
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

       objetivo.Marcar(0);

       objetivo.SumarValentia(1);
     }
    
    
    }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_HombroConHombro");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();

      // Rellena la lista de casillas afectadas de la habilidad (no una variable local)
      lCasillasafectadas.Clear();
      lCasillasafectadas.AddRange(Origen.ObtenerCasillasAdyacentesEnColumna());
      lCasillasafectadas.Add(Origen);

      // Marca visualmente las casillas vélidas para el clic de confirmación
      foreach (Casilla cas in lCasillasafectadas)
      {
        cas.ActivarCapaColorAzul();
      }

      foreach(Casilla c in lCasillasafectadas)
      {
        
        
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }
           
           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
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









