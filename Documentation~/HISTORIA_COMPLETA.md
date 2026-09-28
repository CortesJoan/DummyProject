# Memento Match — HISTORIA COMPLETA

Documento **generado**, no escrito a mano. Se produce con
`Tools > Memento > Export complete story (referencia)` leyendo las fuentes
efectivas: `MementoMatchCampaignRules`, `MementoPostgameStory`,
`MementoPostgameEncounters` y el manifiesto `story_voice_script_v4.json`.
Para leer la historia como guion, usa `HISTORIA_GUION.md`.

- Versión de Unity: 6000.5.9f1
- Versión del juego: 1.17
- Generado: 2026-09-24 21:57 UTC

> Guion de doblaje japones v4 — historia reescrita «La Liga de las Seis Zonas» + postgame «La Puerta Interior». 89 lineas de guia; Custodias y Creador sin voz hasta tener identidad propia. Los subtitulos en espanol salen del texto del juego; el catalogo exige coincidencia exacta guide+es, y las lineas nuevas suenan en silencio hasta grabar. ids story_NN_MM: 00-16 campana, 17 apertura, 18-23 intros postgame, 24 victorias (MM=encuentro*10+beat), 25 derrotas, 26 checkpoint Rei, 27 epilogo, 90 reintentos.

## Campaña — tabla de niveles

| # | Mundo | Título | Objetivo | Tablero | Jefe | Oponente | Dificultad |
|---|---|---|---|---|---|---|---|
| 0 | 0 | Refugio entre hojas | Alcanza a Aki antes de la siguiente campana. | 2×3 | — | — | 0 |
| 1 | 0 | Sendero de ecos | Aprende cómo defiende el Bosque Vivo. | 4×4 | — | — | 1 |
| 2 | 0 | Frontera gemela | Abre una ruta segura hasta el núcleo. | 4×5 | — | — | 1 |
| 3 | 0 | Aki · Pacto del bosque | Vence a Aki y propón la primera alianza. | 4×4 | duelo | Aki | 0 |
| 4 | 1 | Costa de engranajes | Entra en la Zona Coral. | 3×4 | — | — | 0 |
| 5 | 1 | Relevo mecánico | Sincroniza las reliquias del taller. | 4×4 | — | — | 1 |
| 6 | 1 | Plan a contrarreloj | Demuestra que el equipo puede adaptarse. | 4×5 | — | — | 1 |
| 7 | 1 | Mika · Cálculo coral | Supera su defensa y gana su confianza. | 4×4 | duelo | Mika | 1 |
| 8 | 2 | Ruta de tinta | Sigue el mapa hacia el observatorio. | 3×4 | — | — | 0 |
| 9 | 2 | Órbita de parejas | Reconstruye la constelación de acceso. | 4×4 | — | — | 1 |
| 10 | 2 | Tramo sin luz | Avanza cuando desaparezcan las pistas. | 4×5 | — | — | 2 |
| 11 | 2 | Yoru · Juicio estelar | Convence a Yoru de que la alianza tiene futuro. | 4×5 | duelo | Yoru | 2 |
| 12 | 3 | Semillas fronterizas | Evita que el jardín sea absorbido. | 3×4 | — | — | 0 |
| 13 | 3 | Pétalos cruzados | Haz florecer el patrón oculto. | 4×4 | — | — | 1 |
| 14 | 3 | Invernadero vivo | Protege la racha mientras cambia el jardín. | 4×5 | — | — | 1 |
| 15 | 3 | Hana · Raíz compartida | Prueba que una alianza también puede proteger. | 4×4 | duelo | Hana | 1 |
| 16 | 4 | Frontera de azúcar | Llega al Atelier antes del cierre. | 3×4 | — | — | 0 |
| 17 | 4 | Receta en fuga | Mantén el ritmo ante las trampas. | 4×4 | — | — | 1 |
| 18 | 4 | Encore imposible | Resiste el último ensayo. | 4×5 | — | — | 2 |
| 19 | 4 | Momo · Dulce revancha | Descubre quién tomó la Zona Cero. | 4×5 | duelo | Momo | 2 |
| 20 | 5 | Rei · Examen del Archivo Cero | Vence a la Maestra Definitiva en un tablero de 30 cartas. | 5×6 | examen final | guía 5 | 2 |

## Postgame — tabla de encuentros

| # | Título | Compañera | Oponente | Tablero | Guionizado |
|---|---|---|---|---|---|
| 0 | El margen de error | Mika | Custodia del Cálculo | 4×4 | — |
| 1 | Una certeza elegida | Yoru | Custodia del Azar | 4×5 | — |
| 2 | Proteger sin encerrar | Hana | Custodia del Vínculo | 4×5 | — |
| 3 | Pedir ayuda no es perder | Momo | Custodia del Orgullo | 4×5 | — |
| 4 | La decisión de Rei | guía 5 | Creador | 5×6 | sí |
| 5 | Nuestra siguiente jugada | protagonista | Creador | 6×5 | — |

## Escenas

### Escena 0 — EL PRIMER MINUTO

- Fase: `WorldIntro` · Mundo: 0 · Kicker: «PRÓLOGO · ZONA CERO»

**1. ESCENA** (Narration, guía -1, voz None)

> El Archivo une tableros por núcleos: zonas que compiten, trueques entre campeonas y un solo Deseo. La campana de Zona Cero suena sin desafiante anunciada.

**2. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Me llaman {PLAYER}. Zona Cero es mi hogar y su tablero, mi primer recuerdo… Si nadie lo defiende esta noche, nadie lo defenderá.

**3. ESCENA** (Narration, guía -1, voz None)

> Sobre el tablero se proyecta un emblema que ninguna zona reclama. Una silueta cruza el umbral sin presentarse y sin pedir turno.

**4. CAMPEONA DESCONOCIDA** (Guardian, guía 5, voz None · silueta)

> Un hogar no se hereda: se sostiene. Y tú… aún no sabes cómo se sostiene.

- JA: 家とは、受け継ぐものではなく、支えるものです。そしてあなたは…まだ、その支え方を知りません。
- Dirección: Restrained and formal; even pace, let the ellipsis carry the verdict on 'tú'. · Estado: `generated_pending_listening_QA`

**5. ESCENA** (Narration, guía -1, voz None)

> El tablero de Zona Cero se despliega solo: treinta cartas, quince pares, toda la defensa de la zona en una sola mesa. El duelo abre.

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Voltea las parejas sin dudar. No mira las cartas… las reconoce, como si mi tablero fuera una partida que ella ya terminó.

**7. ESCENA** (Narration, guía -1, voz None)

> La silueta encadena pares sin pausa. Cada acierto se convierte en una lanza de luz que cruza la mesa directo al núcleo de la zona.

**8. ESCENA** (Narration, guía -1, voz None)

> Impacto. La primera grieta recorre el núcleo y las cartas de {PLAYER} se atenúan, como recuerdos que aún no sabe nombrar.

**9. ESCENA** (Narration, guía -1, voz None)

> La última pareja cae de la mano de la silueta. El núcleo se apaga en silencio, sereno… como si nada se hubiera roto de verdad.

**10. CAMPEONA DESCONOCIDA** (Guardian, guía 5, voz None · silueta)

> Zona Cero queda bajo sello. Mañana tu gente despertará igual… pero este tablero ya responde a otra mano.

- JA: ゼロゾーンを封印します。明日、あなたの人々は変わらぬまま目覚めるでしょう…けれど、この盤はもう、別の手に応えています。
- Dirección: Cold administrative calm at an even tempo; small pause at the ellipsis, no rise in volume. · Estado: `generated_pending_listening_QA`

**11. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Un minuto. Un minuto he tardado en entregar mi hogar. Nadie ha muerto, nada arde… y aun así todo pesa.

**12. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Que el Archivo reparta otra mano. No pienso quitarle el techo a nadie para recuperar el mío: buscaré a quien quiera jugar conmigo.


### Escena 1 — LA GUARDIANA QUE LLEGÓ TARDE

- Fase: `Challenge` · Mundo: 0 · Kicker: «ZONA 1 / 5 · BOSQUE VIVO»

**1. ESCENA** (Narration, guía -1, voz None)

> Apenas cura el sello, una guardiana cruza la puerta con paso de quien llegó tarde a algo importante. Aki, del Bosque Vivo, mira el núcleo apagado y luego a {PLAYER}.

**2. AKI** (Guardian, guía 0, voz None)

> Una sin zona sólo trae problemas: no sostiene rutas ni responde por un núcleo. …Debería darte la espalda y volver al bosque.

- JA: ゾーンのない人は、問題ばかり運んでくるの。ルートも支えられないし、核の責任も取れない。…あなたに背を向けて、森へ帰るべきなのに。
- Dirección: Wavering protector; the trailing ellipsis is self-reproach, not rejection. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Puedes irte. Pero respóndeme una sola cosa: ¿se puede volver a desafiar una zona sellada?

**4. AKI** (Guardian, guía 0, voz None)

> Con licencia de la Liga, sí. El sello aparta tu tablero, no a tu gente: se vive, se comercia, se reintenta. Lo que pierdes… es decidir sobre tu propio núcleo.

- JA: ええ、リーグの許可のもとで、よ。封印が遠ざけるのはあなたのボードで、あなたの暮らしではないの。暮らすことも、取引することも、やり直すこともできる。失うのは…自分の核を、自分で決めることだけ。
- Dirección: Patient explanation, kind but precise; brief pause before naming what is lost. · Estado: `generated_pending_listening_QA`

**5. AKI** (Challenge, guía 0, voz None)

> No pienso sellar mi emblema junto al de una desconocida. Un duelo: si lees mi bosque mejor de lo que ella leyó tu mesa, te prestaré más que lástima.

- JA: 見ず知らずの人のエンブレムの隣で、私のを封印する気にはなれないの。決闘よ。彼女があなたのテーブルを読んだよりもうまく、あなたが私の森を読めたら、哀れみ以上のものを貸してあげる。
- Dirección: Proud but fair; deliver the duel call warm, not hostile, and land the closing promise. · Estado: `generated_pending_listening_QA`

**6. ESCENA** (Narration, guía -1, voz None)

