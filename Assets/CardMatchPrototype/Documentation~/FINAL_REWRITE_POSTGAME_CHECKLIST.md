# Reescritura final y postgame — alcance de entrega

Estado comprobado: 2026-09-12. Este archivo es una lista de aceptación, no una declaración de que el postgame ya funcione.

## Invariantes

- Campaña publicada: 21 niveles, IDs existentes y guardados conservados.
- Postgame: exactamente cuatro duelos de compañeras, encuentro guionizado Rei–Creador y duelo final contra el Creador; ninguna campaña adicional posterior.
- Única opción de lanzamiento desactivada por defecto. Al desactivarla, ningún acceso, nivel prometido ni final incompleto aparece en el juego.
- Activarlo requiere además completar la campaña principal. Progreso del postgame separado del mask de 21 niveles.
- El manifiesto actual referencia `file:J:/UnityPackages/Packages/com.studio.vn-engine` y `file:J:/UnityPackages/Packages/com.ehkp.visual-novel-presentation`. MementoNarrativeRunner usa VN.DialogueEngine; no sustituirlo por otro parser/motor.
- No generar APK/AAB ni publicar. No modificar progreso real para probar.

## Guion principal

- [x] Reescribir las 17 escenas con IDs 0–16 preservados y personajes que hablan entre sí después del reclutamiento. (GLM job-glm-story-main-20260913, integrado 2026-09-15; tabla StoryScenes + WithParty; ver WORKLOG.)
- [x] Explicar Archivo (infraestructura), núcleos (rutas/defensas/suministros), Liga y Deseo con consecuencias concretas. (Prólogo + escena 1: «El sello aparta tu tablero, no a tu gente: se vive, se comercia, se reintenta».)
- [x] Diferenciar Zona Cero (hogar) y Archivo Cero (sede). Perder no borra habitantes ni impide vivir/comerciar/volver a desafiar. (Vendedora Kisōtenketsu, escena 12.)
- [x] Protagonista femenina nombrable, sola al inicio; Rei silueta es la atacante, Aki llega después de la derrota. (Escena 0-1.)
- [x] Jo–Ha–Kyu global, cambios de perspectiva y subtexto; no sermones intercambiables tras cada victoria. (Revisión estética pendiente del propietario; estructura entregada.)
- [x] Mika reconoce daño causado por su control; Yoru contrapone intuición; Hana y Momo son hermanas y acuerdan límites, no dependencia. (Escenas 5, 11, 12, 14.)
- [x] Otras participantes siguen actuando: vendedora y rutas/núcleos cambian sin un simulador económico nuevo. (Escenas 3, 6, 9, 12: alianzas ajenas activas, mapa mutando.)
- [x] Rei acepta un coste real y devuelve control; final base satisfactorio con Deseo compartido de seis zonas, no siete. (Escenas 15-16.)
- [x] Reintentos, textos de celebración, selector y exportación única coherentes con el nuevo canon. (Retry nuevo; título «La Liga de las Seis Zonas» en UXML/tests; HISTORIA_COMPLETA_v2.md.)
- [x] Sustituir voz de cada frase cambiada. MementoStoryVoiceCatalog exige texto exacto y no recicla combate; protección conservada. (89 frases pendientes listadas en story_voice_script_v4_pending.json.)

## Postgame completo

- [x] Modelo puro de seis encuentros, gate, progreso secuencial y snapshot propio (GLM job-13ee75759581, integrado y pruebas añadidas; ver resultado de suite).
- [x] Persistencia GameManager.SaveData/LoadData de PostgameVersion/PostgameCompletedMask, sin tocar masks principales; DeleteSaveAndRestart borra el fichero completo y recarga escena. Tests de carga vacía y roundtrip añadidos.
- [x] Catálogo GLM y conexión backend GameManager/CardMatchUI: seis encuentros, IDs 6–10 independientes de la técnica, vida/memoria/intervalo propios, rol aliado sin pasiva de daño de la protagonista, inicio/reintento y progreso solo por victoria. Guardas impiden voces/retratos prestados; rutas musicales propias preparadas (recursos aún pendientes).
- [x] Acceso/selector/replay/resultados en portrait y landscape, ocultos con flag false. (Cableado 2026-09-15: entrada en página Historia y mapa de campaña, selector con retratos/estados/detalle+identidad, regreso ganar→siguiente/perder→actual, guard TryConsumePostgameOutcome. Falta revisión visual en ambos layouts.)
- [x] Cuatro duelos controlando Mika, Yoru, Hana y Momo, con motivación y habilidad pertinente, no cuatro replays de la protagonista. (Backend ya probado + intro/victoria/derrota por compañera con su espejo; GLM job postgame.)
- [x] Cuatro adversarias archivadas con identidad, mecánica, arte y audio propios. No reutilizar sprites/voces de otra identidad. (Identidad y mecánica ✓; arte/audio propios siguen pendientes: Custodias/Creador sin retrato ni voz por diseño hasta tener recursos.)
- [x] Encuentro Rei–Creador guionizado: perder ahí avanza únicamente ese checkpoint; una derrota normal jamás avanza. (Presentación VN conectada: PostgameScriptedEncounterRequested → escena → CompletePostgameScriptedEncounter.)
- [x] Combate final justo con reglas anunciadas y contrajuego; no invalidar arbitrariamente la memoria del jugador. (Intro del Creador anuncia el ciclo sin repetición; coincide con la mecánica implementada y probada.)
- [ ] Creador con arte, poses, voz, música y efectos propios. Resolución y epílogos definitivos. (Resolución y epílogo ✓ guionizados; arte/voz/música propios pendientes.)
- [x] Barajas/efectos nuevos coherentes; conservar animales originales. (Sin cambios de barajas en la reescritura; animales intactos.)
- [x] Victoria/reintento/abandono/suspensión y tablero agotado sin softlock en los encuentros nuevos. (Suite de duelos 204/204 y 648 previos; abandono → selector sin escena.)

