using System;
using System.Collections.Generic;
using System.IO;

public enum BanterCampaniaDisparador
{
    IdleNodo,
    PocosSuministros,
    Sobrecarga,
    Cansancio,
    ViajeEsperanzaAlta,
    ViajeActividad,
    ViajeEsperanzaBaja,
    EmboscadaRevelada,
    AtajoSubterraneoRevelado,
    SantuarioPurificadoraRevelado,
    AsentamientoRevelado,
    LlegadaAsentamiento,
    Descanso,
    CuradoCompleto,
    AlientoNegroDistanciaCero
}

public sealed class BanterLineaCampaniaLocal
{
    public string Id { get; }
    public BanterCampaniaDisparador Disparador { get; }
    public int IdActividad { get; }
    public string Espanol { get; }
    public string Ingles { get; }
    public string Portugues { get; }
    public string Chino { get; }

    public BanterLineaCampaniaLocal(
        string id,
        BanterCampaniaDisparador disparador,
        int idActividad,
        string espanol,
        string ingles,
        string portugues,
        string chino = null)
    {
        Id = id;
        Disparador = disparador;
        IdActividad = idActividad;
        Espanol = espanol;
        Ingles = ingles;
        Portugues = portugues;
        Chino = chino;
    }

    public string ObtenerTextoActual()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
        if (idioma == TRADU.IdiomaChinoSimplificado)
        {
            if (!string.IsNullOrWhiteSpace(Chino))
            {
                return Chino;
            }
            return !string.IsNullOrWhiteSpace(Ingles) ? Ingles : Espanol;
        }
        if (idioma == TRADU.IdiomaIngles && !string.IsNullOrWhiteSpace(Ingles))
        {
            return Ingles;
        }
        if (idioma == TRADU.IdiomaPortugues && !string.IsNullOrWhiteSpace(Portugues))
        {
            return Portugues;
        }
        return Espanol;
    }
}

public static class BanterContenidoCampania
{
    private static readonly Dictionary<int, List<BanterLineaCampaniaLocal>> LineasPorClase =
        new Dictionary<int, List<BanterLineaCampaniaLocal>>();
    private static readonly Dictionary<int, List<BanterLineaCampaniaLocal>> LineasPorActividad =
        new Dictionary<int, List<BanterLineaCampaniaLocal>>();

    static BanterContenidoCampania()
    {
        RegistrarClase(0, ContenidoGenerico);
        RegistrarClase(1, ContenidoCaballero);
        RegistrarClase(2, ContenidoExplorador);
        RegistrarClase(3, ContenidoPurificadora);
        RegistrarClase(4, ContenidoAcechador);
        RegistrarClase(5, ContenidoCanalizador);
        RegistrarClase(6, ContenidoDuelista);
        RegistrarActividades(ContenidoActividades);
    }

    public static IReadOnlyList<BanterLineaCampaniaLocal> ObtenerLineas(
        int idClase,
        BanterCampaniaDisparador disparador)
    {
        if (!LineasPorClase.TryGetValue(idClase, out List<BanterLineaCampaniaLocal> lineas))
        {
            return Array.Empty<BanterLineaCampaniaLocal>();
        }

        return lineas.FindAll(linea => linea != null && linea.Disparador == disparador);
    }

    public static IReadOnlyList<BanterLineaCampaniaLocal> ObtenerLineasActividad(int idActividad)
    {
        return LineasPorActividad.TryGetValue(idActividad, out List<BanterLineaCampaniaLocal> lineas)
            ? lineas
            : Array.Empty<BanterLineaCampaniaLocal>();
    }

    private static void RegistrarClase(int idClase, string contenido)
    {
        List<BanterLineaCampaniaLocal> lineas = new List<BanterLineaCampaniaLocal>();
        using StringReader lector = new StringReader(contenido);
        string fila;
        while ((fila = lector.ReadLine()) != null)
        {
            fila = fila.Trim();
            if (string.IsNullOrWhiteSpace(fila))
            {
                continue;
            }

            string[] campos = fila.Split('|');
            if ((campos.Length != 5 && campos.Length != 6)
                || !Enum.TryParse(campos[1], out BanterCampaniaDisparador disparador))
            {
                throw new InvalidOperationException("Contenido de banter de campaña inválido: " + fila);
            }

            lineas.Add(new BanterLineaCampaniaLocal(
                campos[0],
                disparador,
                0,
                campos[2],
                campos[3],
                campos[4],
                campos.Length == 6 ? campos[5] : null));
        }

        LineasPorClase[idClase] = lineas;
    }

    private static void RegistrarActividades(string contenido)
    {
        using StringReader lector = new StringReader(contenido);
        string fila;
        while ((fila = lector.ReadLine()) != null)
        {
            fila = fila.Trim();
            if (string.IsNullOrWhiteSpace(fila))
            {
                continue;
            }

            string[] campos = fila.Split('|');
            if ((campos.Length != 5 && campos.Length != 6) || !int.TryParse(campos[1], out int idActividad))
            {
                throw new InvalidOperationException("Contenido de actividad de banter inválido: " + fila);
            }

            if (!LineasPorActividad.TryGetValue(idActividad, out List<BanterLineaCampaniaLocal> lineas))
            {
                lineas = new List<BanterLineaCampaniaLocal>();
                LineasPorActividad.Add(idActividad, lineas);
            }

            lineas.Add(new BanterLineaCampaniaLocal(
                campos[0],
                BanterCampaniaDisparador.ViajeActividad,
                idActividad,
                campos[2],
                campos[3],
                campos[4],
                campos.Length == 6 ? campos[5] : null));
        }
    }