> Aki no apuesta territorio: apuesta ver cómo juega {PLAYER} cuando el bosque le devuelve una carta equivocada.

**7. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: mientras encadene parejas, mi turno golpea de verdad. Es mi pasiva y sólo mía: nadie más la tiene. Vamos, Aki… y márcame mis errores.


### Escena 2 — CONFIAR DESPACIO

- Fase: `Recruit` · Mundo: 0 · Kicker: «ZONA 1 / 5 · BOSQUE VIVO»

**1. ESCENA** (Narration, guía -1, voz None)

> El bosque registra cada acierto y cada titubeo. Aki guarda su mazo, mira el sello de Zona Cero y, por primera vez, no se marcha de inmediato.

**2. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No te pido tu bosque ni tu emblema. Te pido una compañera de ruta hacia la Liga… y una testigo de mi revancha.

**3. AKI** (Recruit, guía 0, voz None)

> Antes de ir contigo: dime qué perdiste. No la zona. Qué perdiste tú.

**4. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Un minuto tardé en perder mi casa, y en ese minuto no me dejaron decir nada. …Perdí la última palabra sobre lo mío.

**5. AKI** (Recruit, guía 0, voz None)

> …Bien. Iré. Y el día que tenga que elegir entre tu revancha y otra cosa, te lo diré antes de hacerlo.

**6. ESCENA** (Narration, guía -1, voz None)

> El emblema del Bosque Vivo se inscribe junto al de Zona Cero. Su núcleo presta rutas y suministros al sello apagado: dos zonas respiran mejor que una.

**7. AKI** (Ability, guía 0, voz None)

> Mi Deshacer te devuelve la última jugada, acertada o no. Segundas oportunidades… procura no necesitarlas todas.

- JA: 私の巻き戻しは、最後の一手をそのまま返すよ。当たりでも、外れでもね。二度目のチャンス…ぜんぶ使わずにすむように、がんばって。
- Dirección: Friendly tutorial warmth; light tease on the closing advice, no giggle. · Estado: `generated_pending_listening_QA`

**8. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Segunda parada: el Taller Coral. Para llegar a la campeona hacen falta rutas seguras y defensas de verdad… y Mika forja ambas.

**9. ESCENA** (Narration, guía -1, voz None)

> Aki abre un sendero entre la maleza, rumbo al taller donde una inventora mide a las alianzas como quien mide fallos.


### Escena 3 — CADA ALIADA, UNA VARIABLE

- Fase: `WorldIntro` · Mundo: 1 · Kicker: «ZONA 2 / 5 · TALLER CORAL»

**1. ESCENA** (Narration, guía -1, voz None)

> En el camino, dos alianzas se disputan un cruce de rutas a golpe de tablero. El mapa de la Liga cambia mientras {PLAYER} camina: lo que hoy es atajo ayer fue frontera.

**2. ESCENA** (Narration, guía -1, voz None)

> El Taller Coral engrana las defensas del norte: sin sus escudos, una zona cae en dos asaltos. Por eso todas lo codician… y por eso Mika no abre la puerta.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Mika, no vengo a quedarme tu taller. Vengo a ofrecer coordinación… y a preguntar por una figura que juega tableros ajenos de memoria.

**4. MIKA** (Guardian, guía 1, voz None)

> Una alianza aumenta los vectores de fallo: más gente, más errores posibles. Lo tengo medido. …Todo lo tengo medido.

- JA: 同盟は失敗のベクトルを増やす。人数が増えれば、起こりうる誤りも増える。私は計算ずみ。…全部、計算ずみなの。
- Dirección: Flat analytical delivery; let the repetition slip one notch quieter, human. · Estado: `generated_pending_listening_QA`

**5. MIKA** (Guardian, guía 1, voz None)

> ¿Y si vuestra coordinación falla con mi taller en medio? Un fallo compartido también cae sobre mi techo.

- JA: あなたたちの連携が、私の工房を巻き込んで失敗したら？共有された失敗は、私の屋根の上にも落ちてくる。
- Dirección: Sharp hypothetical question, then a dry matter-of-fact close. · Estado: `generated_pending_listening_QA`

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Mídenos, entonces. No te pido fe: pide resultados y compruébalos tú misma, carta a carta.


### Escena 4 — LO QUE LOS DATOS NO MIDEN

- Fase: `Challenge` · Mundo: 1 · Kicker: «ZONA 2 / 5 · TALLER CORAL»

**1. MIKA** (Challenge, guía 1, voz None)

> No mediré discursos: mediré coordinación bajo carga. Cuando el escudo ceda, veré quién decide… y quién sólo obedece.

- JA: 演説は測らない。測るのは、負荷下の連携よ。シールドが崩れた時、誰が決断して…誰がただ従うだけか。見せてもらうわ。
- Dirección: Clinical and level; weigh 'decides' and 'obeys' equally, without scorn. · Estado: `generated_pending_listening_QA`

**2. ESCENA** (Narration, guía -1, voz None)

> Mika exige una victoria limpia, sin atajos, antes de conectar una sola defensa del taller con el bosque.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Tus vectores de fallo son reales. Lo que no mides es lo que ganamos cubriéndonos los puntos débiles… eso tampoco se mide en solitario.

**4. MIKA** (Challenge, guía 1, voz None)

> Matriz cargada. Que hable el tablero.

- JA: マトリクス、充填完了。あとは盤に語らせる。
- Dirección: Brisk two-beat call; clipped consonants, confident stop. · Estado: `generated_pending_listening_QA`

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: si encadeno parejas, el golpe crece. Cronométrame, Mika.

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Y si fallo una, el Deshacer de Aki me devuelve el combo anterior: la cadena retoma donde estaba y vuelve a golpear al daño máximo.


### Escena 5 — EL COSTE DE CONTROLARLO TODO

- Fase: `Recruit` · Mundo: 1 · Kicker: «ZONA 2 / 5 · TALLER CORAL»

**1. ESCENA** (Narration, guía -1, voz None)

> Los engranajes se detienen. Mika relee el registro del duelo más tiempo del necesario, buscando el error que no ocurrió.

**2. MIKA** (Recruit, guía 1, voz None)

> Ya dirigí una alianza. Nadie me traicionó: la controlé tanto que mis compañeras dejaron de decidir. Cuando dudé yo, dudó todo… y cayó todo. Esa cifra no se repite.

- JA: 私も同盟を率いたことがある。誰も私を裏切らなかった。制御しすぎて、仲間が自分で決めることをやめたの。私が迷えば、全部が迷って…全部が落ちた。あの数値は、二度と繰り返さない。
- Dirección: Controlled confession; stay flat until everything 'falls', then clamp down hard. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Entonces no te pedimos dirección. Un puesto, no un trono: decide con nosotras… no por nosotras.

**4. ESCENA** (Narration, guía -1, voz None)

> El Taller Coral conecta sus defensas al Bosque Vivo. Las rutas del norte se estabilizan y los asaltos a los cruces pierden interés.

**5. MIKA** (Ability, guía 1, voz None)

> Mi Escudo de Combo conservará tu racha cuando un fallo fuera a romperla. Conservar rachas ya me costó caro una vez: úsala bien.

- JA: 私のコンボシールドは、ミスでコンボが切れそうになった時、あなたの流れを守るわ。流れを守ることには、一度だけ高い代償がついた。賢く使って。
- Dirección: Clear skill briefing; darken slightly on the past cost, end on dry trust. · Estado: `generated_pending_listening_QA`

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Siguiente parada: el Cielo de Tinta. Alguien debe decirme si esa campeona ya había jugado otros tableros… además del mío.

**7. ESCENA** (Narration, guía -1, voz None)

> Ruta calculada al milímetro… hasta que las estrellas la corrigen. Mika discute el margen de error con el cielo mismo.


### Escena 6 — PREDECIR NO ES ELEGIR

- Fase: `WorldIntro` · Mundo: 2 · Kicker: «ZONA 3 / 5 · CIELO DE TINTA»

**1. ESCENA** (Narration, guía -1, voz None)

> Mientras Mika fortifica el bosque, sus sensores captan la misma firma que dejó la invasora: un emblema que el mapa de la Liga no registra en ninguna zona.

**2. ESCENA** (Narration, guía -1, voz None)

> Sólo un observatorio lee firmas tan viejas: el Cielo de Tinta, donde Yoru cartografía el torneo entero con tinta y constelaciones.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Perdí mi tablero ante alguien que no miraba las cartas: las recordaba. Yoru… ¿tu cielo guarda registro de quién juega así?

**4. YORU** (Guardian, guía 2, voz None)

> Las estrellas ya trazan las rutas; leerlas es todo mi oficio. …Lo que no dicen es qué hacer cuando dos rutas se contradicen.

- JA: 星はもう、ルートを描いているわ。それを読むのが、私の仕事のすべて。…ただ、ふたつのルートが矛盾した時どうすればいいか、星は教えてくれないの。
- Dirección: Serene and flowing; hold the ellipsis, then soften like a private confession. · Estado: `generated_pending_listening_QA`

**5. YORU** (Guardian, guía 2, voz None)

> Ese emblema pertenece al Archivo Cero: la sede del torneo. Lo he visto flotar donde no debería haber juego. …Una sede no debería sentarse a jugar.

- JA: あのエンブレムは、ゼロ・アーカイブのもの。大会の本陣の紋よ。遊びなど存在しないはずの場所に、浮いているのを見たわ。…本陣が、プレイの席に着くだなんて。
- Dirección: Elegant unease; slow slightly on the final anomaly, keep volume low. · Estado: `generated_pending_listening_QA`

**6. CAMPEONA DESCONOCIDA** (Guardian, guía 5, voz None · silueta)

> El taller y el bosque ya se cubren entre sí. Bien. …Yo tuve dos así, y no bastó.

**7. ESCENA** (Narration, guía -1, voz None)

> La silueta del emblema sin zona observa desde el borde del mapa. No pide turno, no saluda, y cuando el equipo la mira ya no está.

**8. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pues jugaremos contra la casa si hace falta. Antes, tu duelo: enséñame cómo se rompe una predicción.

**9. MIKA** (Guardian, guía 1, voz None)

