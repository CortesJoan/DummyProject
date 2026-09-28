using System;

namespace AnimalMemory.Progression
{
    /// <summary>
    /// Catálogo puro de las escenas VN del postgame: apertura, intro antes de
    /// cada duelo, cierre tras victoria o derrota, el checkpoint guionizado
    /// de Rei y el epílogo definitivo. Sin UnityEngine y sin depender de
    /// MementoPostgameRelease.Enabled: la puerta sólo se presenta cuando el
    /// GameManager la declara disponible.
    /// Los ids de escena son negativos y distintos de -1: nunca se registran
    /// como vistas de campaña ni entran en el flujo de replays de la historia.
    /// Custodias y Creador hablan sin GuideId (sin retrato ni voz prestada):
    /// sus identidades aún no tienen arte propio y el catálogo de voces exige
    /// texto exacto por guía, por lo que quedan en silencio hasta regrabar.
    /// Guion: GLM (scenario-route-director, job-glm-story-postgame-20260913);
    /// cinco ajustes de continuidad sobre la autoría de la regla del Deseo,
    /// reconciliados con la reescritura de la campaña (la regla la escribió
    /// Rei; el Creador es el arquitecto del Archivo con su propia ley).
    /// </summary>
    public static class MementoPostgameStory
    {
        private const int DoorWorldId = 5; // Fondo de Archivo Cero.

        public static bool HasIntro(int encounterId)
        {
            ValidateEncounter(encounterId);
            return encounterId != 4;
        }

        public static bool HasOutcome(int encounterId)
        {
            ValidateEncounter(encounterId);
            return encounterId != 4;
        }

        /// <summary>Apertura de la puerta: se presenta una sola vez, antes del
        /// primer encuentro (CompletedMask == 0).</summary>
        public static MementoMatchStoryScene BuildOpeningScene() =>
            Scene(-42, "LA PUERTA INTERIOR · APERTURA", "LA PUERTA INTERIOR",
                N("Terminada la campaña, quedó una puerta que nadie recordaba haber visto: la Puerta Interior. Rei la miró más tiempo que nadie."),
                G(5,"Más allá vive el arquitecto del Archivo: el Creador. Guarda las técnicas de todas las campeonas anteriores... incluida la mía, y leyes que ninguna firmó."),
                N("Antes de la sala final, cuatro antecámaras custodian el paso. Cuatro espejos llevados al extremo: Cálculo, Azar, Vínculo y Orgullo."),
                P("Pulso del Relevo: hoy el turno se presta. Cada espejo lo enfrenta una de nosotras; la última jugada la hacemos juntos."));

