using System;

namespace AnimalMemory.UI
{
    /// <summary>Readable landscape hint sizes expressed in UI Toolkit panel units.</summary>
    public readonly struct MementoTutorialTypography
    {
        public float TitleSize { get; }
        public float BodySize { get; }
        public float HeaderHeight { get; }
        public float ActionSize { get; }
        public float MenuWidth { get; }
        public float MenuTextSize { get; }
        public float CloseTextSize { get; }
        public float ContentRightInset { get; }

        private MementoTutorialTypography(float unitsPerPixel)
        {
            TitleSize = Math.Max(22f, 14f * unitsPerPixel);
            BodySize = Math.Max(20f, 13f * unitsPerPixel);
            HeaderHeight = Math.Max(68f, 60f * unitsPerPixel);
            ActionSize = Math.Max(44f, 44f * unitsPerPixel);
            MenuWidth = Math.Max(100f, 90f * unitsPerPixel);
            MenuTextSize = Math.Max(11f, 13f * unitsPerPixel);
            CloseTextSize = Math.Max(23f, 22f * unitsPerPixel);
            ContentRightInset = MenuWidth + Math.Max(18f, 16f * unitsPerPixel);
        }

        /// <summary>
        /// Compensates for panel downscaling so small landscape screens retain
        /// a 14-pixel heading, 13-pixel body, 44-pixel touch targets and
        /// a 60-pixel reserved header.
        /// This does not own portrait styles or any board/gameplay state.
        /// </summary>
        public static MementoTutorialTypography ForLandscape(float panelWidth, int pixelWidth)
        {
            float unitsPerPixel = pixelWidth > 0 ? panelWidth / pixelWidth : 1f;
            if (float.IsNaN(unitsPerPixel) || float.IsInfinity(unitsPerPixel) ||
                unitsPerPixel <= 0f)
                unitsPerPixel = 1f;

            return new MementoTutorialTypography(unitsPerPixel);
        }
    }
}