    private const string ContenidoGenerico = @"
cam_gen_ata_01|AtajoSubterraneoRevelado|Un atajo subterráneo. No sabremos qué hay al otro lado hasta cruzarlo.|An underground passage. We will not know what lies beyond until we cross it.|Uma passagem subterrânea. Só saberemos o que há do outro lado quando atravessarmos.|一条地下近道。不穿过去，就不知道那头是什么。
cam_gen_ata_02|AtajoSubterraneoRevelado|Este túnel podría acortar el viaje... o llevarnos directo al peligro.|This tunnel may shorten the journey... or lead us straight into danger.|Este túnel pode encurtar a viagem... ou nos levar direto ao perigo.|这条隧道也许能少走弯路……也许直接把我们送进险境。
";

    private const string ContenidoCaballero = @"
cam_cab_idle_01|IdleNodo|Ya hemos descansado suficiente. Movámonos.|We have rested long enough. Let us move.|Já descansamos o suficiente. Vamos seguir.|我们已经休息够了，动身吧。
cam_cab_idle_02|IdleNodo|Un camino está para recorrerlo, no para mirarlo.|A road is meant to be traveled, not stared at.|Uma estrada existe para ser percorrida, não contemplada.|道路是用来走的，不是用来看的。
cam_cab_sum_01|PocosSuministros|Nuestras reservas escasean. Raciónenlas con disciplina.|Our stores are thin. Ration them with discipline.|Nossas reservas estão baixas. Racionem com disciplina.|补给所剩无几，务必按纪律分配。
cam_cab_sum_02|PocosSuministros|No podemos marchar solo con valor.|We cannot march on courage alone.|Não podemos marchar apenas com coragem.|光靠勇气，行军不了多远。
cam_cab_sob_01|Sobrecarga|La caravana carga demasiado.|The caravan bears too much.|A caravana carrega peso demais.|车队负重过多了。
cam_cab_sob_02|Sobrecarga|No hay honor en quebrar a los bueyes bajo nuestra carga.|No honor in breaking the oxen beneath our burden.|Não há honra em quebrar os bois sob nossa carga.|让牛群被我们的重担压垮，绝非荣耀之事。
cam_cab_can_01|Cansancio|Estamos exhaustos, no derrotados. Mantengan la formación.|We are exhausted, not defeated. Keep formation.|Estamos exaustos, não derrotados. Mantenham a formação.|我们只是疲惫，不是溃败。保持队形。
cam_cab_can_02|Cansancio|Mis miembros pesan, pero mi juramento no.|My limbs are heavy, but my oath is not.|Meus membros pesam, mas meu juramento não.|四肢沉重，但誓言不会。
cam_cab_alt_01|ViajeEsperanzaAlta|Serria está más cerca con cada paso.|Serria draws closer with every step.|Serria se aproxima a cada passo.|每走一步，塞里亚都更近了。
cam_cab_alt_02|ViajeEsperanzaAlta|La esperanza nos ha hecho fuertes.|Hope has made us strong.|A esperança nos tornou fortes.|希望让我们变得坚强。
cam_cab_baj_01|ViajeEsperanzaBaja|No se rindan ante la desesperación.|Do not surrender to despair.|Não se rendam ao desespero.|不要向绝望屈服。
cam_cab_baj_02|ViajeEsperanzaBaja|Alcen la cabeza. El camino es largo.|Raise your heads. The road is long.|Ergam a cabeça. A estrada é longa.|抬起头来，前路还长。
cam_cab_emb_01|EmboscadaRevelada|Hay peligros mayores adelante...|There are greater dangers ahead...|Há perigos maiores adiante...|前方危机四伏……
cam_cab_emb_02|EmboscadaRevelada|Preparen el acero. Ese camino es una trampa.|Ready your steel. That road is trapped.|Preparem o aço. Aquela estrada é uma armadilha.|亮出兵刃，那条路有埋伏。
cam_cab_ase_01|AsentamientoRevelado|Hay un asentamiento cerca... por este camino.|A settlement is near... down this road.|Há um assentamento perto... por esta estrada.|附近有个定居点……就在这条路上。
cam_cab_ase_02|AsentamientoRevelado|Una aldea adelante. Corramos la voz.|A village ahead. Let us spread the news.|Uma aldeia adiante. Vamos espalhar a notícia.|前方有个村庄，把消息传下去。
cam_cab_lle_01|LlegadaAsentamiento|Muros y un techo. Aseguren el perímetro antes de bajar la guardia.|Walls and a roof. Secure the perimeter before lowering your guard.|Muros e um teto. Protejam o perímetro antes de baixar a guarda.|城墙与屋顶，放松警惕前先确保周边安全。
cam_cab_des_01|Descanso|Descansen bien. Mañana marchamos.|Rest well. Tomorrow we march.|Descansem bem. Amanhã marchamos.|好好休息，明日启程。
cam_cab_des_02|Descanso|Hoy no marchamos. ¡Recupérense!|Today we do not march. Recover!|Hoje não marchamos. Recuperem-se!|今天不必赶路，养精蓄锐！
cam_cab_cur_01|CuradoCompleto|Entero otra vez.|Whole again.|Inteiro outra vez.|又完整如初了。
cam_cab_cur_02|CuradoCompleto|Mi fuerza ha regresado.|My strength has returned.|Minha força voltou.|力量已经回来了。
cam_cab_ali_01|AlientoNegroDistanciaCero|El Aliento Negro nos alcanzó. Si flaqueamos ahora, no quedará nadie a quien proteger.|The Black Breath has caught us. If we falter now, there will be no one left to protect.|O Hálito Negro nos alcançou. Se fraquejarmos agora, não restará ninguém para proteger.|黑息追上我们了。此刻若退缩，便再无人可护。
";

