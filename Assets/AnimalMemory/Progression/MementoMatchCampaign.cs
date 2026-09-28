using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    public enum MementoMatchStoryPhase { WorldIntro, Challenge, Retry, Recruit, FinalBoss, Epilogue }
    public enum MementoMatchStoryVoiceCue { None, StoryIntro, StoryChallenge, StoryRetry, StoryRecruit }
    public enum MementoMatchStoryBeatKind { Narration, Protagonist, Guardian, Challenge, Retry, Recruit, Ability }
    public enum MementoStoryCue { None, Warning, ChampionEntrance, OpeningDuel, Launch, Impact, Defeat, Aftermath, Resolve }

    public readonly struct MementoMatchCampaignLevel
    {
        public int Id { get; }
        public int WorldId { get; }
        public int NodeInWorld { get; }
        public int SetId { get; }
        public int OpponentGuideId { get; }
        public int DifficultyId { get; }
        public int Rows { get; }
        public int Columns { get; }
        public int EfficiencyAllowance { get; }
        public int MismatchGoal { get; }
        public int ComboGoal { get; }
        public string Title { get; }
        public string Objective { get; }
        public bool IsBoss { get; }
        public bool IsFinalBoss => Id == MementoMatchCampaignRules.FinalBossLevelId;

        public MementoMatchCampaignLevel(int id, int worldId, int nodeInWorld, int setId,
            int opponentGuideId, int difficultyId, int rows, int columns,
            int efficiencyAllowance, int mismatchGoal, int comboGoal,
            string title, string objective, bool isBoss)
        {
            Id = id; WorldId = worldId; NodeInWorld = nodeInWorld; SetId = setId;
            OpponentGuideId = opponentGuideId; DifficultyId = difficultyId;
            Rows = rows; Columns = columns; EfficiencyAllowance = efficiencyAllowance;
            MismatchGoal = mismatchGoal; ComboGoal = comboGoal; Title = title;
            Objective = objective; IsBoss = isBoss;
        }
    }

    public readonly struct MementoMatchStoryBeat
    {
        public string Speaker { get; }
        public string Text { get; }
        public int GuideId { get; }
        public MementoMatchStoryVoiceCue VoiceCue { get; }
        public MementoMatchStoryBeatKind Kind { get; }
        public bool IsSilhouette { get; }
        public MementoStoryCue PresentationCue { get; }

        public MementoMatchStoryBeat(string speaker, string text, int guideId,
            MementoMatchStoryVoiceCue voiceCue,
            MementoMatchStoryBeatKind kind = MementoMatchStoryBeatKind.Guardian,
            bool isSilhouette = false, MementoStoryCue presentationCue = MementoStoryCue.None)
        {
            Speaker = speaker; Text = text; GuideId = guideId; VoiceCue = voiceCue;
            Kind = kind; IsSilhouette = isSilhouette;
            PresentationCue = presentationCue;
        }

        /// <summary>Attaches scene direction without changing dialogue or shared VN execution.</summary>
        public MementoMatchStoryBeat WithPresentation(MementoStoryCue cue) =>
            new MementoMatchStoryBeat(Speaker, Text, GuideId, VoiceCue, Kind, IsSilhouette, cue);
    }

    public sealed class MementoMatchStoryScene
    {
        public int Id { get; }
        public int WorldId { get; }
        public MementoMatchStoryPhase Phase { get; }
        public string Kicker { get; }
        public string Chapter { get; }
        public IReadOnlyList<MementoMatchStoryBeat> Beats { get; }
        public bool IsPersistent => Id >= 0;

        public MementoMatchStoryScene(int id, int worldId, MementoMatchStoryPhase phase,
            string kicker, string chapter, params MementoMatchStoryBeat[] beats)
        {
            Id = id; WorldId = worldId; Phase = phase; Kicker = kicker; Chapter = chapter;
            Beats = beats ?? Array.Empty<MementoMatchStoryBeat>();
        }
    }

    public static class MementoMatchCampaignRules
    {
        public const string CampaignTitle = "La Liga de las Seis Zonas";
        public const string ProtagonistGoal = "Recuperar Zona Cero sin arrebatar el hogar de nadie y llevar a una alianza de seis zonas a decidir juntas un mismo Deseo.";
        public const int WorldCount = 5;
        public const int LevelsPerWorld = 4;
        public const int BaseLevelCount = WorldCount * LevelsPerWorld;
        public const int FinalBossWorldId = 5;
        public const int FinalBossLevelId = BaseLevelCount;
        public const int LevelCount = BaseLevelCount + 1;
        public const int FinalBossSceneId = WorldCount * 3;
        public const int EpilogueSceneId = FinalBossSceneId + 1;

        private static readonly MementoMatchCampaignLevel[] Levels =
        {
            L(0,0,0,0,0,0,2,3,1,0,2,"Refugio entre hojas","Alcanza a Aki antes de la siguiente campana."),
            L(1,0,1,0,0,1,4,4,2,1,3,"Sendero de ecos","Aprende cómo defiende el Bosque Vivo."),
            L(2,0,2,0,0,1,4,5,3,1,4,"Frontera gemela","Abre una ruta segura hasta el núcleo."),
            L(3,0,3,0,0,0,4,4,2,2,4,"Aki · Pacto del bosque","Vence a Aki y propón la primera alianza.",true),
            L(4,1,0,1,1,0,3,4,2,0,3,"Costa de engranajes","Entra en la Zona Coral."),
            L(5,1,1,1,1,1,4,4,2,1,4,"Relevo mecánico","Sincroniza las reliquias del taller."),
            L(6,1,2,1,1,1,4,5,3,1,5,"Plan a contrarreloj","Demuestra que el equipo puede adaptarse."),
            L(7,1,3,1,1,1,4,4,2,2,5,"Mika · Cálculo coral","Supera su defensa y gana su confianza.",true),
            L(8,2,0,2,2,0,3,4,2,0,3,"Ruta de tinta","Sigue el mapa hacia el observatorio."),
            L(9,2,1,2,2,1,4,4,2,1,4,"Órbita de parejas","Reconstruye la constelación de acceso."),
            L(10,2,2,2,2,2,4,5,3,2,5,"Tramo sin luz","Avanza cuando desaparezcan las pistas."),
            L(11,2,3,2,2,2,4,5,3,2,6,"Yoru · Juicio estelar","Convence a Yoru de que la alianza tiene futuro.",true),
            L(12,3,0,3,3,0,3,4,2,0,3,"Semillas fronterizas","Evita que el jardín sea absorbido."),
            L(13,3,1,3,3,1,4,4,2,1,4,"Pétalos cruzados","Haz florecer el patrón oculto."),
            L(14,3,2,3,3,1,4,5,3,1,5,"Invernadero vivo","Protege la racha mientras cambia el jardín."),
            L(15,3,3,3,3,1,4,4,2,2,5,"Hana · Raíz compartida","Prueba que una alianza también puede proteger.",true),
            L(16,4,0,4,4,0,3,4,2,0,3,"Frontera de azúcar","Llega al Atelier antes del cierre."),
            L(17,4,1,4,4,1,4,4,2,1,4,"Receta en fuga","Mantén el ritmo ante las trampas."),
            L(18,4,2,4,4,2,4,5,3,2,6,"Encore imposible","Resiste el último ensayo."),
            L(19,4,3,4,4,2,4,5,3,2,6,"Momo · Dulce revancha","Descubre quién tomó la Zona Cero.",true),
            L(20,5,0,5,AnimalMemoryContentIds.UltimateMasterOpponent,2,5,6,5,3,8,
                "Rei · Examen del Archivo Cero","Vence a la Maestra Definitiva en un tablero de 30 cartas.",true)
        };

        public static MementoMatchCampaignLevel GetLevel(int id)
        {
            if (id < 0 || id >= LevelCount) throw new ArgumentOutOfRangeException(nameof(id));
            return Levels[id];
        }

        public static bool IsLevelUnlocked(int id, int clearedMask) =>
            id >= 0 && id < LevelCount && (id == 0 || (clearedMask & (1 << (id - 1))) != 0);

        public static bool IsFinalBossUnlocked(int clearedMask) =>
            (clearedMask & ((1 << BaseLevelCount) - 1)) == (1 << BaseLevelCount) - 1;

        public static int EvaluateStars(int id, int turns, int pairs, int mismatches, int maxCombo)
        {
            var level = GetLevel(id);
            int stars = 1;
            if (turns <= pairs + level.EfficiencyAllowance) stars++;
            if (mismatches <= level.MismatchGoal || maxCombo >= level.ComboGoal) stars++;
            return Math.Min(3, stars);
        }

        public static int GetStoredStars(int id, int cleared, int two, int three)
        {
            int bit = 1 << id;
            if ((cleared & bit) == 0) return 0;
            if ((three & bit) != 0) return 3;
            return (two & bit) != 0 ? 2 : 1;
        }

        public static int CountTotalStars(int cleared, int two, int three)
        {
            int total = 0;
            for (int i = 0; i < LevelCount; i++) total += GetStoredStars(i, cleared, two, three);
            return total;
        }

        public static int GetRecommendedLevel(int cleared)
        {
            for (int i = 0; i < LevelCount; i++) if ((cleared & (1 << i)) == 0) return i;
            return LevelCount - 1;
        }

        public static string GetWorldName(int id)
        {
            switch (id)
            {
                case 0: return "Zona del Bosque Vivo";
                case 1: return "Zona del Taller Coral";
                case 2: return "Zona del Cielo de Tinta";
                case 3: return "Zona del Jardín Errante";
                case 4: return "Zona del Dulce Atelier";
                case 5: return "Archivo Cero";
                default: return string.Empty;
            }
        }

        public static string GetWorldShortName(int id)
        {
            switch (id)
            {
                case 0: return "AKI · BOSQUE";
                case 1: return "MIKA · CORAL";
                case 2: return "YORU · CIELO";
                case 3: return "HANA · JARDÍN";
                case 4: return "MOMO · DULCES";
                case 5: return "REI · ARCHIVO CERO";
                default: return string.Empty;
            }
        }

        public static int GetPreSceneId(int id)
        {
            var level = GetLevel(id);
            if (level.IsFinalBoss) return FinalBossSceneId;
            if (level.NodeInWorld == 0) return level.WorldId * 3;
            return level.IsBoss ? level.WorldId * 3 + 1 : -1;
        }

        public static int GetPostSceneId(int id)
        {
            var level = GetLevel(id);
            if (level.IsFinalBoss) return EpilogueSceneId;
            return level.IsBoss ? level.WorldId * 3 + 2 : -1;
        }

        public static MementoMatchStoryScene GetRetryScene(int id)
        {
            var level = GetLevel(id);
            if (level.IsFinalBoss)
                return Scene(-1,5,MementoMatchStoryPhase.Retry,"REINTENTO · EXAMEN FINAL",
                    level.Title.ToUpperInvariant(),
                    N("Rei reúne las treinta cartas sin prisa, como quien relee su propia caligrafía. La sede abre una vez más el tablero final."),
                    G(5,"Volvisteis a elegir… y volvisteis juntas. Mostradme que también sabéis corregir juntas.",MementoMatchStoryBeatKind.Retry),
                    P("Ya leí tu tablero una vez, Rei. Ahora jugamos el nuestro."),
                    N("Las treinta cartas se tienden de nuevo bajo la luz de la sede. Última decisión… última función."));
            return Scene(-1,level.WorldId,MementoMatchStoryPhase.Retry,
                "REINTENTO · LA ZONA SIGUE EN JUEGO",level.Title.ToUpperInvariant(),
                N("La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra."),
                G(level.WorldId,RetryLines[level.WorldId],MementoMatchStoryBeatKind.Retry),
                P("Lo jugado no se borra: se aprende. Esta vez elegimos mejor."),
                N("La sala respira… y el duelo vuelve a empezar."));
        }

        public static MementoMatchStoryScene GetStoryScene(int id)
        {
            if (id == FinalBossSceneId) return WithParty(StoryScenes[15]);
            if (id == EpilogueSceneId) return StoryScenes[16];
            if (id < 0 || id >= FinalBossSceneId) throw new ArgumentOutOfRangeException(nameof(id));
            return WithParty(StoryScenes[id]);
        }

        private static MementoMatchStoryScene WithParty(MementoMatchStoryScene scene)
        {
            var beats = new List<MementoMatchStoryBeat>(scene.Beats);
            var party = new List<MementoMatchStoryBeat>();
            switch (scene.Id)
            {
                case 6:
                    party.AddRange(new[] {
                        G(1,"Constelaciones no sostienen defensas… aunque esa coordenada que marcó anoche encaja con mi matriz. No lo tomen como elogio.",MementoMatchStoryBeatKind.Guardian),
                        N("Mika refuerza el cruce norte por su cuenta, sin consultar el reparto de rutas. El bosque queda fuera de la red hasta la mañana siguiente."),
                        G(2,"Decidiste por cuatro zonas, inventora. Otra vez. …Nadie te quitó el mando: lo soltaste tú.",MementoMatchStoryBeatKind.Guardian),
                        G(1,"…Lo sé. Lo hice igual. Apunta el cruce: lo repongo yo, y esta vez lo reparto antes de reforzarlo.",MementoMatchStoryBeatKind.Guardian) });
                    break;
                case 8:
                    party.AddRange(new[] {
                        G(1,"Tu ruta oriental seguía abierta esta mañana. Lo comprobé dos veces… no lo tomes como disculpa.",MementoMatchStoryBeatKind.Guardian),
                        G(2,"Lo tomo como una coordenada, Mika. …Y una coordenada a tiempo también es cariño.",MementoMatchStoryBeatKind.Guardian) });
                    break;
                case 11:
                    party.AddRange(new[] {
                        G(3,"Antes de cruzar, debéis saberlo: Momo, la guardiana del atelier… es mi hermana menor.",MementoMatchStoryBeatKind.Guardian) });
                    break;
                case 12:
                    party.AddRange(new[] {
                        G(3,"Momo. …Tus vitrinas siguen impecables. Y el claro del sur sigue sin arraigo: tu ronda lo cruza y no lo cuida.",MementoMatchStoryBeatKind.Guardian),
                        G(4,"Y tus flores siguen llegando sin avisar. …Cuánto tiempo, hermana.",MementoMatchStoryBeatKind.Guardian),
                        G(4,"Cuido mi atelier, no tus senderos. Si el jardín quiere el claro, que lo pida con un tablero y no con una carta con flores dentro.",MementoMatchStoryBeatKind.Guardian),
                        G(3,"Una carta con flores dentro es todo lo que mandé en dos años. Y no pedí nada a cambio.",MementoMatchStoryBeatKind.Guardian),
                        N("Ninguna de las dos cruza el claro. Hana recoge su maceta y Momo vuelve a la vitrina: el sur queda sin repartir, y las dos lo saben.") });
                    break;
                case 14:
                    party.AddRange(new[] {
                        G(4,"Iré… si nadie me saca del tablero para protegerme. La función es mía hasta el final.",MementoMatchStoryBeatKind.Guardian),
                        G(3,"Nadie te quita nada, Momo. Hoy cuido al público… y dejo el escenario a las artistas.",MementoMatchStoryBeatKind.Guardian),
                        G(3,"Y te pido una cosa, no te la concedo: el claro del sur. Repártelo tú, a tu ritmo, y firma con tu nombre.",MementoMatchStoryBeatKind.Guardian),
                        G(4,"…El claro. Lo reparto yo, sí. Y la primera ronda de la mesa la pago con glaseado, que conste en acta.",MementoMatchStoryBeatKind.Guardian),
                        N("Momo cuelga por fin un emblema en el claro: el suyo, no el del atelier. Hana no lo toca. Sólo lo mira, y sigue caminando.") });
                    break;
                case 15:
                    party.AddRange(new[] {
                        G(1,"Apertura calculada: quien encadena marca el ritmo y yo aseguro la racha. Sin órdenes… por fin.",MementoMatchStoryBeatKind.Guardian),
                        G(2,"Y si el cálculo falla, miraré las estrellas. Elegir… ya sabes elegir tú.",MementoMatchStoryBeatKind.Guardian) });
                    break;
            }
            if (party.Count == 0) return scene;
            // Recruits end on the next route; final allies speak before Rei's challenge.
            int index = scene.Phase == MementoMatchStoryPhase.Recruit ||
                scene.Phase == MementoMatchStoryPhase.FinalBoss ? Math.Max(0, beats.Count - 1) : beats.Count;
            beats.InsertRange(index, party);
            return new MementoMatchStoryScene(scene.Id, scene.WorldId, scene.Phase, scene.Kicker, scene.Chapter, beats.ToArray());
        }

        private static readonly string[] Names = { "AKI","MIKA","YORU","HANA","MOMO","REI" };
        private static readonly string[] RetryLines = {
            "El bosque guardó tu error y te lo devuelve corregido: deshaz… y vuelve a andar.",
            "Derrota registrada. Ahora tienes el dato que faltaba: sigue midiendo.",
            "Las mismas estrellas, otro cielo: mira de nuevo antes de mover.",
            "El tallo que se dobla sabe por dónde subir: ajusta el ritmo y florece.",
            "¡Turno extra! La receta fallida era ensayo… ¡que empiece la función buena!"
        };

        private static readonly MementoMatchStoryScene[] StoryScenes =
        {
            Scene(0,0,MementoMatchStoryPhase.WorldIntro,
                "PRÓLOGO · ZONA CERO","EL PRIMER MINUTO",
                N("El Archivo une tableros por núcleos: zonas que compiten, trueques entre campeonas y un solo Deseo. La campana de Zona Cero suena sin desafiante anunciada.").WithPresentation(MementoStoryCue.Warning),
                P("Me llaman {PLAYER}. Zona Cero es mi hogar y su tablero, mi primer recuerdo… Si nadie lo defiende esta noche, nadie lo defenderá."),
                N("Sobre el tablero se proyecta un emblema que ninguna zona reclama. Una silueta cruza el umbral sin presentarse y sin pedir turno.").WithPresentation(MementoStoryCue.ChampionEntrance),
                S("CAMPEONA DESCONOCIDA","Un hogar no se hereda: se sostiene. Y tú… aún no sabes cómo se sostiene.",5),
                N("El tablero de Zona Cero se despliega solo: treinta cartas, quince pares, toda la defensa de la zona en una sola mesa. El duelo abre.").WithPresentation(MementoStoryCue.OpeningDuel),
                P("Voltea las parejas sin dudar. No mira las cartas… las reconoce, como si mi tablero fuera una partida que ella ya terminó."),
                N("La silueta encadena pares sin pausa. Cada acierto se convierte en una lanza de luz que cruza la mesa directo al núcleo de la zona.").WithPresentation(MementoStoryCue.Launch),
                N("Impacto. La primera grieta recorre el núcleo y las cartas de {PLAYER} se atenúan, como recuerdos que aún no sabe nombrar.").WithPresentation(MementoStoryCue.Impact),
                N("La última pareja cae de la mano de la silueta. El núcleo se apaga en silencio, sereno… como si nada se hubiera roto de verdad.").WithPresentation(MementoStoryCue.Defeat),
                S("CAMPEONA DESCONOCIDA","Zona Cero queda bajo sello. Mañana tu gente despertará igual… pero este tablero ya responde a otra mano.",5),
                P("Un minuto. Un minuto he tardado en entregar mi hogar. Nadie ha muerto, nada arde… y aun así todo pesa.").WithPresentation(MementoStoryCue.Aftermath),
                P("Que el Archivo reparta otra mano. No pienso quitarle el techo a nadie para recuperar el mío: buscaré a quien quiera jugar conmigo.").WithPresentation(MementoStoryCue.Resolve)),
            Scene(1,0,MementoMatchStoryPhase.Challenge,
                "ZONA 1 / 5 · BOSQUE VIVO","LA GUARDIANA QUE LLEGÓ TARDE",
                N("Apenas cura el sello, una guardiana cruza la puerta con paso de quien llegó tarde a algo importante. Aki, del Bosque Vivo, mira el núcleo apagado y luego a {PLAYER}."),
                G(0,"Una sin zona sólo trae problemas: no sostiene rutas ni responde por un núcleo. …Debería darte la espalda y volver al bosque.",MementoMatchStoryBeatKind.Guardian),
                P("Puedes irte. Pero respóndeme una sola cosa: ¿se puede volver a desafiar una zona sellada?"),
                G(0,"Con licencia de la Liga, sí. El sello aparta tu tablero, no a tu gente: se vive, se comercia, se reintenta. Lo que pierdes… es decidir sobre tu propio núcleo.",MementoMatchStoryBeatKind.Guardian),
                G(0,"No pienso sellar mi emblema junto al de una desconocida. Un duelo: si lees mi bosque mejor de lo que ella leyó tu mesa, te prestaré más que lástima.",MementoMatchStoryBeatKind.Challenge),
                N("Aki no apuesta territorio: apuesta ver cómo juega {PLAYER} cuando el bosque le devuelve una carta equivocada."),
                P("Pulso del Relevo: mientras encadene parejas, mi turno golpea de verdad. Es mi pasiva y sólo mía: nadie más la tiene. Vamos, Aki… y márcame mis errores.")),
            Scene(2,0,MementoMatchStoryPhase.Recruit,
                "ZONA 1 / 5 · BOSQUE VIVO","CONFIAR DESPACIO",
                N("El bosque registra cada acierto y cada titubeo. Aki guarda su mazo, mira el sello de Zona Cero y, por primera vez, no se marcha de inmediato."),
                P("No te pido tu bosque ni tu emblema. Te pido una compañera de ruta hacia la Liga… y una testigo de mi revancha."),
                G(0,"Antes de ir contigo: dime qué perdiste. No la zona. Qué perdiste tú.",MementoMatchStoryBeatKind.Recruit),
                P("Un minuto tardé en perder mi casa, y en ese minuto no me dejaron decir nada. …Perdí la última palabra sobre lo mío."),
                G(0,"…Bien. Iré. Y el día que tenga que elegir entre tu revancha y otra cosa, te lo diré antes de hacerlo.",MementoMatchStoryBeatKind.Recruit),
                N("El emblema del Bosque Vivo se inscribe junto al de Zona Cero. Su núcleo presta rutas y suministros al sello apagado: dos zonas respiran mejor que una."),
                G(0,"Mi Deshacer te devuelve la última jugada, acertada o no. Segundas oportunidades… procura no necesitarlas todas.",MementoMatchStoryBeatKind.Ability),
                P("Segunda parada: el Taller Coral. Para llegar a la campeona hacen falta rutas seguras y defensas de verdad… y Mika forja ambas."),
                N("Aki abre un sendero entre la maleza, rumbo al taller donde una inventora mide a las alianzas como quien mide fallos.")),
            Scene(3,1,MementoMatchStoryPhase.WorldIntro,
                "ZONA 2 / 5 · TALLER CORAL","CADA ALIADA, UNA VARIABLE",
                N("En el camino, dos alianzas se disputan un cruce de rutas a golpe de tablero. El mapa de la Liga cambia mientras {PLAYER} camina: lo que hoy es atajo ayer fue frontera."),
                N("El Taller Coral engrana las defensas del norte: sin sus escudos, una zona cae en dos asaltos. Por eso todas lo codician… y por eso Mika no abre la puerta."),
                P("Mika, no vengo a quedarme tu taller. Vengo a ofrecer coordinación… y a preguntar por una figura que juega tableros ajenos de memoria."),
                G(1,"Una alianza aumenta los vectores de fallo: más gente, más errores posibles. Lo tengo medido. …Todo lo tengo medido.",MementoMatchStoryBeatKind.Guardian),
                G(1,"¿Y si vuestra coordinación falla con mi taller en medio? Un fallo compartido también cae sobre mi techo.",MementoMatchStoryBeatKind.Guardian),
                P("Mídenos, entonces. No te pido fe: pide resultados y compruébalos tú misma, carta a carta.")),
            Scene(4,1,MementoMatchStoryPhase.Challenge,
                "ZONA 2 / 5 · TALLER CORAL","LO QUE LOS DATOS NO MIDEN",
                G(1,"No mediré discursos: mediré coordinación bajo carga. Cuando el escudo ceda, veré quién decide… y quién sólo obedece.",MementoMatchStoryBeatKind.Challenge),
                N("Mika exige una victoria limpia, sin atajos, antes de conectar una sola defensa del taller con el bosque."),
                P("Tus vectores de fallo son reales. Lo que no mides es lo que ganamos cubriéndonos los puntos débiles… eso tampoco se mide en solitario."),
                G(1,"Matriz cargada. Que hable el tablero.",MementoMatchStoryBeatKind.Challenge),
                P("Pulso del Relevo: si encadeno parejas, el golpe crece. Cronométrame, Mika."),
                P("Y si fallo una, el Deshacer de Aki me devuelve el combo anterior: la cadena retoma donde estaba y vuelve a golpear al daño máximo.")),
            Scene(5,1,MementoMatchStoryPhase.Recruit,
                "ZONA 2 / 5 · TALLER CORAL","EL COSTE DE CONTROLARLO TODO",
                N("Los engranajes se detienen. Mika relee el registro del duelo más tiempo del necesario, buscando el error que no ocurrió."),
                G(1,"Ya dirigí una alianza. Nadie me traicionó: la controlé tanto que mis compañeras dejaron de decidir. Cuando dudé yo, dudó todo… y cayó todo. Esa cifra no se repite.",MementoMatchStoryBeatKind.Recruit),
                P("Entonces no te pedimos dirección. Un puesto, no un trono: decide con nosotras… no por nosotras."),
                N("El Taller Coral conecta sus defensas al Bosque Vivo. Las rutas del norte se estabilizan y los asaltos a los cruces pierden interés."),
                G(1,"Mi Escudo de Combo conservará tu racha cuando un fallo fuera a romperla. Conservar rachas ya me costó caro una vez: úsala bien.",MementoMatchStoryBeatKind.Ability),
                P("Siguiente parada: el Cielo de Tinta. Alguien debe decirme si esa campeona ya había jugado otros tableros… además del mío."),
                N("Ruta calculada al milímetro… hasta que las estrellas la corrigen. Mika discute el margen de error con el cielo mismo.")),
            Scene(6,2,MementoMatchStoryPhase.WorldIntro,
                "ZONA 3 / 5 · CIELO DE TINTA","PREDECIR NO ES ELEGIR",
                N("Mientras Mika fortifica el bosque, sus sensores captan la misma firma que dejó la invasora: un emblema que el mapa de la Liga no registra en ninguna zona."),
                N("Sólo un observatorio lee firmas tan viejas: el Cielo de Tinta, donde Yoru cartografía el torneo entero con tinta y constelaciones."),
                P("Perdí mi tablero ante alguien que no miraba las cartas: las recordaba. Yoru… ¿tu cielo guarda registro de quién juega así?"),
                G(2,"Las estrellas ya trazan las rutas; leerlas es todo mi oficio. …Lo que no dicen es qué hacer cuando dos rutas se contradicen.",MementoMatchStoryBeatKind.Guardian),
                G(2,"Ese emblema pertenece al Archivo Cero: la sede del torneo. Lo he visto flotar donde no debería haber juego. …Una sede no debería sentarse a jugar.",MementoMatchStoryBeatKind.Guardian),
                S("CAMPEONA DESCONOCIDA","El taller y el bosque ya se cubren entre sí. Bien. …Yo tuve dos así, y no bastó.",5),
                N("La silueta del emblema sin zona observa desde el borde del mapa. No pide turno, no saluda, y cuando el equipo la mira ya no está."),
                P("Pues jugaremos contra la casa si hace falta. Antes, tu duelo: enséñame cómo se rompe una predicción.")),
            Scene(7,2,MementoMatchStoryPhase.Challenge,
                "ZONA 3 / 5 · CIELO DE TINTA","ROMPER LA PREDICCIÓN",
                G(2,"Mi Visión Estelar verá cada pareja que escondas antes que tú. Si tu juego puede más que mi cielo… te creeré libre.",MementoMatchStoryBeatKind.Challenge),
                N("Yoru apostará su mapa del torneo, con la entrada que ninguna zona reclama, a cambio de un duelo que no pueda pronosticar."),
                P("Las predicciones no existen para rendirse: existen para elegir mejor. Hoy elijo jugar tu duelo, Yoru."),
                G(2,"Constelación a la vista. Juega… y que el cielo se equivoque conmigo.",MementoMatchStoryBeatKind.Challenge),
                P("Pulso del Relevo: encadenar parejas también golpea. Sorpréndete, Yoru.")),
            Scene(8,2,MementoMatchStoryPhase.Recruit,
                "ZONA 3 / 5 · CIELO DE TINTA","LAS CINCO TÉCNICAS AJENAS",
                N("El cielo se equivocó una vez, exactamente donde había jurado que era imposible. Yoru anota el fallo con tinta dorada."),
                G(2,"Nunca me habían vencido donde ya lo había visto todo. …Me gusta. Me uno a vuestra alianza: alguien debe mirar lo que el mapa esconde.",MementoMatchStoryBeatKind.Recruit),
                N("Yoru despliega su mapa y fija la entrada sin zona. En el margen, una nota antigua: cinco técnicas de campeonas anteriores, copiadas y archivadas por la misma sede."),
                G(2,"Debajo hay una sexta línea, y no es tinta lo que falta: alguien raspó el nombre. Cinco técnicas se archivan. La sexta se guarda.",MementoMatchStoryBeatKind.Guardian),
                P("Apunta esa línea en tu mapa, Yoru. Si alguien borró un nombre, es que ese nombre juega."),
                G(2,"Mi Visión Estelar te mostrará una pareja real aún oculta. Ver no obliga a decidir… decidir te lo dejo a ti.",MementoMatchStoryBeatKind.Ability),
                P("El jardín: la entrada cruza el Jardín Errante, y los senderos de Hana sostienen más fronteras de las que ella admite."),
                N("Las estrellas se fijan sobre el jardín. Mika protesta por el ángulo de aproximación; Yoru responde con tres constelaciones y una sonrisa.")),
            Scene(9,3,MementoMatchStoryPhase.WorldIntro,
                "ZONA 4 / 5 · JARDÍN ERRANTE","QUIÉN ESCRIBIÓ LA REGLA",
                N("El Jardín Errante no descansa: una alianza rompe un puente al norte mientras, al sur, flores y azúcar se disputan un claro a golpe de tablero. Nadie pidió permiso al jardín."),
                N("Hana cuida cada sendero: si el jardín cae, cuatro zonas pierden sus rutas de suministro en una sola noche."),
                P("No venimos a cruzar el jardín y olvidarlo. Venimos a que llegue con nosotras… y a preguntar por la placa de la bifurcación: la regla del Deseo no tiene autora."),
                G(3,"Tengo que proteger las rutas. …Aunque nadie me lo pida, aunque nadie lo note, aunque el jardín entero dé la espalda.",MementoMatchStoryBeatKind.Guardian),
                G(3,"La placa es anterior a mis caminos. Quien escribió la regla no dejó nombre… y aun así todas la obedecemos. Descansa ese misterio: hoy importa el jardín.",MementoMatchStoryBeatKind.Guardian),
                P("Ganaremos sin romper un solo sendero. Y si la regla no tiene autora… la final la obligará a mostrar la cara.")),
            Scene(10,3,MementoMatchStoryPhase.Challenge,
                "ZONA 4 / 5 · JARDÍN ERRANTE","EL RITMO DEL JARDÍN",
                G(3,"El jardín sólo camina con quien respeta su compás. Desafío: sostened mi tempo sin pisar una sola raíz del tablero.",MementoMatchStoryBeatKind.Challenge),
                N("Hana registrará sendas compartidas sólo si el equipo protege el tablero durante el duelo entero… no sólo al final."),
                P("Cuidar el ritmo no es ir despacio: es saber cuándo florecer. Vamos."),
                G(3,"Raíces firmes, {PLAYER}. El jardín está mirando.",MementoMatchStoryBeatKind.Challenge),
                P("Pulso del Relevo: mis parejas encadenadas empujan conmigo. Ritmo respetado, Hana.")),
            Scene(11,3,MementoMatchStoryPhase.Recruit,
                "ZONA 4 / 5 · JARDÍN ERRANTE","FLORES Y AZÚCAR",
                N("Por primera vez en semanas, el jardín completa su ronda sin perder un sendero. Hana corta una flor, la guarda… y no explica por qué."),
                G(3,"Otra vez habría dicho que el jardín me obliga. Hoy digo la verdad: quiero ver el final del torneo, y quiero que el jardín llegue entero.",MementoMatchStoryBeatKind.Recruit),
                P("Plaza asegurada. Y una condición nuestra: no tienes que cuidar nuestras rutas, Hana… sólo caminar a nuestro lado."),
                N("Los senderos de Hana cosen cuatro zonas: bosque, taller, cielo y jardín respiran por la misma red de rutas."),
                G(3,"Mi Floración completará una pareja oculta y la hará florecer en pleno duelo: daño y vida en un mismo gesto.",MementoMatchStoryBeatKind.Ability),
                P("Sólo queda el sur: el Dulce Atelier y su guardiana solitaria. Y la placa… seguirá esperando una autora."),
                N("Hana hace crecer un puente de ramas hacia el atelier y lo cruza despacio, como quien teme lo que encontrará al otro lado.")),
            Scene(12,4,MementoMatchStoryPhase.WorldIntro,
                "ZONA 5 / 5 · DULCE ATELIER","LA TIENDA SIN EMBLEMA",
                N("El Dulce Atelier resiste otro asalto firmado sólo por Momo. En el mercado del borde, una vendedora atiende con normalidad… aunque su tenderete no cuelga emblema de zona."),
                P("Perdone, señora… su puesto no tiene emblema. ¿Desde qué zona comercia?"),
                N("La vendedora sonríe: perdió su zona anoche, en un asalto de tres turnos. El tenderete es suyo; el tablero era de la zona, la tienda es de ella."),
                N("Perder una zona no borra a nadie: retira la licencia de un tablero. Se vive, se comercia, se vuelve a desafiar… la vendedora ya pidió revancha para el viernes."),
                G(4,"¡Puedo ganar yo sola! ¿Emblemas? ¿Alianzas? Yo con mi atelier y mis turnos… ¡eso hago, eso hago!",MementoMatchStoryBeatKind.Guardian),
                G(4,"La del emblema sin zona me ofreció un asiento de espectadora para la final. Le dije que el espectáculo lo cocino yo… y lo sirvo ganando.",MementoMatchStoryBeatKind.Guardian),
                P("No vengo a quedarme tu atelier ni a salvarte: vengo a invitarte al tablero final. Primero, claro… hay que ganarte.")),
            Scene(13,4,MementoMatchStoryPhase.Challenge,
                "ZONA 5 / 5 · DULCE ATELIER","SOBREVIVIR NO ES GANAR",
                G(4,"¡La pista de la puerta sellada se gana en mi mesa: batid mi Encore! Yo sola hasta el final… ¡y con público!",MementoMatchStoryBeatKind.Challenge),
                N("Momo revelará cómo abrir la puerta del Archivo Cero sólo si el equipo aguanta su ritmo de superviviente: sin pausa y sin quemar el caramelo."),
                P("Tu zona está a salvo de nosotras. Pero tu sitio en la final no te lo regala nadie… así que duelo de verdad, Momo."),
                G(4,"¡Atelier a toda potencia! Receta de la casa: dulce, directa… e imposible de repetir.",MementoMatchStoryBeatKind.Challenge),
                P("Pulso del Relevo: cada pareja encadenada golpea más fuerte. Que empiece la función.")),
            Scene(14,4,MementoMatchStoryPhase.Recruit,
                "ZONA 5 / 5 · DULCE ATELIER","EL NOMBRE DE LA SILUETA",
                N("El azúcar cae rendido. Momo se apoya en la vitrina, mira cinco emblemas juntos y, por primera vez, no corre a reponer existencias."),
                G(4,"Me uno… con condiciones: mis turnos los decido yo. Y cobro por adelantado: la invasora es Rei, la Maestra que vive dentro del Archivo Cero.",MementoMatchStoryBeatKind.Recruit),
                P("Condiciones aceptadas: aquí nadie decide por ti. Bienvenida al escenario, Momo… faltaba justo el azúcar."),
                N("Cinco emblemas se sincronizan sobre el mapa: bosque, taller, cielo, jardín y atelier. Sólo queda una coordenada en blanco: la entrada de la sede."),
                G(4,"Mi Encore: tu próxima jugada no gasta turno ni cede el mando. Extra de función… cortesía de la casa.",MementoMatchStoryBeatKind.Ability),
                P("Cinco técnicas, cinco zonas aliadas y una sede que juega su propio torneo. Vamos a por la sexta: Zona Cero… y por el Deseo."),
                N("La entrada de la sede se abre sola, como si esperara la cita. Las seis cruzan el umbral con el mapa de Yoru en alto.")),
            Scene(15,5,MementoMatchStoryPhase.FinalBoss,
                "FINAL · ARCHIVO CERO","LA REGLA DEL DESEO",
                N("La silueta espera junto a una mesa de treinta cartas. Al encenderse las luces de la sede, por fin tiene rostro: Rei, Maestra del Archivo Cero."),
                G(5,"Compruebo que vuestra alianza sea estable antes de abrir el Deseo. …Comprobar. Es lo único que me dejaron mi equipo y mi torneo.",MementoMatchStoryBeatKind.Guardian),
                G(5,"Fui yo quien tocó tu campana, {PLAYER}. Te arrebaté Zona Cero en un minuto para verte elegir: rendirte… o construir. Elegiste construir.",MementoMatchStoryBeatKind.Guardian),
                G(5,"Y no te pedí permiso. Te quité la última palabra sobre tu casa para hacerme una pregunta a mí misma. Eso no lo arregla perder.",MementoMatchStoryBeatKind.Guardian),
                G(5,"Mi equipo llegó aquí con el Deseo a un turno de distancia y se traicionó en la última carta. Sellé la sede… y me quedé a comprobar si todas eran iguales.",MementoMatchStoryBeatKind.Guardian),
                N("Rei alza la mano y cinco técnicas giran a su alrededor: Deshacer, Escudo de Combo, Visión Estelar, Floración, Encore. La sede archiva copia de cada campeona que pasa."),
                P("Una sede que juega y colecciona técnicas. Dime una sola cosa, Rei: ¿quién escribió la regla del Deseo?"),
                N("Rei tarda en contestar. Cuando lo hace, no mira al tablero."),
                G(5,"Yo. La redacté para que el Deseo sólo pudiera ganarse como mi equipo nunca aceptó: poniéndolo en común. Seis zonas, una decisión… o nada.",MementoMatchStoryBeatKind.Guardian),
                G(5,"El examen es simple: seis zonas, un tablero, una sola decisión. Si dudan juntas… pierden juntas. Comiencen.",MementoMatchStoryBeatKind.Challenge),
                P("Pulso del Relevo, frente a la mesa de la sede: cada pareja encadenada llevará seis zonas dentro. ¡Reparte, Rei!"),
                N("El tablero de la sede se enciende: treinta cartas, cinco técnicas prestadas y seis emblemas jugando como uno. La última función del torneo comienza.")),
            Scene(16,5,MementoMatchStoryPhase.Epilogue,
                "EPÍLOGO · SEIS ZONAS","EL DESEO COMPARTIDO",
                N("Cuando la carta final se voltea, la sede queda en silencio. Rei baja la mano y las cinco técnicas prestadas vuelven a sus dueñas como pétalos devueltos."),
                G(5,"Estable. …Lo comprobé pagando el único precio que me quedaba: perder. La regla está cumplida; el Deseo os pertenece.",MementoMatchStoryBeatKind.Guardian),
                N("Con un Deshacer ya devuelto, Rei rompe su propio sello y entrega a {PLAYER} el núcleo de Zona Cero. El tablero vuelve a responder a su mano."),
                G(5,"Y propongo enmendar la regla que escribí: el Deseo no premiará a una zona, sino a seis que lo firmen juntas. Yo no firmo… la sede sólo pone el tablero.",MementoMatchStoryBeatKind.Guardian),
                G(0,"El Bosque Vivo firma. …Yo ya firmé una vez, antes de conoceros.",MementoMatchStoryBeatKind.Guardian),
                G(1,"Seis firmas y ningún trono: lo suscribo. Y conste que lo predije… a la segunda carta.",MementoMatchStoryBeatKind.Guardian),
                G(2,"El cielo no previó esto. …Me alegra haber estado equivocada.",MementoMatchStoryBeatKind.Guardian),
                G(3,"El jardín caminará con vosotras. Y esta vez… alguien me lo pidió.",MementoMatchStoryBeatKind.Guardian),
                G(4,"¡Firmo con glaseado! Y que conste: mi sitio en la final me lo gané a pulso.",MementoMatchStoryBeatKind.Guardian),
                P("No puedo prometer que nadie vuelva a perder. Sí puedo decidir qué hacemos después. …Seis zonas, una promesa."),
                N("Los seis emblemas se encienden en dorado sobre la mesa de la sede. En el mapa de la Liga, las rutas vuelven a moverse."),
                N("Zona Cero vuelve a sonar a hogar. El Archivo guarda la partida completa: cada pareja, cada duelo… y el deseo que ninguna ganó sola."),
                N("Cinco técnicas cruzaron la mesa de vuelta a sus dueñas. La sexta no se movió: la de Rei nunca estuvo sobre la mesa."),
                G(5,"La mía no vuelve. …Nunca volvió. La dejé en el Archivo el día que sellé la sede, y alguien la firmó por mí.",MementoMatchStoryBeatKind.Guardian),
                P("¿Quién firma por una campeona?"),
                G(5,"Eso llevo preguntándomelo desde antes de conocerte. …Recupera tu casa, {PLAYER}. Yo tengo una pregunta pendiente.",MementoMatchStoryBeatKind.Guardian))
        };

        private static MementoMatchCampaignLevel L(int id,int world,int node,int set,int opponent,
            int difficulty,int rows,int columns,int allowance,int mismatch,int combo,
            string title,string objective,bool boss=false) =>
            new MementoMatchCampaignLevel(id,world,node,set,opponent,difficulty,rows,columns,
                allowance,mismatch,combo,title,objective,boss);

        private static MementoMatchStoryScene Scene(int id,int world,MementoMatchStoryPhase phase,
            string kicker,string chapter,params MementoMatchStoryBeat[] beats) =>
            new MementoMatchStoryScene(id,world,phase,kicker,chapter,beats);

        private static MementoMatchStoryBeat N(string text) =>
            new MementoMatchStoryBeat("ESCENA",text,-1,MementoMatchStoryVoiceCue.None,MementoMatchStoryBeatKind.Narration);
        private static MementoMatchStoryBeat P(string text) =>
            new MementoMatchStoryBeat("TÚ · ASPIRANTE",text,-1,MementoMatchStoryVoiceCue.None,MementoMatchStoryBeatKind.Protagonist);
        private static MementoMatchStoryBeat G(int id,string text,MementoMatchStoryBeatKind kind) =>
            new MementoMatchStoryBeat(Names[id],text,id,MementoMatchStoryVoiceCue.None,kind);
        private static MementoMatchStoryBeat S(string speaker,string text,int id) =>
            new MementoMatchStoryBeat(speaker,text,id,MementoMatchStoryVoiceCue.None,MementoMatchStoryBeatKind.Guardian,true);

    }
}