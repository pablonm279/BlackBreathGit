using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.AssetImporters;

public class ClasePurificadora : Unidad
{
  
public int PASIVA_AuraSagrada; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
public int PASIVA_EcosDivinos; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
[SerializeField]private int Cantidad_Fervor;

public override void ComienzoBatallaClase()
{
  base.ComienzoBatallaClase();

  // Inicializa la clase purificadora
  float avanceHalitoNegro = CampaignManager.Instance.GetTierAlientoNegro();
  
  //Fervor
  Cantidad_Fervor += 1 + CampaignManager.Instance.GetEsperanzaActual()/15;
  ActualizarFervor();

 #region Debilidad Alma Endeble
  if(avanceHalitoNegro == 2)
  {
      /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Afligida I";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.cantTsMental -= 1;
        buff.cantTsFortaleza -= 1;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }
  if(avanceHalitoNegro == 3)
  {
      /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Afligida II";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.cantTsMental -= 2;
        buff.cantTsFortaleza -= 2;
        buff.cantDanioPorcentaje -= 10;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }
  if(avanceHalitoNegro == 4)
  {
      /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Afligida III";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.cantTsMental -= 3;
        buff.cantTsFortaleza -= 2;
        buff.cantDanioPorcentaje -= 15;
        buff.cantPMMax -= 1;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }
   if(avanceHalitoNegro > 4)
  {
      /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Afligida IV";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.cantTsMental -= 4;
        buff.cantTsFortaleza -= 3;
        buff.cantDanioPorcentaje -= 25;
        buff.cantPMMax -= 1;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }
  #endregion

}

public override void ActualizarClaseComienzoTurno()
{
  base.ActualizarClaseComienzoTurno();

  ActualizarFervor();

  ActualizarAuraSagrada();

  if(PASIVA_EcosDivinos > 0)
  {
    CrearTrampaEcosDivinos();    
  }

}

void ActualizarFervor()
{
   barreraDeDanio -= 1*Cantidad_Fervor;
   if(barreraDeDanio < 0){barreraDeDanio = 0;}
   
    if (TieneBuffNombre("Fervor"))
    {
      Buff[] buffs = GetComponents<Buff>();
      foreach (Buff buff in buffs)
      {
        if (buff.buffNombre.StartsWith("Fervor"))
        {
          buff.RemoverBuff(this);
        }
      }
    }

    {
      if (Cantidad_Fervor > 0) 
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = $"Fervor: {Cantidad_Fervor}";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.suprimeTextoFlotante = true;
        buff.esBuffVisibleUI = false;
        barreraDeDanio += 1 * Cantidad_Fervor;
        buff.cantDamBonusElementalDiv += 1 * Cantidad_Fervor;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
    }
}

void CrearTrampaEcosDivinos()
{
  int ladoRandom =UnityEngine.Random.Range(1, 3);
   Casilla cas = null;

  if(ladoRandom == 1) //Aliados
  {
     cas = BattleManager.Instance.ladoB.ObtenerCasillaAleatoria(true);
     cas.AddComponent<EcosDivinos>();
     cas.GetComponent<EcosDivinos>().InicializarCreador(this);
     cas.GetComponent<EcosDivinos>().NIVEL = PASIVA_EcosDivinos;
  }

  if(ladoRandom == 2) //Enemigos
  {
     cas = BattleManager.Instance.ladoA.ObtenerCasillaAleatoria(true);
     cas.AddComponent<EcosDivinos>();
     cas.GetComponent<EcosDivinos>().InicializarCreador(this);
     cas.GetComponent<EcosDivinos>().NIVEL = PASIVA_EcosDivinos;
  }
 
  
  
}
int otorgoEstoBarrerraAaliadosPorAuraSagrada = 0;
void ActualizarAuraSagrada()
{ 

    List<Unidad> listaUnidades = new List<Unidad>();
    listaUnidades = ObtenerListaAliados(false);

     foreach (Unidad unidad in listaUnidades)
     {
     //Si la unidad ya tiene el buff lo remueve, y le saca tambien la barrera de daño
       Buff[] buffs = unidad.GetComponents<Buff>();
       foreach(Buff buff2 in buffs)
       {
        if(buff2.buffNombre == "Aura Sagrada")
        { 
          unidad.barreraDeDanio -= otorgoEstoBarrerraAaliadosPorAuraSagrada;
          if(unidad.barreraDeDanio < 0){unidad.barreraDeDanio = 0;}
          
           buff2.RemoverBuff(unidad); 
        }
       }
     }



  if((PASIVA_AuraSagrada > 0 && Cantidad_Fervor > 0)||(PASIVA_AuraSagrada == 4))
  {
    
    otorgoEstoBarrerraAaliadosPorAuraSagrada = 1;
    if(PASIVA_AuraSagrada > 1){ otorgoEstoBarrerraAaliadosPorAuraSagrada += 1;}
    if(PASIVA_AuraSagrada == 5 && Cantidad_Fervor > 2){  otorgoEstoBarrerraAaliadosPorAuraSagrada += 2;}

    foreach (Unidad unidad in listaUnidades)
    {
       if(unidad == this){continue;}//no a esta unidad

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Aura Sagrada";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.suprimeTextoFlotante = true;
        buff.cantDamBonusElementalDiv += 1;
        if(PASIVA_AuraSagrada > 2){  buff.cantDamBonusElementalDiv ++;}
        if(PASIVA_AuraSagrada == 5 && Cantidad_Fervor > 2){ buff.cantDamBonusElementalDiv += 2;}
        buff.AplicarBuff(unidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);

         unidad.barreraDeDanio += otorgoEstoBarrerraAaliadosPorAuraSagrada;

      
    }


     
  }
}
public void CambiarFervor(int n)
{
  Cantidad_Fervor += n;
    if (Cantidad_Fervor < 0) { Cantidad_Fervor = 0; }
  BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);

}
public int ObtenerFervor()
{
  return Cantidad_Fervor;
}




}
