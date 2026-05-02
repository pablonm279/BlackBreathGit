using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSobrecarga : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Canalizador_Sobrecarga");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

  
    public override void  ActualizarDescripcion()
    {

      
         txtDescripcion = "<color=#5dade2><b>Sobrecarga</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al final de cada combate, el personaje recibe un 15% de su Vida Máxima como daño por cada Nivel de Energía acumulada. Este daño no puede ser Mortal, pero incrementará el tiempo de recuperación del Personaje en Campaña.</i>\n\n";

          if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
          {
              txtDescripcion = "<color=#5dade2><b>Overload</b></color>\n\n";
              txtDescripcion += "<i>(Passive) At the end of each combat, the character receives 15% of their Maximum Health as damage for each accumulated Energy Level. This damage cannot be fatal, but will increase the character's recovery time in Campaign.</i>\n\n";
          }
          else if (TRADU.i.nIdioma == 3)
          {
              txtDescripcion = "<color=#5dade2><b>Sobrecarga</b></color>\n\n";
              txtDescripcion += "<i>(Passiva) No fim de cada combate, o personagem recebe 15% da Vida Maxima como dano para cada Nivel de Energia acumulado. Esse dano nao pode ser fatal, mas aumenta o tempo de recuperacao do personagem na Campanha.</i>\n\n";
          }

          bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
          bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
          string colorEncabezado = "#44d3ec";
          string colorValor = "#ffffff";
          string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
          string titulo = esIngles ? "Overload" : "Sobrecarga";
          string subtitulo = esIngles
            ? "Campaign drawback based on Energy left at combat end."
            : esPortugues
              ? "Custo de campanha baseado na Energia restante ao fim do combate."
              : "Costo de campaña segun la Energia restante al final del combate.";
          string cuerpoFormato = "";
          if (esIngles)
          {
            cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Passive drawback</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Trigger:</b></color> <color={colorValor}>End of combat</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Effect:</b></color> <color={colorValor}>Takes 15% Max HP damage per {iconoEnergia} Energy Tier</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Limit:</b></color> <color={colorValor}>Cannot be fatal; increases campaign recovery time</color>";
          }
          else if (esPortugues)
          {
            cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Passiva negativa</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Ativa:</b></color> <color={colorValor}>Fim do combate</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Efeito:</b></color> <color={colorValor}>Recebe 15% da Vida Maxima por {iconoEnergia} Nivel de Energia</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Limite:</b></color> <color={colorValor}>Nao pode matar; aumenta o tempo de recuperacao na campanha</color>";
          }
          else
          {
            cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Pasiva negativa</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Activa:</b></color> <color={colorValor}>Final del combate</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Efecto:</b></color> <color={colorValor}>Recibe 15% de Vida Maxima por {iconoEnergia} Nivel de Energia</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Limite:</b></color> <color={colorValor}>No puede matar; aumenta el tiempo de recuperacion en campaña</color>";
          }

          txtDescripcion =
            $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n" +
            $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
            "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
            cuerpoFormato;

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



