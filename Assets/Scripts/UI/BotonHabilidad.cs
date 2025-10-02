using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NUnit.Framework.Internal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;

public class BotonHabilidad : MonoBehaviour
{
   
    public Habilidad HabilidadRepresentada;
    public Image HabilidadCooldownMuestra;


    [SerializeField]private bool BotonActivo = false;
    [SerializeField]private GameObject goDesc;
    [SerializeField]private TextMeshProUGUI txtDescHab;


    

    public UIBotonesHabilidades scUiBotonesHabilidades;


    TextMeshProUGUI nombreHabilidad;
    private void Awake()
    {
        scUiBotonesHabilidades = transform.parent.GetComponent<UIBotonesHabilidades>();
        nombreHabilidad = transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {

        gameObject.GetComponent<Image>().sprite = HabilidadRepresentada.imHab;    




    }

   public void hoverDescripcion(int n)
   {
    if (n == 1)
    {
        HabilidadRepresentada.ActualizarDescripcion();
        txtDescHab.text = HabilidadRepresentada.txtDescripcion;
        goDesc.SetActive(true);

        // Asegurarnos de que el goDesc (RectTransform) no salga de los márgenes de la pantalla
        RectTransform descRect = goDesc.GetComponent<RectTransform>();

        // Obtener las dimensiones de la pantalla
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Obtener las coordenadas de la imagen en pantalla (anclada a su pivote)
        Vector3[] corners = new Vector3[4];
        descRect.GetWorldCorners(corners);

        // Comprobar si alguna esquina está fuera de los márgenes de la pantalla
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = corners[i];

            // Si alguna parte de la descripción está fuera del lado izquierdo de la pantalla
            if (corner.x < 0)
            {
                descRect.position += new Vector3(-corner.x, 0, 0); // Ajustar al margen izquierdo
            }

            // Si alguna parte de la descripción está fuera del lado derecho de la pantalla
            if (corner.x > screenSize.x)
            {
                descRect.position += new Vector3(screenSize.x - corner.x, 0, 0); // Ajustar al margen derecho
            }

            // Si alguna parte de la descripción está fuera de la parte inferior de la pantalla
            if (corner.y < 0)
            {
                descRect.position += new Vector3(0, -corner.y, 0); // Ajustar al margen inferior
            }

            // Si alguna parte de la descripción está fuera de la parte superior de la pantalla
            if (corner.y > screenSize.y)
            {
                descRect.position += new Vector3(0, screenSize.y - corner.y, 0); // Ajustar al margen superior
            }
        }
    }
    else
    {
        goDesc.SetActive(false);
    }
   }


