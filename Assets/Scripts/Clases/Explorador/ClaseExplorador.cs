using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ClaseExplorador : Unidad
{
  
public int Cantidad_flechas;

 public int PASIVA_VistaLejana; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
 public int PASIVA_Acrobatico; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
 public int PASIVA_Reconocimiento; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b





public override void ComienzoBatallaClase()
{
  base.ComienzoBatallaClase();



  
   Cantidad_flechas = 7;

  
  #region Pasiva_Acrobatico
   if (PASIVA_Acrobatico == 1)
   {
     estado_evasion = 1;
     
   }
   else if (PASIVA_Acrobatico == 2)
   {
     estado_evasion = 1;
     mod_TSReflejos++;
   }
   else if (PASIVA_Acrobatico == 3)
   {
     estado_evasion = 2;
     mod_TSReflejos++;
   }
   else if (PASIVA_Acrobatico == 4)
   {
     estado_evasion = 3;
     mod_TSReflejos++;
   }
   else if (PASIVA_Acrobatico == 5)
   {
     estado_evasion = 3;
     mod_TSReflejos++;
   }
#endregion
  
#region Pasiva_Reconocimiento
   if (PASIVA_Reconocimiento > 0)
   {
     List<Unidad> unidadesAliadas = ObtenerListaAliados(true); 

     Invoke("delayRefuerzos", 0.3f);
 

          
     foreach (Unidad objetivo in unidadesAliadas)
     {
      
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Reconocimiento";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAPMax += 1;

        if(PASIVA_Reconocimiento > 1){ buff.cantIniciativa += 1;}
        if(PASIVA_Reconocimiento > 2){ buff.cantIniciativa += 1;}
        if(PASIVA_Reconocimiento == 4){buff.cantAPMax += 1;}
       
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

     }
   }
  
#endregion
}

void delayRefuerzos()
{
  BattleManager.Instance.delayRefuerzo += 1; 
   if(PASIVA_Reconocimiento == 5)
   {
    BattleManager.Instance.delayRefuerzo += 1;
   }

   BattleManager.Instance.ActualizarRefuerzosUI(); 
}
public override void AcabaDeHacerDañoA(Unidad victima)
{
   base.AcabaDeHacerDañoA(victima);
 
   
   
    //Acechar
    //Al dañar a una unidad se pierde buff de 1 solo ataque de "Acechar"
    RemoverBuffAcecharAlAtacar();
}

void RemoverBuffAcecharAlAtacar()
{
    //Acechar
    //Al dañar a una unidad se pierde buff que es por 1 solo ataque de "Acechar"
    if(gameObject.GetComponent<Acechar>() != null)
    {
      if(gameObject.GetComponent<Acechar>().NIVEL != 5) //Si es nivel 5 no se remueve
      {
        Buff[] buffs = GetComponents<Buff>();
        // Recorre cada buff y realiza una acción
        foreach (Buff buff in buffs)
        {
            if (buff.buffNombre == "Acechando")
            {
            buff.RemoverBuff(this);
            }
        }
      }
    }

}
public override void AcabaDeMatarUnidad(Unidad uVictima)
{
  // Llamar al método base
  base.AcabaDeMatarUnidad(uVictima);

  int tieneMarcarPresa = ChequearTieneMarcarPresa(uVictima); //0 no tiene, + de 0, es el NIVEL de la marca
  if(tieneMarcarPresa > 0 && tieneMarcarPresa < 5)
  {
      SumarValentia(1);
      /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Presa Completada";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantAPMax += 1;
       buff.cantTsMental += 2;
       buff.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }
  if(tieneMarcarPresa == 5)
  {
      SumarValentia(2);
      /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Presa Completada";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantAPMax += 2;
       buff.cantTsMental += 3;
       buff.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
  }



}
 int ChequearTieneMarcarPresa(Unidad obj)
 { 
      if(obj.GetComponent<MarcaMarcarPresa>() != null)
      {
        if(obj.GetComponent<MarcaMarcarPresa>().quienMarco == this)
        {
          return obj.GetComponent<MarcaMarcarPresa>().NIVEL;
        }
      
      }
      return 0;
  }



public void CambiarCantidadFlechas(int n)
{
    Cantidad_flechas += n;
}

  public override void ActualizarClaseComienzoTurno()
  {
    if (gameObject.GetComponent<REPRESENTACIONPasoCauteloso>() != null)
    {
      gameObject.GetComponent<REPRESENTACIONPasoCauteloso>().seusoEsteTurno = false; //Reactiva el uso de la pasiva Paso Cauteloso
    }

    Invoke("PasivaVistaLejana", 0.25f); //Chequea si tiene la pasiva de Vista Lejana

    //Chequea Tiene Fogata cerca
    ChequeartieneFogataCerca();

  }

  public void PasivaVistaLejana()
  {
    //PASIVA VISTA LEJANA
    if (CasillaPosicion.posX == 1 && !((TieneBuffNombre("Vista Lejana I") || TieneBuffNombre("Vista Lejana II") || TieneBuffNombre("Vista Lejana III") || TieneBuffNombre("Vista Lejana IVa") || TieneBuffNombre("Vista Lejana IVb"))))
    {
      if (PASIVA_VistaLejana == 1)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Vista Lejana I";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 1;
        buff.cantDanioPorcentaje = 10;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
      if (PASIVA_VistaLejana == 2)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Vista Lejana II";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 1;
        buff.cantDanioPorcentaje = 15;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
      if (PASIVA_VistaLejana == 3)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Vista Lejana III";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 2;
        buff.cantDanioPorcentaje = 15;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
      if (PASIVA_VistaLejana == 4)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Vista Lejana IVa";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 2;
        buff.cantDanioPorcentaje = 15;
        buff.cantCritDado = 1;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
      if (PASIVA_VistaLejana == 5)
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Vista Lejana IVb";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 2;
        buff.cantDanioPorcentaje = 15;
        buff.cantDefensa = 1;
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
      }
    }
    else if (CasillaPosicion.posX > 1)
    {
      RemoverBuffNombre("Vista Lejana I");
      RemoverBuffNombre("Vista Lejana II");
      RemoverBuffNombre("Vista Lejana III");
      RemoverBuffNombre("Vista Lejana IVa");
      RemoverBuffNombre("Vista Lejana IVb");
    }

    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
  }

  public void ChequeartieneFogataCerca()
  {
    bool tieneFogataCerca = false;

    foreach (Casilla c in CasillaPosicion.ObtenerCasillasAlrededor(1))
    {
      if (c.GetComponent<TrampaFogata>() != null)
      {
        tieneFogataCerca = true;
      }
    }

    if (tieneFogataCerca)
    {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Flechas de Fuego";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 1;
      buff.cantDamBonusElementalFue += 6; //+1d6
      if (gameObject.GetComponent<Fogata>() != null)
      {
        if (gameObject.GetComponent<Fogata>().NIVEL == 4)
        {
          buff.cantDamBonusElementalFue += 3; //+1d9
        }

      }
      buff.AplicarBuff(this);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);
    }
    else
    {
      Buff[] buffs = gameObject.GetComponents<Buff>();
      // Recorre cada buff y realiza una acción
      foreach (Buff buff in buffs)
      {
        if (buff.buffNombre == "Flechas de Fuego")
        {
          buff.RemoverBuff(this);
        }
      }
    }
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);

  }

  public override void ChequearSeMovio()
  {
    base.ChequearSeMovio();

    ChequeartieneFogataCerca();

    PasivaVistaLejana();

  }

  public void PrepararFlechasDelay()
  {
    Invoke("Agregar3FlechasDelay", 0.5f);
  }
  
  void Agregar3FlechasDelay()
  {
    Cantidad_flechas += 3;
  }
  
}
