using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnomic.Core
{
    public abstract class GameEntity
    {
        // Entities should override this to affect their draw sorting position.
        // Smaller values at the back
        public virtual float DrawOrder { get { return 0.0f; } }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
