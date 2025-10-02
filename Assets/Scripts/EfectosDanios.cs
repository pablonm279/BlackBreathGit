using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EfectosDanios : MonoBehaviour
{
   
  public static float CriticoCortante(float danio, Unidad objetivo)
  {
      
        float final = danio - objetivo.ObtenerArmaduraActual();

        int sangrado = (int)(danio/4);
        Estados.Aplicar_Sangrado(objetivo, sangrado);

        return final;
  }

  public static float CriticoPerforante(float danio, Unidad objetivo)
  {
        float final = danio; //No se resta armadura.
        return final;
  }

  public static float CriticoContundente(float danio, Unidad objetivo) 
  {
        
        objetivo.estado_APModificador -=1;
        float final = danio - objetivo.ObtenerArmaduraActual();
        return final;
  }

  public static float CriticoFuego(float danio, Unidad objetivo) 
  {
        float final = danio - objetivo.ObtenerResistenciaA(1);
        
        int stacks = (int)(final/3);
        Estados.Aplicar_Ardiendo(objetivo, stacks);
        
        return final;
  }

  public static float CriticoHielo(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(2);
        
        int stacks = (int)(final/4);
        Estados.Aplicar_Congelado(objetivo, stacks);

        return final;
  }

  public static float CriticoRayo(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(3);
         
        Estados.Aplicar_Aturdido(objetivo, 1);

        return final;
  }

  public static float CriticoAcido(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(4);
        
       Estados.Aplicar_Acido(objetivo, (int)(final/5));

        return final;
  }
  public static float CriticoArcano(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(5);
        
         objetivo.estado_ResistenciasReducidas += 1;

        return final;
  }
  public static float CriticoNecro(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(6);
        
         if(UnityEngine.Random.Range(0,100) < (final/2)) //0.5% chances de matar por cada daño.
         {
            final = objetivo.mod_maxHP+1;        
         }

        return final;
  }

   public static float CriticoDivino(float danio, Unidad objetivo) 
  {
        
        float final = danio - objetivo.ObtenerResistenciaA(7);
        if(final > 0 && objetivo.TiradaSalvacion(objetivo.mod_TSMental,17)) 
        {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Condenado";
            buff.boolfDebufftBuff = false;
            buff.DuracionBuffRondas = -1;
            buff.cantAtaque -= 1;
            buff.cantResDiv -= 5;
            buff.AplicarBuff(objetivo);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);        
        }
         

        return final;
  }

 
}
