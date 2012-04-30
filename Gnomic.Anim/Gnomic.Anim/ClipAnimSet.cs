using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
    public class ClipAnimSet
    {
        [ContentSerializer(FlattenContent = true, CollectionItemName = "Anim")] 
        public List<ClipAnim> Anims;

        public void Init(ContentManager content, Clip clip)
        {
            foreach (ClipAnim ca in Anims)
            {
                ca.Init(content, clip);
            }
        }

        public ClipAnim this[string animName]
        {
            get
            {
                foreach (ClipAnim anim in this.Anims)
                {
                    if (anim.Name == animName)
                        return anim;
                }
                return null;
            }
        }
    }
}
