namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public struct HandControllerInputFinger : IHandControllerInputFinger
    {
        public HandControllerInputFinger(float closedPercent)
        {
            ClosedPercent = closedPercent;
        }
        public float ClosedPercent { get; private set; }
    }
}