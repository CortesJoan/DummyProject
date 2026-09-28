# Worklog — Reescritura principal + postgame cerrado

Documento de cambios de sesión. Cada entrada lleva fecha, qué se cambió, dónde,
y estado de verificación. No sustituye a FINAL_REWRITE_POSTGAME_CHECKLIST.md
(criterios de aceptación); este archivo registra el trabajo real realizado.

---

## 2026-09-13 — Sesión: desbloqueo del guionista + cableado del postgame

### 1. Corregido el clasificador de rechazos del Círculo (control plane)

**Problema.** `dispatch_task` clasificaba la respuesta completa del proveedor con
`classify_continuity_event`, que hace coincidencia por subcadena en todo el
texto. Las señales de `policy_refusal` incluyen frases tan amplias como
«no puedo hacer», «me niego a», «refuse to». Un guion entregado que contenga
esas frases dentro de un diálogo (normal en ficción) se marcaba como rechazo de
política y el trabajo terminaba `needs_continuity` sin conservar la entrega.
Eso fue lo que ocurrió con el encargo de reescritura (job-30a128c25d05): no se
probó que fuera un rechazo real.

**Cambio.** Nuevo método `classify_provider_response` en
`adapters/circle_admin.py` que solo escanea la ventana inicial de la respuesta
(`response_refusal_scan.leading_chars`, por defecto 600, en
`policies/continuity.yaml`). Un rechazo real encabeza la respuesta; una entrega
que cita frases de rechazo dentro de su contenido ya no se clasifica mal. Los
mensajes de error de adaptadores siguen pasando por `classify_continuity_event`
(sin cambios). Ninguna señal se eliminó ni debilitó: un rechazo genuino sigue
exigiendo revisión administrativa.

**Archivos.**
- `J:/AI/EHKP-Agent-Circle/adapters/circle_admin.py` — nuevo método + `dispatch` lo usa.
- `J:/AI/EHKP-Agent-Circle/policies/continuity.yaml` — `response_refusal_scan.leading_chars: 600`.
- `J:/AI/EHKP-Agent-Circle/tests/test_circle_admin.py` — 4 pruebas nuevas
  (entrega larga con frase de rechazo en diálogo → completada; rechazo al
  inicio de respuesta larga → revisión; unitarias de la ventana).

**Verificación.** `python -m unittest discover -s tests`: 124/124 OK.

### 2. Estado comprobado antes de editar (sin cambios aún)

- `MementoPostgameSelectionView.cs` existe pero no está referenciada por
  ningún controlador: el selector sigue sin cablearse.
- Backend postgame completo en `GameManager` (inicio, victoria, checkpoint
  Rei guionizado, guardado) — integrado en sesiones anteriores.
- `ReturnToMenu()` del controlador de menú no trata
  `LastResultWasPostgame`: falta el regreso al selector (ganar → siguiente
  encuentro; perder → conservar actual).
- `PostgameScriptedEncounterRequested` existe pero nadie lo escucha: la
  escena guionizada Rei–Creador no tiene presentación VN.
- `MementoStoryVoiceCatalog` exige texto exacto (guía + frase) para usar una
  grabación: las frases reescritas quedarán sin voz hasta regrabar, y esa es
  la protección que pide el checklist (conservar).

### 3. Catálogo puro de escenas VN del postgame

**Nuevo archivo.** `Assets/AnimalMemory/Progression/MementoPostgameStory.cs`
(primera versión de textos; pendiente de sustitución por la entrega de GLM):

- Intro antes de cada duelo (encuentros 0–3 y 5), cierre de victoria (≤ 4
  beats) y de derrota (≤ 3 beats), escena guionizada de Rei (checkpoint 4) y
  epílogo definitivo tras la victoria final.
- Custodias y Creador hablan con `GuideId = -1`: sin retrato ni voz prestados
  hasta que existan recursos propios (protección del checklist).
- Ids de escena negativos (−10…−41): nunca se registran como vistas de
  campaña ni entran en replays; `IsPersistent = false` garantizado.
- Perfiles de identidad y taunt para las cinco adversoras (6–10).

