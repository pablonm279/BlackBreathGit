using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ArrojarAbrojos : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Arrojar Abrojos";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      bAfectaObstaculos = false;
      poneTrampas = true;
      poneObstaculo = false;
      
      targetEspecial = 7;
      esDiscreta = true; //No quita sigilo
     
      
      imHab = Resources.Load<Sprite>("imHab/Acechador_ArrojarAbrojos");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int dcBase = NIVEL > 2 ? 12 : 11;
      int bleedAplicado = 4 + (NIVEL == 4 ? 1 : 0);
      bool drenaAp = NIVEL == 5;
      string danioBase = NIVEL > 1 ? FormatearRangoDados(1, 12, 1) : FormatearRangoDados(1, 12);
      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

      string tituloEs = "Arrojar Abrojos I";
      string tituloEn = "Throw Caltrops I";
      string tituloPt = "Lancar Abrolhos I";
      if (NIVEL == 2) { tituloEs = "Arrojar Abrojos II"; tituloEn = "Throw Caltrops II"; }
      if (NIVEL == 3) { tituloEs = "Arrojar Abrojos III"; tituloEn = "Throw Caltrops III"; }
      if (NIVEL == 4) { tituloEs = "Arrojar Abrojos IV a"; tituloEn = "Throw Caltrops IV a"; }
      if (NIVEL == 5) { tituloEs = "Arrojar Abrojos IV b"; tituloEn = "Throw Caltrops IV b"; }
      if (NIVEL == 2) { tituloPt = "Lancar Abrolhos II"; }
      if (NIVEL == 3) { tituloPt = "Lancar Abrolhos III"; }
      if (NIVEL == 4) { tituloPt = "Lancar Abrolhos IV a"; }
      if (NIVEL == 5) { tituloPt = "Lancar Abrolhos IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged trap (3 range)\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> empty diagonals around the target\n";
        cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> places caltrop traps\n";
        cuerpo += $"<color={colorEncabezado}><b>Trap:</b></color> 1 use, 10 turns duration\n";
        cuerpo += $"<color={colorEncabezado}><b>Trigger damage:</b></color> {danioBase}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
        cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Reflex vs DC {dcBase}\n";
        cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> x2 damage, +{bleedAplicado} Bleed";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += $"\n<color={colorEncabezado}><b>Stealth:</b></color> Discreet; does not reveal the caster";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Armadilha a distancia (3 de alcance)\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> diagonais vazias ao redor do alvo\n";
        cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> coloca armadilhas de abrolhos\n";
        cuerpo += $"<color={colorEncabezado}><b>Armadilha:</b></color> 1 uso, 10 turnos de duracao\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano ao ativar:</b></color> {danioBase}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
        cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Reflexos vs CD {dcBase}\n";
        cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> x2 dano, +{bleedAplicado} Sangramento";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += $"\n<color={colorEncabezado}><b>Furtividade:</b></color> Discreta; nao revela o lancador";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Trampa a distancia (3 alcance)\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> diagonales vacias alrededor del objetivo\n";
        cuerpo += $"<color={colorEncabezado}><b>Al lanzarla:</b></color> coloca trampas de abrojos\n";
        cuerpo += $"<color={colorEncabezado}><b>Trampa:</b></color> 1 uso, 10 turnos de duración\n";
        cuerpo += $"<color={colorEncabezado}><b>Daño al activar:</b></color> {danioBase}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
        cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Reflejos vs DC {dcBase}\n";
        cuerpo += $"<color={colorEncabezado}><b>Si falla TS:</b></color> x2 daño, +{bleedAplicado} Sangrado";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += $"\n<color={colorEncabezado}><b>Sigilo:</b></color> Discreta; no revela al lanzador";
      }

      string subtitulo = esIngles
        ? "Places caltrops that punish movement."
        : esPortugues
          ? "Coloca abrolhos que punem movimento."
          : "Coloca abrojos que castigan movimiento.";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 trap damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Bleed) or Option B (-1 AP on failed save).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 dano da armadilha.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 na CD base da resistencia.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 Sangramento) ou Opcao B (-1 AP ao falhar na resistencia).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 daño de trampa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al DC base de TS.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+1 Sangrado) u Opción B (-1 AP al fallar TS).</color>"; }
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
    
    public override async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
    {
      if (!EsCasillaObjetivoValida(casillaOrigenTrampas))
      {
        return;
      }

      await base.Resolver(Objetivos, casillaOrigenTrampas);
    }

    private bool EsCasillaObjetivoValida(Casilla cas)
    {
      Unidad unidadObjetivo = cas != null && cas.Presente != null ? cas.Presente.GetComponent<Unidad>() : null;
      return unidadObjetivo != null && lObjetivosPosibles.Contains(unidadObjetivo);
    }
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
      
      List<Casilla> CasillasXcas = new List<Casilla>();

    if (!EsCasillaObjetivoValida(cas))
    {
      return;
    }

    foreach (Casilla c in BattleManager.Instance.lCasillasTotal)
    {

      if (c != null && cas != null)
      {
        if (c.Presente != null)
        {
          continue; // Si la casilla  tiene presente, no la agregamos
        }
        if (c != cas && (cas.lado == c.lado))
        {

          if (c.posX + 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

          if (c.posX - 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

        }
      }
    }

    foreach (Casilla c in CasillasXcas)
      {
        c.AddComponent<Abrojo>();
        c.GetComponent<Abrojo>().InicializarCreador(scEstaUnidad, NIVEL);

      }
    


       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
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
      
     //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(3,2);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
       c.ActivarCapaColorRojo();
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
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
    
         
    }

   
    

 
}




