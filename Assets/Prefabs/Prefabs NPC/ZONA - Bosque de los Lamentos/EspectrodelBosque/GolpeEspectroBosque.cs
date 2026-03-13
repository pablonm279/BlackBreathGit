using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class GolpeEspectroBosque : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro
   
  void Awake()
   {
      if (GetComponent<EspectroPlanoMaterialVisual>() == null)
      {
        gameObject.AddComponent<EspectroPlanoMaterialVisual>();
      }

      nombre = "Golpe de Espectro";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = true;
      hAlcance = 1;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = 1;
      costoAP = 3;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 2;
      XdDanio = 3;
      daniodX = 6; //3d6+4
      tipoDanio = 9; //Necro


   
    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
      hActualCooldown = hCooldownMax;
      
      AplicarPerdidaEtereo();
      //scEstaUnidad.ReproducirAnimacionAtaque();
      object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
          PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo
      await BattleManager.DelayCombateAsync(450);
      AplicarEfectosHabilidad(Objetivo);
     
   }
        void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GarraEspectro");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
    
    public override void AplicarEfectosHabilidad(object obj)
    {
     if(obj is Unidad)
     {
          Unidad objetivo = (Unidad)obj;
          float defensaObjetivo = objetivo.ObtenerdefensaActual();

          int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;
          
       

          if(resultadoTirada == -1)
          {//PIFIA 
        //    print("Pifia");
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
         
       scEstaUnidad.EstablecerAPActualA(0);
            
          }
          else if (resultadoTirada == 0)
          {//FALLO
           // print("Fallo");

            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

          }
          else if (resultadoTirada == 1)
          {//ROCE
         //   print("Roce");
            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+4;
             danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

            danio -= danio/2; //Reduce 50% por roce
          VFXAplicar(objetivo.gameObject);
            objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);
           
          }
          else if (resultadoTirada == 2)
          {//GOLPE
         //   print("Golpe");

            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
          VFXAplicar(objetivo.gameObject);

            objetivo.RecibirDanio(danio+4, tipoDanio, false,  scEstaUnidad);

          }
          else if (resultadoTirada == 3)
          {//CRITICO
          //  print("Critico");
          VFXAplicar(objetivo.gameObject);
            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

          
            
            objetivo.RecibirDanio(danio+8, tipoDanio, true, scEstaUnidad);
          
          }
          
            objetivo.AplicarDebuffPorAtaquesreiterados(1);
     }
     else if(obj is Obstaculo)
     {
          Obstaculo objetivo = (Obstaculo)obj;
          
          float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
           danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
          objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }

    }


void AplicarPerdidaEtereo()
{
 
      if(gameObject.GetComponent<Unidad>().mod_Armadura > 80)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "En plano material";
        buff.buffDescr = "El Espectro acaba de atacar, haciéndolo vulnerable en el plano material.";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantArmadura -= 100;
        buff.unidadOrigen = scEstaUnidad;
        
        buff.AplicarBuff(gameObject.GetComponent<Unidad>());
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject.GetComponent<Unidad>().gameObject);

      }

}
public GameObject VFXEstadoPrefab;

public override object EstablecerObjetivoPrioritario() 
{
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;

    // Filtrar las unidades
    var unidades = objPosibles.OfType<Unidad>().ToList();
    // Filtrar los obstáculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
    var unidadesOrdenadas = unidades
        .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
        return unidadesOrdenadas.FirstOrDefault();
    }

    // Si no hay unidades, devolver el obstáculo
    var obstaculo = obstaculos.FirstOrDefault();
    return obstaculo;
}



}

[DisallowMultipleComponent]
public class EspectroPlanoMaterialVisual : MonoBehaviour
{
    [SerializeField] private string nombreBuffPlanoMaterial = "En plano material";
    [SerializeField][Range(0f, 1f)] private float alphaEtereo = 0.48f;
    [SerializeField][Range(0f, 1f)] private float alphaPlanoMaterial = 0.92f;
    [SerializeField] private float duracionFlash = 0.08f;
    [SerializeField] private float duracionTransicion = 0.18f;

    private Unidad unidad;
    private Image imagenUnidad;
    private bool estadoInicializado;
    private bool estabaEnPlanoMaterial;
    private Coroutine rutinaTransicion;
    private Color colorBase = Color.white;

    void Awake()
    {
        VincularReferencias();
    }

    void OnEnable()
    {
        AplicarEstadoActual(true);
    }

    void Update()
    {
        AplicarEstadoActual(false);
    }

    private void VincularReferencias()
    {
        if (unidad == null)
        {
            unidad = GetComponent<Unidad>();
        }

        if (imagenUnidad == null && unidad != null)
        {
            imagenUnidad = unidad.uImage;
            if (imagenUnidad != null)
            {
                colorBase = imagenUnidad.color;
                colorBase.a = 1f;
            }
        }
    }

    private void AplicarEstadoActual(bool instantaneo)
    {
        VincularReferencias();
        if (unidad == null || imagenUnidad == null)
        {
            return;
        }

        bool enPlanoMaterial = unidad.TieneBuffNombre(nombreBuffPlanoMaterial);
        if (!estadoInicializado)
        {
            estadoInicializado = true;
            estabaEnPlanoMaterial = enPlanoMaterial;
            AplicarAlphaInstantaneo(AlphaObjetivo(enPlanoMaterial));
            return;
        }

        if (estabaEnPlanoMaterial == enPlanoMaterial)
        {
            return;
        }

        estabaEnPlanoMaterial = enPlanoMaterial;
        IniciarTransicion(instantaneo, AlphaObjetivo(enPlanoMaterial));
    }

    private float AlphaObjetivo(bool enPlanoMaterial)
    {
        return enPlanoMaterial ? alphaPlanoMaterial : alphaEtereo;
    }

    private void IniciarTransicion(bool instantaneo, float alphaObjetivo)
    {
        if (rutinaTransicion != null)
        {
            StopCoroutine(rutinaTransicion);
            rutinaTransicion = null;
        }

        if (instantaneo || !gameObject.activeInHierarchy)
        {
            AplicarAlphaInstantaneo(alphaObjetivo);
            return;
        }

        rutinaTransicion = StartCoroutine(AnimarCambio(alphaObjetivo));
    }

    private IEnumerator AnimarCambio(float alphaObjetivo)
    {
        float alphaInicial = imagenUnidad.color.a;
        float alphaFlash = 1f;

        if (duracionFlash > 0f)
        {
            float t = 0f;
            while (t < duracionFlash)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duracionFlash);
                AplicarAlphaInstantaneo(Mathf.Lerp(alphaInicial, alphaFlash, k));
                yield return null;
            }
        }

        if (duracionTransicion > 0f)
        {
            float t = 0f;
            while (t < duracionTransicion)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duracionTransicion);
                AplicarAlphaInstantaneo(Mathf.Lerp(alphaFlash, alphaObjetivo, k));
                yield return null;
            }
        }

        AplicarAlphaInstantaneo(alphaObjetivo);
        rutinaTransicion = null;
    }

    private void AplicarAlphaInstantaneo(float alpha)
    {
        if (imagenUnidad == null)
        {
            return;
        }

        Color color = colorBase;
        color.a = Mathf.Clamp01(alpha);
        imagenUnidad.color = color;
    }
}

  
 
  