    private const string ContenidoExplorador = @"
cam_exp_idle_01|IdleNodo|Quedarnos quietos nos vuelve un blanco fácil.|Standing still makes us an easy target.|Ficar parados nos torna um alvo fácil.|站着不动，只会让我们变成活靶子。
cam_exp_idle_02|IdleNodo|Si nos quedamos, al menos montemos guardia.|If we are staying, at least set a watch.|Se vamos ficar, pelo menos montemos guarda.|既然要留下，至少派人放哨。
cam_exp_sum_01|PocosSuministros|Quedan pocos suministros.|Supplies are low.|Os suprimentos estão baixos.|补给不多了。
cam_exp_sum_02|PocosSuministros|Las reservas de comida están casi vacías.|The food stores are nearly empty.|As reservas de comida estão quase vazias.|粮食储备几乎见底。
cam_exp_sob_01|Sobrecarga|Estamos sobrecargados. Cada cuesta nos castigará.|We are overloaded. Every hill will punish us for it.|Estamos sobrecarregados. Cada subida vai nos castigar.|我们超载了，每道坡都会要我们的命。
cam_exp_sob_02|Sobrecarga|Demasiado peso. Dejen algo.|Too much weight. Drop something.|Peso demais. Larguem alguma coisa.|东西太重了，扔掉点吧。
cam_exp_can_01|Cansancio|Ya no estamos cansados. Necesitamos descansar antes de que alguien caiga.|We are past tired. We need rest before someone drops.|Já passamos do cansaço. Precisamos descansar antes que alguém caia.|早就过了疲惫的地步，得在有人倒下前休息。
cam_exp_can_02|Cansancio|Cada paso suena más fuerte... y más lento.|Every step is getting louder... and slower.|Cada passo soa mais alto... e mais lento.|每一步都更响……也更慢了。
cam_exp_alt_01|ViajeEsperanzaAlta|Todos sonríen. El camino se aligera.|Everyone is smiling. The road feels lighter.|Todos estão sorrindo. A estrada parece mais leve.|大家都在笑，路也感觉轻松了。
cam_exp_alt_02|ViajeEsperanzaAlta|El camino ya no se ve tan mal.|The road ahead does not look so bad.|A estrada à frente não parece tão ruim.|前面的路看起来没那么糟了。
cam_exp_baj_01|ViajeEsperanzaBaja|Están perdiendo la esperanza. Necesitamos buenas noticias.|They are losing hope. We need good news.|Estão perdendo a esperança. Precisamos de boas notícias.|他们在失去希望，我们需要好消息。
cam_exp_baj_02|ViajeEsperanzaBaja|Demasiados ojos miran al suelo.|Too many eyes are fixed on the ground.|Olhos demais estão presos ao chão.|太多人低着头走路了。
cam_exp_emb_01|EmboscadaRevelada|Huellas, cobertura, silencio... hay enemigos adelante.|Tracks, cover, silence... enemies ahead.|Rastros, cobertura, silêncio... inimigos adiante.|脚印、掩体、寂静……前面有敌人。
cam_exp_emb_02|EmboscadaRevelada|Mejor no entrar de frente en esa trampa.|Best not walk straight into that trap.|Melhor não entrar de frente nessa armadilha.|最好别一头闯进那个陷阱。
cam_exp_ase_01|AsentamientoRevelado|Hay un asentamiento cerca. Comida y refugio.|A settlement is close. Food and shelter.|Há um assentamento perto. Comida e abrigo.|附近有个定居点，有吃的有地方住。
cam_exp_ase_02|AsentamientoRevelado|Huellas de carretas. Hay un asentamiento cerca.|Wagon tracks. A settlement is near.|Rastros de carroças. Há um assentamento perto.|有车辙，附近有个定居点。
cam_exp_lle_01|LlegadaAsentamiento|Por fin un lugar con comida, camas y gente que conoce estos caminos.|At last, a place with food, beds, and people who know these roads.|Enfim, um lugar com comida, camas e gente que conhece estes caminhos.|终于有地方吃饭睡觉，还有熟悉这些路的人。
cam_exp_des_01|Descanso|Por fin. Hora de descansar.|Finally. Time to rest.|Finalmente. Hora de descansar.|终于到了，该休息了。
cam_exp_des_02|Descanso|Primero revisemos el perímetro. Después descansamos.|Check the perimeter first. Then we rest.|Primeiro, verifiquem o perímetro. Depois descansamos.|先查看一下周边，再休息。
cam_exp_cur_01|CuradoCompleto|Mis heridas cerraron.|My wounds have closed.|Minhas feridas fecharam.|伤口愈合了。
cam_exp_cur_02|CuradoCompleto|Totalmente remendado. Que siga así.|Fully patched up. Let us keep it that way.|Totalmente remendado. Que continue assim.|彻底养好了，但愿能保持。
cam_exp_ali_01|AlientoNegroDistanciaCero|Ya está sobre nosotros. El aire huele a podredumbre... y no veo una ruta limpia.|It is upon us. The air reeks of rot... and I see no clear route.|Já está sobre nós. O ar fede a podridão... e não vejo uma rota segura.|已经追上我们了，空气里全是腐臭……我看不到干净的路。
";

