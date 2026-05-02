using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Fogata : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Fogata";
      IDenClase = 8;
      costoAP = 5;
      if(NIVEL > 2){costoAP--;}
      if(NIVEL == 5){costoAP--;}
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      bAfectaObstaculos = false;
      poneTrampas = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Explorador_Fogata");
      ActualizarDescripcion();
    }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int trampaUsos = NIVEL > 1 ? 4 : 3;
    string danoTrampa = FormatearRangoDados(1, 3);
    int duracionTrampaTurnos = NIVEL > 1 ? 5 : 4;
    string danoFuego = NIVEL == 4 ? FormatearRangoDados(1, 9) : FormatearRangoDados(1, 6);

    string tituloEs = "Fogata I";
    string tituloEn = "Campfire I";
    string tituloPt = "Fogueira I";
    if (NIVEL == 2) { tituloEs = "Fogata II"; tituloEn = "Campfire II"; }
    if (NIVEL == 3) { tituloEs = "Fogata III"; tituloEn = "Campfire III"; }
    if (NIVEL == 4) { tituloEs = "Fogata IV a"; tituloEn = "Campfire IV a"; }
    if (NIVEL == 5) { tituloEs = "Fogata IV b"; tituloEn = "Campfire IV b"; }
    if (NIVEL == 2) { tituloPt = "Fogueira II"; }
    if (NIVEL == 3) { tituloPt = "Fogueira III"; }
    if (NIVEL == 4) { tituloPt = "Fogueira IV a"; }
    if (NIVEL == 5) { tituloPt = "Fogueira IV b"; }

    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Utility trap\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> Adjacent tile, including own tile\n";
      cuerpo += $"<color={colorEncabezado}><b>Campfire profile:</b></color> {trampaUsos} uses, {duracionTrampaTurnos} turns duration, persistent\n";
      cuerpo += $"<color={colorEncabezado}><b>On trap trigger:</b></color> {danoTrampa} fire damage\n";
      cuerpo += $"<color={colorEncabezado}><b>Adjacent allies:</b></color> attacks gain +{danoFuego} fire damage\n";
      cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Armadilha de utilidade\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> Casa adjacente, incluindo a propria casa\n";
      cuerpo += $"<color={colorEncabezado}><b>Perfil da fogueira:</b></color> {trampaUsos} usos, {duracionTrampaTurnos} turnos de duracao, persistente\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao ativar:</b></color> {danoTrampa} dano de fogo\n";
      cuerpo += $"<color={colorEncabezado}><b>Aliados adjacentes:</b></color> ataques ganham +{danoFuego} dano de fogo\n";
      cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Trampa de utilidad\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> Casilla adyacente, incluida tu propia casilla\n";
      cuerpo += $"<color={colorEncabezado}><b>Perfil de fogata:</b></color> {trampaUsos} usos, {duracionTrampaTurnos} turnos de duracion, persistente\n";
      cuerpo += $"<color={colorEncabezado}><b>Al activarse:</b></color> {danoTrampa} danio de fuego\n";
      cuerpo += $"<color={colorEncabezado}><b>Aliados adyacentes:</b></color> ataques ganan +{danoFuego} danio de fuego\n";
      cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Places a campfire trap and empowers nearby attacks."
      : esPortugues
        ? "Coloca uma fogueira e fortalece ataques proximos."
        : "Coloca una fogata y potencia ataques cercanos.";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel) { return; }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 trap use and +1 trap duration turn.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 AP cost.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1-3 fire damage bonus) or Option B (-1 AP cost).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso de armadilha e +1 turno de duracao.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 custo AP.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1-3 dano de fogo bonus) ou Opcao B (-1 custo AP).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso de trampa y +1 turno de duracion.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo AP.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1-3 danio de fuego bonus) u Opcion B (-1 costo AP).</color>"; }
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
        ClaseExplorador clas = (ClaseExplorador)scEstaUnidad;
        clas.ChequeartieneFogataCerca();
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
       TrampaFogata trampa = cas.AddComponent<TrampaFogata>();
       trampa.Inicializar(NIVEL);
       trampa.AsignarCreador(scEstaUnidad);
     
   
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
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




