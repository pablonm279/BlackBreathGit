using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ImprovisarFlechas : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
     public override void  Awake()
    {
      nombre = "Improvisar Flechas";
      IDenClase = 3;
      costoAP = 0;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 1;
      bAfectaObstaculos = false;
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Explorador_ImprovisarFlechas");
      ActualizarDescripcion();
    }
        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int flechasFijas = (NIVEL > 1 ? 2 : 1) + (NIVEL == 4 ? 1 : 0);
      int criticoPorcentaje = (1 + (NIVEL > 2 ? 1 : 0)) * 5;
      int buffPenetracion = 1;
      int duracionBuff = 2;
      bool sumaDanioNivel5 = NIVEL == 5;

      string tituloEs = "Improvisar Flechas I";
      string tituloEn = "Improvise Arrows I";
      string tituloPt = "Improvisar Flechas I";
      if (NIVEL == 2) { tituloEs = "Improvisar Flechas II"; tituloEn = "Improvise Arrows II"; }
      if (NIVEL == 3) { tituloEs = "Improvisar Flechas III"; tituloEn = "Improvise Arrows III"; }
      if (NIVEL == 4) { tituloEs = "Improvisar Flechas IV a"; tituloEn = "Improvise Arrows IV a"; }
      if (NIVEL == 5) { tituloEs = "Improvisar Flechas IV b"; tituloEn = "Improvise Arrows IV b"; }
      if (NIVEL == 2) { tituloPt = "Improvisar Flechas II"; }
      if (NIVEL == 3) { tituloPt = "Improvisar Flechas III"; }
      if (NIVEL == 4) { tituloPt = "Improvisar Flechas IV a"; }
      if (NIVEL == 5) { tituloPt = "Improvisar Flechas IV b"; }

      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

      string flechasGanadasEn = flechasFijas > 0 ? $"current AP + {flechasFijas}" : "current AP";
      string flechasGanadasPt = flechasFijas > 0 ? $"AP atuais + {flechasFijas}" : "AP atuais";
      string flechasGanadasEs = flechasFijas > 0 ? $"AP actuales + {flechasFijas}" : "AP actuales";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Utility\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> Self\n";
        cuerpo += $"<color={colorEncabezado}><b>Arrows gained:</b></color> {flechasGanadasEn}\n";
        cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> sets current AP to 0\n";
        cuerpo += $"<color={colorEncabezado}><b>Effect ({duracionBuff} turns):</b></color> +{criticoPorcentaje}% Crit, +{buffPenetracion} Armor Penetration";
        if (sumaDanioNivel5) { cuerpo += ", +15% Damage"; }
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Utilidade\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> Si mesmo\n";
        cuerpo += $"<color={colorEncabezado}><b>Flechas ganhas:</b></color> {flechasGanadasPt}\n";
        cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> zera os AP atuais\n";
        cuerpo += $"<color={colorEncabezado}><b>Efeito ({duracionBuff} turnos):</b></color> +{criticoPorcentaje}% Critico, +{buffPenetracion} Penetracao de armadura";
        if (sumaDanioNivel5) { cuerpo += ", +15% Dano"; }
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Utilidad\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> Uno mismo\n";
        cuerpo += $"<color={colorEncabezado}><b>Flechas ganadas:</b></color> {flechasGanadasEs}\n";
        cuerpo += $"<color={colorEncabezado}><b>Al lanzarla:</b></color> deja los AP actuales en 0\n";
        cuerpo += $"<color={colorEncabezado}><b>Efecto ({duracionBuff} turnos):</b></color> +{criticoPorcentaje}% Critico, +{buffPenetracion} Penetracion de armadura";
        if (sumaDanioNivel5) { cuerpo += ", +15% Danio"; }
      }

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Convert current AP into arrows and a short attack boost."
        : esPortugues
          ? "Converte AP atuais em flechas e um impulso ofensivo curto."
          : "Convierte AP actuales en flechas y una mejora ofensiva breve.";

      txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
      txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
      txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
      txtDescripcion += cuerpo;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel) { return; }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 fixed Arrow.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Crit in the effect.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 fixed Arrow) or Option B (+15% Damage in the effect).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 flecha fixa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico no efeito.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 flecha fixa) ou Opcao B (+15% Dano no efeito).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 flecha fija.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico en el efecto.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 flecha fija) u Opcion B (+15% Danio en el efecto).</color>"; }
      }

    }
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       
        
        //AplicarEfectosHabilidad(scEstaUnidad, 0);
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
      cooldownActual = cooldownMax;
      
      
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();
       int flechasCreadas = 1;
      
        for (int veces = 0; veces < APusados; veces++)
        {
           flechasCreadas++;
        }
        if( NIVEL > 1)
        {
         flechasCreadas++;
        }
        if( NIVEL == 4)
        {
         flechasCreadas++;
        }
        
        Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(flechasCreadas);

        VFXAplicar(Usuario);
       scEstaUnidad.EstablecerAPActualA(0);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Flechas Preparadas";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantCritDado += 1;
       buff.cantPenetracionArmadura  += 1;
       if (NIVEL > 2)
    {
      buff.cantCritDado += 1;
    }
        if( NIVEL == 5)
       {
         buff.cantDanioPorcentaje += 15;
       }
       buff.AplicarBuff(scEstaUnidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
       

      

    }
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ImprovisarFlechas");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private void ObtenerObjetivos()
    {
      lObjetivosPosibles.Add(scEstaUnidad);

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }
     
    }
      
         
   

   
    

 
}





