using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
    public class JointAnimColour : JointAnim
    {
        public struct Frame
        {
            public int FrameNumber;
            public int R;
            public int G;
            public int B;
            public int A;
            
            [ContentSerializerIgnore()]
            public Color Color;

            public void Init()
            {
                Color = new Color(R, G, B, A);
            }
        }

        [ContentSerializer(FlattenContent = true, CollectionItemName="Frame")]
        public Frame[] Frames;
        
        public override int FrameCount { get { return Frames.Length; } }
        public override int GetFrameNumber(int i) { return Frames[i].FrameNumber; }

        public override void Init(ContentManager content, ClipAnim clipAnim)
        {
            base.Init(content, clipAnim);

            for (int i = 0; i < Frames.Length; ++i)
            {
                Frames[i].Init();
            }
        }


        public override void ApplySate(int currentKeyframeIndex, int nextKeyframeIndex, float lerpValue, ref JointState jointState)
        {
            //jointState.Color = Color.Lerp(
            //    Frames[currentKeyframeIndex].Color,
            //    Frames[nextKeyframeIndex].Color,
            //    lerpValue);

            jointState.Color = Frames[currentKeyframeIndex].Color;
        }

        public override JointAnimState<JointAnim> CreateState()
        {
            return new JointAnimState<JointAnim>(this);
        }
    }
}