        public static MementoMatchStoryScene BuildIntro(int encounterId)
        {
            ValidateEncounter(encounterId);
            switch (encounterId)
            {
                case 0: return Scene(-10, "LA PUERTA INTERIOR · 1 / 6",
                    "EL MARGEN DE ERROR",
                    N("La primera antecámara del Archivo es un taller detenido: cronómetros en cero, cartas alineadas con regla y compás. Una figura espera, quieta como una ecuación."),
                    C(6,"Muestra: Mika, inventora. Incertidumbre media del 0,4 por ciento. Inaceptable. Hoy juego con tu baraja y tu margen de error: cero."),
                    G(1,"Un duelo contra mi propio espejo. Perfecto. Traigo un dato que no está en tu archivo: yo ya no juego sola."),
                    N("La Custodia cubre media baraja con una plantilla metálica: de pronto, las posiciones visibles ya no bastan para ningún cálculo limpio."),
                    C(6,"Datos incompletos. Ríndete o falla. O deja que yo decida por ti en cada turno, y el error desaparecerá de tu vida."),
                    G(1,"Rechazado: el error no desaparece, se comparte y se corrige. {PLAYER}, préstame el pulso. Hoy decido yo, con los datos que tenga."));
                case 1: return Scene(-11, "LA PUERTA INTERIOR · 2 / 6",
                    "UNA CERTEZA ELEGIDA",
                    N("La segunda antecámara no tiene suelo firme: cartas que caen como nieve, se barajan solas y vuelven a caer. En el centro, alguien sonríe sin mirar nada."),
                    C(7,"Aquí nada se decide y nada se pierde. Es la paz que pedías cada noche, astrónoma. Quédate a mirar el cielo conmigo."),
                    G(2,"Un cielo donde nada se mueve con sentido... No, gracias. Yo no aprendí a leer las estrellas para renunciar a elegir."),
                    N("La Visión Estelar encuentra un patrón entre las cartas que caen. La Custodia lo deshace con un soplo, sin dejar de sonreír."),
                    C(7,"Todo patrón es casualidad. Acepta mi regalo: yo te diré qué carta es, y tú solo repite la certeza."),
                    G(2,"Las certezas prestadas no son certezas. {PLAYER}, el pulso, por favor: esta noche elijo yo, dentro de la niebla."));
                case 2: return Scene(-12, "LA PUERTA INTERIOR · 3 / 6",
                    "PROTEGER SIN ENCERRAR",
                    N("La tercera antecámara es un jardín bajo una cúpula perfecta: ni una hoja fuera de marco, ni una semilla en el suelo. Huele a siempre."),
                    C(8,"Aquí lo que amas no puede ser dañado: nadie entra, nadie sale, nadie cambia. Ese es el amor completo, jardinera."),
                    G(3,"Es hermoso... y está quieto. Un jardín que ya no crece no protege a nadie: solo se conserva a sí mismo."),
                    N("Hana apoya la palma contra el cristal. Del otro lado, una flor gira muy despacio, buscando una luz que nunca se mueve."),
                    C(8,"Renuncia al turno. A partir de hoy decido yo por ti y por las tuyas: así jamás perderéis a nadie."),
                    G(3,"Cuidar no es decidir por otra flor. {PLAYER}, el pulso, por favor: voy a abrir esta cúpula."));
                case 3: return Scene(-13, "LA PUERTA INTERIOR · 4 / 6",
                    "PEDIR AYUDA NO ES PERDER",
                    N("La cuarta antecámara es un escenario vacío con un solo reflector. Bajo la luz, una figura aplaude de pie: invicta, intacta, completamente sola."),
                    C(9,"Invicta. Sin equipo, sin favores, sin deudas. Ese era tu sueño de niña, campeona pequeña: yo lo conservé por ti."),
                    G(4,"Momo... Momo sí soñó eso. Pero Momo también soñó con alguien a quien enseñarle la copa."),
                    N("La Custodia imita, exacta, la pose de victoria de Momo. No hay alegría en ella: solo el reflejo de un aplauso que nadie da."),
                    C(9,"Última oferta: quédate el reflector. Gana sola para siempre y no volverás a necesitar a nadie."),
                    G(4,"¡Momo rechaza! ¡Momo quiere ganar CON todos! {PLAYER}, préstame el pulso: ¡función especial, con público!"));
                case 5: return Scene(-15, "LA PUERTA INTERIOR · FINAL",
                    "NUESTRA SIGUIENTE JUGADA",
                    N("El Archivo entero se pliega en un tablero: seis zonas boca abajo, girando despacio. El Creador baraja con dedos de tinta."),
                    C(10,"Duelo final. Soy la séptima respuesta a «¿qué haces cuando tienes algo que perder?»: eliminar la posibilidad de perder controlando las reglas."),
                    C(10,"Cada rayo puede resolver varias parejas. Si me obligáis a abrir las alas, también borraré cartas. Aprended a jugar con lo que quede."),
                    G(2,"Entonces miraremos el tablero después de cada rayo. No vamos a buscar una carta que ya no existe."),
                    P("No puedo prometer que no fallemos. Mika, Yoru, Hana, Momo: si esto empeora, os pediré ayuda. Una jugada cada vez."),
                    N("Mika, Yoru, Hana y Momo se colocan junto a la protagonista. El lugar de Aki queda vacío. Rei permanece junto al marco, recuperándose del duelo anterior."));
                default: throw FailEncounter();
            }
        }

