using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TRADU : MonoBehaviour
{
    public const int IdiomaEspanol = 1;
    public const int IdiomaIngles = 2;
    public const int IdiomaPortugues = 3;

    public static TRADU i { get; private set; }
    public int nIdioma = 3; //1 Español  -  2 Inglés  -  3 Portugues
    private void Awake()
    {
        if (i == null)
        {
            i = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerPrefs.HasKey("nIdioma"))
        {
            nIdioma = NormalizarIdioma(PlayerPrefs.GetInt("nIdioma"));
        }
        else
        {
            nIdioma = IdiomaIngles;
            PlayerPrefs.SetInt("nIdioma", nIdioma); 
        }

    }

    void Start()
    {    
        if (IdiomaRequiereTraduccionMasiva(nIdioma))
        {
            Invoke(nameof(TraducirTodosTextosSegunIdioma), 0.1f); // espera a que carguen todos los textos
        }
    }
    public void ActualizarIdioma()
    {
        if (PlayerPrefs.HasKey("nIdioma"))
        {
            nIdioma = NormalizarIdioma(PlayerPrefs.GetInt("nIdioma"));
        }

        if (IdiomaRequiereTraduccionMasiva(nIdioma))
        {
            Invoke(nameof(TraducirTodosTextosSegunIdioma), 0.5f);
        }

    }
    public string Traducir(string textComponent)
    {
        string resultado = "SIN TRADUCCION";

        switch (nIdioma)
        {
            case 1: resultado = textComponent; break; //En español no traduce
            case IdiomaIngles:
                resultado = TraducirConCompatibilidadMojibake(textComponent, TraducirIngles);
                break;
            case IdiomaPortugues:
                resultado = TraducirConCompatibilidadMojibake(textComponent, TraducirPortugues);
                break;

        }





        return resultado;

    }






    private static int NormalizarIdioma(int idioma)
    { 
        return idioma == IdiomaEspanol || idioma == IdiomaIngles || idioma == IdiomaPortugues
            ? idioma
            : IdiomaIngles;
    }

    private static bool IdiomaRequiereTraduccionMasiva(int idioma)
    {
        return idioma == IdiomaIngles || idioma == IdiomaPortugues;
    }

    private string TraducirConCompatibilidadMojibake(string textComponent, System.Func<string, bool, string> traductor)
    {
        string resultado = traductor(textComponent, false);
        if (resultado == textComponent && !string.IsNullOrEmpty(textComponent))
        {
            string claveMojibake = ConvertirATextoMojibake(textComponent);
            if (claveMojibake != textComponent)
            {
                string resultadoFallback = traductor(claveMojibake, false);
                if (resultadoFallback != claveMojibake)
                {
                    resultado = resultadoFallback;
                }
            }
        }

        return resultado;
    }

    string ConvertirATextoMojibake(string txt)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txt);
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
        catch
        {
            return txt;
        }
    }

    string TraducirIngles(string txt, bool esBotonFijo = false)
    {

        string r = txt;
        if (!esBotonFijo) //Si es un boton fijo, y no se encuentra la traduccion, no poner error, no cambia el texto
        {
             r = /*"Error tradu: " +*/txt; //para que al devolverlo en español, se sepa que linea falla
        }

        if (!string.IsNullOrEmpty(txt) && (txt.StartsWith("Determinación ") || txt.StartsWith("Determinación ")))
        {
            return "Determination " + txt.Substring(txt.IndexOf(' ') + 1);
        }

        switch (txt)
        {
            case "Retraso Nocturno":
                r = "Nighttime Delay";
                break;
            case "Desapariciones Misteriosas":
                r = "Mysterious Disappearances";
                break;
            case "Bueyes Enfermos":
                r = "Sick Oxen";
                break;
            case "Peaje Criminal":
                r = "Criminal Toll";
                break;
            case "Personaje Enfermo":
                r = "Sick Character";
                break;
            case "Arcas Robadas":
                r = "Stolen Chests";
                break;
            case "Carro Deteriorado":
                r = "Damaged Wagon";
                break;
            case "Liderazgo Cuestionado":
                r = "Questioned Leadership";
                break;
            case "Destello Esperanzador":
                r = "Hopeful Glimmer";
                break;
            case "Risotadas en la Caravana":
                r = "Laughter in the Caravan";
                break;
            case "Caravana Perdida":
                r = "Lost Caravan";
                break;
            case "Aserradero Abandonado":
                r = "Abandoned Sawmill";
                break;
            case "Manada de Bueyes":
                r = "Herd of Oxen";
                break;
            case "Civiles en Apuros":
                r = "Civilians in Distress";
                break;
            case "Tranquilidad":
                r = "Tranquility";
                break;
            case "Voto de Confianza":
                r = "Vote of Confidence";
                break;
            case "Claro":
                r = "Glade";
                break;
            case "Asentamiento.":
                r = "Settlement";
                break;
            case "Recursos":
                r = "Resources";
                break;
            case "Continuar":
                r = "Continue";
                break;
            case "Revisarlos":
                r = "Inspect Them";
                break;
            case "Ignorar":
                r = "Ignore";
                break;
            case "Pagar":
                r = "Pay";
                break;
            case "No pagar":
                r = "Don't pay";
                break;
            case "Interrogar":
                r = "Interrogate";
                break;
            case "No interrogar":
                r = "Do not interrogate";
                break;
            case "Aceptar":
                r = "Accept";
                break;
            case "No aceptar":
                r = "Do not accept";
                break;
            case "Negarse":
                r = "Refuse";
                break;
            case "Dejarlos":
                r = "Let them";
                break;
            case "Discurso":
                r = "Speech";
                break;
            case "Golpear":
                r = "Strike";
                break;
            case "Saquear":
                r = "Loot";
                break;
            case "Honrar":
                r = "Honor";
                break;
            case "Todo":
                r = "All";
                break;
            case "Un poco":
                r = "A little";
                break;
            case "Cazarlos":
                r = "Hunt them";
                break;
            case "Domesticarlos":
                r = "Domesticate them";
                break;
            case "Rechazar":
                r = "Decline";
                break;
            case "Atajo":
                r = "Shortcut";
                break;
            case "Area":
                r = "Area";
                break;
            // EventosAdmin remaining literals (exact text keys)

            case "Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n":
                r = "One of the main people in charge of guiding the caravan and choosing the safest routes accidentally lost their maps.\n"; break;
            case "Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podráa perderse.\n\n\n\n\n\n\n":
                r = "The other leaders will help search for them since those maps contain crucial information about the current area, and without them the caravan could get lost.\n\n\n\n\n\n\n"; break;
            case "Obtendrá el estado Enfermo por 4-7 dí­as. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 dí­a.\n\n\n\n\n":
                r = "Will gain the Sick status for 4-7 days. Each tier of the Healers' Retinue will reduce recovery time by 1 day.\n\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un dí­a extra.</color>\n\n":
                r = "<color=#ba3fef>-You can buy medicine for 45 Gold to reduce the illness by one extra day.</color>\n\n"; break;
            case "Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botón.\n\n":
                r = "At a guard's shout, your attention turns to one of the wagons carrying the caravan's treasures. One of its chests is tipped and gold wass spilled on the ground. Apparently during the night, someone managed to force it and took part of the loot.\n\n"; break;
            case "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perderí­a 5 de Esperanza, <i>":
                r = "<color=#ba3fef>-You can subject the Civilians to an interrogation to try to find the thief:\n\n You would lose 5 Hope, <i>"; break;
            case "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n":
                r = "% Chances (40 base + Militiamen)</i> of finding the culprit and recovering the gold, -1 Civilian due to banishment.</color>\n\n"; break;
            case "Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n":
                r = "After a loud noise, you turn your head back and see that one of the supply wagons has had an accident. The wheels are stuck in the mud and the wagon seems to be lost for good.\n\n"; break;
            case "<color=#ba3fef>-Puedes pasar los 60 suministros caí­dos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n":
                r = "<color=#ba3fef>-You can transfer the 60 fallen supplies to another wagon, sacrificing 20 Materials; or accept the loss of supplies.</color>\n\n"; break;
            case "La Caravana encuentra un rí­o con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n":
                r = "The Caravan finds a river with good flow and seemingly decent water. Several excited civilians head towards it to recreate and refresh themselves.\n\n"; break;
            case "El agua podráa estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n":
                r = "The water could be contaminated by the Black Breath. You can deny the Civilians access to the water or leave them to their own fate.\n\n"; break;
            case "<color=#ba3fef>-Si les niegas el acceso perderás 15 de Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you deny access, you will lose 15 Hope.</color>\n\n"; break;
            case "<color=#ba3fef>-Si los dejas ir, hay un %":
                r = "<color=#ba3fef>-If you let them go, there is a %"; break;
            case "<i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no está contaminada descansarán (-1 Fatiga).</color>\n\n":
                r = " <i>(Determined by Black Breath)</i> chance they are contaminated and 25 Civilians die. If not contaminated they will rest (-1 Fatigue).</color>\n\n"; break;
            case "\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n":
                r = "\nApparently they had an incident during a light training they set out to do in which both were slightly injured.\n\n"; break;
            case "La tensión sube y los demás caravaneros miran con incomodidad. Ambos reclaman tener la razón y esperan tu juicio.\n\n":
                r = "Tension rises and the other caravaners look on uncomfortably. Both claim to be right and await your judgment.\n\n"; break;
            case "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 dí­as. Apoyas a:</color>\n\n":
                r = "<color=#ba3fef>-You must intervene in support of one of the two. The other will gain Low Morale for 5 days. You support:</color>\n\n"; break;
            case "Un Civil de origen noble se acerca a ti con altanerí­a y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podráa hacerlo mejor.\n":
                r = "A Civilian of noble origin approaches you arrogantly and begins to question your leadership, arguing that you are not making the right decisions for the Caravan's well-being and that he himself could do better.\n"; break;
            case "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n":
                r = "While his points are not very coherent, as he speaks loudly, several civilians begin to gather around, curious.\n\n"; break;
            case "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n":
                r = "<color=#ba3fef>-Hit him.</color> His family leaves the Caravan, withdrawing their investment. -65 Gold -8 Civilians -10 Hope\n\n"; break;
            case "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del puerto.\n":
                r = "During the night, the gathered civilians spot a clear and beautiful flash of light on the horizon towards the port.\n"; break;
            case "Quizás sea una señal, quizás casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n":
                r = "Perhaps it is a sign, perhaps coincidence, but the civilians now seem more optimistic, even though there is still a long way to go.\n\n\n\n\n\n\n"; break;
            case "La atmásfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n":
                r = "The atmosphere becomes lighter and more optimistic, and for a brief moment, the weight of the situation seems to fade away.\n\n\n\n"; break;
            case "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>":
                r = "<color=#a0e812><b>+5 Hope</b>\n\n</color>"; break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadoveres civiles. Una lucha tuvo lugar aquí­ y esta caravana no sobrevivió.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n"; break;
            case "Si bien la situación es sombrí­a, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n":
                r = "Although the situation is bleak, several supplies in good condition were not looted, remaining on the side of the road.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n":
                r = "<color=#ba3fef>-You can bury the Civilians and honor their memory, without looting them.</color> +15 Hope \n\n"; break;
            case "La Caravana se detiene en un aserradero abandonado, algunos Ç­rboles han sido talados y la madera estÇ­ apilada en desorden.\n":
                r = "The Caravan stops at an abandoned sawmill; some trees have been felled and the wood is piled up in disarray.\n"; break;
            case "Hay suficiente madera como para llenar un par de carros, pero juntarla toda cansará a los Civiles que participen y llevará algunas horas.\n\n\n\n":
                r = "There is enough wood to fill a couple of wagons, but gathering it all will tire the participating Civilians and take a few hours.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes juntar solo lo que está a mano y continuar sin retraso.</color> +15-26 Materiales \n\n":
                r = "<color=#ba3fef>-You can gather only what is at hand and continue without delay.</color> +15-26 Materials \n\n"; break;
            case "La Caravana se detiene en un claro donde pasta una manada de bueyes. Los animales parecen sanos y bien alimentados, pero están asustados por la presencia de la Caravana.\n":
                r = "The Caravan stops in a clearing where a herd of oxen is grazing. The animals look healthy and well-fed, but are frightened by the Caravan's presence.\n"; break;
            case "La Caravana se detiene al escuchar gritos de auxilio provenientes de un lado del camino. Al investigar encuentras a un puñado de Civiles escapando de una banda de bandidos en dirección a la Caravana.\n":
                r = "The Caravan stops upon hearing cries for help from the side of the road. Investigating, you find a handful of Civilians fleeing a bandit gang toward the Caravan.\n"; break;
            case "'Son bandidos! no pudimos ver cuántos, pero se acercan.' - Dice un Civil aterrorizado. 'Ayúdanos'\n\n":
                r = "'It's bandits! We couldn't see how many, but they're getting closer.' - Says a terrified Civilian. 'Help us'\n\n"; break;
            case "En un momento repentino, te das cuenta que hay mucha paz. Se escuchan los pasos constantes de la caravana, algún murmullo, risa y la naturaleza alrededor.\n":
                r = "In a sudden moment, you realize there is a lot of peace. You hear the constant footsteps of the caravan, some murmurs, laughter, and nature around you.\n"; break;
            case "Estos momentos son muy escasos y sientes que cada individuo de la caravana lo valoró a su manera. \nDe alguna forma, el aire se siente más limpio.\n\n":
                r = "These moments are very scarce and you feel that each individual in the caravan valued it in their own way. Somehow, the air feels cleaner.\n\n"; break;
            // EventosAdmin extra keys and segments
            case "<b>Oro Robado:  ":
                r = "<b>Stolen Gold:  "; break;
            case "\n\n</b>":
                r = "\n\n</b>"; break;
            case "<color=#ba3fef>-Luchar con los Bandidos.</color>\n\n":
                r = "<color=#ba3fef>-Fight the Bandits.</color>\n\n"; break;
            case "<color=#ba3fef>-2 al Avance del Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-2 to Black Breath Advance.</color>\n\n"; break;
            // Logs (segments for concatenation)
            case "-Has encontrado al ladrón y recuperado el oro robado, pero has tenido que desterrar al ladrón. -5 Esperanza -1 Civil.":
                r = "-You found the thief and recovered the stolen gold, but had to banish them. -5 Hope -1 Civilian."; break;
            case "-No has logrado encontrar al ladrón y se perdieron ":
                r = "-You failed to find the thief and lost "; break;
            case " de oro.":
                r = " gold."; break;
            case "-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza":
                r = "-You gave a motivational speech and refuted the Noble's arguments. +15 Hope"; break;
            case "-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza.":
                r = "-You gave an unconvincing speech that raised more doubts than certainties. -20 Hope."; break;
            case "-La cacerí­a de ":
                r = "-The hunt by "; break;
            case " ha sido exitosa. +":
                r = " was successful. +"; break;
            case " Suministros +55 Experiencia.":
                r = " Supplies +55 Experience."; break;
            case " sufrió un accidente durante la cacerí­a. Herido.":
                r = " suffered an accident during the hunt. Wounded."; break;
            case "-Los Civiles se han contaminado y han muerto ":
                r = "-The Civilians were contaminated and "; break;
            case " Civiles. -10 Esperanza":
                r = " Civilians. -10 Hope"; break;
            case "-Los Civiles han descansado en el rí­o y se han refrescado. -1 Fatiga ":
                r = "-The Civilians rested by the river and cooled off. -1 Fatigue "; break;
            // Riña description segments
            case "Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>":
                r = "You hear a commotion near the Heroes' wagons. As you approach to investigate, you see <b><color=#d1006f>"; break;
            case "</color></b> y <b><color=#d1006f>":
                r = "</color></b> and <b><color=#d1006f>"; break;
            case "</color></b> discutiendo acaloradamente.":
                r = "</color></b> arguing heatedly."; break;
            case "Rí­o Contaminado":
                r = "Contaminated River";
                break;
            case "Riña":
                r = "Brawl";
                break;
            case "Lugareño Anciano ":
                r = "Local Elder ";
                break;
            case "Sueño Inspirador":
                r = "Inspiring Dream";
                break;
            case "<color=#a0e812><b>+15 Esperanza</b></color>":
                r = "<color=#a0e812><b>+15 Hope</b></color>";
                break;
            case "<color=#ba3fef><b>Pasan las Horas: +1 Avance Aliento Negro</b></color>":
                r = "<color=#ba3fef><b>Time Pass: +1 Black Breath Advance</b></color>";
                break;
            case "\n<b><color=#d1006f>":
                r = "\n<b><color=#d1006f>";
                break;
            case "</color></b> cree que puede cazar algunos de estos Bueyes para obtener comida.  Chances: %":
                r = "</color></b> believes they can hunt some of these Oxen for food.  Chances: %";
                break;
            case " <i>(Determinado por Nivel)  Exito: +50-80 Suministros +55 Experiencia.  Fallo: Recibe Herida.</i>\n\n\n\n":
                r = " <i>(Determined by Level)  Success: +50-80 Supplies +55 Experience.  Failure: Receives Wound.</i>\n\n\n\n";
                break;
            case "Caballero":
                r = "Knight";
                break;
            case "Explorador":
                r = "Explorer";
                break;
            case "Purificadora":
                r = "Purifier";
                break;
            case "Acechador":
                r = "Stalker";
                break;
            case "Canalizador":
                r = "Channeler";
                break;
            case "Ronda":
                r = "Round";
                break;
            case "Clima normal.":
                r = "Normal weather."; break;
            case "Calor: todas las unidades obtienen 'Acalorado'.":
                r = "Heat: All units gain 'Heated'."; break;
            case "Lluvia: todas las unidades obtienen 'Mojado'.":
                r = "Rain: All units gain 'Wet'."; break;
            case "Nieve: todas las unidades obtienen 'Frí­o'.":
                r = "Snow: All units gain 'Cold'."; break;
            case "Niebla: -1 Ataque a habilidades de rango.":
                r = "Fog: -1 Attack to ranged skills."; break;
            case "<color=#c5c5c5>cortante</color>":
                r = "<color=#c5c5c5>slashing</color>"; break; //Cortante
            case "<color=#c69360>perforante</color>":
                r = "<color=#c69360>piercing</color>"; break; //Perforante
            case "<color=#c67f60>contundente</color>":
                r = "<color=#c67f60>bludgeoning</color>"; break; //Contundente
            case "<color=#ce3715>fuego</color>":
                r = "<color=#ce3715>fire</color>"; break; //Fuego
            case "<color=#63c4b7>hielo</color>":
                r = "<color=#63c4b7>ice</color>"; break; //Hielo
            case "<color=#7758df>rayo</color>":
                r = "<color=#7758df>lightning</color>"; break; //Rayo
            case "<color=#28b717>Ácido</color>":
                r = "<color=#28b717>acid</color>"; break; //Acido
            case "<color=#1760b7>arcano</color>":
                r = "<color=#1760b7>arcane</color>"; break; //Arcano
            case "<color=#8038b2>necrótico</color>":
                r = "<color=#8038b2>necrotic</color>"; break; //Necro
            case "<color=#d6c304>verdadero</color>":
                r = "<color=#d6c304>true</color>"; break; //Verdadero
            case "<color=#d6c304>divino</color>":
                r = "<color=#d6c304>divine</color>"; break; //Divino
            case "Has llegado a un improvisado Puesto Comerciar, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\n\nTu Séquito de Mercaderes ha actualizado su Inventario.":
                r = "You have arrived at an improvised Trading Post, they offer basic survival Supplies to travelers.\nThe Tier of your Merchant Retinue will help lower prices.\n\n\nYour Merchant Retinue has updated its inventory.";
                break;
            case "El Séquito de Mercaderes ha actualizado su inventario en el Puesto Comercial.":
                r = "The Merchant Retinue has updated its inventory at the Trading Post.";
                break;
            case "Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n\n. ":
                r = "You have arrived at a Purifier's Sanctuary, several have been built in the area to provide support and prayers to the brave who fought the Lich.\nToday, although it is abandoned, it maintains its aura of tranquility and you can deposit offerings to make a purification prayer.\n\n\n.";
                break;
            case "-La caravana ha llegado a un Santuario de Purificadores. Los personajes se han curado un 15%. +10 Esperanza.":
                r = "The caravan has arrived at a Purifier's Sanctuary. Characters have healed by 15%. +10 Hope.";
                break;
            case "-Como Purificadora,":
                r = $"-As a Purifier,";
                break;
            case " gana 60 Experiencia por la visita al santuario.":
                r = " obtains 60 Experience for visiting the sanctuary.";
                break;
            case "<color=#8708a4><b>                  El Aliento Negro</b></color>\n\n\n":
                r = "<color=#8708a4><b>                  The Black Breath</b></color>\n\n\n";
                break;
            case "<color=#ebdeef>Al morir el Liche, liberó un último estertor de muerte y putrefacción que se expande por cientos de kilómetros alrededor.</color>":
                r = "<color=#ebdeef>Upon the Lich's death, it released a final gasp of death and rot that spreads for hundreds of miles around the land.</color>";
                break;
            case "\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente está envolviendo a los seres vivos que no logran escapar, provocándoles la muerte, o peor. </color>\n\n\n\n":
                r = "\n\nCalled the Black Breath, this putrefaction is slowly enveloping living beings who fail to escape, causing them death, or worse. </color>\n\n\n\n";
                break;
            case "<color=#bae895><b>Estado: Distante</b> (":
                r = "<color=#bae895><b>Status: Distant</b> (";
                break;
            case "<color=#c8a6e8><b>Estado: Cerca</b> (":
                r = "<color=#c8a6e8><b>Status: Close</b> (";
                break;
            case "<color=#aa66ea><b>Estado: Dentro</b> (":
                r = "<color=#aa66ea><b>Status: Inside</b> (";
                break;
            case "<color=#7a1dd1><b>Estado: Nocivo</b> (":
                r = "<color=#7a1dd1><b>Status: Noxious</b> (";
                break;
            case "/20) - La Caravana viaja con tranquilidad.</color>":
                r = "/20) - The Caravan travels peacefully.</color>";
                break;
            case "/20) - La Caravana comienza a preocuparse y la podredumbre se siente en el aire. Los Corrompidos acechan en las sombras.</color>":
                r = "/20) - The Caravan begins to worry and the rot is felt in the air. The Corrupted lurk in the shadows.</color>";
                break;
            case "/20) - La Caravana ya es directamente afectada por el hedor. Los Corrompidos se dejan ver.</color>":
                r = "/20) - The Caravan is now directly affected by the stench. The Corrupted are now visible.</color>";
                break;
            case "/20) - La peste comienza a tomar vidas civiles. Los Corrompidos son implacables.</color>":
                r = "/20) - The plague begins to take civilian lives. The Corrupted are relentless.</color>";
                break;
            case "Enérgicos(0)":
                r = "Energetic(0)";
                break;
            case "Descansados(1)":
                r = "Rested(1)";
                break;
            case "Frescos(2)":
                r = "Fresh(2)";
                break;
            case "En Marcha(3)":
                r = "Marching(3)";
                break;
            case "Agitados(4)":
                r = "Agitated(4)";
                break;
            case "Cansados(5)":
                r = "Tired(5)";
                break;
            case "Exhaustos(6)":
                r = "Exhausted(6)";
                break;
            case "La <color=#a0e812>Esperanza</color> determina el optimismo de la Caravana en general sobre la posibilidad de cumplir la misión y llegar al puerto.\n\n":
                r = "The <color=#a0e812>Hope</color> determines the Caravan's overall optimism about the possibility of completing the mission and reaching the port.\n\n";
                break;
            case "/100 de <color=#a0e812>Esperanza</color>\n":
                r = "/100 <color=#a0e812>Hope</color>\n";
                break;
            case " <color=#982a1b>1-20 Civiles abandonarán la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-20 Civilians will abandon the Caravan each rest.</color>\n";
                break;
            case " <color=#982a1b>1-10 Civiles abandonarán la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-10 Civilians will abandon the Caravan each rest.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarán algo de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Civilians will donate some Gold each rest.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarán buena cantidad de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Civilians will donate a good amount of Gold each rest.</color>\n";
                break;
            case "Los <color=#c918bb>Civiles</color> que lleva la caravana hacia el Puerto. Salvar la mayor cantidad es el objetivo principal de esta misión.\n\nCada uno consume 1 de <color=#b7972c>Suministros</color> cada Descanso, y la cantidad de Civiles determina la eficiencia de las Tareas Civiles.\n":
                r = "The <color=#c918bb>Civilians</color> that conforms the caravan. Saving as many as possible is the main objective of this mission.\n\nEach one consumes 1 <color=#b7972c>Supplies</color> each Rest, and the number of Civilians determines the efficiency of Civil Tasks.\n";
                break;
            case "\nLlevas ":
                r = "\nYou carry ";
                break;
            case " <color=#c918bb>Civiles</color> en la caravana.\n\n":
                r = " <color=#c918bb>Civilians</color> in the caravan.\n\n";
                break;
            case "\nLas fuerzas de la Milicia de la caravana son de <color=#a8a29c>":
                r = "\nThe strength of the caravan's Militia is of <color=#a8a29c>";
                break;
            case ", que equivalen a ":
                r = "which is equivalent to ";
                break;
            case "</color> Milicianos que ayudarán a defenderla de ataques directos.\n\n":
                r = "</color> Militiamen who will help defend it from direct attacks.\n\n";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b7972c>5 Suministros</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Click to abandon <color=#b7972c>5 Supplies</color> and lighten the Load. -1 Hope</b>---</color>\n\n";
                break;
            case "Los <color=#b7972c>Suministros</color> constituyen las reservas de comida y elementos de supervivencia de la caravana.\n\nCada <color=#c918bb>Civil</color> consume 1 en cada Descanso. Los Bueyes consumen 2.\n":
                r = "The <color=#b7972c>Supplies</color> are conformed by the food reserves and survival items of the caravan.\n\nEach <color=#c918bb>Civil</color> consumes 1 at Rest. Each <color=#c918bb>Ox</color> consumes 2.\n";
                break;
            case " <color=#b7972c>Suministros</color>, por un total de peso de ":
                r = " <color=#b7972c>Supplies</color>, for a total weight of ";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b34f09>2 Materiales</color> y alivianar la Carga.</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Click to abandon <color=#b34f09>2 Materials</color> and lighten the Load.</b>---</color>\n\n";
                break;
            case "Los <color=#b34f09>Materiales</color> son elementos básicos de construcción utilizados para mantenimiento y expansión de la caravana.\nCada uno pesa 3.\n":
                r = "The <color=#b34f09>Materials</color> are basic construction elements used for maintenance and expansion of the caravan.\nEach one weighs 3.\n";
                break;
            case " <color=#b34f09>Materiales</color>, por un total de peso de ":
                r = " <color=#b34f09>Materials</color>, for a total weight of ";
                break;
            case "<color=#ffdda5>---<b>Haz click para sacrificar <color=#9e2a1c>1 Buey</color> para obtener <color=#b7972c>20 Suministros</color>. -2 Esperanza</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Click to sacrifice <color=#9e2a1c>1 Ox</color> to obtain <color=#b7972c>20 Supplies</color>. -2 Hope</b>---</color>\n\n";
                break;
            case "Los <color=#9e2a1c>Bueyes</color> son utilizados para llevar la carga de la caravana.\nCada uno da ":
                r = "The <color=#9e2a1c>Oxen</color> are used to carry the caravan's load.\nEach one provides ";
                break;
            case " de Capacidad de Carga.\n":
                r = " Load Capacity.\n";
                break;
            case " <color=#9e2a1c>Bueyes</color>, por un total de Capacidad de Carga de ":
                r = " <color=#9e2a1c>Oxen</color>, for a total Load Capacity of ";
                break;
            case " <color=#b7972c>Suministros</color> y ":
                r = " <color=#b7972c>Supplies</color> and ";
                break;
            case " <color=#b34f09>Materiales</color> por un total de peso de ":
                r = " <color=#b34f09>Materials</color> for a total weight of ";
                break;
            case "<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo que se haga duplica la Fatiga obtenida y reduce 10 la <color=#a0e812>Esperanza</color></color>.\n\n":
                r = "<color=#cc0d0d>The Caravan is Overloaded. Each segment traveled doubles the Fatigue gained and reduces Hope by 10</color>.\n\n";
                break;
            case "El <color=#d8a205>Oro</color> que lleva la Caravana, utilizado para comprar bienes y contratar servicios.":
                r = "<color=#d8a205>Gold</color> carried by the Caravan, used to purchase goods and hire services.";
                break;
            case "Indica que tanta <color=#06c297>Fatiga</color> tiene la Caravana en general.\n":
                r = "Shows how much <color=#06c297>Fatigue</color> the Caravan has in general.\n";
                break;
            case "Cada tramo de viaje la aumenta en 1.\n":
                r = "Each node traveled increases the Fatigue by 1.\n";
                break;
            case "Si descansas volverá a 0 y arrancarán el nuevo dí­a Descansados(1).\n\n":
                r = "If you rest it will return to 0 and the Caravan will start the next day Rested(1).\n\n";
                break;
            case "Actualmente estan Descansados(1), no habrá penalizaciones por viajar.\n\n":
                r = "Currently <color=#a8ff9e>Rested</color>(<color=#a8ff9e>1</color>), there will be no penalties for traveling.\n\n";
                break;
            case "Actualmente estan Frescos(2), no habrá penalizaciones por viajar.":
                r = "Currently <color=#d4ff9e>Fresh</color>(<color=#d4ff9e>2</color>), there will be no penalties for traveling.";
                break;
            case "Actualmente estan En Marcha(3), no habrá penalizaciones por viajar.":
                r = "Currently <color=#fff79e>Marching</color>(<color=#fff79e>3</color>), there will be no penalties for traveling.";
                break;
            case "Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podráan morir si viajas.":
                r = "Currently <color=#ffd19e>Agitated</color>(<color=#ffd19e>4</color>), -10 Hope and few Oxen may die if you travel.";
                break;
            case "Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas.":
                r = "Currently <color=#ff9e9e>Tired</color>(<color=#ff9e9e>5</color>), -15 Hope and some Oxen may die if you travel.";
                break;
            case "Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas.":
                r = "Currently <color=#ff3c3c>Exhausted</color>(<color=#ff3c3c>6</color>), -20 Hope and several Oxen may die if you travel.";
                break;
            case "Dí­a ":
                r = "Day ";
                break;
            case "Soleado: +5 Esperanza.":
                r = "Sunny: +5 Hope.";
                break;
            case "Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3.":
                r = "Heat Wave: +1 Fatigue. \"Free Day\" gives +5 Hope, other Civil Tasks give -3.";
                break;
            case "Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada.":
                r = "Rain: -5 Hope. -15% Supply Gathering, -20% Ambush chances.";
                break;
            case "Nieve: +3 Esperanza. -15% Recolecciones, -20% Emboscada. Viajar lleva el doble de tiempo.":
                r = "Snow: +3 Hope. -15% Gatherings, -20% Ambush. Traveling takes double time.";
                break;
            case "Niebla: -20% Recolecciones, -20% Emboscada, -20% Exploración, +10% Nodos Misteriosos.":
                r = "Fog: -20% Gatherings, -20% Ambush, -20% Exploration, +10% Mysterious Nodes.";
                break;
            case "De un momento a otro, varios miembros de la caravana han desaparecido sin dejar rastro. Nadie tiene una explicación de lo que ha sucedido. Pero el miedo y la incertidumbre se apoderan de todos.\n":
                r = "Suddenly, several members of the caravan have disappeared without a trace. No one has an explanation for what happened. But fear and uncertainty take hold of everyone.\n";
                break;
            case "Luego de buscar vagamente en la cercaní­a y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n":
                r = "After vaguely searching the area and concluding that there are no clues, you decide to comfort the relatives and move on.\n\n\n\n\n\n\n";
                break;
            case "<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>You lose 4-12 Civilians, -5 Hope.</b></color>";
                break;
            case "Uno de los bueyes de la caravana ha caí­do enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n":
                r = "One of the oxen in the caravan has fallen ill and cannot continue. You receive recommendations from some livestock specialists advising you to check the other oxen to prevent the spread of the disease.\n\n\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisarlos tomará unas horas: +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to check them it will take a few hours: +1 Black Breath Progress.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides ignorar las advertencias: 1-3 Bueyes mas morirán.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to ignore the warnings: 1-3 more Oxen will die.</color>\n\n";
                break;
            case "Mientras la caravana se dispone a avanzar por un terreno peligroso, se topa con un grupo de bandidos que exige un peaje exorbitante para dejar pasar a la caravana.\n\n":
                r = "As the caravan prepares to move through dangerous terrain, it encounters a group of bandits demanding an exorbitant toll to let the caravan pass.\n\n";
                break;
            case "<color=#ba3fef>-Si decides pagar el peaje, perderás 1 de Oro por Civil.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to pay the toll, you will lose 1 Gold per Civilian.</color>\n\n";
                break;
            case "</color></b> se acerca a ti y no luce nada bien. Te comenta que ha empezado a sentirse enfermo y necesita medicina para mejorar pronto y estar nuevamente en condiciones de combatir.\n\n":
                r = "</color></b> approaches you and doesn't look well at all. He tells you that he has started to feel sick and needs medicine to get better soon and be in fighting condition again.\n\n";
                break;
            case "<color=#ba3fef>-Puedes dar un discurso motivador, refutando sus argumentos con hechos.</color> Chances: %":
                r = "<color=#ba3fef>-You can give a motivational speech, refuting his arguments with facts.</color> Chances: %";
                break;
            case " <i>(Determinado por Esperanza) Éxito: +15 Esperanza. Fallo: -20 Esperanza.</i> \n\n":
                r = " <i>(Determined by Hope) Success: +15 Hope. Failure: -20 Hope.</i> \n\n";
                break;
            case "Durante la noche, <b><color=#d1006f>":
                r = "During the night, <b><color=#d1006f>";
                break;
            case "</color></b> junto con algunos Civiles comienzan a contar chistes y anécdotas divertidas, riendo y disfrutando del momento.\n":
                r = "</color></b> along with some Civilians start telling jokes and funny anecdotes, laughing and enjoying the moment.\n";
                break;
            case " y ":
                r = " and ";
                break;
            case " ganan Alta Moral por 3 dí­as.</b></color>":
                r = " gain High Morale for 3 days.</b></color>";
                break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí­ y esta caravana no sobrevivió.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n":
                r = "<color=#ba3fef>-You can order the Caravan to loot the Supplies.</color> +21-35 Supplies, +5-11 Materials, +15-35 Gold, -5 Hope.</i> \n\n";
                break;
            case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
                r = "The Caravan stops at an abandoned sawmill, some trees have been felled and the wood is piled up in disarray.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n":
                r = "<color=#ba3fef>-You can order the Caravan to gather all the wood.</color> +65-90 Materials, +1 Fatigue, +1 Progress of the Black Breath.</i> \n\n";
                break;
            case "<color=#ba3fef>-Puedes optar por dejarlo cazar, o directamente domesticar a un puñado para que se sumen a la Caravana. +2-3 Bueyes</i> \n\n":
                r = "<color=#ba3fef>-You can choose to let it hunt, or directly tame a handful to join the Caravan. +2-3 Oxen</i> \n\n";
                break;
            case "<color=#ba3fef>-Puedes defender a los civiles de sus perseguidores mientras les das tiempo a los más débiles a sumarse a la Caravana.</color> Combate Normal - +18-26 Civiles\n\n":
                r = "<color=#ba3fef>-You can defend the civilians from their pursuers while giving the weaker ones time to join the Caravan.</color> Normal Combat - +18-26 Civilians\n\n";
                break;
            case "<color=#ba3fef>-Puedes aceptar solo a los mas ágiles y huir para evitar confrontar con sus perseguidores.</color> +5-10 Civiles -5 Esperanza\n\n":
                r = "<color=#ba3fef>-You can only accept the most agile ones and flee to avoid confronting their pursuers.</color> +5-10 Civilians -5 Hope\n\n";
                break;
            case "</color></b> se acerca a ti y coloca una mano en tu hombro y dice: -'Tengo mucha esperanza en usted, y creo que será exitoso al liderarnos a salvo hacia el puerto'.\n":
                r = "</color></b> approaches you, places a hand on your shoulder and says: -'I have a lot of hope in you, and I believe you will be successful in leading us safely to the port'.\n";
                break;
            case "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un sí­mbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ":
                r = "With his other outstretched hand, he holds a bag of gold and offers it to you kindly. -'Consider it a symbol of my trust in you, as well as a contribution that may be useful for the Caravan.'-he says\n ";
                break;
            case "<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: ":
                r = "<color=#ba3fef>You respond: -'Keep the money, your contribution to the Caravan is already considerable with your daily effort, and I am more than grateful to be able to count on you.'</color> Effects: ";
                break;
            case " gana Alta Moral por 4 dí­as y 50 Experiencia. \n\n":
                r = " gains High Morale for 4 days and 50 Experience. \n\n";
                break;
            case "<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n":
                r = "<color=#ba3fef>You respond: -'I accept your offer, there is no money to spare in our current situation and it will surely help us during the journey, thank you.'</color> Effects: +120-160 Gold. \n\n";
                break;
            case "Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchí­simos años en la zona y la conoce a la perfección.\n":
                r = "An old man appears at the side of the road waving his hands at the Caravan. Up close, you realize that this man has been living in the area for many years and knows it perfectly.\n";
                break;
            case "'Aliento Negro o no, mis dí­as ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n":
                r = "'Black Breath or not, my days are already numbered. But I can share my knowledge about this land, as a final act of kindness.'- he says\n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'¿Conoce algún atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n":
                r = "<color=#ba3fef>Question: -'Do you know of any shortcut that can take us away from imminent danger for at least a few miles?'</color> Effects: If possible, a subterranean shortcut will be generated. \n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n":
                r = "<color=#ba3fef>Question: -'Describe the surrounding area so we can make more informed decisions.'</color> Effects: Upcoming nodes will be revealed. \n\n";
                break;
            case "</color></b> se lo ve con mucha energí­a y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n":
                r = "</color></b> looks very energetic and determined as he goes about his usual tasks. When you approach him, he tells you that he had a Dream in which he saw the Caravan reaching its destination.\n";
                break;
            case "'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n":
                r = "'In the dream, I saw a clear path to our destination. There will be dangers and difficulties, but I am convinced that we will make it. Let's follow that route.'- he says with Determination\n\n\n";
                break;
            case "</color></b> obtiene 150 Experiencia y Alta Moral por 5 dí­as.</color>\n\n":
                r = "</color></b> gains 150 Experience and High Morale for 5 days.</color>\n\n";
                break;
            case "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mí­nimo.\n":
                r = "You have arrived at a beautiful natural clearing that seems to have been untouched by corruption and pestilence in the slightest.\n";
                break;
            case "Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n":
                r = "It is an excellent place to rest and recover strength.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrá también beneficios adicionales:\n-El Aliento Negro avanzará solo 1.\n-+10% curación recibida.\n-El evento será positivo.</b></color>":
                r = "<color=#a0e812><b>+5 Hope.\n\nResting in this place will also have additional benefits:\n-The Black Breath will only advance 1.\n-+10% healing received.\n-The event will be positive.</b></color>";
                break;
            case "Has llegado a un pequeño asentamiento. Notas que los civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.":
                r = "You have arrived at a small settlement. You notice that the civilians are disorganized and need leadership to survive the Black Breath.";
                break;
            case "\nDe 15-25 Civiles se unirán a la Caravana y brindarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.":
                r = "\n15-25 Civilians will join the Caravan and provide 50-60 Supplies, 6-8 Materials, 2-4 Oxen, and 60-70 Gold.";
                break;
            case "\nUn Héroe aleatorio se sumará a tus fuerzas.\n\n\n\n\n":
                r = "\nA random Hero will join your forces.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>\nDescansar en este lugar tendrá beneficios adicionales:\n-+20% curación recibida.\n-0% chances de emboscada al descansar.</b></color>":
                r = "<color=#a0e812><b>\nResting in this place will have additional benefits:\n-+20% healing received.\n-0% ambush chance while resting.</b></color>";
                break;
            case "Has llegado a un lugar rico en recursos naturales, los civiles se han puesto a recolectar lo que han podido.":
                r = "You have arrived at a resource-rich area, and the civilians have started gathering what they can.";
                break;
            case "\nSe conseguirán de 18-30 Materiales y 80-140 Suministros.":
                r = "\n18-30 Materials and 80-140 Supplies will be gathered.";
                break;
            case "<color=#a0e812><b>\n\nDescansar en este lugar tendrá beneficios adicionales:+20% efectividad a tareas de Recolección.</b></color>":
                r = "<color=#a0e812><b>\n\nResting in this place will have additional benefits:+20% effectiveness on Gathering tasks.</b></color>";
                break;
            case " de oro. -5 Esperanza por el interrogatorio":
                r = " Gold. -5 Hope for the interrogation";
                break;
            case "Omitir Tutorial":
                r = "Skip Tutorial";
                break;
            case "Selecciona una tarea civil para el descanso":
                r = "Select a civilian task";
                break;
            case "Menu de Descanso ":
                r = "Camp Menu";
                break;
            case "Descansar":
                r = "Rest";
                break;
            case "Carga":
                r = "Load";
                break;
            case "Puesto Comercial":
                r = "Trading Post";
                break;
            case "Suministros":
                r = "Supplies";
                break;
            case "Compra 10x 200 Oro":
                r = "Buy 10x 200 Gold";
                break;
            case "Materiales":
                r = "Materials";
                break;
            case "Bueyes":
                r = "Oxen";
                break;
            case "Santuario de Purificadores":
                r = "Purifier's Sanctuary";
                break;
            case "3 Bueyes":
                r = "3 Oxen";
                break;
            case "200 Oro":
                r = "200 Gold";
                break;
            case "Haz tu ofrenda":
                r = "Make your offering";
                break;
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3 y un personaje con Corrupción al azar será curado.":
                r = "Upon making the offering, the Black Breath will recede by 3 and a random character with Corruption will be healed.";
                break;
            case "Sacrificar ":
                r = "Sacrifice ";
                break;
            case "Donar":
                r = "Donate";
                break;
            case "Abandonar":
                r = "Abandon";
                break;
            case "Elegir":
                r = "Choose";
                break;
            case "Un solitario viajero pide unirse a la Caravana, parece capaz de defenderse sólo, seguramente sumarlo a la Caravana pueda ser beneficioso.":
                r = "A solitary traveler asks to join the Caravan, they seem capable of defending themselves alone, surely adding them to the Caravan could be beneficial.";
                break;
            case "Aceptarlos":
                r = "Take them in";
                break;
            case "Defensas: Cada Tier mejora las defensas de la Caravana en ataques directos y reduce 10% las chances de perder un Séquito. ":
                r = "Defenses: Each Tier improves the Caravan's defenses in direct attacks and reduces the chances of losing a Retinue by 10%. ";
                break;
            case "30 Materiales":
                r = "30 Materials";
                break;
            case "Antorchas de Pie: Cada Tier reduce 5% el riesgo de sufrir una emboscada al Descansar.":
                r = "Standing Torches: Each Tier reduces the risk of ambush while Resting by 5%.";
                break;
            case "Alforjas: Cada Tier aumenta en 1 la Capacidad de carga de cada Buey.":
                r = "Saddlebags: Each Tier increases the carrying capacity of each Ox by 1.";
                break;
            case "Tiendas: Cada Tier da 5 de Esperanza al descansar y +1 Capacidad de Personaje.":
                r = "Tents: Each Tier grants 5 Hope when resting and +1 Character Capacity.";
                break;
            case "Catalejos: Cada Tier aumenta 5% las chances de Exploración y 5% las chances de encontrar Objetos tras una Batalla ganada.":
                r = "Spyglasses: Each Tier increases Exploration chances by 5% and the chances of finding Items after a won Battle by 5%.";
                break;
            case "Carro Almacén: Cada Tier reduce 5% Suministros consumidos por Descanso.":
                r = "Supply Wagon: Each Tier reduces 5% supplies consumed by Resting.";
                break;
            case "Planes de mejoras ":
                r = "Improvement plans ";
                break;
            case "  Resistencias":
                r = "  Resistances";
                break;
            case "Rasgos":
                r = "Traits";
                break;
            case "Punto de Atributo!":
                r = "Attribute Point!";
                break;
            case "Punto de Salvación!":
                r = "Saving throw Point!";
                break;
            case "Punto de Habilidad!":
                r = "Skill Point!";
                break;
            case "Posición":
                r = "Position";
                break;
            case "Elije una nueva Habilidad!":
                r = "Choose a new Skill!";
                break;
            case "¡Batalla!":
                r = "Battle!";
                break;
            case "Selecciona a tus personajes.":
                r = "Select your characters.";
                break;
            case "Comenzar":
                r = "Start Battle";
                break;
            case "¡Ataque a la Caravana!":
                r = "The caravan is under attack!";
                break;
            case "Personajes en Guardia disponibles.":
                r = "Available Guarding Characters.";
                break;
            case "Victoria":
                r = "Victory";
                break;
            case "Derrota":
                r = "Defeat";
                break;
            case "Turno Enemigo":
                r = "Enemy Turn";
                break;
            case "Turno Aliado":
                r = "Ally Turn";
                break;
            case "Terminar Turno":
                r = "End Turn";
                break;
            case "Ronda Nueva":
                r = "New Round";
                break;
            case "Volver":
                r = "Back";
                break;
            case "Salir":
                r = "Exit";
                break;
            case "-Es un dí­a hermoso. +5 Esperanza.":
                r = "It's a beautiful day. +5 Hope.";
                break;
            case "-La Ola de Calor se hace insoportable. +1 Fatiga.":
                r = "The Heat Wave becomes unbearable. +1 Fatigue.";
                break;
            case "-La Lluvia hace el viaje más difí­cil. -5 Esperanza.":
                r = "The Rain makes the journey more difficult. -5 Hope.";
                break;
            case "-La Nieve mejora el ánimo. +3 Esperanza.":
                r = "The Snow improves morale. +3 Hope.";
                break;
            case "% - Tirada: 1d100 = ":
                r = "% - Roll: 1d100 = ";
                break;
            case "Tirada: ":
                r = "Roll: ";
                break;
            case "-La caravana han sufrido un Ataque durante el descanso. Probabilidades ":
                r = "-The caravan suffered an attack during rest. Chances ";
                break;
            case "-Durante el descanso, el Aliento Negro ha avanzado 2.":
                r = "-During rest, the Black Breath has advanced 2.";
                break;
            case "-Durante el descanso en el Claro, el Aliento Negro ha avanzado 1.":
                r = "-During rest in the Clearing, the Black Breath has advanced 1.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro.":
                r = " has successfully performed a Cleansing Ritual during rest, preventing the advance of the Black Breath.";
                break;
            case "-Debido a la alta Esperanza, los Acechadores han decidido no cobrar su sueldo esta vez.":
                r = "-Due to the high Hope, the Stalkers have decided not to collect their pay this time.";
                break;
            case "-Los Acechadores en la Caravana se han cobrado su sueldo por Oro: ":
                r = "-The Stalkers in the Caravan have collected their pay in Gold: ";
                break;
            case "-Debido al gran optimismo que rodea la Caravana, los Civiles han donado Oro: ":
                r = "-Due to the great optimism surrounding the Caravan, the Civilians have donated Gold: ";
                break;
            case "-Debido al optimismo que rodea la Caravana, los Civiles han donado Oro: ":
                r = "-Due to the optimism surrounding the Caravan, the Civilians have donated Gold: ";
                break;
            case "-Por la muy baja Esperanza ":
                r = "-Due to the very low Hope ";
                break;
            case " Civiles han abandonado la Caravana.":
                r = " Civilians have abandoned the Caravan.";
                break;
            case "-Por la baja Esperanza ":
                r = "-Due to the low Hope ";
                break;
            case " Civiles.":
                r = "Civilians.";
                break;
            case "-La falta de Suministros ha provocado la muerte de ":
                r = "-The lack of Supplies has caused the death of ";
                break;
            case "-Los Esclavos han recolectado ":
                r = "-The Slaves have collected ";
                break;
            case "-Los Herboristas han preparado sus Bálsamos.":
                r = "-The Herbalists have prepared their Balms.";
                break;
            case "-En la Feria, los Artistas han realizado un espectáculo que ha levantado el ánimo de los Civiles. +10 Esperanza":
                r = "-At the Fair, the Artists put on a show that lifted the spirits of the Civilians. +10 Hope";
                break;
            case " se cura ":
                r = " heals ";
                break;
            case " PV tras el Descanso.":
                r = " HP after Rest.";
                break;
            case "-El Séquito de Curanderos ha reducido la enfermedad de":
                r = "-The Healer Retinue has reduced the illness of";
                break;
            case " en 1 extra.":
                r = " by 1 extra.";
                break;
            case " comparte sus historias de batalla con los civiles. +4 Esperanza":
                r = " shares his battle stories with the civilians. +4 Hope";
                break;
            case "-El tener que trabajar en plena Ola de Calor, ha caí­do mal en los Civiles. -3 Esperanza":
                r = "-Having to work in the middle of the Heat Wave has not gone well with the Civilians. -3 Hope";
                break;
            case "-El tener un Dí­a Libre en plena Ola de Calor, ha caí­do bien en los Civiles. +5 Esperanza":
                r = "-Having a Day Off in the middle of the Heat Wave has been well received by the Civilians. +5 Hope";
                break;
            case "Las probabilidades de exploración: ":
                r = "Exploration chances: ";
                break;
            case "Las probabilidades de sufrir un ataque a la Caravana ":
                r = "Ambush chances: ";
                break;
            case "<b><u>Estado de Alerta</b></u>\n\n\n":
                r = "<b><u>High Alert</b></u>\n\n\n";
                break;
            case "Durante el descanso, se asignarán a los civiles mas aptos fí­sicamente a la vigilancia del area circundante al campamento.\n\n":
                r = "During rest, the most physically fit civilians will be assigned to surveil the area surrounding the camp.\n\n";
                break;
            case "<color=#d8a205>Reduce chances de ataque a caravana. +20% a Exploración. -10 Esperanza.</color>\n\n\n":
                r = "<color=#d8a205>Reduces chances of Ambush during this rest. +20% to Exploration. -10 Hope.</color>\n\n\n";
                break;
            case "<b><u>Dí­a Libre</b></u>\n\n\n":
                r = "<b><u>Day Off</b></u>\n\n\n";
                break;
            case "Los civiles se tomarán el dí­a para descansar y recobrar fuerzas.\n\n":
                r = "The civilians will take the day to rest and regain strength.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá 10 de Esperanza y el dí­a siguiente arrancará con -1 Fatiga.</color>\n\n\n":
                r = "<color=#d8a205>You will gain 10 Hope and the next day will start with -1 Fatigue.</color>\n\n\n";
                break;
            case "<b><u>Feria</b></u>\n\n\n":
                r = "<b><u>Fair</b></u>\n\n\n";
                break;
            case "Los civiles dedicarán el dí­a a organizar una feria con varios juegos y celebraciones.\n\n":
                r = "The civilians will dedicate the day to organizing a fair with various games and celebrations.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá entre 15 y 25 de Esperanza y se consumirán 20% más de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n":
                r = "<color=#d8a205>You will gain between 15 and 25 Hope and consume 20% more Supplies. <color=#bb280d>+10% chances of Ambush.</color></color>\n\n\n";
                break;
            case "<b><u>Recolección de Materiales</b></u>\n\n\n":
                r = "<b><u>Material Collection</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarán a recolectar materiales básicos en la zona.\n\n":
                r = "The civilians will dedicate themselves to collecting basic materials in the area.\n\n";
                break;
            case "<color=#d8a205>Se juntarán entre ":
                r = "<color=#d8a205>You will gather between ";
                break;
            case " materiales. </color>\n\n\n":
                r = " materials. </color>\n\n\n";
                break;
            case "<b><u>Recolección de Suministros</b></u>\n\n\n":
                r = "<b><u>Supplies Collection</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarán a recolectar distintos suministros de las inmediaciones al campamento.\n\n":
                r = "The civilians will dedicate themselves to collecting various supplies from the surroundings of the camp.\n\n";
                break;
            case " suministros. </color>\n\n\n":
                r = " supplies. </color>\n\n\n";
                break;
            case "Combate directo.":
                r = "Direct combat.";
                break;
            case "Evento aleatorio.":
                r = "Random event.";
                break;
            case "Claro tranquilo.":
                r = "Peaceful clearing.";
                break;
            case "Recolección de Recursos.":
                r = "Resource Gathering.";
                break;
            case "Puesto de Comercio.":
                r = "Trading Post.";
                break;
            case "Adquisición de Personajes.":
                r = "Character Acquisition.";
                break;
            case "Combate directo contra enemigos de élite.":
                r = "Direct combat against Elite enemies.";
                break;
            case "Batalla final de la Zona actual.":
                r = "Final battle of the current Zone.";
                break;
            case "<b>(!)</b> Zona Expuesta, la caravana será emboscada.":
                r = "<b>(!)</b> Exposed Zone, the caravan will be ambushed.";
                break;
            case "Nodo Desconocido.":
                r = "Unknown Node.";
                break;
            case "Nodo Misterioso, no se ha logrado revelar.":
                r = "Mysterious Node, it has not been revealed.";
                break;
            case "Salida del atajo subterraneo, no sabemos que hay del otro lado.":
                r = "Exit of the underground passage, we don't know what's on the other side.";
                break;
            case "Santuario de Purificadores.":
                r = "Purifier's Sanctuary.";
                break;
            case "<color=#7ED6F7>-Durante el Descanso, se ha Explorado con Éxito el camino adelante.</color>":
                r = "<color=#7ED6F7>-During Rest, the path ahead has been successfully explored.</color>";
                break;
            case " ha Explorado con Éxito el camino adelante.</color>":
                r = " has successfully scouted the path ahead.</color>";
                break;
            case "-Al viajar por el atajo subterráneo, la moral de la caravana disminuye. -5 Esperanza":
                r = "-While traveling through the underground passage, the caravan's morale decreases. -5 Hope";
                break;
            case "-Se ha encontrado un atajo subterráneo.":
                r = "-An underground passage has been found.";
                break;
            case "<color=#7ED6F7>-Entre la bruma del camino, la caravana distingue una aldea a la distancia. Se ha descubierto un asentamiento.</color>":
                r = "<color=#7ED6F7>-Through the road's mist, the caravan spots a village in the distance. A settlement has been discovered.</color>";
                break;
            case "-La Caravana ha viajado con exceso de Carga. -10 Esperanza +1 Fatiga":
                r = "-The Caravan has traveled with excess Load. -10 Hope +1 Fatigue";
                break;
            case "Fuerza: ":
                r = "Strength: ";
                break;
            case "Agilidad: ":
                r = "Agility: ";
                break;
            case "Poder: ":
                r = "Power: ";
                break;
            case "Iniciativa: ":
                r = "Initiative: ";
                break;
            case "PA: ":
                r = "AP: ";
                break;
            case "Valentí­a: ":
                r = "Valour: ";
                break;
            case "Armadura: ":
                r = "Armor: ";
                break;
            case "Defensa: ":
                r = "Defense: ";
                break;
            case "-Reflejos: ":
                r = "-Reflexes: ";
                break;
            case "-Fortaleza: ":
                r = "-Fortitude: ";
                break;
            case "-Mental: ":
                r = "-Mental: ";
                break;
            case "<color=#2a9c71>\n\nFatigado: -1 PA máximo. </color>":
                r = "<color=#2a9c71>\n\nFatigued: -1 max AP. </color>";
                break;
            case "Bendecido por Plegaria: +1 Ataque +1 Defensa +5 Res.Necro +2 TSMental.</color>":
                r = "Blessed by Prayer: +1 Attack +1 Defense +5 Res.Necro +2 TS Mental.</color>";
                break;
            case "<color=#d80404>\n\nHerido:-1 Atributos. Si cae en combate, muere. </color>":
                r = "<color=#d80404>\n\nWounded: -1 Attributes. If falls in combat, dies. </color>";
                break;
            case "<color=#d80404>\n\nCorrupto: Los enemigos corrompidos se curan al atacarlo, le infligen mas daño, y si lo derriban en combate, muere. </color>":
                r = "<color=#d80404>\n\nCorrupted: Corrupted enemies heal when attacking it, deal more damage, and if knocked down in combat, dies. </color>";
                break;
            case "<color=#d80404>\n\nEnfermo por ":
                r = "<color=#d80404>\n\nSick for ";
                break;
            case " dí­as. -15% daño, -3 TS Fortaleza, -1 PA </color>":
                r = " days. -15% damage, -3 Fortitude, -1 AP </color>";
                break;
            case "<color=#d80404>\n\nBaja Moral por ":
                r = "<color=#d80404>\n\nLow Morale for ";
                break;
            case " dí­as. -1 Ataque y Defensa, -3 TS Mental, -2 Valentí­a Inicial</color>":
                r = " days. -1 Attack and Defense, -3 TS Mental, -2 Initial Valour</color>";
                break;
            case "<color=#d80404>\n\nAlta Moral por ":
                r = "<color=#d80404>\n\nHigh Morale for ";
                break;
            case " dí­as. +1 Ataque, +2 TS Mental, +2 Valentí­a Inicial</color>":
                r = " days. +1 Attack, +2 TS Mental, +2 Initial Valour</color>";
                break;
            case "Torpe: +1 Rango Pifias. ":
                r = "Clumsy: +1 Fumble Range.";
                break;
            case "Valiente: +2 Valentí­a Máxima.":
                r = "Brave: +2 Max Valour.";
                break;
            case "Alegre: +2 Esperanza al Descansar.":
                r = "Cheerful: +2 Hope when Resting.";
                break;
            case "Inventario":
                r = "Inventory";
                break;
            case "Accesorios":
                r = "Accessories";
                break;
            case "Arma":
                r = "Weapon";
                break;
            case "Armadura":
                r = "Armor";
                break;
            case "Consumibles":
                r = "Consumables";
                break;
            case "<color=#0cca74><b>Guardia: </b></color><color=#d3d3d3><i>El personaje se mantendrá alerta y custodiará la caravana.</color></i>\\n\\nSi se produce una emboscada, podrá participar de la defensa sin penalización. +3% Exploración al descansar.":
                r = "<color=#0cca74><b>Guard: </b></color><color=#d3d3d3><i>The character will remain alert and guard the caravan.</color></i>\\n\\nIf an ambush occurs, they can participate in the defense without penalty. +3% Scouuting when resting.";
                break;
            case "<color=#0cca74><b>Coerción: </b></color><color=#d3d3d3><i>Con métodos cuestionables, el Acechador obliga a los Mercaderes a donar dinero a la caravana.</color></i>\\n\\n+1-10 Oro y -1 Esperanza por dí­a.":
                r = "<color=#0cca74><b>Coercion: </b></color><color=#d3d3d3><i>Using questionable methods, the Stalker forces Merchants to donate money to the caravan.</color></i>\\n\\n+1-10 Gold and -1 Hope per day.";
                break;
            case "<color=#0cca74><b>Exploración: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo Subterráneo\\nSi se da un combate, lo arranca Fatigado.":
                r = "<color=#0cca74><b>Scouting: </b></color><color=#d3d3d3><i>The character scouts possible destinations ahead of the caravan.</color></i>\\n\\nHas a 40% chance to reveal future Nodes when traveling to a new Node. -5% Chance of Mysterious Node. +5% Chance of Underground Shortcut\\nIf a combat occurs, they start Fatigued.";
                break;
            case "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirá su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrá +3 Flechas y +5% daño.":
                r = "<color=#0cca74><b>Prepare Arrows: </b></color><color=#d3d3d3><i>The character will spend their time creating and improving their arrows.</color></i>\\n\\nIf a combat occurs, they will have +3 Arrows and +5% damage.";
                break;
            case "<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocupará de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzará con +3 Armadura.":
                r = "<color=#0cca74><b>Armor Maintenance: </b></color><color=#d3d3d3><i>The character will take care of his armor.</color></i>\\n\\nIf a combat occurs, he will start with +3 Armor.";
                break;
            case "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerá vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrá participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daño los primeros 2 turnos.":
                r = "<color=#0cca74><b>Watch: </b></color><color=#d3d3d3><i>The character will remain vigilant against any danger.</color></i>\\n\\nIf an ambush occurs, they can actively participate in the defense and gain +2 AP, +5 Initiative, and +20% damage for the first 2 turns.";
                break;
            case "<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizará su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada dí­a que pase ganará 15 Experiencia.\\nSi se produce un combate, lo arrancará Fatigado.":
                r = "<color=#0cca74><b>Train: </b></color><color=#d3d3d3><i>The character will use their free time to train and stay in shape.</color></i>\\n\\nEach day that passes, they will gain 15 Experience.\\nIf a combat occurs, they will start Fatigued.";
                break;
            case "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nCada dí­a que pase recuperará un 15% de salud.\\nSi se produce un combate, lo arrancará Fresco.":
                r = "<color=#0cca74><b>Rest: </b></color><color=#d3d3d3><i>The character will focus on resting and recovering their health.</color></i>\\n\\nEach day that passes, they will recover 15% of their health.\\nIf a combat occurs, they will start Fresh.";
                break;
            case "<color=#0cca74><b>Afilar Armas: </b></color><color=#d3d3d3><i>El Acechador se encarga de mantener sus armas afiladas.</color></i>\\n\\nSi se produce un combate tendrá +10% daño.":
                r = "<color=#0cca74><b>Prepare Weapons: </b></color><color=#d3d3d3><i>The Stalker is responsible for keeping his weapons sharp.</color></i>\\n\\nIf a combat occurs, he will have +10% damage.";
                break;
            case "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.":
                r = "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>With his arcane telekinesis powers, he helps with the caravan's load.</color></i>\\n\\n+20 Carrying Capacity.";
                break;
            case "<color=#0cca74><b>Caza Nocturna: </b></color><color=#d3d3d3><i>El personaje cazará en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+1d4 Suministros por dí­a. +3% probabilidad de Emboscada Enemiga al descansar.":
                r = "<color=#0cca74><b>Night Hunting: </b></color><color=#d3d3d3><i>The character will hunt in the vicinity to obtain food for the caravan.</color></i>\\n\\n+1d4 Supplies per day. +3% chance of Enemy Ambush while resting.";
                break;
            case "<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirá los relatos de sus hazañas con quienes quieran oí­rlas.</color></i>\\n\\n+10 Experiencia por dí­a a personajes de nivel inferior. +4 Esperanza al descansar.":
                r = "<color=#0cca74><b>Battle Tales: </b></color><color=#d3d3d3><i>The character will share the tales of their exploits with those who wish to hear them.</color></i>\\n\\n+10 Experience per day to lower-level characters. +4 Hope while resting.";
                break;
            case "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizará rituales de protección para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por dí­a.":
                r = "<color=#0cca74><b>Ritual of Cleansing: </b></color><color=#d3d3d3><i>The Purifier will perform protection rituals to combat the Black Breath.</color></i>\\n\\nChance to avoid Black Breath advance: 25% while resting, 15% per day.";
                break;
            case "<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usará su tiempo para ayudar a los rezagados y más débiles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.":
                r = "<color=#0cca74><b>Help the Hopeless: </b></color><color=#d3d3d3><i>The Purifier will use her time to help the laggards and weaker members of the caravan.</color></i>\\n\\n+1d3 Hope per day. +1 Fervor in combat.";
                break;
            case "<color=#0cca74><b>Concentración Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de Energí­a al iniciar combates.":
                r = "<color=#0cca74><b>Arcane Concentration: </b></color><color=#d3d3d3><i>The Channeler focuses and keeps their power ready for any combat that arises.</color></i>\\n\\n+1 Energy Level at the start of combats.";
                break;
            case "<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.\\nEn Ataque a Caravana cuenta como Guardia y comienza en Sigilo.":
                r = "<color=#0cca74><b>Watch from Shadows: </b></color><color=#d3d3d3><i>The Stalker moves stealthily around the caravan, trying to anticipate enemy ambushes.</color></i>\\n\\n-5% chance of ambushes.\\nIn Caravan Attack it counts as Guard and starts Hidden.";
                break;
            case "<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>Séquito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curación diaria del Séquito de Curanderos.":
                r = "<color=#0cca74><b>Help the Healers' Retinue: </b></color><color=#d3d3d3><i>Helps the <b>Healers' Retinue</b> in their tasks, increasing their effectiveness.</color></i>\\n\\nIncreases the Healers' Retinue's daily healing by 5%.";
                break;
            case "<color=#0cca74><b>Crear Sí­mbolo Arcano de Protección: </b></color><color=#d3d3d3><i>El Canalizador concentra energí­a arcana protectora en un sí­mbolo que puede proteger a quien lo utilice.</color></i>\\n\\nCrea un Sí­mbolo Arcano de Protección por dí­a.":
                r = "<color=#0cca74><b>Create Arcane Protection Symbol: </b></color><color=#d3d3d3><i>The Channeler concentrates protective arcane energy into a symbol that can protect its user.</color></i>\\n\\nCreates one Arcane Protection Symbol per day.";
                break;
            case "-El viaje por el camino sinuoso ha retrasado la caravana. +":
                r = "-The journey along the winding path has delayed the caravan. +";
                break;
            case " Avance del Aliento Negro":
                r = " Black Breath advance";
                break;
            case "-La nieve a retrasado el viaje. +1 Avance del Aliento Negro":
                r = "-The snow has delayed the journey. +1 Black Breath advance";
                break;
            case "-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza":
                r = "-The absence of Black Breath while traveling inspires the Caravan. +2 Hope";
                break;
            case "-La presencia notable del Aliento Negro al viajar, provoca incertidumbre en la Caravana. -3 Esperanza":
                r = "-The noticeable presence of Black Breath while traveling causes uncertainty in the Caravan. -3 Hope";
                break;
            case "-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -5 Esperanza":
                r = "-The strong presence of Black Breath in the air causes fear in the Caravan. -5 Hope";
                break;
            case "-La presencia de Aliento Negro en el aire es fatal para los Civiles. -7 Esperanza -":
                r = "-The Black Breath in the air is fatal for Civilians. -7 Hope -";
                break;
            case " Civiles":
                r = " Civilians";
                break;
            case "-El Séquito de Herboristas ha visitado un Claro y recolectado hierbas curativas.":
                r = "-The Herbalist Retinue has visited a Glade and collected healing herbs.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza, previniendo el avance del Aliento Negro.":
                r = "has succesfully performed a Cleansing Ritual, avoiding the Black Breath advance.";
                break;
            case "-Los rezos constantes del Séquito de Clérigos han logrado frenar el avance del Aliento Negro.":
                r = "-The Cleric Retinue's constant prayers have halted the advance of the Black Breath.";
                break;
            case "-Un nuevo personaje se ha unido a la caravana: ":
                r = "-A new character has joined the caravan: ";
                break;
            case "Envenenado":
                r = "Poisoned";
                break;
            case " ha sido envenenado por ":
                r = " has been poisoned by ";
                break;
            case " fue Encarnado por Fuego Fatuo":
                r = " was Possessed by Will-o'-the-Wisp";
                break;
            case " reacciona con ":
                r = " reacts with ";
                break;
            case " se ha unido a la batalla. Quedan ":
                r = " has joined the battle. Remaining ";
                break;
            case " refuerzos.</color> ":
                r = " reinforcements.</color> ";
                break;
            case " ya no tiene ":
                r = " is no longer ";
                break;
            case "No puedes intercambiar con una unidad inmovilizada.":
                r = "You cannot swap with an immobilized unit.";
                break;
            case "No puedes intercambiar con una unidad que ya está Desplazada.":
                r = "You cannot swap with a unit that is already Displaced.";
                break;
            case "No puedes intercambiar con obstáculos.":
                r = "You cannot swap with obstacles.";
                break;
            case "No tienes PA suficientes para intercambiar.":
                r = "You do not have enough AP to swap.";
                break;
            case "Apagando!":
                r = "Extinguishing!";
                break;
            case " gasta 1 PA para apagar el fuego.":
                r = " spends 1 AP to extinguish the fire.";
                break;
            case " está congelado.":
                r = " is frozen.";
                break;
            case "Descongelado!":
                r = "Unfrozen!";
                break;
            case " se libró del congelamiento.":
                r = " is no longer frozen.";
                break;
            case " está aturdido.":
                r = " is stunned.";
                break;
            case " regenera ":
                r = " regenerates ";
                break;
            case " Armadura.":
                r = " Armor.";
                break;
            case " está inmovilizado.":
                r = " is immobilized.";
                break;
            case " recibe ":
                r = " receives ";
                break;
            case " daño veneno.":
                r = " poison damage.";
                break;
            case "Veneno":
                r = "Poison";
                break;
            case "Sangrado":
                r = "Bleeding";
                break;
            case "Ardiendo":
                r = "Burning";
                break;
            case " resiste totalmente al veneno.":
                r = " resist the poison.";
                break;
            case " falla su Tirada de salvación y el veneno empeora.":
                r = " fails its Saving Throw and the poison intensifies.";
                break;
            case " arde":
                r = " burns";
                break;
            case "Inmune":
                r = "Immune";
                break;
            case " veneno":
                r = " Poison";
                break;
            case " frio":
                r = " Cold";
                break;
            case " aturde":
                r = " Stunned";
                break;
            case " inmóvil":
                r = " Immobile";
                break;
            case " sangrado":
                r = " Bleeding";
                break;
            case " acido":
                r = " Acid";
                break;
            case " sigue canalizando.":
                r = " is still channeling.";
                break;
            case " ya no está escondido.":
                r = " is no longer hidden.";
                break;
            case " está escondido.":
                r = " is hidden.";
                break;
            case "La Barrera de ":
                r = "The Barrier of ";
                break;
            case " absorbió ":
                r = " absorbed ";
                break;
            case " de daño.":
                r = " of damage.";
                break;
            case " de daño ":
                r = " of damage type";
                break;
            case "Cura ":
                r = "Heal ";
                break;
            case " recibe <color=#11c66b>":
                r = " receives <color=#11c66b>";
                break;
            case "</color> de curación.":
                r = "</color> healing.";
                break;
            case " muere.":
                r = " dies.";
                break;
            case " realiza Tirada de Salvación: 1d20 = ":
                r = " makes a Saving Throw: 1d20 = ";
                break;
            case " vs Tirada Dificultad: ":
                r = " vs Difficulty Check: ";
                break;
            case ". Resultado: No se salva.":
                r = ". Result: Fails.";
                break;
            case ". Resultado: Se salva.":
                r = ". Result: Succeeds.";
                break;
            case "Resiste":
                r = "Resists";
                break;
            case "-El Séquito de Cronistas ha registrado el viaje. +20 Valor Crónica.":
                r = "-The Chronicler Retinue has recorded the journey. +20 Chronicle Value.";
                break;
            case "-El Séquito de Nobles ha hecho una donación. Oro: ":
                r = "-The Noble Retinue has made a donation. Gold: ";
                break;
            case "-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza.":
                r = "-The Civilians feel guilty about the presence of the Slaves. -2 Hope.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se ha gastado 200 de oro.":
                r = "-You have performed a ritual in the sanctuary. The Black Breath recedes by 3 and 200 gold has been spent.";
                break;
            case " ha sido purificado de la corrupción.":
                r = " has been purified of corruption.";
                break;
            case "-No hay personajes corruptos para purificar.":
                r = "-There are no corrupt characters to purify.";
                break;
            case "-No tienes suficientes bueyes para realizar el ritual en el santuario.":
                r = "-You do not have enough oxen to perform the ritual in the sanctuary.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se han sacrificado 3 bueyes.":
                r = "-You have performed a ritual in the sanctuary. The Black Breath recedes by 3 and 3 oxen have been sacrificed.";
                break;
            case "-El Séquito de Artistas ha tenido un festán y despilfarrado suministros: ":
                r = "-The Artist Retinue has feasted and squandered supplies: ";
                break;
            case " PV por su Actividad de <b>Descanso</b>.":
                r = " HP for their <b>Rest</b> Activity.";
                break;
            case " Experiencia por su Actividad de <b>Entrenamiento</b>.":
                r = " Experience for their <b>Training</b> Activity.";
                break;
            case " gana ":
                r = " gains ";
                break;
            case " pierde ":
                r = " loses ";
                break;
            case " brinda 10 Experiencia a sus compañeros de menor nivel por su Actividad de <b>Relatos de Batalla</b>.":
                r = " grants 10 Experience to their lower-level companions for their <b>Battles of Tales</b> Activity.";
                break;
            case " consigue ":
                r = " gets ";
                break;
            case " suministros por su Actividad de <b>Caza Nocturna</b>.":
                r = " supplies for their <b>Night Hunt</b> Activity.";
                break;
            case " realiza su actividad <b>Ayudar a los Desamparados</b> y la esperanza aumenta en ":
                r = " performs their activity <b>Help the Hopeless</b> and hope increases by ";
                break;
            case " de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza":
                r = " Gold from the Caravan Merchants, who were coerced into donating to the cause. -1 Hope";
                break;
            case " ha creado un Sí­mbolo de Protección Arcano.":
                r = " has created an Arcane Protection Symbol.";
                break;
            case "-La fatiga ha provocado la muerte de algunos Bueyes.":
                r = "-Fatigue has caused the death of some Oxen.";
                break;
            case " Bueyes":
                r = " Oxen";
                break;
            case "-La fatiga extrema ha provocado la muerte de algunos Bueyes y Civiles.":
                r = "-Extreme fatigue has caused the death of some Oxen and Civilians.";
                break;
            case " Bueyes -":
                r = " Oxen -";
                break;
            case "-El Séquito de Nobles se queja por la falta de descanso. -2 Esperanza":
                r = "-The Noble Retinue complains about the lack of rest. -2 Hope";
                break;
            case "-Tus personajes están fatigados. Afectará su rendimiento en batalla.":
                r = "-Your characters are tired. It will affect their performance in battle.";
                break;
            case "-El sacrificio de Bueyes para obtener Suministros ha provocado preocupación. -2 Esperanza":
                r = "-The sacrifice of Oxen to obtain Supplies has caused concern. -2 Hope";
                break;
            case "-Los Cronistas han registrado la victoria, +50 Valor Crónica, +5 Esperanza.":
                r = "-The Chroniclers have recorded the victory, +50 Chronicle Valor, +5 Hope.";
                break;
            case "-Los Cronistas han registrado la derrota, -50 Valor Crónica. -3 Esperanza.":
                r = "-The Chroniclers have recorded the defeat, -50 Chronicle Valor. -3 Hope.";
                break;
            case "Victoria sin recompensas definidas para este encuentro clásico.":
                r = "Victory with no rewards defined for this classic encounter.";
                break;
            case "Derrota en un encuentro clásico. Los efectos especí­ficos aún no están configurados.":
                r = "Defeat in a classic encounter. Specific effects are not configured yet.";
                break;
            case "sin botón":
                r = "no loot";
                break;
            case " ha sido corrompido.":
                r = " has been corrupted.";
                break;
            case "-Se ha unido el Séquito de Artistas a la caravana. +25 Civiles":
                r = "-The Artist Retinue has joined the caravan. +25 Civilians";
                break;
            case "Séquito de Herreros":
                r = "Blacksmith Retinue";
                break;
            case "Séquito de Curanderos":
                r = "Healer Retinue";
                break;
            case "Séquito de Mercaderes":
                r = "Merchant Retinue";
                break;
            case "Séquito de Artistas":
                r = "Artist Retinue";
                break;
            case "Séquito de Herboristas":
                r = "Herbalist Retinue";
                break;
            case "Séquito de Desertores":
                r = "Deserter Retinue";
                break;
            case "Séquito de Cronistas":
                r = "Chronicler Retinue";
                break;
            case "Séquito de Refugiados":
                r = "Refugee Retinue";
                break;
            case "Séquito de Nobles":
                r = "Noble Retinue";
                break;
            case "Séquito de Clérigos":
                r = "Cleric Retinue";
                break;
            case "Séquito de Esclavos":
                r = "Slave Retinue";
                break;
            case "-Se ha unido el Séquito de Herboristas a la caravana. +10 Civiles":
                r = "-The Herbalist Retinue has joined the caravan. +10 Civilians";
                break;
            case "-Los Desertores se han unido a la Caravana. +15 Civiles -8 Esperanza":
                r = "-The Deserter Retinue has joined the caravan. +15 Civilians -8 Hope";
                break;
            case "-Los Cronistas se han unido a la Caravana. +10 Civiles":
                r = "-The Chronicler Retinue has joined the caravan. +10 Civilians";
                break;
            case "-Los Refugiados se han unido a la Caravana. +35 Civiles  +30 Esperanza":
                r = "-The Refugee Retinue has joined the caravan. +35 Civilians +30 Hope";
                break;
            case "-Los Nobles se han unido a la Caravana. +25 Civiles":
                r = "-The Noble Retinue has joined the caravan. +25 Civilians";
                break;
            case "-Los Clérigos del Sol Purificador se han unido a la Caravana. +20 Civiles +15 Esperanza":
                r = "-The Purifying Sun Cleric Retinue has joined the caravan. +20 Civilians +15 Hope";
                break;
            case "-Los Esclavos se han unido a la Caravana. +30 Civiles":
                r = "-The Slave Retinue has joined the caravan. +30 Civilians";
                break;
            case "-El Séquito de Artistas ha abandonado la caravana. -25 Civiles -15 Esperanza":
                r = "-The Artist Retinue has left the caravan. -25 Civilians -15 Hope";
                break;
            case "-El Séquito de Herboristas ha abandonado la caravana. -10 Civiles":
                r = "-The Herbalist Retinue has left the caravan. -10 Civilians";
                break;
            case "-Los Desertores han abandonado la Caravana. -15 Civiles":
                r = "-The Deserter Retinue has left the caravan. -15 Civilians";
                break;
            case "-Los Cronistas han abandonado la Caravana. -10 Civiles":
                r = "-The Chronicler Retinue has left the caravan. -10 Civilians";
                break;
            case "-Los Refugiados han abandonado la Caravana. -35 Civiles -40 Esperanza":
                r = "-The Refugee Retinue has left the caravan. -35 Civilians -40 Hope";
                break;
            case "-Los Nobles han abandonado la Caravana. -25 Civiles":
                r = "-The Noble Retinue has left the caravan. -25 Civilians";
                break;
            case "-Se ha vendido la crónica del viaje por Oro: ":
                r = "-The chronicle of this journey has been sold for Gold: ";
                break;
            case " ha recibido tratamiento especial y sus heridas han sanado.":
                r = " has received special treatment and their wounds have healed.";
                break;
            case "Un grupo de eruditos unidos que se dedican a registrar los sucesos del viaje de la caravana hacia el puerto. Sus escrituras pueden ser una fuenta de ingresos y moral, pero también puede ser contraproducente en los peores momentos.\n\n":
                r = "A united group of scholars dedicated to recording the caravan's journey to the port. Their writings can bring income and morale, but in bad moments they can also backfire.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +5 de Esperanza por batallas ganadas (-3 Derrotas). ":
                r = "PASSIVE EFFECTS:\n\n-Grant +5 Hope after victories (-3 after defeats). ";
                break;
            case "\n\n-Ya se ha vendido la crónica de este viaje.":
                r = "\n\n-The chronicle of this journey has been sold.";
                break;
            case "\n\n- Crónica: Acumula valor de la siguiente manera:":
                r = "\n\n- Chronicle: Builds value as follows:";
                break;
            case "\n   - Base: 150 Oro":
                r = "\n   - Base: 150 Gold";
                break;
            case "\n   - +1 Oro por cada punto de Esperanza":
                r = "\n   - +1 Gold for each point of Hope";
                break;
            case "\n   - +20 Oro por cada nodo viajado":
                r = "\n   - +20 Gold for each node traveled";
                break;
            case "\n   - +50 Oro por cada batalla ganada / -50 Oro por cada batalla perdida":
                r = "\n   - +50 Gold for each battle won / -50 Gold for each battle lost";
                break;
            case "\n\nSe puede vender en Asentamientos o Puestos Comerciales.":
                r = "\n\nIt can be sold in Settlements or Trading Posts.";
                break;
            case "\n\n\n\n-Valor Crónica: Oro: ":
                r = "\n\n\n\n-Chronicle Value: Gold: ";
                break;
            case "% por Herboristas":
                r = " % for Herbalists";
                break;
            case "Carros de Tratamiento: Mejorar los carros utilizados por el Séquito de Curanderos para tratar heridos significará una mejora en los tratamientos recibidos por los heridos y su tiempo de recuperación. \nCada Tier aumenta en 5% la curación diaria de los personajes que Descansen y reduce el costo de Tratar Heridas. \nAdemás cada tier da un 10% extra a las posibilidades de reducir Enfermedades al Descansar (20% base). \nCuración proporcionada: ":
                r = "Treatment Carts: Improving the carts used by the Healers' Retinue to treat the wounded will mean an improvement in the treatments received by the wounded characters and their recovery time. \nEach Tier increases the daily healing of characters who Rest by 5% and reduces the cost of Treating Wounds. \nIn addition, each tier gives an extra 10% to the chances of reducing Diseases when Resting (20% base). \nHealing provided: ";
                break;
            case " Materiales":
                r = " Materials";
                break;
            case "Tratar Heridas - Coste: <color=#A5B328>":
                r = "Treat Wounds - Cost: <color=#A5B328>";
                break;
            case "Tratar Heridas - Coste: <color=#C40E0E>":
                r = "Treat Wounds - Cost: <color=#C40E0E>";
                break;
            case "Han sido esclavos toda su vida, e incluso en estas circunstancias se comportan como tal. La situación amerita aprovecharse de su condición para obtener ventajas de mano de obra, ¿o quizás llegó el momento de liberarlos?\n\n":
                r = "They have lived as slaves all their lives, and even now they behave that way. The situation invites using that labor for advantage, or perhaps it is time to free them?\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +50 Capacidad de Carga\n\n-Cada descanso juntan 10-15 Materiales.\n\n-Cada Viaje se pierden 2 de Esperanza.\n\n-Al ser liberados, se convierten en Civiles comunes y otorgan +25 Esperanza.":
                r = "PASSIVE EFFECTS:\n\n-Grant +50 Carry Capacity\n\n-Each rest gathers 10-15 Materials.\n\n-Each journey loses 2 Hope.\n\n-When freed, they become regular Civilians and grant +25 Hope.";
                break;
            case "-Los Esclavos han sido liberados y ahora son Civiles comunes. +25 Esperanza":
                r = "-The Slaves have been freed and are now common Civilians. +25 Hope";
                break;
            case "Tamaño Tiendas: ":
                r = "Store Size: ";
                break;
            case "-El Séquito de Mercaderes ha actualizado su oferta.":
                r = "-The Merchant Retinue has updated its offer.";
                break;
            case " es escondido en las sombras tras recibir un ataque crí­tico por su Armadura de Velo.":
                r = " is hidden in the shadows after receiving a critical hit from its Cloak Armor.";
                break;
            case "Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periódicamente parte de su riqueza para asegurarse de que no serán abandonados.\n\n":
                r = "A group of nobles forced to leave the comfort of their lands now travels with the caravan. They complain and offer little help, but they donate part of their wealth to ensure they are not abandoned.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Cada dí­a donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.":
                r = "PASSIVE EFFECTS:\n\n-Each day they donate Gold equal to 1/3 of Hope.\n\n-Traveling with fatigue 4+ loses 2 Hope.";
                break;
            case "Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayorí­a murieron en la onda expansiva en ese momento, pero todaví­a quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n":
                r = "The Clerics of the Purifying Radiant Sun supported the fight against the Lich. Most died in the blast, but some groups still try to reach the port and survive while slowing the Black Breath.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a nivel superior a 16, los Clérigos mueren.":
                r = "PASSIVE EFFECTS:\n\n-Grant 15 Hope when they join the Caravan, -20 Hope if lost.\n\n-20% chance to delay the Black Breath on each journey.\n\n-If Black Breath rises above 16, the Clerics die.";
                break;
            case "<color=red>La plegaria ya fue realizada.</color>":
                r = "<color=red>The prayer has already been made.</color>";
                break;
            case "<color=red>No hay oro suficiente para una donación de 250 Oro.</color>":
                r = "<color=red>There is not enough gold for a donation of 250 Gold.</color>";
                break;
            case "Se hará una donación de 250 Oro.":
                r = "A donation of 250 Gold will be made.";
                break;
            case "Mantenimiento Armas: El Herrero se encargará de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daño en 2. Este efecto Dura 3 dí­as.":
                r = "Weapon Maintenance: The Blacksmith will take care of general maintenance of the characters' weapons. Increasing their Attack by 1 and their damage by 2. This effect lasts 3 days.";
                break;
            case "Mantenimiento Armaduras: El Herrero se encargará de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 dí­as.":
                r = "Armor Maintenance: The Blacksmith will take care of general maintenance of the characters' armor. Increasing their Defense by 1 and their Armor by 2. This effect lasts 3 days.";
                break;
            case "Realizar: 200 Oro":
                r = "Perform: 200 Gold";
                break;
            case "Activo por ":
                r = "Active for ";
                break;
            case " Dí­as":
                r = " Days";
                break;
            case "Armas Civiles: El herrero se dedica a mejorar las armas rudimentarias de los civiles, mejorando las posibilidades de defensa de las Milicias. \nCada Tier aumenta en 10% los Civiles que suman fuerza para la Milicia.":
                r = "Civilian Weapons: The blacksmith improves civilians' rudimentary weapons, strengthening Militia defense. \nEach Tier increases by 10% the Civilians who add strength to the Militia.";
                break;
            case "Estos soldados abandonaron su puesto en el ejército en pos de sobrevivir. Hambrientos y avergonzados, ofrecen protección a la Caravana pidiendo solo un lugar en ella, aunque a una parte de los civiles les desagrade la idea.\n\n":
                r = "These soldiers deserted their posts to survive. Hungry and ashamed, they offer protection in exchange for a place in the Caravan, though some civilians dislike the idea.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Participan en la defensa de la Caravana, reemplazando a los inexpertos Milicianos. \n\n-Otorga 10 Experiencia extra a Personajes que Entrenan. \n\n-Al aceptarlos la Esperanza disminuye en 8.":
                r = "PASSIVE EFFECTS:\n\n-They defend the Caravan, replacing inexperienced Militiamen. \n\n-Grants 10 extra Experience to Characters who Train. \n\n-Accepting them reduces Hope by 8.";
                break;
            case "Varios civiles que estuvieron a la deriva mucho tiempo buscando sobrevivir. Compuesto de mayormente de ancianos, mujeres y niños desnutridos. Consumen menos comida de lo normal y su presencia llena de regocijo a la Caravana porque se hizo lo correcto al recibirlos. Ahora habrá que cuidar de ellos.\n\n":
                r = "These civilians drifted for a long time trying to survive. Mostly elders, women, and malnourished children. They consume less food, and their presence lifts the Caravan because taking them in was the right choice. Now they must be protected.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Consumen la mitad de Suministros que los Civiles habituales. \n\n-Al aceptarlos la Esperanza aumenta en 30. \n\n-Al perderlos la Esperanza disminuye en 40.":
                r = "PASSIVE EFFECTS:\n\n-They consume half as many Supplies as regular Civilians. \n\n-Accepting them increases Hope by 30. \n\n-Losing them decreases Hope by 40.";
                break;
            case "Un grupo de especialistas en recolectar hierbas y crear con ellas bélsamos especiales para vender. \nAdemás, sus hierbas proporcionarán beneficios curativos a la caravana.\nPero quizás no sean demasiado cuidadosos al adentrarse en zonas peligrosas para recolectar hierbas.\n\n":
                r = "A group of specialists who gather herbs and make special balms to sell. \nTheir herbs also provide healing benefits to the Caravan.\nBut they may be careless when entering dangerous areas to gather more herbs.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Hierbas curativas: Mejoran ":
                r = "PASSIVE EFFECTS:\n\n-Healing Herbs: Improve ";
                break;
            case "% la curación pasiva de la Caravana.\n\nEste í­ndice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.":
                r = "% passive Caravan healing.\n\nThis value increases by 3% each time the Caravan visits a Glade.\n\n-They can be careless while gathering herbs. +2% chance of a caravan attack after resting.";
                break;
            case "50 de oro":
                r = "50 gold";
                break;
            case "El séquito de Herreros se encarga del mantenimiento y manufactura de las armas y armaduras de la Caravana. Su carro es especialmente pesado ya que, montado ingeniosamente, carga con todas las necesidades básicas de un herrero":
                r = "The Blacksmith Retinue handles weapon and armor maintenance for the Caravan. Their cart is heavy, but it carries all the essentials of a working forge.";
                break;
            case "Cantidad de Civiles: No.":
                r = "Number of Civilians: No.";
                break;
            case "Civiles representados: ":
                r = "Civilians represented: ";
                break;
            case "Civiles representados: No.":
                r = "Civilians represented: No.";
                break;
            case "150 Oro":
                r = "150 Gold";
                break;
            case "300 Oro":
                r = "300 Gold";
                break;
            case "El Séquito de Curanderos se encarga de atender a los heridos y enfermos de la Caravana. Pese a las circunstancias del viaje mismo, logran mantenerse en funcionamiento y brindan un servicio escencial para la supervivencia de quienes lo necesiten.":
                r = "The Healer Retinue tends to the Caravan's wounded and sick. Despite the hardship of the journey, they stay operational and provide an essential service for those who need it.";
                break;
            case "Tratar Heridas":
                r = "Treat Wounds";
                break;
            case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                r = "This retinue is made up of merchants forced to leave their shops, but not their goods. They trade at reduced prices while keeping a minimal profit.";
                break;
            case "Aumentar el tamaño de las tiendas incrementa la cantidad de objetos ofrecidos.":
                r = "Increasing the size of the shops increases the number of items offered.";
                break;
            case "Varios artistas y miembros de una feria ambulante se han unido a la caravana, si bien son ostentosos y despilfarran recursos, pueden ayudar a la moral de la caravana en determinadas ocasiones festivas.":
                r = "Artists and members of a traveling fair have joined the caravan. Though flashy and wasteful, they can boost morale during festive occasions.";
                break;
            case "Cantidad de Civiles: 25":
                r = "Number of Civilians: 25";
                break;
            case "EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada dí­a hay un 30% de chances de que hagan un festán y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.":
                r = "PASSIVE EFFECTS:\n\n-Joining the Caravan grants 15 Hope.\n\n-Each time Fair is chosen as a Civil Rest Task, gain 10 extra Hope.\n\n-Each day there is a 30% chance they hold a feast and waste 1-4 Supplies.\n\n-If they leave the Caravan, lose 15 Hope.";
                break;
            case "usa ":
                r = "uses ";
                break;
            case " (-1 Lluvia)":
                r = " (-1 Rain)";
                break;
            case " (-2 Niebla)":
                r = " (-2 Fog)";
                break;
            case "<b>Pifia</b>":
                r = "<b>Fumble</b>";
                break;
            case "-Tirada de Ataque: 1d20 = ":
                r = "-Attack Roll: 1d20 = ";
                break;
            case ". Resultado: Pifia.":
                r = ". Result: Fumble.";
                break;
            case ". Resultado: Fallo.":
                r = ". Result: Miss.";
                break;
            case ". Resultado: Roce.":
                r = ". Result: Graze.";
                break;
            case ". Resultado: Golpe.":
                r = ". Result: Hit.";
                break;
            case "Fallo":
                r = "Miss";
                break;
            case "Pifia":
                r = "Fumble";
                break;
            case "Nido Defensivo":
                r = "Defensive Nest";
                break;
            case "Al Acecho":
                r = "Stalking";
                break;
            case "Arma Envenenada":
                r = "Poisoned Weapon";
                break;
            case "Desestabilizado":
                r = "Unstable";
                break;
            case "<b>¿Enfurecido!</b>":
                r = "<b>Enraged!</b>";
                break;
            case "Sangre Devorada":
                r = "Leeched Blood";
                break;
            case "Eufórico":
                r = "Euphoric";
                break;
            case "Sangre Contaminada":
                r = "Contaminated Blood";
                break;
            case "Aturdido por Chirrido":
                r = "Stunned by Screech";
                break;
            case "Atemorizado":
                r = "Frightened";
                break;
            case "Enredado":
                r = "Entangled";
                break;
            case "Enredadera Ardiente":
                r = "Fiery Vine";
                break;
            case "En plano material":
                r = "In Material Plane";
                break;
            case "Perdición":
                r = "Damnation";
                break;
            case "Encarnado":
                r = "Incarnated";
                break;
            case " está encarnado y no puede actuar este turno.":
                r = " is incarnated and cannot act this turn.";
                break;
            case "Aullido de la Manada":
                r = "Pack Howl";
                break;
            case "Furia":
                r = "Fury";
                break;
            case "Sorprendido":
                r = "Surprised";
                break;
            case "Acalorado":
                r = "Heated";
                break;
            case "Mojado":
                r = "Wet";
                break;
            case "Frí­o":
                r = "Cold";
                break;
            case "Aliento Negro: Débil":
                r = "Black Breath: Weak";
                break;
            case "Aliento Negro: Presente":
                r = "Black Breath: Present";
                break;
            case "Aliento Negro: Fuerte":
                r = "Black Breath: Strong";
                break;
            case "Aliento Negro: Empoderante":
                r = "Black Breath: Empowering";
                break;
            case "Oscuridad":
                r = "Darkness";
                break;
            case "Fatigado":
                r = "Tired";
                break;
            case "Bendecido por Plegaria":
                r = "Blessed by Prayer";
                break;
            case "Herido":
                r = "Wounded";
                break;
            case "Enfermo":
                r = "Sick";
                break;
            case "Baja Moral":
                r = "Low Morale";
                break;
            case "Alta Moral":
                r = "High Morale";
                break;
            case "Armadura Cuidada":
                r = "Well-Cared Armor";
                break;
            case "Fresco":
                r = "Fresh";
                break;
            case "Flechas Preparadas":
                r = "Prepared Arrows";
                break;
            case "Fatigado por Explorar":
                r = "Exploration Fatigue";
                break;
            case "Arma Afilada":
                r = "Sharp Weapon";
                break;
            case "Invulnerable":
                r = "Invulnerable";
                break;
            case "Desplazado":
                r = "Displaced";
                break;
            case "Condenado":
                r = "Condemned";
                break;
            case "Escudado por Fe":
                r = "Shielded by Faith";
                break;
            case "Descansado":
                r = "Rested";
                break;
            case "Etereo":
                r = "Ethereal";
                break;
            case "Escondido Por Humo":
                r = "Hidden by Smoke";
                break;
            case "Motivado":
                r = "Motivated";
                break;
            case "Euforia":
                r = "Euphoria";
                break;
            case "Desmotivado":
                r = "Demotivated";
                break;
            case "Desesperanzado":
                r = "Hopeless";
                break;
            case "Cobertura de Barricada":
                r = "Barricade Coverage";
                break;
            case "Hombro Con Hombro":
                r = "Shoulder to Shoulder";
                break;
            case "Masacre":
                r = "Massacre";
                break;
            case "Aterrorizado":
                r = "Terrified";
                break;
            case "Consumevida":
                r = "Life Consumption";
                break;
            case "Incapacitado":
                r = "Incapacitated";
                break;
            case "Distraí­do":
                r = "Distracted";
                break;
            case "Implacable":
                r = "Unrelenting";
                break;
            case "Determinación":
                r = "Determination";
                break;
            case "Grito Motivador":
                r = "War Cry";
                break;
            case "Grito Desmotivador":
                r = "Demoralized";
                break;
            case "Postura Defensiva":
                r = "Defensive Stance";
                break;
            case "Amedrentado":
                r = "Intimidated";
                break;
            case "Acumulando":
                r = "Gathering";
                break;
            case " falló la Tirada de Concentración y ya no acumula energí­a.":
                r = " failed the Concentration Roll and is no longer Gathering energy.";
                break;
            case "Energizado":
                r = "Energized";
                break;
            case "Acumulacion Inestable":
                r = "Unstable Gathering";
                break;
            case "Escudo Energético":
                r = "Energy Shield";
                break;
            case "Energí­a Absorbida":
                r = "Absorbed Energy";
                break;
            case "Residuo Energético":
                r = "Energy Residue";
                break;
            case "Reconocimiento":
                r = "Recognition";
                break;
            case "Presa Completada":
                r = "Completed Prey";
                break;
            case "Vista Lejana I":
                r = "Long Sight I";
                break;
            case "Vista Lejana II":
                r = "Long Sight II";
                break;
            case "Vista Lejana III":
                r = "Long Sight III";
                break;
            case "Vista Lejana IVa":
                r = "Long Sight IVa";
                break;
            case "Vista Lejana IVb":
                r = "Long Sight IVb";
                break;
            case "Flechas de Fuego":
                r = "Fire Arrows";
                break;
            case "Ralentizado":
                r = "Slowed";
                break;
            case "Acechando":
                r = "Stalking";
                break;
            case "Marcando Presa":
                r = "Marking Prey";
                break;
            case "Afligida I":
                r = "Afflicted I";
                break;
            case "Afligida II":
                r = "Afflicted II";
                break;
            case "Afligida III":
                r = "Afflicted III";
                break;
            case "Afligida IV":
                r = "Afflicted IV";
                break;
            case "Fervor":
                r = "Fervor";
                break;
            case "Aura Sagrada":
                r = "Sacred Aura";
                break;
            case "Ciego":
                r = "Blind";
                break;
            case " de ":
                r = " of ";
                break;
            case " remueve ":
                r = " removes ";
                break;
            case "Bálsamo de Claridad":
                r = "Balm of Clarity";
                break;
            case "Bálsamo Energizante":
                r = "Energizing Balm";
                break;
            case "Bálsamo Fortalecedor":
                r = "Fortifying Balm";
                break;
            case "Elixir de Resistencia al Frí­o":
                r = "Elixir of Cold Resistance";
                break;
            case "Elixir de Resistencia al Fuego":
                r = "Elixir of Fire Resistance";
                break;
            case "Elixir de Resistencia al Fuego.":
                r = "Elixir of Fire Resistance";
                break;
            case "Elixir de Resistencia al Rayo":
                r = "Elixir of Lightning Resistance";
                break;
            case "Elixir de Resistencia al Ácido":
                r = "Elixir of Acid Resistance";
                break;
            case "Protección Arcana":
                r = "Arcane Protection";
                break;
            case "IA Lenguetazo":
                r = "AI Tongue Lash";
                break;
            case "Saboreado":
                r = "Savored";
                break;
            case "recibirá más daño del Zarkilever":
                r = "will take more damage from the Zarkilever";
                break;
            case "Armadura Rota":
                r = "Broken Armor";
                break;
            case "Potenciado por Masa Contaminada":
                r = "Empowered by Contaminated Mass";
                break;
            case "Herida":
                r = "Wound";
                break;
            case "Ardiendo: causa daño cada turno, se apaga con AP disponibles.":
                r = "Burning: deals damage each turn, extinguished with available AP.";
                break;
            case "Aturdido: no puede actuar.":
                r = "Stunned: cannot act.";
                break;
            case "Ácido: cada acumulación reduce en 1 la armadura.":
                r = "Acid: each stack reduces armor by 1.";
                break;
            case "Congelado: reduce PA disponibles y aumenta armadura.":
                r = "Frozen: reduces available AP and increases armor.";
                break;
            case "Resistencias Reducidas: reduce todas las resistencias 1 por acumulación.":
                r = "Reduces all resistances by 1 per stack.";
                break;
            case "Armadura Rota: reduce la armadura en 1 por acumulación.":
                r = "Broken Armor: reduces armor by 1 per stack.";
                break;
            case "Sangrado: cada acumulación resta 1 HP máxima por turno y previene 2 de curación.":
                r = "Bleeding: each stack reduces max HP by 1 per turn and prevents 2 healing.";
                break;
            case "Veneno: provoca daño por turno, se debe hacer una tirada de salvación de Fortaleza cada turno para curarse, si falla se incrementa en 1.":
                r = "Poison: deals damage each turn, a Fortitude saving throw must be made each turn to heal, if it fails it increases by 1.";
                break;
            case "Regeneración: recupera vida cada turno.":
                r = "Regeneration: recovers health each turn.";
                break;
            case "Regeneración Armadura: recupera Armadura perdida cada turno.":
                r = "Armor Regeneration: recovers lost armor each turn.";
                break;
            case "Evasión: cada stack aumenta 1 la Defensa, se elimina al recibir daño.":
                r = "Evasion: each stack increases Defense by 1, removed upon taking damage.";
                break;
            case "Flechas: Cantidad de flechas disponibles.":
                r = "Arrows: Number of available arrows.";
                break;
            case " Bonus daño elemental Acido.":
                r = " Bonus Acid elemental damage.";
                break;
            case " Bonus daño elemental Arcano.":
                r = " Bonus Arcane elemental damage.";
                break;
            case " Bonus daño elemental Fuego.":
                r = " Bonus Fire elemental damage.";
                break;
            case " Bonus daño elemental Hielo.":
                r = " Bonus Cold elemental damage.";
                break;
            case " Bonus daño elemental Necro.":
                r = " Bonus Necrotic elemental damage.";
                break;
            case " Bonus daño elemental Divino.":
                r = " Bonus Divine elemental damage.";
                break;
            case " Bonus daño elemental Rayo.":
                r = " Bonus Lightning elemental damage.";
                break;
            case "Fervor: Cantidad de Fervor que tiene la purificadora.":
                r = "Fervor: Amount of Fervor the purifier has.";
                break;
            case "Barrera: previene X cantidad de daño.":
                r = "Barrier: prevents X amount of damage.";
                break;
            case "Residuo de Tejido: se obtiene al recibir curación de origen mágico. Previene X puntos de curación.":
                r = "Tissue Residue: obtained when receiving healing from magical sources. Prevents X points of healing.";
                break;
            case "Escondido I: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto se remueve al atacar o recibir daño.":
                r = "Hidden I: This unit is hidden and enemies cannot attack it. The effect is removed upon attacking or taking damage.";
                break;
            case "Escondido II: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto no se remueve al recibir daño.":
                r = "Hidden II: This unit is hidden and enemies cannot attack it. The effect is not removed upon taking damage.";
                break;
            case "Energí­a: Nivel de Energí­a Acumulada por el Canalizador.":
                r = "Energy Level accumulated by the Channeler.";
                break;
            case "Corrupto: Recibe daño adicional de enemigos Corrompidos que además se curan al dañarlo. Si lo deja fuera de combate un enemigo corrompido, muere.":
                r = "Corrupted: Takes additional damage from Corrupted enemies who also heal when damaging it. If a Corrupted enemy takes it out of combat, it dies.";
                break;
            case "HP Máximo: ":
                r = "Max HP: ";
                break;
            case "PA Máximo: ":
                r = "Max AP: ";
                break;
            case "PM Máximo: ":
                r = "Max PM: ";
                break;
            case "Resistencia Fuego: ":
                r = "Fire Resistance: ";
                break;
            case "Resistencia Hielo: ":
                r = "Cold Resistance: ";
                break;
            case "Resistencia Rayo: ":
                r = "Lightning Resistance: ";
                break;
            case "Resistencia Ácido: ":
                r = "Acid Resistance: ";
                break;
            case "Resistencia Arcano: ":
                r = "Arcane Resistance: ";
                break;
            case "Resistencia Necrótica: ":
                r = "Necrotic Resistance: ";
                break;
            case "Resistencia Divina: ":
                r = "Divine Resistance: ";
                break;
            case "Barrera inicial: ":
                r = "Starting Barrier: ";
                break;
            case "Evasion inicial: ":
                r = "Starting Evasion: ";
                break;
            case "Penetracion armadura: ":
                r = "Armor Penetration: ";
                break;
            case "Reduccion dano recibido: ":
                r = "Damage reduction: ";
                break;
            case "Reduccion dano critico recibido: ":
                r = "Critical damage reduction: ";
                break;
            case "Resistencia estados: ":
                r = "Status resistance: ";
                break;
            case "Espinas dano plano: ":
                r = "Flat thorns damage: ";
                break;
            case "Espinas dano %: ":
                r = "Thorns damage %: ";
                break;
            case "Daño: ":
                r = "Damage: ";
                break;
            case "Crí­tico Dado: ":
                r = "Critical Hit: ";
                break;
            case "Daño Crí­tico: ":
                r = "Critical Damage: ";
                break;
            case "TS Reflejos: ":
                r = "Reflex Save: ";
                break;
            case "TS Fortaleza: ":
                r = "Fortitude Save: ";
                break;
            case "TS Mental: ":
                r = "Mental Save: ";
                break;
            case "Bonus daño Ácido: ":
                r = "Bonus Acid Damage: ";
                break;
            case "Bonus daño arcano: ":
                r = "Bonus Arcane Damage: ";
                break;
            case "Bonus daño fuego: ":
                r = "Bonus Fire Damage: ";
                break;
            case "Bonus daño hielo: ":
                r = "Bonus Cold Damage: ";
                break;
            case "Bonus daño necro: ":
                r = "Bonus Necrotic Damage: ";
                break;
            case "Bonus daño rayo: ":
                r = "Bonus Lightning Damage: ";
                break;
            case "Duración: ":
                r = "Duration: ";
                break;
            case "Duración: Permanente\n":
                r = "Duration: Permanent\n";
                break;
            case " rondas\n":
                r = " rounds\n";
                break;
            case "Valentí­a Global Alta":
                r = "Global Valour High";
                break;
            case "Valentia Global Alta":
                r = "Global Valour High";
                break;
            case "Valentí­a Global Muy Alta":
                r = "Global Valour Very High";
                break;
            case "Valentia Global Muy Alta":
                r = "Global Valour Very High";
                break;
            case "Dudando":
                r = "Doubting";
                break;
            case "La moral colectiva impulsa al grupo. +1 PA máximo esta ronda.":
                r = "Collective morale drives the party. +1 Max AP this round.";
                break;
            case "La moral colectiva desborda. +15% daño y +1 PA máximo esta ronda.":
                r = "Collective morale surges. +15% damage and +1 Max AP this round.";
                break;
            case "La moral flaquea por la presión del combate.":
                r = "Morale falters under battle pressure.";
                break;
            case "Ataque: ":
                r = "Attack: ";
                break;
            case "Defensa: determina capacidad para evadir ataques.":
                r = "Defense: determines ability to evade attacks.";
                break;
            case "Armadura: reduce el daño fí­sico recibido.":
                r = "Armor: reduces physical damage taken.";
                break;
            case "Reflejos: resistencia a determinados efectos de ataques.":
                r = "Reflexes: resistance to certain attack effects.";
                break;
            case "Fortaleza: resistencia a efectos fí­sicos.":
                r = "Fortitude: resistance to physical effects.";
                break;
            case "Mental: resistencia a efectos mentales.":
                r = "Mental: resistance to mental effects.";
                break;
            case "Valentí­a: moral general en combate.":
                r = "Valour: general moral in combat.";
                break;
            case "Resistencia al Fuego: Cantidad de daño que previene.":
                r = "Fire Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Frí­o: Cantidad de daño que previene.":
                r = "Cold Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Rayo: Cantidad de daño que previene.":
                r = "Lightning Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Ácido: Cantidad de daño que previene.":
                r = "Acid Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia Arcana: Cantidad de daño que previene.":
                r = "Arcane Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia Necrótica: Cantidad de daño que previene.":
                r = "Necrotic Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia Divina: Cantidad de daño que previene.":
                r = "Divine Resistance: Amount of damage it prevents.";
                break;
            case "Residuo Energético: Otorga daño arcano y hiere levemente.":
                r = "Energetic Residue: Grants arcane damage and lightly wounds.";
                break;
            case "Zona bajo Vigilancia del Explorador.":
                r = "Area under Explorer's Watch.";
                break;
            case "Añade daño fuego al Explorador si está adyacente.":
                r = "Adds fire damage to the Explorer if adjacent.";
                break;
            case "Abrojos: Inflige daño y puede desangrar.":
                r = "Caltrops: Inflicts damage and can bleed.";
                break;
            case "Eco Divino: Cura a aliados y daña a enemigos.":
                r = "Divine Echo: Heals allies and damages enemies.";
                break;
            case "Humo: Esconde a los personajes dentro.":
                r = "Smoke: Hides characters within.";
                break;
            case "Escudo de Fe: Protege a los aliados dentro.":
                r = "Shield of Faith: Protects allies within.";
                break;
            case "Masa Contaminada: Hace daño Ácido. Potencia enemigos corruptos.":
                r = "Contaminated Mass: Deals acid damage. Empowers corrupted enemies.";
                break;
            case "Pinchos: Daña a enemigos que los pisen.":
                r = "Spikes: Damages enemies that step on them.";
                break;
            case "Barricada: Obstáculo para enemigos. Hiere al ser atacada.":
                r = "Barricade: Obstacle for enemies. Damage when attacked.";
                break;
            case "Puesto de Tiro: Aumenta ataque y defensa a aliados dentro.":
                r = "Shooting Post: Increases attack and defense for allies within.";
                break;
            case "Pilar de Luz: Obstáculo que daña a enemigos al ser atacado.":
                r = "Light Pillar: Obstacle that damages enemies when attacked.";
                break;
            case "Fin del Tutorial":
                r = "End of Tutorial";
                break;
            case "Nueva Partida":
                r = "New Game";
                break;
            case "Opciones":
                r = "Options";
                break;
            case "Debes reiniciar para que tenga efecto.":
                r = "You must restart for it to take effect.";
                break;
            case "<i>Los Caballeros siempre andan equipados con un mandoble muy pesado y poderoso. Junto con su armadura pesada, hacen el núcleo del equipo de estos valientes guerreros.</i><b>\n\nOtorga: Corte Vertical</b>":
                r = "<i>The Knights are always equipped with a very heavy and powerful greatsword. Along with their heavy armor, they form the core of the equipment of these brave warriors.</i><b>\n\nGrants: Vertical Slash</b>";
                break;
            case "Mandoble":
                r = "Greatsword";
                break;
            case "Armadura de Cuero Reforzado":
                r = "Reinforced Leather Armor";
                break;
            case "Armadura de Cuero Reforzado +1":
                r = "Reinforced Leather Armor +1";
                break;
            case "Armadura de Cuero Reforzado +2":
                r = "Reinforced Leather Armor +2";
                break;
            case "Armadura de Cuero Reforzado +3":
                r = "Reinforced Leather Armor +3";
                break;
            case "Armadura de Cuero Reforzado de Ligereza +1":
                r = "Reinforced Light Leather Armor +1";
                break;
            case "Armadura de Cuero Reforzado de Protección Elemental +1":
                r = "Reinforced Leather Armor of Elemental Protection +1";
                break;
            case "Precio: ":
                r = "Price: ";
                break;
            case "Efectos del item:":
                r = "Item effects:";
                break;
            case "Rareza: ":
                r = "Rarity: ";
                break;
            case "Tipo de item: ":
                r = "Item type: ";
                break;
            case "Accesorio":
                r = "Accessory";
                break;
            case "Consumible":
                r = "Consumable";
                break;
            case "Baculo":
                r = "Staff";
                break;
            case "Vestidura":
                r = "Vestment";
                break;
            case "Común":
                r = "Common";
                break;
            case "Infrecuente":
                r = "Uncommon";
                break;
            case "Raro":
                r = "Rare";
                break;
            case "Épico":
                r = "Epic";
                break;
            case "Legendario":
                r = "Legendary";
                break;
            case "Artefacto":
                r = "Artifact";
                break;
            case "Desconocida":
                r = "Unknown";
                break;
            case "Agrega habilidad: ":
                r = "Adds ability: ";
                break;
            case "Armadura de Cuero Reforzado de Velo +2":
                r = "Reinforced Leather Armor of Veil +2";
                break;
            case "Espada Corta":
                r = "Short Sword";
                break;
            case "Espada Corta +1":
                r = "Short Sword +1";
                break;
            case "Espada Corta +2":
                r = "Short Sword +2";
                break;
            case "Espada Corta +3":
                r = "Short Sword +3";
                break;
            case "Espada Corta Arcana +1":
                r = "Arcane Short Sword +1";
                break;
            case "Espada Corta Filonegro +1":
                r = "Blackthorn Short Sword +1";
                break;
            case "Espada Corta Consumevida":
                r = "Lifedrinker Short Sword";
                break;
            case "Coraza":
                r = "Heavy Armor";
                break;
            case "Coraza +1":
                r = "Heavy Armor +1";
                break;
            case "Coraza +2":
                r = "Heavy Armor +2";
                break;
            case "Coraza +3":
                r = "Heavy Armor +3";
                break;
            case "Coraza de Llamas +1":
                r = "Flame Armor +1";
                break;
            case "Coraza Liviana":
                r = "Light Armor";
                break;
            case "Coraza de Fuerza de Gigante +2":
                r = "Giant Strength Armor +2";
                break;
            case "Mandoble +1":
                r = "Greatsword +1";
                break;
            case "Mandoble +2":
                r = "Greatsword +2";
                break;
            case "Mandoble +3":
                r = "Greatsword +3";
                break;
            case "Mandoble Sagrado +1":
                r = "Holy Greatsword +1";
                break;
            case "Mandoble Congelado  +2":
                r = "Frozen Greatsword +2";
                break;
            case "Armadura de Cuero":
                r = "Leather Armor";
                break;
            case "Armadura de Cuero +1":
                r = "Leather Armor +1";
                break;
            case "Armadura de Cuero +2":
                r = "Leather Armor +2";
                break;
            case "Armadura de Cuero +3":
                r = "Leather Armor +3";
                break;
            case "Armadura de Cuero de Fortaleza +1":
                r = "Fortified Leather Armor +1";
                break;
            case "Armadura de Cuero necrótico +1":
                r = "Necrotic Leather Armor +1";
                break;
            case "Armadura de Cuero Borrosa +2":
                r = "Blurred Leather Armor +2";
                break;
            case "Arco Largo":
                r = "Longbow";
                break;
            case "Arco Largo +1":
                r = "Longbow +1";
                break;
            case "Arco Largo +2":
                r = "Longbow +2";
                break;
            case "Arco Largo +3":
                r = "Longbow +3";
                break;
            case "Arco Largo Ácido +1":
                r = "Acid Longbow +1";
                break;
            case "Arco Largo Potente +1":
                r = "Powerful Longbow +1";
                break;
            case "Arco Largo Ralentizante +2":
                r = "Slowing Longbow +2";
                break;
            case "Báculo Purificador":
                r = "Purifying Staff";
                break;
            case "Báculo Purificador +1":
                r = "Purifying Staff +1";
                break;
            case "Báculo Purificador +2":
                r = "Purifying Staff +2";
                break;
            case "Báculo Purificador +3":
                r = "Purifying Staff +3";
                break;
            case "Poción de Curación Menor":
                r = "Lesser Healing Potion";
                break;
            case "Poción de Curación Mayor":
                r = "Greater Healing Potion";
                break;
            case "Poción de Curación":
                r = "Healing Potion";
                break;
            case "<Color=#e6b50f>\nPrecio: ":
                r = "<Color=#e6b50f>\nPrice: ";
                break;
            case "<Color=#e60f0f>\nPrecio: ":
                r = "<Color=#e60f0f>\nPrice: ";
                break;
            case "\n\n- Has encontrado un objeto de recompensa: ":
                r = "\n\n- You have found a reward item: ";
                break;
            case "\n\n-Los enemigos han eliminado al ":
                r = "\n\n-The enemies have eliminated the ";
                break;
            case " luego de la Batalla.":
                r = " after the Battle.";
                break;
            case "Se han obtenido ":
                r = "You have won ";
                break;
            case " Oro, ":
                r = " Gold, ";
                break;
            case " Materiales y +":
                r = " Materials and +";
                break;
            case " Esperanza.":
                r = " Hope.";
                break;
            case "+2 TS Mental por todo el combate.":
                r = "+2 Mental Save for the entire combat.";
                break;
            case "+2 TS Reflejos por todo el combate.":
                r = "+2 Reflex Save for the entire combat.";
                break;
            case "+2 TS Fortaleza por todo el combate.":
                r = "+2 Fortitude Save for the entire combat.";
                break;
            case "Panacea":
                r = "Panacea";
                break;
            case "Sí­mbolo de Protección Arcano":
                r = "Arcane Protection Symbol";
                break;
            case "Otorga 3 de Resistencia contra todos los elementos. Dura 4 turnos.":
                r = "Grants 3 Resistance against all elements. Lasts 4 turns.";
                break;
            case "Restaura 20 + 2d8 puntos de vida.":
                r = "Restores 20 + 2d8 hit points.";
                break;
            case "Restaura 12 + 1d8 puntos de vida.":
                r = "Restores 12 + 1d8 hit points.";
                break;
            case "Restaura 6 + 1d6 puntos de vida.":
                r = "Restores 6 + 1d6 hit points.";
                break;
            case "Aumenta la resistencia al frí­o en 5 por el combate.":
                r = "Increases cold resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al fuego en 5 por el combate.":
                r = "Increases fire resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al rayo en 5 por el combate.":
                r = "Increases lightning resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al Ácido en 5 por el combate.":
                r = "Increases acid resistance by 5 for the combat.";
                break;
            case "Remueve todos los debuffs de la unidad.":
                r = "Removes all debuffs from the unit.";
                break;
            case "Ataque de Espada":
                r = "Sword Attack";
                break;
            case "Tiro de Arco":
                r = "Bow Shot";
                break;
            case "Golpe Manifestacion":
                r = "Manifestation Strike";
                break;
            case "Descarga Arcana":
                r = "Arcane Blast";
                break;
            case "Ataque de Lanza":
                r = "Spear Attack";
                break;
            case "Tiro de Ballesta":
                r = "Crossbow Shot";
                break;
            case "Espada Corta Ladrón":
                r = "Thief's Short Sword";
                break;
            case "Envenenar Arma":
                r = "Poison Weapon";
                break;
            case "Mordida Perro Adiestrado":
                r = "Trained Dog Bite";
                break;
            case "Empujón Rufián":
                r = "Shove";
                break;
            case "Mazo Rufián":
                r = "Mace Strike";
                break;
            case "Arrojar Corrosión":
                r = "Throw Corrosion";
                break;
            case "Proliferar Corrupción":
                r = "Proliferate Corruption";
                break;
            case "Devorar Sangre":
                r = "Consume Blood";
                break;
            case "Garra de Devorador":
                r = "Devourer's Claw";
                break;
            case "Ataque de Garra":
                r = "Claw Attack";
                break;
            case "Chirrido de Vagranilo":
                r = "Vagranilo's Squeal";
                break;
            case "Mordida Vagranilo":
                r = "Vagranilo's Bite";
                break;
            case "Chirrido Mayor":
                r = "Unbearable Squeal";
                break;
            case "Mordida Vagranilo Mayor":
                r = "Adult Vagranilo's Bite";
                break;
            case "Enredadera Espinoza":
                r = "Thorned Vine";
                break;
            case "Ataque Raiz":
                r = "Root Attack";
                break;
            case "Ataque Vaina":
                r = "Vine Attack";
                break;
            case "Crecimiento Espinoso":
                r = "Thorn-garden Growth";
                break;
            case "Lamento del Bosque":
                r = "Forest's Lament";
                break;
            case "Caricia del Bosque":
                r = "Forest's Caress";
                break;
            case "Enredar":
                r = "Entangle";
                break;
            case "Ráfaga de Espinas":
                r = "Thorn Burst";
                break;
            case "Golpe de Espectro":
                r = "Spectral Strike";
                break;
            case "Golpe de Fuego Fatuo":
                r = "Will-o'-the-Wisp Strike";
                break;
            case "Garra Espectral":
                r = "Spectral Claw";
                break;
            case "Mordisco Ardiente":
                r = "Fiery Bite";
                break;
            case "Reacción: Al morir, enfurecerá a otros Lobos Espectrales.":
                r = "Reaction: Upon death, it will enrage other Spectral Wolves.";
                break;
            case "Golpe Enredado":
                r = "Entangled Strike";
                break;
            case "Lobo Espectral":
                r = "Spectral Wolf";
                break;
            case "<i>El Lobo Espectral es un enemigo feroz que se mueve y ataca rápidamente, mientras su destreza animal le brinda una buena defensa.</i>\n\n<color=#199F10>-Posee un mordisco imbuido en fuego que además de dañar, puede hacer arder a sus enemigos.</color>\n<color=#EE0000>-Estadí­sticas débiles.</color>":
                r = "<i>The Spectral Wolf is a fierce enemy that moves and attacks quickly, while its animal dexterity provides good defense.</i>\n\n<color=#199F10>-Has a fire-infused bite that not only damages but can also set enemies ablaze.</color>\n<color=#EE0000>-Weak statistics.</color>";
                break;
            case "Lobo Alfa Espectral":
                r = "Alpha Spectral Wolf";
                break;
            case "<i>El Lobo Alfa Espectral es el lí­der de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>":
                r = "<i>The Alpha Spectral Wolf is the leader of the pack, possessing a stronger and more resilient build than the other wolves, though it is slightly less agile.</i>\n\n<color=#199F10>-Has the ability to howl to motivate other wolves.</color>\n<color=#EE0000>-If left alone, it will be unable to motivate anyone.</color>";
                break;
            case "Driada Quemada":
                r = "Burnt Dryad";
                break;
            case "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raí­ces igní­fugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Once servants and caretakers of the forest, they are now manifestations of vengeance and hatred against any invader of the Burning Forest.</i>\n\n<color=#199F10>-Can entangle with fire-resistant roots.\n-Ranged attack.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Espectro del Bosque":
                r = "Forest Specter";
                break;
            case "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques fí­sicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad fí­sica momentáneamente al atacar.</color>":
                r = "<i>The Forest Specter is a restless soul trapped among the ashes of a scorched forest, its rage fueled by the destruction it could not prevent. Wandering and vengeful, it attacks those who dare to cross its charred land.</i>\n\n<color=#199F10>-Immune to physical attacks.\n-Can curse with Perdition.</color>\n<color=#EE0000>-Loses part of its physical immunity momentarily when attacking.</color>";
                break;
            case "Fuego Fatuo":
                r = "Will-o'-the-Wisp";
                break;
            case "<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guí­a a los incautos hacia la perdición, vengando la memoria del bosque caí­do.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>":
                r = "<i>An ethereal echo of the flames that consumed it, dancing among the ashes as a reminder of the disaster. Though it seems harmless, it leads the unwary to their doom, avenging the memory of the fallen forest.</i>\n\n<color=#199F10>-Resistant to physical attacks.\n-Can incarnate into its enemies.</color>\n<color=#EE0000>-Has low health.</color>";
                break;
            case "Treant Espectral":
                r = "Spectral Treant";
                break;
            case "<i>Con su madera marcada y deformada por el fuego, estos antes pastores de árboles ahora deambulan trayendo muerte a los invasores de su hogar.</i>\n\n<color=#199F10>-Buena armadura que se regenera.\n-Puede enredar al golpear a sus enemigos.</color>\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>With its wood marked and deformed by fire, these once shepherds of trees now wander, bringing death to the invaders of their home.</i>\n\n<color=#199F10>-Good armor that regenerates.\n-Can entangle when hitting its enemies.</color>\n<color=#EE0000>-Weak to fire.</color>";
                break;
            case "Manifestación Arcana":
                r = "Arcane Manifestation";
                break;
            case "<i>Constituido por pura energí­a arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.</color>":
                r = "<i>Composed of pure arcane energy, this ethereal entity defends the Channeler that shaped it.</i>\n\n<color=#199F10>-Resistant to physical attacks.</color>";
                break;
            case "Vagranilo":
                r = "Vagranile";
                break;
            case "<i>Un ser volador cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Evasivo.\n-Puede aturdir.\n-Puede atacar a enemigos escondidos.</color>\n<color=#EE0000>-Débil al daño Divino.</color>":
                r = "<i>A quasi-human flying being native to the depths, it has no vision but compensates with exceptional hearing.</i>\n\n<color=#199F10>-Evasive.\n-Can stun.\n-Can attack hidden enemies.</color>\n<color=#EE0000>-Weak to Divine damage.</color>";
                break;
            case "Vagranilo Mayor":
                r = "Elder Vagranile";
                break;
            case "<i>Un ser terrible cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Chirrido Ensordecedor.\n-Puede atacar a enemigos escondidos.\n-Se cura al morder victimas con Sangre Contaminada.</color>\n<color=#EE0000>-Débil al daño Divino.</color>":
                r = "<i>A terrible quasi-human being from the depths, it has no vision but compensates with exceptional hearing.</i>\n\n<color=#199F10>-Deafening Squeal.\n-Can attack hidden enemies.\n-Heals by biting victims with Contaminated Blood.</color>\n<color=#EE0000>-Weak to Divine damage.</color>";
                break;
            case "Ladrón":
                r = "Thief";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crí­tico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Good critical hit capability.\n-Backstabs while hidden.\n-Can poison his weapon.</color>\n<color=#EE0000>-Quite weak.</color>";
                break;
            case "Rufián con Ballesta":
                r = "Crossbow Grunt";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Puede empujar.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Resistant.\n-Can push.</color>";
                break;
            case "Rufián con Mazo":
                r = "Mace Grunt";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Se enfurece.</color>\n<color=#EE0000>-Lento para actuar.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Resistant.\n-Devastating blows.\n-Enrages.</color>\n<color=#EE0000>-Slow to act.</color>";
                break;
            case "Perro Adiestrado":
                r = "Tamed Dog";
                break;
            case "<i>Un perro adiestrado para la batalla, fiel a su amo y feroz con sus enemigos.</i>\n\n<color=#199F10>-Puede Inmovilizar al morder.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>A dog trained for battle, loyal to its master and fierce with its enemies.</i>\n\n<color=#199F10>-Can immobilize when biting.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Devorador Corrompido":
                r = "Corrupted Devourer";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Puede debilitar.\n-Absorbe vida de Personajes Corruptos.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Once an inhabitant of the lands, now corrupted by the Black Breath, deformed and hungry.</i>\n\n<color=#A020F0>-Corrupted.</color>\n<color=#199F10>-Can weaken.\n-Absorbs life from Corrupted Characters.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Guerrero Corrompido":
                r = "Corrupted Warrior";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Once an inhabitant of the lands, now corrupted by the Black Breath, deformed and hungry.</i>\n\n<color=#A020F0>-Corrupted.</color>\n<color=#199F10>-Strong.\n-Strikes in area.</color>\n<color=#EE0000>-Has only one type of attack.</color>";
                break;
            case "Alimaña Corrompida":
                r = "Corrupted Vermin";
                break;
            case "<i>No se logra discernir facilmente que animal fue originalmente, pero ahora es una criatura corrompida y muy nociva.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Largo alcance.\n-Crea Masa Contaminada.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>It is not easy to discern what animal it was originally, but now it is a corrupted and very harmful creature.</i>\n\n<color=#A020F0>-Corrupted.</color>\n<color=#199F10>-Long range.\n-Creates Contaminated Mass.</color>\n<color=#EE0000>-Limited movement.</color>";
                break;
            case "Dar Feedback":
                r = "Give Feedback";
                break;
            case "Luchar":
                r = "Fight";
                break;
            case "-Las Almas Danzantes de animales inocentes guian a la caravana. +5 Esperanza, 0% chances de emboscada.":
                r = "-The Dancing Souls of innocent animals guide the caravan. +5 Hope, 0% chance of ambush.";
                break;
            case "Almas Danzantes: +5 Esperanza, -100% chances de Emboscada.":
                r = "Dancing Souls: +5 Hope, -100% chance of Ambush.";
                break;
            case "-Las Almas Danzantes guí­an a la caravana. +5 Esperanza":
                r = "-The Dancing Souls guide the caravan. +5 Hope";
                break;
            case "-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza":
                r = "-The Northern Lights amaze the entire caravan. +10 Hope";
                break;
            case "Aurora Boreal: +10 Esperanza.":
                r = "Northern Lights: +10 Hope.";
                break;
            case "Caní­bal Kale'Tav":
                r = "Kale'Tav Cannibal";
                break;
            case "Garra Caní­bal":
                r = "Cannibal Claw";
                break;
            case "Tentado por Sangre":
                r = "Bloodlusted";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Empieza combate con Evasión.\n-Se potencia si el enemigo está lastimado.</color>\n<color=#EE0000>-Una vez que perdió la evasión, es fácil de eliminar.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Starts combat with Evasion.\n-Empowered if the enemy is wounded.</color>\n<color=#EE0000>-Once it loses evasion, it is easy to eliminate.</color>";
                break;
            case "Ataque Lanza":
                r = "Spear Attack";
                break;
            case "Arrojar Lanza":
                r = "Throw Spear";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Ataque de lanza arrojadiza peligroso.</color>\n<color=#EE0000>-Poca Precisión.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Dangerous thrown spear attack.</color>\n\n<color=#199F10>-Creates traps.</color>\n<color=#EE0000>-Low Accuracy.</color>";
                break;
            case "Improvisar Trampas":
                r = "Improvise Traps";
                break;
            case "Trampa Improvisada: Daña y marca a unidades que la pisen.":
                r = "Improvised Trap: Damages and marks units that step on it.";
                break;
            case "Marcado":
                r = "Marked";
                break;
            case "Guerrero Kale'Tav":
                r = "Kale'Tav Warrior";
                break;
            case "Cazador Kale'Tav":
                r = "Kale'Tav Hunter";
                break;
            case "Furioso por Herida":
                r = "Furious from Wound";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Recibir Herida lo potencia.\n-Al matar a un enemigo se potencia.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Receiving Wound empowers it.\n-Killing an enemy empowers it.</color>\n<color=#EE0000>-Only one type of attack.</color>";
                break;
            case "Regocijo Asesino":
                r = "Killer's Rage";
                break;
            case "Hachazo Tribal":
                r = "Tribal Axe Strike";
                break;
            case "Bruja Kale'Tav":
                r = "Kale'Tav Witch";
                break;
            case "Golpe Bastón":
                r = "Staff Strike";
                break;
            case "Ataque de Cuervo":
                r = "Raven Attack";
                break;
            case "<i><i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Empowers Allies.\n-Its raven defends her.</color>\n<color=#EE0000>-Weak resistance.</color>";
                break;
            case "Frenesí­ del Asesinato":
                r = "Murderous Frenzy";
                break;
            case "Derribado":
                r = "Knocked Down";
                break;
            case "<i>Una criatura feroz nativa de la tundra. Es uno de los depredadores más temidos de la región y fuente de varias leyendas entre los Kale'Tav</i>\n\n<color=#199F10>-Regeneración leve.\n-Ataque de embestida en fila.</color>\n<color=#EE0000>-Suelen aparecer sólos o con una pareja como mucho.</color>":
                r = "<i>A fierce creature native to the tundra. It is one of the most feared predators in the region and the source of several legends among the Kale'Tav</i>\n\n<color=#199F10>-Minor regeneration.\n-Row charge attack.</color>\n<color=#EE0000>-They usually appear alone or with a partner at most.</color>";
                break;
            case "Mordisco Faagdan":
                r = "Faagdan Bite";
                break;
            case "Armadura masticada":
                r = "Chewed Armor";
                break;
            case "Embestida Faagdan":
                r = "Faagdan Charge";
                break;
            case "Garra Faagdan":
                r = "Faagdan Claw";
                break;
            case "Pájaro Rompe-Hielos":
                r = "Icebreaker Bird";
                break;
            case "Volador: Esta unidad no puede ser alcanzada por ataques melee, puede perder el vuelo al ser dañado o fallar un ataque.":
                r = "Flying: This unit cannot be reached by melee attacks, it may lose flight when damaged or missing an attack.";
                break;
            case "Picotazo Rompehielo":
                r = "Icebreaker Peck";
                break;
            case "Defensa abrumada":
                r = "Defense Overwhelmed";
                break;
            case "Vuelo Alto":
                r = "High Flight";
                break;
            case "<i>Este pájaro es muy territorial y ataca en grupo, su pico está hecho para romper el hielo grueso y poder pescar peces de gran tamaño, por lo tanto es muy peligroso.</i>\n\n<color=#199F10>-Vuela.\n-Su ataque baja defensa</color>\n<color=#EE0000>-Una vez que pierde su vuelo, es vulnerable.</color>":
                r = "<i>This bird is very territorial and attacks in groups, its beak is made to break thick ice and fish, therefore it is very dangerous.</i>\n\n<color=#199F10>-Flies.\n-Its attack lowers defense</color>\n<color=#EE0000>-Once it loses its flight, it is vulnerable.</color>";
                break;
            case "Efigie Animada":
                r = "Animated Effigy";
                break;
            case "Reacción: Al morir condena al enemigo que dió el último golpe.":
                r = "REACTION: Upon death, it condemns the enemy that dealt the final blow.";
                break;
            case " es condenado por 3 turnos.":
                r = " is condemned for 3 turns.";
                break;
            case "Condena: En X cantidad de turnos recibirá daño verdadero igual al 10% de su vida máxima por turno con el efecto.":
                r = "Condemned: In X turns, it will receive true damage equal to 10% of its maximum health per turn with the effect.";
                break;
            case " es dañado por la Condena.":
                r = " is damaged by Condemnation.";
                break;
            case "Corte Hoz":
                r = "Sickle Cut";
                break;
            case "<i>Armadas por la magia oscura de los Kale'Tav, estas efigies están por todo su territorio como primer linea de defensa en contra de quienes se atrevan a cruzar el Paso.</i>\n\n<color=#199F10>-Al ser destruida condena a su atacante.\n-Provoca sangrado.</color>\n<color=#EE0000>-Débiles.</color>":
                r = "<i>Raised with the dark magic of the Kale'Tav, these effigies are spread throughout their territory as the first line of defense against those who dare to cross the Pass.</i>\n\n<color=#199F10>-Upon destruction, it condemns its attacker.\n-Causes bleeding.</color>\n<color=#EE0000>-Weak.</color>";
                break;
            case "Levantar Martillo":
                r = "Raise Hammer";
                break;
            case "Martillo Listo":
                r = "Hammer Ready";
                break;
            case "Gulek Gul pierde el buff 'Martillo Listo' tras recibir daño y no podrá utilizarlo.":
                r = "Gulek Gul loses the 'Hammer Ready' buff after taking damage and will not be able to use it.";
                break;
            case "Martillo Pequeño":
                r = "Small Hammer";
                break;
            case "Martillo Grande":
                r = "Great Hammer";
                break;
            case "<i>Gulek-Gul es un Ettin muy venerado por los Kale'Tav. No habita con ellos, pero cuando se encuentran intrusos en la zona, baja de su colina decidido a proteger su territorio.</i>\n\n<color=#199F10>-Fuerza descomunal.\n-Golpea en zona.\n-Doble intento en tiradas de voluntad.</color>\n<color=#EE0000>-Necesita levantar el martillo grande antes de usarlo.\n-Si recibe daño o falla tirada de voluntad, deja caer el martillo.</color>":
                r = "<i>Gulek-Gul is a Ettin highly revered by the Kale'Tav. He does not dwell with them, but when intruders are found in the area, he descends from his hill determined to protect his territory.</i>\n\n<color=#199F10>-Immense strength.\n-Hits in area.\n-Double attempt on will rolls.</color>\n<color=#EE0000>-Needs to lift the great hammer before using it.\n-If he takes damage or fails a will roll, he drops the hammer.</color>";
                break;
            case "Discutir Tácticas":
                r = "Discuss Tactics";
                break;
            case "Enfoque Defensivo":
                r = "Defensive Focus";
                break;
            case "Enfoque Agresivo":
                r = "Aggressive Focus";
                break;
            case "Descansando":
                r = "Resting";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque defensivo.":
                r = "Gulek and Gul discuss tactics and decide to adopt a defensive approach.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque ofensivo.":
                r = "Gulek and Gul discuss tactics and decide to adopt an offensive approach.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven descansar para recuperar fuerzas.":
                r = "Gulek and Gul discuss tactics and decide to rest to regain strength.";
                break;
            case "Escudado: 10% chances por stack de evitar un ataque fí­sico. Al evitar uno, pierde un stack.":
                r = "Shielded: 10% chance per stack to avoid a physical attack. Upon avoiding one, it loses a stack.";
                break;
            case " bloquea el daño con su escudo.":
                r = " blocks damage with its shield.";
                break;
            case "Bloqueado":
                r = "Blocked";
                break;
            case "Golpe Mazo":
                r = "Mace Strike";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Unidad Escudada.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of Lich Kadryn. Now they seek revenge by ensuring that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Shielded Unit.\n-Good Armor.\n-Upon death, leaves a cloud of black breath.</color>\n<color=#EE0000>-Limited movement.</color>";
                break;
            case "Extasiado por Aliento Negro":
                r = "Ecstatic from Black Breath";
                break;
            case "Restos de Aliento: Potencia y cura a los Vengadores de Kadryn.":
                r = "Breath Residue: Empowers and heals the Avengers of Kadryn.";
                break;
            case "Reacción: Al morir genera restos de Aliento Negro en el campo de batalla.":
                r = "Reaction: Upon death, generates residues of Black Breath on the battlefield.";
                break;
            case "Soldado Vengador de Kadryn":
                r = "Footman Avenger of Kadryn";
                break;
            case " reacciona con Primer Golpe.":
                r = " reacts with First Strike.";
                break;
            case "Primer Golpe: el Alabardero ataca a la primera unidad que entra en la casilla.":
                r = "First Strike: the Halberdier attacks the first unit that enters the square.";
                break;
            case "Alabardero Vengador de Kadryn":
                r = "Halberdier Avenger of Kadryn";
                break;
            case "Estocada Alabarda":
                r = "Halberd Thrust";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Buen ataque.\n-Flecha envenenada.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of Lich Kadryn. Now they seek revenge by ensuring that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Precise.\n-Poison Arrow.\n-Upon death, leaves a cloud of black breath.</color>\n<color=#EE0000>-Poor resistance.";
                break;
            case "Tiro con Arco":
                r = "Bow Shot";
                break;
            case "Primer Golpe":
                r = "First Strike";
                break;
            case "Predicador del Aliento Negro":
                r = "Preacher of the Black Breath";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de rango infalible.\n-Potencia Aliados.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of Lich Kadryn. Now they seek revenge by ensuring that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Infallible range attack.\n-Ally Empowerment.\n-Upon death, leaves a cloud of black breath.</color>\n<color=#EE0000>-Poor resistance.</color>";
                break;
            case "Oración de Kadryn":
                r = "Kadryn's Prayer";
                break;
            case "Rayo necrótico":
                r = "Necrotic Ray";
                break;
            case "Liturgia de la Putrefacción":
                r = "Litany of Putrefaction";
                break;
            case "es presa de la Putrefacción.":
                r = "falls prey to the Putrefaction.";
                break;
            case "Putrefacción":
                r = "Putrefaction";
                break;
            case "El Aliento Negro se expande por el campo enemigo.":
                r = "The Black Breath spreads across the enemy field.";
                break;
            case "desata un rayo necrótico sobre":
                r = "unleashes a necrotic ray upon";
                break;
            case "Sus defensas se corroen por el Aliento Negro.":
                r = "Their defenses are corroded by the Black Breath.";
                break;
            case "Castigar a los Malvados":
                r = "Punish the Wicked";
                break;
            case "Marca: ":
                r = "Mark: ";
                break;
            case " posee bonificaciones de daño y ataque con ataques individuales contra este enemigo.":
                r = " has bonuses to damage and attack with individual attacks against this enemy.";
                break;
            case "Ninfa Ardiendo":
                r = "Burning Nymph";
                break;
            case "Ataque Raiz Ardiente":
                r = "Burning Root Attack";
                break;
            case "se entierra y desaparece del campo.":
                r = "buries itself and disappears from the field.";
                break;
            case "emerge de vuelta.":
                r = "emerges back.";
                break;
            case "Emergida":
                r = "Emerged";
                break;
            case "Enterrarse":
                r = "Bury";
                break;
            case "Enterrado":
                r = "Buried";
                break;
            case "La raiz permanece oculta bajo tierra, preparandose para emerger.":
                r = "The root remains hidden underground, preparing to emerge.";
                break;
            case "Llamarada Raiz":
                r = "Flame attack";
                break;
            case "<i>Raiz-Viva del bosque mismo que ha salido a la superficie obligada por las llamas, ahora atacará furiosa a cualquier invasor del bosque.</i>\n\n<color=#199F10>-Ataque de llamas infalible.\n-Se entierra para curarse.</color>\n<color=#EE0000>-Inmóvil.</color>":
                r = "<i>Living-Root of the forest itself that has come to the surface forced by the flames, it will now furiously attack any invader of the forest.</i>\n\n<color=#199F10>-Infallible flame attack.\n-Buries itself to heal.</color>\n<color=#EE0000>-Immobile.</color>";
                break;
            case "Garra Oso Espectral":
                r = "Spectral Bear Claw";
                break;
            case "<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia fí­sica es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>":
                r = "<i>This bear has become a fierce specter roaming the burning forest. Its physical power is terrifying.</i>\n\n<color=#199F10>-Overwhelming attacks.\n-Great amount of life.</color>\n<color=#EE0000>-Higher chance of fumble.</color>";
                break;
            case "Bonus de daño elemental.":
                r = "Bonus elemental damage.";
                break;
            case "<i>Esta bestia oriunda del Paso es material de varias leyendas y pesadillas entre los Kale'Tav. De cuerpo robusto y cuernos afilados, supone un peligro para los viajeros incautos.</i>\n\n<color=#199F10>-Ataques de carga en fila.\n-Regeneración leve.</color>\n<color=#EE0000>-Lento.</color>":
                r = "<i>This beast native to the Passage is the origin of various legends and nightmares among the Kale'Tav. With a robust body and sharp horns, it poses a danger to unwary travelers.</i>\n\n<color=#199F10>-Line charge attacks.\n-Slight regeneration.</color>\n<color=#EE0000>-Slow.</color>";
                break;
            case "Milicianos disponibles: ":
                r = "Available Militiamen: ";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de oportunidad.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of the Lich Kadryn. Now they seek revenge by trying to ensure that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Opportunity attack.\n-Good Armor.\n-When they die, they leave a cloud of black breath.</color>\n<color=#EE0000>-Limited movement.</color>";
                break;
            case "Refuerzos":
                r = "Reinforcements";
                break;
            case "Refuerzos aliados disponibles, irán uniéndose a la batalla gradualmente.":
                r = "Allied reinforcements available, will gradually join the battle.";
                break;
            case "Refuerzos enemigos disponibles, irán uniéndose a la batalla gradualmente.":
                r = "Enemy reinforcements available, will gradually join the battle.";
                break;
            case "El Bosque Ardiente":
                r = "The Burning Forest";
                break;
            case "Paso Vientohelado":
                r = "Frozenwind Passage";
                break;
            case "A medida que viajas por el bosque, las llamas envolverán regiones del mapa de forma inesperada.\n\nSi intentas atravesar un Nodo prendido fuego, perderás 10 de Esperanza y 8-15 Civiles.\nNo se podrá descansar en nodos incendiados.\n\nAdemás, las batallas que tengan lugar en un Nodo incendiado, tendrán llamas en el campo de batalla.":
                r = "As you travel through the forest, flames will engulf regions of the map unexpectedly.\n\nIf you try to cross a node on fire, you will lose 10 Hope and 8-15 Civilians.\nYou will not be able to rest in burning nodes.\n\nAdditionally, battles taking place in a burning Node will have flames on the battlefield.";
                break;
            case "<color=#FF3D00>-El incendio ha envuelto un nodo cercano al camino de la caravana.</color>":
                r = "<color=#FF3D00>-The fire has engulfed a node near the caravan's path.</color>";
                break;
            case "\n<color=#FF3D00>--Incendiado--</color>":
                r = "\n<color=#FF3D00>--Burning--</color>";
                break;
            case "La lluvia desactiva la mecanica del Bosque Ardiente.":
                r = "Rain disables the Burning Forest fire mechanic.";
                break;
            case "La lluvia apaga los focos de incendio actuales.":
                r = "The rain puts out the active fire hotspots.";
                break;
            case "La lluvia ha apagado los incendios en el área temporalmente.":
                r = "The rain has temporarily extinguished the fires in the area.";
                break;
            case "Llamas: infligen daño fuego a unidades que entren en la casilla.":
                r = "Flames: deal fire damage to units entering the tile.";
                break;
            case "Raiz-Viva Ardiendo":
                r = "Burning Living-Root";
                break;
            case "Enfurecido por el Fuego":
                r = "Enraged by Fire";
                break;
            case "Barro: reduce 2 PA a unidades que entren en la casilla.":
                r = "Mud: reduces 2 AP to units entering the tile.";
                break;
            case "La tribu Kale'Tav está realizando rituales en el área, preparándose para el Aliento Negro.\n\nAl escuchar sus tambores a lo lejos sabrás dónde se encuentran.\nPor cada Ritual completado, sus combatientes recibirán bonificaciones en batalla.\n\nPara interrumpir un ritual debes aproximarte a los nodos marcados y derrotarlos.\n\nFuerza Kale'Tav: ":
                r = "The Kale'Tav tribe is performing rituals in the area, preparing for the Black Breath.\n\nHearing their drums in the distance will let you know where they are.\nFor each completed Ritual, their fighters will receive bonuses in battle.\n\nTo interrupt a ritual you must approach the marked nodes and defeat them.\n\nKale'Tav Strength: ";
                break;
            case "<color=#6A0DAD>-Un ritual Kale'Tav ha comenzado en un nodo cercano. La másica profana desalienta a la caravana. -5 Esperanza.</color>":
                r = "<color=#6A0DAD>-A Kale'Tav ritual has started at a node near the caravan's path.</color>";
                break;
            case "<color=#FF3D00>-Un ritual Kale'Tav ha sido completado. La fuerza de Kale'Tav aumenta en 1.</color>":
                r = "<color=#FF3D00>-A Kale'Tav ritual has been completed. Kale'Tav's strength increases by 1.</color>";
                break;
            case "-El ritual Kale'Tav ha sido detenido. +10 Esperanza.":
                r = "-The Kale'Tav ritual has been stopped. +10 Hope.";
                break;
            case "Batalla Kale'Tav":
                r = "Kale'Tav Battle";
                break;
            case "Fuerza Kale'Tav":
                r = "Kale'Tav Strength";
                break;
            case "Manual":
                r = "Handbook";
                break;
            case "Mapa":
                r = "Map";
                break;
            case "Zonas":
                r = "Zones";
                break;
            case "Civiles":
                r = "Civilians";
                break;
            case "Personajes":
                r = "Characters";
                break;
            case "Aliento Negro":
                r = "Black Breath";
                break;
            case "Ir al Manual de Combate":
                r = "Go to Battle Handbook";
                break;
            case "Ir al Manual de Campaña":
                r = "Go to Campaign Handbook";
                break;
            case "Combate":
                r = "Combat";
                break;
            case "Grillas":
                r = "Grids";
                break;
            case "Turnos":
                r = "Turns";
                break;
            case "Acciones":
                r = "Actions";
                break;
            case "Sistema":
                r = "System";
                break;
            case "Daños":
                r = "Damages";
                break;
            case "Estados":
                r = "Statuses";
                break;
            case "Activa!":
                r = "Active!";
                break;
            case "Coste: ":
                r = "Cost: ";
                break;
            case "PA":
                r = "AP";
                break;
            case "Abrumado":
                r = "Overwhelmed";
                break;
            case "Destruyes":
                r = "You destroy";
                break;
            case "Este obstaculo no puede ser destruido por tus unidades.":
                r = "This obstacle cannot be destroyed by your units.";
                break;
            case "Gasta 3 PA, para destruir un obstaculo adyacente de tu mismo lado si lo permite.":
                r = "Spend 3 AP to destroy an adjacent obstacle on your side if allowed.";
                break;
            case "No tienes flechas para usar esta habilidad.":
                r = "You don't have arrows to use this ability.";
                break;
            case "Sin flechas!":
                r = "No more arrows!";
                break;
            case "Volumen de la Música":
                r = "Music Volume";
                break;
            case "Reproducir másica al minimizar":
                r = "Play music when minimized";
                break;
            case "Idioma":
                r = "Language";
                break;
            case "Inglés":
                r = "English";
                break;
            case "Español":
                r = "Spanish";
                break;
            case "Idioma del juego":
                r = "Game Language";
                break;
            case "Gráficos":
                r = "Graphics";
                break;
            case "Controles":
                r = "Controls";
                break;
            case "Jugabilidad":
                r = "Gameplay";
                break;
            case "Salir del juego":
                r = "Exit Game";
                break;
            case "Pantalla Completa":
                r = "Fullscreen";
                break;
            case "Resolución de Pantalla":
                r = "Screen Resolution";
                break;
            case "Calidad Gráficos":
                r = "Graphics Quality";
                break;
            case "Alta":
                r = "High";
                break;
            case "Media":
                r = "Medium";
                break;
            case "Baja":
                r = "Low";
                break;
            case "Accesos Rápidos":
                r = "Hotkeys";
                break;
            case "Dificultad":
                r = "Difficulty";
                break;
            case "-----Combate-----":
                r = "-----Combat-----";
                break;
            case "Modo Rápido":
                r = "Fast Mode";
                break;
            case "Debido a la invasión, Nedukazal está envuelta en caos y oscuridad, por lo tanto la caravana no podrá ver claramente el camino adelante.\n\nAl depender de la luz propia, será mas propensa a sufrir emboscadas (+20%).\n\nMejora las <b>Antorchas de Pie</b> para aumentar el rango de visión.\n\nEl Aliento Negro no será una preocupación en esta zona.":
                r = "Due to the invasion, Nedukazal is shrouded in chaos and darkness, so the caravan will not be able to clearly see the path ahead.\n\nRelying on its own light, it will be more prone to ambushes (+20%).\n\nUpgrade the <b>Standing Torches</b> to increase the vision range.\n\nThe Black Breath will not be a concern in this area.";
                break;
            case "\nSe conseguirán de 25-40 Materiales y 60-85 Suministros.":
                r = "\n25-40 Materials and 60-85 Supplies will be gathered.";
                break;
            case "Nedukazal está a oscuras.":
                r = "Nedukazal is in darkness.";
                break;
            case "Masacre: Nedukazal está siendo atacada. -10 Esperanza. +10% Emboscada. Los Zarkil están potenciados.":
                r = "Massacre: Nedukazal is being attacked. -10 Hope. +10% Ambush. The Zarkil are empowered.";
                break;
            case "Garra Zarkil":
                r = "";
                break;
            case "Zarkil Acechador":
                r = "Zarkil Stalker";
                break;
            case "Masacre Zarkil":
                r = "Zarkil Massacre";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverán.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-When dodging an attack, they will move.\n-Can see hidden units.</color>\n<color=#EE0000>-Only one type of attack.</color>";
                break;
            case "Agazapado":
                r = "Lurking";
                break;
            case "Zarkil Guerrero":
                r = "Zarkil Warrior";
                break;
            case "Mirada de la Masacre":
                r = "Gaze of the Massacre";
                break;
            case "Victima de la masacre":
                r = "Massacre's Victim";
                break;
            case " reacciona con Mirada de la Masacre.":
                r = " reacts with Gaze of the Massacre.";
                break;
            case "Aterrado":
                r = "Terrified";
                break;
            case "Mirada de Masacre: al moverse aquí­, Tirada de salvación mental CD 13 o se pierde el turno.":
                r = "Gaze of the Massacre: when moving here, Mental saving throw DC 13 or lose the turn.";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Can terrify creatures in front.\n-Can see hidden units.</color>\n<color=#EE0000>-Only one type of attack.</color>";
                break;
            case "Zarkil Vociferador":
                r = "Zarkil Shouter";
                break;
            case "Grito de batalla Zarkil":
                r = "Zarkil Battle Cry";
                break;
            case "Orden Recibida":
                r = "Order Received";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que además motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Stunning shout that also motivates allies.\n-Can see hidden units.\n-Can attack repeatedly.</color>\n<color=#EE0000></color>";
                break;
            case "Rayo Debilitador":
                r = "Weakening Ray";
                break;
            case "Debilitado":
                r = "Weakened";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-Débil</color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Unerring weakening attack.\n-Can see hidden units.\n-Flying.</color>\n<color=#EE0000>-Weak</color>";
                break;
            case "Zarkil Alado":
                r = "Winged Zarkil";
                break;
            case "Mordisco Zarkilever":
                r = "Zarkilever Bite";
                break;
            case " sufre el efecto de Saboreado, recibiendo 15 de daño extra y curando al Zarkilever por 14 puntos de vida.":
                r = " suffers the Savored effect, receiving 15 extra damage and healing the Zarkilever for 14 health points.";
                break;
            case "Saborear":
                r = "Taste";
                break;
            case "Saboreando!":
                r = "Tasting!";
                break;
            case "<i>Criatura muy feroz controlada porlos Zarkils utilizada como fuerza de impacto y para causar grietas en superficies duras. </color>\n\n<color=#199F10>-Buena Armadura.\n-Saborea a las ví­ctimas.</color>\n<color=#EE0000></color>":
                r = " <i>Very fierce creature controlled by the Zarkils used as a shock force and to cause cracks in hard surfaces. </color>\n\n<color=#199F10>-Good Armor.\n-Tastes victims.</color>\n<color=#EE0000></color>";
                break;
            case "Por la masacre":
                r = "For the massacre";
                break;
            case "Llamada Zarkil":
                r = "Zarkil Call";
                break;
            case "Rayo Zarkil":
                r = "Zarkil Ray";
                break;
            case "Comandante Zarkil":
                r = "Zarkil Commander";
                break;
            case "<i>Tiene una legión entera de Zarkils bajo su liderazgo, simplemente debe señalar un objetivo y sus súbditos se encargarán del resto.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque debilitador infalible.</color>\n<color=#EE0000>-No es fuerte por si solo.</color>":
                r = "<i>He has an entire legion of Zarkils under his leadership, he just has to point to a target and his subjects will take care of the rest.</color>\n\n<color=#199F10>-Calls endless reinforcements.\n-Unerring weakening attack.</color>\n<color=#EE0000>-Not strong on his own.</color>";
                break;
            case "Llamada Espectral":
                r = "Spectral Call";
                break;
            case "Condena del bosque":
                r = "Forest's Curse";
                break;
            case "<i>Manifestación de la energí­a espectral del bosque. Desde su interior emana un fulgor fantasmal frí­o, como un espí­ritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrótico que condena a dos objetivos.</color>\n<color=#EE0000>-Inmóvil.</color>":
                r = "<i>Manifestation of the spectral energy of the forest. From within it emanates a cold ghostly glow, like a trapped spirit writhing to escape.</color>\n\n<color=#199F10>-Calls endless reinforcements.\n-Necrotic attack that condemns two targets.</color>\n<color=#EE0000>-Immobile.</color>";
                break;
            case "Aliento Helado":
                r = "Frost Breath";
                break;
            case "Draco de Hielo":
                r = "Ice Drake";
                break;
            case "Garra Draco":
                r = "Drake Claw";
                break;
            case "<i>Estas criaturas aladas habitan en las regiones más frí­as del Paso. Son conocidas por ser muy territoriales y por su aliento gélido.</i>\n\n<color=#199F10>-Vuelo.\n-Aliento gélido en zona.\n-Regenera armadura.</color>\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>These winged creatures inhabit the coldest regions of the Pass. They are known for being very territorial and for their icy breath.</i>\n\n<color=#199F10>-Flight.\n-Icy breath in area.\n-Regenerates armor.</color>\n<color=#EE0000>-Weak to fire.</color>";
                break;
            case "Aturdido":
                r = "Stunned";
                break;
            case "Inalcanzable: unidad volando":
                r = "Unreachable: flying unit";
                break;
            case "Inalcanzable: unidad escondida":
                r = "Unreachable: hidden unit";
                break;
            case " PA":
                r = " AP";
                break;
            case "No hay suficientes flechas":
                r = "Not enough arrows";
                break;
            case "No hay suficientes energí­a":
                r = "Not enough energy";
                break;
            case "ATQ":
                r = "ATK";
                break;
            case "TS":
                r = "SV";
                break;
            case "mods":
                r = "mods";
                break;
            case "atr":
                r = "attr";
                break;
            case "hab":
                r = "skill";
                break;
            case "atq":
                r = "atk";
                break;
            case "clima":
                r = "weather";
                break;
            case "situacional":
                r = "situational";
                break;
            case "vs":
                r = "vs";
                break;
            case "DEF":
                r = "DEF";
                break;
            case "crit":
                r = "crit";
                break;
            case "pifia":
                r = "fumble";
                break;
            case "ESTADO":
                r = "STATUS";
                break;
            case "BUFF":
                r = "BUFF";
                break;
            case "DEBUFF":
                r = "DEBUFF";
                break;
            case "TRAMPA":
                r = "TRAP";
                break;
            case "DAÑO":
                r = "DAM";
                break;
            case "MUERTE":
                r = "DEATH";
                break;
            case "CURACION":
                r = "HEAL";
                break;
            case "Golpe":
                r = "Hit";
                break;
            case "Crí­tico":
                r = "Critical";
                break;
            case "Roce":
                r = "Graze";
                break;
            case "Exito":
                r = "Success";
                break;
            case "Tiro Potente":
                r = "Powerful Shot";
                break;
            case "Cargando...":
                r = "Loading...";
                break;
            case "Acampar":
                r = "Camp";
                break;
            case "<color=#FF6666>No puedes descansar aquí­.</color>":
                r = "<color=#FF6666>You can't rest here.</color>";
                break;
            case "Actualmente encarnado en un enemigo, invulnerable.":
                r = "Currently incarnated in an enemy, invulnerable.";
                break;
            case "Encarnado en Enemigo":
                r = "Incarnated in Enemy";
                break;
            case "Adelántate para usarla":
                r = "Melee: Move ahead to use it";
                break;
            case "Inmóvil, Melee solo adyacente.":
                r = "Immobile: melee only against adjacent targets.";
                break;
            case "Melee disponible":
                r = "Melee available";
                break;
            case "Intercambiable":
                r = "Exchangeable";
                break;
            case "Habilidades de Combate":
                r = "Combat Skills";
                break;
            case "Actividad durante el viaje":
                r = "Activity while traveling";
                break;
            case "Selecciona un objetivo.":
                r = "Choose a target.";
                break;
            case " % Chances":
                r = " % To hit";
                break;
            case "No hay objetivos al alcance.":
                r = "No targets in range.";
                break;
            case "¡Comienza la batalla!":
                r = "The battle begins!";
                break;
            case "¡Viaje completado!":
                r = "Travel completed!";
                break;
            case "Finalmente la caravana ha llegado a la Ciudad Puerto de Serria, donde la población civil se prepara para embarcar y así­ escapar del Aliento Negro.":
                r = "At last, the caravan has arrived at the Port City of Serria, where the civilian population is preparing to embark and escape the Black Breath.";
                break;
            case "El viaje ha durado ":
                r = "This trip lasted ";
                break;
            case " dí­as enteros y han sobrevivido ":
                r = " days and survived ";
                break;
            case "civiles.\n\n":
                r = " civilians.\n\n";
                break;
            case "Además, el oro restante (":
                r = "Additionally, the remaining gold (";
                break;
            case ") se ha donado a las arcas de la ciudad para ayudar a financiar la evacuación.\n\nLos Personajes sobrevivientes también se han unido al esfuerzo de evacuación para defender la ciudad.\n\n":
                r = ") has been donated to the city treasury to help fund the evacuation.\n\nThe surviving Characters have also joined the evacuation effort to defend the city.\n";
                break;
            case "<b>Valor de Trabajo obtenido: ":
                r = "<b>Work Value obtained: ";
                break;
            case "Valor de Trabajo Disponible:":
                r = "Available Work Value:";
                break;
            case "Valor de Corrupción actual:":
                r = "Current Corruption Value:";
                break;
            case "El <b>Nivel de Peligro</b> actual en el Bosque Ardiente es: ":
                r = "The current <b>Danger Level</b> in the Burning Forest is: ";
                break;
            case "El <b>Nivel de Peligro</b> actual en el Paso Vientohelado es: ":
                r = "The current <b>Danger Level</b> in the Frozen Wind Pass is: ";
                break;
            case "El <b>Nivel de Peligro</b> actual en Nedukazal es: ":
                r = "The current <b>Danger Level</b> in Nedukazal is: ";
                break;
            case "Menu de Mejoras":
                r = "Improvements Menu";
                break;
            case "Barcos":
                r = "Ships";
                break;
            case "Templo":
                r = "Temple";
                break;
            case "Barricadas":
                r = "Barricades";
                break;
            case "Cuartel":
                r = "Barracks";
                break;
            case "Almenaras":
                r = "Beacons";
                break;
            case "Palacio":
                r = "Palace";
                break;
            case "Granjas":
                r = "Farms";
                break;
            case "La ciudad puede permitirse esperar ":
                r = "The city can afford to wait ";
                break;
            case " caravanas más antes de tener que zarpar.":
                r = " more caravans before having to set sail.";
                break;
            case "Misiones de Salvamento: ":
                r = "Rescue Missions Available: ";
                break;
            case "Pueden ser solicitadas por caravanas futuras para ayudar en momentos de crisis.":
                r = "They can be requested by future caravans to help in times of crisis.";
                break;
            case "Misiones Disponibles: ":
                r = "Available Missions: ";
                break;
            case "Solicitar Salvamento:":
                r = "Request Rescue:";
                break;
            case "Pedir Ayuda":
                r = "Ask for Help";
                break;
            case "Misión de Salvamento":
                r = "Rescue Mission";
                break;
            case "El ave mensajera regresa con un mensaje atado a sus patas. En él se indica el punto exacto al que la caravana deberá dirigirse para encontrarse con el equipo de salvamento, junto con los recursos cedidos por la ciudad de Serria.\n":
                r = "The messenger bird returns with a message tied to its legs. It indicates the exact point where the caravan must go to meet the rescue team, along with the resources provided by the city of Serria.\n";
                break;
            case "Ubicación de la Misión de Salvamento":
                r = "Rescue Mission Location";
                break;
            case "<color=#a0e812><b>\n\nSe ha marcado en el camino adelante el nodo al cual deberáas dirigirte para encontrarte con el equipo de salvamento.</b></color>":
                r = "<color=#a0e812><b>\n\nThe node ahead where you should head to meet the rescue team has been marked on the path.</b></color>";
                break;
            case "Un encuentro esperado":
                r = "An Expected Encounter";
                break;
            case "Has llegado al lugar señalado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traí­do para ayudarles en su travesí­a.\n\nInmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.\n":
                r = "You have arrived at the location indicated by the messenger bird and have met the rescue team sent by the Port City of Serria.\nThey immediately greet the caravan and begin unloading the resources they have brought to assist you on your journey.\n\nThe spirits in the caravan immediately improve upon seeing that they are not alone in this struggle.\n";
                break;
            case "<color=#a0e812><b>\n\nSe han entregado ":
                r = "<color=#a0e812><b>\n\nThe caravan won ";
                break;
            case " suministros. +25 Esperanza. +20 Materiales y 200 Oro y un nuevo personaje se suma a la caravana</b></color>":
                r = " supplies. +25 Hope. +20 Materials and 200 Gold and a new character joins the caravan</b></color>";
                break;
            case "-Las oraciones de los Purificadores del Templo de Serria merman el avance del Aliento Negro en: ":
                r = "-The prayers of the Purifiers in the Temple of Serria reduce the advance of the Black Breath by: ";
                break;
            case " Esperanza":
                r = " Hope";
                break;
            case "-Las almenaras de Serria se divisan a lo lejos sobre las montañas, brillando con fuerza y marcando el destino de la caravana: ":
                r = "-The beacons of Serria can be seen in the distance over the mountains, shining brightly and marking the caravan's destination: ";
                break;
            case "<b>Una patrulla de milicianos de Serria se une a la batalla como refuerzos.</b>":
                r = "<b>A patrol of militiamen from Serria joins the battle as reinforcements.</b>";
                break;
            case "<b>Arbol Vengativo</b>":
                r = "<b>Cursed Tree</b>";
                break;
            case "Arbol Vengativo":
                r = "Cursed Tree";
                break;
            case "Árbol Vengativo":
                r = "Cursed Tree";
                break;
            case "Puntos de Acción":
                r = "Action Points";
                break;
            case "Volver al Menu Principal":
                r = "Return to Main Menu";
                break;
            case "Tutorial activo, atajos deshabilitados.":
                r = "Tutorial active, shortcuts disabled.";
                break;
            case "Cargar Partida":
                r = "Load Game";
                break;
            case "es condenado por":
                r = "is condemned by";
                break;
            case "turnos.":
                r = "turns.";
                break;
            case "resiste la condena, pero sufre el latido necrotico.":
                r = "resists the condemnation, but suffers the necrotic beat.";
                break;
            case "desata una llamarada ardiente sobre":
                r = "unleashes a burning attack on";
                break;
            case "desata un rayo debilitador sobre":
                r = "unleashes a weakening bolt on";
                break;
            case " se aterra por Mirada de la Masacre y pierde el turno.":
                r = " is terrified by the Gaze of Massacre and loses a turn.";
                break;
            case " obtiene un intento adicional de Tirada de Salvación.":
                r = " gets an additional attempt at a Saving Throw.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque agresivo.":
                r = "Gulek and Gul discuss tactics, and decide to adopt an aggressive approach.";
                break;
            case "Aturdido!":
                r = "Stunned!";
                break;
            case "Turno de":
                r = "Turn of";
                break;
            case "No se salva":
                r = "Not saved";
                break;
            case "Se salva":
                r = "Saved";
                break;
            case "Fortaleza":
                r = "Fortitude";
                break;
            case "Reflejos":
                r = "Reflexes";
                break;
            case "":
                r = "";
                break;
            case "Mental":
                r = "Mental";
                break;
            case " obtiene ":
                r = " gets ";
                break;
            case ": -Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada.":
                r = ": -Rain: -5 Hope. -15% Supplies Gathering, -20% Ambush Chance.";
                break;
            case "Defender":
                r = "Defend";
                break;
            case "Huir":
                r = "Flee";
                break;
            case "Asentamiento":
                r = "Settlement";
                break;
            case "Esperanza.":
                r = "Hope.";
                break;
            case "Sin consecuencias graves.":
                r = "No serious consequences.";
                break;
            case "Derrota: ":
                r = "Defeat: ";
                break;
            case "-Victoria contra ":
                r = "-Victory against ";
                break;
            case "-Derrota frente a ":
                r = "-Defeat against ";
                break;
            case " Materiales.":
                r = " Materials.";
                break;
            case "<color=#2a9c71>\n\nFatigado: -1 Atributos hasa próximo descanso. </color>":
                r = "<color=#2a9c71>\n\nTired: -1 Attributes until next rest. </color>";
                break;
            case "Torpe: +1 Rango Pifias":
                r = "Clumsy: +1 Range Miss.";
                break;
            case "Poción de Curación Media":
                r = "Medium Healing Potion";
                break;
            case "objetivo":
                r = "target";
                break;
            case "Impacto crí­tico":
                r = "Critical Hit";
                break;
            case " usa ":
                r = " uses ";
                break;
            case "Bonus daño elemental Acido.":
                r = "Acid Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Arcano.":
                r = "Arcane Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Fuego.":
                r = "Fire Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Hielo.":
                r = "Ice Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Necro.":
                r = "Necrotic Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Rayo.":
                r = "Lightning Elemental Damage Bonus.";
                break;
            case "Bonus daño elemental Divino.":
                r = "Divine Elemental Damage Bonus.";
                break;
            case "Armadura reforzada que prioriza movilidad.":
                r = "Reinforced armor that prioritizes mobility.";
                break;
            case "Armadura reforzada con defensa elemental adicional.":
                r = "Reinforced armor with additional elemental defense.";
                break;
            case "Armadura reforzada pensada para sigilo y evasiones.":
                r = "Reinforced armor designed for stealth and evasions.";
                break;
            case "Armadura de cuero reforzado equilibrada y resistente.":
                r = "Balanced and durable reinforced leather armor.";
                break;
            case "Espada corta imbuida con energia arcana.":
                r = "Shortsword infused with arcane energy.";
                break;
            case "Espada corta de filo oscuro y dano certero.":
                r = "Dark-edged shortsword with precise damage.";
                break;
            case "Espada corta agil y versatil para combate cercano.":
                r = "Agile and versatile shortsword for close combat.";
                break;
            case "Coraza templada para contraataques de fuego.":
                r = "Tempered cuirass for fire-based counterattacks.";
                break;
            case "Coraza liviana para mantener proteccion con movilidad.":
                r = "Lightweight cuirass that keeps protection and mobility.";
                break;
            case "Coraza robusta que incrementa la fuerza bruta.":
                r = "Sturdy cuirass that increases raw strength.";
                break;
            case "Coraza de caballero para defensa frontal solida.":
                r = "Knight cuirass for solid frontal defense.";
                break;
            case "Mandoble bendecido con energia sagrada.":
                r = "Greatsword blessed with sacred energy.";
                break;
            case "Mandoble que castiga con mas fuerza a enemigos heridos.":
                r = "Greatsword that hits harder against wounded enemies.";
                break;
            case "Mandoble helado que enfria y desgasta al objetivo.":
                r = "Frosted greatsword that chills and wears down the target.";
                break;
            case "Mandoble pesado para golpes contundentes.":
                r = "Heavy greatsword for crushing blows.";
                break;
            case "Armadura de cuero que mejora la resistencia fisica.":
                r = "Leather armor that improves physical resilience.";
                break;
            case "Armadura de cuero flexible para buena movilidad.":
                r = "Flexible leather armor for better mobility.";
                break;
            case "Armadura de cuero ligera orientada a evasion.":
                r = "Lightweight leather armor focused on evasion.";
                break;
            case "Arco largo con disparos corrosivos de acido.":
                r = "Longbow with corrosive acid shots.";
                break;
            case "Arco largo reforzado para disparos de alto impacto.":
                r = "Reinforced longbow for high-impact shots.";
                break;
            case "Arco largo que reduce el ritmo del objetivo.":
                r = "Longbow that slows the target.";
                break;
            case "Arco largo versatil para combate a distancia.":
                r = "Versatile longbow for ranged combat.";
                break;
            case "Baculo purificador que canaliza energia sagrada.":
                r = "Purifying staff that channels sacred energy.";
                break;
            case "Balsamo que mejora la mente y la concentracion por un combate.":
                r = "Balm that improves mental clarity and focus for one battle.";
                break;
            case "Balsamo que acelera reflejos y reaccion por un combate.":
                r = "Balm that boosts reflexes and reactions for one battle.";
                break;
            case "Balsamo que refuerza la fortaleza y la resistencia por un combate.":
                r = "Balm that reinforces toughness and resistance for one battle.";
                break;
            case "Anillo orientado al dano ofensivo y al impacto magico.":
                r = "Ring focused on offensive damage and magical impact.";
                break;
            case "Anillo orientado al enfoque mental y control arcano.":
                r = "Ring focused on mental clarity and arcane control.";
                break;
            case "Arco estandar del explorador, fiable para ataques a distancia.":
                r = "Standard explorer bow, reliable for ranged attacks.";
                break;
            case "Baston de purificadora para canalizar energia y golpear en melee.":
                r = "Purifier staff used to channel energy and strike in melee.";
                break;
            case "Mandoble pesado del caballero, potente en combate frontal.":
                r = "Heavy knight greatsword, powerful in frontal combat.";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Tribe from the Vientohelado pass, these wild beings are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Strengthens Allies.\n-Its raven defends it.</color>\n<color=#EE0000>-Lowly resistant.</color>";
                break;
            case "Tiro Ballesta de Mano":
                r = "Hand Crossbow Shot";
                break;
            case "Acumular Energí­a":
                r = "Accumulate Energy";
                break;
            case "Acumulación Inestable":
                r = "Unstable Accumulation";
                break;
            case "Acechar":
                r = "Stalk";
                break;
            case "Arrojar Abrojos":
                r = "Throw Caltrops";
                break;
            case "Asesinar":
                r = "Assassinate";
                break;
            case "Bomba de Humo":
                r = "Smoke Bomb";
                break;
            case "Corte Daga":
                r = "Dagger Slash";
                break;
            case "Corte de Espada Corta":
                r = "Short Sword Slash";
                break;
            case "Corte de Espada Corta Arcana":
                r = "Arcane Short Sword Slash";
                break;
            case "Corte de Espada Corta Consumevida":
                r = "Lifedrinker Short Sword Slash";
                break;
            case "Corte de Espada Corta Filonegro":
                r = "Blackedge Short Sword Slash";
                break;
            case "Corte Horizontal":
                r = "Horizontal Cut";
                break;
            case "Corte Incapacitante":
                r = "Incapacitating Slash";
                break;
            case "Corte Vertical":
                r = "Vertical Cut";
                break;
            case "Corte Vertical Congelado":
                r = "Frozen Vertical Cut";
                break;
            case "Corte Vertical Sagrado":
                r = "Sacred Vertical Cut";
                break;
            case "Corte Vertical Sediento":
                r = "Thirsting Vertical Cut";
                break;
            case "Descarga De Poder":
                r = "Power Discharge";
                break;
            case "Descarga Desintegradora":
                r = "Disintegrating Discharge";
                break;
            case "Disparo Envenenado":
                r = "Poisoned Shot";
                break;
            case "Distraer":
                r = "Distract";
                break;
            case "Eco Divino":
                r = "Divine Echo";
                break;
            case "Enmendar":
                r = "Mend";
                break;
            case "Escudo de Fe":
                r = "Shield of Faith";
                break;
            case "Fogata":
                r = "Campfire";
                break;
            case "Hacia Las Sombras":
                r = "Into the Shadows";
                break;
            case "Hoja de Energí­a":
                r = "Energy Blade";
                break;
            case "HombroConHombro":
                r = "Shoulder to Shoulder";
                break;
            case "Improvisar Flechas":
                r = "Improvise Arrows";
                break;
            case "Instatransporte":
                r = "Insta Transport";
                break;
            case "Llama Divina":
                r = "Divine Flame";
                break;
            case "Luz Cegadora":
                r = "Blinding Light";
                break;
            case "Marcar Presa":
                r = "Mark Prey";
                break;
            case "Presa Marcada":
                r = "Marked Prey";
                break;
            case "Partir":
                r = "Cleave";
                break;
            case "Pilares De Luz":
                r = "Pillars of Light";
                break;
            case "Primeros Auxilios":
                r = "First Aid";
                break;
            case "Purificación":
                r = "Purification";
                break;
            case "Ráfaga":
                r = "Barrage";
                break;
            case "Residuo Energetico":
                r = "Energy Residue";
                break;
            case "Salmo Purificador":
                r = "Purifying Psalm";
                break;
            case "Sigues Tú":
                r = "You Are Next";
                break;
            case "Sigues Tu":
                r = "You Are Next";
                break;
            case "Sifón Arcano":
                r = "Arcane Siphon";
                break;
            case "Tiro con Arco Acido":
                r = "Acid Bow Shot";
                break;
            case "Tiro con Arco Potente":
                r = "Powerful Bow Shot";
                break;
            case "Tiro con Arco Ralentizante":
                r = "Slowing Bow Shot";
                break;
            case "Vigilancia":
                r = "Vigilance";
                break;
            case "Abrojo":
                r = "Caltrop";
                break;
            case "Acid Bow Shot":
                r = "Acid Bow Shot";
                break;
            case "Oso Espectral":
                r = "Spectral Bear";
                break;
            case "<color=#8a5b32>perforante</color>":
                r = "<color=#8a5b32>piercing</color>";
                break;
            case "Determinación ":
                r = "Determination ";
                break;
            case "Barrera":
                r = "Barrier";
                break;
            case "Muerto":
                r = "Dead";
                break;
            case "reacciona con ":
                r = "reacts with ";
                break;
            case "gana 1 Fervor por matar con ":
                r = "gains 1 Fervor for killing with ";
                break;
            case "fue Desintegrado.":
                r = "was Disintegrated.";
                break;
            case "==== Ronda ":
                r = "==== Round ";
                break;
            case " comienza ====":
                r = " begins ====";
                break;
            case "No puedes intercambiar con enemigos.":
                r = "You can't swap with enemies.";
                break;
            case "-La Caravana ha sido emboscada por un ataque subterráneo.":
                r = "-The Caravan has been ambushed by an underground attack.";
                break;
            case "El viaje de la caravana ha comenzado.":
                r = "The caravan's journey has begun.";
                break;
            case "-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crí­tico. -20 Esperanza":
                r = "-The Cleric Retinue has perished, as the Black Breath has reached a critical level. -20 Hope";
                break;
            case " ahora Maneja un Nivel ":
                r = " now has Energy Level ";
                break;
            case " de Energí­a.":
                r = ".";
                break;
            case " de Valentí­a.":
                r = " Valour.";
                break;
            case "ARM":
                r = "ARM";
                break;
            case "RES":
                r = "RES";
                break;
            case "BAR":
                r = "BAR";
                break;
            case "MIT":
                r = "MIT";
                break;
            case "BON":
                r = "BON";
                break;
            case "Mandoble Filonegro":
                r = "Blacksharp Greatsword";
                break;
            case "Mandoble Caótico":
                r = "Chaotic Greatsword";
                break;
            case "Impacto Caótico":
                r = "Chaotic Impact";
                break;
            case "Guantelete de Llamas":
                r = "Flame Gauntlet";
                break;
            case "Guantelete Estrella":
                r = "Star Gauntlet";
                break;
            case "Grieta Arcana":
                r = "Arcane Rift";
                break;
            /*  case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comercial a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                 r = "This retinue is made up of several merchants who have had to abandon their shops, but have not given up on their merchandise. They are willing to trade at discounted prices but without giving up at least a minimal profit.";
                 break;*/
            /*  case "El Séquito de Curanderos se encarga de atender a los heridos y enfermos de la Caravana. Pese a las circunstancias del viaje mismo, logran mantenerse en funcionamiento y brindan un servicio escencial para la supervivencia de quienes lo necesiten.":
                 r = "The Healers' Retinue is responsible for attending to the wounded and sick of the Caravan. Despite the circumstances of the journey itself, they manage to remain operational and provide an essential service for the survival of those who need it.";
                 break;*/
            case "El séquito de Herreros se encarga del mantenimiento y manufactura de las armas y armaduras de la Caravana. Su carro es especialmente pesado ya que, montado ingeniosamente, carga con todas las necesidades básicas de un herrero.":
                r = "The Blacksmith Retinue handles weapon and armor maintenance for the Caravan. Their cart is heavy, but it carries all the essentials of a working forge.";
                break;
            /* case "Aumentar el tamaño de las tiendas incrementa la cantidad de objetos ofrecidos.":
                r = "Increasing the size of the shops increases the number of items offered.";
                break;*/
            case "Amuleto de Hueso Liso":
                r = "Sharp Bone Amulet";
                break;
            case "Amuleto de Segunda Piel":
                r = "Second Skin Amulet";
                break;
            case "Ancla de la Ultima Linea":
                r = "Last Line Anchor";
                break;
            case "Anillo de Destrucción":
                r = "Destruction Ring";
                break;
            case "Anillo de Filo Interno":
                r = "Inner Edge Ring";
                break;
            case "Anillo de Inteligencia":
                r = "Intelligence Ring";
                break;
            case "Anillo de Resistencia al Ácido":
                r = "Acid Resistance Ring";
                break;
            case "Anillo de Tormenta Quieta":
                r = "Calm Storm Ring";
                break;
            case "Anillo del Vigia":
                r = "Warden's Ring";
                break;
            case "Broche Runico de Bronce":
                r = "Bronze Runic Brooch";
                break;
            case "Cinta de Enfoque":
                r = "Focus Ribbon";
                break;
            case "Collar de Mente Clara":
                r = "Clear Mind Collar";
                break;
            case "Corazon de Bastion":
                r = "Bastion Heart";
                break;
            case "Corazon de Tormenta Primigenia":
                r = "Primordial Storm Heart";
                break;
            case "Corona del Eclipse":
                r = "Eclipse Crown";
                break;
            case "Estandarte del Baluarte Inmortal":
                r = "Immortal Bastion Banner";
                break;
            case "Insignia del Duelista":
                r = "Duelist's Insignia";
                break;
            case "Juramento del Inquebrantable":
                r = "Oath of the Unbreakable";
                break;
            case "Medalla de Guardia":
                r = "Guardian's Medal";
                break;
            case "Nucleo del Eclipse Arcano":
                r = "Arcane Eclipse Core";
                break;
            case "Ojo del Acecho":
                r = "Stalkers Eye";
                break;
            case "Ojo del Veredicto":
                r = "Verdict Eye";
                break;
            case "Piedra de Sangre Calma":
                r = "Calm Bloodstone";
                break;
            case "Reliquia del Trono Vacio":
                r = "Empty Throne Relic";
                break;
            case "Reliquia del Umbral Arcano":
                r = "Arcane Threshold Relic";
                break;
            case "Rosario del Alba":
                r = "Dawn Locket";
                break;
            case "Sello de Sangre Fria":
                r = "Cold Blood Seal";
                break;
            case "Sello del Caminante":
                r = "Walker's Seal";
                break;
            case "Sello del Rastreador Nocturno":
                r = "Nocturnal Tracker's Seal";
                break;
            case "Talisman de Escarcha Viva":
                r = "Living Frost Talisman";
                break;
            case "Talisman del Muro Espinado":
                r = "Thorn Wall Talisman";
                break;
            case "Cristal de Escarcha Fracta":
                r = "Fractured Frost Crystal";
                break;
            case "Escarcha Cortante":
                r = "Cutting Frost";
                break;
            case "Extraños en el camino":
                r = "Strangers on the Path";
                break;
            case "Séquito":
                r = "Retinue";
                break;
            case "Personaje":
                r = "Character";
                break;
            case "Elige una Clase":
                r = "Choose a Class";
                break;
            case "Dí­a":
                r = "Day";
                break;
            case "-La caravana ha llegado a un nodo incendiado. -10 Esperanza.  ":
                r = "The caravan has arrived at a burned node. -10 Hope.";
                break;
            case " Civiles Muertos.":
                r = " Civilians Dead.";
                break;
            // Missing item names from ItemDatabase
            case "Aceite de Tormenta":
                r = "Storm Oil";
                break;
            case "Ampolla Aislante":
                r = "Insulating Vial";
                break;
            case "Arco de Explorador":
                r = "Explorer's Bow";
                break;
            case "Arco Largo Potente":
                r = "Powerful Longbow";
                break;
            case "Arco Largo Ralentizante":
                r = "Slowing Longbow";
                break;
            case "Arco Largo Ácido":
                r = "Acid Longbow";
                break;
            case "Armadura de Cuero Borrosa":
                r = "Blurred Leather Armor";
                break;
            case "Armadura de Cuero de Fortaleza":
                r = "Fortitude Leather Armor";
                break;
            case "Armadura de Cuero del Cazador Gris":
                r = "Gray Hunter's Leather Armor";
                break;
            case "Armadura de Cuero del Horizonte":
                r = "Horizon Leather Armor";
                break;
            case "Armadura de Cuero del Rastreador":
                r = "Tracker's Leather Armor";
                break;
            case "Armadura de Cuero necrótico":
                r = "Necrotic Leather Armor";
                break;
            case "Armadura de Cuero Reforzado de Ligereza":
                r = "Reinforced Light Leather Armor";
                break;
            case "Armadura de Cuero Reforzado de Protección Elemental":
                r = "Elemental Protection Reinforced Leather Armor";
                break;
            case "Armadura de Cuero Reforzado de Velo":
                r = "Reinforced Veil Leather Armor";
                break;
            case "Armadura de Cuero Sombria":
                r = "Shadow Leather Armor";
                break;
            case "Armadura de Cuero Veloz":
                r = "Swift Leather Armor";
                break;
            case "Armadura Pesada de Caballero":
                r = "Knight Heavy Armor";
                break;
            case "Armadura Reforzada de Niebla":
                r = "Mist Reinforced Armor";
                break;
            case "Armadura Reforzada del Acecho":
                r = "Stalker Reinforced Armor";
                break;
            case "Armadura Reforzada del Verdugo":
                r = "Executioner Reinforced Armor";
                break;
            case "Armadura Reforzada Filo Umbrio":
                r = "Umbral Edge Reinforced Armor";
                break;
            case "Armadura Reforzada Ojo Nocturno":
                r = "Night Eye Reinforced Armor";
                break;
            case "Baculo Purificador":
                r = "Purifier Staff";
                break;
            case "Bastón de Purificadora":
                r = "Purifier's Staff";
                break;
            case "Brebaje Vampirico":
                r = "Vampiric Brew";
                break;
            case "Bálsamo de Resistencia":
                r = "Fortifying Balm";
                break;
            case "Coraza de Fuerza de Gigante":
                r = "Giant Strength Cuirass";
                break;
            case "Coraza de Guardia Roja":
                r = "Red Guard Cuirass";
                break;
            case "Coraza de Llamas":
                r = "Flame Cuirass";
                break;
            case "Coraza del Baluarte":
                r = "Bastion Cuirass";
                break;
            case "Coraza del Juramento":
                r = "Oath Cuirass";
                break;
            case "Coraza del Sol de Hierro":
                r = "Iron Sun Cuirass";
                break;
            case "Coraza Muralla Eterna":
                r = "Eternal Wall Cuirass";
                break;
            case "Elixir de Reflejos":
                r = "Reflex Elixir";
                break;
            case "Esencia del Bastion Antiguo":
                r = "Ancient Bastion Essence";
                break;
            case "Espada Corta Arcana":
                r = "Arcane Shortsword";
                break;
            case "Espada Corta de Acechador":
                r = "Stalker Shortsword";
                break;
            case "Espada Corta Filonegro":
                r = "Blackedge Shortsword";
                break;
            case "Extracto Corrosivo":
                r = "Corrosive Extract";
                break;
            case "Filtro Antidoto":
                r = "Antidote Filter";
                break;
            case "Frasco de Corteza":
                r = "Bark Flask";
                break;
            case "Guantelete de Poder":
                r = "Power Gauntlet";
                break;
            case "Infusion de Claridad":
                r = "Clarity Infusion";
                break;
            case "Jarabe del Acechador":
                r = "Stalker's Syrup";
                break;
            case "Licor de Fortaleza":
                r = "Fortitude Liquor";
                break;
            case "Mandoble Congelado":
                r = "Frozen Greatsword";
                break;
            case "Mandoble De Caballero":
                r = "Knight Greatsword";
                break;
            case "Mandoble Sagrado":
                r = "Sacred Greatsword";
                break;
            case "Mandoble Sediento":
                r = "Thirsting Greatsword";
                break;
            case "Polvora Catalitica":
                r = "Catalytic Powder";
                break;
            case "Reliquia de Segundo Aliento":
                r = "Second Wind Relic";
                break;
            case "Resina del Armero":
                r = "Armorer's Resin";
                break;
            case "Sello de Ceniza Negra":
                r = "Black Ash Seal";
                break;
            case "Sí­mbolo de Proteccion Arcano":
                r = "Arcane Protection Symbol";
                break;
            case "Solucion Neutralizante":
                r = "Neutralizing Solution";
                break;
            case "Tinta de Condena":
                r = "Condemnation Ink";
                break;
            case "Tonico Vital del Campamento":
                r = "Camp Vital Tonic";
                break;
            case "Unguento de Guardia":
                r = "Guard Ointment";
                break;
            case "Vela Arcana Bendita":
                r = "Blessed Arcane Candle";
                break;
            case "Vestidura Purificadora de Credo":
                r = "Purifying Vestment of Creed";
                break;
            case "Vestidura Purificadora de Guardia":
                r = "Purifying Vestment of Guard";
                break;
            case "Vestidura Purificadora de Lumen":
                r = "Purifying Vestment of Lumen";
                break;
            case "Vestidura Purificadora del Alba":
                r = "Purifying Vestment of Dawn";
                break;
            case "Vestidura Purificadora del Santuario":
                r = "Purifying Vestment of Sanctuary";
                break;
            // Missing buffs/debuffs from ItemDatabase
            case "Aislamiento Electrico":
                r = "Electric Insulation";
                break;
            case "Balsamo de Claridad":
                r = "Clarity Balm";
                break;
            case "Balsamo Energizante":
                r = "Energizing Balm";
                break;
            case "Balsamo Fortalecedor":
                r = "Fortifying Balm";
                break;
            case "Bastion Ancestral":
                r = "Ancestral Bastion";
                break;
            case "Bendicion Arcana":
                r = "Arcane Blessing";
                break;
            case "Catalisis Ignea":
                r = "Igneous Catalysis";
                break;
            case "Ceniza Vigilante":
                r = "Vigilant Ash";
                break;
            case "Claridad Serena":
                r = "Serene Clarity";
                break;
            case "Condena Marcada":
                r = "Marked Condemnation";
                break;
            case "Corrosion Activa":
                r = "Active Corrosion";
                break;
            case "Corteza Viva":
                r = "Living Bark";
                break;
            case "Efecto de consumible":
                r = "Consumable Effect";
                break;
            case "Elixir de Resistencia al Acido":
                r = "Acid Resistance Elixir";
                break;
            case "Elixir de Resistencia al Frio":
                r = "Cold Resistance Elixir";
                break;
            case "Fortaleza Liquida":
                r = "Liquid Fortitude";
                break;
            case "Guardia Ungida":
                r = "Anointed Guard";
                break;
            case "Hambre Carmesi":
                r = "Crimson Hunger";
                break;
            case "Instinto de Caza":
                r = "Hunting Instinct";
                break;
            case "Piel Neutralizada":
                r = "Neutralized Skin";
                break;
            case "Proteccion Arcana":
                r = "Arcane Protection";
                break;
            case "Reflejos Afilados":
                r = "Sharpened Reflexes";
                break;
            case "Resina Defensiva":
                r = "Defensive Resin";
                break;
            case "Segundo Aliento":
                r = "Second Wind";
                break;
            case "Tormenta Cargada":
                r = "Charged Storm";
                break;
            case "Filoacero":
                r = "Steel Edge";
                break;
            case "La caravana ha sido destruida y todos sus miembros han muerto. El Aliento Negro es implacable.":
                r = "The caravan has been destroyed and all its members have died. The Black Breath is relentless.";
                break;
            case "Valor:":
                r = "Valour:";
                break;
            case "Valor":
                r = "Valour";
                break;
            case "Encarnar":
                r = "Incarnate";
                break;
            case "Pasivas":
                r = "Passives";
                break;
            case "-Campaña guardada.":
                r = "-Campaign saved.";
                break;
            case "-No se pudo guardar la campaña. ":
                r = "-Failed to save the campaign. ";
                break;
            case "Si sales de la partida se perderán todos los cambios no guardados. ¿Continuar?":
                r = "If you leave the game, all unsaved changes will be lost. Continue?";
                break;
            case "Cancelar":
                r = "Cancel";
                break;
            case "Reproducir música al minimizar":
                r = "Play music when minimized";
                break;
            case "Este séquito está constituí­do por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                r = "This retinue is composed of several merchants who have had to abandon their shops, but who have not given up their merchandise. They are willing to trade at reduced prices but without giving up at least a minimal profit.";
                break;
            case "El Espectro acaba de atacar, haciéndolo vulnerable en el plano material.":
                r = "The Specter has just attacked, making it vulnerable in the material plane.";
                break;
            case "Echar.":
                r = "Expel.";
                break;
            case "La caravana no tiene más tiendas para otro personaje.":
                r = "The caravan has no more tents for another character.";
                break;
            case "La caravana llegado a un pequeño asentamiento aislado en el camino. Parece que los lugareños ignoran que el <b>Aliento Negro</b> se aproxima o carecen de un líder quien pueda guiarlos lejos de la inminente catástrofe.":
                r = "The caravan has arrived at a small isolated settlement on the road. It seems that the locals are unaware that the <b>Black Breath</b> is approaching or lack a leader who could guide them away from the impending catastrophe.";
                break;
            case "Teniendo el cuenta que el tiempo apremia, analizas tus opciones:":
                r = "Considering that time is of the essence, you analyze your options:";
                break;
            case "Asentamiento Destruído":
                r = "Destroyed Settlement";
                break;
            case "Este asentamiento ha sido consumido por el Aliento Negro y los únicos moradores aquí son los Corrompidos.  Prepárate a luchar.":
            r = "This settlement has been consumed by the Black Breath and the only inhabitants here are the Corrupted. Prepare to fight.";
            break;
             



























































        }


        return r;
    }


    string TraducirPortugues(string txt, bool esBotonFijo = false)
    {
       
        string r = txt;
        if (!esBotonFijo)
        {
            r = txt;
        }

        // Tabla PT en preparacion: por ahora conserva el texto base hasta cargar cada caso.
        switch (txt)
        {
           case "Retraso Nocturno":
                r = "Atraso Noturno";
                break;
            case "Desapariciones Misteriosas":
                r = "Desaparecimentos Misteriosos";
                break;
            case "Bueyes Enfermos":
                r = "Bois Doentes";
                break;
            case "Peaje Criminal":
                r = "Pedágio Criminoso";
                break;
            case "Personaje Enfermo":
                r = "Personagem Doente";
                break;
            case "Arcas Robadas":
                r = "Baús Roubados";
                break;
            case "Carro Deteriorado":
                r = "Carroça Danificada";
                break;
            case "Liderazgo Cuestionado":
                r = "Liderança Questionada";
                break;
            case "Destello Esperanzador":
                r = "Vislumbre de Esperança";
                break;
            case "Risotadas en la Caravana":
                r = "Risadas na Caravana";
                break;
            case "Caravana Perdida":
                r = "Caravana Perdida";
                break;
            case "Aserradero Abandonado":
                r = "Serraria Abandonada";
                break;
            case "Manada de Bueyes":
                r = "Manada de Bois";
                break;
            case "Civiles en Apuros":
                r = "Civis em Apuros";
                break;
            case "Tranquilidad":
                r = "Tranquilidade";
                break;
            case "Voto de Confianza":
                r = "Voto de Confiança";
                break;
            case "Claro":
                r = "Clareira";
                break;
            case "Asentamiento.":
                r = "Assentamento";
                break;
            case "Recursos":
                r = "Recursos";
                break;
            case "Continuar":
                r = "Continuar";
                break;
            case "Revisarlos":
                r = "Inspecioná-los";
                break;
            case "Ignorar":
                r = "Ignorar";
                break;
            case "Pagar":
                r = "Pagar";
                break;
            case "No pagar":
                r = "Nío pagar";
                break;
            case "Interrogar":
                r = "Interrogar";
                break;
            case "No interrogar":
                r = "Nío interrogar";
                break;
            case "Aceptar":
                r = "Aceitar";
                break;
            case "No aceptar":
                r = "Nío aceitar";
                break;
            case "Negarse":
                r = "Recusar";
                break;
            case "Dejarlos":
                r = "Deixá-los";
                break;
            case "Discurso":
                r = "Discurso";
                break;
            case "Golpear":
                r = "Golpear";
                break;
            case "Saquear":
                r = "Saquear";
                break;
            case "Honrar":
                r = "Honrar";
                break;
            case "Todo":
                r = "Tudo";
                break;
            case "Un poco":
                r = "Um pouco";
                break;
            case "Cazarlos":
                r = "Caçá-los";
                break;
            case "Domesticarlos":
                r = "Domesticá-los";
                break;
            case "Rechazar":
                r = "Recusar";
                break;
            case "Atajo":
                r = "Atalho";
                break;
            case "Area":
                r = "Área";
                break;
            // EventosAdmin remaining literals (exact text keys)

            case "Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n":
                r = "Um dos principais responsáveis por guiar a caravana e escolher as rotas mais seguras perdeu seus mapas por acidente.\n"; break;
            case "Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podráa perderse.\n\n\n\n\n\n\n":
                r = "Os outros responsáveis vío ajudá-lo a procurá-los, já que esses mapas contêm informações cruciais sobre a área atual, e sem eles a caravana pode se perder.\n\n\n\n\n\n\n"; break;
            case "Obtendrá el estado Enfermo por 4-7 dí­as. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 dí­a.\n\n\n\n\n":
                r = "Receberá o estado Doente por 4-7 dias. Cada ní­vel do Séquito de Curandeiros reduzirá o tempo de recuperaçío em 1 dia.\n\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un dí­a extra.</color>\n\n":
                r = "<color=#ba3fef>-Você pode comprar remédio por 45 de Ouro para reduzir a Doença em 1 dia extra.</color>\n\n"; break;
            case "Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botón.\n\n":
                r = "Ao grito de um guarda, sua atençío se volta para uma das carroças que leva os baús com o ouro da caravana. Um de seus cofres está virado, e o ouro se espalhou pelo chío. Aparentemente, durante a noite, alguém conseguiu arrombá-lo e levou parte do saque.\n\n"; break;
            case "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perderí­a 5 de Esperanza, <i>":
                r = "<color=#ba3fef>-Você pode submeter os Civis a um interrogatório para tentar encontrar o ladrío:\n\n Você perderia 5 de Esperança, <i>"; break;
            case "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n":
                r = "% de chance (40 base + Milicianos)</i> de encontrar o culpado e recuperar o ouro, -1 Civil por banimento.</color>\n\n"; break;
            case "Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n":
                r = "Após um estrondo, você vira a cabeça para trás e vê que uma das carroças de suprimentos da caravana sofreu um acidente. As rodas estío atoladas na lama, e a carroça parece estar perdida para sempre.\n\n"; break;
            case "<color=#ba3fef>-Puedes pasar los 60 suministros caí­dos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n":
                r = "<color=#ba3fef>-Você pode passar os 60 suprimentos caí­dos para outra carroça, sacrificando 20 Materiais; ou assumir a perda dos suprimentos.</color>\n\n"; break;
            case "La Caravana encuentra un rí­o con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n":
                r = "A Caravana encontra um rio com bom fluxo e água que parece aceitável. Vários civis, animados, começam a ir até lá com a intençío de descansar e se refrescar.\n\n"; break;
            case "El agua podráa estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n":
                r = "A água pode estar contaminada pelo Alento Negro. Você pode negar aos Civis o acesso á água ou deixá-los á própria sorte.\n\n"; break;
            case "<color=#ba3fef>-Si les niegas el acceso perderás 15 de Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se negar o acesso, você perderá 15 de Esperança.</color>\n\n"; break;
            case "<color=#ba3fef>-Si los dejas ir, hay un %":
                r = "<color=#ba3fef>-Se você deixá-los ir, há %"; break;
            case "<i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no está contaminada descansarán (-1 Fatiga).</color>\n\n":
                r = " <i>(Determinado pelo Alento Negro)</i> de chance de que se contaminem e 25 Civis morram. Se nío estiver contaminada, eles descansarío (-1 Fadiga).</color>\n\n"; break;
            case "\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n":
                r = "\nAparentemente, eles tiveram um incidente durante um treino leve que decidiram fazer, no qual ambos se machucaram levemente.\n\n"; break;
            case "La tensión sube y los demás caravaneros miran con incomodidad. Ambos reclaman tener la razón y esperan tu juicio.\n\n":
                r = "A tensío aumenta, e os demais integrantes da caravana observam com desconforto. Ambos afirmam estar certos e aguardam seu julgamento.\n\n"; break;
            case "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 dí­as. Apoyas a:</color>\n\n":
                r = "<color=#ba3fef>-Você deve intervir em apoio a um dos dois. O outro receberá Moral Baixa por 5 dias. Você apoia:</color>\n\n"; break;
            case "Un Civil de origen noble se acerca a ti con altanerí­a y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podráa hacerlo mejor.\n":
                r = "Um Civil de origem nobre se aproxima de você com arrogância e começa a questionar sua liderança, argumentando que você nío está tomando as decisões corretas para o bem-estar da Caravana e que ele mesmo poderia fazer melhor.\n"; break;
            case "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n":
                r = "Embora seus argumentos sejam pouco coerentes, á medida que ele fala em voz alta, vários civis começam a se reunir ao redor, curiosos.\n\n"; break;
            case "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n":
                r = "<color=#ba3fef>-Golpeá-lo.</color> Sua famí­lia abandona a Caravana, retirando seu investimento. -65 Ouro -8 Civis -10 Esperança\n\n"; break;
            case "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del puerto.\n":
                r = "Durante a noite, os civis reunidos avistam um clarío de luz ní­tida e bela no horizonte, na direçío do porto.\n"; break;
            case "Quizás sea una señal, quizás casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n":
                r = "Talvez seja um sinal, talvez uma coincidência, mas os civis agora parecem mais otimistas, embora ainda reste um longo caminho.\n\n\n\n\n\n\n"; break;
            case "La atmásfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n":
                r = "A atmosfera fica mais leve e otimista, e por um breve instante o peso da situaçío parece desaparecer.\n\n\n\n"; break;
            case "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>":
                r = "<color=#a0e812><b>+5 Esperança</b>\n\n</color>"; break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadoveres civiles. Una lucha tuvo lugar aquí­ y esta caravana no sobrevivió.\n":
                r = "Ao avançar pelo caminho, você encontra várias carroças destruí­das rodeadas por cadáveres de civis. Uma luta aconteceu aqui, e esta caravana nío sobreviveu.\n"; break;
            case "Si bien la situación es sombrí­a, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n":
                r = "Embora a situaçío seja sombria, vários suprimentos em bom estado nío foram saqueados, permanecendo á beira da estrada.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n":
                r = "<color=#ba3fef>-Você pode enterrar os Civis e honrar sua memória, sem saqueá-los.</color> +15 Esperança \n\n"; break;
            case "La Caravana se detiene en un aserradero abandonado, algunos Ç­rboles han sido talados y la madera estÇ­ apilada en desorden.\n":
                r = "A Caravana para em uma serraria abandonada; algumas árvores foram derrubadas e a madeira está empilhada em desordem.\n"; break;
            case "Hay suficiente madera como para llenar un par de carros, pero juntarla toda cansará a los Civiles que participen y llevará algunas horas.\n\n\n\n":
                r = "Há madeira suficiente para encher um par de carroças, mas juntar tudo vai cansar os Civis que participarem e levará algumas horas.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes juntar solo lo que está a mano y continuar sin retraso.</color> +15-26 Materiales \n\n":
                r = "<color=#ba3fef>-Você pode recolher apenas o que está á mío e seguir sem atraso.</color> +15-26 Materiais \n\n"; break;
            case "La Caravana se detiene en un claro donde pasta una manada de bueyes. Los animales parecen sanos y bien alimentados, pero están asustados por la presencia de la Caravana.\n":
                r = "A Caravana para em uma clareira onde uma manada de bois está pastando. Os animais parecem saudáveis e bem alimentados, mas estío assustados com a presença da Caravana.\n"; break;
            case "La Caravana se detiene al escuchar gritos de auxilio provenientes de un lado del camino. Al investigar encuentras a un puñado de Civiles escapando de una banda de bandidos en dirección a la Caravana.\n":
                r = "A Caravana para ao ouvir gritos de socorro vindos da lateral do caminho. Ao investigar, você encontra um punhado de Civis fugindo de uma quadrilha de bandidos em direçío á Caravana.\n"; break;
            case "'Son bandidos! no pudimos ver cuántos, pero se acercan.' - Dice un Civil aterrorizado. 'Ayúdanos'\n\n":
                r = "'Sío bandidos! Nío conseguimos ver quantos, mas estío se aproximando.' - diz um Civil aterrorizado. 'Ajude-nos'\n\n"; break;
            case "En un momento repentino, te das cuenta que hay mucha paz. Se escuchan los pasos constantes de la caravana, algún murmullo, risa y la naturaleza alrededor.\n":
                r = "Em um momento repentino, você percebe que há muita paz. Ouvem-se os passos constantes da caravana, alguns murmúrios, risos e a natureza ao redor.\n"; break;
            case "Estos momentos son muy escasos y sientes que cada individuo de la caravana lo valoró a su manera. \nDe alguna forma, el aire se siente más limpio.\n\n":
                r = "Esses momentos sío muito raros, e você sente que cada indiví­duo da caravana os valorizou á sua maneira. \nDe alguma forma, o ar parece mais limpo.\n\n"; break;
            // EventosAdmin extra keys and segments
            case "<b>Oro Robado:  ":
                r = "<b>Ouro Roubado:  "; break;
            case "\n\n</b>":
                r = "\n\n</b>"; break;
            case "<color=#ba3fef>-Luchar con los Bandidos.</color>\n\n":
                r = "<color=#ba3fef>-Lutar contra os Bandidos.</color>\n\n"; break;
            case "<color=#ba3fef>-2 al Avance del Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-2 no Avanço do Alento Negro.</color>\n\n"; break;
            // Logs (segments for concatenation)
            case "-Has encontrado al ladrón y recuperado el oro robado, pero has tenido que desterrar al ladrón. -5 Esperanza -1 Civil.":
                r = "-Você encontrou o ladrío e recuperou o ouro roubado, mas teve de bani-lo. -5 Esperança -1 Civil."; break;
            case "-No has logrado encontrar al ladrón y se perdieron ":
                r = "-Você nío conseguiu encontrar o ladrío e perdeu "; break;
            case " de oro.":
                r = " de ouro."; break;
            case "-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza":
                r = "-Você fez um discurso motivador e rebateu os argumentos do Nobre. +15 Esperança"; break;
            case "-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza.":
                r = "-Você fez um discurso pouco convincente, que gerou mais dúvidas do que certezas. -20 de Esperança."; break;
            case "-La cacerí­a de ":
                r = "-A caçada de "; break;
            case " ha sido exitosa. +":
                r = " foi bem-sucedida. +"; break;
            case " Suministros +55 Experiencia.":
                r = " Suprimentos +55 Experiência."; break;
            case " sufrió un accidente durante la cacerí­a. Herido.":
                r = " sofreu um acidente durante a caçada. Ferido."; break;
            case "-Los Civiles se han contaminado y han muerto ":
                r = "-Os Civis foram contaminados e morreram "; break;
            case " Civiles. -10 Esperanza":
                r = " Civis. -10 Esperança"; break;
            case "-Los Civiles han descansado en el rí­o y se han refrescado. -1 Fatiga ":
                r = "-Os Civis descansaram no rio e se refrescaram. -1 Fadiga "; break;
            // Riña description segments
            case "Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>":
                r = "Você ouve um alvoroço nas proximidades das carroças dos Heróis. Ao se aproximar para investigar, vê <b><color=#d1006f>"; break;
            case "</color></b> y <b><color=#d1006f>":
                r = "</color></b> e <b><color=#d1006f>"; break;
            case "</color></b> discutiendo acaloradamente.":
                r = "</color></b> discutindo acaloradamente."; break;
            case "Rí­o Contaminado":
                r = "Rio Contaminado";
                break;
            case "Riña":
                r = "Briga";
                break;
            case "Lugareño Anciano ":
                r = "Anciío Local ";
                break;
            case "Sueño Inspirador":
                r = "Sonho Inspirador";
                break;
            case "<color=#a0e812><b>+15 Esperanza</b></color>":
                r = "<color=#a0e812><b>+15 Esperança</b></color>";
                break;
            case "<color=#ba3fef><b>Pasan las Horas: +1 Avance Aliento Negro</b></color>":
                r = "<color=#ba3fef><b>As Horas Passam: +1 Avanço do Alento Negro</b></color>";
                break;
            case "\n<b><color=#d1006f>":
                r = "\n<b><color=#d1006f>";
                break;
            case "</color></b> cree que puede cazar algunos de estos Bueyes para obtener comida.  Chances: %":
                r = "</color></b> acredita que pode caçar alguns desses Bois para obter comida.  Chance: %";
                break;
            case " <i>(Determinado por Nivel)  Exito: +50-80 Suministros +55 Experiencia.  Fallo: Recibe Herida.</i>\n\n\n\n":
                r = " <i>(Determinado pelo Ní­vel)  Sucesso: +50-80 Suprimentos +55 Experiência.  Falha: Recebe Ferida.</i>\n\n\n\n";
                break;
            case "Caballero":
                r = "Cavaleiro";
                break;
            case "Explorador":
                r = "Explorador";
                break;
            case "Purificadora":
                r = "Purificadora";
                break;
            case "Acechador":
                r = "Espreitador";
                break;
            case "Canalizador":
                r = "Canalizador";
                break;
            case "Ronda":
                r = "Rodada";
                break;
            case "Clima normal.":
                r = "Clima normal."; break;
            case "Calor: todas las unidades obtienen 'Acalorado'.":
                r = "Calor: todas as unidades recebem 'Acalorado'."; break;
            case "Lluvia: todas las unidades obtienen 'Mojado'.":
                r = "Chuva: todas as unidades recebem 'Molhado'."; break;
            case "Nieve: todas las unidades obtienen 'Frí­o'.":
                r = "Neve: todas as unidades recebem 'Frio'."; break;
            case "Niebla: -1 Ataque a habilidades de rango.":
                r = "Névoa: -1 de Ataque para habilidades de alcance."; break;
            case "<color=#c5c5c5>cortante</color>":
                r = "<color=#c5c5c5>cortante</color>"; break; //Cortante
            case "<color=#c69360>perforante</color>":
                r = "<color=#c69360>perfurante</color>"; break; //Perforante
            case "<color=#c67f60>contundente</color>":
                r = "<color=#c67f60>contundente</color>"; break; //Contundente
            case "<color=#ce3715>fuego</color>":
                r = "<color=#ce3715>fogo</color>"; break; //Fuego
            case "<color=#63c4b7>hielo</color>":
                r = "<color=#63c4b7>gelo</color>"; break; //Hielo
            case "<color=#7758df>rayo</color>":
                r = "<color=#7758df>raio</color>"; break; //Rayo
            case "<color=#28b717>Ácido</color>":
                r = "<color=#28b717>ácido</color>"; break; //Acido
            case "<color=#1760b7>arcano</color>":
                r = "<color=#1760b7>arcano</color>"; break; //Arcano
            case "<color=#8038b2>necrótico</color>":
                r = "<color=#8038b2>necrótico</color>"; break; //Necro
            case "<color=#d6c304>verdadero</color>":
                r = "<color=#d6c304>verdadeiro</color>"; break; //Verdadero
            case "<color=#d6c304>divino</color>":
                r = "<color=#d6c304>divino</color>"; break; //Divino
            case "Has llegado a un improvisado Puesto Comercial, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\n\nTu Séquito de Mercaderes ha actualizado su Inventario.":
                r = "Você chegou a um Posto Comercial improvisado, onde oferecem Suprimentos básicos de sobrevivência aos viajantes.\nO Ní­vel do seu Séquito de Mercadores ajudará a reduzir os preços.\n\n\nSeu Séquito de Mercadores atualizou o Inventário.";
                break;
            case "El Séquito de Mercaderes ha actualizado su inventario en el Puesto Comercial.":
                r = "O Séquito de Mercadores atualizou seu inventário no Posto Comercial.";
                break;
            case "Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n\n. ":
                r = "Você chegou a um Santuário dos Purificadores; vários foram construí­dos na regiío para oferecer apoio e preces aos valentes que combateram o Lich.\nHoje, embora esteja abandonado, ele mantém sua aura de tranquilidade, e você pode depositar oferendas para realizar uma prece de purificaçío.\n\n\n. ";
                break;
            case "-La caravana ha llegado a un Santuario de Purificadores. Los personajes se han curado un 15%. +10 Esperanza.":
                r = "A caravana chegou a um Santuário dos Purificadores. Os personagens se curaram em 15%. +10 Esperança.";
                break;
            case "-Como Purificadora,":
                r = $"-Como Purificadora,";
                break;
            case " gana 60 Experiencia por la visita al santuario.":
                r = " recebe 60 de Experiência pela visita ao santuário.";
                break;
            case "<color=#8708a4><b>                  El Aliento Negro</b></color>\n\n\n":
                r = "<color=#8708a4><b>                  O Alento Negro</b></color>\n\n\n";
                break;
            case "<color=#ebdeef>Al morir el Liche, liberó un último estertor de muerte y putrefacción que se expande por cientos de kilómetros alrededor.</color>":
                r = "<color=#ebdeef>Ao morrer, o Lich liberou um último estertor de morte e putrefaçío que se espalha por centenas de quilômetros ao redor.</color>";
                break;
            case "\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente está envolviendo a los seres vivos que no logran escapar, provocándoles la muerte, o peor. </color>\n\n\n\n":
                r = "\n\nChamado de Alento Negro, essa onda de peste e podridío está lentamente envolvendo os seres vivos que nío conseguem escapar, causando-lhes a morte, ou algo pior. </color>\n\n\n\n";
                break;
            case "<color=#bae895><b>Estado: Distante</b> (":
                r = "<color=#bae895><b>Estado: Distante</b> (";
                break;
            case "<color=#c8a6e8><b>Estado: Cerca</b> (":
                r = "<color=#c8a6e8><b>Estado: Próximo</b> (";
                break;
            case "<color=#aa66ea><b>Estado: Dentro</b> (":
                r = "<color=#aa66ea><b>Estado: Dentro</b> (";
                break;
            case "<color=#7a1dd1><b>Estado: Nocivo</b> (":
                r = "<color=#7a1dd1><b>Estado: Nocivo</b> (";
                break;
            case "/20) - La Caravana viaja con tranquilidad.</color>":
                r = "/20) - A Caravana viaja em tranquilidade.</color>";
                break;
            case "/20) - La Caravana comienza a preocuparse y la podredumbre se siente en el aire. Los Corrompidos acechan en las sombras.</color>":
                r = "/20) - A Caravana começa a se preocupar, e a podridío já é sentida no ar. Os Corrompidos espreitam nas sombras.</color>";
                break;
            case "/20) - La Caravana ya es directamente afectada por el hedor. Los Corrompidos se dejan ver.</color>":
                r = "/20) - A Caravana já é diretamente afetada pelo fedor. Os Corrompidos se deixam ver.</color>";
                break;
            case "/20) - La peste comienza a tomar vidas civiles. Los Corrompidos son implacables.</color>":
                r = "/20) - A peste começa a ceifar vidas civis. Os Corrompidos sío implacáveis.</color>";
                break;
            case "Enérgicos(0)":
                r = "Enérgicos(0)";
                break;
            case "Descansados(1)":
                r = "Descansados(1)";
                break;
            case "Frescos(2)":
                r = "Dispostos(2)";
                break;
            case "En Marcha(3)":
                r = "Em Marcha(3)";
                break;
            case "Agitados(4)":
                r = "Agitados(4)";
                break;
            case "Cansados(5)":
                r = "Cansados(5)";
                break;
			case "Exhaustos(6)":
                r = "Exaustos(6)";
                break;
            case "La <color=#a0e812>Esperanza</color> determina el optimismo de la Caravana en general sobre la posibilidad de cumplir la misión y llegar al puerto.\n\n":
                r = "A <color=#a0e812>Esperança</color> determina o otimismo geral da Caravana quanto á possibilidade de cumprir a missío e chegar ao porto.\n\n";
                break;
            case "/100 de <color=#a0e812>Esperanza</color>\n":
                r = "/100 de <color=#a0e812>Esperança</color>\n";
                break;
            case " <color=#982a1b>1-20 Civiles abandonarán la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-20 Civis abandonarío a Caravana a cada descanso.</color>\n";
                break;
            case " <color=#982a1b>1-10 Civiles abandonarán la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-10 Civis abandonarío a Caravana a cada descanso.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarán algo de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Os Civis doarío um pouco de Ouro a cada descanso.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarán buena cantidad de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Os Civis doarío uma boa quantidade de Ouro a cada descanso.</color>\n";
                break;
            case "Los <color=#c918bb>Civiles</color> que lleva la caravana hacia el Puerto. Salvar la mayor cantidad es el objetivo principal de esta misión.\n\nCada uno consume 1 de <color=#b7972c>Suministros</color> cada Descanso, y la cantidad de Civiles determina la eficiencia de las Tareas Civiles.\n":
                r = "Os <color=#c918bb>Civis</color> que a caravana leva em direçío ao Porto. Salvar o maior número possí­vel é o objetivo principal desta missío.\n\nCada um consome 1 de <color=#b7972c>Suprimentos</color> a cada Descanso, e a quantidade de Civis determina a eficiência das Tarefas Civis.\n";
                break;
            case "\nLlevas ":
                r = "\nVocê leva ";
                break;
            case " <color=#c918bb>Civiles</color> en la caravana.\n\n":
                r = " <color=#c918bb>Civis</color> na caravana.\n\n";
                break;
            case "\nLas fuerzas de la Milicia de la caravana son de <color=#a8a29c>":
                r = "\nA força da Milí­cia da caravana é de <color=#a8a29c>";
                break;
            case ", que equivalen a ":
                r = ", o que equivale a ";
                break;
            case "</color> Milicianos que ayudarán a defenderla de ataques directos.\n\n":
                r = "</color> Milicianos que ajudarío a defendê-la de ataques diretos.\n\n";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b7972c>5 Suministros</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Clique para abandonar <color=#b7972c>5 Suprimentos</color> e aliviar a Carga. -1 Esperança</b>---</color>\n\n";
                break;
            case "Los <color=#b7972c>Suministros</color> constituyen las reservas de comida y elementos de supervivencia de la caravana.\n\nCada <color=#c918bb>Civil</color> consume 1 en cada Descanso. Los Bueyes consumen 2.\n":
                r = "Os <color=#b7972c>Suprimentos</color> constituem as reservas de comida e itens de sobrevivência da caravana.\n\nCada <color=#c918bb>Civil</color> consome 1 a cada Descanso. Os Bois consomem 2.\n";
                break;
            case " <color=#b7972c>Suministros</color>, por un total de peso de ":
                r = " <color=#b7972c>Suprimentos</color>, com um peso total de ";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b34f09>2 Materiales</color> y alivianar la Carga.</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Clique para abandonar <color=#b34f09>2 Materiais</color> e aliviar a Carga.</b>---</color>\n\n";
                break;
            case "Los <color=#b34f09>Materiales</color> son elementos básicos de construcción utilizados para mantenimiento y expansión de la caravana.\nCada uno pesa 3.\n":
                r = "Os <color=#b34f09>Materiais</color> sío elementos básicos de construçío usados para manutençío e expansío da caravana.\nCada um pesa 3.\n";
                break;
            case " <color=#b34f09>Materiales</color>, por un total de peso de ":
                r = " <color=#b34f09>Materiais</color>, com um peso total de ";
                break;
            case "<color=#ffdda5>---<b>Haz click para sacrificar <color=#9e2a1c>1 Buey</color> para obtener <color=#b7972c>20 Suministros</color>. -2 Esperanza</b>---</color>\n\n":
                r = "<color=#ffdda5>---<b>Clique para sacrificar <color=#9e2a1c>1 Boi</color> para obter <color=#b7972c>20 Suprimentos</color>. -2 Esperança</b>---</color>\n\n";
                break;
            case "Los <color=#9e2a1c>Bueyes</color> son utilizados para llevar la carga de la caravana.\nCada uno da ":
                r = "Os <color=#9e2a1c>Bois</color> sío usados para carregar a carga da caravana.\nCada um fornece ";
                break;
            case " de Capacidad de Carga.\n":
                r = " de Capacidade de Carga.\n";
                break;
            case " <color=#9e2a1c>Bueyes</color>, por un total de Capacidad de Carga de ":
                r = " <color=#9e2a1c>Bois</color>, com uma Capacidade de Carga total de ";
                break;
            case " <color=#b7972c>Suministros</color> y ":
                r = " <color=#b7972c>Suprimentos</color> e ";
                break;
            case " <color=#b34f09>Materiales</color> por un total de peso de ":
                r = " <color=#b34f09>Materiais</color> com um peso total de ";
                break;
            case "<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo que se haga duplica la Fatiga obtenida y reduce 10 la <color=#a0e812>Esperanza</color></color>.\n\n":
                r = "<color=#cc0d0d>A Caravana está com Sobrecarga. Cada trecho percorrido duplica a Fadiga obtida e reduz em 10 a <color=#a0e812>Esperança</color></color>.\n\n";
                break;
            case "El <color=#d8a205>Oro</color> que lleva la Caravana, utilizado para comprar bienes y contratar servicios.":
                r = "O <color=#d8a205>Ouro</color> que a Caravana carrega, usado para comprar bens e contratar serviços.";
                break;
            case "Indica que tanta <color=#06c297>Fatiga</color> tiene la Caravana en general.\n":
                r = "Indica quanta <color=#06c297>Fadiga</color> a Caravana tem no geral.\n";
                break;
            case "Cada tramo de viaje la aumenta en 1.\n":
                r = "Cada trecho de viagem a aumenta em 1.\n";
                break;
            case "Si descansas volverá a 0 y arrancarán el nuevo dí­a Descansados(1).\n\n":
                r = "Se descansar, ela voltará a 0 e vocês começarío o novo dia Descansados(1).\n\n";
                break;
            case "Actualmente estan Descansados(1), no habrá penalizaciones por viajar.\n\n":
                r = "Atualmente estío <color=#a8ff9e>Descansados</color>(<color=#a8ff9e>1</color>), nío haverá penalidades por viajar.\n\n";
                break;
            case "Actualmente estan Frescos(2), no habrá penalizaciones por viajar.":
                r = "Atualmente estío <color=#d4ff9e>Dispostos</color>(<color=#d4ff9e>2</color>), nío haverá penalidades por viajar.";
                break;
            case "Actualmente estan En Marcha(3), no habrá penalizaciones por viajar.":
                r = "Atualmente estío <color=#fff79e>Em Marcha</color>(<color=#fff79e>3</color>), nío haverá penalidades por viajar.";
                break;
            case "Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podráan morir si viajas.":
                r = "Atualmente estío <color=#ffd19e>Agitados</color>(<color=#ffd19e>4</color>), -10 Esperança, e alguns poucos Bois podem morrer se você viajar.";
                break;
            case "Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas.":
                r = "Atualmente estío <color=#ff9e9e>Cansados</color>(<color=#ff9e9e>5</color>), -15 Esperança, e alguns Bois podem morrer se você viajar.";
                break;
            case "Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas.":
                r = "Atualmente estío <color=#ff3c3c>Exaustos</color>(<color=#ff3c3c>6</color>), -20 Esperança, e vários Bois podem morrer se você viajar.";
                break;
            case "Dí­a ":
                r = "Dia ";
                break;
            case "Soleado: +5 Esperanza.":
                r = "Ensolarado: +5 Esperança.";
                break;
            case "Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3.":
                r = "Onda de Calor: +1 Fadiga. \"Dia Livre\" concede +5 Esperança, outras Tarefas Civis concedem -3.";
                break;
            case "Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada.":
                r = "Chuva: -5 Esperança. -15% Coleta de Suprimentos, -20% chance de Emboscada.";
                break;
            case "Nieve: +3 Esperanza. -15% Recolecciones, -20% Emboscada. Viajar lleva el doble de tiempo.":
                r = "Neve: +3 Esperança. -15% Coletas, -20% Emboscada. Viajar leva o dobro do tempo.";
                break;
            case "Niebla: -20% Recolecciones, -20% Emboscada, -20% Exploración, +10% Nodos Misteriosos.":
                r = "Névoa: -20% Coletas, -20% Emboscada, -20% Exploraçío, +10% Nós Misteriosos.";
                break;
            case "De un momento a otro, varios miembros de la caravana han desaparecido sin dejar rastro. Nadie tiene una explicación de lo que ha sucedido. Pero el miedo y la incertidumbre se apoderan de todos.\n":
                r = "De uma hora para outra, vários membros da caravana desapareceram sem deixar rastros. Ninguém tem uma explicaçío para o que aconteceu. Mas o medo e a incerteza tomam conta de todos.\n";
                break;
            case "Luego de buscar vagamente en la cercaní­a y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n":
                r = "Depois de procurar superficialmente pelos arredores e concluir que nío há pistas, você decide consolar os familiares e seguir em frente.\n\n\n\n\n\n\n";
                break;
            case "<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>Você perde 4-12 Civis, -5 Esperança</b></color>";
                break;
            case "Uno de los bueyes de la caravana ha caí­do enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n":
                r = "Um dos bois da caravana adoeceu e nío pode continuar. Você recebe recomendações de alguns especialistas em gado que aconselham examinar os outros bois para evitar a propagaçío da doença.\n\n\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisarlos tomará unas horas: +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir examiná-los, isso levará algumas horas: +1 Avanço do Alento Negro.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides ignorar las advertencias: 1-3 Bueyes mas morirán.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir ignorar os avisos: mais 1-3 Bois morrerío.</color>\n\n";
                break;
            case "Mientras la caravana se dispone a avanzar por un terreno peligroso, se topa con un grupo de bandidos que exige un peaje exorbitante para dejar pasar a la caravana.\n\n":
                r = "Enquanto a caravana se prepara para avançar por um terreno perigoso, ela se depara com um grupo de bandidos que exige um pedágio exorbitante para deixá-la passar.\n\n";
                break;
            case "<color=#ba3fef>-Si decides pagar el peaje, perderás 1 de Oro por Civil.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir pagar o pedágio, você perderá 1 de Ouro por Civil.</color>\n\n";
                break;
            case "</color></b> se acerca a ti y no luce nada bien. Te comenta que ha empezado a sentirse enfermo y necesita medicina para mejorar pronto y estar nuevamente en condiciones de combatir.\n\n":
                r = "</color></b> se aproxima de você e nío parece nada bem. Ele comenta que começou a se sentir doente e precisa de remédio para melhorar logo e voltar a ter condições de lutar.\n\n";
                break;
            case "<color=#ba3fef>-Puedes dar un discurso motivador, refutando sus argumentos con hechos.</color> Chances: %":
                r = "<color=#ba3fef>-Você pode fazer um discurso motivador, refutando seus argumentos com fatos.</color> Chance: %";
                break;
            case " <i>(Determinado por Esperanza) Éxito: +15 Esperanza. Fallo: -20 Esperanza.</i> \n\n":
                r = " <i>(Determinado pela Esperança) Sucesso: +15 Esperança. Falha: -20 Esperança.</i> \n\n";
                break;
            case "Durante la noche, <b><color=#d1006f>":
                r = "Durante a noite, <b><color=#d1006f>";
                break;
            case "</color></b> junto con algunos Civiles comienzan a contar chistes y anécdotas divertidas, riendo y disfrutando del momento.\n":
                r = "</color></b> junto com alguns Civis começam a contar piadas e histórias divertidas, rindo e aproveitando o momento.\n";
                break;
            case " y ":
                r = " e ";
                break;
            case " ganan Alta Moral por 3 dí­as.</b></color>":
                r = " ganham Moral Alta por 3 dias.</b></color>";
                break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí­ y esta caravana no sobrevivió.\n":
                r = "Ao avançar pelo caminho, você encontra várias carroças destruí­das rodeadas de cadáveres de civis. Uma luta aconteceu aqui, e esta caravana nío sobreviveu.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n":
                r = "<color=#ba3fef>-Você pode ordenar que a Caravana saqueie os Suprimentos.</color> +21-35 Suprimentos, +5-11 Materiais, +15-35 Ouro, -5 Esperança.</i> \n\n";
                break;
            case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
                r = "A Caravana para em uma serraria abandonada, algumas árvores foram derrubadas e a madeira está empilhada em desordem.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n":
                r = "<color=#ba3fef>-Você pode ordenar que a Caravana recolha toda a madeira.</color> +65-90 Materiais, +1 Fadiga, +1 Avanço do Alento Negro.</i> \n\n";
                break;
            case "<color=#ba3fef>-Puedes optar por dejarlo cazar, o directamente domesticar a un puñado para que se sumen a la Caravana. +2-3 Bueyes</i> \n\n":
                r = "<color=#ba3fef>-Você pode optar por deixá-lo caçar, ou domesticar alguns diretamente para que se juntem á Caravana. +2-3 Bois</i> \n\n";
                break;
            case "<color=#ba3fef>-Puedes defender a los civiles de sus perseguidores mientras les das tiempo a los más débiles a sumarse a la Caravana.</color> Combate Normal - +18-26 Civiles\n\n":
                r = "<color=#ba3fef>-Você pode defender os civis de seus perseguidores enquanto dá tempo para que os mais fracos se juntem á Caravana.</color> Combate Normal - +18-26 Civis\n\n";
                break;
            case "<color=#ba3fef>-Puedes aceptar solo a los mas ágiles y huir para evitar confrontar con sus perseguidores.</color> +5-10 Civiles -5 Esperanza\n\n":
                r = "<color=#ba3fef>-Você pode aceitar apenas os mais ágeis e fugir para evitar confrontar seus perseguidores.</color> +5-10 Civis -5 Esperança\n\n";
                break;
            case "</color></b> se acerca a ti y coloca una mano en tu hombro y dice: -'Tengo mucha esperanza en usted, y creo que será exitoso al liderarnos a salvo hacia el puerto'.\n":
                r = "</color></b> se aproxima de você, coloca uma mío no seu ombro e diz: -'Tenho muita esperança no senhor, e acredito que terá sucesso ao nos conduzir em segurança até o porto'.\n";
                break;
            case "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un sí­mbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ":
                r = "Com a outra mío estendida, ele segura uma bolsa com ouro e a oferece amigavelmente a você. -'Considere isso um sí­mbolo da minha confiança em você, além de uma contribuiçío que pode ser útil para a Caravana.'- diz\n ";
                break;
            case "<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: ":
                r = "<color=#ba3fef>Você responde: -'Fique com o dinheiro, sua contribuiçío para a Caravana já é considerável com seu esforço diário, e sou mais do que grato por poder contar com você.'</color> Efeitos: ";
                break;
            case " gana Alta Moral por 4 dí­as y 50 Experiencia. \n\n":
                r = " ganha Moral Alta por 4 dias e 50 de Experiência. \n\n";
                break;
            case "<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n":
                r = "<color=#ba3fef>Você responde: -'Aceito sua oferta, nío há moeda sobrando em nossa situaçío atual, e isso certamente nos ajudará durante a viagem, obrigado.'</color> Efeitos: +120-160 Ouro. \n\n";
                break;
            case "Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchí­simos años en la zona y la conoce a la perfección.\n":
                r = "Um homem idoso aparece ao lado do caminho fazendo sinais com as míos para a Caravana. De perto, você percebe que esse homem vive na regiío há muitos anos e a conhece perfeitamente.\n";
                break;
            case "'Aliento Negro o no, mis dí­as ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n":
                r = "'Alento Negro ou nío, meus dias já estío contados. Mas posso transmitir a vocês meu conhecimento sobre esta terra, como último ato de bondade.'- diz\n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'¿Conoce algún atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n":
                r = "<color=#ba3fef>Você pergunta: -'Conhece algum atalho que nos afaste do perigo iminente por pelo menos alguns quilômetros?'</color> Efeitos: Se possí­vel, será gerado um Atalho subterrâneo. \n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n":
                r = "<color=#ba3fef>Você pergunta: -'Descreva-nos a área ao redor para que possamos tomar decisões com mais informaçío.'</color> Efeitos: Os próximos nós serío revelados. \n\n";
                break;
            case "</color></b> se lo ve con mucha energí­a y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n":
                r = "</color></b> aparenta estar com muita energia e determinaçío enquanto realiza suas tarefas habituais. Quando você se aproxima, ele diz que teve um Sonho no qual viu a Caravana chegando ao seu destino.\n";
                break;
            case "'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n":
                r = "'No sonho, vi um caminho claro até o nosso destino. Haverá perigos e dificuldades, mas estou convencido de que conseguiremos. Vamos seguir essa rota.'- diz com Determinaçío\n\n\n";
                break;
            case "</color></b> obtiene 150 Experiencia y Alta Moral por 5 dí­as.</color>\n\n":
                r = "</color></b> recebe 150 de Experiência e Moral Alta por 5 dias.</color>\n\n";
                break;
            case "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mí­nimo.\n":
                r = "Você chegou a uma bela clareira natural que parece nío ter sido manchada nem minimamente pela corrupçío e pela pestilência.\n";
                break;
            case "Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n":
                r = "É um excelente lugar para descansar e recuperar as forças.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrá también beneficios adicionales:\n-El Aliento Negro avanzará solo 1.\n-+10% curación recibida.\n-El evento será positivo.</b></color>":
                r = "<color=#a0e812><b>+5 Esperança.\n\nDescansar neste lugar também terá benefí­cios adicionais:\n-O Alento Negro avançará apenas 1.\n-+10% de cura recebida.\n-O evento será positivo.</b></color>";
                break;
            case "Has llegado a un pequeño asentamiento. Notas que los civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.":
                r = "Você chegou a um pequeno assentamento. Você nota que os civis estío desorganizados e precisam de liderança para sobreviver ao Alento Negro.";
                break;
            case "\nDe 15-25 Civiles se unirán a la Caravana y brindarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.":
                r = "\n15-25 Civis se juntarío á Caravana e fornecerío 50-60 Suprimentos, 6-8 Materiais, 2-4 Bois e 60-70 Ouro.";
                break;
            case "\nUn Héroe aleatorio se sumará a tus fuerzas.\n\n\n\n\n":
                r = "\nUm Herói aleatório se juntará ás suas forças.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>\nDescansar en este lugar tendrá beneficios adicionales:\n-+20% curación recibida.\n-0% chances de emboscada al descansar.</b></color>":
                r = "<color=#a0e812><b>\nDescansar neste lugar terá benefí­cios adicionais:\n-+20% de cura recebida.\n-0% de chance de emboscada ao descansar.</b></color>";
                break;
            case "Has llegado a un lugar rico en recursos naturales, los civiles se han puesto a recolectar lo que han podido.":
                r = "Você chegou a um lugar rico em recursos naturais, e os civis começaram a recolher o que puderam.";
                break;
            case "\nSe conseguirán de 18-30 Materiales y 80-140 Suministros.":
                r = "\nSerío obtidos 18-30 Materiais e 80-140 Suprimentos.";
                break;
            case "<color=#a0e812><b>\n\nDescansar en este lugar tendrá beneficios adicionales:+20% efectividad a tareas de Recolección.</b></color>":
                r = "<color=#a0e812><b>\n\nDescansar neste lugar terá benefí­cios adicionais: +20% de efetividade nas tarefas de Coleta.</b></color>";
                break;
            case " de oro. -5 Esperanza por el interrogatorio":
                r = " de ouro. -5 Esperança pelo interrogatório";
                break;
            case "Omitir Tutorial":
                r = "Pular Tutorial";
                break;
            case "Selecciona una tarea civil para el descanso":
                r = "Selecione uma tarefa civil para o descanso";
                break;
            case "Menu de Descanso ":
                r = "Menu de Descanso";
                break;
            case "Descansar":
                r = "Descansar";
                break;
            case "Carga":
                r = "Carga";
                break;
            case "Puesto Comercial":
                r = "Posto Comercial";
                break;
            case "Suministros":
                r = "Suprimentos";
                break;
            case "Compra 10x 200 Oro":
                r = "Comprar 10x 200 Ouro";
                break;
            case "Materiales":
                r = "Materiais";
                break;
            case "Bueyes":
                r = "Bois";
                break;
            case "Santuario de Purificadores":
                r = "Santuário dos Purificadores";
                break;
            case "3 Bueyes":
                r = "3 Bois";
                break;
            case "200 Oro":
                r = "200 Ouro";
                break;
            case "Haz tu ofrenda":
                r = "Faça sua oferenda";
                break;
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3 y un personaje con Corrupción al azar será curado.":
                r = "Ao fazer a oferenda, o Alento Negro recuará em 3 e um personagem aleatório com Corrupçío será curado.";
                break;
            case "Sacrificar ":
                r = "Sacrificar ";
                break;
            case "Donar":
                r = "Doar";
                break;
            case "Abandonar":
                r = "Abandonar";
                break;
            case "Elegir":
                r = "Escolher";
                break;
            case "Un solitario viajero pide unirse a la Caravana, parece capaz de defenderse sólo, seguramente sumarlo a la Caravana pueda ser beneficioso.":
                r = "Um viajante solitário pede para se juntar á Caravana. Ele parece capaz de se defender sozinho, e adicioná-lo á Caravana certamente pode ser benéfico.";
                break;
            case "Aceptarlos":
                r = "Aceitá-los";
                break;
            case "Defensas: Cada Tier mejora las defensas de la Caravana en ataques directos y reduce 10% las chances de perder un Séquito. ":
                r = "Defesas: Cada Tier melhora as defesas da Caravana em ataques diretos e reduz em 10% as chances de perder um Séquito. ";
                break;
            case "30 Materiales":
                r = "30 Materiais";
                break;
            case "Antorchas de Pie: Cada Tier reduce 5% el riesgo de sufrir una emboscada al Descansar.":
                r = "Tochas em Pé: Cada Tier reduz em 5% o risco de sofrer uma emboscada ao Descansar.";
                break;
            case "Alforjas: Cada Tier aumenta en 1 la Capacidad de carga de cada Buey.":
                r = "Alforjes: Cada Tier aumenta em 1 a Capacidade de carga de cada Boi.";
                break;
            case "Tiendas: Cada Tier da 5 de Esperanza al descansar y +1 Capacidad de Personaje.":
                r = "Tendas: Cada Tier concede 5 de Esperança ao descansar e +1 Capacidade de Personagem.";
                break;
            case "Catalejos: Cada Tier aumenta 5% las chances de Exploración y 5% las chances de encontrar Objetos tras una Batalla ganada.":
                r = "Lunetas: Cada Tier aumenta em 5% as chances de Exploraçío e em 5% as chances de encontrar Itens após uma Batalha vencida.";
                break;
            case "Carro Almacén: Cada Tier reduce 5% Suministros consumidos por Descanso.":
                r = "Carroça de Armazenamento: Cada Tier reduz em 5% os Suprimentos consumidos por Descanso.";
                break;
            case "Planes de mejoras ":
                r = "Planos de melhorias ";
                break;
            case "  Resistencias":
                r = "  Resistências";
                break;
            case "Rasgos":
                r = "Traços";
                break;
            case "Punto de Atributo!":
                r = "Ponto de Atributo!";
                break;
            case "Punto de Salvación!":
                r = "Ponto de Resistência!";
                break;
            case "Punto de Habilidad!":
                r = "Ponto de Habilidade!";
                break;
            case "Posición":
                r = "Posiçío";
                break;
            case "Elije una nueva Habilidad!":
                r = "Escolha uma nova Habilidade!";
                break;
            case "¡Batalla!":
                r = "Batalha!";
                break;
            case "Selecciona a tus personajes.":
                r = "Selecione seus personagens.";
                break;
            case "Comenzar":
                r = "Iniciar Batalha";
                break;
            case "¡Ataque a la Caravana!":
                r = "Ataque á Caravana!";
                break;
            case "Personajes en Guardia disponibles.":
                r = "Personagens de Guarda disponí­veis.";
                break;
            case "Victoria":
                r = "Vitória";
                break;
            case "Derrota":
                r = "Derrota";
                break;
            case "Turno Enemigo":
                r = "Turno Inimigo";
                break;
            case "Turno Aliado":
                r = "Turno Aliado";
                break;
            case "Terminar Turno":
                r = "Encerrar Turno";
                break;
            case "Ronda Nueva":
                r = "Nova Rodada";
                break;
            case "Volver":
                r = "Voltar";
                break;
            case "Salir":
                r = "Sair";
                break;
            case "-Es un dí­a hermoso. +5 Esperanza.":
                r = "-É um dia lindo. +5 Esperança.";
                break;
            case "-La Ola de Calor se hace insoportable. +1 Fatiga.":
                r = "-A Onda de Calor se torna insuportável. +1 Fadiga.";
                break;
            case "-La Lluvia hace el viaje más difí­cil. -5 Esperanza.":
                r = "-A Chuva torna a viagem mais difí­cil. -5 Esperança.";
                break;
            case "-La Nieve mejora el ánimo. +3 Esperanza.":
                r = "-A Neve melhora o ânimo. +3 Esperança.";
                break;
            case "% - Tirada: 1d100 = ":
                r = "% - Rolagem: 1d100 = ";
                break;
            case "Tirada: ":
                r = "Rolagem: ";
                break;
            case "-La caravana han sufrido un Ataque durante el descanso. Probabilidades ":
                r = "-A caravana sofreu um Ataque durante o descanso. Probabilidades ";
                break;
            case "-Durante el descanso, el Aliento Negro ha avanzado 2.":
                r = "-Durante o descanso, o Alento Negro avançou 2.";
                break;
            case "-Durante el descanso en el Claro, el Aliento Negro ha avanzado 1.":
                r = "-Durante o descanso na Clareira, o Alento Negro avançou 1.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro.":
                r = " realizou com Sucesso um Ritual de Limpeza durante o descanso, impedindo o avanço do Alento Negro.";
                break;
            case "-Debido a la alta Esperanza, los Acechadores han decidido no cobrar su sueldo esta vez.":
                r = "-Devido á alta Esperança, os Espreitadores decidiram nío cobrar seu salário desta vez.";
                break;
            case "-Los Acechadores en la Caravana se han cobrado su sueldo por Oro: ":
                r = "-Os Espreitadores na Caravana receberam seu salário em Ouro: ";
                break;
            case "-Debido al gran optimismo que rodea la Caravana, los Civiles han donado Oro: ":
                r = "-Devido ao grande otimismo que cerca a Caravana, os Civis doaram Ouro: ";
                break;
            case "-Debido al optimismo que rodea la Caravana, los Civiles han donado Oro: ":
                r = "-Devido ao otimismo que cerca a Caravana, os Civis doaram Ouro: ";
                break;
            case "-Por la muy baja Esperanza ":
                r = "-Devido á Esperança muito baixa ";
                break;
            case " Civiles han abandonado la Caravana.":
                r = " Civis abandonaram a Caravana.";
                break;
            case "-Por la baja Esperanza ":
                r = "-Devido á baixa Esperança ";
                break;
            case " Civiles.":
                r = " Civis.";
                break;
            case "-La falta de Suministros ha provocado la muerte de ":
                r = "-A falta de Suprimentos causou a morte de ";
                break;
            case "-Los Esclavos han recolectado ":
                r = "-Os Escravos coletaram ";
                break;
            case "-Los Herboristas han preparado sus Bálsamos.":
                r = "-Os Herboristas prepararam seus Bálsamos.";
                break;
            case "-En la Feria, los Artistas han realizado un espectáculo que ha levantado el ánimo de los Civiles. +10 Esperanza":
                r = "-Na Feira, os Artistas realizaram um espetáculo que elevou o ânimo dos Civis. +10 Esperança";
                break;
            case " se cura ":
                r = " recupera ";
                break;
            case " PV tras el Descanso.":
                r = " PV após o Descanso.";
                break;
            case "-El Séquito de Curanderos ha reducido la enfermedad de":
                r = "-O Séquito de Curandeiros reduziu a doença de";
                break;
            case " en 1 extra.":
                r = " em 1 extra.";
                break;
            case " comparte sus historias de batalla con los civiles. +4 Esperanza":
                r = " compartilha suas histórias de batalha com os civis. +4 Esperança";
                break;
            case "-El tener que trabajar en plena Ola de Calor, ha caí­do mal en los Civiles. -3 Esperanza":
                r = "-Ter que trabalhar em plena Onda de Calor foi mal recebido pelos Civis. -3 Esperança";
                break;
            case "-El tener un Dí­a Libre en plena Ola de Calor, ha caí­do bien en los Civiles. +5 Esperanza":
                r = "-Ter um Dia Livre em plena Onda de Calor foi bem recebido pelos Civis. +5 Esperança";
                break;
            case "Las probabilidades de exploración: ":
                r = "As probabilidades de exploraçío: ";
                break;
            case "Las probabilidades de sufrir un ataque a la Caravana ":
                r = "As probabilidades de sofrer um ataque á Caravana ";
                break;
            case "<b><u>Estado de Alerta</b></u>\n\n\n":
                r = "<b><u>Estado de Alerta</b></u>\n\n\n";
                break;
            case "Durante el descanso, se asignarán a los civiles mas aptos fí­sicamente a la vigilancia del area circundante al campamento.\n\n":
                r = "Durante o descanso, os civis fisicamente mais aptos serío designados para vigiar a área ao redor do acampamento.\n\n";
                break;
            case "<color=#d8a205>Reduce chances de ataque a caravana. +20% a Exploración. -10 Esperanza.</color>\n\n\n":
                r = "<color=#d8a205>Reduz as chances de ataque á caravana. +20% de Exploraçío. -10 Esperança.</color>\n\n\n";
                break;
            case "<b><u>Dí­a Libre</b></u>\n\n\n":
                r = "<b><u>Dia Livre</b></u>\n\n\n";
                break;
            case "Los civiles se tomarán el dí­a para descansar y recobrar fuerzas.\n\n":
                r = "Os civis tirarío o dia para descansar e recuperar as forças.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá 10 de Esperanza y el dí­a siguiente arrancará con -1 Fatiga.</color>\n\n\n":
                r = "<color=#d8a205>Serío obtidos 10 de Esperança, e o dia seguinte começará com -1 Fadiga.</color>\n\n\n";
                break;
            case "<b><u>Feria</b></u>\n\n\n":
                r = "<b><u>Feira</b></u>\n\n\n";
                break;
            case "Los civiles dedicarán el dí­a a organizar una feria con varios juegos y celebraciones.\n\n":
                r = "Os civis dedicarío o dia a organizar uma feira com vários jogos e celebrações.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá entre 15 y 25 de Esperanza y se consumirán 20% más de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n":
                r = "<color=#d8a205>Serío obtidos entre 15 e 25 de Esperança, e serío consumidos 20% a mais de Suprimentos. <color=#bb280d>+10% de chance de Emboscada.</color></color>\n\n\n";
                break;
            case "<b><u>Recolección de Materiales</b></u>\n\n\n":
                r = "<b><u>Coleta de Materiais</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarán a recolectar materiales básicos en la zona.\n\n":
                r = "Os civis se dedicarío a coletar materiais básicos na área.\n\n";
                break;
            case "<color=#d8a205>Se juntarán entre ":
                r = "<color=#d8a205>Serío reunidos entre ";
                break;
            case " materiales. </color>\n\n\n":
                r = " materiais. </color>\n\n\n";
                break;
            case "<b><u>Recolección de Suministros</b></u>\n\n\n":
                r = "<b><u>Coleta de Suprimentos</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarán a recolectar distintos suministros de las inmediaciones al campamento.\n\n":
                r = "Os civis se dedicarío a coletar diferentes suprimentos nos arredores do acampamento.\n\n";
                break;
            case " suministros. </color>\n\n\n":
                r = " suprimentos. </color>\n\n\n";
                break;
            case "Combate directo.":
                r = "Combate direto.";
                break;
            case "Evento aleatorio.":
                r = "Evento aleatório.";
                break;
            case "Claro tranquilo.":
                r = "Clareira tranquila.";
                break;
            case "Recolección de Recursos.":
                r = "Coleta de Recursos.";
                break;
            case "Puesto de Comercio.":
                r = "Posto Comercial.";
                break;
            case "Adquisición de Personajes.":
                r = "Aquisiçío de Personagens.";
                break;
            case "Combate directo contra enemigos de élite.":
                r = "Combate direto contra inimigos de elite.";
                break;
            case "Batalla final de la Zona actual.":
                r = "Batalha final da Zona atual.";
                break;
            case "<b>(!)</b> Zona Expuesta, la caravana será emboscada.":
                r = "<b>(!)</b> Zona Exposta, a caravana será emboscada.";
                break;
            case "Nodo Desconocido.":
                r = "Nó Desconhecido.";
                break;
            case "Nodo Misterioso, no se ha logrado revelar.":
                r = "Nó Misterioso, nío foi possí­vel revelá-lo.";
                break;
            case "Salida del atajo subterraneo, no sabemos que hay del otro lado.":
                r = "Saí­da do atalho subterrâneo, nío sabemos o que há do outro lado.";
                break;
            case "Santuario de Purificadores.":
                r = "Santuário dos Purificadores.";
                break;
            case "<color=#7ED6F7>-Durante el Descanso, se ha Explorado con Éxito el camino adelante.</color>":
                r = "<color=#7ED6F7>-Durante o Descanso, o caminho á frente foi explorado com Sucesso.</color>";
                break;
            case " ha Explorado con Éxito el camino adelante.</color>":
                r = " explorou com Sucesso o caminho á frente.</color>";
                break;
            case "-Al viajar por el atajo subterráneo, la moral de la caravana disminuye. -5 Esperanza":
                r = "-Ao viajar pelo atalho subterrâneo, a moral da caravana diminui. -5 Esperança";
                break;
            case "-Se ha encontrado un atajo subterráneo.":
                r = "-Um atalho subterrâneo foi encontrado.";
                break;
            case "<color=#7ED6F7>-Entre la bruma del camino, la caravana distingue una aldea a la distancia. Se ha descubierto un asentamiento.</color>":
                r = "<color=#7ED6F7>-Em meio a neblina da estrada, a caravana avista uma aldeia ao longe. Um assentamento foi descoberto.</color>";
                break;
            case "-La Caravana ha viajado con exceso de Carga. -10 Esperanza +1 Fatiga":
                r = "-A Caravana viajou com excesso de Carga. -10 Esperança +1 Fadiga";
                break;
            case "Fuerza: ":
                r = "Força: ";
                break;
            case "Agilidad: ":
                r = "Agilidade: ";
                break;
            case "Poder: ":
                r = "Poder: ";
                break;
            case "Iniciativa: ":
                r = "Iniciativa: ";
                break;
            case "PA: ":
                r = "PA: ";
                break;
            case "Valentí­a: ":
                r = "Bravura: ";
                break;
            case "Armadura: ":
                r = "Armadura: ";
                break;
            case "Defensa: ":
                r = "Defesa: ";
                break;
            case "-Reflejos: ":
                r = "-Reflexos: ";
                break;
            case "-Fortaleza: ":
                r = "-Fortaleza: ";
                break;
            case "-Mental: ":
                r = "-Mental: ";
                break;
            case "<color=#2a9c71>\n\nFatigado: -1 PA máximo. </color>":
                r = "<color=#2a9c71>\n\nFatigado: -1 PA máximo. </color>";
                break;
            case "Bendecido por Plegaria: +1 Ataque +1 Defensa +5 Res.Necro +2 TSMental.</color>":
                r = "Abençoado por Prece: +1 Ataque +1 Defesa +5 Res.Necro +2 TSMental.</color>";
                break;
            case "<color=#d80404>\n\nHerido:-1 Atributos. Si cae en combate, muere. </color>":
                r = "<color=#d80404>\n\nFerido: -1 Atributos. Se cair em combate, morre. </color>";
                break;
            case "<color=#d80404>\n\nCorrupto: Los enemigos corrompidos se curan al atacarlo, le infligen mas daño, y si lo derriban en combate, muere. </color>":
                r = "<color=#d80404>\n\nCorrompido: os inimigos corrompidos se curam ao atacá-lo, causam mais dano e, se o derrubarem em combate, ele morre. </color>";
                break;
            case "<color=#d80404>\n\nEnfermo por ":
                r = "<color=#d80404>\n\nDoente por ";
                break;
            case " dí­as. -15% daño, -3 TS Fortaleza, -1 PA </color>":
                r = " dias. -15% de dano, -3 TS Fortaleza, -1 PA </color>";
                break;
            case "<color=#d80404>\n\nBaja Moral por ":
                r = "<color=#d80404>\n\nMoral Baixa por ";
                break;
            case " dí­as. -1 Ataque y Defensa, -3 TS Mental, -2 Valentí­a Inicial</color>":
                r = " dias. -1 Ataque e Defesa, -3 TS Mental, -2 Bravura Inicial</color>";
                break;
            case "<color=#d80404>\n\nAlta Moral por ":
                r = "<color=#d80404>\n\nMoral Alta por ";
                break;
            case " dí­as. +1 Ataque, +2 TS Mental, +2 Valentí­a Inicial</color>":
                r = " dias. +1 Ataque, +2 TS Mental, +2 Bravura Inicial</color>";
                break;
            case "Torpe: +1 Rango Pifias. ":
                r = "Desajeitado: +1 Faixa de Erros. ";
                break;
            case "Valiente: +2 Valentí­a Máxima.":
                r = "Valente: +2 Bravura Máxima.";
                break;
			case "Alegre: +2 Esperanza al Descansar.":
                r = "Alegre: +2 Esperança ao Descansar.";
                break;
            case "Inventario":
                r = "Inventário";
                break;
            case "Accesorios":
                r = "Acessórios";
                break;
            case "Arma":
                r = "Arma";
                break;
            case "Armadura":
                r = "Armadura";
                break;
            case "Consumibles":
                r = "Consumí­veis";
                break;
            case "<color=#0cca74><b>Guardia: </b></color><color=#d3d3d3><i>El personaje se mantendrá alerta y custodiará la caravana.</color></i>\\n\\nSi se produce una emboscada, podrá participar de la defensa sin penalización. +3% Exploración al descansar.":
                r = "<color=#0cca74><b>Guarda: </b></color><color=#d3d3d3><i>O personagem permanecerá alerta e protegerá a caravana.</color></i>\\n\\nSe ocorrer uma emboscada, poderá participar da defesa sem penalidade. +3% Exploraçío ao descansar.";
                break;
            case "<color=#0cca74><b>Coerción: </b></color><color=#d3d3d3><i>Con métodos cuestionables, el Acechador obliga a los Mercaderes a donar dinero a la caravana.</color></i>\\n\\n+1-10 Oro y -1 Esperanza por dí­a.":
                r = "<color=#0cca74><b>Coerçío: </b></color><color=#d3d3d3><i>Com métodos questionáveis, o Espreitador obriga os Mercadores a doar dinheiro para a caravana.</color></i>\\n\\n+1-10 Ouro e -1 Esperança por dia.";
                break;
            case "<color=#0cca74><b>Exploración: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo Subterráneo\\nSi se da un combate, lo arranca Fatigado.":
                r = "<color=#0cca74><b>Exploraçío: </b></color><color=#d3d3d3><i>O personagem explora os possí­veis destinos á frente da caravana.</color></i>\\n\\nTem 40% de chance de revelar Nós futuros ao viajar para um novo Nó. -5% de chance de Nó Misterioso. +5% de chance de Atalho Subterrâneo\\nSe ocorrer um combate, ele o inicia Fatigado.";
                break;
            case "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirá su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrá +3 Flechas y +5% daño.":
                r = "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>O personagem dedicará seu tempo a criar e aprimorar suas flechas.</color></i>\\n\\nSe ocorrer um combate, terá +3 Flechas e +5% de dano.";
                break;
            case "<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocupará de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzará con +3 Armadura.":
                r = "<color=#0cca74><b>Manutençío de Armadura: </b></color><color=#d3d3d3><i>O personagem cuidará da manutençío da sua armadura.</color></i>\\n\\nSe ocorrer um combate, começará com +3 de Armadura.";
                break;
            case "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerá vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrá participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daño los primeros 2 turnos.":
                r = "<color=#0cca74><b>Vigiar: </b></color><color=#d3d3d3><i>O personagem permanecerá vigilante diante de qualquer perigo.</color></i>\\n\\nSe ocorrer uma emboscada, poderá participar ativamente da defesa e recebe +2 PA, +5 Iniciativa e +20% de dano nos 2 primeiros turnos.";
                break;
            case "<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizará su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada dí­a que pase ganará 15 Experiencia.\\nSi se produce un combate, lo arrancará Fatigado.":
                r = "<color=#0cca74><b>Treinar: </b></color><color=#d3d3d3><i>O personagem usará seu tempo livre para treinar e se manter em forma.</color></i>\\n\\nA cada dia, ganhará 15 de Experiência.\\nSe ocorrer um combate, ele o iniciará Fatigado.";
                break;
            case "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nCada dí­a que pase recuperará un 15% de salud.\\nSi se produce un combate, lo arrancará Fresco.":
                r = "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>O personagem vai se concentrar em descansar e recuperar sua saúde.</color></i>\\n\\nA cada dia, recuperará 15% de saúde.\\nSe ocorrer um combate, ele o iniciará Disposto.";
                break;
            case "<color=#0cca74><b>Afilar Armas: </b></color><color=#d3d3d3><i>El Acechador se encarga de mantener sus armas afiladas.</color></i>\\n\\nSi se produce un combate tendrá +10% daño.":
                r = "<color=#0cca74><b>Afiar Armas: </b></color><color=#d3d3d3><i>O Espreitador se encarrega de manter suas armas afiadas.</color></i>\\n\\nSe ocorrer um combate, terá +10% de dano.";
                break;
            case "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.":
                r = "<color=#0cca74><b>Telecinese: </b></color><color=#d3d3d3><i>Com seus poderes arcanos de telecinese, ajuda com a carga da caravana.</color></i>\\n\\n+20 de Capacidade de carga.";
                break;
            case "<color=#0cca74><b>Caza Nocturna: </b></color><color=#d3d3d3><i>El personaje cazará en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+1d4 Suministros por dí­a. +3% probabilidad de Emboscada Enemiga al descansar.":
                r = "<color=#0cca74><b>Caça Noturna: </b></color><color=#d3d3d3><i>O personagem caçará nos arredores para conseguir comida para a caravana.</color></i>\\n\\n+1d4 Suprimentos por dia. +3% de probabilidade de Emboscada Inimiga ao descansar.";
                break;
            case "<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirá los relatos de sus hazañas con quienes quieran oí­rlas.</color></i>\\n\\n+10 Experiencia por dí­a a personajes de nivel inferior. +4 Esperanza al descansar.":
                r = "<color=#0cca74><b>Relatos de Batalha: </b></color><color=#d3d3d3><i>O personagem compartilhará os relatos de seus feitos com quem quiser ouvi-los.</color></i>\\n\\n+10 de Experiência por dia para personagens de ní­vel inferior. +4 Esperança ao descansar.";
                break;
            case "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizará rituales de protección para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por dí­a.":
                r = "<color=#0cca74><b>Ritual de Limpeza: </b></color><color=#d3d3d3><i>A Purificadora realizará rituais de proteçío para combater o Alento Negro.</color></i>\\n\\nProbabilidade de evitar o avanço do Alento Negro: 25% ao descansar, 15% por dia.";
                break;
            case "<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usará su tiempo para ayudar a los rezagados y más débiles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.":
                r = "<color=#0cca74><b>Ajudar os Desamparados: </b></color><color=#d3d3d3><i>A Purificadora usará seu tempo para ajudar os mais atrasados e frágeis da caravana.</color></i>\\n\\n+1d3 de Esperança por dia. +1 Fervor em combate.";
                break;
            case "<color=#0cca74><b>Concentración Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de Energí­a al iniciar combates.":
                r = "<color=#0cca74><b>Concentraçío Arcana: </b></color><color=#d3d3d3><i>O Canalizador se concentra e mantém seu poder preparado para qualquer combate que surgir.</color></i>\\n\\n+1 Ní­vel de Energia ao iniciar combates.";
                break;
            case "<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.\\nEn Ataque a Caravana cuenta como Guardia y comienza en Sigilo.":
                r = "<color=#0cca74><b>Vigiar das Sombras: </b></color><color=#d3d3d3><i>O Espreitador percorre os arredores da caravana em sigilo, tentando antecipar emboscadas inimigas.</color></i>\\n\\n-5% de chance de emboscadas.\\nEm Ataque a Caravana conta como Guarda e comeca em Sigilo.";
                break;
            case "<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>Séquito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curación diaria del Séquito de Curanderos.":
                r = "<color=#0cca74><b>Colaborar com os Curandeiros: </b></color><color=#d3d3d3><i>Ajuda o <b>Séquito de Curandeiros</b> em suas tarefas, aumentando sua eficácia.</color></i>\\n\\nAumenta em 5% a cura diária do Séquito de Curandeiros.";
                break;
            case "<color=#0cca74><b>Crear Sí­mbolo Arcano de Protección: </b></color><color=#d3d3d3><i>El Canalizador concentra energí­a arcana protectora en un sí­mbolo que puede proteger a quien lo utilice.</color></i>\\n\\nCrea un Sí­mbolo Arcano de Protección por dí­a.":
                r = "<color=#0cca74><b>Criar Sí­mbolo Arcano de Proteçío: </b></color><color=#d3d3d3><i>O Canalizador concentra energia arcana protetora em um sí­mbolo que pode proteger quem o utilizar.</color></i>\\n\\nCria um Sí­mbolo Arcano de Proteçío por dia.";
                break;
            case "-El viaje por el camino sinuoso ha retrasado la caravana. +":
                r = "-A viagem pelo caminho sinuoso atrasou a caravana. +";
                break;
            case " Avance del Aliento Negro":
                r = " de Avanço do Alento Negro";
                break;
            case "-La nieve a retrasado el viaje. +1 Avance del Aliento Negro":
                r = "-A neve atrasou a viagem. +1 de Avanço do Alento Negro";
                break;
            case "-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza":
                r = "-A ausência do Alento Negro durante a viagem inspira a Caravana. +2 Esperança";
                break;
            case "-La presencia notable del Aliento Negro al viajar, provoca incertidumbre en la Caravana. -3 Esperanza":
                r = "-A presença perceptí­vel do Alento Negro durante a viagem provoca incerteza na Caravana. -3 Esperança";
                break;
            case "-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -5 Esperanza":
                r = "-A forte presença do Alento Negro no ar provoca medo na Caravana. -5 Esperança";
                break;
            case "-La presencia de Aliento Negro en el aire es fatal para los Civiles. -7 Esperanza -":
                r = "-A presença do Alento Negro no ar é fatal para os Civis. -7 Esperança -";
                break;
            case " Civiles":
                r = " Civis";
                break;
            case "-El Séquito de Herboristas ha visitado un Claro y recolectado hierbas curativas.":
                r = "-O Séquito de Herboristas visitou uma Clareira e coletou ervas curativas.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza, previniendo el avance del Aliento Negro.":
                r = " realizou com Úxito um Ritual de Limpeza, impedindo o avanço do Alento Negro.";
                break;
            case "-Los rezos constantes del Séquito de Clérigos han logrado frenar el avance del Aliento Negro.":
                r = "-As orações constantes do Séquito de Clérigos conseguiram frear o avanço do Alento Negro.";
                break;
            case "-Un nuevo personaje se ha unido a la caravana: ":
                r = "-Um novo personagem se juntou á caravana: ";
                break;
            case "Envenenado":
                r = "Envenenado";
                break;
            case " ha sido envenenado por ":
                r = " foi envenenado por ";
                break;
            case " fue Encarnado por Fuego Fatuo":
                r = " foi Encarnado por Fogo-Fátuo";
                break;
            case " reacciona con ":
                r = " reage com ";
                break;
            case " se ha unido a la batalla. Quedan ":
                r = " entrou na batalha. Restam ";
                break;
            case " refuerzos.</color> ":
                r = " reforços.</color> ";
                break;
            case " ya no tiene ":
                r = " nío tem mais ";
                break;
            case "No puedes intercambiar con una unidad inmovilizada.":
                r = "Você nío pode trocar de lugar com uma unidade imobilizada.";
                break;
            case "No puedes intercambiar con una unidad que ya está Desplazada.":
                r = "Você nío pode trocar de lugar com uma unidade que já está Deslocada.";
                break;
            case "No puedes intercambiar con obstáculos.":
                r = "Você nío pode trocar de lugar com obstáculos.";
                break;
            case "No tienes PA suficientes para intercambiar.":
                r = "Você nío tem PA suficientes para trocar de lugar.";
                break;
            case "Apagando!":
                r = "Apagando!";
                break;
            case " gasta 1 PA para apagar el fuego.":
                r = " gasta 1 PA para apagar o fogo.";
                break;
            case " está congelado.":
                r = " está congelado.";
                break;
            case "Descongelado!":
                r = "Descongelado!";
                break;
            case " se libró del congelamiento.":
                r = " se livrou do congelamento.";
                break;
            case " está aturdido.":
                r = " está atordoado.";
                break;
            case " regenera ":
                r = " regenera ";
                break;
            case " Armadura.":
                r = " de Armadura.";
                break;
            case " está inmovilizado.":
                r = " está imobilizado.";
                break;
            case " recibe ":
                r = " recebe ";
                break;
            case " daño veneno.":
                r = " de dano de veneno.";
                break;
            case "Veneno":
                r = "Veneno";
                break;
            case "Sangrado":
                r = "Sangramento";
                break;
            case "Ardiendo":
                r = "Em Chamas";
                break;
            case " resiste totalmente al veneno.":
                r = " resiste totalmente ao veneno.";
                break;
            case " falla su Tirada de salvación y el veneno empeora.":
                r = " falha na sua Jogada de resistência e o veneno piora.";
                break;
            case " arde":
                r = " arde";
                break;
            case "Inmune":
                r = "Imune";
                break;
            case " veneno":
                r = " veneno";
                break;
            case " frio":
                r = " frio";
                break;
            case " aturde":
                r = " atordoado";
                break;
            case " inmóvil":
                r = " imóvel";
                break;
            case " sangrado":
                r = " sangramento";
                break;
            case " acido":
                r = " ácido";
                break;
            case " sigue canalizando.":
                r = " continua canalizando.";
                break;
            case " ya no está escondido.":
                r = " nío está mais escondido.";
                break;
            case " está escondido.":
                r = " está escondido.";
                break;
            case "La Barrera de ":
                r = "A Barreira de ";
                break;
            case " absorbió ":
                r = " absorveu ";
                break;
            case " de daño.":
                r = " de dano.";
                break;
            case " de daño ":
                r = " de dano ";
                break;
            case "Cura ":
                r = "Cura ";
                break;
            case " recibe <color=#11c66b>":
                r = " recebe <color=#11c66b>";
                break;
            case "</color> de curación.":
                r = "</color> de cura.";
                break;
            case " muere.":
                r = " morre.";
                break;
            case " realiza Tirada de Salvación: 1d20 = ":
                r = " realiza Jogada de Resistência: 1d20 = ";
                break;
            case " vs Tirada Dificultad: ":
                r = " vs Dificuldade: ";
                break;
            case ". Resultado: No se salva.":
                r = ". Resultado: Falha.";
                break;
            case ". Resultado: Se salva.":
                r = ". Resultado: Sucesso.";
                break;
            case "Resiste":
                r = "Resiste";
                break;
            case "-El Séquito de Cronistas ha registrado el viaje. +20 Valor Crónica.":
                r = "-O Séquito de Cronistas registrou a viagem. +20 Valor da Crônica.";
                break;
            case "-El Séquito de Nobles ha hecho una donación. Oro: ":
                r = "-O Séquito de Nobres fez uma doaçío. Ouro: ";
                break;
            case "-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza.":
                r = "-Os Civis se sentem culpados pela presença dos Escravos. -2 Esperança.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se ha gastado 200 de oro.":
                r = "-Você realizou um ritual no santuário. O Alento Negro recua em 3 e 200 de ouro foram gastos.";
                break;
            case " ha sido purificado de la corrupción.":
                r = " foi purificado da corrupçío.";
                break;
            case "-No hay personajes corruptos para purificar.":
                r = "-Nío há personagens corrompidos para purificar.";
                break;
            case "-No tienes suficientes bueyes para realizar el ritual en el santuario.":
                r = "-Você nío tem bois suficientes para realizar o ritual no santuário.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se han sacrificado 3 bueyes.":
                r = "-Você realizou um ritual no santuário. O Alento Negro recua em 3 e 3 bois foram sacrificados.";
                break;
            case "-El Séquito de Artistas ha tenido un festán y despilfarrado suministros: ":
                r = "-O Séquito de Artistas fez um banquete e desperdiçou suprimentos: ";
                break;
            case " PV por su Actividad de <b>Descanso</b>.":
                r = " PV por sua Atividade de <b>Descanso</b>.";
                break;
            case " Experiencia por su Actividad de <b>Entrenamiento</b>.":
                r = " de Experiência por sua Atividade de <b>Treinamento</b>.";
                break;
            case " gana ":
                r = " ganha ";
                break;
            case " pierde ":
                r = " perde ";
                break;
            case " brinda 10 Experiencia a sus compañeros de menor nivel por su Actividad de <b>Relatos de Batalla</b>.":
                r = " concede 10 de Experiência a seus companheiros de ní­vel inferior por sua Atividade de <b>Relatos de Batalha</b>.";
                break;
            case " consigue ":
                r = " consegue ";
                break;
            case " suministros por su Actividad de <b>Caza Nocturna</b>.":
                r = " suprimentos por sua Atividade de <b>Caça Noturna</b>.";
                break;
            case " realiza su actividad <b>Ayudar a los Desamparados</b> y la esperanza aumenta en ":
                r = " realiza sua atividade <b>Ajudar os Desamparados</b> e a esperança aumenta em ";
                break;
            case " de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza":
                r = " de Ouro dos Mercadores da Caravana, que foram coagidos a doar para a causa. -1 Esperança";
                break;
            case " ha creado un Sí­mbolo de Protección Arcano.":
                r = " criou um Sí­mbolo Arcano de Proteçío.";
                break;
            case "-La fatiga ha provocado la muerte de algunos Bueyes.":
                r = "-A fadiga provocou a morte de alguns Bois.";
                break;
            case " Bueyes":
                r = " Bois";
                break;
            case "-La fatiga extrema ha provocado la muerte de algunos Bueyes y Civiles.":
                r = "-A fadiga extrema provocou a morte de alguns Bois e Civis.";
                break;
            case " Bueyes -":
                r = " Bois -";
                break;
            case "-El Séquito de Nobles se queja por la falta de descanso. -2 Esperanza":
                r = "-O Séquito de Nobres reclama da falta de descanso. -2 Esperança";
                break;
            case "-Tus personajes están fatigados. Afectará su rendimiento en batalla.":
                r = "-Seus personagens estío fatigados. Isso afetará seu desempenho em batalha.";
                break;
            case "-El sacrificio de Bueyes para obtener Suministros ha provocado preocupación. -2 Esperanza":
                r = "-O sacrifí­cio de Bois para obter Suprimentos provocou preocupaçío. -2 Esperança";
                break;
            case "-Los Cronistas han registrado la victoria, +50 Valor Crónica, +5 Esperanza.":
                r = "-Os Cronistas registraram a vitória, +50 Valor da Crônica, +5 Esperança.";
                break;
            case "-Los Cronistas han registrado la derrota, -50 Valor Crónica. -3 Esperanza.":
                r = "-Os Cronistas registraram a derrota, -50 Valor da Crônica. -3 Esperança.";
                break;
            case "Victoria sin recompensas definidas para este encuentro clásico.":
                r = "Vitória sem recompensas definidas para este encontro clássico.";
                break;
            case "Derrota en un encuentro clásico. Los efectos especí­ficos aún no están configurados.":
                r = "Derrota em um encontro clássico. Os efeitos especí­ficos ainda nío estío configurados.";
                break;
            case "sin botón":
                r = "sem saque";
                break;
            case " ha sido corrompido.":
                r = " foi corrompido.";
                break;
            case "-Se ha unido el Séquito de Artistas a la caravana. +25 Civiles":
                r = "-O Séquito de Artistas se juntou á caravana. +25 Civis";
                break;
            case "Séquito de Herreros":
                r = "Séquito de Ferreiros";
                break;
            case "Séquito de Curanderos":
                r = "Séquito de Curandeiros";
                break;
            case "Séquito de Mercaderes":
                r = "Séquito de Mercadores";
                break;
            case "Séquito de Artistas":
                r = "Séquito de Artistas";
                break;
            case "Séquito de Herboristas":
                r = "Séquito de Herboristas";
                break;
            case "Séquito de Desertores":
                r = "Séquito de Desertores";
                break;
            case "Séquito de Cronistas":
                r = "Séquito de Cronistas";
                break;
            case "Séquito de Refugiados":
                r = "Séquito de Refugiados";
                break;
            case "Séquito de Nobles":
                r = "Séquito de Nobres";
                break;
            case "Séquito de Clérigos":
                r = "Séquito de Clérigos";
                break;
            case "Séquito de Esclavos":
                r = "Séquito de Escravos";
                break;
            case "-Se ha unido el Séquito de Herboristas a la caravana. +10 Civiles":
                r = "-O Séquito de Herboristas se juntou á caravana. +10 Civis";
                break;
            case "-Los Desertores se han unido a la Caravana. +15 Civiles -8 Esperanza":
                r = "-Os Desertores se juntaram á Caravana. +15 Civis -8 Esperança";
                break;
            case "-Los Cronistas se han unido a la Caravana. +10 Civiles":
                r = "-Os Cronistas se juntaram á Caravana. +10 Civis";
                break;
            case "-Los Refugiados se han unido a la Caravana. +35 Civiles  +30 Esperanza":
                r = "-Os Refugiados se juntaram á Caravana. +35 Civis +30 Esperança";
                break;
            case "-Los Nobles se han unido a la Caravana. +25 Civiles":
                r = "-Os Nobres se juntaram á Caravana. +25 Civis";
                break;
            case "-Los Clérigos del Sol Purificador se han unido a la Caravana. +20 Civiles +15 Esperanza":
                r = "-Os Clérigos do Sol Purificador se juntaram á Caravana. +20 Civis +15 Esperança";
                break;
            case "-Los Esclavos se han unido a la Caravana. +30 Civiles":
                r = "-Os Escravos se juntaram á Caravana. +30 Civis";
                break;
            case "-El Séquito de Artistas ha abandonado la caravana. -25 Civiles -15 Esperanza":
                r = "-O Séquito de Artistas abandonou a caravana. -25 Civis -15 Esperança";
                break;
            case "-El Séquito de Herboristas ha abandonado la caravana. -10 Civiles":
                r = "-O Séquito de Herboristas abandonou a caravana. -10 Civis";
                break;
            case "-Los Desertores han abandonado la Caravana. -15 Civiles":
                r = "-Os Desertores abandonaram a Caravana. -15 Civis";
                break;
            case "-Los Cronistas han abandonado la Caravana. -10 Civiles":
                r = "-Os Cronistas abandonaram a Caravana. -10 Civis";
                break;
            case "-Los Refugiados han abandonado la Caravana. -35 Civiles -40 Esperanza":
                r = "-Os Refugiados abandonaram a Caravana. -35 Civis -40 Esperança";
                break;
            case "-Los Nobles han abandonado la Caravana. -25 Civiles":
                r = "-Os Nobres abandonaram a Caravana. -25 Civis";
                break;
            case "-Se ha vendido la crónica del viaje por Oro: ":
                r = "-A crônica da viagem foi vendida por Ouro: ";
                break;
            case " ha recibido tratamiento especial y sus heridas han sanado.":
                r = " recebeu tratamento especial e seus ferimentos cicatrizaram.";
                break;
            case "Un grupo de eruditos unidos que se dedican a registrar los sucesos del viaje de la caravana hacia el puerto. Sus escrituras pueden ser una fuenta de ingresos y moral, pero también puede ser contraproducente en los peores momentos.\n\n":
                r = "Um grupo unido de eruditos que se dedica a registrar os acontecimentos da viagem da caravana até o porto. Seus escritos podem ser uma fonte de renda e moral, mas também podem ser contraproducentes nos piores momentos.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +5 de Esperanza por batallas ganadas (-3 Derrotas). ":
                r = "EFEITOS PASSIVOS:\n\n-Concedem +5 de Esperança por batalhas vencidas (-3 em Derrotas). ";
                break;
            case "\n\n-Ya se ha vendido la crónica de este viaje.":
                r = "\n\n-A crônica desta viagem já foi vendida.";
                break;
            case "\n\n- Crónica: Acumula valor de la siguiente manera:":
                r = "\n\n- Crônica: Acumula valor da seguinte forma:";
                break;
            case "\n   - Base: 150 Oro":
                r = "\n   - Base: 150 Ouro";
                break;
            case "\n   - +1 Oro por cada punto de Esperanza":
                r = "\n   - +1 Ouro por cada ponto de Esperança";
                break;
            case "\n   - +20 Oro por cada nodo viajado":
                r = "\n   - +20 Ouro por cada nó percorrido";
                break;
            case "\n   - +50 Oro por cada batalla ganada / -50 Oro por cada batalla perdida":
                r = "\n   - +50 Ouro por cada batalha vencida / -50 Ouro por cada batalha perdida";
                break;
            case "\n\nSe puede vender en Asentamientos o Puestos Comerciales.":
                r = "\n\nPode ser vendida em Assentamentos ou Postos Comerciais.";
                break;
            case "\n\n\n\n-Valor Crónica: Oro: ":
                r = "\n\n\n\n-Valor da Crônica: Ouro: ";
                break;
            case "% por Herboristas":
                r = "% por Herboristas";
                break;
            case "Carros de Tratamiento: Mejorar los carros utilizados por el Séquito de Curanderos para tratar heridos significará una mejora en los tratamientos recibidos por los heridos y su tiempo de recuperación. \nCada Tier aumenta en 5% la curación diaria de los personajes que Descansen y reduce el costo de Tratar Heridas. \nAdemás cada tier da un 10% extra a las posibilidades de reducir Enfermedades al Descansar (20% base). \nCuración proporcionada: ":
                r = "Carroças de Tratamento: Melhorar as carroças usadas pelo Séquito de Curandeiros para tratar feridos significará uma melhora nos tratamentos recebidos pelos personagens feridos e no seu tempo de recuperaçío. \nCada Tier aumenta em 5% a cura diária dos personagens que Descansarem e reduz o custo de Tratar Feridas. \nAlém disso, cada tier concede 10% extras nas chances de reduzir Doenças ao Descansar (20% base). \nCura fornecida: ";
                break;
            case " Materiales":
                r = " Materiais";
                break;
            case "Tratar Heridas - Coste: <color=#A5B328>":
                r = "Tratar Feridas - Custo: <color=#A5B328>";
                break;
            case "Tratar Heridas - Coste: <color=#C40E0E>":
                r = "Tratar Feridas - Custo: <color=#C40E0E>";
                break;
            case "Han sido esclavos toda su vida, e incluso en estas circunstancias se comportan como tal. La situación amerita aprovecharse de su condición para obtener ventajas de mano de obra, ¿o quizás llegó el momento de liberarlos?\n\n":
                r = "Foram escravos a vida inteira e, mesmo nestas circunstâncias, ainda se comportam como tal. A situaçío convida a aproveitar sua condiçío para obter vantagens de mío de obra, ou talvez tenha chegado a hora de libertá-los?\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +50 Capacidad de Carga\n\n-Cada descanso juntan 10-15 Materiales.\n\n-Cada Viaje se pierden 2 de Esperanza.\n\n-Al ser liberados, se convierten en Civiles comunes y otorgan +25 Esperanza.":
                r = "EFEITOS PASSIVOS:\n\n-Concedem +50 de Capacidade de Carga\n\n-A cada descanso, juntam 10-15 Materiais.\n\n-A cada Viagem, perdem-se 2 de Esperança.\n\n-Ao serem libertados, tornam-se Civis comuns e concedem +25 Esperança.";
                break;
            case "-Los Esclavos han sido liberados y ahora son Civiles comunes. +25 Esperanza":
                r = "-Os Escravos foram libertados e agora sío Civis comuns. +25 Esperança";
                break;
            case "Tamaño Tiendas: ":
                r = "Tamanho das Lojas: ";
                break;
            case "-El Séquito de Mercaderes ha actualizado su oferta.":
                r = "-O Séquito de Mercadores atualizou sua oferta.";
                break;
            case " es escondido en las sombras tras recibir un ataque crí­tico por su Armadura de Velo.":
                r = " se esconde nas sombras após receber um ataque crí­tico por sua Armadura de Véu.";
                break;
            case "Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periódicamente parte de su riqueza para asegurarse de que no serán abandonados.\n\n":
                r = "Um grupo de nobres que foi obrigado a abandonar o conforto de suas terras agora viaja junto á caravana. Embora sejam queixosos e nío tenham grande utilidade, ao menos doam periodicamente parte de sua riqueza para garantir que nío serío abandonados.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Cada dí­a donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.":
                r = "EFEITOS PASSIVOS:\n\n-A cada dia, doam Ouro equivalente a 1/3 da Esperança.\n\n-Perdem-se 2 de Esperança ao viajar com fadiga 4 ou maior.";
                break;
            case "Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayorí­a murieron en la onda expansiva en ese momento, pero todaví­a quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n":
                r = "Os Clérigos do Sol Radiante Purificador participaram como apoio no combate contra o Lich. A maioria morreu na onda de choque naquele momento, mas ainda restam alguns grupos tentando chegar ao porto e sobreviver enquanto lutam para atrasar o Alento Negro.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a nivel superior a 16, los Clérigos mueren.":
                r = "EFEITOS PASSIVOS:\n\n-Concedem 15 de Esperança ao se juntar á Caravana, -20 de Esperança ao serem perdidos.\n\n-20% de probabilidade de atrasar o Alento Negro em cada viagem.\n\n-Se o Alento Negro chegar a um ní­vel superior a 16, os Clérigos morrem.";
                break;
            case "<color=red>La plegaria ya fue realizada.</color>":
                r = "<color=red>A prece já foi realizada.</color>";
                break;
            case "<color=red>No hay oro suficiente para una donación de 250 Oro.</color>":
                r = "<color=red>Nío há ouro suficiente para uma doaçío de 250 Ouro.</color>";
                break;
            case "Se hará una donación de 250 Oro.":
                r = "Será feita uma doaçío de 250 Ouro.";
                break;
            case "Mantenimiento Armas: El Herrero se encargará de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daño en 2. Este efecto Dura 3 dí­as.":
                r = "Manutençío de Armas: O Ferreiro se encarregará de fazer uma manutençío geral nas armas dos personagens. Aumentando seu Ataque em 1 e seu dano em 2. Este efeito dura 3 dias.";
                break;
            case "Mantenimiento Armaduras: El Herrero se encargará de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 dí­as.":
                r = "Manutençío de Armaduras: O Ferreiro se encarregará de fazer uma manutençío geral nas armaduras dos personagens. Aumentando sua Defesa em 1 e sua Armadura em 2. Este efeito dura 3 dias.";
                break;
            case "Realizar: 200 Oro":
                r = "Realizar: 200 Ouro";
                break;
            case "Activo por ":
                r = "Ativo por ";
                break;
            case " Dí­as":
                r = " Dias";
                break;
            case "Armas Civiles: El herrero se dedica a mejorar las armas rudimentarias de los civiles, mejorando las posibilidades de defensa de las Milicias. \nCada Tier aumenta en 10% los Civiles que suman fuerza para la Milicia.":
                r = "Armas Civis: O ferreiro se dedica a melhorar as armas rudimentares dos civis, aumentando as possibilidades de defesa das Milí­cias. \nCada Tier aumenta em 10% os Civis que somam força para a Milí­cia.";
                break;
            case "Estos soldados abandonaron su puesto en el ejército en pos de sobrevivir. Hambrientos y avergonzados, ofrecen protección a la Caravana pidiendo solo un lugar en ella, aunque a una parte de los civiles les desagrade la idea.\n\n":
                r = "Esses soldados abandonaram seu posto no exército para sobreviver. Famintos e envergonhados, oferecem proteçío á Caravana pedindo apenas um lugar nela, embora parte dos civis nío goste da ideia.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Participan en la defensa de la Caravana, reemplazando a los inexpertos Milicianos. \n\n-Otorga 10 Experiencia extra a Personajes que Entrenan. \n\n-Al aceptarlos la Esperanza disminuye en 8.":
                r = "EFEITOS PASSIVOS:\n\n-Participam da defesa da Caravana, substituindo os Milicianos inexperientes. \n\n-Concedem 10 de Experiência extra a Personagens que Treinam. \n\n-Ao aceitá-los, a Esperança diminui em 8.";
                break;
            case "Varios civiles que estuvieron a la deriva mucho tiempo buscando sobrevivir. Compuesto de mayormente de ancianos, mujeres y niños desnutridos. Consumen menos comida de lo normal y su presencia llena de regocijo a la Caravana porque se hizo lo correcto al recibirlos. Ahora habrá que cuidar de ellos.\n\n":
                r = "Vários civis que ficaram á deriva por muito tempo tentando sobreviver. O grupo é formado principalmente por idosos, mulheres e crianças desnutridas. Consomem menos comida que o normal, e sua presença enche a Caravana de alegria porque foi feito o certo ao recebê-los. Agora será preciso cuidar deles.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Consumen la mitad de Suministros que los Civiles habituales. \n\n-Al aceptarlos la Esperanza aumenta en 30. \n\n-Al perderlos la Esperanza disminuye en 40.":
                r = "EFEITOS PASSIVOS:\n\n-Consomem metade dos Suprimentos dos Civis comuns. \n\n-Ao aceitá-los, a Esperança aumenta em 30. \n\n-Ao perdê-los, a Esperança diminui em 40.";
                break;
            case "Un grupo de especialistas en recolectar hierbas y crear con ellas bélsamos especiales para vender. \nAdemás, sus hierbas proporcionarán beneficios curativos a la caravana.\nPero quizás no sean demasiado cuidadosos al adentrarse en zonas peligrosas para recolectar hierbas.\n\n":
                r = "Um grupo de especialistas em coletar ervas e criar com elas bálsamos especiais para vender. \nAlém disso, suas ervas proporcionarío benefí­cios curativos á caravana.\nMas talvez nío sejam cuidadosos demais ao entrar em áreas perigosas para coletar ervas.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Hierbas curativas: Mejoran ":
                r = "EFEITOS PASSIVOS:\n\n-Ervas curativas: Melhoram ";
                break;
            case "% la curación pasiva de la Caravana.\n\nEste í­ndice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.":
                r = "% a cura passiva da Caravana.\n\nEsse í­ndice aumenta em 3% cada vez que a Caravana visita uma Clareira.\n\n-Às vezes sío descuidados ao coletar ervas. +2% de chance de ocorrer um ataque á caravana após descansar.";
                break;
            case "50 de oro":
                r = "50 de ouro";
                break;
            case "El séquito de Herreros se encarga del mantenimiento y manufactura de las armas y armaduras de la Caravana. Su carro es especialmente pesado ya que, montado ingeniosamente, carga con todas las necesidades básicas de un herrero":
                r = "O séquito de Ferreiros se encarrega da manutençío e fabricaçío das armas e armaduras da Caravana. Sua carroça é especialmente pesada, pois, montada com engenho, carrega tudo o que um ferreiro precisa de básico";
                break;
            case "Cantidad de Civiles: No.":
                r = "Quantidade de Civis: Nío.";
                break;
            case "Civiles representados: ":
                r = "Civis representados: ";
                break;
            case "Civiles representados: No.":
                r = "Civis representados: Nío.";
                break;
            case "150 Oro":
                r = "150 Ouro";
                break;
            case "300 Oro":
                r = "300 Ouro";
                break;
            case "El Séquito de Curanderos se encarga de atender a los heridos y enfermos de la Caravana. Pese a las circunstancias del viaje mismo, logran mantenerse en funcionamiento y brindan un servicio escencial para la supervivencia de quienes lo necesiten.":
                r = "O Séquito de Curandeiros se encarrega de atender os feridos e doentes da Caravana. Apesar das circunstâncias da própria viagem, conseguem se manter em funcionamento e oferecem um serviço essencial para a sobrevivência de quem precisar.";
                break;
            case "Tratar Heridas":
                r = "Tratar Feridas";
                break;
            case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mí­nimo.";
                break;
            case "Aumentar el tamaño de las tiendas incrementa la cantidad de objetos ofrecidos.":
                r = "Aumentar o tamanho das lojas aumenta a quantidade de itens oferecidos.";
                break;
            case "Varios artistas y miembros de una feria ambulante se han unido a la caravana, si bien son ostentosos y despilfarran recursos, pueden ayudar a la moral de la caravana en determinadas ocasiones festivas.":
                r = "Vários artistas e membros de uma feira ambulante se juntaram á caravana. Embora sejam espalhafatosos e desperdicem recursos, podem ajudar a moral da caravana em determinadas ocasiões festivas.";
                break;
            case "Cantidad de Civiles: 25":
                r = "Quantidade de Civis: 25";
                break;
            case "EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada dí­a hay un 30% de chances de que hagan un festán y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.":
                r = "EFEITOS PASSIVOS:\n\n-Ao se juntar á Caravana, ganham-se 15 de Esperança.\n\n-Cada vez que Feira é selecionada como Tarefa Civil de Descanso, ganham-se 10 de Esperança extra.\n\n-A cada dia, há 30% de chance de fazerem um banquete e desperdiçarem 1-4 Suprimentos.\n\n-Se abandonarem a Caravana, perdem-se 15 de Esperança.";
                break;
            case "usa ":
                r = "usa ";
                break;
            case " (-1 Lluvia)":
                r = " (-1 Chuva)";
                break;
            case " (-2 Niebla)":
                r = " (-2 Névoa)";
                break;
            case "<b>Pifia</b>":
                r = "<b>Falha Crí­tica</b>";
                break;
            case "-Tirada de Ataque: 1d20 = ":
                r = "-Rolagem de Ataque: 1d20 = ";
                break;
            case ". Resultado: Pifia.":
                r = ". Resultado: Falha Crí­tica.";
                break;
            case ". Resultado: Fallo.":
                r = ". Resultado: Erro.";
                break;
            case ". Resultado: Roce.":
                r = ". Resultado: Raspío.";
                break;
            case ". Resultado: Golpe.":
                r = ". Resultado: Acerto.";
                break;
            case "Fallo":
                r = "Erro";
                break;
            case "Pifia":
                r = "Falha Crí­tica";
                break;
            case "Nido Defensivo":
                r = "Ninho Defensivo";
                break;
            case "Al Acecho":
                r = "À Espreita";
                break;
            case "Arma Envenenada":
                r = "Arma Envenenada";
                break;
            case "Desestabilizado":
                r = "Desestabilizado";
                break;
            case "<b>¿Enfurecido!</b>":
                r = "<b>Enfurecido!</b>";
                break;
            case "Sangre Devorada":
                r = "Sangue Devorado";
                break;
            case "Eufórico":
                r = "Eufórico";
                break;
            case "Sangre Contaminada":
                r = "Sangue Contaminado";
                break;
            case "Aturdido por Chirrido":
                r = "Atordoado por Guincho";
                break;
            case "Atemorizado":
                r = "Amedrontado";
                break;
            case "Enredado":
                r = "Enredado";
                break;
            case "Enredadera Ardiente":
                r = "Videira Ardente";
                break;
            case "En plano material":
                r = "No plano material";
                break;
            case "Perdición":
                r = "Perdiçío";
                break;
            case "Encarnado":
                r = "Encarnado";
                break;
            case " está encarnado y no puede actuar este turno.":
                r = " está encarnado e nío pode agir neste turno.";
                break;
            case "Aullido de la Manada":
                r = "Uivo da Matilha";
                break;
            case "Furia":
                r = "Fúria";
                break;
            case "Sorprendido":
                r = "Surpreendido";
                break;
            case "Acalorado":
                r = "Acalorado";
                break;
            case "Mojado":
                r = "Molhado";
                break;
            case "Frí­o":
                r = "Frio";
                break;
            case "Aliento Negro: Débil":
                r = "Alento Negro: Fraco";
                break;
            case "Aliento Negro: Presente":
                r = "Alento Negro: Presente";
                break;
            case "Aliento Negro: Fuerte":
                r = "Alento Negro: Forte";
                break;
            case "Aliento Negro: Empoderante":
                r = "Alento Negro: Fortalecedor";
                break;
            case "Oscuridad":
                r = "Escuridío";
                break;
            case "Fatigado":
                r = "Fatigado";
                break;
            case "Bendecido por Plegaria":
                r = "Abençoado por Prece";
                break;
            case "Herido":
                r = "Ferido";
                break;
            case "Enfermo":
                r = "Doente";
                break;
            case "Baja Moral":
                r = "Moral Baixa";
                break;
            case "Alta Moral":
                r = "Moral Alta";
                break;
            case "Armadura Cuidada":
                r = "Armadura Bem Cuidada";
                break;
            case "Fresco":
                r = "Disposto";
                break;
            case "Flechas Preparadas":
                r = "Flechas Preparadas";
                break;
            case "Fatigado por Explorar":
                r = "Fatigado por Explorar";
                break;
            case "Arma Afilada":
                r = "Arma Afiada";
                break;
            case "Invulnerable":
                r = "Invulnerável";
                break;
            case "Desplazado":
                r = "Deslocado";
                break;
            case "Condenado":
                r = "Condenado";
                break;
            case "Escudado por Fe":
                r = "Protegido pela Fé";
                break;
            case "Descansado":
                r = "Descansado";
                break;
            case "Etereo":
                r = "Etéreo";
                break;
            case "Escondido Por Humo":
                r = "Escondido pela Fumaça";
                break;
            case "Motivado":
                r = "Motivado";
                break;
            case "Euforia":
                r = "Euforia";
                break;
            case "Desmotivado":
                r = "Desmotivado";
                break;
            case "Desesperanzado":
                r = "Sem Esperança";
                break;
            case "Cobertura de Barricada":
                r = "Cobertura de Barricada";
                break;
            case "Hombro Con Hombro":
                r = "Ombro a Ombro";
                break;
            case "Masacre":
                r = "Massacre";
                break;
            case "Aterrorizado":
                r = "Aterrorizado";
                break;
            case "Consumevida":
                r = "Consomevida";
                break;
			case "Incapacitado":
                r = "Incapacitado";
                break;
            case "Distraí­do":
                r = "Distraí­do";
                break;
            case "Implacable":
                r = "Implacável";
                break;
            case "Determinación":
                r = "Determinaçío";
                break;
            case "Grito Motivador":
                r = "Grito Motivador";
                break;
            case "Grito Desmotivador":
                r = "Grito Desmotivador";
                break;
            case "Postura Defensiva":
                r = "Postura Defensiva";
                break;
            case "Amedrentado":
                r = "Amedrontado";
                break;
            case "Acumulando":
                r = "Acumulando";
                break;
            case " falló la Tirada de Concentración y ya no acumula energí­a.":
                r = " falhou na Jogada de Concentraçío e nío está mais acumulando energia.";
                break;
            case "Energizado":
                r = "Energizado";
                break;
            case "Acumulacion Inestable":
                r = "Acumulaçío Instável";
                break;
            case "Escudo Energético":
                r = "Escudo Energético";
                break;
            case "Energí­a Absorbida":
                r = "Energia Absorvida";
                break;
            case "Residuo Energético":
                r = "Resí­duo Energético";
                break;
            case "Reconocimiento":
                r = "Reconhecimento";
                break;
            case "Presa Completada":
                r = "Presa Concluí­da";
                break;
            case "Vista Lejana I":
                r = "Visío Distante I";
                break;
            case "Vista Lejana II":
                r = "Visío Distante II";
                break;
            case "Vista Lejana III":
                r = "Visío Distante III";
                break;
            case "Vista Lejana IVa":
                r = "Visío Distante IVa";
                break;
            case "Vista Lejana IVb":
                r = "Visío Distante IVb";
                break;
            case "Flechas de Fuego":
                r = "Flechas de Fogo";
                break;
            case "Ralentizado":
                r = "Lentificado";
                break;
            case "Acechando":
                r = "À Espreita";
                break;
            case "Marcando Presa":
                r = "Marcando Presa";
                break;
            case "Afligida I":
                r = "Afligida I";
                break;
            case "Afligida II":
                r = "Afligida II";
                break;
            case "Afligida III":
                r = "Afligida III";
                break;
            case "Afligida IV":
                r = "Afligida IV";
                break;
            case "Fervor":
                r = "Fervor";
                break;
            case "Aura Sagrada":
                r = "Aura Sagrada";
                break;
            case "Ciego":
                r = "Cego";
                break;
            case " de ":
                r = " de ";
                break;
            case " remueve ":
                r = " remove ";
                break;
            case "Bálsamo de Claridad":
                r = "Bálsamo de Clareza";
                break;
            case "Bálsamo Energizante":
                r = "Bálsamo Energizante";
                break;
            case "Bálsamo Fortalecedor":
                r = "Bálsamo Fortalecedor";
                break;
            case "Elixir de Resistencia al Frí­o":
                r = "Elixir de Resistência ao Frio";
                break;
            case "Elixir de Resistencia al Fuego":
                r = "Elixir de Resistência ao Fogo";
                break;
            case "Elixir de Resistencia al Fuego.":
                r = "Elixir de Resistência ao Fogo";
                break;
            case "Elixir de Resistencia al Rayo":
                r = "Elixir de Resistência ao Raio";
                break;
            case "Elixir de Resistencia al Ácido":
                r = "Elixir de Resistência ao Ácido";
                break;
            case "Protección Arcana":
                r = "Proteçío Arcana";
                break;
            case "IA Lenguetazo":
                r = "IA Lambida";
                break;
            case "Saboreado":
                r = "Saboreado";
                break;
            case "recibirá más daño del Zarkilever":
                r = "receberá mais dano do Zarkilever";
                break;
            case "Armadura Rota":
                r = "Armadura Quebrada";
                break;
            case "Potenciado por Masa Contaminada":
                r = "Fortalecido por Massa Contaminada";
                break;
            case "Herida":
                r = "Ferida";
                break;
            case "Ardiendo: causa daño cada turno, se apaga con AP disponibles.":
                r = "Em Chamas: causa dano a cada turno e pode ser apagado com PA disponí­veis.";
                break;
            case "Aturdido: no puede actuar.":
                r = "Atordoado: nío pode agir.";
                break;
            case "Ácido: cada acumulación reduce en 1 la armadura.":
                r = "Ácido: cada acúmulo reduz a armadura em 1.";
                break;
            case "Congelado: reduce PA disponibles y aumenta armadura.":
                r = "Congelado: reduz os PA disponí­veis e aumenta a armadura.";
                break;
            case "Resistencias Reducidas: reduce todas las resistencias 1 por acumulación.":
                r = "Resistências Reduzidas: reduz todas as resistências em 1 por acúmulo.";
                break;
            case "Armadura Rota: reduce la armadura en 1 por acumulación.":
                r = "Armadura Quebrada: reduz a armadura em 1 por acúmulo.";
                break;
            case "Sangrado: cada acumulación resta 1 HP máxima por turno y previene 2 de curación.":
                r = "Sangramento: cada acúmulo reduz 1 de HP máximo por turno e impede 2 de cura.";
                break;
            case "Veneno: provoca daño por turno, se debe hacer una tirada de salvación de Fortaleza cada turno para curarse, si falla se incrementa en 1.":
                r = "Veneno: causa dano por turno; é preciso fazer uma jogada de resistência de Fortaleza a cada turno para se curar; se falhar, aumenta em 1.";
                break;
            case "Regeneración: recupera vida cada turno.":
                r = "Regeneraçío: recupera vida a cada turno.";
                break;
            case "Regeneración Armadura: recupera Armadura perdida cada turno.":
                r = "Regeneraçío de Armadura: recupera a Armadura perdida a cada turno.";
                break;
            case "Evasión: cada stack aumenta 1 la Defensa, se elimina al recibir daño.":
                r = "Evasío: cada acúmulo aumenta a Defesa em 1 e é removido ao receber dano.";
                break;
            case "Flechas: Cantidad de flechas disponibles.":
                r = "Flechas: Quantidade de flechas disponí­veis.";
                break;
            case " Bonus daño elemental Acido.":
                r = " Bônus de dano elemental de Ácido.";
                break;
            case " Bonus daño elemental Arcano.":
                r = " Bônus de dano elemental Arcano.";
                break;
            case " Bonus daño elemental Fuego.":
                r = " Bônus de dano elemental de Fogo.";
                break;
            case " Bonus daño elemental Hielo.":
                r = " Bônus de dano elemental de Gelo.";
                break;
            case " Bonus daño elemental Necro.":
                r = " Bônus de dano elemental Necrótico.";
                break;
            case " Bonus daño elemental Divino.":
                r = " Bônus de dano elemental Divino.";
                break;
            case " Bonus daño elemental Rayo.":
                r = " Bônus de dano elemental de Raio.";
                break;
            case "Fervor: Cantidad de Fervor que tiene la purificadora.":
                r = "Fervor: Quantidade de Fervor que a Purificadora possui.";
                break;
            case "Barrera: previene X cantidad de daño.":
                r = "Barreira: previne X quantidade de dano.";
                break;
            case "Residuo de Tejido: se obtiene al recibir curación de origen mágico. Previene X puntos de curación.":
                r = "Resí­duo de Tecido: é obtido ao receber cura de origem mágica. Impede X pontos de cura.";
                break;
            case "Escondido I: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto se remueve al atacar o recibir daño.":
                r = "Escondido I: Esta unidade está escondida e os inimigos nío podem atacá-la. O efeito é removido ao atacar ou receber dano.";
                break;
            case "Escondido II: Esta unidad está escondida y los enemigos no pueden atacarla. El efecto no se remueve al recibir daño.":
                r = "Escondido II: Esta unidade está escondida e os inimigos nío podem atacá-la. O efeito nío é removido ao receber dano.";
                break;
            case "Energí­a: Nivel de Energí­a Acumulada por el Canalizador.":
                r = "Energia: Ní­vel de Energia acumulada pelo Canalizador.";
                break;
            case "Corrupto: Recibe daño adicional de enemigos Corrompidos que además se curan al dañarlo. Si lo deja fuera de combate un enemigo corrompido, muere.":
                r = "Corrompido: Recebe dano adicional de inimigos Corrompidos, que também se curam ao feri-lo. Se for deixado fora de combate por um inimigo corrompido, morre.";
                break;
            case "HP Máximo: ":
                r = "HP Máximo: ";
                break;
            case "PA Máximo: ":
                r = "PA Máximo: ";
                break;
            case "PM Máximo: ":
                r = "PM Máximo: ";
                break;
            case "Resistencia Fuego: ":
                r = "Resistência a Fogo: ";
                break;
            case "Resistencia Hielo: ":
                r = "Resistência a Gelo: ";
                break;
            case "Resistencia Rayo: ":
                r = "Resistência a Raio: ";
                break;
            case "Resistencia Ácido: ":
                r = "Resistência a Ácido: ";
                break;
            case "Resistencia Arcano: ":
                r = "Resistência Arcana: ";
                break;
            case "Resistencia Necrótica: ":
                r = "Resistência Necrótica: ";
                break;
            case "Resistencia Divina: ":
                r = "Resistência Divina: ";
                break;
            case "Barrera inicial: ":
                r = "Barreira inicial: ";
                break;
            case "Evasion inicial: ":
                r = "Evasío inicial: ";
                break;
            case "Penetracion armadura: ":
                r = "Penetraçío de armadura: ";
                break;
            case "Reduccion dano recibido: ":
                r = "Reduçío de dano recebido: ";
                break;
            case "Reduccion dano critico recibido: ":
                r = "Reduçío de dano crí­tico recebido: ";
                break;
            case "Resistencia estados: ":
                r = "Resistência a estados: ";
                break;
            case "Espinas dano plano: ":
                r = "Espinhos dano fixo: ";
                break;
            case "Espinas dano %: ":
                r = "Espinhos dano %: ";
                break;
            case "Daño: ":
                r = "Dano: ";
                break;
            case "Crí­tico Dado: ":
                r = "Crí­tico causado: ";
                break;
            case "Daño Crí­tico: ":
                r = "Dano Crí­tico: ";
                break;
            case "TS Reflejos: ":
                r = "TS Reflexos: ";
                break;
            case "TS Fortaleza: ":
                r = "TS Fortaleza: ";
                break;
            case "TS Mental: ":
                r = "TS Mental: ";
                break;
            case "Bonus daño Ácido: ":
                r = "Bônus de dano Ácido: ";
                break;
            case "Bonus daño arcano: ":
                r = "Bônus de dano arcano: ";
                break;
            case "Bonus daño fuego: ":
                r = "Bônus de dano fogo: ";
                break;
            case "Bonus daño hielo: ":
                r = "Bônus de dano gelo: ";
                break;
            case "Bonus daño necro: ":
                r = "Bônus de dano necro: ";
                break;
            case "Bonus daño rayo: ":
                r = "Bônus de dano raio: ";
                break;
            case "Duración: ":
                r = "Duraçío: ";
                break;
            case "Duración: Permanente\n":
                r = "Duraçío: Permanente\n";
                break;
            case " rondas\n":
                r = " rodadas\n";
                break;
            case "Valentí­a Global Alta":
                r = "Bravura Global Alta";
                break;
            case "Valentia Global Alta":
                r = "Bravura Global Alta";
                break;
            case "Valentí­a Global Muy Alta":
                r = "Bravura Global Muito Alta";
                break;
            case "Valentia Global Muy Alta":
                r = "Bravura Global Muito Alta";
                break;
            case "Dudando":
                r = "Hesitante";
                break;
            case "La moral colectiva impulsa al grupo. +1 PA máximo esta ronda.":
                r = "A moral coletiva impulsiona o grupo. +1 PA máximo nesta rodada.";
                break;
            case "La moral colectiva desborda. +15% daño y +1 PA máximo esta ronda.":
                r = "A moral coletiva transborda. +15% de dano e +1 PA máximo nesta rodada.";
                break;
            case "La moral flaquea por la presión del combate.":
                r = "A moral vacila sob a pressío do combate.";
                break;
            case "Ataque: ":
                r = "Ataque: ";
                break;
            case "Defensa: determina capacidad para evadir ataques.":
                r = "Defesa: determina a capacidade de evitar ataques.";
                break;
            case "Armadura: reduce el daño fí­sico recibido.":
                r = "Armadura: reduz o dano fí­sico recebido.";
                break;
            case "Reflejos: resistencia a determinados efectos de ataques.":
                r = "Reflexos: resistência a determinados efeitos de ataques.";
                break;
            case "Fortaleza: resistencia a efectos fí­sicos.":
                r = "Fortaleza: resistência a efeitos fí­sicos.";
                break;
            case "Mental: resistencia a efectos mentales.":
                r = "Mental: resistência a efeitos mentais.";
                break;
            case "Valentí­a: moral general en combate.":
                r = "Bravura: moral geral em combate.";
                break;
            case "Resistencia al Fuego: Cantidad de daño que previene.":
                r = "Resistência a Fogo: quantidade de dano que previne.";
                break;
            case "Resistencia al Frí­o: Cantidad de daño que previene.":
                r = "Resistência a Frio: quantidade de dano que previne.";
                break;
            case "Resistencia al Rayo: Cantidad de daño que previene.":
                r = "Resistência a Raio: quantidade de dano que previne.";
                break;
            case "Resistencia al Ácido: Cantidad de daño que previene.":
                r = "Resistência a Ácido: quantidade de dano que previne.";
                break;
            case "Resistencia Arcana: Cantidad de daño que previene.":
                r = "Resistência Arcana: quantidade de dano que previne.";
                break;
            case "Resistencia Necrótica: Cantidad de daño que previene.":
                r = "Resistência Necrótica: quantidade de dano que previne.";
                break;
            case "Resistencia Divina: Cantidad de daño que previene.":
                r = "Resistência Divina: quantidade de dano que previne.";
                break;
            case "Residuo Energético: Otorga daño arcano y hiere levemente.":
                r = "Resí­duo Energético: concede dano arcano e fere levemente.";
                break;
            case "Zona bajo Vigilancia del Explorador.":
                r = "Área sob Vigilância do Explorador.";
                break;
            case "Añade daño fuego al Explorador si está adyacente.":
                r = "Adiciona dano de fogo ao Explorador se estiver adjacente.";
                break;
            case "Abrojos: Inflige daño y puede desangrar.":
                r = "Abrolhos: infligem dano e podem causar sangramento.";
                break;
            case "Eco Divino: Cura a aliados y daña a enemigos.":
                r = "Eco Divino: cura aliados e causa dano aos inimigos.";
                break;
            case "Humo: Esconde a los personajes dentro.":
                r = "Fumaça: esconde os personagens dentro dela.";
                break;
            case "Escudo de Fe: Protege a los aliados dentro.":
                r = "Escudo de Fé: protege os aliados dentro dele.";
                break;
            case "Masa Contaminada: Hace daño Ácido. Potencia enemigos corruptos.":
                r = "Massa Contaminada: causa dano de Ácido. Fortalece inimigos corrompidos.";
                break;
            case "Pinchos: Daña a enemigos que los pisen.":
                r = "Espinhos: causam dano aos inimigos que pisarem neles.";
                break;
            case "Barricada: Obstáculo para enemigos. Hiere al ser atacada.":
                r = "Barricada: obstáculo para inimigos. Fere ao ser atacada.";
                break;
            case "Puesto de Tiro: Aumenta ataque y defensa a aliados dentro.":
                r = "Posto de Tiro: aumenta ataque e defesa dos aliados dentro dele.";
                break;
            case "Pilar de Luz: Obstáculo que daña a enemigos al ser atacado.":
                r = "Pilar de Luz: obstáculo que causa dano aos inimigos ao ser atacado.";
                break;
            case "Fin del Tutorial":
                r = "Fim do Tutorial";
                break;
            case "Nueva Partida":
                r = "Novo Jogo";
                break;
            case "Opciones":
                r = "Opções";
                break;
            case "Debes reiniciar para que tenga efecto.":
                r = "Você deve reiniciar para que tenha efeito.";
                break;
            case "<i>Los Caballeros siempre andan equipados con un mandoble muy pesado y poderoso. Junto con su armadura pesada, hacen el núcleo del equipo de estos valientes guerreros.</i><b>\n\nOtorga: Corte Vertical</b>":
                r = "<i>Os Cavaleiros sempre andam equipados com um montante muito pesado e poderoso. Junto com sua armadura pesada, formam o núcleo do equipamento desses valentes guerreiros.</i><b>\n\nConcede: Corte Vertical</b>";
                break;
            case "Mandoble":
                r = "Montante";
                break;
            case "Armadura de Cuero Reforzado":
                r = "Armadura de Couro Reforçado";
                break;
            case "Armadura de Cuero Reforzado +1":
                r = "Armadura de Couro Reforçado +1";
                break;
            case "Armadura de Cuero Reforzado +2":
                r = "Armadura de Couro Reforçado +2";
                break;
            case "Armadura de Cuero Reforzado +3":
                r = "Armadura de Couro Reforçado +3";
                break;
            case "Armadura de Cuero Reforzado de Ligereza +1":
                r = "Armadura de Couro Reforçado de Leveza +1";
                break;
            case "Armadura de Cuero Reforzado de Protección Elemental +1":
                r = "Armadura de Couro Reforçado de Proteçío Elemental +1";
                break;
            case "Precio: ":
                r = "Preço: ";
                break;
            case "Efectos del item:":
                r = "Efeitos do item:";
                break;
            case "Rareza: ":
                r = "Raridade: ";
                break;
            case "Tipo de item: ":
                r = "Tipo de item: ";
                break;
            case "Accesorio":
                r = "Acessório";
                break;
            case "Consumible":
                r = "Consumí­vel";
                break;
            case "Baculo":
                r = "Cajado";
                break;
            case "Vestidura":
                r = "Vestimenta";
                break;
            case "Común":
                r = "Comum";
                break;
            case "Infrecuente":
                r = "Incomum";
                break;
            case "Raro":
                r = "Raro";
                break;
            case "Épico":
                r = "Épico";
                break;
            case "Legendario":
                r = "Lendário";
                break;
            case "Artefacto":
                r = "Artefato";
                break;
            case "Desconocida":
                r = "Desconhecida";
                break;
            case "Agrega habilidad: ":
                r = "Adiciona habilidade: ";
                break;
            case "Armadura de Cuero Reforzado de Velo +2":
                r = "Armadura de Couro Reforçado de Véu +2";
                break;
            case "Espada Corta":
                r = "Espada Curta";
                break;
            case "Espada Corta +1":
                r = "Espada Curta +1";
                break;
            case "Espada Corta +2":
                r = "Espada Curta +2";
                break;
            case "Espada Corta +3":
                r = "Espada Curta +3";
                break;
            case "Espada Corta Arcana +1":
                r = "Espada Curta Arcana +1";
                break;
            case "Espada Corta Filonegro +1":
                r = "Espada Curta Fio Negro +1";
                break;
            case "Espada Corta Consumevida":
                r = "Espada Curta Consomevida";
                break;
            case "Coraza":
                r = "Cota";
                break;
            case "Coraza +1":
                r = "Cota +1";
                break;
            case "Coraza +2":
                r = "Cota +2";
                break;
            case "Coraza +3":
                r = "Cota +3";
                break;
            case "Coraza de Llamas +1":
                r = "Cota das Chamas +1";
                break;
            case "Coraza Liviana":
                r = "Cota Leve";
                break;
            case "Coraza de Fuerza de Gigante +2":
                r = "Cota da Força de Gigante +2";
                break;
            case "Mandoble +1":
                r = "Montante +1";
                break;
            case "Mandoble +2":
                r = "Montante +2";
                break;
            case "Mandoble +3":
                r = "Montante +3";
                break;
            case "Mandoble Sagrado +1":
                r = "Montante Sagrado +1";
                break;
            case "Mandoble Congelado  +2":
                r = "Montante Congelado +2";
                break;
            case "Armadura de Cuero":
                r = "Armadura de Couro";
                break;
            case "Armadura de Cuero +1":
                r = "Armadura de Couro +1";
                break;
            case "Armadura de Cuero +2":
                r = "Armadura de Couro +2";
                break;
            case "Armadura de Cuero +3":
                r = "Armadura de Couro +3";
                break;
            case "Armadura de Cuero de Fortaleza +1":
                r = "Armadura de Couro de Fortaleza +1";
                break;
            case "Armadura de Cuero necrótico +1":
                r = "Armadura de Couro Necrí­tica +1";
                break;
            case "Armadura de Cuero Borrosa +2":
                r = "Armadura de Couro Nebulosa +2";
                break;
            case "Arco Largo":
                r = "Arco Longo";
                break;
            case "Arco Largo +1":
                r = "Arco Longo +1";
                break;
            case "Arco Largo +2":
                r = "Arco Longo +2";
                break;
            case "Arco Largo +3":
                r = "Arco Longo +3";
                break;
            case "Arco Largo Ácido +1":
                r = "Arco Longo Ácido +1";
                break;
            case "Arco Largo Potente +1":
                r = "Arco Longo Potente +1";
                break;
            case "Arco Largo Ralentizante +2":
                r = "Arco Longo Lentificante +2";
                break;
            case "Báculo Purificador":
                r = "Cajado Purificador";
                break;
            case "Báculo Purificador +1":
                r = "Cajado Purificador +1";
                break;
            case "Báculo Purificador +2":
                r = "Cajado Purificador +2";
                break;
            case "Báculo Purificador +3":
                r = "Cajado Purificador +3";
                break;
            case "Poción de Curación Menor":
                r = "Poçío de Cura Menor";
                break;
            case "Poción de Curación Mayor":
                r = "Poçío de Cura Maior";
                break;
            case "Poción de Curación":
                r = "Poçío de Cura";
                break;
            case "<Color=#e6b50f>\nPrecio: ":
                r = "<Color=#e6b50f>\nPreço: ";
                break;
            case "<Color=#e60f0f>\nPrecio: ":
                r = "<Color=#e60f0f>\nPreço: ";
                break;
            case "\n\n- Has encontrado un objeto de recompensa: ":
                r = "\n\n- Você encontrou um item de recompensa: ";
                break;
            case "\n\n-Los enemigos han eliminado al ":
                r = "\n\n-Os inimigos eliminaram o ";
                break;
            case " luego de la Batalla.":
                r = " após a Batalha.";
                break;
            case "Se han obtenido ":
                r = "Foram obtidos ";
                break;
            case " Oro, ":
                r = " Ouro, ";
                break;
            case " Materiales y +":
                r = " Materiais e +";
                break;
            case " Esperanza.":
                r = " Esperança.";
                break;
            case "+2 TS Mental por todo el combate.":
                r = "+2 TS Mental durante todo o combate.";
                break;
            case "+2 TS Reflejos por todo el combate.":
                r = "+2 TS Reflexos durante todo o combate.";
                break;
            case "+2 TS Fortaleza por todo el combate.":
                r = "+2 TS Fortaleza durante todo o combate.";
                break;
            case "Panacea":
                r = "Panaceia";
                break;
            case "Sí­mbolo de Protección Arcano":
                r = "Sí­mbolo Arcano de Proteçío";
                break;
            case "Otorga 3 de Resistencia contra todos los elementos. Dura 4 turnos.":
                r = "Concede 3 de Resistência contra todos os elementos. Dura 4 turnos.";
                break;
            case "Restaura 20 + 2d8 puntos de vida.":
                r = "Restaura 20 + 2d8 pontos de vida.";
                break;
            case "Restaura 12 + 1d8 puntos de vida.":
                r = "Restaura 12 + 1d8 pontos de vida.";
                break;
            case "Restaura 6 + 1d6 puntos de vida.":
                r = "Restaura 6 + 1d6 pontos de vida.";
                break;
            case "Aumenta la resistencia al frí­o en 5 por el combate.":
                r = "Aumenta a resistência a frio em 5 durante o combate.";
                break;
            case "Aumenta la resistencia al fuego en 5 por el combate.":
                r = "Aumenta a resistência a fogo em 5 durante o combate.";
                break;
            case "Aumenta la resistencia al rayo en 5 por el combate.":
                r = "Aumenta a resistência a raio em 5 durante o combate.";
                break;
            case "Aumenta la resistencia al Ácido en 5 por el combate.":
                r = "Aumenta a resistência a Ácido em 5 durante o combate.";
                break;
            case "Remueve todos los debuffs de la unidad.":
                r = "Remove todos os debuffs da unidade.";
                break;
            case "Ataque de Espada":
                r = "Ataque de Espada";
                break;
            case "Tiro de Arco":
                r = "Disparo de Arco";
                break;
            case "Golpe Manifestacion":
                r = "Golpe de Manifestaçío";
                break;
            case "Descarga Arcana":
                r = "Descarga Arcana";
                break;
            case "Ataque de Lanza":
                r = "Ataque de Lança";
                break;
            case "Tiro de Ballesta":
                r = "Disparo de Besta";
                break;
            case "Espada Corta Ladrón":
                r = "Espada Curta de Ladrío";
                break;
            case "Envenenar Arma":
                r = "Envenenar Arma";
                break;
            case "Mordida Perro Adiestrado":
                r = "Mordida de Cío Adestrado";
                break;
            case "Empujón Rufián":
                r = "Empurrío de Rufiío";
                break;
            case "Mazo Rufián":
                r = "Golpe de Maça de Rufiío";
                break;
            case "Arrojar Corrosión":
                r = "Arremessar Corrosío";
                break;
            case "Proliferar Corrupción":
                r = "Proliferar Corrupçío";
                break;
            case "Devorar Sangre":
                r = "Devorar Sangue";
                break;
            case "Garra de Devorador":
                r = "Garra de Devorador";
                break;
            case "Ataque de Garra":
                r = "Ataque de Garra";
                break;
            case "Chirrido de Vagranilo":
                r = "Guincho de Vagranilo";
                break;
            case "Mordida Vagranilo":
                r = "Mordida de Vagranilo";
                break;
            case "Chirrido Mayor":
                r = "Guincho Ensurdecedor";
                break;
            case "Mordida Vagranilo Mayor":
                r = "Mordida de Vagranilo Maior";
                break;
            case "Enredadera Espinoza":
                r = "Videira Espinhosa";
                break;
            case "Ataque Raiz":
                r = "Ataque de Raiz";
                break;
            case "Ataque Vaina":
                r = "Ataque de Vagem";
                break;
            case "Crecimiento Espinoso":
                r = "Crescimento Espinhoso";
                break;
            case "Lamento del Bosque":
                r = "Lamento da Floresta";
                break;
            case "Caricia del Bosque":
                r = "Carí­cia da Floresta";
                break;
            case "Enredar":
                r = "Enredar";
                break;
            case "Ráfaga de Espinas":
                r = "Rajada de Espinhos";
                break;
            case "Golpe de Espectro":
                r = "Golpe de Espectro";
                break;
            case "Golpe de Fuego Fatuo":
                r = "Golpe de Fogo-Fátuo";
                break;
            case "Garra Espectral":
                r = "Garra Espectral";
                break;
            case "Mordisco Ardiente":
                r = "Mordida Ardente";
                break;
            case "Reacción: Al morir, enfurecerá a otros Lobos Espectrales.":
                r = "Reaçío: ao morrer, enfurecerá outros Lobos Espectrais.";
                break;
            case "Golpe Enredado":
                r = "Golpe Enredado";
                break;
            case "Lobo Espectral":
                r = "Lobo Espectral";
                break;
            case "<i>El Lobo Espectral es un enemigo feroz que se mueve y ataca rápidamente, mientras su destreza animal le brinda una buena defensa.</i>\n\n<color=#199F10>-Posee un mordisco imbuido en fuego que además de dañar, puede hacer arder a sus enemigos.</color>\n<color=#EE0000>-Estadí­sticas débiles.</color>":
                r = "<i>O Lobo Espectral é um inimigo feroz que se move e ataca rapidamente, enquanto sua destreza animal lhe dá uma boa defesa.</i>\n\n<color=#199F10>-Possui uma mordida imbuí­da em fogo que, além de causar dano, pode incendiar seus inimigos.</color>\n<color=#EE0000>-Estatí­sticas fracas.</color>";
                break;
            case "Lobo Alfa Espectral":
                r = "Lobo Alfa Espectral";
                break;
            case "<i>El Lobo Alfa Espectral es el lí­der de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>":
                r = "<i>O Lobo Alfa Espectral é o lí­der da matilha, possui uma constituiçío mais forte e resistente que a dos outros lobos, embora seja um pouco menos ágil.</i>\n\n<color=#199F10>-Tem a capacidade de uivar para motivar os outros lobos.</color>\n<color=#EE0000>-Se ficar sozinho, nío poderá motivar ninguém.</color>";
                break;
            case "Driada Quemada":
                r = "Drí­ade Queimada";
                break;
            case "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raí­ces igní­fugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Antes servas e cuidadoras da floresta, agora sío manifestações de vingança e ódio contra qualquer invasor da Floresta Ardente.</i>\n\n<color=#199F10>-Pode enredar com raí­zes igní­fugas.\n-Ataque á distância.</color>\n<color=#EE0000>-Relativamente fraca.</color>";
                break;
            case "Espectro del Bosque":
                r = "Espectro da Floresta";
                break;
            case "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques fí­sicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad fí­sica momentáneamente al atacar.</color>":
                r = "<i>O Espectro da Floresta é uma alma penada presa entre as cinzas de uma floresta calcinada, com sua ira alimentada pela destruiçío que nío pôde evitar. Errante e vingativo, ataca aqueles que ousam cruzar sua terra carbonizada.</i>\n\n<color=#199F10>-Imune a ataques fí­sicos.\n-Pode amaldiçoar com Perdiçío.</color>\n<color=#EE0000>-Perde parte de sua imunidade fí­sica momentaneamente ao atacar.</color>";
                break;
            case "Fuego Fatuo":
                r = "Fogo-Fátuo";
                break;
            case "<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guí­a a los incautos hacia la perdición, vengando la memoria del bosque caí­do.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>":
                r = "<i>Um eco etéreo das chamas que o consumiram, dançando entre as cinzas como lembrança do desastre. Embora pareça inofensivo, guia os incautos á perdiçío, vingando a memória da floresta caí­da.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.\n-Pode encarnar em seus inimigos.</color>\n<color=#EE0000>-Tem pouca vida.</color>";
                break;
            case "Treant Espectral":
                r = "Treant Espectral";
                break;
            case "<i>Con su madera marcada y deformada por el fuego, estos antes pastores de árboles ahora deambulan trayendo muerte a los invasores de su hogar.</i>\n\n<color=#199F10>-Buena armadura que se regenera.\n-Puede enredar al golpear a sus enemigos.</color>\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>Com sua madeira marcada e deformada pelo fogo, estes antigos pastores de árvores agora vagueiam levando a morte aos invasores de seu lar.</i>\n\n<color=#199F10>-Boa armadura que se regenera.\n-Pode enredar ao atingir seus inimigos.</color>\n<color=#EE0000>-Fraco contra fogo.</color>";
                break;
            case "Manifestación Arcana":
                r = "Manifestaçío Arcana";
                break;
            case "<i>Constituido por pura energí­a arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.</color>":
                r = "<i>Constituí­do de pura energia arcana, este ser etéreo defende o Canalizador que lhe deu forma.</i>\n\n<color=#199F10>-Resistente a ataques fí­sicos.</color>";
                break;
            case "Vagranilo":
                r = "Vagranilo";
                break;
            case "<i>Un ser volador cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Evasivo.\n-Puede aturdir.\n-Puede atacar a enemigos escondidos.</color>\n<color=#EE0000>-Débil al daño Divino.</color>":
                r = "<i>Um ser voador quase humano oriundo das profundezas, sem visío, mas com uma capacidade auditiva excepcional para compensar.</i>\n\n<color=#199F10>-Evasivo.\n-Pode atordoar.\n-Pode atacar inimigos escondidos.</color>\n<color=#EE0000>-Fraco contra dano Divino.</color>";
                break;
            case "Vagranilo Mayor":
                r = "Vagranilo Maior";
                break;
            case "<i>Un ser terrible cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Chirrido Ensordecedor.\n-Puede atacar a enemigos escondidos.\n-Se cura al morder victimas con Sangre Contaminada.</color>\n<color=#EE0000>-Débil al daño Divino.</color>":
                r = "<i>Um terrí­vel ser quase humano oriundo das profundezas, sem visío, mas com uma capacidade auditiva excepcional para compensar.</i>\n\n<color=#199F10>-Guincho Ensurdecedor.\n-Pode atacar inimigos escondidos.\n-Cura-se ao morder ví­timas com Sangue Contaminado.</color>\n<color=#EE0000>-Fraco contra dano Divino.</color>";
                break;
            case "Ladrón":
                r = "Ladrío";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crí­tico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>":
                r = "<i>Este homem já era maligno antes, e agora a situaçío desesperadora acentuou sua crueldade.</i>\n\n<color=#199F10>-Boa capacidade de Crí­tico.\n-Começa escondido.\n-Pode envenenar sua arma.</color>\n<color=#EE0000>-Bastante fraco.</color>";
                break;
            case "Rufián con Ballesta":
                r = "Rufiío com Besta";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Puede empujar.</color>":
                r = "<i>Este homem já era maligno antes, e agora a situaçío desesperadora acentuou sua crueldade.</i>\n\n<color=#199F10>-Resistente.\n-Pode empurrar.</color>";
                break;
            case "Rufián con Mazo":
                r = "Rufiío com Maça";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Se enfurece.</color>\n<color=#EE0000>-Lento para actuar.</color>":
                r = "<i>Este homem já era maligno antes, e agora a situaçío desesperadora acentuou sua crueldade.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Enfurece-se.</color>\n<color=#EE0000>-Lento para agir.</color>";
                break;
            case "Perro Adiestrado":
                r = "Cío Adestrado";
                break;
            case "<i>Un perro adiestrado para la batalla, fiel a su amo y feroz con sus enemigos.</i>\n\n<color=#199F10>-Puede Inmovilizar al morder.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Um cío adestrado para a batalha, fiel ao seu dono e feroz com seus inimigos.</i>\n\n<color=#199F10>-Pode Imobilizar ao morder.</color>\n<color=#EE0000>-Relativamente fraco.</color>";
                break;
            case "Devorador Corrompido":
                r = "Devorador Corrompido";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Puede debilitar.\n-Absorbe vida de Personajes Corruptos.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Antes um habitante destas terras, agora corrompido pelo Alento Negro, deformado e faminto.</i>\n\n<color=#A020F0>-Corrompido.</color>\n<color=#199F10>-Pode enfraquecer.\n-Absorve vida de Personagens Corrompidos.</color>\n<color=#EE0000>-Relativamente fraco.</color>";
                break;
            case "Guerrero Corrompido":
                r = "Guerreiro Corrompido";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Antes um habitante destas terras, agora corrompido pelo Alento Negro, deformado e faminto.</i>\n\n<color=#A020F0>-Corrompido.</color>\n<color=#199F10>-Forte.\n-Atinge em área.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
                break;
            case "Alimaña Corrompida":
                r = "Alimária Corrompida";
                break;
            case "<i>No se logra discernir facilmente que animal fue originalmente, pero ahora es una criatura corrompida y muy nociva.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Largo alcance.\n-Crea Masa Contaminada.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Nío é fácil discernir que animal foi originalmente, mas agora é uma criatura corrompida e muito nociva.</i>\n\n<color=#A020F0>-Corrompido.</color>\n<color=#199F10>-Longo alcance.\n-Cria Massa Contaminada.</color>\n<color=#EE0000>-Movimento limitado.</color>";
                break;
            case "Dar Feedback":
                r = "Enviar Feedback";
                break;
            case "Luchar":
                r = "Lutar";
                break;
            case "-Las Almas Danzantes de animales inocentes guian a la caravana. +5 Esperanza, 0% chances de emboscada.":
                r = "-As Almas Dançantes de animais inocentes guiam a caravana. +5 Esperança, 0% de chance de emboscada.";
                break;
            case "Almas Danzantes: +5 Esperanza, -100% chances de Emboscada.":
                r = "Almas Dançantes: +5 Esperança, -100% de chance de Emboscada.";
                break;
            case "-Las Almas Danzantes guí­an a la caravana. +5 Esperanza":
                r = "-As Almas Dançantes guiam a caravana. +5 Esperança";
                break;
            case "-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza":
                r = "-A Aurora Boreal deslumbra toda a caravana. +10 Esperança";
                break;
            case "Aurora Boreal: +10 Esperanza.":
                r = "Aurora Boreal: +10 Esperança.";
                break;
            case "Caní­bal Kale'Tav":
                r = "Canibal Kale'Tav";
                break;
            case "Garra Caní­bal":
                r = "Garra Canibal";
                break;
            case "Tentado por Sangre":
                r = "Tentado por Sangue";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Empieza combate con Evasión.\n-Se potencia si el enemigo está lastimado.</color>\n<color=#EE0000>-Una vez que perdió la evasión, es fácil de eliminar.</color>":
                r = "<i>Tribo oriunda da Passagem Vento Gelado, estes seres selvagens sío temidos por sua ferocidade e rituais pagíos.</i>\n\n<color=#199F10>-Começa o combate com Evasío.\n-Fica fortalecido se o inimigo estiver ferido.</color>\n<color=#EE0000>-Depois que perde a evasío, é fácil de eliminar.</color>";
                break;
            case "Ataque Lanza":
                r = "Ataque de Lança";
                break;
            case "Arrojar Lanza":
                r = "Arremessar Lança";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Ataque de lanza arrojadiza peligroso.</color>\n<color=#EE0000>-Poca Precisión.</color>":
                r = "<i>Tribo oriunda da Passagem Vento Gelado, estes seres selvagens sío temidos por sua ferocidade e rituais pagíos.</i>\n\n<color=#199F10>-Ataque perigoso com lança arremessada.</color>\n<color=#EE0000>-Pouca Precisío.</color>";
                break;
            case "Improvisar Trampas":
                r = "Improvisar Armadilhas";
                break;
            case "Trampa Improvisada: Daña y marca a unidades que la pisen.":
                r = "Armadilha Improvisada: causa dano e marca as unidades que pisarem nela.";
                break;
            case "Marcado":
                r = "Marcado";
                break;
            case "Guerrero Kale'Tav":
                r = "Guerreiro Kale'Tav";
                break;
            case "Cazador Kale'Tav":
                r = "Caçador Kale'Tav";
                break;
            case "Furioso por Herida":
                r = "Furioso por Ferida";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Recibir Herida lo potencia.\n-Al matar a un enemigo se potencia.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Tribo oriunda da Passagem Vento Gelado, estes seres selvagens sío temidos por sua ferocidade e rituais pagíos.</i>\n\n<color=#199F10>-Receber Ferida o fortalece.\n-Ao matar um inimigo, ele se fortalece.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
                break;
            case "Regocijo Asesino":
                r = "Regozijo Assassino";
                break;
            case "Hachazo Tribal":
                r = "Golpe Tribal de Machado";
                break;
            case "Bruja Kale'Tav":
                r = "Bruxa Kale'Tav";
                break;
            case "Golpe Bastón":
                r = "Golpe de Bastío";
                break;
            case "Ataque de Cuervo":
                r = "Ataque de Corvo";
                break;
            case "<i><i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Tribo oriunda da Passagem Vento Gelado, estes seres selvagens sío temidos por sua ferocidade e rituais pagíos.</i>\n\n<color=#199F10>-Fortalece Aliados.\n-Seu corvo a defende.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Frenesí­ del Asesinato":
                r = "Frenesi Assassino";
                break;
            case "Derribado":
                r = "Derrubado";
                break;
            case "<i>Una criatura feroz nativa de la tundra. Es uno de los depredadores más temidos de la región y fuente de varias leyendas entre los Kale'Tav</i>\n\n<color=#199F10>-Regeneración leve.\n-Ataque de embestida en fila.</color>\n<color=#EE0000>-Suelen aparecer sólos o con una pareja como mucho.</color>":
                r = "<i>Uma criatura feroz nativa da tundra. É um dos predadores mais temidos da regiío e origem de várias lendas entre os Kale'Tav</i>\n\n<color=#199F10>-Regeneraçío leve.\n-Ataque de investida em linha.</color>\n<color=#EE0000>-Costuma aparecer sozinha ou, no máximo, com um parceiro.</color>";
                break;
            case "Mordisco Faagdan":
                r = "Mordida de Faagdan";
                break;
            case "Armadura masticada":
                r = "Armadura Mastigada";
                break;
            case "Embestida Faagdan":
                r = "Investida de Faagdan";
                break;
			case "Garra Faagdan":
                r = "Garra de Faagdan";
                break;
            case "Pájaro Rompe-Hielos":
                r = "Pássaro Quebra-Gelo";
                break;
            case "Volador: Esta unidad no puede ser alcanzada por ataques melee, puede perder el vuelo al ser dañado o fallar un ataque.":
                r = "Voador: esta unidade nío pode ser atingida por ataques corpo a corpo, e pode perder o voo ao receber dano ou errar um ataque.";
                break;
            case "Picotazo Rompehielo":
                r = "Bicada Quebra-Gelo";
                break;
            case "Defensa abrumada":
                r = "Defesa Sobrepujada";
                break;
            case "Vuelo Alto":
                r = "Voo Alto";
                break;
            case "<i>Este pájaro es muy territorial y ataca en grupo, su pico está hecho para romper el hielo grueso y poder pescar peces de gran tamaño, por lo tanto es muy peligroso.</i>\n\n<color=#199F10>-Vuela.\n-Su ataque baja defensa</color>\n<color=#EE0000>-Una vez que pierde su vuelo, es vulnerable.</color>":
                r = "<i>Este pássaro é muito territorial e ataca em grupo. Seu bico é feito para quebrar o gelo espesso e pescar peixes de grande porte, por isso é muito perigoso.</i>\n\n<color=#199F10>-Voa.\n-Seu ataque reduz a defesa.</color>\n<color=#EE0000>-Quando perde o voo, fica vulnerável.</color>";
                break;
            case "Efigie Animada":
                r = "Efí­gie Animada";
                break;
            case "Reacción: Al morir condena al enemigo que dió el último golpe.":
                r = "REAÇíƒO: ao morrer, condena o inimigo que deu o golpe final.";
                break;
            case " es condenado por 3 turnos.":
                r = " é condenado por 3 turnos.";
                break;
            case "Condena: En X cantidad de turnos recibirá daño verdadero igual al 10% de su vida máxima por turno con el efecto.":
                r = "Condenaçío: em X turnos, receberá dano verdadeiro igual a 10% da sua vida máxima por turno com o efeito.";
                break;
            case " es dañado por la Condena.":
                r = " sofre dano da Condenaçío.";
                break;
            case "Corte Hoz":
                r = "Corte de Foice";
                break;
            case "<i>Armadas por la magia oscura de los Kale'Tav, estas efigies están por todo su territorio como primer linea de defensa en contra de quienes se atrevan a cruzar el Paso.</i>\n\n<color=#199F10>-Al ser destruida condena a su atacante.\n-Provoca sangrado.</color>\n<color=#EE0000>-Débiles.</color>":
                r = "<i>Erguidas pela magia sombria dos Kale'Tav, estas efí­gies estío espalhadas por todo o seu território como primeira linha de defesa contra aqueles que ousam cruzar a Passagem.</i>\n\n<color=#199F10>-Ao ser destruí­da, condena seu atacante.\n-Causa sangramento.</color>\n<color=#EE0000>-Fracas.</color>";
                break;
            case "Levantar Martillo":
                r = "Erguer Martelo";
                break;
            case "Martillo Listo":
                r = "Martelo Pronto";
                break;
            case "Gulek Gul pierde el buff 'Martillo Listo' tras recibir daño y no podrá utilizarlo.":
                r = "Gulek Gul perde o buff 'Martelo Pronto' ao receber dano e nío poderá usá-lo.";
                break;
            case "Martillo Pequeño":
                r = "Martelo Pequeno";
                break;
            case "Martillo Grande":
                r = "Martelo Grande";
                break;
            case "<i>Gulek-Gul es un Ettin muy venerado por los Kale'Tav. No habita con ellos, pero cuando se encuentran intrusos en la zona, baja de su colina decidido a proteger su territorio.</i>\n\n<color=#199F10>-Fuerza descomunal.\n-Golpea en zona.\n-Doble intento en tiradas de voluntad.</color>\n<color=#EE0000>-Necesita levantar el martillo grande antes de usarlo.\n-Si recibe daño o falla tirada de voluntad, deja caer el martillo.</color>":
                r = "<i>Gulek-Gul é um Ettin muito venerado pelos Kale'Tav. Ele nío vive com eles, mas quando intrusos sío encontrados na regiío, desce de sua colina decidido a proteger seu território.</i>\n\n<color=#199F10>-Força descomunal.\n-Atinge em área.\n-Dupla tentativa em jogadas de vontade.</color>\n<color=#EE0000>-Precisa erguer o martelo grande antes de usá-lo.\n-Se receber dano ou falhar em uma jogada de vontade, deixa o martelo cair.</color>";
                break;
            case "Discutir Tácticas":
                r = "Discutir Táticas";
                break;
            case "Enfoque Defensivo":
                r = "Foco Defensivo";
                break;
            case "Enfoque Agresivo":
                r = "Foco Ofensivo";
                break;
            case "Descansando":
                r = "Descansando";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque defensivo.":
                r = "Gulek e Gul discutem táticas e decidem adotar uma abordagem defensiva.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque ofensivo.":
                r = "Gulek e Gul discutem táticas e decidem adotar uma abordagem ofensiva.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven descansar para recuperar fuerzas.":
                r = "Gulek e Gul discutem táticas e decidem descansar para recuperar as forças.";
                break;
            case "Escudado: 10% chances por stack de evitar un ataque fí­sico. Al evitar uno, pierde un stack.":
                r = "Escudado: 10% de chance por acúmulo de evitar um ataque fí­sico. Ao evitar um, perde um acúmulo.";
                break;
            case " bloquea el daño con su escudo.":
                r = " bloqueia o dano com seu escudo.";
                break;
            case "Bloqueado":
                r = "Bloqueado";
                break;
            case "Golpe Mazo":
                r = "Golpe de Maça";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Unidad Escudada.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Alento Negro de seu mestre.</i>\n\n<color=#199F10>-Unidade Escudada.\n-Boa Armadura.\n-Ao morrer, deixa uma nuvem de Alento Negro.</color>\n<color=#EE0000>-Movimento limitado.</color>";
                break;
            case "Extasiado por Aliento Negro":
                r = "Extasiado pelo Alento Negro";
                break;
            case "Restos de Aliento: Potencia y cura a los Vengadores de Kadryn.":
                r = "Resí­duos de Alento: fortalecem e curam os Vingadores de Kadryn.";
                break;
            case "Reacción: Al morir genera restos de Aliento Negro en el campo de batalla.":
                r = "Reaçío: ao morrer, gera resí­duos de Alento Negro no campo de batalha.";
                break;
            case "Soldado Vengador de Kadryn":
                r = "Soldado Vingador de Kadryn";
                break;
            case " reacciona con Primer Golpe.":
                r = " reage com Primeiro Golpe.";
                break;
            case "Primer Golpe: el Alabardero ataca a la primera unidad que entra en la casilla.":
                r = "Primeiro Golpe: o Alabardeiro ataca a primeira unidade que entra na casa.";
                break;
            case "Alabardero Vengador de Kadryn":
                r = "Alabardeiro Vingador de Kadryn";
                break;
            case "Estocada Alabarda":
                r = "Estocada de Alabarda";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Buen ataque.\n-Flecha envenenada.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Alento Negro de seu mestre.</i>\n\n<color=#199F10>-Bom ataque.\n-Flecha envenenada.\n-Ao morrer, deixa uma nuvem de Alento Negro.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Tiro con Arco":
                r = "Disparo de Arco";
                break;
            case "Primer Golpe":
                r = "Primeiro Golpe";
                break;
            case "Predicador del Aliento Negro":
                r = "Pregador do Alento Negro";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de rango infalible.\n-Potencia Aliados.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Alento Negro de seu mestre.</i>\n\n<color=#199F10>-Ataque á distância infalí­vel.\n-Fortalece Aliados.\n-Ao morrer, deixa uma nuvem de Alento Negro.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Oración de Kadryn":
                r = "Oraçío de Kadryn";
                break;
            case "Rayo necrótico":
                r = "Raio Necrótico";
                break;
            case "Liturgia de la Putrefacción":
                r = "Liturgia da Putrefaçío";
                break;
            case "es presa de la Putrefacción.":
                r = "é ví­tima da Putrefaçío.";
                break;
            case "Putrefacción":
                r = "Putrefaçío";
                break;
            case "El Aliento Negro se expande por el campo enemigo.":
                r = "O Alento Negro se espalha pelo campo inimigo.";
                break;
            case "desata un rayo necrótico sobre":
                r = "desencadeia um raio necrótico sobre";
                break;
            case "Sus defensas se corroen por el Aliento Negro.":
                r = "Suas defesas sío corroí­das pelo Alento Negro.";
                break;
            case "Castigar a los Malvados":
                r = "Punir os Malvados";
                break;
            case "Marca: ":
                r = "Marca: ";
                break;
            case " posee bonificaciones de daño y ataque con ataques individuales contra este enemigo.":
                r = " recebe bônus de dano e ataque com ataques individuais contra este inimigo.";
                break;
            case "Ninfa Ardiendo":
                r = "Ninfa Ardente";
                break;
            case "Ataque Raiz Ardiente":
                r = "Ataque de Raiz Ardente";
                break;
            case "se entierra y desaparece del campo.":
                r = "se enterra e desaparece do campo.";
                break;
            case "emerge de vuelta.":
                r = "emerge novamente.";
                break;
            case "Emergida":
                r = "Emergida";
                break;
            case "Enterrarse":
                r = "Enterrar-se";
                break;
            case "Enterrado":
                r = "Enterrado";
                break;
            case "La raiz permanece oculta bajo tierra, preparandose para emerger.":
                r = "A raiz permanece oculta sob a terra, preparando-se para emergir.";
                break;
            case "Llamarada Raiz":
                r = "Chama da Raiz";
                break;
            case "<i>Raiz-Viva del bosque mismo que ha salido a la superficie obligada por las llamas, ahora atacará furiosa a cualquier invasor del bosque.</i>\n\n<color=#199F10>-Ataque de llamas infalible.\n-Se entierra para curarse.</color>\n<color=#EE0000>-Inmóvil.</color>":
                r = "<i>Raiz Viva da própria floresta que veio á superfí­cie forçada pelas chamas, agora atacará furiosamente qualquer invasor da floresta.</i>\n\n<color=#199F10>-Ataque de chamas infalí­vel.\n-Enterra-se para se curar.</color>\n<color=#EE0000>-Imóvel.</color>";
                break;
            case "Garra Oso Espectral":
                r = "Garra de Urso Espectral";
                break;
            case "<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia fí­sica es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>":
                r = "<i>Este urso se tornou um espectro feroz que vagueia pela floresta ardente. Seu poder fí­sico é aterrador.</i>\n\n<color=#199F10>-Ataques esmagadores.\n-Grande quantidade de vida.</color>\n<color=#EE0000>-Maior probabilidade de falha crí­tica.</color>";
                break;
            case "Bonus de daño elemental.":
                r = "Bônus de dano elemental.";
                break;
            case "<i>Esta bestia oriunda del Paso es material de varias leyendas y pesadillas entre los Kale'Tav. De cuerpo robusto y cuernos afilados, supone un peligro para los viajeros incautos.</i>\n\n<color=#199F10>-Ataques de carga en fila.\n-Regeneración leve.</color>\n<color=#EE0000>-Lento.</color>":
                r = "<i>Esta besta oriunda da Passagem é tema de várias lendas e pesadelos entre os Kale'Tav. De corpo robusto e chifres afiados, representa um perigo para os viajantes incautos.</i>\n\n<color=#199F10>-Ataques de investida em linha.\n-Regeneraçío leve.</color>\n<color=#EE0000>-Lento.</color>";
                break;
            case "Milicianos disponibles: ":
                r = "Milicianos disponí­veis: ";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de oportunidad.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Alento Negro de seu mestre.</i>\n\n<color=#199F10>-Ataque de oportunidade.\n-Boa Armadura.\n-Ao morrer, deixa uma nuvem de Alento Negro.</color>\n<color=#EE0000>-Movimento limitado.</color>";
                break;
            case "Refuerzos":
                r = "Reforços";
                break;
            case "Refuerzos aliados disponibles, irán uniéndose a la batalla gradualmente.":
                r = "Reforços aliados disponí­veis, entrarío gradualmente na batalha.";
                break;
            case "Refuerzos enemigos disponibles, irán uniéndose a la batalla gradualmente.":
                r = "Reforços inimigos disponí­veis, entrarío gradualmente na batalha.";
                break;
            case "El Bosque Ardiente":
                r = "A Floresta Ardente";
                break;
            case "Paso Vientohelado":
                r = "Passagem Vento Gelado";
                break;
            case "A medida que viajas por el bosque, las llamas envolverán regiones del mapa de forma inesperada.\n\nSi intentas atravesar un Nodo prendido fuego, perderás 10 de Esperanza y 8-15 Civiles.\nNo se podrá descansar en nodos incendiados.\n\nAdemás, las batallas que tengan lugar en un Nodo incendiado, tendrán llamas en el campo de batalla.":
                r = "À medida que você viaja pela floresta, as chamas envolverío regiões do mapa de forma inesperada.\n\nSe tentar atravessar um Nó em chamas, perderá 10 de Esperança e 8-15 Civis.\nNío será possí­vel descansar em nós incendiados.\n\nAlém disso, as batalhas que ocorrerem em um Nó incendiado terío chamas no campo de batalha.";
                break;
            case "<color=#FF3D00>-El incendio ha envuelto un nodo cercano al camino de la caravana.</color>":
                r = "<color=#FF3D00>-O incêndio envolveu um nó próximo ao caminho da caravana.</color>";
                break;
            case "\n<color=#FF3D00>--Incendiado--</color>":
                r = "\n<color=#FF3D00>--Incendiado--</color>";
                break;
            case "La lluvia desactiva la mecanica del Bosque Ardiente.":
                r = "A chuva desativa a mecanica de incendios da Floresta Ardente.";
                break;
            case "La lluvia apaga los focos de incendio actuales.":
                r = "A chuva apaga os focos de incendio atuais.";
                break;
            case "La lluvia ha apagado los incendios en el área temporalmente.":
                r = "A chuva apagou temporariamente os incêndios na área.";
                break;
            case "Llamas: infligen daño fuego a unidades que entren en la casilla.":
                r = "Chamas: causam dano de fogo ás unidades que entrarem na casa.";
                break;
            case "Raiz-Viva Ardiendo":
                r = "Raiz Viva Ardente";
                break;
            case "Enfurecido por el Fuego":
                r = "Enfurecido pelo Fogo";
                break;
            case "Barro: reduce 2 PA a unidades que entren en la casilla.":
                r = "Lama: reduz 2 PA das unidades que entrarem na casa.";
                break;
            case "La tribu Kale'Tav está realizando rituales en el área, preparándose para el Aliento Negro.\n\nAl escuchar sus tambores a lo lejos sabrás dónde se encuentran.\nPor cada Ritual completado, sus combatientes recibirán bonificaciones en batalla.\n\nPara interrumpir un ritual debes aproximarte a los nodos marcados y derrotarlos.\n\nFuerza Kale'Tav: ":
                r = "A tribo Kale'Tav está realizando rituais na área, preparando-se para o Alento Negro.\n\nAo ouvir seus tambores ao longe, você saberá onde eles estío.\nPara cada Ritual concluí­do, seus combatentes receberío bônus em batalha.\n\nPara interromper um ritual, você deve se aproximar dos nós marcados e derrotá-los.\n\nForça Kale'Tav: ";
                break;
            case "<color=#6A0DAD>-Un ritual Kale'Tav ha comenzado en un nodo cercano. La másica profana desalienta a la caravana. -5 Esperanza.</color>":
                r = "<color=#6A0DAD>-Um ritual Kale'Tav começou em um nó próximo. A música profana desencoraja a caravana. -5 Esperança.</color>";
                break;
            case "<color=#FF3D00>-Un ritual Kale'Tav ha sido completado. La fuerza de Kale'Tav aumenta en 1.</color>":
                r = "<color=#FF3D00>-Um ritual Kale'Tav foi concluí­do. A força de Kale'Tav aumenta em 1.</color>";
                break;
            case "-El ritual Kale'Tav ha sido detenido. +10 Esperanza.":
                r = "-O ritual Kale'Tav foi interrompido. +10 Esperança.";
                break;
            case "Batalla Kale'Tav":
                r = "Batalha Kale'Tav";
                break;
            case "Fuerza Kale'Tav":
                r = "Força Kale'Tav";
                break;
            case "Manual":
                r = "Manual";
                break;
            case "Mapa":
                r = "Mapa";
                break;
            case "Zonas":
                r = "Zonas";
                break;
            case "Civiles":
                r = "Civis";
                break;
            case "Personajes":
                r = "Personagens";
                break;
            case "Aliento Negro":
                r = "Respiro Negro";
                break;
            case "Ir al Manual de Combate":
                r = "Ir para o Manual de Combate";
                break;
            case "Ir al Manual de Campaña":
                r = "Ir para o Manual de Campanha";
                break;
            case "Combate":
                r = "Combate";
                break;
            case "Grillas":
                r = "Grades";
                break;
            case "Turnos":
                r = "Turnos";
                break;
            case "Acciones":
                r = "Ações";
                break;
            case "Sistema":
                r = "Sistema";
                break;
            case "Daños":
                r = "Danos";
                break;
            case "Estados":
                r = "Estados";
                break;
            case "Activa!":
                r = "Ativa!";
                break;
            case "Coste: ":
                r = "Custo: ";
                break;
            case "PA":
                r = "PA";
                break;
            case "Abrumado":
                r = "Sobrepujado";
                break;
            case "Destruyes":
                r = "Você destrói";
                break;
            case "Este obstaculo no puede ser destruido por tus unidades.":
                r = "Este obstáculo nío pode ser destruí­do por suas unidades.";
                break;
            case "Gasta 3 PA, para destruir un obstaculo adyacente de tu mismo lado si lo permite.":
                r = "Gasta 3 PA para destruir um obstáculo adjacente do seu lado, se for permitido.";
                break;
            case "No tienes flechas para usar esta habilidad.":
                r = "Você nío tem flechas para usar esta habilidade.";
                break;
            case "Sin flechas!":
                r = "Sem flechas!";
                break;
            case "Volumen de la Música":
                r = "Volume da Música";
                break;
            case "Reproducir másica al minimizar":
                r = "Reproduzir música ao minimizar";
                break;
            case "Idioma":
                r = "Idioma";
                break;
            case "Inglés":
                r = "Inglês";
                break;
            case "Español":
                r = "Espanhol";
                break;
            case "Idioma del juego":
                r = "Idioma do jogo";
                break;
            case "Gráficos":
                r = "Gráficos";
                break;
            case "Controles":
                r = "Controles";
                break;
            case "Jugabilidad":
                r = "Jogabilidade";
                break;
            case "Salir del juego":
                r = "Sair do jogo";
                break;
            case "Pantalla Completa":
                r = "Tela Cheia";
                break;
            case "Resolución de Pantalla":
                r = "Resoluçío de Tela";
                break;
            case "Calidad Gráficos":
                r = "Qualidade Gráfica";
                break;
            case "Alta":
                r = "Alta";
                break;
            case "Media":
                r = "Média";
                break;
            case "Baja":
                r = "Baixa";
                break;
            case "Accesos Rápidos":
                r = "Atalhos";
                break;
            case "Dificultad":
                r = "Dificuldade";
                break;
            case "-----Combate-----":
                r = "-----Combate-----";
                break;
            case "Modo Rápido":
                r = "Modo Rápido";
                break;
            case "Debido a la invasión, Nedukazal está envuelta en caos y oscuridad, por lo tanto la caravana no podrá ver claramente el camino adelante.\n\nAl depender de la luz propia, será mas propensa a sufrir emboscadas (+20%).\n\nMejora las <b>Antorchas de Pie</b> para aumentar el rango de visión.\n\nEl Aliento Negro no será una preocupación en esta zona.":
                r = "Devido á invasío, Nedukazal está envolta em caos e escuridío, portanto a caravana nío conseguirá ver claramente o caminho á frente.\n\nPor depender da própria luz, será mais propensa a sofrer emboscadas (+20%).\n\nMelhore as <b>Tochas de Pé</b> para aumentar o alcance da visío.\n\nO Alento Negro nío será uma preocupaçío nesta zona.";
                break;
            case "\nSe conseguirán de 25-40 Materiales y 60-85 Suministros.":
                r = "\nSerío obtidos 25-40 Materiais e 60-85 Suprimentos.";
                break;
            case "Nedukazal está a oscuras.":
                r = "Nedukazal está ás escuras.";
                break;
            case "Masacre: Nedukazal está siendo atacada. -10 Esperanza. +10% Emboscada. Los Zarkil están potenciados.":
                r = "Massacre: Nedukazal está sendo atacada. -10 Esperança. +10% Emboscada. Os Zarkil estío fortalecidos.";
                break;
            case "Garra Zarkil":
                r = "";
                break;
            case "Zarkil Acechador":
                r = "Zarkil Espreitador";
                break;
            case "Masacre Zarkil":
                r = "Massacre Zarkil";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverán.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Raça de criaturas demoní­acas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Ao esquivar de um ataque, irío se mover.\n-Pode ver escondidos.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
                break;
            case "Agazapado":
                r = "Agachado";
                break;
            case "Zarkil Guerrero":
                r = "Zarkil Guerreiro";
                break;
            case "Mirada de la Masacre":
                r = "Olhar do Massacre";
                break;
            case "Victima de la masacre":
                r = "Ví­tima do massacre";
                break;
            case " reacciona con Mirada de la Masacre.":
                r = " reage com Olhar do Massacre.";
                break;
            case "Aterrado":
                r = "Aterrorizado";
                break;
            case "Mirada de Masacre: al moverse aquí­, Tirada de salvación mental CD 13 o se pierde el turno.":
                r = "Olhar do Massacre: ao se mover para cá, faça uma jogada de resistência mental CD 13 ou perca o turno.";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Raça de criaturas demoní­acas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Pode aterrorizar criaturas á frente.\n-Pode ver escondidos.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
                break;
            case "Zarkil Vociferador":
                r = "Zarkil Vociferador";
                break;
            case "Grito de batalla Zarkil":
                r = "Grito de Batalha Zarkil";
                break;
            case "Orden Recibida":
                r = "Ordem Recebida";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que además motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>":
                r = "<i>Raça de criaturas demoní­acas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Grito atordoante que também motiva aliados.\n-Pode ver escondidos.\n-Pode atacar repetidamente.</color>\n<color=#EE0000></color>";
                break;
            case "Rayo Debilitador":
                r = "Raio Enfraquecedor";
                break;
            case "Debilitado":
                r = "Enfraquecido";
                break;
            case "<i>Raza de criaturas demoní­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-Débil</color>":
                r = "<i>Raça de criaturas demoní­acas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Ataque enfraquecedor infalí­vel.\n-Pode ver escondidos.\n-Voador.</color>\n<color=#EE0000>-Fraco</color>";
                break;
            case "Zarkil Alado":
                r = "Zarkil Alado";
                break;
            case "Mordisco Zarkilever":
                r = "Mordida de Zarkilever";
                break;
            case " sufre el efecto de Saboreado, recibiendo 15 de daño extra y curando al Zarkilever por 14 puntos de vida.":
                r = " sofre o efeito de Saboreado, recebendo 15 de dano extra e curando o Zarkilever em 14 pontos de vida.";
                break;
            case "Saborear":
                r = "Saborear";
                break;
            case "Saboreando!":
                r = "Saboreando!";
                break;
            case "<i>Criatura muy feroz controlada porlos Zarkils utilizada como fuerza de impacto y para causar grietas en superficies duras. </color>\n\n<color=#199F10>-Buena Armadura.\n-Saborea a las ví­ctimas.</color>\n<color=#EE0000></color>":
                r = "<i>Criatura muito feroz controlada pelos Zarkils, usada como força de impacto e para causar rachaduras em superfí­cies duras. </color>\n\n<color=#199F10>-Boa Armadura.\n-Saboreia as ví­timas.</color>\n<color=#EE0000></color>";
                break;
            case "Por la masacre":
                r = "Pelo massacre";
                break;
            case "Llamada Zarkil":
                r = "Chamado Zarkil";
                break;
            case "Rayo Zarkil":
                r = "Raio Zarkil";
                break;
            case "Comandante Zarkil":
                r = "Comandante Zarkil";
                break;
            case "<i>Tiene una legión entera de Zarkils bajo su liderazgo, simplemente debe señalar un objetivo y sus súbditos se encargarán del resto.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque debilitador infalible.</color>\n<color=#EE0000>-No es fuerte por si solo.</color>":
                r = "<i>Ele tem uma legiío inteira de Zarkils sob seu comando. Basta apontar um alvo e seus subordinados cuidarío do resto.</color>\n\n<color=#199F10>-Chama reforços sem fim.\n-Ataque enfraquecedor infalí­vel.</color>\n<color=#EE0000>-Nío é forte por si só.</color>";
                break;
            case "Llamada Espectral":
                r = "Chamado Espectral";
                break;
            case "Condena del bosque":
                r = "Condenaçío da floresta";
                break;
            case "<i>Manifestación de la energí­a espectral del bosque. Desde su interior emana un fulgor fantasmal frí­o, como un espí­ritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrótico que condena a dos objetivos.</color>\n<color=#EE0000>-Inmóvil.</color>":
                r = "<i>Manifestaçío da energia espectral da floresta. De seu interior emana um brilho fantasmagórico frio, como um espí­rito preso que se contorce para escapar.</color>\n\n<color=#199F10>-Chama reforços sem fim.\n-Ataque necrótico que condena dois alvos.</color>\n<color=#EE0000>-Imóvel.</color>";
                break;
            case "Aliento Helado":
                r = "Sopro Gélido";
                break;
            case "Draco de Hielo":
                r = "Draco de Gelo";
                break;
            case "Garra Draco":
                r = "Garra de Draco";
                break;
            case "<i>Estas criaturas aladas habitan en las regiones más frí­as del Paso. Son conocidas por ser muy territoriales y por su aliento gélido.</i>\n\n<color=#199F10>-Vuelo.\n-Aliento gélido en zona.\n-Regenera armadura.</color>\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>Estas criaturas aladas habitam as regiões mais frias da Passagem. Sío conhecidas por serem muito territoriais e por seu sopro gélido.</i>\n\n<color=#199F10>-Voo.\n-Sopro gélido em área.\n-Regenera armadura.</color>\n<color=#EE0000>-Fraco contra fogo.</color>";
                break;
            case "Aturdido":
                r = "Atordoado";
                break;
            case "Inalcanzable: unidad volando":
                r = "Inalcançável: unidade voando";
                break;
            case "Inalcanzable: unidad escondida":
                r = "Inalcançável: unidade escondida";
                break;
            case " PA":
                r = " PA";
                break;
            case "No hay suficientes flechas":
                r = "Nío há flechas suficientes";
                break;
            case "No hay suficientes energí­a":
                r = "Nío há energia suficiente";
                break;
            case "ATQ":
                r = "ATQ";
                break;
            case "TS":
                r = "TS";
                break;
            case "mods":
                r = "mods";
                break;
            case "atr":
                r = "atr";
                break;
            case "hab":
                r = "hab";
                break;
            case "atq":
                r = "atq";
                break;
            case "clima":
                r = "clima";
                break;
            case "situacional":
                r = "situacional";
                break;
            case "vs":
                r = "vs";
                break;
            case "DEF":
                r = "DEF";
                break;
            case "crit":
                r = "crit";
                break;
            case "pifia":
                r = "falha";
                break;
            case "ESTADO":
                r = "ESTADO";
                break;
            case "BUFF":
                r = "BUFF";
                break;
            case "DEBUFF":
                r = "DEBUFF";
                break;
            case "TRAMPA":
                r = "ARMADILHA";
                break;
            case "DAÑO":
                r = "DANO";
                break;
            case "MUERTE":
                r = "MORTE";
                break;
            case "CURACION":
                r = "CURA";
                break;
            case "Golpe":
                r = "Acerto";
                break;
            case "Crí­tico":
                r = "Crí­tico";
                break;
            case "Roce":
                r = "Raspío";
                break;
            case "Exito":
                r = "Sucesso";
                break;
            case "Tiro Potente":
                r = "Disparo Potente";
                break;
            case "Cargando...":
                r = "Carregando...";
                break;
            case "Acampar":
                r = "Acampar";
                break;
            case "<color=#FF6666>No puedes descansar aquí­.</color>":
                r = "<color=#FF6666>Você nío pode descansar aqui.</color>";
                break;
            case "Actualmente encarnado en un enemigo, invulnerable.":
                r = "Atualmente encarnado em um inimigo, invulnerável.";
                break;
            case "Encarnado en Enemigo":
                r = "Encarnado em Inimigo";
                break;
            case "Adelántate para usarla":
                r = "Avance para usá-la";
                break;
            case "Inmóvil, Melee solo adyacente.":
                r = "Imóvel, corpo a corpo apenas adjacente.";
                break;
            case "Melee disponible":
                r = "Corpo a corpo disponí­vel";
                break;
            case "Intercambiable":
                r = "Intercambiável";
                break;
            case "Habilidades de Combate":
                r = "Habilidades de Combate";
                break;
            case "Actividad durante el viaje":
                r = "Atividade durante a viagem";
                break;
            case "Selecciona un objetivo.":
                r = "Selecione um alvo.";
                break;
            case " % Chances":
                r = " % de chance";
                break;
            case "No hay objetivos al alcance.":
                r = "Nío há alvos ao alcance.";
                break;
            case "¡Comienza la batalla!":
                r = "A batalha começa!";
                break;
            case "¡Viaje completado!":
                r = "Viagem concluí­da!";
                break;
            case "Finalmente la caravana ha llegado a la Ciudad Puerto de Serria, donde la población civil se prepara para embarcar y así­ escapar del Aliento Negro.":
                r = "Finalmente, a caravana chegou á Cidade Portuária de Serria, onde a populaçío civil se prepara para embarcar e assim escapar do Alento Negro.";
                break;
            case "El viaje ha durado ":
                r = "A viagem durou ";
                break;
            case " dí­as enteros y han sobrevivido ":
                r = " dias inteiros e sobreviveram ";
                break;
            case "civiles.\n\n":
                r = " civis.\n\n";
                break;
            case "Además, el oro restante (":
                r = "Além disso, o ouro restante (";
                break;
            case ") se ha donado a las arcas de la ciudad para ayudar a financiar la evacuación.\n\nLos Personajes sobrevivientes también se han unido al esfuerzo de evacuación para defender la ciudad.\n\n":
                r = ") foi doado aos cofres da cidade para ajudar a financiar a evacuaçío.\n\nOs Personagens sobreviventes também se juntaram ao esforço de evacuaçío para defender a cidade.\n\n";
                break;
            case "<b>Valor de Trabajo obtenido: ":
                r = "<b>Valor de Trabalho obtido: ";
                break;
            case "Valor de Trabajo Disponible:":
                r = "Valor de Trabalho Disponí­vel:";
                break;
            case "Valor de Corrupción actual:":
                r = "Valor de Corrupçío atual:";
                break;
            case "El <b>Nivel de Peligro</b> actual en el Bosque Ardiente es: ":
                r = "O <b>Ní­vel de Perigo</b> atual na Floresta Ardente é: ";
                break;
            case "El <b>Nivel de Peligro</b> actual en el Paso Vientohelado es: ":
                r = "O <b>Ní­vel de Perigo</b> atual na Passagem Vento Gelado é: ";
                break;
            case "El <b>Nivel de Peligro</b> actual en Nedukazal es: ":
                r = "O <b>Ní­vel de Perigo</b> atual em Nedukazal é: ";
                break;
            case "Menu de Mejoras":
                r = "Menu de Melhorias";
                break;
            case "Barcos":
                r = "Navios";
                break;
            case "Templo":
                r = "Templo";
                break;
            case "Barricadas":
                r = "Barricadas";
                break;
            case "Cuartel":
                r = "Quartel";
                break;
            case "Almenaras":
                r = "Almenaras";
                break;
            case "Palacio":
                r = "Palácio";
                break;
            case "Granjas":
                r = "Fazendas";
                break;
            case "La ciudad puede permitirse esperar ":
                r = "A cidade pode se dar ao luxo de esperar ";
                break;
            case " caravanas más antes de tener que zarpar.":
                r = " caravanas a mais antes de ter que zarpar.";
                break;
            case "Misiones de Salvamento: ":
                r = "Missões de Salvamento: ";
                break;
            case "Pueden ser solicitadas por caravanas futuras para ayudar en momentos de crisis.":
                r = "Podem ser solicitadas por caravanas futuras para ajudar em momentos de crise.";
                break;
            case "Misiones Disponibles: ":
                r = "Missões Disponí­veis: ";
                break;
            case "Solicitar Salvamento:":
                r = "Solicitar Salvamento:";
                break;
            case "Pedir Ayuda":
                r = "Pedir Ajuda";
                break;
            case "Misión de Salvamento":
                r = "Missío de Salvamento";
                break;
            case "El ave mensajera regresa con un mensaje atado a sus patas. En él se indica el punto exacto al que la caravana deberá dirigirse para encontrarse con el equipo de salvamento, junto con los recursos cedidos por la ciudad de Serria.\n":
                r = "A ave mensageira retorna com uma mensagem presa ás patas. Nela, está indicado o ponto exato para onde a caravana deverá seguir para encontrar a equipe de salvamento, junto com os recursos cedidos pela cidade de Serria.\n";
                break;
            case "Ubicación de la Misión de Salvamento":
                r = "Localizaçío da Missío de Salvamento";
                break;
            case "<color=#a0e812><b>\n\nSe ha marcado en el camino adelante el nodo al cual deberáas dirigirte para encontrarte con el equipo de salvamento.</b></color>":
                r = "<color=#a0e812><b>\n\nFoi marcado no caminho á frente o nó ao qual você deverá se dirigir para encontrar a equipe de salvamento.</b></color>";
                break;
            case "Un encuentro esperado":
                r = "Um encontro esperado";
                break;
            case "Has llegado al lugar señalado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traí­do para ayudarles en su travesí­a.\n\nInmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.\n":
                r = "Você chegou ao local indicado pela ave mensageira e encontrou a equipe de salvamento enviada pela Cidade Portuária de Serria.\nImediatamente eles saúdam a caravana e começam a descarregar os recursos que trouxeram para ajudar em sua travessia.\n\nO ânimo na caravana melhora na mesma hora ao ver que eles nío estío sozinhos nessa luta.\n";
                break;
            case "<color=#a0e812><b>\n\nSe han entregado ":
                r = "<color=#a0e812><b>\n\nForam entregues ";
                break;
            case " suministros. +25 Esperanza. +20 Materiales y 200 Oro y un nuevo personaje se suma a la caravana</b></color>":
                r = " suprimentos. +25 Esperança. +20 Materiais e 200 Ouro, e um novo personagem se junta á caravana</b></color>";
                break;
            case "-Las oraciones de los Purificadores del Templo de Serria merman el avance del Aliento Negro en: ":
                r = "-As orações dos Purificadores do Templo de Serria reduzem o avanço do Alento Negro em: ";
                break;
            case " Esperanza":
                r = " Esperança";
                break;
            case "-Las almenaras de Serria se divisan a lo lejos sobre las montañas, brillando con fuerza y marcando el destino de la caravana: ":
                r = "-As almenaras de Serria podem ser vistas ao longe sobre as montanhas, brilhando com força e marcando o destino da caravana: ";
                break;
            case "<b>Una patrulla de milicianos de Serria se une a la batalla como refuerzos.</b>":
                r = "<b>Uma patrulha de milicianos de Serria se junta á batalha como reforços.</b>";
                break;
            case "<b>Arbol Vengativo</b>":
                r = "<b>Árvore Vingativa</b>";
                break;
            case "Arbol Vengativo":
                r = "Árvore Vingativa";
                break;
            case "Árbol Vengativo":
                r = "Árvore Vingativa";
                break;
            case "Puntos de Acción":
                r = "Pontos de Açío";
                break;
            case "Volver al Menu Principal":
                r = "Voltar ao Menu Principal";
                break;
			case "Tutorial activo, atajos deshabilitados.":
                r = "Tutorial ativo, atalhos desabilitados.";
                break;
            case "Cargar Partida":
                r = "Carregar Partida";
                break;
            case "es condenado por":
                r = "é condenado por";
                break;
            case "turnos.":
                r = "turnos.";
                break;
            case "resiste la condena, pero sufre el latido necrotico.":
                r = "resiste á condenaçío, mas sofre o pulso necrótico.";
                break;
            case "desata una llamarada ardiente sobre":
                r = "desencadeia uma labareda ardente sobre";
                break;
            case "desata un rayo debilitador sobre":
                r = "desencadeia um raio enfraquecedor sobre";
                break;
            case " se aterra por Mirada de la Masacre y pierde el turno.":
                r = " é aterrorizado pelo Olhar do Massacre e perde o turno.";
                break;
            case " obtiene un intento adicional de Tirada de Salvación.":
                r = " recebe uma tentativa adicional de Jogada de Resistência.";
                break;
            case "Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque agresivo.":
                r = "Gulek e Gul discutem táticas e decidem adotar uma abordagem agressiva.";
                break;
            case "Aturdido!":
                r = "Atordoado!";
                break;
            case "Turno de":
                r = "Turno de";
                break;
            case "No se salva":
                r = "Falha";
                break;
            case "Se salva":
                r = "Resiste";
                break;
            case "Fortaleza":
                r = "Fortaleza";
                break;
            case "Reflejos":
                r = "Reflexos";
                break;
            case "":
                r = "";
                break;
            case "Mental":
                r = "Mental";
                break;
            case " obtiene ":
                r = " recebe ";
                break;
            case ": -Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada.":
                r = ": -Chuva: -5 Esperança. -15% Coleta de Suprimentos, -20% de chance de Emboscada.";
                break;
            case "Defender":
                r = "Defender";
                break;
            case "Huir":
                r = "Fugir";
                break;
            case "Asentamiento":
                r = "Assentamento";
                break;
            case "Esperanza.":
                r = "Esperança.";
                break;
            case "Sin consecuencias graves.":
                r = "Sem consequências graves.";
                break;
            case "Derrota: ":
                r = "Derrota: ";
                break;
            case "-Victoria contra ":
                r = "-Vitória contra ";
                break;
            case "-Derrota frente a ":
                r = "-Derrota contra ";
                break;
            case " Materiales.":
                r = " Materiais.";
                break;
            case "<color=#2a9c71>\n\nFatigado: -1 Atributos hasa próximo descanso. </color>":
                r = "<color=#2a9c71>\n\nFatigado: -1 Atributos até o próximo descanso. </color>";
                break;
            case "Torpe: +1 Rango Pifias":
                r = "Desajeitado: +1 Faixa de Falha Crí­tica";
                break;
            case "Poción de Curación Media":
                r = "Poçío de Cura Média";
                break;
            case "objetivo":
                r = "alvo";
                break;
            case "Impacto crí­tico":
                r = "Impacto crí­tico";
                break;
            case " usa ":
                r = " usa ";
                break;
            case "Bonus daño elemental Acido.":
                r = "Bônus de dano elemental Ácido.";
                break;
            case "Bonus daño elemental Arcano.":
                r = "Bônus de dano elemental Arcano.";
                break;
            case "Bonus daño elemental Fuego.":
                r = "Bônus de dano elemental Fogo.";
                break;
            case "Bonus daño elemental Hielo.":
                r = "Bônus de dano elemental Gelo.";
                break;
            case "Bonus daño elemental Necro.":
                r = "Bônus de dano elemental Necrótico.";
                break;
            case "Bonus daño elemental Rayo.":
                r = "Bônus de dano elemental Raio.";
                break;
            case "Bonus daño elemental Divino.":
                r = "Bônus de dano elemental Divino.";
                break;
            case "Armadura reforzada que prioriza movilidad.":
                r = "Armadura reforçada que prioriza mobilidade.";
                break;
            case "Armadura reforzada con defensa elemental adicional.":
                r = "Armadura reforçada com defesa elemental adicional.";
                break;
            case "Armadura reforzada pensada para sigilo y evasiones.":
                r = "Armadura reforçada pensada para furtividade e evasões.";
                break;
            case "Armadura de cuero reforzado equilibrada y resistente.":
                r = "Armadura de couro reforçado equilibrada e resistente.";
                break;
            case "Espada corta imbuida con energia arcana.":
                r = "Espada curta imbuí­da com energia arcana.";
                break;
            case "Espada corta de filo oscuro y dano certero.":
                r = "Espada curta de fio sombrio e dano preciso.";
                break;
            case "Espada corta agil y versatil para combate cercano.":
                r = "Espada curta ágil e versátil para combate próximo.";
                break;
            case "Coraza templada para contraataques de fuego.":
                r = "Cota temperada para contra-ataques de fogo.";
                break;
            case "Coraza liviana para mantener proteccion con movilidad.":
                r = "Cota leve para manter proteçío com mobilidade.";
                break;
            case "Coraza robusta que incrementa la fuerza bruta.":
                r = "Cota robusta que aumenta a força bruta.";
                break;
            case "Coraza de caballero para defensa frontal solida.":
                r = "Cota de cavaleiro para defesa frontal sólida.";
                break;
            case "Mandoble bendecido con energia sagrada.":
                r = "Montante abençoado com energia sagrada.";
                break;
            case "Mandoble que castiga con mas fuerza a enemigos heridos.":
                r = "Montante que golpeia com mais força inimigos feridos.";
                break;
            case "Mandoble helado que enfria y desgasta al objetivo.":
                r = "Montante gélido que esfria e desgasta o alvo.";
                break;
            case "Mandoble pesado para golpes contundentes.":
                r = "Montante pesado para golpes contundentes.";
                break;
            case "Armadura de cuero que mejora la resistencia fisica.":
                r = "Armadura de couro que melhora a resistência fí­sica.";
                break;
            case "Armadura de cuero flexible para buena movilidad.":
                r = "Armadura de couro flexí­vel para boa mobilidade.";
                break;
            case "Armadura de cuero ligera orientada a evasion.":
                r = "Armadura de couro leve voltada para evasío.";
                break;
            case "Arco largo con disparos corrosivos de acido.":
                r = "Arco longo com disparos corrosivos de ácido.";
                break;
            case "Arco largo reforzado para disparos de alto impacto.":
                r = "Arco longo reforçado para disparos de alto impacto.";
                break;
            case "Arco largo que reduce el ritmo del objetivo.":
                r = "Arco longo que reduz o ritmo do alvo.";
                break;
            case "Arco largo versatil para combate a distancia.":
                r = "Arco longo versátil para combate á distância.";
                break;
            case "Baculo purificador que canaliza energia sagrada.":
                r = "Cajado purificador que canaliza energia sagrada.";
                break;
            case "Balsamo que mejora la mente y la concentracion por un combate.":
                r = "Bálsamo que melhora a mente e a concentraçío por um combate.";
                break;
            case "Balsamo que acelera reflejos y reaccion por un combate.":
                r = "Bálsamo que acelera reflexos e reaçío por um combate.";
                break;
            case "Balsamo que refuerza la fortaleza y la resistencia por un combate.":
                r = "Bálsamo que reforça a fortaleza e a resistência por um combate.";
                break;
            case "Anillo orientado al dano ofensivo y al impacto magico.":
                r = "Anel voltado para dano ofensivo e impacto mágico.";
                break;
            case "Anillo orientado al enfoque mental y control arcano.":
                r = "Anel voltado para foco mental e controle arcano.";
                break;
            case "Arco estandar del explorador, fiable para ataques a distancia.":
                r = "Arco padrío do explorador, confiável para ataques á distância.";
                break;
            case "Baston de purificadora para canalizar energia y golpear en melee.":
                r = "Bastío da purificadora para canalizar energia e golpear corpo a corpo.";
                break;
            case "Mandoble pesado del caballero, potente en combate frontal.":
                r = "Montante pesado do cavaleiro, potente em combate frontal.";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Tribo oriunda da Passagem Vento Gelado, estes seres selvagens sío temidos por sua ferocidade e rituais pagíos.</i>\n\n<color=#199F10>-Fortalece Aliados.\n-Seu corvo a defende.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Tiro Ballesta de Mano":
                r = "Disparo de Besta de Mío";
                break;
            case "Acumular Energí­a":
                r = "Acumular Energia";
                break;
            case "Acumulación Inestable":
                r = "Acumulaçío Instável";
                break;
            case "Acechar":
                r = "Espreitar";
                break;
            case "Arrojar Abrojos":
                r = "Arremessar Abrolhos";
                break;
            case "Asesinar":
                r = "Assassinar";
                break;
            case "Bomba de Humo":
                r = "Bomba de Fumaça";
                break;
            case "Corte Daga":
                r = "Corte de Adaga";
                break;
            case "Corte de Espada Corta":
                r = "Corte de Espada Curta";
                break;
            case "Corte de Espada Corta Arcana":
                r = "Corte de Espada Curta Arcana";
                break;
            case "Corte de Espada Corta Consumevida":
                r = "Corte de Espada Curta Consomevida";
                break;
            case "Corte de Espada Corta Filonegro":
                r = "Corte de Espada Curta Fio Negro";
                break;
            case "Corte Horizontal":
                r = "Corte Horizontal";
                break;
            case "Corte Incapacitante":
                r = "Corte Incapacitante";
                break;
            case "Corte Vertical":
                r = "Corte Vertical";
                break;
            case "Corte Vertical Congelado":
                r = "Corte Vertical Congelado";
                break;
            case "Corte Vertical Sagrado":
                r = "Corte Vertical Sagrado";
                break;
            case "Corte Vertical Sediento":
                r = "Corte Vertical Sedento";
                break;
            case "Descarga De Poder":
                r = "Descarga de Poder";
                break;
            case "Descarga Desintegradora":
                r = "Descarga Desintegradora";
                break;
            case "Disparo Envenenado":
                r = "Disparo Envenenado";
                break;
            case "Distraer":
                r = "Distrair";
                break;
            case "Eco Divino":
                r = "Eco Divino";
                break;
            case "Enmendar":
                r = "Remendar";
                break;
            case "Escudo de Fe":
                r = "Escudo de Fé";
                break;
            case "Fogata":
                r = "Fogueira";
                break;
            case "Hacia Las Sombras":
                r = "Rumo ás Sombras";
                break;
            case "Hoja de Energí­a":
                r = "Lâmina de Energia";
                break;
            case "HombroConHombro":
                r = "Ombro a Ombro";
                break;
            case "Improvisar Flechas":
                r = "Improvisar Flechas";
                break;
            case "Instatransporte":
                r = "Instatransporte";
                break;
            case "Llama Divina":
                r = "Chama Divina";
                break;
            case "Luz Cegadora":
                r = "Luz Cegante";
                break;
            case "Marcar Presa":
                r = "Marcar Presa";
                break;
            case "Presa Marcada":
                r = "Presa Marcada";
                break;
            case "Partir":
                r = "Partir";
                break;
            case "Pilares De Luz":
                r = "Pilares de Luz";
                break;
            case "Primeros Auxilios":
                r = "Primeiros Socorros";
                break;
            case "Purificación":
                r = "Purificaçío";
                break;
            case "Ráfaga":
                r = "Rajada";
                break;
            case "Residuo Energetico":
                r = "Resí­duo Energético";
                break;
            case "Salmo Purificador":
                r = "Salmo Purificador";
                break;
            case "Sigues Tú":
                r = "Você é o Próximo";
                break;
            case "Sigues Tu":
                r = "Você é o Próximo";
                break;
            case "Sifón Arcano":
                r = "Sifío Arcano";
                break;
            case "Tiro con Arco Acido":
                r = "Disparo de Arco Ácido";
                break;
            case "Tiro con Arco Potente":
                r = "Disparo de Arco Potente";
                break;
            case "Tiro con Arco Ralentizante":
                r = "Disparo de Arco Lentificante";
                break;
            case "Vigilancia":
                r = "Vigilância";
                break;
            case "Abrojo":
                r = "Abrolho";
                break;
            case "Acid Bow Shot":
                r = "Disparo de Arco Ácido";
                break;
            case "Oso Espectral":
                r = "Urso Espectral";
                break;
            case "<color=#8a5b32>perforante</color>":
                r = "<color=#8a5b32>perfurante</color>";
                break;
            case "Determinación ":
                r = "Determinaçío ";
                break;
            case "Barrera":
                r = "Barreira";
                break;
            case "Muerto":
                r = "Morto";
                break;
            case "reacciona con ":
                r = "reage com ";
                break;
            case "gana 1 Fervor por matar con ":
                r = "ganha 1 Fervor por matar com ";
                break;
            case "fue Desintegrado.":
                r = "foi Desintegrado.";
                break;
            case "==== Ronda ":
                r = "==== Rodada ";
                break;
            case " comienza ====":
                r = " começa ====";
                break;
            case "No puedes intercambiar con enemigos.":
                r = "Você nío pode trocar com inimigos.";
                break;
            case "-La Caravana ha sido emboscada por un ataque subterráneo.":
                r = "-A Caravana foi emboscada por um ataque subterrâneo.";
                break;
            case "El viaje de la caravana ha comenzado.":
                r = "A viagem da caravana começou.";
                break;
            case "-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crí­tico. -20 Esperanza":
                r = "-O Séquito de Clérigos pereceu, pois o Alento Negro atingiu um ní­vel crí­tico. -20 Esperança";
                break;
            case " ahora Maneja un Nivel ":
                r = " agora possui um Ní­vel ";
                break;
            case " de Energí­a.":
                r = " de Energia.";
                break;
            case " de Valentí­a.":
                r = " de Bravura.";
                break;
            case "ARM":
                r = "ARM";
                break;
            case "RES":
                r = "RES";
                break;
            case "BAR":
                r = "BAR";
                break;
            case "MIT":
                r = "MIT";
                break;
            case "BON":
                r = "BON";
                break;
            case "Mandoble Filonegro":
                r = "Montante Fio Negro";
                break;
            case "Mandoble Caótico":
                r = "Montante Caótico";
                break;
            case "Impacto Caótico":
                r = "Impacto Caótico";
                break;
            case "Guantelete de Llamas":
                r = "Manopla de Chamas";
                break;
            case "Guantelete Estrella":
                r = "Manopla Estrela";
                break;
            case "Grieta Arcana":
                r = "Fenda Arcana";
                break;
            /*  case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comercial a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                 r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mí­nimo.";
                 break;*/
            /*  case "El Séquito de Curanderos se encarga de atender a los heridos y enfermos de la Caravana. Pese a las circunstancias del viaje mismo, logran mantenerse en funcionamiento y brindan un servicio escencial para la supervivencia de quienes lo necesiten.":
                 r = "O Séquito de Curandeiros se encarrega de atender os feridos e doentes da Caravana. Apesar das circunstâncias da própria viagem, conseguem se manter em funcionamento e oferecem um serviço essencial para a sobrevivência de quem precisar.";
                 break;*/
            case "El séquito de Herreros se encarga del mantenimiento y manufactura de las armas y armaduras de la Caravana. Su carro es especialmente pesado ya que, montado ingeniosamente, carga con todas las necesidades básicas de un herrero.":
                r = "O séquito de Ferreiros se encarrega da manutençío e fabricaçío das armas e armaduras da Caravana. Sua carroça é especialmente pesada, pois, montada com engenho, carrega tudo o que um ferreiro precisa de básico.";
                break;
            /* case "Aumentar el tamaño de las tiendas incrementa la cantidad de objetos ofrecidos.":
                r = "Aumentar o tamanho das lojas aumenta a quantidade de itens oferecidos.";
                break;*/
            case "Amuleto de Hueso Liso":
                r = "Amuleto de Osso Liso";
                break;
            case "Amuleto de Segunda Piel":
                r = "Amuleto de Segunda Pele";
                break;
            case "Ancla de la Ultima Linea":
                r = "Âncora da Última Linha";
                break;
            case "Anillo de Destrucción":
                r = "Anel de Destruiçío";
                break;
            case "Anillo de Filo Interno":
                r = "Anel de Fio Interno";
                break;
            case "Anillo de Inteligencia":
                r = "Anel de Inteligência";
                break;
            case "Anillo de Resistencia al Ácido":
                r = "Anel de Resistência ao Ácido";
                break;
            case "Anillo de Tormenta Quieta":
                r = "Anel da Tempestade Silenciosa";
                break;
            case "Anillo del Vigia":
                r = "Anel do Vigia";
                break;
            case "Broche Runico de Bronce":
                r = "Broche Rúnico de Bronze";
                break;
            case "Cinta de Enfoque":
                r = "Faixa de Foco";
                break;
            case "Collar de Mente Clara":
                r = "Colar de Mente Clara";
                break;
            case "Corazon de Bastion":
                r = "Coraçío de Bastiío";
                break;
            case "Corazon de Tormenta Primigenia":
                r = "Coraçío da Tempestade Primordial";
                break;
            case "Corona del Eclipse":
                r = "Coroa do Eclipse";
                break;
            case "Estandarte del Baluarte Inmortal":
                r = "Estandarte do Baluarte Imortal";
                break;
            case "Insignia del Duelista":
                r = "Insí­gnia do Duelista";
                break;
            case "Juramento del Inquebrantable":
                r = "Juramento do Inquebrantável";
                break;
            case "Medalla de Guardia":
                r = "Medalha de Guarda";
                break;
            case "Nucleo del Eclipse Arcano":
                r = "Núcleo do Eclipse Arcano";
                break;
            case "Ojo del Acecho":
                r = "Olho da Espreita";
                break;
            case "Ojo del Veredicto":
                r = "Olho do Veredito";
                break;
            case "Piedra de Sangre Calma":
                r = "Pedra de Sangue Calma";
                break;
            case "Reliquia del Trono Vacio":
                r = "Relí­quia do Trono Vazio";
                break;
            case "Reliquia del Umbral Arcano":
                r = "Relí­quia do Umbral Arcano";
                break;
            case "Rosario del Alba":
                r = "Rosário da Aurora";
                break;
            case "Sello de Sangre Fria":
                r = "Selo de Sangue Frio";
                break;
            case "Sello del Caminante":
                r = "Selo do Caminhante";
                break;
            case "Sello del Rastreador Nocturno":
                r = "Selo do Rastreador Noturno";
                break;
            case "Talisman de Escarcha Viva":
                r = "Talismí de Geada Viva";
                break;
            case "Talisman del Muro Espinado":
                r = "Talismí do Muro Espinhoso";
                break;
            case "Cristal de Escarcha Fracta":
                r = "Cristal de Geada Fraturada";
                break;
            case "Escarcha Cortante":
                r = "Geada Cortante";
                break;
            case "Extraños en el camino":
                r = "Estranhos no caminho";
                break;
            case "Séquito":
                r = "Séquito";
                break;
            case "Personaje":
                r = "Personagem";
                break;
            case "Elige una Clase":
                r = "Escolha uma Classe";
                break;
            case "Dí­a":
                r = "Dia";
                break;
            case "-La caravana ha llegado a un nodo incendiado. -10 Esperanza.  ":
                r = "A caravana chegou a um nó incendiado. -10 Esperança.  ";
                break;
            case " Civiles Muertos.":
                r = " Civis Mortos.";
                break;
            // Missing item names from ItemDatabase
            case "Aceite de Tormenta":
                r = "Óleo da Tempestade";
                break;
            case "Ampolla Aislante":
                r = "Ampola Isolante";
                break;
            case "Arco de Explorador":
                r = "Arco do Explorador";
                break;
            case "Arco Largo Potente":
                r = "Arco Longo Potente";
                break;
            case "Arco Largo Ralentizante":
                r = "Arco Longo Lentificante";
                break;
            case "Arco Largo Ácido":
                r = "Arco Longo Ácido";
                break;
            case "Armadura de Cuero Borrosa":
                r = "Armadura de Couro Nebulosa";
                break;
            case "Armadura de Cuero de Fortaleza":
                r = "Armadura de Couro de Fortaleza";
                break;
            case "Armadura de Cuero del Cazador Gris":
                r = "Armadura de Couro do Caçador Cinzento";
                break;
            case "Armadura de Cuero del Horizonte":
                r = "Armadura de Couro do Horizonte";
                break;
            case "Armadura de Cuero del Rastreador":
                r = "Armadura de Couro do Rastreador";
                break;
            case "Armadura de Cuero necrótico":
                r = "Armadura de Couro Necrí­tica";
                break;
            case "Armadura de Cuero Reforzado de Ligereza":
                r = "Armadura de Couro Reforçado de Leveza";
                break;
            case "Armadura de Cuero Reforzado de Protección Elemental":
                r = "Armadura de Couro Reforçado de Proteçío Elemental";
                break;
            case "Armadura de Cuero Reforzado de Velo":
                r = "Armadura de Couro Reforçado de Véu";
                break;
            case "Armadura de Cuero Sombria":
                r = "Armadura de Couro Sombria";
                break;
            case "Armadura de Cuero Veloz":
                r = "Armadura de Couro Veloz";
                break;
            case "Armadura Pesada de Caballero":
                r = "Armadura Pesada de Cavaleiro";
                break;
            case "Armadura Reforzada de Niebla":
                r = "Armadura Reforçada de Névoa";
                break;
            case "Armadura Reforzada del Acecho":
                r = "Armadura Reforçada da Espreita";
                break;
            case "Armadura Reforzada del Verdugo":
                r = "Armadura Reforçada do Verdugo";
                break;
            case "Armadura Reforzada Filo Umbrio":
                r = "Armadura Reforçada Fio Umbrio";
                break;
            case "Armadura Reforzada Ojo Nocturno":
                r = "Armadura Reforçada Olho Noturno";
                break;
            case "Baculo Purificador":
                r = "Cajado Purificador";
                break;
            case "Bastón de Purificadora":
                r = "Bastío da Purificadora";
                break;
            case "Brebaje Vampirico":
                r = "Bebida Vampí­rica";
                break;
            case "Bálsamo de Resistencia":
                r = "Bálsamo de Resistência";
                break;
            case "Coraza de Fuerza de Gigante":
                r = "Cota da Força de Gigante";
                break;
            case "Coraza de Guardia Roja":
                r = "Cota da Guarda Vermelha";
                break;
            case "Coraza de Llamas":
                r = "Cota das Chamas";
                break;
            case "Coraza del Baluarte":
                r = "Cota do Baluarte";
                break;
            case "Coraza del Juramento":
                r = "Cota do Juramento";
                break;
            case "Coraza del Sol de Hierro":
                r = "Cota do Sol de Ferro";
                break;
            case "Coraza Muralla Eterna":
                r = "Cota Muralha Eterna";
                break;
            case "Elixir de Reflejos":
                r = "Elixir de Reflexos";
                break;
            case "Esencia del Bastion Antiguo":
                r = "Essência do Bastiío Antigo";
                break;
            case "Espada Corta Arcana":
                r = "Espada Curta Arcana";
                break;
            case "Espada Corta de Acechador":
                r = "Espada Curta de Espreitador";
                break;
            case "Espada Corta Filonegro":
                r = "Espada Curta Fio Negro";
                break;
            case "Extracto Corrosivo":
                r = "Extrato Corrosivo";
                break;
            case "Filtro Antidoto":
                r = "Filtro Antí­doto";
                break;
            case "Frasco de Corteza":
                r = "Frasco de Casca";
                break;
            case "Guantelete de Poder":
                r = "Manopla de Poder";
                break;
            case "Infusion de Claridad":
                r = "Infusío de Clareza";
                break;
            case "Jarabe del Acechador":
                r = "Xarope do Espreitador";
                break;
            case "Licor de Fortaleza":
                r = "Licor de Fortaleza";
                break;
            case "Mandoble Congelado":
                r = "Montante Congelado";
                break;
            case "Mandoble De Caballero":
                r = "Montante de Cavaleiro";
                break;
            case "Mandoble Sagrado":
                r = "Montante Sagrado";
                break;
            case "Mandoble Sediento":
                r = "Montante Sedento";
                break;
            case "Polvora Catalitica":
                r = "Pólvora Catalí­tica";
                break;
            case "Reliquia de Segundo Aliento":
                r = "Relí­quia do Segundo Fôlego";
                break;
            case "Resina del Armero":
                r = "Resina do Armeiro";
                break;
            case "Sello de Ceniza Negra":
                r = "Selo de Cinza Negra";
                break;
            case "Sí­mbolo de Proteccion Arcano":
                r = "Sí­mbolo Arcano de Proteçío";
                break;
            case "Solucion Neutralizante":
                r = "Soluçío Neutralizante";
                break;
            case "Tinta de Condena":
                r = "Tinta de Condenaçío";
                break;
            case "Tonico Vital del Campamento":
                r = "Tônico Vital do Acampamento";
                break;
            case "Unguento de Guardia":
                r = "Unguento de Guarda";
                break;
            case "Vela Arcana Bendita":
                r = "Vela Arcana Bendita";
                break;
            case "Vestidura Purificadora de Credo":
                r = "Vestimenta Purificadora do Credo";
                break;
            case "Vestidura Purificadora de Guardia":
                r = "Vestimenta Purificadora da Guarda";
                break;
            case "Vestidura Purificadora de Lumen":
                r = "Vestimenta Purificadora de Lumen";
                break;
            case "Vestidura Purificadora del Alba":
                r = "Vestimenta Purificadora da Aurora";
                break;
            case "Vestidura Purificadora del Santuario":
                r = "Vestimenta Purificadora do Santuário";
                break;
            // Missing buffs/debuffs from ItemDatabase
            case "Aislamiento Electrico":
                r = "Isolamento Elétrico";
                break;
            case "Balsamo de Claridad":
                r = "Bálsamo de Clareza";
                break;
            case "Balsamo Energizante":
                r = "Bálsamo Energizante";
                break;
            case "Balsamo Fortalecedor":
                r = "Bálsamo Fortalecedor";
                break;
            case "Bastion Ancestral":
                r = "Bastiío Ancestral";
                break;
            case "Bendicion Arcana":
                r = "Bênçío Arcana";
                break;
            case "Catalisis Ignea":
                r = "Catálise ígnea";
                break;
            case "Ceniza Vigilante":
                r = "Cinza Vigilante";
                break;
            case "Claridad Serena":
                r = "Clareza Serena";
                break;
            case "Condena Marcada":
                r = "Condenaçío Marcada";
                break;
            case "Corrosion Activa":
                r = "Corrosío Ativa";
                break;
            case "Corteza Viva":
                r = "Casca Viva";
                break;
            case "Efecto de consumible":
                r = "Efeito de consumí­vel";
                break;
            case "Elixir de Resistencia al Acido":
                r = "Elixir de Resistência ao Ácido";
                break;
            case "Elixir de Resistencia al Frio":
                r = "Elixir de Resistência ao Frio";
                break;
            case "Fortaleza Liquida":
                r = "Fortaleza Lí­quida";
                break;
            case "Guardia Ungida":
                r = "Guarda Ungida";
                break;
            case "Hambre Carmesi":
                r = "Fome Carmesim";
                break;
            case "Instinto de Caza":
                r = "Instinto de Caça";
                break;
            case "Piel Neutralizada":
                r = "Pele Neutralizada";
                break;
            case "Proteccion Arcana":
                r = "Proteçío Arcana";
                break;
            case "Reflejos Afilados":
                r = "Reflexos Afiados";
                break;
            case "Resina Defensiva":
                r = "Resina Defensiva";
                break;
            case "Segundo Aliento":
                r = "Segundo Fôlego";
                break;
            case "Tormenta Cargada":
                r = "Tempestade Carregada";
                break;
            case "Filoacero":
                r = "Fio de Aço";
                break;
            case "La caravana ha sido destruida y todos sus miembros han muerto. El Aliento Negro es implacable.":
                r = "A caravana foi destruí­da e todos os seus membros morreram. O Alento Negro é implacável.";
                break;
            case "Valor:":
                r = "Bravura:";
                break;
            case "Valor":
                r = "Bravura";
                break;
            case "Encarnar":
                r = "Encarnar";
                break;
            case "Pasivas":
                r = "Passivas";
                break;
            case "-Campaña guardada.":
                r = "-Campanha salva.";
                break;
            case "-No se pudo guardar la campaña. ":
                r = "-Nío foi possí­vel salvar a campanha. ";
                break;
            case "Si sales de la partida se perderán todos los cambios no guardados. ¿Continuar?":
                r = "Se você sair da partida, todas as alterações nío salvas serío perdidas. Continuar?";
                break;
            case "Cancelar":
                r = "Cancelar";
                break;
            case "Reproducir música al minimizar":
                r = "Reproduzir música ao minimizar";
                break;
            case "Este séquito está constituí­do por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderí­a. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mí­nima ganancia.":
                r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mí­nimo.";
                break;
            case "El Espectro acaba de atacar, haciéndolo vulnerable en el plano material.":
                r = "O Espectro acabou de atacar, tornando-se vulnerável no plano material.";
                break;
            case "Echar.":
                r = "Lançar";
                break;
            case "La caravana no tiene más tiendas para otro personaje.":
                r = "a caravana nío tem mais lojas para outro personagem.";
                break;
              case "La caravana llegado a un pequeño asentamiento aislado en el camino. Parece que los lugareños ignoran que el <b>Aliento Negro</b> se aproxima o carecen de un líder quien pueda guiarlos lejos de la inminente catástrofe.":
                r = "A caravana chegou a um pequeno assentamento isolado no caminho. Parece que os moradores locais desconhecem que o <b>Respiro Negro</b> se aproxima ou carecem de um líder que possa guiá-los para longe da catástrofe iminente.";
                break;
            case "Teniendo el cuenta que el tiempo apremia, analizas tus opciones:":
                r = "Considerando que o tempo está escasso, você analisa suas opções:";
                break;
           case "Asentamiento Destruído":
                r = "Assentamento Destruído";
                break;
            case "Este asentamiento ha sido consumido por el Aliento Negro y los únicos moradores aquí son los Corrompidos.  Prepárate a luchar.":
                r = "Este assentamento foi consumido pelo Respiro Negro e os únicos moradores aqui são os Corrompidos. Prepare-se para lutar.";
                break;
             
               
             
        }

        return r;
    }



    public void TraducirTodosTextosSegunIdioma()
{

    var textos = Object.FindObjectsOfType<TMPro.TextMeshProUGUI>(includeInactive: true);

    foreach (var txt in textos)
    {
        string original = txt.text;
        string traducido = original;

        if (nIdioma == IdiomaIngles)
        {
            traducido = TraducirConCompatibilidadMojibake(original.Trim(), TraducirIngles);
        }
        else if (nIdioma == IdiomaPortugues)
        {
            traducido = TraducirConCompatibilidadMojibake(original.Trim(), TraducirPortugues);
        }

        if (traducido != original)
        {
            txt.text = traducido;
        }
    }
}




}