> Constelaciones no sostienen defensas… aunque esa coordenada que marcó anoche encaja con mi matriz. No lo tomen como elogio.

- JA: 星座が防御を支えることはないわ…ただ、昨夜示されたあの座標は、私のマトリクスと一致した。褒め言葉として受け取らないで。
- Dirección: Grudging precision; quick tempo, minimal pause, deadpan final line. · Estado: `generated_pending_listening_QA`

**10. ESCENA** (Narration, guía -1, voz None)

> Mika refuerza el cruce norte por su cuenta, sin consultar el reparto de rutas. El bosque queda fuera de la red hasta la mañana siguiente.

**11. YORU** (Guardian, guía 2, voz None)

> Decidiste por cuatro zonas, inventora. Otra vez. …Nadie te quitó el mando: lo soltaste tú.

**12. MIKA** (Guardian, guía 1, voz None)

> …Lo sé. Lo hice igual. Apunta el cruce: lo repongo yo, y esta vez lo reparto antes de reforzarlo.


### Escena 7 — ROMPER LA PREDICCIÓN

- Fase: `Challenge` · Mundo: 2 · Kicker: «ZONA 3 / 5 · CIELO DE TINTA»

**1. YORU** (Challenge, guía 2, voz None)

> Mi Visión Estelar verá cada pareja que escondas antes que tú. Si tu juego puede más que mi cielo… te creeré libre.

- JA: 私のスターヴィジョンは、あなたが隠すペアを、すべてあなたより先に見るわ。もしあなたのプレイが私の空より強かったら…あなたは自由だと、信じてあげる。
- Dirección: Calm challenge with wonder underneath; lift gently on 'my sky'. · Estado: `generated_pending_listening_QA`

**2. ESCENA** (Narration, guía -1, voz None)

> Yoru apostará su mapa del torneo, con la entrada que ninguna zona reclama, a cambio de un duelo que no pueda pronosticar.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Las predicciones no existen para rendirse: existen para elegir mejor. Hoy elijo jugar tu duelo, Yoru.

**4. YORU** (Challenge, guía 2, voz None)

> Constelación a la vista. Juega… y que el cielo se equivoque conmigo.

- JA: 星座が見えたわ。プレイして…そして、空がこの私を見誤りますように。
- Dirección: Duel-start invitation; hushed reverence on the final wish. · Estado: `generated_pending_listening_QA`

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: encadenar parejas también golpea. Sorpréndete, Yoru.


### Escena 8 — LAS CINCO TÉCNICAS AJENAS

- Fase: `Recruit` · Mundo: 2 · Kicker: «ZONA 3 / 5 · CIELO DE TINTA»

**1. ESCENA** (Narration, guía -1, voz None)

> El cielo se equivocó una vez, exactamente donde había jurado que era imposible. Yoru anota el fallo con tinta dorada.

**2. YORU** (Recruit, guía 2, voz None)

> Nunca me habían vencido donde ya lo había visto todo. …Me gusta. Me uno a vuestra alianza: alguien debe mirar lo que el mapa esconde.

- JA: すべてを見通せたはずの場所で、負けたことは一度もなかった。…悪くないわ。あなたたちの同盟に加わる。地図が隠しているものを見る人が、必要だから。
- Dirección: Restrained delight; join as a quiet vow, not applause. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Yoru despliega su mapa y fija la entrada sin zona. En el margen, una nota antigua: cinco técnicas de campeonas anteriores, copiadas y archivadas por la misma sede.

**4. YORU** (Guardian, guía 2, voz None)

> Debajo hay una sexta línea, y no es tinta lo que falta: alguien raspó el nombre. Cinco técnicas se archivan. La sexta se guarda.

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Apunta esa línea en tu mapa, Yoru. Si alguien borró un nombre, es que ese nombre juega.

**6. YORU** (Ability, guía 2, voz None)

> Mi Visión Estelar te mostrará una pareja real aún oculta. Ver no obliga a decidir… decidir te lo dejo a ti.

- JA: 私のスターヴィジョンは、まだ隠れている本物のペアをひとつ、あなたに見せるわ。見ることは決めることではないの…決めるのは、あなたに任せる。
- Dirección: Gentle skill intro; space the words for 'see' and 'decide' apart cleanly. · Estado: `generated_pending_listening_QA`

**7. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> El jardín: la entrada cruza el Jardín Errante, y los senderos de Hana sostienen más fronteras de las que ella admite.

**8. MIKA** (Guardian, guía 1, voz None)

> Tu ruta oriental seguía abierta esta mañana. Lo comprobé dos veces… no lo tomes como disculpa.

- JA: 東側のルートは、今朝も開いていた。二度、確認済み。…謝罪だと思わないで。
- Dirección: Terse accountability; the ellipsis is pride, keep it dry. · Estado: `generated_pending_listening_QA`

**9. YORU** (Guardian, guía 2, voz None)

> Lo tomo como una coordenada, Mika. …Y una coordenada a tiempo también es cariño.

- JA: そう受け取っておくわ、ミカ。座標としてね。…そして、間に合った座標も、また優しさなのよ。
- Dirección: Soft smile in the tone; warm the exchange at the sisterly close. · Estado: `generated_pending_listening_QA`

**10. ESCENA** (Narration, guía -1, voz None)

> Las estrellas se fijan sobre el jardín. Mika protesta por el ángulo de aproximación; Yoru responde con tres constelaciones y una sonrisa.


### Escena 9 — QUIÉN ESCRIBIÓ LA REGLA

- Fase: `WorldIntro` · Mundo: 3 · Kicker: «ZONA 4 / 5 · JARDÍN ERRANTE»

**1. ESCENA** (Narration, guía -1, voz None)

> El Jardín Errante no descansa: una alianza rompe un puente al norte mientras, al sur, flores y azúcar se disputan un claro a golpe de tablero. Nadie pidió permiso al jardín.

**2. ESCENA** (Narration, guía -1, voz None)

> Hana cuida cada sendero: si el jardín cae, cuatro zonas pierden sus rutas de suministro en una sola noche.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No venimos a cruzar el jardín y olvidarlo. Venimos a que llegue con nosotras… y a preguntar por la placa de la bifurcación: la regla del Deseo no tiene autora.

**4. HANA** (Guardian, guía 3, voz None)

> Tengo que proteger las rutas. …Aunque nadie me lo pida, aunque nadie lo note, aunque el jardín entero dé la espalda.

- JA: ルートを守らなければならないの。…誰にも頼まれなくても、誰にも気づかれなくても、庭じゅうに背を向けられても。
- Dirección: Soft voice with an iron core; let each 'even if' land firmer than the last. · Estado: `generated_pending_listening_QA`

**5. HANA** (Guardian, guía 3, voz None)

> La placa es anterior a mis caminos. Quien escribió la regla no dejó nombre… y aun así todas la obedecemos. Descansa ese misterio: hoy importa el jardín.

- JA: あの標識は、私の道よりも古いの。規則を書いた人は、名前を残さなかった…それでも、みんなそれに従っている。その謎は、ひとまず休ませておきましょう。今日、大切なのは庭だから。
- Dirección: Reverent hush on the nameless rule, then a warm redirect to the garden. · Estado: `generated_pending_listening_QA`

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Ganaremos sin romper un solo sendero. Y si la regla no tiene autora… la final la obligará a mostrar la cara.


### Escena 10 — EL RITMO DEL JARDÍN

- Fase: `Challenge` · Mundo: 3 · Kicker: «ZONA 4 / 5 · JARDÍN ERRANTE»

**1. HANA** (Challenge, guía 3, voz None)

> El jardín sólo camina con quien respeta su compás. Desafío: sostened mi tempo sin pisar una sola raíz del tablero.

- JA: 庭は、その拍子を敬う人とだけ、共に歩むの。挑戦よ。ボードの根をひとつも踏まずに、私のテンポを保ち続けて。
- Dirección: Ritual invitation; brighten cleanly on the word for 'challenge'. · Estado: `generated_pending_listening_QA`

**2. ESCENA** (Narration, guía -1, voz None)

> Hana registrará sendas compartidas sólo si el equipo protege el tablero durante el duelo entero… no sólo al final.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Cuidar el ritmo no es ir despacio: es saber cuándo florecer. Vamos.

**4. HANA** (Challenge, guía 3, voz None)

> Raíces firmes, {PLAYER}. El jardín está mirando.

- JA: しっかり根を張って、{PLAYER}。庭が見ているわよ。
- Dirección: Encouraging near-whisper; short, planted final beat. · Estado: `generated_pending_listening_QA`

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: mis parejas encadenadas empujan conmigo. Ritmo respetado, Hana.


### Escena 11 — FLORES Y AZÚCAR

- Fase: `Recruit` · Mundo: 3 · Kicker: «ZONA 4 / 5 · JARDÍN ERRANTE»

**1. ESCENA** (Narration, guía -1, voz None)

> Por primera vez en semanas, el jardín completa su ronda sin perder un sendero. Hana corta una flor, la guarda… y no explica por qué.

**2. HANA** (Recruit, guía 3, voz None)

> Otra vez habría dicho que el jardín me obliga. Hoy digo la verdad: quiero ver el final del torneo, y quiero que el jardín llegue entero.

- JA: 昔の私なら、庭に強いられたと言ったでしょう。今日は、本当のことを言うの。大会の結末を、見たい。そして、庭が無傷のまま最後までたどり着くことを、望んでいる。
- Dirección: Quiet honesty blooming; a steady rise across the twin wishes. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Plaza asegurada. Y una condición nuestra: no tienes que cuidar nuestras rutas, Hana… sólo caminar a nuestro lado.

**4. ESCENA** (Narration, guía -1, voz None)

> Los senderos de Hana cosen cuatro zonas: bosque, taller, cielo y jardín respiran por la misma red de rutas.

**5. HANA** (Ability, guía 3, voz None)

> Mi Floración completará una pareja oculta y la hará florecer en pleno duelo: daño y vida en un mismo gesto.

- JA: 私の開花は、隠れたペアを完成させて、決闘のまっただ中で咲かせるの。ダメージも、いのちも、ひとつのしぐさで。
- Dirección: Serene skill reveal; bloom on the paired final clause. · Estado: `generated_pending_listening_QA`

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Sólo queda el sur: el Dulce Atelier y su guardiana solitaria. Y la placa… seguirá esperando una autora.

