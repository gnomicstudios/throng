using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnomic.Anim
{
    public class ClipInstance
    {
        public Clip Clip;
        public SpriteState[] JointStates;
        public Transform2D[] AbsoluteTransforms;
        ClipAnimInstance currentAnim;
        public ClipAnim CurrentAnim { get { return currentAnim.Anim; } }
        
		ClipInstance linkToParentClipInstance;
		int linkToParetJointId;

        public Vector2 Position
        {
            get { return JointStates[0].Transform.Pos; }
            set { JointStates[0].Transform.Pos = value; }
        }
        public Vector2 Scale
        {
            get { return JointStates[0].Transform.Scale; }
            set { JointStates[0].Transform.Scale = value; }
        }
        public float Rotation
        {
            get { return JointStates[0].Transform.Rot; }
            set { JointStates[0].Transform.Rot = value; }
        }

        public ClipInstance(Clip clip)
        {
            Clip = clip;
            JointStates = new SpriteState[clip.Joints.Length];
            AbsoluteTransforms = new Transform2D[clip.Joints.Length];
			JointStates[0].Transform = Transform2D.Identity;
            currentAnim = new ClipAnimInstance(this);
			Play(clip.AnimSet.Anims[0]);
			Update(0.0f);
        }

		public void LinkToParentClipInstance(ClipInstance parentClipInstance, int parentJointId)
		{
			linkToParentClipInstance = parentClipInstance;
			linkToParetJointId = parentJointId;
		}

        public void Play(ClipAnim anim)
        {
            Play(anim, true);
        }

        public void Play(ClipAnim anim, bool loop)
        {
            currentAnim.Play(anim, loop);
        }

        public void Play(string animName)
        {
            Play(animName, true);
        }

        public void Play(string animName, bool loop)
        {
            ClipAnim animToPlay = Clip.AnimSet[animName];
            System.Diagnostics.Trace.Assert(animToPlay != null);
            Play(animToPlay, loop);
        }

        public void Update(float dt)
        {
            currentAnim.Update(dt);
            ComputeAbsoluteTransforms();
        }
        
        public void ComputeAbsoluteTransforms()
        {
            for (int i = 0; i < Clip.Joints.Length; i++)
            {
                Joint joint = Clip.Joints[i];
                if (joint.ParentId < 0)
				{
					if (i == 0 && linkToParentClipInstance != null)
					{
						Transform2D.Compose(ref linkToParentClipInstance.AbsoluteTransforms[linkToParetJointId], ref JointStates[0].Transform, out AbsoluteTransforms[0]);
					}
					else
					{
						AbsoluteTransforms[i] = JointStates[i].Transform;
					}
				}
                else
                {
					Transform2D.Compose(ref AbsoluteTransforms[joint.ParentId], ref JointStates[i].Transform, out AbsoluteTransforms[i]);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Clip.DrawOrder.Length; ++i)
            {
                int index = Clip.DrawOrder[i];
                SpriteState js = JointStates[index];
                if (js.Texture != null && js.Visible)
                {
                    Transform2D xform = AbsoluteTransforms[index];
                    spriteBatch.Draw(
                        js.Texture,
                        new Vector2((int)xform.Pos.X, (int)xform.Pos.Y), 
                        new Rectangle?(js.TextureRect), 
                        js.Color,
                        xform.Rot,
						xform.Origin,
                        xform.Scale, 
                        js.FlipState, 
                        0.0f);
                }
            }
        }
    }
}
