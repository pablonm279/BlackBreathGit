using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour
{
   
    public float fVidaActual;
    public float fVidaMaxima;
    public string sNombre;

    public int IDClase; //1: Caballero - 2: Explorador - 3: Purificadora  -  4: Acechador

    public float fExperienciaActual;
    public float fNivelActual;

    public int iPuestoDeseado = 1; //1: Primera Columna - 2: Segunda Columna - 3: Tercera Columna

    public Sprite spRetrato;
    public int idRetrato;
   
    public int iFuerza;
    public int iAgi;
    public int iPoder;
    public int iIniciativa;
    public int iApMax;
    public int iValMax;
    public int iArmadura;
    public int iDefensa;
    public int iTSReflejo;
    public int iTSFortaleza;
    public int iTSMental;
    public int iResFuego;
    public int iResRayo;
    public int iResHielo;
    public int iResArcano;
    public int iResAcido;
    public int iResNecro;
    public int iResDivino;
    public float fCritRango;
    public float fCritDanio;
    public float fBonusAtaque;

    //Habilidades - Aca se aclara que Habilidades de la clase tiene 0 no 1 si NO EL NIVEL
    //(ver REPRESENTACIONCorajeInquebrantalbe, para que las PASIVAS aparezcan en lista habilidades)
    public int Habilidad_1;
    public int Habilidad_2;
    public int Habilidad_3;
    public int Habilidad_4;
    public int Habilidad_5;
    public int Habilidad_6;
    public int Habilidad_7;
    public int Habilidad_8;
    public int Habilidad_9;
    public int Habilidad_10;

    public int Actividad_1;
    public int Actividad_2;
    public int Actividad_3; 
    public int ActividadSeleccionada;

    public int NivelPuntoAtributo;
    public int NivelPuntoTS;
    public int NivelPuntoHabilidad;
    public int NivelNuevaHabilidadBase;
   

    //Inventario
    public Arma itemArma;
    public Armadura itemArmadura;
    public Accesorio Accesorio1;
    public Accesorio Accesorio2;
    public Consumible Consumible1;
    public Consumible Consumible2;


    //Estados de Campaña AGREGAR EN MenuPersonaje  "//Estados Campaña" para que se vean
    public bool Camp_Fatigado;
    public bool Camp_Bendecido_SequitoClerigos;
    public bool Camp_Herido;
    public int Camp_Enfermo; //es int porque al descender a 0 se va, -1 por viaje.
    public int Camp_Moral; //positiva buena, negativa mala tiende a cero cada dia

    public bool Camp_Muerto;
    public bool Camp_Corrupto;
   


  
    public int[] aRasgos = new int[300]; 


    public void QuitarArma(Arma iArma)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] scripts = GetComponents<Habilidad>();

    // Recorre todos los scripts
    foreach (Habilidad script in scripts)
    {
        // Si la ahbilidad la agregó el arma que saca, quita la habilidad
        if (script.agregaDesdeArmaUI == iArma)
        {
            Destroy(script);
        }
    }
        itemArma = null;
    }

    public void QuitarArmadura(Armadura iArma)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

    // Recorre todos los scripts
    foreach (Habilidad script in hab)
    {
        // Si la ahbilidad la agregó el arma que saca, quita la habilidad
        if (script.agregaDesdeArmaUI== iArma)
        {
            Destroy(script);
        }
    }
        itemArmadura = null;
    }

    public void QuitarAccesorio1(Accesorio iAccesorio)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iAccesorio)
            {
                Destroy(script);
            }
        }
            Accesorio1 = null;
    }

    public void QuitarAccesorio2(Accesorio iAccesorio)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iAccesorio)
            {
                Destroy(script);
            }
        }
            Accesorio2 = null;
    }

    public void QuitarConsumible1(Consumible iCons)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iCons)
            {
                Destroy(script);
            }
        }
            Consumible1 = null;
    }
    public void QuitarConsumible2(Consumible iCons)
    {
      // Obtiene todos los scripts (componentes que heredan de MonoBehaviour) del GameObject
    Habilidad[] hab = GetComponents<Habilidad>();

        // Recorre todos los scripts
        foreach (Habilidad script in hab)
        {
            // Si la ahbilidad la agregó el arma que saca, quita la habilidad
            if (script.agregaDesdeArmaUI== iCons)
            {
                Destroy(script);
            }
        }
            Consumible2 = null;
    }
    
    public void RecibirCuracion(float cant)
    {
        if(Camp_Herido)//Si esta herido, se cura la mitad
        { 
          cant = cant /2;
        }
        
        fVidaActual += cant;
        if(fVidaActual >= fVidaMaxima)
        {
            fVidaActual = fVidaMaxima;
            Camp_Herido = false; //si se sana del todo, se remueve la herida
        }
    }

    public void RecibirExperiencia(float cant)
    {
       fExperienciaActual += cant;
       
        float ExperienciaNecesaria = 100 + (fNivelActual*50); //100 + (nivel actual *50)
        if (fExperienciaActual >= ExperienciaNecesaria)
        {
          fExperienciaActual -= ExperienciaNecesaria;
          fNivelActual++;



          float cantHPMensaje;
          string nvMensaje = "";
      if (TRADU.i.nIdioma == 1)
      {
        nvMensaje = $"{sNombre} ha subido a Nivel {fNivelActual} y obtuvo: ";
        switch (fNivelActual)
        {

          case 2: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Punto Atributo"; break;
          case 3: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Habilidad Nueva  +1 AP Máximo"; break;
          case 4: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +2 Puntos de Habilidad +1 Punto Salvación"; break;
          case 5: cantHPMensaje = fVidaMaxima / 15; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Punto Atributo"; break;
          case 6: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Habilidad Nueva"; break;
          case 7: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 AP Máximo"; break;
          case 8: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +2 Puntos de Habilidad +1 Punto Salvación"; break;
          case 9: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Punto Atributo"; break;
          case 10: cantHPMensaje = fVidaMaxima / 15; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Habilidad Definitiva"; break;
          case 11: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad +1 Punto Salvación"; break;
          case 12: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +2 Puntos de Habilidad +1 Punto Atributo"; break;
          case 13: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Vida +1 Punto de Habilidad"; break;

        }
      }
      else if (TRADU.i.nIdioma == 2)
      {

        nvMensaje = $"{sNombre} is now level {fNivelActual} and obtained: ";
        switch (fNivelActual)
        {
          case 2: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Attribute Point"; break;
          case 3: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 New Skill +1 Max AP"; break;
          case 4: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +2 Skill Points +1 Saving Throw Point"; break;
          case 5: cantHPMensaje = fVidaMaxima / 15; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Attribute Point"; break;
          case 6: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 New Skill"; break;
          case 7: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Max AP"; break;
          case 8: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +2 Skill Points +1 Saving Throw Point"; break;
          case 9: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Attribute Point"; break;
          case 10: cantHPMensaje = fVidaMaxima / 15; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Ultimate Skill"; break;
          case 11: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point +1 Saving Throw Point"; break;
          case 12: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +2 Skill Points +1 Attribute Point"; break;
          case 13: cantHPMensaje = fVidaMaxima / 10; nvMensaje += $"{(int)cantHPMensaje} Health +1 Skill Point"; break;
        }


        }
          CampaignManager.Instance.EscribirLog("<Color=#F0CC39><b>"+nvMensaje+"</b></color>");
      }



      if (fNivelActual == 2)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;
        NivelPuntoHabilidad++;
        NivelPuntoAtributo++;
      }
      else if (fNivelActual == 3)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad++;
        NivelNuevaHabilidadBase++;
        iApMax++;

      }
      else if (fNivelActual == 4)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 2;
        NivelPuntoTS++;

      }
      else if (fNivelActual == 5)
      {
        //+15% HP
        float cantHP = fVidaMaxima / 15;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
        NivelPuntoAtributo++;

      }
      else if (fNivelActual == 6)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
        NivelNuevaHabilidadBase++;

      }
      else if (fNivelActual == 7)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
        iApMax++;

      }
      else if (fNivelActual == 8)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 2;
        NivelPuntoTS++;

      }
      else if (fNivelActual == 9)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
        NivelPuntoAtributo++;

      }
      else if (fNivelActual == 10)
      {
        //+15% HP
        float cantHP = fVidaMaxima / 15;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;


        int rand =UnityEngine.Random.Range(1, 3);   //agregar HABILIDAD DEFINITIVA SEGUN CLASE
        if (IDClase == 1) //Caballero
        {
          if (rand == 1) { gameObject.AddComponent<REPRESENTACIONImplacable>(); gameObject.GetComponent<REPRESENTACIONImplacable>().NIVEL = 1; }
          else { gameObject.AddComponent<HombroConHombro>(); gameObject.GetComponent<HombroConHombro>().NIVEL = 1; }
        }
        if (IDClase == 2) //Explorador
        {
          if (rand == 1) { gameObject.AddComponent<REPRESENTACIONReconocimiento>(); gameObject.GetComponent<REPRESENTACIONReconocimiento>().NIVEL = 1; }
          else { gameObject.AddComponent<Rafaga>(); gameObject.GetComponent<Rafaga>().NIVEL = 1; }
        }
        if (IDClase == 3) //Purificadora
        {
          if (rand == 1) { gameObject.AddComponent<Purificacion>(); gameObject.GetComponent<Purificacion>().NIVEL = 1; }
          else { gameObject.AddComponent<EscudodeFe>(); gameObject.GetComponent<EscudodeFe>().NIVEL = 1; }
        }
        if (IDClase == 4) //Acechador
        {
          if (rand == 1) { gameObject.AddComponent<HaciaLasSombras>(); gameObject.GetComponent<HaciaLasSombras>().NIVEL = 1; }
          else { gameObject.AddComponent<REPRESENTACIONMasacre>(); gameObject.GetComponent<REPRESENTACIONMasacre>().NIVEL = 1; }
        }
        if (IDClase == 5) //Canalizador
        {
          if (rand == 1) { gameObject.AddComponent<DescargaDesintegradora>(); gameObject.GetComponent<DescargaDesintegradora>().NIVEL = 1; }
          else { gameObject.AddComponent<ManifestacionArcana>(); gameObject.GetComponent<ManifestacionArcana>().NIVEL = 1; }
        }      
            
          }
      else if (fNivelActual == 11)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
        NivelPuntoTS++;

      }
      else if (fNivelActual == 12)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 2;
        NivelPuntoAtributo++;

      }
      else if (fNivelActual == 13)
      {
        //+10% HP
        float cantHP = fVidaMaxima / 10;
        fVidaMaxima += cantHP;
        fVidaActual += cantHP;

        NivelPuntoHabilidad += 1;
          
      }
   }
}










