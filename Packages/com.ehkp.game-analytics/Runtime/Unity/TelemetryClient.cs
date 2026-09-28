using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EHKP.GameAnalytics
{
    /// <summary>Owns a small local queue. Sending is impossible without opt-in and a valid HTTPS endpoint.</summary>
    public sealed class TelemetryClient : MonoBehaviour
    {
        [Serializable] private sealed class QueueFile { public TelemetryEvent[] events; }
        [Serializable] private sealed class Batch { public int schema = 1; public string game; public TelemetryEvent[] events; }
        [Serializable] private sealed class Ack { public string[] acceptedEventIds; }
        [Serializable] private sealed class Configuration { public string endpoint; }
        private const string ConsentKey = "EHKP.Telemetry.Consent.v1";
        private const string IdKey = "EHKP.Telemetry.Installation.v1";
        private const string PurposeKey = "EHKP.Telemetry.ConsentPurpose.v1";
        private string Purpose => RemoteConfigured ? endpoint : "local-only";
        private TelemetryQueue queue;
        private string installationId, sessionId, endpoint;
        private int sequence, failures;
        private float nextFlush;
        private UnityWebRequest request;
        private bool sending;
        public static TelemetryClient Instance { get; private set; }
        public bool HasConsent => queue != null && queue.Consent;
        public bool RemoteConfigured => TelemetryQueue.ValidEndpoint(endpoint);
        public int PendingCount => queue == null ? 0 : queue.Count;
        private string QueuePath => Path.Combine(Application.persistentDataPath, "ehkp-telemetry-v1.json");
        public static long UtcNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            queue = new TelemetryQueue(UtcNow);
            TextAsset config = Resources.Load<TextAsset>("ehkp-analytics");
            if (config != null)
            {
                try { endpoint = JsonUtility.FromJson<Configuration>(config.text)?.endpoint; }
                catch (ArgumentException) { endpoint = null; }
            }
            if (PlayerPrefs.GetInt(ConsentKey, 0) == 1 &&
                PlayerPrefs.GetString(PurposeKey, "") == Purpose)
            {
                EnableSession();
                Restore();
                Track("session_start");
            }
            else DeleteQueue();
            nextFlush = Time.unscaledTime + 30f;
        }
        private void EnableSession()
        {
            installationId = PlayerPrefs.GetString(IdKey, "");
            if (!TelemetryQueue.ValidId(installationId))
            {
                installationId = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(IdKey, installationId);
                PlayerPrefs.Save();
            }
            sessionId = Guid.NewGuid().ToString("N");
            sequence = 0;
            queue.SetConsent(true);
        }
        public void SetConsent(bool value)
        {
            if (value == HasConsent) return;
            PlayerPrefs.SetInt(ConsentKey, value ? 1 : 0);
            PlayerPrefs.SetString(PurposeKey, value ? Purpose : "");
            if (!value)
            {
                request?.Abort();
                queue.SetConsent(false);
                installationId = sessionId = null;
                PlayerPrefs.DeleteKey(IdKey);
                DeleteQueue();
            }
            else { EnableSession(); Track("session_start"); }
            PlayerPrefs.Save();
        }
        public void Track(string name, int level = -1, int scene = -1, int beat = -1,
            int guide = -1, int set = -1, int turns = 0, int pairs = 0, int combo = 0,
            int playerHealth = 0, int rivalHealth = 0, int value = 0)
        {
            if (!HasConsent) return;
            var entry = new TelemetryEvent {
                eventId = Guid.NewGuid().ToString("N"), installationId = installationId,
                sessionId = sessionId, sequence = ++sequence, utcSeconds = UtcNow(),
                name = name, build = Clip(Application.version, 64), platform = Clip(Application.platform.ToString(), 32),
                level = level, scene = scene, beat = beat, guide = guide, set = set,
                turns = turns, pairs = pairs, combo = combo, playerHealth = playerHealth,
                rivalHealth = rivalHealth, value = value, width = Screen.width, height = Screen.height
            };
            if (queue.Enqueue(entry)) Persist();
        }
        private static string Clip(string value, int maximum) =>
            string.IsNullOrEmpty(value) ? "unknown" : value.Substring(0, Math.Min(value.Length, maximum));
        private void Update()
        {
            if (!HasConsent || !RemoteConfigured || sending || Time.unscaledTime < nextFlush) return;
            nextFlush = Time.unscaledTime + 30f;
            if (queue.Count > 0) StartCoroutine(Flush());
        }
        private IEnumerator Flush()
        {
            TelemetryEvent[] batch = queue.Peek(32);
            if (batch.Length == 0 || !HasConsent || !RemoteConfigured) yield break;
            sending = true;
            string sessionAtSend = sessionId;
            var body = JsonUtility.ToJson(new Batch { game = Application.identifier, events = batch });
            using (var transport = new UnityWebRequest(endpoint, "POST"))
            {
                request = transport;
                transport.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                transport.downloadHandler = new DownloadHandlerBuffer();
                transport.SetRequestHeader("Content-Type", "application/json");
                transport.timeout = 10;
                transport.redirectLimit = 0;
                yield return transport.SendWebRequest();
                bool accepted = false;
                if (HasConsent && sessionAtSend == sessionId &&
                    transport.result == UnityWebRequest.Result.Success &&
                    transport.downloadedBytes <= 16384)
                {
                    try
                    {
                        Ack ack = JsonUtility.FromJson<Ack>(transport.downloadHandler.text);
                        if (ack != null && ack.acceptedEventIds != null)
                        {
                            var ids = batch.Select(x => x.eventId).Intersect(ack.acceptedEventIds).ToArray();
                            queue.Acknowledge(ids);
                            accepted = ids.Length == batch.Length;
                            Persist();
                        }
                    }
                    catch (ArgumentException) { /* Invalid acknowledgement: keep queue for retry. */ }
                }
                failures = accepted ? 0 : Math.Min(8, failures + 1);
                nextFlush = Time.unscaledTime + (accepted ? 5f : TelemetryQueue.RetrySeconds(failures));
                request = null;
            }
            sending = false;
        }
        private void Persist()
        {
            if (!HasConsent) return;
            try
            {
                string temporary = QueuePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(new QueueFile { events = queue.Snapshot() }));
                if (File.Exists(QueuePath)) File.Delete(QueuePath);
                File.Move(temporary, QueuePath);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Telemetry queue storage unavailable; gameplay is unaffected."); }
        }
        private void Restore()
        {
            try
            {
                if (!File.Exists(QueuePath)) return;
                if (new FileInfo(QueuePath).Length > 1048576) { DeleteQueue(); return; }
                var saved = JsonUtility.FromJson<QueueFile>(File.ReadAllText(QueuePath));
                if (saved?.events == null) return;
                foreach (var entry in saved.events)
                    if (entry != null && entry.installationId == installationId) queue.Enqueue(entry);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { DeleteQueue(); }
        }
        private void DeleteQueue()
        {
            try { if (File.Exists(QueuePath)) File.Delete(QueuePath); if (File.Exists(QueuePath + ".tmp")) File.Delete(QueuePath + ".tmp"); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Unable to erase the local telemetry queue."); }
        }
        private void OnApplicationPause(bool pause) { Track(pause ? "app_pause" : "app_resume"); }
        private void OnApplicationQuit() { Track("session_end"); Persist(); }
        private void OnDestroy()
        {
            request?.Abort();
            if (Instance == this) Instance = null;
        }
    }
}
