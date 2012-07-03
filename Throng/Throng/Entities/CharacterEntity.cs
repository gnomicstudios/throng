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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Gnomic.Anim;

namespace Eggtastic
{
    public abstract class CharacterEntity : BaseEntity
    {
        protected EggGameScreen GameScreen { get; set; }
        protected float _physicsRadius;
        protected Vector2 _physicsOffset;
        
        public enum Type
        {
            Player, Egg, Enemy
        }

        public enum CollisionCategory
        {
            Environment = Category.Cat1,
            Player = Category.Cat2,
            Enemy = Category.Cat3,
            Egg = Category.Cat4,
            Sensor = Category.Cat5
        }

        // For a category of object, what other categories does it
        // collide with?
        public enum CollidesWith
        {
            Environment = CollisionCategory.Environment | CollisionCategory.Player | CollisionCategory.Egg,
            Player = CollisionCategory.Environment | CollisionCategory.Enemy | CollisionCategory.Egg,
            Enemy = CollisionCategory.Player | CollisionCategory.Egg | CollisionCategory.Sensor,
            Egg = CollisionCategory.Player | CollisionCategory.Enemy | CollisionCategory.Egg | CollisionCategory.Environment,
            Sensor = CollisionCategory.Enemy
        }

        public Type EntityType { get; set; }
        public bool IsValid { get; set; }

        public Vector2 Position
        {
            get { return ClipInstance.Position; }
            set { ClipInstance.Position = value; }
        }
        public Vector2 Scale
        {
            get { return ClipInstance.Scale; }
            set { ClipInstance.Scale = value; }
        }
        public float Rotation
        {
            get { return ClipInstance.Rotation; }
            set { ClipInstance.Rotation = value; }
        }

        public CharacterEntity(EggGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f, Vector2.Zero, 1.0f)
        { }

        public CharacterEntity(EggGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f, Vector2.Zero, 1.0f)
        { }

        public CharacterEntity(EggGameScreen gameScreen, Clip clip, Vector2 position,
                               Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f, Vector2.Zero, 1.0f)
        { }

        public CharacterEntity(EggGameScreen gameScreen, Clip clip,
                               Vector2 position, Vector2 scale, float rotation, Vector2 physicsOffset, float physicsRadius)
        {
            ClipInstance = new ClipInstance(clip);
            GameScreen = gameScreen;
            Position = position; Scale = scale; Rotation = rotation;
            _physicsRadius = physicsRadius;
            _physicsOffset = physicsOffset;
            IsValid = true;

            SetupDynamics();
        }

        protected void CreateCircularBody()
        {
            DynamicBody = BodyFactory.CreateBody(GameScreen.World, ConvertUnits.ToSimUnits(Position));
            Fixture fixture = FixtureFactory.AttachCircle(_physicsRadius, 1f, DynamicBody, _physicsOffset);
            
            DynamicBody.Restitution = 0.1f;
            DynamicBody.LinearDamping = 5f;
            DynamicBody.BodyType = BodyType.Dynamic;
            DynamicBody.UserData = this;
            DynamicBody.FixedRotation = true;
        }

        protected abstract void SetupDynamics();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = ConvertUnits.ToDisplayUnits(DynamicBody.Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
