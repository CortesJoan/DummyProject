using System;
using AnimalMemory.Progression;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Store and inventory presentation; wallet and board own all game rules.</summary>
public sealed class MementoSuppliesView : IDisposable
{
    private readonly GameManager manager;
    private readonly CardMatchUI board;
    private readonly Button storeEntry, inventoryEntry;
    private readonly VisualElement overlay, items;
    private readonly Label title, balance, explanation, feedback;
    private bool open, shopping;
    private int cachedCoins = -1, cachedRows = -1, cachedColumns = -1;

    public MementoSuppliesView(VisualElement root, GameManager manager, CardMatchUI board)
    {
        this.manager = manager; this.board = board;
        storeEntry = root.Q<Button>("shop-open");
        inventoryEntry = root.Q<Button>("supplies-open");
        overlay = new VisualElement { name = "supplies-overlay", pickingMode = PickingMode.Position };
        overlay.AddToClassList("supplies-overlay");
        var panel = new VisualElement();
        panel.AddToClassList("supplies-panel");
        overlay.Add(panel);
        var heading = new VisualElement();
        heading.AddToClassList("supplies-heading");
        panel.Add(heading);
        title = new Label(); title.AddToClassList("supplies-title"); heading.Add(title);
        var close = new Button(Close) { text = "CERRAR" };
        close.AddToClassList("utility-button"); heading.Add(close);
        balance = new Label(); balance.AddToClassList("supplies-balance"); panel.Add(balance);
        explanation = new Label(); explanation.AddToClassList("supplies-copy"); panel.Add(explanation);
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.AddToClassList("supplies-scroll");
        scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        panel.Add(scroll);
        items = new VisualElement(); scroll.Add(items);
        feedback = new Label(); feedback.AddToClassList("supplies-feedback"); panel.Add(feedback);
        root.Add(overlay);
        if (storeEntry != null) storeEntry.clicked += OpenStore;
        if (inventoryEntry != null) inventoryEntry.clicked += OpenInventory;
        Close();
    }

    public void OpenStore()
    {
        if (manager == null || !manager.IsMainMenuOpen) return;
        shopping = true; Show();
    }

    public void OpenInventory()
    {
        if (manager == null || !manager.IsGameplayActive || board == null ||
            !board.CanUseSupplies) return;
        shopping = false; Show();
    }

    private void Show()
    {
        open = true;
        feedback.text = "";
        overlay.style.display = DisplayStyle.Flex;
        overlay.BringToFront();
        title.text = shopping ? "TIENDA DE SUMINISTROS" : "TU BOLSA DE AYUDAS";
        explanation.text = shopping
            ? "Ayudas de un uso. Se llevan automáticamente a la partida, sin sustituir a tu guía. Ganas 4 monedas por victoria. Máximo 3 ayudas por intento."
            : "Elige una línea: se descubre durante 3 segundos sin gastar turno ni romper el combo. Filas de arriba abajo; columnas de izquierda a derecha.";
        Populate(); Refresh();
    }

    private void Populate()
    {
        if (manager == null) return;
        items.Clear();
        AddItem(MementoSupplyKind.Row, "VISTA DE FILA", "Recuerda todas las cartas ocultas de una fila.", board != null ? board.BoardRows : 0);
        AddItem(MementoSupplyKind.Column, "VISTA DE COLUMNA", "Recuerda todas las cartas ocultas de una columna.", board != null ? board.BoardColumns : 0);
        cachedCoins = manager.Supplies.Coins;
        cachedRows = manager.Supplies.Rows;
        cachedColumns = manager.Supplies.Columns;
    }

    private void AddItem(MementoSupplyKind kind, string name, string detail, int lines)
    {
        var card = new VisualElement(); card.AddToClassList("supply-item"); items.Add(card);
        var art = new VisualElement(); art.AddToClassList("supply-symbol"); card.Add(art);
        for (int i = 0; i < 9; i++)
        {
            var tile = new VisualElement(); tile.AddToClassList("supply-symbol-tile");
            tile.EnableInClassList("lit", kind == MementoSupplyKind.Row ? i / 3 == 1 : i % 3 == 1);
            art.Add(tile);
        }
        var text = new Label(name + "  ·  ×" + manager.Supplies.Count(kind));
        text.AddToClassList("supply-item-title"); card.Add(text);
        var copy = new Label(detail); copy.AddToClassList("supplies-copy"); card.Add(copy);
        if (shopping)
        {
            var buy = new Button(() =>
            {
                bool bought = manager.TryBuySupply(kind);
                feedback.text = bought ? "Guardado en tu bolsa. Lo encontrarás en AYUDAS durante la partida." : "No se pudo comprar: comprueba el saldo y el espacio.";
                if (bought) MementoMatchSfx.PlayUiConfirm(); else MementoMatchSfx.PlayUiLocked();
                Populate();
            }) { text = manager.Supplies.Count(kind) >= MementoSupplyWallet.MaxStock
                ? "BOLSA LLENA · MÁXIMO 9"
                : "COMPRAR · " + MementoSupplyWallet.Price + " MONEDAS" };
            buy.AddToClassList("supply-action"); buy.SetEnabled(manager.Supplies.CanBuy(kind)); card.Add(buy);
        }
        else
        {
            if (manager.Supplies.Count(kind) == 0)
            {
                var none = new Label("Sin unidades. Puedes comprarlas en la tienda del menú.");
                none.AddToClassList("supplies-copy"); card.Add(none); return;
            }
            var choices = new VisualElement(); choices.AddToClassList("supply-line-choices"); card.Add(choices);
            for (int line = 0; line < lines; line++)
            {
                int selected = line;
                var choose = new Button(() =>
                {
                    if (manager.TryUseSupply(kind, selected)) Close();
                    else { feedback.text = "No se ha gastado ninguna ayuda. Espera a tu turno y elige una línea con cartas."; Populate(); }
                }) { text = (kind == MementoSupplyKind.Row ? "FILA " : "COLUMNA ") + (line + 1) };
                choose.AddToClassList("supply-line");
                choose.SetEnabled(board.CanRevealSupplyLine(kind, line)); choices.Add(choose);
            }
        }
    }

    public void Refresh()
    {
        if (manager == null) return;
        overlay.EnableInClassList("portrait", Screen.height > Screen.width);
        if (inventoryEntry != null)
        {
            inventoryEntry.text = "AYUDAS · " + (manager.Supplies.Rows + manager.Supplies.Columns);
            inventoryEntry.SetEnabled(board != null && board.CanUseSupplies &&
                manager.IsGameplayActive && !manager.IsResultOverlayOpen);
        }
        if (!open) return;
        if ((shopping && !manager.IsMainMenuOpen) ||
            (!shopping && (!manager.IsGameplayActive || manager.IsResultOverlayOpen))) { Close(); return; }
        balance.text = shopping ? manager.Supplies.Coins + " MONEDAS · las estrellas no se gastan"
            : board.SupplyUsesRemaining + " / " + CardMatchUI.SuppliesPerAttempt + " USOS RESTANTES EN ESTE INTENTO";
        if (cachedCoins != manager.Supplies.Coins || cachedRows != manager.Supplies.Rows ||
            cachedColumns != manager.Supplies.Columns) Populate();
    }

    public void Close() { open = false; overlay.style.display = DisplayStyle.None; }

    public void Dispose()
    {
        if (storeEntry != null) storeEntry.clicked -= OpenStore;
        if (inventoryEntry != null) inventoryEntry.clicked -= OpenInventory;
        overlay.RemoveFromHierarchy();
    }
}