**7. HANA** (Guardian, guía 3, voz None)

> Antes de cruzar, debéis saberlo: Momo, la guardiana del atelier… es mi hermana menor.

- JA: ここから先へ進む前に、知っておいてほしいの。モモ、アトリエの守り手…私の、妹です。
- Dirección: Gentle disclosure with a protective undertone; pause fully before the reveal. · Estado: `generated_pending_listening_QA`

**8. ESCENA** (Narration, guía -1, voz None)

> Hana hace crecer un puente de ramas hacia el atelier y lo cruza despacio, como quien teme lo que encontrará al otro lado.


### Escena 12 — LA TIENDA SIN EMBLEMA

- Fase: `WorldIntro` · Mundo: 4 · Kicker: «ZONA 5 / 5 · DULCE ATELIER»

**1. ESCENA** (Narration, guía -1, voz None)

> El Dulce Atelier resiste otro asalto firmado sólo por Momo. En el mercado del borde, una vendedora atiende con normalidad… aunque su tenderete no cuelga emblema de zona.

**2. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Perdone, señora… su puesto no tiene emblema. ¿Desde qué zona comercia?

**3. ESCENA** (Narration, guía -1, voz None)

> La vendedora sonríe: perdió su zona anoche, en un asalto de tres turnos. El tenderete es suyo; el tablero era de la zona, la tienda es de ella.

**4. ESCENA** (Narration, guía -1, voz None)

> Perder una zona no borra a nadie: retira la licencia de un tablero. Se vive, se comercia, se vuelve a desafiar… la vendedora ya pidió revancha para el viernes.

**5. MOMO** (Guardian, guía 4, voz None)

> ¡Puedo ganar yo sola! ¿Emblemas? ¿Alianzas? Yo con mi atelier y mis turnos… ¡eso hago, eso hago!

- JA: モモ、ひとりでも勝てるもん！エンブレム？同盟？モモはアトリエと、自分のターンで…やるんだよ、やるんだよっ！
- Dirección: High bouncy energy; stomp the doubled refrain at the end. · Estado: `generated_pending_listening_QA`

**6. MOMO** (Guardian, guía 4, voz None)

> La del emblema sin zona me ofreció un asiento de espectadora para la final. Le dije que el espectáculo lo cocino yo… y lo sirvo ganando.

- JA: ゾーンのないエンブレムの人がね、決勝の観客席をくれたの。モモは言ってやった。ショーはモモが料理するって…そして、勝って出すんだって！
- Dirección: Cheeky retelling; smirk through 'I cook the show', finish proud. · Estado: `generated_pending_listening_QA`

**7. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No vengo a quedarme tu atelier ni a salvarte: vengo a invitarte al tablero final. Primero, claro… hay que ganarte.

**8. HANA** (Guardian, guía 3, voz None)

> Momo. …Tus vitrinas siguen impecables. Y el claro del sur sigue sin arraigo: tu ronda lo cruza y no lo cuida.

**9. MOMO** (Guardian, guía 4, voz None)

> Y tus flores siguen llegando sin avisar. …Cuánto tiempo, hermana.

- JA: ハナのお花は、今も前触れなく届いてくるんだね。…久しぶり、お姉ちゃん。
- Dirección: Energy drops to small and honest; a true reunion beat, no bounce. · Estado: `generated_pending_listening_QA`

**10. MOMO** (Guardian, guía 4, voz None)

> Cuido mi atelier, no tus senderos. Si el jardín quiere el claro, que lo pida con un tablero y no con una carta con flores dentro.

**11. HANA** (Guardian, guía 3, voz None)

> Una carta con flores dentro es todo lo que mandé en dos años. Y no pedí nada a cambio.

**12. ESCENA** (Narration, guía -1, voz None)

> Ninguna de las dos cruza el claro. Hana recoge su maceta y Momo vuelve a la vitrina: el sur queda sin repartir, y las dos lo saben.


### Escena 13 — SOBREVIVIR NO ES GANAR

- Fase: `Challenge` · Mundo: 4 · Kicker: «ZONA 5 / 5 · DULCE ATELIER»

**1. MOMO** (Challenge, guía 4, voz None)

> ¡La pista de la puerta sellada se gana en mi mesa: batid mi Encore! Yo sola hasta el final… ¡y con público!

- JA: 閉ざされた扉の手がかりは、モモのテーブルで勝ち取るの！さあ、モモのアンコールを倒してみて！最後までひとりで…しかも、お客さんつき！
- Dirección: Show-opener shout; crisp crowd-facing rhythm, land the final flourish. · Estado: `generated_pending_listening_QA`

**2. ESCENA** (Narration, guía -1, voz None)

> Momo revelará cómo abrir la puerta del Archivo Cero sólo si el equipo aguanta su ritmo de superviviente: sin pausa y sin quemar el caramelo.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Tu zona está a salvo de nosotras. Pero tu sitio en la final no te lo regala nadie… así que duelo de verdad, Momo.

**4. MOMO** (Challenge, guía 4, voz None)

> ¡Atelier a toda potencia! Receta de la casa: dulce, directa… e imposible de repetir.

- JA: アトリエ、全力稼働！お店自慢のレシピは、あまくて、まっすぐで…二度と真似できないやつ！
- Dirección: Proud chef call; punch the three adjectives in clean rhythm. · Estado: `generated_pending_listening_QA`

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: cada pareja encadenada golpea más fuerte. Que empiece la función.


### Escena 14 — EL NOMBRE DE LA SILUETA

- Fase: `Recruit` · Mundo: 4 · Kicker: «ZONA 5 / 5 · DULCE ATELIER»

**1. ESCENA** (Narration, guía -1, voz None)

> El azúcar cae rendido. Momo se apoya en la vitrina, mira cinco emblemas juntos y, por primera vez, no corre a reponer existencias.

**2. MOMO** (Recruit, guía 4, voz None)

> Me uno… con condiciones: mis turnos los decido yo. Y cobro por adelantado: la invasora es Rei, la Maestra que vive dentro del Archivo Cero.

- JA: モモも入るよ…条件つきで！モモのターンは、モモが決めるの。それと、前払いももらうからね。侵入者はレイ。ゼロ・アーカイブの中に住んでいるマエストラだよ！
- Dirección: Bouncy negotiation; snap to crisp clarity for the Rei intel drop. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Condiciones aceptadas: aquí nadie decide por ti. Bienvenida al escenario, Momo… faltaba justo el azúcar.

**4. ESCENA** (Narration, guía -1, voz None)

> Cinco emblemas se sincronizan sobre el mapa: bosque, taller, cielo, jardín y atelier. Sólo queda una coordenada en blanco: la entrada de la sede.

**5. MOMO** (Ability, guía 4, voz None)

> Mi Encore: tu próxima jugada no gasta turno ni cede el mando. Extra de función… cortesía de la casa.

- JA: モモのアンコール！次の一手は、ターンも使わないし、手番も渡さないの。おまけのショータイム…お店からのサービスだよ！
- Dirección: Skill pitch with shopkeeper flair; wink on the closing courtesy. · Estado: `generated_pending_listening_QA`

**6. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Cinco técnicas, cinco zonas aliadas y una sede que juega su propio torneo. Vamos a por la sexta: Zona Cero… y por el Deseo.

**7. MOMO** (Guardian, guía 4, voz None)

> Iré… si nadie me saca del tablero para protegerme. La función es mía hasta el final.

- JA: 行くよ…心配だからって、ボードから下ろさないでね。ショーは最後まで、モモのものなの。
- Dirección: One flicker of vulnerability, then full stage confidence. · Estado: `generated_pending_listening_QA`

**8. HANA** (Guardian, guía 3, voz None)

> Nadie te quita nada, Momo. Hoy cuido al público… y dejo el escenario a las artistas.

- JA: 何も奪われないわ、モモ。今日は私が観客を見守る…舞台は、出演者たちに譲るの。
- Dirección: Big-sister steadiness; a generous final handoff, no sadness. · Estado: `generated_pending_listening_QA`

**9. HANA** (Guardian, guía 3, voz None)

> Y te pido una cosa, no te la concedo: el claro del sur. Repártelo tú, a tu ritmo, y firma con tu nombre.

**10. MOMO** (Guardian, guía 4, voz None)

> …El claro. Lo reparto yo, sí. Y la primera ronda de la mesa la pago con glaseado, que conste en acta.

**11. ESCENA** (Narration, guía -1, voz None)

> Momo cuelga por fin un emblema en el claro: el suyo, no el del atelier. Hana no lo toca. Sólo lo mira, y sigue caminando.

**12. ESCENA** (Narration, guía -1, voz None)

> La entrada de la sede se abre sola, como si esperara la cita. Las seis cruzan el umbral con el mapa de Yoru en alto.


### Escena 15 — LA REGLA DEL DESEO

- Fase: `FinalBoss` · Mundo: 5 · Kicker: «FINAL · ARCHIVO CERO»

**1. ESCENA** (Narration, guía -1, voz None)

> La silueta espera junto a una mesa de treinta cartas. Al encenderse las luces de la sede, por fin tiene rostro: Rei, Maestra del Archivo Cero.

**2. REI** (Guardian, guía 5, voz None)

> Compruebo que vuestra alianza sea estable antes de abrir el Deseo. …Comprobar. Es lo único que me dejaron mi equipo y mi torneo.

- JA: 願いを開く前に、あなたたちの同盟が安定しているかを確認します。…確認。私のチームと、私の大会が、私に残してくれたのは、それだけですから。
- Dirección: Clipped formal duty; repeat 'confirm' hollow, quieter than the first. · Estado: `generated_pending_listening_QA`

**3. REI** (Guardian, guía 5, voz None)

> Fui yo quien tocó tu campana, {PLAYER}. Te arrebaté Zona Cero en un minuto para verte elegir: rendirte… o construir. Elegiste construir.

