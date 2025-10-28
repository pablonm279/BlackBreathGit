using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;

public class EnemigoGulekGul : Unidad
{



    public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);


        if (TieneBuffNombre("Martillo Listo"))
        {

            if (TiradaSalvacion(mod_TSFortaleza, (danio / 2) + 2))
            {
                //Salvación exitosa

                RemoverBuffNombre("Martillo Listo");
                gameObject.GetComponent<IALevantarMartillo>().hActualCooldown = 0;
                gameObject.GetComponent<IAMartilloPesado>().hActualCooldown = 1;
                BattleManager.Instance.EscribirLog("Gulek Gul pierde el buff 'Martillo Listo' tras recibir daño y no podrá utilizarlo.");

            }

        }


    }
    
    public override bool TiradaSalvacion(float atributoDefiende, float dificultadHabilidada) //TRUE no se salva FALSE se salva (xd)
  {
     bool resultado = false;
        int intentos = 1;

      if (atributoDefiende == 3) { intentos++; } //Si el atributo es 3 (Voluntad), tiene un intento adicional
     for (int i = 0; i < intentos; i++) // Permitir un intento adicional si falla
     {
        float iTiradaDefensa = UnityEngine.Random.Range(1, 21);

        float iResultadoAtaque = dificultadHabilidada;
        float iResultadoDefensa = iTiradaDefensa + atributoDefiende;

        resultado = iResultadoAtaque > iResultadoDefensa;

        if (resultado) // positivo NO se salva
        {
            BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" realiza Tirada de Salvación: 1d20 = ") + iTiradaDefensa + " +" + atributoDefiende + " vs Tirada Dificultad: " + iResultadoAtaque + ". Resultado: No se salva.");
        }
        else // Negativo Se Salva
        {
            BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" realiza Tirada de Salvación: 1d20 = ") + iTiradaDefensa + " +" + atributoDefiende + " vs Tirada Dificultad: " + iResultadoAtaque + ". Resultado: Se salva.");
            GenerarTextoFlotante(TRADU.i.Traducir("Resiste"), Color.green);
            return resultado; // Si se salva, no se realiza un segundo intento
        }

        if (intentos == 0 && atributoDefiende == 3 && resultado) // Si falla en el primer intento y atributoDefiende es 3
        {
            BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" obtiene un intento adicional de Tirada de Salvación."));
        }
     }
        if (atributoDefiende == 3 && resultado) { RemoverBuffNombre("Martillo Listo"); } //Si pierde una tirada de voluntad, deja el martillo
     return resultado;
  }
  

}