    private const string ContenidoPurificadora = @"
cam_pur_idle_01|IdleNodo|Una pausa breve es sabia. Demorarnos no.|A brief pause is wise. Lingering is not.|Uma breve pausa é sábia. Demorar, não.|短暂停留是明智的，久留却不然。
cam_pur_idle_02|IdleNodo|Que la Luz guíe pronto nuestro próximo paso.|May the Light guide our next step soon.|Que a Luz guie em breve nosso próximo passo.|愿光尽快指引我们的下一步。
cam_pur_sum_01|PocosSuministros|Nuestra gente necesitará más suministros.|Our people will need more supplies.|Nosso povo precisará de mais suprimentos.|我们的人还需要更多补给。
cam_pur_sum_02|PocosSuministros|Compartamos lo que queda. La Luz prueba nuestra compasión.|Share what remains. The Light tests our compassion.|Partilhemos o que resta. A Luz testa nossa compaixão.|把剩下的分了吧，光考验的是我们的怜悯之心。
cam_pur_sob_01|Sobrecarga|Les exigimos demasiado a los bueyes.|We ask too much of the oxen.|Exigimos demais dos bois.|我们对牛群的要求太过分了。
cam_pur_sob_02|Sobrecarga|Cargamos demasiado peso.|We are carrying far too much.|Estamos carregando peso demais.|我们背负的重量太多了。
cam_pur_can_01|Cansancio|Incluso los fieles deben descansar. Estamos agotados.|Even the faithful must rest. We are spent.|Até os fiéis precisam descansar. Estamos esgotados.|即便虔诚之人也需要休息，我们已经精疲力尽。
cam_pur_can_02|Cansancio|Nuestros cuerpos suplican piedad. Deberíamos escucharlos.|Our bodies plead for mercy. We should listen.|Nossos corpos imploram por misericórdia. Devemos ouvi-los.|我们的身体在恳求怜悯，该听一听了。
cam_pur_alt_01|ViajeEsperanzaAlta|La esperanza arde con fuerza entre nosotros.|Hope burns brightly among us.|A esperança arde intensamente entre nós.|希望在我们心中熊熊燃烧。
cam_pur_alt_02|ViajeEsperanzaAlta|La Luz brilla en cada uno de nosotros.|The Light shines in every one of us.|A Luz brilha em cada um de nós.|光照耀着我们每一个人。
cam_pur_baj_01|ViajeEsperanzaBaja|La oscuridad pesa sobre ellos.|The darkness weighs on them.|A escuridão pesa sobre eles.|黑暗正压在他们心头。
cam_pur_baj_02|ViajeEsperanzaBaja|La esperanza mengua lentamente...|Hope is slowly fading...|A esperança se apaga lentamente...|希望正在慢慢消散……
cam_pur_emb_01|EmboscadaRevelada|Siento peligro en la oscuridad que nos espera...|I sense danger in the darkness ahead...|Sinto perigo na escuridão adiante...|我感觉到前方的黑暗中藏着危险……
cam_pur_emb_02|EmboscadaRevelada|Hay enemigos ocultos en ese camino.|Enemies hide along that road.|Inimigos se escondem naquela estrada.|那条路上藏着敌人。
cam_pur_san_01|SantuarioPurificadoraRevelado|¡Un altar! Deberíamos ir allí...|An altar! We should travel there...|Um altar! Deveríamos ir até lá...|一座祭坛！我们该去那里……
cam_pur_san_02|SantuarioPurificadoraRevelado|Uno de nuestros altares. Debemos acercarnos.|One of our altars. We should approach.|Um de nossos altares. Devemos nos aproximar.|那是我们的祭坛之一，该靠近看看。
cam_pur_ase_01|AsentamientoRevelado|¡Hay una pequeña aldea cerca, por aquí!|A small village is close, this way!|Há uma pequena aldeia perto, por aqui!|这边有个小村庄，就在附近！
cam_pur_ase_02|AsentamientoRevelado|Un asentamiento. La Luz lo ha preservado.|A settlement. The Light has preserved it.|Um assentamento. A Luz o preservou.|一处定居点，是光保全了它。
cam_pur_lle_01|LlegadaAsentamiento|Que la Luz ampare este lugar. Ayudemos a quienes aún resisten.|May the Light shelter this place. Let us help those who still endure.|Que a Luz proteja este lugar. Vamos ajudar aqueles que ainda resistem.|愿光庇护此地，帮帮那些仍在坚持的人吧。
cam_pur_des_01|Descanso|Descansen. Que la Luz custodie sus sueños.|Rest now. May the Light guard your dreams.|Descansem. Que a Luz guarde seus sonhos.|休息吧，愿光守护你们的梦。
cam_pur_des_02|Descanso|Dejen sus cargas. Están a salvo.|Lay down your burdens. You are safe.|Deixem seus fardos. Vocês estão seguros.|放下重担，你们很安全。
cam_pur_cur_01|CuradoCompleto|La Luz me ha devuelto la plenitud.|The Light has made me whole again.|A Luz me tornou plena novamente.|光让我恢复了完整。
cam_pur_cur_02|CuradoCompleto|Mis heridas desaparecieron. Doy gracias.|My wounds are gone. I give thanks.|Minhas feridas se foram. Dou graças.|伤口都消失了，我心怀感激。
cam_pur_ali_01|AlientoNegroDistanciaCero|La corrupción ya nos envuelve. Apenas puedo sentir la Luz bajo este hedor.|The corruption surrounds us now. I can barely feel the Light beneath this stench.|A corrupção já nos envolve. Mal consigo sentir a Luz sob este fedor.|腐化已经将我们包围，在这股恶臭之下，我几乎感觉不到光了。
";

