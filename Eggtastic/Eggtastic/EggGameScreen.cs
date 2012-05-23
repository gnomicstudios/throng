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

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.DebugViews;

using Gnomic.Anim;
using Gnomic.Core;

namespace Eggtastic
{
    public class EggGameScreen : GameScreen
    {
        private const int STARTING_EGGS = 1;
        private const float EGG_SPAWN_MIN_DIST = 100f;
        private const float EGG_SPAWN_MAX_DIST = 200f;
        private const float INITIAL_NO_INPUT_TIME = 1f;

        private int _eggCounterLast = -1;
        private int _eggCounterCurrent = 0;
        private string _eggCounterString = "";
        private double _levelActiveTime;

        private Border _border;
        private EnemySpawner _enemySpawner;
        private Game1 _eggtastic;

        public List<EnemyEntity> Enemies;
        public List<EggEntity> Eggs;

        // convenience references
        public GraphicsDevice GraphicsDevice { get; set; }
        public Viewport Viewport { get; set; }
        public ContentManager Content { get; set; }

        public Vector2 ScreenCenter { get; set; }

        public Vertices Corners { get; set; }

        protected DebugViewXNA DebugView { get; set; }

        public World World { get; set; }
        public PlayerEntity Player { get; set; }

        protected Matrix projection;

        public Dictionary<string, Clip> Clips { get; set; }

        public Song BackgroundMusic { get; set; }
        public Dictionary<string, SoundEffect> SoundEffects { get; set; }

        public List<CharacterEntity> QueuedForDisposal { get; set; }
        public List<CharacterEntity> QueuedForCreation { get; set; }

        SpriteFont gameFont;

        private void InitialiseClips()
        {
            Clip playerClip = Content.Load<Gnomic.Anim.Clip>("player");
            playerClip.Init(Content);
            Clips.Add("player", playerClip);

            Clip eggClip = Content.Load<Gnomic.Anim.Clip>("egg");
            eggClip.Init(Content);
            Clips.Add("egg", eggClip);

            Clip enemyClip = Content.Load<Gnomic.Anim.Clip>("enemy");
            enemyClip.Init(Content);
            Clips.Add("enemy", enemyClip);

        }

        public EggGameScreen(Game1 game)
            : base(game, game.Camera.Clone())
        {
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);
            
            _eggtastic = game;

            Clips = new Dictionary<string, Clip>();
            Enemies = new List<EnemyEntity>();
            Eggs = new List<EggEntity>();
            QueuedForDisposal = new List<CharacterEntity>();
            QueuedForCreation = new List<CharacterEntity>();

            GraphicsDevice = game.GraphicsDevice;
            Viewport = GraphicsDevice.Viewport;
            ScreenCenter = new Vector2(Viewport.Width / 2f, Viewport.Height / 2f);
            Content = game.Content;

            gameFont = Content.Load<SpriteFont>("GameFont");

#if !ANDROID
            BackgroundMusic =
                Content.Load<Song>("background-music");
            MediaPlayer.Play(BackgroundMusic);
            MediaPlayer.Volume = 0.5f;
#endif
		    
            Corners = new Vertices(4);
            Corners.Add(new Vector2(0f));                              // top-left
            Corners.Add(new Vector2(Viewport.Width, 0f));              // top-right
            Corners.Add(new Vector2(Viewport.Width, Viewport.Height)); // bottom-right
            Corners.Add(new Vector2(0f, Viewport.Height));             // bottom-left

            projection =
                Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(Viewport.Width),
                                                   ConvertUnits.ToSimUnits(Viewport.Height), 0f, 0f, 1f);

            World = new World(new Vector2(0, 0));
            DebugView = new DebugViewXNA(World);
            DebugView.RemoveFlags(DebugViewFlags.Shape);
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;
            DebugView.LoadContent(GraphicsDevice, Content);

            InitialiseClips();

            _enemySpawner = new EnemySpawner(this, Clips["enemy"]);

            _border = new Border(World, Viewport);