        public static MementoMatchStoryScene BuildVictory(int encounterId)
        {
            ValidateEncounter(encounterId);
            switch (encounterId)
            {
                case 0: return Scene(-20, "CUSTODIA SUPERADA",
                    "EL MARGEN DE ERROR",
                    C(6,"Anomalía: decidiste con datos incompletos y ganaste. Mi modelo no lo permite."),
                    G(1,"Tu modelo tenía una variable sin registrar: una compañera a quien pedirle la carta siguiente."),
                    G(1,"Para el acta oficial: gané con un modelo mejor. Entre nos... gracias por el pulso, {PLAYER}."),
                    N("La Custodia baja el compás. En el taller detenido, un cronómetro olvidado empieza a contar hacia atrás: vuelve el tiempo."));
                case 1: return Scene(-21, "CUSTODIA SUPERADA",
                    "UNA CERTEZA ELEGIDA",
                    C(7,"Elegiste sin conocer el resultado. Ninguna estrella te lo dictó. ¿No te dio miedo?"),
                    G(2,"Me lo dio. Elegí de todos modos: a eso le llamo voluntad. Tu azar solo la ponía a prueba."),
                    G(2,"Y registra esto, por favor: la certeza no se encuentra en el cielo. Se elige, a mano, aquí abajo."),
                    N("La nieve de cartas se asienta. Por un instante forman una constelación nueva; nadie la registra y a nadie le hace falta."));
                case 2: return Scene(-22, "CUSTODIA SUPERADA",
                    "PROTEGER SIN ENCERRAR",
                    C(8,"La cúpula está abierta. Afuera hay frío, sequía, gusanos. ¿A esto le llamas proteger?"),
                    G(3,"Proteger es estar ahí cuando lleguen. El cristal solo postergaba la primera helada."),
                    G(3,"(Yo también quise guardar a alguien bajo cristal, ¿sabes?... Perdón. Crece cuanto quieras.)"),
                    N("El viento entra por primera vez en años. La flor gira hacia él y, en el giro, encuentra un sol nuevo."));
                case 3: return Scene(-23, "CUSTODIA SUPERADA",
                    "PEDIR AYUDA NO ES PERDER",
                    C(9,"Tu jugada final era imposible... salvo que alguien te sostuviera la mano. La sentí temblar."),
                    G(4,"¡Me tembló y la pedí! Pedir ayuda es la única jugada que no tienes archivada, Custodia."),
                    G(4,"Ganar sola sabe a media victoria. Esto sabe entero... ¿Encore? ¡Encore con todas!"),
                    N("El reflector se parte en seis haces de luz. El escenario vacío, por fin, tiene público."));
                case 5: return Scene(-25, "VICTORIA FINAL",
                    "NUESTRA SIGUIENTE JUGADA",
                    N("La última pareja queda boca arriba: dos cartas idénticas con el nombre del Creador. El tablero se detiene por completo."),
                    C(10,"Imposible. Regístralo como error del sistema: yo escribí las reglas para que nadie perdiera jamás."),
                    P("Un torneo sin pérdidas es un torneo sin riesgo, y sin riesgo nadie cambia. No puedo asegurar que nadie vaya a perder; sí puedo decidir qué hacemos después de perder."),
                    N("El Creador espera un deseo que no llega. En el silencio, el Archivo entero contiene la respiración que no tiene."));
                default: throw FailEncounter();
            }
        }

        public static MementoMatchStoryScene BuildDefeat(int encounterId)
        {
            ValidateEncounter(encounterId);
            switch (encounterId)
            {
                case 0: return Scene(-30, "REINTENTO · LA PUERTA SIGUE ABIERTA",
                    "EL MARGEN DE ERROR",
                    C(6,"Derrota registrada. Margen de error: humano."),
                    G(1,"Cero intentos también es un margen inaceptable. Reviso la partida, ajusto dos variables y vuelvo."),
                    N("La plantilla metálica se retira sola. La Custodia reordena el mazo; Mika, de pie, ya toma notas."));
                case 1: return Scene(-31, "REINTENTO · LA PUERTA SIGUE ABIERTA",
                    "UNA CERTEZA ELEGIDA",
                    C(7,"Nada se decidió. Tú tampoco. ¿No es esto descansar?"),
                    G(2,"Descansaré cuando haya elegido. La niebla solo me dio tiempo: volvamos a mirar juntas."),
                    N("Las cartas siguen cayendo, pacientes. Yoru alisa una manga y levanta la vista al cielo incompleto."));
                case 2: return Scene(-32, "REINTENTO · LA PUERTA SIGUE ABIERTA",
                    "PROTEGER SIN ENCERRAR",
                    C(8,"El cristal resistió. Es más fuerte que cualquier primavera."),
                    G(3,"Ningún jardín florece a la primera. Riego, aprendo el suelo y vuelvo: la cúpula no es eterna."),
                    N("Dentro de la cúpula, la flor gira un milímetro hacia Hana. El cristal lo anota con un crujido."));
                case 3: return Scene(-33, "REINTENTO · LA PUERTA SIGUE ABIERTA",
                    "PEDIR AYUDA NO ES PERDER",
                    C(9,"Invicta sigo siendo yo. Tú volviste a necesitar a alguien."),
                    G(4,"¡Momo no perdió: ensayó! Y los ensayos van con equipo. ¡Preparen bambalinas: segunda función!"),
                    N("Desde las butacas vacías, cinco siluetas aplauden despacio. Momo hace una reverencia y respira hondo."));
                case 5: return Scene(-35, "REINTENTO · EXAMEN DEL CREADOR",
                    "NUESTRA SIGUIENTE JUGADA",
                    C(10,"Derrota registrada. El Deseo sigue disponible: puedo borrar esta partida de todos los recuerdos."),
                    P("No la borres. Las derrotas también son recuerdos: deja que el Archivo las cuide."),
                    N("El tablero se rebaraja, paciente. La Puerta Interior no se cierra: ninguna regla obliga a rendirse."));
                default: throw FailEncounter();
            }
        }

