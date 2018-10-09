using FVTC.LearningInnovations.Unity.MixedReality.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVTC.LearningInnovations.Unity.MixedReality.Input
{

    [RequireComponent(typeof(Animator))]
    public class HandController : MonoBehaviour
    {

        public HandControllerType handType = HandControllerType.Unknown;
        public float animationSpeed = 1f;

        public Vector3 positionOffset = Vector3.zero;
        public Vector3 positionOffsetAngles = Vector3.zero;

        [Header("Animation Names")]
        public string pointerAnimationName = "PointerAni";
        public string pointerAnimationReverseName = "PointerAniRev";
        public string thumbAnimationName = "ThumbAni";
        public string thumbAnimationReverseName = "ThumbAniRev";
        public string graspAnimationName = "AnimateThree";
        public string graspAnimationReverseName = "AnimateThreeRev";

        [HideInInspector]
        Animator animator;

        [Header("Thresholds")]
        public float pointThreshold = 0.5f;
        public float graspThreshold = 0.5f;
        public float thumbThreshold = 0.5f;

        [Header("Misc.")]
        public bool reverseThumbAnimation;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Use this for initialization
        void Start()
        {
            if (animator == null)
                this.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            animator.speed = animationSpeed;

            var hand = HandManager.Instance.GetHand(this.handType);

            if (hand != null)
            {
                animator.Play(hand.IndexFinder.ClosedPercent > pointThreshold ? pointerAnimationName : pointerAnimationReverseName);

                float averageGrasp = (hand.MiddleFinder.ClosedPercent + hand.RingFinger.ClosedPercent + hand.LittleFinger.ClosedPercent) / 3f;

                animator.Play(averageGrasp > graspThreshold ? graspAnimationName : graspAnimationReverseName);
                animator.Play(hand.Thumb.ClosedPercent > thumbThreshold ? thumbAnimationName : thumbAnimationReverseName);

                Quaternion rotationOffset = Quaternion.Euler(this.positionOffsetAngles);

                this.gameObject.transform.position = hand.Pose.position + this.positionOffset;
                this.gameObject.transform.rotation = hand.Pose.rotation * rotationOffset;
            }
        }
    }
}