            InitialiseLevel();
        }

        public void InitialiseLevel()
        {
            Texture2D backgroundTexture = Content.Load<Texture2D>("background");
            base.ActiveEntities.Add(new SpriteEntity(backgroundTexture, ScreenCenter));
            
            Player = new PlayerEntity(this, Clips["player"], ScreenCenter);
            ActiveEntities.Add(Player);
        }

        public EggEntity AddEgg(Vector2 pos)
        {
            _eggCounterCurrent++;

            EggEntity egg =
                new EggEntity(this, Clips["egg"], pos);
            egg.DynamicBody.LinearDamping = 5f;
            Eggs.Add(egg);
            QueuedForCreation.Add(egg);
            return egg;
        }

        public void AddEnemy(EnemyEntity enemy)
        {
            ActiveEntities.Add(enemy);
            Enemies.Add(enemy);
        }

        public void DestroyEnemy(EnemyEntity enemy)
        {
            Enemies.Remove(enemy);
            QueuedForDisposal.Add(enemy);
        }

        public void DestroyEgg(EggEntity egg)
        {
            egg.IsValid = false;

            Eggs.Remove(egg);
            QueuedForDisposal.Add(egg);
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((DebugView.Flags & flag) == flag)
            {
                DebugView.RemoveFlags(flag);
            }
            else
            {
                DebugView.AppendFlags(flag);
            }
        }
        
        public void Reset()
        {
            Player.Position = ScreenCenter;
            Player.ResetSounds();
            FlushEntities();
            InitialiseLevel();

            _eggCounterCurrent = 0;
            //SpawnSomeEggs();
            _levelActiveTime = 0;

            _enemySpawner.Reset();

            Vector2 spawn = ScreenCenter;
            spawn.Y += Viewport.Height / 2.25f;
            AddEgg(spawn);
        }

        // spawn some eggs around the player
        private void SpawnSomeEggs()
        {
            for (int i = 0; i < STARTING_EGGS; i++)
            {
                Vector2 spawn = new Vector2(0f);
                spawn.X = ((float)RandomNum.NextDouble() * 2f) - 1f;
                spawn.Y = ((float)RandomNum.NextDouble() * 2f) - 1f;
                spawn.Normalize();

                float range = EGG_SPAWN_MAX_DIST - EGG_SPAWN_MIN_DIST;
                float dist = EGG_SPAWN_MIN_DIST +
                    ((float)RandomNum.NextDouble()) * range;
                spawn = Player.Position + (spawn * dist);

                AddEgg(spawn);
            }
        }

        // removes all entities from the game
        private void FlushEntities()
        {
            Enemies.Clear();
            Eggs.Clear();

            Player.Sensor.Dispose();

            foreach (GameEntity entity in ActiveEntities)
            {
                CharacterEntity character = entity as CharacterEntity;
                if (character != null)
                {
                    character.DynamicBody.Enabled = false;
                    character.DynamicBody.Dispose();
                }
            }

            ActiveEntities.Clear();
        }

        private void HandleKeyboardInput()
        {
            if (Input.KeyJustDown(Keys.F1))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
            }
            if (Input.KeyJustDown(Keys.F2))
            {
                _enemySpawner.SpawnEnemyRandomly();
            }
            Player.HandleKeyboardInput();
        }

        private void CleanupDeletedEntities()
        {
            if (QueuedForDisposal.Count > 0)
            {
                foreach (CharacterEntity entity in QueuedForDisposal)
                {
                    entity.DynamicBody.Enabled = false;
                    entity.DynamicBody.Dispose();
                    ActiveEntities.Remove(entity);
                }
                QueuedForDisposal.Clear();
            }
        }

        private void CreateQueuedEntities()
        {
            if (QueuedForCreation.Count > 0)
            {
                foreach (CharacterEntity entity in QueuedForCreation)
                {
                    ActiveEntities.Add(entity);
                }
            }
            QueuedForCreation.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            _levelActiveTime += gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
            RandomNum = new Random(gameTime.TotalGameTime.Milliseconds);

            if (Eggs.Count == 0)
            {
                _eggtastic.GameOver();
            }

			UpdateEggPhysics();

			// Follow player with camera
			Camera.Position = (Player.Position - Camera.Origin);

            if (_eggCounterCurrent != _eggCounterLast)
            {
                _eggCounterString = "Eggs: " + _eggCounterCurrent.ToString();
                _eggCounterLast = _eggCounterCurrent;
            }
			
            CreateQueuedEntities();

            _enemySpawner.Tick(gameTime);
            World.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            CleanupDeletedEntities();
            
            if (_levelActiveTime > INITIAL_NO_INPUT_TIME)
            {
                HandleKeyboardInput();
            }
        }

		// Constants to be tuned in UpdateEggPhysics()
		const float TARGET_EGG_DIST_SQR = 400; // 20 (in default 1280 width size)
		const float MAX_EGG_DIST_SQR = 20000;
		const float MAX_EGG_FORCE = 8;

		/// <summary>
		/// Apply a follow mechanic to the eggs. The first egg follows the player and
		/// the rest of the eggs follow the previous egg in the list
		/// </summary>
		private void UpdateEggPhysics()
		{
			Vector2 previousEggPos = Player.Position;

			for (int i = 0; i < Eggs.Count; ++i)
			{
				Vector2 eggPos = Eggs[i].Position;
				float eggDistSqr = Vector2.DistanceSquared(previousEggPos, eggPos);

				// Get direction egg should travel
				Vector2 eggAt = previousEggPos - eggPos;
				eggAt.Normalize();

				if (eggDistSqr > TARGET_EGG_DIST_SQR)
				{
					float eggForceMultiplier = MAX_EGG_FORCE * ((eggDistSqr - TARGET_EGG_DIST_SQR) / (MAX_EGG_DIST_SQR - TARGET_EGG_DIST_SQR));
					Eggs[i].DynamicBody.ApplyForce(eggAt * eggForceMultiplier);
				}
				previousEggPos = eggPos;
			}
		}

        public override void Draw()
        {
            base.Draw();

            spriteBatch.Begin();
            spriteBatch.DrawString(gameFont, _eggCounterString, new Vector2(10.0f, 10.0f), Color.White);
            spriteBatch.End();

            DebugView.RenderDebugData(ref projection);
        }
    }
}