    private const string ContenidoAcechador = @"
cam_ace_idle_01|IdleNodo|La vacilación huele a miedo.|Hesitation smells like fear.|Hesitação tem cheiro de medo.|犹豫的气味，就是恐惧的气味。
cam_ace_idle_02|IdleNodo|Quédense mucho más y algo nos encontrará.|Stay here much longer and something will find us.|Fiquem aqui por mais tempo e algo vai nos encontrar.|再多留一会儿，就会有东西找上门来。
cam_ace_sum_01|PocosSuministros|La comida escasea. Estoy guardando algo para mí.|Food is scarce. I am hoarding some for myself.|A comida está escassa. Estou guardando um pouco para mim.|食物不多了，我给自己留了一点。
cam_ace_sum_02|PocosSuministros|El hambre eliminará primero a los descuidados.|Hunger will cull the careless first.|A fome eliminará primeiro os descuidados.|饥饿会先淘汰那些粗心的人。
cam_ace_sob_01|Sobrecarga|El peso muerto sigue siendo peso muerto. Desháganse de él.|Dead weight is still dead. Cut it loose.|Peso morto continua sendo peso morto. Livrem-se dele.|死重终究是死重，扔掉它。
cam_ace_sob_02|Sobrecarga|Esta carga nos hará más lentos.|This burden will slow us.|Essa carga vai nos atrasar.|这负担只会拖慢我们。
cam_ace_can_01|Cansancio|El agotamiento nos convierte a todos en presa.|Exhaustion makes prey of us all.|A exaustão transforma todos nós em presa.|疲惫让我们所有人都成了猎物。
cam_ace_can_02|Cansancio|Las piernas débiles matan. Necesitamos descansar.|Weak legs get people killed. We need rest.|Pernas fracas matam. Precisamos descansar.|腿软的人会送命，我们需要休息。
cam_ace_alt_01|ViajeEsperanzaAlta|La confianza tiene sus usos.|Confidence has its uses.|A confiança tem sua utilidade.|信心，总有它的用处。
cam_ace_alt_02|ViajeEsperanzaAlta|Al fin caminan como supervivientes, no víctimas.|They finally walk like survivors, not victims.|Finalmente caminham como sobreviventes, não vítimas.|他们终于走得像幸存者，而不是猎物了。
cam_ace_baj_01|ViajeEsperanzaBaja|La desesperación se extiende.|Despair is spreading.|O desespero está se espalhando.|绝望在蔓延。
cam_ace_baj_02|ViajeEsperanzaBaja|Ya marchan como fantasmas.|They already march like ghosts.|Já marcham como fantasmas.|他们走路的样子已经像幽灵了。
cam_ace_emb_01|EmboscadaRevelada|Ese camino nos lleva a una trampa...|That road leads us into a trap...|Aquela estrada nos leva a uma armadilha...|那条路会把我们带进陷阱……
cam_ace_emb_02|EmboscadaRevelada|Hay cazadores esperando adelante.|Hunters are waiting ahead.|Há caçadores esperando adiante.|前面有猎手在等着。
cam_ace_ase_01|AsentamientoRevelado|Este camino de piedra lleva a una aldea...|This stone road leads to a village...|Esta estrada de pedra leva a uma aldeia...|这条石路通向一个村庄……
cam_ace_ase_02|AsentamientoRevelado|Muros, gente, secretos.|Walls, people, secrets.|Muros, gente, segredos.|城墙、人群、秘密。
cam_ace_lle_01|LlegadaAsentamiento|Demasiadas puertas cerradas y ojos curiosos. Vigilen sus pertenencias.|Too many closed doors and curious eyes. Watch your belongings.|Portas fechadas e olhos curiosos demais. Vigiem seus pertences.|太多紧闭的门和好奇的眼睛，看好自己的东西。
cam_ace_des_01|Descanso|Duerman con cuidado. La seguridad es temporal.|Sleep lightly. Safety is temporary.|Durmam com cautela. A segurança é temporária.|睡浅一点，安全只是暂时的。
cam_ace_des_02|Descanso|Mantendré un ojo abierto.|I will keep one eye open.|Vou manter um olho aberto.|我会睁一只眼盯着。
cam_ace_cur_01|CuradoCompleto|Mis heridas cerraron. Bien.|My wounds have closed. Good.|Minhas feridas fecharam. Ótimo.|伤口愈合了，很好。
cam_ace_cur_02|CuradoCompleto|Estoy recuperado y listo para pelear.|I am recovered and ready to fight.|Estou recuperado e pronto para lutar.|我已经恢复了，准备好战斗。
cam_ace_ali_01|AlientoNegroDistanciaCero|El Aliento nos alcanzó. Ahora somos presas caminando dentro de su niebla.|The Breath has caught us. Now we are prey walking inside its fog.|O Hálito nos alcançou. Agora somos presas caminhando dentro de sua névoa.|黑息追上我们了，现在我们成了在它迷雾里行走的猎物。
";

