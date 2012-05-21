using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnomic.Core
{
    public abstract class GameScreen
    {
        public List<GameEntity> ActiveEntities = new List<GameEntity>();
        public Game Game { get; set; }
        public GameTime CurrentGameTime { get; set; }
        public bool SortEntitiesForDraw { get; set; }
		public Camera2D Camera { get; set; }

        public Random RandomNum { get; set; }

        protected SpriteBatch spriteBatch;

        public GameScreen(Game game, Camera2D camera)
        {
            Game = game;
			Camera = camera;
            RandomNum = new Random();
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            SortEntitiesForDraw = true;
        }

        public virtual void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;

            foreach (GameEntity ge in ActiveEntities)
            {
                ge.Update(gameTime);
            }
        }

        public virtual void Draw()
        {
            if (SortEntitiesForDraw)
            {
                // simple bubble sort. Can just do one pass per frame as positions don't change massively per frame.
                for (int i = 0; i < ActiveEntities.Count - 1; ++i)
                {
                    GameEntity g1 = ActiveEntities[i];
                    GameEntity g2 = ActiveEntities[i + 1];
                    if (g1.DrawOrder > g2.DrawOrder)
                    {
                        ActiveEntities[i] = g2;
                        ActiveEntities[i + 1] = g1;
                    }
                }
            }

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Camera.GetViewMatrix());

            foreach (GameEntity ge in ActiveEntities)
            {
                ge.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
            
    }
}
