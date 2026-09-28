using System;

namespace AnimalMemory.Progression
{
    public enum MementoSupplyKind { Row, Column }

    /// <summary>Soft-currency purchases, independent of stars and guardian skills.</summary>
    public sealed class MementoSupplyWallet
    {
        public const int Price = 5;
        public const int VictoryCoins = 4;
        public const int InitialCoins = 10;
        public const int MaxStock = 9;
        public int Coins { get; private set; } = InitialCoins;
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public static bool IsValid(MementoSupplyKind kind) =>
            kind == MementoSupplyKind.Row || kind == MementoSupplyKind.Column;
        public int Count(MementoSupplyKind kind) => !IsValid(kind) ? 0 :
            kind == MementoSupplyKind.Row ? Rows : Columns;
        public bool CanBuy(MementoSupplyKind kind) =>
            IsValid(kind) && Coins >= Price && Count(kind) < MaxStock;

        public bool TryBuy(MementoSupplyKind kind)
        {
            if (!CanBuy(kind)) return false;
            Coins -= Price;
            if (kind == MementoSupplyKind.Row) Rows++; else Columns++;
            return true;
        }

        /// <summary>Charge only after a legal board effect has actually started.</summary>
        public bool TryUse(MementoSupplyKind kind, Func<bool> apply)
        {
            if (Count(kind) <= 0 || apply == null || !apply()) return false;
            if (kind == MementoSupplyKind.Row) Rows--; else Columns--;
            return true;
        }

        public void AwardVictory() => Coins = Math.Min(100000, Coins + VictoryCoins);

        public void Load(int version, int coins, int rows, int columns, int previousWins)
        {
            // One-time migration is written with version 1 on the next normal save.
            // Never deduct or reinterpret progression stars.
            Coins = version <= 0
                ? InitialCoins + Math.Min(10000, Math.Max(0, previousWins)) * VictoryCoins
                : Math.Min(100000, Math.Max(0, coins));
            Rows = version <= 0 ? 0 : Math.Min(MaxStock, Math.Max(0, rows));
            Columns = version <= 0 ? 0 : Math.Min(MaxStock, Math.Max(0, columns));
        }
    }
}