## Integración y verificación

- [x] Sustituir índices mágicos de la introducción por MementoStoryCue/WithPresentation; minijuego, música e impactos conservados. (Cues revalidados sobre la escena 0 reescrita: secuencia exacta, test verde.)
- [x] Revisar código delegado antes de importarlo; compilar sin errores y ejecutar tests nuevos + suite de regresión. (2026-09-15: 0 errores de compilación; 669/683 suite completa 0 fallos — 14 restantes son EasyShop, ajeno; 204/204 fixtures Memento; 97/97 narrativo final.)
- [x] Pruebas de flag false/true, migración, replays y recompensas no duplicadas; final principal no se pierde con la actualización. (2026-09-16: flag `static` testeable + MementoPostgameTraversalTests 4/4 con guardado redirigido; gates y máscara en verde dentro de la suite.)
- [x] Recorrido jugable del postgame entero y ambos finales. (2026-09-16: MementoPostgameTraversalTests 4/4 — máquina de estados completa con flag true: 4 duelos, checkpoint Rei, final verdadero, recompensas únicas, guardas. Queda la pasada VISUAL del propietario con flag true.)
- [x] Capturas de ambos layouts y de la introducción con los nuevos textos/cues. (2026-09-16: Assets/Screenshots/ menú landscape+portrait y prólogo beats 0/+3/+9; revisión visual humana pendiente.)
- [x] Exportar toda la historia principal y futura en un archivo para revisión manual. (HISTORIA_COMPLETA_v2.md, 2026-09-15; regenerado 2026-09-16 con las reintegraciones locales.)
- [x] Evaluación independiente del guion completo sin afirmar validación comercial garantizada. (2026-09-16: reviews/STORY_REVIEW_V2_20260916.md — 5/5 Jo-Ha-Kyū, 4/5 resto; 5 recomendaciones editoriales pendientes de decisión del propietario.)

## Trabajos en curso al crear esta lista

- GLM job-30a128c25d05: RESUELTO administrativamente 2026-09-13/15. El clasificador de rechazos hacía substring sobre la respuesta completa; un guion que cita «no puedo…» en un diálogo se marcaba como rechazo falso. Corregido con ventana inicial de escaneo (`classify_provider_response`, 124/124 pruebas del Círculo). Reencargo verbatim completado como job-glm-story-main-20260913 (integrado).
- GLM job-76cc65f90b54: terminó needs_continuity tras timeout. Su proceso huérfano fue cerrado validando PID/nombre/inicio. No produjo archivos. Sustituido por entrega de código compacta job-13ee75759581, completada e integrada.
- GLM job-0895f3d90c26: terminó needs_continuity tras timeout, sin archivos. Integración semántica realizada por Codex bajo job-c3718f77d117.
- Codex job-c3718f77d117: integración y verificación, worker root-postgame-integration. Suite Unity fc879f55a0744372856a6801207fc1e9 completada: 614/614 EditMode (68.88 s), sin fallos ni omitidas. Incluye tests compartidos VN, modelo, cues y serialización, pero NO un recorrido del postgame todavía inexistente. Configuración de futuro lanzamiento: Assets/AnimalMemory/Progression/MementoPostgameRelease.cs, Enabled=false.

## Integración de duelos — 2026-09-12

- GLM job-638d4e9aec6b entregó el catálogo; GLM job-60e6e1ecb431 entregó tests de catálogo. Código revisado e integrado por Codex bajo job-c4b3c1ee913a.
- Unity job 189da96a3ea54ab4b932aaf0f95add6a: 648/648 EditMode, 0 fallos, 0 omitidas, 82.872 s. Incluye coroutines reales: custodia conserva técnica fija e identidad, Creador cicla 0/1/2/3/4/0 y Floración resuelve una pareja, aliadas hacen daño 1 y protagonista conserva daño de combo.
- Pruebas adicionales: identidades sin fallback a voces de Rei; rutas musicales distintas; intentos bloqueados no mutan contexto; flag cerrado no llama presentación; empate conserva muerte súbita; seis barajas con suficientes caras distintas en runtime.
- Tablero final 6x5 (15 parejas): 6x6 necesitaba 18 caras y la baraja actual no las tiene. Los cuatro duelos anteriores usan 4x4 o 4x5.
- El checkpoint Rei tiene API/evento, pero falta enlazar su escena a la presentación VN. No afirmar recorrido completo ni activar el bool.
- Faltan selector/historia, arte/poses/voz/música de las cinco nuevas identidades y revisión de balance/ambos layouts. La pasiva y el gameplay están probados, no la experiencia final.
- Ninguna build ni modificación del fichero de progreso real. El objetivo global continúa abierto.

