using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gnomic.Anim
{
    public class Joint
    {
        public int ParentId;
        public string Name;
        public string TextureName;
        public Rectangle TextureRect;
        public SpriteEffects FlipState;
        public Vector2 Origin;
        public Transform2D Transform;

        [ContentSerializerIgnore()]
        public Texture2D Texture;

        public void Init(ContentManager content)
        {
            if (!string.IsNullOrEmpty(TextureName))
                Texture = content.Load<Texture2D>(TextureName);
        }
    }
}
