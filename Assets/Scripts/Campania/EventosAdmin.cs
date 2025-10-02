using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventosAdmin : MonoBehaviour
{
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
   [SerializeField] Sprite Evento008; //Río Contaminado
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
    public void TirarEventoMalo()
    {
        int eventoGenericooZonal =UnityEngine.Random.Range(1, 100);
        if (eventoGenericooZonal <= 100/*SÓLO HAY GENÉRICOS POR AHORA*/) // 70% Generico       !!!!!!!!!!!!!!!! Cambiar en Eventos Buenos tmb
        {

            int random = UnityEngine.Random.Range(1, 10);//!!! //1 a 80
            EmpezarEvento(random);//Retraso Nocturno

        }
        else //30% Zonal 
        {
            switch (CampaignManager.Instance.scAtributosZona.ID)
            {
                case 1: //Bosque Ardiente
                    int random = UnityEngine.Random.Range(81, 82);//!!! //81 a 95
                    EmpezarEvento(random);//Retraso Nocturno
                    break;

            }
        }


    }
   public void TirarEventoBueno()
   {
        int eventoGenericooZonal =UnityEngine.Random.Range(1, 100);
        if (eventoGenericooZonal <= 100/*SÓLO HAY GENÉRICOS POR AHORA*/) // 70% Generico        !!!!!!!!!!!!!!!! Cambiar en Malos tmb
        {

            int random = UnityEngine.Random.Range(1, 10)+200;//!!! //201 a 281
            EmpezarEvento(random);//Retraso Nocturno

        }
        else //30% Zonal 
        {
            switch (CampaignManager.Instance.scAtributosZona.ID)
            {
                case 1: //Bosque Ardiente
                    int random = UnityEngine.Random.Range(81, 82+200);//!!! //281 a 295
                    EmpezarEvento(random);//Retraso Nocturno
                    break;

            }
        }

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

        //EVENTOS MALOS  GENERICOS 1 -80  Hechos: 10/80
        if(ID == 1) //Retraso Nocturno
        {
            imRetrato.sprite = Evento001;
            txtTitulo.text = "Retraso Nocturno";

            txtDescripcion.text = "Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n";
            txtDescripcion.text += "Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podría perderse.\n\n\n\n\n\n\n";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += "<color=#ba3fef><b>Pasan las Horas: +1 Avance Aliento Negro</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";
        }
        if(ID == 2) //Desapariciones Misteriosas
        {
            imRetrato.sprite = Evento002;
            txtTitulo.text = "Desapariciones Misteriosas";

            txtDescripcion.text="De un momento a otro, varios miembros de la caravana han desaparecido sin dejar rastro. Nadie tiene una explicación de lo que ha sucedido. Pero el miedo y la incertidumbre se apoderan de todos.\n";
            txtDescripcion.text+="Luego de buscar vagamente en la cercanía y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text+="<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";
        }
        if(ID == 3) //Bueyes Enfermos
        {
            imRetrato.sprite = Evento003;
            txtTitulo.text = "Bueyes Enfermos";

            txtDescripcion.text="Uno de los bueyes de la caravana ha caído enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n";
            txtDescripcion.text+="<color=#ba3fef>-Si decides revisarlos tomará unas horas: +1 Avance Aliento Negro.</color>\n\n";
            txtDescripcion.text+="<color=#ba3fef>-Si decides ignorar las advertencias: 1-3 Bueyes mas morirán.</color>\n\n";


            textBotonA.text = "Revisarlos";
            textBotonB.text = "Ignorar";
        }
        if(ID == 4) //Peaje criminal
        {
            imRetrato.sprite = Evento004;
            txtTitulo.text = "Peaje Criminal";

            txtDescripcion.text="Mientras la caravana se dispone a avanzar por un terreno peligroso, se topa con un grupo de bandidos que exige un peaje exorbitante para dejar pasar a la caravana.\n\n";
            txtDescripcion.text+="<color=#ba3fef>-Si decides pagar el peaje, perderás 1 de Oro por Civil.</color>\n\n";
            txtDescripcion.text+="<color=#ba3fef>-Luchar con los Bandidos.</color>\n\n";


            textBotonA.text = "Pagar";
            textBotonB.text = "Luchar";
        }
        if(ID == 5) //Personaje Enfermo
        {
            imRetrato.sprite = Evento005;
            txtTitulo.text = "Personaje Enfermo";
            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = $"<b><color=#d1006f>{participanteEvento1.sNombre}</color></b> se acerca a ti y no luce nada bien. Te comenta que ha empezado a sentirse enfermo y necesita médicina para mejorar pronto y estar nuevamente en condiciones de combatir.\n\n";
            txtDescripcion.text += "Obtendrá el estado Enfermo por 4-7 días. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 día.\n\n\n\n\n";

            txtDescripcion.text += "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un día extra.</color>\n\n";


            textBotonA.text = "Pagar";
            if (CampaignManager.Instance.GetOroActuales() < 45)
            {
                botonA.SetActive(false);
            }
            textBotonB.text = "No pagar";
        }
        if(ID == 6) //Arcas Robadas
        {
            imRetrato.sprite = Evento006;
            txtTitulo.text = "Arcas Robadas";

            txtDescripcion.text = "Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botín.\n\n";
            int oroRobado = CampaignManager.Instance.GetOroActuales()/4; //25% del oro actual
            if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
            int seguridad = 40 + (int)CampaignManager.Instance.GetMiliciasActual();

            txtDescripcion.text +="<b>Oro Robado:  " + oroRobado + "\n\n</b>";
            txtDescripcion.text += "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perdería 5 de Esperanza, <i>" + seguridad + "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n";


            textBotonA.text = "Interrogar";

            textBotonB.text = "No interrogar";
        }
        if(ID == 7) //Carro Deteriorado
        {
            imRetrato.sprite = Evento007;
            txtTitulo.text = "Carro Deteriorado";

            txtDescripcion.text = "Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n";
            txtDescripcion.text += "<color=#ba3fef>-Puedes pasar los 60 suministros caídos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n";

            textBotonA.text = "Aceptar";

            textBotonB.text = "No aceptar";
        }
        if(ID == 8) //Río Contaminado
        {
            imRetrato.sprite = Evento008;
            txtTitulo.text = "Río Contaminado";

            txtDescripcion.text = "La Caravana encuentra un río con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n";
            txtDescripcion.text += "El agua podría estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n";

            int chancesContaminado = 30+(int)CampaignManager.Instance.GetValorAlientoNegro()*3; 
            txtDescripcion.text += "<color=#ba3fef>-Si les niegas el acceso perderás 15 de Esperanza.</color>\n\n";
            txtDescripcion.text += "<color=#ba3fef>-Si los dejas ir, hay un %" + chancesContaminado + " <i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no está contaminada descansarán (-1 Fatiga).</color>\n\n";

            textBotonA.text = "Negarse";

            textBotonB.text = "Dejarlos";
        }
        if(ID == 9) //Riña
        {
            imRetrato.sprite = Evento009;
            txtTitulo.text = "Riña";

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(new List<Personaje> { participanteEvento1 });
            retratoParticipante2.SetActive(true);
            retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;

            txtDescripcion.text = $"Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>{participanteEvento1.sNombre}</color></b> y <b><color=#d1006f>{participanteEvento2.sNombre}</color></b> discutiendo acaloradamente.";
            txtDescripcion.text += "\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n";
            txtDescripcion.text += "La tensión sube y los demás caravaneros miran con incomodidad. Ambos reclaman tener la razón y esperan tu juicio.\n\n";
            txtDescripcion.text += "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 días. Apoyas a:</color>\n\n";

            textBotonA.text = "" + participanteEvento1.sNombre;

            textBotonB.text = "" + participanteEvento2.sNombre;
        }
        if(ID == 10) //Liderazgo Cuestionado
        {
            imRetrato.sprite = Evento010;
            txtTitulo.text = "Liderazgo Cuestionado";

            txtDescripcion.text = "Un Civil de origen noble se acerca a ti con altanería y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podría hacerlo mejor.\n";
            txtDescripcion.text += "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n";

            int chances = 35+(int)CampaignManager.Instance.GetEsperanzaActual()/3; 
            txtDescripcion.text += $"<color=#ba3fef>-Puedes dar un discurso motivador, refutando sus argumentos con hechos.</color> Chances: %{chances} <i>(Determinado por Esperanza) Éxito: +15 Esperanza. Fallo: -20 Esperanza.</i> \n\n";
            txtDescripcion.text += "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n";

            textBotonA.text = "Discurso";

            textBotonB.text = "Golpear";
        }
      
        //EVENTOS BUENOS GENERICOS  201-280 Hechos: 10/80
        if (ID == 201) // Destello Esperanzador
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = "Destello Esperanzador";

            txtDescripcion.text = "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del arca.\n";
            txtDescripcion.text += "Quizás sea una señal, quizás una casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += "<color=#a0e812><b>+15 Esperanza</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";
        }
        if (ID == 202) // Risotadas en la Caravana
        {
            imRetrato.sprite = Evento202;
            txtTitulo.text = "Risotadas en la Caravana";

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            participanteEvento2 = CampaignManager.Instance.ObtenerPersonajeAleatorio(new List<Personaje> { participanteEvento1 });
            retratoParticipante2.SetActive(true);
            retratoParticipante2.GetComponent<Image>().sprite = participanteEvento2.spRetrato;

            txtDescripcion.text = $"Durante la noche, <b><color=#d1006f>{participanteEvento1.sNombre}</color></b> y <b><color=#d1006f>{participanteEvento2.sNombre}</color></b> junto con algunos Civiles comienzan a contar chistes y anécdotas divertidas, riendo y disfrutando del momento.\n";
            txtDescripcion.text += "La atmósfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>";
            txtDescripcion.text += $"<color=#a0e812><b>{participanteEvento1.sNombre} y {participanteEvento2.sNombre} ganan Alta Moral por 3 días.</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";
        }
        if (ID == 203) // Caravana perdida
        {
            imRetrato.sprite = Evento203;
            txtTitulo.text = "Caravana Perdida";

            txtDescripcion.text = "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí y esta caravana no sobrevivió.\n";
            txtDescripcion.text += "Si bien la situación es sombría, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n";

            txtDescripcion.text += $"<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n";
            txtDescripcion.text += "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n";

            textBotonA.text = "Saquear";

            textBotonB.text = "Honrar";
        }
        if (ID == 204) // Aserradero Abandonado
        {
            imRetrato.sprite = Evento204;
            txtTitulo.text = "Aserradero Abandonado";

            txtDescripcion.text = "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n";
            txtDescripcion.text += "Hay suficiente madera como para llenar un par de carros, pero juntarla toda cansará a los Civiles que participen y llevará algunas horas.\n\n\n\n";

            txtDescripcion.text += $"<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n";
            txtDescripcion.text += "<color=#ba3fef>-Puedes juntar solo lo que está a mano y continuar sin retraso.</color> +15-26 Materiales \n\n";

            textBotonA.text = "Todo";

            textBotonB.text = "Un poco";
        }
        if (ID == 205) // Manada de Bueyes
        {
            imRetrato.sprite = Evento205;
            txtTitulo.text = "Manada de Bueyes";
            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio(null, 2);
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            int chances = 60 + (int)(participanteEvento1.fNivelActual * 5);
            txtDescripcion.text = "La Caravana se detiene en un claro donde pasta una manada de bueyes. Los animales parecen sanos y bien alimentados, pero están asustados por la presencia de la Caravana.\n";
            txtDescripcion.text += $"\n<b><color=#d1006f>{participanteEvento1.sNombre}</color></b> cree que puede cazar algunos de estos Bueyes para obtener comida.  Chances: %{chances} <i>(Determinado por Nivel)  Exito: +50-80 Suministros +55 Experiencia.  Fallo: Recibe Herida.</i>\n\n\n\n";
            txtDescripcion.text += $"<color=#ba3fef>-Puedes optar por dejarlo cazar, o directamente domesticar a un puñado para que se sumen a la Caravana. +2-3 Bueyes</i> \n\n";

            textBotonA.text = "Cazarlos";

            textBotonB.text = "Domesticarlos";
        }
        if (ID == 206) // Civiles en Apuros
        {
            imRetrato.sprite = Evento206;
            txtTitulo.text = "Civiles en Apuros";
           
            txtDescripcion.text = "La Caravana se detiene al escuchar gritos de auxilio provenientes de un lado del camino. Al investigar encuentras a un puñado de Civiles escapando de un enemigo desconocido en dirección a la Caravana.\n";
            txtDescripcion.text += "'¡Nos persiguen! no pudimos verlos, pero se acercan.' - Dice un Civil aterrorizado. 'Ayúdanos'\n\n";
            txtDescripcion.text += $"<color=#ba3fef>-Puedes defender a los civiles de sus perseguidores mientras les das tiempo a los más débiles a sumarse a la Caravana.</color> Combate Normal - +18-26 Civiles\n\n";
            txtDescripcion.text += $"<color=#ba3fef>-Puedes aceptar solo a los mas ágiles y huir para evitar confrontar con sus perseguidores.</color> +5-10 Civiles -5 Esperanza\n\n";

            textBotonA.text = "Defender";

            textBotonB.text = "Huir";
        }
        if (ID == 207) // Tranquilidad
        {
            imRetrato.sprite = Evento207;
            txtTitulo.text = "Tranquilidad";

            txtDescripcion.text = "En un momento repentino, te das cuenta que hay mucha paz. Se escuchan los pasos constantes de la caravana, algún murmullo, risa y la naturaleza alrededor.\n";
            txtDescripcion.text += "Estos momentos son muy escasos y sientes que cada individuo de la caravana lo valoró a su manera. \nDe alguna forma, el aire se siente más limpio.\n\n";
            txtDescripcion.text += $"<color=#ba3fef>-2 al Avance del Aliento Negro.</color>\n\n";

           
            botonA.SetActive(false);

            textBotonB.text = "Continuar";
        }
        if (ID == 208) // Voto de Confianza
        {
            imRetrato.sprite = Evento208;
            txtTitulo.text = "Voto de Confianza";

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = "<b><color=#d1006f>" + participanteEvento1.sNombre + "</color></b> se acerca a ti y coloca una mano en tu hombro y dice: -'Tengo mucha esperanza en usted, y creo que será exitoso al liderarnos a salvo hacia el puerto'.\n";
            txtDescripcion.text += "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un símbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ";
            txtDescripcion.text += $"<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: {participanteEvento1.sNombre} gana Alta Moral por 4 días y 50 Experiencia. \n\n";
            txtDescripcion.text += $"<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n";

            textBotonA.text = "Rechazar";

             textBotonB.text = "Aceptar";
        }
        if (ID == 209) // Lugareño Anciano
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = "Lugareño Anciano ";


            txtDescripcion.text = "Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchísimos años en la zona y la conoce a la perfección.\n";
            txtDescripcion.text += "'Aliento Negro o no, mis días ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n";
            txtDescripcion.text += $"<color=#ba3fef>Preguntas: -'¿Conoce algun atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n";
            txtDescripcion.text += $"<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n";

            textBotonA.text = "Atajo";

            textBotonB.text = "Area";
        }
        if (ID == 210) // Sueño Inspirador
        {
            imRetrato.sprite = Evento209;
            txtTitulo.text = "Sueño Inspirador";

            participanteEvento1 = CampaignManager.Instance.ObtenerPersonajeAleatorio();
            retratoParticipante1.SetActive(true);
            retratoParticipante1.GetComponent<Image>().sprite = participanteEvento1.spRetrato;

            txtDescripcion.text = $"A <b><color=#d1006f>" + participanteEvento1.sNombre + "</color></b> se lo ve con mucha energía y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n";
            txtDescripcion.text += "'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n";
            txtDescripcion.text += $"<color=#ba3fef><b><color=#d1006f>" + participanteEvento1.sNombre + "</color></b> obtiene 150 Experiencia y Alta Moral por 5 días.</color>\n\n";

            botonA.SetActive(false);
            textBotonB.text = "Continuar";
        }
       
         //EVENTOS ESPECIFICOS (que no toquen al azar)  401 a ++
         if (ID == 401) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = "Claro";

            txtDescripcion.text = "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mínimo.\n";
            txtDescripcion.text += "Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text += "<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrá también beneficios adicionales:\n-El Aliento Negro avanzará solo 1.\n-+10% curación recibida.\n-El evento será positivo.</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";


        }
         if(ID == 402) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = "Asentamiento";

            txtDescripcion.text="Has llegado a un pequeño asentamiento. Notas que los civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.";
            txtDescripcion.text+="\nDe 15-25 Civiles se unirán a la Caravana y brindarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.";
            txtDescripcion.text+="\nUn Héroe aleatorio se sumará a tus fuerzas.\n\n\n\n\n";



            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text+="<color=#a0e812><b>\nDescansar en este lugar tendrá beneficios adicionales:+20% curación recibida.</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";


        }
         if(ID == 403) //
        {
            imRetrato.sprite = Evento201;
            txtTitulo.text = "Recursos";

            txtDescripcion.text="Has llegado a un lugar rico en recursos naturales, los civiles se han puesto a recolectar lo que han podido.";
            txtDescripcion.text+="\nSe conseguirán de 18-30 Materiales y 80-140 Suministros.";

            //El efecto de los eventos se aplica al apretar el boton de salir o de opcion
            txtDescripcion.text+="<color=#a0e812><b>\nDescansar en este lugar tendrá beneficios adicionales:+20% efectividad a tareas de Recolección.</b></color>";

            botonA.SetActive(false);

            textBotonB.text = "Continuar";


        }

        

        
   }

    Personaje participanteEvento1;
    Personaje participanteEvento2;

    public void ElegirOpcionA()
    {
        //Eventos Malos
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
                CampaignManager.Instance.EscribirLog($"-Has encontrado al ladrón y recuperado el oro robado, pero has tenido que desterrar al ladrón. -5 Esperanza -1 Civil.");
            }
            else
            {
                int oroRobado = CampaignManager.Instance.GetOroActuales() / 4; //25% del oro actual
                if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
                CampaignManager.Instance.CambiarOroActual(-oroRobado);
                CampaignManager.Instance.CambiarEsperanzaActual(-5);
                CampaignManager.Instance.EscribirLog($"-No has logrado encontrar al ladrón y se perdieron {oroRobado} de oro. -5 Esperanza por el interrogatorio");
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
                CampaignManager.Instance.EscribirLog($"-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza");
            }
            else
            {
                CampaignManager.Instance.CambiarEsperanzaActual(-20);
                CampaignManager.Instance.EscribirLog($"-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza.");
            }
            gameObject.SetActive(false);
        }

        //Eventos Buenos
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
                CampaignManager.Instance.EscribirLog($"-La cacería de {participanteEvento1.sNombre} ha sido exitosa. +{rand} Suministros +55 Experiencia.");
            }
            else
            {
                participanteEvento1.Camp_Herido = true;
                participanteEvento1.fVidaActual -= participanteEvento1.fVidaActual * 0.6f;
                CampaignManager.Instance.EscribirLog($"-{participanteEvento1.sNombre} sufrió un accidente durante la cacería. Herido.");
            }
            gameObject.SetActive(false);
        }
        if (eventoActual == 206)
        {
            gameObject.SetActive(false);

            CampaignManager.Instance.CambiarCivilesActuales(UnityEngine.Random.Range(18, 27));
            CampaignManager.Instance.scMenuBatallas.EventoBatallaNormal(0, 0);
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

    }
   public void ElegirOpcionB()
   {
    //Malos
    if(eventoActual == 1)
    {
        CampaignManager.Instance.CambiarValorAlientoNegro(1);
        gameObject.SetActive(false);
    }
    if(eventoActual == 2)
    {   int civilesperdidos =UnityEngine.Random.Range(4, 13);
        CampaignManager.Instance.CambiarCivilesActuales(-civilesperdidos);
        CampaignManager.Instance.CambiarEsperanzaActual(-5);
        gameObject.SetActive(false);
    }
    if(eventoActual == 3)
    {   int bueyesperdidos = 1+UnityEngine.Random.Range(1, 4);
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
        float tierCuranderos =((CampaignManager.Instance.sequitoCuranderosMejoraCuracion*100)-10)/5;
        int rand =UnityEngine.Random.Range(4, 8);
        participanteEvento1.Camp_Enfermo += rand-(int)tierCuranderos; 
        gameObject.SetActive(false);
    }
    if (eventoActual == 6)
    {
            
                int oroRobado = CampaignManager.Instance.GetOroActuales() / 4; //25% del oro actual
                if (oroRobado > CampaignManager.Instance.GetOroActuales()) { oroRobado = CampaignManager.Instance.GetOroActuales(); }
                CampaignManager.Instance.CambiarOroActual(-oroRobado);
                CampaignManager.Instance.EscribirLog($"-No has logrado encontrar al ladrón y se perdieron {oroRobado} de oro.");
            
            
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
            if(UnityEngine.Random.Range(1, 101) <= chancesCont)
            {
                int civilesPerdidos = 25;
                CampaignManager.Instance.CambiarCivilesActuales(-civilesPerdidos);
                CampaignManager.Instance.CambiarEsperanzaActual(-10);
                CampaignManager.Instance.EscribirLog($"-Los Civiles se han contaminado y han muerto {civilesPerdidos} Civiles. -10 Esperanza");
            }
            else
            {
                CampaignManager.Instance.CambiarFatigaActual(-1);
                CampaignManager.Instance.EscribirLog($"-Los Civiles han descansado en el río y se han refrescado. -1 Fatiga ");
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


    //Buenos
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
            CampaignManager.Instance.CambiarBueyesActuales(UnityEngine.Random.Range(2,4));
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
           CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(100,true);

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
            int civiles =UnityEngine.Random.Range(15, 26);
            CampaignManager.Instance.CambiarCivilesActuales(civiles);
            int suministros =UnityEngine.Random.Range(50, 60);
            CampaignManager.Instance.CambiarSuministrosActuales(suministros);
            int materiales =UnityEngine.Random.Range(6, 9);
            CampaignManager.Instance.CambiarMaterialesActuales(materiales);
            int bueyes =UnityEngine.Random.Range(2, 5);
            CampaignManager.Instance.CambiarBueyesActuales(bueyes);
            int oro =UnityEngine.Random.Range(60, 71);
            CampaignManager.Instance.CambiarOroActual(oro);
            gameObject.SetActive(false);

            //Añadir un héroe aleatorio
            CampaignManager.Instance.AgregarHeroe(0);
        }

   
    if(eventoActual == 403) //Recursos
    {
       
        int suministros =UnityEngine.Random.Range(80,141);
        CampaignManager.Instance.CambiarSuministrosActuales(suministros);
        int materiales =UnityEngine.Random.Range(18,31);
        CampaignManager.Instance.CambiarMaterialesActuales(materiales);
       
        gameObject.SetActive(false);
    }

   }
}