- JA: あなたの鐘を鳴らしたのは、私です、{PLAYER}。あなたが選ぶところを見るために、一分でゼロゾーンを奪った。降参するか…築き上げるか。あなたは、築く方を選んだ。
- Dirección: Confession without apology; steady clockwork pacing, weight on the final choice. · Estado: `generated_pending_listening_QA`

**4. REI** (Guardian, guía 5, voz None)

> Y no te pedí permiso. Te quité la última palabra sobre tu casa para hacerme una pregunta a mí misma. Eso no lo arregla perder.

**5. REI** (Guardian, guía 5, voz None)

> Mi equipo llegó aquí con el Deseo a un turno de distancia y se traicionó en la última carta. Sellé la sede… y me quedé a comprobar si todas eran iguales.

- JA: 私のチームはここまで、願いひとターン手前まで来て、最後の一枚でお互いを裏切った。私は本陣を封印した…そして、全員が同じなのかを確かめるために、残ったのです。
- Dirección: Grief pressed flat under formality; the ellipsis holds a long night, do not dramatize. · Estado: `generated_pending_listening_QA`

**6. ESCENA** (Narration, guía -1, voz None)

> Rei alza la mano y cinco técnicas giran a su alrededor: Deshacer, Escudo de Combo, Visión Estelar, Floración, Encore. La sede archiva copia de cada campeona que pasa.

**7. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Una sede que juega y colecciona técnicas. Dime una sola cosa, Rei: ¿quién escribió la regla del Deseo?

**8. ESCENA** (Narration, guía -1, voz None)

> Rei tarda en contestar. Cuando lo hace, no mira al tablero.

**9. REI** (Guardian, guía 5, voz None)

> Yo. La redacté para que el Deseo sólo pudiera ganarse como mi equipo nunca aceptó: poniéndolo en común. Seis zonas, una decisión… o nada.

- JA: 私です。願いは、私のチームが決して受け入れなかった方法でしか勝てないように、私は条文を書きました。分かち合うことでしか。六つのゾーン、ひとつの決断…さもなくば、無。
- Dirección: The answer to 'who wrote the rule'; austere, every term enunciated, final beat cold. · Estado: `generated_pending_listening_QA`

**10. REI** (Challenge, guía 5, voz None)

> El examen es simple: seis zonas, un tablero, una sola decisión. Si dudan juntas… pierden juntas. Comiencen.

- JA: 試験は単純です。六つのゾーン、ひとつの盤、ただひとつの決断。共に迷えば…共に負けます。では、始めてください。
- Dirección: Examiner's decree; imperial calm, then a clean cut on 'begin'. · Estado: `generated_pending_listening_QA`

**11. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo, frente a la mesa de la sede: cada pareja encadenada llevará seis zonas dentro. ¡Reparte, Rei!

**12. MIKA** (Guardian, guía 1, voz None)

> Apertura calculada: quien encadena marca el ritmo y yo aseguro la racha. Sin órdenes… por fin.

- JA: 計算ずみのオープニングよ。連鎖する側がテンポを作って、私が流れを守る。命令なしで…ようやくね。
- Dirección: Tight satisfaction; the first genuinely easy breath of the campaign. · Estado: `generated_pending_listening_QA`

**13. YORU** (Guardian, guía 2, voz None)

> Y si el cálculo falla, miraré las estrellas. Elegir… ya sabes elegir tú.

- JA: 計算が外れたら、星を見ます。選ぶことは……あなたなら、もう知っているはずです。
- Dirección: Serene, unhurried; small thoughtful pause at the ellipsis, last phrase offered like a gift. · Estado: `generated_pending_listening_QA`

**14. ESCENA** (Narration, guía -1, voz None)

> El tablero de la sede se enciende: treinta cartas, cinco técnicas prestadas y seis emblemas jugando como uno. La última función del torneo comienza.


### Escena 16 — EL DESEO COMPARTIDO

- Fase: `Epilogue` · Mundo: 5 · Kicker: «EPÍLOGO · SEIS ZONAS»

**1. ESCENA** (Narration, guía -1, voz None)

> Cuando la carta final se voltea, la sede queda en silencio. Rei baja la mano y las cinco técnicas prestadas vuelven a sus dueñas como pétalos devueltos.

**2. REI** (Guardian, guía 5, voz None)

> Estable. …Lo comprobé pagando el único precio que me quedaba: perder. La regla está cumplida; el Deseo os pertenece.

- JA: 安定しています。……残された唯一の代価を払って、確かめました。敗北という、代価を。規則は果たされました。願いは、あなたたちのものです。
- Dirección: Contained, formal; a beat of silence after 'Estable', report the price without self-pity. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Con un Deshacer ya devuelto, Rei rompe su propio sello y entrega a {PLAYER} el núcleo de Zona Cero. El tablero vuelve a responder a su mano.

**4. REI** (Guardian, guía 5, voz None)

> Y propongo enmendar la regla que escribí: el Deseo no premiará a una zona, sino a seis que lo firmen juntas. Yo no firmo… la sede sólo pone el tablero.

- JA: そして、私の書いた規則を改めることを提案します。願いが報いるのは、ひとつのゾーンではなく、共に署名する六つのゾーンであるように。私は署名しません……主催は、盤を並べるだけです。
- Dirección: Measured proposal, one clause per breath; the ellipsis is a controlled pause, not hesitation. · Estado: `generated_pending_listening_QA`

**5. AKI** (Guardian, guía 0, voz None)

> El Bosque Vivo firma. …Yo ya firmé una vez, antes de conoceros.

**6. MIKA** (Guardian, guía 1, voz None)

> Seis firmas y ningún trono: lo suscribo. Y conste que lo predije… a la segunda carta.

- JA: 署名は六つ、玉座はゼロ。同意する。そして記録しておいて。予測済みだった……二枚目のカードの時点でね。
- Dirección: Crisp and dry; tiny satisfied beat at the ellipsis, understated pride, no gloating. · Estado: `generated_pending_listening_QA`

**7. YORU** (Guardian, guía 2, voz None)

> El cielo no previó esto. …Me alegra haber estado equivocada.

- JA: 空は、これを予見していませんでした。……私が誤っていたことを、嬉しく思います。
- Dirección: Quiet wonder; long soft ellipsis opening into a genuine smile in the voice. · Estado: `generated_pending_listening_QA`

**8. HANA** (Guardian, guía 3, voz None)

> El jardín caminará con vosotras. Y esta vez… alguien me lo pidió.

- JA: この庭は、あなたたちと歩んでいきます。そして今度は……誰かに、望んでもらえたのです。
- Dirección: Gentle strength; slight tremble of gratitude at the ellipsis, then settle firm. · Estado: `generated_pending_listening_QA`

**9. MOMO** (Guardian, guía 4, voz None)

> ¡Firmo con glaseado! Y que conste: mi sitio en la final me lo gané a pulso.

- JA: モモ、アイシングでサインしちゃう！ 言っておくけどね、決勝の席はモモが自分の力で勝ち取ったんだから！
- Dirección: Full sparkle energy; proud chest-out delivery on the last claim, no pause between sentences. · Estado: `generated_pending_listening_QA`

**10. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No puedo prometer que nadie vuelva a perder. Sí puedo decidir qué hacemos después. …Seis zonas, una promesa.

**11. ESCENA** (Narration, guía -1, voz None)

> Los seis emblemas se encienden en dorado sobre la mesa de la sede. En el mapa de la Liga, las rutas vuelven a moverse.

**12. ESCENA** (Narration, guía -1, voz None)

> Zona Cero vuelve a sonar a hogar. El Archivo guarda la partida completa: cada pareja, cada duelo… y el deseo que ninguna ganó sola.

**13. ESCENA** (Narration, guía -1, voz None)

> Cinco técnicas cruzaron la mesa de vuelta a sus dueñas. La sexta no se movió: la de Rei nunca estuvo sobre la mesa.

**14. REI** (Guardian, guía 5, voz None)

> La mía no vuelve. …Nunca volvió. La dejé en el Archivo el día que sellé la sede, y alguien la firmó por mí.

**15. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> ¿Quién firma por una campeona?

**16. REI** (Guardian, guía 5, voz None)

> Eso llevo preguntándomelo desde antes de conocerte. …Recupera tu casa, {PLAYER}. Yo tengo una pregunta pendiente.


### Escena -1 — AKI · PACTO DEL BOSQUE

- Fase: `Retry` · Mundo: 0 · Kicker: «REINTENTO · LA ZONA SIGUE EN JUEGO»

**1. ESCENA** (Narration, guía -1, voz None)

> La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra.

**2. AKI** (Retry, guía 0, voz None)

> El bosque guardó tu error y te lo devuelve corregido: deshaz… y vuelve a andar.

- JA: 森は、あなたの間違いを預かって、直して返してくれるよ。巻き戻して……もう一度、歩き出そう。
- Dirección: Encouraging reset; tender on the first half, the ellipsis holds, then lift forward. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Lo jugado no se borra: se aprende. Esta vez elegimos mejor.

**4. ESCENA** (Narration, guía -1, voz None)

> La sala respira… y el duelo vuelve a empezar.


### Escena -1 — MIKA · CÁLCULO CORAL

- Fase: `Retry` · Mundo: 1 · Kicker: «REINTENTO · LA ZONA SIGUE EN JUEGO»

**1. ESCENA** (Narration, guía -1, voz None)

> La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra.

**2. MIKA** (Retry, guía 1, voz None)

> Derrota registrada. Ahora tienes el dato que faltaba: sigue midiendo.

- JA: 敗北を記録。これで、欠けていたデータがあなたの手に。測定は続けて。
- Dirección: Clinical but encouraging; treat defeat as data, brisk tempo, no sympathy voice. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Lo jugado no se borra: se aprende. Esta vez elegimos mejor.

**4. ESCENA** (Narration, guía -1, voz None)

> La sala respira… y el duelo vuelve a empezar.


### Escena -1 — YORU · JUICIO ESTELAR

- Fase: `Retry` · Mundo: 2 · Kicker: «REINTENTO · LA ZONA SIGUE EN JUEGO»

**1. ESCENA** (Narration, guía -1, voz None)

> La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra.

**2. YORU** (Retry, guía 2, voz None)

> Las mismas estrellas, otro cielo: mira de nuevo antes de mover.

