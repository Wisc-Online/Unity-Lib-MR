using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{
    public class InteractionManagerHandControllerInputSource : GameSingleton<InteractionManagerHandControllerInputSource>, IHandInputSource
    {
      

        private void Start()
        {
            HandManager.Instance.RegisterSource(this);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HandManager.Instance.RemoveSource(this);
        }


        HandControllerType GetControllerType(InteractionSourceHandedness handedness)
        {
            switch (handedness)
            {
                case InteractionSourceHandedness.Left:
                    return HandControllerType.LeftHand;
                case InteractionSourceHandedness.Right:
                    return HandControllerType.RightHand;
                default:
                    return HandControllerType.Unknown;
            }

        }

        public IEnumerable<HandControllerInput> GetReading()
        {
            InteractionSource source;


            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                source = sourceState.source;
                if (source.kind == InteractionSourceKind.Controller)
                {

                    // on these controllers, the middle, ring and pinky fingers are
                    // not individually tracked.
                    // Instead, set all three of these fingers from the grasp
                    HandControllerInputFinger graspFinger;

                    float thumbDownPercent;
                    graspFinger = new HandControllerInputFinger(sourceState.grasped ? 1f : 0f);

                    if (sourceState.thumbstickPressed || sourceState.touchpadPressed)
                    {
                        thumbDownPercent = 1f;
                    }
                    else if (sourceState.touchpadTouched)
                    {
                        thumbDownPercent = 0.5f;
                    }
                    else
                    {
                        thumbDownPercent = 0f;
                    }

                    yield return new HandControllerInput
                    {
                        Hand = GetControllerType(source.handedness),
                        Pose = GetPose(sourceState),
                        Thumb = new HandControllerInputThumb(thumbDownPercent, sourceState.thumbstickPosition),
                        IndexFinder = new HandControllerInputFinger(sourceState.selectPressedAmount),
                        MiddleFinder = graspFinger,
                        RingFinger = graspFinger,
                        LittleFinger = graspFinger
                    };
                }
            }
        }

        float GetAxis(string axisName)
        {
            return string.IsNullOrEmpty(axisName) ? 0f : UnityEngine.Input.GetAxis(axisName);
        }

        private Pose GetPose(InteractionSourceState sourceState)
        {
            Vector3 position;
            Quaternion rotation;

            if (!sourceState.sourcePose.TryGetPosition(out position) || !PositionIsValid(position))
            {
                position = Vector3.zero;
            }

            if (!sourceState.sourcePose.TryGetRotation(out rotation) || !RotationIsValid(rotation))
            {
                rotation = Quaternion.identity;
            }

            return new Pose(position, rotation);
        }

        private bool RotationIsValid(Quaternion newRotation)
        {
            return !float.IsNaN(newRotation.x) && !float.IsNaN(newRotation.y) && !float.IsNaN(newRotation.z) && !float.IsNaN(newRotation.w) &&
                !float.IsInfinity(newRotation.x) && !float.IsInfinity(newRotation.y) && !float.IsInfinity(newRotation.z) && !float.IsInfinity(newRotation.w);
        }

        private bool PositionIsValid(Vector3 position)
        {
            return !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
                !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z);
        }
    }
}