    private const string ContenidoCanalizador = @"
cam_can_idle_01|IdleNodo|El camino no se elegirá solo.|The path will not choose itself.|O caminho não vai se escolher sozinho.|路不会自己选好。
cam_can_idle_02|IdleNodo|Nuestra demora se vuelve tediosa.|Our delay is becoming tedious.|Nossa demora está se tornando tediosa.|我们的耽搁越来越乏味了。
cam_can_sum_01|PocosSuministros|El consumo supera nuestras reservas.|Consumption exceeds our reserves.|O consumo excede nossas reservas.|消耗已经超过了我们的储备。
cam_can_sum_02|PocosSuministros|Los números son claros: necesitamos provisiones.|The numbers are clear: we need provisions.|Os números são claros: precisamos de provisões.|数字很清楚：我们需要补给。
cam_can_sob_01|Sobrecarga|Nuestra carga supera la capacidad.|Our load exceeds capacity.|Nossa carga excede a capacidade.|负重已经超过了承载力。
cam_can_sob_02|Sobrecarga|El exceso de masa desperdicia nuestra fuerza.|Excess mass is wasting our strength.|O excesso de massa desperdiça nossa força.|多余的重量在浪费我们的力气。
cam_can_can_01|Cansancio|Nuestras reservas casi se agotaron. Irritante.|Our reserves are nearly depleted. Irritating.|Nossas reservas estão quase esgotadas. Irritante.|储备快耗尽了，真烦人。
cam_can_can_02|Cansancio|La fatiga erosiona nuestro juicio. Descansar es lógico.|Fatigue is eroding our judgment. Rest is logical.|A fadiga corrói nosso julgamento. Descansar é lógico.|疲劳正在侵蚀判断力，休息才合乎逻辑。
cam_can_alt_01|ViajeEsperanzaAlta|Nuestro impulso es favorable.|Our momentum is favorable.|Nosso impulso é favorável.|我们的势头不错。
cam_can_alt_02|ViajeEsperanzaAlta|La determinación de la caravana es fuerte.|The caravan's resolve is strong.|A determinação da caravana é forte.|车队的决心很坚定。
cam_can_baj_01|ViajeEsperanzaBaja|La moral se derrumba... nada bueno.|Morale is collapsing... not good.|A moral está desmoronando... nada bom.|士气在崩溃……不太妙。
cam_can_baj_02|ViajeEsperanzaBaja|El miedo distorsiona el juicio de la caravana.|Fear is distorting the caravan's judgment.|O medo está distorcendo o julgamento da caravana.|恐惧正在扭曲车队的判断。
cam_can_emb_01|EmboscadaRevelada|La energía adelante... algo anda mal.|The energy ahead... something is wrong.|A energia adiante... algo está errado.|前方的能量……有些不对劲。
cam_can_emb_02|EmboscadaRevelada|Hay demasiadas variables ocultas en ese camino.|Too many variables are hidden along that road.|Há variáveis ocultas demais naquela estrada.|那条路上藏着太多未知变量。
cam_can_ase_01|AsentamientoRevelado|Este camino lleva a una zona poblada.|This road leads to a populated area.|Esta estrada leva a uma área povoada.|这条路通向一片有人居住的区域。
cam_can_ase_02|AsentamientoRevelado|Civilización, aunque temporal. Útil.|Civilization, however temporary. Useful.|Civilização, ainda que temporária. Útil.|文明，虽是暂时的，但有用。
cam_can_lle_01|LlegadaAsentamiento|La concentración de vidas aquí altera las corrientes. Será... instructivo.|The concentration of life here disturbs the currents. This will be... instructive.|A concentração de vidas aqui altera as correntes. Será... instrutivo.|这里生命的聚集扰动了气流，会……很有启发。
cam_can_des_01|Descanso|Al fin. Necesito silencio para meditar.|At last. I need silence to meditate.|Enfim. Preciso de silêncio para meditar.|终于，我需要安静来冥想。
cam_can_des_02|Descanso|El cuerpo descansa. La mente no.|The body rests. The mind does not.|O corpo descansa. A mente não.|身体在休息，思维却没有。
cam_can_cur_01|CuradoCompleto|Mi cuerpo está restaurado.|My body is restored.|Meu corpo está restaurado.|我的身体已经恢复。
cam_can_cur_02|CuradoCompleto|Recuperado por completo. El poder regresa.|Fully recovered. Power returns.|Totalmente recuperado. O poder retorna.|完全康复，力量回来了。
cam_can_ali_01|AlientoNegroDistanciaCero|La energía del Aliento está en todas partes. Está contaminándolo todo.|The Breath's energy is everywhere. It is tainting everything.|A energia do Hálito está em toda parte. Está contaminando tudo.|黑息的气息无处不在，它正在污染一切。
";