**Pruebas.** `MementoPostgameStoryTests.cs` (nuevo): forma de todas las
escenas, brevedad de cierres, checkpoint de Rei con Rei hablando, epílogo,
identidades/taunts, rechazo de ids inválidos, beats de custodia sin GuideId,
líneas no vacías y ≤ 240 caracteres, y anuncio de reglas en el duelo final.

### 4. Cableado del selector y navegación del postgame

**Archivos.**
- `GameManager.cs`: nuevo `TryConsumePostgameOutcome()` + flag interno
  `postgameOutcomePending` (se marca en victoria y derrota postgame, se limpia
  al iniciar encuentro). Guard contra victorias caducas tras abandonar un
  duelo a medias. Cambio aditivo; sin tocar masks ni guardados.
- `AnimalMemoryMenuController.cs` (la vista `MementoPostgameSelectionView`
  creada la sesión anterior ahora queda conectada):
  - Construcción en `Start` + `Dispose` y desuscripciones en `OnDestroy`
    (también en el bloque de reconexión perezosa de `RefreshAll`).
  - `OpenPostgameSelection/ClosePostgameSelection/SelectPostgameEncounter`:
    abre desde la página Historia y desde el mapa de campaña (botón
    «LA PUERTA INTERIOR», oculto con flag false), selecciona el recomendado.
  - `StartSelectedPostgameEncounter` → intro VN → `BeginSelectedPostgameEncounter`
    (encuentro 4 sin intro: directo al checkpoint guionizado).
  - `OnPostgameScriptedEncounterRequested`: presenta la escena de Rei y
    completa el checkpoint; después reabre el selector con el encuentro 5.
  - `RefreshPostgameSelection` enganchado al refresco throttled del menú
    (0,1 s) con las mismas condiciones de superposición que el mapa.
  - `ReturnToMenu`: rama postgame ANTES que la de campaña — resultado
    terminado presenta victoria/derrota/epílogo y reabre el selector; duelo
    abandonado sólo reabre el selector. Ganar selecciona el siguiente
    encuentro; perder conserva el actual.
  - Exclusión mutua de overlays: `postgameOpen = false` en `OpenCampaign`,
    `OpenStoryReplay`, `ShowStory` y `ReturnToMenu`.

### 5. Entregas de GLM integradas (guion principal + postgame)

**Trabajos del Círculo completados** (ambos con `retain_content`, respuesta
conservada en `jobs/completed/…20260913.json`):
- `job-glm-story-main-20260913`: reescritura íntegra de la campaña (24.843
  caracteres, JSON válido con las 17 escenas, party, retry, título y objetivo).
- `job-glm-story-postgame-20260913`: guion completo del postgame (24.913
  caracteres; llegó envuelto en el esquema de resultados del Círculo con el
  guion en `evidence.script` — se desenvolvió al integrar; el envoltorio
  repara con una llave ausente por corte de longitud).

**Nueva campaña.** Título: «La Liga de las Seis Zonas». Jo-Ha-Kyū aplicado:
prólogo claro (reglas en lenguaje llano + «Un minuto.»), HA con consecuencias
(alianzas riñendo cruces de rutas, mapa cambiando, vendedora Kisōtenketsu en
la escena 12, anomalía del emblema en 6), KYÜ con Rei revelando que ELLA
escribió la regla del Deseo (cierre de la anomalía sembrada) y epílogo con
las cinco aliadas firmando con su voz antes de encender los emblemas.
Técnicas visibles: ma («…Todo lo tengo medido»), honne/tatemae en cada
guardiana, espejos de «¿qué haces cuando tienes algo que perder?».

**Archivos integrados.**
- `MementoMatchCampaign.cs`: región de historia regenerada — tabla explícita
  `StoryScenes[0..16]` (IDs, fases y mundos preservados), `WithParty` con
  líneas nuevas (6/8/11/12/14/15), retry nuevo, `RetryLines` nuevas,
  `CampaignTitle`/`ProtagonistGoal` nuevos. Niveles, lógica de desbloqueo y
  API intactos.
- `MementoPostgameStory.cs`: regenerado con el guion de GLM — apertura única
  (4 beats, antes del primer encuentro con mask==0), intros/victorias/derrotas
  de los seis encuentros, checkpoint guionizado de Rei (14 beats), epílogo
  «La carta en el marco» (12 beats), identidades de custodias.
