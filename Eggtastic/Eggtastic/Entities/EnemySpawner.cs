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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Gnomic.Anim;

namespace Eggtastic
{
    public class EnemySpawner
    {
        private const float SPAWN_INTERVAL_DEFAULT = 5.0f;

        private EggGameScreen _gameScreen;
        private Random _rand;
        private float _secondsSinceLastSpawn;
        private float _minDistFromPlayer;
        private Clip _clip;

        public float SpawnInterval { get; set; }

        // TODO: refactor this to use the content manager for retrieving
        // different enemy types ..
        public EnemySpawner(EggGameScreen gameScreen, Clip clip)
        {
            _gameScreen = gameScreen;
            _secondsSinceLastSpawn = 0f;
            _minDistFromPlayer = 10f;
            _clip = clip;

            // spawn every 5sec
            SpawnInterval = 5f;
        }

        public enum Edge
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }

        public void Reset()
        {
            SpawnInterval = SPAWN_INTERVAL_DEFAULT;
            _secondsSinceLastSpawn = 0.0f;
        }

        public void Tick(GameTime gameTime)
        {
            SpawnInterval -= (0.03f * (float)gameTime.ElapsedGameTime.TotalSeconds);

            _secondsSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsSinceLastSpawn > SpawnInterval)
            {
                _secondsSinceLastSpawn = 0f;
                SpawnEnemyRandomly();
            }
        }

        public void SpawnEnemyRandomly()
        {
            _rand = new Random(_gameScreen.CurrentGameTime.TotalGameTime.Seconds);

            // // pick a corner
            // int cornerIdx = _rand.Next(0, 3);
            // int nextCornerIdx = ((cornerIdx + 1) > 3) ? 0 : (cornerIdx + 1);
            // // dodgy hack .. fix this later

            // Vertices newCorners = new Vertices(4);
            // newCorners.Add(_gameScreen.Corners[1]);
            // newCorners.Add(_gameScreen.Corners[2]);
            // newCorners.Add(_gameScreen.Corners[3]);
            // newCorners.Add(_gameScreen.Corners[0]);

            float rCoef = (float)_rand.NextDouble();

            // Vector2 corner = newCorners[cornerIdx];
            // Vector2 nextCorner = newCorners[nextCornerIdx];
            // Vector2 side = rCoef * (nextCorner - corner);
            // Vector2 spawnPoint = corner + side;

            Vector2 topLeft = _gameScreen.Corners[0];
            Vector2 topRight = _gameScreen.Corners[1];
            Vector2 top = rCoef * (topRight - topLeft);
            Vector2 spawnPoint = topLeft + top;

            const float buffer = 20f;

            spawnPoint.X += ((top.X - buffer) < 0) ? buffer : -buffer;
            spawnPoint.Y += ((top.Y - buffer) < 0) ? buffer : -buffer;

            // if spawn point is too close to player, try again ..
            if (Vector2.Distance(_gameScreen.Player.Position, spawnPoint)
                < _minDistFromPlayer)
            {
                SpawnEnemyRandomly();
                return;
            }
            else
            {
                EnemyEntity enemy = new EnemyEntity(_gameScreen, _clip, spawnPoint);
                enemy.AttackPlayerWeight = 0.75f;
                if (_gameScreen.RandomNum.NextDouble() > 0.5)
                {
                    enemy.AttackEggWeight = 0.75f;
                }
                _gameScreen.AddEnemy(enemy);
            }
        }
    }
}