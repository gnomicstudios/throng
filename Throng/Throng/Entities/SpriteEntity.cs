using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gnomic.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Throng
{
    public class SpriteEntity : GameEntity
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Rectangle? SourceRect;
        public Color Color;
        public float Rotation;
        public Vector2 Origin;
        public Vector2 Scale;
        public SpriteEffects FlipState;
        public float Depth;

        public SpriteEntity(Texture2D texture, Vector2 position)
            : this(texture, position, null, Color.White, 0.0f, new Vector2(texture.Width / 2, texture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f)
        {

        }

        public SpriteEntity(
            Texture2D texture, Vector2 position, Rectangle? sourceRect, 
            Color color, float rotation, Vector2 origin, Vector2 scale, 
            SpriteEffects flipState, float depth)
        {
            Texture = texture;
            Position = position;
            SourceRect = sourceRect;
            Color = color;
            Rotation = rotation;
            Origin = origin;
            Scale = scale;
            FlipState = flipState;
            Depth = depth;
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, SourceRect, Color, Rotation, Origin, Scale, FlipState, Depth);
        }
    }
}