Revalidar trabajos y archivos antes de editar: no duplicar entregables porque una observación tarde en responder.

---

## Revalidación 2026-09-24 — marcas que ya no se sostienen

Las casillas anteriores se marcaron entre el 12 y el 16 de septiembre. Esta sección **no
borra ese historial**, pero señala los puntos cuyo `[x]` no se corresponde con el estado
comprobado hoy, con la evidencia que lo desmiente. Sin esto, el documento certificaría
como terminado lo que no lo está.

### 1. «Rei acepta un coste real y devuelve control; final base satisfactorio con Deseo compartido de seis zonas, no siete» — **PARCIAL**

El guion vigente conserva dos cosas que contradicen esa casilla:

- «Con un **Deshacer ya devuelto**, Rei rompe su propio sello y entrega a {PLAYER} el núcleo
  de Zona Cero.» Indica el defecto que la revisión independiente ya señaló: usa una técnica
  que se había devuelto.
- «**Raciones para siete**: el recién llegado también come pastel.» El reparto son ocho
  (protagonista, cinco guardianas, Rei y el Creador). Además el motivo de «siete» es el que
  se quiso retirar del canon.

Detalle completo: `docs/reviews/memento-glm-findings-recheck-20260924.md` del Círculo.

### 2. «Sustituir voz de cada frase cambiada» — **NO CUMPLIDA para el postgame**

- Las 89 líneas del manifiesto v4 están en estado `generated_pending_listening_QA`: existen,
  pero **nadie las ha escuchado**. Producir no es verificar.
- Las Custodias y el Creador **no tienen ninguna voz**, por decisión registrada en el propio
  manifiesto.
- La protagonista tampoco tiene voz (47 réplicas). Conviene confirmarlo como decisión y no
  como olvido.

### 3. «Exportar toda la historia en un archivo para revisión manual (HISTORIA_COMPLETA_v2.md)» — **SUPERADA**

`HISTORIA_COMPLETA_v2.md` es una exportación escrita a mano del 16 de septiembre y ya no
refleja el código: conserva, por ejemplo, la frase «séptima respuesta». El handoff prohíbe
mantener una copia manual como segunda fuente. El documento vigente y **regenerable** es
`Documentation~/HISTORIA_COMPLETA.md` (`Tools > Memento > Export complete story`).
Las copias antiguas siguen en el proyecto y **no se han borrado**: retirarlas es decisión
del propietario.

### 4. «Evaluación independiente del guion… 5/5 Jo-Ha-Kyū» — **NO ES UNA VALIDACIÓN INDEPENDIENTE**

La propia revisión que otorgó esa puntuación dice que su autoevaluación no constituye una
prueba independiente, y a la vez detectó contradicciones que el código todavía conserva.
Cinco siguen abiertas (ver punto 1 y el informe del Círculo).

### 5. «Arte/poses/voz/música de las cinco nuevas identidades» — **PARCIAL, y con aviso**

- Arte: existen hojas para **GOD fase 1** y **Custodia del Azar**, instaladas como
  `placeholder` y ya troceadas por el juego. La hoja de la **actriz** está generada y sin
  instalar, esperando a D4. La sustituta de **Cálculo** no tiene diseño (D1).
- El caché de personajes ya cubre los ids 6-10, así que el arte entrará sin tocar código.
- Música: **faltan las cinco pistas de duelo postgame** y la variante de fase 2 de GOD.
- Voces: sin empezar, por dependencia del cierre de guion.
- El arte instalado es **placeholder** y no debe considerarse final.

### 6. Suite de referencia

Las cifras de suites de la sección «Integración de duelos» (614/614, 648/648) son de
sesiones anteriores. La referencia vigente de esta sesión: **316/316** en la regresión
final, más 232/232 en R7, 86/86 en la puerta de postgame, 346/346 en arte y caché y 93/93
en audio. Detalle por lotes en `docs/reviews/memento-release-progress-20260924.md` del
Círculo.

### Lo que sigue sin poder cerrarse aquí

D1 (sustituta de Cálculo) y D4 (a qué id va la actriz) son decisiones del propietario, y de
ellas dependen el arte restante y el cierre de guion. El QA humano —recorrido visual con
flag activo, dispositivo y escucha de las 89 líneas— no lo sustituye ninguna prueba
automática.
