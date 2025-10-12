using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuBatallas : MonoBehaviour
{
  
 
 public GameObject prefabBtnPersonaje; 
 public GameObject contenedorUIPersonajes; 
 public AdministradorEscenas scAdministradorEscenas;

 public GameObject btnComenzar;


  public void ActualizarLista()
 {
    int seleccionados = 0;
    if (scAdministradorEscenas.Personaje1 != null) seleccionados++;
    if (scAdministradorEscenas.Personaje2 != null) seleccionados++;
    if (scAdministradorEscenas.Personaje3 != null) seleccionados++;
    if (scAdministradorEscenas.Personaje4 != null) seleccionados++;

    // Asumiendo que tienes un TextMeshProUGUI llamado txtSeleccionadosPersonajes
    if (txtSeleccionadosPersonajes != null)
    {
        txtSeleccionadosPersonajes.text = $"{seleccionados}/4";
    }
    // Verifica si todos los personajes son null
    bool todosVacios = scAdministradorEscenas.Personaje1 == null &&
                       scAdministradorEscenas.Personaje2 == null &&
                       scAdministradorEscenas.Personaje3 == null &&
                       scAdministradorEscenas.Personaje4 == null;

    // Configura el estado del botón basándose en si hay personajes seleccionados o no
    btnComenzar.SetActive(!todosVacios);

    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
            Destroy(transform.gameObject);
    }

        if (!UIEmpezarBatallaACaravana.activeInHierarchy)
        {
            foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (!pers.Camp_Muerto)
                {
                    GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
                    btnPers.GetComponent<Image>().sprite = pers.spRetrato;
                    btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
                }

            }
        }
        else //Si es ataque a la caravana, solo muestra los personajes haciendo guardia
        {
            foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (!pers.Camp_Muerto && pers.fVidaActual > 1) //Si no está muerto y tiene vida
                {
                    if ((/*Base: Guardia*/pers.ActividadSeleccionada == 3) || (/*Caballero: Vigilar*/pers.ActividadSeleccionada == 6))
                    {
                        GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
                        btnPers.GetComponent<Image>().sprite = pers.spRetrato;
                        btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
                    }
                   
                }
            }
        }
       
   foreach (Transform child in contenedorUIPersonajes.transform)
        {
            // Intenta obtener el componente btnPersonaje del hijo
            btnPersonaje btn = child.GetComponent<btnPersonaje>();

            if (btn != null) // Asegúrate de que el componente btnPersonaje exista
            {
                btn.representarVida();

                //Marca los retratos de los personajes seleccionados
                if (scAdministradorEscenas.Personaje1 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(0).gameObject.SetActive(true);

                }
                else if (scAdministradorEscenas.Personaje2 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(0).gameObject.SetActive(true);
                }
                else if (scAdministradorEscenas.Personaje3 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(0).gameObject.SetActive(true);
                }
                else if (scAdministradorEscenas.Personaje4 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(0).gameObject.SetActive(true);
                }
                else { btn.transform.GetChild(0).gameObject.SetActive(false); }

            }


           btn.RepresentarIconos();


        }
   
   


 }
 
  public void DejanEnListaParticipantesSolo()
  {
     
    foreach (Transform boton in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
           btnPersonaje btn = boton.gameObject.GetComponent<btnPersonaje>();
           
            bool participo = false;

            if(btn.personajeRepresentado == scAdministradorEscenas.Personaje1)
            {participo = true;}
            if(btn.personajeRepresentado == scAdministradorEscenas.Personaje2)
            {participo = true;}
             if(btn.personajeRepresentado == scAdministradorEscenas.Personaje3)
            {participo = true;}
             if(btn.personajeRepresentado == scAdministradorEscenas.Personaje4)
            {participo = true;}
            
            if(!participo)
            {
             Destroy(boton.gameObject);
            }else{btn.representarVida();}
    }

  }

 public void Seleccionar(Personaje pers)
 {
     // Verificar si el personaje ya está seleccionado en alguna posición
    if (scAdministradorEscenas.Personaje1 == pers)
    {
        scAdministradorEscenas.Personaje1 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje2 == pers)
    {
        scAdministradorEscenas.Personaje2 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje3 == pers)
    {
        scAdministradorEscenas.Personaje3 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje4 == pers)
    {
        scAdministradorEscenas.Personaje4 = null;
        ActualizarLista();
        return;
    }
    // Verificar si la primera posición está disponible
    if (scAdministradorEscenas.Personaje1 == null)
    {
        scAdministradorEscenas.Personaje1 = pers;
    }
    // Verificar si la segunda posición está disponible
    else if (scAdministradorEscenas.Personaje2 == null)
    {
        scAdministradorEscenas.Personaje2 = pers;
    }
    // Verificar si la tercera posición está disponible
    else if (scAdministradorEscenas.Personaje3 == null)
    {
        scAdministradorEscenas.Personaje3 = pers;
    }
    // Verificar si la cuarta posición está disponible
    else if (scAdministradorEscenas.Personaje4 == null)
    {
        scAdministradorEscenas.Personaje4 = pers;
    }
    // Si todas están ocupadas, reemplazar al cuarto scAdministradorEscenas.Personaje
    else
    {
        scAdministradorEscenas.Personaje4 = pers;
    }

    ActualizarLista();

   

    
 }
 public TextMeshProUGUI txtSeleccionadosPersonajes;
 public int esEmboscadaEnemiga;
 public int EventoBatallaID = 0;
 public void EventoBatallaNormal(int n, int esEmboscada = 0)
 {  
    esEmboscadaEnemiga = esEmboscada;

    gameObject.SetActive(true);
    UIEmpezarBatalla.SetActive(true);
    UIEmpezarBatallaACaravana.SetActive(false);
    UITerminarBatalla.SetActive(false);
   //Si n es 0, el ID se determina al azar
   if (n==0)
   {
    AtributosZona scZona = CampaignManager.Instance.scAtributosZona;

            if (scZona.FASE == 1) //Encuentros para FASE 1
            {
              int probabilidadesBatallaCorrupta = (int)CampaignManager.Instance.GetValorAlientoNegro()*4;

              int tiradaCorrupta = UnityEngine.Random.Range(1, 100);
              if (tiradaCorrupta < probabilidadesBatallaCorrupta) // Batalla CORRUPTA
                {
                    int randomEncuentroCorrupto =UnityEngine.Random.Range(1, 6);
                    switch (randomEncuentroCorrupto)
                    {
                        case 1: EventoBatallaID = 600; break;
                        case 2: EventoBatallaID = 601; break;
                        case 3: EventoBatallaID = 602; break;
                        case 4: EventoBatallaID = 603; break;
                        case 5: EventoBatallaID = 604; break;
                    }

                }
                else if(UnityEngine.Random.Range(1, 100) < 70) // Batalla de ZONA   
                {

                    int randomEncuentroNormal =UnityEngine.Random.Range(1, 8);
                    switch (randomEncuentroNormal)
                    {
                        case 1: EventoBatallaID = scZona.FASE1IDEncuentroNormal1; break;
                        case 2: EventoBatallaID = scZona.FASE1IDEncuentroNormal2; break;
                        case 3: EventoBatallaID = scZona.FASE1IDEncuentroNormal3; break;
                        case 4: EventoBatallaID = scZona.FASE1IDEncuentroNormal4; break;
                        case 5: EventoBatallaID = scZona.FASE1IDEncuentroNormal5; break;
                        case 6: EventoBatallaID = scZona.FASE1IDEncuentroNormal6; break;
                        case 7: EventoBatallaID = scZona.FASE1IDEncuentroNormal7; break;
                    }
                }
                else //Batalla GENERICA 
                {
                    int randomEncuentroGenerico =UnityEngine.Random.Range(1, 5);
                    switch (randomEncuentroGenerico)
                    {
                        case 1: EventoBatallaID = 500; break; //Bandidos 
                        case 2: EventoBatallaID = 501; break; //Bandidos II
                        case 3: EventoBatallaID = 502; break; //Bandidos III
                        case 4: EventoBatallaID = 503; break; //Bandidos IV

                    }
                }
            }
   }
   else //Si no es 0, es especifico
   { 
    EventoBatallaID = n;
   }
    ActualizarLista();
 }
 public void EventoBatallaElite(int n, int esEmboscada = 0)
 {  
    esEmboscadaEnemiga = esEmboscada;

    gameObject.SetActive(true);
    UIEmpezarBatalla.SetActive(true);
    UIEmpezarBatallaACaravana.SetActive(false);
    UITerminarBatalla.SetActive(false);
   //Si n es 0, el ID se determina al azar
   if (n==0)
   {
    AtributosZona scZona = CampaignManager.Instance.scAtributosZona;
    if(scZona.FASE == 1) //Encuentros para FASE 1
    {
        int randomEncuentroNormal =UnityEngine.Random.Range(1,4);
        switch(randomEncuentroNormal)
        {
            case 1: EventoBatallaID = scZona.FASE1IDEncuentroElite1; break;
            case 2: EventoBatallaID = scZona.FASE1IDEncuentroElite2; break;
            case 3: EventoBatallaID = scZona.FASE1IDEncuentroElite3; break;
        
        }
    }
   }
   else //Si no es 0, es especifico
   { 
    EventoBatallaID = n;
   }

    ActualizarLista();
 }

 public void EventoBatallaFinal(int n, int esEmboscada = 0)
 {  
    esEmboscadaEnemiga = esEmboscada;

    gameObject.SetActive(true);
    UIEmpezarBatalla.SetActive(true);
    UIEmpezarBatallaACaravana.SetActive(false);
    UITerminarBatalla.SetActive(false);
   //Si n es 0, el ID se determina al azar
   if (n==0)
   {
    AtributosZona scZona = CampaignManager.Instance.scAtributosZona;
    if(scZona.FASE == 1) //Encuentros para FASE 1
    {
        int randomEncuentroFinal = UnityEngine.Random.Range(1, 3);
        switch (randomEncuentroFinal)
        {
            case 1: EventoBatallaID = scZona.FASE1IDEncuentroJefe1; break;
            case 2: EventoBatallaID = scZona.FASE1IDEncuentroJefe2; break;
        }
    }
   }
   else //Si no es 0, es especifico
   { 
    EventoBatallaID = n;
   }

    ActualizarLista();
 }

     public TextMeshProUGUI txtMilicianosDisponibles;

    public void EventoBatallaCaravana(int n, int esEmboscada = 3/*3 es Ataque a Caravana*/)
    {
        esEmboscadaEnemiga = esEmboscada;

        gameObject.SetActive(true);
        UIEmpezarBatallaACaravana.SetActive(true);
        UIEmpezarBatalla.SetActive(false);
        UITerminarBatalla.SetActive(false);

        txtMilicianosDisponibles.text = "Milicianos disponibles: " + (int)CampaignManager.Instance.GetMiliciasActual() / 10;

        // Autoseleccionar guardia/vigilar: tomar los primeros 4 de la lista si hay mas de 4
        scAdministradorEscenas.Personaje1 = null;
        scAdministradorEscenas.Personaje2 = null;
        scAdministradorEscenas.Personaje3 = null;
        scAdministradorEscenas.Personaje4 = null;

        int asignados = 0;
        foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
        {
            if (asignados >= 4) break;
            if (pers == null) continue;
            if (pers.Camp_Muerto) continue;
            if (pers.fVidaActual <= 1) continue;
            if (!((/*Base: Guardia*/pers.ActividadSeleccionada == 3) || (/*Caballero: Vigilar*/pers.ActividadSeleccionada == 6))) continue;

            asignados++;
            if (asignados == 1) scAdministradorEscenas.Personaje1 = pers;
            else if (asignados == 2) scAdministradorEscenas.Personaje2 = pers;
            else if (asignados == 3) scAdministradorEscenas.Personaje3 = pers;
            else if (asignados == 4) scAdministradorEscenas.Personaje4 = pers;
        }

        //Si n es 0, el ID se determina al azar
        if (n == 0)
        {
            AtributosZona scZona = CampaignManager.Instance.scAtributosZona;
            if (scZona.FASE == 1) //Encuentros para FASE 1
            {
                int probabilidadesBatallaCorrupta = (int)CampaignManager.Instance.GetValorAlientoNegro() * 4;

                int tiradaCorrupta =UnityEngine.Random.Range(1, 100);
                if (tiradaCorrupta < probabilidadesBatallaCorrupta) // Batalla CORRUPTA
                {
                    int randomEncuentroCorrupto = UnityEngine.Random.Range(1, 6);
                    switch (randomEncuentroCorrupto)
                    {
                        case 1: EventoBatallaID = 605; break;//Corruptos Ataque a Caravana I
                        case 2: EventoBatallaID = 606; break;//Corruptos Ataque a Caravana II
                    }

                }
                else if(UnityEngine.Random.Range(1, 100) < 70)
                {
                    int randomEncuentroNormal = UnityEngine.Random.Range(1, 3);
                    switch (randomEncuentroNormal)
                    {
                        case 1: EventoBatallaID = scZona.FASE1IDAtaqueCaravana1; break;
                        case 2: EventoBatallaID = scZona.FASE1IDAtaqueCaravana2; break;
                    }

                }
                else //Encuentros genéricos
                {
                    int randomEncuentroGenerico = UnityEngine.Random.Range(1, 3);
                    switch (randomEncuentroGenerico)
                    {
                        case 1: EventoBatallaID = 504; break; //Bandidos Ataque a Caravana I
                        case 2: EventoBatallaID = 505; break; //Bandidos Ataque a Caravana II
                    }
                }

                   if(EventoBatallaID == 0) //Si por alguna razón no se asignó un ID, se asigna uno genérico
                {
                    EventoBatallaID =  EventoBatallaID = scZona.FASE1IDAtaqueCaravana1; //Bandidos Ataque a Caravana I

                }



            }

            // Se actualiza la UI al final del metodo para ambos caminos
        }
        else //Si no es 0, es especifico
        {
         EventoBatallaID = n;
        }

        // Refrescar la lista con la seleccion automatica aplicada
        ActualizarLista();

    }

    public void EventoBatallaSubterranea(int FASE) //ID de 400 a 449
    {
        //Es emboscada enemiga, es un ataque subterráneo

        esEmboscadaEnemiga = 1; //Es emboscada, es un ataque subterráneo

        gameObject.SetActive(true);
        UIEmpezarBatalla.SetActive(true);
        UITerminarBatalla.SetActive(false);

        txtMilicianosDisponibles.text = "Milicianos disponibles: " + (int)CampaignManager.Instance.GetMiliciasActual() / 10;

        if (FASE == 1)
        {
            int randomSubterraneo = UnityEngine.Random.Range(1, 4);
            switch (randomSubterraneo)
            {
                case 1: EventoBatallaID = 400; break;
                case 2: EventoBatallaID = 401; break;
                case 3: EventoBatallaID = 402; break;
            }
        }
        else if (FASE == 2)
        {
            int randomSubterraneo = UnityEngine.Random.Range(1, 4);
            switch (randomSubterraneo)
            {
                case 1: EventoBatallaID = 403; break;
                case 2: EventoBatallaID = 404; break;
                case 3: EventoBatallaID = 405; break;
            }
        }
        else if (FASE == 3)
        {
            int randomSubterraneo = UnityEngine.Random.Range(1, 4);
            switch (randomSubterraneo)
            {
                case 1: EventoBatallaID = 406; break;
                case 2: EventoBatallaID = 407; break;
                case 3: EventoBatallaID = 408; break;
            }
        }


        ActualizarLista();
    }
   
 public GameObject UIEmpezarBatalla;
 public GameObject UIEmpezarBatallaACaravana;

 public GameObject UITerminarBatalla;
 public GameObject txtVictoria;
 public GameObject txtDerrota;
 public TextMeshProUGUI txtRecompensa;
 public void EfectosDeBatallaEnCampaña(int resultado) //1 Ganó, 0 Perdió
 {
    UIEmpezarBatalla.SetActive(false);
    UITerminarBatalla.SetActive(true);

        if (resultado == 1)
        {
            txtVictoria.SetActive(true);
            txtDerrota.SetActive(false);
            CampaignManager.Instance.CambiarEsperanzaActual(10); //ganar siempre aumenta 10 de base
        }
        else
        {
            txtVictoria.SetActive(false);
            txtDerrota.SetActive(true);
        }

    int aumentochancesitem = 0;
    //Resultados batalla BOSQUE ARDIENTE
    #region
    if (EventoBatallaID == 1) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 120 Exp, 140 Oro y 10 Materiales.";
            CampaignManager.Instance.CambiarOroActual(140);
            CampaignManager.Instance.CambiarMaterialesActuales(10);
            DarExperiencia(120);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 15 Civiles fueron asesinados, -15 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-15);
            CampaignManager.Instance.CambiarEsperanzaActual(-15);
        }
    }
    if (EventoBatallaID == 2) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 135 Exp, 100 Oro y 15 Materiales.";
            CampaignManager.Instance.CambiarOroActual(100);
            CampaignManager.Instance.CambiarMaterialesActuales(15);
            DarExperiencia(135);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 20 Civiles fueron asesinados, -17 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-20);
            CampaignManager.Instance.CambiarEsperanzaActual(-17);
        }
    }
    if (EventoBatallaID == 3) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 105 Exp, 80 Oro y 13 Materiales.";
            CampaignManager.Instance.CambiarOroActual(80);
            CampaignManager.Instance.CambiarMaterialesActuales(13);
            DarExperiencia(105);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 12 Civiles fueron asesinados, -12 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-12);
            CampaignManager.Instance.CambiarEsperanzaActual(-12);
        }
    }
    if (EventoBatallaID == 4) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 125 Exp, 110 Oro y 10 Materiales.";
            CampaignManager.Instance.CambiarOroActual(110);
            CampaignManager.Instance.CambiarMaterialesActuales(10);
            DarExperiencia(125);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 18 Civiles fueron asesinados, -16 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-18);
            CampaignManager.Instance.CambiarEsperanzaActual(-16);
        }
    }
    if (EventoBatallaID == 5) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 152 Exp, 101 Oro y 22 Materiales.";
            CampaignManager.Instance.CambiarOroActual(101);
            CampaignManager.Instance.CambiarMaterialesActuales(22);
            DarExperiencia(152);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 17 Civiles fueron asesinados, -14 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-17);
            CampaignManager.Instance.CambiarEsperanzaActual(-14);
        }
    }
    if (EventoBatallaID == 6) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 220 Exp, 141 Oro.";
            CampaignManager.Instance.CambiarOroActual(141);
            DarExperiencia(220);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 13 Civiles fueron asesinados, -21 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-13);
            CampaignManager.Instance.CambiarEsperanzaActual(-21);
        }
    }
    if (EventoBatallaID == 7) //FASE 1 Bosque Ardiente Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 95 Exp, 88 Oro y 9 Materiales.";
            CampaignManager.Instance.CambiarOroActual(88);
            CampaignManager.Instance.CambiarMaterialesActuales(9);
            DarExperiencia(95);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 11 Civiles fueron asesinados, -10 esperanza";
            CampaignManager.Instance.CambiarCivilesActuales(-11);
            CampaignManager.Instance.CambiarEsperanzaActual(-10);
        }
    }
    if (EventoBatallaID == 8) //FASE 1 Bosque Ardiente Elite
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 310 Exp, 180 Oro y 25 Materiales.";
            CampaignManager.Instance.CambiarOroActual(180);
            CampaignManager.Instance.CambiarMaterialesActuales(25);
            aumentochancesitem += 100; //Batallas elite garantizan Items
            DarExperiencia(310);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 21 Civiles fueron asesinados, -15 esperanza, 1 Buey asesinado.";
            CampaignManager.Instance.CambiarCivilesActuales(-21);
            CampaignManager.Instance.CambiarEsperanzaActual(-15);
            CampaignManager.Instance.CambiarBueyesActuales(-1);

        }
    }
    if (EventoBatallaID == 9) //FASE 1 Bosque Ardiente Elite
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 305 Exp, 120 Oro y 32 Materiales.";
            CampaignManager.Instance.CambiarOroActual(120);
            CampaignManager.Instance.CambiarMaterialesActuales(32);
            aumentochancesitem += 100; //Batallas elite garantizan Items
            DarExperiencia(305);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 19 Civiles fueron asesinados, -13 esperanza, 1 Buey asesinado.";
            CampaignManager.Instance.CambiarCivilesActuales(-19);
            CampaignManager.Instance.CambiarEsperanzaActual(-13);
            CampaignManager.Instance.CambiarBueyesActuales(-1);
        }
    }
        if (EventoBatallaID == 11) //FASE 1 Bosque Ardiente Jefe Arbol Lamentos
        {
            if (resultado == 1) //Victoria
            {
                txtRecompensa.text = "Se han obtenido 580 Exp, 310 Oro y 32 Materiales.";
                CampaignManager.Instance.CambiarOroActual(310);
                CampaignManager.Instance.CambiarMaterialesActuales(40);
                aumentochancesitem += 100; //Batallas elite garantizan Items
                DarExperiencia(580);
                //Efectos victoria siguiente zona- HACER

            }
            else //Derrota
            {
                txtRecompensa.text = "La caravana ha fracasado. Fin del juego.";
            }
           
           
                if (CampaignManager.Instance.scTutorialManager.tutorialActivo)
                { 
                    CampaignManager.Instance.scTutorialManager.establecerPasoEspecifico(12);
                }
    }
        #endregion

        //Resultados batalla BANDIDOS
        #region 
        if (EventoBatallaID == 500) //FASE 1 Bandidos Normal
        {
            if (resultado == 1) //Victoria
            {
                txtRecompensa.text = "Se han obtenido 110 Exp, 150 Oro y 3 Materiales.";
                CampaignManager.Instance.CambiarOroActual(150);
                CampaignManager.Instance.CambiarMaterialesActuales(3);
                DarExperiencia(110);
            }
            else //Derrota
            {
                txtRecompensa.text = "Has fallado en defender a la caravana, 5 Civiles fueron asesinados, -8 esperanza, 65 oro robado.";
                CampaignManager.Instance.CambiarCivilesActuales(-5);
                CampaignManager.Instance.CambiarEsperanzaActual(-8);
                CampaignManager.Instance.CambiarOroActual(-65);
            }
        }
    if (EventoBatallaID == 501) //FASE 1 Bandidos Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 120 Exp, 130 Oro y 2 Materiales.";
            CampaignManager.Instance.CambiarOroActual(130);
            CampaignManager.Instance.CambiarMaterialesActuales(2);
            DarExperiencia(120);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 8 Civiles fueron asesinados, -11 esperanza, 45 oro robado.";
            CampaignManager.Instance.CambiarCivilesActuales(-8);
            CampaignManager.Instance.CambiarEsperanzaActual(-11);
            CampaignManager.Instance.CambiarOroActual(-45);
        }
    }
    if (EventoBatallaID == 502) //FASE 1 Bandidos Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 140 Exp, 170 Oro y 11 Materiales.";
            CampaignManager.Instance.CambiarOroActual(170);
            CampaignManager.Instance.CambiarMaterialesActuales(11);
            DarExperiencia(140);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana, 10 Civiles fueron asesinados, -14 esperanza, 61 oro robado.";
            CampaignManager.Instance.CambiarCivilesActuales(-10);
            CampaignManager.Instance.CambiarEsperanzaActual(-14);
            CampaignManager.Instance.CambiarOroActual(-61);
        }
    }
    if (EventoBatallaID == 503) //FASE 1 Bandidos Normal
        {
            if (resultado == 1) //Victoria
            {
                txtRecompensa.text = "Se han obtenido 114 Exp, 155 Oro y 9 Materiales.";
                CampaignManager.Instance.CambiarOroActual(155);
                CampaignManager.Instance.CambiarMaterialesActuales(9);
                DarExperiencia(114);
            }
            else //Derrota
            {
                txtRecompensa.text = "Has fallado en defender a la caravana, 4 Civiles fueron asesinados, -9 esperanza, 50 oro robado.";
                CampaignManager.Instance.CambiarCivilesActuales(-4);
                CampaignManager.Instance.CambiarEsperanzaActual(-9);
                CampaignManager.Instance.CambiarOroActual(-50);
            }
        }
    #endregion

    //Resultados batalla CORRUPTOS
    #region 
    if (EventoBatallaID == 600) //FASE 1 Corruptos Normal
        {
            if (resultado == 1) //Victoria
            {
                txtRecompensa.text = "Se han obtenido 160 Exp, -1 Avance Aliento Negro.";
                DarExperiencia(160);
                CampaignManager.Instance.CambiarValorAlientoNegro(-1);
            }
            else //Derrota
            {
                txtRecompensa.text = "Has fallado en defender a la caravana de los Corruptos, 13 Civiles fueron asesinados, -11 esperanza, +1 Avance Aliento Negro.";
                CampaignManager.Instance.CambiarCivilesActuales(-13);
                CampaignManager.Instance.CambiarEsperanzaActual(-11);
                CampaignManager.Instance.CambiarValorAlientoNegro(1);
            }
        }
    if (EventoBatallaID == 601) //FASE 1 Corruptos Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 180 Exp, -1 Avance Aliento Negro.";
            DarExperiencia(180);
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana de los Corruptos, 15 Civiles fueron asesinados, -12 esperanza, +1 Avance Aliento Negro.";
            CampaignManager.Instance.CambiarCivilesActuales(-15);
            CampaignManager.Instance.CambiarEsperanzaActual(-12);
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
        }
    }
    if (EventoBatallaID == 602) //FASE 1 Corruptos Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 140 Exp, -1 Avance Aliento Negro.";
            DarExperiencia(140);
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana de los Corruptos, 15 Civiles fueron asesinados, -12 esperanza, +1 Avance Aliento Negro.";
            CampaignManager.Instance.CambiarCivilesActuales(-11);
            CampaignManager.Instance.CambiarEsperanzaActual(-12);
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
        }
    }
    if (EventoBatallaID == 603) //FASE 1 Corruptos Normal
    {
        if (resultado == 1) //Victoria
        {
            txtRecompensa.text = "Se han obtenido 140 Exp, -1 Avance Aliento Negro.";
            DarExperiencia(140);
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
        }
        else //Derrota
        {
            txtRecompensa.text = "Has fallado en defender a la caravana de los Corruptos, 14 Civiles fueron asesinados, -10 esperanza, +1 Avance Aliento Negro.";
            CampaignManager.Instance.CambiarCivilesActuales(-14);
            CampaignManager.Instance.CambiarEsperanzaActual(-10);
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
        }
    }
    if (EventoBatallaID == 604) //FASE 1 Corruptos Normal
        {
            if (resultado == 1) //Victoria
            {
                txtRecompensa.text = "Se han obtenido 170 Exp, -1 Avance Aliento Negro.";
                DarExperiencia(170);
                CampaignManager.Instance.CambiarValorAlientoNegro(-1);
            }
            else //Derrota
            {
                txtRecompensa.text = "Has fallado en defender a la caravana de los Corruptos, 16 Civiles fueron asesinados, -9 esperanza, +1 Avance Aliento Negro.";
                CampaignManager.Instance.CambiarCivilesActuales(-16);
                CampaignManager.Instance.CambiarEsperanzaActual(-9);
                CampaignManager.Instance.CambiarValorAlientoNegro(1);
            }
        }
    #endregion

     
     
     
     
    // Resultados batallas de Caravana sin importar zona, recompensa al azar, al perder se pierde la partida
    // Batallas Caravanas Resultados
    HashSet<int> idsBatallaCaravana = new HashSet<int> { 13, 14, 504, 505, 605, 606 }; //Agregar ACA ids de ataques a caravana!!
    if (idsBatallaCaravana.Contains(EventoBatallaID))
    {
        if (resultado == 1) //Victoria
        {
            int oro =UnityEngine.Random.Range(280, 351); // Oro entre 280 y 350
            int materiales =UnityEngine.Random.Range(22, 36); // Materiales entre 22 y 35
            int esperanza =UnityEngine.Random.Range(12, 25); // Esperanza entre 12 y 20

            txtRecompensa.text = $"Se han obtenido {oro} Oro, {materiales} Materiales y +{esperanza} Esperanza.";
            CampaignManager.Instance.CambiarOroActual(oro);
            CampaignManager.Instance.CambiarMaterialesActuales(materiales);
            CampaignManager.Instance.CambiarEsperanzaActual(esperanza);
        }
        else //Derrota
        {
            //FIN DEL JUEGO
        }
    }





    //Al perder se tiran chances de eliminar sequito. 50% -10% por tier mejora Defensa
    if (resultado == 2) //Al perder se tiran chances de eliminar sequito. 50% -10% por tier mejora Defensa
    {
        int rand =UnityEngine.Random.Range(1, 101);
        int prob = 50 - CampaignManager.Instance.mejoraCaravanaDefensas*10;
        if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                if (CampaignManager.Instance.scMenuSequito.SequitoAlAzarPerdido(out string nombre))
                {
                    txtRecompensa.text += $"\n\n-Los enemigos han eliminado al {nombre} luego de la Batalla.";
                }
            }
        
    }
    
    //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        if (resultado == 1) //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        {
            int rand =UnityEngine.Random.Range(1, 101);
            aumentochancesitem += CampaignManager.Instance.mejoraCaravanaCatalejos*5;
            int prob = 35+aumentochancesitem; //!! 35%
            if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                Item recompensa = CampaignManager.Instance.scMenuSequito.Sequito003Mercaderes.GetComponent<SequitoMercaderes>().ObtenerItemAlAzar();
                CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(recompensa.gameObject);
                txtRecompensa.text += $"\n\n- Has encontrado un objeto de recompensa: {recompensa.sNombreItem}.";

            }

        }

    AdministrarHeridas(scAdministradorEscenas.Personaje1, scAdministradorEscenas.unidadPers1);
    AdministrarHeridas(scAdministradorEscenas.Personaje2, scAdministradorEscenas.unidadPers2);
    AdministrarHeridas(scAdministradorEscenas.Personaje3, scAdministradorEscenas.unidadPers3);
    AdministrarHeridas(scAdministradorEscenas.Personaje4, scAdministradorEscenas.unidadPers4);
    //Cronistas
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(7)) //Cronistas -- Diferenciar cuando esten todas las batallas elite etc. para que de mas.
    {
        if (!CampaignManager.Instance.scSequitoCronistas.yaVendioCronica)
        {
            if (resultado == 1) //Victoria
            {
                CampaignManager.Instance.scSequitoCronistas.valorCambiosCronicas += 50;
                CampaignManager.Instance.CambiarEsperanzaActual(+5);
                CampaignManager.Instance.EscribirLog("-Los Cronistas han registrado la victoria, +50 Valor Crónica, +5 Esperanza.");
            }
            else //Derrota
            {
                CampaignManager.Instance.scSequitoCronistas.valorCambiosCronicas -= 50;
                CampaignManager.Instance.EscribirLog("-Los Cronistas han registrado la derrota, -50 Valor Crónica. -3 Esperanza.");
                CampaignManager.Instance.CambiarEsperanzaActual(-3);

            }
        }
    }
         

   //Resetear
            EventoBatallaID = 0;
 }

 void DarExperiencia(int cant)
 {
    if( scAdministradorEscenas.Personaje1 != null)
   { scAdministradorEscenas.Personaje1.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje2 != null)
   { scAdministradorEscenas.Personaje2.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje3 != null)
   { scAdministradorEscenas.Personaje3.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje4 != null)
   { scAdministradorEscenas.Personaje4.RecibirExperiencia(cant);}
 }

    public void AdministrarHeridas(Personaje pers, Unidad uni)
    {
       
        if (pers != null)
        {
            if (pers.fVidaActual < 1)
            {
                if (pers.Camp_Herido) //Si ya estaba herido
                {
                    //muere
                    pers.Camp_Muerto = true;
                }
                else //Si no estaba herido, hiere
                {
                    pers.Camp_Herido = true;
                    pers.fVidaActual = 5;
                }

                if (uni.loMatoCorrompido) //Si lo mató un corrupto, se marca como corrupto
                {

                    if (pers.Camp_Corrupto)
                    {
                         //muere
                         pers.Camp_Muerto = true;

                    }
                    else
                    {
                        pers.Camp_Corrupto = true;
                        CampaignManager.Instance.EscribirLog($"{uni.uNombre} ha sido corrompido.");


                    }
                   
                    
                }

            }

        
           

            //Actualiza estados visuales herida y muerte
            foreach (Transform boton in contenedorUIPersonajes.transform)
            {
                btnPersonaje btn = boton.gameObject.GetComponent<btnPersonaje>();

                if (btn.personajeRepresentado.Camp_Herido)
                {
                    boton.transform.GetChild(3).gameObject.SetActive(true);
                }
                else { boton.transform.GetChild(3).gameObject.SetActive(false); }

                if (btn.personajeRepresentado.Camp_Muerto)
                {
                    boton.transform.GetChild(3).gameObject.SetActive(false);
                    boton.transform.GetChild(4).gameObject.SetActive(true);
                }
                else { boton.transform.GetChild(4).gameObject.SetActive(false); }


            }

        }
    }

 public void CerrarMenuBatalla()
    {

        gameObject.SetActive(false);
    }
 public void ComenzarBatalla()
 {
    // Autorelleno para Ataque a Caravana: prioriza personajes con m1s vida
    if (UIEmpezarBatallaACaravana != null && UIEmpezarBatallaACaravana.activeInHierarchy)
    {
        esEmboscadaEnemiga = 3; // Marcar tipo Ataque a Caravana
        // Reunir seleccionados actuales
        HashSet<Personaje> seleccionados = new HashSet<Personaje>();
        if (scAdministradorEscenas.Personaje1 != null) seleccionados.Add(scAdministradorEscenas.Personaje1);
        if (scAdministradorEscenas.Personaje2 != null) seleccionados.Add(scAdministradorEscenas.Personaje2);
        if (scAdministradorEscenas.Personaje3 != null) seleccionados.Add(scAdministradorEscenas.Personaje3);
        if (scAdministradorEscenas.Personaje4 != null) seleccionados.Add(scAdministradorEscenas.Personaje4);

        int countSel = seleccionados.Count;
        if (countSel < 4)
        {
            // Candidatos: vivos, con vida > 1, no repetidos
            List<Personaje> candidatos = new List<Personaje>();
            foreach (Personaje p in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (p == null) continue;
                if (p.Camp_Muerto) continue;
                if (p.fVidaActual <= 1) continue;
                if (seleccionados.Contains(p)) continue;
                candidatos.Add(p);
            }
            // Ordenar por vida actual desc, luego vida mbxima desc
            candidatos.Sort((a, b) => {
                int cmp = b.fVidaActual.CompareTo(a.fVidaActual);
                if (cmp == 0) cmp = b.fVidaMaxima.CompareTo(a.fVidaMaxima);
                return cmp;
            });

            if (scAdministradorEscenas.PersonajesSorprendidosInicioCaravana == null)
            {
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana = new List<Personaje>();
            }

            // Rellenar huecos en orden 1..4
            if (scAdministradorEscenas.Personaje1 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje1 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (scAdministradorEscenas.Personaje2 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje2 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (scAdministradorEscenas.Personaje3 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje3 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (scAdministradorEscenas.Personaje4 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje4 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
        }
    }

    scAdministradorEscenas.CargarBatalla(EventoBatallaID, esEmboscadaEnemiga);
    
 }


}
