using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class BombaDeHumo : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
     public override void  Awake()
    {
      nombre = "Bomba de Humo";
      IDenClase = 5;
      costoAP = 2;
      if(NIVEL > 2){costoAP--;}
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 5;
      if(NIVEL > 1){cooldownMax--;}
      bAfectaObstaculos = false;

      poneTrampas = true;
      


      imHab = Resources.Load<Sprite>("imHab/Acechador_BombaDeHumo");
      ActualizarDescripcion();

    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int radioZona = NIVEL == 5 ? 2 : 1;
      int duracionHumo = NIVEL == 4 ? 3 : 2;
      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

      string tituloEs = "Bomba de Humo I";
      string tituloEn = "Smoke Bomb I";
      string tituloPt = "Bomba de Fumaca I";
      if (NIVEL == 2) { tituloEs = "Bomba de Humo II"; tituloEn = "Smoke Bomb II"; }
      if (NIVEL == 3) { tituloEs = "Bomba de Humo III"; tituloEn = "Smoke Bomb III"; }
      if (NIVEL == 4) { tituloEs = "Bomba de Humo IV a"; tituloEn = "Smoke Bomb IV a"; }
      if (NIVEL == 5) { tituloEs = "Bomba de Humo IV b"; tituloEn = "Smoke Bomb IV b"; }
      if (NIVEL == 2) { tituloPt = "Bomba de Fumaca II"; }
      if (NIVEL == 3) { tituloPt = "Bomba de Fumaca III"; }
      if (NIVEL == 4) { tituloPt = "Bomba de Fumaca IV a"; }
      if (NIVEL == 5) { tituloPt = "Bomba de Fumaca IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Utility trap (4 range)\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 tile in range\n";
        cuerpo += $"<color={colorEncabezado}><b>Roll/Save:</b></color> none\n";
        cuerpo += $"<color={colorEncabezado}><b>Valour cost:</b></color> {costoPM}\n";
        cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> creates smoke traps in area radius {radioZona} around target\n";
        cuerpo += $"<color={colorEncabezado}><b>Smoke trap:</b></color> 30 uses, {duracionHumo} turns duration, persistent\n";
        cuerpo += $"<color={colorEncabezado}><b>On trigger (any unit):</b></color> grants Hidden (1) if not hidden\n";
        cuerpo += $"<color={colorEncabezado}><b>Non-Stalker units:</b></color> 2 turns, +2 Attack, +5% Crit";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Armadilha de utilidade (4 de alcance)\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 celula no alcance\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem/Resistencia:</b></color> nao tem\n";
        cuerpo += $"<color={colorEncabezado}><b>Custo Valentia:</b></color> {costoPM}\n";
        cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> cria armadilhas de fumaca em area de raio {radioZona} ao redor da celula alvo\n";
        cuerpo += $"<color={colorEncabezado}><b>Armadilha de fumaca:</b></color> 30 usos, {duracionHumo} turnos de duracao, persistente\n";
        cuerpo += $"<color={colorEncabezado}><b>Ao ativar (qualquer unidade):</b></color> concede Escondido (1) se nao estava escondido\n";
        cuerpo += $"<color={colorEncabezado}><b>Unidades que nao sao Acechador:</b></color> 2 turnos, +2 Ataque, +5% Critico";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Trampa de utilidad (4 alcance)\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 casilla en rango\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada/TS:</b></color> no tiene\n";
        cuerpo += $"<color={colorEncabezado}><b>Costo Valentia:</b></color> {costoPM}\n";
        cuerpo += $"<color={colorEncabezado}><b>Al lanzarla:</b></color> crea trampas de humo en area de radio {radioZona} alrededor de la casilla objetivo\n";
        cuerpo += $"<color={colorEncabezado}><b>Trampa de humo:</b></color> 30 usos, {duracionHumo} turnos de duracion, persistente\n";
        cuerpo += $"<color={colorEncabezado}><b>Al activar (cualquier unidad):</b></color> otorga Escondido (1) si no estaba escondido\n";
        cuerpo += $"<color={colorEncabezado}><b>Unidades no Acechador:</b></color> 2 turnos, +2 Ataque, +5% Critico";
      }

      string subtitulo = esIngles
        ? "Creates smoke traps that grant Hidden and support non-Stalker units."
        : esPortugues
          ? "Cria armadilhas de fumaca que concedem Escondido e apoiam unidades que nao sao Acechador."
          : "Crea trampas de humo que otorgan Escondido y apoyan a unidades no Acechador.";

      txtDescripcion = $"<size=115%><color={colorTitulo}><b>{(esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs)}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
      txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
      txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
      txtDescripcion += cuerpo;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 AP cost.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 turn smoke duration) or Option B (radius 2 area).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 custo AP.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 turno de duracao da fumaca) ou Opcao B (area de raio 2).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo AP.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 turno de duracion de humo) u Opcion B (area radio 2).</color>"; }
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
    if (cas == null)
    {
      if (obj is Unidad) //Acá van los efectos a Unidades.
      {
        Unidad objetivo = (Unidad)obj;
        
        cas = objetivo.GetComponent<Unidad>().CasillaPosicion; //Si no se pasa una casilla, se usa la del origen
      }
    }
      List<Casilla> casillasAlrededor = new List<Casilla>();
      int alre = 1;
      if(NIVEL == 5){alre = 2;} //Aumenta el alcance de las casillas alrededor a 2 si es nivel 5
      casillasAlrededor = cas.ObtenerCasillasAlrededor(alre);
      casillasAlrededor.Add(cas); //Agrega la casilla origen


      foreach (Casilla c in casillasAlrededor)
      {
        TrampaBombaHumo trampa = c.AddComponent<TrampaBombaHumo>();
        trampa.Inicializar(NIVEL);
        trampa.AsignarCreador(scEstaUnidad);
      }


     
     
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4); //alcance
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
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}




