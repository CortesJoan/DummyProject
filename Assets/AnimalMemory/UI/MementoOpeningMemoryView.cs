using System;
using System.Collections.Generic;
using AnimalMemory.Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMemory.UI
{
    /// <summary>Presentation-only practice/cinematic. Never starts a real campaign level.</summary>
    public sealed class MementoOpeningMemoryView : IDisposable
    {
        private readonly VisualElement overlay;
        private readonly VisualElement grid;
        private readonly VisualElement panel;
        private readonly Label title;
        private readonly Label copy;
        private readonly Label status;
        private readonly Button continueButton;
        private readonly List<Button> cards = new List<Button>();
        private readonly List<IVisualElementScheduledItem> scheduled = new List<IVisualElementScheduledItem>();
        private readonly IReadOnlyList<Sprite> art;
        private readonly MementoPracticeBoard board;
        private Action completed;
        private readonly bool cinematic;
        private bool disposed;
        private bool preview = true;
        private bool complete;
        public bool IsOpen => !disposed;

        public MementoOpeningMemoryView(VisualElement root, bool cinematic, Action completed, Action cancelled = null)
        {
            this.completed = completed;
            this.cinematic = cinematic;
            board = new MementoPracticeBoard(Environment.TickCount);
            var catalog = Resources.Load<AnimalMemoryCardSetCatalog>("AnimalMemory/AnimalMemoryCardSetCatalog");
            art = cinematic ? MementoIllustratedCards.GetSprites(5) : catalog != null ? catalog.GetSprites(0) : null;
            overlay = new VisualElement { name = cinematic ? "opening-ambush" : "opening-practice" };
            overlay.AddToClassList("opening-memory-overlay");
            panel = new VisualElement();
            panel.AddToClassList("opening-memory-panel");
            panel.Add(new Label(cinematic ? "EMBOSCADA · ESCENA" : "ANTES DE EMPEZAR"));
            title = new Label(cinematic ? "NO ME DA TIEMPO…" : "DOS PAREJAS. TU PRIMER PASO.");
            title.AddToClassList("opening-memory-title"); panel.Add(title);
            copy = new Label(cinematic
                ? "La campeona encadena sus ataques antes de que puedas responder."
                : "Mira dónde está cada dibujo. Después, toca dos cartas iguales.");
            copy.AddToClassList("opening-memory-copy"); panel.Add(copy);
            status = new Label(cinematic ? "ZONA CERO · INTEGRIDAD 100%" : "MEMORIZA");
            status.AddToClassList("opening-memory-status"); panel.Add(status);
            grid = new VisualElement(); grid.AddToClassList("opening-memory-grid");
            grid.EnableInClassList("cinematic", cinematic);
            grid.RegisterCallback<GeometryChangedEvent>(RelayoutCards);
            panel.Add(grid);
            int count = cinematic ? 30 : 4;
            for (int index = 0; index < count; index++)
            {
                int captured = index;
                var button = new Button(() => Flip(captured));
                button.AddToClassList("opening-memory-card");
                button.EnableInClassList("cinematic", cinematic);
                cards.Add(button); grid.Add(button);
                if (cinematic) { ShowFace(index, index / 2); button.SetEnabled(false); }
            }
            continueButton = new Button(() => { if (complete || cinematic) Finish(); });
            continueButton.text = cinematic ? "OMITIR ESCENA" : "CONTINUAR";
            continueButton.AddToClassList("page-primary-button");
            continueButton.SetEnabled(cinematic);
            panel.Add(continueButton);
            if (!cinematic)
            {
                var back = new Button(() => { Dispose(); cancelled?.Invoke(); }) { text = "VOLVER AL MENÚ" };
                back.AddToClassList("page-secondary-button"); panel.Add(back);
            }
            overlay.RegisterCallback<GeometryChangedEvent>(Relayout);
            overlay.Add(panel); root.Add(overlay);
            if (cinematic) StartAmbush();
            else
            {
                PaintPractice();
                Later(() => { preview = false; status.text = "ENCUENTRA LAS DOS PAREJAS"; PaintPractice(); }, 2200);
            }
        }

        private void RelayoutCards(GeometryChangedEvent evt)
        {
            int columns = cinematic ? 6 : 2, rows = cinematic ? 5 : 2;
            float margin = cinematic ? 3f : 7f;
            float size = Mathf.Max(12f, Mathf.Min(evt.newRect.height / rows - margin * 2f,
                (evt.newRect.width / columns - margin * 2f) / 0.8f));
            float width = size * 0.8f;
            float left = (evt.newRect.width - columns * (width + margin * 2f)) * 0.5f;
            float top = (evt.newRect.height - rows * (size + margin * 2f)) * 0.5f;
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.style.position = Position.Absolute;
                card.style.width = width;
                card.style.height = size;
                card.style.left = left + (i % columns) * (width + margin * 2f) + margin;
                card.style.top = top + (i / columns) * (size + margin * 2f) + margin;
                card.style.marginLeft = card.style.marginRight = 0;
                card.style.marginTop = card.style.marginBottom = 0;
            }
        }

        private void Relayout(GeometryChangedEvent evt)
        {
            float scale = Mathf.Clamp(Mathf.Min(evt.newRect.width, evt.newRect.height) / 600f, 1f, 2f);
            panel.style.maxWidth = 700f * scale;
            title.style.fontSize = 23f * scale;
            copy.style.fontSize = 16f * scale;
            status.style.fontSize = 15f * scale;
            foreach (var button in panel.Children())
            {
                if (!(button is Button)) continue;
                button.style.height = 44f * scale;
                button.style.minHeight = 44f * scale;
                button.style.fontSize = 16f * scale;
            }
        }

        private void Later(Action action, long milliseconds)
        {
            scheduled.Add(overlay.schedule.Execute(() => { if (!disposed) action(); }).StartingIn(milliseconds));
        }
        private void Flip(int index)
        {
            if (preview || complete || !board.TryFlip(index)) return;
            MementoMatchSfx.PlayCardFlip();
            PaintPractice();
            if (!board.AwaitingResolution) return;
            Later(() =>
            {
                bool matched = board.Resolve();
                if (matched) MementoMatchSfx.PlayComboPulse(board.Pairs);
                else MementoMatchSfx.PlayMatchFail();
                complete = board.IsComplete;
                copy.text = complete
                    ? "¡Lo tienes! Las parejas seguidas forman un combo. En los duelos, tu combo aumenta el daño. Las habilidades llegarán cuando hagas tus primeras alianzas."
                    : matched ? "¡Primera pareja! Encuentra la otra sin fallar para encadenarlas."
                    : "No pasa nada: recuerda esos dibujos y prueba otra pareja.";
                status.text = complete ? "LISTA PARA COMENZAR" : board.Pairs + " / 2 PAREJAS";
                continueButton.SetEnabled(complete);
                PaintPractice();
            }, 650);
        }
        private void PaintPractice()
        {
            for (int i = 0; i < 4; i++)
            {
                bool visible = preview || board.IsVisible(i);
                cards[i].SetEnabled(!preview && !board.IsMatched(i) && !board.AwaitingResolution);
                cards[i].EnableInClassList("matched", board.IsMatched(i));
                if (visible) ShowFace(i, board.Identity(i));
                else { cards[i].style.backgroundImage = StyleKeyword.None; cards[i].text = "✦"; }
            }
        }
        private void ShowFace(int index, int identity)
        {
            cards[index].text = art != null && art.Count > identity ? "" : (identity + 1).ToString();
            if (art != null && art.Count > identity)
                cards[index].style.backgroundImage = new StyleBackground(art[identity]);
        }
        private void StartAmbush()
        {
            MementoMatchSfx.PlayBoardDeal();
            for (int pair = 0; pair < 15; pair++)
            {
                int captured = pair;
                Later(() =>
                {
                    cards[captured * 2].style.opacity = 0.12f;
                    cards[captured * 2 + 1].style.opacity = 0.12f;
                    MementoMatchSfx.PlayDuelLaunch(true);
                    status.text = "ZONA CERO · INTEGRIDAD " + Mathf.RoundToInt((14 - captured) * 100f / 15f) + "%";
                    overlay.EnableInClassList("impact", true);
                    Later(() => overlay.RemoveFromClassList("impact"), 100);
                    if (captured % 3 == 2) MementoMatchSfx.PlayDuelImpact(true);
                }, 1200 + pair * 270);
            }
            Later(() =>
            {
                complete = true;
                status.text = "ZONA CERO HA CAÍDO";
                copy.text = "Has perdido tu territorio, no tu oportunidad. Esto acaba de empezar.";
                MementoMatchSfx.PlayResult(MementoMatchResultSfx.Defeat);
                continueButton.text = "LEVANTARME";
            }, 5400);
        }
        private void Finish()
        {
            Action next = completed;
            Dispose();
            next?.Invoke();
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var item in scheduled) item.Pause();
            scheduled.Clear(); completed = null;
            overlay.UnregisterCallback<GeometryChangedEvent>(Relayout);
            grid.UnregisterCallback<GeometryChangedEvent>(RelayoutCards);
            overlay.RemoveFromHierarchy();
        }
    }
}
