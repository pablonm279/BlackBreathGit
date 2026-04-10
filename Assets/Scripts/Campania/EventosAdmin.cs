using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventosAdmin : MonoBehaviour
{
   const int DificultadBestiasAterradas = 12;
   const int DificultadHieloQuebradizo = 12;
   const int DificultadBrechaEnLaCalzada = 12;
   const int DificultadTechoInestable = 12;
   const int DificultadPasoPrecario = 11;
   const int DificultadAireEnrarecido = 10;
   const int DificultadRumorDeDesbande = 12;
   const int DificultadVadoTraicionero = 14;
   const int DificultadCarroEncajado = 13;
   const int DificultadMarcasDelCorreo = 10;
   const int DificultadPulsoDeMando = 11;
   const int DificultadHombrosFirmes = 13;
   const int DificultadManoCierta = 13;
   const int DificultadDosMiradas = 9;
   const int DificultadArengaEnLaLluvia = 11;
   const int DificultadRepasoDeManiobras = 12;
   const int DificultadRastroSospechoso = 10;
   const int DificultadCuervosDelPaso = 11;
   const int DificultadVigiaDelHielo = 10;
   const string ItemIdBolsaOlvidada = "ITEM_SCRIPTS_CONSUMIBLES_GENERADOS_CONS12_AMPOLLAAISLANTE";
   [SerializeField] TextMeshProUGUI txtTitulo;
   [SerializeField] TextMeshProUGUI txtDescripcion;
   [SerializeField] Image imRetrato;
   [SerializeField] GameObject botonA;
   [SerializeField] GameObject botonB;
   [SerializeField] TextMeshProUGUI textBotonA;
   [SerializeField] TextMeshProUGUI textBotonB;
   
   [SerializeField] GameObject retratoParticipante1;
   [SerializeField] GameObject retratoParticipante2;

   //Malos
   [SerializeField] Sprite Evento001; //Retraso Nocturno
   [SerializeField] Sprite Evento002; //Desapariciones Misteriosas
   [SerializeField] Sprite Evento003; //Bueyes Enfermos
   [SerializeField] Sprite Evento004; //Peaje Criminal
   [SerializeField] Sprite Evento005; //Personaje Enfermo
   [SerializeField] Sprite Evento006; //Arcas Robadas
   [SerializeField] Sprite Evento007; //Carro Deteriorado
   [SerializeField] Sprite Evento008; //Rí­o Contaminado
   [SerializeField] Sprite Evento009; //Riña entre personajes
   [SerializeField] Sprite Evento010; //Liderazgo Cuestionado  


   [SerializeField] Sprite Evento201; //Destello Esperanzador
   [SerializeField] Sprite Evento202; //Risotadas en la Caravana 
   [SerializeField] Sprite Evento203; //Caravana perdida 
   [SerializeField] Sprite Evento204; //Aserradero Abandonado 
   [SerializeField] Sprite Evento205; //Manada de Bueyes 
   [SerializeField] Sprite Evento206; //Civiles en Apuros 
   [SerializeField] Sprite Evento207; //Tranquilidad 
   [SerializeField] Sprite Evento208; //Voto de Confianza 
   [SerializeField] Sprite Evento209; //Lugareño Anciano 
   [SerializeField] Sprite Evento210; //Sueño Inspirador 
    public bool TirarEventoMalo()
    {
        return TirarEventoMalo(TipoOrigenEventoCampania.Nodo);
    }

   public bool TirarEventoMalo(TipoOrigenEventoCampania origen)
    {
        return TirarEventoAleatorio(origen, TipoResultadoEventoCampania.Malo);
    }

   public bool TirarEventoBueno()
   {
        return TirarEventoBueno(TipoOrigenEventoCampania.Nodo);
   }

   public bool TirarEventoBueno(TipoOrigenEventoCampania origen)
   {
        return TirarEventoAleatorio(origen, TipoResultadoEventoCampania.Bueno);
   }

   bool TirarEventoAleatorio(TipoOrigenEventoCampania origen, TipoResultadoEventoCampania resultado)
   {
        int zonaId = CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null
            ? CampaignManager.Instance.scAtributosZona.ID
            : IdsZonaCampania.Generica;
        List<int> eventosUsados = CampaignManager.Instance != null
            ? CampaignManager.Instance.ObtenerEventosAleatoriosUsadosMapa()
            : null;

        if (!CatalogoEventosCampania.TryObtenerEventoAleatorio(origen, resultado, zonaId, eventosUsados, out int idEvento))
        {
            return false;
        }

        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.RegistrarEventoAleatorioUsadoEnMapa(idEvento);
        }

        EmpezarEvento(idEvento);
        return true;
   }

   int eventoActual;
    public void EmpezarEvento(int ID)
    {
        eventoActual = ID;
        botonA.SetActive(true);
        botonB.SetActive(true);

        participanteEvento1 = null;
        participanteEvento2 = null;
        retratoParticipante1.SetActive(false);
        retratoParticipante2.SetActive(false);

        // Los eventos aleatorios llegan filtrados por origen y zona desde CatalogoEventosCampania.

        // EVENTOS DE NODO MALOS GENERICOS 1 -80  Hechos: 10/80
        if (ID == 1) //Retraso Nocturno
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Retraso Nocturno");

            txtDescripcion.text = TRADU.i.Traducir("Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n");
            txtDescripcion.text += TRADU.i.Traducir("Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podráa perderse.\n\n\n\n\n\n\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Pasan las Horas: +1 Avance Aliento Negro</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 2) //Desapariciones Misteriosas
        {
            imRetrato.sprite = Evento002;
            txtTitulo.text = TRADU.i.Traducir("Desapariciones Misteriosas");

            txtDescripcion.text = TRADU.i.Traducir("De un momento a otro, varios miembros de la caravana han desaparecido sin dejar rastro. Nadie tiene una explicación de lo que ha sucedido. Pero el miedo y la incertidumbre se apoderan de todos.\n");
            txtDescripcion.text += TRADU.i.Traducir("Luego de buscar vagamente en la cercaní­a y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 3) //Bueyes Enfermos
        {
            imRetrato.sprite = Evento003;
            txtTitulo.text = TRADU.i.Traducir("Bueyes Enfermos");

            txtDescripcion.text = TRADU.i.Traducir("Uno de los bueyes de la caravana ha caí­do enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides revisarlos tomará unas horas: +1 Avance Aliento Negro.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides ignorar las advertencias: 1-3 Bueyes mas morirán.</color>\n\n");


            textBotonA.text = TRADU.i.Traducir("Revisarlos");
            textBotonB.text = TRADU.i.Traducir("Ignorar");
        }
        if (ID == 4) //Peaje criminal
        {
            imRetrato.sprite = Evento004;
            txtTitulo.text = TRADU.i.Traducir("Peaje Criminal");

            txtDescripcion.text = TRADU.i.Traducir("Mientras la caravana se dispone a avanzar por un terreno peligroso, se topa con un grupo de bandidos que exige un peaje exorbitante para dejar pasar a la caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides pagar el peaje, perderás 1 de Oro por Civil.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Luchar con los Bandidos.</color>\n\n");


            textBotonA.text = TRADU.i.Traducir("Pagar");
            textBotonB.text = TRADU.i.Traducir("Luchar");
        }
        if (ID == 5) //Personaje Enfermo
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Personaje Enfermo");
            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = $"<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se acerca a ti y no luce nada bien. Te comenta que ha empezado a sentirse enfermo y necesita medicina para mejorar pronto y estar nuevamente en condiciones de combatir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Obtendrá el estado Enfermo por 4-7 dí­as. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 dí­a.\n\n\n\n\n");

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un dí­a extra.</color>\n\n");


            textBotonA.text = TRADU.i.Traducir("Pagar");
            if (CampaignManager.Instance.GetOroActuales() < 45)
            {
                botonA.SetActive(false);
            }
            textBotonB.text = TRADU.i.Traducir("No pagar");
        }
        if (ID == 6) //Arcas Robadas
        {
            imRetrato.sprite = Evento006;
            txtTitulo.text = TRADU.i.Traducir("Arcas Robadas");

            txtDescripcion.text = TRADU.i.Traducir("Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botón.\n\n");
            int oroRobado = CampaignManager.Instance.GetOroActuales() / 4; //25% del oro actual
            if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
            int seguridad = 40 + (int)CampaignManager.Instance.GetMiliciasActual();

            txtDescripcion.text += TRADU.i.Traducir("<b>Oro Robado:  ") + oroRobado + "\n\n</b>";
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perderí­a 5 de Esperanza, <i>") + seguridad + TRADU.i.Traducir("% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n");


            textBotonA.text = TRADU.i.Traducir("Interrogar");

            textBotonB.text = TRADU.i.Traducir("No interrogar");
        }
        if (ID == 7) //Carro Deteriorado
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Carro Deteriorado");

            txtDescripcion.text = TRADU.i.Traducir("Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes pasar los 60 suministros caí­dos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Aceptar");

            textBotonB.text = TRADU.i.Traducir("No aceptar");
        }
        if (ID == 8) //Rí­o Contaminado
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = TRADU.i.Traducir("Rí­o Contaminado");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana encuentra un rí­o con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("El agua podráa estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n");

            int chancesContaminado = 30 + (int)CampaignManager.Instance.GetValorAlientoNegro() * 3;
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si les niegas el acceso perderás 15 de Esperanza.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si los dejas ir, hay un %") + chancesContaminado + TRADU.i.Traducir("<i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no está contaminada descansarán (-1 Fatiga).</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Negarse");

            textBotonB.text = TRADU.i.Traducir("Dejarlos");
        }
        if (ID == 9) //Riña
        {
            imRetrato.sprite = Evento009;
            txtTitulo.text = TRADU.i.Traducir("Riña");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(new List<Personaje> { participanteEvento1 });
            retratoParticipante2.SetActive(true);
            retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;

            txtDescripcion.text = TRADU.i.Traducir("Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> y <b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b> discutiendo acaloradamente.");
            txtDescripcion.text += TRADU.i.Traducir("\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La tensión sube y los demás caravaneros miran con incomodidad. Ambos reclaman tener la razón y esperan tu juicio.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 dí­as. Apoyas a:</color>\n\n");

            textBotonA.text = "" + participanteEvento1.sNombre;

            textBotonB.text = "" + participanteEvento2.sNombre;
        }
        if (ID == 10) //Liderazgo Cuestionado
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Liderazgo Cuestionado");

            txtDescripcion.text = TRADU.i.Traducir("Un Civil de origen noble se acerca a ti con altanerí­a y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podráa hacerlo mejor.\n");
            txtDescripcion.text += TRADU.i.Traducir("Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n");

            int chances = 35 + (int)CampaignManager.Instance.GetEsperanzaActual() / 3;
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes dar un discurso motivador, refutando sus argumentos con hechos.</color> Chances: %") + chances + TRADU.i.Traducir(" <i>(Determinado por Esperanza) Éxito: +15 Esperanza. Fallo: -20 Esperanza.</i> \n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n");

            textBotonA.text = TRADU.i.Traducir("Discurso");

            textBotonB.text = TRADU.i.Traducir("Golpear");
        }
        if (ID == 11) // Paso Precario
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Paso Precario");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("La Caravana llega a un tramo estrecho, quebrado y lleno de tablones flojos. No parece imposible de cruzar, pero sí lo bastante traicionero como para convertir un descuido en accidente.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar guiar el cruce antes de que cunda el pánico.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC ") + DificultadPasoPrecario + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar guiar el cruce antes de que cunda el pánico.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 11. Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides rodear el tramo, la Caravana ganará +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Guiar el cruce");
            textBotonB.text = TRADU.i.Traducir("Rodear");
        }
        if (ID == 12) // Aire Enrarecido
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Aire Enrarecido");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Desde una bodega medio tapada llegan golpes apagados y pedidos de ayuda. El aire que sale por la entrada está cargado de polvo viejo, moho y algo que raspa la garganta apenas uno se acerca.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar entrar y sacar a quienes sigan con vida antes de que colapse el lugar.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Fortaleza DC ") + DificultadAireEnrarecido + TRADU.i.Traducir(" <i>(TS Fortaleza actual: ") + ObtenerTSFortalezaTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, rescatará 6-10 Civiles y ganará 30 Experiencia. Si falla, obtendrá Enfermo por 3 días.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar entrar y sacar a quienes sigan con vida antes de que colapse el lugar.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Fortaleza DC 10. Si la supera, rescatará 6-10 Civiles y ganará 30 Experiencia. Si falla, obtendrá Enfermo por 3 días.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides sellar la entrada y seguir, -4 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Entrar");
            textBotonB.text = TRADU.i.Traducir("Sellarla");
        }
        if (ID == 13) // Rumor de Desbande
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Rumor de Desbande");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Una versión exagerada de un peligro cercano se esparce de carro en carro y empieza a levantar un pánico innecesario. En pocos minutos, varios Civiles ya hablan de abandonar la marcha antes de quedar atrapados.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar frenarlo con calma antes de que empeore.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Mental DC ") + DificultadRumorDeDesbande + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, ganará 35 Experiencia y +4 Esperanza. Si falla, obtendrá Baja Moral por 3 días y la Caravana perderá 5 Esperanza.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar frenar el rumor con calma antes de que empeore.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Mental DC 12. Si la supera, ganará 35 Experiencia y +4 Esperanza. Si falla, obtendrá Baja Moral por 3 días y la Caravana perderá 5 Esperanza.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides imponer silencio por la fuerza, -3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Hablar");
            textBotonB.text = TRADU.i.Traducir("Imponer silencio");
        }
        if (ID == 14) // Vado Traicionero
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = TRADU.i.Traducir("Vado Traicionero");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(participanteEvento1 != null ? new List<Personaje> { participanteEvento1 } : null);
            retratoParticipante1.SetActive(participanteEvento1 != null);
            retratoParticipante2.SetActive(participanteEvento2 != null);
            txtDescripcion.text = TRADU.i.Traducir("La corriente parece mansa desde lejos, pero apenas los primeros carros tocan el vado queda claro que el fondo es resbaladizo y el agua tira con más fuerza de la esperada.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Alguien tendrá que adelantarse para ordenar el cruce de los bueyes y evitar que todo se desarme en medio del paso.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b>: TS Reflejos DC ") + DificultadVadoTraicionero + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 40 Experiencia. Si falla, obtendrá Herida y la Caravana perderá 1 Buey.</color>\n\n");
            }

            if (participanteEvento2 != null)
            {
                retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-<b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b>: TS Reflejos DC ") + DificultadVadoTraicionero + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento2) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 40 Experiencia. Si falla, obtendrá Herida y la Caravana perderá 1 Buey.</color>\n\n");
                textBotonA.text = participanteEvento1 != null ? participanteEvento1.sNombre : TRADU.i.Traducir("Héroe 1");
                textBotonB.text = participanteEvento2.sNombre;
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides no arriesgar el cruce, el rodeo hará avanzar al Aliento Negro.</color>\n\n");
                textBotonA.text = TRADU.i.Traducir("Cruzar");
                textBotonB.text = TRADU.i.Traducir("Rodear");
            }
        }
        if (ID == 15) // Carro Encajado
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Carro Encajado");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(participanteEvento1 != null ? new List<Personaje> { participanteEvento1 } : null);
            retratoParticipante1.SetActive(participanteEvento1 != null);
            retratoParticipante2.SetActive(participanteEvento2 != null);
            txtDescripcion.text = TRADU.i.Traducir("Uno de los carros queda mal encajado entre piedras y barro duro. Si no lo sacan pronto, la marcha se trabará alrededor suyo y el malhumor empezará a crecer.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Hace falta fuerza y aguante para moverlo sin terminar lastimado.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b>: TS Fortaleza DC ") + DificultadCarroEncajado + TRADU.i.Traducir(" <i>(TS Fortaleza actual: ") + ObtenerTSFortalezaTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 35 Experiencia y +3 Esperanza. Si falla, obtendrá Herida y la Caravana ganará +1 Fatiga.</color>\n\n");
            }

            if (participanteEvento2 != null)
            {
                retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-<b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b>: TS Fortaleza DC ") + DificultadCarroEncajado + TRADU.i.Traducir(" <i>(TS Fortaleza actual: ") + ObtenerTSFortalezaTotal(participanteEvento2) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 35 Experiencia y +3 Esperanza. Si falla, obtendrá Herida y la Caravana ganará +1 Fatiga.</color>\n\n");
                textBotonA.text = participanteEvento1 != null ? participanteEvento1.sNombre : TRADU.i.Traducir("Héroe 1");
                textBotonB.text = participanteEvento2.sNombre;
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides descargar el carro y seguir, +1 Fatiga.</color>\n\n");
                textBotonA.text = TRADU.i.Traducir("Empujarlo");
                textBotonB.text = TRADU.i.Traducir("Descargarlo");
            }
        }
        if (ID == 82) //Bestias Aterradas
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = TRADU.i.Traducir("Bestias Aterradas");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);

            txtDescripcion.text = TRADU.i.Traducir("Un grupo de bestias enloquecidas por el humo y el fuego irrumpe cerca del camino, cruzando entre los árboles calcinados con una violencia desesperada.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Los bueyes se inquietan al instante y varios Civiles retroceden alarmados. Si nadie actúa rápido, el caos podría extenderse a toda la Caravana.\n\n");
            txtDescripcion.text += "<color=#ba3fef>-<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar contener a los animales.</color> ");
            txtDescripcion.text += TRADU.i.Traducir("Tirada de Salvación: TS Reflejos DC ") + DificultadBestiasAterradas + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + tsReflejos + TRADU.i.Traducir(").</i> ");
            txtDescripcion.text += TRADU.i.Traducir("Si lo logra, ganará 40 Experiencia. Si falla, la Caravana perderá 2 Bueyes.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides apartarte y ceder el paso, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Contenerlos");
            textBotonB.text = TRADU.i.Traducir("Apartarse");
        }
        if (ID == 83) //Fuego en la Retaguardia
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Fuego en la Retaguardia");

            txtDescripcion.text = TRADU.i.Traducir("Un foco de incendio vuelve a encenderse detrás de la Caravana y el viento empuja las llamas hacia la retaguardia.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Durante unos instantes cunde el pánico: varios Civiles gritan, los bueyes tironean de los carros y parte de la carga corre peligro de prenderse fuego.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides apagarlo, la Caravana consumirá recursos en contener las llamas. -15 Suministros, +3 Esperanza.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides abandonar carga, perderás 15-25 Materiales, pero evitarás que el fuego se acerque más.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Apagarlo");
            textBotonB.text = TRADU.i.Traducir("Abandonar carga");
        }
        if (ID == 84) // Tambores en la Niebla
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Tambores en la Niebla");

            txtDescripcion.text = TRADU.i.Traducir("Entre la niebla y el viento se cuela un ritmo de tambores que nadie logra ubicar con claridad.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Los Civiles miran alrededor con inquietud. Puedes forzar a la Caravana a apurar el paso o frenar un momento hasta recuperar la calma.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides apurar el paso, el esfuerzo dejará a la Caravana más cansada. +1 Fatiga.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides esperar, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Apurar el paso");
            textBotonB.text = TRADU.i.Traducir("Esperar");
        }
        if (ID == 85) // Hielo Quebradizo
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Hielo Quebradizo");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = TRADU.i.Traducir("Un tramo helado del camino cruje bajo el peso de la Caravana. ") + "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar guiar el cruce antes de que el hielo ceda.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC 12 (TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir("). Si supera la tirada, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Un tramo helado del camino cruje bajo el peso de la Caravana. Uno de los Héroes puede intentar guiar el cruce antes de que el hielo ceda.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides rodear el tramo, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Cruzar");
            textBotonB.text = TRADU.i.Traducir("Rodear");
        }
        if (ID == 86) // Efigies del Paso
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Efigies del Paso");

            txtDescripcion.text = TRADU.i.Traducir("A los costados del camino aparecen varias efigies Kale'Tav clavadas en la nieve, adornadas con huesos, plumas y telas endurecidas por el hielo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Aunque nadie se acerque, su sola presencia alcanza para inquietar a la Caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>-6 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 87) // Partida de Caza
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Partida de Caza");

            txtDescripcion.text = TRADU.i.Traducir("Figuras encapuchadas se recortan un instante entre las rocas y luego desaparecen. No hace falta ver más para entender que una partida de caza Kale'Tav anda cerca.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes preparar a los Héroes para un enfrentamiento o esconder a los Civiles y perder tiempo en el desorden.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides prepararte, comenzará una batalla.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides esconder a los Civiles, el miedo y el desorden dejarán secuelas. +1 Fatiga, -3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Prepararse");
            textBotonB.text = TRADU.i.Traducir("Esconder a los Civiles");
        }
        if (ID == 88) // Frío Hasta los Huesos
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Frío Hasta los Huesos");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Enfermo por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 89) // Tótem de Guerra
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Tótem de Guerra");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana encuentra un tótem recién erigido, cubierto con sangre seca y cintas agitadas por el viento.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Los Civiles entienden enseguida que no están cruzando un desierto vacío, sino un territorio que alguien defiende con fanatismo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -3 Esperanza.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 90) // Golpes Bajo el Empedrado
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Golpes Bajo el Empedrado");

            txtDescripcion.text = TRADU.i.Traducir("Bajo el suelo helado y la piedra llegan golpes sordos y repetidos, como si algo enorme estuviera tanteando el camino desde abajo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Los Civiles aprietan el paso sin que nadie se los ordene. Aunque no llegue a emerger nada, el simple sonido basta para desgastar a la Caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga, -4 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 91) // Brecha en la Calzada
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Brecha en la Calzada");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Un tramo del camino se ha hundido y dejó un paso quebrado entre carros volcados, zanjas y piedras sueltas.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC ") + DificultadBrechaEnLaCalzada + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides rodear la brecha, la Caravana ganará +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Guiar el cruce");
            textBotonB.text = TRADU.i.Traducir("Rodear la brecha");
        }
        if (ID == 92) // Ecos en el Pozo
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Ecos en el Pozo");

            txtDescripcion.text = TRADU.i.Traducir("Desde la boca de un viejo pozo subterráneo suben gritos cortados y el sonido de uñas raspando piedra.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Nadie alcanza a ver qué hay abajo, pero está claro que algo se mueve en las profundidades. Puedes investigar o forzar a la Caravana a seguir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides investigar, comenzará una batalla.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides seguir, el miedo se extenderá entre los Civiles. -5 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Investigar");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }
        if (ID == 93) // Campanas sin Torre
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Campanas sin Torre");

            txtDescripcion.text = TRADU.i.Traducir("En algún punto de Nedukazal suenan campanas de alarma, aunque nadie alcanza a ver de dónde vienen.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("El sonido viaja entre ruinas, corrales vacíos y casas abandonadas, y deja a la Caravana con la sensación de haber llegado demasiado tarde para ayudar a alguien.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>-6 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 94) // Acecho en los Tejados
        {
            imRetrato.sprite = Evento002;
            txtTitulo.text = TRADU.i.Traducir("Acecho en los Tejados");

            txtDescripcion.text = TRADU.i.Traducir("Sombras veloces se recortan por momentos sobre tejados, tapias y galpones derruidos, y desaparecen antes de que nadie pueda apuntarlas bien.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes cerrar filas y avanzar con más cuidado o apurar el paso antes de que bajen sobre la Caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides cerrar filas, el avance será más tenso. +1 Fatiga.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides apurar el paso, varios Civiles quedarán rezagados en el desorden. -3 Civiles, -4 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Cerrar filas");
            textBotonB.text = TRADU.i.Traducir("Apurar el paso");
        }
        if (ID == 95) // Puerta Astillada
        {
            imRetrato.sprite = Evento006;
            txtTitulo.text = TRADU.i.Traducir("Puerta Astillada");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Tras una puerta atrancada, la Caravana encuentra un refugio improvisado que no resistió el ataque.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Cuando vuelven al camino, la imagen sigue pesándole.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Baja Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 81) //Cenizas en el Camino
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Cenizas en el Camino");

            txtDescripcion.text = TRADU.i.Traducir("Una ráfaga caliente levanta una espesa nube de cenizas y brasas apagadas alrededor de la Caravana.\n");
            txtDescripcion.text += TRADU.i.Traducir("Los civiles se cubren el rostro como pueden, los bueyes se inquietan y por varios instantes avanzar se vuelve peligroso.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes ordenar hacer una breve parada hasta que el aire se despeje o forzar la marcha para no perder tiempo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides esperar, el Aliento Negro avanzará.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides seguir, las cenizas incomodarán a los Civiles. -5 Esperanza, +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Esperar");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }

        // EVENTOS DE NODO BUENOS GENERICOS  201-280 Hechos: 10/80
        if (ID == 201) // Destello Esperanzador
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Destello Esperanzador");

            txtDescripcion.text = TRADU.i.Traducir("Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del puerto.\n");
            txtDescripcion.text += TRADU.i.Traducir("Quizás sea una señal, quizás casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+15 Esperanza</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 202) // Risotadas en la Caravana
        {
            imRetrato.sprite = Evento202;
            txtTitulo.text = TRADU.i.Traducir("Risotadas en la Caravana");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(new List<Personaje> { participanteEvento1 });
            retratoParticipante2.SetActive(true);
            retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;

            txtDescripcion.text = TRADU.i.Traducir("Durante la noche, <b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> y <b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b> junto con algunos Civiles comienzan a contar chistes y anécdotas divertidas, riendo y disfrutando del momento.\n");
            txtDescripcion.text += TRADU.i.Traducir("La atmósfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+5 Esperanza</b>\n\n</color>");
            txtDescripcion.text += "<color=#a0e812><b>" + participanteEvento1.sNombre + TRADU.i.Traducir(" y ") + participanteEvento2.sNombre + TRADU.i.Traducir(" ganan Alta Moral por 3 dí­as.</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 203) // Caravana perdida
        {
            imRetrato.sprite = Evento203;
            txtTitulo.text = TRADU.i.Traducir("Caravana Perdida");

            txtDescripcion.text = TRADU.i.Traducir("Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí­ y esta caravana no sobrevivió.\n");
            txtDescripcion.text += TRADU.i.Traducir("Si bien la situación es sombrí­a, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n");

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n");

            textBotonA.text = TRADU.i.Traducir("Saquear");

            textBotonB.text = TRADU.i.Traducir("Honrar");
        }
        if (ID == 204) // Aserradero Abandonado
        {
            imRetrato.sprite = Evento204;
            txtTitulo.text = TRADU.i.Traducir("Aserradero Abandonado");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n");
            txtDescripcion.text += TRADU.i.Traducir("Hay suficiente madera como para llenar un par de carros, pero juntarla toda cansará a los Civiles que participen y llevará algunas horas.\n\n\n\n");

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes juntar solo lo que está a mano y continuar sin retraso.</color> +15-26 Materiales \n\n");

            textBotonA.text = TRADU.i.Traducir("Todo");

            textBotonB.text = TRADU.i.Traducir("Un poco");
        }
        if (ID == 205) // Manada de Bueyes
        {
            imRetrato.sprite = Evento205;
            txtTitulo.text = TRADU.i.Traducir("Manada de Bueyes");
            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio(null, 2);
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            int chances = 60 + (int)(participanteEvento1.fNivelActual * 5);
            txtDescripcion.text = TRADU.i.Traducir("La Caravana se detiene en un claro donde pasta una manada de bueyes. Los animales parecen sanos y bien alimentados, pero están asustados por la presencia de la Caravana.\n");
            txtDescripcion.text += TRADU.i.Traducir("\n<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> cree que puede cazar algunos de estos Bueyes para obtener comida.  Chances: %") + chances + TRADU.i.Traducir(" <i>(Determinado por Nivel)  Exito: +50-80 Suministros +55 Experiencia.  Fallo: Recibe Herida.</i>\n\n\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes optar por dejarlo cazar, o directamente domesticar a un puñado para que se sumen a la Caravana. +2-3 Bueyes</i> \n\n");

            textBotonA.text = TRADU.i.Traducir("Cazarlos");

            textBotonB.text = TRADU.i.Traducir("Domesticarlos");
        }
        if (ID == 206) // Civiles en Apuros
        {
            imRetrato.sprite = Evento206;
            txtTitulo.text = TRADU.i.Traducir("Civiles en Apuros");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana se detiene al escuchar gritos de auxilio provenientes de un lado del camino. Al investigar encuentras a un puñado de Civiles escapando de una banda de bandidos en dirección a la Caravana.\n");
            txtDescripcion.text += TRADU.i.Traducir("'Son bandidos! no pudimos ver cuántos, pero se acercan.' - Dice un Civil aterrorizado. 'Ayúdanos'\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes defender a los civiles de sus perseguidores mientras les das tiempo a los más débiles a sumarse a la Caravana.</color> Combate Normal - +18-26 Civiles\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Puedes aceptar solo a los mas ágiles y huir para evitar confrontar con sus perseguidores.</color> +5-10 Civiles -5 Esperanza\n\n");

            textBotonA.text = TRADU.i.Traducir("Defender");

            textBotonB.text = TRADU.i.Traducir("Huir");
        }
        if (ID == 207) // Tranquilidad
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Tranquilidad");

            txtDescripcion.text = TRADU.i.Traducir("En un momento repentino, te das cuenta que hay mucha paz. Se escuchan los pasos constantes de la caravana, algún murmullo, risa y la naturaleza alrededor.\n");
            txtDescripcion.text += TRADU.i.Traducir("Estos momentos son muy escasos y sientes que cada individuo de la caravana lo valoró a su manera. \nDe alguna forma, el aire se siente más limpio.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-2 al Avance del Aliento Negro.</color>\n\n");


            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 208) // Voto de Confianza
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = TRADU.i.Traducir("Voto de Confianza");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se acerca a ti y coloca una mano en tu hombro y dice: -'Tengo mucha esperanza en usted, y creo que será exitoso al liderarnos a salvo hacia el puerto'.\n");
            txtDescripcion.text += TRADU.i.Traducir("Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un sí­mbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: " + participanteEvento1.sNombre + " gana Alta Moral por 4 dí­as y 50 Experiencia. \n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n");

            textBotonA.text = TRADU.i.Traducir("Rechazar");

            textBotonB.text = TRADU.i.Traducir("Aceptar");
        }
        if (ID == 209) // Lugareño Anciano
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Lugareño Anciano ");


            txtDescripcion.text = TRADU.i.Traducir("Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchí­simos años en la zona y la conoce a la perfección.\n");
            txtDescripcion.text += TRADU.i.Traducir("'Aliento Negro o no, mis dí­as ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>Preguntas: -'¿Conoce algún atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n");

            textBotonA.text = TRADU.i.Traducir("Atajo");

            textBotonB.text = TRADU.i.Traducir("Area");
        }
        if (ID == 210) // Sueño Inspirador
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Sueño Inspirador");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = $"A <b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se lo ve con mucha energí­a y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n");
            txtDescripcion.text += TRADU.i.Traducir("'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n");
            txtDescripcion.text += $"<color=#ba3fef><b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> obtiene 150 Experiencia y Alta Moral por 5 dí­as.</color>\n\n");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 211) // Marcas del Correo
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Marcas del Correo");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("En un poste vencido y en varias piedras cercanas aparecen marcas antiguas de correo, casi borradas por el tiempo. Todavía parece posible sacar algo útil de ese código si alguien sabe leerlo bien.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar interpretarlas antes de que se pierda la luz.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Mental DC ") + DificultadMarcasDelCorreo + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, se revelarán nodos cercanos y ganará 30 Experiencia. Si falla, la demora hará que la Caravana gane +1 Fatiga.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar interpretarlas antes de que se pierda la luz.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, hará una Tirada de Salvación: TS Mental DC 10. Si la supera, se revelarán nodos cercanos y ganará 30 Experiencia. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides seguir sin detenerte, +3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Interpretarlas");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }
        if (ID == 212) // Pulso de Mando
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = TRADU.i.Traducir("Pulso de Mando");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(participanteEvento1 != null ? new List<Personaje> { participanteEvento1 } : null);
            retratoParticipante1.SetActive(participanteEvento1 != null);
            retratoParticipante2.SetActive(participanteEvento2 != null);
            txtDescripcion.text = TRADU.i.Traducir("Un embotellamiento de carros, Civiles y animales corta el ritmo de la marcha. Todavía no es grave, pero si nadie ordena la fila con autoridad la confusión puede extenderse bastante.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b>: TS Mental DC ") + DificultadPulsoDeMando + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 30 Experiencia y la Caravana obtendrá +6 Esperanza. Si falla, la Caravana perderá 2 Esperanza.</color>\n\n");
            }

            if (participanteEvento2 != null)
            {
                retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-<b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b>: TS Mental DC ") + DificultadPulsoDeMando + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento2) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 30 Experiencia y la Caravana obtendrá +6 Esperanza. Si falla, la Caravana perderá 2 Esperanza.</color>\n\n");
                textBotonA.text = participanteEvento1 != null ? participanteEvento1.sNombre : TRADU.i.Traducir("Héroe 1");
                textBotonB.text = participanteEvento2.sNombre;
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides dejar que la fila se acomode sola, +3 Esperanza.</color>\n\n");
                textBotonA.text = TRADU.i.Traducir("Ordenar");
                textBotonB.text = TRADU.i.Traducir("Esperar");
            }
        }
        if (ID == 213) // Hombros Firmes
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Hombros Firmes");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Un Civil agotado se desploma en el camino. Nadie parece notarlo, o darle importancia alguna y pasan a su lado como si nada.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se ofrece a levantarlo y cargarlo. Pero puedes optar por ordenarle que guarde sus fuerzas para futuros peligros del camino.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Fortaleza DC ") + DificultadHombrosFirmes + TRADU.i.Traducir(" <i>(TS Fortaleza actual: ") + ObtenerTSFortalezaTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, ganará 50 Experiencia y la Caravana obtendrá +5 Esperanza. Si falla, obtendrá Fatigado.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes se ofrece a levantarlo y cargarlo. Pero puedes optar por ordenarle que guarde sus fuerzas para futuros peligros del camino.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, hará una Tirada de Salvación: TS Fortaleza DC 13. Si la supera, ganará 50 Experiencia y la Caravana obtendrá +5 Esperanza. Si falla, obtendrá Fatigado.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides dejar al Civil, -5 Esperanza. -1 Civil.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Cargarlo");
            textBotonB.text = TRADU.i.Traducir("Dejarlo");
        }
        if (ID == 214) // Manos Certeras
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Manos Certeras");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Una ráfaga arrastra una cartera de viaje con mapas, notas y referencias útiles justo hasta un borde incómodo de alcanzar. Todavía puede recuperarse, pero hace falta velocidad y precisión.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar atraparla antes de que se pierda del todo.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Reflejos DC ") + DificultadManoCierta + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, se revelarán nodos cercanos, ganará 35 Experiencia y la Caravana obtendrá +4 Esperanza. Si falla, obtendrá Herida.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar atraparla antes de que se pierda del todo.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 13. Si la supera, se revelarán nodos cercanos, ganará 35 Experiencia y la Caravana obtendrá +4 Esperanza. Si falla, obtendrá Herida.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides dejarla ir, -3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Recuperarla");
            textBotonB.text = TRADU.i.Traducir("Dejarla ir");
        }
        if (ID == 215) // Dos Miradas
        {
            imRetrato.sprite = Evento210;
            txtTitulo.text = TRADU.i.Traducir("Dos Miradas");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(participanteEvento1 != null ? new List<Personaje> { participanteEvento1 } : null);
            retratoParticipante1.SetActive(participanteEvento1 != null);
            retratoParticipante2.SetActive(participanteEvento2 != null);
            txtDescripcion.text = TRADU.i.Traducir("La ruta se abre en varias direcciones parecidas y las pocas señales útiles parecen haberse cruzado unas con otras. Dos miembros de la caravana parecen tener opiniones encontradas. ¿A quién escucharás?\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b>: TS Mental DC ") + DificultadDosMiradas + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 25 Experiencia y el Aliento Negro retrocederá 1. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
            }

            if (participanteEvento2 != null)
            {
                retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-<b><color=#d1006f>") + participanteEvento2.sNombre + TRADU.i.Traducir("</color></b>: TS Mental DC ") + DificultadDosMiradas + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento2) + TRADU.i.Traducir(").</i> Si supera la tirada, ganará 25 Experiencia y el Aliento Negro retrocederá 1. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
                textBotonA.text = participanteEvento1 != null ? participanteEvento1.sNombre : TRADU.i.Traducir("Héroe 1");
                textBotonB.text = participanteEvento2.sNombre;
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides mantener la ruta sin arriesgarte, +4 Esperanza.</color>\n\n");
                textBotonA.text = TRADU.i.Traducir("Decidir");
                textBotonB.text = TRADU.i.Traducir("Mantener la ruta");
            }
        }
        if (ID == 281) // Brote entre las Brasas
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Brote entre las Brasas");

            txtDescripcion.text = TRADU.i.Traducir("Entre los troncos calcinados y la tierra ennegrecida, algunos Civiles descubren un pequeño brote verde abriéndose paso entre las brasas frías.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La visión recorre rápidamente la Caravana. Por un instante, el Bosque Ardiente deja de parecer un lugar completamente perdido.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+10 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 282) // Madera Medio Quemada
        {
            imRetrato.sprite = Evento204;
            txtTitulo.text = TRADU.i.Traducir("Madera Medio Quemada");

            txtDescripcion.text = TRADU.i.Traducir("Al borde del camino, la Caravana encuentra restos de árboles derribados y estructuras carbonizadas. No todo quedó reducido a ceniza: parte de la madera todavía podría aprovecharse.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Algunos Civiles sugieren detenerse para separar lo útil antes de seguir adelante. Tomará algo de tiempo, pero podría reforzar las reservas de Materiales.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides recolectar, obtendrás 15-30 Materiales, pero el Aliento Negro avanzará.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides dejarlo, evitarás el retraso. +3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Recolectar");
            textBotonB.text = TRADU.i.Traducir("Dejarlo");
        }
        if (ID == 283) // Refugio de Piedra
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Refugio de Piedra");

            txtDescripcion.text = TRADU.i.Traducir("Entre varias rocas altas, la Caravana encuentra un reparo natural que corta el viento por un rato.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No dura mucho, pero alcanza para recomponerse antes de seguir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 284) // Sendero del Carnero
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Sendero del Carnero");

            txtDescripcion.text = TRADU.i.Traducir("Unas huellas frescas de carnero de montaña se internan por una cornisa estrecha que parece evitar parte del ascenso.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes seguir el rastro e intentar usar ese paso o mantener la ruta principal sin arriesgarte.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides seguir el rastro, se intentará encontrar un Atajo.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides mantener la ruta, la visión del sendero levantará el ánimo. +4 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Seguir el rastro");
            textBotonB.text = TRADU.i.Traducir("Mantener la ruta");
        }
        if (ID == 285) // Cielo Abierto
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Cielo Abierto");

            txtDescripcion.text = TRADU.i.Traducir("Por un momento, la niebla se abre y desde una altura se alcanza a ver con claridad buena parte del Paso.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La Caravana aprovecha para orientarse mejor antes de seguir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Se revelarán nodos cercanos.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 286) // Efigie Derribada
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Efigie Derribada");

            txtDescripcion.text = TRADU.i.Traducir("Una efigie Kale'Tav yace derribada a un lado del camino, partida por la mitad y cubierta de nieve.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La imagen corre rápido entre los Civiles: por una vez, algo del Paso parece menos invencible.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fuerza Kale'Tav, +5 Esperanza.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 287) // Juramento del Paso
        {
            imRetrato.sprite = Evento210;
            txtTitulo.text = TRADU.i.Traducir("Juramento del Paso");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = TRADU.i.Traducir("Frente al viento helado, <b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Frente al viento helado, uno de los Héroes se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("La determinación con la que retoma la marcha contagia al resto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Gana 50 Experiencia y Alta Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 288) // Viento a Favor
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Viento a Favor");

            txtDescripcion.text = TRADU.i.Traducir("El viento cambia de golpe y barre la niebla helada del frente por un buen trecho.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La Caravana consigue avanzar con mejor ritmo y algo más de seguridad.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Avance Aliento Negro</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 289) // Faroles Prestados
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Faroles Prestados");

            txtDescripcion.text = TRADU.i.Traducir("En puertas, postes y cercos alguien dejó faroles encendidos apuntando hacia el rumbo más seguro, como si Nedukazal todavía intentara guiar a los vivos.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La Caravana aprovecha esas luces para orientarse mejor entre caseríos arrasados y ruinas dispersas.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Se revelarán nodos cercanos.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 290) // Barricada Todavía en Pie
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Barricada Todavía en Pie");

            txtDescripcion.text = TRADU.i.Traducir("Una vieja barricada de muebles, carros y vigas todavía sigue en pie y ofrece un breve reparo contra ataques y miradas desde las ruinas.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No es segura a largo plazo, pero alcanza para que la Caravana recupere el aliento antes de seguir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 291) // Sótano con Vida
        {
            imRetrato.sprite = Evento206;
            txtTitulo.text = TRADU.i.Traducir("Sótano con Vida");

            txtDescripcion.text = TRADU.i.Traducir("Detrás de una bodega semienterrada y casi tapada por escombros encuentran a un pequeño grupo de supervivientes escondidos.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Suben a la luz temblando, pero al ver a la Caravana aceptan marcharse con ustedes.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+6-12 Civiles, +4 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 292) // Señales de Tiza
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Señales de Tiza");

            txtDescripcion.text = TRADU.i.Traducir("En cercos, muros y postes derruidos aparecen marcas de tiza hechas a toda prisa: flechas, cruces y advertencias.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Alguien estuvo guiando a otros supervivientes entre caminos, puestos y asentamientos en ruinas. Puedes seguir esas señales o reforzarlas para los que vengan detrás.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides seguirlas, se revelarán nodos cercanos.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides reforzar el camino, el gesto levantará el ánimo. +5 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Seguirlas");
            textBotonB.text = TRADU.i.Traducir("Reforzar el camino");
        }
        if (ID == 293) // Valor en la Plaza
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = TRADU.i.Traducir("Valor en la Plaza");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("El ejemplo no cambia la guerra, pero sí la forma en que retoma la marcha.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Gana 45 Experiencia y Alta Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 294) // Campamento entre Columnas
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Campamento entre Columnas");

            txtDescripcion.text = TRADU.i.Traducir("Al atravesar un viejo puesto de paso derruido, la Caravana encuentra un tramo cubierto entre columnas caídas y muros aún firmes, bastante más seguro que el terreno abierto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Por un momento, avanzar deja de sentirse como exponerse a cada sombra.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga, +3 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }

        // EVENTOS DE DESCANSO MALOS GENERICOS 101-199
        if (ID == 101) // Humo en el Campamento
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Humo en el Campamento");

            txtDescripcion.text = TRADU.i.Traducir("La leña húmeda y el viento jugaron en contra. El humo del campamento se metió entre los carros y casi nadie pudo descansar bien.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Por la mañana, hay ojos irritados, tos y bastante malhumor.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga, -3 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 102) // Guardia Somnolienta
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Guardia Somnolienta");

            txtDescripcion.text = TRADU.i.Traducir("Una parte de la guardia nocturna se quedó dormida por momentos. No pasó nada grave, pero el campamento amaneció inquieto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes despertar a más gente para reforzar la vigilancia o dejar que el resto siga durmiendo y recuperar el tiempo al amanecer.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides doblar guardia, varios caravaneros descansarán peor. +1 Fatiga.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides dejarlos dormir, la salida será más lenta. +1 Avance Aliento Negro.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Doblar guardia");
            textBotonB.text = TRADU.i.Traducir("Dejarlos dormir");
        }
        if (ID == 103) // Raciones Mojadas
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Raciones Mojadas");

            txtDescripcion.text = TRADU.i.Traducir("Durante la noche se filtró agua en uno de los carros de comida y parte de las raciones quedó inutilizable.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes extender lo salvable junto al fuego antes de partir o desecharlo y seguir adelante.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides secarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides desecharlas, perderás 18 Suministros.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Secarlas");
            textBotonB.text = TRADU.i.Traducir("Desecharlas");
        }
        if (ID == 104) // Discusión en la Fogata
        {
            imRetrato.sprite = Evento009;
            txtTitulo.text = TRADU.i.Traducir("Discusión en la Fogata");

            txtDescripcion.text = TRADU.i.Traducir("Una discusión menor cerca de la fogata fue subiendo de tono y terminó dejando al campamento entero de mal humor.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Nadie salió herido, pero el descanso se sintió más pesado de lo normal.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>-5 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 105) // Herramientas Perdidas
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Herramientas Perdidas");

            txtDescripcion.text = TRADU.i.Traducir("Al levantar el campamento, varios civiles notan que faltan herramientas básicas de trabajo. Puede que hayan quedado tiradas en la oscuridad.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes ordenar una búsqueda rápida o reemplazarlas con lo que quede en reserva.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides buscarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides reemplazarlas, perderás 12 Materiales.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Buscarlas");
            textBotonB.text = TRADU.i.Traducir("Reemplazarlas");
        }
        if (ID == 106) // Escalofríos Nocturnos
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Escalofríos Nocturnos");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Enfermo por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 107) // Noche en Vela
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Noche en Vela");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Al amanecer se lo ve agotado y le cuesta seguir el ritmo del resto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 108) // Práctica Imprudente
        {
            imRetrato.sprite = Evento009;
            txtTitulo.text = TRADU.i.Traducir("Práctica Imprudente");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Herida.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 109) // Brasas Errantes
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Brasas Errantes");

            txtDescripcion.text = TRADU.i.Traducir("El viento nocturno arrastra brasas encendidas desde los árboles caídos y obliga a mover parte del campamento una y otra vez.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Nadie duerme del todo tranquilo en el Bosque Ardiente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga, -4 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 110) // Tronco Reavivado
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Tronco Reavivado");

            txtDescripcion.text = TRADU.i.Traducir("Ya entrada la noche, un tronco que parecía apagado vuelve a encenderse cerca de los carros.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Logran contenerlo antes de que pase a mayores, pero se consumen recursos en el apuro.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>-12 Suministros</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 111) // Cánticos de Madrugada
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Cánticos de Madrugada");

            txtDescripcion.text = TRADU.i.Traducir("Antes del amanecer, unos cánticos graves atraviesan el Paso y llegan al campamento mezclados con el viento.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Nadie ve a los Kale'Tav, pero el sonido basta para que casi nadie vuelva a dormir tranquilo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga, -4 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 112) // Huellas alrededor del Campamento
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Huellas alrededor del Campamento");

            txtDescripcion.text = TRADU.i.Traducir("Al amanecer encuentran huellas frescas marcando un círculo incompleto alrededor del campamento.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No parece un ataque fallido. Más bien un mensaje. Puedes revisar bien el perímetro o mantener la calma y evitar que el rumor corra entre los Civiles.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides revisar, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides mantener la calma, los rumores igual harán mella. -3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Revisar");
            textBotonB.text = TRADU.i.Traducir("Mantener la calma");
        }
        if (ID == 113) // Vigilia Helada
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Vigilia Helada");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Al amanecer sigue en pie, pero el descanso no alcanzó para despejarle la cabeza.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Baja Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 114) // Símbolo en la Nieve
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Símbolo en la Nieve");

            txtDescripcion.text = TRADU.i.Traducir("Muy cerca de las tiendas aparece un símbolo Kale'Tav trazado durante la noche sobre la nieve endurecida.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La marca deja claro que la Caravana fue observada mientras dormía.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -2 Esperanza.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 115) // Luz Ahogada
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Luz Ahogada");

            txtDescripcion.text = TRADU.i.Traducir("El hollín, la humedad y el viento apagaron varias luces del campamento una y otra vez hasta volver la oscuridad insoportable.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Nadie llega a descansar del todo bien cuando Nedukazal vuelve a tragarse la poca luz disponible.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>+1 Fatiga, -3 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 116) // Techo Inestable
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Techo Inestable");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("La Caravana arma un descanso precario bajo el techo vencido de un galpón saqueado. Cada crujido hace mirar hacia arriba.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar asegurarlo antes de que ceda.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC ") + DificultadTechoInestable + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("Uno de los Héroes puede intentar asegurarlo antes de que ceda.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides mover el campamento, la noche será más larga. +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Asegurarlo");
            textBotonB.text = TRADU.i.Traducir("Mover campamento");
        }
        if (ID == 117) // Ruidos en la Bodega
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = TRADU.i.Traducir("Ruidos en la Bodega");

            txtDescripcion.text = TRADU.i.Traducir("Durante el descanso, desde una bodega cerrada cercana llegan golpes irregulares, respiraciones ásperas y algo arrastrándose entre cajones y barriles rotos.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes investigar antes de que eso venga hacia el campamento o atrancar la entrada y pasar la noche en tensión.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides investigar, comenzará una batalla.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides atrancar la bodega, +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Investigar");
            textBotonB.text = TRADU.i.Traducir("Atrancar");
        }
        if (ID == 118) // Lista en la Pared
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Lista en la Pared");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Después de leerla, el descanso ya no consigue apartarle esa imagen de la cabeza.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef><b>Obtiene Baja Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }

        // EVENTOS DE DESCANSO BUENOS GENERICOS 301-399
        if (ID == 301) // Noche Serena
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Noche Serena");

            txtDescripcion.text = TRADU.i.Traducir("Por una noche, el campamento se mantiene en calma. No hay sobresaltos, no hay discusiones y hasta el aire parece más liviano.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("El descanso le hace bien a la Caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 302) // Fogón Compartido
        {
            imRetrato.sprite = Evento202;
            txtTitulo.text = TRADU.i.Traducir("Fogón Compartido");

            txtDescripcion.text = TRADU.i.Traducir("Alrededor del fogón, algunos civiles y héroes comparten historias simples, comida caliente y un rato de charla.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No soluciona nada, pero por unas horas la Caravana vuelve a sentirse un poco más unida.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+6 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 303) // Manos Voluntariosas
        {
            imRetrato.sprite = Evento204;
            txtTitulo.text = TRADU.i.Traducir("Manos Voluntariosas");

            txtDescripcion.text = TRADU.i.Traducir("Antes de dormir, un grupo de civiles se ofrece a ayudar con tareas atrásadas del campamento.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes organizar una pequeña ronda de reparaciones o agradecer el gesto y dejarlos descansar.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides organizar, la Caravana ganará 15 Materiales.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides dejarlos descansar, +4 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Organizar");
            textBotonB.text = TRADU.i.Traducir("Descansar");
        }
        if (ID == 304) // Sueño Reparador
        {
            imRetrato.sprite = Evento210;
            txtTitulo.text = TRADU.i.Traducir("Sueño Reparador");

            txtDescripcion.text = TRADU.i.Traducir("El cansancio pesa, pero esta vez el campamento logra dormir sin interrupciones. Incluso quienes suelen despertarse con cualquier ruido descansan mejor.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Al amanecer, el ánimo acompaña.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga, +3 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 305) // Hallazgo entre los Carros
        {
            imRetrato.sprite = Evento203;
            txtTitulo.text = TRADU.i.Traducir("Hallazgo entre los Carros");

            txtDescripcion.text = TRADU.i.Traducir("Al ordenar los carros antes de partir, encuentran un pequeño lote de provisiones que había quedado mal inventariado.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No es mucho, pero alcanza para decidir entre guardarlo para el camino o repartirlo enseguida.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides guardarlo, +20 Suministros.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides repartirlo, +5 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Guardar");
            textBotonB.text = TRADU.i.Traducir("Repartir");
        }
        if (ID == 306) // Bolsa Olvidada
        {
            imRetrato.sprite = Evento203;
            txtTitulo.text = TRADU.i.Traducir("Bolsa Olvidada");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Dentro había un consumible todavía intacto, olvidado desde hace quién sabe cuánto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Obtienes 1 consumible.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 307) // Lección junto al Fuego
        {
            imRetrato.sprite = Evento202;
            txtTitulo.text = TRADU.i.Traducir("Lección junto al Fuego");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("La charla termina dándole una idea útil para lo que venga.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Gana 45 Experiencia.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 308) // Palabras Necesarias
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = TRADU.i.Traducir("Palabras Necesarias");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = TRADU.i.Traducir("Antes de dormir, varios Civiles se acercan a <b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> para agradecerle por lo que viene haciendo.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Antes de dormir, varios Civiles se acercan a uno de los Héroes para agradecerle por lo que viene haciendo.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("No cambia el camino, pero sí la forma en que piensa enfrentarlo al día siguiente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Obtiene Alta Moral por 4 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 309) // Calor de las Cenizas
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Calor de las Cenizas");

            txtDescripcion.text = TRADU.i.Traducir("El suelo todavía guarda un calor tenue bajo la ceniza y por una vez el descanso no se siente hostil.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("El campamento logra dormir mejor de lo esperado.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 310) // Hongos del Carbón
        {
            imRetrato.sprite = Evento203;
            txtTitulo.text = TRADU.i.Traducir("Hongos del Carbón");

            txtDescripcion.text = TRADU.i.Traducir("Entre raíces chamuscadas y troncos huecos, algunos Civiles encuentran hongos resistentes al calor todavía aprovechables.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No es un gran banquete, pero alcanza para reforzar las reservas antes de partir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+18 Suministros</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 314) // Paredón contra el Viento
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Paredón contra el Viento");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana logra montar el campamento al abrigo de un paredón de roca que corta las peores ráfagas del Paso.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Por una noche, dormir no se siente como resistir una agresión constante.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 315) // Paso en Silencio
        {
            imRetrato.sprite = Evento202;
            txtTitulo.text = TRADU.i.Traducir("Paso en Silencio");

            txtDescripcion.text = TRADU.i.Traducir("La noche cae y, contra toda costumbre del lugar, no se oyen tambores, cuervos ni cánticos a la distancia.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Ese silencio extraño no inspira confianza, pero sí regala unas horas de paz que la Caravana necesitaba.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+5 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 316) // Rastro del Rebaño
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Rastro del Rebaño");

            txtDescripcion.text = TRADU.i.Traducir("Antes de levantar el campamento, encuentran un rastro de animales que cruza una ladera más amable que la ruta habitual.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Puedes seguirlo para intentar encontrar un paso mejor o estudiarlo con calma para orientarte antes de partir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides seguirlo, se intentará encontrar un Atajo.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides estudiarlo, se revelarán nodos cercanos.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Seguirlo");
            textBotonB.text = TRADU.i.Traducir("Estudiarlo");
        }
        if (ID == 317) // Aurora del Paso
        {
            imRetrato.sprite = Evento210;
            txtTitulo.text = TRADU.i.Traducir("Aurora del Paso");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("La imagen queda grabada con fuerza y le devuelve algo de ánimo para lo que viene.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Gana 35 Experiencia y Alta Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 318) // Ventana Vigilante
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = TRADU.i.Traducir("Ventana Vigilante");

            txtDescripcion.text = TRADU.i.Traducir("Desde un alto todavía firme, parte de la guardia consigue observar caminos, corrales y techos que desde abajo resultaban imposibles de leer.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("A la mañana siguiente, la Caravana parte con una idea mucho más clara del terreno inmediato.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Se revelarán nodos cercanos.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 319) // Fogón entre Escombros
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = TRADU.i.Traducir("Fogón entre Escombros");

            txtDescripcion.text = TRADU.i.Traducir("Entre muros caídos, carretas rotas y lonas viejas, la Caravana consigue armar un fogón protegido del viento y de las miradas del terreno abierto.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("No es cómodo, pero sí lo bastante estable como para dormir mejor de lo esperado.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>-1 Fatiga</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 320) // Patio de Evacuación
        {
            imRetrato.sprite = Evento206;
            txtTitulo.text = TRADU.i.Traducir("Patio de Evacuación");

            txtDescripcion.text = TRADU.i.Traducir("Al reparo de un patio de posta, ocultos por lonas y carros volcados, encuentran a varios habitantes de Nedukazal esperando el momento para huir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Al enterarse de que la Caravana partirá al amanecer, piden sumarse antes de que los Zarkil vuelvan a cruzar la zona.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+5-10 Civiles, +3 Esperanza</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 321) // Carta sin Enviar
        {
            imRetrato.sprite = Evento210;
            txtTitulo.text = TRADU.i.Traducir("Carta sin Enviar");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n");
            }
            else
            {
                txtDescripcion.text = TRADU.i.Traducir("Uno de los Héroes encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n");
            }

            txtDescripcion.text += TRADU.i.Traducir("Leerla durante el descanso le devuelve perspectiva sobre por qué todavía vale la pena seguir.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Gana 40 Experiencia y Alta Moral por 3 días.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 311) // Comerciante Visitante
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = TRADU.i.Traducir("Comerciante Visitante");

            txtDescripcion.text = TRADU.i.Traducir("Cuando el campamento ya está armado, un comerciante rezagado se acerca a la Caravana con una mula cargada y una sonrisa cansada.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Dice que viene siguiendo el rastro del convoy desde hace días y que, si lo dejas instalarse un rato, puede abrir un pequeño puesto antes de seguir su camino.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Al continuar, se abrirá un Puesto Comercial.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 312) // Refuerzo en el Camino
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Refuerzo en el Camino");

            txtDescripcion.text = TRADU.i.Traducir("Ya entrada la noche, una figura se acerca al campamento con las manos a la vista y el equipo a cuestas.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Cuenta que perdió a su grupo en el camino y pide un lugar en la Caravana. No promete milagros, pero sí pelear mientras le queden fuerzas.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides aceptar, un Héroe aleatorio se unirá a la Caravana.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#d6d6d6>-Si decides rechazar, seguirá su camino por su cuenta.</color>");

            textBotonA.text = TRADU.i.Traducir("Aceptar");
            textBotonB.text = TRADU.i.Traducir("Rechazar");
        }
        if (ID == 313) // Mensaje desde Serria
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Mensaje desde Serria");

            txtDescripcion.text = TRADU.i.Traducir("En mitad del descanso, un ave desciende sobre uno de los carros con un mensaje atado a la pata.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("La nota viene de Serria: han enviado una misión de salvamento para asistir a la Caravana y marcan un punto de encuentro más adelante en el camino.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>Al continuar, se marcará una Misión de Salvamento en el mapa.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }

        // EVENTOS ESPECIFICOS (que no toquen al azar)  401 a ++
        if (ID == 401) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Claro");

            txtDescripcion.text = TRADU.i.Traducir("Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mí­nimo.\n");
            txtDescripcion.text += TRADU.i.Traducir("Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrá también beneficios adicionales:\n-El Aliento Negro avanzará solo 1.\n-+10% curación recibida.\n-El evento será positivo.</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");


        }
        if (ID == 402) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Asentamiento");

            txtDescripcion.text = TRADU.i.Traducir("Has llegado a un pequeño asentamiento. Notas que los civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.");
            txtDescripcion.text += TRADU.i.Traducir("\nDe 15-25 Civiles se unirán a la Caravana y brindarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.");
            txtDescripcion.text += TRADU.i.Traducir("\nUn Héroe aleatorio se sumará a tus fuerzas.\n\n\n\n\n");



            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>\nDescansar en este lugar tendrá beneficios adicionales:+20% curación recibida.</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");


        }
        if (ID == 403) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Recursos");

            txtDescripcion.text = TRADU.i.Traducir("Has llegado a un lugar rico en recursos naturales, los civiles se han puesto a recolectar lo que han podido.");

            if (CampaignManager.Instance.scAtributosZona.ID == 3)//Nedukazal
            { txtDescripcion.text += TRADU.i.Traducir("\nSe conseguirán de 25-40 Materiales y 60-85 Suministros."); }
            else
            { txtDescripcion.text += TRADU.i.Traducir("\nSe conseguirán de 18-30 Materiales y 80-140 Suministros."); }

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>\n\nDescansar en este lugar tendrá beneficios adicionales:+20% efectividad a tareas de Recolección.</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 404) //Mision de Rescate solicitada en menu descanso
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Misión de Salvamento");

            txtDescripcion.text = TRADU.i.Traducir("El ave mensajera regresa con un mensaje atado a sus patas. En él se indica el punto exacto al que la caravana deberá dirigirse para encontrarse con el equipo de salvamento, junto con los recursos cedidos por la ciudad de Serria.\n");

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>\n\nSe ha marcado en el camino adelante el nodo al cual deberáas dirigirte para encontrarte con el equipo de salvamento.</b></color>");

            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 405) //Llegar a Recursos solicitada en menu descanso
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Un encuentro esperado");

            txtDescripcion.text = TRADU.i.Traducir("Has llegado al lugar señalado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traí­do para ayudarles en su travesí­a.\n\nInmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.\n");


            int suministros = 30;
            suministros += MetaprogresionManager.Instance.SerriaTierGranjas * 10;

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>\n\nSe han entregado ") + suministros + TRADU.i.Traducir(" suministros. +25 Esperanza. +20 Materiales y 200 Oro y un nuevo personaje se suma a la caravana</b></color>");
            CampaignManager.Instance.CambiarSuministrosActuales(suministros);
            CampaignManager.Instance.CambiarMaterialesActuales(20);
            CampaignManager.Instance.CambiarOroActual(200);
            CampaignManager.Instance.CambiarEsperanzaActual(25);
            int randomHeroe = UnityEngine.Random.Range(1, 101); ;
            if (randomHeroe <= 50)
            { CampaignManager.Instance.CrearAcechador(); }
            else { CampaignManager.Instance.CrearExplorador(); }
            botonA.SetActive(false);

            textBotonB.text = TRADU.i.Traducir("Continuar");
        }

        if (ID == 16) // Siluetas entre los Arboles
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Siluetas entre los Arboles");

            txtDescripcion.text = TRADU.i.Traducir("Al costado del camino, varias siluetas se mueven entre la maleza justo fuera del alcance de la vista. Nadie logra confirmar si hay una amenaza real o solo trucos de la mente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("Los rumores corren rápido entre los carros y varios Civiles ya esperan un ataque inminente.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si ordenas cerrar filas y seguir, la Caravana obtendrá Acobardados para el próximo combate. -2 VAL a todos.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides frenar para revisar, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Cerrar filas");
            textBotonB.text = TRADU.i.Traducir("Revisar");
        }
        if (ID == 17) // Barro que Retiene
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Barro que Retiene");

            txtDescripcion.text = TRADU.i.Traducir("Un tramo de barro pegajoso se agarra a ruedas, botas y arreos. Cada metro parece costar el doble, y la columna entera empieza a moverse con una pesadez desesperante.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si fuerzas la marcha igual, la Caravana obtendrá Aletargados. El Aliento Negro avanzará +1 en el próximo viaje y la marcha se verá más lenta.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si ordenas reacomodar la marcha, la Caravana ganará +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Forzar");
            textBotonB.text = TRADU.i.Traducir("Reacomodar");
        }
        if (ID == 18) // Promesa Incumplida
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Promesa Incumplida");

            txtDescripcion.text = TRADU.i.Traducir("Encuentran un punto de espera abandonado: una manta, un fogón apagado y una señal vieja que promete ayuda que nunca llego. La escena cae pesado sobre la Caravana.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides seguir sin detenerte, la Caravana obtendrá Desmotivación. Ganará 20% menos Experiencia en el próximo combate.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si haces una breve parada para ordenar el paso, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Seguir");
            textBotonB.text = TRADU.i.Traducir("Detenerse");
        }
        if (ID == 19) // Rutina Floja
        {
            imRetrato.sprite = Evento009;
            txtTitulo.text = TRADU.i.Traducir("Rutina Floja");

            txtDescripcion.text = TRADU.i.Traducir("Tras varias horas sin sobresaltos, parte de la Caravana empieza a moverse por pura costumbre. Se aflojan formaciones, cambian relevos tarde y más de uno deja de mirar el terreno con atención.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si no dices nada, la Caravana obtendrá Descuidados por 1 viaje. -10% Exploración y +10% emboscadas.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si reorganizas puestos y ritmo, la Caravana ganará +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Dejarla");
            textBotonB.text = TRADU.i.Traducir("Ordenar");
        }
        if (ID == 119) // Pesadillas Compartidas
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Pesadillas Compartidas");

            txtDescripcion.text = TRADU.i.Traducir("Durante la noche, gritos ahogados despiertan a medio campamento. Al amanecer nadie logra explicar bien lo que soño, pero el miedo queda flotando igual entre las tiendas.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene 1 estado positivo aleatorio.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 120) // Descanso Incompleto
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Descanso Incompleto");

            txtDescripcion.text = TRADU.i.Traducir("El suelo es incomodo, el viento no afloja y los carros crujen toda la noche. Nadie descansa de verdad, y la Caravana se levanta con la sensacion de haber dormido a medias.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene 1 estado positivo aleatorio.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 121) // Quejas en Voz Baja
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Quejas en Voz Baja");

            txtDescripcion.text = TRADU.i.Traducir("Lo que empieza como murmullo termina recorriendo el campamento entero: cansancio, dudas, comparaciones con dias mejores. No hay gritos ni desbande, solo una erosion lenta del ánimo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si dejas que se descarguen, la Caravana obtendrá 1 estado positivo aleatorio.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si cortas la charla y apagas el fuego, la Caravana perdera 3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Escucharlas");
            textBotonB.text = TRADU.i.Traducir("Cortarlas");
        }
        if (ID == 122) // Fogatas Demasiado Lejos
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Fogatas Demasiado Lejos");

            txtDescripcion.text = TRADU.i.Traducir("El campamento queda armado demasiado disperso. Las fogatas no se cubren entre si, los llamados tardan en llegar y cuesta saber quien esta atento y quien no.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene 1 estado positivo aleatorio.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 216) // Arenga en la Lluvia
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Arenga en la Lluvia");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("La marcha se sostiene bajo una lluvia pesada y muda. Los Civiles avanzan con la cabeza gacha, hasta que alguien propone decir unas palabras antes de que el desaliento se vuelva costumbre.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede intentar levantar a la Caravana.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Mental DC ") + DificultadArengaEnLaLluvia + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, la Caravana obtendrá Inspiración para el próximo combate y ganará 30 Experiencia. Si falla, solo obtendrá +2 Esperanza.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, un Héroe hará una Tirada de Salvación Mental DC 11. Si la supera, la Caravana obtendrá Inspiración y ese Héroe ganará 30 Experiencia.</color>\n\n");
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides no detener la marcha, +3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Hablar");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }
        if (ID == 217) // Camino a Favor
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Camino a Favor");

            txtDescripcion.text = TRADU.i.Traducir("La Caravana encuentra un tramo de camino firme, bien orientado y sorprendentemente limpio. No durará mucho, pero alcanza para ordenar la columna y pensar en un próximo avance veloz.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si aprovechas el ritmo que da el terreno, la Caravana obtendrá Presteza. El Aliento Negro no avanzará en el próximo viaje.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si prefieres revisar bien los bordes del camino, +3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Aprovecharlo");
            textBotonB.text = TRADU.i.Traducir("Revisar");
        }
        if (ID == 218) // Juramento de la Escolta
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Juramento de la Escolta");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(participanteEvento1 != null ? new List<Personaje> { participanteEvento1 } : null);
            retratoParticipante1.SetActive(participanteEvento1 != null);
            retratoParticipante2.SetActive(participanteEvento2 != null);
            txtDescripcion.text = TRADU.i.Traducir("Antes de retomar la marcha, dos Héroes se ofrecen a formalizar delante de la Caravana un juramento sencillo: no ceder terreno mientras quede alguien a quien proteger.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
            }
            if (participanteEvento2 != null)
            {
                retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si aceptas el juramento, la Caravana obtendrá Compromiso. Ganará 20% más Experiencia en el próximo combate.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si les pides reservar fuerzas y seguir, +4 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Aceptar");
            textBotonB.text = TRADU.i.Traducir("Reservarse");
        }
        if (ID == 219) // Rastro Sospechoso
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = TRADU.i.Traducir("Rastro Sospechoso");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Unas marcas recientes junto al camino sugieren que alguien o algo estuvo siguiendo la columna desde hace rato. La noticia corre rapido entre quienes van en los carros traseros.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede leer el rastro y ordenar a tiempo la vigilancia.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Reflejos DC ") + DificultadRastroSospechoso + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, la Caravana obtendrá Vigilante por 1 viaje y ganará 30 Experiencia. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, un Héroe hará una Tirada de Salvación de Reflejos DC 10. Si la supera, la Caravana obtendrá Vigilante.</color>\n\n");
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides no detenerte, +2 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Leer el rastro");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }
        if (ID == 322) // Circulo de Historias
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Circulo de Historias");

            txtDescripcion.text = TRADU.i.Traducir("Alguien empieza a contar una historia vieja junto al fuego. Otra voz corrige un detalle, otra suma un recuerdo, y pronto media Caravana esta escuchando con una sonrisa cansada.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene Inspiración para el próximo combate. +2 VAL a todos.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 323) // Campamento Ligero
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Campamento Ligero");

            txtDescripcion.text = TRADU.i.Traducir("Sin que nadie lo ordene demasiado, el campamento se arma con lo justo y queda listo para levantarse en minutos. Hay una sensación compartida de que hoy convendrá moverse rapido.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene Presteza. El Aliento Negro no avanzará en el próximo viaje.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 324) // Repaso de Maniobras
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Repaso de Maniobras");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Antes de dormir, un Héroe propone repasar señales, posiciones y respuestas rápidas junto al fuego. No cambia el cansancio, pero podría dejar a todos mejor parados para el proximo choque.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede dirigir el repaso.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Mental DC ") + DificultadRepasoDeManiobras + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, la Caravana obtendrá Compromiso y ganará 35 Experiencia. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, un Héroe hará una Tirada de Salvación Mental DC 12. Si la supera, la Caravana obtendrá Compromiso.</color>\n\n");
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si prefieres descansar de inmediato, +2 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Repasar");
            textBotonB.text = TRADU.i.Traducir("Descansar");
        }
        if (ID == 325) // Guardias Relevados
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = TRADU.i.Traducir("Guardias Relevados");

            txtDescripcion.text = TRADU.i.Traducir("Los turnos de guardia salen mejor de lo esperado. Nadie queda de más, nadie llega tarde y el campamento entero se siente más atento sin perder descanso.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812><b>La Caravana obtiene Vigilante por 1 viaje. +10% Exploración y -10% emboscadas.</b></color>");

            botonA.SetActive(false);
            textBotonB.text = TRADU.i.Traducir("Continuar");
        }
        if (ID == 96) // Humo Inquieto
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Humo Inquieto");

            txtDescripcion.text = TRADU.i.Traducir("En el Bosque Ardiente, el humo cambia de direccion de golpe y se mete bajo telas, capuchas y lonas. La Caravana avanza entre toses y ojos llorosos, cada vez más lenta.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides avanzar igual, la Caravana obtendrá Aletargados.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si haces una parada corta hasta que abra el aire, el Aliento Negro avanzará.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Avanzar");
            textBotonB.text = TRADU.i.Traducir("Esperar");
        }
        if (ID == 97) // Cuervos del Paso
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = TRADU.i.Traducir("Cuervos del Paso");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Un circulo de cuervos se posa cerca del camino y no se mueve aunque la Caravana se acerque. Su quietud resulta peor que cualquier graznido, y el presagio corre rapido entre los Civiles.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede romper el malestar antes de que prenda.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Tirada de Salvación: TS Mental DC ") + DificultadCuervosDelPaso + TRADU.i.Traducir(" <i>(TS Mental actual: ") + ObtenerTSMentalTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, +2 Esperanza. Si falla, la Caravana obtendrá Acobardados.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si lo intentas, un Héroe hará una Tirada de Salvación Mental DC 11. Si falla, la Caravana obtendrá Acobardados.</color>\n\n");
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si decides seguir sin mirarlos, la Caravana obtendrá Acobardados.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Romper el clima");
            textBotonB.text = TRADU.i.Traducir("Seguir");
        }
        if (ID == 98) // Eco Bajo los Pies
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = TRADU.i.Traducir("Eco Bajo los Pies");

            txtDescripcion.text = TRADU.i.Traducir("En Nedukazal, un golpeteo hueco sube desde abajo de la tierra y vuelve a cortarse antes de que alguien lo ubique. La reaccion inmediata es apurar el paso, pero no todos conservan la disciplina al hacerlo.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si ordenas avanzar sin mirar atrás, la Caravana obtendrá Descuidados por 1 viaje.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#ba3fef>-Si impones una marcha mas cerrada y cauta, la Caravana ganará +1 Fatiga.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Apurar");
            textBotonB.text = TRADU.i.Traducir("Cerrar filas");
        }
        if (ID == 295) // Veta de Resina
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Veta de Resina");

            txtDescripcion.text = TRADU.i.Traducir("Una veta de resina endurecida marca un paso firme entre raices y tierra negra. La ruta apenas se sostiene, pero si la toman bien podría regalarle a la Caravana una salida rápida del sector.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si la aprovechas, la Caravana obtendrá Presteza.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si prefieres cruzar con maxima cautela, +3 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Aprovechar");
            textBotonB.text = TRADU.i.Traducir("Cautela");
        }
        if (ID == 296) // Vigia del Hielo
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = TRADU.i.Traducir("Vigia del Hielo");

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(participanteEvento1 != null);
            txtDescripcion.text = TRADU.i.Traducir("Un filo de roca y hielo ofrece un punto de vista raro en el Paso. Desde ahi, un ojo atento podría leer mejor el terreno y ordenar la marcha antes de que llegue el peligro.\n\n");
            if (participanteEvento1 != null)
            {
                retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;
                txtDescripcion.text += TRADU.i.Traducir("<b><color=#d1006f>") + participanteEvento1.sNombre + TRADU.i.Traducir("</color></b> puede trepar y vigilar desde arriba.\n\n");
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Tirada de Salvación: TS Reflejos DC ") + DificultadVigiaDelHielo + TRADU.i.Traducir(" <i>(TS Reflejos actual: ") + ObtenerTSReflejosTotal(participanteEvento1) + TRADU.i.Traducir(").</i> Si la supera, la Caravana obtendrá Vigilante y ganará 30 Experiencia. Si falla, la Caravana ganará +1 Fatiga.</color>\n\n");
            }
            else
            {
                txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si lo intentas, un Héroe hará una Tirada de Salvación de Reflejos DC 10. Si la supera, la Caravana obtendrá Vigilante.</color>\n\n");
            }
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si decides no exponer a nadie, +2 Esperanza.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Subir");
            textBotonB.text = TRADU.i.Traducir("No subir");
        }
        if (ID == 297) // Señal de los Resistentes
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = TRADU.i.Traducir("Señal de los Resistentes");

            txtDescripcion.text = TRADU.i.Traducir("En una pared semiderruida aparecen marcas recientes: no son de Zarkil ni de viejas rutas, sino señales de gente que todavía resiste y se niega a entregar el reino.\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si sigues la señal y la tomas como ejemplo, la Caravana obtendrá Compromiso.</color>\n\n");
            txtDescripcion.text += TRADU.i.Traducir("<color=#a0e812>-Si dejas una respuesta para quienes pasen después, la Caravana obtendrá Inspiración.</color>\n\n");

            textBotonA.text = TRADU.i.Traducir("Seguirla");
            textBotonB.text = TRADU.i.Traducir("Responder");
        }

        

        
   }

    Personaje participanteEvento1;
    Personaje participanteEvento2;

    int ObtenerTSReflejosTotal(Personaje personaje)
    {
        if (personaje == null)
        {
            return 0;
        }

        int total = personaje.iTSReflejo;

        if (personaje.itemArma != null) total += personaje.itemArma.buffTSReflejo;
        if (personaje.itemArmadura != null) total += personaje.itemArmadura.buffTSReflejo;
        if (personaje.Accesorio1 != null) total += personaje.Accesorio1.buffTSReflejo;
        if (personaje.Accesorio2 != null) total += personaje.Accesorio2.buffTSReflejo;
        if (personaje.TieneCampBendecido()) total += 3;

        return total;
    }

    int ObtenerTSFortalezaTotal(Personaje personaje)
    {
        if (personaje == null)
        {
            return 0;
        }

        int total = personaje.iTSFortaleza;

        if (personaje.itemArma != null) total += personaje.itemArma.buffTSFortaleza;
        if (personaje.itemArmadura != null) total += personaje.itemArmadura.buffTSFortaleza;
        if (personaje.Accesorio1 != null) total += personaje.Accesorio1.buffTSFortaleza;
        if (personaje.Accesorio2 != null) total += personaje.Accesorio2.buffTSFortaleza;
        if (personaje.TieneCampBendecido()) total += 3;

        return total;
    }

    int ObtenerTSMentalTotal(Personaje personaje)
    {
        if (personaje == null)
        {
            return 0;
        }

        int total = personaje.iTSMental;

        if (personaje.itemArma != null) total += personaje.itemArma.buffTSMental;
        if (personaje.itemArmadura != null) total += personaje.itemArmadura.buffTSMental;
        if (personaje.Accesorio1 != null) total += personaje.Accesorio1.buffTSMental;
        if (personaje.Accesorio2 != null) total += personaje.Accesorio2.buffTSMental;
        if (personaje.TieneCampBendecido()) total += 3;

        return total;
    }

    void OtorgarItemDeEvento(string itemId)
    {
        if (CampaignManager.Instance == null
            || CampaignManager.Instance.scMenuPersonajes == null
            || CampaignManager.Instance.scMenuPersonajes.scEquipo == null)
        {
            return;
        }

        ItemDatabase itemDatabase = ItemSaveCatalog.GetRuntimeItemDatabase(CampaignManager.Instance);
        Item item = ItemSaveCatalog.InstantiateItemById(itemId, itemDatabase);
        if (item == null)
        {
            Debug.LogWarning("[EventosAdmin] No se pudo otorgar el item del evento. ID: " + itemId);
            return;
        }

        CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(item.gameObject);
    }

    void CambiarFuerzaKaleTav(int delta)
    {
        if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
        {
            return;
        }

        int fuerzaActual = CampaignManager.Instance.scAtributosZona.PasoVientoHelado_FuerzaKaleTav;
        CampaignManager.Instance.scAtributosZona.PasoVientoHelado_FuerzaKaleTav = Mathf.Max(0, fuerzaActual + delta);
    }

    void OtorgarEstadoCaravana(TipoEstadoCaravana tipo, string textoLog)
    {
        if (CampaignManager.Instance == null)
        {
            return;
        }

        CampaignManager.Instance.AgregarEstadoCaravana(tipo, 1);
        if (!string.IsNullOrEmpty(textoLog))
        {
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir(textoLog));
        }
    }

    void OtorgarEstadoCaravanaPositivoAleatorio(string textoLog)
    {
        if (CampaignManager.Instance == null)
        {
            return;
        }

        TipoEstadoCaravana estado = CampaignManager.Instance.AgregarEstadoCaravanaPositivoAleatorio(1);
        if (!string.IsNullOrEmpty(textoLog))
        {
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir(textoLog) + " +" + EstadosCaravana.ObtenerNombreVisible(estado) + ".");
        }
    }

    void IntentarAbrirPuestoComercialDesdeEvento()
    {
        gameObject.SetActive(false);
        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.AbrirPuestoComercial();
        }
    }

    void IntentarMarcarMisionSalvamentoDesdeEvento()
    {
        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.IntentarCrearMisionSalvamentoEnMapa();
        }

        gameObject.SetActive(false);
    }

    public void ElegirOpcionA()
    {
        // Eventos de nodo malos
        if (eventoActual == 3)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            CampaignManager.Instance.CambiarBueyesActuales(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 4)
        {
            CampaignManager.Instance.CambiarOroActual((int)(CampaignManager.Instance.GetCivilesActual() * -1));
            gameObject.SetActive(false);
        }
        if (eventoActual == 5)
        {
            CampaignManager.Instance.CambiarOroActual(-45);

            float tierCuranderos = ((CampaignManager.Instance.sequitoCuranderosMejoraCuracion * 100) - 10) / 5;
            int rand =UnityEngine.Random.Range(3, 7);
            participanteEvento1.Camp_Enfermo += rand - (int)tierCuranderos;
            gameObject.SetActive(false);
        }
        if (eventoActual == 6)
        {
            int random =UnityEngine.Random.Range(1, 101);
            int seguridad = 40 + (int)CampaignManager.Instance.GetMiliciasActual();
            if (random <= seguridad) //Encuentra al culpable
            {

                CampaignManager.Instance.CambiarCivilesActuales(-1);
                CampaignManager.Instance.CambiarEsperanzaActual(-5);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Has encontrado al ladrón y recuperado el oro robado, pero has tenido que desterrar al ladrón. -5 Esperanza -1 Civil."));
            }
            else
            {
                int oroRobado = CampaignManager.Instance.GetOroActuales() / 4; //25% del oro actual
                if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
                CampaignManager.Instance.CambiarOroActual(-oroRobado);
                CampaignManager.Instance.CambiarEsperanzaActual(-5);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-No has logrado encontrar al ladrón y se perdieron ") + oroRobado + TRADU.i.Traducir(" de oro. -5 Esperanza por el interrogatorio"));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 7)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(-20);
            gameObject.SetActive(false);
        }
        if (eventoActual == 8)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-15);
            gameObject.SetActive(false);
        }
        if (eventoActual == 9)
        {
            participanteEvento2.Camp_Moral -= 5; //Baja Moral
            gameObject.SetActive(false);
        }
        if (eventoActual == 10)
        {
            int chances = 35 + (int)CampaignManager.Instance.GetEsperanzaActual() / 3;
            if(UnityEngine.Random.Range(1, 101) <= chances)
            {
                CampaignManager.Instance.CambiarEsperanzaActual(15);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza"));
            }
            else
            {
                CampaignManager.Instance.CambiarEsperanzaActual(-20);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza."));
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 11)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadPasoPrecario && participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(35);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadPasoPrecario + TRADU.i.Traducir(") y logró guiar a la Caravana por el paso precario. +35 Experiencia."));
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadPasoPrecario + TRADU.i.Traducir(") y sufrió una Herida al intentar guiar a la Caravana."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 12)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsFortaleza = ObtenerTSFortalezaTotal(participanteEvento1);
            int resultado = tirada + tsFortaleza;

            if (resultado >= DificultadAireEnrarecido && participanteEvento1 != null)
            {
                int civiles = UnityEngine.Random.Range(6, 11);
                participanteEvento1.RecibirExperiencia(30);
                CampaignManager.Instance.CambiarCivilesActuales(civiles);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Fortaleza (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                    + DificultadAireEnrarecido + TRADU.i.Traducir("), logró entrar en la bodega y rescató ")
                    + civiles + TRADU.i.Traducir(" Civiles. +30 Experiencia."));
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Enfermo += 3;
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Fortaleza (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                    + DificultadAireEnrarecido + TRADU.i.Traducir(") y quedó Enfermo por 3 días tras respirar el aire enrarecido."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 13)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsMental = ObtenerTSMentalTotal(participanteEvento1);
            int resultado = tirada + tsMental;

            if (resultado >= DificultadRumorDeDesbande && participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(35);
                CampaignManager.Instance.CambiarEsperanzaActual(4);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsMental + TRADU.i.Traducir(" vs DC ")
                    + DificultadRumorDeDesbande + TRADU.i.Traducir(") y logró apagar el rumor antes de que creciera. +35 Experiencia, +4 Esperanza."));
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Moral -= 3;
                CampaignManager.Instance.CambiarEsperanzaActual(-5);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación Mental (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsMental + TRADU.i.Traducir(" vs DC ")
                    + DificultadRumorDeDesbande + TRADU.i.Traducir("). El rumor de desbande se agravó. Baja Moral por 3 días, -5 Esperanza."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 14)
        {
            Personaje participante = participanteEvento1;
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participante);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadVadoTraicionero && participante != null)
            {
                participante.RecibirExperiencia(40);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadVadoTraicionero + TRADU.i.Traducir(") y logró ordenar el cruce del vado. +40 Experiencia."));
            }
            else if (participante != null)
            {
                participante.Camp_Herido = true;
                CampaignManager.Instance.CambiarBueyesActuales(-1);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadVadoTraicionero + TRADU.i.Traducir("). Sufrió una Herida y la Caravana perdió 1 Buey en el vado."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 15)
        {
            Personaje participante = participanteEvento1;
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsFortaleza = ObtenerTSFortalezaTotal(participante);
            int resultado = tirada + tsFortaleza;

            if (resultado >= DificultadCarroEncajado && participante != null)
            {
                participante.RecibirExperiencia(35);
                CampaignManager.Instance.CambiarEsperanzaActual(3);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Fortaleza (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                    + DificultadCarroEncajado + TRADU.i.Traducir("), destrabó el carro y sostuvo el ánimo de la Caravana. +35 Experiencia, +3 Esperanza."));
            }
            else if (participante != null)
            {
                participante.Camp_Herido = true;
                CampaignManager.Instance.CambiarFatigaActual(1);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Fortaleza (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                    + DificultadCarroEncajado + TRADU.i.Traducir("). Sufrió una Herida al intentar mover el carro y la Caravana ganó +1 Fatiga."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 82)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadBestiasAterradas)
            {
                participanteEvento1.RecibirExperiencia(40);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadBestiasAterradas + TRADU.i.Traducir(") y logró contener a las bestias aterradas. +40 Experiencia."));
            }
            else
            {
                CampaignManager.Instance.CambiarBueyesActuales(-2);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadBestiasAterradas + TRADU.i.Traducir("). Las bestias aterradas desataron el caos y la Caravana perdió 2 Bueyes."));
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 83)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(-15);
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 84)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 85)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadHieloQuebradizo)
            {
                if (participanteEvento1 != null)
                {
                    participanteEvento1.RecibirExperiencia(40);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadHieloQuebradizo + TRADU.i.Traducir(") y logró guiar a la Caravana por el hielo. +40 Experiencia."));
                }
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadHieloQuebradizo + TRADU.i.Traducir(") y sufrió una Herida al quebrarse el hielo."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 91)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadBrechaEnLaCalzada)
            {
                if (participanteEvento1 != null)
                {
                    participanteEvento1.RecibirExperiencia(40);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadBrechaEnLaCalzada + TRADU.i.Traducir(") y logró guiar a la Caravana por la brecha. +40 Experiencia."));
                }
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadBrechaEnLaCalzada + TRADU.i.Traducir(") y sufrió una Herida al intentar guiar a la Caravana por la brecha."));
            }

            gameObject.SetActive(false);
        }
        if (eventoActual == 87)
        {
            gameObject.SetActive(false);
            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(0, 0);
        }
        if (eventoActual == 92)
        {
            gameObject.SetActive(false);
            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(0, 0);
        }
        if (eventoActual == 81)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 94)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 282)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(UnityEngine.Random.Range(15, 31));
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 284)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.EncontrarAtajo(2, 3);
            gameObject.SetActive(false);
        }

        // Eventos de nodo buenos
        if (eventoActual == 203)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(UnityEngine.Random.Range(21, 36));
            CampaignManager.Instance.CambiarMaterialesActuales(UnityEngine.Random.Range(5, 12));
            CampaignManager.Instance.CambiarOroActual(UnityEngine.Random.Range(15, 36));
            CampaignManager.Instance.CambiarEsperanzaActual(-5);

            gameObject.SetActive(false);
        }
        if (eventoActual == 204)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(UnityEngine.Random.Range(65, 90));
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            CampaignManager.Instance.CambiarFatigaActual(1);

            gameObject.SetActive(false);
        }
        if (eventoActual == 205)
        {
            int chances = 60 + (int)(participanteEvento1.fNivelActual * 5);
            if(UnityEngine.Random.Range(1, 101) <= chances)
            {
                int rand =UnityEngine.Random.Range(50, 80);
                CampaignManager.Instance.CambiarSuministrosActuales(rand);
                participanteEvento1.RecibirExperiencia(55);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La cacerí­a de ") + participanteEvento1.sNombre + TRADU.i.Traducir(" ha sido exitosa. +") + rand.ToString() + TRADU.i.Traducir(" Suministros +55 Experiencia."));
            }
            else
            {
                participanteEvento1.Camp_Herido = true;
                participanteEvento1.fVidaActual -= participanteEvento1.fVidaActual * 0.6f;
                CampaignManager.Instance.EscribirLog("-" + participanteEvento1.sNombre + TRADU.i.Traducir(" sufrió un accidente durante la cacerí­a. Herido."));
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 206)
        {
            gameObject.SetActive(false);

            CampaignManager.Instance.CambiarCivilesActuales(UnityEngine.Random.Range(18, 27));
            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(-206, 0); // Forzar encuentro de facción Bandidos
        }
        if (eventoActual == 208)
        {
            participanteEvento1.RecibirExperiencia(55);
            participanteEvento1.Camp_Moral += 4;

            gameObject.SetActive(false);

        }
        if (eventoActual == 209)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.EncontrarAtajo(2, 3);

            gameObject.SetActive(false);

        }
        if (eventoActual == 210)
        {
            
            participanteEvento1.RecibirExperiencia(150);
            participanteEvento1.Camp_Moral += 5;
            gameObject.SetActive(false);

        }
        if (eventoActual == 211)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsMental = ObtenerTSMentalTotal(participanteEvento1);
            int resultado = tirada + tsMental;

            if (resultado >= DificultadMarcasDelCorreo && participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(30);
                CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsMental + TRADU.i.Traducir(" vs DC ")
                    + DificultadMarcasDelCorreo + TRADU.i.Traducir("), interpretó las marcas del correo y reveló nodos cercanos. +30 Experiencia."));
            }
            else
            {
                CampaignManager.Instance.CambiarFatigaActual(1);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 212)
        {
            Personaje participante = participanteEvento1;
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsMental = ObtenerTSMentalTotal(participante);
            int resultado = tirada + tsMental;

            if (resultado >= DificultadPulsoDeMando && participante != null)
            {
                participante.RecibirExperiencia(30);
                CampaignManager.Instance.CambiarEsperanzaActual(6);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsMental + TRADU.i.Traducir(" vs DC ")
                    + DificultadPulsoDeMando + TRADU.i.Traducir("), recompuso la fila y levantó el ánimo de la Caravana. +30 Experiencia, +6 Esperanza."));
            }
            else
            {
                CampaignManager.Instance.CambiarEsperanzaActual(-2);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 213)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsFortaleza = participanteEvento1 != null ? ObtenerTSFortalezaTotal(participanteEvento1) : 0;
            int resultado = tirada + tsFortaleza;

            if (resultado >= DificultadHombrosFirmes && participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(50);
                CampaignManager.Instance.CambiarEsperanzaActual(5);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Fortaleza (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                    + DificultadHombrosFirmes + TRADU.i.Traducir("), cargó al Civil durante la marcha. +50 Experiencia, +5 Esperanza."));
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.SetCampFatigado(true);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 214)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadManoCierta && participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(35);
                CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
                CampaignManager.Instance.CambiarEsperanzaActual(4);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadManoCierta + TRADU.i.Traducir("), recuperó la cartera de viaje y reveló nodos cercanos. +35 Experiencia, +4 Esperanza."));
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;

                 CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadManoCierta + TRADU.i.Traducir("), y ha sufrido una herida."));
            
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 215)
        {
            Personaje participante = participanteEvento1;
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsMental = ObtenerTSMentalTotal(participante);
            int resultado = tirada + tsMental;

            if (resultado >= DificultadDosMiradas && participante != null)
            {
                participante.RecibirExperiencia(25);
                CampaignManager.Instance.CambiarValorAlientoNegro(-1);
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participante.sNombre
                    + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsMental + TRADU.i.Traducir(" vs DC ")
                    + DificultadDosMiradas + TRADU.i.Traducir("), eligió bien la ruta y el Aliento Negro retrocedió 1. +25 Experiencia."));
            }
            else
            {
                CampaignManager.Instance.CambiarFatigaActual(1);
            }
            gameObject.SetActive(false);
        }

        // Eventos de descanso malos
        if (eventoActual == 102)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 103)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 105)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 112)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 116)
        {
            int tirada = UnityEngine.Random.Range(1, 21);
            int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
            int resultado = tirada + tsReflejos;

            if (resultado >= DificultadTechoInestable)
            {
                if (participanteEvento1 != null)
                {
                    participanteEvento1.RecibirExperiencia(35);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadTechoInestable + TRADU.i.Traducir(") y logró asegurar el techo. +35 Experiencia."));
                }
            }
            else if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;
                CampaignManager.Instance.EscribirLog(
                    TRADU.i.Traducir("-") + participanteEvento1.sNombre
                    + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                    + tirada + TRADU.i.Traducir(" + ")
                    + tsReflejos + TRADU.i.Traducir(" vs DC ")
                    + DificultadTechoInestable + TRADU.i.Traducir(") y sufrió una Herida por el derrumbe del techo."));
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 117)
        {
            gameObject.SetActive(false);
            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(0, 0);
        }

        // Eventos de descanso buenos
        if (eventoActual == 303)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(15);
            gameObject.SetActive(false);
        }
        if (eventoActual == 305)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(20);
            gameObject.SetActive(false);
        }
        if (eventoActual == 316)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.EncontrarAtajo(2, 3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 318)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
            gameObject.SetActive(false);
        }
        if (eventoActual == 16)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Acobardados, "-La Caravana cerró filas, pero el miedo quedó instalado. Acobardados para el próximo combate.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 17)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Aletargados, "-La Caravana forzó el paso entre el barro y quedó Aletargada.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 18)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Desmotivacion, "-La escena de la promesa incumplida dejó a la Caravana desmotivada.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 19)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Descuidados, "-La rutina floja se impuso y la Caravana quedó Descuidados por 1 viaje.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 121)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Desmotivacion, "-Las quejas en voz baja drenaron el ánimo. Desmotivación para el próximo combate.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 216)
        {
            if (participanteEvento1 != null)
            {
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsMental = ObtenerTSMentalTotal(participanteEvento1);
                int resultado = tirada + tsMental;
                if (resultado >= DificultadArengaEnLaLluvia)
                {
                    participanteEvento1.RecibirExperiencia(30);
                    OtorgarEstadoCaravana(TipoEstadoCaravana.Inspiracion,
                        "-" + participanteEvento1.sNombre + TRADU.i.Traducir(" sostuvo la arenga bajo la lluvia (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ") + tsMental + TRADU.i.Traducir(" vs DC ")
                        + DificultadArengaEnLaLluvia + TRADU.i.Traducir("). +30 Experiencia y Inspiración para el próximo combate."));
                }
                else
                {
                    CampaignManager.Instance.CambiarEsperanzaActual(2);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" no logró encender a todos con su arenga (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsMental + TRADU.i.Traducir(" vs DC ")
                        + DificultadArengaEnLaLluvia + TRADU.i.Traducir("), pero la Caravana sostuvo el ánimo. +2 Esperanza."));
                }
            }
            else
            {
                OtorgarEstadoCaravana(TipoEstadoCaravana.Inspiracion, "-La arenga logró encender a la Caravana. Inspiración para el próximo combate.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 217)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Presteza, "-La Caravana aprovechó el buen tramo de camino y obtuvo Presteza.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 218)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Compromiso, "-El juramento de la escolta reforzó el Compromiso de la Caravana.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 219)
        {
            if (participanteEvento1 != null)
            {
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
                int resultado = tirada + tsReflejos;
                if (resultado >= DificultadRastroSospechoso)
                {
                    participanteEvento1.RecibirExperiencia(30);
                    OtorgarEstadoCaravana(TipoEstadoCaravana.Vigilante,
                        "-" + participanteEvento1.sNombre + TRADU.i.Traducir(" leyo el rastro a tiempo (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ") + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadRastroSospechoso + TRADU.i.Traducir("). +30 Experiencia y Vigilante por 1 viaje."));
                }
                else
                {
                    CampaignManager.Instance.CambiarFatigaActual(1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" no logró leer bien el rastro (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadRastroSospechoso + TRADU.i.Traducir("). +1 Fatiga."));
                }
            }
            else
            {
                OtorgarEstadoCaravana(TipoEstadoCaravana.Vigilante, "-La Caravana ajusto la vigilancia tras ver el rastro.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 324)
        {
            if (participanteEvento1 != null)
            {
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsMental = ObtenerTSMentalTotal(participanteEvento1);
                int resultado = tirada + tsMental;
                if (resultado >= DificultadRepasoDeManiobras)
                {
                    participanteEvento1.RecibirExperiencia(35);
                    OtorgarEstadoCaravana(TipoEstadoCaravana.Compromiso,
                        "-" + participanteEvento1.sNombre + TRADU.i.Traducir(" dirigio un repaso de maniobras util (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ") + tsMental + TRADU.i.Traducir(" vs DC ")
                        + DificultadRepasoDeManiobras + TRADU.i.Traducir("). +35 Experiencia y Compromiso."));
                }
                else
                {
                    CampaignManager.Instance.CambiarFatigaActual(1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" no logró ordenar bien el repaso de maniobras. +1 Fatiga."));
                }
            }
            else
            {
                OtorgarEstadoCaravana(TipoEstadoCaravana.Compromiso, "-El repaso de maniobras dejó a la Caravana con más Compromiso.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 96)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Aletargados, "-El humo inquieto dejó a la Caravana Aletargada.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 97)
        {
            if (participanteEvento1 != null)
            {
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsMental = ObtenerTSMentalTotal(participanteEvento1);
                int resultado = tirada + tsMental;
                if (resultado >= DificultadCuervosDelPaso)
                {
                    CampaignManager.Instance.CambiarEsperanzaActual(2);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" rompio el mal augurio de los cuervos. +2 Esperanza."));
                }
                else
                {
                    OtorgarEstadoCaravana(TipoEstadoCaravana.Acobardados,
                        "-" + participanteEvento1.sNombre + TRADU.i.Traducir(" no logró cortar el malestar de la Caravana. Acobardados para el próximo combate."));
                }
            }
            else
            {
                OtorgarEstadoCaravana(TipoEstadoCaravana.Acobardados, "-Los cuervos del paso dejaron a la Caravana Acobardada.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 98)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Descuidados, "-El eco bajo los pies desordeno la marcha. Descuidados por 1 viaje.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 295)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Presteza, "-La veta de resina permitió ordenar una salida rápida. Presteza.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 296)
        {
            if (participanteEvento1 != null)
            {
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsReflejos = ObtenerTSReflejosTotal(participanteEvento1);
                int resultado = tirada + tsReflejos;
                if (resultado >= DificultadVigiaDelHielo)
                {
                    participanteEvento1.RecibirExperiencia(30);
                    OtorgarEstadoCaravana(TipoEstadoCaravana.Vigilante,
                        "-" + participanteEvento1.sNombre + TRADU.i.Traducir(" vigiló desde el hielo alto y ordeno la marcha. +30 Experiencia y Vigilante."));
                }
                else
                {
                    CampaignManager.Instance.CambiarFatigaActual(1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participanteEvento1.sNombre
                        + TRADU.i.Traducir(" bajo agotado del filo helado. +1 Fatiga."));
                }
            }
            else
            {
                OtorgarEstadoCaravana(TipoEstadoCaravana.Vigilante, "-La Caravana logró una mejor vigilancia desde el hielo.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 297)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Compromiso, "-La señal de los resistentes reforzó el Compromiso de la Caravana.");
            gameObject.SetActive(false);
        }

    }
    public void ElegirOpcionB()
    {
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && (CampaignManager.Instance.scTutorialManager.pasoActual == 12 || CampaignManager.Instance.scTutorialManager.pasoActual == 22))
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
        }
        // Eventos de nodo malos
        if (eventoActual == 1)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 2)
        {
            int civilesperdidos = UnityEngine.Random.Range(4, 13);
            CampaignManager.Instance.CambiarCivilesActuales(-civilesperdidos);
            CampaignManager.Instance.CambiarEsperanzaActual(-5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 3)
        {
            int bueyesperdidos = 1 + UnityEngine.Random.Range(1, 4);
            CampaignManager.Instance.CambiarBueyesActuales(-bueyesperdidos);
            gameObject.SetActive(false);
        }
        if (eventoActual == 4)
        {
            gameObject.SetActive(false);

            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(502, 0);
        }
        if (eventoActual == 5)
        {
            float tierCuranderos = ((CampaignManager.Instance.sequitoCuranderosMejoraCuracion * 100) - 10) / 5;
            int rand = UnityEngine.Random.Range(4, 8);
            participanteEvento1.Camp_Enfermo += rand - (int)tierCuranderos;
            gameObject.SetActive(false);
        }
        if (eventoActual == 6)
        {

            int oroRobado = CampaignManager.Instance.GetOroActuales() / 4; //25% del oro actual
            if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
            CampaignManager.Instance.CambiarOroActual(-oroRobado);
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-No has logrado encontrar al ladrón y se perdieron ") + oroRobado + TRADU.i.Traducir(" de oro. -5 Esperanza por el interrogatorio"));


            gameObject.SetActive(false);
        }
        if (eventoActual == 7)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(-60);
            gameObject.SetActive(false);
        }
        if (eventoActual == 8)
        {
            int chancesCont = 30 + (int)CampaignManager.Instance.GetValorAlientoNegro() * 3;
            if (UnityEngine.Random.Range(1, 101) <= chancesCont)
            {
                int civilesPerdidos = 25;
                CampaignManager.Instance.CambiarCivilesActuales(-civilesPerdidos);
                CampaignManager.Instance.CambiarEsperanzaActual(-10);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Civiles se han contaminado y han muerto ") + civilesPerdidos + TRADU.i.Traducir(" Civiles. -10 Esperanza"));
            }
            else
            {
                CampaignManager.Instance.CambiarFatigaActual(-1);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Civiles han descansado en el rí­o y se han refrescado. -1 Fatiga "));
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 9)
        {
            participanteEvento1.Camp_Moral -= 5; //Baja Moral
            gameObject.SetActive(false);
        }
        if (eventoActual == 10)
        {

            CampaignManager.Instance.CambiarEsperanzaActual(-10);
            CampaignManager.Instance.CambiarOroActual(-65);
            CampaignManager.Instance.CambiarCivilesActuales(-8);

            gameObject.SetActive(false);
        }
        if (eventoActual == 11)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 12)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 13)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 14)
        {
            if (participanteEvento2 != null)
            {
                Personaje participante = participanteEvento2;
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsReflejos = ObtenerTSReflejosTotal(participante);
                int resultado = tirada + tsReflejos;

                if (resultado >= DificultadVadoTraicionero)
                {
                    participante.RecibirExperiencia(40);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación de Reflejos (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadVadoTraicionero + TRADU.i.Traducir(") y logró ordenar el cruce del vado. +40 Experiencia."));
                }
                else
                {
                    participante.Camp_Herido = true;
                    CampaignManager.Instance.CambiarBueyesActuales(-1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" falló su Tirada de Salvación de Reflejos (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsReflejos + TRADU.i.Traducir(" vs DC ")
                        + DificultadVadoTraicionero + TRADU.i.Traducir("). Sufrió una Herida y la Caravana perdió 1 Buey en el vado."));
                }
            }
            else
            {
                CampaignManager.Instance.CambiarValorAlientoNegro(1);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 15)
        {
            if (participanteEvento2 != null)
            {
                Personaje participante = participanteEvento2;
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsFortaleza = ObtenerTSFortalezaTotal(participante);
                int resultado = tirada + tsFortaleza;

                if (resultado >= DificultadCarroEncajado)
                {
                    participante.RecibirExperiencia(35);
                    CampaignManager.Instance.CambiarEsperanzaActual(3);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación de Fortaleza (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                        + DificultadCarroEncajado + TRADU.i.Traducir("), destrabó el carro y sostuvo el ánimo de la Caravana. +35 Experiencia, +3 Esperanza."));
                }
                else
                {
                    participante.Camp_Herido = true;
                    CampaignManager.Instance.CambiarFatigaActual(1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" falló su Tirada de Salvación de Fortaleza (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsFortaleza + TRADU.i.Traducir(" vs DC ")
                        + DificultadCarroEncajado + TRADU.i.Traducir("). Sufrió una Herida al intentar mover el carro y la Caravana ganó +1 Fatiga."));
                }
            }
            else
            {
                CampaignManager.Instance.CambiarFatigaActual(1);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 82)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 83)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(-UnityEngine.Random.Range(15, 26));
            gameObject.SetActive(false);
        }
        if (eventoActual == 84)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 85)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 90)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 91)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 92)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 93)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-6);
            gameObject.SetActive(false);
        }
        if (eventoActual == 94)
        {
            CampaignManager.Instance.CambiarCivilesActuales(-3);
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 95)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Moral -= 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 87)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 81)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-5);
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }


        // Eventos de nodo buenos
        if (eventoActual == 201)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(15);
            gameObject.SetActive(false);
        }
        if (eventoActual == 202)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            participanteEvento1.Camp_Moral += 3; //Buena Moral
            participanteEvento2.Camp_Moral += 3; //Buena Moral
            gameObject.SetActive(false);
        }
        if (eventoActual == 203)
        {


            CampaignManager.Instance.CambiarEsperanzaActual(15);

            gameObject.SetActive(false);
        }
        if (eventoActual == 204)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(UnityEngine.Random.Range(15, 26));
            gameObject.SetActive(false);
        }
        if (eventoActual == 205)
        {
            CampaignManager.Instance.CambiarBueyesActuales(UnityEngine.Random.Range(2, 4));
            gameObject.SetActive(false);
        }
        if (eventoActual == 206)
        {
            gameObject.SetActive(false);

            CampaignManager.Instance.CambiarCivilesActuales(UnityEngine.Random.Range(5, 11));
            CampaignManager.Instance.CambiarEsperanzaActual(-5);

        }
        if (eventoActual == 207)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(-2);
            gameObject.SetActive(false);
        }
        if (eventoActual == 208)
        {
            CampaignManager.Instance.CambiarOroActual(UnityEngine.Random.Range(120, 161));
            gameObject.SetActive(false);

        }
        if (eventoActual == 209)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);

            gameObject.SetActive(false);

        }
        if (eventoActual == 211)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 212)
        {
            if (participanteEvento2 != null)
            {
                Personaje participante = participanteEvento2;
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsMental = ObtenerTSMentalTotal(participante);
                int resultado = tirada + tsMental;

                if (resultado >= DificultadPulsoDeMando)
                {
                    participante.RecibirExperiencia(30);
                    CampaignManager.Instance.CambiarEsperanzaActual(6);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsMental + TRADU.i.Traducir(" vs DC ")
                        + DificultadPulsoDeMando + TRADU.i.Traducir("), recompuso la fila y levantó el ánimo de la Caravana. +30 Experiencia, +6 Esperanza."));
                }
                else
                {
                    CampaignManager.Instance.CambiarEsperanzaActual(-2);
                }
            }
            else
            {
                CampaignManager.Instance.CambiarEsperanzaActual(3);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 213)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-5);
            CampaignManager.Instance.CambiarCivilesActuales(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 214)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 215)
        {
            if (participanteEvento2 != null)
            {
                Personaje participante = participanteEvento2;
                int tirada = UnityEngine.Random.Range(1, 21);
                int tsMental = ObtenerTSMentalTotal(participante);
                int resultado = tirada + tsMental;

                if (resultado >= DificultadDosMiradas)
                {
                    participante.RecibirExperiencia(25);
                    CampaignManager.Instance.CambiarValorAlientoNegro(-1);
                    CampaignManager.Instance.EscribirLog(
                        TRADU.i.Traducir("-") + participante.sNombre
                        + TRADU.i.Traducir(" superó su Tirada de Salvación Mental (1d20: ")
                        + tirada + TRADU.i.Traducir(" + ")
                        + tsMental + TRADU.i.Traducir(" vs DC ")
                        + DificultadDosMiradas + TRADU.i.Traducir("), eligió bien la ruta y el Aliento Negro retrocedió 1. +25 Experiencia."));
                }
                else
                {
                    CampaignManager.Instance.CambiarFatigaActual(1);
                }
            }
            else
            {
                CampaignManager.Instance.CambiarEsperanzaActual(4);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 281)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(10);
            gameObject.SetActive(false);
        }
        if (eventoActual == 282)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 284)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 289)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
            gameObject.SetActive(false);
        }
        if (eventoActual == 291)
        {
            CampaignManager.Instance.CambiarCivilesActuales(UnityEngine.Random.Range(6, 13));
            CampaignManager.Instance.CambiarEsperanzaActual(4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 292)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
            gameObject.SetActive(false);
        }

        // Eventos de descanso malos
        if (eventoActual == 86)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-6);
            gameObject.SetActive(false);
        }
        if (eventoActual == 88)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Enfermo += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 89)
        {
            CambiarFuerzaKaleTav(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 283)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 285)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
            gameObject.SetActive(false);
        }
        if (eventoActual == 286)
        {
            CambiarFuerzaKaleTav(-1);
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 287)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(50);
                participanteEvento1.Camp_Moral += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 288)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 290)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 292)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 293)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(45);
                participanteEvento1.Camp_Moral += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 294)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }

        // Eventos de descanso malos
        if (eventoActual == 101)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 102)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 103)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(-18);
            gameObject.SetActive(false);
        }
        if (eventoActual == 104)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 105)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(-12);
            gameObject.SetActive(false);
        }
        if (eventoActual == 106)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Enfermo += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 107)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 108)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Herido = true;
            }
            gameObject.SetActive(false);
        }

        // Eventos de descanso buenos
        if (eventoActual == 301)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 302)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(6);
            gameObject.SetActive(false);
        }
        if (eventoActual == 303)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 304)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 305)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 306)
        {
            OtorgarItemDeEvento(ItemIdBolsaOlvidada);
            gameObject.SetActive(false);
        }
        if (eventoActual == 307)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(45);
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 308)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Moral += 4;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 312)
        {
            CampaignManager.Instance.AgregarHeroe(0);
            gameObject.SetActive(false);
        }
        if (eventoActual == 109)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 110)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(-12);
            gameObject.SetActive(false);
        }
        if (eventoActual == 112)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 111)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 113)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Moral -= 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 114)
        {
            CambiarFuerzaKaleTav(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-2);
            gameObject.SetActive(false);
        }
        if (eventoActual == 115)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 116)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 117)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 118)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.Camp_Moral -= 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 309)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 310)
        {
            CampaignManager.Instance.CambiarSuministrosActuales(18);
            gameObject.SetActive(false);
        }
        if (eventoActual == 314)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 315)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            gameObject.SetActive(false);
        }
        if (eventoActual == 316)
        {
            CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100, true);
            gameObject.SetActive(false);
        }
        if (eventoActual == 317)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(35);
                participanteEvento1.Camp_Moral += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 319)
        {
            CampaignManager.Instance.CambiarFatigaActual(-1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 320)
        {
            CampaignManager.Instance.CambiarCivilesActuales(UnityEngine.Random.Range(5, 11));
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 321)
        {
            if (participanteEvento1 != null)
            {
                participanteEvento1.RecibirExperiencia(40);
                participanteEvento1.Camp_Moral += 3;
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 312)
        {
            gameObject.SetActive(false);
        }
        if (eventoActual == 311)
        {
            IntentarAbrirPuestoComercialDesdeEvento();
        }
        if (eventoActual == 313)
        {
            IntentarMarcarMisionSalvamentoDesdeEvento();
        }
        if (eventoActual == 16)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 17)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 18)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 19)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 119)
        {
            OtorgarEstadoCaravanaPositivoAleatorio("-La noche dejó a la Caravana reforzada.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 120)
        {
            OtorgarEstadoCaravanaPositivoAleatorio("-El descanso incompleto terminó templando a la Caravana.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 121)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(-3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 122)
        {
            OtorgarEstadoCaravanaPositivoAleatorio("-La reorganización del campamento dejó una enseñanza útil en la Caravana.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 216)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 217)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 218)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(4);
            gameObject.SetActive(false);
        }
        if (eventoActual == 219)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(2);
            gameObject.SetActive(false);
        }
        if (eventoActual == 322)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Inspiracion, "-Las historias junto al fuego dejaron a la Caravana con Inspiración.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 323)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Presteza, "-El campamento ligero dejó a la Caravana lista para avanzar con Presteza.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 324)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(2);
            gameObject.SetActive(false);
        }
        if (eventoActual == 325)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Vigilante, "-Los guardias relevados dejaron a la Caravana Vigilante.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 96)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 97)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Acobardados, "-El presagio de los cuervos dejó a la Caravana Acobardada.");
            gameObject.SetActive(false);
        }
        if (eventoActual == 98)
        {
            CampaignManager.Instance.CambiarFatigaActual(1);
            gameObject.SetActive(false);
        }
        if (eventoActual == 295)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(3);
            gameObject.SetActive(false);
        }
        if (eventoActual == 296)
        {
            CampaignManager.Instance.CambiarEsperanzaActual(2);
            gameObject.SetActive(false);
        }
        if (eventoActual == 297)
        {
            OtorgarEstadoCaravana(TipoEstadoCaravana.Inspiracion, "-La respuesta dejada en Nedukazal inspiró a la Caravana.");
            gameObject.SetActive(false);
        }


        //Especificos no aleatorios
        if (eventoActual == 401) //Claro
        {
            CampaignManager.Instance.CambiarEsperanzaActual(5);
            gameObject.SetActive(false);
        }

        if (eventoActual == 402) //Asentamiento
        {
            int civiles = UnityEngine.Random.Range(15, 26);
            CampaignManager.Instance.CambiarCivilesActuales(civiles);
            int suministros = UnityEngine.Random.Range(50, 60);
            CampaignManager.Instance.CambiarSuministrosActuales(suministros);
            int materiales = UnityEngine.Random.Range(6, 9);
            CampaignManager.Instance.CambiarMaterialesActuales(materiales);
            int bueyes = UnityEngine.Random.Range(2, 5);
            CampaignManager.Instance.CambiarBueyesActuales(bueyes);
            int oro = UnityEngine.Random.Range(60, 71);
            CampaignManager.Instance.CambiarOroActual(oro);
            gameObject.SetActive(false);

            CampaignManager.Instance.AgregarHeroe(0);
        }


        if (eventoActual == 403) //Recursos
        {
            if (CampaignManager.Instance.scAtributosZona.ID == 3)//Nedukazal
            {
                int suministros = UnityEngine.Random.Range(60, 86);
                CampaignManager.Instance.CambiarSuministrosActuales(suministros);
                int materiales = UnityEngine.Random.Range(25, 41);
                CampaignManager.Instance.CambiarMaterialesActuales(materiales);

                gameObject.SetActive(false);
            }
            else
            {
                int suministros = UnityEngine.Random.Range(80, 141);
                CampaignManager.Instance.CambiarSuministrosActuales(suministros);
                int materiales = UnityEngine.Random.Range(18, 31);
                CampaignManager.Instance.CambiarMaterialesActuales(materiales);
            }
            gameObject.SetActive(false);
        }

        if (eventoActual == 404) //Mision de Salvamento
        {
            IntentarMarcarMisionSalvamentoDesdeEvento();
        }
         if(eventoActual == 405) //Mision de Salvamento Llegada
        {
           //Efectos arriba
           gameObject.SetActive(false);
       }

   }
}