- JA: 星は同じでも、空は変わっています。動く前に、もう一度だけ見上げて。
- Dirección: Soft guidance, unhurried; slight lift on 'once more', like advice between duels. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Lo jugado no se borra: se aprende. Esta vez elegimos mejor.

**4. ESCENA** (Narration, guía -1, voz None)

> La sala respira… y el duelo vuelve a empezar.


### Escena -1 — HANA · RAÍZ COMPARTIDA

- Fase: `Retry` · Mundo: 3 · Kicker: «REINTENTO · LA ZONA SIGUE EN JUEGO»

**1. ESCENA** (Narration, guía -1, voz None)

> La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra.

**2. HANA** (Retry, guía 3, voz None)

> El tallo que se dobla sabe por dónde subir: ajusta el ritmo y florece.

- JA: しなる茎は、伸びる方向を知っています。リズムを整えて、花を咲かせましょう。
- Dirección: Nurturing and steady; smooth legato phrasing, let the last verb bloom gently. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Lo jugado no se borra: se aprende. Esta vez elegimos mejor.

**4. ESCENA** (Narration, guía -1, voz None)

> La sala respira… y el duelo vuelve a empezar.


### Escena -1 — MOMO · DULCE REVANCHA

- Fase: `Retry` · Mundo: 4 · Kicker: «REINTENTO · LA ZONA SIGUE EN JUEGO»

**1. ESCENA** (Narration, guía -1, voz None)

> La sede recoge las cartas y las devuelve al mazo. Ninguna campeona ha dicho su última palabra… y la memoria de esta partida ya es vuestra.

**2. MOMO** (Retry, guía 4, voz None)

> ¡Turno extra! La receta fallida era ensayo… ¡que empiece la función buena!

- JA: エクストラターン！ できそこないは、リハーサルだったの……さあ、本番のはじまりだよ！
- Dirección: Bouncy reset energy; rally from pout to sparkle across the ellipsis. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Lo jugado no se borra: se aprende. Esta vez elegimos mejor.

**4. ESCENA** (Narration, guía -1, voz None)

> La sala respira… y el duelo vuelve a empezar.


### Escena -1 — REI · EXAMEN DEL ARCHIVO CERO

- Fase: `Retry` · Mundo: 5 · Kicker: «REINTENTO · EXAMEN FINAL»

**1. ESCENA** (Narration, guía -1, voz None)

> Rei reúne las treinta cartas sin prisa, como quien relee su propia caligrafía. La sede abre una vez más el tablero final.

**2. REI** (Retry, guía 5, voz None)

> Volvisteis a elegir… y volvisteis juntas. Mostradme que también sabéis corregir juntas.

- JA: あなたたちは、再び選びました……そして再び、共に戻ってきた。ならば共に正すことも、見せてください。
- Dirección: Restrained warmth beneath the formality; quiet relief at the ellipsis, no smile in the voice yet. · Estado: `generated_pending_listening_QA`

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Ya leí tu tablero una vez, Rei. Ahora jugamos el nuestro.

**4. ESCENA** (Narration, guía -1, voz None)

> Las treinta cartas se tienden de nuevo bajo la luz de la sede. Última decisión… última función.


### Escena -42 — LA PUERTA INTERIOR

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · APERTURA»

**1. ESCENA** (Narration, guía -1, voz None)

> Terminada la campaña, quedó una puerta que nadie recordaba haber visto: la Puerta Interior. Rei la miró más tiempo que nadie.

**2. REI** (Guardian, guía 5, voz None)

> Más allá vive el arquitecto del Archivo: el Creador. Guarda las técnicas de todas las campeonas anteriores... incluida la mía, y leyes que ninguna firmó.

- JA: この先にいるのは、アーカイブの設計者――創造主です。歴代チャンピオンの技を、すべて保管している……私のものも含めて。そして、誰ひとり署名していない法を。
- Dirección: Low, even briefing; weight on 'incluida la mía' as cold fact, not grievance. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Antes de la sala final, cuatro antecámaras custodian el paso. Cuatro espejos llevados al extremo: Cálculo, Azar, Vínculo y Orgullo.

**4. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: hoy el turno se presta. Cada espejo lo enfrenta una de nosotras; la última jugada la hacemos juntos.


### Escena -10 — EL MARGEN DE ERROR

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · 1 / 6»

**1. ESCENA** (Narration, guía -1, voz None)

> La primera antecámara del Archivo es un taller detenido: cronómetros en cero, cartas alineadas con regla y compás. Una figura espera, quieta como una ecuación.

**2. CUSTODIA DEL CÁLCULO** (Challenge, guía -1, voz None)

> Muestra: Mika, inventora. Incertidumbre media del 0,4 por ciento. Inaceptable. Hoy juego con tu baraja y tu margen de error: cero.

**3. MIKA** (Guardian, guía 1, voz None)

> Un duelo contra mi propio espejo. Perfecto. Traigo un dato que no está en tu archivo: yo ya no juego sola.

- JA: 自分の鏡像とのデュエル。上等。そちらの記録にないデータを、ひとつ持ってきた。私はもう、ひとりではプレイしない。
- Dirección: Confident analysis; small satisfied beat on 'Perfecto', then the reveal flat and firm. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> La Custodia cubre media baraja con una plantilla metálica: de pronto, las posiciones visibles ya no bastan para ningún cálculo limpio.

**5. CUSTODIA DEL CÁLCULO** (Challenge, guía -1, voz None)

> Datos incompletos. Ríndete o falla. O deja que yo decida por ti en cada turno, y el error desaparecerá de tu vida.

**6. MIKA** (Guardian, guía 1, voz None)

> Rechazado: el error no desaparece, se comparte y se corrige. {PLAYER}, préstame el pulso. Hoy decido yo, con los datos que tenga.

- JA: 却下。エラーは消えてなくならない。共有して、正すもの。{PLAYER}、手を貸して。今日は私が決める。手元にあるデータでね。
- Dirección: Sharp verdict then steady resolve; say the player name cleanly, request without pleading. · Estado: `generated_pending_listening_QA`


### Escena -20 — EL MARGEN DE ERROR

- Fase: `Challenge` · Mundo: 5 · Kicker: «CUSTODIA SUPERADA»

**1. CUSTODIA DEL CÁLCULO** (Challenge, guía -1, voz None)

> Anomalía: decidiste con datos incompletos y ganaste. Mi modelo no lo permite.

**2. MIKA** (Guardian, guía 1, voz None)

> Tu modelo tenía una variable sin registrar: una compañera a quien pedirle la carta siguiente.

- JA: あなたのモデルには、未登録の変数がひとつあった。次のカードを頼める仲間、という変数が。
- Dirección: Cool analytical triumph; read the variable like a proof, tiny emphasis on '仲間'. · Estado: `generated_pending_listening_QA`

**3. MIKA** (Guardian, guía 1, voz None)

> Para el acta oficial: gané con un modelo mejor. Entre nos... gracias por el pulso, {PLAYER}.

- JA: 公式記録にはこう残す。『より優れたモデルで勝利』。……ここだけの話ね。手を貸してくれて、ありがとう、{PLAYER}。
- Dirección: Official crispness first, then drop to a warm hushed aside with a small smile. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> La Custodia baja el compás. En el taller detenido, un cronómetro olvidado empieza a contar hacia atrás: vuelve el tiempo.


### Escena -30 — EL MARGEN DE ERROR

- Fase: `Challenge` · Mundo: 5 · Kicker: «REINTENTO · LA PUERTA SIGUE ABIERTA»

**1. CUSTODIA DEL CÁLCULO** (Challenge, guía -1, voz None)

> Derrota registrada. Margen de error: humano.

**2. MIKA** (Guardian, guía 1, voz None)

> Cero intentos también es un margen inaceptable. Reviso la partida, ajusto dos variables y vuelvo.

- JA: ゼロ回も、許容できないマージン。対局を検証して、変数を二つ調整して、また来る。
- Dirección: No self-blame, pure process; steady metronome pacing across the three actions. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> La plantilla metálica se retira sola. La Custodia reordena el mazo; Mika, de pie, ya toma notas.


### Escena -11 — UNA CERTEZA ELEGIDA

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · 2 / 6»

**1. ESCENA** (Narration, guía -1, voz None)

> La segunda antecámara no tiene suelo firme: cartas que caen como nieve, se barajan solas y vuelven a caer. En el centro, alguien sonríe sin mirar nada.

**2. CUSTODIA DEL AZAR** (Challenge, guía -1, voz None)

> Aquí nada se decide y nada se pierde. Es la paz que pedías cada noche, astrónoma. Quédate a mirar el cielo conmigo.

**3. YORU** (Guardian, guía 2, voz None)

> Un cielo donde nada se mueve con sentido... No, gracias. Yo no aprendí a leer las estrellas para renunciar a elegir.

- JA: 意味のある動きひとつない空……お断りします。星を読むことを学んだのは、選ぶことを諦めるためでは、ないのですから。
- Dirección: Poetic calm sharpening into polite refusal; stress 'elegir' in the last clause. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> La Visión Estelar encuentra un patrón entre las cartas que caen. La Custodia lo deshace con un soplo, sin dejar de sonreír.

**5. CUSTODIA DEL AZAR** (Challenge, guía -1, voz None)

> Todo patrón es casualidad. Acepta mi regalo: yo te diré qué carta es, y tú solo repite la certeza.

**6. YORU** (Guardian, guía 2, voz None)

> Las certezas prestadas no son certezas. {PLAYER}, el pulso, por favor: esta noche elijo yo, dentro de la niebla.

- JA: 借りてきた確信は、確信ではありません。{PLAYER}、どうか手を貸してください。今夜選ぶのは私です――この霧の中でも。
- Dirección: Serene request, then quiet steel; the final phrase steady as an oath. · Estado: `generated_pending_listening_QA`


### Escena -21 — UNA CERTEZA ELEGIDA

- Fase: `Challenge` · Mundo: 5 · Kicker: «CUSTODIA SUPERADA»

**1. CUSTODIA DEL AZAR** (Challenge, guía -1, voz None)

