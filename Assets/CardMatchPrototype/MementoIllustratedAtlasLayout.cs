using System;
using UnityEngine;

/// <summary>Finds empty atlas gutters without rotating art or cutting through painted pixels.</summary>
public static class MementoIllustratedAtlasLayout
{
    /// <summary>Returns rectangles in reading order: top row first, left to right.</summary>
    public static RectInt[] BuildRects(Color32[] pixels, int width, int height, int columns, int rows)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));
        if (width <= 0 || height <= 0 || (long)width * height != pixels.Length ||
            columns <= 0 || rows <= 0 || columns > width || rows > height)
            throw new ArgumentException("Invalid atlas dimensions.");
        var occupiedX = new bool[width];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (pixels[y * width + x].a != 0)
            {
                occupiedX[x] = true;
            }
        int[] xs = Cuts(occupiedX, columns);
        // Each column may have a slightly different safe row gutter in generated art.
        // Never relax the no-painted-pixels rule just to fit a regular grid.
        var columnCuts = new int[columns][];
        for (int column = 0; column < columns; column++)
        {
            var occupiedY = new bool[height];
            for (int y = 0; y < height; y++)
            for (int x = xs[column]; x < xs[column + 1]; x++)
                if (pixels[y * width + x].a != 0) { occupiedY[y] = true; break; }
            columnCuts[column] = Cuts(occupiedY, rows);
        }
        var result = new RectInt[columns * rows];
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            int[] ys = columnCuts[column];
            int yIndex = rows - row - 1;
            var rect = new RectInt(xs[column], ys[yIndex],
                xs[column + 1] - xs[column], ys[yIndex + 1] - ys[yIndex]);
            bool hasPaint = false;
            for (int y = rect.yMin; y < rect.yMax && !hasPaint; y++)
            for (int x = rect.xMin; x < rect.xMax; x++)
                if (pixels[y * width + x].a != 0) { hasPaint = true; break; }
            if (!hasPaint) throw new InvalidOperationException("Atlas contains an empty cell.");
            result[row * columns + column] = rect;
        }
        return result;
    }

    private static int[] Cuts(bool[] occupied, int count)
    {
        var cuts = new int[count + 1];
        cuts[count] = occupied.Length;
        int radius = Math.Max(1, occupied.Length / count / 5);
        for (int i = 1; i < count; i++)
        {
            int ideal = (int)((long)occupied.Length * i / count);
            int cut = -1;
            for (int distance = 0; distance <= radius && cut < 0; distance++)
            {
                int lower = ideal - distance, upper = ideal + distance;
                if (lower > cuts[i - 1] && lower < occupied.Length && !occupied[lower])
                    cut = lower;
                else if (upper > cuts[i - 1] && upper < occupied.Length && !occupied[upper])
                    cut = upper;
            }
            if (cut < 0) throw new InvalidOperationException("No safe transparent atlas gutter.");
            cuts[i] = cut;
        }
        return cuts;
    }
}
