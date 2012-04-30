using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
    public class JointAnimSpatial : JointAnim
    {
        public struct Frame
        {
            public int FrameNumber;
            public Transform2D Transform;
        }

        [ContentSerializer(FlattenContent = true, CollectionItemName = "Frame")]
        public Frame[] Frames;

        public override int FrameCount { get { return Frames.Length; } }
        public override int GetFrameNumber(int i) { return Frames[i].FrameNumber; }

        public override void ApplySate(int currentKeyframeIndex, int nextKeyframeIndex, float lerpValue, ref JointState jointState)
        {
            //Transform2D.Lerp(
            //    ref Frames[currentKeyframeIndex].Transform, 
            //    ref Frames[nextKeyframeIndex].Transform,
            //    lerpValue, ref jointState.Transform);

            jointState.Transform = Frames[currentKeyframeIndex].Transform;

        }

        public override JointAnimState<JointAnim> CreateState()
        {
            return new JointAnimState<JointAnim>(this);
        }
    }
}
