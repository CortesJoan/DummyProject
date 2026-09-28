using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMemory.Narrative
{
    /// <summary>
    /// Compact UI Toolkit typewriter adapted from 30 Days of Silence.
    /// It is intentionally linear: no history, rollback, save/load or branching state.
    /// </summary>
    public sealed class MementoTypewriterEffect : IDisposable
    {
        private readonly Label label;
        private IVisualElementScheduledItem scheduledItem;
        private string fullText = string.Empty;
        private int visibleCharacters;
        private double lastTickMilliseconds;
        private float charactersPerSecond;

        public bool IsRunning { get; private set; }
        public event Action Completed;

        public MementoTypewriterEffect(Label target)
        {
            label = target ?? throw new ArgumentNullException(nameof(target));
        }

        public void Play(string text, float speed = 42f)
        {
            Cancel();
            fullText = text ?? string.Empty;
            charactersPerSecond = Mathf.Max(1f, speed);
            visibleCharacters = 0;
            lastTickMilliseconds = Time.realtimeSinceStartupAsDouble * 1000d;
            label.text = string.Empty;
            IsRunning = fullText.Length > 0;
            if (!IsRunning)
            {
                Completed?.Invoke();
                return;
            }

            scheduledItem = label.schedule.Execute(Tick).Every(16);
        }

        public bool CompleteImmediately()
        {
            if (!IsRunning)
                return false;

            StopSchedule();
            visibleCharacters = fullText.Length;
            label.text = fullText;
            IsRunning = false;
            Completed?.Invoke();
            return true;
        }

        public void Cancel()
        {
            StopSchedule();
            IsRunning = false;
        }

        public void Dispose()
        {
            Cancel();
        }

        private void Tick()
        {
            if (!IsRunning)
                return;

            double now = Time.realtimeSinceStartupAsDouble * 1000d;
            double elapsedSeconds = Math.Max(0d, (now - lastTickMilliseconds) / 1000d);
            int revealCount = Math.Max(1, Mathf.FloorToInt((float)elapsedSeconds * charactersPerSecond));
            visibleCharacters = Math.Min(fullText.Length, visibleCharacters + revealCount);
            lastTickMilliseconds = now;
            label.text = fullText.Substring(0, visibleCharacters);
            if (visibleCharacters < fullText.Length)
                return;

            StopSchedule();
            IsRunning = false;
            Completed?.Invoke();
        }

        private void StopSchedule()
        {
            scheduledItem?.Pause();
            scheduledItem = null;
        }
    }
}
