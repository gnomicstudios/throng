using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Gnomic.Anim
{
	public struct SpriteState
	{
		public Transform2D Transform;
		public Color Color;
		[ContentSerializerIgnore()]
		public Texture2D Texture;
		public Rectangle TextureRect;
		public SpriteEffects FlipState;
		public bool Visible;
	}
}
