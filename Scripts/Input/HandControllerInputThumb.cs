using UnityEngine;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public struct HandControllerInputThumb : IHandControllerInputFinger
    {
        public HandControllerInputThumb(float closedPercent, Vector2 position)
        {
            ClosedPercent = closedPercent;
            Position = position;
        }

        public float ClosedPercent { get; private set; }
        public Vector2 Position { get; private set; }
    }
}