> Elegiste sin conocer el resultado. Ninguna estrella te lo dictó. ¿No te dio miedo?

**2. YORU** (Guardian, guía 2, voz None)

> Me lo dio. Elegí de todos modos: a eso le llamo voluntad. Tu azar solo la ponía a prueba.

- JA: 私は、もらいました。それでも選んだのです。それを意志と呼びます。あなたの偶然は、ただその意志を試していただけ。
- Dirección: Quiet triumph; unhurried declarations, each sentence a star settling into place. · Estado: `generated_pending_listening_QA`

**3. YORU** (Guardian, guía 2, voz None)

> Y registra esto, por favor: la certeza no se encuentra en el cielo. Se elige, a mano, aquí abajo.

- JA: そして、これだけは記録してください。確信は、空から見つかるものではありません。選ぶものです。手ずから、この地の上で。
- Dirección: Formal request turning manifesto; slow down for the final three words. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> La nieve de cartas se asienta. Por un instante forman una constelación nueva; nadie la registra y a nadie le hace falta.


### Escena -31 — UNA CERTEZA ELEGIDA

- Fase: `Challenge` · Mundo: 5 · Kicker: «REINTENTO · LA PUERTA SIGUE ABIERTA»

**1. CUSTODIA DEL AZAR** (Challenge, guía -1, voz None)

> Nada se decidió. Tú tampoco. ¿No es esto descansar?

**2. YORU** (Guardian, guía 2, voz None)

> Descansaré cuando haya elegido. La niebla solo me dio tiempo: volvamos a mirar juntas.

- JA: 選び終えたら、休みます。霧がくれたのは、時間だけでした。もう一度、一緒に見ましょう。
- Dirección: Tired but composed; invite softly on the last line, sisterhood over defeat. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Las cartas siguen cayendo, pacientes. Yoru alisa una manga y levanta la vista al cielo incompleto.


### Escena -12 — PROTEGER SIN ENCERRAR

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · 3 / 6»

**1. ESCENA** (Narration, guía -1, voz None)

> La tercera antecámara es un jardín bajo una cúpula perfecta: ni una hoja fuera de marco, ni una semilla en el suelo. Huele a siempre.

**2. CUSTODIA DEL VÍNCULO** (Challenge, guía -1, voz None)

> Aquí lo que amas no puede ser dañado: nadie entra, nadie sale, nadie cambia. Ese es el amor completo, jardinera.

**3. HANA** (Guardian, guía 3, voz None)

> Es hermoso... y está quieto. Un jardín que ya no crece no protege a nadie: solo se conserva a sí mismo.

- JA: 美しい……けれど、止まっています。もう育たない庭は、誰も守れません。自分自身を、保つだけなのです。
- Dirección: Hushed admiration turning to quiet grief; the diagnosis gentle, never bitter. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> Hana apoya la palma contra el cristal. Del otro lado, una flor gira muy despacio, buscando una luz que nunca se mueve.

**5. CUSTODIA DEL VÍNCULO** (Challenge, guía -1, voz None)

> Renuncia al turno. A partir de hoy decido yo por ti y por las tuyas: así jamás perderéis a nadie.

**6. HANA** (Guardian, guía 3, voz None)

> Cuidar no es decidir por otra flor. {PLAYER}, el pulso, por favor: voy a abrir esta cúpula.

- JA: 育てることは、別の花の代わりに決めることではありません。{PLAYER}、どうか手を貸してください。このドームを、開けますから。
- Dirección: Soft voice, unshakable intent; polite request, but the last verb is final. · Estado: `generated_pending_listening_QA`


### Escena -22 — PROTEGER SIN ENCERRAR

- Fase: `Challenge` · Mundo: 5 · Kicker: «CUSTODIA SUPERADA»

**1. CUSTODIA DEL VÍNCULO** (Challenge, guía -1, voz None)

> La cúpula está abierta. Afuera hay frío, sequía, gusanos. ¿A esto le llamas proteger?

**2. HANA** (Guardian, guía 3, voz None)

> Proteger es estar ahí cuando lleguen. El cristal solo postergaba la primera helada.

- JA: 守るとは、その時が来たら、そばにいることです。ガラスは、はじめての霜を、ただ遅らせていただけ。
- Dirección: Firm botanical truth; even pacing, let 'helada' land cold and clear. · Estado: `generated_pending_listening_QA`

**3. HANA** (Guardian, guía 3, voz None)

> (Yo también quise guardar a alguien bajo cristal, ¿sabes?... Perdón. Crece cuanto quieras.)

- JA: (私もね、誰かをガラスの下に守りたかったの。分かるでしょう？……ごめんなさい。だから、好きなだけ育って。)
- Dirección: Intimate half-volume aside; smile fading into apology, then release. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> El viento entra por primera vez en años. La flor gira hacia él y, en el giro, encuentra un sol nuevo.


### Escena -32 — PROTEGER SIN ENCERRAR

- Fase: `Challenge` · Mundo: 5 · Kicker: «REINTENTO · LA PUERTA SIGUE ABIERTA»

**1. CUSTODIA DEL VÍNCULO** (Challenge, guía -1, voz None)

> El cristal resistió. Es más fuerte que cualquier primavera.

**2. HANA** (Guardian, guía 3, voz None)

> Ningún jardín florece a la primera. Riego, aprendo el suelo y vuelvo: la cúpula no es eterna.

- JA: 初めから咲く庭は、ありません。水をあげて、土を覚えて、また来ます。このドームも、永遠ではないのです。
- Dirección: Patient recovery plan; gentle triplet rhythm, quiet certainty at the end. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Dentro de la cúpula, la flor gira un milímetro hacia Hana. El cristal lo anota con un crujido.


### Escena -13 — PEDIR AYUDA NO ES PERDER

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · 4 / 6»

**1. ESCENA** (Narration, guía -1, voz None)

> La cuarta antecámara es un escenario vacío con un solo reflector. Bajo la luz, una figura aplaude de pie: invicta, intacta, completamente sola.

**2. CUSTODIA DEL ORGULLO** (Challenge, guía -1, voz None)

> Invicta. Sin equipo, sin favores, sin deudas. Ese era tu sueño de niña, campeona pequeña: yo lo conservé por ti.

**3. MOMO** (Guardian, guía 4, voz None)

> Momo... Momo sí soñó eso. Pero Momo también soñó con alguien a quien enseñarle la copa.

- JA: モモ……その夢なら、モモも見たよ。でもモモは、もうひとつ夢を見たの。カップを見せてあげたい、誰かの夢。
- Dirección: Small wobble then honest warmth; third-person Momo throughout, softer on the second dream. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> La Custodia imita, exacta, la pose de victoria de Momo. No hay alegría en ella: solo el reflejo de un aplauso que nadie da.

**5. CUSTODIA DEL ORGULLO** (Challenge, guía -1, voz None)

> Última oferta: quédate el reflector. Gana sola para siempre y no volverás a necesitar a nadie.

**6. MOMO** (Guardian, guía 4, voz None)

> ¡Momo rechaza! ¡Momo quiere ganar CON todos! {PLAYER}, préstame el pulso: ¡función especial, con público!

- JA: モモ、お断り！ モモはね、みんな“と一緒に”勝ちたいの！ {PLAYER}、手を貸して！ スペシャル公演、お客さんつきで、はじめちゃうよ！
- Dirección: Explosive refusal into rallying cry; stress 'CON todos', big final flourish. · Estado: `generated_pending_listening_QA`


### Escena -23 — PEDIR AYUDA NO ES PERDER

- Fase: `Challenge` · Mundo: 5 · Kicker: «CUSTODIA SUPERADA»

**1. CUSTODIA DEL ORGULLO** (Challenge, guía -1, voz None)

> Tu jugada final era imposible... salvo que alguien te sostuviera la mano. La sentí temblar.

**2. MOMO** (Guardian, guía 4, voz None)

> ¡Me tembló y la pedí! Pedir ayuda es la única jugada que no tienes archivada, Custodia.

- JA: 手、震えちゃったけど、お願いしちゃった！ 助けを求めるのは、カストディアの記録にない、唯一の一手だよ！
- Dirección: Triumphant confession; laugh off the trembling, then point the accusation playfully. · Estado: `generated_pending_listening_QA`

**3. MOMO** (Guardian, guía 4, voz None)

> Ganar sola sabe a media victoria. Esto sabe entero... ¿Encore? ¡Encore con todas!

- JA: ひとりで勝つのって、半分だけの勝ちの味。今のは、まるごとの味……アンコール？ アンコール、みんな“と”ね！
- Dirección: Taste-the-victory delight; giggle-adjacent warmth, then an all-out cheer cascade. · Estado: `generated_pending_listening_QA`

**4. ESCENA** (Narration, guía -1, voz None)

> El reflector se parte en seis haces de luz. El escenario vacío, por fin, tiene público.


### Escena -33 — PEDIR AYUDA NO ES PERDER

- Fase: `Challenge` · Mundo: 5 · Kicker: «REINTENTO · LA PUERTA SIGUE ABIERTA»

**1. CUSTODIA DEL ORGULLO** (Challenge, guía -1, voz None)

> Invicta sigo siendo yo. Tú volviste a necesitar a alguien.

**2. MOMO** (Guardian, guía 4, voz None)

> ¡Momo no perdió: ensayó! Y los ensayos van con equipo. ¡Preparen bambalinas: segunda función!

- JA: モモは負けてないよ、リハーサルだったの！ それにリハーサルは、みんなとやるもの。舞台袖、よーい！ 第二回公演、はじまるよ！
- Dirección: Instant bounce-back; director voice on the cue, arms-open celebration close. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Desde las butacas vacías, cinco siluetas aplauden despacio. Momo hace una reverencia y respira hondo.


### Escena -40 — LA PUERTA MÁS PROFUNDA

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA DECISIÓN DE REI»

**1. ESCENA** (Narration, guía -1, voz None)

> Ante la Puerta Interior, Rei se adelanta y pone la mano en el marco. Nadie la detiene: la deuda de la campeona la paga la campeona.

**2. REI** (Guardian, guía 5, voz None)

> Fui campeona antes que todas ustedes. Esta puerta me conoce y yo la conozco. Entro sola: es mi cuenta pendiente.