- `AnimalMemoryMenuController.cs`: apertura encadenada antes de la intro del
  encuentro 0; detalle del selector con identidad de la adversora
  (`MementoPostgameSelectionView`).

**Ajustes de continuidad (5, sobre la autoría de la regla del Deseo).**
El postgame decía que el Creador escribió la regla del Deseo; la campaña
reescrita establece que la escribió Rei. Canon reconciliado: Rei escribió la
regla del Deseo (payoff de escena 15/16); el Creador es el arquitecto del
Archivo con SU propia ley sin riesgo. Ediciones: apertura de Rei («arquitecto
del Archivo… y leyes que ninguna firmó»), tres beats del epílogo (regla→ley,
«Retira tu ley sin riesgo», «La ley del estrado se disuelve»).
**Reintegros locales menores:** pasiva exclusiva explícita en escena 1
(«Es mi pasiva y sólo mía»), «Me uno a vuestra alianza» en el reclutamiento
de Yoru (invariante de reclutamiento), identidad propia del Creador.

### 6. Pruebas actualizadas y ejecutadas

- `MementoPostgameStoryTests`: apertura nueva, taunts fuera (la entrega no
  los trae), reglas del duelo final case-insensitive.
- `MementoMatchCampaignNarrativeTests`: reescritos al canon nuevo conservando
  los invariantes (mundo explicado en el prólogo, pérdida concreta cronometrada,
  oferta sin conquista antes de reclutar, pasiva exclusiva, cinco aliadas antes
  del deseo, epílogo resuelve regla/seis/juntas), palabras clave de alianza
  ampliadas (compañera / me uno / a nuestro lado).
- `AnimalMemoryProgressionTests` / `MementoMenuCopyTests`: título nuevo
  (comparación case-insensitive con la etiqueta UXML); objetivo con «alianza».
- UXML: cuatro etiquetas de título → «LA LIGA DE LAS SEIS ZONAS» (v1.20).

**Resultados Unity EditMode (puente MCP, 127.0.0.1:6400):**
- Suite completa: 669/683 ejecutadas, 0 fallos; los 14 restantes son
  `CortesJoan.EasyShop` (módulo ajeno a este encargo) — su test
  `CurrencyCreationTool_CreatesScriptAndAsset_Successfully` congela el runner
  esperando compilación; afecta a la corrida, no a los resultados previos.
- Suite filtrada (15 fixtures Memento): 204/204, 1 fallo → corregido → rerun
  de fixtures narrativos: **97/97, 0 fallos**. Estado final: succeeded.
- Sin errores de compilación en ningún punto (verificado tras cada refresh).

### 7. Entregables de documentación

- `HISTORIA_COMPLETA_v2.md` (nuevo): exportación única para revisión manual
  de toda la historia — campaña (17 escenas + party + reintentos) y postgame
  completo (apertura, 6 encuentros, epílogo), con cues marcados.
- `story_voice_script_v4_pending.json` (nuevo): 89 frases de guía pendientes
  de grabación. El catálogo exige texto exacto: las frases reescritas suenan
  en silencio hasta regrabar (protección conservada).
- `FINAL_REWRITE_POSTGAME_CHECKLIST.md`: estado actualizado de los ítems.

---

## 2026-09-16 — Sesión: recorrido, voces, revisión independiente y arte

### 8. Recorrido completo del postgame (máquina de estados) probado

- `MementoPostgameRelease.Enabled`: `const` → `static bool` (misma línea,
  mismo false por defecto; ahora los tests pueden activarlo y restaurarlo;
  el desbloqueo de la actualización sigue siendo cambiar ese bool).
- `SaveSystem`: gancho `internal TestOnlySavePathOverride` (sólo tests:
  redirige game.dat a temporal; producción sin cambios).
- **Nuevo `MementoPostgameTraversalTests` (4 tests, 4/4 ✓):** la puerta se
  recorre entera con flag true — cuatro duelos de compañeras en orden con
  la guía correcta, checkpoint guionizado de Rei por la vía pública real
  (evento → `CompletePostgameScriptedEncounter`), duelo final, final
  verdadero desbloqueado, resultado pendiente de consumo único, rejugada
  sin recompensas duplicadas (monedas por `SaveData`), inicio nuevo que
  limpia victorias caducas, y flag true sin campaña completada mantiene la
  puerta cerrada. Sin tocar la partida real del jugador.

