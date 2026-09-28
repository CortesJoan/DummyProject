using UnityEngine.UIElements;

namespace AnimalMemory.Narrative
{
    /// <summary>
    /// Lightweight portrait transition adapted from 30 Days of Silence.
    /// Only entrance/reset effects required by Memento Match are retained.
    /// </summary>
    public static class MementoCharacterAnimator
    {
        private const string BeforeLeft = "narrative-before-left";
        private const string BeforeRight = "narrative-before-right";
        private const string Active = "narrative-active";

        public static void Enter(VisualElement target, bool fromRight)
        {
            if (target == null)
                return;

            Reset(target);
            string before = fromRight ? BeforeRight : BeforeLeft;
            target.AddToClassList(before);
            target.schedule.Execute(() =>
            {
                target.RemoveFromClassList(before);
                target.AddToClassList(Active);
            }).StartingIn(16);
        }

        public static void Reset(VisualElement target)
        {
            if (target == null)
                return;
            target.RemoveFromClassList(BeforeLeft);
            target.RemoveFromClassList(BeforeRight);
            target.RemoveFromClassList(Active);
        }
    }
}
