using System;
using Minigames.Core;
using UnityEngine;

/// <summary>
/// Owns one memory attempt through the shared framework. Memory rules and coroutines
/// remain in the existing board; this adapter never creates a second turn loop.
/// </summary>
public sealed class MementoMemorySession : IDisposable
{
    private readonly MinigameDefinition definition;
    private readonly Minigame core;
    private bool disposed;

    public IMinigame Minigame => core;
    public MinigameStatus Status => core.Status;
    public event Action<MinigameResult> Completed;

    public MementoMemorySession(int seed = 0)
    {
        definition = ScriptableObject.CreateInstance<MinigameDefinition>();
        definition.hideFlags = HideFlags.HideAndDontSave;
        definition.MinigameId = "memento.memory";
        definition.DisplayName = "Memento Memory";
        var context = new MinigameContext(definition, seed);
        core = new Minigame(definition, context);
        core.Initialize(context);
        core.OnEnded += HandleEnded;
    }

    public bool Start(Action startBoard)
    {
        if (disposed || Status != MinigameStatus.Ready) return false;
        core.Start();
        try { startBoard?.Invoke(); }
        catch
        {
            Dispose();
            throw;
        }
        return true;
    }

    public bool TryComplete(MinigameResult result)
    {
        if (result == MinigameResult.None) return false;
        if (disposed || (Status != MinigameStatus.Playing && Status != MinigameStatus.Paused))
            return false;
        core.End(result);
        return true;
    }

    private void HandleEnded(MinigameResult result) => Completed?.Invoke(result);

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        core.OnEnded -= HandleEnded;
        Completed = null;
        try
        {
            if (core.Status != MinigameStatus.Ended) core.End(MinigameResult.Aborted);
        }
        finally
        {
            core.Dispose();
            if (Application.isPlaying) UnityEngine.Object.Destroy(definition);
            else UnityEngine.Object.DestroyImmediate(definition);
        }
    }
}
