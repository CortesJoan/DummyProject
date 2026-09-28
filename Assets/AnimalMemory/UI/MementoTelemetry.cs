using EHKP.GameAnalytics;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Game-specific event mapping. The reusable package knows nothing about Memento Match.</summary>
public sealed class MementoTelemetry : MonoBehaviour
{
    private GameManager manager;
    private CardMatchUI board;
    private TelemetryClient client;
    private bool wasPlaying;
    private int previousLevel = -2;
    private int previousCleared;
    private float levelStarted, heartbeat;
    private void Start()
    {
        manager = GameManager.Instance;
        board = FindFirstObjectByType<CardMatchUI>();
        client = TelemetryClient.Instance;
        if (client == null) client = gameObject.AddComponent<TelemetryClient>();
        if (manager != null)
        {
            manager.ResultChanged += Result;
            manager.GuidePowerPresentationRequested += Skill;
            manager.ProgressionChanged += Progression;
            previousCleared = manager.CampaignClearedCount;
        }
        if (board != null) board.PlayerMoveResolved += Move;
        var document = GetComponent<UIDocument>();
        var host = document != null ? document.rootVisualElement.Q<ScrollView>("settings-scroll") : null;
        if (host != null)
        {
            var card = new VisualElement(); card.AddToClassList("audio-mixer-card");
            var title = new Label(client.RemoteConfigured ? "ANALÍTICAS OPCIONALES" : "DIAGNÓSTICO LOCAL OPCIONAL");
            title.AddToClassList("audio-mixer-title"); card.Add(title);
            var copy = new Label(client.RemoteConfigured
                ? "Si lo permites, se enviarán avances, intentos, escenas, habilidades y datos de sesión a nuestro servidor para mejorar el juego. Usamos un identificador aleatorio, no tu nombre ni un identificador publicitario."
                : "Puedes registrar avances, intentos, escenas y habilidades solo en este dispositivo. No hay servidor configurado: no se enviará nada. No se registra el nombre de tu protagonista.");
            copy.AddToClassList("orientation-hint"); card.Add(copy);
            var toggle = new Toggle(client.RemoteConfigured ? "Permitir analíticas de juego" : "Activar registro local") { value = client.HasConsent };
            toggle.AddToClassList("telemetry-toggle");
            toggle.RegisterValueChangedCallback(evt => client.SetConsent(evt.newValue));
            card.Add(toggle);
            var revoke = new Label("Desactivarlo borra la cola local y el identificador. No afecta a tu partida.");
            revoke.AddToClassList("orientation-hint"); card.Add(revoke);
            host.Add(card);
        }
    }
    private void Update()
    {
        if (manager == null) return;
        bool playing = manager.IsGameplayActive;
        if (playing && (!wasPlaying || previousLevel != manager.ActiveCampaignLevel))
        {
            levelStarted = Time.unscaledTime; heartbeat = Time.unscaledTime + 30f;
            Record("level_start"); previousLevel = manager.ActiveCampaignLevel;
        }
        if (!playing && wasPlaying && !manager.IsResultOverlayOpen) Record("level_abandon");
        if (playing && Time.unscaledTime >= heartbeat)
        { heartbeat = Time.unscaledTime + 30f; Record("level_checkpoint", Mathf.RoundToInt(Time.unscaledTime - levelStarted)); }
        wasPlaying = playing;
    }
    private void Move(bool matched, int combo) => Record("move_resolved", matched ? 1 : 0);
    private void Skill(int guideId, bool resolved) => Record(resolved ? "skill_effect" : "skill_cast", guideId);
    private void Result()
    {
        if (manager != null && manager.IsResultOverlayOpen)
            Record(manager.LastResultWasVictory ? "level_win" : "level_loss", Mathf.RoundToInt(Time.unscaledTime - levelStarted));
    }
    private void Progression()
    {
        if (manager == null || manager.CampaignClearedCount <= previousCleared) return;
        previousCleared = manager.CampaignClearedCount;
        Record("campaign_progress", previousCleared);
    }
    public static void Record(string name, int value = 0, int scene = -1, int beat = -1)
    {
        var client = TelemetryClient.Instance;
        if (client == null || !client.HasConsent) return;
        var game = GameManager.Instance;
        client.Track(name,
            level: game != null ? game.ActiveCampaignLevel : -1, scene: scene, beat: beat,
            guide: game != null ? game.SelectedGuideId : -1, set: game != null ? game.SelectedSetId : -1,
            turns: game != null ? game.CurrentTurns : 0, pairs: game != null ? game.CurrentMatches : 0,
            combo: game != null ? game.CurrentCombo : 0, playerHealth: game != null ? game.DuelPlayerHealth : 0,
            rivalHealth: game != null ? game.DuelOpponentHealth : 0, value: value);
    }
    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.ResultChanged -= Result;
            manager.GuidePowerPresentationRequested -= Skill;
            manager.ProgressionChanged -= Progression;
        }
        if (board != null) board.PlayerMoveResolved -= Move;
    }
}
