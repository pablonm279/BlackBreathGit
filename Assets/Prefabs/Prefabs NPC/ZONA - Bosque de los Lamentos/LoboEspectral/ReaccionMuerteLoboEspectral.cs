using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class ReaccionMuerteLoboEspectral : Reaccion
{
   Cortevertical corteVertical;
   void Start()
   {

    TipoTrigger =3; //Muerte
    usos = 1;
   
    permanente = true;

    nombre = "Muerte de manada";
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    corteVertical =  gameObject.GetComponent<Cortevertical>();
    

    descripcion = $"Reacción: Al morir, enfurecerá a otros Lobos Espectrales.";

   }

   public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
   {
     
       
        
        await Task.Delay(400);
        
        BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacciona con {nombre}.");

        List<Unidad> unidades = BattleManager.Instance.lUnidadesTotal;
        foreach(Unidad unidad in unidades )
        {
          if(unidad.CasillaPosicion.lado == 2)
          {
            continue;
          }

           if(unidad.GetComponent<ReaccionMuerteLoboEspectral>() != null)
           {
              /////////////////////////////////////////////
              //BUFF ---- Así se aplica un buff/debuff
              Buff buff = new Buff();
              buff.buffNombre = "Furia";
              buff.boolfDebufftBuff = true;
              buff.DuracionBuffRondas = -1;
              buff.cantDanioPorcentaje += 15;
              buff.cantAtaque += 1;
              buff.cantDefensa -= 1;
              buff.AplicarBuff(unidad);
              // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
              Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);

           }


        }
      
        //--------------------------
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      
   }


}
