# Suministros, retratos y salto de habilidad — 2026-09-12

## Cambios
- Tienda separada, accesible desde TIENDA en la cabecera del menú.
- Monedas de suministros independientes de las estrellas: 10 iniciales, 4 por victoria, 5 por ayuda. Máximo 9 unidades de cada tipo.
- Vista de fila / columna: elegir una línea, mostrar sus cartas ocultas durante 3 segundos, ocultarlas de nuevo. No resuelve parejas, no modifica turnos, combo o vida, ni entrega esa información privada a la IA.
- Hasta 3 usos por intento. Se conserva el límite al continuar tras una derrota o entrar en un desempate. Se reinicia al empezar otra partida.
- Las ayudas se llevan automáticamente en la bolsa y no sustituyen la habilidad de una guía. Abrir AYUDAS requiere turno de la jugadora, tablero estable y ninguna carta a medio seleccionar.
- Se cobra una unidad solo si empieza un efecto válido; una línea vacía, índice inválido, falta de stock o estado ocupado no la consume.
- El guardado incorpora SuppliesVersion/Coins/Rows/Columns. Migración de partidas antiguas: 10 + 4 por victoria registrada. La versión persistida evita repetir el crédito. No se modifican estrellas, desbloqueos, selección ni historial.
- Los seis retratos comparten ahora recortes específicos de su arte neutral, con límites seguros; no se modificó el alfa de las ilustraciones.
- Un clic/toque en el cut-in salta imagen y voz, invalida sus cierres programados y libera el temporizador. El efecto real continúa y conserva su cooldown. Breve guardia de entrada de 0,12 s evita que el mismo gesto voltee una carta. La espera mínima enemiga se arma antes de la notificación, nunca después del salto.

## Separación de responsabilidades
- AnimalMemory/Progression/MementoSupplyWallet.cs: reglas de moneda e inventario.
- CardMatchPrototype/CardMatchUI.cs: legalidad, selección de línea y revelación temporal.
- CardMatchPrototype/GameManager.cs: compra/consumo, recompensas y serialización.
- AnimalMemory/UI/MementoSuppliesView.cs: tienda y bolsa, sin reglas duplicadas.
- AnimalMemory/UI/MementoPortraitFraming.cs: metadatos de encuadre.
- AnimalMemory/UI/AnimalMemoryMenuController.cs: integración y salto del cut-in.

## Exportación narrativa
HISTORIA_COMPLETA_MEMENTO_MATCH.md contiene las 17 escenas canónicas (128 intervenciones), seis variantes de reintento y rótulos de la apertura interactiva y del cierre. Incluye WithParty. No se reescribió la narrativa ni se envió a otro proveedor.

## Verificación
- Regresión específica inicial: 22/22.
- Regresión final de juego + VN compartido: **585/585**, cero fallos y cero omitidos, 67,52 s.
- Job Unity: 5a57cf0e7ea34bb1a1f40652d2152476.
- Assemblies: Assembly-CSharp-Editor, AnimalMemory.Effects.Tests, AnimalMemory.Progression.Tests, VN.Engine.Shared.Tests.
- Nuevas pruebas: saldo, inventario, capacidad, datos corruptos, migración, serialización de GameManager sin tocar archivos reales, encuadres, columna/fila reales, rechazo por selección parcial, cancelación, límite de intento, salto de visión y salto de habilidad enemiga.
- Revisión visual de tienda/bolsa, HUD y retratos de cierre en orientación móvil; tienda también en horizontal. Clic enviado a la interfaz real confirmó ocultación del cut-in.
- No se generó APK/AAB ni se cambió el perfil real mediante compras de prueba. Valores observados al cargar: 90 estrellas, 25 victorias, campaña terminada; crédito de suministros calculado: 110 monedas.
- No se añadió SDK de anuncios, dinero real, analíticas externas ni despliegues.
- Balance económico inicial: funcional y probado mecánicamente; la dificultad percibida de Rei requiere una partida humana con las nuevas ayudas.
