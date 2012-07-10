using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
    public class ClipAnim
    {
        /// <summary>
        /// Name of the clip anim
        /// </summary>
        public string Name;

        /// <summary>
        /// Duration in frames
        /// </summary>
        public int Duration;

        /// <summary>
        /// Number of frames per second
        /// </summary>
        public float Framerate;

        /// <summary>
        /// List of joint animations
        /// </summary>
        [ContentSerializer(FlattenContent = true, CollectionItemName = "JointAnim")] 
        public List<JointAnim> JointAnims = new List<JointAnim>();


        protected Clip parentClip;
        [ContentSerializerIgnore()]
        public Clip ParentClip { get { return parentClip; } }

        public void Init(ContentManager content, Clip clip)
        {
            parentClip = clip;

            foreach (JointAnim ja in JointAnims)
            {
                ja.Init(content, this);
            }
        }
    }

}
