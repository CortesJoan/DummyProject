using System;
using System.Collections.Generic;
using AnimalMemory.Progression;
using VN;

namespace AnimalMemory.Narrative
{
    /// <summary>
    /// Maps Memento story content onto the shared 30 Days VN command executor.
    /// This adapter owns no separate cursor, parser or save/load system. Metadata
    /// such as guardian portraits remains in the game's story model.
    /// </summary>
    public sealed class MementoNarrativeRunner
    {
        private readonly DialogueEngine engine =
            new DialogueEngine(new CommandInterpreter(new Tokenizer()), false);
        private IEnumerator<Command> execution;

        public MementoMatchStoryScene Scene { get; private set; }
        public int BeatIndex => Math.Max(0, engine.CurrentLineIndex - 1);
        public bool IsRunning => Scene != null;

        public MementoMatchStoryBeat CurrentBeat
        {
            get
            {
                if (!IsRunning || BeatIndex >= Scene.Beats.Count)
                    throw new InvalidOperationException("No narrative beat is active.");
                return Scene.Beats[BeatIndex];
            }
        }

        public void Begin(MementoMatchStoryScene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));
            if (scene.Beats.Count == 0)
                throw new ArgumentException("A narrative scene needs at least one beat.", nameof(scene));

            End();
            var commands = new List<Command>(scene.Beats.Count);
            foreach (MementoMatchStoryBeat beat in scene.Beats)
                commands.Add(new SayCommand(beat.Speaker, beat.Text));

            engine.LoadCommands("memento/" + scene.Id, commands);
            execution = engine.RunAllCommands().GetEnumerator();
            Scene = scene;
            execution.MoveNext();
        }

        public bool MoveNext()
        {
            return IsRunning && execution != null && execution.MoveNext();
        }

        public void End()
        {
            execution?.Dispose();
            execution = null;
            engine.UnloadScene();
            Scene = null;
        }
    }
}