    private const string ContenidoDuelista = @"
cam_due_idle_01|IdleNodo|¿Esperamos que el camino se mueva primero?|Are we waiting for the road to move first?|Estamos esperando a estrada se mover primeiro?|我们是在等这条路自己先动吗？
cam_due_idle_02|IdleNodo|Ya miramos este camino suficiente.|We have stared at this road long enough.|Já encaramos esta estrada por tempo demais.|这条路我们盯得够久了。
cam_due_sum_01|PocosSuministros|Queda poco. Busquemos comida.|We are running low. Let us find food.|Está acabando. Vamos encontrar comida.|快没了，去找点吃的吧。
cam_due_sum_02|PocosSuministros|Mochilas casi vacías y estómagos vacíos.|Almost empty packs and empty stomachs.|Mochilas quase vazias e estômagos vazios.|包几乎空了，肚子也是。
cam_due_sob_01|Sobrecarga|Estamos equipados para un asedio.|We are packed for a siege.|Estamos equipados para um cerco.|我们这是准备打一场围城战吗。
cam_due_sob_02|Sobrecarga|Cargamos demasiado peso.|We are carrying too much weight.|Estamos carregando peso demais.|东西背得太多了。
cam_due_can_01|Cansancio|Puedo seguir caminando. Con gracia es otro asunto.|I can keep walking. Gracefully is another matter.|Posso continuar andando. Com elegância é outra história.|我还能走，但要走得优雅，那是另一回事了。
cam_due_can_02|Cansancio|Estamos exhaustos, amigos. El orgullo no nos cargará.|We are exhausted, friends. Pride will not carry us.|Estamos exaustos, amigos. O orgulho não vai nos carregar.|我们都累坏了，朋友们，骄傲可撑不住我们。
cam_due_alt_01|ViajeEsperanzaAlta|Mírennos, casi alegres.|Look at us, almost cheerful.|Olhem para nós, quase alegres.|瞧瞧我们，都快高兴起来了。
cam_due_alt_02|ViajeEsperanzaAlta|Conserven ese ánimo, amigos.|Keep that spirit, friends.|Mantenham esse ânimo, amigos.|保持这股劲儿，朋友们。
cam_due_baj_01|ViajeEsperanzaBaja|Quédense conmigo, amigos. Un paso a la vez.|Stay with me, friends. One step at a time.|Fiquem comigo, amigos. Um passo de cada vez.|跟紧我，朋友们，一步一步来。
cam_due_baj_02|ViajeEsperanzaBaja|Sé que el camino es oscuro. Sigan avanzando.|I know the path is dark. Keep moving.|Sei que o caminho é escuro. Continuem andando.|我知道前路黑暗，但继续走。
cam_due_emb_01|EmboscadaRevelada|Tengamos cuidado en el camino que sigue.|Let us be careful on the road ahead.|Vamos ter cuidado na estrada adiante.|接下来这段路，大家小心点。
cam_due_emb_02|EmboscadaRevelada|Parece que alguien preparó una bienvenida.|Looks like someone prepared a welcome.|Parece que alguém preparou uma recepção.|看来有人给我们准备了欢迎仪式。
cam_due_ase_01|AsentamientoRevelado|¡Un asentamiento! Visitemos la taberna.|A settlement! Let us visit the tavern.|Um assentamento! Vamos visitar a taverna.|一个定居点！去酒馆坐坐吧。
cam_due_ase_02|AsentamientoRevelado|Este camino lleva a un fuego cálido...|This road leads to a warm fire...|Esta estrada leva a um fogo acolhedor...|这条路通向一堆温暖的篝火……
cam_due_lle_01|LlegadaAsentamiento|Techos, cerveza y un público nuevo. Este sitio ya me agrada.|Roofs, ale, and a new audience. I like this place already.|Tetos, cerveja e um público novo. Já gostei deste lugar.|屋顶、麦酒，还有新观众，我已经喜欢这地方了。
cam_due_des_01|Descanso|Duerman un poco, amigos. Se lo ganaron.|Get some sleep, friends. You earned it.|Durmam um pouco, amigos. Vocês mereceram.|睡一觉吧，朋友们，你们应得的。
cam_due_des_02|Descanso|Descansen tranquilos. Nosotros vigilaremos.|Rest easy. We will watch.|Descansem tranquilos. Nós vamos vigiar.|安心休息，我们来放哨。
cam_due_cur_01|CuradoCompleto|¡Me siento mejor!|I am feeling better!|Estou me sentindo melhor!|感觉好多了！
cam_due_cur_02|CuradoCompleto|Otra vez en forma. Ya no se preocupen.|Back in form. Do not worry anymore.|De volta à forma. Não se preocupem mais.|又恢复状态了，别再担心了。
cam_due_ali_01|AlientoNegroDistanciaCero|El Aliento nos pisa los talones... Esta vez, una hoja no bastará.|The Breath is at our heels... This time, a blade will not be enough.|O Hálito está em nossos calcanhares... Desta vez, uma lâmina não será suficiente.|黑息已经贴到我们脚跟了……这次光靠一把剑可不够。
";