### 9. Evaluación independiente del guion (checklist)

- GLM como `game-reviewer` (job-glm-review-story-20260915) leyó el export
  completo por el gateway y entregó veredicto estructurado. Puntuaciones:
  Jo-Ha-Kyū 5/5; ma, subtexto, espejos, spokon, kishōtenketsu y claridad
  4/5. Dos hallazgos de continuidad: uno era el export desactualizado
  (regenerado con las reintegraciones locales) y «séptima respuesta» es
  correcto según el canon de diseño. Cinco recomendaciones editoriales
  quedan documentadas como decisiones del propietario en
  `Documentation~/reviews/STORY_REVIEW_V2_20260916.md`.
- **Aplicado** (micro-ajuste de canon sugerido): escena 0, «recuerdos a
  punto de olvidarse» → «recuerdos que aún no sabe nombrar» (no implica
  borrado). Suite narrativa tras el cambio: 76/76 ✓.

### 10. Fichas de arte de Custodias y Creador

- GLM como `bishoujo-art-director` (job-glm-art-custodias-20260915):
  especificación textual completa — paleta compartida de esmalte con acento
  único por Custodia, reglas de anti-préstamo por espejo, motivo clave,
  poses (neutral/intro/derrota), retrato VN, avatar de selector y presencia
  final del Creador. En `Documentation~/ART_SHEETS_CREADOR_CUSTODIAS.md`
  (nombre exacto: `ART_SHEETS_CUSTODIAS_CREADOR.md`).
- Nota del integrador: las fichas conciben al Creador como figura femenina;
  el guion usa el título «el Creador». Decisión visual pendiente del
  propietario al producir los assets.

### 11. Capturas (checklist)

- 5 capturas en `Assets/Screenshots/`: menú landscape y portrait (clase
  portrait forzada por código, como hace el runtime) y tres momentos del
  prólogo reescrito (beat 0, +3, +9) con el flujo real de juego en play
  mode. Protecciones: guardado redirigido a temporal, nombre del jugador
  snapshot/restaurado («EvilHack» intacto), play mode cerrado.
- Las capturas usan el nombre temporal «ALBA» para {PLAYER} sólo durante
  la sesión de captura; nada persistió.

### 12. Voces japonesas v4 — COMPLETADO

- Tanda única falló (error transitorio del entorno, no del contenido —
  reproducido y descartado con una llamada directa al adaptador).
- Reintentos en dos tandas de 45/44: `job-glm-voice-v4-chunk1/2-20260916b`,
  ambas completadas (18,5 KB y 14,5 KB; la tanda 1 llegó envuelta en el
  esquema de resultados del Círculo y se desenvolvió al integrar).
- **`Assets/AnimalMemory/Audio/story_voice_script_v4.json`**: 89 líneas con
  `guide/id/es/ja/direction/status`, validadas es literal contra la fuente,
  ids únicos y sin ja/direction vacíos. Reparto: Rei 17, Mika/Yoru/Hana/
  Momo 16, Aki 8.
- Ubicación deliberada: el guion vive en `Audio/` (fuente de grabación),
  NO en `Resources/` — el catálogo runtime exige grabaciones reales por
  id; hasta grabar, las frases reescritas suenan en silencio (protección
  conservada). `story_voice_script_v4_pending.json` queda como índice
  previo; v4 es el documento de producción.

### Estado de verificación de la sesión

- Compilación sin errores en cada refresh.
- Narrativa/menú/progresión/traversal: 76/76 ✓ (post-cambio de texto).
- Recorrido postgame: 4/4 ✓.

### Pendiente tras esta sesión

- Grabación (o TTS aprobada) de las 89 líneas de
  `story_voice_script_v4.json`; tras grabar, montar el catálogo runtime.
- Producción de arte/voz/música de Custodias y Creador (fichas listas en
  `ART_SHEETS_CUSTODIAS_CREADOR.md`). El arte tiene ahora una vía local:
  el stack Anima 3.8B del Círculo (ver `docs/IMAGE_STACK_WORKLOG.md` del
  repo del Círculo) dará txt2img anime local como fallback de GPT Image.