        /// <summary>Checkpoint guionizado: la derrota de Rei es la única forma de avanzar.</summary>
        public static MementoMatchStoryScene BuildScriptedScene() =>
            Scene(-40, "LA DECISIÓN DE REI", "LA PUERTA MÁS PROFUNDA",
                N("Ante la Puerta Interior, Rei se adelanta y pone la mano en el marco. Nadie la detiene: la deuda de la campeona la paga la campeona."),
                G(5,"Fui campeona antes que todas ustedes. Esta puerta me conoce y yo la conozco. Entro sola: es mi cuenta pendiente."),
                N("Del otro lado no hay sala, sino estanterías hasta el techo: el Archivo, con cada duelo jamás jugado repitiéndose en miniatura."),
                C(10,"Bienvenida de nuevo, Rei. Guardé cada una de tus jugadas. Hoy jugaré contra ti con las técnicas de todas las campeonas... y con la tuya al final."),
                G(5,"Empieza, entonces. Quiero comprobar cuánto he cambiado desde que me archivaste."),
                N("Deshacer deshace su apertura. El Escudo de Combo le cierra el paso. La Visión Estelar le roba la lectura."),
                N("La Floración la enreda. El Encore la adelanta. Y llega la sexta técnica: la de Rei, ejecutada sin una sola duda."),
                C(10,"¿Sientes la diferencia? Ellas dudaban; yo no. Sin duda no hay derrota, y sin derrota nadie cambia. Yo les quité el riesgo a todos."),
                N("Rei cae sobre una rodilla. En el umbral, seis siluetas se detienen sin entrar: esperan, en silencio, a que ella decida."),
                G(5,"...No puedo ganar sola."),
                G(5,"Archiva esto, Creador, con fecha y todo: la campeona Rei acaba de perder en voz alta y por decisión propia."),
                C(10,"Dato inútil. La debilidad no le enseña nada a nadie."),
                G(5,"A ti no. A nosotras sí. {PLAYER}... el control vuelve a la alianza: es mi jugada final y la mejor que tengo."),
                N("Rei cruza el umbral y deposita su ficha de turno en la mano de {PLAYER}. La Puerta Interior queda abierta: por primera vez, para todas."));

