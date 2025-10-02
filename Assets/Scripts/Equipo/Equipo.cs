using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Equipo : MonoBehaviour
{
  
  [SerializeField] Image imItemArma;
  [SerializeField] Image imItemArmadura;
  [SerializeField] Image imItemAccesorio1;
  [SerializeField] Image imItemAccesorio2;
  [SerializeField] Image imItemConsumible1;
  [SerializeField] Image imItemConsumible2;
  [SerializeField] Sprite vacio;
 

  public List<GameObject> listInventario = new List<GameObject>();
 


 public GameObject goInventario;
 public GameObject prefabNtnInventario; 
 public Transform listaItems;



 public int accesorioACambiar;
 public int consumibleACambiar;

 public void MostrarInventario(int tipo) //1 Armas
 {

    foreach (Transform transform in listaItems)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
            Destroy(transform.gameObject);
    }
   
   
    goInventario.SetActive(true);

    if(tipo==1)
    {
        foreach (GameObject goItem in listInventario)
        {
            if(goItem.GetComponent<Arma>()!= null)
            {
               
                GameObject btnItem =  Instantiate(prefabNtnInventario,listaItems);
                btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();

                scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Arma>().imItem;
                scBtnItem.itemRepresentado = goItem.GetComponent<Arma>();

            }

            
        }

    }
     if(tipo==2)
    {
        foreach (GameObject goItem in listInventario)
        {
            if(goItem.GetComponent<Armadura>()!= null)
            {
                GameObject btnItem =  Instantiate(prefabNtnInventario,listaItems);
                btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();

                scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Armadura>().imItem;
                scBtnItem.itemRepresentado = goItem.GetComponent<Armadura>();

            }

            
        }

    }
    if(tipo==3)
    {
        foreach (GameObject goItem in listInventario)
        {
            if(goItem.GetComponent<Accesorio>()!= null)
            {
                GameObject btnItem =  Instantiate(prefabNtnInventario,listaItems);
                btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();

                scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Accesorio>().imItem;
                scBtnItem.itemRepresentado = goItem.GetComponent<Accesorio>();

            }

            
        }

    }
    if(tipo==4)
    {
        foreach (GameObject goItem in listInventario)
        {
            if(goItem.GetComponent<Consumible>()!= null)
            {
                GameObject btnItem =  Instantiate(prefabNtnInventario,listaItems);
                btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();

                scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Consumible>().imItem;
                scBtnItem.itemRepresentado = goItem.GetComponent<Consumible>();

            }

            
        }

    }
    if(tipo==5) //todos
    {
        foreach (GameObject goItem in listInventario)
        {
            
                GameObject btnItem =  Instantiate(prefabNtnInventario,listaItems);
                btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();

                scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Item>().imItem;
                scBtnItem.itemRepresentado = goItem.GetComponent<Item>();

           
            
        }

    }

 }

 public void CerrarInventario()
 {
    goInventario.SetActive(false);

 }
 

  public void ActualizarEquipo(Personaje scPerssel)
  {
    ResetearBuffs();


   #region //ARMAS
   if(scPerssel.itemArma != null)
   {
    imItemArma.sprite = scPerssel.itemArma.imItem;
    //Aplica a buff total los buffs del arma
    BuffTOTALEQUIPOFuerza += scPerssel.itemArma.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.itemArma.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.itemArma.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.itemArma.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.itemArma.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.itemArma.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.itemArma.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.itemArma.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.itemArma.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.itemArma.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.itemArma.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.itemArma.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.itemArma.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.itemArma.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.itemArma.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.itemArma.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.itemArma.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.itemArma.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.itemArma.buffResDivino;
 
 
//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades
if (scPerssel.itemArma.habilidadAtaque != null) // Agrega la habilidad del arma
{   
   
    // Obtener el tipo de la habilidad de ataque
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadAtaque.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}
if (scPerssel.itemArma.habilidadExtra1 != null) // Agrega la habilidad extra 1 del arma
{ 
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}
if (scPerssel.itemArma.habilidadExtra2 != null) // Agrega la habilidad extra 2 del arma
{   
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}


   }else{ imItemArma.sprite = vacio;}
 #endregion

   #region //ARMADURAS
   if(scPerssel.itemArmadura != null)
   {
    imItemArmadura.sprite = scPerssel.itemArmadura.imItem;
    //Aplica a buff total los buffs del Armadura
    BuffTOTALEQUIPOFuerza += scPerssel.itemArmadura.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.itemArmadura.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.itemArmadura.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.itemArmadura.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.itemArmadura.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.itemArmadura.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.itemArmadura.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.itemArmadura.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.itemArmadura.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.itemArmadura.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.itemArmadura.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.itemArmadura.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.itemArmadura.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.itemArmadura.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.itemArmadura.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.itemArmadura.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.itemArmadura.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.itemArmadura.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.itemArmadura.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades
if (scPerssel.itemArmadura.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Armadura
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.itemArmadura.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArmadura;
    }
}
if (scPerssel.itemArmadura.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Armadura
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.itemArmadura.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArmadura;
    }
}


   }else{ imItemArmadura.sprite = vacio;}
 #endregion

   #region //Accesorio1
   if(scPerssel.Accesorio1 != null)
   {
    imItemAccesorio1.sprite = scPerssel.Accesorio1.imItem;
    //Aplica a buff total los buffs del Accesorio1
    BuffTOTALEQUIPOFuerza += scPerssel.Accesorio1.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.Accesorio1.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.Accesorio1.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.Accesorio1.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.Accesorio1.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.Accesorio1.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.Accesorio1.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.Accesorio1.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.Accesorio1.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.Accesorio1.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.Accesorio1.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.Accesorio1.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.Accesorio1.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.Accesorio1.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.Accesorio1.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.Accesorio1.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.Accesorio1.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.Accesorio1.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.Accesorio1.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades

if (scPerssel.Accesorio1.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Accesorio1
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.Accesorio1.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio1;
    }
}
if (scPerssel.Accesorio1.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Accesorio1
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.Accesorio1.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio1;
    }
}


   }else{ imItemAccesorio1.sprite = vacio;}
 #endregion

   #region //Accesorio2
   if(scPerssel.Accesorio2 != null)
   {
    imItemAccesorio2.sprite = scPerssel.Accesorio2.imItem;
    //Aplica a buff total los buffs del Accesorio2
    BuffTOTALEQUIPOFuerza += scPerssel.Accesorio2.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.Accesorio2.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.Accesorio2.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.Accesorio2.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.Accesorio2.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.Accesorio2.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.Accesorio2.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.Accesorio2.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.Accesorio2.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.Accesorio2.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.Accesorio2.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.Accesorio2.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.Accesorio2.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.Accesorio2.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.Accesorio2.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.Accesorio2.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.Accesorio2.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.Accesorio2.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.Accesorio2.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades

if (scPerssel.Accesorio2.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Accesorio2
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.Accesorio2.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio2;
    }
}
if (scPerssel.Accesorio2.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Accesorio2
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.Accesorio2.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio2;
    }
}


   }else{ imItemAccesorio2.sprite = vacio;}
 #endregion

   #region //Consumible1
   if(scPerssel.Consumible1 != null)
   {
     imItemConsumible1.sprite = scPerssel.Consumible1.imItem;
   }else{ imItemConsumible1.sprite = vacio;}
   #endregion

   #region //Consumible2
   if(scPerssel.Consumible2 != null)
   {
     imItemConsumible2.sprite = scPerssel.Consumible2.imItem;
   }else{ imItemConsumible2.sprite = vacio;}
   #endregion
   
  }


  void ResetearBuffs()
  {

        BuffTOTALEQUIPOFuerza = 0;
        BuffTOTALEQUIPOAgi = 0;
        BuffTOTALEQUIPOPoder = 0;
        BuffTOTALEQUIPOIniciativa = 0;
        BuffTOTALEQUIPOApMax = 0;
        BuffTOTALEQUIPOhpMax = 0;
        BuffTOTALEQUIPOValMax = 0;
        BuffTOTALEQUIPOArmadura = 0;
        BuffTOTALEQUIPODefensa = 0;
        BuffTOTALEQUIPOTSReflejo = 0;
        BuffTOTALEQUIPOTSFortaleza = 0;
        BuffTOTALEQUIPOTSMental = 0;
        BuffTOTALEQUIPOResFuego = 0;
        BuffTOTALEQUIPOResRayo = 0;
        BuffTOTALEQUIPOResHielo = 0;
        BuffTOTALEQUIPOResArcano = 0;
        BuffTOTALEQUIPOResAcido = 0;
        BuffTOTALEQUIPOResNecro = 0;
        BuffTOTALEQUIPOResDivino = 0;


  }
 //BuffTOTALEQUIPOs
    public int BuffTOTALEQUIPOFuerza;
    public int BuffTOTALEQUIPOAgi;
    public int BuffTOTALEQUIPOPoder;
    public int BuffTOTALEQUIPOIniciativa;
    public int BuffTOTALEQUIPOApMax;
    public int BuffTOTALEQUIPOhpMax;
    public int BuffTOTALEQUIPOValMax;
    public int BuffTOTALEQUIPOArmadura;
    public int BuffTOTALEQUIPODefensa;
    public int BuffTOTALEQUIPOTSReflejo;
    public int BuffTOTALEQUIPOTSFortaleza;
    public int BuffTOTALEQUIPOTSMental;
    public int BuffTOTALEQUIPOResFuego;
    public int BuffTOTALEQUIPOResRayo;
    public int BuffTOTALEQUIPOResHielo;
    public int BuffTOTALEQUIPOResArcano;
    public int BuffTOTALEQUIPOResAcido;
    public int BuffTOTALEQUIPOResNecro;
    public int BuffTOTALEQUIPOResDivino;


}