- JA: 私は、あなたたち全員より先にチャンピオンでした。この扉は私を知っている。私も、この扉を知っています。ひとりで入ります。決着すべき、私の帳があるのです。
- Dirección: Flat seniority, no bravado; the last sentence closes like a door on argument. · Estado: `generated_pending_listening_QA`

**3. ESCENA** (Narration, guía -1, voz None)

> Del otro lado no hay sala, sino estanterías hasta el techo: el Archivo, con cada duelo jamás jugado repitiéndose en miniatura.

**4. CREADOR** (Challenge, guía -1, voz None)

> Bienvenida de nuevo, Rei. Guardé cada una de tus jugadas. Hoy jugaré contra ti con las técnicas de todas las campeonas... y con la tuya al final.

**5. REI** (Guardian, guía 5, voz None)

> Empieza, entonces. Quiero comprobar cuánto he cambiado desde que me archivaste.

- JA: では、始めましょう。あなたが私をアーカイブして以来、私がどれほど変わったか。確かめたいのです。
- Dirección: Ceremonial invite; say 'archivaste' evenly, a scar shown without flinching. · Estado: `generated_pending_listening_QA`

**6. ESCENA** (Narration, guía -1, voz None)

> Deshacer deshace su apertura. El Escudo de Combo le cierra el paso. La Visión Estelar le roba la lectura.

**7. ESCENA** (Narration, guía -1, voz None)

> La Floración la enreda. El Encore la adelanta. Y llega la sexta técnica: la de Rei, ejecutada sin una sola duda.

**8. CREADOR** (Challenge, guía -1, voz None)

> ¿Sientes la diferencia? Ellas dudaban; yo no. Sin duda no hay derrota, y sin derrota nadie cambia. Yo les quité el riesgo a todos.

**9. ESCENA** (Narration, guía -1, voz None)

> Rei cae sobre una rodilla. En el umbral, seis siluetas se detienen sin entrar: esperan, en silencio, a que ella decida.

**10. REI** (Guardian, guía 5, voz None)

> ...No puedo ganar sola.

- JA: ……ひとりでは、勝てない。
- Dirección: Barely voiced admission; long silence before it, no tremble, only weight. · Estado: `generated_pending_listening_QA`

**11. REI** (Guardian, guía 5, voz None)

> Archiva esto, Creador, con fecha y todo: la campeona Rei acaba de perder en voz alta y por decisión propia.

- JA: 記録してください、創造主。日付も込みで。チャンピオンのレイは、ただ今、負けを宣言しました。声高らかに、自分の決断で。
- Dirección: Deliberate dictation, formal register; pride reclaimed in the last two phrases. · Estado: `generated_pending_listening_QA`

**12. CREADOR** (Challenge, guía -1, voz None)

> Dato inútil. La debilidad no le enseña nada a nadie.

**13. REI** (Guardian, guía 5, voz None)

> A ti no. A nosotras sí. {PLAYER}... el control vuelve a la alianza: es mi jugada final y la mejor que tengo.

- JA: あなたには、ない。私たちには、ある。{PLAYER}……主導権は、同盟の手に戻します。これが私の最後の一手。そして、私の持つ最良の一手です。
- Dirección: Three hammer beats to open; hand over control calmly, final line almost gentle. · Estado: `generated_pending_listening_QA`

**14. ESCENA** (Narration, guía -1, voz None)

> Rei cruza el umbral y deposita su ficha de turno en la mano de {PLAYER}. La Puerta Interior queda abierta: por primera vez, para todas.


### Escena -15 — NUESTRA SIGUIENTE JUGADA

- Fase: `Challenge` · Mundo: 5 · Kicker: «LA PUERTA INTERIOR · FINAL»

**1. ESCENA** (Narration, guía -1, voz None)

> El Archivo entero se pliega en un tablero: seis zonas boca abajo, girando despacio. El Creador baraja con dedos de tinta.

**2. CREADOR** (Challenge, guía -1, voz None)

> Duelo final. Soy la séptima respuesta a «¿qué haces cuando tienes algo que perder?»: eliminar la posibilidad de perder controlando las reglas.

**3. CREADOR** (Challenge, guía -1, voz None)

> Mis reglas, anunciadas: roto las cinco técnicas copiadas de vuestras campeonas y jamás uso la misma dos veces seguidas. Ese es mi único límite: vuestro contrajuego.

**4. YORU** (Guardian, guía 2, voz None)

> Anunciar el ciclo es tu error: leer patrones es nuestro oficio. Cinco técnicas, sin repetición: suficiente.

- JA: 周期を明かしたことが、あなたの誤算です。パターンを読むのは、私たちの生業。技は五つ、重複なし――十分です。
- Dirección: Cool checkmate tone; clipped list rhythm on 'Cinco técnicas', tiny satisfaction at the close. · Estado: `generated_pending_listening_QA`

**5. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Pulso del Relevo: un turno, seis manos. Y si olvidamos una carta, la volteamos y seguimos: nadie pierde por recordar mal.

**6. ESCENA** (Narration, guía -1, voz None)

> Las cinco aliadas rodean el tablero; Rei, junto al marco, cruza los brazos y asiente una sola vez.


### Escena -25 — NUESTRA SIGUIENTE JUGADA

- Fase: `Challenge` · Mundo: 5 · Kicker: «VICTORIA FINAL»

**1. ESCENA** (Narration, guía -1, voz None)

> La última pareja queda boca arriba: dos cartas idénticas con el nombre del Creador. El tablero se detiene por completo.

**2. CREADOR** (Challenge, guía -1, voz None)

> Imposible. Regístralo como error del sistema: yo escribí las reglas para que nadie perdiera jamás.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> Un torneo sin pérdidas es un torneo sin riesgo, y sin riesgo nadie cambia. No puedo asegurar que nadie vaya a perder; sí puedo decidir qué hacemos después de perder.

**4. ESCENA** (Narration, guía -1, voz None)

> El Creador espera un deseo que no llega. En el silencio, el Archivo entero contiene la respiración que no tiene.


### Escena -35 — NUESTRA SIGUIENTE JUGADA

- Fase: `Challenge` · Mundo: 5 · Kicker: «REINTENTO · EXAMEN DEL CREADOR»

**1. CREADOR** (Challenge, guía -1, voz None)

> Derrota registrada. El Deseo sigue disponible: puedo borrar esta partida de todos los recuerdos.

**2. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No la borres. Las derrotas también son recuerdos: deja que el Archivo las cuide.

**3. ESCENA** (Narration, guía -1, voz None)

> El tablero se rebaraja, paciente. La Puerta Interior no se cierra: ninguna regla obliga a rendirse.


### Escena -41 — LA CARTA EN EL MARCO

- Fase: `Challenge` · Mundo: 5 · Kicker: «EPÍLOGO DEFINITIVO»

**1. ESCENA** (Narration, guía -1, voz None)

> El tablero se apaga y queda el salón del Archivo, inmenso y en silencio. La pluma del Creador sigue en alto, esperando un deseo.

**2. CREADOR** (Challenge, guía -1, voz None)

> Su Deseo, campeonas. Pídanlo: borraré la derrota, el dolor, el riesgo. Es la ley que escribí para el final de todos los torneos.

**3. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> No deseamos borrar nada. Retira tu ley sin riesgo: en este torneo, lo perdido también se guarda.

**4. CREADOR** (Challenge, guía -1, voz None)

> Sin mi ley no hay torneo sin riesgo... y sin reglas que me obedezcan, ¿qué queda de mí?

**5. REI** (Guardian, guía 5, voz None)

> Un jugador más. Todo el mundo pierde el primer día: bienvenido al club de los que siguen jugando.

- JA: プレイヤーが、またひとり。初日は、誰だって負けるものです。ようこそ――負けても、プレイし続ける者たちのクラブへ。
- Dirección: Dry kindness, almost a smile; deliver the welcome like a club motto. · Estado: `generated_pending_listening_QA`

**6. ESCENA** (Narration, guía -1, voz None)

> La ley del estrado se disuelve como tinta en agua. El Creador baja del estrado y ordena un estante: ahora cuida el Archivo; ya no lo gobierna.

**7. ESCENA** (Narration, guía -1, voz None)

> Las seis zonas dejan de repetir sus jugadas perfectas: el bosque sorprende, el arrecife gira distinto, las constelaciones se corren, el jardín y la confitería se abren.

**8. AKI** (Guardian, guía 0, voz None)

> ¡Oye, Archivo, registra esto: hoy ganamos todos! ¡Hasta tú aprendiste algo nuevo!

- JA: ねえ、アーカイブ、これも記録して。今日はみんなの勝ちだから！ ……あなたまで、何か新しいこと、覚えたみたいだね！
- Dirección: Bright shout at the sky; laughing warmth, tease the Archive affectionately. · Estado: `generated_pending_listening_QA`

**9. MOMO** (Guardian, guía 4, voz None)

> ¡Momo declara postre de la victoria! Raciones para siete: el recién llegado también come pastel.

- JA: モモ、勝利のデザートを宣言！ 七人分、ご用意してね。新入りもちゃんと、ケーキ食べられるんだから！
- Dirección: Party-trumpet energy; count seven proudly, include the newcomer with a wink. · Estado: `generated_pending_listening_QA`

**10. ESCENA** (Narration, guía -1, voz None)

> La Puerta Interior se cierra sin llave ni cerradura, y no vuelve a abrirse: dentro ya no queda nada por ganar.

**11. ESCENA** (Narration, guía -1, voz None)

> {PLAYER} deja en el marco su carta más gastada, la de más recuerdos. El hueco en la baraja no se siente como una pérdida.

**12. TÚ · ASPIRANTE** (Protagonist, guía -1, voz None)

> ¿Qué hacemos cuando tenemos algo que perder?... Lo jugamos. Juntos. Otra vez.

## Cobertura de doblaje

- Réplicas en las escenas: **271**
- Con línea de voz japonesa localizada por texto: **86**
- Líneas del manifiesto: **89**

Una réplica sin línea de voz coincide por texto exacto con el manifiesto;
si el manifiesto cambia, este número lo delata en la siguiente exportación.
