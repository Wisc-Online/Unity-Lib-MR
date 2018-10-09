using System.Collections.Generic;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public interface IHandInputSource
    {
        IEnumerable<HandControllerInput> GetReading();
    }
}