using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gnomic.Anim;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace Gnomic.Core
{
    public class BaseEntity : GameEntity
    {
        public ClipInstance ClipInstance;
        public Body DynamicBody;

        public override float DrawOrder 
        { 
            get
            {
                if (ClipInstance != null)
                {
                    return ClipInstance.Position.Y;
                }
                return 0.0f; 
            } 
        }

        public override void Update(GameTime gameTime)
        {
            ClipInstance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            ClipInstance.Draw(spriteBatch);
        }
    }
}
