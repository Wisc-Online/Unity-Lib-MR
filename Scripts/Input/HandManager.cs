using System.Collections.Generic;
using System.Linq;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public class HandManager : GameSingleton<HandManager>
    {
        private readonly List<IHandInputSource> _sources = new List<IHandInputSource>();

        public void RegisterSource(IHandInputSource source)
        {
            _sources.Add(source);
        }

        public void RemoveSource(IHandInputSource source)
        {
            _sources.Remove(source);
        }

        readonly Dictionary<HandControllerType, HandControllerInput> _frameInputs = new Dictionary<HandControllerType, HandControllerInput>();


        private bool _isReadingForThisFrame = false;

        private void LateUpdate()
        {
            _isReadingForThisFrame = false;
        }

        public HandControllerInput GetHand(HandControllerType hand)
        {
            EnsureReadingIsForFrame();

            HandControllerInput reading = null;

            if (!_frameInputs.TryGetValue(hand, out reading))
            {
                reading = null;
            }

            return reading;
        }

        public HandControllerInput[] GetAllHands()
        {
            EnsureReadingIsForFrame();

            return _frameInputs.Values.ToArray();
        }

        void EnsureReadingIsForFrame()
        {
            if (!_isReadingForThisFrame)
            {
                UpdateReadingForFrame();

                _isReadingForThisFrame = true;
            }
        }

        void UpdateReadingForFrame()
        {
            _frameInputs.Clear();

            foreach (var reading in _sources.SelectMany(x => x.GetReading()))
            {
                _frameInputs[reading.Hand] = reading;
            }
        }
    }
}