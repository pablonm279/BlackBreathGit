using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TRADU : MonoBehaviour
{
    public static TRADU i { get; private set; }
    public int nIdioma = 2; //1 EspaÃ±ol  -  2 InglÃ©s
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
            nIdioma = PlayerPrefs.GetInt("nIdioma");
        }
        else
        {
            PlayerPrefs.SetInt("nIdioma", 2); //Por defecto en ingles
        }
    }

    void Start()
    {
        if (nIdioma == 2) 
        {
            Invoke("TraducirTodosTextosIngles", 0.1f); //espera medio segundo para que carguen todos los textos
        }
    }
    public void ActualizarIdioma()
    {
        if (nIdioma == 2)
        {
            Invoke("TraducirTodosTextosIngles", 0.5f);
        }

    }
    public string Traducir(string textComponent)
    {
        string resultado = "SIN TRADUCCION";

        switch (nIdioma)
        {
            case 1: resultado = textComponent; break; //En espaÃ±ol no traduce
            case 2:
                resultado = TraducirIngles(textComponent);
                if (resultado == textComponent && !string.IsNullOrEmpty(textComponent))
                {
                    // Compat fallback for mojibake keys already stored in the switch table.
                    string claveMojibake = ConvertirATextoMojibake(textComponent);
                    if (claveMojibake != textComponent)
                    {
                        string resultadoFallback = TraducirIngles(claveMojibake);
                        if (resultadoFallback != claveMojibake)
                        {
                            resultado = resultadoFallback;
                        }
                    }
                }
                break; //INGLES

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
             r = /*"Error tradu: " +*/txt; //para que al devolverlo en espaÃ±ol, se sepa que linea falla
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

            case "Uno de los principales encargados de guiar la caravana y elegir las rutas mÃ¡s seguras accidentalmente perdiÃ³ sus mapas.\n":
                r = "One of the main people in charge of guiding the caravan and choosing the safest routes accidentally lost their maps.\n"; break;
            case "Los demÃ¡s encargados lo ayudarÃ¡n a buscarlos ya que esos mapas contiene informaciÃ³n crucial de la zona actual, y sin su ayuda la caravana podrÃ­a perderse.\n\n\n\n\n\n\n":
                r = "The other leaders will help search for them since those maps contain crucial information about the current area, and without them the caravan could get lost.\n\n\n\n\n\n\n"; break;
            case "ObtendrÃ¡ el estado Enfermo por 4-7 dÃ­as. Cada nivel del SÃ©quito de Curanderos reducirÃ¡ el tiempo de recuperaciÃ³n en 1 dÃ­a.\n\n\n\n\n":
                r = "Will gain the Sick status for 4-7 days. Each tier of the Healers' Retinue will reduce recovery time by 1 day.\n\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un dÃ­a extra.</color>\n\n":
                r = "<color=#ba3fef>-You can buy medicine for 45 Gold to reduce the illness by one extra day.</color>\n\n"; break;
            case "Al grito de un guardia, tu atenciÃ³n se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres estÃ¡ volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logrÃ³ forzarlo y se llevÃ³ parte del botÃ­n.\n\n":
                r = "At a guard's shout, your attention turns to one of the wagons carrying the caravan's treasures. One of its chests is tipped and gold wass spilled on the ground. Apparently during the night, someone managed to force it and took part of the loot.\n\n"; break;
            case "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrÃ³n:\n\n Se perderÃ­a 5 de Esperanza, <i>":
                r = "<color=#ba3fef>-You can subject the Civilians to an interrogation to try to find the thief:\n\n You would lose 5 Hope, <i>"; break;
            case "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n":
                r = "% Chances (40 base + Militiamen)</i> of finding the culprit and recovering the gold, -1 Civilian due to banishment.</color>\n\n"; break;
            case "Tras un estruendo, volteas la cabeza hacia atrÃ¡s y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas estÃ¡n atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n":
                r = "After a loud noise, you turn your head back and see that one of the supply wagons has had an accident. The wheels are stuck in the mud and the wagon seems to be lost for good.\n\n"; break;
            case "<color=#ba3fef>-Puedes pasar los 60 suministros caÃ­dos a otro carro, sacrificando 20 Materiales; o asumir la pÃ©rdida de suministros.</color>\n\n":
                r = "<color=#ba3fef>-You can transfer the 60 fallen supplies to another wagon, sacrificing 20 Materials; or accept the loss of supplies.</color>\n\n"; break;
            case "La Caravana encuentra un rÃ­o con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia Ã©l con la intenciÃ³n de recrearse y refrescarse.\n\n":
                r = "The Caravan finds a river with good flow and seemingly decent water. Several excited civilians head towards it to recreate and refresh themselves.\n\n"; break;
            case "El agua podrÃ­a estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n":
                r = "The water could be contaminated by the Black Breath. You can deny the Civilians access to the water or leave them to their own fate.\n\n"; break;
            case "<color=#ba3fef>-Si les niegas el acceso perderÃ¡s 15 de Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you deny access, you will lose 15 Hope.</color>\n\n"; break;
            case "<color=#ba3fef>-Si los dejas ir, hay un %":
                r = "<color=#ba3fef>-If you let them go, there is a %"; break;
            case "<i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no estÃ¡ contaminada descansarÃ¡n (-1 Fatiga).</color>\n\n":
                r = " <i>(Determined by Black Breath)</i> chance they are contaminated and 25 Civilians die. If not contaminated they will rest (-1 Fatigue).</color>\n\n"; break;
            case "\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n":
                r = "\nApparently they had an incident during a light training they set out to do in which both were slightly injured.\n\n"; break;
            case "La tensiÃ³n sube y los demÃ¡s caravaneros miran con incomodidad. Ambos reclaman tener la razÃ³n y esperan tu juicio.\n\n":
                r = "Tension rises and the other caravaners look on uncomfortably. Both claim to be right and await your judgment.\n\n"; break;
            case "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrÃ¡ Baja Moral por 5 dÃ­as. Apoyas a:</color>\n\n":
                r = "<color=#ba3fef>-You must intervene in support of one of the two. The other will gain Low Morale for 5 days. You support:</color>\n\n"; break;
            case "Un Civil de origen noble se acerca a ti con altanerÃ­a y comienza a cuestionar tu liderazgo. Argumentando que no estÃ¡s tomando las decisiones correctas para el bienestar de la Caravana y que Ã©l mismo podrÃ­a hacerlo mejor.\n":
                r = "A Civilian of noble origin approaches you arrogantly and begins to question your leadership, arguing that you are not making the right decisions for the Caravan's well-being and that he himself could do better.\n"; break;
            case "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n":
                r = "While his points are not very coherent, as he speaks loudly, several civilians begin to gather around, curious.\n\n"; break;
            case "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversiÃ³n. -65 Oro -8 Civiles -10 Esperanza\n\n":
                r = "<color=#ba3fef>-Hit him.</color> His family leaves the Caravan, withdrawing their investment. -65 Gold -8 Civilians -10 Hope\n\n"; break;
            case "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la direcciÃ³n del puerto.\n":
                r = "During the night, the gathered civilians spot a clear and beautiful flash of light on the horizon towards the port.\n"; break;
            case "QuizÃ¡s sea una seÃ±al, quizÃ¡s casualidad, pero los civiles se ven ahora mÃ¡s optimistas, por mÃ¡s que aÃºn falte un largo trecho.\n\n\n\n\n\n\n":
                r = "Perhaps it is a sign, perhaps coincidence, but the civilians now seem more optimistic, even though there is still a long way to go.\n\n\n\n\n\n\n"; break;
            case "La atmÃ³sfera se vuelve mÃ¡s ligera y optimista, y por un breve instante, el peso de la situaciÃ³n parece desvanecerse.\n\n\n\n":
                r = "The atmosphere becomes lighter and more optimistic, and for a brief moment, the weight of the situation seems to fade away.\n\n\n\n"; break;
            case "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>":
                r = "<color=#a0e812><b>+5 Hope</b>\n\n</color>"; break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadÇ­veres civiles. Una lucha tuvo lugar aquÃ­ y esta caravana no sobreviviÃ³.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n"; break;
            case "Si bien la situaciÃ³n es sombrÃ­a, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n":
                r = "Although the situation is bleak, several supplies in good condition were not looted, remaining on the side of the road.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n":
                r = "<color=#ba3fef>-You can bury the Civilians and honor their memory, without looting them.</color> +15 Hope \n\n"; break;
            case "La Caravana se detiene en un aserradero abandonado, algunos Ç­rboles han sido talados y la madera estÇ­ apilada en desorden.\n":
                r = "The Caravan stops at an abandoned sawmill; some trees have been felled and the wood is piled up in disarray.\n"; break;
            case "Hay suficiente madera como para llenar un par de carros, pero juntarla toda cansarÃ¡ a los Civiles que participen y llevarÃ¡ algunas horas.\n\n\n\n":
                r = "There is enough wood to fill a couple of wagons, but gathering it all will tire the participating Civilians and take a few hours.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes juntar solo lo que estÃ¡ a mano y continuar sin retraso.</color> +15-26 Materiales \n\n":
                r = "<color=#ba3fef>-You can gather only what is at hand and continue without delay.</color> +15-26 Materials \n\n"; break;
            case "La Caravana se detiene en un claro donde pasta una manada de bueyes. Los animales parecen sanos y bien alimentados, pero estÃ¡n asustados por la presencia de la Caravana.\n":
                r = "The Caravan stops in a clearing where a herd of oxen is grazing. The animals look healthy and well-fed, but are frightened by the Caravan's presence.\n"; break;
            case "La Caravana se detiene al escuchar gritos de auxilio provenientes de un lado del camino. Al investigar encuentras a un puÃ±ado de Civiles escapando de un enemigo desconocido en direcciÃ³n a la Caravana.\n":
                r = "The Caravan stops upon hearing cries for help from the side of the road. Investigating, you find a handful of Civilians fleeing an unknown enemy toward the Caravan.\n"; break;
            case "'Nos persiguen! no pudimos verlos, pero se acercan.' - Dice un Civil aterrorizado. 'AyÃºdanos'\n\n":
                r = "'They are chasing us! We couldn't see them, but they're approaching.' - Says a terrified Civilian. 'Help us'\n\n"; break;
            case "En un momento repentino, te das cuenta que hay mucha paz. Se escuchan los pasos constantes de la caravana, algÃºn murmullo, risa y la naturaleza alrededor.\n":
                r = "In a sudden moment, you realize there is a lot of peace. You hear the constant footsteps of the caravan, some murmurs, laughter, and nature around you.\n"; break;
            case "Estos momentos son muy escasos y sientes que cada individuo de la caravana lo valorÃ³ a su manera. \nDe alguna forma, el aire se siente mÃ¡s limpio.\n\n":
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
            case "-Has encontrado al ladrÃ³n y recuperado el oro robado, pero has tenido que desterrar al ladrÃ³n. -5 Esperanza -1 Civil.":
                r = "-You found the thief and recovered the stolen gold, but had to banish them. -5 Hope -1 Civilian."; break;
            case "-No has logrado encontrar al ladrÃ³n y se perdieron ":
                r = "-You failed to find the thief and lost "; break;
            case " de oro.":
                r = " gold."; break;
            case "-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza":
                r = "-You gave a motivational speech and refuted the Noble's arguments. +15 Hope"; break;
            case "-Has dado un discurso poco convincente que ha generado mÃ¡s dudas que certezas. -20 de Esperanza.":
                r = "-You gave an unconvincing speech that raised more doubts than certainties. -20 Hope."; break;
            case "-La cacerÃ­a de ":
                r = "-The hunt by "; break;
            case " ha sido exitosa. +":
                r = " was successful. +"; break;
            case " Suministros +55 Experiencia.":
                r = " Supplies +55 Experience."; break;
            case " sufriÃ³ un accidente durante la cacerÃ­a. Herido.":
                r = " suffered an accident during the hunt. Wounded."; break;
            case "-Los Civiles se han contaminado y han muerto ":
                r = "-The Civilians were contaminated and "; break;
            case " Civiles. -10 Esperanza":
                r = " Civilians. -10 Hope"; break;
            case "-Los Civiles han descansado en el rÃ­o y se han refrescado. -1 Fatiga ":
                r = "-The Civilians rested by the river and cooled off. -1 Fatigue "; break;
            // RiÃ±a description segments
            case "Escuchas un alboroto en las proximidades a los carros de los HÃ©roes. Al acercarte a investigar ves a <b><color=#d1006f>":
                r = "You hear a commotion near the Heroes' wagons. As you approach to investigate, you see <b><color=#d1006f>"; break;
            case "</color></b> y <b><color=#d1006f>":
                r = "</color></b> and <b><color=#d1006f>"; break;
            case "</color></b> discutiendo acaloradamente.":
                r = "</color></b> arguing heatedly."; break;
            case "RÃ­o Contaminado":
                r = "Contaminated River";
                break;
            case "RiÃ±a":
                r = "Brawl";
                break;
            case "LugareÃ±o Anciano ":
                r = "Local Elder ";
                break;
            case "SueÃ±o Inspirador":
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
            case "Lluvia: todas las unidades obtienen 'Mojado'. -1 Ataque a habilidades de rango.":
                r = "Rain: All units gain 'Wet'. -1 Attack to ranged skills."; break;
            case "Nieve: todas las unidades obtienen 'FrÃ­o'.":
                r = "Snow: All units gain 'Cold'."; break;
            case "Niebla: -2 Ataque a habilidades de rango.":
                r = "Fog: -2 Attack to ranged skills."; break;
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
            case "<color=#28b717>Ã¡cido</color>":
                r = "<color=#28b717>acid</color>"; break; //Acido
            case "<color=#1760b7>arcano</color>":
                r = "<color=#1760b7>arcane</color>"; break; //Arcano
            case "<color=#8038b2>necrÃ³tico</color>":
                r = "<color=#8038b2>necrotic</color>"; break; //Necro
            case "<color=#d6c304>verdadero</color>":
                r = "<color=#d6c304>true</color>"; break; //Verdadero
            case "<color=#d6c304>divino</color>":
                r = "<color=#d6c304>divine</color>"; break; //Divino
            case "Has llegado a un improvisado Puesto Comercial, ofrecen Suministros bÃ¡sicos de supervivencia a los viajeros.\nEl Tier de tu SÃ©quito de Mercaderes ayudarÃ¡ a bajar los precios.\n\n\nTu SÃ©quito de Mercaderes ha actualizado su Inventario.":
                r = "You have arrived at an improvised Trading Post, they offer basic survival Supplies to travelers.\nThe Tier of your Merchant Retinue will help lower prices.\n\n\nYour Merchant Escort has updated its Inventory.";
                break;
            case "El SÃ©quito de Mercaderes ha actualizado su inventario en el Puesto Comercial.":
                r = "The Merchant Retinue has updated its inventory at the Trading Post.";
                break;
            case "Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien estÃ¡ abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificaciÃ³n.\n\n\n. ":
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
            case "<color=#ebdeef>Al morir el Liche, liberÃ³ un Ãºltimo estertor de muerte y putrefacciÃ³n que se expande por cientos de kilÃ³metros alrededor.</color>":
                r = "<color=#ebdeef>Upon the Lich's death, it released a final gasp of death and rot that spreads for hundreds of miles around the land.</color>";
                break;
            case "\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente estÃ¡ envolviendo a los seres vivos que no logran escapar, provocÃ¡ndoles la muerte, o peor. </color>\n\n\n\n":
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
            case "EnÃ©rgicos(0)":
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
            case "La <color=#a0e812>Esperanza</color> determina el optimismo de la Caravana en general sobre la posibilidad de cumplir la misiÃ³n y llegar al puerto.\n\n":
                r = "The <color=#a0e812>Hope</color> determines the Caravan's overall optimism about the possibility of completing the mission and reaching the port.\n\n";
                break;
            case "/100 de <color=#a0e812>Esperanza</color>\n":
                r = "/100 <color=#a0e812>Hope</color>\n";
                break;
            case " <color=#982a1b>1-20 Civiles abandonarÃ¡n la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-20 Civilians will abandon the Caravan each rest.</color>\n";
                break;
            case " <color=#982a1b>1-10 Civiles abandonarÃ¡n la Caravana cada descanso.</color>\n":
                r = " <color=#982a1b>1-10 Civilians will abandon the Caravan each rest.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarÃ¡n algo de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Civilians will donate some Gold each rest.</color>\n";
                break;
            case " <color=#39a91b>Los Civiles donarÃ¡n buena cantidad de Oro cada descanso.</color>\n":
                r = " <color=#39a91b>Civilians will donate a good amount of Gold each rest.</color>\n";
                break;
            case "Los <color=#c918bb>Civiles</color> que lleva la caravana hacia el Puerto. Salvar la mayor cantidad es el objetivo principal de esta misiÃ³n.\n\nCada uno consume 1 de <color=#b7972c>Suministros</color> cada Descanso, y la cantidad de Civiles determina la eficiencia de las Tareas Civiles.\n":
                r = "The <color=#c918bb>Civilians</color> that conforms the caravan. Saving as many as possible is the main objective of this mission.\n\nEach one consumes 1 <color=#b7972c>Supplies</color> each Rest, and the number of Civilians determines the efficiency of Civil Tasks.\n";
                break;
            case "\nLlevas ":
                r = "\nYou carry ";
                break;
            case " <color=#c918bb>Civiles</color>, deben ser al menos 100 para que la misiÃ³n se considere exitosa.\n\n":
                r = " <color=#c918bb>Civilians</color>, must be at least 100 for the mission to be considered successful.\n\n";
                break;
            case "\nLas fuerzas de la Milicia de la caravana son de <color=#a8a29c>":
                r = "\nThe strength of the caravan's Militia is of <color=#a8a29c>";
                break;
            case ", que equivalen a ":
                r = "which is equivalent to ";
                break;
            case "</color> Milicianos que ayudarÃ¡n a defenderla de ataques directos.\n\n":
                r = "</color> Militiamen who will help defend it from direct attacks.\n\n";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b7972c>5 Suministros</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n\n":
                r = "<color=#ffdda5>---<b>Click to abandon <color=#b7972c>5 Supplies</color> and lighten the Load. -1 Hope</b>---</color>\n\n\n";
                break;
            case "Los <color=#b7972c>Suministros</color> constituyen las reservas de comida y elementos de supervivencia de la caravana.\n\nCada <color=#c918bb>Civil</color> consume 1 en cada Descanso. Los Bueyes consumen 2.\n":
                r = "The <color=#b7972c>Supplies</color> are conformed by the food reserves and survival items of the caravan.\n\nEach <color=#c918bb>Civil</color> consumes 1 at Rest. Each <color=#c918bb>Ox</color> consumes 2.\n";
                break;
            case " <color=#b7972c>Suministros</color>, por un total de peso de ":
                r = " <color=#b7972c>Supplies</color>, for a total weight of ";
                break;
            case "<color=#ffdda5>---<b>Haz click para abandonar <color=#b34f09>2 Materiales</color> y alivianar la Carga.</b>---</color>\n\n\n":
                r = "<color=#ffdda5>---<b>Click to abandon <color=#b34f09>2 Materials</color> and lighten the Load.</b>---</color>\n\n\n";
                break;
            case "Los <color=#b34f09>Materiales</color> son elementos bÃ¡sicos de construcciÃ³n utilizados para mantenimiento y expansiÃ³n de la caravana.\nCada uno pesa 3.\n":
                r = "The <color=#b34f09>Materials</color> are basic construction elements used for maintenance and expansion of the caravan.\nEach one weighs 3.\n";
                break;
            case " <color=#b34f09>Materiales</color>, por un total de peso de ":
                r = " <color=#b34f09>Materials</color>, for a total weight of ";
                break;
            case "<color=#ffdda5>---<b>Haz click para sacrificar <color=#9e2a1c>1 Buey</color> para obtener <color=#b7972c>20 Suministros</color>. -2 Esperanza</b>---</color>\n\n\n":
                r = "<color=#ffdda5>---<b>Click to sacrifice <color=#9e2a1c>1 Ox</color> to obtain <color=#b7972c>20 Supplies</color>. -2 Hope</b>---</color>\n\n\n";
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
            case "<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo que se haga duplica la Fatiga obtenida y reduce 10 la <color=#a0e812>Esperanza</color></color>.\n\n\n":
                r = "<color=#cc0d0d>The Caravan is Overloaded. Each segment traveled doubles the Fatigue gained and reduces Hope by 10</color>.\n\n\n";
                break;
            case "El <color=#d8a205>Oro</color> que lleva la Caravana, utilizado para comprar bienes y contratar servicios.":
                r = "<color=#d8a205>Gold</color> carried by the Caravan, used to purchase goods and hire services.";
                break;
            case "Indica que tanta <color=#06c297>Fatiga</color> tiene la Caravana en general.\n\n\n":
                r = "Shows how much <color=#06c297>Fatigue</color> the Caravan has in general.\n\n\n";
                break;
            case "Cada tramo de viaje la aumenta en 1.\n":
                r = "Each node traveled increases the Fatigue by 1.\n";
                break;
            case "Si descansas volverÃ¡ a 0 y arrancarÃ¡n el nuevo dÃ­a Descansados(1).\n\n":
                r = "If you rest it will return to 0 and the Caravan will start the next day Rested(1).\n\n";
                break;
            case "Actualmente estan Descansados(1), no habrÃ¡ penalizaciones por viajar.\n\n":
                r = "Currently <color=#a8ff9e>Rested</color>(<color=#a8ff9e>1</color>), there will be no penalties for traveling.\n\n";
                break;
            case "Actualmente estan Frescos(2), no habrÃ¡ penalizaciones por viajar.":
                r = "Currently <color=#d4ff9e>Fresh</color>(<color=#d4ff9e>2</color>), there will be no penalties for traveling.";
                break;
            case "Actualmente estan En Marcha(3), no habrÃ¡ penalizaciones por viajar.":
                r = "Currently <color=#fff79e>Marching</color>(<color=#fff79e>3</color>), there will be no penalties for traveling.";
                break;
            case "Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podrÃ­an morir si viajas.":
                r = "Currently <color=#ffd19e>Agitated</color>(<color=#ffd19e>4</color>), -10 Hope and few Oxen may die if you travel.";
                break;
            case "Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrÃ¡n morir si viajas.":
                r = "Currently <color=#ff9e9e>Tired</color>(<color=#ff9e9e>5</color>), -15 Hope and some Oxen may die if you travel.";
                break;
            case "Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrÃ¡n morir si viajas.":
                r = "Currently <color=#ff3c3c>Exhausted</color>(<color=#ff3c3c>6</color>), -20 Hope and several Oxen may die if you travel.";
                break;
            case "DÃ­a ":
                r = "Day ";
                break;
            case ": -Soleado: +5 Esperanza.":
                r = " -Sunny: +5 Hope.";
                break;
            case ": -Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3.":
                r = " -Heat Wave: +1 Fatigue. \"Free Day\" gives +5 Hope, other Civil Tasks give -3.";
                break;
            case ": -Lluvia: -5 Esperanza. -15% RecolecciÃ³n Suministros, -20% Emboscada.":
                r = " -Rain: -5 Hope. -15% Supply Gathering, -20% Ambush.";
                break;
            case ": -Nieve: +3 Esperanza. -15% RecolecciÃ³nes, -20% Emboscada. Viajar lleva el doble de tiempo.":
                r = " -Snow: +3 Hope. -15% Gatherings, -20% Ambush. Traveling takes double time.";
                break;
            case ": -Niebla: -20% RecolecciÃ³nes, -20% Emboscada, -20% ExploraciÃ³n, +10% Nodos Misteriosos.":
                r = " -Fog: -20% Gatherings, -20% Ambush, -20% Exploration, +10% Mysterious Nodes.";
                break;
            case "De un momento a otro, varios miembros de la caravana han desaparecido sin dejar rastro. Nadie tiene una explicaciÃ³n de lo que ha sucedido. Pero el miedo y la incertidumbre se apoderan de todos.\n":
                r = "Suddenly, several members of the caravan have disappeared without a trace. No one has an explanation for what happened. But fear and uncertainty take hold of everyone.\n";
                break;
            case "Luego de buscar vagamente en la cercanÃ­a y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n":
                r = "After vaguely searching the area and concluding that there are no clues, you decide to comfort the relatives and move on.\n\n\n\n\n\n\n";
                break;
            case "<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>You lose 4-12 Civilians, -5 Hope.</b></color>";
                break;
            case "Uno de los bueyes de la caravana ha caÃ­do enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagaciÃ³n de la enfermedad.\n\n\n\n":
                r = "One of the oxen in the caravan has fallen ill and cannot continue. You receive recommendations from some livestock specialists advising you to check the other oxen to prevent the spread of the disease.\n\n\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisarlos tomarÃ¡ unas horas: +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to check them it will take a few hours: +1 Black Breath Progress.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides ignorar las advertencias: 1-3 Bueyes mas morirÃ¡n.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to ignore the warnings: 1-3 more Oxen will die.</color>\n\n";
                break;
            case "Mientras la caravana se dispone a avanzar por un terreno peligroso, se topa con un grupo de bandidos que exige un peaje exorbitante para dejar pasar a la caravana.\n\n":
                r = "As the caravan prepares to move through dangerous terrain, it encounters a group of bandits demanding an exorbitant toll to let the caravan pass.\n\n";
                break;
            case "<color=#ba3fef>-Si decides pagar el peaje, perderÃ¡s 1 de Oro por Civil.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to pay the toll, you will lose 1 Gold per Civilian.</color>\n\n";
                break;
            case "</color></b> se acerca a ti y no luce nada bien. Te comenta que ha empezado a sentirse enfermo y necesita mÃ©dicina para mejorar pronto y estar nuevamente en condiciones de combatir.\n\n":
                r = "</color></b> approaches you and doesn't look well at all. He tells you that he has started to feel sick and needs medicine to get better soon and be in fighting condition again.\n\n";
                break;
            case "<color=#ba3fef>-Puedes dar un discurso motivador, refutando sus argumentos con hechos.</color> Chances: %":
                r = "<color=#ba3fef>-You can give a motivational speech, refuting his arguments with facts.</color> Chances: %";
                break;
            case " <i>(Determinado por Esperanza) Ã‰xito: +15 Esperanza. Fallo: -20 Esperanza.</i> \n\n":
                r = " <i>(Determined by Hope) Success: +15 Hope. Failure: -20 Hope.</i> \n\n";
                break;
            case "Durante la noche, <b><color=#d1006f>":
                r = "During the night, <b><color=#d1006f>";
                break;
            case "</color></b> junto con algunos Civiles comienzan a contar chistes y anÃ©cdotas divertidas, riendo y disfrutando del momento.\n":
                r = "</color></b> along with some Civilians start telling jokes and funny anecdotes, laughing and enjoying the moment.\n";
                break;
            case " y ":
                r = " and ";
                break;
            case " ganan Alta Moral por 3 dÃ­as.</b></color>":
                r = " gain High Morale for 3 days.</b></color>";
                break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadÃ¡veres civiles. Una lucha tuvo lugar aquÃ­ y esta caravana no sobreviviÃ³.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n":
                r = "<color=#ba3fef>-You can order the Caravan to loot the Supplies.</color> +21-35 Supplies, +5-11 Materials, +15-35 Gold, -5 Hope.</i> \n\n";
                break;
            case "La Caravana se detiene en un aserradero abandonado, algunos Ã¡rboles han sido talados y la madera estÃ¡ apilada en desorden.\n":
                r = "The Caravan stops at an abandoned sawmill, some trees have been felled and the wood is piled up in disarray.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n":
                r = "<color=#ba3fef>-You can order the Caravan to gather all the wood.</color> +65-90 Materials, +1 Fatigue, +1 Progress of the Black Breath.</i> \n\n";
                break;
            case "<color=#ba3fef>-Puedes optar por dejarlo cazar, o directamente domesticar a un puÃ±ado para que se sumen a la Caravana. +2-3 Bueyes</i> \n\n":
                r = "<color=#ba3fef>-You can choose to let it hunt, or directly tame a handful to join the Caravan. +2-3 Oxen</i> \n\n";
                break;
            case "'Â¡Nos persiguen! no pudimos verlos, pero se acercan.' - Dice un Civil aterrorizado. 'AyÃºdanos'\n\n":
                r = "'They're chasing us! We couldn't see them, but they're getting closer.' - Says a terrified Civilian. 'Help us'\n\n";
                break;
            case "<color=#ba3fef>-Puedes defender a los civiles de sus perseguidores mientras les das tiempo a los mÃ¡s dÃ©biles a sumarse a la Caravana.</color> Combate Normal - +18-26 Civiles\n\n":
                r = "<color=#ba3fef>-You can defend the civilians from their pursuers while giving the weaker ones time to join the Caravan.</color> Normal Combat - +18-26 Civilians\n\n";
                break;
            case "<color=#ba3fef>-Puedes aceptar solo a los mas Ã¡giles y huir para evitar confrontar con sus perseguidores.</color> +5-10 Civiles -5 Esperanza\n\n":
                r = "<color=#ba3fef>-You can only accept the most agile ones and flee to avoid confronting their pursuers.</color> +5-10 Civilians -5 Hope\n\n";
                break;
            case "</color></b> se acerca a ti y coloca una mano en tu hombro y dice: -'Tengo mucha esperanza en usted, y creo que serÃ¡ exitoso al liderarnos a salvo hacia el puerto'.\n":
                r = "</color></b> approaches you, places a hand on your shoulder and says: -'I have a lot of hope in you, and I believe you will be successful in leading us safely to the port'.\n";
                break;
            case "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'ConsidÃ©ralo un sÃ­mbolo de mi confianza en ti, ademÃ¡s de un aporte que puede ser Ãºtil para la Caravana.'-dice\n ":
                r = "With his other outstretched hand, he holds a bag of gold and offers it to you kindly. -'Consider it a symbol of my trust in you, as well as a contribution that may be useful for the Caravan.'-he says\n ";
                break;
            case "<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy mÃ¡s que agradecido de poder contar contigo.'</color> Efectos: ":
                r = "<color=#ba3fef>You respond: -'Keep the money, your contribution to the Caravan is already considerable with your daily effort, and I am more than grateful to be able to count on you.'</color> Effects: ";
                break;
            case " gana Alta Moral por 4 dÃ­as y 50 Experiencia. \n\n":
                r = " gains High Morale for 4 days and 50 Experience. \n\n";
                break;
            case "<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situaciÃ³n actual y seguramente nos ayudarÃ¡ durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n":
                r = "<color=#ba3fef>You respond: -'I accept your offer, there is no money to spare in our current situation and it will surely help us during the journey, thank you.'</color> Effects: +120-160 Gold. \n\n";
                break;
            case "Un hombre anciano aparece a un lado del camino haciendole seÃ±as con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchÃ­simos aÃ±os en la zona y la conoce a la perfecciÃ³n.\n":
                r = "An old man appears at the side of the road waving his hands at the Caravan. Up close, you realize that this man has been living in the area for many years and knows it perfectly.\n";
                break;
            case "'Aliento Negro o no, mis dÃ­as ya estÃ¡n contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como Ãºltimo acto de bien.'- dice\n\n":
                r = "'Black Breath or not, my days are already numbered. But I can share my knowledge about this land, as a final act of kindness.'- he says\n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Â¿Conoce algun atajo que nos aleje del peligro inminente al menos por unos kilÃ³metros?'</color> Efectos: Si es posible se generarÃ¡ un Atajo subterrÃ¡neo. \n\n":
                r = "<color=#ba3fef>Question: -'Do you know of any shortcut that can take us away from imminent danger for at least a few miles?'</color> Effects: If possible, a subterranean shortcut will be generated. \n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con mÃ¡s informaciÃ³n.'</color> Efectos: Se revelarÃ¡n prÃ³ximos nodos. \n\n":
                r = "<color=#ba3fef>Question: -'Describe the surrounding area so we can make more informed decisions.'</color> Effects: Upcoming nodes will be revealed. \n\n";
                break;
            case "</color></b> se lo ve con mucha energÃ­a y determinaciÃ³n mientras realiza sus labores habituales. Cuando te acercas a Ã©l, te dice que tuvo un SueÃ±o en el cual vio a la Caravana llegando a su destino.\n":
                r = "</color></b> looks very energetic and determined as he goes about his usual tasks. When you approach him, he tells you that he had a Dream in which he saw the Caravan reaching its destination.\n";
                break;
            case "'En el sueÃ±o, vi un claro camino hacia nuestro destino. HabrÃ¡ peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con DeterminaciÃ³n\n\n\n":
                r = "'In the dream, I saw a clear path to our destination. There will be dangers and difficulties, but I am convinced that we will make it. Let's follow that route.'- he says with Determination\n\n\n";
                break;
            case "</color></b> obtiene 150 Experiencia y Alta Moral por 5 dÃ­as.</color>\n\n":
                r = "</color></b> gains 150 Experience and High Morale for 5 days.</color>\n\n";
                break;
            case "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupciÃ³n y la pestilencia en lo mas mÃ­nimo.\n":
                r = "You have arrived at a beautiful natural clearing that seems to have been untouched by corruption and pestilence in the slightest.\n";
                break;
            case "Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n":
                r = "It is an excellent place to rest and recover strength.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrÃ¡ tambiÃ©n beneficios adicionales:\n-El Aliento Negro avanzarÃ¡ solo 1.\n-+10% curaciÃ³n recibida.\n-El evento serÃ¡ positivo.</b></color>":
                r = "<color=#a0e812><b>+5 Hope.\n\nResting in this place will also have additional benefits:\n-The Black Breath will only advance 1.\n-+10% healing received.\n-The event will be positive.</b></color>";
                break;
            case "Has llegado a un pequeÃ±o asentamiento. Notas que los civiles estÃ¡n desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.":
                r = "You have arrived at a small settlement. You notice that the civilians are disorganized and need leadership to survive the Black Breath.";
                break;
            case "\nDe 15-25 Civiles se unirÃ¡n a la Caravana y brindarÃ¡n 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.":
                r = "\n15-25 Civilians will join the Caravan and provide 50-60 Supplies, 6-8 Materials, 2-4 Oxen, and 60-70 Gold.";
                break;
            case "\nUn HÃ©roe aleatorio se sumarÃ¡ a tus fuerzas.\n\n\n\n\n":
                r = "\nA random Hero will join your forces.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>\nDescansar en este lugar tendrÃ¡ beneficios adicionales:+20% curaciÃ³n recibida.</b></color>":
                r = "<color=#a0e812><b>\nResting in this place will have additional benefits:+20% healing received.</b></color>";
                break;
            case "Has llegado a un lugar rico en recursos naturales, los civiles se han puesto a recolectar lo que han podido.":
                r = "You have arrived at a resource-rich area, and the civilians have started gathering what they can.";
                break;
            case "\nSe conseguirÃ¡n de 18-30 Materiales y 80-140 Suministros.":
                r = "\n18-30 Materials and 80-140 Supplies will be gathered.";
                break;
            case "<color=#a0e812><b>\n\nDescansar en este lugar tendrÃ¡ beneficios adicionales:+20% efectividad a tareas de RecolecciÃ³n.</b></color>":
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
            case "Al realizar la ofrenda, el Aliento Negro retrocederÃ¡ en 3 y un personaje con CorrupciÃ³n al azar serÃ¡ curado.":
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
            case "Un misterioso personaje pide unirse a la Caravana, parece capaz de defenderse sÃ³lo, seguramente sumarlo a la Caravana pueda ser beneficioso.":
                r = "A mysterious character asks to join the Caravan, they seem capable of defending themselves alone, surely adding them to the Caravan could be beneficial.\n\n\n\n\n\nChoose a Class:";
                break;
            case "Aceptarlos":
                r = "Take them in";
                break;
            case "Defensas: Cada Tier mejora las defensas de la Caravana en ataques directos y reduce 10% las chances de perder un SÃ©quito. ":
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
            case "Tiendas: Cada Tier da 5 de Esperanza al descansar.":
                r = "Tents: Each Tier grants 5 Hope when resting.";
                break;
            case "Catalejos: Cada Tier aumenta 5% las chances de ExploraciÃ³n y 5% las chances de encontrar Objetos tras una Batalla ganada.":
                r = "Spyglasses: Each Tier increases Exploration chances by 5% and the chances of finding Items after a won Battle by 5%.";
                break;
            case "Carro AlmacÃ©n: Cada Tier reduce 5% Suministros consumidos por Descanso.":
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
            case "Punto de SalvaciÃ³n!":
                r = "Saving throw Point!";
                break;
            case "Punto de Habilidad!":
                r = "Skill Point!";
                break;
            case "PosiciÃ³n":
                r = "Position";
                break;
            case "Elije una nueva Habilidad!":
                r = "Choose a new Skill!";
                break;
            case "Â¡Batalla!":
                r = "Battle!";
                break;
            case "Selecciona a tus personajes.":
                r = "Select your characters.";
                break;
            case "Comenzar":
                r = "Start Battle";
                break;
            case "Â¡Ataque a la Caravana!":
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
            case "-Es un dÃ­a hermoso. +5 Esperanza.":
                r = "It's a beautiful day. +5 Hope.";
                break;
            case "-La Ola de Calor se hace insoportable. +1 Fatiga.":
                r = "The Heat Wave becomes unbearable. +1 Fatigue.";
                break;
            case "-La Lluvia hace el viaje mÃ¡s difÃ­cil. -5 Esperanza.":
                r = "The Rain makes the journey more difficult. -5 Hope.";
                break;
            case "-La Nieve mejora el Ã¡nimo. +3 Esperanza.":
                r = "The Snow improves morale. +3 Hope.";
                break;
            case "% - Tirada: 1d100 = ":
                r = "% - Roll: 1d100 = ";
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
            case " ha realizado con Ã©xito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro.":
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
            case "-Los Herboristas han preparado sus BÃ¡lsamos.":
                r = "-The Herbalists have prepared their Balms.";
                break;
            case "-En la Feria, los Artistas han realizado un espectÃ¡culo que ha levantado el Ã¡nimo de los Civiles. +10 Esperanza":
                r = "-At the Fair, the Artists put on a show that lifted the spirits of the Civilians. +10 Hope";
                break;
            case " se cura ":
                r = " heals ";
                break;
            case " PV tras el Descanso.":
                r = " HP after Rest.";
                break;
            case "-El SÃ©quito de Curanderos ha reducido la enfermedad de":
                r = "-The Healers Retinue has reduced the illness of";
                break;
            case " en 1 extra.":
                r = " by 1 extra.";
                break;
            case " comparte sus historias de batalla con los civiles. +4 Esperanza":
                r = " shares his battle stories with the civilians. +4 Hope";
                break;
            case "-El tener que trabajar en plena Ola de Calor, ha caÃ­do mal en los Civiles. -3 Esperanza":
                r = "-Having to work in the middle of the Heat Wave has not gone well with the Civilians. -3 Hope";
                break;
            case "-El tener un DÃ­a Libre en plena Ola de Calor, ha caÃ­do bien en los Civiles. +5 Esperanza":
                r = "-Having a Day Off in the middle of the Heat Wave has been well received by the Civilians. +5 Hope";
                break;
            case "Las probabilidades de exploraciÃ³n: ":
                r = "Exploration chances: ";
                break;
            case "Las probabilidades de sufrir un ataque a la Caravana ":
                r = "Ambush chances: ";
                break;
            case "<b><u>Estado de Alerta</b></u>\n\n\n":
                r = "<b><u>High Alert</b></u>\n\n\n";
                break;
            case "Durante el descanso, se asignarÃ¡n a los civiles mas aptos fÃ­sicamente a la vigilancia del area circundante al campamento.\n\n":
                r = "During rest, the most physically fit civilians will be assigned to monitor the area surrounding the camp.\n\n";
                break;
            case "<color=#d8a205>Reduce chances de ataque a caravana. +20% a ExploraciÃ³n. -10 Esperanza.</color>\n\n\n":
                r = "<color=#d8a205>Reduces chances of Ambush during this rest. +20% to Exploration. -10 Hope.</color>\n\n\n";
                break;
            case "<b><u>DÃ­a Libre</b></u>\n\n\n":
                r = "<b><u>Day Off</b></u>\n\n\n";
                break;
            case "Los civiles se tomarÃ¡n el dÃ­a para descansar y recobrar fuerzas.\n\n":
                r = "The civilians will take the day to rest and regain strength.\n\n";
                break;
            case "<color=#d8a205>Se conseguirÃ¡ 10 de Esperanza y el dÃ­a siguiente arrancarÃ¡ con -1 Fatiga.</color>\n\n\n":
                r = "<color=#d8a205>You will gain 10 Hope and the next day will start with -1 Fatigue.</color>\n\n\n";
                break;
            case "<b><u>Feria</b></u>\n\n\n":
                r = "<b><u>Fair</b></u>\n\n\n";
                break;
            case "Los civiles dedicarÃ¡n el dÃ­a a organizar una feria con varios juegos y celebraciones.\n\n":
                r = "The civilians will dedicate the day to organizing a fair with various games and celebrations.\n\n";
                break;
            case "<color=#d8a205>Se conseguirÃ¡ entre 15 y 25 de Esperanza y se consumirÃ¡n 20% mÃ¡s de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n":
                r = "<color=#d8a205>You will gain between 15 and 25 Hope and consume 20% more Supplies. <color=#bb280d>+10% chances of Ambush.</color></color>\n\n\n";
                break;
            case "<b><u>RecolecciÃ³n de Materiales</b></u>\n\n\n":
                r = "<b><u>Material Collection</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarÃ¡n a recolectar materiales bÃ¡sicos en la zona.\n\n":
                r = "The civilians will dedicate themselves to collecting basic materials in the area.\n\n";
                break;
            case "<color=#d8a205>Se juntarÃ¡n entre ":
                r = "<color=#d8a205>You will gather between ";
                break;
            case " materiales. </color>\n\n\n":
                r = " materials. </color>\n\n\n";
                break;
            case "<b><u>RecolecciÃ³n de Suministros</b></u>\n\n\n":
                r = "<b><u>Supplies Collection</b></u>\n\n\n";
                break;
            case "Los civiles se dedicarÃ¡n a recolectar distintos suministros de las inmediaciones al campamento.\n\n":
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
            case "RecolecciÃ³n de Recursos.":
                r = "Resource Gathering.";
                break;
            case "Puesto de Comercio.":
                r = "Trading Post.";
                break;
            case "AdquisiciÃ³n de Personajes.":
                r = "Character Acquisition.";
                break;
            case "Combate directo contra enemigos de Ã‰lite.":
                r = "Direct combat against Elite enemies.";
                break;
            case "Batalla final de la Zona actual.":
                r = "Final battle of the current Zone.";
                break;
            case "<b>(!)</b> Zona Expuesta, la caravana serÃ¡ emboscada.":
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
            case "<color=#7ED6F7>-Durante el Descanso, se ha Explorado con Ã©xito el camino adelante.</color>":
                r = "<color=#7ED6F7>-During Rest, the path ahead has been successfully explored.</color>";
                break;
            case " ha Explorado con Ã©xito el camino adelante.</color>":
                r = " has successfully scouted the path ahead.</color>";
                break;
            case "-Al viajar por el atajo subterrÃ¡neo, la moral de la caravana disminuye. -5 Esperanza":
                r = "-While traveling through the underground passage, the caravan's morale decreases. -5 Hope";
                break;
            case "-Se ha encontrado un atajo subterrÃ¡neo.":
                r = "-An underground passage has been found.";
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
            case "ValentÃ­a: ":
                r = "Valor: ";
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
            case "<color=#2a9c71>\n\nFatigado: -1 PA mÃ¡ximo. </color>":
                r = "<color=#2a9c71>\n\nFatigued: -1 max AP. </color>";
                break;
            case "Bendecido por Plegaria: +1 Ataque +1 Defensa +5 Res.Necro +2 TSMental.</color>":
                r = "Blessed by Prayer: +1 Attack +1 Defense +5 Res.Necro +2 TS Mental.</color>";
                break;
            case "<color=#d80404>\n\nHerido:-1 Atributos. Si cae en combate, muere. </color>":
                r = "<color=#d80404>\n\nWounded: -1 Attributes. If falls in combat, dies. </color>";
                break;
            case "<color=#d80404>\n\nCorrupto: Los enemigos corrompidos se curan al atacarlo, le infligen mas daÃ±o, y si lo derriban en combate, muere. </color>":
                r = "<color=#d80404>\n\nCorrupted: Corrupted enemies heal when attacking it, deal more damage, and if knocked down in combat, dies. </color>";
                break;
            case "<color=#d80404>\n\nEnfermo por ":
                r = "<color=#d80404>\n\nSick for ";
                break;
            case " dÃ­as. -15% daÃ±o, -3 TS Fortaleza, -1 PA </color>":
                r = " days. -15% damage, -3 Fortitude, -1 AP </color>";
                break;
            case "<color=#d80404>\n\nBaja Moral por ":
                r = "<color=#d80404>\n\nLow Morale for ";
                break;
            case " dÃ­as. -1 Ataque y Defensa, -3 TS Mental, -2 ValentÃ­a Inicial</color>":
                r = " days. -1 Attack and Defense, -3 TS Mental, -2 Initial Valor</color>";
                break;
            case "<color=#d80404>\n\nAlta Moral por ":
                r = "<color=#d80404>\n\nHigh Morale for ";
                break;
            case " dÃ­as. +1 Ataque, +2 TS Mental, +2 ValentÃ­a Inicial</color>":
                r = " days. +1 Attack, +2 TS Mental, +2 Initial Valor</color>";
                break;
            case "Torpe: +1 Rango Pifias. ":
                r = "Clumsy: +1 Fumble Range.";
                break;
            case "Valiente: +2 ValentÃ­a MÃ¡xima.":
                r = "Brave: +2 Max Valor.";
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
            case "<color=#0cca74><b>Guardia: </b></color><color=#d3d3d3><i>El personaje se mantendrÃ¡ alerta y custodiarÃ¡ la caravana.</color></i>\\n\\nSi se produce una emboscada, podrÃ¡ participar de la defensa sin penalizaciÃ³n. +3% ExploraciÃ³n al descansar.":
                r = "<color=#0cca74><b>Guard: </b></color><color=#d3d3d3><i>The character will remain alert and guard the caravan.</color></i>\\n\\nIf an ambush occurs, they can participate in the defense without penalty. +3% Scouuting when resting.";
                break;
            case "<color=#0cca74><b>CoerciÃ³n: </b></color><color=#d3d3d3><i>Con mÃ©todos cuestionables, el Acechador obliga a los Mercaderes a donar dinero a la caravana.</color></i>\\n\\n+1-10 Oro y -1 Esperanza por dÃ­a.":
                r = "<color=#0cca74><b>Coercion: </b></color><color=#d3d3d3><i>Using questionable methods, the Stalker forces Merchants to donate money to the caravan.</color></i>\\n\\n+1-10 Gold and -1 Hope per day.";
                break;
            case "<color=#0cca74><b>ExploraciÃ³n: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo SubterrÃ¡neo\\nSi se da un combate, lo arranca Fatigado.":
                r = "<color=#0cca74><b>Scouting: </b></color><color=#d3d3d3><i>The character scouts possible destinations ahead of the caravan.</color></i>\\n\\nHas a 40% chance to reveal future Nodes when traveling to a new Node. -5% Chance of Mysterious Node. +5% Chance of Underground Shortcut\\nIf a combat occurs, they start Fatigued.";
                break;
            case "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirÃ¡ su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrÃ¡ +3 Flechas y +5% daÃ±o.":
                r = "<color=#0cca74><b>Prepare Arrows: </b></color><color=#d3d3d3><i>The character will spend their time creating and improving their arrows.</color></i>\\n\\nIf a combat occurs, they will have +3 Arrows and +5% damage.";
                break;
            case "<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocuparÃ¡ de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzarÃ¡ con +3 Armadura.":
                r = "<color=#0cca74><b>Armor Maintenance: </b></color><color=#d3d3d3><i>The character will take care of his armor.</color></i>\\n\\nIf a combat occurs, he will start with +3 Armor.";
                break;
            case "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerÃ¡ vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrÃ¡ participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daÃ±o los primeros 2 turnos.":
                r = "<color=#0cca74><b>Watch: </b></color><color=#d3d3d3><i>The character will remain vigilant against any danger.</color></i>\\n\\nIf an ambush occurs, they can actively participate in the defense and gain +2 AP, +5 Initiative, and +20% damage for the first 2 turns.";
                break;
            case "<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizarÃ¡ su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada dÃ­a que pase ganarÃ¡ 15 Experiencia.\\nSi se produce un combate, lo arrancarÃ¡ Fatigado.":
                r = "<color=#0cca74><b>Train: </b></color><color=#d3d3d3><i>The character will use their free time to train and stay in shape.</color></i>\\n\\nEach day that passes, they will gain 15 Experience.\\nIf a combat occurs, they will start Fatigued.";
                break;
            case "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrarÃ¡ en descansar y recuperar su salud.</color></i>\\n\\nCada dÃ­a que pase recuperarÃ¡ un 15% de salud.\\nSi se produce un combate, lo arrancarÃ¡ Fresco.":
                r = "<color=#0cca74><b>Rest: </b></color><color=#d3d3d3><i>The character will focus on resting and recovering their health.</color></i>\\n\\nEach day that passes, they will recover 15% of their health.\\nIf a combat occurs, they will start Fresh.";
                break;
            case "<color=#0cca74><b>Afilar Armas: </b></color><color=#d3d3d3><i>El Acechador se encarga de mantener sus armas afiladas.</color></i>\\n\\nSi se produce un combate tendrÃ¡ +10% daÃ±o.":
                r = "<color=#0cca74><b>Prepare Weapons: </b></color><color=#d3d3d3><i>The Stalker is responsible for keeping his weapons sharp.</color></i>\\n\\nIf a combat occurs, he will have +10% damage.";
                break;
            case "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.":
                r = "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>With his arcane telekinesis powers, he helps with the caravan's load.</color></i>\\n\\n+20 Carrying Capacity.";
                break;
            case "<color=#0cca74><b>Caza Nocturna: </b></color><color=#d3d3d3><i>El personaje cazarÃ¡ en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+1d4 Suministros por dÃ­a. +3% probabilidad de Emboscada Enemiga al descansar.":
                r = "<color=#0cca74><b>Night Hunting: </b></color><color=#d3d3d3><i>The character will hunt in the vicinity to obtain food for the caravan.</color></i>\\n\\n+1d4 Supplies per day. +3% chance of Enemy Ambush while resting.";
                break;
            case "<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirÃ¡ los relatos de sus hazaÃ±as con quienes quieran oÃ­rlas.</color></i>\\n\\n+10 Experiencia por dÃ­a a personajes de nivel inferior. +4 Esperanza al descansar.":
                r = "<color=#0cca74><b>Battle Tales: </b></color><color=#d3d3d3><i>The character will share the tales of their exploits with those who wish to hear them.</color></i>\\n\\n+10 Experience per day to lower-level characters. +4 Hope while resting.";
                break;
            case "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizarÃ¡ rituales de protecciÃ³n para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por dÃ­a.":
                r = "<color=#0cca74><b>Ritual of Cleansing: </b></color><color=#d3d3d3><i>The Purifier will perform protection rituals to combat the Black Breath.</color></i>\\n\\nChance to avoid Black Breath advance: 25% while resting, 15% per day.";
                break;
            case "<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usarÃ¡ su tiempo para ayudar a los rezagados y mÃ¡s dÃ©biles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.":
                r = "<color=#0cca74><b>Help the Hopeless: </b></color><color=#d3d3d3><i>The Purifier will use her time to help the laggards and weaker members of the caravan.</color></i>\\n\\n+1d3 Hope per day. +1 Fervor in combat.";
                break;
            case "<color=#0cca74><b>ConcentraciÃ³n Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de EnergÃ­a al iniciar combates.":
                r = "<color=#0cca74><b>Arcane Concentration: </b></color><color=#d3d3d3><i>The Channeler focuses and keeps their power ready for any combat that arises.</color></i>\\n\\n+1 Energy Level at the start of combats.";
                break;
            case "<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.":
                r = "<color=#0cca74><b>Watch from Shadows: </b></color><color=#d3d3d3><i>The Stalker moves stealthily around the caravan, trying to anticipate enemy ambushes.</color></i>\\n\\n-5% chance of ambushes.";
                break;
            case "<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>SÃ©quito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curaciÃ³n diaria del SÃ©quito de Curanderos.":
                r = "<color=#0cca74><b>Help the Healers' Retinue: </b></color><color=#d3d3d3><i>Helps the <b>Healers' Retinue</b> in their tasks, increasing their effectiveness.</color></i>\\n\\nIncreases the Healers' Retinue's daily healing by 5%.";
                break;
            case "<color=#0cca74><b>Crear SÃ­mbolo Arcano de ProtecciÃ³n: </b></color><color=#d3d3d3><i>El Canalizador concentra energÃ­a arcana protectora en un sÃ­mbolo que puede proteger a quien lo utilice.</color></i>\\n\\nCrea un SÃ­mbolo Arcano de ProtecciÃ³n por dÃ­a.":
                r = "<color=#0cca74><b>Create Arcane Protection Symbol: </b></color><color=#d3d3d3><i>The Channeler concentrates protective arcane energy into a symbol that can protect its user.</color></i>\\n\\nCreates one Arcane Protection Symbol per day.";
                break;
            case "-El viaje por el camino escarpado ha demorado la caravana. +":
                r = "-The journey along the steep path has delayed the caravan. +";
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
            case "-El SÃ©quito de Herboristas ha visitado un Claro y recolectado hierbas curativas.":
                r = "-The Herbalists' Retinue has visited a Glade and collected healing herbs.";
                break;
            case " ha realizado con Ã©xito un Ritual de Limpieza, previniendo el avance del Aliento Negro.":
                r = "has succesfully performed a Cleansing Ritual, avoiding the Black Breath advance.";
                break;
            case "-Los rezos constantes del SÃ©quito de ClÃ©rigos han logrado frenar el avance del Aliento Negro.":
                r = "-The constant prayers of the Clerics' Retinue have managed to halt the advance of the Black Breath.";
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
            case "No puedes intercambiar con una unidad que ya estÃ¡ Desplazada.":
                r = "You cannot swap with a unit that is already Displaced.";
                break;
            case "No puedes intercambiar con obstÃ¡culos.":
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
            case " estÃ¡ congelado.":
                r = " is frozen.";
                break;
            case "Descongelado!":
                r = "Unfrozen!";
                break;
            case " se librÃ³ del congelamiento.":
                r = " is no longer frozen.";
                break;
            case " estÃ¡ aturdido.":
                r = " is stunned.";
                break;
            case " regenera ":
                r = " regenerates ";
                break;
            case " Armadura.":
                r = " Armor.";
                break;
            case " estÃ¡ inmovilizado.":
                r = " is immobilized.";
                break;
            case " recibe ":
                r = " receives ";
                break;
            case " daÃ±o veneno.":
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
            case " falla su Tirada de salvaciÃ³n y el veneno empeora.":
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
            case " inmÃ³vil":
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
            case " ya no estÃ¡ escondido.":
                r = " is no longer hidden.";
                break;
            case " estÃ¡ escondido.":
                r = " is hidden.";
                break;
            case "La Barrera de ":
                r = "The Barrier of ";
                break;
            case " absorbiÃ³ ":
                r = " absorbed ";
                break;
            case " de daÃ±o.":
                r = " of damage.";
                break;
            case " de daÃ±o ":
                r = " of damage type";
                break;
            case "Cura ":
                r = "Heal ";
                break;
            case " recibe <color=#11c66b>":
                r = " receives <color=#11c66b>";
                break;
            case "</color> de curaciÃ³n.":
                r = "</color> healing.";
                break;
            case " muere.":
                r = " dies.";
                break;
            case " realiza Tirada de SalvaciÃ³n: 1d20 = ":
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
            case "-El SÃ©quito de Cronistas ha registrado el viaje. +20 Valor CrÃ³nica.":
                r = "-The Chroniclers Retinue has recorded the journey. +20 Chronicle Value.";
                break;
            case "-El SÃ©quito de Nobles ha hecho una donaciÃ³n. Oro: ":
                r = "-The Nobles Retinue has made a donation. Gold: ";
                break;
            case "-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza.":
                r = "-The Civilians feel guilty about the presence of the Slaves. -2 Hope.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se ha gastado 200 de oro.":
                r = "-You have performed a ritual in the sanctuary. The Black Breath recedes by 3 and 200 gold has been spent.";
                break;
            case " ha sido purificado de la corrupciÃ³n.":
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
            case "-El SÃ©quito de Artistas ha tenido un festÃ­n y despilfarrado suministros: ":
                r = "-The Artists' Retinue has feasted and squandered supplies: ";
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
            case " brinda 10 Experiencia a sus compaÃ±eros de menor nivel por su Actividad de <b>Relatos de Batalla</b>.":
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
            case " ha creado un SÃ­mbolo de ProtecciÃ³n Arcano.":
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
            case "-El SÃ©quito de Nobles se queja por la falta de descanso. -2 Esperanza":
                r = "-The Nobles' Retinue complains about the lack of rest. -2 Hope";
                break;
            case "-Tus personajes estÃ¡n fatigados. AfectarÃ¡ su rendimiento en batalla.":
                r = "-Your characters are tired. It will affect their performance in battle.";
                break;
            case "-El sacrificio de Bueyes para obtener Suministros ha provocado preocupaciÃ³n. -2 Esperanza":
                r = "-The sacrifice of Oxen to obtain Supplies has caused concern. -2 Hope";
                break;
            case "-Los Cronistas han registrado la victoria, +50 Valor CrÃ³nica, +5 Esperanza.":
                r = "-The Chroniclers have recorded the victory, +50 Chronicle Valor, +5 Hope.";
                break;
            case "-Los Cronistas han registrado la derrota, -50 Valor CrÃ³nica. -3 Esperanza.":
                r = "-The Chroniclers have recorded the defeat, -50 Chronicle Valor. -3 Hope.";
                break;
            case "Victoria sin recompensas definidas para este encuentro clÃ¡sico.":
                r = "Victory with no rewards defined for this classic encounter.";
                break;
            case "Derrota en un encuentro clÃ¡sico. Los efectos especÃ­ficos aÃºn no estÃ¡n configurados.":
                r = "Defeat in a classic encounter. Specific effects are not configured yet.";
                break;
            case "sin botÃ­n":
                r = "no loot";
                break;
            case " ha sido corrompido.":
                r = " has been corrupted.";
                break;
            case "-Se ha unido el SÃ©quito de Artistas a la caravana. +25 Civiles":
                r = "-The Artists' Retinue has joined the caravan. +25 Civilians";
                break;
            case "SÃ©quito de Herreros":
                r = "Blacksmiths' Retinue";
                break;
            case "SÃ©quito de Curanderos":
                r = "Healers' Retinue";
                break;
            case "SÃ©quito de Mercaderes":
                r = "Merchants' Retinue";
                break;
            case "SÃ©quito de Artistas":
                r = "Artists' Retinue";
                break;
            case "SÃ©quito de Herboristas":
                r = "Herbalists' Retinue";
                break;
            case "SÃ©quito de Desertores":
                r = "Deserters' Retinue";
                break;
            case "SÃ©quito de Cronistas":
                r = "Chroniclers' Retinue";
                break;
            case "SÃ©quito de Refugiados":
                r = "Refugees' Retinue";
                break;
            case "SÃ©quito de Nobles":
                r = "Nobles' Retinue";
                break;
            case "SÃ©quito de ClÃ©rigos":
                r = "Clerics' Retinue";
                break;
            case "SÃ©quito de Esclavos":
                r = "Slaves' Retinue";
                break;
            case "-Se ha unido el SÃ©quito de Herboristas a la caravana. +10 Civiles":
                r = "-The Herbalists' Retinue has joined the caravan. +10 Civilians";
                break;
            case "-Los Desertores se han unido a la Caravana. +15 Civiles -8 Esperanza":
                r = "-The Deserters' Retinue has joined the caravan. +15 Civilians -8 Hope";
                break;
            case "-Los Cronistas se han unido a la Caravana. +10 Civiles":
                r = "-The Chroniclers' Retinue has joined the caravan. +10 Civilians";
                break;
            case "-Los Refugiados se han unido a la Caravana. +35 Civiles  +30 Esperanza":
                r = "-The Refugees' Retinue has joined the caravan. +35 Civilians +30 Hope";
                break;
            case "-Los Nobles se han unido a la Caravana. +25 Civiles":
                r = "-The Nobles' Retinue has joined the caravan. +25 Civilians";
                break;
            case "-Los ClÃ©rigos del Sol Purificador se han unido a la Caravana. +20 Civiles +15 Esperanza":
                r = "-The Purifying Sun Clerics' Retinue has joined the caravan. +20 Civilians +15 Hope";
                break;
            case "-Los Esclavos se han unido a la Caravana. +30 Civiles":
                r = "-The Slaves' Retinue has joined the caravan. +30 Civilians";
                break;
            case "-El SÃ©quito de Artistas ha abandonado la caravana. -25 Civiles -15 Esperanza":
                r = "-The Artists' Retinue has left the caravan. -25 Civilians -15 Hope";
                break;
            case "-El SÃ©quito de Herboristas ha abandonado la caravana. -10 Civiles":
                r = "-The Herbalists' Retinue has left the caravan. -10 Civilians";
                break;
            case "-Los Desertores han abandonado la Caravana. -15 Civiles":
                r = "-The Deserters' Retinue has left the caravan. -15 Civilians";
                break;
            case "-Los Cronistas han abandonado la Caravana. -10 Civiles":
                r = "-The Chroniclers' Retinue has left the caravan. -10 Civilians";
                break;
            case "-Los Refugiados han abandonado la Caravana. -35 Civiles -40 Esperanza":
                r = "-The Refugees' Retinue has left the caravan. -35 Civilians -40 Hope";
                break;
            case "-Los Nobles han abandonado la Caravana. -25 Civiles":
                r = "-The Nobles' Retinue has left the caravan. -25 Civilians";
                break;
            case "-Se ha vendido la crÃ³nica del viaje por Oro: ":
                r = "-The chronicle of this journey has been sold for Gold: ";
                break;
            case " ha recibido tratamiento especial y sus heridas han sanado.":
                r = " has received special treatment and their wounds have healed.";
                break;
            case "Un grupo de eruditos unidos que se dedican a registrar los sucesos del viaje de la caravana hacia el puerto. Sus escrituras pueden ser una fuenta de ingresos y moral, pero tambiÃ©n puede ser contraproducente en los peores momentos.\n\n":
                r = "A group of scholars united to record the events of the caravan's journey to the port. Their writings can be a source of income and morale, but they can also be counterproductive in the worst moments.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +5 de Esperanza por batallas ganadas (-3 Derrotas). ":
                r = "PASSIVE EFFECTS:\n\n-Grant +5 Hope for battles won (-3 Defeats). ";
                break;
            case "\n\n-Ya se ha vendido la crÃ³nica de este viaje.":
                r = "\n\n-The chronicle of this journey has been sold.";
                break;
            case "\n\n- CrÃ³nica: Acumula valor de la siguiente manera:":
                r = "\n\n- Chronicle: Accumulates value as follows:";
                break;
            case "\n   â€¢ Base: 150 Oro":
                r = "\n   â€¢ Base: 150 Gold";
                break;
            case "\n   â€¢ +1 Oro por cada punto de Esperanza":
                r = "\n   â€¢ +1 Gold for each point of Hope";
                break;
            case "\n   â€¢ +20 Oro por cada nodo viajado":
                r = "\n   â€¢ +20 Gold for each node traveled";
                break;
            case "\n   â€¢ +50 Oro por cada batalla ganada / -50 Oro por cada batalla perdida":
                r = "\n   â€¢ +50 Gold for each battle won / -50 Gold for each battle lost";
                break;
            case "\n\nSe puede vender en Asentamientos o Puestos Comerciales.":
                r = "\n\nIt can be sold in Settlements or Trading Posts.";
                break;
            case "\n\n\n\n-Valor CrÃ³nica: Oro: ":
                r = "Chronicle Value: Gold: ";
                break;
            case "% por Herboristas":
                r = " % for Herbalists";
                break;
            case "Carros de Tratamiento: Mejorar los carros utilizados por el SÃ©quito de Curanderos para tratar heridos significarÃ¡ una mejora en los tratamientos recibidos por los heridos y su tiempo de recuperaciÃ³n. \nCada Tier aumenta en 5% la curaciÃ³n diaria de los personajes que Descansen y reduce el costo de Tratar Heridas. \nAdemÃ¡s cada tier da un 10% extra a las posibilidades de reducir Enfermedades al Descansar (20% base). \nCuraciÃ³n proporcionada: ":
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
            case "Han sido esclavos toda su vida, e incluso en estas circunstancias se comportan como tal. La situaciÃ³n amerita aprovecharse de su condiciÃ³n para obtener ventajas de mano de obra, Â¿o quizÃ¡s llegÃ³ el momento de liberarlos?\n\n":
                r = "They have been slaves all their lives, and even in these circumstances they behave as such. The situation calls for taking advantage of their condition to gain labor advantages, or perhaps the time has come to free them?\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan +50 Capacidad de Carga\n\n-Cada descanso juntan 10-15 Materiales.\n\n-Cada Viaje se pierden 2 de Esperanza.\n\n-Al ser liberados, se convierten en Civiles comunes y otorgan +25 Esperanza.":
                r = "PASSIVE EFFECTS:\n\n-Grant +50 Carrying Capacity\n\n-Each rest gathers 10-15 Materials.\n\n-Each Journey loses 2 Hope.\n\n-When freed, they become common Civilians and grant +25 Hope.";
                break;
            case "-Los Esclavos han sido liberados y ahora son Civiles comunes. +25 Esperanza":
                r = "-The Slaves have been freed and are now common Civilians. +25 Hope";
                break;
            case "TamaÃ±o Tiendas: ":
                r = "Store Size:";
                break;
            case "-El SÃ©quito de Mercaderes ha actualizado su oferta.":
                r = "-The Merchants' Retinue has updated its offer.";
                break;
            case " es escondido en las sombras tras recibir un ataque crÃ­tico por su Armadura de Velo.":
                r = " is hidden in the shadows after receiving a critical hit from its Cloak Armor.";
                break;
            case "Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periÃ³dicamente parte de su riqueza para asegurarse de que no serÃ¡n abandonados.\n\n":
                r = "A group of nobles who were forced to leave the comfort of their lands now travel with the caravan. While they are complaining and not very useful, at least they periodically donate part of their wealth to ensure they are not abandoned.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Cada dÃ­a donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.":
                r = "PASSIVE EFFECTS:\n\n-Each day they donate Gold equivalent to 1/3 of Hope.\n\n-2 Hope is lost when traveling with fatigue 4 or higher.";
                break;
            case "Los ClÃ©rigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayorÃ­a murieron en la onda expansiva en ese momento, pero todavÃ­a quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n":
                r = "The Clerics of the Radiant Purifying Sun participated as support in the fight against the Lich. Most died in the shockwave at that moment, but there are still some groups trying to reach the port and survive while they fight to delay the Black Breath.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a nivel superior a 16, los ClÃ©rigos mueren.":
                r = "PASSIVE EFFECTS:\n\n-Grant 15 Hope when joining the Caravan, -20 Hope when getting lost.\n\n-20% chance to Delay the Black Breath on each journey.\n\n-If the Black Breath reaches a level higher than 16, the Clerics die.";
                break;
            case "<color=red>La plegaria ya fue realizada.</color>":
                r = "<color=red>The prayer has already been made.</color>";
                break;
            case "<color=red>No hay oro suficiente para una donaciÃ³n de 250 Oro.</color>":
                r = "<color=red>There is not enough gold for a donation of 250 Gold.</color>";
                break;
            case "Se harÃ¡ una donaciÃ³n de 250 Oro.":
                r = "A donation of 250 Gold will be made.";
                break;
            case "Mantenimiento Armas: El Herrero se encargarÃ¡ de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daÃ±o en 2. Este efecto Dura 3 dÃ­as.":
                r = "Weapon Maintenance: The Blacksmith will take care of general maintenance of the characters' weapons. Increasing their Attack by 1 and their damage by 2. This effect lasts 3 days.";
                break;
            case "Mantenimiento Armaduras: El Herrero se encargarÃ¡ de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 dÃ­as.":
                r = "Armor Maintenance: The Blacksmith will take care of general maintenance of the characters' armor. Increasing their Defense by 1 and their Armor by 2. This effect lasts 3 days.";
                break;
            case "Realizar: 200 Oro":
                r = "Perform: 200 Gold";
                break;
            case "Activo por ":
                r = "Active for ";
                break;
            case " DÃ­as":
                r = " Days";
                break;
            case "Armas Civiles: El herrero se dedica a mejorar las armas rudimentarias de los civiles, mejorando las posibilidades de defensa de las Milicias. \nCada Tier aumenta en 10% los Civiles que suman fuerza para la Milicia.":
                r = "Civilian Weapons: The blacksmith is dedicated to improving the rudimentary weapons of civilians, enhancing the defense capabilities of the Militias. \nEach Tier increases the Civilians contributing strength to the Militia by 10%.";
                break;
            case "Estos soldados abandonaron su puesto en el ejÃ©rcito en pos de sobrevivir. Hambrientos y avergonzados, ofrecen protecciÃ³n a la Caravana pidiendo solo un lugar en ella, aunque a una parte de los civiles les desagrade la idea.\n\n":
                r = "These soldiers abandoned their post in the army in order to survive. Hungry and ashamed, they offer protection to the Caravan asking for only a place in it, although some of the civilians dislike the idea.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Participan en la defensa de la Caravana, reemplazando a los inexpertos Milicianos. \n\n-Otorga 10 Experiencia extra a Personajes que Entrenan. \n\n-Al aceptarlos la Esperanza disminuye en 8.":
                r = "PASSIVE EFFECTS:\n\n-They participate in the defense of the Caravan, replacing inexperienced Militiamen. \n\n-Grants 10 extra Experience to Characters Training. \n\n-By accepting them, Hope decreases by 8.";
                break;
            case "Varios civiles que estuvieron a la deriva mucho tiempo buscando sobrevivir. Compuesto de mayormente de ancianos, mujeres y niÃ±os desnutridos. Consumen menos comida de lo normal y su presencia llena de regocijo a la Caravana porque se hizo lo correcto al recibirlos. Ahora habrÃ¡ que cuidar de ellos.\n\n":
                r = "Several civilians who have been adrift for a long time searching for survival. Composed mostly of elderly, women, and malnourished children. They consume less food than normal and their presence fills the Caravan with joy because it was the right thing to do to receive them. Now they will have to be taken care of.";
                break;
            case "EFECTOS PASIVOS:\n\n-Consumen la mitad de Suministros que los Civiles habituales. \n\n-Al aceptarlos la Esperanza aumenta en 30. \n\n-Al perderlos la Esperanza disminuye en 40.":
                r = "PASSIVE EFFECTS:\n\n-They consume half the Supplies of regular Civilians. \n\n-By accepting them, Hope increases by 30. \n\n-By losing them, Hope decreases by 40.";
                break;
            case "Un grupo de especialistas en recolectar hierbas y crear con ellas bÃ¡lsamos especiales para vender. \nAdemÃ¡s, sus hierbas proporcionarÃ¡n beneficios curativos a la caravana.\nPero quizÃ¡s no sean demasiado cuidadosos al adentrarse en zonas peligrosas para recolectar hierbas.\n\n":
                r = "A group of specialists in gathering herbs and creating special balms from them to sell. \nIn addition, their herbs will provide healing benefits to the caravan.\nBut perhaps they are not too careful when venturing into dangerous areas to gather herbs.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Hierbas curativas: Mejoran ":
                r = "PASSIVE EFFECTS:\n\n-Healing Herbs: Improve ";
                break;
            case "% la curaciÃ³n pasiva de la Caravana.\n\nEste Ã­ndice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.":
                r = "the passive healing of the Caravan.\n\nThis rate increases by 3% each time the Caravan visits a Clearing.\n\n-They are sometimes careless when gathering herbs. +2% chance of a caravan attack occurring after resting.";
                break;
            case "50 de oro":
                r = "50 gold";
                break;
            case "El sÃ©quito de Herreros se encarga del mantenimiento y manufactura de las armas y armaduras de la Caravana. Su carro es especialmente pesado ya que, montado ingeniosamente, carga con todas las necesidades bÃ¡sicas de un herrero":
                r = "The Smiths' Retinue is responsible for the maintenance and manufacture of the Caravan's weapons and armor. Their cart is especially heavy as it ingeniously carries all the basic needs of a blacksmith.";
                break;
            case "Cantidad de Civiles: No.":
                r = "Number of Civilians: No.";
                break;
            case "150 Oro":
                r = "150 Gold";
                break;
            case "300 Oro":
                r = "300 Gold";
                break;
            case "El SÃ©quito de Curanderos se encarga de atender a los heridos y enfermos de la Caravana. Pese a las circunstancias del viaje mismo, logran mantenerse en funcionamiento y brindan un servicio escencial para la supervivencia de quienes lo necesiten.":
                r = "The Healers' Retinue is responsible for attending to the wounded and sick of the Caravan. Despite the circumstances of the journey itself, they manage to keep functioning and provide an essential service for the survival of those in need.";
                break;
            case "Tratar Heridas":
                r = "Treat Wounds";
                break;
            case "Este sÃ©quito estÃ¡ constituÃ­do por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercaderÃ­a. EstÃ¡n dispuestos a comercial a precios rebajados pero sin renunciar al menos a una mÃ­nima ganancia.":
                r = "This retinue consists of several merchants who have had to abandon their shops but have not given up their merchandise. They are willing to trade at reduced prices but without renouncing at least a minimal profit.";
                break;
            case "Aumentar el tamaÃ±o de las tiendas incrementa la cantidad de objetos ofrecidos.":
                r = "Increasing the size of the shops increases the number of items offered.";
                break;
            case "Varios artistas y miembros de una feria ambulante se han unido a la caravana, si bien son ostentosos y despilfarran recursos, pueden ayudar a la moral de la caravana en determinadas ocasiones festivas.":
                r = "Several artists and members of a traveling fair have joined the caravan, and although they are ostentatious and wasteful of resources, they can help boost the morale of the caravan during certain festive occasions.";
                break;
            case "Cantidad de Civiles: 25":
                r = "Number of Civilians: 25";
                break;
            case "EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada dÃ­a hay un 30% de chances de que hagan un festÃ­n y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.":
                r = "PASSIVE EFFECTS:\n\n-Joining the Caravan grants 15 Hope.\n\n-Each time Fair is selected as a Civil Rest Task, an additional 10 Hope is gained.\n\n-Each day there is a 30% chance they will hold a feast and waste 1-4 Supplies.\n\n-If they leave the Caravan, 15 Hope is lost.";
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
            case "<b>Â¡Enfurecido!</b>":
                r = "<b>Enraged!</b>";
                break;
            case "Sangre Devorada":
                r = "Leeched Blood";
                break;
            case "EufÃ³rico":
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
            case "PerdiciÃ³n":
                r = "Damnation";
                break;
            case "Encarnado":
                r = "Incarnated";
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
            case "FrÃ­o":
                r = "Cold";
                break;
            case "Aliento Negro: DÃ©bil":
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
            case "DistraÃ­do":
                r = "Distracted";
                break;
            case "Implacable":
                r = "Unrelenting";
                break;
            case "DeterminaciÃ³n":
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
            case " fallÃ³ la Tirada de ConcentraciÃ³n y ya no acumula energÃ­a.":
                r = " failed the Concentration Roll and is no longer Gathering energy.";
                break;
            case "Energizado":
                r = "Energized";
                break;
            case "Acumulacion Inestable":
                r = "Unstable Gathering";
                break;
            case "Escudo EnergÃ©tico":
                r = "Energy Shield";
                break;
            case "EnergÃ­a Absorbida":
                r = "Absorbed Energy";
                break;
            case "Residuo EnergÃ©tico":
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
            case "BÃ¡lsamo de Claridad":
                r = "Balm of Clarity";
                break;
            case "BÃ¡lsamo Energizante":
                r = "Energizing Balm";
                break;
            case "BÃ¡lsamo Fortalecedor":
                r = "Fortifying Balm";
                break;
            case "Elixir de Resistencia al FrÃ­o":
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
            case "Elixir de Resistencia al Ãcido":
                r = "Elixir of Acid Resistance";
                break;
            case "ProtecciÃ³n Arcana":
                r = "Arcane Protection";
                break;
            case "IA Lenguetazo":
                r = "AI Tongue Lash";
                break;
            case "Saboreado":
                r = "Savored";
                break;
            case "recibirÃ¡ mÃ¡s daÃ±o del Zarkilever":
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
            case "Ardiendo: causa daÃ±o cada turno, se apaga con AP disponibles.":
                r = "Burning: deals damage each turn, extinguished with available AP.";
                break;
            case "Aturdido: no puede actuar.":
                r = "Stunned: cannot act.";
                break;
            case "Ãcido: cada acumulaciÃ³n reduce en 1 la armadura.":
                r = "Acid: each stack reduces armor by 1.";
                break;
            case "Congelado: reduce PA disponibles y aumenta armadura.":
                r = "Frozen: reduces available AP and increases armor.";
                break;
            case "Resistencias Reducidas: reduce todas las resistencias 1 por acumulaciÃ³n.":
                r = "Reduces all resistances by 1 per stack.";
                break;
            case "Armadura Rota: reduce la armadura en 1 por acumulaciÃ³n.":
                r = "Broken Armor: reduces armor by 1 per stack.";
                break;
            case "Sangrado: cada acumulaciÃ³n resta 1 HP mÃ¡xima por turno y previene 2 de curaciÃ³n.":
                r = "Bleeding: each stack reduces max HP by 1 per turn and prevents 2 healing.";
                break;
            case "Veneno: provoca daÃ±o por turno, se debe hacer una tirada de salvaciÃ³n de Fortaleza cada turno para curarse, si falla se incrementa en 1.":
                r = "Poison: deals damage each turn, a Fortitude saving throw must be made each turn to heal, if it fails it increases by 1.";
                break;
            case "RegeneraciÃ³n: recupera vida cada turno.":
                r = "Regeneration: recovers health each turn.";
                break;
            case "RegeneraciÃ³n Armadura: recupera Armadura perdida cada turno.":
                r = "Armor Regeneration: recovers lost armor each turn.";
                break;
            case "EvasiÃ³n: cada stack aumenta 1 la Defensa, se elimina al recibir daÃ±o.":
                r = "Evasion: each stack increases Defense by 1, removed upon taking damage.";
                break;
            case "Flechas: Cantidad de flechas disponibles.":
                r = "Arrows: Number of available arrows.";
                break;
            case " Bonus daÃ±o elemental Acido.":
                r = " Bonus Acid elemental damage.";
                break;
            case " Bonus daÃ±o elemental Arcano.":
                r = " Bonus Arcane elemental damage.";
                break;
            case " Bonus daÃ±o elemental Fuego.":
                r = " Bonus Fire elemental damage.";
                break;
            case " Bonus daÃ±o elemental Hielo.":
                r = " Bonus Cold elemental damage.";
                break;
            case " Bonus daÃ±o elemental Necro.":
                r = " Bonus Necrotic elemental damage.";
                break;
            case " Bonus daÃ±o elemental Divino.":
                r = " Bonus Divine elemental damage.";
                break;
            case " Bonus daÃ±o elemental Rayo.":
                r = " Bonus Lightning elemental damage.";
                break;
            case "Fervor: Cantidad de Fervor que tiene la purificadora.":
                r = "Fervor: Amount of Fervor the purifier has.";
                break;
            case "Barrera: previene X cantidad de daÃ±o.":
                r = "Barrier: prevents X amount of damage.";
                break;
            case "Residuo de Tejido: se obtiene al recibir curaciÃ³n de origen mÃ¡gico. Previene X puntos de curaciÃ³n.":
                r = "Tissue Residue: obtained when receiving healing from magical sources. Prevents X points of healing.";
                break;
            case "Escondido I: Esta unidad estÃ¡ escondida y los enemigos no pueden atacarla. El efecto se remueve al atacar o recibir daÃ±o.":
                r = "Hidden I: This unit is hidden and enemies cannot attack it. The effect is removed upon attacking or taking damage.";
                break;
            case "Escondido II: Esta unidad estÃ¡ escondida y los enemigos no pueden atacarla. El efecto no se remueve al recibir daÃ±o.":
                r = "Hidden II: This unit is hidden and enemies cannot attack it. The effect is not removed upon taking damage.";
                break;
            case "EnergÃ­a: Nivel de EnergÃ­a Acumulada por el Canalizador.":
                r = "Energy Level accumulated by the Channeler.";
                break;
            case "Corrupto: Recibe daÃ±o adicional de enemigos Corrompidos que ademÃ¡s se curan al daÃ±arlo. Si lo deja fuera de combate un enemigo corrompido, muere.":
                r = "Corrupted: Takes additional damage from Corrupted enemies who also heal when damaging it. If a Corrupted enemy takes it out of combat, it dies.";
                break;
            case "HP MÃ¡ximo: ":
                r = "Max HP: ";
                break;
            case "PA MÃ¡ximo: ":
                r = "Max AP: ";
                break;
            case "PM MÃ¡ximo: ":
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
            case "Resistencia Ãcido: ":
                r = "Acid Resistance: ";
                break;
            case "Resistencia Arcano: ":
                r = "Arcane Resistance: ";
                break;
            case "Resistencia NecrÃ³tica: ":
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
            case "DaÃ±o: ":
                r = "Damage: ";
                break;
            case "CrÃ­tico Dado: ":
                r = "Critical Hit: ";
                break;
            case "DaÃ±o CrÃ­tico: ":
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
            case "Bonus daÃ±o Ã¡cido: ":
                r = "Bonus Acid Damage: ";
                break;
            case "Bonus daÃ±o arcano: ":
                r = "Bonus Arcane Damage: ";
                break;
            case "Bonus daÃ±o fuego: ":
                r = "Bonus Fire Damage: ";
                break;
            case "Bonus daÃ±o hielo: ":
                r = "Bonus Cold Damage: ";
                break;
            case "Bonus daÃ±o necro: ":
                r = "Bonus Necrotic Damage: ";
                break;
            case "Bonus daÃ±o rayo: ":
                r = "Bonus Lightning Damage: ";
                break;
            case "DuraciÃ³n: ":
                r = "Duration: ";
                break;
            case "DuraciÃ³n: Permanente\n":
                r = "Duration: Permanent\n";
                break;
            case " rondas\n":
                r = " rounds\n";
                break;
            case "Ataque: ":
                r = "Attack: ";
                break;
            case "Defensa: determina capacidad para evadir ataques.":
                r = "Defense: determines ability to evade attacks.";
                break;
            case "Armadura: reduce el daÃ±o fÃ­sico recibido.":
                r = "Armor: reduces physical damage taken.";
                break;
            case "Reflejos: resistencia a determinados efectos de ataques.":
                r = "Reflexes: resistance to certain attack effects.";
                break;
            case "Fortaleza: resistencia a efectos fÃ­sicos.":
                r = "Fortitude: resistance to physical effects.";
                break;
            case "Mental: resistencia a efectos mentales.":
                r = "Mental: resistance to mental effects.";
                break;
            case "ValentÃ­a: recurso para habilidades especiales.":
                r = "Courage: resource for special abilities.";
                break;
            case "Resistencia al Fuego: Cantidad de daÃ±o que previene.":
                r = "Fire Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al FrÃ­o: Cantidad de daÃ±o que previene.":
                r = "Cold Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Rayo: Cantidad de daÃ±o que previene.":
                r = "Lightning Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Ãcido: Cantidad de daÃ±o que previene.":
                r = "Acid Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia Arcana: Cantidad de daÃ±o que previene.":
                r = "Arcane Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia NecrÃ³tica: Cantidad de daÃ±o que previene.":
                r = "Necrotic Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia Divina: Cantidad de daÃ±o que previene.":
                r = "Divine Resistance: Amount of damage it prevents.";
                break;
            case "Residuo EnergÃ©tico: Otorga daÃ±o arcano y hiere levemente.":
                r = "Energetic Residue: Grants arcane damage and lightly wounds.";
                break;
            case "Zona bajo Vigilancia del Explorador.":
                r = "Area under Explorer's Watch.";
                break;
            case "AÃ±ade daÃ±o fuego al Explorador si estÃ¡ adyacente.":
                r = "Adds fire damage to the Explorer if adjacent.";
                break;
            case "Abrojos: Inflige daÃ±o y puede desangrar.":
                r = "Caltrops: Inflicts damage and can bleed.";
                break;
            case "Eco Divino: Cura a aliados y daÃ±a a enemigos.":
                r = "Divine Echo: Heals allies and damages enemies.";
                break;
            case "Humo: Esconde a los personajes dentro.":
                r = "Smoke: Hides characters within.";
                break;
            case "Escudo de Fe: Protege a los aliados dentro.":
                r = "Shield of Faith: Protects allies within.";
                break;
            case "Masa Contaminada: Hace daÃ±o Ã¡cido. Potencia enemigos corruptos.":
                r = "Contaminated Mass: Deals acid damage. Empowers corrupted enemies.";
                break;
            case "Pinchos: DaÃ±a a enemigos que los pisen.":
                r = "Spikes: Damages enemies that step on them.";
                break;
            case "Barricada: ObstÃ¡culo para enemigos. Hiere al ser atacada.":
                r = "Barricade: Obstacle for enemies. Damage when attacked.";
                break;
            case "Puesto de Tiro: Aumenta ataque y defensa a aliados dentro.":
                r = "Shooting Post: Increases attack and defense for allies within.";
                break;
            case "Pilar de Luz: ObstÃ¡culo que daÃ±a a enemigos al ser atacado.":
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
            case "<i>Los Caballeros siempre andan equipados con un mandoble muy pesado y poderoso. Junto con su armadura pesada, hacen el nÃºcleo del equipo de estos valientes guerreros.</i><b>\n\nOtorga: Corte Vertical</b>":
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
            case "Armadura de Cuero Reforzado de ProtecciÃ³n Elemental +1":
                r = "Reinforced Leather Armor of Elemental Protection +1";
                break;
            case "Precio: ":
                r = "Price: ";
                break;
            case "Efectos del item:":
                r = "Item effects:";
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
            case "Armadura de Cuero NecrÃ³tico +1":
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
            case "Arco Largo Ãcido +1":
                r = "Acid Longbow +1";
                break;
            case "Arco Largo Potente +1":
                r = "Powerful Longbow +1";
                break;
            case "Arco Largo Ralentizante +2":
                r = "Slowing Longbow +2";
                break;
            case "BÃ¡culo Purificador":
                r = "Purifying Staff";
                break;
            case "BÃ¡culo Purificador +1":
                r = "Purifying Staff +1";
                break;
            case "BÃ¡culo Purificador +2":
                r = "Purifying Staff +2";
                break;
            case "BÃ¡culo Purificador +3":
                r = "Purifying Staff +3";
                break;
            case "PociÃ³n de CuraciÃ³n Menor":
                r = "Lesser Healing Potion";
                break;
            case "PociÃ³n de CuraciÃ³n Mayor":
                r = "Greater Healing Potion";
                break;
            case "PociÃ³n de CuraciÃ³n":
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
            case "SÃ­mbolo de ProtecciÃ³n Arcano":
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
            case "Aumenta la resistencia al frÃ­o en 5 por el combate.":
                r = "Increases cold resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al fuego en 5 por el combate.":
                r = "Increases fire resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al rayo en 5 por el combate.":
                r = "Increases lightning resistance by 5 for the combat.";
                break;
            case "Aumenta la resistencia al Ã¡cido en 5 por el combate.":
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
            case "Espada Corta LadrÃ³n":
                r = "Thief's Short Sword";
                break;
            case "Envenenar Arma":
                r = "Poison Weapon";
                break;
            case "Mordida Perro Adiestrado":
                r = "Trained Dog Bite";
                break;
            case "EmpujÃ³n RufiÃ¡n":
                r = "Shove";
                break;
            case "Mazo RufiÃ¡n":
                r = "Mace Strike";
                break;
            case "Arrojar CorrosiÃ³n":
                r = "Throw Corrosion";
                break;
            case "Proliferar CorrupciÃ³n":
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
            case "RÃ¡faga de Espinas":
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
            case "ReacciÃ³n: Al morir, enfurecerÃ¡ a otros Lobos Espectrales.":
                r = "Reaction: Upon death, it will enrage other Spectral Wolves.";
                break;
            case "Golpe Enredado":
                r = "Entangled Strike";
                break;
            case "Lobo Espectral":
                r = "Spectral Wolf";
                break;
            case "<i>El Lobo Espectral es un enemigo feroz que se mueve y ataca rÃ¡pidamente, mientras su destreza animal le brinda una buena defensa.</i>\n\n<color=#199F10>-Posee un mordisco imbuÃ­do en fuego que ademÃ¡s de daÃ±ar, puede hacer arder a sus enemigos.</color>\n<color=#EE0000>-EstadÃ­sticas dÃ©biles.</color>":
                r = "<i>The Spectral Wolf is a fierce enemy that moves and attacks quickly, while its animal dexterity provides good defense.</i>\n\n<color=#199F10>-Has a fire-infused bite that not only damages but can also set enemies ablaze.</color>\n<color=#EE0000>-Weak statistics.</color>";
                break;
            case "Lobo Alfa Espectral":
                r = "Alpha Spectral Wolf";
                break;
            case "<i>El Lobo Alfa Espectral es el lÃ­der de la manada, posee una complexiÃ³n mas fuerte y resistente que los demÃ¡s lobos aunque es un poco menos Ã¡gil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demÃ¡s lobos.</color>\n<color=#EE0000>-Si queda sÃ³lo no podrÃ¡ motivar a nadie.</color>":
                r = "<i>The Alpha Spectral Wolf is the leader of the pack, possessing a stronger and more resilient build than the other wolves, though it is slightly less agile.</i>\n\n<color=#199F10>-Has the ability to howl to motivate other wolves.</color>\n<color=#EE0000>-If left alone, it will be unable to motivate anyone.</color>";
                break;
            case "Driada Quemada":
                r = "Burnt Dryad";
                break;
            case "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raÃ­ces ignÃ­fugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente dÃ©bil.</color>":
                r = "<i>Once servants and caretakers of the forest, they are now manifestations of vengeance and hatred against any invader of the Burning Forest.</i>\n\n<color=#199F10>-Can entangle with fire-resistant roots.\n-Ranged attack.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Espectro del Bosque":
                r = "Forest Specter";
                break;
            case "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucciÃ³n que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques fÃ­sicos.\n-Puede maldecir con PerdiciÃ³n.</color>\n<color=#EE0000>-Pierde parte de su inmunidad fÃ­sica momentÃ¡neamente al atacar.</color>":
                r = "<i>The Forest Specter is a restless soul trapped among the ashes of a scorched forest, its rage fueled by the destruction it could not prevent. Wandering and vengeful, it attacks those who dare to cross its charred land.</i>\n\n<color=#199F10>-Immune to physical attacks.\n-Can curse with Perdition.</color>\n<color=#EE0000>-Loses part of its physical immunity momentarily when attacking.</color>";
                break;
            case "Fuego Fatuo":
                r = "Will-o'-the-Wisp";
                break;
            case "<i>Un eco etÃ©reo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guÃ­a a los incautos hacia la perdiciÃ³n, vengando la memoria del bosque caÃ­do.</i>\n\n<color=#199F10>-Resistente a ataques fÃ­sicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>":
                r = "<i>An ethereal echo of the flames that consumed it, dancing among the ashes as a reminder of the disaster. Though it seems harmless, it leads the unwary to their doom, avenging the memory of the fallen forest.</i>\n\n<color=#199F10>-Resistant to physical attacks.\n-Can incarnate into its enemies.</color>\n<color=#EE0000>-Has low health.</color>";
                break;
            case "Treant Espectral":
                r = "Spectral Treant";
                break;
            case "<i>Con su madera marcada y deformada por el fuego, estos antes pastores de Ã¡rboles ahora deambulan trayendo muerte a los invasores de su hogar.</i>\n\n<color=#199F10>-Buena armadura que se regenera.\n-Puede enredar al golpear a sus enemigos.</color>\n<color=#EE0000>-DÃ©bil al fuego.</color>":
                r = "<i>With its wood marked and deformed by fire, these once shepherds of trees now wander, bringing death to the invaders of their home.</i>\n\n<color=#199F10>-Good armor that regenerates.\n-Can entangle when hitting its enemies.</color>\n<color=#EE0000>-Weak to fire.</color>";
                break;
            case "ManifestaciÃ³n Arcana":
                r = "Arcane Manifestation";
                break;
            case "<i>ConstituÃ­do por pura energÃ­a arcana, este ente etÃ©reo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques fÃ­sicos.</color>":
                r = "<i>Composed of pure arcane energy, this ethereal entity defends the Channeler that shaped it.</i>\n\n<color=#199F10>-Resistant to physical attacks.</color>";
                break;
            case "Vagranilo":
                r = "Vagranile";
                break;
            case "<i>Un ser volador cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audiciÃ³n excepcional.</i>\n\n<color=#199F10>-Evasivo.\n-Puede aturdir.\n-Puede atacar a enemigos escondidos.</color>\n<color=#EE0000>-DÃ©bil al daÃ±o Divino.</color>":
                r = "<i>A quasi-human flying being native to the depths, it has no vision but compensates with exceptional hearing.</i>\n\n<color=#199F10>-Evasive.\n-Can stun.\n-Can attack hidden enemies.</color>\n<color=#EE0000>-Weak to Divine damage.</color>";
                break;
            case "Vagranilo Mayor":
                r = "Elder Vagranile";
                break;
            case "<i>Un ser terrible cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audiciÃ³n excepcional.</i>\n\n<color=#199F10>-Chirrido Ensordecedor.\n-Puede atacar a enemigos escondidos.\n-Se cura al morder victimas con Sangre Contaminada.</color>\n<color=#EE0000>-DÃ©bil al daÃ±o Divino.</color>":
                r = "<i>A terrible quasi-human being from the depths, it has no vision but compensates with exceptional hearing.</i>\n\n<color=#199F10>-Deafening Squeal.\n-Can attack hidden enemies.\n-Heals by biting victims with Contaminated Blood.</color>\n<color=#EE0000>-Weak to Divine damage.</color>";
                break;
            case "LadrÃ³n":
                r = "Thief";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situaciÃ³n desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de CrÃ­tico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante dÃ©bil.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Good critical hit capability.\n-Backstabs while hidden.\n-Can poison his weapon.</color>\n<color=#EE0000>-Quite weak.</color>";
                break;
            case "RufiÃ¡n con Ballesta":
                r = "Crossbow Grunt";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situaciÃ³n desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Puede empujar.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Resistant.\n-Can push.</color>";
                break;
            case "RufiÃ¡n con Mazo":
                r = "Mace Grunt";
                break;
            case "<i>Este hombre ya era malvado antes, y ahora la situaciÃ³n desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Se enfurece.</color>\n<color=#EE0000>-Lento para actuar.</color>":
                r = "<i>This man was already evil before, and now the desperate situation has accentuated his cruelty.</i>\n\n<color=#199F10>-Resistant.\n-Devastating blows.\n-Enrages.</color>\n<color=#EE0000>-Slow to act.</color>";
                break;
            case "Perro Adiestrado":
                r = "Tamed Dog";
                break;
            case "<i>Un perro adiestrado para la batalla, fiel a su amo y feroz con sus enemigos.</i>\n\n<color=#199F10>-Puede Inmovilizar al morder.</color>\n<color=#EE0000>-Relativamente dÃ©bil.</color>":
                r = "<i>A dog trained for battle, loyal to its master and fierce with its enemies.</i>\n\n<color=#199F10>-Can immobilize when biting.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Devorador Corrompido":
                r = "Corrupted Devourer";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Puede debilitar.\n-Absorbe vida de Personajes Corruptos.</color>\n<color=#EE0000>-Relativamente dÃ©bil.</color>":
                r = "<i>Once an inhabitant of the lands, now corrupted by the Black Breath, deformed and hungry.</i>\n\n<color=#A020F0>-Corrupted.</color>\n<color=#199F10>-Can weaken.\n-Absorbs life from Corrupted Characters.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Guerrero Corrompido":
                r = "Corrupted Warrior";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sÃ³lo un tipo de ataque.</color>":
                r = "<i>Once an inhabitant of the lands, now corrupted by the Black Breath, deformed and hungry.</i>\n\n<color=#A020F0>-Corrupted.</color>\n<color=#199F10>-Strong.\n-Strikes in area.</color>\n<color=#EE0000>-Has only one type of attack.</color>";
                break;
            case "AlimaÃ±a Corrompida":
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
            case ": -Almas Danzantes: +5 Esperanza, -100% chances de Emboscada.":
                r = ": -Dancing Souls: +5 Hope, -100% chance of Ambush.";
                break;
            case "-Las Almas Danzantes guÃ­an a la caravana. +5 Esperanza":
                r = "-The Dancing Souls guide the caravan. +5 Hope";
                break;
            case "-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza":
                r = "-The Northern Lights amaze the entire caravan. +10 Hope";
                break;
            case ": -Aurora Boreal: +10 Esperanza.":
                r = ": -Northern Lights: +10 Hope.";
                break;
            case "CanÃ­bal Kale'Tav":
                r = "Kale'Tav Cannibal";
                break;
            case "Garra CanÃ­bal":
                r = "Cannibal Claw";
                break;
            case "Tentado por Sangre":
                r = "Bloodlusted";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Empieza combate con EvasiÃ³n.\n-Se potencia si el enemigo estÃ¡ lastimado.</color>\n<color=#EE0000>-Una vez que perdiÃ³ la evasiÃ³n, es fÃ¡cil de eliminar.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Starts combat with Evasion.\n-Empowered if the enemy is wounded.</color>\n<color=#EE0000>-Once it loses evasion, it is easy to eliminate.</color>";
                break;
            case "Ataque Lanza":
                r = "Spear Attack";
                break;
            case "Arrojar Lanza":
                r = "Throw Spear";
                break;
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Ataque de lanza arrojadiza peligroso.</color>\n<color=#EE0000>-Poca PrecisiÃ³n.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Dangerous thrown spear attack.</color>\n\n<color=#199F10>-Creates traps.</color>\n<color=#EE0000>-Low Accuracy.</color>";
                break;
            case "Improvisar Trampas":
                r = "Improvise Traps";
                break;
            case "Trampa Improvisada: DaÃ±a y marca a unidades que la pisen.":
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
            case "<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Recibir Herida lo potencia.\n-Al matar a un enemigo se potencia.</color>\n<color=#EE0000>-Posee sÃ³lo un tipo de ataque.</color>":
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
            case "Golpe BastÃ³n":
                r = "Staff Strike";
                break;
            case "Ataque de Cuervo":
                r = "Raven Attack";
                break;
            case "<i><i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Tribe originating from the Frozenwind Passage, these wild humanoids are feared for their ferocity and pagan rituals.</i>\n\n<color=#199F10>-Empowers Allies.\n-Its raven defends her.</color>\n<color=#EE0000>-Weak resistance.</color>";
                break;
            case "FrenesÃ­ del Asesinato":
                r = "Murderous Frenzy";
                break;
            case "Derribado":
                r = "Knocked Down";
                break;
            case "<i>Una criatura feroz nativa de la tundra. Es uno de los depredadores mÃ¡s temidos de la regiÃ³n y fuente de varias leyendas entre los Kale'Tav</i>\n\n<color=#199F10>-RegeneraciÃ³n leve.\n-Ataque de embestida en fila.</color>\n<color=#EE0000>-Suelen aparecer sÃ³los o con una pareja como mucho.</color>":
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
            case "PÃ¡jaro Rompe-Hielos":
                r = "Icebreaker Bird";
                break;
            case "Volador: Esta unidad no puede ser alcanzada por ataques melee, puede perder el vuelo al ser daÃ±ado o fallar un ataque.":
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
            case "<i>Este pÃ¡jaro es muy territorial y ataca en grupo, su pico estÃ¡ hecho para romper el hielo grueso y poder pescar peces de gran tamaÃ±o, por lo tanto es muy peligroso.</i>\n\n<color=#199F10>-Vuela.\n-Su ataque baja defensa</color>\n<color=#EE0000>-Una vez que pierde su vuelo, es vulnerable.</color>":
                r = "<i>This bird is very territorial and attacks in groups, its beak is made to break thick ice and fish, therefore it is very dangerous.</i>\n\n<color=#199F10>-Flies.\n-Its attack lowers defense</color>\n<color=#EE0000>-Once it loses its flight, it is vulnerable.</color>";
                break;
            case "Efigie Animada":
                r = "Animated Effigy";
                break;
            case "ReacciÃ³n: Al morir condena al enemigo que diÃ³ el Ãºltimo golpe.":
                r = "REACTION: Upon death, it condemns the enemy that dealt the final blow.";
                break;
            case " es condenado por 3 turnos.":
                r = " is condemned for 3 turns.";
                break;
            case "Condena: En X cantidad de turnos recibirÃ¡ daÃ±o verdadero igual al 10% de su vida mÃ¡xima por turno con el efecto.":
                r = "Condemned: In X turns, it will receive true damage equal to 10% of its maximum health per turn with the effect.";
                break;
            case " es daÃ±ado por la Condena.":
                r = " is damaged by Condemnation.";
                break;
            case "Corte Hoz":
                r = "Sickle Cut";
                break;
            case "<i>Armadas por la magia oscura de los Kale'Tav, estas efigies estÃ¡n por todo su territorio como primer linea de defensa en contra de quienes se atrevan a cruzar el Paso.</i>\n\n<color=#199F10>-Al ser destruÃ­da condena a su atacante.\n-Provoca sangrado.</color>\n<color=#EE0000>-DÃ©biles.</color>":
                r = "<i>Raised with the dark magic of the Kale'Tav, these effigies are spread throughout their territory as the first line of defense against those who dare to cross the Pass.</i>\n\n<color=#199F10>-Upon destruction, it condemns its attacker.\n-Causes bleeding.</color>\n<color=#EE0000>-Weak.</color>";
                break;
            case "Levantar Martillo":
                r = "Raise Hammer";
                break;
            case "Martillo Listo":
                r = "Hammer Ready";
                break;
            case "Gulek Gul pierde el buff 'Martillo Listo' tras recibir daÃ±o y no podrÃ¡ utilizarlo.":
                r = "Gulek Gul loses the 'Hammer Ready' buff after taking damage and will not be able to use it.";
                break;
            case "Martillo PequeÃ±o":
                r = "Small Hammer";
                break;
            case "Martillo Grande":
                r = "Great Hammer";
                break;
            case "<i>Gulek-Gul es un Ettin muy venerado por los Kale'Tav. No habita con ellos, pero cuando se encuentran intrusos en la zona, baja de su colina decidido a proteger su territorio.</i>\n\n<color=#199F10>-Fuerza descomunal.\n-Golpea en zona.\n-Doble intento en tiradas de voluntad.</color>\n<color=#EE0000>-Necesita levantar el martillo grande antes de usarlo.\n-Si recibe daÃ±o o falla tirada de voluntad, deja caer el martillo.</color>":
                r = "<i>Gulek-Gul is a Ettin highly revered by the Kale'Tav. He does not dwell with them, but when intruders are found in the area, he descends from his hill determined to protect his territory.</i>\n\n<color=#199F10>-Immense strength.\n-Hits in area.\n-Double attempt on will rolls.</color>\n<color=#EE0000>-Needs to lift the great hammer before using it.\n-If he takes damage or fails a will roll, he drops the hammer.</color>";
                break;
            case "Discutir TÃ¡cticas":
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
            case "Gulek y Gul discuten tÃ¡cticas, y resuelven adoptar un enfoque defensivo.":
                r = "Gulek and Gul discuss tactics and decide to adopt a defensive approach.";
                break;
            case "Gulek y Gul discuten tÃ¡cticas, y resuelven adoptar un enfoque ofensivo.":
                r = "Gulek and Gul discuss tactics and decide to adopt an offensive approach.";
                break;
            case "Gulek y Gul discuten tÃ¡cticas, y resuelven descansar para recuperar fuerzas.":
                r = "Gulek and Gul discuss tactics and decide to rest to regain strength.";
                break;
            case "Escudado: 10% chances por stack de evitar un ataque fÃ­sico. Al evitar uno, pierde un stack.":
                r = "Shielded: 10% chance per stack to avoid a physical attack. Upon avoiding one, it loses a stack.";
                break;
            case " bloquea el daÃ±o con su escudo.":
                r = " blocks damage with its shield.";
                break;
            case "Bloqueado":
                r = "Blocked";
                break;
            case "Golpe Mazo":
                r = "Mace Strike";
                break;
            case "<i>OrganizaciÃ³n de mercenarios humanos que eran parte del ejÃ©rcito derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Unidad Escudada.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of Lich Kadryn. Now they seek revenge by ensuring that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Shielded Unit.\n-Good Armor.\n-Upon death, leaves a cloud of black breath.</color>\n<color=#EE0000>-Limited movement.</color>";
                break;
            case "Extasiado por Aliento Negro":
                r = "Ecstatic from Black Breath";
                break;
            case "Restos de Aliento: Potencia y cura a los Vengadores de Kadryn.":
                r = "Breath Residue: Empowers and heals the Avengers of Kadryn.";
                break;
            case "ReacciÃ³n: Al morir genera restos de Aliento Negro en el campo de batalla.":
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
            case "<i>OrganizaciÃ³n de mercenarios humanos que eran parte del ejÃ©rcito derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Buen ataque.\n-Flecha envenenada.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
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
            case "<i>OrganizaciÃ³n de mercenarios humanos que eran parte del ejÃ©rcito derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de rango infalible.\n-Potencia Aliados.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of Lich Kadryn. Now they seek revenge by ensuring that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Infallible range attack.\n-Ally Empowerment.\n-Upon death, leaves a cloud of black breath.</color>\n<color=#EE0000>-Poor resistance.</color>";
                break;
            case "OraciÃ³n de Kadryn":
                r = "Kadryn's Prayer";
                break;
            case "Rayo NecrÃ³tico":
                r = "Necrotic Ray";
                break;
            case "Liturgia de la PutrefacciÃ³n":
                r = "Litany of Putrefaction";
                break;
            case "es presa de la PutrefacciÃ³n.":
                r = "falls prey to the Putrefaction.";
                break;
            case "PutrefacciÃ³n":
                r = "Putrefaction";
                break;
            case "El Aliento Negro se expande por el campo enemigo.":
                r = "The Black Breath spreads across the enemy field.";
                break;
            case "desata un rayo necrÃ³tico sobre":
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
            case " posee bonificaciones de daÃ±o y ataque con ataques individuales contra este enemigo.":
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
            case "<i>Raiz-Viva del bosque mismo que ha salido a la superficie obligada por las llamas, ahora atacarÃ¡ furiosa a cualquier invasor del bosque.</i>\n\n<color=#199F10>-Ataque de llamas infalible.\n-Se entierra para curarse.</color>\n<color=#EE0000>-InmÃ³vil.</color>":
                r = "<i>Living-Root of the forest itself that has come to the surface forced by the flames, it will now furiously attack any invader of the forest.</i>\n\n<color=#199F10>-Infallible flame attack.\n-Buries itself to heal.</color>\n<color=#EE0000>-Immobile.</color>";
                break;
            case "Garra Oso Espectral":
                r = "Spectral Bear Claw";
                break;
            case "<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia fÃ­sica es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>":
                r = "<i>This bear has become a fierce specter roaming the burning forest. Its physical power is terrifying.</i>\n\n<color=#199F10>-Overwhelming attacks.\n-Great amount of life.</color>\n<color=#EE0000>-Higher chance of fumble.</color>";
                break;
            case "Bonus de daÃ±o elemental.":
                r = "Bonus elemental damage.";
                break;
            case "<i>Esta bestia oriunda del Paso es material de varias leyendas y pesadillas entre los Kale'Tav. De cuerpo robusto y cuernos afilados, supone un peligro para los viajeros incautos.</i>\n\n<color=#199F10>-Ataques de carga en fila.\n-RegeneraciÃ³n leve.</color>\n<color=#EE0000>-Lento.</color>":
                r = "<i>This beast native to the Passage is the origin of various legends and nightmares among the Kale'Tav. With a robust body and sharp horns, it poses a danger to unwary travelers.</i>\n\n<color=#199F10>-Line charge attacks.\n-Slight regeneration.</color>\n<color=#EE0000>-Slow.</color>";
                break;
            case "Milicianos disponibles: ":
                r = "Available Militiamen: ";
                break;
            case "<i>OrganizaciÃ³n de mercenarios humanos que eran parte del ejÃ©rcito derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de oportunidad.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organization of human mercenaries who were part of the defeated army of the Lich Kadryn. Now they seek revenge by trying to ensure that no one escapes the Black Breath of their master.</i>\n\n<color=#199F10>-Opportunity attack.\n-Good Armor.\n-When they die, they leave a cloud of black breath.</color>\n<color=#EE0000>-Limited movement.</color>";
                break;
            case "Refuerzos":
                r = "Reinforcements";
                break;
            case "Refuerzos aliados disponibles, irÃ¡n uniÃ©ndose a la batalla gradualmente.":
                r = "Allied reinforcements available, will gradually join the battle.";
                break;
            case "Refuerzos enemigos disponibles, irÃ¡n uniÃ©ndose a la batalla gradualmente.":
                r = "Enemy reinforcements available, will gradually join the battle.";
                break;
            case "El Bosque Ardiente":
                r = "The Burning Forest";
                break;
            case "Paso Vientohelado":
                r = "Frozenwind Passage";
                break;
            case "A medida que viajas por el bosque, las llamas envolverÃ¡n regiones del mapa de forma inesperada.\n\nSi intentas atravesar un Nodo prendido fuego, perderÃ¡s 10 de Esperanza y 8-15 Civiles.\nNo se podrÃ¡ descansar en nodos incendiados.\n\nAdemÃ¡s, las batallas que tengan lugar en un Nodo incendiado, tendrÃ¡n llamas en el campo de batalla.":
                r = "As you travel through the forest, flames will engulf regions of the map unexpectedly.\n\nIf you try to cross a node on fire, you will lose 10 Hope and 8-15 Civilians.\nYou will not be able to rest in burning nodes.\n\nAdditionally, battles taking place in a burning Node will have flames on the battlefield.";
                break;
            case "<color=#FF3D00>-El incendio ha envuelto un nodo cercano al camino de la caravana.</color>":
                r = "<color=#FF3D00>-The fire has engulfed a node near the caravan's path.</color>";
                break;
            case "\n<color=#FF3D00>--Incendiado--</color>":
                r = "\n<color=#FF3D00>--Burning--</color>";
                break;
            case "La lluvia ha apagado los incendios en el Ã¡rea temporalmente.":
                r = "The rain has temporarily extinguished the fires in the area.";
                break;
            case "Llamas: infligen daÃ±o fuego a unidades que entren en la casilla.":
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
            case "La tribu Kale'Tav estÃ¡ realizando rituales en el Ã¡rea, preparÃ¡ndose para el Aliento Negro.\n\nAl escuchar sus tambores a lo lejos sabrÃ¡s dÃ³nde se encuentran.\nPor cada Ritual completado, sus combatientes recibirÃ¡n bonificaciones en batalla.\n\nPara interrumpir un ritual debes aproximarte a los nodos marcados y derrotarlos.\n\nFuerza Kale'Tav: ":
                r = "The Kale'Tav tribe is performing rituals in the area, preparing for the Black Breath.\n\nHearing their drums in the distance will let you know where they are.\nFor each completed Ritual, their fighters will receive bonuses in battle.\n\nTo interrupt a ritual you must approach the marked nodes and defeat them.\n\nKale'Tav Strength: ";
                break;
            case "<color=#6A0DAD>-Un ritual Kale'Tav ha comenzado en un nodo cercano. La mÃºsica profana desalienta a la caravana. -5 Esperanza.</color>":
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
            case "Ir al Manual de CampaÃ±a":
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
            case "DaÃ±os":
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
            case "Volumen de la MÃºsica":
                r = "Music Volume";
                break;
            case "Reproducir mÃºsica al minimizar":
                r = "Play music when minimized";
                break;
            case "Idioma":
                r = "Language";
                break;
            case "InglÃ©s":
                r = "English";
                break;
            case "EspaÃ±ol":
                r = "Spanish";
                break;
            case "Idioma del juego":
                r = "Game Language";
                break;
            case "GrÃ¡ficos":
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
            case "ResoluciÃ³n de Pantalla":
                r = "Screen Resolution";
                break;
            case "Calidad GrÃ¡ficos":
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
            case "Accesos RÃ¡pidos":
                r = "Hotkeys";
                break;
            case "Dificultad":
                r = "Difficulty";
                break;
            case "-----Combate-----":
                r = "-----Combat-----";
                break;
            case "Modo RÃ¡pido":
                r = "Fast Mode";
                break;
            case "Debido a la invasiÃ³n, Nedukazal estÃ¡ envuelta en caos y oscuridad, por lo tanto la caravana no podrÃ¡ ver claramente el camino adelante.\n\nAl depender de la luz propia, serÃ¡ mas propensa a sufrir emboscadas (+20%).\n\nMejora las <b>Antorchas de Pie</b> para aumentar el rango de visiÃ³n.\n\nEl Aliento Negro no serÃ¡ una preocupaciÃ³n en esta zona.":
                r = "Due to the invasion, Nedukazal is shrouded in chaos and darkness, so the caravan will not be able to clearly see the path ahead.\n\nRelying on its own light, it will be more prone to ambushes (+20%).\n\nUpgrade the <b>Standing Torches</b> to increase the vision range.\n\nThe Black Breath will not be a concern in this area.";
                break;
            case "\nSe conseguirÃ¡n de 25-40 Materiales y 60-85 Suministros.":
                r = "\n25-40 Materials and 60-85 Supplies will be gathered.";
                break;
            case ": -Nedukazal estÃ¡ a oscuras.":
                r = ": -Nedukazal is in darkness.";
                break;
            case ": -Masacre: Nedukazal estÃ¡ siendo atacada. -10 Esperanza. +10% Emboscada. Los ZÃ¢rkil estÃ¡n potenciados.":
                r = ": -Massacre: Nedukazal is being attacked. -10 Hope. +10% Ambush. The ZÃ¢rkil are empowered.";
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
            case "<i>Raza de criaturas demonÃ­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverÃ¡n.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sÃ³lo un tipo de ataque.</color>":
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
            case "Mirada de Masacre: al moverse aquÃ­, Tirada de salvaciÃ³n mental CD 13 o se pierde el turno.":
                r = "Gaze of the Massacre: when moving here, Mental saving throw DC 13 or lose the turn.";
                break;
            case "<i>Raza de criaturas demonÃ­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sÃ³lo un tipo de ataque.</color>":
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
            case "<i>Raza de criaturas demonÃ­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que ademÃ¡s motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Stunning shout that also motivates allies.\n-Can see hidden units.\n-Can attack repeatedly.</color>\n<color=#EE0000></color>";
                break;
            case "Rayo Debilitador":
                r = "Weakening Ray";
                break;
            case "Debilitado":
                r = "Weakened";
                break;
            case "<i>Raza de criaturas demonÃ­acas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-DÃ©bil</color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Unerring weakening attack.\n-Can see hidden units.\n-Flying.</color>\n<color=#EE0000>-Weak</color>";
                break;
            case "Zarkil Alado":
                r = "Winged Zarkil";
                break;
            case "Mordisco Zarkilever":
                r = "Zarkilever Bite";
                break;
            case " sufre el efecto de Saboreado, recibiendo 15 de daÃ±o extra y curando al Zarkilever por 14 puntos de vida.":
                r = " suffers the Savored effect, receiving 15 extra damage and healing the Zarkilever for 14 health points.";
                break;
            case "Saborear":
                r = "Taste";
                break;
            case "Saboreando!":
                r = "Tasting!";
                break;
            case "<i>Criatura muy feroz controlada porlos Zarkils utilizada como fuerza de impacto y para causar grietas en superficies duras. </color>\n\n<color=#199F10>-Buena Armadura.\n-Saborea a las vÃ­ctimas.</color>\n<color=#EE0000></color>":
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
            case "<i>Tiene una legiÃ³n entera de Zarkils bajo su liderazgo, simplemente debe seÃ±alar un objetivo y sus sÃºbditos se encargarÃ¡n del resto.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque debilitador infalible.</color>\n<color=#EE0000>-No es fuerte por si solo.</color>":
                r = "<i>He has an entire legion of Zarkils under his leadership, he just has to point to a target and his subjects will take care of the rest.</color>\n\n<color=#199F10>-Calls endless reinforcements.\n-Unerring weakening attack.</color>\n<color=#EE0000>-Not strong on his own.</color>";
                break;
            case "Llamada Espectral":
                r = "Spectral Call";
                break;
            case "Condena del bosque":
                r = "Forest's Curse";
                break;
            case "<i>ManifestaciÃ³n de la energÃ­a espectral del bosque. Desde su interior emana un fulgor fantasmal frÃ­o, como un espÃ­ritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrÃ³tico que condena a dos objetivos.</color>\n<color=#EE0000>-InmÃ³vil.</color>":
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
            case "<i>Estas criaturas aladas habitan en las regiones mÃ¡s frÃ­as del Paso. Son conocidas por ser muy territoriales y por su aliento gÃ©lido.</i>\n\n<color=#199F10>-Vuelo.\n-Aliento gÃ©lido en zona.\n-Regenera armadura.</color>\n<color=#EE0000>-DÃ©bil al fuego.</color>":
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
            case "No hay suficientes energÃ­a":
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
            case "DAÃ‘O":
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
            case "CrÃ­tico":
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
            case "<color=#FF6666>No puedes descansar aquÃ­.</color>":
                r = "<color=#FF6666>You can't rest here.</color>";
                break;
            case "Actualmente encarnado en un enemigo, invulnerable.":
                r = "Currently incarnated in an enemy, invulnerable.";
                break;
            case "Encarnado en Enemigo":
                r = "Incarnated in Enemy";
                break;
            case "AdelÃ¡ntate para usarla":
                r = "Melee: Move ahead to use it";
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
            case "Â¡Comienza la batalla!":
                r = "The battle begins!";
                break;
            case "Â¡Viaje completado!":
                r = "Travel completed!";
                break;
            case "Finalmente la caravana ha llegado a la Ciudad Puerto de Serria, donde la poblaciÃ³n civil se prepara para embarcar y asÃ­ escapar del Aliento Negro.":
                r = "At last, the caravan has arrived at the Port City of Serria, where the civilian population is preparing to embark and escape the Black Breath.";
                break;
            case "El viaje ha durado ":
                r = "This trip lasted ";
                break;
            case " dÃ­as enteros y han sobrevivido ":
                r = " days and survived ";
                break;
            case "civiles.\n\n":
                r = " civilians.\n\n";
                break;
            case "AdemÃ¡s, el oro restante (":
                r = "Additionally, the remaining gold (";
                break;
            case ") se ha donado a las arcas de la ciudad para ayudar a financiar la evacuaciÃ³n.\n\nLos Personajes sobrevivientes tambiÃ©n se han unido al esfuerzo de evacuaciÃ³n para defender la ciudad.\n\n":
                r = ") has been donated to the city treasury to help fund the evacuation.\n\nThe surviving Characters have also joined the evacuation effort to defend the city.\n";
                break;
            case "<b>Valor de Trabajo obtenido: ":
                r = "<b>Work Value obtained: ";
                break;
            case "Valor de Trabajo Disponible:":
                r = "Available Work Value:";
                break;
            case "Valor de CorrupciÃ³n actual:":
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
            case " caravanas mÃ¡s antes de tener que zarpar.":
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
            case "MisiÃ³n de Salvamento":
                r = "Rescue Mission";
                break;
            case "El ave mensajera regresa con un mensaje atado a sus patas. En Ã©l se indica el punto exacto al que la caravana deberÃ¡ dirigirse para encontrarse con el equipo de salvamento, junto con los recursos cedidos por la ciudad de Serria.\n":
                r = "The messenger bird returns with a message tied to its legs. It indicates the exact point where the caravan must go to meet the rescue team, along with the resources provided by the city of Serria.\n";
                break;
            case "UbicaciÃ³n de la MisiÃ³n de Salvamento":
                r = "Rescue Mission Location";
                break;
            case "<color=#a0e812><b>\n\nSe ha marcado en el camino adelante el nodo al cual deberÃ­as dirigirte para encontrarte con el equipo de salvamento.</b></color>":
                r = "<color=#a0e812><b>\n\nThe node ahead where you should head to meet the rescue team has been marked on the path.</b></color>";
                break;
            case "Un encuentro esperado":
                r = "An Expected Encounter";
                break;
            case "Has llegado al lugar seÃ±alado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traÃ­do para ayudarles en su travesÃ­a.\n\nInmediatamente los Ã¡nimos mejoran en la caravana al ver que no estÃ¡n solos en esta lucha.\n":
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
            case "-Las almenaras de Serria se divisan a lo lejos sobre las montaÃ±as, brillando con fuerza y marcando el destino de la caravana: ":
                r = "-The beacons of Serria can be seen in the distance over the mountains, shining brightly and marking the caravan's destination: ";
                break;
            case "<b>Una patrulla de milicianos de Serria se une a la batalla como refuerzos.</b>":
                r = "<b>A patrol of militiamen from Serria joins the battle as reinforcements.</b>";
                break;
            case "<b>Arbol Vengativo</b>":
                r = "<b>Cursed Tree</b>";
                break;
            case "Puntos de AcciÃ³n":
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
            case " obtiene un intento adicional de Tirada de SalvaciÃ³n.":
                r = " gets an additional attempt at a Saving Throw.";
                break;
            case "Gulek y Gul discuten tÃ¡cticas, y resuelven adoptar un enfoque agresivo.":
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
            case ": -Lluvia: -5 Esperanza. -15% RecolecciÃ³n Suministros, -20% chances de Emboscada.":
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
            case "<color=#2a9c71>\n\nFatigado: -1 Atributos hasa prÃ³ximo descanso. </color>":
                r = "<color=#2a9c71>\n\nTired: -1 Attributes until next rest. </color>";
                break;
            case "Torpe: +1 Rango Pifias":
                r = "Clumsy: +1 Range Miss.";
                break;
              case "PociÃ³n de CuraciÃ³n Media":
                r = "Medium Healing Potion";
                break;
            case "objetivo":
                r = "target";
                break;
            case "Impacto crÃ­tico":
                r = "Critical Hit";
                break;
            case " usa ":
                r = " uses ";
                break;
            case "Bonus daÃ±o elemental Acido.":
                r = "Acid Elemental Damage Bonus.";
                break;
              case "Bonus daÃ±o elemental Arcano.":
                r = "Arcane Elemental Damage Bonus.";
                break;
            case "Bonus daÃ±o elemental Fuego.":
                r = "Fire Elemental Damage Bonus.";
                break;
            case "Bonus daÃ±o elemental Hielo.":
                r = "Ice Elemental Damage Bonus.";
                break;
            case "Bonus daÃ±o elemental Necro.":
                r = "Necrotic Elemental Damage Bonus.";
                break;
            case "Bonus daÃ±o elemental Rayo.":
                r = "Lightning Elemental Damage Bonus.";
                break;
            case "Bonus daÃ±o elemental Divino.":
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
            case "Acumular Energ\u00eda":
                r = "Accumulate Energy";
                break;
            case "Acumulaci\u00f3n Inestable":
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
            case "Hoja de EnergÃ­a":
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
            case "Purificaci\u00f3n":
                r = "Purification";
                break;
            case "R\u00e1faga":
                r = "Barrage";
                break;
            case "Residuo Energetico":
                r = "Energy Residue";
                break;
            case "Salmo Purificador":
                r = "Purifying Psalm";
                break;
            case "Sigues T\u00FA":
                r = "You Are Next";
                break;
            case "Sigues Tu":
                r = "You Are Next";
                break;
            case "Sif\u00f3n Arcano":
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
            case "DeterminaciÃ³n ":
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
            case "-La Caravana ha sido emboscada por un ataque subterrÃ¡neo.":
                r = "-The Caravan has been ambushed by an underground attack.";
                break;
            case "-El SÃ©quito de ClÃ©rigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crÃ­tico. -20 Esperanza":
                r = "-The Clerics' Retinue has perished, as the Black Breath has reached a critical level. -20 Hope";
                break;
            case " ahora Maneja un Nivel ":
                r = " now has Energy Level ";
                break;
            case " de EnergÃ­a.":
                r = ".";
                break;
            case " de ValentÃ­a.":
                r = " Valor.";
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
            /*case "":
                r = "";
                break;
            case "":
                r = "";
                break;
             /*case "":
                r = "";
                break;
            case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;
             case "":
                r = "";
                break;*/
                
             



























































        }


        return r;
    }



    public void TraducirTodosTextosIngles()
    {
        var textos = Object.FindObjectsOfType<TMPro.TextMeshProUGUI>(includeInactive: true);
        foreach (var txt in textos)
        {  txt.text = TraducirIngles(txt.text, true);  }
   }




}
