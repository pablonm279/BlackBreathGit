using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IALenguetazo : IAHabilidad
{

    [SerializeField] public int pPrioridad = 15;
    [SerializeField] private int bonusAtaque = 3;
    [SerializeField] private float danioBase = 5f;
    [SerializeField] private int tipoDanio = 3; // 3: Contundente
    [SerializeField] private float vfxDuracion = 0.35f;

    void Awake()
    {
        nombre = "Saborear";
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        hAncho = 1;
        esMelee = false;
        hAlcance = 3;
        hCooldownMax = 3;
        hActualCooldown = 0;
        esHostil = true;
        prioridad = pPrioridad;
        costoAP = 1;
        afectaObstaculos = false;
    }

    void Start()
    {
        prioridad = pPrioridad;
    }

    public async override Task ActivarHabilidad()
    {
        scEstaUnidad.CambiarAPActual(-costoAP);

        object objetivo = EstablecerObjetivoPrioritario();

        if (objetivo is Unidad unidadObjetivo)
        {
            VFXAplicar(unidadObjetivo.gameObject);
        }
      
        PrepararInicioAnimacion(null, objetivo);//Despues de establecer objetivo
    
        scEstaUnidad.ReproducirAnimacionAtaque();
        await Task.Delay(2000);

        AplicarEfectosHabilidad(objetivo);
        hActualCooldown = hCooldownMax;
    }

      void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_LenguetazoZarkilever");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

    public override void AplicarEfectosHabilidad(object obj)
    {
        if (obj is Unidad objetivo)
        {
            float defensaObjetivo = objetivo.ObtenerdefensaActual();
            int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo);

            if (resultadoTirada <= 0)
            {
                objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
                if (resultadoTirada == -1)
                {
                    scEstaUnidad.EstablecerAPActualA(0);
                }
                return;
            }

            float danio = danioBase;
            danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);

            bool esCritico = resultadoTirada == 3;

            if (resultadoTirada == 1)
            {
                danio *= 0.5f; // Roce
            }

            objetivo.RecibirDanio(danio, tipoDanio, esCritico, scEstaUnidad);

            if (objetivo.HP_actual > 0)
            {
                AplicarDebuffSaboreado(objetivo);
            }

            objetivo.AplicarDebuffPorAtaquesreiterados(1);
        }
        else if (obj is Obstaculo obstaculo)
        {
            float danio = danioBase;
            danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);
            obstaculo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        }
    }

    private void AplicarDebuffSaboreado(Unidad objetivo)
    {
        if (objetivo == null)
        {
            return;
        }

        objetivo.RemoverBuffNombre("Saboreado");

        Buff saboreado = new Buff
        {
            buffNombre = "Saboreado",
            buffDescr = "",
            boolfDebufftBuff = false,
            DuracionBuffRondas = 3,
            cantDefensa = -1,
            cantTsMental = -2,
            esBuffVisibleUI = true,
            esRemovible = true,
            esStackeable = false
        };

        saboreado.AplicarBuff(objetivo, scEstaUnidad, true);
        ComponentCopier.CopyComponent(saboreado, objetivo.gameObject);
    }

  /*  private void DesplegarLenguaVfx(Transform objetivo)
    {
        if (objetivo == null)
        {
            return;
        }

        GameObject vfx = new GameObject("VFX_Lenguetazo");
        LineRenderer line = vfx.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.numCapVertices = 5;
        line.numCornerVertices = 5;
        line.startWidth = 0.08f;
        line.endWidth = 0.12f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.7f, 0.1f, 0.08f, 0.9f);
        line.endColor = new Color(0.6f, 0.05f, 0.05f, 0.35f);
        line.sortingOrder = 5;

        Vector3 origen = scEstaUnidad.transform.position + new Vector3(0f, 0.9f, 0f);
        Vector3 destino = objetivo.position + new Vector3(0f, 0.6f, 0f);

        line.SetPosition(0, origen);
        line.SetPosition(1, destino);

        Destroy(vfx, vfxDuracion);
    }*/

    public override object EstablecerObjetivoPrioritario()
    {
        Unidad unidadDuena = gameObject.GetComponent<Unidad>();
        if (unidadDuena == null)
        {
            return null;
        }

        var unidades = objPosibles.OfType<Unidad>()
            .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
            .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuena.CasillaPosicion.posY))
            .ToList();

        if (unidades.Any())
        {
            return unidades.First();
        }

        return objPosibles.OfType<Obstaculo>().FirstOrDefault();
    }
}
