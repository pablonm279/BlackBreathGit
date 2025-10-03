using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDePoder : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga De Poder";
      IDenClase = 2;
      costoAP = 4;
      if (NIVEL == 4) { costoAP--; }

      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      if (NIVEL == 5) { cooldownMax--; }
      bAfectaObstaculos = true;

      targetEspecial = 8; //T    

      bonusAtaque = 0;
      if (NIVEL > 2) { bonusAtaque += 1; }
      XdDanio = 3;
      daniodX = 6; //3d6
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDePoder");
      

      
    }
   
    public override void ActualizarDescripcion()
    {
     if(NIVEL < 2)
{
  txtDescripcion = "<color=#5dade2><b>Descarga de Poder I</b></color>\n\n"; 
  txtDescripcion += "<i>(Rango) Concentrando su energía, lanza una descarga arrolladora que impacta en un área en forma de T.</i>\n\n";
  txtDescripcion += $"<color=#c8c8c8><b>Área:</b> T (3 horizontal + 2 en vertical al final)\n";
  txtDescripcion += $"- Ataque: <color=#ea0606>Poder</color>\n";
  txtDescripcion += $"- Daño: 3d6+Poder Arcano\n";
  txtDescripcion += $"- +2 Rango Crítico</color>\n\n";
  txtDescripcion += $"<color=#44d3ec>-AP: 4  -Val: 0  -Esforzable: Sí\n-Enfriamiento: 4</color>\n\n";

  if (EsEscenaCampaña())
  {
    if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
    {
      if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +3 Daño</color>\n\n";
      }
    }
  }
}

if(NIVEL == 2)
{
  txtDescripcion = "<color=#5dade2><b>Descarga de Poder II</b></color>\n\n"; 
  txtDescripcion += "<i>(Rango) Concentrando su energía, lanza una descarga arrolladora que impacta en un área en forma de T.</i>\n\n";
  txtDescripcion += $"<color=#c8c8c8><b>Área:</b> T (3 horizontal + 2 en vertical al final)\n";
  txtDescripcion += $"- Ataque: <color=#ea0606>Poder</color>\n";
  txtDescripcion += $"- Daño: 3d6+3+Poder Arcano\n";
  txtDescripcion += $"- +2 Rango Crítico</color>\n\n";
  txtDescripcion += $"<color=#44d3ec>-AP: 4  -Val: 0  -Esforzable: Sí\n-Enfriamiento: 4</color>\n\n";

  if (EsEscenaCampaña())
  {
    if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
    {
      if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
      }
    }
  }
}

if(NIVEL == 3)
{
  txtDescripcion = "<color=#5dade2><b>Descarga de Poder III</b></color>\n\n"; 
  txtDescripcion += "<i>(Rango) Concentrando su energía, lanza una descarga arrolladora que impacta en un área en forma de T.</i>\n\n";
  txtDescripcion += $"<color=#c8c8c8><b>Área:</b> T (3 horizontal + 2 en vertical al final)\n";
  txtDescripcion += $"- Ataque: <color=#ea0606>Poder +1</color>\n";
  txtDescripcion += $"- Daño: 3d6+3+Poder Arcano\n";
  txtDescripcion += $"- +2 Rango Crítico</color>\n\n";
  txtDescripcion += $"<color=#44d3ec>-AP: 4  -Val: 0  -Esforzable: Sí\n-Enfriamiento: 4</color>\n\n";

  if (EsEscenaCampaña())
  {
    if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
    {
      if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        txtDescripcion += $"<color=#dfea02>-Opción A: -1 AP</color>\n";
        txtDescripcion += $"<color=#dfea02>-Opción B: -1 Enfriamiento</color>\n";
      }
    }
  }
}

if(NIVEL == 4)
{
  txtDescripcion = "<color=#5dade2><b>Descarga de Poder IV a</b></color>\n\n"; 
  txtDescripcion += "<i>(Rango) Concentrando su energía, lanza una descarga arrolladora que impacta en un área en forma de T.</i>\n\n";
  txtDescripcion += $"<color=#c8c8c8><b>Área:</b> T (3 horizontal + 2 en vertical al final)\n";
  txtDescripcion += $"- Ataque: <color=#ea0606>Poder +1</color>\n";
  txtDescripcion += $"- Daño: 3d6+3+Poder Arcano\n";
  txtDescripcion += $"- +2 Rango Crítico</color>\n\n";
  txtDescripcion += $"<color=#44d3ec>-AP: 3  -Val: 0  -Esforzable: Sí\n-Enfriamiento: 4</color>\n\n";
}

