using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
    public class Clip
    {
        [ContentSerializer(FlattenContent=true, CollectionItemName="Joint")]
        public Joint[] Joints;
        public int[] DrawOrder;
        public ClipAnimSet AnimSet;

        public void Init(ContentManager content)
        {
            foreach (Joint j in Joints)
            {
                j.Init(content);
            }

            if (AnimSet != null)
            {
                AnimSet.Init(content, this);
            }
        }
    }
}
