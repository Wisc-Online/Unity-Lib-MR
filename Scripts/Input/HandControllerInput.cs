using UnityEngine;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public class HandControllerInput
    {
        private HandControllerType hand = HandControllerType.Unknown;

        public HandControllerType Hand
        {
            get
            {
                return hand;
            }

            set
            {
                hand = value;
            }
        }

        public Pose Pose { get; set; }

        public HandControllerInputThumb Thumb { get; set; }

        public HandControllerInputFinger IndexFinder { get; set; }

        public HandControllerInputFinger MiddleFinder { get; set; }

        public HandControllerInputFinger RingFinger { get; set; }

        public HandControllerInputFinger LittleFinger { get; set; }
    }
}