if(NIVEL == 5)
{
  txtDescripcion = "<color=#5dade2><b>Descarga de Poder IV b</b></color>\n\n"; 
  txtDescripcion += "<i>(Rango) Concentrando su energía, lanza una descarga arrolladora que impacta en un área en forma de T.</i>\n\n";
  txtDescripcion += $"<color=#c8c8c8><b>Área:</b> T (3 horizontal + 2 en vertical al final)\n";
  txtDescripcion += $"- Ataque: <color=#ea0606>Poder +1</color>\n";
  txtDescripcion += $"- Daño: 3d6+3+Poder Arcano\n";
  txtDescripcion += $"- +2 Rango Crítico</color>\n\n";
  txtDescripcion += $"<color=#44d3ec>-AP: 4  -Val: 0  -Esforzable: Sí\n-Enfriamiento: 3</color>\n\n";
}

    }

    Casilla Origen;
    private Task preImpactoPendiente;

    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        Casilla referencia = casillaOrigenTrampas;

        if (objetivos != null && objetivos.Count > 0)
        {
            if (objetivos[0] is Unidad unidadObjetivo)
            {
                referencia = unidadObjetivo.CasillaPosicion;
            }
            else if (objetivos[0] is Obstaculo obstaculoObjetivo)
            {
                referencia = obstaculoObjetivo.CasillaPosicion;
            }
        }

        if (referencia == null && BattleManager.Instance.casillaClickHabilidad != null)
        {
            referencia = BattleManager.Instance.casillaClickHabilidad;
        }

        if (referencia == null)
        {
            referencia = Origen;
        }

        preImpactoPendiente = CrearProyectilFila(referencia);
        return preImpactoPendiente ?? Task.CompletedTask;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;
       
       int danioExtra = 0;
       if (NIVEL > 1) { danioExtra += 3; }

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);
       
      
       //----
     
       if(resultadoTirada == -1)
       {//PIFIA 
         print("Pifia");
         objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
         //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
       }
       else if (resultadoTirada == 0)
       {//FALLO
         print("Fallo");
         objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

       }
       else if (resultadoTirada == 1)
       {//ROCE
         print("Roce");
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    
        

       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioExtra+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioExtra);
         if(NIVEL > 1){danio += 2;}
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
  
  
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
    private Task CrearProyectilFila(Casilla casillaClick)
    {
        if (casillaClick == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilFilaAsync(casillaClick);
    }

    private async Task LanzarProyectilFilaAsync(Casilla casillaClick)
    {
        await Task.Delay(10);

        int filaY = casillaClick.posY;
        int ladoRef = casillaClick.lado;
        List<Casilla> filaFull = new List<Casilla>();
        foreach (var c in BattleManager.Instance.lCasillasTotal)
        {
            if (c.lado == ladoRef && c.posY == filaY)
            {
                filaFull.Add(c);
            }
        }
        if (filaFull.Count == 0)
        {
            return;
        }

        Casilla startCas = null;
        foreach (var c in filaFull)
        {
            if (c.posX == 3)
            {
                startCas = c;
                break;
            }
        }
        if (startCas == null)
        {
            foreach (var c in filaFull)
            {
                if (startCas == null || c.posX > startCas.posX)
                {
                    startCas = c;
                }
            }
        }

        Casilla endCas = startCas;
        foreach (var c in filaFull)
        {
            if (c.posX < endCas.posX)
            {
                endCas = c;
            }
        }

        Vector3 dir = (endCas.transform.position - startCas.transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector3.right;
        }

        float offsetBehind = 2.2f;
        Vector3 spawnPos = startCas.transform.position - dir * offsetBehind;

        GameObject vfxPrefab = BattleManager.Instance.contenedorPrefabs.VFXDescargaDePoder_Fila;
        if (vfxPrefab == null)
        {
            return;
        }

        Quaternion rotacion = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
        GameObject vfx = Instantiate(vfxPrefab, spawnPos, rotacion);
        FlechaPotenteVuelo vuelo = vfx.GetComponent<FlechaPotenteVuelo>();
        if (vuelo != null)
        {
            vuelo.Configure(dir);
            await vuelo.EsperarFinalAsync();
        }
        else
        {
            await Task.Delay(400);
        }
    }

private void ObtenerObjetivos()
    {
      
     //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,0);
    
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







