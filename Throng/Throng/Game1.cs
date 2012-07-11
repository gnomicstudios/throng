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
using System.Threading;

namespace Throng
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public readonly Vector2 ScreenSizeDefault = new Vector2(1280, 720);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameScreen _activeScreen;
        List<GameScreen> _inactiveScreens;

        // Game screens:
        ThrongGameScreen _gameScreen;
        PauseScreen _pauseScreen;
        GameOverScreen _gameOverScreen;
        StartScreen _startScreen;

		public Camera2D Camera { get; set; }

        public enum State
        {
            Start,
            Paused,
            Running,
            GameOver
        }
        public State GameState;

        public Game1()
        {
			GameState = State.Start;

            graphics = new GraphicsDeviceManager(this);
#if WINDOWS
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
#else
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
#endif

#if WINDOWS
            Content = new Gnomic.Core.ContentTracker(this.Services);
            ((Gnomic.Core.ContentTracker)Content).UseSourceAssets = true;
#endif
            Content.RootDirectory = "Content";

            _activeScreen = null;
            _inactiveScreens = new List<GameScreen>();
        }

        public void Pause()
        {
            if (GameState == State.Running)
            {
                GameState = State.Paused;
                SwitchToGameScreen(_pauseScreen);
                //_gameScreen.PauseMusic();
            }
            else if (GameState == State.Paused)
            {
                GameState = State.Running;
                SwitchToGameScreen(_gameScreen);
                //_gameScreen.ResumeMusic();
            }
        }

        public void Reset()
        {
            GameState = State.Running;
            SwitchToGameScreen(_gameScreen);
            _gameScreen.Reset();
            //_gameScreen.ResumeMusic();
        }

        public void GameOver()
        {
            GameState = State.GameOver;
            SwitchToGameScreen(_gameOverScreen);
            //_gameScreen.PauseMusic();
            _gameScreen.Player.ResetSounds();
        }

        public void BackToMenu()
        {
            GameState = State.Start;
            SwitchToGameScreen(_startScreen);
            //_gameScreen.ResumeMusic();
        }
        
        public void StartGame()
        {
            SwitchToGameScreen(_gameScreen);
            _gameScreen.Reset();
            _gameScreen.Player.PlayFartSound();
        }

        protected void InitialiseGameScreens()
        {
            _pauseScreen = new PauseScreen(this);
            _gameOverScreen = new GameOverScreen(this);
            _startScreen = new StartScreen(this);

            WaitCallback loadGameCallback = new WaitCallback(LoadMainGameScreen);
            ThreadPool.QueueUserWorkItem(loadGameCallback);

            _activeScreen = _startScreen;
            _inactiveScreens.Add(_pauseScreen);
            _inactiveScreens.Add(_gameOverScreen);
        }

        void LoadMainGameScreen(object asyncState)
        {
            _gameScreen = new ThrongGameScreen(this);
            _inactiveScreens.Add(_gameScreen);
            _startScreen.IsGameLoaded = true;
        }

        protected void SwitchToGameScreen(GameScreen to)
        {
            Debug.Assert(to != null);
            
            _inactiveScreens.Add(_activeScreen);
            _activeScreen = to;
            _inactiveScreens.Remove(to);
        }
        
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
			Camera = new Camera2D(
				GraphicsDevice.Viewport, ScreenSizeDefault);

            InitialiseGameScreens();

            base.Initialize();

            Input.Initialize(GraphicsDevice);

			// Set up button mappings. Note that TouchTwinStick translates the left touch pad to Pad0, which is the same 
			// input device as the first connected Xbox controller.
			Input.ClearMappings();
			Input.ButtonMappings.Add((int)Controls.Up, new ButtonGeneric[] { ButtonGeneric.Pad0LeftThumbstickUp, ButtonGeneric.Up, ButtonGeneric.W });
			Input.ButtonMappings.Add((int)Controls.Down, new ButtonGeneric[] { ButtonGeneric.Pad0LeftThumbstickDown, ButtonGeneric.Down, ButtonGeneric.S });
			Input.ButtonMappings.Add((int)Controls.Left, new ButtonGeneric[] { ButtonGeneric.Pad0LeftThumbstickLeft, ButtonGeneric.Left, ButtonGeneric.A });
			Input.ButtonMappings.Add((int)Controls.Right, new ButtonGeneric[] { ButtonGeneric.Pad0LeftThumbstickRight, ButtonGeneric.Right, ButtonGeneric.D });

			//Input.ButtonMappings.Add((int)Controls.Suck, new ButtonGeneric[] { ButtonGeneric.TouchRightSide, ButtonGeneric.Space });
            //unfortunately you can't press up+left+space at the same time! so we have to use a different key...
            Input.ButtonMappings.Add((int)Controls.Attack1, new ButtonGeneric[] { ButtonGeneric.TouchRightSide, ButtonGeneric.LeftControl, ButtonGeneric.RightControl});
            Input.ButtonMappings.Add((int)Controls.Select, new ButtonGeneric[] { ButtonGeneric.TapAnywhere, ButtonGeneric.Space });

			// Default value for this is not sensitive enough for the Throng gameplay. 
			// By default it's tuned to menu item selection and requires much bigger deviation
			Input.StickDirectionThreshold = 0.2f;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            HandleKeyboardInput();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _activeScreen.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (GameState == State.Paused || GameState == State.GameOver)
            {
                _gameScreen.Draw();
            }
            _activeScreen.Draw();
            
            base.Draw(gameTime);
        }

        private void HandleKeyboardInput()
        {
            if (Input.KeyJustDown(Keys.P) || Input.KeyJustDown(Keys.Escape))
            {
                Pause();
            }
            if (Input.KeyJustDown(Keys.R))
            {
                Reset();
            }
        }
    }
}
