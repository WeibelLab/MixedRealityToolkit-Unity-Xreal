// TODO: [Optional] Add copyright and license statement(s).

using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.Scripting;
using NRKernal;
using System.Collections.Generic;
using MixedReality.Toolkit.Input;
using UnityEngine.XR;
using UnityEngine.InputSystem;

namespace XREAL.MRTK3.Subsystems
{
    [Preserve]
    [MRTKSubsystem(
        Name = "xreal.mrtk3.subsystems",
        DisplayName = "Subsystem for XREAL NRSDK Hands API",
        Author = "XREAL",
        ProviderType = typeof(XREALHandsSubsystemProvider),
        SubsystemTypeOverride = typeof(XREALHandsSubsystem),
        ConfigType = typeof(BaseSubsystemConfig))]
    public class XREALHandsSubsystem : HandsSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            // Fetch subsystem metadata from the attribute.
            // var cinfo = XRSubsystemHelpers.ConstructCinfo<XREALHandsSubsystem, HandsSubsystemCinfo>();

            var cinfo = new HandsSubsystemCinfo
            {
                Name = "XREALHandsSubsystem",
                DisplayName = "XREAL Hands Subsystem",
                ProviderType = typeof(XREALHandsSubsystemProvider),
                SubsystemTypeOverride = typeof(XREALHandsSubsystem)
            };

            if (!Register(cinfo))
            {
                Debug.LogError($"Failed to register the {cinfo.Name} subsystem.");
            }
        }

        private class XREALHandContainer : HandDataContainer
        {
            private static readonly Dictionary<TrackedHandJoint, HandJointID> MrtkToXREALJointMapping = new()
            {
                { TrackedHandJoint.Wrist, HandJointID.Wrist },
                { TrackedHandJoint.Palm, HandJointID.Palm},
                { TrackedHandJoint.ThumbMetacarpal, HandJointID.ThumbMetacarpal},
                { TrackedHandJoint.ThumbProximal, HandJointID.ThumbProximal},
                { TrackedHandJoint.ThumbDistal, HandJointID.ThumbDistal},
                { TrackedHandJoint.ThumbTip, HandJointID.ThumbTip},
                { TrackedHandJoint.IndexProximal, HandJointID.IndexProximal},
                { TrackedHandJoint.IndexIntermediate, HandJointID.IndexMiddle},
                { TrackedHandJoint.IndexDistal, HandJointID.IndexDistal},
                { TrackedHandJoint.IndexTip, HandJointID.IndexTip},
                { TrackedHandJoint.MiddleProximal, HandJointID.MiddleProximal },
                { TrackedHandJoint.MiddleIntermediate, HandJointID.MiddleMiddle },
                { TrackedHandJoint.MiddleDistal, HandJointID.MiddleDistal },
                { TrackedHandJoint.MiddleTip, HandJointID.MiddleTip },
                { TrackedHandJoint.RingProximal, HandJointID.RingProximal },
                { TrackedHandJoint.RingIntermediate, HandJointID.RingMiddle },
                { TrackedHandJoint.RingDistal, HandJointID.RingDistal },
                { TrackedHandJoint.RingTip, HandJointID.RingTip },
                { TrackedHandJoint.LittleMetacarpal, HandJointID.PinkyMetacarpal },
                { TrackedHandJoint.LittleProximal, HandJointID.PinkyProximal },
                { TrackedHandJoint.LittleIntermediate, HandJointID.PinkyMiddle },
                { TrackedHandJoint.LittleDistal, HandJointID.PinkyDistal },
                { TrackedHandJoint.LittleTip, HandJointID.PinkyTip }
            };

            public XREALHandContainer(XRNode handNode) : base(handNode)
            {
            }

            public override bool TryGetEntireHand(out IReadOnlyList<HandJointPose> result)
            {
                if (!AlreadyFullQueried)
                {
                    UpdateEntireHand();
                }
                result = HandJoints;
                return FullQueryValid;
            }

            private void UpdateEntireHand()
            {
                if (!GetHandState().isTracked)
                    return;
                AlreadyFullQueried = true;
                for (int i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
                {
                    if (MrtkToXREALJointMapping.ContainsKey((TrackedHandJoint)i))
                    {
                        HandJoints[i] = new HandJointPose
                        {
                            Pose = GetHandState().GetJointPose(MrtkToXREALJointMapping[(TrackedHandJoint)i])
                        };
                    }
                }
            }

            public override bool TryGetJoint(TrackedHandJoint joint, out HandJointPose pose)
            {
                if (!AlreadyFullQueried)
                {
                    UpdateEntireHand();
                }

                pose = HandJoints[(int)joint];

                return GetHandState().isTracked;
            }

            private HandState GetHandState()
            {
                return NRInput.Hands.GetHandState(HandNode == XRNode.RightHand ? HandEnum.RightHand : HandEnum.LeftHand);
            }
        }

        private class XREALHandsSubsystemProvider : Provider, IHandsSubsystem
        {
            private Dictionary<XRNode, XREALHandContainer> hands = null;

            public override void Start()
            {
                base.Start();

                hands ??= new Dictionary<XRNode, XREALHandContainer>
                {
                    {XRNode.LeftHand, new XREALHandContainer(XRNode.LeftHand)},
                    {XRNode.RightHand, new XREALHandContainer(XRNode.RightHand)}
                };

                InputSystem.onBeforeUpdate += ResetHands;
            }

            public override void Stop()
            {
                ResetHands();
                InputSystem.onBeforeUpdate -= ResetHands;
                base.Stop();
            }

            public override bool TryGetEntireHand(XRNode handNode, out IReadOnlyList<HandJointPose> jointPoses)
            {
                Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, "Non-hand XRNode used in TryGetEntireHand query");

                return hands[handNode].TryGetEntireHand(out jointPoses);
            }

            public override bool TryGetJoint(TrackedHandJoint joint, XRNode handNode, out HandJointPose jointPose)
            {
                Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, "Non-hand XRNode used in TryGetJoint query");

                return hands[handNode].TryGetJoint(joint, out jointPose);
            }

            private void ResetHands()
            {
                hands[XRNode.LeftHand].Reset();
                hands[XRNode.RightHand].Reset();
            }
        }
    }
}