- Recorrido VISUAL del postgame con flag true por el propietario.
- Revisión manual de `HISTORIA_COMPLETA_v2.md` y de las 5 recomendaciones
  editoriales de la revisión.
- El flag `MementoPostgameRelease.Enabled` sigue en false.

---

## 2026-09-24 — Sesión: cierre de R7, puerta de postgame, arte y auditorías

El trabajo de esta sesión está registrado en detalle en el repositorio del Círculo, no
aquí, porque incluye jobs rastreados y evidencia de corridas. Este entry es el puntero.

### Dónde está el detalle

- `docs/reviews/memento-release-progress-20260924.md` — registro por lotes (1-18).
- `docs/reviews/memento-r7-ticket-evidence-20260924.md` — matriz de tickets R7.
- `docs/reviews/memento-art-sprite-review-20260924.md` — revisión del arte, sprite a sprite.
- `docs/reviews/memento-art-pipeline-qwen-20260924.md` — pipeline de imagen local.
- `docs/reviews/memento-r8-ads-analytics-evidence-20260924.md` — anuncios y analítica.
- `docs/reviews/memento-r6-audio-audit-20260924.md` — inventario y huecos de audio.
- `docs/reviews/memento-glm-findings-recheck-20260924.md` — qué defectos narrativos siguen.

### Lo que cambió en el código de este proyecto

- `MementoIllustratedCharacters.cs`: el catálogo de hojas pasa de dos arrays de seis
  entradas a un `Dictionary<int, Sheet>` que cubre **0-10**, con `IsRegistered` y
  `GetSheetSource`. Un id registrado sin PNG sigue devolviendo null, así que el arte
  entra sin tocar código cuando el asset aparece.
- `MementoIllustratedCharactersTests.cs`: el test que afirmaba que el id 6 devuelve null
  se reescribió en tres (el id 6 ahora sí está registrado). No se borró.
- `MementoMatchAudioCatalogTests.cs`: añadido `PostgameDuelMusic_NeverBorrowsAGuardianTheme`,
  que fija que ningún oponente postgame comparte tema con una guardiana.
- `Assets/AnimalMemory/Editor/MementoStoryExporter.cs`: **nuevo**. Genera
  `Documentation~/HISTORIA_COMPLETA.md` desde las fuentes efectivas.
- `GraphMementoMenuController.cs` (`AnimalMemory/UI`): la confirmación de borrado se
  extrajo a `DeleteSaveConfirmation` (estado puro) y ahora tiene 7 tests.
- Arte instalado como **placeholder**: `custodia-azar-poses-green-placeholder-v0.png`
  (id 7) y `creador-poses-green-placeholder-v0.png` (id 10) en
  `Assets/AnimalMemory/Resources/AnimalMemory/ArtV2/`.

### Verificación real de la sesión

- R7: 232/232. Puerta de postgame: 86/86. Arte y caché: 346/346. Audio: 93/93.
  Regresión final tras el exportador: 436/436. Compilación limpia en cada paso.
- `MementoPostgameRelease.Enabled` sigue en **false** y el archivo no se tocó nunca:
  el ensayo de la puerta se hizo activando el flag por test.

### Pendiente real, por dependencia

- **D1**: sustituta de Cálculo sin diseño; el id 6 sigue sin hoja.
- **D4**: hoja de la actriz generada y **sin instalar** hasta saber si es id 8 o 9.
- **Arte de GOD fase 2 en el duelo**: el caché sirve una hoja por id y ese combate cambia
  de fase a mitad. Necesita decisión de diseño.
- **Música**: los cinco duelos postgame y la variante de fase 2 de GOD están sin producir;
  la generación está autorizada pero falta concretar proveedor.
- **QA humano**: recorrido visual con flag true, tickets R7 en dispositivo y la escucha de
  las 89 líneas de voz. Nada de esto se sustituye con tests.

### Aviso sobre documentos de historia duplicados

`HISTORIA_COMPLETA_MEMENTO_MATCH.md` y `HISTORIA_COMPLETA_v2.md` son exportaciones
escritas a mano en septiembre y **no reflejan el estado actual**. El handoff prohíbe
mantener una copia manual como segunda fuente. El documento vigente y regenerable es
`Documentation~/HISTORIA_COMPLETA.md`. No se han borrado las copias antiguas: la decisión
de retirarlas es del propietario.