    private const string ContenidoActividades = @"
cam_act_01_01|1|Despiértenme cuando el camino empeore.|Wake me when the road gets worse.|Acordem-me quando a estrada piorar.|路况变差时叫醒我。
cam_act_01_02|1|Guardo fuerzas para lo que viene.|I am saving my strength for what comes next.|Estou poupando forças para o que vem a seguir.|我在为接下来的事儿养精蓄锐。
cam_act_02_01|2|Una caravana en marcha no es excusa para no entrenar.|A moving caravan is no excuse to skip training.|Uma caravana em marcha não é desculpa para deixar de treinar.|车队在赶路，也不是不能训练的理由。
cam_act_02_02|2|Un ejercicio más antes de acampar.|One more drill before we make camp.|Mais um exercício antes de acamparmos.|扎营前再练一组。
cam_act_03_01|3|Vigilo el camino. Sigan avanzando.|I am watching the road. Keep moving.|Estou vigiando a estrada. Continuem andando.|我在盯着这条路，你们继续走。
cam_act_03_02|3|La caravana está protegida. Nada se acercará sin que lo note.|The caravan is protected. Nothing gets close unnoticed.|A caravana está protegida. Nada se aproxima sem que eu perceba.|车队很安全，没有什么能靠近而不被我发现。
cam_act_04_01|4|Esta gente debería saber cómo suena el valor.|These people should know what courage sounds like.|Estas pessoas deveriam saber como soa a coragem.|这些人该听听勇气是什么声音。
cam_act_04_02|4|Otra milla, otro relato de batalla.|Another mile, another tale of battle.|Mais uma milha, mais uma história de batalha.|再走一里，再讲一个战场故事。
cam_act_05_01|5|Una correa floja mata más rápido que un enemigo.|A loose strap kills faster than an enemy.|Uma correia frouxa mata mais rápido que um inimigo.|一根松掉的皮带，比敌人杀得还快。
cam_act_05_02|5|No se muevan. Esta placa necesita trabajo.|Hold steady. This plate needs work.|Fique firme. Esta placa precisa de ajuste.|别动，这块护甲还得修一修。
cam_act_06_01|6|Vigilaré el camino adelante.|I will watch the road ahead.|Vou vigiar a estrada adiante.|我会盯着前面的路。
cam_act_06_02|6|Que venga el peligro. Estoy listo.|Let danger come. I am ready.|Que o perigo venha. Estou pronto.|危险尽管来，我已经准备好了。
cam_act_07_01|7|Vi huellas frescas. Quizás comamos bien esta noche.|I saw fresh tracks. We may eat well tonight.|Vi rastros frescos. Talvez comamos bem esta noite.|看到新鲜的脚印，今晚也许能吃顿好的。
cam_act_07_02|7|Sigan. Cazaré cuando caiga la luz.|Keep moving. I will hunt when the light fades.|Continuem. Vou caçar quando a luz cair.|继续走，天黑了我再去打猎。
cam_act_08_01|8|Unas flechas más podrían mantenernos vivos.|A few more arrows may keep us alive.|Mais algumas flechas podem nos manter vivos.|多几支箭，也许能多保住几条命。
cam_act_08_02|8|Astas rectas, puntas afiladas. Sin desperdicio.|Straight shafts, sharp heads. No waste.|Hastes retas, pontas afiadas. Sem desperdício.|箭杆要直，箭头要利，一点都不能浪费。
cam_act_09_01|9|Me adelantaré a explorar el camino.|I will advance to explore the road.|Vou adiantar-me para explorar a estrada.|我先去前面探探路。
cam_act_09_02|9|Exploraré el próximo tramo antes del anochecer.|I will scout the next stretch before dark.|Vou explorar o próximo trecho antes de escurecer.|天黑之前，我会探完下一段路。
cam_act_10_01|10|La oscuridad se acerca. La contendré.|The darkness presses close. I will hold it back.|A escuridão se aproxima. Vou contê-la.|黑暗正在逼近，我来挡住它。
cam_act_10_02|10|Mis ritos protegerán esta caravana.|My rites will shield this caravan.|Meus ritos protegerão esta caravana.|我的仪式会护住这支车队。
cam_act_11_01|11|Permanezcan cerca. Nadie quedará atrás.|Stay near. No one is left behind.|Fiquem perto. Ninguém ficará para trás.|都靠近点，谁都不会掉队。
cam_act_11_02|11|Los más débiles necesitan una mano firme.|The weakest need a steady hand.|Os mais fracos precisam de uma mão firme.|最弱的人，需要一只稳定的手。
cam_act_12_01|12|Los curanderos necesitan ayuda. Tengo tiempo.|The healers need help. I can spare the time.|Os curandeiros precisam de ajuda. Tenho tempo.|治疗师需要帮手，我有的是时间。
cam_act_12_02|12|Tráiganme a los heridos. Trabajaremos juntos.|Bring me the wounded. We work as one.|Tragam os feridos. Trabalharemos juntos.|把伤员带来吧，我们一起处理。
cam_act_13_01|13|Una hoja desafilada es un insulto.|A dull blade is an insult.|Uma lâmina cega é um insulto.|一把不锋利的刀，简直是种侮辱。
cam_act_13_02|13|Que vengan. Mi filo está listo.|Let them come. My edge is ready.|Que venham. Meu fio está pronto.|让他们来吧，我的刀刃已经准备好了。
cam_act_14_01|14|No me verán vigilarlos.|They will not see me watching.|Não vão me ver observando.|他们不会发现我在看。
cam_act_14_02|14|Si algo nos sigue, lo sabré.|If something follows us, I will know.|Se algo nos seguir, eu saberei.|要是有什么在跟踪我们，我会知道。
cam_act_15_01|15|Los mercaderes contribuirán. Por voluntad o por fuerza.|The merchants will contribute. Willingly or otherwise.|Os mercadores vão contribuir. Por vontade ou pela força.|商人们会出份力的，不管是自愿还是被迫。
cam_act_15_02|15|Déjenme las negociaciones.|Leave the negotiations to me.|Deixem as negociações comigo.|谈判的事交给我。
cam_act_16_01|16|Mi poder se acumula. No me distraigan.|My power is gathering. Do not distract me.|Meu poder está se acumulando. Não me distraiam.|我的力量正在汇聚，别打扰我。
cam_act_16_02|16|Cada paso enfoca mejor el patrón.|Every step brings the pattern into focus.|Cada passo deixa o padrão mais nítido.|每一步都让这个法阵更清晰。
cam_act_17_01|17|La carga me obedece mejor que los bueyes.|The load obeys me better than the oxen do.|A carga me obedece melhor que os bois.|这些货物比牛群更听我的话。
cam_act_17_02|17|Sigan avanzando. Yo llevo el peso.|Keep moving. I have the weight.|Continuem andando. Eu cuido do peso.|继续走，重量我来扛。
cam_act_18_01|18|Una línea más y el sello estará completo.|One more line and the ward is complete.|Mais uma linha e o selo estará completo.|再画一笔，这道封印就完成了。
cam_act_18_02|18|Este símbolo podría mantener vivo a uno de ustedes.|This symbol may keep one of you alive.|Este símbolo pode manter um de vocês vivo.|这个符号，也许能救你们中的一个人。
cam_act_19_01|19|Si hay problemas, seré la primera en moverme.|If trouble starts, I will be first to move.|Se houver problemas, serei a primeira a agir.|如果出事，我会第一个行动。
cam_act_19_02|19|Relájense. Estoy bastante alerta por todos.|Relax. I am alert enough for all of us.|Relaxem. Estou alerta o bastante por todos.|放松点，我一个人的警惕就够所有人用了。
cam_act_20_01|20|Una charla acorta el camino.|A little conversation makes the road shorter.|Uma conversa encurta a estrada.|聊聊天，路也会短一些。
cam_act_20_02|20|Vamos, alguien debe tener una buena historia.|Come on, someone must have a good story.|Vamos, alguém deve ter uma boa história.|来吧，总该有人有个好故事。
cam_act_21_01|21|Quédense cerca, amigos. Las malas noticias pesan menos juntos.|Keep close, friends. Bad news weighs less together.|Fiquem perto, amigos. Notícias ruins pesam menos juntos.|靠近点，朋友们，坏消息一起扛会轻一些。
cam_act_21_02|21|Un día duro no termina el viaje.|One hard day does not end the journey.|Um dia difícil não encerra a jornada.|难熬的一天，不代表旅程就此结束。
";
}
