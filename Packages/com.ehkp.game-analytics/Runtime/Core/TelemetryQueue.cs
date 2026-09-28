using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EHKP.GameAnalytics
{
    /// <summary>Versioned, bounded event shape. Context is numeric IDs, never dialogue or player names.</summary>
    [Serializable]
    public sealed class TelemetryEvent
    {
        public int schema = 1;
        public string eventId, installationId, sessionId, name, build, platform;
        public long utcSeconds;
        public int sequence, level = -1, scene = -1, beat = -1, guide = -1, set = -1;
        public int turns, pairs, combo, playerHealth, rivalHealth, value, width, height;
    }

    /// <summary>Platform independent retention/consent and at-least-once delivery queue.</summary>
    public sealed class TelemetryQueue
    {
        public const int Capacity = 512;
        public const long RetentionSeconds = 14 * 86400;
        private readonly List<TelemetryEvent> pending = new List<TelemetryEvent>();
        private readonly Func<long> clock;
        public bool Consent { get; private set; }
        public int Count => pending.Count;
        public int Dropped { get; private set; }
        public TelemetryQueue(Func<long> clock) { this.clock = clock ?? throw new ArgumentNullException(nameof(clock)); }
        public void SetConsent(bool value)
        {
            Consent = value;
            if (!value) { pending.Clear(); Dropped = 0; }
        }
        public bool Enqueue(TelemetryEvent value)
        {
            if (!Consent || value == null || value.schema != 1 || !ValidName(value.name) ||
                !ValidId(value.eventId) || !ValidId(value.installationId) || !ValidId(value.sessionId) ||
                value.utcSeconds < clock() - RetentionSeconds || value.utcSeconds > clock() + 300 ||
                value.build == null || value.build.Length > 64 ||
                value.platform == null || value.platform.Length > 32) return false;
            Prune();
            if (pending.Exists(x => x.eventId == value.eventId)) return false;
            if (pending.Count == Capacity) { pending.RemoveAt(0); Dropped++; }
            pending.Add(value);
            return true;
        }
        public TelemetryEvent[] Peek(int count)
        {
            Prune();
            return pending.Take(Math.Max(0, Math.Min(count, 32))).ToArray();
        }
        public TelemetryEvent[] Snapshot() { Prune(); return pending.ToArray(); }
        public void Acknowledge(IEnumerable<string> ids)
        {
            var acknowledged = new HashSet<string>(ids ?? Array.Empty<string>());
            pending.RemoveAll(x => acknowledged.Contains(x.eventId));
        }
        public void Prune() { long oldest = clock() - RetentionSeconds; pending.RemoveAll(x => x.utcSeconds < oldest); }
        public static bool ValidName(string name) =>
            name != null && Regex.IsMatch(name, @"\A[a-z][a-z0-9_]{0,47}\z");
        public static bool ValidId(string id) => Guid.TryParseExact(id, "N", out _);
        public static bool ValidEndpoint(string endpoint)
        {
            Uri uri;
            return Uri.TryCreate(endpoint, UriKind.Absolute, out uri) && uri.Scheme == "https" &&
                string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) &&
                string.IsNullOrEmpty(uri.Fragment);
        }
        public static float RetrySeconds(int failures) =>
            (float)Math.Min(900, 5 * Math.Pow(2, Math.Max(0, Math.Min(8, failures))));
    }
}
