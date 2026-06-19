using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class TRADU : MonoBehaviour
{
    public const int IdiomaEspanol = 1;
    public const int IdiomaIngles = 2;
    public const int IdiomaPortugues = 3;

    public static TRADU i { get; private set; }
    public int nIdioma = IdiomaIngles; //1 Español  -  2 Inglés
    private readonly Dictionary<TMPro.TextMeshProUGUI, string> textosOriginalesTMP = new Dictionary<TMPro.TextMeshProUGUI, string>();

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

        if (resultado == textComponent && !string.IsNullOrEmpty(textComponent))
        {
            string claveSinTildes = RemoverTildesYEnies(textComponent);
            if (claveSinTildes != textComponent)
            {
                string resultadoFallback = traductor(claveSinTildes, false);
                if (resultadoFallback != claveSinTildes)
                {
                    resultado = resultadoFallback;
                }
            }
        }

        return resultado;
    }

    private string RemoverTildesYEnies(string texto)
    {
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder(normalizado.Length);

        foreach (char c in normalizado)
        {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
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

    bool TryTraducirEventosCampaniaUiIngles(string txt, out string traduccion)
    {
        if (TryTraducirEventosCampaniaUiInglesV2(txt, out traduccion))
        {
            return true;
        }

        switch (txt)
        {
            case "Paso Precario":
                traduccion = "Precarious Crossing";
                return true;
            case "Aire Enrarecido":
                traduccion = "Stale Air";
                return true;
            case "Rumor de Desbande":
                traduccion = "Rumor of Desertion";
                return true;
            case "Vado Traicionero":
                traduccion = "Treacherous Ford";
                return true;
            case "Carro Encajado":
                traduccion = "Stuck Wagon";
                return true;
            case "Marcas del Correo":
                traduccion = "Courier Marks";
                return true;
            case "Pulso de Mando":
                traduccion = "Command Presence";
                return true;
            case "Hombros Firmes":
                traduccion = "Steady Shoulders";
                return true;
            case "Mano Cierta":
                traduccion = "Steady Hand";
                return true;
            case "Dos Miradas":
                traduccion = "Two Views";
                return true;
            case "La Caravana llega a un tramo estrecho, quebrado y lleno de tablones flojos. No parece imposible de cruzar, pero sÃ­ lo bastante traicionero como para convertir un descuido en accidente.\n\n":
                traduccion = "The Caravan reaches a narrow, broken stretch full of loose planks. It does not seem impossible to cross, but it is treacherous enough for one mistake to turn into an accident.\n\n";
                return true;
            case "</color></b> puede intentar guiar el cruce antes de que cunda el pÃ¡nico.\n\n":
                traduccion = "</color></b> can try to guide the crossing before panic spreads.\n\n";
                return true;
            case "<color=#ba3fef>-Tirada de SalvaciÃ³n: TS Reflejos DC ":
                traduccion = "<color=#ba3fef>-Saving Throw: Reflex DC ";
                return true;
            case " <i>(TS Reflejos actual: ":
                traduccion = " <i>(Current Reflex Save: ";
                return true;
            case ").</i> Si la supera, ganarÃ¡ 35 Experiencia. Si falla, obtendrÃ¡ Herida.</color>\n\n":
                traduccion = ").</i> On success, they gain 35 Experience. On failure, they suffer an Injury.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede intentar guiar el cruce antes de que cunda el pÃ¡nico.\n\n":
                traduccion = "One of the Heroes can try to guide the crossing before panic spreads.\n\n";
                return true;
            case "<color=#ba3fef>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Reflejos DC 11. Si la supera, ganarÃ¡ 35 Experiencia. Si falla, obtendrÃ¡ Herida.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you try it, they will make a Reflex Saving Throw, DC 11. On success, they gain 35 Experience. On failure, they suffer an Injury.</color>\n\n";
                return true;
            case "<color=#ba3fef>-Si decides rodear el tramo, la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you choose to go around the stretch, the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "Desde una bodega medio tapada llegan golpes apagados y pedidos de ayuda. El aire que sale por la entrada estÃ¡ cargado de polvo viejo, moho y algo que raspa la garganta apenas uno se acerca.\n\n":
                traduccion = "Muffled knocks and cries for help come from a half-covered cellar. The air pouring out is heavy with old dust, mold, and something that scratches at the throat the moment anyone draws near.\n\n";
                return true;
            case "</color></b> puede intentar entrar y sacar a quienes sigan con vida antes de que colapse el lugar.\n\n":
                traduccion = "</color></b> can try to get inside and bring out anyone still alive before the place collapses.\n\n";
                return true;
            case "<color=#ba3fef>-Tirada de SalvaciÃ³n: TS Fortaleza DC ":
                traduccion = "<color=#ba3fef>-Saving Throw: Fortitude DC ";
                return true;
            case " <i>(TS Fortaleza actual: ":
                traduccion = " <i>(Current Fortitude Save: ";
                return true;
            case ").</i> Si la supera, rescatarÃ¡ 6-10 Civiles y ganarÃ¡ 30 Experiencia. Si falla, obtendrÃ¡ Enfermo por 3 dÃ­as.</color>\n\n":
                traduccion = ").</i> On success, they rescue 6-10 Civilians and gain 30 Experience. On failure, they become Sick for 3 days.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede intentar entrar y sacar a quienes sigan con vida antes de que colapse el lugar.\n\n":
                traduccion = "One of the Heroes can try to get inside and bring out anyone still alive before the place collapses.\n\n";
                return true;
            case "<color=#ba3fef>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Fortaleza DC 10. Si la supera, rescatarÃ¡ 6-10 Civiles y ganarÃ¡ 30 Experiencia. Si falla, obtendrÃ¡ Enfermo por 3 dÃ­as.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you try it, they will make a Fortitude Saving Throw, DC 10. On success, they rescue 6-10 Civilians and gain 30 Experience. On failure, they become Sick for 3 days.</color>\n\n";
                return true;
            case "<color=#ba3fef>-Si decides sellar la entrada y seguir, -12 Esperanza.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you choose to seal the entrance and move on, -12 Hope.</color>\n\n";
                return true;
            case "Una versiÃ³n exagerada de un peligro cercano se esparce de carro en carro y empieza a levantar un pÃ¡nico innecesario. En pocos minutos, varios Civiles ya hablan de abandonar la marcha antes de quedar atrapados.\n\n":
                traduccion = "An exaggerated version of a nearby danger spreads from wagon to wagon and starts stirring needless panic. Within minutes, several Civilians are already talking about abandoning the march before they get trapped.\n\n";
                return true;
            case "</color></b> puede intentar frenarlo con calma antes de que empeore.\n\n":
                traduccion = "</color></b> can try to calm things down before it gets worse.\n\n";
                return true;
            case "<color=#ba3fef>-Tirada de SalvaciÃ³n: TS Mental DC ":
                traduccion = "<color=#ba3fef>-Saving Throw: Mental DC ";
                return true;
            case " <i>(TS Mental actual: ":
                traduccion = " <i>(Current Mental Save: ";
                return true;
            case ").</i> Si la supera, ganarÃ¡ 35 Experiencia y +4 Esperanza. Si falla, obtendrÃ¡ Baja Moral por 3 dÃ­as y la Caravana perderÃ¡ 5 Esperanza.</color>\n\n":
                traduccion = ").</i> On success, they gain 35 Experience and +4 Hope. On failure, they suffer Low Morale for 3 days and the Caravan loses 5 Hope.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede intentar frenar el rumor con calma antes de que empeore.\n\n":
                traduccion = "One of the Heroes can try to calm the rumor before it gets worse.\n\n";
                return true;
            case "<color=#ba3fef>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Mental DC 12. Si la supera, ganarÃ¡ 35 Experiencia y +4 Esperanza. Si falla, obtendrÃ¡ Baja Moral por 3 dÃ­as y la Caravana perderÃ¡ 5 Esperanza.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you try it, they will make a Mental Saving Throw, DC 12. On success, they gain 35 Experience and +4 Hope. On failure, they suffer Low Morale for 3 days and the Caravan loses 5 Hope.</color>\n\n";
                return true;
            case "<color=#ba3fef>-Si decides imponer silencio por la fuerza, -9 Esperanza.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you choose to impose silence by force, -9 Hope.</color>\n\n";
                return true;
            case "La corriente parece mansa desde lejos, pero apenas los primeros carros tocan el vado queda claro que el fondo es resbaladizo y el agua tira con mÃ¡s fuerza de la esperada.\n\n":
                traduccion = "The current looks gentle from afar, but as soon as the first wagons touch the ford it becomes clear the bottom is slippery and the water pulls harder than expected.\n\n";
                return true;
            case "Alguien tendrÃ¡ que adelantarse para ordenar el cruce de los bueyes y evitar que todo se desarme en medio del paso.\n\n":
                traduccion = "Someone will have to move ahead to organize the oxen crossing and keep everything from falling apart in the middle of the ford.\n\n";
                return true;
            case "</color></b>: TS Reflejos DC ":
                traduccion = "</color></b>: Reflex Save DC ";
                return true;
            case ").</i> Si supera la tirada, ganarÃ¡ 40 Experiencia. Si falla, obtendrÃ¡ Herida y la Caravana perderÃ¡ 1 Buey.</color>\n\n":
                traduccion = ").</i> On success, they gain 40 Experience. On failure, they suffer an Injury and the Caravan loses 1 Ox.</color>\n\n";
                return true;
            case "<color=#ba3fef>-Si decides no arriesgar el cruce, el rodeo harÃ¡ avanzar al Aliento Negro.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you choose not to risk the crossing, the detour will advance the Black Breath.</color>\n\n";
                return true;
            case "Uno de los carros queda mal encajado entre piedras y barro duro. Si no lo sacan pronto, la marcha se trabarÃ¡ alrededor suyo y el malhumor empezarÃ¡ a crecer.\n\n":
                traduccion = "One of the wagons gets badly wedged between stones and hard mud. If they do not free it quickly, the whole march will jam around it and tempers will start to rise.\n\n";
                return true;
            case "Hace falta fuerza y aguante para moverlo sin terminar lastimado.\n\n":
                traduccion = "It will take strength and endurance to move it without getting hurt.\n\n";
                return true;
            case "</color></b>: TS Fortaleza DC ":
                traduccion = "</color></b>: Fortitude Save DC ";
                return true;
            case ").</i> Si supera la tirada, ganarÃ¡ 35 Experiencia y +3 Esperanza. Si falla, obtendrÃ¡ Herida y la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = ").</i> On success, they gain 35 Experience and +3 Hope. On failure, they suffer an Injury and the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "<color=#ba3fef>-Si decides descargar el carro y seguir, +1 Fatiga.</color>\n\n":
                traduccion = "<color=#ba3fef>-If you choose to unload the wagon and move on, +1 Fatigue.</color>\n\n";
                return true;
            case "En un poste vencido y en varias piedras cercanas aparecen marcas antiguas de correo, casi borradas por el tiempo. TodavÃ­a parece posible sacar algo Ãºtil de ese cÃ³digo si alguien sabe leerlo bien.\n\n":
                traduccion = "Ancient courier marks appear on a toppled post and several nearby stones, nearly erased by time. It still seems possible to get something useful out of that code if someone can read it properly.\n\n";
                return true;
            case "</color></b> puede intentar interpretarlas antes de que se pierda la luz.\n\n":
                traduccion = "</color></b> can try to interpret them before the light fades.\n\n";
                return true;
            case "<color=#a0e812>-Tirada de SalvaciÃ³n: TS Mental DC ":
                traduccion = "<color=#a0e812>-Saving Throw: Mental DC ";
                return true;
            case ").</i> Si la supera, se revelarÃ¡n nodos cercanos y ganarÃ¡ 30 Experiencia. Si falla, la demora harÃ¡ que la Caravana gane +1 Fatiga.</color>\n\n":
                traduccion = ").</i> On success, nearby nodes will be revealed and they gain 30 Experience. On failure, the delay causes the Caravan to gain +1 Fatigue.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede intentar interpretarlas antes de que se pierda la luz.\n\n":
                traduccion = "One of the Heroes can try to interpret them before the light fades.\n\n";
                return true;
            case "<color=#a0e812>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Mental DC 10. Si la supera, se revelarÃ¡n nodos cercanos y ganarÃ¡ 30 Experiencia. Si falla, la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = "<color=#a0e812>-If you try it, they will make a Mental Saving Throw, DC 10. On success, nearby nodes will be revealed and they gain 30 Experience. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "<color=#a0e812>-Si decides seguir sin detenerte, +3 Esperanza.</color>\n\n":
                traduccion = "<color=#a0e812>-If you choose to keep moving without stopping, +3 Hope.</color>\n\n";
                return true;
            case "Un embotellamiento de carros, Civiles y animales corta el ritmo de la marcha. TodavÃ­a no es grave, pero si nadie ordena la fila con autoridad la confusiÃ³n puede extenderse bastante.\n\n":
                traduccion = "A bottleneck of wagons, Civilians, and animals breaks the march's rhythm. It is not serious yet, but if no one orders the line with authority the confusion may spread quickly.\n\n";
                return true;
            case "</color></b>: TS Mental DC ":
                traduccion = "</color></b>: Mental Save DC ";
                return true;
            case ").</i> Si supera la tirada, ganarÃ¡ 30 Experiencia y la Caravana obtendrÃ¡ +6 Esperanza. Si falla, la Caravana perderÃ¡ 2 Esperanza.</color>\n\n":
                traduccion = ").</i> On success, they gain 30 Experience and the Caravan gains +6 Hope. On failure, the Caravan loses 2 Hope.</color>\n\n";
                return true;
            case "<color=#a0e812>-Si decides dejar que la fila se acomode sola, +3 Esperanza.</color>\n\n":
                traduccion = "<color=#a0e812>-If you choose to let the line settle on its own, +3 Hope.</color>\n\n";
                return true;
            case "Un Civil agotado se desploma justo cuando la fila empieza a recuperar ritmo. Si nadie lo asiste, el grupo volverÃ¡ a frenarse y crecerÃ¡ el malhumor.\n\n":
                traduccion = "An exhausted Civilian collapses just as the line begins to recover its pace. If no one helps them, the group will slow again and tempers will worsen.\n\n";
                return true;
            case "</color></b> puede cargarlo y sostener el paso hasta la prÃ³xima pausa.\n\n":
                traduccion = "</color></b> can carry them and keep the pace until the next pause.\n\n";
                return true;
            case "<color=#a0e812>-Tirada de SalvaciÃ³n: TS Fortaleza DC ":
                traduccion = "<color=#a0e812>-Saving Throw: Fortitude DC ";
                return true;
            case ").</i> Si la supera, ganarÃ¡ 35 Experiencia y la Caravana obtendrÃ¡ -1 Fatiga. Si falla, la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = ").</i> On success, they gain 35 Experience and the Caravan gets -1 Fatigue. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede cargarlo y sostener el paso hasta la prÃ³xima pausa.\n\n":
                traduccion = "One of the Heroes can carry them and keep the pace until the next pause.\n\n";
                return true;
            case "<color=#a0e812>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Fortaleza DC 12. Si la supera, ganarÃ¡ 35 Experiencia y la Caravana obtendrÃ¡ -1 Fatiga. Si falla, la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = "<color=#a0e812>-If you try it, they will make a Fortitude Saving Throw, DC 12. On success, they gain 35 Experience and the Caravan gets -1 Fatigue. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "<color=#a0e812>-Si decides relevarlo entre varios, +2 Esperanza.</color>\n\n":
                traduccion = "<color=#a0e812>-If you choose to rotate the burden among several people, +2 Hope.</color>\n\n";
                return true;
            case "Una rÃ¡faga arrastra una cartera de viaje con mapas, notas y referencias Ãºtiles justo hasta un borde incÃ³modo de alcanzar. TodavÃ­a puede recuperarse, pero hace falta velocidad y precisiÃ³n.\n\n":
                traduccion = "A gust of wind drags away a travel satchel with maps, notes, and useful references right to an awkward ledge. It can still be recovered, but it will take speed and precision.\n\n";
                return true;
            case "</color></b> puede intentar atraparla antes de que se pierda del todo.\n\n":
                traduccion = "</color></b> can try to catch it before it is lost for good.\n\n";
                return true;
            case "<color=#a0e812>-Tirada de SalvaciÃ³n: TS Reflejos DC ":
                traduccion = "<color=#a0e812>-Saving Throw: Reflex DC ";
                return true;
            case ").</i> Si la supera, se revelarÃ¡n nodos cercanos, ganarÃ¡ 35 Experiencia y la Caravana obtendrÃ¡ +4 Esperanza. Si falla, obtendrÃ¡ Herida.</color>\n\n":
                traduccion = ").</i> On success, nearby nodes will be revealed, they gain 35 Experience, and the Caravan gains +4 Hope. On failure, they suffer an Injury.</color>\n\n";
                return true;
            case "Uno de los HÃ©roes puede intentar atraparla antes de que se pierda del todo.\n\n":
                traduccion = "One of the Heroes can try to catch it before it is lost for good.\n\n";
                return true;
            case "<color=#a0e812>-Si lo intentas, harÃ¡ una Tirada de SalvaciÃ³n: TS Reflejos DC 13. Si la supera, se revelarÃ¡n nodos cercanos, ganarÃ¡ 35 Experiencia y la Caravana obtendrÃ¡ +4 Esperanza. Si falla, obtendrÃ¡ Herida.</color>\n\n":
                traduccion = "<color=#a0e812>-If you try it, they will make a Reflex Saving Throw, DC 13. On success, nearby nodes will be revealed, they gain 35 Experience, and the Caravan gains +4 Hope. On failure, they suffer an Injury.</color>\n\n";
                return true;
            case "<color=#a0e812>-Si decides dejarla ir, -10 Esperanza.</color>\n\n":
                traduccion = "<color=#a0e812>-If you choose to let it go, -10 Hope.</color>\n\n";
                return true;
            case "La ruta se abre en varias direcciones parecidas y las pocas seÃ±ales Ãºtiles parecen haberse cruzado unas con otras. Dos miembros de la caravana parecen tener opiniones encontradas. Â¿A quiÃ©n escucharÃ¡s?\n\n":
                traduccion = "The road opens into several similar directions and the few useful signs seem to contradict one another. Two members of the caravan appear to disagree. Who will you listen to?\n\n";
                return true;
            case ").</i> Si supera la tirada, ganarÃ¡ 25 Experiencia y el Aliento Negro retrocederÃ¡ 1. Si falla, la Caravana ganarÃ¡ +1 Fatiga.</color>\n\n":
                traduccion = ").</i> On success, they gain 25 Experience and the Black Breath is pushed back by 1. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
                return true;
            case "<color=#a0e812>-Si decides mantener la ruta sin arriesgarte, +4 Esperanza.</color>\n\n":
                traduccion = "<color=#a0e812>-If you choose to stay on the route without taking risks, +4 Hope.</color>\n\n";
                return true;
            case "Entrar":
                traduccion = "Enter";
                return true;
            case "Sellarla":
                traduccion = "Seal It";
                return true;
            case "Imponer silencio":
                traduccion = "Impose Silence";
                return true;
            case "Empujarlo":
                traduccion = "Push It";
                return true;
            case "Descargarlo":
                traduccion = "Unload It";
                return true;
            case "Interpretarlas":
                traduccion = "Interpret Them";
                return true;
            case "Cargarlo":
                traduccion = "Carry Them";
                return true;
            case "Relevarlo":
                traduccion = "Relieve Them";
                return true;
            case "Recuperarla":
                traduccion = "Recover It";
                return true;
            case "Dejarla ir":
                traduccion = "Let It Go";
                return true;
            case "Decidir":
                traduccion = "Decide";
                return true;
            case "HÃ©roe 1":
                traduccion = "Hero 1";
                return true;
        }

        traduccion = null;
        return false;
    }

    bool TryTraducirEventosCampaniaUiInglesV2(string txt, out string traduccion)
    {
        if (string.IsNullOrEmpty(txt))
        {
            traduccion = null;
            return false;
        }

        if (txt == "Paso Precario") { traduccion = "Precarious Crossing"; return true; }
        if (txt == "Aire Enrarecido") { traduccion = "Stale Air"; return true; }
        if (txt == "Rumor de Desbande") { traduccion = "Rumor of Desertion"; return true; }
        if (txt == "Vado Traicionero") { traduccion = "Treacherous Ford"; return true; }
        if (txt == "Carro Encajado") { traduccion = "Stuck Wagon"; return true; }
        if (txt == "Marcas del Correo") { traduccion = "Courier Marks"; return true; }
        if (txt == "Pulso de Mando") { traduccion = "Command Presence"; return true; }
        if (txt == "Hombros Firmes") { traduccion = "Steady Shoulders"; return true; }
        if (txt == "Manos Certeras") { traduccion = "Steady Hands"; return true; }
        if (txt == "Dos Miradas") { traduccion = "Two Views"; return true; }

        if (txt == "Entrar") { traduccion = "Enter"; return true; }
        if (txt == "Sellarla") { traduccion = "Seal It"; return true; }
        if (txt == "Imponer silencio") { traduccion = "Impose Silence"; return true; }
        if (txt == "Empujarlo") { traduccion = "Push It"; return true; }
        if (txt == "Descargarlo") { traduccion = "Unload It"; return true; }
        if (txt == "Interpretarlas") { traduccion = "Interpret Them"; return true; }
        if (txt == "Cargarlo") { traduccion = "Carry Them"; return true; }
        if (txt == "Relevarlo") { traduccion = "Relieve Them"; return true; }
        if (txt == "Recuperarla") { traduccion = "Recover It"; return true; }
        if (txt == "Dejarla ir") { traduccion = "Let It Go"; return true; }
        if (txt == "Decidir") { traduccion = "Decide"; return true; }
        if (txt.StartsWith("H") && txt.EndsWith("roe 1")) { traduccion = "Hero 1"; return true; }

        if (txt.StartsWith("La Caravana llega a un tramo estrecho, quebrado y lleno de tablones flojos."))
        {
            traduccion = "The Caravan reaches a narrow, broken stretch full of loose planks. It does not seem impossible to cross, but it is treacherous enough for one mistake to turn into an accident.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar guiar el cruce antes de que cunda el p"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes can try to guide the crossing before panic spreads.\n\n"
                : "</color></b> can try to guide the crossing before panic spreads.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Reflejos DC "))
        {
            traduccion = "<color=#ba3fef>-Saving Throw: Reflex DC ";
            return true;
        }
        if (txt == " <i>(TS Reflejos actual: ")
        {
            traduccion = " <i>(Current Reflex Save: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("35 Experiencia. Si falla, obtendr") && txt.Contains("Herida.</color>"))
        {
            traduccion = ").</i> On success, they gain 35 Experience. On failure, they suffer an Injury.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Reflejos DC 11"))
        {
            traduccion = "<color=#ba3fef>-If you try it, they will make a Reflex Saving Throw, DC 11. On success, they gain 35 Experience. On failure, they suffer an Injury.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides rodear el tramo"))
        {
            traduccion = "<color=#ba3fef>-If you choose to go around the stretch, the Caravan gains +1 Fatigue.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Desde una bodega medio tapada llegan golpes apagados"))
        {
            traduccion = "Muffled knocks and cries for help come from a half-covered cellar. The air pouring out is heavy with old dust, mold, and something that scratches at the throat the moment anyone draws near.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar entrar y sacar a quienes sigan con vida"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes can try to get inside and bring out anyone still alive before the place collapses.\n\n"
                : "</color></b> can try to get inside and bring out anyone still alive before the place collapses.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Fortaleza DC "))
        {
            traduccion = "<color=#ba3fef>-Saving Throw: Fortitude DC ";
            return true;
        }
        if (txt == " <i>(TS Fortaleza actual: ")
        {
            traduccion = " <i>(Current Fortitude Save: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, rescatar") && txt.Contains("6-10 Civiles") && txt.Contains("30 Experiencia"))
        {
            traduccion = ").</i> On success, they rescue 6-10 Civilians and gain 30 Experience. On failure, they become Sick for 3 days.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Fortaleza DC 10"))
        {
            traduccion = "<color=#ba3fef>-If you try it, they will make a Fortitude Saving Throw, DC 10. On success, they rescue 6-10 Civilians and gain 30 Experience. On failure, they become Sick for 3 days.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides sellar la entrada"))
        {
            traduccion = "<color=#ba3fef>-If you choose to seal the entrance and move on, -4 Hope.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Una versi") && txt.Contains("peligro cercano se esparce de carro en carro"))
        {
            traduccion = "An exaggerated version of a nearby danger spreads from wagon to wagon and starts stirring needless panic. Within minutes, several Civilians are already talking about abandoning the march before they get trapped.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar frenarlo con calma antes de que empeore"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes can try to calm the rumor before it gets worse.\n\n"
                : "</color></b> can try to calm things down before it gets worse.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Mental DC "))
        {
            traduccion = "<color=#ba3fef>-Saving Throw: Mental DC ";
            return true;
        }
        if (txt == " <i>(TS Mental actual: ")
        {
            traduccion = " <i>(Current Mental Save: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("+4 Esperanza") && txt.Contains("Baja Moral por 3"))
        {
            traduccion = ").</i> On success, they gain 35 Experience and +4 Hope. On failure, they suffer Low Morale for 3 days and the Caravan loses 5 Hope.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Mental DC 12"))
        {
            traduccion = "<color=#ba3fef>-If you try it, they will make a Mental Saving Throw, DC 12. On success, they gain 35 Experience and +4 Hope. On failure, they suffer Low Morale for 3 days and the Caravan loses 5 Hope.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides imponer silencio"))
        {
            traduccion = "<color=#ba3fef>-If you choose to impose silence by force, -3 Hope.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("La corriente parece mansa desde lejos"))
        {
            traduccion = "The current looks gentle from afar, but as soon as the first wagons touch the ford it becomes clear the bottom is slippery and the water pulls harder than expected.\n\n";
            return true;
        }
        if (txt.StartsWith("Alguien tendr") && txt.Contains("ordenar el cruce de los bueyes"))
        {
            traduccion = "Someone will have to move ahead to organize the oxen crossing and keep everything from falling apart in the middle of the ford.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Reflejos DC ")
        {
            traduccion = "</color></b>: Reflex Save DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("40 Experiencia") && txt.Contains("1 Buey"))
        {
            traduccion = ").</i> On success, they gain 40 Experience. On failure, they suffer an Injury and the Caravan loses 1 Ox.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides no arriesgar el cruce"))
        {
            traduccion = "<color=#ba3fef>-If you choose not to risk the crossing, the detour will advance the Black Breath.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Uno de los carros queda mal encajado entre piedras"))
        {
            traduccion = "One of the wagons gets badly wedged between stones and hard mud. If they do not free it quickly, the whole march will jam around it and tempers will start to rise.\n\n";
            return true;
        }
        if (txt.StartsWith("Hace falta fuerza y aguante para moverlo"))
        {
            traduccion = "It will take strength and endurance to move it without getting hurt.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Fortaleza DC ")
        {
            traduccion = "</color></b>: Fortitude Save DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("+3 Esperanza") && txt.Contains("+1 Fatiga"))
        {
            traduccion = ").</i> On success, they gain 35 Experience and +3 Hope. On failure, they suffer an Injury and the Caravan gains +1 Fatigue.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides descargar el carro"))
        {
            traduccion = "<color=#ba3fef>-If you choose to unload the wagon and move on, +1 Fatigue.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("En un poste vencido y en varias piedras cercanas"))
        {
            traduccion = "Ancient courier marks appear on a toppled post and several nearby stones, nearly erased by time. It still seems possible to get something useful out of that code if someone can read it properly.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar interpretarlas antes de que se pierda la luz"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes can try to interpret them before the light fades.\n\n"
                : "</color></b> can try to interpret them before the light fades.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Mental DC "))
        {
            traduccion = "<color=#a0e812>-Saving Throw: Mental DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, se revelar") && txt.Contains("30 Experiencia") && txt.Contains("+1 Fatiga"))
        {
            traduccion = ").</i> On success, nearby nodes will be revealed and they gain 30 Experience. On failure, the delay causes the Caravan to gain +1 Fatigue.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Mental DC 10"))
        {
            traduccion = "<color=#a0e812>-If you try it, they will make a Mental Saving Throw, DC 10. On success, nearby nodes will be revealed and they gain 30 Experience. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides seguir sin detenerte"))
        {
            traduccion = "<color=#a0e812>-If you choose to keep moving without stopping, +3 Hope.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Un embotellamiento de carros, Civiles y animales"))
        {
            traduccion = "A bottleneck of wagons, Civilians, and animals breaks the march's rhythm. It is not serious yet, but if no one orders the line with authority the confusion may spread quickly.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Mental DC ")
        {
            traduccion = "</color></b>: Mental Save DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("+6 Esperanza") && txt.Contains("2 Esperanza"))
        {
            traduccion = ").</i> On success, they gain 30 Experience and the Caravan gains +6 Hope. On failure, the Caravan loses 2 Hope.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejar que la fila"))
        {
            traduccion = "<color=#a0e812>-If you choose to let the line settle on its own, +3 Hope.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Un Civil agotado se desploma en el camino."))
        {
            traduccion = "An exhausted Civilian collapses on the road. Nobody seems to notice, or care, and they pass by as if nothing happened.\n\n";
            return true;
        }
        if (txt.Contains("se ofrece a levantarlo y cargarlo"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes offers to lift them and carry them. But you may order them to save their strength for dangers still ahead on the road.\n\n"
                : "</color></b> offers to lift them and carry them. But you may order them to save their strength for dangers still ahead on the road.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Fortaleza DC "))
        {
            traduccion = "<color=#a0e812>-Saving Throw: Fortitude DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("50 Experiencia") && txt.Contains("+5 Esperanza") && txt.Contains("Fatigado"))
        {
            traduccion = ").</i> On success, they gain 50 Experience and the Caravan gets +5 Hope. On failure, they become Fatigued.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Fortaleza DC 13") && txt.Contains("+5 Esperanza") && txt.Contains("Fatigado"))
        {
            traduccion = "<color=#a0e812>-If you try it, they will make a Fortitude Saving Throw, DC 13. On success, they gain 50 Experience and the Caravan gets +5 Hope. On failure, they become Fatigued.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejar al Civil"))
        {
            traduccion = "<color=#a0e812>-If you choose to leave the Civilian behind, -5 Hope. -1 Civilian.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Una r") && txt.Contains("cartera de viaje con mapas, notas y referencias"))
        {
            traduccion = "A gust of wind drags away a travel satchel with maps, notes, and useful references right to an awkward ledge. It can still be recovered, but it will take speed and precision.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar atraparla antes de que se pierda del todo"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "One of the Heroes can try to catch it before it is lost for good.\n\n"
                : "</color></b> can try to catch it before it is lost for good.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Reflejos DC "))
        {
            traduccion = "<color=#a0e812>-Saving Throw: Reflex DC ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, se revelar") && txt.Contains("35 Experiencia") && txt.Contains("+4 Esperanza"))
        {
            traduccion = ").</i> On success, nearby nodes will be revealed, they gain 35 Experience, and the Caravan gains +4 Hope. On failure, they suffer an Injury.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Reflejos DC 13"))
        {
            traduccion = "<color=#a0e812>-If you try it, they will make a Reflex Saving Throw, DC 13. On success, nearby nodes will be revealed, they gain 35 Experience, and the Caravan gains +4 Hope. On failure, they suffer an Injury.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejarla ir"))
        {
            traduccion = "<color=#a0e812>-If you choose to let it go, -4 Hope.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("La ruta se abre en varias direcciones parecidas"))
        {
            traduccion = "The road opens into several similar directions and the few useful signs seem to contradict one another. Two members of the caravan appear to disagree. Who will you listen to?\n\n";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("25 Experiencia") && txt.Contains("retroced"))
        {
            traduccion = ").</i> On success, they gain 25 Experience and the Black Breath is pushed back by 1. On failure, the Caravan gains +1 Fatigue.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides mantener la ruta"))
        {
            traduccion = "<color=#a0e812>-If you choose to stay on the route without taking risks, +4 Hope.</color>\n\n";
            return true;
        }

        traduccion = null;
        return false;
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

        if (TryTraducirEventosCampaniaUiIngles(txt, out string traduccionEventoUiIngles))
        {
            return traduccionEventoUiIngles;
        }
        string final = txt.Normalize(NormalizationForm.FormC);
        switch (final)
        {
            case "Danza del Estoque":
                r = "Sword Dance";
                break;
            case "Estoque del Primer Sangre":
                r = "First Blood Estoc";
                break;
            case "Estoque de Veloz Replica":
                r = "Swift Riposte Estoc";
                break;
            case "Estoque de la Rosa Negra":
                r = "Black Rose Estoc";
                break;
            case "Gambeson de Esgrima Ligera":
                r = "Light Fencing Gambeson";
                break;
            case "Gambeson del Temple":
                r = "Gambeson of Composure";
                break;
            case "Gambeson del Ultimo Paso":
                r = "Last Step Gambeson";
                break;
            case "Blanco Medido":
                r = "Measured Target";
                break;
            case "Rosa Negra":
                r = "Black Rose";
                break;
            case "Ultimo Paso":
                r = "Last Step";
                break;
            case "Danzando":
                r = "Dancing";
                break;
            case "Encadena bajas por este turno: +1 Ataque y +15% Danio.":
                r = "Chains kills this turn: +1 Attack and +15% Damage.";
                break;
            case "Encadena bajas por este turno: +1 Ataque, +15% Danio y +1 rango critico.":
                r = "Chains kills this turn: +1 Attack, +15% Damage and +1 Crit Range.";
                break;
            case "Presencia Provocadora":
                r = "Provoking Presence";
                break;
            case "Distraído":
                r = "Distracted";
                break;
            case "Pierde foco: -2 Defensa y -3 Armadura.":
                r = "Loses focus: -2 Defense and -3 Armor.";
                break;
            case "Pierde foco: -2 Defensa y -4 Armadura.":
                r = "Loses focus: -2 Defense and -4 Armor.";
                break;
            case "Recuperando Aire":
                r = "Catching Breath";
                break;
            case "Descansa para el turno siguiente: +3 PA maximo, -4 Defensa.":
                r = "Rests for the next turn: +3 Max AP, -4 Defense.";
                break;
            case "Descansa para el turno siguiente: +3 PA maximo, -3 Defensa.":
                r = "Rests for the next turn: +3 Max AP, -3 Defense.";
                break;
            case "Solo en columna trasera.":
                r = "Rear column only.";
                break;
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
            case "Cenizas en el Camino":
                r = "Ashes on the Road";
                break;
            case "Bestias Aterradas":
                r = "Panicked Beasts";
                break;
            case "Fuego en la Retaguardia":
                r = "Fire in the Rearguard";
                break;
            case "Tambores en la Niebla":
                r = "Drums in the Mist";
                break;
            case "Hielo Quebradizo":
                r = "Thin Ice";
                break;
            case "Efigies del Paso":
                r = "Effigies of the Pass";
                break;
            case "Partida de Caza":
                r = "Hunting Party";
                break;
            case "Frío Hasta los Huesos":
                r = "Cold to the Bone";
                break;
            case "Tótem de Guerra":
                r = "War Totem";
                break;
            case "Brote entre las Brasas":
                r = "Sprout Among the Embers";
                break;
            case "Madera Medio Quemada":
                r = "Half-Burnt Timber";
                break;
            case "Refugio de Piedra":
                r = "Stone Shelter";
                break;
            case "Sendero del Carnero":
                r = "Ram's Trail";
                break;
            case "Cielo Abierto":
                r = "Open Sky";
                break;
            case "Efigie Derribada":
                r = "Toppled Effigy";
                break;
            case "Juramento del Paso":
                r = "Oath of the Pass";
                break;
            case "Viento a Favor":
                r = "Tailwind";
                break;
            case "Humo en el Campamento":
                r = "Smoke in the Camp";
                break;
            case "Guardia Somnolienta":
                r = "Drowsy Watch";
                break;
            case "Raciones Mojadas":
                r = "Soaked Rations";
                break;
            case "Discusión en la Fogata":
                r = "Argument by the Fire";
                break;
            case "Herramientas Perdidas":
                r = "Missing Tools";
                break;
            case "Noche Serena":
                r = "Calm Night";
                break;
            case "Fogón Compartido":
                r = "Shared Campfire";
                break;
            case "Manos Voluntariosas":
                r = "Helping Hands";
                break;
            case "Sueño Reparador":
                r = "Restful Sleep";
                break;
            case "Hallazgo entre los Carros":
                r = "Find Among the Carts";
                break;
            case "Escalofríos Nocturnos":
                r = "Night Chills";
                break;
            case "Noche en Vela":
                r = "Sleepless Night";
                break;
            case "Práctica Imprudente":
                r = "Reckless Practice";
                break;
            case "Bolsa Olvidada":
                r = "Forgotten Pouch";
                break;
            case "Lección junto al Fuego":
                r = "Lesson by the Fire";
                break;
            case "Palabras Necesarias":
                r = "Needed Words";
                break;
            case "Brasas Errantes":
                r = "Wandering Embers";
                break;
            case "Tronco Reavivado":
                r = "Rekindled Log";
                break;
            case "Calor de las Cenizas":
                r = "Warmth of the Ashes";
                break;
            case "Hongos del Carbón":
                r = "Charcoal Mushrooms";
                break;
            case "Cánticos de Madrugada":
                r = "Dawn Chants";
                break;
            case "Huellas alrededor del Campamento":
                r = "Tracks around the Camp";
                break;
            case "Vigilia Helada":
                r = "Frozen Vigil";
                break;
            case "Símbolo en la Nieve":
                r = "Symbol in the Snow";
                break;
            case "Paredón contra el Viento":
                r = "Rock Wall against the Wind";
                break;
            case "Paso en Silencio":
                r = "Pass in Silence";
                break;
            case "Rastro del Rebaño":
                r = "Trail of the Herd";
                break;
            case "Aurora del Paso":
                r = "Aurora over the Pass";
                break;
            case "Golpes Bajo el Empedrado":
                r = "Thuds Beneath the Cobblestones";
                break;
            case "Brecha en la Calzada":
                r = "Breach in the Roadway";
                break;
            case "Ecos en el Pozo":
                r = "Echoes in the Well";
                break;
            case "Campanas sin Torre":
                r = "Bells without a Tower";
                break;
            case "Acecho en los Tejados":
                r = "Stalking on the Rooftops";
                break;
            case "Puerta Astillada":
                r = "Splintered Door";
                break;
            case "Faroles Prestados":
                r = "Borrowed Lanterns";
                break;
            case "Barricada Todavía en Pie":
                r = "Barricade Still Standing";
                break;
            case "Sótano con Vida":
                r = "A Living Cellar";
                break;
            case "Señales de Tiza":
                r = "Chalk Marks";
                break;
            case "Valor en la Plaza":
                r = "Valor in the Square";
                break;
            case "Campamento entre Columnas":
                r = "Camp among Columns";
                break;
            case "Luz Ahogada":
                r = "Smothered Light";
                break;
            case "Techo Inestable":
                r = "Unstable Roof";
                break;
            case "Ruidos en la Bodega":
                r = "Noises in the Cellar";
                break;
            case "Lista en la Pared":
                r = "List on the Wall";
                break;
            case "Ventana Vigilante":
                r = "Watch Window";
                break;
            case "Fogón entre Escombros":
                r = "Fire among the Rubble";
                break;
            case "Patio de Evacuación":
                r = "Evacuation Courtyard";
                break;
            case "Carta sin Enviar":
                r = "Unsent Letter";
                break;
            case "Siluetas entre los Arboles":
                r = "Figures Among the Trees";
                break;
            case "Barro que Retiene":
                r = "Clinging Mud";
                break;
            case "Promesa Incumplida":
                r = "Broken Promise";
                break;
            case "Rutina Floja":
                r = "Slack Routine";
                break;
            case "Pesadillas Compartidas":
                r = "Shared Nightmares";
                break;
            case "Descanso Incompleto":
                r = "Incomplete Rest";
                break;
            case "Quejas en Voz Baja":
                r = "Low Whispers";
                break;
            case "Fogatas Demasiado Lejos":
                r = "Campfires Too Far Apart";
                break;
            case "Arenga en la Lluvia":
                r = "Speech in the Rain";
                break;
            case "Camino a Favor":
                r = "Favorable Road";
                break;
            case "Juramento de la Escolta":
                r = "Oath of the Escort";
                break;
            case "Rastro Sospechoso":
                r = "Suspicious Trail";
                break;
            case "Circulo de Historias":
                r = "Circle of Stories";
                break;
            case "Campamento Ligero":
                r = "Light Camp";
                break;
            case "Repaso de Maniobras":
                r = "Drill Review";
                break;
            case "Guardias Relevados":
                r = "Relieved Watch";
                break;
            case "Inspiración":
                r = "Inspiration";
                break;

            case "Presteza":
                r = "Swiftness";
                break;
            case "Compromiso":
                r = "Commitment";
                break;
            case "Vigilante":
                r = "Watchful";
                break;
            case "Acobardados":
                r = "Cowed";
                break;
            case "Aletargados":
                r = "Sluggish";
                break;
            case "Desmotivación":
                r = "Demotivation";
                break;
            case "Descuidados":
                r = "Careless";
                break;
            case "Acumulaciones: ":
                r = "Stacks: ";
                break;
            case "+2 VAL a toda la Caravana en el próximo combate.":
                r = "+2 VAL to the whole Caravan in the next combat.";
                break;
            case "El Aliento Negro no avanza en el próximo viaje.":
                r = "The Black Breath does not advance on the next journey.";
                break;
            case "+20% Experiencia en el próximo combate.":
                r = "+20% Experience in the next combat.";
                break;
            case "+10% Exploración y -10% emboscadas durante 1 viaje.":
                r = "+10% Exploration and -10% ambushes for 1 journey.";
                break;
            case "-2 VAL a toda la Caravana en el próximo combate.":
                r = "-2 VAL to the whole Caravan in the next combat.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje.":
                r = "+1 Black Breath advance on the next journey.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje y marcha visual más lenta.":
                r = "+1 Black Breath advance on the next journey and slower visual march speed.";
                break;
            case "-20% Experiencia en el próximo combate.":
                r = "-20% Experience in the next combat.";
                break;
            case "-10% Exploración y +10% emboscadas durante 1 viaje.":
                r = "-10% Exploration and +10% ambushes for 1 journey.";
                break;
            case "-La Caravana cerro filas, pero el miedo quedo instalado. Acobardados para el proximo combate.":
                r = "-The Caravan closed ranks, but the fear remained. Cowed for the next combat.";
                break;
            case "-La Caravana forzo el paso entre el barro y quedo Aletargada.":
                r = "-The Caravan forced its way through the mud and became Sluggish.";
                break;
            case "-La escena de la promesa incumplida dejo a la Caravana Desmotivada.":
                r = "-The broken promise left the Caravan Demotivated.";
                break;
            case "-La escena de la promesa incumplida dejo a la Caravana desmotivada.":
                r = "-The broken promise left the Caravan demotivated.";
                break;
            case "-La rutina floja se impuso y la Caravana quedo Descuidados por 1 viaje.":
                r = "-A slack routine took hold and the Caravan became Careless for 1 journey.";
                break;
            case "-Las quejas en voz baja drenaron el animo. Desmotivacion para el proximo combate.":
                r = "-Low whispers drained morale. Demotivation for the next combat.";
                break;
            case "-Las quejas en voz baja drenaron el animo. Desmotivación para el proximo combate.":
                r = "-Low whispers drained morale. Demotivation for the next combat.";
                break;
            case " sostuvo la arenga bajo la lluvia (1d20: ":
                r = " held the speech beneath the rain (1d20: ";
                break;
            case "). +30 Experiencia e Inspiracion para el proximo combate.":
                r = "). +30 Experience and Inspiration for the next combat.";
                break;
            case "). +30 Experiencia e Inspiración para el proximo combate.":
                r = "). +30 Experience and Inspiration for the next combat.";
                break;
            case " no logro encender a todos con su arenga (1d20: ":
                r = " failed to inspire everyone with the speech (1d20: ";
                break;
            case "), pero la Caravana sostuvo el animo. +2 Esperanza.":
                r = "), but the Caravan held its nerve. +2 Hope.";
                break;
            case "-La arenga logro encender a la Caravana. Inspiracion para el proximo combate.":
                r = "-The speech stirred the Caravan. Inspiration for the next combat.";
                break;
            case "-La arenga logro encender a la Caravana. Inspiración para el proximo combate.":
                r = "-The speech stirred the Caravan. Inspiration for the next combat.";
                break;
            case "-La Caravana aprovecho el buen tramo de camino y obtuvo Presteza.":
                r = "-The Caravan took advantage of the favorable stretch of road and gained Swiftness.";
                break;
            case "-El juramento de la escolta reforzo el Compromiso de la Caravana.":
                r = "-The escort's oath strengthened the Caravan's Commitment.";
                break;
            case " leyo el rastro a tiempo (1d20: ":
                r = " read the trail in time (1d20: ";
                break;
            case "). +30 Experiencia y Vigilante por 1 viaje.":
                r = "). +30 Experience and Watchful for 1 journey.";
                break;
            case " no logro leer bien el rastro (1d20: ":
                r = " failed to read the trail properly (1d20: ";
                break;
            case "). +1 Fatiga.":
                r = "). +1 Fatigue.";
                break;
            case "-La Caravana ajusto la vigilancia tras ver el rastro.":
                r = "-The Caravan tightened its watch after seeing the trail.";
                break;
            case " dirigio un repaso de maniobras util (1d20: ":
                r = " led a useful drill review (1d20: ";
                break;
            case "). +35 Experiencia y Compromiso.":
                r = "). +35 Experience and Commitment.";
                break;
            case " no logro ordenar bien el repaso de maniobras. +1 Fatiga.":
                r = " failed to organize the drill review properly. +1 Fatigue.";
                break;
            case "-El repaso de maniobras dejo a la Caravana con mas Compromiso.":
                r = "-The drill review left the Caravan with greater Commitment.";
                break;
            case "-El humo inquieto dejo a la Caravana Aletargada.":
                r = "-The restless smoke left the Caravan Sluggish.";
                break;
            case " rompio el mal augurio de los cuervos. +2 Esperanza.":
                r = " broke the ill omen of the crows. +2 Hope.";
                break;
            case " no logro cortar el malestar de la Caravana. Acobardados para el proximo combate.":
                r = " failed to break the Caravan's unease. Cowed for the next combat.";
                break;
            case "-Los cuervos del paso dejaron a la Caravana Acobardada.":
                r = "-The crows of the pass left the Caravan Cowed.";
                break;
            case "-El eco bajo los pies desordeno la marcha. Descuidados por 1 viaje.":
                r = "-The echo beneath their feet disrupted the march. Careless for 1 journey.";
                break;
            case "-La veta de resina permitio ordenar una salida rapida. Presteza.":
                r = "-The resin vein allowed for a quick departure. Swiftness.";
                break;
            case " vigilo desde el hielo alto y ordeno la marcha. +30 Experiencia y Vigilante.":
                r = " kept watch from the high ice and organized the march. +30 Experience and Watchful.";
                break;
            case " bajo agotado del filo helado. +1 Fatiga.":
                r = " came down exhausted from the frozen ledge. +1 Fatigue.";
                break;
            case "-La Caravana logro una mejor vigilancia desde el hielo.":
                r = "-The Caravan achieved better watch from the ice.";
                break;
            case "-La senal de los resistentes reforzo el Compromiso de la Caravana.":
                r = "-The sign of the holdouts strengthened the Caravan's Commitment.";
                break;
            case "-La señal de los resistentes reforzo el Compromiso de la Caravana.":
                r = "-The sign of the holdouts strengthened the Caravan's Commitment.";
                break;
            case "-La señal de los resistentes reforzó el Compromiso de la Caravana.":
                r = "-The sign of the holdouts strengthened the Caravan's Commitment.";
                break;
            case "-La noche dejo a la Caravana Acobardada para el proximo combate.":
                r = "-The night left the Caravan Cowed for the next combat.";
                break;
            case "-El descanso incompleto dejo a la Caravana Aletargada.":
                r = "-Incomplete rest left the Caravan Sluggish.";
                break;
            case "-Las fogatas demasiado lejos dejaron a la Caravana Descuidados por 1 viaje.":
                r = "-Campfires too far apart left the Caravan Careless for 1 journey.";
                break;
            case "-Las historias junto al fuego dejaron a la Caravana con Inspiracion.":
                r = "-Stories by the fire left the Caravan with Inspiration.";
                break;
            case "-Las historias junto al fuego dejaron a la Caravana con Inspiración.":
                r = "-Stories by the fire left the Caravan with Inspiration.";
                break;
            case "-El campamento ligero dejo a la Caravana lista para avanzar con Presteza.":
                r = "-The light camp left the Caravan ready to advance with Swiftness.";
                break;
            case "-Los guardias relevados dejaron a la Caravana Vigilante.":
                r = "-The relieved guards left the Caravan Watchful.";
                break;
            case "-El presagio de los cuervos dejo a la Caravana Acobardada.":
                r = "-The omen of the crows left the Caravan Cowed.";
                break;
            case "-La respuesta dejada en Nedukazal inspiro a la Caravana.":
                r = "-The answer left behind in Nedukazal inspired the Caravan.";
                break;
            case "-La respuesta dejada en Nedukazal inspiró a la Caravana.":
                r = "-The answer left behind in Nedukazal inspired the Caravan.";
                break;
            case "-La Presteza de la Caravana ha evitado el avance del Aliento Negro durante el viaje.":
                r = "-The Caravan's Swiftness has prevented the Black Breath from advancing during the journey.";
                break;
            case "-La derrota dejó a la Caravana con Acobardados.":
                r = "-Defeat left the Caravan Cowed.";
                break;
            case "-La derrota dejó a la Caravana con Aletargados.":
                r = "-Defeat left the Caravan Sluggish.";
                break;
            case "-La derrota dejó a la Caravana con Desmotivación.":
                r = "-Defeat left the Caravan Demotivated.";
                break;
            case "-La derrota dejó a la Caravana con Descuidados.":
                r = "-Defeat left the Caravan Careless.";
                break;
            case "+2 VAL a toda la Caravana en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "+2 VAL to the whole Caravan in the next combat. Consumes 1 stack when a combat starts.";
                break;
            case "El Aliento Negro no avanza en el próximo viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "The Black Breath does not advance on the next journey. Consumes 1 stack when a journey starts.";
                break;
            case "+20% Experiencia en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "+20% Experience in the next combat. Consumes 1 stack when a combat starts.";
                break;
            case "+10% Exploración y -10% emboscadas durante 1 viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "+10% Exploration and -10% ambushes for 1 journey. Consumes 1 stack when a journey starts.";
                break;
            case "-2 VAL a toda la Caravana en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "-2 VAL to the whole Caravan in the next combat. Consumes 1 stack when a combat starts.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje y marcha visual más lenta. Consume 1 acumulación al iniciar un viaje.":
                r = "+1 Black Breath advance on the next journey and slower visual march speed. Consumes 1 stack when a journey starts.";
                break;
            case "-20% Experiencia en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "-20% Experience in the next combat. Consumes 1 stack when a combat starts.";
                break;
            case "-10% Exploración y +10% emboscadas durante 1 viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "-10% Exploration and +10% ambushes for 1 journey. Consumes 1 stack when a journey starts.";
                break;
            case "Humo Inquieto":
                r = "Restless Smoke";
                break;
            case "Cuervos del Paso":
                r = "Crows of the Pass";
                break;
            case "Eco Bajo los Pies":
                r = "Echo Beneath the Feet";
                break;
            case "Veta de Resina":
                r = "Resin Vein";
                break;
            case "Vigia del Hielo":
                r = "Ice Lookout";
                break;
            case "Señal de los Resistentes":
                r = "Sign of the Holdouts";
                break;
            case "Al costado del camino, varias siluetas se mueven entre la maleza justo fuera del alcance de la vista. Nadie logra confirmar si hay una amenaza real o solo trucos de la mente.\n\n":
                r = "Alongside the road, several shapes move through the brush just beyond clear sight. No one can tell whether there is a real threat or only exhausted imagination.\n\n";
                break;
            case "Los rumores corren rápido entre los carros y varios Civiles ya esperan un ataque inminente.\n\n":
                r = "Rumors spread quickly between the carts, and several Civilians already expect an imminent attack.\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas cerrar filas y seguir, la Caravana obtendrá Acobardados para el próximo combate. -2 VAL a todos.</color>\n\n":
                r = "<color=#ba3fef>-If you order them to close ranks and keep moving, the Caravan will gain Cowed for the next combat. -2 VAL to everyone.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides frenar para revisar, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to stop and check, the Black Breath will advance.</color>\n\n";
                break;
            case "Un tramo de barro pegajoso se agarra a ruedas, botas y arreos. Cada metro parece costar el doble, y la columna entera empieza a moverse con una pesadez desesperante.\n\n":
                r = "A stretch of sticky mud clings to wheels, boots, and harnesses. Every yard feels twice as costly, and the whole column starts moving with desperate heaviness.\n\n";
                break;
            case "<color=#ba3fef>-Si fuerzas la marcha igual, la Caravana obtendra Aletargados. El Aliento Negro avanzara +1 en el proximo viaje y la marcha se vera mas lenta.</color>\n\n":
                r = "<color=#ba3fef>-If you force the march anyway, the Caravan will gain Sluggish. The Black Breath will advance +1 on the next trip and the march will look slower.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas reacomodar la marcha, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you order the march to be reorganized, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "Encuentran un punto de espera abandonado: una manta, un fogon apagado y una senal vieja que promete ayuda que nunca llego. La escena cae pesado sobre la Caravana.\n\n":
                r = "They find an abandoned waiting point: a blanket, a dead fire, and an old sign promising help that never arrived. The scene weighs heavily on the Caravan.\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin detenerte, la Caravana obtendra Desmotivacion. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to keep going without stopping, the Caravan will gain Demotivation. It will earn 20% less Experience in the next combat.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin detenerte, la Caravana obtendra Desmotivación. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to keep going without stopping, the Caravan will gain Demotivation. It will earn 20% less Experience in the next combat.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si haces una breve parada para ordenar el paso, el Aliento Negro avanzara.</color>\n\n":
                r = "<color=#ba3fef>-If you call for a short halt to steady the march, the Black Breath will advance.</color>\n\n";
                break;
            case "Tras varias horas sin sobresaltos, parte de la Caravana empieza a moverse por pura costumbre. Se aflojan formaciones, cambian relevos tarde y mas de uno deja de mirar el terreno con atencion.\n\n":
                r = "After several quiet hours, part of the Caravan starts moving on pure habit. Formations loosen, reliefs come late, and more than one person stops watching the terrain closely.\n\n";
                break;
            case "<color=#ba3fef>-Si no dices nada, la Caravana obtendra Descuidados por 1 viaje. -10% Exploracion y +10% emboscadas.</color>\n\n":
                r = "<color=#ba3fef>-If you say nothing, the Caravan will gain Careless for 1 trip. -10% Exploration and +10% ambushes.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si reorganizas puestos y ritmo, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you reorganize positions and pace, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "Durante la noche, gritos ahogados despiertan a medio campamento. Al amanecer nadie logra explicar bien lo que soño, pero el miedo queda flotando igual entre las tiendas.\n\n":
                r = "During the night, muffled screams wake half the camp. By dawn no one can clearly explain what they dreamed, but the fear still hangs between the tents.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Acobardados para el proximo combate. -2 VAL a todos.</b></color>":
                r = "<color=#ba3fef><b>The Caravan gains Cowed for the next combat. -2 VAL to everyone.</b></color>";
                break;
            case "El suelo es incomodo, el viento no afloja y los carros crujen toda la noche. Nadie descansa de verdad, y la Caravana se levanta con la sensacion de haber dormido a medias.\n\n":
                r = "The ground is uncomfortable, the wind never lets up, and the carts creak all night. No one truly rests, and the Caravan rises feeling only half asleep.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Aletargados. El Aliento Negro avanzara +1 en el proximo viaje y la marcha se vera mas lenta.</b></color>":
                r = "<color=#ba3fef><b>The Caravan gains Sluggish. The Black Breath will advance +1 on the next trip and the march will look slower.</b></color>";
                break;
            case "Lo que empieza como murmullo termina recorriendo el campamento entero: cansancio, dudas, comparaciones con dias mejores. No hay gritos ni desbande, solo una erosion lenta del animo.\n\n":
                r = "What begins as a murmur ends up spreading through the whole camp: exhaustion, doubt, comparisons with better days. There are no shouts or panic, only a slow erosion of morale.\n\n";
                break;
            case "<color=#ba3fef>-Si dejas que se descarguen, la Caravana obtendra Desmotivacion. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-If you let them vent, the Caravan will gain Demotivation. It will earn 20% less Experience in the next combat.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si dejas que se descarguen, la Caravana obtendra Desmotivación. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-If you let them vent, the Caravan will gain Demotivation. It will earn 20% less Experience in the next combat.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas que se descarguen, la Caravana obtendrá 1 estado positivo aleatorio.</color>\n\n":
                r = "<color=#a0e812>-If you let them vent, the Caravan will gain a random positive status.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si cortas la charla y apagas el fuego, la Caravana perdera 9 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you cut the talk short and put out the fire, the Caravan will lose 9 Hope.</color>\n\n";
                break;
            case "El campamento queda armado demasiado disperso. Las fogatas no se cubren entre si, los llamados tardan en llegar y cuesta saber quien esta atento y quien no.\n\n":
                r = "The camp ends up too spread out. The fires do not cover one another, calls take too long to carry, and it becomes hard to know who is alert and who is not.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Descuidados por 1 viaje. -10% Exploracion y +10% emboscadas.</b></color>":
                r = "<color=#ba3fef><b>The Caravan gains Careless for 1 trip. -10% Exploration and +10% ambushes.</b></color>";
                break;
            case "La marcha se sostiene bajo una lluvia pesada y muda. Los Civiles avanzan con la cabeza gacha, hasta que alguien propone decir unas palabras antes de que el desaliento se vuelva costumbre.\n\n":
                r = "The march goes on beneath a heavy, wordless rain. The Civilians walk with heads lowered, until someone suggests saying a few words before discouragement becomes routine.\n\n";
                break;
            case "</color></b> puede intentar levantar a la Caravana.\n\n":
                r = "</color></b> can try to lift the Caravan's spirits.\n\n";
                break;
            case "<color=#a0e812>-Tirada de Salvacion: TS Mental DC ":
                r = "<color=#a0e812>-Saving Throw: Mental ST DC ";
                break;
            case ").</i> Si la supera, la Caravana obtendra Inspiracion para el proximo combate y ganara 30 Experiencia. Si falla, solo obtendra +2 Esperanza.</color>\n\n":
                r = ").</i> If they succeed, the Caravan will gain Inspiration for the next combat and earn 30 Experience. If they fail, it will only gain +2 Hope.</color>\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Inspiración para el proximo combate y ganara 30 Experiencia. Si falla, solo obtendra +2 Esperanza.</color>\n\n":
                r = ").</i> If they succeed, the Caravan will gain Inspiration for the next combat and earn 30 Experience. If they fail, it will only gain +2 Hope.</color>\n\n";
                break;
            case "<color=#a0e812>-Si lo intentas, un Heroe hara una Tirada de Salvacion Mental DC 11. Si la supera, la Caravana obtendra Inspiración y ese Heroe ganara 30 Experiencia.</color>\n\n":
                r = "<color=#a0e812>-If you try, a Hero will make a Mental Saving Throw DC 11. If they succeed, the Caravan will gain Inspiration and that Hero will earn 30 Experience.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no detener la marcha, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you choose not to stop the march, +3 Hope.</color>\n\n";
                break;
            case "La Caravana encuentra un tramo de camino firme, bien orientado y sorprendentemente limpio. No durara mucho, pero alcanza para ordenar la columna y pensar en un proximo avance veloz.\n\n":
                r = "The Caravan finds a stretch of road that is firm, well aligned, and surprisingly clear. It will not last long, but it is enough to steady the column and plan for a fast advance.\n\n";
                break;
            case "<color=#a0e812>-Si aprovechas el ritmo que da el terreno, la Caravana obtendra Presteza. El Aliento Negro no avanzara en el proximo viaje.</color>\n\n":
                r = "<color=#a0e812>-If you make the most of the terrain's rhythm, the Caravan will gain Swiftness. The Black Breath will not advance on the next trip.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres revisar bien los bordes del camino, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you prefer to carefully inspect the roadside, +3 Hope.</color>\n\n";
                break;
            case "Antes de retomar la marcha, dos Heroes se ofrecen a formalizar delante de la Caravana un juramento sencillo: no ceder terreno mientras quede alguien a quien proteger.\n\n":
                r = "Before the march resumes, two Heroes offer to formalize a simple oath before the Caravan: do not yield ground while there is still someone left to protect.\n\n";
                break;
            case "<color=#a0e812>-Si aceptas el juramento, la Caravana obtendra Compromiso. Ganara 20% mas Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#a0e812>-If you accept the oath, the Caravan will gain Commitment. It will earn 20% more Experience in the next combat.</color>\n\n";
                break;
            case "<color=#a0e812>-Si les pides reservar fuerzas y seguir, +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you ask them to save their strength and move on, +4 Hope.</color>\n\n";
                break;
            case "Unas marcas recientes junto al camino sugieren que alguien o algo estuvo siguiendo la columna desde hace rato. La noticia corre rapido entre quienes van en los carros traseros.\n\n":
                r = "Fresh marks by the roadside suggest that someone or something has been following the column for quite a while. The news spreads quickly among those riding the rear carts.\n\n";
                break;
            case "</color></b> puede leer el rastro y ordenar a tiempo la vigilancia.\n\n":
                r = "</color></b> can read the trail and set the watch in time.\n\n";
                break;
            case "<color=#a0e812>-Tirada de Salvacion: TS Reflejos DC ":
                r = "<color=#a0e812>-Saving Throw: Reflex ST DC ";
                break;
            case ").</i> Si la supera, la Caravana obtendra Vigilante por 1 viaje y ganara 30 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> If they succeed, the Caravan will gain Watchful for 1 trip and earn 30 Experience. If they fail, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no detenerte, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you decide not to stop, +2 Hope.</color>\n\n";
                break;
            case "Alguien empieza a contar una historia vieja junto al fuego. Otra voz corrige un detalle, otra suma un recuerdo, y pronto media Caravana esta escuchando con una sonrisa cansada.\n\n":
                r = "Someone starts telling an old story by the fire. Another voice corrects a detail, another adds a memory, and soon half the Caravan is listening with a tired smile.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Inspiracion para el proximo combate. +2 VAL a todos.</b></color>":
                r = "<color=#a0e812><b>The Caravan gains Inspiration for the next combat. +2 VAL to everyone.</b></color>";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Inspiración para el proximo combate. +2 VAL a todos.</b></color>":
                r = "<color=#a0e812><b>The Caravan gains Inspiration for the next combat. +2 VAL to everyone.</b></color>";
                break;
            case "Sin que nadie lo ordene demasiado, el campamento se arma con lo justo y queda listo para levantarse en minutos. Hay una sensación compartida de que hoy convendrá moverse rapido.\n\n":
                r = "Without any direct order, the camp is set up with the bare essentials and is ready to break in minutes. There is a shared feeling that today it will be best to move fast.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Presteza. El Aliento Negro no avanzara en el proximo viaje.</b></color>":
                r = "<color=#a0e812><b>The Caravan gains Swiftness. The Black Breath will not advance on the next trip.</b></color>";
                break;
            case "Antes de dormir, un Heroe propone repasar senales, posiciones y respuestas rapidas junto al fuego. No cambia el cansancio, pero podria dejar a todos mejor parados para el proximo choque.\n\n":
                r = "Before sleeping, a Hero suggests reviewing signals, positions, and quick responses by the fire. It does not ease the exhaustion, but it could leave everyone better prepared for the next clash.\n\n";
                break;
            case "</color></b> puede dirigir el repaso.\n\n":
                r = "</color></b> can lead the drill.\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Compromiso y ganara 35 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> If they succeed, the Caravan will gain Commitment and earn 35 Experience. If they fail, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres descansar de inmediato, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you prefer to rest immediately, +2 Hope.</color>\n\n";
                break;
            case "Los turnos de guardia salen mejor de lo esperado. Nadie queda de mas, nadie llega tarde y el campamento entero se siente mas atento sin perder descanso.\n\n":
                r = "The watch shifts go better than expected. No one stays too long, no one arrives late, and the whole camp feels more alert without losing rest.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Vigilante por 1 viaje. +10% Exploracion y -10% emboscadas.</b></color>":
                r = "<color=#a0e812><b>The Caravan gains Watchful for 1 trip. +10% Exploration and -10% ambushes.</b></color>";
                break;
            case "En el Bosque Ardiente, el humo cambia de direccion de golpe y se mete bajo telas, capuchas y lonas. La Caravana avanza entre toses y ojos llorosos, cada vez mas lenta.\n\n":
                r = "In the Burning Forest, the smoke suddenly changes direction and slips under cloth, hoods, and tarps. The Caravan pushes on through coughing and stinging eyes, slower and slower.\n\n";
                break;
            case "<color=#ba3fef>-Si decides avanzar igual, la Caravana obtendra Aletargados.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to keep moving anyway, the Caravan will gain Sluggish.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si haces una parada corta hasta que abra el aire, el Aliento Negro avanzara.</color>\n\n":
                r = "<color=#ba3fef>-If you call for a short halt until the air clears, the Black Breath will advance.</color>\n\n";
                break;
            case "Un circulo de cuervos se posa cerca del camino y no se mueve aunque la Caravana se acerque. Su quietud resulta peor que cualquier graznido, y el presagio corre rapido entre los Civiles.\n\n":
                r = "A ring of crows settles near the road and does not move even as the Caravan approaches. Their stillness is worse than any caw, and the omen spreads quickly among the Civilians.\n\n";
                break;
            case "</color></b> puede romper el malestar antes de que prenda.\n\n":
                r = "</color></b> can break the unease before it takes hold.\n\n";
                break;
            case ").</i> Si la supera, +2 Esperanza. Si falla, la Caravana obtendra Acobardados.</color>\n\n":
                r = ").</i> If they succeed, +2 Hope. If they fail, the Caravan will gain Cowed.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, un Heroe hara una Tirada de Salvacion Mental DC 11. Si falla, la Caravana obtendra Acobardados.</color>\n\n":
                r = "<color=#ba3fef>-If you try, a Hero will make a Mental Saving Throw DC 11. If they fail, the Caravan will gain Cowed.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin mirarlos, la Caravana obtendra Acobardados.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to keep moving without looking at them, the Caravan will gain Cowed.</color>\n\n";
                break;
            case "En Nedukazal, un golpeteo hueco sube desde abajo de la tierra y vuelve a cortarse antes de que alguien lo ubique. La reaccion inmediata es apurar el paso, pero no todos conservan la disciplina al hacerlo.\n\n":
                r = "In Nedukazal, a hollow knocking rises from beneath the ground and cuts off again before anyone can place it. The immediate reaction is to hurry forward, but not everyone keeps their discipline while doing so.\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas avanzar sin mirar atras, la Caravana obtendra Descuidados por 1 viaje.</color>\n\n":
                r = "<color=#ba3fef>-If you order them to push on without looking back, the Caravan will gain Careless for 1 trip.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si impones una marcha mas cerrada y cauta, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you impose a tighter, more cautious march, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "Una veta de resina endurecida marca un paso firme entre raices y tierra negra. La ruta apenas se sostiene, pero si la toman bien podria regalarle a la Caravana una salida rapida del sector.\n\n":
                r = "A vein of hardened resin marks a steady passage between roots and blackened earth. The route barely holds, but if they take it well it could give the Caravan a quick way out of the area.\n\n";
                break;
            case "<color=#a0e812>-Si la aprovechas, la Caravana obtendra Presteza.</color>\n\n":
                r = "<color=#a0e812>-If you use it, the Caravan will gain Swiftness.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres cruzar con maxima cautela, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you prefer to cross with maximum caution, +3 Hope.</color>\n\n";
                break;
            case "Un filo de roca y hielo ofrece un punto de vista raro en el Paso. Desde ahi, un ojo atento podria leer mejor el terreno y ordenar la marcha antes de que llegue el peligro.\n\n":
                r = "A ridge of rock and ice offers a rare vantage point in the Pass. From there, a watchful eye could read the terrain better and order the march before danger arrives.\n\n";
                break;
            case "</color></b> puede trepar y vigilar desde arriba.\n\n":
                r = "</color></b> can climb up and keep watch from above.\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Vigilante y ganara 30 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> If they succeed, the Caravan will gain Watchful and earn 30 Experience. If they fail, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no exponer a nadie, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you choose not to expose anyone, +2 Hope.</color>\n\n";
                break;
            case "En una pared semiderruida aparecen marcas recientes: no son de Zarkil ni de viejas rutas, sino senales de gente que todavia resiste y se niega a entregar el reino.\n\n":
                r = "On a half-collapsed wall there are fresh marks: not from Zarkil nor old roads, but signs from people still resisting and refusing to surrender the realm.\n\n";
                break;
            case "<color=#a0e812>-Si sigues la senal y la tomas como ejemplo, la Caravana obtendra Compromiso.</color>\n\n":
                r = "<color=#a0e812>-If you follow the sign and take it as an example, the Caravan will gain Commitment.</color>\n\n";
                break;
            case "<color=#a0e812>-Si sigues la señal y la tomas como ejemplo, la Caravana obtendra Compromiso.</color>\n\n":
                r = "<color=#a0e812>-If you follow the sign and take it as an example, the Caravan will gain Commitment.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas una respuesta para quienes pasen despues, la Caravana obtendra Inspiracion.</color>\n\n":
                r = "<color=#a0e812>-If you leave an answer for those who pass later, the Caravan will gain Inspiration.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas una respuesta para quienes pasen despues, la Caravana obtendra Inspiración.</color>\n\n":
                r = "<color=#a0e812>-If you leave an answer for those who pass later, the Caravan will gain Inspiration.</color>\n\n";
                break;
            case "Comerciante Visitante":
                r = "Visiting Merchant";
                break;
            case "Refuerzo en el Camino":
                r = "Reinforcement on the Road";
                break;
            case "Mensaje desde Serria":
                r = "Message from Serria";
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
            case "Esfuerzo":
                r = "Exherted";
                break;
            case "La unidad se ha esforzado.":
                r = "The unit has overexerted.";
                break;
            case "Continuar":
                r = "Continue";
                break;
            case "Aprovechar":
                r = "Use It";
                break;
            case "Aprovecharlo":
                r = "Use It";
                break;
            case "Apurar":
                r = "Hurry";
                break;
            case "Avanzar":
                r = "Push On";
                break;
            case "Cautela":
                r = "Caution";
                break;
            case "Cortarlas":
                r = "Shut It Down";
                break;
            case "Dejarla":
                r = "Let It Slide";
                break;
            case "Detenerse":
                r = "Stop";
                break;
            case "Escucharlas":
                r = "Listen";
                break;
            case "Forzar":
                r = "Force It";
                break;
            case "Hablar":
                r = "Speak";
                break;
            case "Leer el rastro":
                r = "Read the Trail";
                break;
            case "No subir":
                r = "Stay Down";
                break;
            case "Ordenar":
                r = "Restore Order";
                break;
            case "Reacomodar":
                r = "Reorder";
                break;
            case "Repasar":
                r = "Drill";
                break;
            case "Reservarse":
                r = "Hold Back";
                break;
            case "Responder":
                r = "Answer";
                break;
            case "Romper el clima":
                r = "Break the Omen";
                break;
            case "Subir":
                r = "Climb";
                break;
            case "Revisarlos":
                r = "Inspect Them";
                break;
            case "Revisar":
                r = "Inspect";
                break;
            case "Mantener la calma":
                r = "Keep Calm";
                break;
            case "Seguirlo":
                r = "Follow It";
                break;
            case "Estudiarlo":
                r = "Study It";
                break;
            case "Guiar el cruce":
                r = "Guide the Crossing";
                break;
            case "Rodear la brecha":
                r = "Go around the Breach";
                break;
            case "Investigar":
                r = "Investigate";
                break;
            case "Cerrar filas":
                r = "Close Ranks";
                break;
            case "Seguirlas":
                r = "Follow Them";
                break;
            case "Reforzar el camino":
                r = "Reinforce the Path";
                break;
            case "Asegurarlo":
                r = "Secure It";
                break;
            case "Mover campamento":
                r = "Move Camp";
                break;
            case "Atrancar":
                r = "Barricade It";
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
            case "Esperar":
                r = "Wait";
                break;
            case "Apurar el paso":
                r = "Hurry the March";
                break;
            case "Cruzar":
                r = "Cross";
                break;
            case "Rodear":
                r = "Go Around";
                break;
            case "Prepararse":
                r = "Prepare";
                break;
            case "Esconder a los Civiles":
                r = "Hide the Civilians";
                break;
            case "Seguir el rastro":
                r = "Follow the Trail";
                break;
            case "Mantener la ruta":
                r = "Stay on Route";
                break;
            case "Seguir":
                r = "Push On";
                break;
            case "Contenerlos":
                r = "Hold Them Back";
                break;
            case "Apartarse":
                r = "Step Aside";
                break;
            case "Apagarlo":
                r = "Put It Out";
                break;
            case "Abandonar carga":
                r = "Abandon Cargo";
                break;
            case "Recolectar":
                r = "Gather";
                break;
            case "Dejarlo":
                r = "Leave It";
                break;
            case "Doblar guardia":
                r = "Double the Watch";
                break;
            case "Dejarlos dormir":
                r = "Let Them Sleep";
                break;
            case "Secarlas":
                r = "Dry Them";
                break;
            case "Desecharlas":
                r = "Discard Them";
                break;
            case "Buscarlas":
                r = "Search for Them";
                break;
            case "Reemplazarlas":
                r = "Replace Them";
                break;
            case "Organizar":
                r = "Organize";
                break;
            case "Guardar":
                r = "Store Them";
                break;
            case "Repartir":
                r = "Hand Them Out";
                break;
            // EventosAdmin remaining literals (exact text keys)

            case "Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n":
                r = "One of the main people in charge of guiding the caravan and choosing the safest routes accidentally lost their maps.\n"; break;
            case "Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podría perderse.\n\n\n\n\n\n\n":
                r = "The other leaders will help search for them since those maps contain crucial information about the current area, and without them the caravan could get lost.\n\n\n\n\n\n\n"; break;
            case "Obtendrá el estado Enfermo por 4-7 días. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 día.\n\n\n\n\n":
                r = "Will gain the Sick status for 4-7 days. Each tier of the Healers' Retinue will reduce recovery time by 1 day.\n\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un día extra.</color>\n\n":
                r = "<color=#ba3fef>-You can buy medicine for 45 Gold to reduce the illness by one extra day.</color>\n\n"; break;
            case "Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botón.\n\n":
                r = "At a guard's shout, your attention turns to one of the wagons carrying the caravan's treasures. One of its chests is tipped and gold wass spilled on the ground. Apparently during the night, someone managed to force it and took part of the loot.\n\n"; break;
            case "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perdería 5 de Esperanza, <i>":
                r = "<color=#ba3fef>-You can subject the Civilians to an interrogation to try to find the thief:\n\n You would lose 5 Hope, <i>"; break;
            case "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n":
                r = "% Chances (40 base + Militiamen)</i> of finding the culprit and recovering the gold, -1 Civilian due to banishment.</color>\n\n"; break;
            case "Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n":
                r = "After a loud noise, you turn your head back and see that one of the supply wagons has had an accident. The wheels are stuck in the mud and the wagon seems to be lost for good.\n\n"; break;
            case "<color=#ba3fef>-Puedes pasar los 60 suministros caídos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n":
                r = "<color=#ba3fef>-You can transfer the 60 fallen supplies to another wagon, sacrificing 20 Materials; or accept the loss of supplies.</color>\n\n"; break;
            case "La Caravana encuentra un Río con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n":
                r = "The Caravan finds a river with good flow and seemingly decent water. Several excited civilians head towards it to recreate and refresh themselves.\n\n"; break;
            case "El agua podría estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n":
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
            case "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 días. Apoyas a:</color>\n\n":
                r = "<color=#ba3fef>-You must intervene in support of one of the two. The other will gain Low Morale for 5 days. You support:</color>\n\n"; break;
            case "Un Civil de origen noble se acerca a ti con altanería y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podría hacerlo mejor.\n":
                r = "A Civilian of noble origin approaches you arrogantly and begins to question your leadership, arguing that you are not making the right decisions for the Caravan's well-being and that he himself could do better.\n"; break;
            case "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n":
                r = "While his points are not very coherent, as he speaks loudly, several civilians begin to gather around, curious.\n\n"; break;
            case "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n":
                r = "<color=#ba3fef>-Hit him.</color> His family leaves the Caravan, withdrawing their investment. -65 Gold -8 Civilians -10 Hope\n\n"; break;
            case "Una ráfaga caliente levanta una espesa nube de cenizas y brasas apagadas alrededor de la Caravana.\n":
                r = "A hot gust lifts a thick cloud of ash and spent embers around the Caravan.\n"; break;
            case "Los civiles se cubren el rostro como pueden, los bueyes se inquietan y por varios instantes avanzar se vuelve peligroso.\n\n":
                r = "The civilians cover their faces as best they can, the oxen grow restless, and for several moments moving forward becomes dangerous.\n\n"; break;
            case "Puedes ordenar hacer una breve parada hasta que el aire se despeje o forzar la marcha para no perder tiempo.\n\n":
                r = "You can order a brief stop until the air clears or force the march so as not to lose time.\n\n"; break;
            case "<color=#ba3fef>-Si decides esperar, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to wait, the Black Breath will advance.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides seguir, las cenizas incomodarán a los Civiles. -5 Esperanza, +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to press on, the ash will trouble the Civilians. -5 Hope, +1 Fatigue.</color>\n\n"; break;
            case "Un grupo de bestias enloquecidas por el humo y el fuego irrumpe cerca del camino, cruzando entre los árboles calcinados con una violencia desesperada.\n\n":
                r = "A group of beasts maddened by smoke and fire bursts near the road, rushing between the charred trees with desperate violence.\n\n"; break;
            case "Los bueyes se inquietan al instante y varios Civiles retroceden alarmados. Si nadie actúa rápido, el caos podría extenderse a toda la Caravana.\n\n":
                r = "The oxen grow agitated at once and several Civilians recoil in alarm. If no one acts quickly, the chaos could spread through the whole Caravan.\n\n"; break;
            case "</color></b> puede intentar contener a los animales.</color> ":
                r = "</color></b> can try to hold the animals back.</color> "; break;
            case "Tirada de Salvación: TS Reflejos DC ":
                r = "Saving Throw: Reflex Save DC "; break;
            case " <i>(TS Reflejos actual: ":
                r = " <i>(Current Reflex Save: "; break;
            case ").</i> ":
                r = ").</i> "; break;
            case "Si lo logra, ganará 40 Experiencia. Si falla, la Caravana perderá 2 Bueyes.\n\n":
                r = "If successful, they will gain 40 Experience. On a failure, the Caravan will lose 2 Oxen.\n\n"; break;
            case "<color=#ba3fef>-Si decides apartarte y ceder el paso, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to step aside and yield the way, the Black Breath will advance.</color>\n\n"; break;
            case "Entre los troncos calcinados y la tierra ennegrecida, algunos Civiles descubren un pequeño brote verde abriéndose paso entre las brasas frías.\n\n":
                r = "Among the charred trunks and blackened soil, some Civilians discover a small green sprout pushing through the cold embers.\n\n"; break;
            case "La visión recorre rápidamente la Caravana. Por un instante, el Bosque Ardiente deja de parecer un lugar completamente perdido.\n\n":
                r = "Word spreads quickly through the Caravan. For a moment, the Burning Forest no longer seems like a place beyond saving.\n\n"; break;
            case "<color=#a0e812><b>+10 Esperanza</b></color>":
                r = "<color=#a0e812><b>+10 Hope</b></color>"; break;
            case "Al borde del camino, la Caravana encuentra restos de árboles derribados y estructuras carbonizadas. No todo quedó reducido a ceniza: parte de la madera todavía podría aprovecharse.\n\n":
                r = "At the roadside, the Caravan finds fallen trees and charred structures. Not all of it was reduced to ash: some of the timber could still be useful.\n\n"; break;
            case "Algunos Civiles sugieren detenerse para separar lo útil antes de seguir adelante. Tomará algo de tiempo, pero podría reforzar las reservas de Materiales.\n\n":
                r = "Some Civilians suggest stopping to sort out what is still useful before moving on. It will take some time, but it could bolster the Material reserves.\n\n"; break;
            case "<color=#ba3fef>-Si decides recolectar, obtendrás 15-30 Materiales, pero el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to gather it, you will gain 15-30 Materials, but the Black Breath will advance.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides dejarlo, evitarás el retraso. +3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to leave it, you will avoid the delay. +3 Hope.</color>\n\n"; break;
            case "Un foco de incendio vuelve a encenderse detrás de la Caravana y el viento empuja las llamas hacia la retaguardia.\n\n":
                r = "A fire flares up again behind the Caravan, and the wind drives the flames toward the rear.\n\n"; break;
            case "Durante unos instantes cunde el pánico: varios Civiles gritan, los bueyes tironean de los carros y parte de la carga corre peligro de prenderse fuego.\n\n":
                r = "For a few moments panic spreads: several Civilians scream, the oxen tug at the carts, and part of the cargo is in danger of catching fire.\n\n"; break;
            case "<color=#ba3fef>-Si decides apagarlo, la Caravana consumirá recursos en contener las llamas. -15 Suministros, +3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to put it out, the Caravan will spend resources to contain the flames. -15 Supplies, +3 Hope.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides abandonar carga, perderás 15-25 Materiales, pero evitarás que el fuego se acerque más.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to abandon cargo, you will lose 15-25 Materials, but you will keep the fire from drawing closer.</color>\n\n"; break;
            case "La leña húmeda y el viento jugaron en contra. El humo del campamento se metió entre los carros y casi nadie pudo descansar bien.\n\n":
                r = "Wet firewood and the wind worked against you. Smoke from the camp drifted between the carts and almost nobody rested well.\n\n"; break;
            case "Por la mañana, hay ojos irritados, tos y bastante malhumor.\n\n":
                r = "By morning, there are irritated eyes, coughing, and plenty of bad temper.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga, -3 Esperanza</b></color>":
                r = "<color=#ba3fef><b>+1 Fatigue, -3 Hope</b></color>"; break;
            case "Una parte de la guardia nocturna se quedó dormida por momentos. No pasó nada grave, pero el campamento amaneció inquieto.\n\n":
                r = "Part of the night watch dozed off for a while. Nothing serious happened, but the camp woke up uneasy.\n\n"; break;
            case "Puedes despertar a más gente para reforzar la vigilancia o dejar que el resto siga durmiendo y recuperar el tiempo al amanecer.\n\n":
                r = "You can wake more people to reinforce the watch or let the rest keep sleeping and make up the time at dawn.\n\n"; break;
            case "<color=#ba3fef>-Si decides doblar guardia, varios caravaneros descansarán peor. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to double the watch, several caravaners will rest worse. +1 Fatigue.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides dejarlos dormir, la salida será más lenta. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to let them sleep, departure will be slower. +1 Black Breath Advance.</color>\n\n"; break;
            case "Durante la noche se filtró agua en uno de los carros de comida y parte de las raciones quedó inutilizable.\n\n":
                r = "During the night, water seeped into one of the food carts and part of the rations became unusable.\n\n"; break;
            case "Puedes extender lo salvable junto al fuego antes de partir o desecharlo y seguir adelante.\n\n":
                r = "You can spread out what can still be saved by the fire before leaving or throw it away and move on.\n\n"; break;
            case "<color=#ba3fef>-Si decides secarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to dry them, you will leave later. +1 Black Breath Advance.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides desecharlas, perderás 18 Suministros.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to discard them, you will lose 18 Supplies.</color>\n\n"; break;
            case "Una discusión menor cerca de la fogata fue subiendo de tono y terminó dejando al campamento entero de mal humor.\n\n":
                r = "A minor argument by the fire kept escalating and ended with the whole camp in a sour mood.\n\n"; break;
            case "Nadie salió herido, pero el descanso se sintió más pesado de lo normal.\n\n":
                r = "No one was hurt, but the rest felt heavier than usual.\n\n"; break;
            case "<color=#ba3fef><b>-5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>-5 Hope</b></color>"; break;
            case "Al levantar el campamento, varios civiles notan que faltan herramientas básicas de trabajo. Puede que hayan quedado tiradas en la oscuridad.\n\n":
                r = "As the camp is packed up, several civilians notice that basic work tools are missing. They may have been left behind in the dark.\n\n"; break;
            case "Puedes ordenar una búsqueda rápida o reemplazarlas con lo que quede en reserva.\n\n":
                r = "You can order a quick search or replace them with what remains in reserve.\n\n"; break;
            case "<color=#ba3fef>-Si decides buscarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to search for them, you will leave later. +1 Black Breath Advance.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides reemplazarlas, perderás 12 Materiales.</color>\n\n":
                r = "<color=#ba3fef>-If you choose to replace them, you will lose 12 Materials.</color>\n\n"; break;
            case "Por una noche, el campamento se mantiene en calma. No hay sobresaltos, no hay discusiones y hasta el aire parece más liviano.\n\n":
                r = "For one night, the camp stays calm. There are no scares, no arguments, and even the air feels lighter.\n\n"; break;
            case "El descanso le hace bien a la Caravana.\n\n":
                r = "The rest does the Caravan good.\n\n"; break;
            case "<color=#a0e812><b>-1 Fatiga</b></color>":
                r = "<color=#a0e812><b>-1 Fatigue</b></color>"; break;
            case "Alrededor del fogón, algunos civiles y héroes comparten historias simples, comida caliente y un rato de charla.\n\n":
                r = "Around the campfire, some civilians and heroes share simple stories, hot food, and a while of conversation.\n\n"; break;
            case "No soluciona nada, pero por unas horas la Caravana vuelve a sentirse un poco más unida.\n\n":
                r = "It solves nothing, but for a few hours the Caravan feels a little more united again.\n\n"; break;
            case "<color=#a0e812><b>+6 Esperanza</b></color>":
                r = "<color=#a0e812><b>+6 Hope</b></color>"; break;
            case "Antes de dormir, un grupo de civiles se ofrece a ayudar con tareas atrasadas del campamento.\n\n":
                r = "Before sleeping, a group of civilians offers to help with overdue camp tasks.\n\n"; break;
            case "Puedes organizar una pequeña ronda de reparaciones o agradecer el gesto y dejarlos descansar.\n\n":
                r = "You can organize a small round of repairs or thank them and let them rest.\n\n"; break;
            case "<color=#a0e812>-Si decides organizar, la Caravana ganará 15 Materiales.</color>\n\n":
                r = "<color=#a0e812>-If you choose to organize them, the Caravan will gain 15 Materials.</color>\n\n"; break;
            case "<color=#a0e812>-Si decides dejarlos descansar, +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you choose to let them rest, +4 Hope.</color>\n\n"; break;
            case "El cansancio pesa, pero esta vez el campamento logra dormir sin interrupciones. Incluso quienes suelen despertarse con cualquier ruido descansan mejor.\n\n":
                r = "The fatigue is heavy, but this time the camp manages to sleep without interruptions. Even those who usually wake at any sound rest better.\n\n"; break;
            case "Al amanecer, el ánimo acompaña.\n\n":
                r = "At dawn, morale rises with everyone.\n\n"; break;
            case "<color=#a0e812><b>-1 Fatiga, +3 Esperanza</b></color>":
                r = "<color=#a0e812><b>-1 Fatigue, +3 Hope</b></color>"; break;
            case "Al ordenar los carros antes de partir, encuentran un pequeño lote de provisiones que había quedado mal inventariado.\n\n":
                r = "While sorting the carts before leaving, they find a small batch of provisions that had been inventoried incorrectly.\n\n"; break;
            case "No es mucho, pero alcanza para decidir entre guardarlo para el camino o repartirlo enseguida.\n\n":
                r = "It is not much, but enough to decide between saving it for the road or handing it out right away.\n\n"; break;
            case "<color=#a0e812>-Si decides guardarlo, +20 Suministros.</color>\n\n":
                r = "<color=#a0e812>-If you choose to store it, +20 Supplies.</color>\n\n"; break;
            case "<color=#a0e812>-Si decides repartirlo, +5 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you choose to hand it out, +5 Hope.</color>\n\n"; break;
            case "</color></b> se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n":
                r = "</color></b> woke several times with chills and discomfort. By dawn, they can barely stay on their feet.\n\n"; break;
            case "Uno de los Héroes se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n":
                r = "One of the Heroes woke several times with chills and discomfort. By dawn, they can barely stay on their feet.\n\n"; break;
            case "<color=#ba3fef><b>Obtiene Enfermo por 3 días.</b></color>":
                r = "<color=#ba3fef><b>Gains Sick for 3 days.</b></color>"; break;
            case "</color></b> no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n":
                r = "</color></b> could not rest well because of a string of nightmares and sudden jolts awake.\n\n"; break;
            case "Uno de los Héroes no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n":
                r = "One of the Heroes could not rest well because of a string of nightmares and sudden jolts awake.\n\n"; break;
            case "Al amanecer se lo ve agotado y le cuesta seguir el ritmo del resto.\n\n":
                r = "At dawn they look exhausted and struggle to keep up with the others.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga.</b></color>":
                r = "<color=#ba3fef><b>+1 Fatigue.</b></color>"; break;
            case "</color></b> quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n":
                r = "</color></b> tried to use the rest to practice alone. One bad move ended in an unnecessary injury.\n\n"; break;
            case "Uno de los Héroes quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n":
                r = "One of the Heroes tried to use the rest to practice alone. One bad move ended in an unnecessary injury.\n\n"; break;
            case "<color=#ba3fef><b>Obtiene Herida.</b></color>":
                r = "<color=#ba3fef><b>Gains Injury.</b></color>"; break;
            case "</color></b> encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n":
                r = "</color></b> found a small pouch caught between blankets and ropes while rearranging the carts.\n\n"; break;
            case "Uno de los Héroes encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n":
                r = "One of the Heroes found a small pouch caught between blankets and ropes while rearranging the carts.\n\n"; break;
            case "Dentro había un consumible todavía intacto, olvidado desde hace quién sabe cuánto.\n\n":
                r = "Inside there was a still-intact consumable, forgotten for who knows how long.\n\n"; break;
            case "<color=#a0e812><b>Obtienes 1 consumible.</b></color>":
                r = "<color=#a0e812><b>You gain 1 consumable.</b></color>"; break;
            case "</color></b> pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n":
                r = "</color></b> spent much of the rest going over the road's mistakes and successes by the fire.\n\n"; break;
            case "Uno de los Héroes pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n":
                r = "One of the Heroes spent much of the rest going over the road's mistakes and successes by the fire.\n\n"; break;
            case "La charla termina dándole una idea útil para lo que venga.\n\n":
                r = "The talk ends up giving them a useful idea for what comes next.\n\n"; break;
            case "<color=#a0e812><b>Gana 45 Experiencia.</b></color>":
                r = "<color=#a0e812><b>Gains 45 Experience.</b></color>"; break;
            case "Antes de dormir, varios Civiles se acercan a <b><color=#d1006f>":
                r = "Before going to sleep, several Civilians approach <b><color=#d1006f>"; break;
            case "</color></b> para agradecerle por lo que viene haciendo.\n\n":
                r = "</color></b> to thank them for what they have been doing.\n\n"; break;
            case "Antes de dormir, varios Civiles se acercan a uno de los Héroes para agradecerle por lo que viene haciendo.\n\n":
                r = "Before going to sleep, several Civilians approach one of the Heroes to thank them for what they have been doing.\n\n"; break;
            case "No cambia el camino, pero sí la forma en que piensa enfrentarlo al día siguiente.\n\n":
                r = "It does not change the road ahead, but it does change how they plan to face it the next day.\n\n"; break;
            case "<color=#a0e812><b>Obtiene Alta Moral por 4 días.</b></color>":
                r = "<color=#a0e812><b>Gains High Morale for 4 days.</b></color>"; break;
            case "El viento nocturno arrastra brasas encendidas desde los árboles caídos y obliga a mover parte del campamento una y otra vez.\n\n":
                r = "The night wind drags glowing embers from fallen trees and forces part of the camp to be moved again and again.\n\n"; break;
            case "Nadie duerme del todo tranquilo en el Bosque Ardiente.\n\n":
                r = "No one sleeps fully at ease in the Burning Forest.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga, -4 Esperanza</b></color>":
                r = "<color=#ba3fef><b>+1 Fatigue, -4 Hope</b></color>"; break;
            case "Ya entrada la noche, un tronco que parecía apagado vuelve a encenderse cerca de los carros.\n\n":
                r = "Well into the night, a log that seemed spent catches fire again near the carts.\n\n"; break;
            case "Logran contenerlo antes de que pase a mayores, pero se consumen recursos en el apuro.\n\n":
                r = "They manage to contain it before it gets worse, but resources are spent in the rush.\n\n"; break;
            case "<color=#ba3fef><b>-12 Suministros</b></color>":
                r = "<color=#ba3fef><b>-12 Supplies</b></color>"; break;
            case "El suelo todavía guarda un calor tenue bajo la ceniza y por una vez el descanso no se siente hostil.\n\n":
                r = "The ground still keeps a faint warmth under the ash, and for once the rest does not feel hostile.\n\n"; break;
            case "El campamento logra dormir mejor de lo esperado.\n\n":
                r = "The camp manages to sleep better than expected.\n\n"; break;
            case "Entre raíces chamuscadas y troncos huecos, algunos Civiles encuentran hongos resistentes al calor todavía aprovechables.\n\n":
                r = "Among scorched roots and hollow trunks, some Civilians find heat-resistant mushrooms that are still usable.\n\n"; break;
            case "No es un gran banquete, pero alcanza para reforzar las reservas antes de partir.\n\n":
                r = "It is no grand feast, but it is enough to reinforce the reserves before leaving.\n\n"; break;
            case "<color=#a0e812><b>+18 Suministros</b></color>":
                r = "<color=#a0e812><b>+18 Supplies</b></color>"; break;
            case "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del puerto.\n":
                r = "During the night, the gathered civilians spot a clear and beautiful flash of light on the horizon towards the port.\n"; break;
            case "Quizás sea una señal, quizás casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n":
                r = "Perhaps it is a sign, perhaps coincidence, but the civilians now seem more optimistic, even though there is still a long way to go.\n\n\n\n\n\n\n"; break;
            case "La atmósfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n":
                r = "The atmosphere becomes lighter and more optimistic, and for a brief moment, the weight of the situation seems to fade away.\n\n\n\n"; break;
            case "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>":
                r = "<color=#a0e812><b>+5 Hope</b>\n\n</color>"; break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadoveres civiles. Una lucha tuvo lugar aquí y esta caravana no sobrevivió.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n"; break;
            case "Si bien la situación es sombría, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n":
                r = "Although the situation is bleak, several supplies in good condition were not looted, remaining on the side of the road.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n":
                r = "<color=#ba3fef>-You can bury the Civilians and honor their memory, without looting them.</color> +15 Hope \n\n"; break;
            case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
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
            case " superó su Tirada de Salvación de Reflejos (1d20: ":
                r = " passed their Reflex Save (1d20: "; break;
            case " superó su Tirada de Salvación Mental (1d20: ":
                r = " passed their Mental Save (1d20: "; break;
            case " superó su Tirada de Salvación de Fortaleza (1d20: ":
                r = " passed their Fortitude Save (1d20: "; break;
            case " vs DC ":
                r = " vs DC "; break;
            case ") y logró contener a las bestias aterradas. +40 Experiencia.":
                r = ") and managed to hold back the panicked beasts. +40 Experience."; break;
            case " falló su Tirada de Salvación de Reflejos (1d20: ":
                r = " failed their Reflex Save (1d20: "; break;
            case "), y ha sufrido una herida.":
                r = "), and has suffered an injury."; break;
            case "). Las bestias aterradas desataron el caos y la Caravana perdió 2 Bueyes.":
                r = "). The panicked beasts unleashed chaos and the Caravan lost 2 Oxen."; break;
            case "-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza":
                r = "-You gave a motivational speech and refuted the Noble's arguments. +15 Hope"; break;
            case "-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza.":
                r = "-You gave an unconvincing speech that raised more doubts than certainties. -20 Hope."; break;
            case "-La cacería de ":
                r = "-The hunt by "; break;
            case " ha sido exitosa. +":
                r = " was successful. +"; break;
            case " Suministros +55 Experiencia.":
                r = " Supplies +55 Experience."; break;
            case " sufrió un accidente durante la cacería. Herido.":
                r = " suffered an accident during the hunt. Wounded."; break;
            case "-Los Civiles se han contaminado y han muerto ":
                r = "-The Civilians were contaminated and "; break;
            case " Civiles. -10 Esperanza":
                r = " Civilians. -10 Hope"; break;
            case "-Los Civiles han descansado en el río y se han refrescado. -1 Fatiga ":
                r = "-The Civilians rested by the river and cooled off. -1 Fatigue "; break;
            // Riña description segments
            case "Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>":
                r = "You hear a commotion near the Heroes' wagons. As you approach to investigate, you see <b><color=#d1006f>"; break;
            case "</color></b> y <b><color=#d1006f>":
                r = "</color></b> and <b><color=#d1006f>"; break;
            case "</color></b> discutiendo acaloradamente.":
                r = "</color></b> arguing heatedly."; break;
            case "Río Contaminado":
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
            case "Nieve: todas las unidades obtienen 'Frío'.":
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
            case "Has llegado a un improvisado Puesto Comercial, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\nTu Séquito de Mercaderes ha actualizado su Inventario.":
                r = "You have arrived at an improvised Trading Post, they offer basic survival Supplies to travelers.\nThe Tier of your Merchant Retinue will help lower prices.\n\nYour Merchant Retinue has updated its inventory.";
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
            case "Si descansas volverá a 0 y arrancarán el nuevo día Descansados(1).\n\n":
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
            case "Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podrán morir si viajas.":
                r = "Currently <color=#ffd19e>Agitated</color>(<color=#ffd19e>4</color>), -10 Hope and few Oxen may die if you travel.";
                break;
            case "Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas.":
                r = "Currently <color=#ff9e9e>Tired</color>(<color=#ff9e9e>5</color>), -15 Hope and some Oxen may die if you travel.";
                break;
            case "Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas.":
                r = "Currently <color=#ff3c3c>Exhausted</color>(<color=#ff3c3c>6</color>), -20 Hope and several Oxen may die if you travel.";
                break;
            case "Día ":
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
            case "Luego de buscar vagamente en la cercanía y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n":
                r = "After vaguely searching the area and concluding that there are no clues, you decide to comfort the relatives and move on.\n\n\n\n\n\n\n";
                break;
            case "<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>You lose 4-12 Civilians, -5 Hope.</b></color>";
                break;
            case "Uno de los bueyes de la caravana ha caído enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n":
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
            case " ganan Alta Moral por 3 días.</b></color>":
                r = " gain High Morale for 3 days.</b></color>";
                break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí y esta caravana no sobrevivió.\n":
                r = "As you move along the road, you find several destroyed wagons surrounded by civilian corpses. A fight took place here and this caravan did not survive.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n":
                r = "<color=#ba3fef>-You can order the Caravan to loot the Supplies.</color> +21-35 Supplies, +5-11 Materials, +15-35 Gold, -5 Hope.</i> \n\n";
                break;
            /*  case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
                  r = "The Caravan stops at an abandoned sawmill, some trees have been felled and the wood is piled up in disarray.\n";
                  break;*/
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
            case "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un símbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ":
                r = "With his other outstretched hand, he holds a bag of gold and offers it to you kindly. -'Consider it a symbol of my trust in you, as well as a contribution that may be useful for the Caravan.'-he says\n ";
                break;
            case "<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: ":
                r = "<color=#ba3fef>You answer: -'Keep the money, your contribution to the Caravan is already considerable with your daily effort, and I am more than grateful to be able to count on you.'</color> Effects: ";
                break;
            case " gana Alta Moral por 4 días y 50 Experiencia. \n\n":
                r = " gains High Morale for 4 days and 50 Experience. \n\n";
                break;
            case "<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n":
                r = "<color=#ba3fef>You respond: -'I accept your offer, there is no money to spare in our current situation and it will surely help us during the journey, thank you.'</color> Effects: +120-160 Gold. \n\n";
                break;
            case "Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchísimos años en la zona y la conoce a la perfección.\n":
                r = "An old man appears at the side of the road waving his hands at the Caravan. Up close, you realize that this man has been living in the area for many years and knows it perfectly.\n";
                break;
            case "'Aliento Negro o no, mis días ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n":
                r = "'Black Breath or not, my days are already numbered. But I can share my knowledge about this land, as a final act of kindness.'- he says\n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'¿Conoce algún atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n":
                r = "<color=#ba3fef>Question: -'Do you know of any shortcut that can take us away from imminent danger for at least a few miles?'</color> Effects: If possible, a subterranean shortcut will be generated. \n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n":
                r = "<color=#ba3fef>Question: -'Describe the surrounding area so we can make more informed decisions.'</color> Effects: Upcoming nodes will be revealed. \n\n";
                break;
            case "</color></b> se lo ve con mucha energía y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n":
                r = "</color></b> looks very energetic and determined as he goes about his usual tasks. When you approach him, he tells you that he had a Dream in which he saw the Caravan reaching its destination.\n";
                break;
            case "'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n":
                r = "'In the dream, I saw a clear path to our destination. There will be dangers and difficulties, but I am convinced that we will make it. Let's follow that route.'- he says with Determination\n\n\n";
                break;
            case "</color></b> obtiene 150 Experiencia y Alta Moral por 5 días.</color>\n\n":
                r = "</color></b> gains 150 Experience and High Morale for 5 days.</color>\n\n";
                break;
            case "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mínimo.\n":
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
            case "\nSe conseguirán de 18-30 Materiales y 70-110 Suministros.":
                r = "\n18-30 Materials and 70-110 Supplies will be gathered.";
                break;
            case "<color=#a0e812><b>\n\nDescansar en este lugar tendrá beneficios adicionales: +20% efectividad a tareas de Recolección.</b></color>":
                r = "<color=#a0e812><b>\n\nResting in this place will have additional benefits: +20% effectiveness on Gathering tasks.</b></color>";
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
            case "Menu de Descanso":
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
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3, todos los personajes obtendrán Bendecido por 3 días y un personaje con Corrupción al azar será curado.":
                r = "Upon making the offering, the Black Breath will recede by 3, all characters will gain Blessed for 3 days, and a random character with Corruption will be healed.";
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
            case "Defensas: Cada Tier mejora las defensas de la Caravana en ataques directos y reduce 10% las chances de perder un Séquito.":
                r = "Defenses: Each Tier improves the Caravan's defenses in direct attacks and reduces the chances of losing a Retinue by 10%.";
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
            case "<i>\"Debemos dejar en alto llamas de esperanza que sirvan de guía para aquellas caravanas perdidas en el camino.\"</i> \n\nCada Tier de las Almenaras otorgará un bonus de 5 <b>Esperanza</b> cada vez que una caravana comience a viajar por una región nueva. \n\nAdemás, cada Tier dará <b>1 stack</b> de una mejora de Caravana al azar al comenzar una región nueva. Si otorga varios por Tier, siempre serán de la misma mejora. \n\nAdemás, cada Tier dará <b>+3%</b> chances de Exploración para las caravanas futuras.\n\n\n\n\n ":
                r = "<i>\"We must keep flames of hope burning high to guide those caravans lost along the road.\"</i> \n\nEach Tier of the Beacons grants a bonus of 5 <b>Hope</b> whenever a caravan begins traveling through a new region. \n\nIn addition, each Tier grants <b>1 stack</b> of a random Caravan boon at the start of a new region. If it grants several stacks by Tier, they will always be of the same boon. \n\nAlso, each Tier grants <b>+3%</b> Exploration chance for future caravans.\n\n\n\n\n ";
                break;
            case "Carro Almacén: Cada Tier reduce 5% Suministros consumidos por Descanso.":
                r = "Supply Wagon: Each Tier reduces 5% supplies consumed by Resting.";
                break;
            case "Planes de mejoras":
                r = "Improvement plans";
                break;
            case "  Resistencias":
                r = "  Resistances";
                break;
            case "Rasgos":
                r = "Traits";
                break;
            case "Mejora de atributo disponible":
                r = "Attribute upgrade available";
                break;
            case "Mejora de salvación disponible":
                r = "Saving throw upgrade available";
                break;
            case "Mejora de habilidad disponible":
                r = "Skill upgrade available";
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
            case "Selecciona a tus personajes:":
                r = "Select your characters:";
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
            case "-Es un día hermoso. +5 Esperanza.":
                r = "It's a beautiful day. +5 Hope.";
                break;
            case "-La Ola de Calor se hace insoportable. +1 Fatiga.":
                r = "The Heat Wave becomes unbearable. +1 Fatigue.";
                break;
            case "-La Lluvia hace el viaje más difícil. -5 Esperanza.":
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
            case "-El tener que trabajar en plena Ola de Calor, ha caído mal en los Civiles. -3 Esperanza":
                r = "-Having to work in the middle of the Heat Wave has not gone well with the Civilians. -3 Hope";
                break;
            case "-El tener un Día Libre en plena Ola de Calor, ha caído bien en los Civiles. +5 Esperanza":
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
            case "Durante el descanso, se asignarán a los civiles mas aptos físicamente a la vigilancia del area circundante al campamento.\n\n":
                r = "During rest, the most physically fit civilians will be assigned to surveil the area surrounding the camp.\n\n";
                break;
            case "<color=#d8a205>Reduce chances de ataque a caravana. +20% a Exploración. -10 Esperanza.</color>\n\n\n":
                r = "<color=#d8a205>Reduces chances of Ambush during this rest. +20% to Exploration. -10 Hope.</color>\n\n\n";
                break;
            case "<b><u>Día Libre</b></u>\n\n\n":
                r = "<b><u>Day Off</b></u>\n\n\n";
                break;
            case "Los civiles se tomarán el día para descansar y recobrar fuerzas.\n\n":
                r = "The civilians will take the day to rest and regain strength.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá 10 de Esperanza y el día siguiente arrancará con -1 Fatiga. +10% Curación a personajes.</color>\n\n\n":
                r = "<color=#d8a205>You will gain 10 Hope and the next day will start with -1 Fatigue. +10% Character healing.</color>\n\n\n";
                break;
            case "<b><u>Feria</b></u>\n\n\n":
                r = "<b><u>Fair</b></u>\n\n\n";
                break;
            case "Los civiles dedicarán el día a organizar una feria con varios juegos y celebraciones.\n\n":
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
            case "Nivel: ":
                r = "Level: ";
                break;
            case "Exp: ":
                r = "EXP: ";
                break;
            case "Valentía: ":
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
            case "Bendecido: +3 TS +5 Res.Necro.</color>":
                r = "Blessed: +3 Saves +5 Necro Res.</color>";
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
            case " días. -15% daño, -3 TS Fortaleza, -1 PA </color>":
                r = " days. -15% damage, -3 Fortitude, -1 AP </color>";
                break;
            case "<color=#d80404>\n\nBaja Moral por ":
                r = "<color=#d80404>\n\nLow Morale for ";
                break;
            case " días. -1 Ataque y Defensa, -3 TS Mental, -2 Valentía Inicial</color>":
                r = " days. -1 Attack and Defense, -3 TS Mental, -2 Initial Valour</color>";
                break;
            case "<color=#d80404>\n\nAlta Moral por ":
                r = "<color=#d80404>\n\nHigh Morale for ";
                break;
            case " días. +1 Ataque, +2 TS Mental, +2 Valentía Inicial</color>":
                r = " days. +1 Attack, +2 TS Mental, +2 Initial Valour</color>";
                break;
            case "Torpe: +1 Rango Pifias. ":
                r = "Clumsy: +1 Fumble Range.";
                break;
            case "Valiente: +2 Valentía Máxima.":
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
            case "<color=#0cca74><b>Coerción: </b></color><color=#d3d3d3><i>Con métodos cuestionables, el Acechador obliga a los Mercaderes a donar dinero a la caravana.</color></i>\\n\\n+1-10 Oro y -1 Esperanza por día.":
                r = "<color=#0cca74><b>Coercion: </b></color><color=#d3d3d3><i>Using questionable methods, the Stalker forces Merchants to donate money to the caravan.</color></i>\\n\\n+1-10 Gold and -1 Hope per day.";
                break;
            case "<color=#0cca74><b>Exploración: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo Subterráneo\\nSi se da un combate, lo arranca Fatigado.":
                r = "<color=#0cca74><b>Scouting: </b></color><color=#d3d3d3><i>The character scouts possible destinations ahead of the caravan.</color></i>\\n\\nHas a 40% chance to reveal future Nodes when traveling to a new Node. -5% Chance of Mysterious Node. +5% Chance of Underground Shortcut\\nIf a combat occurs, they start Fatigued.";
                break;
            case "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirá su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrá +3 Flechas y +5% daño.":
                r = "<color=#0cca74><b>Prepare Arrows: </b></color><color=#d3d3d3><i>The character will spend their time creating and improving their arrows.</color></i>\\n\\nIf a combat occurs, they will have +3 Arrows and +5% damage.";
                break;
            case "<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocupará de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzará con +2 Armadura.":
                r = "<color=#0cca74><b>Armor Maintenance: </b></color><color=#d3d3d3><i>The character will take care of his armor.</color></i>\\n\\nIf a combat occurs, he will start with +2 Armor.";
                break;
            case "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerá vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrá participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daño los primeros 2 turnos.":
                r = "<color=#0cca74><b>Watch: </b></color><color=#d3d3d3><i>The character will remain vigilant against any danger.</color></i>\\n\\nIf an ambush occurs, they can actively participate in the defense and gain +2 AP, +5 Initiative, and +20% damage for the first 2 turns.";
                break;
            case "<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizará su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada día que pase ganará 15 Experiencia.\\nSi se produce un combate, lo arrancará Fatigado.":
                r = "<color=#0cca74><b>Train: </b></color><color=#d3d3d3><i>The character will use their free time to train and stay in shape.</color></i>\\n\\nEach day that passes, they will gain 15 Experience.\\nIf a combat occurs, they will start Fatigued.";
                break;
            case "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nCada día que pase recuperará un 15% de salud.\\nSi se produce un combate, lo arrancará Fresco.":
                r = "<color=#0cca74><b>Rest: </b></color><color=#d3d3d3><i>The character will focus on resting and recovering their health.</color></i>\\n\\nEach day that passes, they will recover 15% of their health.\\nIf a combat occurs, they will start Fresh.";
                break;
            case "<color=#0cca74><b>Afilar Armas: </b></color><color=#d3d3d3><i>El Acechador se encarga de mantener sus armas afiladas.</color></i>\\n\\nSi se produce un combate tendrá +10% daño.":
                r = "<color=#0cca74><b>Prepare Weapons: </b></color><color=#d3d3d3><i>The Stalker is responsible for keeping his weapons sharp.</color></i>\\n\\nIf a combat occurs, he will have +10% damage.";
                break;
            case "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.":
                r = "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>With his arcane telekinesis powers, he helps with the caravan's load.</color></i>\\n\\n+20 Carrying Capacity.";
                break;
            case "<color=#0cca74><b>Caza Nocturna: </b></color><color=#d3d3d3><i>El personaje cazará en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+1d4 Suministros por día. +3% probabilidad de Emboscada Enemiga al descansar.":
                r = "<color=#0cca74><b>Night Hunting: </b></color><color=#d3d3d3><i>The character will hunt in the vicinity to obtain food for the caravan.</color></i>\\n\\n+1d4 Supplies per day. +3% chance of Enemy Ambush while resting.";
                break;
            case "<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirá los relatos de sus hazañas con quienes quieran oírlas.</color></i>\\n\\n+10 Experiencia por día a personajes de nivel inferior. +4 Esperanza al descansar.":
                r = "<color=#0cca74><b>Battle Tales: </b></color><color=#d3d3d3><i>The character will share the tales of their exploits with those who wish to hear them.</color></i>\\n\\n+10 Experience per day to lower-level characters. +4 Hope while resting.";
                break;
            case "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizará rituales de protección para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por día.":
                r = "<color=#0cca74><b>Ritual of Cleansing: </b></color><color=#d3d3d3><i>The Purifier will perform protection rituals to combat the Black Breath.</color></i>\\n\\nChance to avoid Black Breath advance: 25% while resting, 15% per day.";
                break;
            case "<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usará su tiempo para ayudar a los rezagados y más débiles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.":
                r = "<color=#0cca74><b>Help the Hopeless: </b></color><color=#d3d3d3><i>The Purifier will use her time to help the laggards and weaker members of the caravan.</color></i>\\n\\n+1d3 Hope per day. +1 Fervor in combat.";
                break;
            case "<color=#0cca74><b>Concentración Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de Energía al iniciar combates.":
                r = "<color=#0cca74><b>Arcane Concentration: </b></color><color=#d3d3d3><i>The Channeler focuses and keeps their power ready for any combat that arises.</color></i>\\n\\n+1 Energy Level at the start of combats.";
                break;
            case "<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.\\nEn Ataque a Caravana cuenta como Guardia y comienza en Sigilo.":
                r = "<color=#0cca74><b>Watch from Shadows: </b></color><color=#d3d3d3><i>The Stalker moves stealthily around the caravan, trying to anticipate enemy ambushes.</color></i>\\n\\n-5% chance of ambushes.\\nIn Caravan Attack it counts as Guard and starts Hidden.";
                break;
            case "<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>Séquito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curación diaria del Séquito de Curanderos.":
                r = "<color=#0cca74><b>Help the Healers' Retinue: </b></color><color=#d3d3d3><i>Helps the <b>Healers' Retinue</b> in their tasks, increasing their effectiveness.</color></i>\\n\\nIncreases the Healers' Retinue's daily healing by 5%.";
                break;
            case "<color=#0cca74><b>Crear Símbolo Arcano de Protección: </b></color><color=#d3d3d3><i>El Canalizador concentra energía arcana protectora en un símbolo que puede proteger a quien lo utilice.</color></i>\\n\\nCrea un Símbolo Arcano de Protección por día.":
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
            case "-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -7 Esperanza":
                r = "-The strong presence of Black Breath in the air causes fear in the Caravan. -7 Hope";
                break;
            case "-La presencia de Aliento Negro en el aire es fatal para los Civiles. -10 Esperanza -":
                r = "-The Black Breath in the air is fatal for Civilians. -10 Hope -";
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
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3, se ha gastado 200 de oro y todos los personajes obtienen Bendecido por 3 días.":
                r = "-You have performed a ritual in the sanctuary. The Black Breath recedes by 3, 200 gold has been spent, and all characters gain Blessed for 3 days.";
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
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3, se han sacrificado 3 bueyes y todos los personajes obtienen Bendecido por 3 días.":
                r = "-You have performed a ritual in the sanctuary. The Black Breath recedes by 3, 3 oxen have been sacrificed, and all characters gain Blessed for 3 days.";
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
            case "-Consuelo reduce la pérdida de Esperanza en ":
                r = "-Solace reduces the Hope loss by ";
                break;
            case " socializa con la caravana. Beneficiados: ":
                r = " socializes with the caravan. High Morale: ";
                break;
            case "nadie":
                r = "no one";
                break;
            case " socializa con la caravana. Sus compañeros realizan una TS Mental DC ":
                r = " socializes with the caravan. Their companions make a Mental Save DC ";
                break;
            case " supera la TS Mental (1d20: ":
                r = " succeeds on the Mental Save (1d20: ";
                break;
            case " falla la TS Mental (1d20: ":
                r = " fails the Mental Save (1d20: ";
                break;
            case ") gracias a <b>Socializar</b> y obtiene Alta Moral por 1 día.":
                r = ") thanks to <b>Socialize</b> and gains High Morale for 1 day.";
                break;
            case ") pese a <b>Socializar</b> y no obtiene Alta Moral.":
                r = ") despite <b>Socialize</b> and does not gain High Morale.";
                break;
            case " de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza":
                r = " Gold from the Caravan Merchants, who were coerced into donating to the cause. -1 Hope";
                break;
            case " ha creado un Símbolo de Protección Arcano.":
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
            case "Derrota en un encuentro clásico. Los efectos específicos aún no están configurados.":
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
            case " es escondido en las sombras tras recibir un ataque crítico por su Armadura de Velo.":
                r = " is hidden in the shadows after receiving a critical hit from its Cloak Armor.";
                break;
            case "Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periódicamente parte de su riqueza para asegurarse de que no serán abandonados.\n\n":
                r = "A group of nobles forced to leave the comfort of their lands now travels with the caravan. They complain and offer little help, but they donate part of their wealth to ensure they are not abandoned.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Cada día donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.":
                r = "PASSIVE EFFECTS:\n\n-Each day they donate Gold equal to 1/3 of Hope.\n\n-Traveling with fatigue 4+ loses 2 Hope.";
                break;
            case "Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayoría murieron en la onda expansiva en ese momento, pero todaví­a quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n":
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
            case "Mantenimiento Armas: El Herrero se encargará de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daño en 2. Este efecto Dura 3 días.":
                r = "Weapon Maintenance: The Blacksmith will take care of general maintenance of the characters' weapons. Increasing their Attack by 1 and their damage by 2. This effect lasts 3 days.";
                break;
            case "Mantenimiento Armaduras: El Herrero se encargará de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 días.":
                r = "Armor Maintenance: The Blacksmith will take care of general maintenance of the characters' armor. Increasing their Defense by 1 and their Armor by 2. This effect lasts 3 days.";
                break;
            case "Realizar: 200 Oro":
                r = "Perform: 200 Gold";
                break;
            case "Activo por ":
                r = "Active for ";
                break;
            case " Días":
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
            case "% la curación pasiva de la Caravana.\n\nEste índice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.":
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
            case "Este séquito está constituído por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
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
            case "EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada día hay un 30% de chances de que hagan un festán y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.":
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
            case "<b>¡Enfurecido!</b>":
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
            case "Frío":
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
            case "Bendecido":
                r = "Blessed";
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
            case "Atento":
                r = "Alert";
                break;
            case "Terminó turno con AP disponible, aumenta defensa vs próximo golpe.":
                r = "Ended turn with AP available, increases defense against the next attack.";
                break;
            case "Se consume al recibir el próximo ataque.":
                r = "Consumed upon receiving the next attack.";
                break;
            case "Jefe":
                r = "Boss";
                break;
            case "Planta":
                r = "Plant";
                break;
            case "Fey":
                r = "Fey";
                break;
            case "Nomuerto":
                r = "Undead";
                break;
            case "Etereo":
                r = "Ethereal";
                break;
            case "Animal":
                r = "Animal";
                break;
            case "Humanoide":
                r = "Humanoid";
                break;
            case "Constructo":
                r = "Construct";
                break;
            case "Criatura":
                r = "Creature";
                break;
            case "Corrupto":
                r = "Corrupted";
                break;
            case "Bestia":
                r = "Beast";
                break;
            case "Volador":
                r = "Flying";
                break;
            case "Gigante":
                r = "Giant";
                break;
            case "Kale'Tav":
                r = "Kale'Tav";
                break;
            case "Zarkil":
                r = "Zarkil";
                break;
            case "Demonio":
                r = "Demon";
                break;
            case "Dragon":
                r = "Dragon";
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
            /*case "Distraído":
                r = "Distracted";
                break;*/
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
            case " falló la Tirada de Concentración y ya no acumula energía.":
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
            case "Energía Absorbida":
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
            case "Elixir de Resistencia al Frío":
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
            case "Energía: Nivel de Energía Acumulada por el Canalizador.":
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
            case "Crítico Dado: ":
                r = "Critical Hit: ";
                break;
            case "Daño Crítico: ":
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
            case "Valentía Global Alta":
                r = "Global Valour High";
                break;
            case "Valentia Global Alta":
                r = "Global Valour High";
                break;
            case "Valentía Global Muy Alta":
                r = "Global Valour Very High";
                break;
            case "Valentia Global Muy Alta":
                r = "Global Valour Very High";
                break;
            case "Dudando":
                r = "Doubting";
                break;
            case "Tambaleando":
                r = "Staggered";
                break;
            case "Provocado":
                r = "Provoked";
                break;
            case "Adolorido":
                r = "Wounded";
                break;
            case "Provocado: solo puede usar acciones hostiles contra quien aplicó este estado.":
                r = "Provoked: can only use hostile actions against the unit that applied this status.";
                break;
            case "Vulnerabilidad Expuesta":
                r = "Exposed Vulnerability";
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
            case "Armadura: reduce el daño físico recibido.":
                r = "Armor: reduces physical damage taken.";
                break;
            case "Reflejos: resistencia a determinados efectos de ataques.":
                r = "Reflexes: resistance to certain attack effects.";
                break;
            case "Fortaleza: resistencia a efectos físicos.":
                r = "Fortitude: resistance to physical effects.";
                break;
            case "Mental: resistencia a efectos mentales.":
                r = "Mental: resistance to mental effects.";
                break;
            case "Valentía: moral general en combate.":
                r = "Valour: general moral in combat.";
                break;
            case "Resistencia al Fuego: Cantidad de daño que previene.":
                r = "Fire Resistance: Amount of damage it prevents.";
                break;
            case "Resistencia al Frío: Cantidad de daño que previene.":
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
            case "Símbolo de Protección Arcano":
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
            case "Aumenta la resistencia al frío en 5 por el combate.":
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
            case "<i>El Lobo Alfa Espectral es el líder de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>":
                r = "<i>The Alpha Spectral Wolf is the leader of the pack, possessing a stronger and more resilient build than the other wolves, though it is slightly less agile.</i>\n\n<color=#199F10>-Has the ability to howl to motivate other wolves.</color>\n<color=#EE0000>-If left alone, it will be unable to motivate anyone.</color>";
                break;
            case "Driada Quemada":
                r = "Burnt Dryad";
                break;
            case "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raíces ignífugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Once servants and caretakers of the forest, they are now manifestations of vengeance and hatred against any invader of the Burning Forest.</i>\n\n<color=#199F10>-Can entangle with fire-resistant roots.\n-Ranged attack.</color>\n<color=#EE0000>-Relatively weak.</color>";
                break;
            case "Espectro del Bosque":
                r = "Forest Specter";
                break;
            case "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques físicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad física momentáneamente al atacar.</color>":
                r = "<i>The Forest Specter is a restless soul trapped among the ashes of a scorched forest, its rage fueled by the destruction it could not prevent. Wandering and vengeful, it attacks those who dare to cross its charred land.</i>\n\n<color=#199F10>-Immune to physical attacks.\n-Can curse with Perdition.</color>\n<color=#EE0000>-Loses part of its physical immunity momentarily when attacking.</color>";
                break;
            case "Fuego Fatuo":
                r = "Will-o'-the-Wisp";
                break;
            case "<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guía a los incautos hacia la perdición, vengando la memoria del bosque caído.</i>\n\n<color=#199F10>-Resistente a ataques físicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>":
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
            case "<i>Constituido por pura energía arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques físicos.</color>":
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
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crítico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>":
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
            case "-Las Almas Danzantes guían a la caravana. +5 Esperanza":
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
            case "Escudado: 10% chances por stack de evitar un ataque físico. Al evitar uno, pierde un stack.":
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
            case "Arquero Vengador de Kadryn":
                r = "Archer Avenger of Kadryn";
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
            case "Estocada":
                r = "Thrust";
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
            case "Rayo Necrótico":
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
            case "<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia física es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>":
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
            case "Destruir Obstaculo":
                r = "Destroy Obstacle";
                break;
            case "Destruyes":
                r = "You destroy";
                break;
            case "Este obstaculo no puede ser destruido por tus unidades.":
                r = "This obstacle cannot be destroyed by your units.";
                break;
            case "Gasta 3 PA para destruir un obstaculo adyacente de tu mismo lado si lo permite. Termina tu turno.":
                r = "Spend 3 AP to destroy an adjacent obstacle on your side if allowed. Ends your turn.";
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
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverán.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
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
            case "Mirada de Masacre: al moverse aquí, Tirada de salvación mental CD 13 o se pierde el turno.":
                r = "Gaze of the Massacre: when moving here, Mental saving throw DC 13 or lose the turn.";
                break;
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
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
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que además motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>":
                r = "<i>Race of demonic creatures that invade Nedulkazan from below in search of sacrifices and gold. </i>\n\n<color=#199F10>-Stunning shout that also motivates allies.\n-Can see hidden units.\n-Can attack repeatedly.</color>\n<color=#EE0000></color>";
                break;
            case "Rayo Debilitador":
                r = "Weakening Ray";
                break;
            case "Debilitado":
                r = "Weakened";
                break;
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-Débil</color>":
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
            case "<i>Manifestación de la energía espectral del bosque. Desde su interior emana un fulgor fantasmal frío, como un espíritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrótico que condena a dos objetivos.</color>\n<color=#EE0000>-Inmóvil.</color>":
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
            case "<i>Estas criaturas aladas habitan en las regiones más frías del Paso. Son conocidas por ser muy territoriales y por su aliento gélido.</i>\n\n<color=#199F10>-Vuelo.\n-Aliento gélido en zona.\n-Regenera armadura.</color>\n<color=#EE0000>-Débil al fuego.</color>":
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
            case "No hay suficientes energía":
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
            case "Crítico":
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
            case "<color=#FF6666>No puedes descansar aquí.</color>":
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
            case "Intercambiar":
                r = "Swap";
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
            case "Finalmente la caravana ha llegado a la Ciudad Puerto de Serria, donde la población civil se prepara para embarcar y así escapar del Aliento Negro.":
                r = "At last, the caravan has arrived at the Port City of Serria, where the civilian population is preparing to embark and escape the Black Breath.";
                break;
            case "El viaje ha durado ":
                r = "This trip lasted ";
                break;
            case " días enteros y han sobrevivido ":
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
            case "Cuando el campamento ya está armado, un comerciante rezagado se acerca a la Caravana con una mula cargada y una sonrisa cansada.\n\n":
                r = "Once the camp is set up, a straggling merchant approaches the Caravan with a loaded mule and a tired smile.\n\n";
                break;
            case "Dice que viene siguiendo el rastro del convoy desde hace días y que, si lo dejas instalarse un rato, puede abrir un pequeño puesto antes de seguir su camino.\n\n":
                r = "He says he has been following the convoy's trail for days and that, if you let him settle in for a while, he can open a small stall before moving on.\n\n";
                break;
            case "<color=#a0e812><b>Al continuar, se abrirá un Puesto Comercial.</b></color>":
                r = "<color=#a0e812><b>Continuing will open a Trading Post.</b></color>";
                break;
            case "Ya entrada la noche, una figura se acerca al campamento con las manos a la vista y el equipo a cuestas.\n\n":
                r = "Late into the night, a figure approaches the camp with their hands visible and their gear slung over their shoulder.\n\n";
                break;
            case "Cuenta que perdió a su grupo en el camino y pide un lugar en la Caravana. No promete milagros, pero sí pelear mientras le queden fuerzas.\n\n":
                r = "They say they lost their group on the road and ask for a place in the Caravan. They promise no miracles, only to fight while they still have strength.\n\n";
                break;
            case "<color=#a0e812>-Si decides aceptar, un Héroe aleatorio se unirá a la Caravana.</color>\n\n":
                r = "<color=#a0e812>-If you accept, a random Hero will join the Caravan.</color>\n\n";
                break;
            case "<color=#d6d6d6>-Si decides rechazar, seguirá su camino por su cuenta.</color>":
                r = "<color=#d6d6d6>-If you reject them, they will continue on their own.</color>";
                break;
            case "En mitad del descanso, un ave desciende sobre uno de los carros con un mensaje atado a la pata.\n\n":
                r = "In the middle of the rest, a bird descends onto one of the wagons with a message tied to its leg.\n\n";
                break;
            case "La nota viene de Serria: han enviado una misión de salvamento para asistir a la Caravana y marcan un punto de encuentro más adelante en el camino.\n\n":
                r = "The note comes from Serria: they have sent a rescue mission to assist the Caravan and marked a rendezvous point farther up the road.\n\n";
                break;
            case "<color=#a0e812><b>Al continuar, se marcará una Misión de Salvamento en el mapa.</b></color>":
                r = "<color=#a0e812><b>Continuing will mark a Rescue Mission on the map.</b></color>";
                break;
            case "Entre la niebla y el viento se cuela un ritmo de tambores que nadie logra ubicar con claridad.\n\n":
                r = "Through the mist and the wind comes the beat of drums that no one can place with certainty.\n\n";
                break;
            case "Los Civiles miran alrededor con inquietud. Puedes forzar a la Caravana a apurar el paso o frenar un momento hasta recuperar la calma.\n\n":
                r = "The Civilians look around uneasily. You can force the Caravan to quicken the march or stop for a moment until everyone regains their composure.\n\n";
                break;
            case "<color=#ba3fef>-Si decides apurar el paso, el esfuerzo dejará a la Caravana más cansada. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you hurry the march, the effort will leave the Caravan more tired. +1 Fatigue.</color>\n\n";
                break;
            case "Un tramo helado del camino cruje bajo el peso de la Caravana. ":
                r = "A frozen stretch of road creaks under the weight of the Caravan. ";
                break;
            case "</color></b> puede intentar guiar el cruce antes de que el hielo ceda.\n\n":
                r = "</color></b> can try to guide the crossing before the ice gives way.\n\n";
                break;
            case "<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC 12 (TS Reflejos actual: ":
                r = "<color=#ba3fef>-Saving Throw: Reflex Save DC 12 (current Reflex Save: ";
                break;
            case "). Si supera la tirada, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "). On success, they gain 40 Experience. On failure, they suffer an Injury.</color>\n\n";
                break;
            case "Un tramo helado del camino cruje bajo el peso de la Caravana. Uno de los Héroes puede intentar guiar el cruce antes de que el hielo ceda.\n\n":
                r = "A frozen stretch of road creaks under the weight of the Caravan. One of the Heroes can try to guide the crossing before the ice gives way.\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "<color=#ba3fef>-If you try it, they will make a Reflex Save DC 12. On success, they gain 40 Experience. On failure, they suffer an Injury.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides rodear el tramo, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-If you go around the stretch, the Black Breath will advance.</color>\n\n";
                break;
            case "A los costados del camino aparecen varias efigies Kale'Tav clavadas en la nieve, adornadas con huesos, plumas y telas endurecidas por el hielo.\n\n":
                r = "Along the road stand several Kale'Tav effigies planted in the snow, decorated with bones, feathers, and cloth stiffened by ice.\n\n";
                break;
            case "Aunque nadie se acerque, su sola presencia alcanza para inquietar a la Caravana.\n\n":
                r = "Even without anyone approaching, their mere presence is enough to unsettle the Caravan.\n\n";
                break;
            case "<color=#ba3fef><b>-6 Esperanza</b></color>":
                r = "<color=#ba3fef><b>-6 Hope</b></color>";
                break;
            case "Figuras encapuchadas se recortan un instante entre las rocas y luego desaparecen. No hace falta ver más para entender que una partida de caza Kale'Tav anda cerca.\n\n":
                r = "Hooded figures appear for a moment between the rocks and then vanish. No more is needed to know a Kale'Tav hunting party is nearby.\n\n";
                break;
            case "Puedes preparar a los Héroes para un enfrentamiento o esconder a los Civiles y perder tiempo en el desorden.\n\n":
                r = "You can ready the Heroes for a fight or hide the Civilians and lose time in the confusion.\n\n";
                break;
            case "<color=#ba3fef>-Si decides prepararte, comenzará una batalla.</color>\n\n":
                r = "<color=#ba3fef>-If you prepare, a battle will begin.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides esconder a los Civiles, el miedo y el desorden dejarán secuelas. +1 Fatiga, -3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you hide the Civilians, fear and confusion will leave their mark. +1 Fatigue, -3 Hope.</color>\n\n";
                break;
            case " soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n":
                r = " endures the march as best they can, but the cold of the Pass sinks deeper than expected.\n\n";
                break;
            case "Uno de los Héroes soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n":
                r = "One of the Heroes endures the march as best they can, but the cold of the Pass sinks deeper than expected.\n\n";
                break;
            case "La Caravana encuentra un tótem recién erigido, cubierto con sangre seca y cintas agitadas por el viento.\n\n":
                r = "The Caravan comes across a freshly raised totem, covered in dried blood and ribbons whipped by the wind.\n\n";
                break;
            case "Los Civiles entienden enseguida que no están cruzando un desierto vacío, sino un territorio que alguien defiende con fanatismo.\n\n":
                r = "The Civilians quickly realize they are not crossing an empty wilderness, but a land defended with fanaticism.\n\n";
                break;
            case "<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -3 Esperanza.</b></color>":
                r = "<color=#ba3fef><b>Kale'Tav Strength increases by 1. -3 Hope.</b></color>";
                break;
            case "Entre varias rocas altas, la Caravana encuentra un reparo natural que corta el viento por un rato.\n\n":
                r = "Between several tall rocks, the Caravan finds a natural shelter that breaks the wind for a while.\n\n";
                break;
            case "No dura mucho, pero alcanza para recomponerse antes de seguir.\n\n":
                r = "It does not last long, but it is enough to recover before moving on.\n\n";
                break;
            case "Unas huellas frescas de carnero de montaña se internan por una cornisa estrecha que parece evitar parte del ascenso.\n\n":
                r = "Fresh mountain ram tracks lead along a narrow ledge that seems to bypass part of the climb.\n\n";
                break;
            case "Puedes seguir el rastro e intentar usar ese paso o mantener la ruta principal sin arriesgarte.\n\n":
                r = "You can follow the trail and try that route, or stay on the main path without taking the risk.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguir el rastro, se intentará encontrar un Atajo.</color>\n\n":
                r = "<color=#a0e812>-If you follow the trail, the Caravan will try to find a Shortcut.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides mantener la ruta, la visión del sendero levantará el ánimo. +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you stay on the route, the sight of the trail will lift spirits. +4 Hope.</color>\n\n";
                break;
            case "Por un momento, la niebla se abre y desde una altura se alcanza a ver con claridad buena parte del Paso.\n\n":
                r = "For a moment, the mist parts and from a higher point much of the Pass becomes clearly visible.\n\n";
                break;
            case "La Caravana aprovecha para orientarse mejor antes de seguir.\n\n":
                r = "The Caravan takes the chance to get its bearings before moving on.\n\n";
                break;
            case "<color=#a0e812><b>Se revelarán nodos cercanos.</b></color>":
                r = "<color=#a0e812><b>Nearby nodes will be revealed.</b></color>";
                break;
            case "Una efigie Kale'Tav yace derribada a un lado del camino, partida por la mitad y cubierta de nieve.\n\n":
                r = "A Kale'Tav effigy lies toppled at the side of the road, split in half and covered in snow.\n\n";
                break;
            case "La imagen corre rápido entre los Civiles: por una vez, algo del Paso parece menos invencible.\n\n":
                r = "Word spreads quickly among the Civilians: for once, something in the Pass seems less than invincible.\n\n";
                break;
            case "<color=#a0e812><b>-1 Fuerza Kale'Tav, +5 Esperanza.</b></color>":
                r = "<color=#a0e812><b>-1 Kale'Tav Strength, +5 Hope.</b></color>";
                break;
            case "Frente al viento helado, <b><color=#d1006f>":
                r = "Facing the freezing wind, <b><color=#d1006f>";
                break;
            case "</color></b> se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n":
                r = "</color></b> stops for a moment, looks down the road, and makes a quiet oath.\n\n";
                break;
            case "Frente al viento helado, uno de los Héroes se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n":
                r = "Facing the freezing wind, one of the Heroes stops for a moment, looks down the road, and makes a quiet oath.\n\n";
                break;
            case "La determinación con la que retoma la marcha contagia al resto.\n\n":
                r = "The determination with which they resume the march spreads to the rest.\n\n";
                break;
            case "<color=#a0e812><b>Gana 50 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Gains 50 Experience and High Morale for 3 days.</b></color>";
                break;
            case "El viento cambia de golpe y barre la niebla helada del frente por un buen trecho.\n\n":
                r = "The wind suddenly shifts and sweeps the freezing mist away for a good stretch ahead.\n\n";
                break;
            case "La Caravana consigue avanzar con mejor ritmo y algo más de seguridad.\n\n":
                r = "The Caravan manages to move forward with a better pace and a bit more safety.\n\n";
                break;
            case "<color=#a0e812><b>-1 Avance Aliento Negro</b></color>":
                r = "<color=#a0e812><b>-1 Black Breath Advance</b></color>";
                break;
            case "Antes del amanecer, unos cánticos graves atraviesan el Paso y llegan al campamento mezclados con el viento.\n\n":
                r = "Before dawn, deep chants cross the Pass and reach the camp carried by the wind.\n\n";
                break;
            case "Nadie ve a los Kale'Tav, pero el sonido basta para que casi nadie vuelva a dormir tranquilo.\n\n":
                r = "No one sees the Kale'Tav, but the sound alone is enough that almost no one sleeps peacefully again.\n\n";
                break;
            case "Al amanecer encuentran huellas frescas marcando un círculo incompleto alrededor del campamento.\n\n":
                r = "At dawn they find fresh tracks marking an incomplete circle around the camp.\n\n";
                break;
            case "No parece un ataque fallido. Más bien un mensaje. Puedes revisar bien el perímetro o mantener la calma y evitar que el rumor corra entre los Civiles.\n\n":
                r = "It does not look like a failed attack. More like a message. You can inspect the perimeter carefully or keep calm and stop the rumor from spreading among the Civilians.\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisar, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to inspect, they will leave later. +1 Black Breath Advance.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides mantener la calma, los rumores igual harán mella. -9 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to keep calm, the rumors will still take their toll. -9 Hope.</color>\n\n";
                break;
            case "</color></b> pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n":
                r = "</color></b> spent much of the night awake, alert to every sound of the wind between the rocks.\n\n";
                break;
            case "Uno de los Héroes pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n":
                r = "One of the Heroes spent much of the night awake, alert to every sound of the wind between the rocks.\n\n";
                break;
            case "Al amanecer sigue en pie, pero el descanso no alcanzó para despejarle la cabeza.\n\n":
                r = "By dawn they are still standing, but the rest was not enough to clear their head.\n\n";
                break;
            case "<color=#ba3fef><b>Obtiene Baja Moral por 3 días.</b></color>":
                r = "<color=#ba3fef><b>Gains Low Morale for 3 days.</b></color>";
                break;
            case "Muy cerca de las tiendas aparece un símbolo Kale'Tav trazado durante la noche sobre la nieve endurecida.\n\n":
                r = "Very close to the tents, a Kale'Tav symbol appears traced during the night over the hardened snow.\n\n";
                break;
            case "La marca deja claro que la Caravana fue observada mientras dormía.\n\n":
                r = "The mark makes it clear that the Caravan was watched while it slept.\n\n";
                break;
            case "<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -2 Esperanza.</b></color>":
                r = "<color=#ba3fef><b>Kale'Tav Strength increases by 1. -2 Hope.</b></color>";
                break;
            case "La Caravana logra montar el campamento al abrigo de un paredón de roca que corta las peores ráfagas del Paso.\n\n":
                r = "The Caravan manages to set camp sheltered by a rock wall that blocks the worst gusts of the Pass.\n\n";
                break;
            case "Por una noche, dormir no se siente como resistir una agresión constante.\n\n":
                r = "For one night, sleeping does not feel like enduring a constant assault.\n\n";
                break;
            case "La noche cae y, contra toda costumbre del lugar, no se oyen tambores, cuervos ni cánticos a la distancia.\n\n":
                r = "Night falls and, against all custom of the place, no drums, crows, or chants can be heard in the distance.\n\n";
                break;
            case "Ese silencio extraño no inspira confianza, pero sí regala unas horas de paz que la Caravana necesitaba.\n\n":
                r = "That strange silence does not inspire trust, but it does grant a few hours of peace that the Caravan needed.\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza</b></color>":
                r = "<color=#a0e812><b>+5 Hope</b></color>";
                break;
            case "Antes de levantar el campamento, encuentran un rastro de animales que cruza una ladera más amable que la ruta habitual.\n\n":
                r = "Before breaking camp, they find an animal trail that crosses a gentler slope than the usual route.\n\n";
                break;
            case "Puedes seguirlo para intentar encontrar un paso mejor o estudiarlo con calma para orientarte antes de partir.\n\n":
                r = "You can follow it to try to find a better way through, or study it calmly to get your bearings before leaving.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguirlo, se intentará encontrar un Atajo.</color>\n\n":
                r = "<color=#a0e812>-If you decide to follow it, a Shortcut will be attempted.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides estudiarlo, se revelarán nodos cercanos.</color>\n\n":
                r = "<color=#a0e812>-If you decide to study it, nearby nodes will be revealed.</color>\n\n";
                break;
            case "</color></b> contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n":
                r = "</color></b> silently watches the frozen aurora opening above the peaks before the march.\n\n";
                break;
            case "Uno de los Héroes contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n":
                r = "One of the Heroes silently watches the frozen aurora opening above the peaks before the march.\n\n";
                break;
            case "La imagen queda grabada con fuerza y le devuelve algo de ánimo para lo que viene.\n\n":
                r = "The sight stays etched strongly in their mind and gives them back some spirit for what lies ahead.\n\n";
                break;
            case "<color=#a0e812><b>Gana 35 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Gains 35 Experience and High Morale for 3 days.</b></color>";
                break;
            case "Bajo el suelo helado y la piedra llegan golpes sordos y repetidos, como si algo enorme estuviera tanteando el camino desde abajo.\n\n":
                r = "From beneath the frozen ground and stone come dull, repeated thuds, as if something enormous were probing the road from below.\n\n";
                break;
            case "Los Civiles aprietan el paso sin que nadie se los ordene. Aunque no llegue a emerger nada, el simple sonido basta para desgastar a la Caravana.\n\n":
                r = "The Civilians quicken their pace without anyone ordering them to. Even if nothing emerges, the sound alone is enough to wear down the Caravan.\n\n";
                break;
            case "Un tramo del camino se ha hundido y dejó un paso quebrado entre carros volcados, zanjas y piedras sueltas.\n\n":
                r = "A stretch of the road has caved in, leaving a broken crossing among overturned carts, ditches, and loose stones.\n\n";
                break;
            case "</color></b> puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n":
                r = "</color></b> can try to guide the crossing before someone falls into the gap.\n\n";
                break;
            case "<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC ":
                r = "<color=#ba3fef>-Saving Throw: Reflexes ST DC ";
                break;
            case ").</i> Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = ").</i> If successful, they gain 40 Experience. If they fail, they suffer a Wound.</color>\n\n";
                break;
            case "Uno de los Héroes puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n":
                r = "One of the Heroes can try to guide the crossing before someone falls into the gap.\n\n";
                break;
            case "<color=#ba3fef>-Si decides rodear la brecha, la Caravana ganará +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to go around the breach, the Caravan will gain +1 Fatigue.</color>\n\n";
                break;
            case "Desde la boca de un viejo pozo subterráneo suben gritos cortados y el sonido de uñas raspando piedra.\n\n":
                r = "From the mouth of an old underground well rise broken screams and the sound of claws scraping stone.\n\n";
                break;
            case "Nadie alcanza a ver qué hay abajo, pero está claro que algo se mueve en las profundidades. Puedes investigar o forzar a la Caravana a seguir.\n\n":
                r = "No one can see what is down there, but it is clear that something is moving in the depths. You can investigate or force the Caravan to keep moving.\n\n";
                break;
            case "<color=#ba3fef>-Si decides investigar, comenzará una batalla.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to investigate, a battle will begin.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir, el miedo se extenderá entre los Civiles. -5 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to move on, fear will spread among the Civilians. -5 Hope.</color>\n\n";
                break;
            case "En algún punto de Nedukazal suenan campanas de alarma, aunque nadie alcanza a ver de dónde vienen.\n\n":
                r = "Somewhere in Nedukazal, alarm bells ring, though no one can tell where they are coming from.\n\n";
                break;
            case "El sonido viaja entre ruinas, corrales vacíos y casas abandonadas, y deja a la Caravana con la sensación de haber llegado demasiado tarde para ayudar a alguien.\n\n":
                r = "The sound carries through ruins, empty pens, and abandoned houses, leaving the Caravan with the feeling that it arrived too late to help anyone.\n\n";
                break;
            case "Sombras veloces se recortan por momentos sobre tejados, tapias y galpones derruidos, y desaparecen antes de que nadie pueda apuntarlas bien.\n\n":
                r = "Swift shadows appear for moments over rooftops, walls, and collapsed sheds, then vanish before anyone can aim properly.\n\n";
                break;
            case "Puedes cerrar filas y avanzar con más cuidado o apurar el paso antes de que bajen sobre la Caravana.\n\n":
                r = "You can close ranks and move more carefully, or hurry the pace before they descend on the Caravan.\n\n";
                break;
            case "<color=#ba3fef>-Si decides cerrar filas, el avance será más tenso. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to close ranks, the march will be more tense. +1 Fatigue.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides apurar el paso, varios Civiles quedarán rezagados en el desorden. -3 Civiles, -4 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to hurry the pace, several Civilians will be left behind in the confusion. -3 Civilians, -4 Hope.</color>\n\n";
                break;
            case "Tras una puerta atrancada, la Caravana encuentra un refugio improvisado que no resistió el ataque.\n\n":
                r = "Behind a barred door, the Caravan finds an improvised shelter that did not withstand the attack.\n\n";
                break;
            case "</color></b> se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n":
                r = "</color></b> falls silent while staring at the claw marks in the wood and the remains of the struggle.\n\n";
                break;
            case "Uno de los Héroes se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n":
                r = "One of the Heroes falls silent while staring at the claw marks in the wood and the remains of the struggle.\n\n";
                break;
            case "Cuando vuelven al camino, la imagen sigue pesándole.\n\n":
                r = "When they return to the road, the sight still weighs on them.\n\n";
                break;
            case "En puertas, postes y cercos alguien dejó faroles encendidos apuntando hacia el rumbo más seguro, como si Nedukazal todavía intentara guiar a los vivos.\n\n":
                r = "On doors, posts, and fences, someone left lit lanterns pointing toward the safest direction, as if Nedukazal still tried to guide the living.\n\n";
                break;
            case "La Caravana aprovecha esas luces para orientarse mejor entre caseríos arrasados y ruinas dispersas.\n\n":
                r = "The Caravan uses those lights to get its bearings more easily among ravaged hamlets and scattered ruins.\n\n";
                break;
            case "Una vieja barricada de muebles, carros y vigas todavía sigue en pie y ofrece un breve reparo contra ataques y miradas desde las ruinas.\n\n":
                r = "An old barricade of furniture, carts, and beams is still standing and offers brief shelter from attacks and watching eyes in the ruins.\n\n";
                break;
            case "No es segura a largo plazo, pero alcanza para que la Caravana recupere el aliento antes de seguir.\n\n":
                r = "It is not safe in the long run, but it is enough for the Caravan to catch its breath before moving on.\n\n";
                break;
            case "Detrás de una bodega semienterrada y casi tapada por escombros encuentran a un pequeño grupo de supervivientes escondidos.\n\n":
                r = "Behind a half-buried storehouse nearly covered by rubble, they find a small group of survivors in hiding.\n\n";
                break;
            case "Suben a la luz temblando, pero al ver a la Caravana aceptan marcharse con ustedes.\n\n":
                r = "They climb up into the light trembling, but at the sight of the Caravan they agree to leave with you.\n\n";
                break;
            case "<color=#a0e812><b>+6-12 Civiles, +4 Esperanza</b></color>":
                r = "<color=#a0e812><b>+6-12 Civilians, +4 Hope</b></color>";
                break;
            case "En cercos, muros y postes derruidos aparecen marcas de tiza hechas a toda prisa: flechas, cruces y advertencias.\n\n":
                r = "On fences, walls, and shattered posts appear hurried chalk marks: arrows, crosses, and warnings.\n\n";
                break;
            case "Alguien estuvo guiando a otros supervivientes entre caminos, puestos y asentamientos en ruinas. Puedes seguir esas señales o reforzarlas para los que vengan detrás.\n\n":
                r = "Someone had been guiding other survivors between roads, outposts, and ruined settlements. You can follow those marks or reinforce them for whoever comes behind.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguirlas, se revelarán nodos cercanos.</color>\n\n":
                r = "<color=#a0e812>-If you decide to follow them, nearby nodes will be revealed.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides reforzar el camino, el gesto levantará el ánimo. +5 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-If you decide to reinforce the path, the gesture will lift spirits. +5 Hope.</color>\n\n";
                break;
            case "</color></b> alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n":
                r = "</color></b> catches sight of a handful of Nedukazal's inhabitants holding the line with torches and improvised spears while others evacuate a nearby hamlet.\n\n";
                break;
            case "Uno de los Héroes alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n":
                r = "One of the Heroes catches sight of a handful of Nedukazal's inhabitants holding the line with torches and improvised spears while others evacuate a nearby hamlet.\n\n";
                break;
            case "El ejemplo no cambia la guerra, pero sí la forma en que retoma la marcha.\n\n":
                r = "The example does not change the war, but it does change how they resume the march.\n\n";
                break;
            case "<color=#a0e812><b>Gana 45 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Gains 45 Experience and High Morale for 3 days.</b></color>";
                break;
            case "Al atravesar un viejo puesto de paso derruido, la Caravana encuentra un tramo cubierto entre columnas caídas y muros aún firmes, bastante más seguro que el terreno abierto.\n\n":
                r = "While crossing a ruined old waystation, the Caravan finds a covered stretch between fallen columns and still-standing walls, much safer than the open ground.\n\n";
                break;
            case "Por un momento, avanzar deja de sentirse como exponerse a cada sombra.\n\n":
                r = "For a moment, moving forward no longer feels like exposing yourselves to every shadow.\n\n";
                break;
            case "El hollín, la humedad y el viento apagaron varias luces del campamento una y otra vez hasta volver la oscuridad insoportable.\n\n":
                r = "Soot, dampness, and wind snuffed out several camp lights again and again until the darkness became unbearable.\n\n";
                break;
            case "Nadie llega a descansar del todo bien cuando Nedukazal vuelve a tragarse la poca luz disponible.\n\n":
                r = "No one truly rests well when Nedukazal swallows up the little light available once more.\n\n";
                break;
            case "La Caravana arma un descanso precario bajo el techo vencido de un galpón saqueado. Cada crujido hace mirar hacia arriba.\n\n":
                r = "The Caravan makes a precarious camp under the sagging roof of a looted shed. Every creak draws eyes upward.\n\n";
                break;
            case "</color></b> puede intentar asegurarlo antes de que ceda.\n\n":
                r = "</color></b> can try to secure it before it gives way.\n\n";
                break;
            case ").</i> Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = ").</i> If successful, they gain 35 Experience. If they fail, they suffer a Wound.</color>\n\n";
                break;
            case "Uno de los Héroes puede intentar asegurarlo antes de que ceda.\n\n":
                r = "One of the Heroes can try to secure it before it gives way.\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "<color=#ba3fef>-If you try it, they will make a Reflexes ST DC 12 Saving Throw. If successful, they gain 35 Experience. If they fail, they suffer a Wound.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides mover el campamento, la noche será más larga. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to move camp, the night will be longer. +1 Fatigue.</color>\n\n";
                break;
            case "Durante el descanso, desde una bodega cerrada cercana llegan golpes irregulares, respiraciones ásperas y algo arrastrándose entre cajones y barriles rotos.\n\n":
                r = "During the rest, irregular thuds, rough breathing, and something dragging itself between crates and broken barrels come from a nearby sealed storehouse.\n\n";
                break;
            case "Puedes investigar antes de que eso venga hacia el campamento o atrancar la entrada y pasar la noche en tensión.\n\n":
                r = "You can investigate before it comes toward the camp, or bar the entrance and spend the night on edge.\n\n";
                break;
            case "<color=#ba3fef>-Si decides atrancar la bodega, +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-If you decide to barricade the cellar, +1 Fatigue.</color>\n\n";
                break;
            case "</color></b> encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n":
                r = "</color></b> finds on a wall a long list of names written in haste, crossed out as Nedukazal kept falling.\n\n";
                break;
            case "Uno de los Héroes encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n":
                r = "One of the Heroes finds on a wall a long list of names written in haste, crossed out as Nedukazal kept falling.\n\n";
                break;
            case "Después de leerla, el descanso ya no consigue apartarle esa imagen de la cabeza.\n\n":
                r = "After reading it, the rest can no longer drive that image from their mind.\n\n";
                break;
            case "Desde un alto todavía firme, parte de la guardia consigue observar caminos, corrales y techos que desde abajo resultaban imposibles de leer.\n\n":
                r = "From a vantage point still standing, part of the watch manages to observe roads, pens, and rooftops that were impossible to read from below.\n\n";
                break;
            case "A la mañana siguiente, la Caravana parte con una idea mucho más clara del terreno inmediato.\n\n":
                r = "The next morning, the Caravan sets out with a much clearer idea of the immediate terrain.\n\n";
                break;
            case "Entre muros caídos, carretas rotas y lonas viejas, la Caravana consigue armar un fogón protegido del viento y de las miradas del terreno abierto.\n\n":
                r = "Among fallen walls, broken carts, and old tarps, the Caravan manages to build a fire sheltered from the wind and from watching eyes across the open ground.\n\n";
                break;
            case "No es cómodo, pero sí lo bastante estable como para dormir mejor de lo esperado.\n\n":
                r = "It is not comfortable, but it is stable enough to sleep better than expected.\n\n";
                break;
            case "Al reparo de un patio de posta, ocultos por lonas y carros volcados, encuentran a varios habitantes de Nedukazal esperando el momento para huir.\n\n":
                r = "Sheltering in a coaching yard, hidden by tarps and overturned carts, they find several inhabitants of Nedukazal waiting for the moment to flee.\n\n";
                break;
            case "Al enterarse de que la Caravana partirá al amanecer, piden sumarse antes de que los Zarkil vuelvan a cruzar la zona.\n\n":
                r = "When they learn the Caravan will leave at dawn, they ask to join before the Zarkil cross the area again.\n\n";
                break;
            case "<color=#a0e812><b>+5-10 Civiles, +3 Esperanza</b></color>":
                r = "<color=#a0e812><b>+5-10 Civilians, +3 Hope</b></color>";
                break;
            case "</color></b> encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n":
                r = "</color></b> finds a letter that never made it out of Nedukazal, written for someone waiting for news beyond the kingdom.\n\n";
                break;
            case "Uno de los Héroes encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n":
                r = "One of the Heroes finds a letter that never made it out of Nedukazal, written for someone waiting for news beyond the kingdom.\n\n";
                break;
            case "Leerla durante el descanso le devuelve perspectiva sobre por qué todavía vale la pena seguir.\n\n":
                r = "Reading it during the rest restores their sense of why it is still worth pressing on.\n\n";
                break;
            case "<color=#a0e812><b>Gana 40 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Gains 40 Experience and High Morale for 3 days.</b></color>";
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
            case "Has llegado al lugar señalado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traído para ayudarles en su travesía.\n\nInmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.\n":
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
            case "Impacto crítico":
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
            case "Acumular Energía":
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
            case "Hoja de Energía":
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
            case "-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crítico. -20 Esperanza":
                r = "-The Cleric Retinue has perished, as the Black Breath has reached a critical level. -20 Hope";
                break;
            case " ahora Maneja un Nivel ":
                r = " now has Energy Level ";
                break;
            case " de Energía.":
                r = ".";
                break;
            case " de Valentía.":
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
            /*  case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comercial a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
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
            case "Día":
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
            case "Símbolo de Proteccion Arcano":
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
            /*  case "Este séquito está constituído por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
                  r = "This retinue is composed of several merchants who have had to abandon their shops, but who have not given up their merchandise. They are willing to trade at reduced prices but without giving up at least a minimal profit.";
                  break;*/
            case "El Espectro acaba de atacar, haciéndolo vulnerable en el plano material.":
                r = "The Specter has just attacked, making it vulnerable in the material plane.";
                break;
            case "Echar.":
                r = "Expel.";
                break;
            case "Echar":
                r = "Expel";
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
            case "Puedes fijar 1 item con el botón derecho del mouse para que no se pierda al actualziar el inventario.":
                r = "You can pin 1 item with the right mouse button so it doesn't get lost when refreshing the inventory.";
                break;
            case "La ruta se abre en varias direcciones parecidas y las pocas señales útiles parecen haberse cruzado unas con otras. Dos miembros de la caravana parecen tener opiniones encontradas. ¿A quién escucharás?\n\n":
                r = "The path opens in several similar directions and the few useful signs seem to have crossed each other. Two members of the caravan seem to have conflicting opinions. Who will you listen to?\n\n";
                break;
            case "Impulso":
                r = "Impulse";
                break;
            case "impulso":
                r = "impulse";
                break;
            case "Impulso: el próximo movimiento a casilla o intercambio cuesta 1 PA menos y consume 1 stack.":
                r = "Impulse: the next move to a tile or swap costs 1 less AP and consumes 1 stack.";
                break;
            case "No hay enemigos en la fila":
                r = "There are no enemies in the row.";
                break;
            case "No hay casilla frontal disponible":
                r = "There is no available frontal tile";
                break;
            case "No hay trayecto válido":
                r = "There is no valid route";
                break;
            case "El trayecto esta bloqueado":
                r = "The route is blocked";
                break;
            case "<color=#0cca74><b>Siempre Alerta: </b></color><color=#d3d3d3><i>La Duelista se mantiene lista para actuar con rapidez si se presenta una batalla.</color></i>\\n\\n+5 Iniciativa en combate. Si no es emboscada, gana 2 Impulso al comenzar la batalla.":
                r = "<color=#0cca74><b>Always Alert: </b></color><color=#d3d3d3><i>The Duelist stays ready to act quickly if a battle arises.</color></i>\\n\\n+5 Initiative in combat. If not ambushed, gains 2 Impulse at the start of battle.";
                break;
            case "<color=#0cca74><b>Socializar: </b></color><color=#d3d3d3><i>La Duelista dedica tiempo a conversar, bromear y sostener el ánimo de la caravana.</color></i>\\n\\nCada día, sus compañeros realizan una TS Mental DC 13. Quienes la superan obtienen Alta Moral por 1 día.":
                r = "<color=#0cca74><b>Socialize: </b></color><color=#d3d3d3><i>The Duelist spends time talking, joking, and keeping the caravan's spirits up.</color></i>\\n\\nEach day, her companions make a Mental Save DC 13. Those who succeed gain High Morale for 1 day.";
                break;
            case "<color=#0cca74><b>Consuelo: </b></color><color=#d3d3d3><i>La Duelista contiene el desánimo de la caravana cuando llegan malas noticias o tiempos difíciles.</color></i>\\n\\nSiempre que se pierda Esperanza por cualquier motivo, se pierde 2 menos.":
                r = "<color=#0cca74><b>Solace: </b></color><color=#d3d3d3><i>The Duelist helps contain the caravan's discouragement when bad news or hard times arrive.</color></i>\\n\\nWhenever Hope would be lost for any reason, 2 less is lost.";
                break;
            case "Este personaje no puede realizar actividades ahora. Descansa.":
                r = "This character can't do activities now. Only rest.";
                break;
            case "-Se ha cambiado la actividad de todos los personajes.":
                r = "The activity of all characters has been changed.";
                break;
            case "Duelista":
                r = "Duelist";
                break;
            case "-La Caravana se mueve con Aletargamiento. +1 Avance del Aliento Negro.":
                r = "-The caravan moves with lethargy. +1 Advance of the Black Breath.";
                break;
            case "Siempre Alerta":
                r = "Always Alert";
                break;
            case "Impulsivo":
                r = "Impulsive";
                break;
            case "Cansado":
                r = "Tired";
                break;
            case " es ejecutado por la Condena.":
                r = " is executed by Condemnation.";
                break;
            case "Guardar Partida":
                r = "Save Game";
                break;
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3 y un personaje con Corrupción al azar será curado.":
                r = "When performing the offering, the Black Breath will retreat by 3 and a random character with Corruption will be healed.";
                break;
            case "<i>El Árbol de los Lamentos extiende sus raices de forma amenazante por sobre la superficie para atacar a sus enemigos y protegerse. </i>\n\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>The Tree of Lamentations extends its roots threateningly over the surface to attack its enemies and protect itself. </i>\n\n<color=#EE0000>-Weak against fire.</color>";
                break;
            case "Raíz Maldita":
                r = "Cursed Root";
                break;
            case "<i>Este gigante árbol maldito bloquea la salida del Bosque Ardiente, poseído por los espíritus caídos en el bosque, buscará impedir el escape de los intrusos.</i>\n\n<color=#199F10>Crea Enredaderas.\n-Ataques de rango que atraen.\n-Regenera armadura.</color><color=#EE0000>-Débil al fuego.</color>":
                r = "<i>This giant cursed tree blocks the exit of the Burning Forest, possessed by the fallen spirits in the forest, it will seek to prevent the escape of intruders.</i>\n\n<color=#199F10>Creates Vines.\n-Range attacks that pull.\n-Regenerates armor.</color><color=#EE0000>-Weak against fire.</color>";
                break;
            case "Árbol de los Lamentos":
                r = "Tree of Lamentations";
                break;
            case "Invocar Raíz Maldita": r = "Summon Cursed Root"; break;
            case "Raiz Maldita": r = "Cursed Root"; break;
            case "Condena Feroz": r = "Fierce Condemnation"; break;
            case "+15% Danio, +2 Ataque, +5 TS Mental.": r = "+15% Damage, +2 Attack, +5 Mental Save."; break;
            case "Estertor Maldito": r = "Cursed Death Rattle"; break;
            case "Reacción: al morir, quien asesta el golpe final debe superar TS Mental 12 o recibe 2d6 daño necrótico.": r = "Reaction: on death, whoever deals the killing blow must pass Mental Save 12 or take 2d6 necrotic damage."; break;
            case "resiste el estertor maldito.": r = "resists the cursed death rattle."; break;
            case "desata una llamarada necrotica y ardiente sobre": r = "unleashes a burning necrotic flare upon"; break;
            case "Bruja Quemada": r = "Bruxa Queimada"; break;
            case "<i>Esta bruja ha sido deformada por las llamas y corrompida la presencia del Aliento Negro. </i>\n\n<color=#199F10>Crea Enredaderas.\n-Ataque de rango que no falla.\n-Estertor Mortal.</color><color=#EE0000>-Poco resistente.</color>":
                r = "<i>This witch has been deformed by the flames and corrupted by the presence of the Black Breath. </i>\n\n<color=#199F10>Creates Vines.\n-Range attack that never fails.\n-Cursed Death Rattle.</color><color=#EE0000>-Not very resistant.</color>";
                break;
            case "), eligió bien la ruta y el Aliento Negro retrocedió 1. +25 Experiencia.":
                r = "), chose the right path and the Black Breath retreated 1. +25 Experience.";
                break;
            case "Escape: los personajes podrán escapar desde esta casilla.":
                r = "Escape: characters can flee the battle from this tile.";
                break;
            case "Paciente":
                r = "Patient";
                break;
            case "Defensa vencida":
                r = "Broken defense";
                break;
            case "Fortitud":
                r = "Fortitude";
                break;
            case "Al Esforzarse se toma prestado AP del turno siguiente y la Defensa bajará por 1T.":
                r = "When Exerting, you borrow AP from the next turn and your defense will decrease by 1T.";
                break;
            case "¡Esforzando!":
                r = "Exerting!";
                break;
            case "Viajando...":
                r = "Traveling...";
                break;
            case "Resolver Combate":
                r = "Resolve Combat";
                break;
            case "Caravana":
                r = "Caravan";
                break;
            case "Séquitos":
                r = "Retinues";
                break;
            case "Bitácora":
                r = "Binnacle";
                break;
            case "-La actividad de todos los personajes ahora es: ":
                r = "-The activity of all characters is now: ";
                break;
            case "Guardia":
                r = "Guard";
                break;
            case "Descanso":
                r = "Rest";
                break;
            case "-Actividad fijada.":
                r = "-Activity fixed.";
                break;
            case "Atributos":
                r = "Attributes";
                break;
            case "Equipo Disponible":
                r = "Available Gear";
                break;
            case " disponibles para ":
                r = " available for ";
                break;
            case "Armas":
                r = "Weapons";
                break;
            case "Armaduras":
                r = "Armors";
                break;
            case "Echar a ":
                r = "Expel ";
                break;
            case " hará que se pierdan ":
                r = " will cause the loss of ";
                break;
            case " Esperanza. ¿Continuar?":
                r = " Hope. Continue?";
                break;
            case "Estadísticas":
                r = "Stats";
                break;
            case "Días viajados con la caravana...":
                r = "Days traveled with the caravan...";
                break;
            case "Enemigos eliminados...":
                r = "Enemies killed...";
                break;
            case "Daño infligido...":
                r = "Damage dealt...";
                break;
            case "Daño recibido...":
                r = "Damage received...";
                break;
            case "Veces derrotado...":
                r = "Times defeated...";
                break;
            case "Exploración":
                r = "Exploration";
                break;
            case "Viaje":
                r = "Travel";
                break;
            case "Reservas":
                r = "Reserves";
                break;
            case "Si la defensa es derrotada, la caravana será destruída.":
                r = "If the defense is defeated, the caravan will be destroyed.";
                break;
                break;
            case "Selecciona el orden de los refuerzos: (-->)":
                r = "Select the order of reinforcements: (-->)";
                break;
            case "Silenciar tips":
                r = "Silence tips";
                break;
            case "Mostrar Ayudas":
                r = "Show Tips";
                break;
            case "Noticias":
                r = "News";
                break;
            case "Prueba jugable":
                r = "Open Playtest";
                break;
            case "Gracias por jugar la demo.\\nEn esta versión podrás experimentar el Tutorial y luego la primer zona del juego completa.\\n\\nTu feedback es muy importante para seguir mejorando.":
                r = "Thank you for playing the demo.\nIn this version, you will be able to experience the Tutorial and then the first region of the game.\n\nYour feedback is very important to help us keep improving.";
                break;
            case "Claridad":
                r = "Clarity";
                break;
            case "Puedes dar feedback y unirte a nuestro Discord aquí.":
                r = "You can give feedback and join the Discord community here.";
                break;
            case "Créditos":
                r = "Credits";
                break;
            case "Wishlist en Steam":
                r = "Wishlist on Steam";
                break;
            case "Prohibido: Individualista":
                r = "Forbidden: Individualist";
                break;
            case "Salir al Menú":
            r = "Exit to Menu";
            break;
         
            
            
            
            
            
            
            

























































        }


        return r;
    }
































































































































































































































































































    bool TryTraducirEventosCampaniaUiPortuguesV2(string txt, out string traduccion)
    {
        if (string.IsNullOrEmpty(txt))
        {
            traduccion = null;
            return false;
        }

        if (txt == "Paso Precario") { traduccion = "Passagem Precária"; return true; }
        if (txt == "Aire Enrarecido") { traduccion = "Ar Rarefeito"; return true; }
        if (txt == "Rumor de Desbande") { traduccion = "Rumor de Debandada"; return true; }
        if (txt == "Vado Traicionero") { traduccion = "Vau Traiçoeiro"; return true; }
        if (txt == "Carro Encajado") { traduccion = "Carroça Atolada"; return true; }
        if (txt == "Marcas del Correo") { traduccion = "Marcas do Correio"; return true; }
        if (txt == "Pulso de Mando") { traduccion = "Pulso de Comando"; return true; }
        if (txt == "Hombros Firmes") { traduccion = "Ombros Firmes"; return true; }
        if (txt == "Manos Certeras") { traduccion = "Mãos Certeras"; return true; }
        if (txt == "Dos Miradas") { traduccion = "Dois Olhares"; return true; }

        if (txt == "Entrar") { traduccion = "Entrar"; return true; }
        if (txt == "Sellarla") { traduccion = "Selar"; return true; }
        if (txt == "Imponer silencio") { traduccion = "Impor silêncio"; return true; }
        if (txt == "Empujarlo") { traduccion = "Empurrar"; return true; }
        if (txt == "Descargarlo") { traduccion = "Descarregar"; return true; }
        if (txt == "Interpretarlas") { traduccion = "Interpretá-las"; return true; }
        if (txt == "Cargarlo") { traduccion = "Carregá-lo"; return true; }
        if (txt == "Relevarlo") { traduccion = "Revezá-lo"; return true; }
        if (txt == "Recuperarla") { traduccion = "Recuperá-la"; return true; }
        if (txt == "Dejarla ir") { traduccion = "Deixá-la ir"; return true; }
        if (txt == "Decidir") { traduccion = "Decidir"; return true; }
        if (txt.StartsWith("H") && txt.EndsWith("roe 1")) { traduccion = "Herói 1"; return true; }

        if (txt.StartsWith("La Caravana llega a un tramo estrecho, quebrado y lleno de tablones flojos."))
        {
            traduccion = "A Caravana chega a um trecho estreito, quebrado e cheio de tábuas soltas. Não parece impossível de atravessar, mas é traiçoeiro o bastante para transformar um descuido em acidente.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar guiar el cruce antes de que cunda el p"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode tentar guiar a travessia antes que o pânico se espalhe.\n\n"
                : "</color></b> pode tentar guiar a travessia antes que o pânico se espalhe.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Reflejos DC "))
        {
            traduccion = "<color=#ba3fef>-Teste de Resistência: Reflexos CD ";
            return true;
        }
        if (txt == " <i>(TS Reflejos actual: ")
        {
            traduccion = " <i>(TR de Reflexos atual: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("35 Experiencia. Si falla, obtendr") && txt.Contains("Herida.</color>"))
        {
            traduccion = ").</i> Se passar, ganhará 35 de Experiência. Se falhar, sofrerá Ferimento.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Reflejos DC 11"))
        {
            traduccion = "<color=#ba3fef>-Se tentar, fará um Teste de Resistência de Reflexos CD 11. Se passar, ganhará 35 de Experiência. Se falhar, sofrerá Ferimento.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides rodear el tramo"))
        {
            traduccion = "<color=#ba3fef>-Se decidir contornar o trecho, a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Desde una bodega medio tapada llegan golpes apagados"))
        {
            traduccion = "De um porão meio encoberto vêm batidas abafadas e pedidos de ajuda. O ar que sai da entrada está carregado de poeira velha, mofo e algo que arranha a garganta assim que alguém se aproxima.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar entrar y sacar a quienes sigan con vida"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode tentar entrar e tirar de lá quem ainda estiver vivo antes que o lugar desabe.\n\n"
                : "</color></b> pode tentar entrar e tirar de lá quem ainda estiver vivo antes que o lugar desabe.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Fortaleza DC "))
        {
            traduccion = "<color=#ba3fef>-Teste de Resistência: Fortaleza CD ";
            return true;
        }
        if (txt == " <i>(TS Fortaleza actual: ")
        {
            traduccion = " <i>(TR de Fortaleza atual: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, rescatar") && txt.Contains("6-10 Civiles") && txt.Contains("30 Experiencia"))
        {
            traduccion = ").</i> Se passar, resgatará 6-10 Civis e ganhará 30 de Experiência. Se falhar, ficará Doente por 3 dias.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Fortaleza DC 10"))
        {
            traduccion = "<color=#ba3fef>-Se tentar, fará um Teste de Resistência de Fortaleza CD 10. Se passar, resgatará 6-10 Civis e ganhará 30 de Experiência. Se falhar, ficará Doente por 3 dias.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides sellar la entrada"))
        {
            traduccion = "<color=#ba3fef>-Se decidir selar a entrada e seguir, -4 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Una versi") && txt.Contains("peligro cercano se esparce de carro en carro"))
        {
            traduccion = "Uma versão exagerada de um perigo próximo se espalha de carroça em carroça e começa a causar um pânico desnecessário. Em poucos minutos, vários Civis já falam em abandonar a marcha antes de ficarem presos.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar frenarlo con calma antes de que empeore"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode tentar conter o rumor com calma antes que piore.\n\n"
                : "</color></b> pode tentar conter isso com calma antes que piore.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Tirada de Salv") && txt.Contains("TS Mental DC "))
        {
            traduccion = "<color=#ba3fef>-Teste de Resistência: Mental CD ";
            return true;
        }
        if (txt == " <i>(TS Mental actual: ")
        {
            traduccion = " <i>(TR Mental atual: ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("+4 Esperanza") && txt.Contains("Baja Moral por 3"))
        {
            traduccion = ").</i> Se passar, ganhará 35 de Experiência e +4 Esperança. Se falhar, sofrerá Moral Baixa por 3 dias e a Caravana perderá 5 Esperança.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si lo intentas, har") && txt.Contains("TS Mental DC 12"))
        {
            traduccion = "<color=#ba3fef>-Se tentar, fará um Teste de Resistência Mental CD 12. Se passar, ganhará 35 de Experiência e +4 Esperança. Se falhar, sofrerá Moral Baixa por 3 dias e a Caravana perderá 5 Esperança.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides imponer silencio"))
        {
            traduccion = "<color=#ba3fef>-Se decidir impor silêncio à força, -3 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("La corriente parece mansa desde lejos"))
        {
            traduccion = "A corrente parece mansa de longe, mas assim que as primeiras carroças tocam o vau fica claro que o fundo é escorregadio e a água puxa com mais força do que parecia.\n\n";
            return true;
        }
        if (txt.StartsWith("Alguien tendr") && txt.Contains("ordenar el cruce de los bueyes"))
        {
            traduccion = "Alguém terá de ir à frente para organizar a travessia dos bois e evitar que tudo se desfaça no meio do vau.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Reflejos DC ")
        {
            traduccion = "</color></b>: TR de Reflexos CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("40 Experiencia") && txt.Contains("1 Buey"))
        {
            traduccion = ").</i> Se passar no teste, ganhará 40 de Experiência. Se falhar, sofrerá Ferimento e a Caravana perderá 1 Boi.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides no arriesgar el cruce"))
        {
            traduccion = "<color=#ba3fef>-Se decidir não arriscar a travessia, o desvio fará o Respiro Negro avançar.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Uno de los carros queda mal encajado entre piedras"))
        {
            traduccion = "Uma das carroças fica presa entre pedras e barro duro. Se não a soltarem logo, a marcha vai travar ao redor dela e o mau humor começará a crescer.\n\n";
            return true;
        }
        if (txt.StartsWith("Hace falta fuerza y aguante para moverlo"))
        {
            traduccion = "É preciso força e resistência para movê-la sem acabar ferido.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Fortaleza DC ")
        {
            traduccion = "</color></b>: TR de Fortaleza CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("+3 Esperanza") && txt.Contains("+1 Fatiga"))
        {
            traduccion = ").</i> Se passar no teste, ganhará 35 de Experiência e +3 Esperança. Se falhar, sofrerá Ferimento e a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#ba3fef>-Si decides descargar el carro"))
        {
            traduccion = "<color=#ba3fef>-Se decidir descarregar a carroça e seguir, +1 Fadiga.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("En un poste vencido y en varias piedras cercanas"))
        {
            traduccion = "Em um poste caído e em várias pedras próximas aparecem marcas antigas de correio, quase apagadas pelo tempo. Ainda parece possível tirar algo útil desse código se alguém souber lê-lo bem.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar interpretarlas antes de que se pierda la luz"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode tentar interpretá-las antes que a luz acabe.\n\n"
                : "</color></b> pode tentar interpretá-las antes que a luz acabe.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Mental DC "))
        {
            traduccion = "<color=#a0e812>-Teste de Resistência: Mental CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, se revelar") && txt.Contains("30 Experiencia") && txt.Contains("+1 Fatiga"))
        {
            traduccion = ").</i> Se passar, nós próximos serão revelados e ganhará 30 de Experiência. Se falhar, a demora fará a Caravana ganhar +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Mental DC 10"))
        {
            traduccion = "<color=#a0e812>-Se tentar, fará um Teste de Resistência Mental CD 10. Se passar, nós próximos serão revelados e ganhará 30 de Experiência. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides seguir sin detenerte"))
        {
            traduccion = "<color=#a0e812>-Se decidir seguir sem parar, +3 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Un embotellamiento de carros, Civiles y animales"))
        {
            traduccion = "Um engarrafamento de carroças, Civis e animais corta o ritmo da marcha. Ainda não é grave, mas se ninguém organizar a fila com autoridade a confusão pode se espalhar bastante.\n\n";
            return true;
        }
        if (txt == "</color></b>: TS Mental DC ")
        {
            traduccion = "</color></b>: TR Mental CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("+6 Esperanza") && txt.Contains("2 Esperanza"))
        {
            traduccion = ").</i> Se passar no teste, ganhará 30 de Experiência e a Caravana obterá +6 Esperança. Se falhar, a Caravana perderá 2 Esperança.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejar que la fila"))
        {
            traduccion = "<color=#a0e812>-Se decidir deixar a fila se ajeitar sozinha, +3 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Un Civil agotado se desploma en el camino."))
        {
            traduccion = "Um Civil exausto desaba no caminho. Ninguem parece notar, ou se importar, e passam por ele como se nada tivesse acontecido.\n\n";
            return true;
        }
        if (txt.Contains("se ofrece a levantarlo y cargarlo"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Herois se oferece para levanta-lo e carrega-lo. Mas voce pode ordenar que ele guarde forcas para perigos futuros no caminho.\n\n"
                : "</color></b> se oferece para levanta-lo e carrega-lo. Mas voce pode ordenar que ele guarde forcas para perigos futuros no caminho.\n\n";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("50 Experiencia") && txt.Contains("+5 Esperanza") && txt.Contains("Fatigado"))
        {
            traduccion = ").</i> Se passar, ganhara 50 de Experiencia e a Caravana obtera +5 Esperanca. Se falhar, ficara Fatigado.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Fortaleza DC 13") && txt.Contains("+5 Esperanza") && txt.Contains("Fatigado"))
        {
            traduccion = "<color=#a0e812>-Se tentar, fara um Teste de Resistencia de Fortaleza CD 13. Se passar, ganhara 50 de Experiencia e a Caravana obtera +5 Esperanca. Se falhar, ficara Fatigado.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejar al Civil"))
        {
            traduccion = "<color=#a0e812>-Se decidir deixar o Civil para tras, -5 Esperanca. -1 Civil.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("Un Civil agotado se desploma justo cuando la fila"))
        {
            traduccion = "Um Civil exausto desaba justo quando a fila começa a recuperar o ritmo. Se ninguém o ajudar, o grupo vai voltar a desacelerar e o mau humor vai crescer.\n\n";
            return true;
        }
        if (txt.Contains("puede cargarlo y sostener el paso hasta la pr"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode carregá-lo e manter o passo até a próxima pausa.\n\n"
                : "</color></b> pode carregá-lo e manter o passo até a próxima pausa.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Fortaleza DC "))
        {
            traduccion = "<color=#a0e812>-Teste de Resistência: Fortaleza CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, ganar") && txt.Contains("-1 Fatiga") && txt.Contains("+1 Fatiga"))
        {
            traduccion = ").</i> Se passar, ganhará 35 de Experiência e a Caravana obterá -1 Fadiga. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Fortaleza DC 12"))
        {
            traduccion = "<color=#a0e812>-Se tentar, fará um Teste de Resistência de Fortaleza CD 12. Se passar, ganhará 35 de Experiência e a Caravana obterá -1 Fadiga. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides relevarlo"))
        {
            traduccion = "<color=#a0e812>-Se decidir revezá-lo entre vários, +2 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("Una r") && txt.Contains("cartera de viaje con mapas, notas y referencias"))
        {
            traduccion = "Uma rajada arrasta uma bolsa de viagem com mapas, notas e referências úteis até uma borda difícil de alcançar. Ainda dá para recuperá-la, mas será preciso velocidade e precisão.\n\n";
            return true;
        }
        if (txt.Contains("puede intentar atraparla antes de que se pierda del todo"))
        {
            traduccion = txt.StartsWith("Uno de los H")
                ? "Um dos Heróis pode tentar pegá-la antes que se perca de vez.\n\n"
                : "</color></b> pode tentar pegá-la antes que se perca de vez.\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Tirada de Salv") && txt.Contains("TS Reflejos DC "))
        {
            traduccion = "<color=#a0e812>-Teste de Resistência: Reflexos CD ";
            return true;
        }
        if (txt.StartsWith(").</i> Si la supera, se revelar") && txt.Contains("35 Experiencia") && txt.Contains("+4 Esperanza"))
        {
            traduccion = ").</i> Se passar, nós próximos serão revelados, ganhará 35 de Experiência e a Caravana obterá +4 Esperança. Se falhar, sofrerá Ferimento.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si lo intentas, har") && txt.Contains("TS Reflejos DC 13"))
        {
            traduccion = "<color=#a0e812>-Se tentar, fará um Teste de Resistência de Reflexos CD 13. Se passar, nós próximos serão revelados, ganhará 35 de Experiência e a Caravana obterá +4 Esperança. Se falhar, sofrerá Ferimento.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides dejarla ir"))
        {
            traduccion = "<color=#a0e812>-Se decidir deixá-la ir, -4 Esperança.</color>\n\n";
            return true;
        }

        if (txt.StartsWith("La ruta se abre en varias direcciones parecidas"))
        {
            traduccion = "A rota se abre em várias direções parecidas e os poucos sinais úteis parecem ter se cruzado uns com os outros. Dois membros da caravana parecem ter opiniões divergentes. Quem você vai ouvir?\n\n";
            return true;
        }
        if (txt.StartsWith(").</i> Si supera la tirada, ganar") && txt.Contains("25 Experiencia") && txt.Contains("retroced"))
        {
            traduccion = ").</i> Se passar no teste, ganhará 25 de Experiência e o Respiro Negro recuará 1. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
            return true;
        }
        if (txt.StartsWith("<color=#a0e812>-Si decides mantener la ruta"))
        {
            traduccion = "<color=#a0e812>-Se decidir manter a rota sem se arriscar, +4 Esperança.</color>\n\n";
            return true;
        }

        traduccion = null;
        return false;
    }

    string TraducirPortugues(string txt, bool esBotonFijo = false)
    {
       
        string r = txt;
        if (!esBotonFijo)
        {
            r = txt;
        }

        if (TryTraducirEventosCampaniaUiPortuguesV2(txt, out string traduccionEventoUiPortugues))
        {
            return traduccionEventoUiPortugues;
        }
        string final = txt.Normalize(NormalizationForm.FormC);

        // Tabla PT en preparacion: por ahora conserva el texto base hasta cargar cada caso.
        switch (final)
        {
            case "Danza del Estoque":
                r = "Danca do Estoque";
                break;
            case "Estoque del Primer Sangre":
                r = "Estoque do Primeiro Sangue";
                break;
            case "Estoque de Veloz Replica":
                r = "Estoque de Replica Veloz";
                break;
            case "Estoque de la Rosa Negra":
                r = "Estoque da Rosa Negra";
                break;
            case "Gambeson de Esgrima Ligera":
                r = "Gambeson de Esgrima Leve";
                break;
            case "Gambeson del Temple":
                r = "Gambeson do Temple";
                break;
            case "Gambeson del Ultimo Paso":
                r = "Gambeson do Ultimo Passo";
                break;
            case "Blanco Medido":
                r = "Alvo Medido";
                break;
            case "Rosa Negra":
                r = "Rosa Negra";
                break;
            case "Ultimo Paso":
                r = "Ultimo Passo";
                break;
            case "Danzando":
                r = "Dancando";
                break;
            case "Encadena bajas por este turno: +1 Ataque y +15% Danio.":
                r = "Encadeia baixas neste turno: +1 Ataque e +15% Dano.";
                break;
            case "Encadena bajas por este turno: +1 Ataque, +15% Danio y +1 rango critico.":
                r = "Encadeia baixas neste turno: +1 Ataque, +15% Dano e +1 faixa critica.";
                break;
            case "Presencia Provocadora":
                r = "Presenca Provocadora";
                break;
            case "Distra\u00EDdo":
                r = "Distraido";
                break;
            case "Pierde foco: -2 Defensa y -3 Armadura.":
                r = "Perde o foco: -2 Defesa e -3 Armadura.";
                break;
            case "Pierde foco: -2 Defensa y -4 Armadura.":
                r = "Perde o foco: -2 Defesa e -4 Armadura.";
                break;
            case "Recuperando Aire":
                r = "Recuperando Folego";
                break;
            case "Descansa para el turno siguiente: +3 PA maximo, -4 Defensa.":
                r = "Descansa para o proximo turno: +3 AP maximo, -4 Defesa.";
                break;
            case "Descansa para el turno siguiente: +3 PA maximo, -3 Defensa.":
                r = "Descansa para o proximo turno: +3 AP maximo, -3 Defesa.";
                break;
            case "Solo en columna trasera.":
                r = "Apenas na coluna traseira.";
                break;
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
            case "Cenizas en el Camino":
                r = "Cinzas no Caminho";
                break;
            case "Bestias Aterradas":
                r = "Bestas Aterrorizadas";
                break;
            case "Fuego en la Retaguardia":
                r = "Fogo na Retaguarda";
                break;
            case "Tambores en la Niebla":
                r = "Tambores na Névoa";
                break;
            case "Hielo Quebradizo":
                r = "Gelo Quebradiço";
                break;
            case "Efigies del Paso":
                r = "Efígies da Passagem";
                break;
            case "Partida de Caza":
                r = "Grupo de Caça";
                break;
            case "Frío Hasta los Huesos":
                r = "Frio Até os Ossos";
                break;
            case "Tótem de Guerra":
                r = "Totem de Guerra";
                break;
            case "Brote entre las Brasas":
                r = "Broto entre as Brasas";
                break;
            case "Madera Medio Quemada":
                r = "Madeira Meio Queimada";
                break;
            case "Refugio de Piedra":
                r = "Refúgio de Pedra";
                break;
            case "Sendero del Carnero":
                r = "Trilha do Carneiro";
                break;
            case "Cielo Abierto":
                r = "Céu Aberto";
                break;
            case "Efigie Derribada":
                r = "Efígie Derrubada";
                break;
            case "Juramento del Paso":
                r = "Juramento da Passagem";
                break;
            case "Viento a Favor":
                r = "Vento a Favor";
                break;
            case "Humo en el Campamento":
                r = "Fumaça no Acampamento";
                break;
            case "Guardia Somnolienta":
                r = "Guarda Sonolenta";
                break;
            case "Raciones Mojadas":
                r = "Rações Molhadas";
                break;
            case "Discusión en la Fogata":
                r = "Discussão na Fogueira";
                break;
            case "Herramientas Perdidas":
                r = "Ferramentas Perdidas";
                break;
            case "Noche Serena":
                r = "Noite Serena";
                break;
            case "Fogón Compartido":
                r = "Fogueira Compartilhada";
                break;
            case "Manos Voluntariosas":
                r = "Mãos Prestativas";
                break;
            case "Sueño Reparador":
                r = "Sono Reparador";
                break;
            case "Hallazgo entre los Carros":
                r = "Achado entre as Carroças";
                break;
            case "Escalofríos Nocturnos":
                r = "Calafrios Noturnos";
                break;
            case "Noche en Vela":
                r = "Noite em Claro";
                break;
            case "Práctica Imprudente":
                r = "Prática Imprudente";
                break;
            case "Bolsa Olvidada":
                r = "Bolsa Esquecida";
                break;
            case "Lección junto al Fuego":
                r = "Lição junto ao Fogo";
                break;
            case "Palabras Necesarias":
                r = "Palavras Necessárias";
                break;
            case "Brasas Errantes":
                r = "Brasas Errantes";
                break;
            case "Tronco Reavivado":
                r = "Tronco Reaceso";
                break;
            case "Calor de las Cenizas":
                r = "Calor das Cinzas";
                break;
            case "Hongos del Carbón":
                r = "Fungos do Carvão";
                break;
            case "Cánticos de Madrugada":
                r = "Cânticos da Madrugada";
                break;
            case "Huellas alrededor del Campamento":
                r = "Pegadas ao redor do Acampamento";
                break;
            case "Vigilia Helada":
                r = "Vigília Gelada";
                break;
            case "Símbolo en la Nieve":
                r = "Símbolo na Neve";
                break;
            case "Paredón contra el Viento":
                r = "Paredão contra o Vento";
                break;
            case "Paso en Silencio":
                r = "Passagem em Silêncio";
                break;
            case "Rastro del Rebaño":
                r = "Rastro do Rebanho";
                break;
            case "Aurora del Paso":
                r = "Aurora da Passagem";
                break;
            case "Golpes Bajo el Empedrado":
                r = "Golpes sob o Calçamento";
                break;
            case "Brecha en la Calzada":
                r = "Brecha na Calçada";
                break;
            case "Ecos en el Pozo":
                r = "Ecos no Poço";
                break;
            case "Campanas sin Torre":
                r = "Sinos sem Torre";
                break;
            case "Acecho en los Tejados":
                r = "Ã€ Espreita nos Telhados";
                break;
            case "Puerta Astillada":
                r = "Porta Estilhaçada";
                break;
            case "Faroles Prestados":
                r = "Lampiões Emprestados";
                break;
            case "Barricada Todavía en Pie":
                r = "Barricada Ainda de Pé";
                break;
            case "Sótano con Vida":
                r = "Porão com Vida";
                break;
            case "Señales de Tiza":
                r = "Sinais de Giz";
                break;
            case "Valor en la Plaza":
                r = "Valor na Praça";
                break;
            case "Campamento entre Columnas":
                r = "Acampamento entre Colunas";
                break;
            case "Luz Ahogada":
                r = "Luz Sufocada";
                break;
            case "Techo Inestable":
                r = "Teto Instável";
                break;
            case "Ruidos en la Bodega":
                r = "Ruídos na Adega";
                break;
            case "Lista en la Pared":
                r = "Lista na Parede";
                break;
            case "Ventana Vigilante":
                r = "Janela de Vigia";
                break;
            case "Fogón entre Escombros":
                r = "Fogão entre Escombros";
                break;
            case "Patio de Evacuación":
                r = "Pátio de Evacuação";
                break;
            case "Carta sin Enviar":
                r = "Carta sem Enviar";
                break;
            case "Siluetas entre los Arboles":
                r = "Silhuetas entre as Árvores";
                break;
            case "Barro que Retiene":
                r = "Barro que Prende";
                break;
            case "Promesa Incumplida":
                r = "Promessa Não Cumprida";
                break;
            case "Rutina Floja":
                r = "Rotina Frouxa";
                break;
            case "Pesadillas Compartidas":
                r = "Pesadelos Compartilhados";
                break;
            case "Descanso Incompleto":
                r = "Descanso Incompleto";
                break;
            case "Quejas en Voz Baja":
                r = "Queixas em Voz Baixa";
                break;
            case "Fogatas Demasiado Lejos":
                r = "Fogueiras Distantes Demais";
                break;
            case "Arenga en la Lluvia":
                r = "Discurso na Chuva";
                break;
            case "Camino a Favor":
                r = "Caminho a Favor";
                break;
            case "Juramento de la Escolta":
                r = "Juramento da Escolta";
                break;
            case "Rastro Sospechoso":
                r = "Rastro Suspeito";
                break;
            case "Circulo de Historias":
                r = "Círculo de Histórias";
                break;
            case "Campamento Ligero":
                r = "Acampamento Leve";
                break;
            case "Repaso de Maniobras":
                r = "Revisão de Manobras";
                break;
            case "Guardias Relevados":
                r = "Guardas Revezados";
                break;
            case "Inspiración":
                r = "Inspiração";
                break;

            case "Presteza":
                r = "Presteza";
                break;
            case "Compromiso":
                r = "Compromisso";
                break;
            case "Vigilante":
                r = "Vigilante";
                break;
            case "Acobardados":
                r = "Acovardados";
                break;
            case "Aletargados":
                r = "Letárgicos";
                break;
            case "Desmotivación":
                r = "Desmotivação";
                break;

            case "Descuidados":
                r = "Descuidados";
                break;
            case "Acumulaciones: ":
                r = "Acúmulos: ";
                break;
            case "+2 VAL a toda la Caravana en el próximo combate.":
                r = "+2 VAL para toda a Caravana no próximo combate.";
                break;
            case "El Aliento Negro no avanza en el próximo viaje.":
                r = "O Respiro Negro não avança na próxima viagem.";
                break;
            case "+20% Experiencia en el próximo combate.":
                r = "+20% Experiência no próximo combate.";
                break;
            case "+10% Exploración y -10% emboscadas durante 1 viaje.":
                r = "+10% Exploração e -10% emboscadas durante 1 viagem.";
                break;
            case "-2 VAL a toda la Caravana en el próximo combate.":
                r = "-2 VAL para toda a Caravana no próximo combate.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje.":
                r = "+1 avanço do Respiro Negro na próxima viagem.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje y marcha visual más lenta.":
                r = "+1 avanço do Respiro Negro na próxima viagem e marcha visual mais lenta.";
                break;
            case "-20% Experiencia en el próximo combate.":
                r = "-20% Experiência no próximo combate.";
                break;
            case "-10% Exploración y +10% emboscadas durante 1 viaje.":
                r = "-10% Exploração e +10% emboscadas durante 1 viagem.";
                break;
            case "-La Caravana cerro filas, pero el miedo quedo instalado. Acobardados para el proximo combate.":
                r = "-A Caravana fechou fileiras, mas o medo permaneceu. Acovardados para o próximo combate.";
                break;
            case "-La Caravana forzo el paso entre el barro y quedo Aletargada.":
                r = "-A Caravana forçou passagem pela lama e ficou Letárgica.";
                break;
            case "-La escena de la promesa incumplida dejo a la Caravana Desmotivada.":
                r = "-A cena da promessa não cumprida deixou a Caravana Desmotivada.";
                break;
            case "-La escena de la promesa incumplida dejo a la Caravana desmotivada.":
                r = "-A cena da promessa não cumprida deixou a Caravana desmotivada.";
                break;
            case "-La rutina floja se impuso y la Caravana quedo Descuidados por 1 viaje.":
                r = "-A rotina frouxa se impÃ´s e a Caravana ficou Descuidados por 1 viagem.";
                break;
            case "-Las quejas en voz baja drenaron el animo. Desmotivacion para el proximo combate.":
                r = "-As queixas em voz baixa drenaram o ânimo. Desmotivação para o próximo combate.";
                break;
            case "-Las quejas en voz baja drenaron el animo. Desmotivación para el proximo combate.":
                r = "-As queixas em voz baixa drenaram o ânimo. Desmotivação para o próximo combate.";
                break;
            case " sostuvo la arenga bajo la lluvia (1d20: ":
                r = " sustentou o discurso sob a chuva (1d20: ";
                break;
            case "). +30 Experiencia e Inspiracion para el proximo combate.":
                r = "). +30 Experiência e Inspiração para o próximo combate.";
                break;
            case "). +30 Experiencia e Inspiración para el proximo combate.":
                r = "). +30 Experiência e Inspiração para o próximo combate.";
                break;
            case " no logro encender a todos con su arenga (1d20: ":
                r = " não conseguiu inspirar todos com o discurso (1d20: ";
                break;
            case "), pero la Caravana sostuvo el animo. +2 Esperanza.":
                r = "), mas a Caravana manteve o ânimo. +2 Esperança.";
                break;
            case "-La arenga logro encender a la Caravana. Inspiracion para el proximo combate.":
                r = "-O discurso inflamou a Caravana. Inspiração para o próximo combate.";
                break;
            case "-La arenga logro encender a la Caravana. Inspiración para el proximo combate.":
                r = "-O discurso inflamou a Caravana. Inspiração para o próximo combate.";
                break;
            case "-La Caravana aprovecho el buen tramo de camino y obtuvo Presteza.":
                r = "-A Caravana aproveitou o bom trecho do caminho e obteve Presteza.";
                break;
            case "-El juramento de la escolta reforzo el Compromiso de la Caravana.":
                r = "-O juramento da escolta reforçou o Compromisso da Caravana.";
                break;
            case " leyo el rastro a tiempo (1d20: ":
                r = " leu o rastro a tempo (1d20: ";
                break;
            case "). +30 Experiencia y Vigilante por 1 viaje.":
                r = "). +30 Experiência e Vigilante por 1 viagem.";
                break;
            case " no logro leer bien el rastro (1d20: ":
                r = " não conseguiu ler bem o rastro (1d20: ";
                break;
            case "). +1 Fatiga.":
                r = "). +1 Fadiga.";
                break;
            case "-La Caravana ajusto la vigilancia tras ver el rastro.":
                r = "-A Caravana reforçou a vigilância após ver o rastro.";
                break;
            case " dirigio un repaso de maniobras util (1d20: ":
                r = " conduziu uma revisão útil de manobras (1d20: ";
                break;
            case "). +35 Experiencia y Compromiso.":
                r = "). +35 Experiência e Compromisso.";
                break;
            case " no logro ordenar bien el repaso de maniobras. +1 Fatiga.":
                r = " não conseguiu organizar bem a revisão de manobras. +1 Fadiga.";
                break;
            case "-El repaso de maniobras dejo a la Caravana con mas Compromiso.":
                r = "-A revisão de manobras deixou a Caravana com mais Compromisso.";
                break;
            case "-El humo inquieto dejo a la Caravana Aletargada.":
                r = "-A fumaça inquieta deixou a Caravana Letárgica.";
                break;
            case " rompio el mal augurio de los cuervos. +2 Esperanza.":
                r = " rompeu o mau presságio dos corvos. +2 Esperança.";
                break;
            case " no logro cortar el malestar de la Caravana. Acobardados para el proximo combate.":
                r = " não conseguiu dissipar o mal-estar da Caravana. Acovardados para o próximo combate.";
                break;
            case "-Los cuervos del paso dejaron a la Caravana Acobardada.":
                r = "-Os corvos da passagem deixaram a Caravana Acovardada.";
                break;
            case "-El eco bajo los pies desordeno la marcha. Descuidados por 1 viaje.":
                r = "-O eco sob os pés desordenou a marcha. Descuidados por 1 viagem.";
                break;
            case "-La veta de resina permitio ordenar una salida rapida. Presteza.":
                r = "-O veio de resina permitiu organizar uma saída rápida. Presteza.";
                break;
            case " vigilo desde el hielo alto y ordeno la marcha. +30 Experiencia y Vigilante.":
                r = " vigiou do alto do gelo e organizou a marcha. +30 Experiência e Vigilante.";
                break;
            case " bajo agotado del filo helado. +1 Fatiga.":
                r = " desceu exausto da borda gelada. +1 Fadiga.";
                break;
            case "-La Caravana logro una mejor vigilancia desde el hielo.":
                r = "-A Caravana conseguiu uma vigilância melhor a partir do gelo.";
                break;
            case "-La senal de los resistentes reforzo el Compromiso de la Caravana.":
                r = "-O sinal dos resistentes reforçou o Compromisso da Caravana.";
                break;
            case "-La señal de los resistentes reforzo el Compromiso de la Caravana.":
                r = "-O sinal dos resistentes reforçou o Compromisso da Caravana.";
                break;
            case "-La señal de los resistentes reforzó el Compromiso de la Caravana.":
                r = "-O sinal dos resistentes reforçou o Compromisso da Caravana.";
                break;
            case "-La noche dejo a la Caravana Acobardada para el proximo combate.":
                r = "-A noite deixou a Caravana Acovardada para o próximo combate.";
                break;
            case "-El descanso incompleto dejo a la Caravana Aletargada.":
                r = "-O descanso incompleto deixou a Caravana Letárgica.";
                break;
            case "-Las fogatas demasiado lejos dejaron a la Caravana Descuidados por 1 viaje.":
                r = "-As fogueiras distantes demais deixaram a Caravana Descuidados por 1 viagem.";
                break;
            case "-Las historias junto al fuego dejaron a la Caravana con Inspiracion.":
                r = "-As histórias junto ao fogo deixaram a Caravana com Inspiração.";
                break;
            case "-Las historias junto al fuego dejaron a la Caravana con Inspiración.":
                r = "-As histórias junto ao fogo deixaram a Caravana com Inspiração.";
                break;
            case "-El campamento ligero dejo a la Caravana lista para avanzar con Presteza.":
                r = "-O acampamento leve deixou a Caravana pronta para avançar com Presteza.";
                break;
            case "-Los guardias relevados dejaron a la Caravana Vigilante.":
                r = "-Os guardas revezados deixaram a Caravana Vigilante.";
                break;
            case "-El presagio de los cuervos dejo a la Caravana Acobardada.":
                r = "-O presságio dos corvos deixou a Caravana Acovardada.";
                break;
            case "-La respuesta dejada en Nedukazal inspiro a la Caravana.":
                r = "-A resposta deixada em Nedukazal inspirou a Caravana.";
                break;
            case "-La respuesta dejada en Nedukazal inspiró a la Caravana.":
                r = "-A resposta deixada em Nedukazal inspirou a Caravana.";
                break;
            case "-La Presteza de la Caravana ha evitado el avance del Aliento Negro durante el viaje.":
                r = "-A Presteza da Caravana evitou o avanço do Respiro Negro durante a viagem.";
                break;
            case "-La derrota dejó a la Caravana con Acobardados.":
                r = "-A derrota deixou a Caravana com Acovardados.";
                break;
            case "-La derrota dejó a la Caravana con Aletargados.":
                r = "-A derrota deixou a Caravana com Letárgicos.";
                break;
            case "-La derrota dejó a la Caravana con Desmotivación.":
                r = "-A derrota deixou a Caravana com Desmotivação.";
                break;
            case "-La derrota dejó a la Caravana con Descuidados.":
                r = "-A derrota deixou a Caravana com Descuidados.";
                break;
            case "+2 VAL a toda la Caravana en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "+2 VAL para toda a Caravana no próximo combate. Consome 1 acúmulo ao iniciar um combate.";
                break;
            case "El Aliento Negro no avanza en el próximo viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "O Respiro Negro não avança na próxima viagem. Consome 1 acúmulo ao iniciar uma viagem.";
                break;
            case "+20% Experiencia en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "+20% Experiência no próximo combate. Consome 1 acúmulo ao iniciar um combate.";
                break;
            case "+10% Exploración y -10% emboscadas durante 1 viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "+10% Exploração e -10% emboscadas durante 1 viagem. Consome 1 acúmulo ao iniciar uma viagem.";
                break;
            case "-2 VAL a toda la Caravana en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "-2 VAL para toda a Caravana no próximo combate. Consome 1 acúmulo ao iniciar um combate.";
                break;
            case "+1 avance del Aliento Negro en el próximo viaje y marcha visual más lenta. Consume 1 acumulación al iniciar un viaje.":
                r = "+1 avanço do Respiro Negro na próxima viagem e marcha visual mais lenta. Consome 1 acúmulo ao iniciar uma viagem.";
                break;
            case "-20% Experiencia en el próximo combate. Consume 1 acumulación al iniciar un combate.":
                r = "-20% Experiência no próximo combate. Consome 1 acúmulo ao iniciar um combate.";
                break;
            case "-10% Exploración y +10% emboscadas durante 1 viaje. Consume 1 acumulación al iniciar un viaje.":
                r = "-10% Exploração e +10% emboscadas durante 1 viagem. Consome 1 acúmulo ao iniciar uma viagem.";
                break;
            case "Humo Inquieto":
                r = "Fumaça Inquieta";
                break;
            case "Cuervos del Paso":
                r = "Corvos da Passagem";
                break;
            case "Eco Bajo los Pies":
                r = "Eco sob os Pés";
                break;
            case "Veta de Resina":
                r = "Veio de Resina";
                break;
            case "Vigia del Hielo":
                r = "Vigia do Gelo";
                break;
            case "Señal de los Resistentes":
                r = "Sinal dos Resistentes";
                break;
            case "Al costado del camino, varias siluetas se mueven entre la maleza justo fuera del alcance de la vista. Nadie logra confirmar si hay una amenaza real o solo trucos de la mente.\n\n":
                r = "Ao lado do caminho, várias silhuetas se movem entre a vegetação logo além do alcance da vista. Ninguém consegue confirmar se há uma ameaça real ou apenas trucos da mente.\n\n";
                break;
            case "Los rumores corren rápido entre los carros y varios Civiles ya esperan un ataque inminente.\n\n":
                r = "Os rumores correm rápido entre as carroças e vários Civis já esperam um ataque iminente.\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas cerrar filas y seguir, la Caravana obtendrá Acobardados para el próximo combate. -2 VAL a todos.</color>\n\n":
                r = "<color=#ba3fef>-Se você ordenar fechar fileiras e seguir, a Caravana obterá Acovardados para o próximo combate. -2 VAL para todos.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides frenar para revisar, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-Se você decidir parar para verificar, o Respiro Negro avançará.</color>\n\n";
                break;
            case "Un tramo de barro pegajoso se agarra a ruedas, botas y arreos. Cada metro parece costar el doble, y la columna entera empieza a moverse con una pesadez desesperante.\n\n":
                r = "Um trecho de barro pegajoso se agarra a rodas, botas e arreios. Cada metro parece custar o dobro, e a coluna inteira começa a se mover com um peso desesperador.\n\n";
                break;
            case "<color=#ba3fef>-Si fuerzas la marcha igual, la Caravana obtendra Aletargados. El Aliento Negro avanzara +1 en el proximo viaje y la marcha se vera mas lenta.</color>\n\n":
                r = "<color=#ba3fef>-Se você forçar a marcha mesmo assim, a Caravana obterá Letargia. O Respiro Negro avançará +1 na próxima viagem e a marcha parecerá mais lenta.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas reacomodar la marcha, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se você ordenar reorganizar a marcha, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "Encuentran un punto de espera abandonado: una manta, un fogon apagado y una senal vieja que promete ayuda que nunca llego. La escena cae pesado sobre la Caravana.\n\n":
                r = "Eles encontram um ponto de espera abandonado: um cobertor, um fogo apagado e um sinal antigo que prometia ajuda que nunca chegou. A cena pesa sobre a Caravana.\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin detenerte, la Caravana obtendra Desmotivacion. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-Se você decidir seguir sem parar, a Caravana obterá Desmotivação. Ganhará 20% menos Experiência no próximo combate.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin detenerte, la Caravana obtendra Desmotivación. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-Se você decidir seguir sem parar, a Caravana obterá Desmotivação. Ganhará 20% menos Experiência no próximo combate.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si haces una breve parada para ordenar el paso, el Aliento Negro avanzara.</color>\n\n":
                r = "<color=#ba3fef>-Se você fizer uma breve parada para organizar a marcha, o Respiro Negro avançará.</color>\n\n";
                break;
            case "Tras varias horas sin sobresaltos, parte de la Caravana empieza a moverse por pura costumbre. Se aflojan formaciones, cambian relevos tarde y mas de uno deja de mirar el terreno con atencion.\n\n":
                r = "Depois de várias horas sem sobressaltos, parte da Caravana começa a se mover por puro hábito. As formações se afrouxam, as trocas de turno se atrasam e mais de um deixa de observar o terreno com atenção.\n\n";
                break;
            case "<color=#ba3fef>-Si no dices nada, la Caravana obtendra Descuidados por 1 viaje. -10% Exploracion y +10% emboscadas.</color>\n\n":
                r = "<color=#ba3fef>-Se você não disser nada, a Caravana obterá Descuidados por 1 viagem. -10% Exploração e +10% emboscadas.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si reorganizas puestos y ritmo, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se você reorganizar posições e ritmo, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "Durante la noche, gritos ahogados despiertan a medio campamento. Al amanecer nadie logra explicar bien lo que soño, pero el miedo queda flotando igual entre las tiendas.\n\n":
                r = "Durante a noite, gritos abafados despertam metade do acampamento. Ao amanhecer ninguém consegue explicar direito o que sonhou, mas o medo continua pairando entre as tendas.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Acobardados para el proximo combate. -2 VAL a todos.</b></color>":
                r = "<color=#ba3fef><b>A Caravana obtém Acovardados para o próximo combate. -2 VAL para todos.</b></color>";
                break;
            case "El suelo es incomodo, el viento no afloja y los carros crujen toda la noche. Nadie descansa de verdad, y la Caravana se levanta con la sensacion de haber dormido a medias.\n\n":
                r = "O chão é desconfortável, o vento não cede e as carroças rangem a noite toda. Ninguém descansa de verdade, e a Caravana se levanta com a sensação de ter dormido pela metade.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Aletargados. El Aliento Negro avanzara +1 en el proximo viaje y la marcha se vera mas lenta.</b></color>":
                r = "<color=#ba3fef><b>A Caravana obtém Letargia. O Respiro Negro avançará +1 na próxima viagem e a marcha parecerá mais lenta.</b></color>";
                break;
            case "Lo que empieza como murmullo termina recorriendo el campamento entero: cansancio, dudas, comparaciones con dias mejores. No hay gritos ni desbande, solo una erosion lenta del animo.\n\n":
                r = "O que começa como um murmúrio acaba percorrendo o acampamento inteiro: cansaço, dúvidas, comparações com dias melhores. Não há gritos nem debandada, só uma erosão lenta do ânimo.\n\n";
                break;
            case "<color=#ba3fef>-Si dejas que se descarguen, la Caravana obtendra Desmotivacion. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-Se você deixar que desabafem, a Caravana obterá Desmotivação. Ganhará 20% menos Experiência no próximo combate.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si dejas que se descarguen, la Caravana obtendra Desmotivación. Ganara 20% menos Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#ba3fef>-Se você deixar que desabafem, a Caravana obterá Desmotivação. Ganhará 20% menos Experiência no próximo combate.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas que se descarguen, la Caravana obtendrá 1 estado positivo aleatorio.</color>\n\n":
                r = "<color=#a0e812>-Se você deixar que desabafem, a Caravana obterá um estado positivo aleatório.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si cortas la charla y apagas el fuego, la Caravana perdera 9 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se você cortar a conversa e apagar o fogo, a Caravana perderá 9 Esperança.</color>\n\n";
                break;
            case "El campamento queda armado demasiado disperso. Las fogatas no se cubren entre si, los llamados tardan en llegar y cuesta saber quien esta atento y quien no.\n\n":
                r = "O acampamento fica montado de forma espalhada demais. As fogueiras não se cobrem entre si, os chamados demoram a chegar e fica difícil saber quem está atento e quem não está.\n\n";
                break;
            case "<color=#ba3fef><b>La Caravana obtiene Descuidados por 1 viaje. -10% Exploracion y +10% emboscadas.</b></color>":
                r = "<color=#ba3fef><b>A Caravana obtém Descuidados por 1 viagem. -10% Exploração e +10% emboscadas.</b></color>";
                break;
            case "La marcha se sostiene bajo una lluvia pesada y muda. Los Civiles avanzan con la cabeza gacha, hasta que alguien propone decir unas palabras antes de que el desaliento se vuelva costumbre.\n\n":
                r = "A marcha segue sob uma chuva pesada e muda. Os Civis avançam de cabeça baixa, até que alguém propõe dizer algumas palavras antes que o desânimo vire costume.\n\n";
                break;
            case "</color></b> puede intentar levantar a la Caravana.\n\n":
                r = "</color></b> pode tentar animar a Caravana.\n\n";
                break;
            case "<color=#a0e812>-Tirada de Salvacion: TS Mental DC ":
                r = "<color=#a0e812>-Teste de Resistência: TR Mental CD ";
                break;
            case ").</i> Si la supera, la Caravana obtendra Inspiracion para el proximo combate y ganara 30 Experiencia. Si falla, solo obtendra +2 Esperanza.</color>\n\n":
                r = ").</i> Se passar, a Caravana obterá Inspiração para o próximo combate e ganhará 30 Experiência. Se falhar, obterá apenas +2 Esperança.</color>\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Inspiración para el proximo combate y ganara 30 Experiencia. Si falla, solo obtendra +2 Esperanza.</color>\n\n":
                r = ").</i> Se passar, a Caravana obterá Inspiração para o próximo combate e ganhará 30 Experiência. Se falhar, obterá apenas +2 Esperança.</color>\n\n";
                break;
            case "<color=#a0e812>-Si lo intentas, un Heroe hara una Tirada de Salvacion Mental DC 11. Si la supera, la Caravana obtendra Inspiración y ese Heroe ganara 30 Experiencia.</color>\n\n":
                r = "<color=#a0e812>-Se você tentar, um Herói fará um Teste de Resistência Mental CD 11. Se passar, a Caravana obterá Inspiração e esse Herói ganhará 30 Experiência.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no detener la marcha, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você decidir não parar a marcha, +3 Esperança.</color>\n\n";
                break;
            case "La Caravana encuentra un tramo de camino firme, bien orientado y sorprendentemente limpio. No durara mucho, pero alcanza para ordenar la columna y pensar en un proximo avance veloz.\n\n":
                r = "A Caravana encontra um trecho de caminho firme, bem orientado e surpreendentemente limpo. Não vai durar muito, mas é suficiente para organizar a coluna e pensar em um avanço rápido em seguida.\n\n";
                break;
            case "<color=#a0e812>-Si aprovechas el ritmo que da el terreno, la Caravana obtendra Presteza. El Aliento Negro no avanzara en el proximo viaje.</color>\n\n":
                r = "<color=#a0e812>-Se você aproveitar o ritmo que o terreno oferece, a Caravana obterá Presteza. O Respiro Negro não avançará na próxima viagem.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres revisar bien los bordes del camino, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você preferir revisar bem as margens do caminho, +3 Esperança.</color>\n\n";
                break;
            case "Antes de retomar la marcha, dos Heroes se ofrecen a formalizar delante de la Caravana un juramento sencillo: no ceder terreno mientras quede alguien a quien proteger.\n\n":
                r = "Antes de retomar a marcha, dois Heróis se oferecem para formalizar diante da Caravana um juramento simples: não ceder terreno enquanto restar alguém a proteger.\n\n";
                break;
            case "<color=#a0e812>-Si aceptas el juramento, la Caravana obtendra Compromiso. Ganara 20% mas Experiencia en el proximo combate.</color>\n\n":
                r = "<color=#a0e812>-Se você aceitar o juramento, a Caravana obterá Compromisso. Ganhará 20% mais Experiência no próximo combate.</color>\n\n";
                break;
            case "<color=#a0e812>-Si les pides reservar fuerzas y seguir, +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você pedir que guardem forças e sigam, +4 Esperança.</color>\n\n";
                break;
            case "Unas marcas recientes junto al camino sugieren que alguien o algo estuvo siguiendo la columna desde hace rato. La noticia corre rapido entre quienes van en los carros traseros.\n\n":
                r = "Marcas recentes ao lado do caminho sugerem que alguém ou alguma coisa vem seguindo a coluna há algum tempo. A notícia corre rápido entre quem vai nas carroças de trás.\n\n";
                break;
            case "</color></b> puede leer el rastro y ordenar a tiempo la vigilancia.\n\n":
                r = "</color></b> pode ler o rastro e organizar a vigilância a tempo.\n\n";
                break;
            case "<color=#a0e812>-Tirada de Salvacion: TS Reflejos DC ":
                r = "<color=#a0e812>-Teste de Resistência: TR Reflexos CD ";
                break;
            case ").</i> Si la supera, la Caravana obtendra Vigilante por 1 viaje y ganara 30 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> Se passar, a Caravana obterá Vigilante por 1 viagem e ganhará 30 Experiência. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no detenerte, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você decidir não parar, +2 Esperança.</color>\n\n";
                break;
            case "Alguien empieza a contar una historia vieja junto al fuego. Otra voz corrige un detalle, otra suma un recuerdo, y pronto media Caravana esta escuchando con una sonrisa cansada.\n\n":
                r = "Alguém começa a contar uma história antiga junto ao fogo. Outra voz corrige um detalhe, outra soma uma lembrança, e logo metade da Caravana está ouvindo com um sorriso cansado.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Inspiracion para el proximo combate. +2 VAL a todos.</b></color>":
                r = "<color=#a0e812><b>A Caravana obtém Inspiração para o próximo combate. +2 VAL para todos.</b></color>";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Inspiración para el proximo combate. +2 VAL a todos.</b></color>":
                r = "<color=#a0e812><b>A Caravana obtém Inspiração para o próximo combate. +2 VAL para todos.</b></color>";
                break;
            case "Sin que nadie lo ordene demasiado, el campamento se arma con lo justo y queda listo para levantarse en minutos. Hay una sensacion compartida de que manana convendra moverse rapido.\n\n":
                r = "Sem que ninguém precise mandar muito, o acampamento é montado com o essencial e fica pronto para ser desmontado em minutos. Há uma sensação compartilhada de que hoje vai valer a pena se mover rápido.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Presteza. El Aliento Negro no avanzara en el proximo viaje.</b></color>":
                r = "<color=#a0e812><b>A Caravana obtém Presteza. O Respiro Negro não avançará na próxima viagem.</b></color>";
                break;
            case "Antes de dormir, un Heroe propone repasar senales, posiciones y respuestas rapidas junto al fuego. No cambia el cansancio, pero podria dejar a todos mejor parados para el proximo choque.\n\n":
                r = "Antes de dormir, um Herói propõe revisar sinais, posições e respostas rápidas junto ao fogo. Isso não muda o cansaço, mas pode deixar todos melhor preparados para o próximo choque.\n\n";
                break;
            case "</color></b> puede dirigir el repaso.\n\n":
                r = "</color></b> pode conduzir a revisão.\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Compromiso y ganara 35 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> Se passar, a Caravana obterá Compromisso e ganhará 35 Experiência. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres descansar de inmediato, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você preferir descansar de imediato, +2 Esperança.</color>\n\n";
                break;
            case "Los turnos de guardia salen mejor de lo esperado. Nadie queda de mas, nadie llega tarde y el campamento entero se siente mas atento sin perder descanso.\n\n":
                r = "Os turnos de guarda saem melhor do que o esperado. Ninguém fica tempo demais, ninguém chega tarde e o acampamento inteiro se sente mais atento sem perder descanso.\n\n";
                break;
            case "<color=#a0e812><b>La Caravana obtiene Vigilante por 1 viaje. +10% Exploracion y -10% emboscadas.</b></color>":
                r = "<color=#a0e812><b>A Caravana obtém Vigilante por 1 viagem. +10% Exploração e -10% emboscadas.</b></color>";
                break;
            case "En el Bosque Ardiente, el humo cambia de direccion de golpe y se mete bajo telas, capuchas y lonas. La Caravana avanza entre toses y ojos llorosos, cada vez mas lenta.\n\n":
                r = "No Bosque Ardente, a fumaça muda de direção de repente e entra por baixo de tecidos, capuzes e lonas. A Caravana avança entre tosses e olhos lacrimejantes, cada vez mais lenta.\n\n";
                break;
            case "<color=#ba3fef>-Si decides avanzar igual, la Caravana obtendra Aletargados.</color>\n\n":
                r = "<color=#ba3fef>-Se você decidir avançar mesmo assim, a Caravana obterá Letargia.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si haces una parada corta hasta que abra el aire, el Aliento Negro avanzara.</color>\n\n":
                r = "<color=#ba3fef>-Se você fizer uma parada curta até o ar abrir, o Respiro Negro avançará.</color>\n\n";
                break;
            case "Un circulo de cuervos se posa cerca del camino y no se mueve aunque la Caravana se acerque. Su quietud resulta peor que cualquier graznido, y el presagio corre rapido entre los Civiles.\n\n":
                r = "Um círculo de corvos pousa perto do caminho e não se move mesmo quando a Caravana se aproxima. A quietude deles é pior do que qualquer grasnado, e o presságio corre rápido entre os Civis.\n\n";
                break;
            case "</color></b> puede romper el malestar antes de que prenda.\n\n":
                r = "</color></b> pode quebrar o mal-estar antes que ele se espalhe.\n\n";
                break;
            case ").</i> Si la supera, +2 Esperanza. Si falla, la Caravana obtendra Acobardados.</color>\n\n":
                r = ").</i> Se passar, +2 Esperança. Se falhar, a Caravana obterá Acovardados.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, un Heroe hara una Tirada de Salvacion Mental DC 11. Si falla, la Caravana obtendra Acobardados.</color>\n\n":
                r = "<color=#ba3fef>-Se você tentar, um Herói fará um Teste de Resistência Mental CD 11. Se falhar, a Caravana obterá Acovardados.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir sin mirarlos, la Caravana obtendra Acobardados.</color>\n\n":
                r = "<color=#ba3fef>-Se você decidir seguir sem olhar para eles, a Caravana obterá Acovardados.</color>\n\n";
                break;
            case "En Nedukazal, un golpeteo hueco sube desde abajo de la tierra y vuelve a cortarse antes de que alguien lo ubique. La reaccion inmediata es apurar el paso, pero no todos conservan la disciplina al hacerlo.\n\n":
                r = "Em Nedukazal, uma batida oca sobe de baixo da terra e volta a se cortar antes que alguém localize a origem. A reação imediata é apressar o passo, mas nem todos mantêm a disciplina ao fazer isso.\n\n";
                break;
            case "<color=#ba3fef>-Si ordenas avanzar sin mirar atras, la Caravana obtendra Descuidados por 1 viaje.</color>\n\n":
                r = "<color=#ba3fef>-Se você ordenar avançar sem olhar para trás, a Caravana obterá Descuidados por 1 viagem.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si impones una marcha mas cerrada y cauta, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se você impor uma marcha mais fechada e cautelosa, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "Una veta de resina endurecida marca un paso firme entre raices y tierra negra. La ruta apenas se sostiene, pero si la toman bien podria regalarle a la Caravana una salida rapida del sector.\n\n":
                r = "Um veio de resina endurecida marca uma passagem firme entre raízes e terra negra. A rota mal se sustenta, mas se for bem usada pode dar Ã  Caravana uma saída rápida da área.\n\n";
                break;
            case "<color=#a0e812>-Si la aprovechas, la Caravana obtendra Presteza.</color>\n\n":
                r = "<color=#a0e812>-Se você aproveitá-la, a Caravana obterá Presteza.</color>\n\n";
                break;
            case "<color=#a0e812>-Si prefieres cruzar con maxima cautela, +3 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você preferir atravessar com máxima cautela, +3 Esperança.</color>\n\n";
                break;
            case "Un filo de roca y hielo ofrece un punto de vista raro en el Paso. Desde ahi, un ojo atento podria leer mejor el terreno y ordenar la marcha antes de que llegue el peligro.\n\n":
                r = "Uma crista de rocha e gelo oferece um ponto de vista raro na Passagem. Dali, um olhar atento poderia ler melhor o terreno e ordenar a marcha antes que o perigo chegue.\n\n";
                break;
            case "</color></b> puede trepar y vigilar desde arriba.\n\n":
                r = "</color></b> pode subir e vigiar lá de cima.\n\n";
                break;
            case ").</i> Si la supera, la Caravana obtendra Vigilante y ganara 30 Experiencia. Si falla, la Caravana ganara +1 Fatiga.</color>\n\n":
                r = ").</i> Se passar, a Caravana obterá Vigilante e ganhará 30 Experiência. Se falhar, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides no exponer a nadie, +2 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se você decidir não expor ninguém, +2 Esperança.</color>\n\n";
                break;
            case "En una pared semiderruida aparecen marcas recientes: no son de Zarkil ni de viejas rutas, sino senales de gente que todavia resiste y se niega a entregar el reino.\n\n":
                r = "Em uma parede semiderruída aparecem marcas recentes: não são de Zarkil nem de rotas antigas, mas sinais de gente que ainda resiste e se recusa a entregar o reino.\n\n";
                break;
            case "<color=#a0e812>-Si sigues la senal y la tomas como ejemplo, la Caravana obtendra Compromiso.</color>\n\n":
                r = "<color=#a0e812>-Se você seguir o sinal e tomá-lo como exemplo, a Caravana obterá Compromisso.</color>\n\n";
                break;
            case "<color=#a0e812>-Si sigues la señal y la tomas como ejemplo, la Caravana obtendra Compromiso.</color>\n\n":
                r = "<color=#a0e812>-Se você seguir o sinal e tomá-lo como exemplo, a Caravana obterá Compromisso.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas una respuesta para quienes pasen despues, la Caravana obtendra Inspiracion.</color>\n\n":
                r = "<color=#a0e812>-Se você deixar uma resposta para quem passar depois, a Caravana obterá Inspiração.</color>\n\n";
                break;
            case "<color=#a0e812>-Si dejas una respuesta para quienes pasen despues, la Caravana obtendra Inspiración.</color>\n\n":
                r = "<color=#a0e812>-Se você deixar uma resposta para quem passar depois, a Caravana obterá Inspiração.</color>\n\n";
                break;
            case "Comerciante Visitante":
                r = "Mercador Visitante";
                break;
            case "Refuerzo en el Camino":
                r = "Reforço no Caminho";
                break;
            case "Mensaje desde Serria":
                r = "Mensagem de Serria";
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
            case "Esfuerzo":
                r = "Esforço";
                break;
            case "La unidad se ha esforzado.":
                r = "A unidade se esforçou.";
                break;
            case "Continuar":
                r = "Continuar";
                break;
            case "Aprovechar":
                r = "Aproveitar";
                break;
            case "Aprovecharlo":
                r = "Aproveitá-lo";
                break;
            case "Apurar":
                r = "Apressar";
                break;
            case "Avanzar":
                r = "Avançar";
                break;
            case "Cautela":
                r = "Cautela";
                break;
            case "Cortarlas":
                r = "Cortá-las";
                break;
            case "Dejarla":
                r = "Deixá-la";
                break;
            case "Detenerse":
                r = "Parar";
                break;
            case "Escucharlas":
                r = "Escutá-las";
                break;
            case "Forzar":
                r = "Forçar";
                break;
            case "Hablar":
                r = "Falar";
                break;
            case "Leer el rastro":
                r = "Ler o rastro";
                break;
            case "No subir":
                r = "Não subir";
                break;
            case "Ordenar":
                r = "Ordenar";
                break;
            case "Reacomodar":
                r = "Reorganizar";
                break;
            case "Repasar":
                r = "Revisar";
                break;
            case "Reservarse":
                r = "Poupar-se";
                break;
            case "Responder":
                r = "Responder";
                break;
            case "Romper el clima":
                r = "Quebrar o clima";
                break;
            case "Subir":
                r = "Subir";
                break;
            case "Revisarlos":
                r = "Inspecioná-los";
                break;
            case "Revisar":
                r = "Inspecionar";
                break;
            case "Mantener la calma":
                r = "Manter a calma";
                break;
            case "Seguirlo":
                r = "Segui-lo";
                break;
            case "Estudiarlo":
                r = "Estudá-lo";
                break;
            case "Guiar el cruce":
                r = "Guiar a travessia";
                break;
            case "Rodear la brecha":
                r = "Contornar a brecha";
                break;
            case "Investigar":
                r = "Investigar";
                break;
            case "Cerrar filas":
                r = "Fechar fileiras";
                break;
            case "Seguirlas":
                r = "Segui-las";
                break;
            case "Reforzar el camino":
                r = "Reforçar o caminho";
                break;
            case "Asegurarlo":
                r = "Protegê-lo";
                break;
            case "Mover campamento":
                r = "Mover acampamento";
                break;
            case "Atrancar":
                r = "Trancar";
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
            case "Esperar":
                r = "Esperar";
                break;
            case "Apurar el paso":
                r = "Apressar a marcha";
                break;
            case "Cruzar":
                r = "Atravessar";
                break;
            case "Rodear":
                r = "Contornar";
                break;
            case "Prepararse":
                r = "Preparar-se";
                break;
            case "Esconder a los Civiles":
                r = "Esconder os Civis";
                break;
            case "Seguir el rastro":
                r = "Seguir a trilha";
                break;
            case "Mantener la ruta":
                r = "Manter a rota";
                break;
            case "Seguir":
                r = "Seguir";
                break;
            case "Contenerlos":
                r = "Contê-los";
                break;
            case "Apartarse":
                r = "Afastar-se";
                break;
            case "Apagarlo":
                r = "Apagá-lo";
                break;
            case "Abandonar carga":
                r = "Abandonar carga";
                break;
            case "Recolectar":
                r = "Recolher";
                break;
            case "Dejarlo":
                r = "Deixá-lo";
                break;
            case "Doblar guardia":
                r = "Reforçar guarda";
                break;
            case "Dejarlos dormir":
                r = "Deixá-los dormir";
                break;
            case "Secarlas":
                r = "Secá-las";
                break;
            case "Desecharlas":
                r = "Descartá-las";
                break;
            case "Buscarlas":
                r = "Procurá-las";
                break;
            case "Reemplazarlas":
                r = "Substituí-las";
                break;
            case "Organizar":
                r = "Organizar";
                break;
            case "Guardar":
                r = "Guardar";
                break;
            case "Repartir":
                r = "Repartir";
                break;
            // EventosAdmin remaining literals (exact text keys)

            case "Uno de los principales encargados de guiar la caravana y elegir las rutas más seguras accidentalmente perdió sus mapas.\n":
                r = "Um dos principais responsáveis por guiar a caravana e escolher as rotas mais seguras perdeu seus mapas por acidente.\n"; break;
            case "Los demás encargados lo ayudarán a buscarlos ya que esos mapas contiene información crucial de la zona actual, y sin su ayuda la caravana podría perderse.\n\n\n\n\n\n\n":
                r = "Os outros responsáveis vío ajudá-lo a procurá-los, já que esses mapas contêm informações cruciais sobre a área atual, e sem eles a caravana pode se perder.\n\n\n\n\n\n\n"; break;
            case "Obtendrá el estado Enfermo por 4-7 días. Cada nivel del Séquito de Curanderos reducirá el tiempo de recuperación en 1 día.\n\n\n\n\n":
                r = "Receberá o estado Doente por 4-7 dias. Cada ní­vel do Séquito de Curandeiros reduzirá o tempo de recuperaçío em 1 dia.\n\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes comprar medicina por 45 Oro para reducir la Enfermedad un día extra.</color>\n\n":
                r = "<color=#ba3fef>-Você pode comprar remédio por 45 de Ouro para reduzir a Doença em 1 dia extra.</color>\n\n"; break;
            case "Al grito de un guardia, tu atención se vuelve a uno de los carros que lleva las arcas con el oro de la caravana. Uno de sus cofres está volcado y el oro se ha derramado por el suelo. Aparentemente durante la noche, alguien logró forzarlo y se llevó parte del botón.\n\n":
                r = "Ao grito de um guarda, sua atençío se volta para uma das carroças que leva os baús com o ouro da caravana. Um de seus cofres está virado, e o ouro se espalhou pelo chío. Aparentemente, durante a noite, alguém conseguiu arrombá-lo e levou parte do saque.\n\n"; break;
            case "<color=#ba3fef>-Puedes someter a los Civiles a un interrogatorio para tratar de encontrar al ladrón:\n\n Se perdería 5 de Esperanza, <i>":
                r = "<color=#ba3fef>-Você pode submeter os Civis a um interrogatório para tentar encontrar o ladrío:\n\n Você perderia 5 de Esperança, <i>"; break;
            case "% Chances (40 base + Milicianos)</i> de encontrar al culpable y recuperar el oro, -1 Civil por destierro.</color>\n\n":
                r = "% de chance (40 base + Milicianos)</i> de encontrar o culpado e recuperar o ouro, -1 Civil por banimento.</color>\n\n"; break;
            case "Tras un estruendo, volteas la cabeza hacia atrás y ves que uno de los carros de suministros de la caravana ha sufrido un accidente. Las ruedas están atascadas en el barro y el carro parece haberse perdido definitivamente.\n\n":
                r = "Após um estrondo, você vira a cabeça para trás e vê que uma das carroças de suprimentos da caravana sofreu um acidente. As rodas estío atoladas na lama, e a carroça parece estar perdida para sempre.\n\n"; break;
            case "<color=#ba3fef>-Puedes pasar los 60 suministros caídos a otro carro, sacrificando 20 Materiales; o asumir la pérdida de suministros.</color>\n\n":
                r = "<color=#ba3fef>-Você pode passar os 60 suprimentos caídos para outra carroça, sacrificando 20 Materiais; ou assumir a perda dos suprimentos.</color>\n\n"; break;
            case "La Caravana encuentra un Río con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.\n\n":
                r = "A Caravana encontra um Rio com bom fluxo e água que parece aceitável. Vários civis, animados, começam a ir até lá com a intençío de descansar e se refrescar.\n\n"; break;
            case "El agua podría estar contaminada por el Aliento Negro. Puedes negarle a los Civiles el acceso al agua o dejarlos a su propia suerte.\n\n":
                r = "A água pode estar contaminada pelo Respiro Negro. Você pode negar aos Civis o acesso á água ou deixá-los á própria sorte.\n\n"; break;
            case "<color=#ba3fef>-Si les niegas el acceso perderás 15 de Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se negar o acesso, você perderá 15 de Esperança.</color>\n\n"; break;
            case "<color=#ba3fef>-Si los dejas ir, hay un %":
                r = "<color=#ba3fef>-Se você deixá-los ir, há %"; break;
            case "<i>(Determinado por Aliento Negro)</i> de que se contaminen y mueran 25 Civiles. Si no está contaminada descansarán (-1 Fatiga).</color>\n\n":
                r = " <i>(Determinado pelo Respiro Negro)</i> de chance de que se contaminem e 25 Civis morram. Se nío estiver contaminada, eles descansarío (-1 Fadiga).</color>\n\n"; break;
            case "\nAparentemente tuvieron un incidente durante un entrenamiento leve que se dispusieron a realizar y en el cual ambos se lastimaron levemente.\n\n":
                r = "\nAparentemente, eles tiveram um incidente durante um treino leve que decidiram fazer, no qual ambos se machucaram levemente.\n\n"; break;
            case "La tensión sube y los demás caravaneros miran con incomodidad. Ambos reclaman tener la razón y esperan tu juicio.\n\n":
                r = "A tensío aumenta, e os demais integrantes da caravana observam com desconforto. Ambos afirmam estar certos e aguardam seu julgamento.\n\n"; break;
            case "<color=#ba3fef>-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 días. Apoyas a:</color>\n\n":
                r = "<color=#ba3fef>-Você deve intervir em apoio a um dos dois. O outro receberá Moral Baixa por 5 dias. Você apoia:</color>\n\n"; break;
            case "Un Civil de origen noble se acerca a ti con altanería y comienza a cuestionar tu liderazgo. Argumentando que no estás tomando las decisiones correctas para el bienestar de la Caravana y que él mismo podría hacerlo mejor.\n":
                r = "Um Civil de origem nobre se aproxima de você com arrogância e começa a questionar sua liderança, argumentando que você nío está tomando as decisões corretas para o bem-estar da Caravana e que ele mesmo poderia fazer melhor.\n"; break;
            case "Si bien sus puntos son poco coherentes, a medida que te habla en voz elevada, varios civiles comienzan a congregarse alrededor, curiosos.\n\n":
                r = "Embora seus argumentos sejam pouco coerentes, á medida que ele fala em voz alta, vários civis começam a se reunir ao redor, curiosos.\n\n"; break;
            case "<color=#ba3fef>-Golpearlo.</color> Su familia abandona la Caravana, retirando su inversión. -65 Oro -8 Civiles -10 Esperanza\n\n":
                r = "<color=#ba3fef>-Golpeá-lo.</color> Sua famí­lia abandona a Caravana, retirando seu investimento. -65 Ouro -8 Civis -10 Esperança\n\n"; break;
            case "Una ráfaga caliente levanta una espesa nube de cenizas y brasas apagadas alrededor de la Caravana.\n":
                r = "Uma rajada quente levanta uma espessa nuvem de cinzas e brasas apagadas ao redor da Caravana.\n"; break;
            case "Los civiles se cubren el rostro como pueden, los bueyes se inquietan y por varios instantes avanzar se vuelve peligroso.\n\n":
                r = "Os civis cobrem o rosto como podem, os bois ficam inquietos e por vários instantes avançar se torna perigoso.\n\n"; break;
            case "Puedes ordenar hacer una breve parada hasta que el aire se despeje o forzar la marcha para no perder tiempo.\n\n":
                r = "Você pode ordenar uma breve parada até que o ar se limpe ou forçar a marcha para não perder tempo.\n\n"; break;
            case "<color=#ba3fef>-Si decides esperar, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir esperar, o Respiro Negro avançará.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides seguir, las cenizas incomodarán a los Civiles. -5 Esperanza, +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir seguir, as cinzas vão incomodar os Civis. -5 Esperança, +1 Fadiga.</color>\n\n"; break;
            case "Un grupo de bestias enloquecidas por el humo y el fuego irrumpe cerca del camino, cruzando entre los árboles calcinados con una violencia desesperada.\n\n":
                r = "Um grupo de bestas enlouquecidas pela fumaça e pelo fogo irrompe perto do caminho, cruzando entre as árvores carbonizadas com uma violência desesperada.\n\n"; break;
            case "Los bueyes se inquietan al instante y varios Civiles retroceden alarmados. Si nadie actúa rápido, el caos podría extenderse a toda la Caravana.\n\n":
                r = "Os bois se inquietam na hora e vários Civis recuam alarmados. Se ninguém agir rápido, o caos pode se espalhar por toda a Caravana.\n\n"; break;
            case "</color></b> puede intentar contener a los animales.</color> ":
                r = "</color></b> pode tentar conter os animais.</color> "; break;
            case "Tirada de Salvación: TS Reflejos DC ":
                r = "Teste de Resistência: TS Reflexos CD "; break;
            case " <i>(TS Reflejos actual: ":
                r = " <i>(TS Reflexos atual: "; break;
            case ").</i> ":
                r = ").</i> "; break;
            case "Si lo logra, ganará 40 Experiencia. Si falla, la Caravana perderá 2 Bueyes.\n\n":
                r = "Se conseguir, ganhará 40 de Experiência. Se falhar, a Caravana perderá 2 Bois.\n\n"; break;
            case "<color=#ba3fef>-Si decides apartarte y ceder el paso, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir se afastar e ceder passagem, o Respiro Negro avançará.</color>\n\n"; break;
            case "Entre los troncos calcinados y la tierra ennegrecida, algunos Civiles descubren un pequeño brote verde abriéndose paso entre las brasas frías.\n\n":
                r = "Entre os troncos carbonizados e a terra escurecida, alguns Civis descobrem um pequeno broto verde abrindo caminho entre as brasas frias.\n\n"; break;
            case "La visión recorre rápidamente la Caravana. Por un instante, el Bosque Ardiente deja de parecer un lugar completamente perdido.\n\n":
                r = "A visão se espalha rapidamente pela Caravana. Por um instante, o Bosque Ardente deixa de parecer um lugar completamente perdido.\n\n"; break;
            case "<color=#a0e812><b>+10 Esperanza</b></color>":
                r = "<color=#a0e812><b>+10 Esperança</b></color>"; break;
            case "Al borde del camino, la Caravana encuentra restos de árboles derribados y estructuras carbonizadas. No todo quedó reducido a ceniza: parte de la madera todavía podría aprovecharse.\n\n":
                r = "Ã€ beira do caminho, a Caravana encontra restos de árvores derrubadas e estruturas carbonizadas. Nem tudo virou cinza: parte da madeira ainda pode ser aproveitada.\n\n"; break;
            case "Algunos Civiles sugieren detenerse para separar lo útil antes de seguir adelante. Tomará algo de tiempo, pero podría reforzar las reservas de Materiales.\n\n":
                r = "Alguns Civis sugerem parar para separar o que ainda serve antes de seguir em frente. Vai levar algum tempo, mas pode reforçar as reservas de Materiais.\n\n"; break;
            case "<color=#ba3fef>-Si decides recolectar, obtendrás 15-30 Materiales, pero el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir recolher, você obterá 15-30 Materiais, mas o Respiro Negro avançará.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides dejarlo, evitarás el retraso. +3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir deixá-lo, evitará o atraso. +3 Esperança.</color>\n\n"; break;
            case "Un foco de incendio vuelve a encenderse detrás de la Caravana y el viento empuja las llamas hacia la retaguardia.\n\n":
                r = "Um foco de incêndio volta a se acender atrás da Caravana e o vento empurra as chamas para a retaguarda.\n\n"; break;
            case "Durante unos instantes cunde el pánico: varios Civiles gritan, los bueyes tironean de los carros y parte de la carga corre peligro de prenderse fuego.\n\n":
                r = "Por alguns instantes o pânico se espalha: vários Civis gritam, os bois puxam as carroças e parte da carga corre perigo de pegar fogo.\n\n"; break;
            case "<color=#ba3fef>-Si decides apagarlo, la Caravana consumirá recursos en contener las llamas. -15 Suministros, +3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir apagá-lo, a Caravana vai consumir recursos para conter as chamas. -15 Suprimentos, +3 Esperança.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides abandonar carga, perderás 15-25 Materiales, pero evitarás que el fuego se acerque más.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir abandonar carga, perderá 15-25 Materiais, mas evitará que o fogo se aproxime mais.</color>\n\n"; break;
            case "La leña húmeda y el viento jugaron en contra. El humo del campamento se metió entre los carros y casi nadie pudo descansar bien.\n\n":
                r = "A lenha úmida e o vento jogaram contra. A fumaça do acampamento entrou entre as carroças e quase ninguém conseguiu descansar bem.\n\n"; break;
            case "Por la mañana, hay ojos irritados, tos y bastante malhumor.\n\n":
                r = "Pela manhã, há olhos irritados, tosse e bastante mau humor.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga, -3 Esperanza</b></color>":
                r = "<color=#ba3fef><b>+1 Fadiga, -3 Esperança</b></color>"; break;
            case "Una parte de la guardia nocturna se quedó dormida por momentos. No pasó nada grave, pero el campamento amaneció inquieto.\n\n":
                r = "Parte da guarda noturna cochilou por alguns momentos. Nada grave aconteceu, mas o acampamento amanheceu inquieto.\n\n"; break;
            case "Puedes despertar a más gente para reforzar la vigilancia o dejar que el resto siga durmiendo y recuperar el tiempo al amanecer.\n\n":
                r = "Você pode acordar mais gente para reforçar a vigilância ou deixar o resto continuar dormindo e recuperar o tempo ao amanhecer.\n\n"; break;
            case "<color=#ba3fef>-Si decides doblar guardia, varios caravaneros descansarán peor. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir reforçar a guarda, vários caravanistas vão descansar pior. +1 Fadiga.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides dejarlos dormir, la salida será más lenta. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir deixá-los dormir, a saída será mais lenta. +1 Avanço do Respiro Negro.</color>\n\n"; break;
            case "Durante la noche se filtró agua en uno de los carros de comida y parte de las raciones quedó inutilizable.\n\n":
                r = "Durante a noite entrou água em uma das carroças de comida e parte das rações ficou inutilizável.\n\n"; break;
            case "Puedes extender lo salvable junto al fuego antes de partir o desecharlo y seguir adelante.\n\n":
                r = "Você pode espalhar o que ainda dá para salvar perto do fogo antes de partir ou descartar tudo e seguir em frente.\n\n"; break;
            case "<color=#ba3fef>-Si decides secarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir secá-las, vocês vão partir mais tarde. +1 Avanço do Respiro Negro.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides desecharlas, perderás 18 Suministros.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir descartá-las, perderá 18 Suprimentos.</color>\n\n"; break;
            case "Una discusión menor cerca de la fogata fue subiendo de tono y terminó dejando al campamento entero de mal humor.\n\n":
                r = "Uma discussão menor perto da fogueira foi aumentando de tom e terminou deixando o acampamento inteiro de mau humor.\n\n"; break;
            case "Nadie salió herido, pero el descanso se sintió más pesado de lo normal.\n\n":
                r = "Ninguém saiu ferido, mas o descanso pareceu mais pesado do que o normal.\n\n"; break;
            case "<color=#ba3fef><b>-5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>-5 Esperança</b></color>"; break;
            case "Al levantar el campamento, varios civiles notan que faltan herramientas básicas de trabajo. Puede que hayan quedado tiradas en la oscuridad.\n\n":
                r = "Ao levantar o acampamento, vários civis percebem que faltam ferramentas básicas de trabalho. Pode ser que tenham ficado perdidas no escuro.\n\n"; break;
            case "Puedes ordenar una búsqueda rápida o reemplazarlas con lo que quede en reserva.\n\n":
                r = "Você pode ordenar uma busca rápida ou substituí-las com o que restar na reserva.\n\n"; break;
            case "<color=#ba3fef>-Si decides buscarlas, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir procurá-las, vocês vão partir mais tarde. +1 Avanço do Respiro Negro.</color>\n\n"; break;
            case "<color=#ba3fef>-Si decides reemplazarlas, perderás 12 Materiales.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir substituí-las, perderá 12 Materiais.</color>\n\n"; break;
            case "Por una noche, el campamento se mantiene en calma. No hay sobresaltos, no hay discusiones y hasta el aire parece más liviano.\n\n":
                r = "Por uma noite, o acampamento se mantém calmo. Não há sobressaltos, não há discussões e até o ar parece mais leve.\n\n"; break;
            case "El descanso le hace bien a la Caravana.\n\n":
                r = "O descanso faz bem para a Caravana.\n\n"; break;
            case "<color=#a0e812><b>-1 Fatiga</b></color>":
                r = "<color=#a0e812><b>-1 Fadiga</b></color>"; break;
            case "Alrededor del fogón, algunos civiles y héroes comparten historias simples, comida caliente y un rato de charla.\n\n":
                r = "Ao redor da fogueira, alguns civis e heróis compartilham histórias simples, comida quente e um tempo de conversa.\n\n"; break;
            case "No soluciona nada, pero por unas horas la Caravana vuelve a sentirse un poco más unida.\n\n":
                r = "Não resolve nada, mas por algumas horas a Caravana volta a se sentir um pouco mais unida.\n\n"; break;
            case "<color=#a0e812><b>+6 Esperanza</b></color>":
                r = "<color=#a0e812><b>+6 Esperança</b></color>"; break;
            case "Antes de dormir, un grupo de civiles se ofrece a ayudar con tareas atrasadas del campamento.\n\n":
                r = "Antes de dormir, um grupo de civis se oferece para ajudar com tarefas atrasadas do acampamento.\n\n"; break;
            case "Puedes organizar una pequeña ronda de reparaciones o agradecer el gesto y dejarlos descansar.\n\n":
                r = "Você pode organizar uma pequena rodada de reparos ou agradecer o gesto e deixá-los descansar.\n\n"; break;
            case "<color=#a0e812>-Si decides organizar, la Caravana ganará 15 Materiales.</color>\n\n":
                r = "<color=#a0e812>-Se decidir organizar, a Caravana ganhará 15 Materiais.</color>\n\n"; break;
            case "<color=#a0e812>-Si decides dejarlos descansar, +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se decidir deixá-los descansar, +4 Esperança.</color>\n\n"; break;
            case "El cansancio pesa, pero esta vez el campamento logra dormir sin interrupciones. Incluso quienes suelen despertarse con cualquier ruido descansan mejor.\n\n":
                r = "O cansaço pesa, mas desta vez o acampamento consegue dormir sem interrupções. Até quem costuma acordar com qualquer barulho descansa melhor.\n\n"; break;
            case "Al amanecer, el ánimo acompaña.\n\n":
                r = "Ao amanhecer, o ânimo acompanha.\n\n"; break;
            case "<color=#a0e812><b>-1 Fatiga, +3 Esperanza</b></color>":
                r = "<color=#a0e812><b>-1 Fadiga, +3 Esperança</b></color>"; break;
            case "Al ordenar los carros antes de partir, encuentran un pequeño lote de provisiones que había quedado mal inventariado.\n\n":
                r = "Ao arrumar as carroças antes de partir, encontram um pequeno lote de provisões que tinha ficado mal inventariado.\n\n"; break;
            case "No es mucho, pero alcanza para decidir entre guardarlo para el camino o repartirlo enseguida.\n\n":
                r = "Não é muito, mas basta para decidir entre guardar para o caminho ou repartir na hora.\n\n"; break;
            case "<color=#a0e812>-Si decides guardarlo, +20 Suministros.</color>\n\n":
                r = "<color=#a0e812>-Se decidir guardá-lo, +20 Suprimentos.</color>\n\n"; break;
            case "<color=#a0e812>-Si decides repartirlo, +5 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se decidir reparti-lo, +5 Esperança.</color>\n\n"; break;
            case "</color></b> se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n":
                r = "</color></b> acordou várias vezes com calafrios e mal-estar. Ao amanhecer, mal consegue se manter de pé.\n\n"; break;
            case "Uno de los Héroes se despertó varias veces con escalofríos y malestar. Al amanecer apenas puede sostenerse en pie.\n\n":
                r = "Um dos Heróis acordou várias vezes com calafrios e mal-estar. Ao amanhecer, mal consegue se manter de pé.\n\n"; break;
            case "<color=#ba3fef><b>Obtiene Enfermo por 3 días.</b></color>":
                r = "<color=#ba3fef><b>Recebe Doente por 3 dias.</b></color>"; break;
            case "</color></b> no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n":
                r = "</color></b> não conseguiu descansar bem por causa de uma sequência de pesadelos e sobressaltos.\n\n"; break;
            case "Uno de los Héroes no logró descansar bien por una serie de pesadillas y sobresaltos.\n\n":
                r = "Um dos Heróis não conseguiu descansar bem por causa de uma sequência de pesadelos e sobressaltos.\n\n"; break;
            case "Al amanecer se lo ve agotado y le cuesta seguir el ritmo del resto.\n\n":
                r = "Ao amanhecer ele parece exausto e tem dificuldade para acompanhar o ritmo dos demais.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga.</b></color>":
                r = "<color=#ba3fef><b>+1 Fadiga.</b></color>"; break;
            case "</color></b> quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n":
                r = "</color></b> quis aproveitar o descanso para treinar por conta própria. Um movimento errado terminou em uma lesão desnecessária.\n\n"; break;
            case "Uno de los Héroes quiso aprovechar el descanso para practicar por su cuenta. Un mal movimiento terminó en una lesión innecesaria.\n\n":
                r = "Um dos Heróis quis aproveitar o descanso para treinar por conta própria. Um movimento errado terminou em uma lesão desnecessária.\n\n"; break;
            case "<color=#ba3fef><b>Obtiene Herida.</b></color>":
                r = "<color=#ba3fef><b>Recebe Ferida.</b></color>"; break;
            case "</color></b> encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n":
                r = "</color></b> encontrou uma pequena bolsa presa entre mantas e cordas enquanto arrumava as carroças.\n\n"; break;
            case "Uno de los Héroes encontró una pequeña bolsa atrapada entre mantas y cuerdas mientras acomodaba los carros.\n\n":
                r = "Um dos Heróis encontrou uma pequena bolsa presa entre mantas e cordas enquanto arrumava as carroças.\n\n"; break;
            case "Dentro había un consumible todavía intacto, olvidado desde hace quién sabe cuánto.\n\n":
                r = "Dentro havia um consumível ainda intacto, esquecido ali há sabe-se lá quanto tempo.\n\n"; break;
            case "<color=#a0e812><b>Obtienes 1 consumible.</b></color>":
                r = "<color=#a0e812><b>Você recebe 1 consumível.</b></color>"; break;
            case "</color></b> pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n":
                r = "</color></b> passou boa parte do descanso revendo erros e acertos do caminho junto ao fogo.\n\n"; break;
            case "Uno de los Héroes pasó buena parte del descanso repasando errores y aciertos del camino junto al fuego.\n\n":
                r = "Um dos Heróis passou boa parte do descanso revendo erros e acertos do caminho junto ao fogo.\n\n"; break;
            case "La charla termina dándole una idea útil para lo que venga.\n\n":
                r = "A conversa acaba lhe dando uma ideia útil para o que vier depois.\n\n"; break;
            case "<color=#a0e812><b>Gana 45 Experiencia.</b></color>":
                r = "<color=#a0e812><b>Ganha 45 de Experiência.</b></color>"; break;
            case "Antes de dormir, varios Civiles se acercan a <b><color=#d1006f>":
                r = "Antes de dormir, vários Civis se aproximam de <b><color=#d1006f>"; break;
            case "</color></b> para agradecerle por lo que viene haciendo.\n\n":
                r = "</color></b> para agradecer pelo que vem fazendo.\n\n"; break;
            case "Antes de dormir, varios Civiles se acercan a uno de los Héroes para agradecerle por lo que viene haciendo.\n\n":
                r = "Antes de dormir, vários Civis se aproximam de um dos Heróis para agradecer pelo que vem fazendo.\n\n"; break;
            case "No cambia el camino, pero sí la forma en que piensa enfrentarlo al día siguiente.\n\n":
                r = "Isso não muda o caminho, mas muda a forma como pensa em enfrentá-lo no dia seguinte.\n\n"; break;
            case "<color=#a0e812><b>Obtiene Alta Moral por 4 días.</b></color>":
                r = "<color=#a0e812><b>Recebe Moral Alta por 4 dias.</b></color>"; break;
            case "El viento nocturno arrastra brasas encendidas desde los árboles caídos y obliga a mover parte del campamento una y otra vez.\n\n":
                r = "O vento noturno arrasta brasas acesas das árvores caídas e obriga a mover parte do acampamento repetidas vezes.\n\n"; break;
            case "Nadie duerme del todo tranquilo en el Bosque Ardiente.\n\n":
                r = "Ninguém dorme totalmente em paz no Bosque Ardente.\n\n"; break;
            case "<color=#ba3fef><b>+1 Fatiga, -4 Esperanza</b></color>":
                r = "<color=#ba3fef><b>+1 Fadiga, -4 Esperança</b></color>"; break;
            case "Ya entrada la noche, un tronco que parecía apagado vuelve a encenderse cerca de los carros.\n\n":
                r = "Já noite alta, um tronco que parecia apagado volta a se acender perto das carroças.\n\n"; break;
            case "Logran contenerlo antes de que pase a mayores, pero se consumen recursos en el apuro.\n\n":
                r = "Eles conseguem conter isso antes que piore, mas recursos são gastos na pressa.\n\n"; break;
            case "<color=#ba3fef><b>-12 Suministros</b></color>":
                r = "<color=#ba3fef><b>-12 Suprimentos</b></color>"; break;
            case "El suelo todavía guarda un calor tenue bajo la ceniza y por una vez el descanso no se siente hostil.\n\n":
                r = "O solo ainda guarda um calor leve sob a cinza e, pela primeira vez, o descanso não parece hostil.\n\n"; break;
            case "El campamento logra dormir mejor de lo esperado.\n\n":
                r = "O acampamento consegue dormir melhor do que o esperado.\n\n"; break;
            case "Entre raíces chamuscadas y troncos huecos, algunos Civiles encuentran hongos resistentes al calor todavía aprovechables.\n\n":
                r = "Entre raízes chamuscadas e troncos ocos, alguns Civis encontram fungos resistentes ao calor ainda aproveitáveis.\n\n"; break;
            case "No es un gran banquete, pero alcanza para reforzar las reservas antes de partir.\n\n":
                r = "Não é um grande banquete, mas basta para reforçar as reservas antes de partir.\n\n"; break;
            case "<color=#a0e812><b>+18 Suministros</b></color>":
                r = "<color=#a0e812><b>+18 Suprimentos</b></color>"; break;
            case "Durante la noche, los civiles reunidos divisan un destello de luz clara y hermosa en el horizonte hacia la dirección del puerto.\n":
                r = "Durante a noite, os civis reunidos avistam um clarío de luz ní­tida e bela no horizonte, na direçío do porto.\n"; break;
            case "Quizás sea una señal, quizás casualidad, pero los civiles se ven ahora más optimistas, por más que aún falte un largo trecho.\n\n\n\n\n\n\n":
                r = "Talvez seja um sinal, talvez uma coincidência, mas os civis agora parecem mais otimistas, embora ainda reste um longo caminho.\n\n\n\n\n\n\n"; break;
            case "La atmósfera se vuelve más ligera y optimista, y por un breve instante, el peso de la situación parece desvanecerse.\n\n\n\n":
                r = "A atmosfera fica mais leve e otimista, e por um breve instante o peso da situaçío parece desaparecer.\n\n\n\n"; break;
            case "<color=#a0e812><b>+5 Esperanza</b>\n\n</color>":
                r = "<color=#a0e812><b>+5 Esperança</b>\n\n</color>"; break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadoveres civiles. Una lucha tuvo lugar aquí y esta caravana no sobrevivió.\n":
                r = "Ao avançar pelo caminho, você encontra várias carroças destruí­das rodeadas por cadáveres de civis. Uma luta aconteceu aqui, e esta caravana nío sobreviveu.\n"; break;
            case "Si bien la situación es sombría, varios suministros en buen estado no fueron saqueados, quedando a un lado del camino.\n\n\n\n":
                r = "Embora a situaçío seja sombria, vários suprimentos em bom estado nío foram saqueados, permanecendo á beira da estrada.\n\n\n\n"; break;
            case "<color=#ba3fef>-Puedes dar entierro a los Civiles y honrar su memoria, sin saquearlos.</color> +15 Esperanza \n\n":
                r = "<color=#ba3fef>-Você pode enterrar os Civis e honrar sua memória, sem saqueá-los.</color> +15 Esperança \n\n"; break;
            case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
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
                r = "<color=#ba3fef>-2 no Avanço do Respiro Negro.</color>\n\n"; break;
            // Logs (segments for concatenation)
            case "-Has encontrado al ladrón y recuperado el oro robado, pero has tenido que desterrar al ladrón. -5 Esperanza -1 Civil.":
                r = "-Você encontrou o ladrío e recuperou o ouro roubado, mas teve de bani-lo. -5 Esperança -1 Civil."; break;
            case "-No has logrado encontrar al ladrón y se perdieron ":
                r = "-Você nío conseguiu encontrar o ladrío e perdeu "; break;
            case " de oro.":
                r = " de ouro."; break;
            case " superó su Tirada de Salvación de Reflejos (1d20: ":
                r = " passou no Teste de Resistência de Reflexos (1d20: "; break;
             case " superó su Tirada de Salvación de Fortaleza (1d20: ":
                r = " passou no Teste de Resistência de Fortaleza (1d20: "; break;
              case " superó su Tirada de Salvación Mental (1d20: ":
                r = " passou no Teste de Resistência Mental (1d20: "; break;
            case " vs DC ":
                r = " vs CD "; break;
            case ") y logró contener a las bestias aterradas. +40 Experiencia.":
                r = ") e conseguiu conter as bestas aterrorizadas. +40 Experiência."; break;
            case " falló su Tirada de Salvación de Reflejos (1d20: ":
                r = " falhou no Teste de Resistência de Reflexos (1d20: "; break;
            case "), y ha sufrido una herida.":
                r = "), e sofreu uma ferida."; break;
            case "). Las bestias aterradas desataron el caos y la Caravana perdió 2 Bueyes.":
                r = "). As bestas aterrorizadas espalharam o caos e a Caravana perdeu 2 Bois."; break;
            case "-Has dado un discurso motivador y has refutado los argumentos del Noble. +15 Esperanza":
                r = "-Você fez um discurso motivador e rebateu os argumentos do Nobre. +15 Esperança"; break;
            case "-Has dado un discurso poco convincente que ha generado más dudas que certezas. -20 de Esperanza.":
                r = "-Você fez um discurso pouco convincente, que gerou mais dúvidas do que certezas. -20 de Esperança."; break;
            case "-La cacería de ":
                r = "-A caçada de "; break;
            case " ha sido exitosa. +":
                r = " foi bem-sucedida. +"; break;
            case " Suministros +55 Experiencia.":
                r = " Suprimentos +55 Experiência."; break;
            case " sufrió un accidente durante la cacería. Herido.":
                r = " sofreu um acidente durante a caçada. Ferido."; break;
            case "-Los Civiles se han contaminado y han muerto ":
                r = "-Os Civis foram contaminados e morreram "; break;
            case " Civiles. -10 Esperanza":
                r = " Civis. -10 Esperança"; break;
            case "-Los Civiles han descansado en el río y se han refrescado. -1 Fatiga ":
                r = "-Os Civis descansaram no rio e se refrescaram. -1 Fadiga "; break;
            // Riña description segments
            case "Escuchas un alboroto en las proximidades a los carros de los Héroes. Al acercarte a investigar ves a <b><color=#d1006f>":
                r = "Você ouve um alvoroço nas proximidades das carroças dos Heróis. Ao se aproximar para investigar, vê <b><color=#d1006f>"; break;
            case "</color></b> y <b><color=#d1006f>":
                r = "</color></b> e <b><color=#d1006f>"; break;
            case "</color></b> discutiendo acaloradamente.":
                r = "</color></b> discutindo acaloradamente."; break;
            case "Río Contaminado":
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
                r = "<color=#ba3fef><b>As Horas Passam: +1 Avanço do Respiro Negro</b></color>";
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
            case "Nieve: todas las unidades obtienen 'Frío'.":
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
            case "Has llegado a un improvisado Puesto Comercial, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\nTu Séquito de Mercaderes ha actualizado su Inventario.":
                r = "Você chegou a um Posto Comercial improvisado, onde oferecem Suprimentos básicos de sobrevivência aos viajantes.\nO Ní­vel do seu Séquito de Mercadores ajudará a reduzir os preços.\n\n\nSeu Séquito de Mercadores atualizou o Inventário.";
                break;
            case "El Séquito de Mercaderes ha actualizado su inventario en el Puesto Comercial.":
                r = "O Séquito de Mercadores atualizou seu inventário no Posto Comercial.";
                break;
            case "Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n\n. ":
                r = "Você chegou a um Santuário dos Purificadores; vários foram construídos na regiío para oferecer apoio e preces aos valentes que combateram o Lich.\nHoje, embora esteja abandonado, ele mantém sua aura de tranquilidade, e você pode depositar oferendas para realizar uma prece de purificaçío.\n\n\n. ";
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
                r = "<color=#8708a4><b>                  O Respiro Negro</b></color>\n\n\n";
                break;
            case "<color=#ebdeef>Al morir el Liche, liberó un último estertor de muerte y putrefacción que se expande por cientos de kilómetros alrededor.</color>":
                r = "<color=#ebdeef>Ao morrer, o Lich liberou um último estertor de morte e putrefaçío que se espalha por centenas de quilÃ´metros ao redor.</color>";
                break;
            case "\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente está envolviendo a los seres vivos que no logran escapar, provocándoles la muerte, o peor. </color>\n\n\n\n":
                r = "\n\nChamado de Respiro Negro, essa onda de peste e podridío está lentamente envolvendo os seres vivos que nío conseguem escapar, causando-lhes a morte, ou algo pior. </color>\n\n\n\n";
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
            case "Si descansas volverá a 0 y arrancarán el nuevo día Descansados(1).\n\n":
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
            case "Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podrían morir si viajas.":
                r = "Atualmente estío <color=#ffd19e>Agitados</color>(<color=#ffd19e>4</color>), -10 Esperança, e alguns poucos Bois podem morrer se você viajar.";
                break;
            case "Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas.":
                r = "Atualmente estío <color=#ff9e9e>Cansados</color>(<color=#ff9e9e>5</color>), -15 Esperança, e alguns Bois podem morrer se você viajar.";
                break;
            case "Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas.":
                r = "Atualmente estío <color=#ff3c3c>Exaustos</color>(<color=#ff3c3c>6</color>), -20 Esperança, e vários Bois podem morrer se você viajar.";
                break;
            case "Día ":
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
            case "Luego de buscar vagamente en la cercanía y concluir que no hay pistas, decides consolar a los familiares y seguir adelante.\n\n\n\n\n\n\n":
                r = "Depois de procurar superficialmente pelos arredores e concluir que nío há pistas, você decide consolar os familiares e seguir em frente.\n\n\n\n\n\n\n";
                break;
            case "<color=#ba3fef><b>Pierdes 4-12 Civiles, -5 Esperanza</b></color>":
                r = "<color=#ba3fef><b>Você perde 4-12 Civis, -5 Esperança</b></color>";
                break;
            case "Uno de los bueyes de la caravana ha caído enfermo y no puede continuar. Recibes recomendaciones de algunos especialistas en ganado que te aconsejan revisar a los otros bueyes para evitar una propagación de la enfermedad.\n\n\n\n":
                r = "Um dos bois da caravana adoeceu e nío pode continuar. Você recebe recomendações de alguns especialistas em gado que aconselham examinar os outros bois para evitar a propagaçío da doença.\n\n\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisarlos tomará unas horas: +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir examiná-los, isso levará algumas horas: +1 Avanço do Respiro Negro.</color>\n\n";
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
            case " ganan Alta Moral por 3 días.</b></color>":
                r = " ganham Moral Alta por 3 dias.</b></color>";
                break;
            case "Al avanzar en el camino, encuentras varios carros destruidos rodeado de cadáveres civiles. Una lucha tuvo lugar aquí y esta caravana no sobrevivió.\n":
                r = "Ao avançar pelo caminho, você encontra várias carroças destruí­das rodeadas de cadáveres de civis. Uma luta aconteceu aqui, e esta caravana nío sobreviveu.\n";
                break;
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que saqueen los Suministros.</color> +21-35 Suministros, +5-11 Materiales, +15-35 Oro, -5 Esperanza.</i> \n\n":
                r = "<color=#ba3fef>-Você pode ordenar que a Caravana saqueie os Suprimentos.</color> +21-35 Suprimentos, +5-11 Materiais, +15-35 Ouro, -5 Esperança.</i> \n\n";
                break;
            /* case "La Caravana se detiene en un aserradero abandonado, algunos árboles han sido talados y la madera está apilada en desorden.\n":
                 r = "A Caravana para em uma serraria abandonada, algumas árvores foram derrubadas e a madeira está empilhada em desordem.\n";
                 break;*/
            case "<color=#ba3fef>-Puedes ordenar a la Caravana que junten toda la madera.</color> +65-90 Materiales, +1 Fatiga, +1 Avance del Aliento Negro.</i> \n\n":
                r = "<color=#ba3fef>-Você pode ordenar que a Caravana recolha toda a madeira.</color> +65-90 Materiais, +1 Fadiga, +1 Avanço do Respiro Negro.</i> \n\n";
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
            case "Con su otra mano extendida sostiene una bolsa con oro y te la ofrece amigablemente. -'Considéralo un símbolo de mi confianza en ti, además de un aporte que puede ser útil para la Caravana.'-dice\n ":
                r = "Com a outra mío estendida, ele segura uma bolsa com ouro e a oferece amigavelmente a você. -'Considere isso um símbolo da minha confiança em você, além de uma contribuiçío que pode ser útil para a Caravana.'- diz\n ";
                break;
            case "<color=#ba3fef>Respondes: -'Conserva el dinero, tu aporte a la Caravana ya es considerable con tu esfuerzo diario, y estoy más que agradecido de poder contar contigo.'</color> Efectos: ":
                r = "<color=#ba3fef>Você responde: -'Fique com o dinheiro, sua contribuiçío para a Caravana já é considerável com seu esforço diário, e sou mais do que grato por poder contar com você.'</color> Efeitos: ";
                break;
            case " gana Alta Moral por 4 días y 50 Experiencia. \n\n":
                r = " ganha Moral Alta por 4 dias e 50 de Experiência. \n\n";
                break;
            case "<color=#ba3fef>Respondes: -'Acepto tu ofrecimiento, no hay moneda que sobre en nuestra situación actual y seguramente nos ayudará durante el viaje, gracias.'</color> Efectos: +120-160 Oro. \n\n":
                r = "<color=#ba3fef>Você responde: -'Aceito sua oferta, nío há moeda sobrando em nossa situaçío atual, e isso certamente nos ajudará durante a viagem, obrigado.'</color> Efeitos: +120-160 Ouro. \n\n";
                break;
            case "Un hombre anciano aparece a un lado del camino haciendole señas con las manos a la Caravana. De cerca, te das cuenta que este hombre lleva viviendo muchísimos años en la zona y la conoce a la perfección.\n":
                r = "Um homem idoso aparece ao lado do caminho fazendo sinais com as míos para a Caravana. De perto, você percebe que esse homem vive na regiío há muitos anos e a conhece perfeitamente.\n";
                break;
            case "'Aliento Negro o no, mis días ya están contados. Pero puedo transmitirles mis conocimientos sobre esta tierra, como último acto de bien.'- dice\n\n":
                r = "'Respiro Negro ou nío, meus dias já estío contados. Mas posso transmitir a vocês meu conhecimento sobre esta terra, como último ato de bondade.'- diz\n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'¿Conoce algún atajo que nos aleje del peligro inminente al menos por unos kilómetros?'</color> Efectos: Si es posible se generará un Atajo subterráneo. \n\n":
                r = "<color=#ba3fef>Você pergunta: -'Conhece algum atalho que nos afaste do perigo iminente por pelo menos alguns quilÃ´metros?'</color> Efeitos: Se possí­vel, será gerado um Atalho subterrâneo. \n\n";
                break;
            case "<color=#ba3fef>Preguntas: -'Describanos el area circundante para que podamos tomar decisiones con más información.'</color> Efectos: Se revelarán próximos nodos. \n\n":
                r = "<color=#ba3fef>Você pergunta: -'Descreva-nos a área ao redor para que possamos tomar decisões com mais informaçío.'</color> Efeitos: Os próximos nós serío revelados. \n\n";
                break;
            case "</color></b> se lo ve con mucha energía y determinación mientras realiza sus labores habituales. Cuando te acercas a él, te dice que tuvo un Sueño en el cual vio a la Caravana llegando a su destino.\n":
                r = "</color></b> aparenta estar com muita energia e determinaçío enquanto realiza suas tarefas habituais. Quando você se aproxima, ele diz que teve um Sonho no qual viu a Caravana chegando ao seu destino.\n";
                break;
            case "'En el sueño, vi un claro camino hacia nuestro destino. Habrá peligros y dificultades, pero estoy convencido que lo lograremos. Sigamos esa ruta.'- dice con Determinación\n\n\n":
                r = "'No sonho, vi um caminho claro até o nosso destino. Haverá perigos e dificuldades, mas estou convencido de que conseguiremos. Vamos seguir essa rota.'- diz com Determinaçío\n\n\n";
                break;
            case "</color></b> obtiene 150 Experiencia y Alta Moral por 5 días.</color>\n\n":
                r = "</color></b> recebe 150 de Experiência e Moral Alta por 5 dias.</color>\n\n";
                break;
            case "Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mínimo.\n":
                r = "Você chegou a uma bela clareira natural que parece nío ter sido manchada nem minimamente pela corrupçío e pela pestilência.\n";
                break;
            case "Es un excelente lugar para descansar y recuperar fuerzas.\n\n\n\n\n":
                r = "É um excelente lugar para descansar e recuperar as forças.\n\n\n\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza.\n\nDescansar en este lugar tendrá también beneficios adicionales:\n-El Aliento Negro avanzará solo 1.\n-+10% curación recibida.\n-El evento será positivo.</b></color>":
                r = "<color=#a0e812><b>+5 Esperança.\n\nDescansar neste lugar também terá benefí­cios adicionais:\n-O Respiro Negro avançará apenas 1.\n-+10% de cura recebida.\n-O evento será positivo.</b></color>";
                break;
            case "Has llegado a un pequeño asentamiento. Notas que los civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.":
                r = "Você chegou a um pequeno assentamento. Você nota que os civis estío desorganizados e precisam de liderança para sobreviver ao Respiro Negro.";
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
            case "\nSe conseguirán de 18-30 Materiales y 70-110 Suministros.":
                r = "\nSerío obtidos 18-30 Materiais e 70-110 Suprimentos.";
                break;
            case "<color=#a0e812><b>\n\nDescansar en este lugar tendrá beneficios adicionales: +20% efectividad a tareas de Recolección.</b></color>":
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
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3, todos los personajes obtendrán Bendecido por 3 días y un personaje con Corrupción al azar será curado.":
                r = "Ao fazer a oferenda, o Respiro Negro recuará em 3, todos os personagens receberão Abençoado por 3 dias e um personagem aleatório com Corrupção será curado.";
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
            case "Defensas: Cada Tier mejora las defensas de la Caravana en ataques directos y reduce 10% las chances de perder un Séquito.":
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
            case "<i>\"Debemos dejar en alto llamas de esperanza que sirvan de guía para aquellas caravanas perdidas en el camino.\"</i> \n\nCada Tier de las Almenaras otorgará un bonus de 5 <b>Esperanza</b> cada vez que una caravana comience a viajar por una región nueva. \n\nAdemás, cada Tier dará <b>1 stack</b> de una mejora de Caravana al azar al comenzar una región nueva. Si otorga varios por Tier, siempre serán de la misma mejora. \n\nAdemás, cada Tier dará <b>+3%</b> chances de Exploración para las caravanas futuras.\n\n\n\n\n ":
                r = "<i>\"Devemos manter altas chamas de esperança que sirvam de guia para aquelas caravanas perdidas pelo caminho.\"</i> \n\nCada Tier das Almenaras concede um bÃ´nus de 5 <b>Esperança</b> sempre que uma caravana começa a viajar por uma nova região. \n\nAlém disso, cada Tier concede <b>1 acúmulo</b> de uma melhoria aleatória da Caravana ao começar uma nova região. Se conceder vários acúmulos por Tier, eles sempre serão da mesma melhoria. \n\nAlém disso, cada Tier concede <b>+3%</b> de chance de Exploração para as caravanas futuras.\n\n\n\n\n ";
                break;
            case "Carro Almacén: Cada Tier reduce 5% Suministros consumidos por Descanso.":
                r = "Carroça de Armazenamento: Cada Tier reduz em 5% os Suprimentos consumidos por Descanso.";
                break;
            case "Planes de mejoras":
                r = "Planos de melhorias";
                break;
            case "  Resistencias":
                r = "  Resistências";
                break;
            case "Rasgos":
                r = "Traços";
                break;
            case "Mejora de atributo disponible":
                r = "Melhoria de atributo disponível";
                break;
            case "Mejora de salvación disponible":
                r = "Melhoria de resistência disponível";
                break;
            case "Mejora de habilidad disponible":
                r = "Melhoria de habilidade disponível";
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
            case "Selecciona a tus personajes:":
                r = "Selecione seus personagens:";
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
            case "-Es un día hermoso. +5 Esperanza.":
                r = "-É um dia lindo. +5 Esperança.";
                break;
            case "-La Ola de Calor se hace insoportable. +1 Fatiga.":
                r = "-A Onda de Calor se torna insuportável. +1 Fadiga.";
                break;
            case "-La Lluvia hace el viaje más difícil. -5 Esperanza.":
                r = "-A Chuva torna a viagem mais difícil. -5 Esperança.";
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
                r = "-Durante o descanso, o Respiro Negro avançou 2.";
                break;
            case "-Durante el descanso en el Claro, el Aliento Negro ha avanzado 1.":
                r = "-Durante o descanso na Clareira, o Respiro Negro avançou 1.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro.":
                r = " realizou com Sucesso um Ritual de Limpeza durante o descanso, impedindo o avanço do Respiro Negro.";
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
            case "-El tener que trabajar en plena Ola de Calor, ha caído mal en los Civiles. -3 Esperanza":
                r = "-Ter que trabalhar em plena Onda de Calor foi mal recebido pelos Civis. -3 Esperança";
                break;
            case "-El tener un Día Libre en plena Ola de Calor, ha caído bien en los Civiles. +5 Esperanza":
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
            case "Durante el descanso, se asignarán a los civiles mas aptos físicamente a la vigilancia del area circundante al campamento.\n\n":
                r = "Durante o descanso, os civis fisicamente mais aptos serío designados para vigiar a área ao redor do acampamento.\n\n";
                break;
            case "<color=#d8a205>Reduce chances de ataque a caravana. +20% a Exploración. -10 Esperanza.</color>\n\n\n":
                r = "<color=#d8a205>Reduz as chances de ataque á caravana. +20% de Exploraçío. -10 Esperança.</color>\n\n\n";
                break;
            case "<b><u>Día Libre</b></u>\n\n\n":
                r = "<b><u>Dia Livre</b></u>\n\n\n";
                break;
            case "Los civiles se tomarán el día para descansar y recobrar fuerzas.\n\n":
                r = "Os civis tirarío o dia para descansar e recuperar as forças.\n\n";
                break;
            case "<color=#d8a205>Se conseguirá 10 de Esperanza y el día siguiente arrancará con -1 Fatiga. +10% Curación a personajes.</color>\n\n\n":
                r = "<color=#d8a205>Serío obtidos 10 de Esperança, e o dia seguinte começará com -1 Fadiga. +10% de Curación a personaxes.</color>\n\n\n";
                break;
            case "<b><u>Feria</b></u>\n\n\n":
                r = "<b><u>Feira</b></u>\n\n\n";
                break;
            case "Los civiles dedicarán el día a organizar una feria con varios juegos y celebraciones.\n\n":
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
            case "Nivel: ":
                r = "Nível: ";
                break;
            case "Exp: ":
                r = "EXP: ";
                break;
            case "Valentía: ":
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
            case "Bendecido: +3 TS +5 Res.Necro.</color>":
                r = "Abençoado: +3 TS +5 Res.Necro.</color>";
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
            case " días. -15% daño, -3 TS Fortaleza, -1 PA </color>":
                r = " dias. -15% de dano, -3 TS Fortaleza, -1 PA </color>";
                break;
            case "<color=#d80404>\n\nBaja Moral por ":
                r = "<color=#d80404>\n\nMoral Baixa por ";
                break;
            case " días. -1 Ataque y Defensa, -3 TS Mental, -2 Valentía Inicial</color>":
                r = " dias. -1 Ataque e Defesa, -3 TS Mental, -2 Bravura Inicial</color>";
                break;
            case "<color=#d80404>\n\nAlta Moral por ":
                r = "<color=#d80404>\n\nMoral Alta por ";
                break;
            case " días. +1 Ataque, +2 TS Mental, +2 Valentía Inicial</color>":
                r = " dias. +1 Ataque, +2 TS Mental, +2 Bravura Inicial</color>";
                break;
            case "Torpe: +1 Rango Pifias. ":
                r = "Desajeitado: +1 Faixa de Erros. ";
                break;
            case "Valiente: +2 Valentía Máxima.":
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
                r = "Consumíveis";
                break;
            case "<color=#0cca74><b>Guardia: </b></color><color=#d3d3d3><i>El personaje se mantendrá alerta y custodiará la caravana.</color></i>\\n\\nSi se produce una emboscada, podrá participar de la defensa sin penalización. +3% Exploración al descansar.":
                r = "<color=#0cca74><b>Guarda: </b></color><color=#d3d3d3><i>O personagem permanecerá alerta e protegerá a caravana.</color></i>\\n\\nSe ocorrer uma emboscada, poderá participar da defesa sem penalidade. +3% Exploraçío ao descansar.";
                break;
            case "<color=#0cca74><b>Coerción: </b></color><color=#d3d3d3><i>Con métodos cuestionables, el Acechador obliga a los Mercaderes a donar dinero a la caravana.</color></i>\\n\\n+1-10 Oro y -1 Esperanza por día.":
                r = "<color=#0cca74><b>Coerçío: </b></color><color=#d3d3d3><i>Com métodos questionáveis, o Espreitador obriga os Mercadores a doar dinheiro para a caravana.</color></i>\\n\\n+1-10 Ouro e -1 Esperança por dia.";
                break;
            case "<color=#0cca74><b>Exploración: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo Subterráneo\\nSi se da un combate, lo arranca Fatigado.":
                r = "<color=#0cca74><b>Exploraçío: </b></color><color=#d3d3d3><i>O personagem explora os possí­veis destinos á frente da caravana.</color></i>\\n\\nTem 40% de chance de revelar Nós futuros ao viajar para um novo Nó. -5% de chance de Nó Misterioso. +5% de chance de Atalho Subterrâneo\\nSe ocorrer um combate, ele o inicia Fatigado.";
                break;
            case "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirá su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrá +3 Flechas y +5% daño.":
                r = "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>O personagem dedicará seu tempo a criar e aprimorar suas flechas.</color></i>\\n\\nSe ocorrer um combate, terá +3 Flechas e +5% de dano.";
                break;
            case "<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocupará de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzará con +2 Armadura.":
                r = "<color=#0cca74><b>Manutençío de Armadura: </b></color><color=#d3d3d3><i>O personagem cuidará da manutençío da sua armadura.</color></i>\\n\\nSe ocorrer um combate, começará com +2 de Armadura.";
                break;
            case "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerá vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrá participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daño los primeros 2 turnos.":
                r = "<color=#0cca74><b>Vigiar: </b></color><color=#d3d3d3><i>O personagem permanecerá vigilante diante de qualquer perigo.</color></i>\\n\\nSe ocorrer uma emboscada, poderá participar ativamente da defesa e recebe +2 PA, +5 Iniciativa e +20% de dano nos 2 primeiros turnos.";
                break;
            case "<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizará su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada día que pase ganará 15 Experiencia.\\nSi se produce un combate, lo arrancará Fatigado.":
                r = "<color=#0cca74><b>Treinar: </b></color><color=#d3d3d3><i>O personagem usará seu tempo livre para treinar e se manter em forma.</color></i>\\n\\nA cada dia, ganhará 15 de Experiência.\\nSe ocorrer um combate, ele o iniciará Fatigado.";
                break;
            case "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nCada día que pase recuperará un 15% de salud.\\nSi se produce un combate, lo arrancará Fresco.":
                r = "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>O personagem vai se concentrar em descansar e recuperar sua saúde.</color></i>\\n\\nA cada dia, recuperará 15% de saúde.\\nSe ocorrer um combate, ele o iniciará Disposto.";
                break;
            case "<color=#0cca74><b>Afilar Armas: </b></color><color=#d3d3d3><i>El Acechador se encarga de mantener sus armas afiladas.</color></i>\\n\\nSi se produce un combate tendrá +10% daño.":
                r = "<color=#0cca74><b>Afiar Armas: </b></color><color=#d3d3d3><i>O Espreitador se encarrega de manter suas armas afiadas.</color></i>\\n\\nSe ocorrer um combate, terá +10% de dano.";
                break;
            case "<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.":
                r = "<color=#0cca74><b>Telecinese: </b></color><color=#d3d3d3><i>Com seus poderes arcanos de telecinese, ajuda com a carga da caravana.</color></i>\\n\\n+20 de Capacidade de carga.";
                break;
            case "<color=#0cca74><b>Caza Nocturna: </b></color><color=#d3d3d3><i>El personaje cazará en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+1d4 Suministros por día. +3% probabilidad de Emboscada Enemiga al descansar.":
                r = "<color=#0cca74><b>Caça Noturna: </b></color><color=#d3d3d3><i>O personagem caçará nos arredores para conseguir comida para a caravana.</color></i>\\n\\n+1d4 Suprimentos por dia. +3% de probabilidade de Emboscada Inimiga ao descansar.";
                break;
            case "<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirá los relatos de sus hazañas con quienes quieran oírlas.</color></i>\\n\\n+10 Experiencia por día a personajes de nivel inferior. +4 Esperanza al descansar.":
                r = "<color=#0cca74><b>Relatos de Batalha: </b></color><color=#d3d3d3><i>O personagem compartilhará os relatos de seus feitos com quem quiser ouvi-los.</color></i>\\n\\n+10 de Experiência por dia para personagens de ní­vel inferior. +4 Esperança ao descansar.";
                break;
            case "<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizará rituales de protección para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por día.":
                r = "<color=#0cca74><b>Ritual de Limpeza: </b></color><color=#d3d3d3><i>A Purificadora realizará rituais de proteçío para combater o Respiro Negro.</color></i>\\n\\nProbabilidade de evitar o avanço do Respiro Negro: 25% ao descansar, 15% por dia.";
                break;
            case "<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usará su tiempo para ayudar a los rezagados y más débiles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.":
                r = "<color=#0cca74><b>Ajudar os Desamparados: </b></color><color=#d3d3d3><i>A Purificadora usará seu tempo para ajudar os mais atrasados e frágeis da caravana.</color></i>\\n\\n+1d3 de Esperança por dia. +1 Fervor em combate.";
                break;
            case "<color=#0cca74><b>Concentración Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de Energía al iniciar combates.":
                r = "<color=#0cca74><b>Concentraçío Arcana: </b></color><color=#d3d3d3><i>O Canalizador se concentra e mantém seu poder preparado para qualquer combate que surgir.</color></i>\\n\\n+1 Ní­vel de Energia ao iniciar combates.";
                break;
            case "<color=#0cca74><b>Vigilar Desde las Sombras: </b></color><color=#d3d3d3><i>El Acechador recorre las inmediaciones de la caravana en sigilo, tratando de anticipar emboscadas enemigas.</color></i>\\n\\n-5% chances de emboscadas.\\nEn Ataque a Caravana cuenta como Guardia y comienza en Sigilo.":
                r = "<color=#0cca74><b>Vigiar das Sombras: </b></color><color=#d3d3d3><i>O Espreitador percorre os arredores da caravana em sigilo, tentando antecipar emboscadas inimigas.</color></i>\\n\\n-5% de chance de emboscadas.\\nEm Ataque a Caravana conta como Guarda e comeca em Sigilo.";
                break;
            case "<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>Séquito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curación diaria del Séquito de Curanderos.":
                r = "<color=#0cca74><b>Colaborar com os Curandeiros: </b></color><color=#d3d3d3><i>Ajuda o <b>Séquito de Curandeiros</b> em suas tarefas, aumentando sua eficácia.</color></i>\\n\\nAumenta em 5% a cura diária do Séquito de Curandeiros.";
                break;
            case "<color=#0cca74><b>Crear Símbolo Arcano de Protección: </b></color><color=#d3d3d3><i>El Canalizador concentra energía arcana protectora en un símbolo que puede proteger a quien lo utilice.</color></i>\\n\\nCrea un Símbolo Arcano de Protección por día.":
                r = "<color=#0cca74><b>Criar Símbolo Arcano de Proteçío: </b></color><color=#d3d3d3><i>O Canalizador concentra energia arcana protetora em um símbolo que pode proteger quem o utilizar.</color></i>\\n\\nCria um Símbolo Arcano de Proteçío por dia.";
                break;
            case "-El viaje por el camino sinuoso ha retrasado la caravana. +":
                r = "-A viagem pelo caminho sinuoso atrasou a caravana. +";
                break;
            case " Avance del Aliento Negro":
                r = " de Avanço do Respiro Negro";
                break;
            case "-La nieve a retrasado el viaje. +1 Avance del Aliento Negro":
                r = "-A neve atrasou a viagem. +1 de Avanço do Respiro Negro";
                break;
            case "-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza":
                r = "-A ausência do Respiro Negro durante a viagem inspira a Caravana. +2 Esperança";
                break;
            case "-La presencia notable del Aliento Negro al viajar, provoca incertidumbre en la Caravana. -3 Esperanza":
                r = "-A presença perceptí­vel do Respiro Negro durante a viagem provoca incerteza na Caravana. -3 Esperança";
                break;
            case "-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -7 Esperanza":
                r = "-A forte presença do Respiro Negro no ar provoca medo na Caravana. -7 Esperança";
                break;
            case "-La presencia de Aliento Negro en el aire es fatal para los Civiles. -10 Esperanza -":
                r = "-A presença do Respiro Negro no ar é fatal para os Civis. -10 Esperança -";
                break;
            case " Civiles":
                r = " Civis";
                break;
            case "-El Séquito de Herboristas ha visitado un Claro y recolectado hierbas curativas.":
                r = "-O Séquito de Herboristas visitou uma Clareira e coletou ervas curativas.";
                break;
            case " ha realizado con Éxito un Ritual de Limpieza, previniendo el avance del Aliento Negro.":
                r = " realizou com Úxito um Ritual de Limpeza, impedindo o avanço do Respiro Negro.";
                break;
            case "-Los rezos constantes del Séquito de Clérigos han logrado frenar el avance del Aliento Negro.":
                r = "-As orações constantes do Séquito de Clérigos conseguiram frear o avanço do Respiro Negro.";
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
                r = "-O Séquito de Cronistas registrou a viagem. +20 Valor da CrÃ´nica.";
                break;
            case "-El Séquito de Nobles ha hecho una donación. Oro: ":
                r = "-O Séquito de Nobres fez uma doaçío. Ouro: ";
                break;
            case "-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza.":
                r = "-Os Civis se sentem culpados pela presença dos Escravos. -2 Esperança.";
                break;
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3, se ha gastado 200 de oro y todos los personajes obtienen Bendecido por 3 días.":
                r = "-Você realizou um ritual no santuário. O Respiro Negro recua em 3, 200 de ouro foram gastos e todos os personagens recebem Abençoado por 3 dias.";
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
            case "-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3, se han sacrificado 3 bueyes y todos los personajes obtienen Bendecido por 3 días.":
                r = "-Você realizou um ritual no santuário. O Respiro Negro recua em 3, 3 bois foram sacrificados e todos os personagens recebem Abençoado por 3 dias.";
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
            case "-Consuelo reduce la pérdida de Esperanza en ":
                r = "-Consolo reduz a perda de Esperança em ";
                break;
            case " socializa con la caravana. Beneficiados: ":
                r = " socializa com a caravana. Beneficiados: ";
                break;
            case "nadie":
                r = "ninguém";
                break;
            case " socializa con la caravana. Sus compañeros realizan una TS Mental DC ":
                r = " socializa com a caravana. Seus companheiros fazem um Teste Mental CD ";
                break;
            case " supera la TS Mental (1d20: ":
                r = " supera o Teste Mental (1d20: ";
                break;
            case " falla la TS Mental (1d20: ":
                r = " falha no Teste Mental (1d20: ";
                break;
            case ") gracias a <b>Socializar</b> y obtiene Alta Moral por 1 día.":
                r = ") graças a <b>Socializar</b> e ganha Alta Moral por 1 dia.";
                break;
            case ") pese a <b>Socializar</b> y no obtiene Alta Moral.":
                r = ") apesar de <b>Socializar</b> e não ganha Alta Moral.";
                break;
            case " de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza":
                r = " de Ouro dos Mercadores da Caravana, que foram coagidos a doar para a causa. -1 Esperança";
                break;
            case " ha creado un Símbolo de Protección Arcano.":
                r = " criou um Símbolo Arcano de Proteçío.";
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
                r = "-Os Cronistas registraram a vitória, +50 Valor da CrÃ´nica, +5 Esperança.";
                break;
            case "-Los Cronistas han registrado la derrota, -50 Valor Crónica. -3 Esperanza.":
                r = "-Os Cronistas registraram a derrota, -50 Valor da CrÃ´nica. -3 Esperança.";
                break;
            case "Victoria sin recompensas definidas para este encuentro clásico.":
                r = "Vitória sem recompensas definidas para este encontro clássico.";
                break;
            case "Derrota en un encuentro clásico. Los efectos específicos aún no están configurados.":
                r = "Derrota em um encontro clássico. Os efeitos específicos ainda nío estío configurados.";
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
                r = "-A crÃ´nica da viagem foi vendida por Ouro: ";
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
                r = "\n\n-A crÃ´nica desta viagem já foi vendida.";
                break;
            case "\n\n- Crónica: Acumula valor de la siguiente manera:":
                r = "\n\n- CrÃ´nica: Acumula valor da seguinte forma:";
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
                r = "\n\n\n\n-Valor da CrÃ´nica: Ouro: ";
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
            case " es escondido en las sombras tras recibir un ataque crítico por su Armadura de Velo.":
                r = " se esconde nas sombras após receber um ataque crítico por sua Armadura de Véu.";
                break;
            case "Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periódicamente parte de su riqueza para asegurarse de que no serán abandonados.\n\n":
                r = "Um grupo de nobres que foi obrigado a abandonar o conforto de suas terras agora viaja junto á caravana. Embora sejam queixosos e nío tenham grande utilidade, ao menos doam periodicamente parte de sua riqueza para garantir que nío serío abandonados.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Cada día donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.":
                r = "EFEITOS PASSIVOS:\n\n-A cada dia, doam Ouro equivalente a 1/3 da Esperança.\n\n-Perdem-se 2 de Esperança ao viajar com fadiga 4 ou maior.";
                break;
            case "Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayoría murieron en la onda expansiva en ese momento, pero todaví­a quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n":
                r = "Os Clérigos do Sol Radiante Purificador participaram como apoio no combate contra o Lich. A maioria morreu na onda de choque naquele momento, mas ainda restam alguns grupos tentando chegar ao porto e sobreviver enquanto lutam para atrasar o Respiro Negro.\n\n";
                break;
            case "EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a nivel superior a 16, los Clérigos mueren.":
                r = "EFEITOS PASSIVOS:\n\n-Concedem 15 de Esperança ao se juntar á Caravana, -20 de Esperança ao serem perdidos.\n\n-20% de probabilidade de atrasar o Respiro Negro em cada viagem.\n\n-Se o Respiro Negro chegar a um ní­vel superior a 16, os Clérigos morrem.";
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
            case "Mantenimiento Armas: El Herrero se encargará de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daño en 2. Este efecto Dura 3 días.":
                r = "Manutençío de Armas: O Ferreiro se encarregará de fazer uma manutençío geral nas armas dos personagens. Aumentando seu Ataque em 1 e seu dano em 2. Este efeito dura 3 dias.";
                break;
            case "Mantenimiento Armaduras: El Herrero se encargará de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 días.":
                r = "Manutençío de Armaduras: O Ferreiro se encarregará de fazer uma manutençío geral nas armaduras dos personagens. Aumentando sua Defesa em 1 e sua Armadura em 2. Este efeito dura 3 dias.";
                break;
            case "Realizar: 200 Oro":
                r = "Realizar: 200 Ouro";
                break;
            case "Activo por ":
                r = "Ativo por ";
                break;
            case " Días":
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
            case "% la curación pasiva de la Caravana.\n\nEste índice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.":
                r = "% a cura passiva da Caravana.\n\nEsse índice aumenta em 3% cada vez que a Caravana visita uma Clareira.\n\n-Ã€s vezes sío descuidados ao coletar ervas. +2% de chance de ocorrer um ataque á caravana após descansar.";
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
            case "Este séquito está constituído por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
                r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mínimo.";
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
            case "EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada día hay un 30% de chances de que hagan un festán y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.":
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
                r = "Ã€ Espreita";
                break;
            case "Arma Envenenada":
                r = "Arma Envenenada";
                break;
            case "Desestabilizado":
                r = "Desestabilizado";
                break;
            case "<b>¡Enfurecido!</b>":
                r = "<b>¡Enfurecido!</b>";
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
            case "Frío":
                r = "Frio";
                break;
            case "Aliento Negro: Débil":
                r = "Respiro Negro: Fraco";
                break;
            case "Aliento Negro: Presente":
                r = "Respiro Negro: Presente";
                break;
            case "Aliento Negro: Fuerte":
                r = "Respiro Negro: Forte";
                break;
            case "Aliento Negro: Empoderante":
                r = "Respiro Negro: Fortalecedor";
                break;
            case "Oscuridad":
                r = "Escuridío";
                break;
            case "Fatigado":
                r = "Fatigado";
                break;
            case "Bendecido":
                r = "Abençoado";
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
            case "Atento":
                r = "Alerta";
                break;
            case "Jefe":
                r = "Chefe";
                break;
            case "Planta":
                r = "Planta";
                break;
            case "Fey":
                r = "Feerico";
                break;
            case "Nomuerto":
                r = "Morto-vivo";
                break;
            case "Animal":
                r = "Animal";
                break;
            case "Humanoide":
                r = "Humanoide";
                break;
            case "Constructo":
                r = "Constructo";
                break;
            case "Criatura":
                r = "Criatura";
                break;
            case "Corrupto":
                r = "Corrompido";
                break;
            case "Bestia":
                r = "Besta";
                break;
            case "Volador":
                r = "Voador";
                break;
            case "Gigante":
                r = "Gigante";
                break;
            case "Kale'Tav":
                r = "Kale'Tav";
                break;
            case "Zarkil":
                r = "Zarkil";
                break;
            case "Demonio":
                r = "Demonio";
                break;
            case "Dragon":
                r = "Dragao";
                break;
            case "Terminó turno con AP disponible, aumenta defensa vs próximo golpe.":
                r = "Terminou o turno com PA disponível, aumenta a defesa contra o próximo ataque.";
                break;
            case "Se consume al recibir el próximo ataque.":
                r = "É consumido ao receber o próximo ataque.";
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
                /*case "Distraído":
                    r = "Distraído";*/
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
            case " falló la Tirada de Concentración y ya no acumula energía.":
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
            case "Energía Absorbida":
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
                r = "Ã€ Espreita";
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
            case "Elixir de Resistencia al Frío":
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
                r = " BÃ´nus de dano elemental de Ácido.";
                break;
            case " Bonus daño elemental Arcano.":
                r = " BÃ´nus de dano elemental Arcano.";
                break;
            case " Bonus daño elemental Fuego.":
                r = " BÃ´nus de dano elemental de Fogo.";
                break;
            case " Bonus daño elemental Hielo.":
                r = " BÃ´nus de dano elemental de Gelo.";
                break;
            case " Bonus daño elemental Necro.":
                r = " BÃ´nus de dano elemental Necrótico.";
                break;
            case " Bonus daño elemental Divino.":
                r = " BÃ´nus de dano elemental Divino.";
                break;
            case " Bonus daño elemental Rayo.":
                r = " BÃ´nus de dano elemental de Raio.";
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
            case "Energía: Nivel de Energía Acumulada por el Canalizador.":
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
                r = "Reduçío de dano crítico recebido: ";
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
            case "Crítico Dado: ":
                r = "Crítico causado: ";
                break;
            case "Daño Crítico: ":
                r = "Dano Crítico: ";
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
                r = "BÃ´nus de dano Ácido: ";
                break;
            case "Bonus daño arcano: ":
                r = "BÃ´nus de dano arcano: ";
                break;
            case "Bonus daño fuego: ":
                r = "BÃ´nus de dano fogo: ";
                break;
            case "Bonus daño hielo: ":
                r = "BÃ´nus de dano gelo: ";
                break;
            case "Bonus daño necro: ":
                r = "BÃ´nus de dano necro: ";
                break;
            case "Bonus daño rayo: ":
                r = "BÃ´nus de dano raio: ";
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
            case "Valentía Global Alta":
                r = "Bravura Global Alta";
                break;
            case "Valentia Global Alta":
                r = "Bravura Global Alta";
                break;
            case "Valentía Global Muy Alta":
                r = "Bravura Global Muito Alta";
                break;
            case "Valentia Global Muy Alta":
                r = "Bravura Global Muito Alta";
                break;
            case "Dudando":
                r = "Hesitante";
                break;
            case "Tambaleando":
                r = "Cambaleando";
                break;
            case "Provocado":
                r = "Provocado";
                break;
            case "Adolorido":
                r = "Ferido";
                break;
            case "Provocado: solo puede usar acciones hostiles contra quien aplicó este estado.":
                r = "Provocado: so pode usar ações hostis contra quem aplicou este estado.";
                break;
            case "Vulnerabilidad Expuesta":
                r = "Vulnerabilidade Exposta";
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
            case "Armadura: reduce el daño físico recibido.":
                r = "Armadura: reduz o dano físico recebido.";
                break;
            case "Reflejos: resistencia a determinados efectos de ataques.":
                r = "Reflexos: resistência a determinados efeitos de ataques.";
                break;
            case "Fortaleza: resistencia a efectos físicos.":
                r = "Fortaleza: resistência a efeitos físicos.";
                break;
            case "Mental: resistencia a efectos mentales.":
                r = "Mental: resistência a efeitos mentais.";
                break;
            case "Valentía: moral general en combate.":
                r = "Bravura: moral geral em combate.";
                break;
            case "Resistencia al Fuego: Cantidad de daño que previene.":
                r = "Resistência a Fogo: quantidade de dano que previne.";
                break;
            case "Resistencia al Frío: Cantidad de daño que previene.":
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
            case "Símbolo de Protección Arcano":
                r = "Símbolo Arcano de Proteçío";
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
            case "Aumenta la resistencia al frío en 5 por el combate.":
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
            case "<i>El Lobo Alfa Espectral es el líder de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>":
                r = "<i>O Lobo Alfa Espectral é o líder da matilha, possui uma constituiçío mais forte e resistente que a dos outros lobos, embora seja um pouco menos ágil.</i>\n\n<color=#199F10>-Tem a capacidade de uivar para motivar os outros lobos.</color>\n<color=#EE0000>-Se ficar sozinho, nío poderá motivar ninguém.</color>";
                break;
            case "Driada Quemada":
                r = "Drí­ade Queimada";
                break;
            case "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raíces ignífugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>":
                r = "<i>Antes servas e cuidadoras da floresta, agora sío manifestações de vingança e ódio contra qualquer invasor da Floresta Ardente.</i>\n\n<color=#199F10>-Pode enredar com raí­zes ignífugas.\n-Ataque á distância.</color>\n<color=#EE0000>-Relativamente fraca.</color>";
                break;
            case "Espectro del Bosque":
                r = "Espectro da Floresta";
                break;
            case "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques físicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad física momentáneamente al atacar.</color>":
                r = "<i>O Espectro da Floresta é uma alma penada presa entre as cinzas de uma floresta calcinada, com sua ira alimentada pela destruiçío que nío pÃ´de evitar. Errante e vingativo, ataca aqueles que ousam cruzar sua terra carbonizada.</i>\n\n<color=#199F10>-Imune a ataques físicos.\n-Pode amaldiçoar com Perdiçío.</color>\n<color=#EE0000>-Perde parte de sua imunidade física momentaneamente ao atacar.</color>";
                break;
            case "Fuego Fatuo":
                r = "Fogo-Fátuo";
                break;
            case "<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guía a los incautos hacia la perdición, vengando la memoria del bosque caído.</i>\n\n<color=#199F10>-Resistente a ataques físicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>":
                r = "<i>Um eco etéreo das chamas que o consumiram, dançando entre as cinzas como lembrança do desastre. Embora pareça inofensivo, guia os incautos á perdiçío, vingando a memória da floresta caí­da.</i>\n\n<color=#199F10>-Resistente a ataques físicos.\n-Pode encarnar em seus inimigos.</color>\n<color=#EE0000>-Tem pouca vida.</color>";
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
            case "<i>Constituido por pura energía arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques físicos.</color>":
                r = "<i>Constituí­do de pura energia arcana, este ser etéreo defende o Canalizador que lhe deu forma.</i>\n\n<color=#199F10>-Resistente a ataques físicos.</color>";
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
            case "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crítico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>":
                r = "<i>Este homem já era maligno antes, e agora a situaçío desesperadora acentuou sua crueldade.</i>\n\n<color=#199F10>-Boa capacidade de Crítico.\n-Começa escondido.\n-Pode envenenar sua arma.</color>\n<color=#EE0000>-Bastante fraco.</color>";
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
                r = "<i>Antes um habitante destas terras, agora corrompido pelo Respiro Negro, deformado e faminto.</i>\n\n<color=#A020F0>-Corrompido.</color>\n<color=#199F10>-Pode enfraquecer.\n-Absorve vida de Personagens Corrompidos.</color>\n<color=#EE0000>-Relativamente fraco.</color>";
                break;
            case "Guerrero Corrompido":
                r = "Guerreiro Corrompido";
                break;
            case "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Antes um habitante destas terras, agora corrompido pelo Respiro Negro, deformado e faminto.</i>\n\n<color=#A020F0>-Corrompido.</color>\n<color=#199F10>-Forte.\n-Atinge em área.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
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
            case "-Las Almas Danzantes guían a la caravana. +5 Esperanza":
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
                r = "REAÃ‡íÆ’O: ao morrer, condena o inimigo que deu o golpe final.";
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
            case "Escudado: 10% chances por stack de evitar un ataque físico. Al evitar uno, pierde un stack.":
                r = "Escudado: 10% de chance por acúmulo de evitar um ataque físico. Ao evitar um, perde um acúmulo.";
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
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Respiro Negro de seu mestre.</i>\n\n<color=#199F10>-Unidade Escudada.\n-Boa Armadura.\n-Ao morrer, deixa uma nuvem de Respiro Negro.</color>\n<color=#EE0000>-Movimento limitado.</color>";
                break;
            case "Extasiado por Aliento Negro":
                r = "Extasiado pelo Respiro Negro";
                break;
            case "Restos de Aliento: Potencia y cura a los Vengadores de Kadryn.":
                r = "Resíduos de Alento: fortalecem e curam os Vingadores de Kadryn.";
                break;
            case "Reacción: Al morir genera restos de Aliento Negro en el campo de batalla.":
                r = "Reaçío: ao morrer, gera resíduos de Respiro Negro no campo de batalha.";
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
            case "Estocada":
                r = "Estocada";
                break;
            case "Estocada Alabarda":
                r = "Estocada de Alabarda";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Buen ataque.\n-Flecha envenenada.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Respiro Negro de seu mestre.</i>\n\n<color=#199F10>-Bom ataque.\n-Flecha envenenada.\n-Ao morrer, deixa uma nuvem de Respiro Negro.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Tiro con Arco":
                r = "Disparo de Arco";
                break;
            case "Primer Golpe":
                r = "Primeiro Golpe";
                break;
            case "Predicador del Aliento Negro":
                r = "Pregador do Respiro Negro";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de rango infalible.\n-Potencia Aliados.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Respiro Negro de seu mestre.</i>\n\n<color=#199F10>-Ataque á distância infalí­vel.\n-Fortalece Aliados.\n-Ao morrer, deixa uma nuvem de Respiro Negro.</color>\n<color=#EE0000>-Pouco resistente.</color>";
                break;
            case "Oración de Kadryn":
                r = "Oraçío de Kadryn";
                break;
            case "Rayo Necrótico":
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
                r = "O Respiro Negro se espalha pelo campo inimigo.";
                break;
            case "desata un rayo necrótico sobre":
                r = "desencadeia um raio necrótico sobre";
                break;
            case "Sus defensas se corroen por el Aliento Negro.":
                r = "Suas defesas sío corroídas pelo Respiro Negro.";
                break;
            case "Castigar a los Malvados":
                r = "Punir os Malvados";
                break;
            case "Marca: ":
                r = "Marca: ";
                break;
            case " posee bonificaciones de daño y ataque con ataques individuales contra este enemigo.":
                r = " recebe bonus de dano e ataque com ataques individuais contra este inimigo.";
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
            case "<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia física es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>":
                r = "<i>Este urso se tornou um espectro feroz que vagueia pela floresta ardente. Seu poder físico é aterrador.</i>\n\n<color=#199F10>-Ataques esmagadores.\n-Grande quantidade de vida.</color>\n<color=#EE0000>-Maior probabilidade de falha crí­tica.</color>";
                break;
            case "Bonus de daño elemental.":
                r = "BÃ´nus de dano elemental.";
                break;
            case "<i>Esta bestia oriunda del Paso es material de varias leyendas y pesadillas entre los Kale'Tav. De cuerpo robusto y cuernos afilados, supone un peligro para los viajeros incautos.</i>\n\n<color=#199F10>-Ataques de carga en fila.\n-Regeneración leve.</color>\n<color=#EE0000>-Lento.</color>":
                r = "<i>Esta besta oriunda da Passagem é tema de várias lendas e pesadelos entre os Kale'Tav. De corpo robusto e chifres afiados, representa um perigo para os viajantes incautos.</i>\n\n<color=#199F10>-Ataques de investida em linha.\n-Regeneraçío leve.</color>\n<color=#EE0000>-Lento.</color>";
                break;
            case "Milicianos disponibles: ":
                r = "Milicianos disponí­veis: ";
                break;
            case "<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de oportunidad.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>":
                r = "<i>Organizaçío de mercenários humanos que faziam parte do exército derrotado do Lich Kadryn. Agora buscam vingança, tentando garantir que ninguém escape do Respiro Negro de seu mestre.</i>\n\n<color=#199F10>-Ataque de oportunidade.\n-Boa Armadura.\n-Ao morrer, deixa uma nuvem de Respiro Negro.</color>\n<color=#EE0000>-Movimento limitado.</color>";
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
                r = "Ã€ medida que você viaja pela floresta, as chamas envolverío regiões do mapa de forma inesperada.\n\nSe tentar atravessar um Nó em chamas, perderá 10 de Esperança e 8-15 Civis.\nNío será possí­vel descansar em nós incendiados.\n\nAlém disso, as batalhas que ocorrerem em um Nó incendiado terío chamas no campo de batalha.";
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
                r = "A tribo Kale'Tav está realizando rituais na área, preparando-se para o Respiro Negro.\n\nAo ouvir seus tambores ao longe, você saberá onde eles estío.\nPara cada Ritual concluí­do, seus combatentes receberío bÃ´nus em batalha.\n\nPara interromper um ritual, você deve se aproximar dos nós marcados e derrotá-los.\n\nForça Kale'Tav: ";
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
            case "Destruir Obstaculo":
                r = "Destruir Obstáculo";
                break;
            case "Destruyes":
                r = "Você destrói";
                break;
            case "Este obstaculo no puede ser destruido por tus unidades.":
                r = "Este obstáculo nío pode ser destruí­do por suas unidades.";
                break;
            case "Gasta 3 PA para destruir un obstaculo adyacente de tu mismo lado si lo permite. Termina tu turno.":
                r = "Gasta 3 PA para destruir um obstaculo adjacente do seu lado, se for permitido. Encerra seu turno.";
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
                r = "Devido á invasío, Nedukazal está envolta em caos e escuridío, portanto a caravana nío conseguirá ver claramente o caminho á frente.\n\nPor depender da própria luz, será mais propensa a sofrer emboscadas (+20%).\n\nMelhore as <b>Tochas de Pé</b> para aumentar o alcance da visío.\n\nO Respiro Negro nío será uma preocupaçío nesta zona.";
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
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverán.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Raça de criaturas demoníacas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Ao esquivar de um ataque, irío se mover.\n-Pode ver escondidos.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
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
            case "Mirada de Masacre: al moverse aquí, Tirada de salvación mental CD 13 o se pierde el turno.":
                r = "Olhar do Massacre: ao se mover para cá, faça uma jogada de resistência mental CD 13 ou perca o turno.";
                break;
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>":
                r = "<i>Raça de criaturas demoníacas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Pode aterrorizar criaturas á frente.\n-Pode ver escondidos.</color>\n<color=#EE0000>-Possui apenas um tipo de ataque.</color>";
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
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que además motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>":
                r = "<i>Raça de criaturas demoníacas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Grito atordoante que também motiva aliados.\n-Pode ver escondidos.\n-Pode atacar repetidamente.</color>\n<color=#EE0000></color>";
                break;
            case "Rayo Debilitador":
                r = "Raio Enfraquecedor";
                break;
            case "Debilitado":
                r = "Enfraquecido";
                break;
            case "<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-Débil</color>":
                r = "<i>Raça de criaturas demoníacas que invadem Nedulkazan por baixo em busca de sacrifí­cios e ouro. </i>\n\n<color=#199F10>-Ataque enfraquecedor infalí­vel.\n-Pode ver escondidos.\n-Voador.</color>\n<color=#EE0000>-Fraco</color>";
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
            case "<i>Manifestación de la energía espectral del bosque. Desde su interior emana un fulgor fantasmal frío, como un espí­ritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrótico que condena a dos objetivos.</color>\n<color=#EE0000>-Inmóvil.</color>":
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
            case "No hay suficientes energía":
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
            case "Crítico":
                r = "Crítico";
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
            case "<color=#FF6666>No puedes descansar aquí.</color>":
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
            case "Intercambiar":
                r = "Trocar";
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
                r = "Finalmente, a caravana chegou á Cidade Portuária de Serria, onde a populaçío civil se prepara para embarcar e assim escapar do Respiro Negro.";
                break;
            case "El viaje ha durado ":
                r = "A viagem durou ";
                break;
            case " días enteros y han sobrevivido ":
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
            case "Cuando el campamento ya está armado, un comerciante rezagado se acerca a la Caravana con una mula cargada y una sonrisa cansada.\n\n":
                r = "Quando o acampamento já está montado, um mercador atrasado se aproxima da Caravana com uma mula carregada e um sorriso cansado.\n\n";
                break;
            case "Dice que viene siguiendo el rastro del convoy desde hace días y que, si lo dejas instalarse un rato, puede abrir un pequeño puesto antes de seguir su camino.\n\n":
                r = "Ele diz que vem seguindo o rastro do comboio há dias e que, se você deixá-lo se instalar por um tempo, pode abrir uma pequena banca antes de seguir viagem.\n\n";
                break;
            case "<color=#a0e812><b>Al continuar, se abrirá un Puesto Comercial.</b></color>":
                r = "<color=#a0e812><b>Ao continuar, um Posto Comercial será aberto.</b></color>";
                break;
            case "Ya entrada la noche, una figura se acerca al campamento con las manos a la vista y el equipo a cuestas.\n\n":
                r = "Já noite adentro, uma figura se aproxima do acampamento com as mãos Ã  vista e o equipamento nas costas.\n\n";
                break;
            case "Cuenta que perdió a su grupo en el camino y pide un lugar en la Caravana. No promete milagros, pero sí pelear mientras le queden fuerzas.\n\n":
                r = "Conta que perdeu seu grupo na estrada e pede um lugar na Caravana. Não promete milagres, mas promete lutar enquanto ainda tiver forças.\n\n";
                break;
            case "<color=#a0e812>-Si decides aceptar, un Héroe aleatorio se unirá a la Caravana.</color>\n\n":
                r = "<color=#a0e812>-Se você aceitar, um Herói aleatório se juntará Ã  Caravana.</color>\n\n";
                break;
            case "<color=#d6d6d6>-Si decides rechazar, seguirá su camino por su cuenta.</color>":
                r = "<color=#d6d6d6>-Se você recusar, ele seguirá seu caminho por conta própria.</color>";
                break;
            case "En mitad del descanso, un ave desciende sobre uno de los carros con un mensaje atado a la pata.\n\n":
                r = "No meio do descanso, uma ave desce sobre uma das carroças com uma mensagem presa na pata.\n\n";
                break;
            case "La nota viene de Serria: han enviado una misión de salvamento para asistir a la Caravana y marcan un punto de encuentro más adelante en el camino.\n\n":
                r = "A nota vem de Serria: eles enviaram uma missão de salvamento para ajudar a Caravana e marcaram um ponto de encontro mais adiante na estrada.\n\n";
                break;
            case "<color=#a0e812><b>Al continuar, se marcará una Misión de Salvamento en el mapa.</b></color>":
                r = "<color=#a0e812><b>Ao continuar, uma Missío de Salvamento será marcada no mapa.</b></color>";
                break;
            case "Entre la niebla y el viento se cuela un ritmo de tambores que nadie logra ubicar con claridad.\n\n":
                r = "Entre a névoa e o vento surge um ritmo de tambores que ninguém consegue localizar com clareza.\n\n";
                break;
            case "Los Civiles miran alrededor con inquietud. Puedes forzar a la Caravana a apurar el paso o frenar un momento hasta recuperar la calma.\n\n":
                r = "Os Civis olham ao redor com inquietação. Você pode forçar a Caravana a apressar a marcha ou parar por um momento até recuperar a calma.\n\n";
                break;
            case "<color=#ba3fef>-Si decides apurar el paso, el esfuerzo dejará a la Caravana más cansada. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se você apressar a marcha, o esforço deixará a Caravana mais cansada. +1 Fadiga.</color>\n\n";
                break;
            case "Un tramo helado del camino cruje bajo el peso de la Caravana. ":
                r = "Um trecho congelado do caminho range sob o peso da Caravana. ";
                break;
            case "</color></b> puede intentar guiar el cruce antes de que el hielo ceda.\n\n":
                r = "</color></b> pode tentar guiar a travessia antes que o gelo ceda.\n\n";
                break;
            case "<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC 12 (TS Reflejos actual: ":
                r = "<color=#ba3fef>-Teste de Resistência: TS Reflexos CD 12 (TS Reflexos atual: ";
                break;
            case "). Si supera la tirada, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "). Se passar no teste, ganhará 40 Experiência. Se falhar, obterá Ferida.</color>\n\n";
                break;
            case "Un tramo helado del camino cruje bajo el peso de la Caravana. Uno de los Héroes puede intentar guiar el cruce antes de que el hielo ceda.\n\n":
                r = "Um trecho congelado do caminho range sob o peso da Caravana. Um dos Heróis pode tentar guiar a travessia antes que o gelo ceda.\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "<color=#ba3fef>-Se tentar, fará um Teste de Resistência: TS Reflexos CD 12. Se passar, ganhará 40 Experiência. Se falhar, obterá Ferida.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides rodear el tramo, el Aliento Negro avanzará.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir contornar o trecho, o Respiro Negro avançará.</color>\n\n";
                break;
            case "A los costados del camino aparecen varias efigies Kale'Tav clavadas en la nieve, adornadas con huesos, plumas y telas endurecidas por el hielo.\n\n":
                r = "Ã€s margens do caminho surgem várias efígies Kale'Tav fincadas na neve, adornadas com ossos, penas e tecidos endurecidos pelo gelo.\n\n";
                break;
            case "Aunque nadie se acerque, su sola presencia alcanza para inquietar a la Caravana.\n\n":
                r = "Mesmo sem ninguém se aproximar, sua mera presença já basta para inquietar a Caravana.\n\n";
                break;
            case "<color=#ba3fef><b>-6 Esperanza</b></color>":
                r = "<color=#ba3fef><b>-6 Esperança</b></color>";
                break;
            case "Figuras encapuchadas se recortan un instante entre las rocas y luego desaparecen. No hace falta ver más para entender que una partida de caza Kale'Tav anda cerca.\n\n":
                r = "Figuras encapuzadas aparecem por um instante entre as rochas e depois somem. Não é preciso ver mais para entender que um grupo de caça Kale'Tav está por perto.\n\n";
                break;
            case "Puedes preparar a los Héroes para un enfrentamiento o esconder a los Civiles y perder tiempo en el desorden.\n\n":
                r = "Você pode preparar os Heróis para um confronto ou esconder os Civis e perder tempo na confusão.\n\n";
                break;
            case "<color=#ba3fef>-Si decides prepararte, comenzará una batalla.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir se preparar, uma batalha começará.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides esconder a los Civiles, el miedo y el desorden dejarán secuelas. +1 Fatiga, -3 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir esconder os Civis, o medo e a desordem deixarão marcas. +1 Fadiga, -3 Esperança.</color>\n\n";
                break;
            case " soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n":
                r = " suporta o avanço como pode, mas o frio da Passagem acaba penetrando mais do que deveria.\n\n";
                break;
            case "Uno de los Héroes soporta el avance como puede, pero el frío del Paso termina calándole más de la cuenta.\n\n":
                r = "Um dos Heróis suporta o avanço como pode, mas o frio da Passagem acaba penetrando mais do que deveria.\n\n";
                break;
            case "La Caravana encuentra un tótem recién erigido, cubierto con sangre seca y cintas agitadas por el viento.\n\n":
                r = "A Caravana encontra um totem recém-erguido, coberto de sangue seco e fitas agitadas pelo vento.\n\n";
                break;
            case "Los Civiles entienden enseguida que no están cruzando un desierto vacío, sino un territorio que alguien defiende con fanatismo.\n\n":
                r = "Os Civis entendem na hora que não estão cruzando um vazio deserto, mas um território que alguém defende com fanatismo.\n\n";
                break;
            case "<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -3 Esperanza.</b></color>":
                r = "<color=#ba3fef><b>A Força Kale'Tav aumenta em 1. -3 Esperança.</b></color>";
                break;
            case "Entre varias rocas altas, la Caravana encuentra un reparo natural que corta el viento por un rato.\n\n":
                r = "Entre várias rochas altas, a Caravana encontra um abrigo natural que corta o vento por um tempo.\n\n";
                break;
            case "No dura mucho, pero alcanza para recomponerse antes de seguir.\n\n":
                r = "Não dura muito, mas basta para se recompor antes de seguir.\n\n";
                break;
            case "Unas huellas frescas de carnero de montaña se internan por una cornisa estrecha que parece evitar parte del ascenso.\n\n":
                r = "Rastros frescos de um carneiro da montanha seguem por uma cornija estreita que parece evitar parte da subida.\n\n";
                break;
            case "Puedes seguir el rastro e intentar usar ese paso o mantener la ruta principal sin arriesgarte.\n\n":
                r = "Você pode seguir a trilha e tentar usar essa passagem, ou manter a rota principal sem se arriscar.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguir el rastro, se intentará encontrar un Atajo.</color>\n\n":
                r = "<color=#a0e812>-Se decidir seguir a trilha, será feita uma tentativa de encontrar um Atalho.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides mantener la ruta, la visión del sendero levantará el ánimo. +4 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se decidir manter a rota, a visão da trilha elevará o ânimo. +4 Esperança.</color>\n\n";
                break;
            case "Por un momento, la niebla se abre y desde una altura se alcanza a ver con claridad buena parte del Paso.\n\n":
                r = "Por um momento, a névoa se abre e de um ponto alto é possível ver com clareza boa parte da Passagem.\n\n";
                break;
            case "La Caravana aprovecha para orientarse mejor antes de seguir.\n\n":
                r = "A Caravana aproveita para se orientar melhor antes de seguir.\n\n";
                break;
            case "<color=#a0e812><b>Se revelarán nodos cercanos.</b></color>":
                r = "<color=#a0e812><b>Nós próximos serão revelados.</b></color>";
                break;
            case "Una efigie Kale'Tav yace derribada a un lado del camino, partida por la mitad y cubierta de nieve.\n\n":
                r = "Uma efígie Kale'Tav jaz derrubada Ã  beira do caminho, partida ao meio e coberta de neve.\n\n";
                break;
            case "La imagen corre rápido entre los Civiles: por una vez, algo del Paso parece menos invencible.\n\n":
                r = "A visão corre rápido entre os Civis: por uma vez, algo da Passagem parece menos invencível.\n\n";
                break;
            case "<color=#a0e812><b>-1 Fuerza Kale'Tav, +5 Esperanza.</b></color>":
                r = "<color=#a0e812><b>-1 Força Kale'Tav, +5 Esperança.</b></color>";
                break;
            case "Frente al viento helado, <b><color=#d1006f>":
                r = "Diante do vento gelado, <b><color=#d1006f>";
                break;
            case "</color></b> se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n":
                r = "</color></b> para por um instante, olha para o caminho e faz um juramento em voz baixa.\n\n";
                break;
            case "Frente al viento helado, uno de los Héroes se detiene un instante, mira el camino y hace un juramento en voz baja.\n\n":
                r = "Diante do vento gelado, um dos Heróis para por um instante, olha para o caminho e faz um juramento em voz baixa.\n\n";
                break;
            case "La determinación con la que retoma la marcha contagia al resto.\n\n":
                r = "A determinação com que retoma a marcha contagia o resto.\n\n";
                break;
            case "<color=#a0e812><b>Gana 50 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Ganha 50 Experiência e Alta Moral por 3 dias.</b></color>";
                break;
            case "El viento cambia de golpe y barre la niebla helada del frente por un buen trecho.\n\n":
                r = "O vento muda de repente e varre a névoa gelada do caminho por um bom trecho.\n\n";
                break;
            case "La Caravana consigue avanzar con mejor ritmo y algo más de seguridad.\n\n":
                r = "A Caravana consegue avançar com melhor ritmo e um pouco mais de segurança.\n\n";
                break;
            case "<color=#a0e812><b>-1 Avance Aliento Negro</b></color>":
                r = "<color=#a0e812><b>-1 Avanço do Respiro Negro</b></color>";
                break;
            case "Antes del amanecer, unos cánticos graves atraviesan el Paso y llegan al campamento mezclados con el viento.\n\n":
                r = "Antes do amanhecer, cânticos graves atravessam a Passagem e chegam ao acampamento misturados ao vento.\n\n";
                break;
            case "Nadie ve a los Kale'Tav, pero el sonido basta para que casi nadie vuelva a dormir tranquilo.\n\n":
                r = "Ninguém vê os Kale'Tav, mas o som basta para que quase ninguém volte a dormir em paz.\n\n";
                break;
            case "Al amanecer encuentran huellas frescas marcando un círculo incompleto alrededor del campamento.\n\n":
                r = "Ao amanhecer, encontram pegadas frescas marcando um círculo incompleto ao redor do acampamento.\n\n";
                break;
            case "No parece un ataque fallido. Más bien un mensaje. Puedes revisar bien el perímetro o mantener la calma y evitar que el rumor corra entre los Civiles.\n\n":
                r = "Não parece um ataque fracassado. Parece mais uma mensagem. Você pode inspecionar bem o perímetro ou manter a calma e evitar que o rumor se espalhe entre os Civis.\n\n";
                break;
            case "<color=#ba3fef>-Si decides revisar, partirán más tarde. +1 Avance Aliento Negro.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir inspecionar, partirão mais tarde. +1 Avanço do Respiro Negro.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides mantener la calma, los rumores igual harán mella. -9 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir manter a calma, os rumores ainda assim vão pesar. -9 Esperança.</color>\n\n";
                break;
            case "</color></b> pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n":
                r = "</color></b> passou boa parte da noite em claro, atento a cada ruído do vento entre as pedras.\n\n";
                break;
            case "Uno de los Héroes pasó buena parte de la noche en vela, atento a cada ruido del viento entre las piedras.\n\n":
                r = "Um dos Heróis passou boa parte da noite em claro, atento a cada ruído do vento entre as pedras.\n\n";
                break;
            case "Al amanecer sigue en pie, pero el descanso no alcanzó para despejarle la cabeza.\n\n":
                r = "Ao amanhecer, ainda está de pé, mas o descanso não bastou para clarear a cabeça.\n\n";
                break;
            case "<color=#ba3fef><b>Obtiene Baja Moral por 3 días.</b></color>":
                r = "<color=#ba3fef><b>Obtém Baixa Moral por 3 dias.</b></color>";
                break;
            case "Muy cerca de las tiendas aparece un símbolo Kale'Tav trazado durante la noche sobre la nieve endurecida.\n\n":
                r = "Bem perto das tendas aparece um símbolo Kale'Tav traçado durante a noite sobre a neve endurecida.\n\n";
                break;
            case "La marca deja claro que la Caravana fue observada mientras dormía.\n\n":
                r = "A marca deixa claro que a Caravana foi observada enquanto dormia.\n\n";
                break;
            case "<color=#ba3fef><b>La Fuerza Kale'Tav aumenta en 1. -2 Esperanza.</b></color>":
                r = "<color=#ba3fef><b>A Força Kale'Tav aumenta em 1. -2 Esperança.</b></color>";
                break;
            case "La Caravana logra montar el campamento al abrigo de un paredón de roca que corta las peores ráfagas del Paso.\n\n":
                r = "A Caravana consegue montar acampamento ao abrigo de um paredão de rocha que corta as piores rajadas da Passagem.\n\n";
                break;
            case "Por una noche, dormir no se siente como resistir una agresión constante.\n\n":
                r = "Por uma noite, dormir não parece resistir a uma agressão constante.\n\n";
                break;
            case "La noche cae y, contra toda costumbre del lugar, no se oyen tambores, cuervos ni cánticos a la distancia.\n\n":
                r = "A noite cai e, contra todo costume do lugar, não se ouvem tambores, corvos nem cânticos Ã  distância.\n\n";
                break;
            case "Ese silencio extraño no inspira confianza, pero sí regala unas horas de paz que la Caravana necesitaba.\n\n":
                r = "Esse silêncio estranho não inspira confiança, mas oferece algumas horas de paz de que a Caravana precisava.\n\n";
                break;
            case "<color=#a0e812><b>+5 Esperanza</b></color>":
                r = "<color=#a0e812><b>+5 Esperança</b></color>";
                break;
            case "Antes de levantar el campamento, encuentran un rastro de animales que cruza una ladera más amable que la ruta habitual.\n\n":
                r = "Antes de levantar acampamento, encontram um rastro de animais que cruza uma encosta mais suave que a rota habitual.\n\n";
                break;
            case "Puedes seguirlo para intentar encontrar un paso mejor o estudiarlo con calma para orientarte antes de partir.\n\n":
                r = "Você pode segui-lo para tentar encontrar uma passagem melhor ou estudá-lo com calma para se orientar antes de partir.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguirlo, se intentará encontrar un Atajo.</color>\n\n":
                r = "<color=#a0e812>-Se decidir segui-lo, será feita uma tentativa de encontrar um Atalho.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides estudiarlo, se revelarán nodos cercanos.</color>\n\n":
                r = "<color=#a0e812>-Se decidir estudá-lo, nós próximos serão revelados.</color>\n\n";
                break;
            case "</color></b> contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n":
                r = "</color></b> contempla em silêncio a aurora gelada que se abre sobre os picos antes da marcha.\n\n";
                break;
            case "Uno de los Héroes contempla en silencio la aurora helada que se abre sobre las cumbres antes de la marcha.\n\n":
                r = "Um dos Heróis contempla em silêncio a aurora gelada que se abre sobre os picos antes da marcha.\n\n";
                break;
            case "La imagen queda grabada con fuerza y le devuelve algo de ánimo para lo que viene.\n\n":
                r = "A imagem fica gravada com força e lhe devolve algum ânimo para o que vem pela frente.\n\n";
                break;
            case "<color=#a0e812><b>Gana 35 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Ganha 35 Experiência e Alta Moral por 3 dias.</b></color>";
                break;
            case "Bajo el suelo helado y la piedra llegan golpes sordos y repetidos, como si algo enorme estuviera tanteando el camino desde abajo.\n\n":
                r = "Sob o solo gelado e a pedra chegam golpes surdos e repetidos, como se algo enorme estivesse sondando o caminho por baixo.\n\n";
                break;
            case "Los Civiles aprietan el paso sin que nadie se los ordene. Aunque no llegue a emerger nada, el simple sonido basta para desgastar a la Caravana.\n\n":
                r = "Os Civis apertam o passo sem que ninguém precise ordenar. Mesmo que nada chegue a emergir, o simples som basta para desgastar a Caravana.\n\n";
                break;
            case "Un tramo del camino se ha hundido y dejó un paso quebrado entre carros volcados, zanjas y piedras sueltas.\n\n":
                r = "Um trecho do caminho afundou e deixou uma passagem quebrada entre carroças viradas, valas e pedras soltas.\n\n";
                break;
            case "</color></b> puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n":
                r = "</color></b> pode tentar guiar a travessia antes que alguém caia no desnível.\n\n";
                break;
            case "<color=#ba3fef>-Tirada de Salvación: TS Reflejos DC ":
                r = "<color=#ba3fef>-Teste de Resistência: TR Reflexos CD ";
                break;
            case ").</i> Si la supera, ganará 40 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = ").</i> Se passar, ganhará 40 Experiência. Se falhar, sofrerá Ferida.</color>\n\n";
                break;
            case "Uno de los Héroes puede intentar guiar el cruce antes de que alguien caiga al desnivel.\n\n":
                r = "Um dos Heróis pode tentar guiar a travessia antes que alguém caia no desnível.\n\n";
                break;
            case "<color=#ba3fef>-Si decides rodear la brecha, la Caravana ganará +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir contornar a brecha, a Caravana ganhará +1 Fadiga.</color>\n\n";
                break;
            case "Desde la boca de un viejo pozo subterráneo suben gritos cortados y el sonido de uñas raspando piedra.\n\n":
                r = "Da boca de um velho poço subterrâneo sobem gritos cortados e o som de unhas raspando a pedra.\n\n";
                break;
            case "Nadie alcanza a ver qué hay abajo, pero está claro que algo se mueve en las profundidades. Puedes investigar o forzar a la Caravana a seguir.\n\n":
                r = "Ninguém consegue ver o que há lá embaixo, mas está claro que algo se move nas profundezas. Você pode investigar ou forçar a Caravana a seguir em frente.\n\n";
                break;
            case "<color=#ba3fef>-Si decides investigar, comenzará una batalla.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir investigar, uma batalha começará.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides seguir, el miedo se extenderá entre los Civiles. -5 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir seguir, o medo se espalhará entre os Civis. -5 Esperança.</color>\n\n";
                break;
            case "En algún punto de Nedukazal suenan campanas de alarma, aunque nadie alcanza a ver de dónde vienen.\n\n":
                r = "Em algum ponto de Nedukazal soam sinos de alarme, embora ninguém consiga ver de onde vêm.\n\n";
                break;
            case "El sonido viaja entre ruinas, corrales vacíos y casas abandonadas, y deja a la Caravana con la sensación de haber llegado demasiado tarde para ayudar a alguien.\n\n":
                r = "O som viaja entre ruínas, currais vazios e casas abandonadas, e deixa a Caravana com a sensação de ter chegado tarde demais para ajudar alguém.\n\n";
                break;
            case "Sombras veloces se recortan por momentos sobre tejados, tapias y galpones derruidos, y desaparecen antes de que nadie pueda apuntarlas bien.\n\n":
                r = "Sombras velozes se recortam por instantes sobre telhados, cercas e galpões destruídos, e desaparecem antes que alguém consiga mirar direito.\n\n";
                break;
            case "Puedes cerrar filas y avanzar con más cuidado o apurar el paso antes de que bajen sobre la Caravana.\n\n":
                r = "Você pode fechar fileiras e avançar com mais cuidado ou apressar a marcha antes que desçam sobre a Caravana.\n\n";
                break;
            case "<color=#ba3fef>-Si decides cerrar filas, el avance será más tenso. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir fechar fileiras, o avanço será mais tenso. +1 Fadiga.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides apurar el paso, varios Civiles quedarán rezagados en el desorden. -3 Civiles, -4 Esperanza.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir apressar a marcha, vários Civis ficarão para trás no desordem. -3 Civis, -4 Esperança.</color>\n\n";
                break;
            case "Tras una puerta atrancada, la Caravana encuentra un refugio improvisado que no resistió el ataque.\n\n":
                r = "Atrás de uma porta travada, a Caravana encontra um abrigo improvisado que não resistiu ao ataque.\n\n";
                break;
            case "</color></b> se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n":
                r = "</color></b> fica olhando em silêncio para as marcas de garras na madeira e os restos da luta.\n\n";
                break;
            case "Uno de los Héroes se queda mirando en silencio las marcas de garras en la madera y los restos del forcejeo.\n\n":
                r = "Um dos Heróis fica olhando em silêncio para as marcas de garras na madeira e os restos da luta.\n\n";
                break;
            case "Cuando vuelven al camino, la imagen sigue pesándole.\n\n":
                r = "Quando voltam ao caminho, a imagem continua pesando sobre ele.\n\n";
                break;
            case "En puertas, postes y cercos alguien dejó faroles encendidos apuntando hacia el rumbo más seguro, como si Nedukazal todavía intentara guiar a los vivos.\n\n":
                r = "Em portas, postes e cercas, alguém deixou lampiões acesos apontando para a direção mais segura, como se Nedukazal ainda tentasse guiar os vivos.\n\n";
                break;
            case "La Caravana aprovecha esas luces para orientarse mejor entre caseríos arrasados y ruinas dispersas.\n\n":
                r = "A Caravana aproveita essas luzes para se orientar melhor entre povoados arrasados e ruínas dispersas.\n\n";
                break;
            case "Una vieja barricada de muebles, carros y vigas todavía sigue en pie y ofrece un breve reparo contra ataques y miradas desde las ruinas.\n\n":
                r = "Uma velha barricada de móveis, carroças e vigas ainda segue de pé e oferece um breve abrigo contra ataques e olhares das ruínas.\n\n";
                break;
            case "No es segura a largo plazo, pero alcanza para que la Caravana recupere el aliento antes de seguir.\n\n":
                r = "Não é segura a longo prazo, mas basta para que a Caravana recupere o fÃ´lego antes de seguir.\n\n";
                break;
            case "Detrás de una bodega semienterrada y casi tapada por escombros encuentran a un pequeño grupo de supervivientes escondidos.\n\n":
                r = "Atrás de um depósito semienterrado e quase coberto por escombros, encontram um pequeno grupo de sobreviventes escondidos.\n\n";
                break;
            case "Suben a la luz temblando, pero al ver a la Caravana aceptan marcharse con ustedes.\n\n":
                r = "Eles sobem para a luz tremendo, mas ao ver a Caravana aceitam partir com vocês.\n\n";
                break;
            case "<color=#a0e812><b>+6-12 Civiles, +4 Esperanza</b></color>":
                r = "<color=#a0e812><b>+6-12 Civis, +4 Esperança</b></color>";
                break;
            case "En cercos, muros y postes derruidos aparecen marcas de tiza hechas a toda prisa: flechas, cruces y advertencias.\n\n":
                r = "Em cercas, muros e postes derrubados aparecem marcas de giz feitas Ã s pressas: flechas, cruzes e avisos.\n\n";
                break;
            case "Alguien estuvo guiando a otros supervivientes entre caminos, puestos y asentamientos en ruinas. Puedes seguir esas señales o reforzarlas para los que vengan detrás.\n\n":
                r = "Alguém esteve guiando outros sobreviventes entre caminhos, postos e assentamentos em ruínas. Você pode seguir esses sinais ou reforçá-los para quem vier depois.\n\n";
                break;
            case "<color=#a0e812>-Si decides seguirlas, se revelarán nodos cercanos.</color>\n\n":
                r = "<color=#a0e812>-Se decidir segui-las, nós próximos serão revelados.</color>\n\n";
                break;
            case "<color=#a0e812>-Si decides reforzar el camino, el gesto levantará el ánimo. +5 Esperanza.</color>\n\n":
                r = "<color=#a0e812>-Se decidir reforçar o caminho, o gesto elevará o ânimo. +5 Esperança.</color>\n\n";
                break;
            case "</color></b> alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n":
                r = "</color></b> chega a ver um punhado de habitantes de Nedukazal resistindo com tochas e lanças improvisadas enquanto outros evacuam um povoado próximo.\n\n";
                break;
            case "Uno de los Héroes alcanza a ver a un puñado de habitantes de Nedukazal resistiendo con antorchas y lanzas improvisadas mientras otros evacuan un caserío cercano.\n\n":
                r = "Um dos Heróis chega a ver um punhado de habitantes de Nedukazal resistindo com tochas e lanças improvisadas enquanto outros evacuam um povoado próximo.\n\n";
                break;
            case "El ejemplo no cambia la guerra, pero sí la forma en que retoma la marcha.\n\n":
                r = "O exemplo não muda a guerra, mas muda a forma como retoma a marcha.\n\n";
                break;
            case "<color=#a0e812><b>Gana 45 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Ganha 45 Experiência e Alta Moral por 3 dias.</b></color>";
                break;
            case "Al atravesar un viejo puesto de paso derruido, la Caravana encuentra un tramo cubierto entre columnas caídas y muros aún firmes, bastante más seguro que el terreno abierto.\n\n":
                r = "Ao atravessar um velho posto de passagem destruído, a Caravana encontra um trecho coberto entre colunas caídas e muros ainda firmes, bem mais seguro que o terreno aberto.\n\n";
                break;
            case "Por un momento, avanzar deja de sentirse como exponerse a cada sombra.\n\n":
                r = "Por um momento, avançar deixa de parecer se expor a cada sombra.\n\n";
                break;
            case "El hollín, la humedad y el viento apagaron varias luces del campamento una y otra vez hasta volver la oscuridad insoportable.\n\n":
                r = "A fuligem, a umidade e o vento apagaram várias luzes do acampamento repetidas vezes, até tornar a escuridão insuportável.\n\n";
                break;
            case "Nadie llega a descansar del todo bien cuando Nedukazal vuelve a tragarse la poca luz disponible.\n\n":
                r = "Ninguém consegue descansar de verdade quando Nedukazal volta a engolir a pouca luz disponível.\n\n";
                break;
            case "La Caravana arma un descanso precario bajo el techo vencido de un galpón saqueado. Cada crujido hace mirar hacia arriba.\n\n":
                r = "A Caravana monta um descanso precário sob o teto cedido de um galpão saqueado. Cada estalo faz todos olharem para cima.\n\n";
                break;
            case "</color></b> puede intentar asegurarlo antes de que ceda.\n\n":
                r = "</color></b> pode tentar reforçá-lo antes que desabe.\n\n";
                break;
            case ").</i> Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = ").</i> Se passar, ganhará 35 Experiência. Se falhar, sofrerá Ferida.</color>\n\n";
                break;
            case "Uno de los Héroes puede intentar asegurarlo antes de que ceda.\n\n":
                r = "Um dos Heróis pode tentar reforçá-lo antes que desabe.\n\n";
                break;
            case "<color=#ba3fef>-Si lo intentas, hará una Tirada de Salvación: TS Reflejos DC 12. Si la supera, ganará 35 Experiencia. Si falla, obtendrá Herida.</color>\n\n":
                r = "<color=#ba3fef>-Se você tentar, ele fará um Teste de Resistência: TR Reflexos CD 12. Se passar, ganhará 35 Experiência. Se falhar, sofrerá Ferida.</color>\n\n";
                break;
            case "<color=#ba3fef>-Si decides mover el campamento, la noche será más larga. +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir mover o acampamento, a noite será mais longa. +1 Fadiga.</color>\n\n";
                break;
            case "Durante el descanso, desde una bodega cerrada cercana llegan golpes irregulares, respiraciones ásperas y algo arrastrándose entre cajones y barriles rotos.\n\n":
                r = "Durante o descanso, de um depósito fechado próximo chegam golpes irregulares, respirações ásperas e algo se arrastando entre caixotes e barris quebrados.\n\n";
                break;
            case "Puedes investigar antes de que eso venga hacia el campamento o atrancar la entrada y pasar la noche en tensión.\n\n":
                r = "Você pode investigar antes que isso venha até o acampamento ou barrar a entrada e passar a noite em tensão.\n\n";
                break;
            case "<color=#ba3fef>-Si decides atrancar la bodega, +1 Fatiga.</color>\n\n":
                r = "<color=#ba3fef>-Se decidir barrar a adega, +1 Fadiga.</color>\n\n";
                break;
            case "</color></b> encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n":
                r = "</color></b> encontra em uma parede uma longa lista de nomes escritos Ã s pressas, riscados Ã  medida que Nedukazal ia caindo.\n\n";
                break;
            case "Uno de los Héroes encuentra en una pared una larga lista de nombres escritos a las apuradas, tachados a medida que Nedukazal iba cayendo.\n\n":
                r = "Um dos Heróis encontra em uma parede uma longa lista de nomes escritos Ã s pressas, riscados Ã  medida que Nedukazal ia caindo.\n\n";
                break;
            case "Después de leerla, el descanso ya no consigue apartarle esa imagen de la cabeza.\n\n":
                r = "Depois de lê-la, o descanso já não consegue tirar essa imagem da cabeça.\n\n";
                break;
            case "Desde un alto todavía firme, parte de la guardia consigue observar caminos, corrales y techos que desde abajo resultaban imposibles de leer.\n\n":
                r = "De um ponto alto ainda firme, parte da guarda consegue observar caminhos, currais e telhados que lá de baixo eram impossíveis de ler.\n\n";
                break;
            case "A la mañana siguiente, la Caravana parte con una idea mucho más clara del terreno inmediato.\n\n":
                r = "Na manhã seguinte, a Caravana parte com uma ideia muito mais clara do terreno imediato.\n\n";
                break;
            case "Entre muros caídos, carretas rotas y lonas viejas, la Caravana consigue armar un fogón protegido del viento y de las miradas del terreno abierto.\n\n":
                r = "Entre muros caídos, carroças quebradas e lonas velhas, a Caravana consegue montar um fogareiro protegido do vento e dos olhares vindos do terreno aberto.\n\n";
                break;
            case "No es cómodo, pero sí lo bastante estable como para dormir mejor de lo esperado.\n\n":
                r = "Não é confortável, mas é estável o bastante para dormir melhor do que o esperado.\n\n";
                break;
            case "Al reparo de un patio de posta, ocultos por lonas y carros volcados, encuentran a varios habitantes de Nedukazal esperando el momento para huir.\n\n":
                r = "Ao abrigo de um pátio de posta, ocultos por lonas e carroças viradas, encontram vários habitantes de Nedukazal esperando o momento de fugir.\n\n";
                break;
            case "Al enterarse de que la Caravana partirá al amanecer, piden sumarse antes de que los Zarkil vuelvan a cruzar la zona.\n\n":
                r = "Ao saber que a Caravana partirá ao amanhecer, pedem para se juntar antes que os Zarkil voltem a cruzar a área.\n\n";
                break;
            case "<color=#a0e812><b>+5-10 Civiles, +3 Esperanza</b></color>":
                r = "<color=#a0e812><b>+5-10 Civis, +3 Esperança</b></color>";
                break;
            case "</color></b> encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n":
                r = "</color></b> encontra uma carta que nunca chegou a sair de Nedukazal, escrita para alguém que esperava notícias fora do reino.\n\n";
                break;
            case "Uno de los Héroes encuentra una carta que nunca llegó a salir de Nedukazal, escrita para alguien que esperaba noticias fuera del reino.\n\n":
                r = "Um dos Heróis encontra uma carta que nunca chegou a sair de Nedukazal, escrita para alguém que esperava notícias fora do reino.\n\n";
                break;
            case "Leerla durante el descanso le devuelve perspectiva sobre por qué todavía vale la pena seguir.\n\n":
                r = "Lê-la durante o descanso lhe devolve perspectiva sobre por que ainda vale a pena seguir em frente.\n\n";
                break;
            case "<color=#a0e812><b>Gana 40 Experiencia y Alta Moral por 3 días.</b></color>":
                r = "<color=#a0e812><b>Ganha 40 Experiência e Alta Moral por 3 dias.</b></color>";
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
            case "Has llegado al lugar señalado por el ave mensajera y te has encontrado con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la caravana y comienzan a descargar los recursos que han traído para ayudarles en su travesía.\n\nInmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.\n":
                r = "Você chegou ao local indicado pela ave mensageira e encontrou a equipe de salvamento enviada pela Cidade Portuária de Serria.\nImediatamente eles saúdam a caravana e começam a descarregar os recursos que trouxeram para ajudar em sua travessia.\n\nO ânimo na caravana melhora na mesma hora ao ver que eles nío estío sozinhos nessa luta.\n";
                break;
            case "<color=#a0e812><b>\n\nSe han entregado ":
                r = "<color=#a0e812><b>\n\nForam entregues ";
                break;
            case " suministros. +25 Esperanza. +20 Materiales y 200 Oro y un nuevo personaje se suma a la caravana</b></color>":
                r = " suprimentos. +25 Esperança. +20 Materiais e 200 Ouro, e um novo personagem se junta á caravana</b></color>";
                break;
            case "-Las oraciones de los Purificadores del Templo de Serria merman el avance del Aliento Negro en: ":
                r = "-As orações dos Purificadores do Templo de Serria reduzem o avanço do Respiro Negro em: ";
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
            case "Impacto crítico":
                r = "Impacto crítico";
                break;
            case " usa ":
                r = " usa ";
                break;
            case "Bonus daño elemental Acido.":
                r = "BÃ´nus de dano elemental Ácido.";
                break;
            case "Bonus daño elemental Arcano.":
                r = "BÃ´nus de dano elemental Arcano.";
                break;
            case "Bonus daño elemental Fuego.":
                r = "BÃ´nus de dano elemental Fogo.";
                break;
            case "Bonus daño elemental Hielo.":
                r = "BÃ´nus de dano elemental Gelo.";
                break;
            case "Bonus daño elemental Necro.":
                r = "BÃ´nus de dano elemental Necrótico.";
                break;
            case "Bonus daño elemental Rayo.":
                r = "BÃ´nus de dano elemental Raio.";
                break;
            case "Bonus daño elemental Divino.":
                r = "BÃ´nus de dano elemental Divino.";
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
                r = "Armadura de couro que melhora a resistência física.";
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
            case "Acumular Energía":
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
            case "Hoja de Energía":
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
            case "-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crítico. -20 Esperanza":
                r = "-O Séquito de Clérigos pereceu, pois o Respiro Negro atingiu um ní­vel crítico. -20 Esperança";
                break;
            case " ahora Maneja un Nivel ":
                r = " agora possui um Ní­vel ";
                break;
            case " de Energía.":
                r = " de Energia.";
                break;
            case " de Valentía.":
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
            /*  case "Este séquito está constituido por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comercial a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
                 r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mínimo.";
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
                r = "Ã‚ncora da Última Linha";
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
            case "Día":
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
                r = "Relí­quia do Segundo FÃ´lego";
                break;
            case "Resina del Armero":
                r = "Resina do Armeiro";
                break;
            case "Sello de Ceniza Negra":
                r = "Selo de Cinza Negra";
                break;
            case "Símbolo de Proteccion Arcano":
                r = "Símbolo Arcano de Proteçío";
                break;
            case "Solucion Neutralizante":
                r = "Soluçío Neutralizante";
                break;
            case "Tinta de Condena":
                r = "Tinta de Condenaçío";
                break;
            case "Tonico Vital del Campamento":
                r = "TÃ´nico Vital do Acampamento";
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
                r = "Segundo FÃ´lego";
                break;
            case "Tormenta Cargada":
                r = "Tempestade Carregada";
                break;
            case "Filoacero":
                r = "Fio de Aço";
                break;
            case "La caravana ha sido destruida y todos sus miembros han muerto. El Aliento Negro es implacable.":
                r = "A caravana foi destruí­da e todos os seus membros morreram. O Respiro Negro é implacável.";
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
            /*case "Este séquito está constituído por varios mercaderes que han tenido que abandonar sus tiendas, pero que no han renunciado a su mercadería. Están dispuestos a comerciar a precios rebajados pero sin renunciar al menos a una mínima ganancia.":
                r = "Este séquito é composto por vários mercadores que tiveram de abandonar suas lojas, mas nío renunciaram ás suas mercadorias. Estío dispostos a negociar com preços reduzidos, mas sem abrir mío de pelo menos um lucro mínimo.";
                break;*/
            case "El Espectro acaba de atacar, haciéndolo vulnerable en el plano material.":
                r = "O Espectro acabou de atacar, tornando-se vulnerável no plano material.";
                break;
            case "Echar.":
                r = "Lançar";
                break;
            case "Echar":
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
            case "Puedes fijar 1 item con el botón derecho del mouse para que no se pierda al actualziar el inventario.":
                r = "Você pode fixar 1 item com o botão direito do mouse para que não se perca ao atualizar o inventário.";
                break;
            case "La ruta se abre en varias direcciones parecidas y las pocas señales útiles parecen haberse cruzado unas con otras. Dos miembros de la caravana parecen tener opiniones encontradas. ¿A quién escucharás?\n\n":
                r = "A rota se abre em várias direções semelhantes e as poucas sinalizações úteis parecem ter se cruzado umas com outras. Dois membros da caravana parecem ter opiniões encontradas. A quem você irá ouvir?\n\n";
                break;
            case "Impulso":
                r = "Impulso";
                break;
            case "impulso":
                r = "impulso";
                break;
            case "Impulso: el próximo movimiento a casilla o intercambio cuesta 1 PA menos y consume 1 stack.":
                r = "Impulso: o próximo movimento para célula ou troca custa 1 PA a menos e consome 1 acúmulo.";
                break;
            case "No hay enemigos en la fila.":
                r = "Não há inimigos na fila.";
                break;
            case "No hay casilla frontal disponible":
                r = "Nenhuma célula frontal disponível";
                break;
            case "No hay trayecto válido":
                r = "Não há trajeto válido";
                break;
            case "El trayecto esta bloqueado":
                r = "O trajeto está bloqueado";
                break;
            case "<color=#0cca74><b>Siempre Alerta: </b></color><color=#d3d3d3><i>La Duelista se mantiene lista para actuar con rapidez si se presenta una batalla.</color></i>\\n\\n+5 Iniciativa en combate. Si no es emboscada, gana 2 Impulso al comenzar la batalla.":
                r = "<color=#0cca74><b>Sempre Alerta: </b></color><color=#d3d3d3><i>A Duelista se mantém pronta para agir rapidamente se uma batalha ocorrer.</color></i>\\n\\n+5 Iniciativa em combate. Se não for emboscada, ganha 2 Impulso ao começar a batalha.";
                break;
            case "<color=#0cca74><b>Socializar: </b></color><color=#d3d3d3><i>La Duelista dedica tiempo a conversar, bromear y sostener el ánimo de la caravana.</color></i>\\n\\nCada día, sus compañeros realizan una TS Mental DC 13. Quienes la superan obtienen Alta Moral por 1 día.":
                r = "<color=#0cca74><b>Socializar: </b></color><color=#d3d3d3><i>A Duelista dedica tempo a conversar, brincar e sustentar o ânimo da caravana.</color></i>\\n\\nA cada dia, seus companheiros fazem um Teste Mental CD 13. Quem passar ganha Alta Moral por 1 dia.";
                break;
            case "<color=#0cca74><b>Consuelo: </b></color><color=#d3d3d3><i>La Duelista contiene el desánimo de la caravana cuando llegan malas noticias o tiempos difíciles.</color></i>\\n\\nSiempre que se pierda Esperanza por cualquier motivo, se pierde 2 menos.":
                r = "<color=#0cca74><b>Consolo: </b></color><color=#d3d3d3><i>A Duelista contém o desânimo da caravana quando chegam más notícias ou tempos difíceis.</color></i>\\n\\nSempre que se perder Esperança por qualquer motivo, perdem-se 2 a menos.";
                break;
            case "Este personaje no puede realizar actividades ahora. Descansa.":
                r = "Este personagem não realiza Atividades. Descansa.";
                break;
            case "-Se ha cambiado la actividad de todos los personajes.":
                r = "A atividade de todos os personagens foi alterada.";
                break;
            case "Duelista":
                r = "Duelista";
                break;
            case "-La Caravana se mueve con Aletargamiento. +1 Avance del Aliento Negro.":
                r = "-A Caravana avança com Lentidão. +1 Avanço do Respiro Negro.";
                break;
            case "Siempre Alerta":
                r = "Sempre Alerta";
                break;
            case "Impulsivo":
                r = "Impulsivo";
                break;
            case "Cansado":
                r = "Cansado";
                break;
            case " es ejecutado por la Condena.":
                r = " ele foi executado pela sentença.";
                break;
            case "Guardar Partida":
                r = "Salvar jogo";
                break;
            case "Al realizar la ofrenda, el Aliento Negro retrocederá en 3 y un personaje con Corrupción al azar será curado.":
                r = "Ao fazer a oferenda, o Respiro Negro recuará em 3 e um personagem aleatório com Corrupção será curado.";
                break;
            case "<i>El Árbol de los Lamentos extiende sus raices de forma amenazante por sobre la superficie para atacar a sus enemigos y protegerse. </i>\n\n<color=#EE0000>-Débil al fuego.</color>":
                r = "<i>A Árvore dos Lamentos estende suas raízes de forma ameaçadora sobre a superfície para atacar seus inimigos e se proteger. </i>\n\n<color=#EE0000>-Fraco contra fogo.</color>";
                break;
            case "Raíz Maldita":
                r = "Raiz Maldita";
                break;
            case "<i>Este gigante árbol maldito bloquea la salida del Bosque Ardiente, poseído por los espíritus caídos en el bosque, buscará impedir el escape de los intrusos.</i>\n\n<color=#199F10>Crea Enredaderas.\n-Ataques de rango que atraen.\n-Regenera armadura.</color><color=#EE0000>-Débil al fuego.</color>":
                r = "<i>Esta gigantesca árvore amaldiçoada bloqueia a saída da Floresta Ardente, possuída pelos espíritos caídos no bosque, e tentará impedir a fuga dos intrusos.</i>\n\n<color=#199F10>Cria Vinhas.\n-Ataques à distância que puxam o alvo.\n-Regenera armadura.</color><color=#EE0000>-Fraca contra fogo.</color>";
                break;
            case "Árbol de los Lamentos":
                r = "Árvore dos Lamentos";
                break;
            case "Invocar Raíz Maldita": r = "Summon Cursed Root"; break;
            case "Raiz Maldita": r = "Cursed Root"; break;
            case "Condena Feroz": r = "Fierce Condemnation"; break;
            case "+15% Danio, +2 Ataque, +5 TS Mental.": r = "+15% Damage, +2 Attack, +5 Mental Save."; break;
            case "Estertor Maldito": r = "Cursed Death Rattle"; break;
            case "Reacción: al morir, quien asesta el golpe final debe superar TS Mental 12 o recibe 2d6 daño necrótico.": r = "Reaction: on death, whoever deals the killing blow must pass Mental Save 12 or take 2d6 necrotic damage."; break;
            case "resiste el estertor maldito.": r = "resists the cursed death rattle."; break;
            case "desata una llamarada necrotica y ardiente sobre": r = "unleashes a burning necrotic flare upon"; break;
            case "Bruja Quemada": r = "Burnt Hag"; break;
             case "<i>Esta bruja ha sido deformada por las llamas y corrompida la presencia del Aliento Negro. </i>\n\n<color=#199F10>Crea Enredaderas.\n-Ataque de rango que no falla.\n-Estertor Mortal.</color><color=#EE0000>-Poco resistente.</color>":
            r = "<i>Esta bruxa foi deformada pelas chamas e corrompida pela presença do Respiro Negro.</i>\n\n<color=#199F10>Cria Vinhas.\n-Ataque à distância que nunca erra.\n-Último suspiro.</color><color=#EE0000>-Resistência fraca.</color>";
            break;
             case "), eligió bien la ruta y el Aliento Negro retrocedió 1. +25 Experiencia.":
             r ="), escolheu o caminho certo e o Respiro Negro recuou 1. +25 de experiência.";
              break;
            case "Escape: los personajes podrán escapar desde esta casilla.":
            r="Escape: os personagens podem escapar deste quadrado.";
            break;
             case "Paciente":
            r="Paciente";
            break;
            case "Defensa vencida":
            r="Defesa derrotada";
            break;
            case "Fortitud":
            r="Fortitude";
            break;
            case "Al Esforzarse se toma prestado AP del turno siguiente y la Defensa bajará por 1T.":
            r="Ao se esforçar, você empresta PA do próximo turno e sua Defesa diminuirá em 1T.";
            break;
            case "¡Esforzando!":
            r="Esforçando-se!";
            break;
            case "Viajando...":
            r="Viajando...";
            break;
             case "Resolver Combate":
            r="Resolver Combate";
            break;
            case "Caravana":
            r="Caravana";
            break;
            case "Séquitos":
            r="Séquitos";
             break;
            case "Bitácora":
            r="Binácula";
            break;
            case "-La actividad de todos los personajes ahora es: ":
            r="-A atividade de todos os personagens agora é: ";
             break;
            case "Guardia":
            r="Guarda";
             break;
            case "Descanso":
            r="Repouso";
             break;
            case "-Actividad fijada.":
            r="-Atividade fixa.";
             break;
            case "Atributos":
            r="Atributos";
             break;
            case "Equipo Disponible":
            r="Equipamento Disponível";
             break;
            case " disponibles para ":
            r=" disponível para ";
             break;
            case "Armas":
            r="Braços";
             break;
            case "Armaduras":
            r="Armadura";
             break;
            case "Echar a ":
            r="Echar a ";
             break;
            case " hará que se pierdan ":
            r=" fará com que eles se percam ";
             break;
            case " Esperanza. ¿Continuar?":
            r=" Esperança. Continuar?";
             break;
            case "Estadísticas":
            r="Estadística";
             break;
            case"Días viajados con la caravana...":
            r="Dias viajando com a caravana...";
            break;
            case"Enemigos eliminados...":
            r="Inimigos eliminados...";
            break;
            case"Daño infligido...":
            r="Danos causados...";
            break;
            case"Daño recibido...":
            r="Danos recebidos...";
            break;
            case"Veces derrotado...":
            r="Tempos de derrota...";
            break;
            case"Exploración":
            r="Exploração";
            break;
            case"Viaje":
            r="Viaje";
            break;
            case"Reservas":
            r="Reservas";
            break;
            case"Selecciona el orden de los refuerzos: (-->)":
            r="Selecione a ordem dos reforços: (-->)";
            break;
            case"Si la defensa es derrotada, la caravana será destruída.":
            r="Se a defesa for derrotada, a caravana será destruída.";
             break;
            case "Silenciar tips":
                r = "Silenciar dicas";
                break;
            case"Mostrar Ayudas":
            r="Mostrar ajuda";
            break;
            case"Noticias":
            r="Notícias";
            break;
            case"Prueba jugable":
            r="Teste de jogovel";
             break;
            case"Gracias por jugar la demo.\nEn esta versión podrás experimentar el Tutorial y luego la primer zona del juego completa.\n\nTu feedback es muy importante para seguir mejorando.":
            r="Obrigado por jogar a demo.\nNesta versão, você poderá experimentar o Tutorial e depois a primeira zona completa do jogo.\n\nO seu feedback é muito importante para continuarmos melhorando.";
            break;
            case "Arquero Vengador de Kadryn":
            r = "Arqueiro Vingador de Kadryn";
            break;
            case"Claridad":
            r="Clareza";
            break;
            case"Puedes dar feedback y unirte a nuestro Discord aquí.":
            r="Você pode dar feedback e se juntar à comunidade do Discord aqui.";
            break;
            case "Créditos":
            r = "Créditos";
            break;
            case "Wishlist en Steam":
            r = "Wishlist no Steam";
            break;
            case "Prohibido: Individualista":
            r = "Prohibido: Individualista";
            break;
            case "Salir al Menú":
            r = "Sair para o menu";
            break;
         
            
            
           
           






               
             
      }

        return r;
    }



    public void TraducirTodosTextosSegunIdioma()
{

    var textos = Object.FindObjectsOfType<TMPro.TextMeshProUGUI>(includeInactive: true);

    foreach (var txt in textos)
    {
        if (txt == null)
        {
            continue;
        }

        if (!textosOriginalesTMP.TryGetValue(txt, out string original))
        {
            original = txt.text;
            textosOriginalesTMP[txt] = original;
        }

        string traducido = original;

        if (nIdioma == IdiomaIngles)
        {
            traducido = TraducirConCompatibilidadMojibake(original.Normalize(NormalizationForm.FormC), TraducirIngles);
        }
        else if (nIdioma == IdiomaPortugues)
        {
            traducido = TraducirConCompatibilidadMojibake(original.Normalize(NormalizationForm.FormC), TraducirPortugues);
        }

        if (txt.text != traducido)
        {
            txt.text = traducido;
        }
    }
}




}