        public static MementoMatchStoryScene BuildEpilogue() =>
            Scene(-41, "EPÍLOGO DEFINITIVO", "LA CARTA EN EL MARCO",
                N("El tablero se apaga y queda el salón del Archivo, inmenso y en silencio. La pluma del Creador sigue en alto, esperando un deseo."),
                C(10,"Su Deseo, campeonas. Pídanlo: borraré la derrota, el dolor, el riesgo. Es la ley que escribí para el final de todos los torneos."),
                P("No deseamos borrar nada. Retira tu ley sin riesgo: en este torneo, lo perdido también se guarda."),
                C(10,"Sin mi ley no hay torneo sin riesgo... y sin reglas que me obedezcan, ¿qué queda de mí?"),
                G(5,"Un jugador más. Todo el mundo pierde el primer día: bienvenido al club de los que siguen jugando."),
                N("La ley del estrado se disuelve como tinta en agua. El Creador baja del estrado y ordena un estante: ahora cuida el Archivo; ya no lo gobierna."),
                N("Las seis zonas dejan de repetir sus jugadas perfectas: el bosque sorprende, el arrecife gira distinto, las constelaciones se corren, el jardín y la confitería se abren."),
                G(0,"¡Oye, Archivo, registra esto: hoy ganamos todos! ¡Hasta tú aprendiste algo nuevo!"),
                G(4,"¡Momo declara postre de la victoria! Raciones para siete: el recién llegado también come pastel."),
                N("La Puerta Interior se cierra sin llave ni cerradura, y no vuelve a abrirse: dentro ya no queda nada por ganar."),
                N("{PLAYER} deja en el marco su carta más gastada, la de más recuerdos. El hueco en la baraja no se siente como una pérdida."),
                P("¿Qué hacemos cuando tenemos algo que perder?... Lo jugamos. Juntos. Otra vez."));

        public static bool HasCustodiaProfile(int opponentId) =>
            MementoPostgameEncounters.IsPostgameOpponent(opponentId);

        public static string GetCustodiaIdentity(int opponentId)
        {
            switch (opponentId)
            {
                case 6: return "control total, margen de error cero.";
                case 7: return "azar puro donde nada se decide.";
                case 8: return "una jaula perfecta que protege encerrando.";
                case 9: return "un aislamiento invicto que no necesita a nadie.";
                case 10: return "Arquitecto del Archivo. Elimina la posibilidad de perder controlando las reglas.";
                default: throw FailOpponent(opponentId);
            }
        }

        private static void ValidateEncounter(int encounterId)
        {
            if (encounterId < 0 || encounterId >= MementoPostgameEncounters.Count)
                throw new ArgumentOutOfRangeException(nameof(encounterId), encounterId,
                    "Encuentro postgame inexistente.");
        }

        private static ArgumentOutOfRangeException FailEncounter() =>
            new ArgumentOutOfRangeException("encounterId",
                "El encuentro 4 es un checkpoint guionizado sin intro ni cierre propios.");

        private static ArgumentOutOfRangeException FailOpponent(int opponentId) =>
            new ArgumentOutOfRangeException(nameof(opponentId), opponentId,
                "Oponente postgame inexistente.");

        private static MementoMatchStoryScene Scene(int id, string kicker, string chapter,
            params MementoMatchStoryBeat[] beats) =>
            new MementoMatchStoryScene(id, DoorWorldId, MementoMatchStoryPhase.Challenge,
                kicker, chapter, beats);

        private static MementoMatchStoryBeat N(string text) =>
            new MementoMatchStoryBeat("ESCENA", text, -1, MementoMatchStoryVoiceCue.None,
                MementoMatchStoryBeatKind.Narration);
        private static MementoMatchStoryBeat P(string text) =>
            new MementoMatchStoryBeat("TÚ · ASPIRANTE", text, -1, MementoMatchStoryVoiceCue.None,
                MementoMatchStoryBeatKind.Protagonist);
        private static MementoMatchStoryBeat G(int guideId, string text) =>
            new MementoMatchStoryBeat(GuideSpeaker(guideId), text, guideId,
                MementoMatchStoryVoiceCue.None, MementoMatchStoryBeatKind.Guardian);

        /// <summary>Custodia o Creador: sin GuideId para no tomar retrato ni voz prestados.</summary>
        private static MementoMatchStoryBeat C(int opponentId, string text) =>
            new MementoMatchStoryBeat(MementoPostgameEncounters.GetOpponentName(opponentId)
                .ToUpperInvariant(), text, -1, MementoMatchStoryVoiceCue.None,
                MementoMatchStoryBeatKind.Challenge);

        private static string GuideSpeaker(int guideId)
        {
            switch (guideId)
            {
                case 0: return "AKI";
                case 1: return "MIKA";
                case 2: return "YORU";
                case 3: return "HANA";
                case 4: return "MOMO";
                case 5: return "REI";
                default: throw new ArgumentOutOfRangeException(nameof(guideId), guideId,
                    "Guía inexistente.");
            }
        }
    }
}
