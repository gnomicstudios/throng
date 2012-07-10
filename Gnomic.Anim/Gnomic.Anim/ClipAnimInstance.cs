using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gnomic.Anim
{
    public enum AnimPlayingState
    {
        Stopped,
        Playing,
        PlayingInReverse,
    }

    public class ClipAnimInstance
    {
        public ClipAnim Anim;
        public float AnimPos;
        public bool Loop;
        
        public JointAnimState<JointAnim>[] JointAnimStates;

        public float DurationInSeconds
        {
            get { return durationInSeconds; }
        }

        ClipInstance parentClipInstance;
        float durationInSeconds;
        AnimPlayingState playingState = AnimPlayingState.Stopped;

        public ClipAnimInstance(ClipInstance clipInstance)
        {
            parentClipInstance = clipInstance;
            // There are potentially 3 types of animation per joint
            JointAnimStates = new JointAnimState<JointAnim>[parentClipInstance.Clip.Joints.Length * 3];
        }

        public void Play(ClipAnim anim, bool loop)
        {
            Anim = anim;
            AnimPos = 0.0f;
            Loop = loop;
            durationInSeconds = Anim.Duration * Anim.Framerate;
            playingState = AnimPlayingState.Playing;

            for (int i = 0; i < Anim.JointAnims.Count; ++i)
            {
                JointAnimStates[i] = Anim.JointAnims[i].CreateState();
            }
        }

        public void Update(float dt)
        {
            if (Anim != null)
            {
                if (playingState == AnimPlayingState.Playing)
                {
                    AnimPos += dt;
                    if (AnimPos > durationInSeconds)
                    {
                        if (Loop)
                        {
                            AnimPos %= durationInSeconds;
                        }
                        else
                        {
                            dt = Math.Max(0.0f, AnimPos - durationInSeconds);
                        }
                    }
                }

                for (int i = 0; i < Anim.JointAnims.Count; ++i)
                {
                    JointAnimStates[i].Update(dt, ref parentClipInstance.JointStates[JointAnimStates[i].JointAnim.JointId]);
                }
            }
        }
    }
}
