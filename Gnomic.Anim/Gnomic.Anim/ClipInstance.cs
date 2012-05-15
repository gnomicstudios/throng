using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnomic.Anim
{
    public struct JointState
    {
        public Transform2D Transform;
        public Color Color;
        public Texture2D Texture;
        public Rectangle TextureRect;
        public SpriteEffects FlipState;
        public bool Visible;
        public Vector2 Origin;
    }
    
    public class ClipInstance
    {
        public Clip Clip;
        public JointState[] JointStates;
        public JointAnimState<JointAnim>[] JointAnimStates;
        public Transform2D[] AbsoluteTransforms;
        ClipAnim currentAnim;
        public ClipAnim CurrentAnim { get { return currentAnim; } }

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
            JointStates = new JointState[clip.Joints.Length];
            AbsoluteTransforms = new Transform2D[clip.Joints.Length];

            // There are potentially 3 types of animation per joint
            JointAnimStates = new JointAnimState<JointAnim>[clip.Joints.Length * 3];

            for (int i = 0; i < JointStates.Length; ++i)
            {
                JointStates[i].Color = Color.White;
                JointStates[i].Transform = clip.Joints[i].Transform;
                JointStates[i].Texture = clip.Joints[i].Texture;
                JointStates[i].TextureRect = clip.Joints[i].TextureRect;
                JointStates[i].FlipState = clip.Joints[i].FlipState;
                JointStates[i].Origin = clip.Joints[i].Origin;
                JointStates[i].Visible = true;
            }
            ComputeAbsoluteTransforms();
        }

        public void Play(ClipAnim anim)
        {
            currentAnim = anim;
            for (int i = 0; i < currentAnim.JointAnims.Count; ++i)
            {
                JointAnimStates[i] = currentAnim.JointAnims[i].CreateState();
            }
        }

        public void Play(string animName)
        {
            ClipAnim animToPlay = Clip.AnimSet[animName];
            System.Diagnostics.Debug.Assert(animToPlay != null);
            Play(animToPlay);
        }

        public void Update(float dt)
        {
            if (currentAnim != null)
            {
                for (int i = 0; i < currentAnim.JointAnims.Count; ++i)
                {
                    JointAnimStates[i].Update(dt, ref JointStates[JointAnimStates[i].JointAnim.JointId]);
                }
            }

            ComputeAbsoluteTransforms();
        }
        
        public void ComputeAbsoluteTransforms()
        {
            for (int i = 0; i < Clip.Joints.Length; i++)
            {
                Joint joint = Clip.Joints[i];
                if (joint.ParentId < 0)
                    AbsoluteTransforms[i] = JointStates[i].Transform; //.Translate(JointStates[i].Origin);
                else
                {
                    Transform2D combinedTransform;
                    // Translate current local transform to origin
                    Transform2D thisAtOrigin = JointStates[i].Transform.Translate(-JointStates[i].Origin);
                    // Compose with parent absolute transform
                    Transform2D.Compose(ref AbsoluteTransforms[joint.ParentId], ref thisAtOrigin, out combinedTransform);
                    // Move back to original position, but with parent scale applied
                    AbsoluteTransforms[i] = combinedTransform.Translate(JointStates[i].Origin * AbsoluteTransforms[joint.ParentId].Scale);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Clip.DrawOrder.Length; ++i)
            {
                int index = Clip.DrawOrder[i];
                JointState js = JointStates[index];
                if (js.Texture != null && js.Visible)
                {
                    spriteBatch.Draw(
                        js.Texture,
                        AbsoluteTransforms[index].Pos, 
                        new Rectangle?(js.TextureRect), 
                        js.Color, 
                        AbsoluteTransforms[index].Rot, 
                        js.Origin, 
                        AbsoluteTransforms[index].Scale, 
                        js.FlipState, 
                        0.0f);
                }
            }
        }
    }
}
