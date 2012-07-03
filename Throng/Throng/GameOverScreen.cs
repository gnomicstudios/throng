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

namespace Throng
{
    public class GameOverScreen : GameScreen
    {
        public GameOverScreen(Game1 game)
            : base(game, game.Camera)
        {
            Texture2D backgroundTexture = game.Content.Load<Texture2D>("gameOverScreen");

            base.ActiveEntities.Add(
                new SpriteEntity(backgroundTexture, game.ScreenSizeDefault / 2.0f));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Input.ButtonJustUpMapped((int)Controls.Select))
            {
                ((Game1)base.Game).BackToMenu();
            }
        }
    }
}