    public void ActivarHabilidad(bool yaVienedeCargando)
    { BattleManager.Instance.bOcupado = false;
      if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
      {return;} // Sale del método si la escena no es "ES-Batallas"
      
      if(HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
      {return;} //Si se clickea el boton de una pasiva, no pasa nada

      if(HabilidadRepresentada.cooldownActual >0)
      {return;} //Control extra para que no se puedan activar habilidades en cooldown

        if (HabilidadRepresentada.requiereRecurso > 0) //de las habilidades que requieran un recurso, se fija una por una si lo cumplen
        {
            if (HabilidadRepresentada.nombre == "Tiro con Arco")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().Cantidad_flechas < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Tiro Potente")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().Cantidad_flechas < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Vigilancia")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().Cantidad_flechas < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Enmendar")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClasePurificadora>().ObtenerFervor() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Asesinar")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseAcechador>().ObtenerEstaEscondido() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Descarga Desintegradora")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            if (HabilidadRepresentada.nombre == "Manifestacion Arcana")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { return; }
            }
            
      }
      
       if(!yaVienedeCargando)
       {
        if(ChequearCargaHabilidad())
        { return; }
       }


        BattleManager.Instance.LimpiarCapasCasillas();
        BattleManager.Instance.scUIContadorAP.ResetearCirculos();

       
        int esfuerzo;
        if(yaVienedeCargando)
        {
         ActivarBoton(0);   
       
        
        }
        else if(BotonActivo == false && HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
          ActivarBoton(esfuerzo);  
        } 
        else if(HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
          DesactivarBoton();
        }
      
       
    }
 

    public void UpdateCooldownMuestra()
    {
        
        
        if(HabilidadRepresentada != null)
        {
            
            if(HabilidadRepresentada.cooldownMax > 0 && HabilidadRepresentada.cooldownActual > 0)
            {
                float nMax = HabilidadRepresentada.cooldownMax;
                float nCurr = HabilidadRepresentada.cooldownActual;
                float fillRes = nCurr/nMax;

   
                HabilidadCooldownMuestra.fillAmount =fillRes ;
               
            }
            else
            {
                HabilidadCooldownMuestra.fillAmount = 0;
            }



        }        
    }
    
    void ActivarBoton(int iEsfuerzo)
    {      
         
           int esfuerzo = iEsfuerzo;

           scUiBotonesHabilidades.UIDesactivarHabilidades(); 
           HabilidadRepresentada.Activar();
           VisualBotonActivo(1);
          // BattleManager.Instance.OpacarCasillasMelee();
           BotonActivo = true;
           if( BattleManager.Instance.unidadActiva.valorCargando > 0)
           {
              BattleManager.Instance.scUIContadorAP.MarcarCirculos(BattleManager.Instance.unidadActiva.valorCargando);
           }
           else
           {
             BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)HabilidadRepresentada.costoAP);
           }
           BattleManager.Instance.scUIContadorAP.SeEsforzaría(esfuerzo);

          

    }

    public void DesactivarBoton()
    {
       if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
       {return;} // Sale del método si la escena no es "ES-Batallas"

        int esfuerzo = 0;
        if(HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
           VisualBotonActivo(0);
           BotonActivo = false;
           BattleManager.Instance.LimpiarCapasCasillas();
           BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
           BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
           BattleManager.Instance.SeleccionandoObjetivo = false;
           BattleManager.Instance.HabilidadActiva = null;

           BattleManager.Instance.scUIContadorAP.ResetearCirculos();

        
        }
    }





     public void DesactivarHabilidad()
    {   
        BattleManager.Instance.DesmarcarTodasLasUnidades();
        if(BotonActivo == true)
        {VisualBotonActivo(0);  }     
        BotonActivo = false;
    }

   
    private void Update()
    {
        if(HabilidadRepresentada != null)
        {
         nombreHabilidad.text = HabilidadRepresentada.nombre;

         if(HabilidadRepresentada.cooldownActual > 0){nombreHabilidad.text += " "+HabilidadRepresentada.cooldownActual+" T";}

        }
    }

    private  void VisualBotonActivo(int estado)
    {
       if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
      {return;} // Sale del método si la escena no es "ES-Batallas"

      
        if(estado == 1)
        {
            if(BotonActivo == false)
            {
                if(HabilidadRepresentada.esHostil)
                    {
                        BattleManager.Instance.TiltearCamaraLadoEnemigo(true);
                    }
                gameObject.GetComponent<RectTransform>().localScale += new Vector3(0.1f, 0.1f, 0.1f);
            }

        }
        else if(estado == 0)
        {
 
            if(BotonActivo == true)
            { BattleManager.Instance.DesmarcarTodasLasUnidades();
                if(HabilidadRepresentada.esHostil)
                    {
                        BattleManager.Instance.TiltearCamaraLadoEnemigo(false);
                    }
                gameObject.GetComponent<RectTransform>().localScale -= new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }

    bool ChequearCargaHabilidad()
    {
         Unidad uActiva = BattleManager.Instance.unidadActiva;

        if(uActiva.ObtenerAPActual() < HabilidadRepresentada.costoAP && HabilidadRepresentada.esCargable)
        {
           uActiva.estaCargando = HabilidadRepresentada;
           uActiva.valorCargando = (int)(HabilidadRepresentada.costoAP - uActiva.ObtenerAPActual());

           BattleManager.Instance.TerminarTurno();

         
       
         return true;
        }
        else
        {
         return false;
        }
    }


   public MenuPersonajes scMenuPersonajes;
   public void SubirHabDeNivel(int n)
   {    
    scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
    if(n == 0) //0 - Subida Normal
    {
     scMenuPersonajes.pSel.NivelPuntoHabilidad--;
     HabilidadRepresentada.NIVEL++; 
     scMenuPersonajes.ActualizarInfo();
    }
     if(n == 4) //4 - Subida a 4a
    {scMenuPersonajes.pSel.NivelPuntoHabilidad--;
     HabilidadRepresentada.NIVEL = 4;
     scMenuPersonajes.ActualizarInfo();
     }
      if(n == 5) //5 - Subida a 4b
    {scMenuPersonajes.pSel.NivelPuntoHabilidad--;
     HabilidadRepresentada.NIVEL = 5;
     scMenuPersonajes.ActualizarInfo();
     }
   }


   public void AgregarComoHabilidadNueva()
   {  scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
        // Verificamos si la habilidad representada no es nula
        if (HabilidadRepresentada != null)
        {
            // Obtenemos el tipo de la habilidad representada
            System.Type tipoHabilidad = HabilidadRepresentada.GetType();

            // Agregamos el componente del tipo de la habilidad al objeto scMenuPersonajes.pSel
            Habilidad nuevaHabilidad = (Habilidad)scMenuPersonajes.pSel.gameObject.AddComponent(tipoHabilidad);


            switch (nuevaHabilidad.IDenClase)
            {
                case 1: scMenuPersonajes.pSel.Habilidad_1 = 1; break;
                case 2: scMenuPersonajes.pSel.Habilidad_2 = 1; break;
                case 3: scMenuPersonajes.pSel.Habilidad_3 = 1; break;
                case 4: scMenuPersonajes.pSel.Habilidad_4 = 1; break;
                case 5: scMenuPersonajes.pSel.Habilidad_5 = 1; break;
                case 6: scMenuPersonajes.pSel.Habilidad_6 = 1; break;
                case 7: scMenuPersonajes.pSel.Habilidad_7 = 1; break;
                case 8: scMenuPersonajes.pSel.Habilidad_8 = 1; break;
                case 9: scMenuPersonajes.pSel.Habilidad_9 = 1; break;
                case 10: scMenuPersonajes.pSel.Habilidad_10 = 1; break;
            }
            nuevaHabilidad.NIVEL = 1;

            // Actualizamos la información y reducimos el nivel de la nueva habilidad base
            scMenuPersonajes.pSel.NivelNuevaHabilidadBase--;
            scMenuPersonajes.yaTiroHabRand = false;
            scMenuPersonajes.ActualizarInfo();
            scMenuPersonajes.LimpiarComponentesHab();
   }
   }
}
