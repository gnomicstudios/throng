using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Gnomic.Core;

namespace Eggtastic
{
    public class StartScreen : GameScreen
    {
        public bool IsGameLoaded { get; set; }

        SpriteFont gameFont;

        public StartScreen(Game1 game)
            : base(game, game.Camera) 
        {
            Texture2D backgroundTexture = game.Content.Load<Texture2D>("startScreen");
            base.ActiveEntities.Add(
                new SpriteEntity(backgroundTexture, game.ScreenSizeDefault / 2.0f));

            gameFont = game.Content.Load<SpriteFont>("GameFont");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (IsGameLoaded && Input.ButtonJustUpMapped((int)Controls.Select))
            {
                ((Game1)base.Game).StartGame();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (!IsGameLoaded)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, ViewMatrix);
                spriteBatch.DrawString(gameFont, "Loading...", Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }
    }
}