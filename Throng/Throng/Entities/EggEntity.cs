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

namespace Throng
{
    public class EggEntity : CharacterEntity
    {
        private static readonly float PHYSICS_RADIUS = 0.15f;
        private static readonly Vector2 PHYSICS_OFFSET = new Vector2(0.0f, -0.15f);
        private static readonly float DEATH_ANIM_TIME = 1f;

        private Clip _clip = null;
        private Dictionary<State, ClipAnim> _animations =
            new Dictionary<State, ClipAnim>();
        private float _deathTime = 0.0f;

        SoundEffect _sfxExplode;

        public enum State
        {
            Idle, Death
        }

        public State CurrentState { get; set; }

        public EggEntity(ThrongGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f)
        { }

        public EggEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f)
        { }

        public EggEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position,
                         Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f)
        { }

        public EggEntity(ThrongGameScreen gameScreen, Clip clip,
                         Vector2 position, Vector2 scale, float rotation)
            : base(gameScreen, clip, position, scale, rotation, PHYSICS_OFFSET, PHYSICS_RADIUS)
        {
            _clip = clip;
            
            EntityType = Type.Egg;
            CurrentState = State.Idle;

            InitialiseAnimations();
            PlayNewEggAnimation();

            _sfxExplode = gameScreen.Content.Load<SoundEffect>("Sounds/Explode 1");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CurrentState == State.Death)
            {
                Death((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void SwitchToDeathState()
        {
            CurrentState = State.Death;
            IsValid = false;

            PlayNewEggAnimation();

            _sfxExplode.Play();
        }

        protected override void SetupDynamics()
        {
            CreateCircularBody();
            DynamicBody.CollisionCategories = (Category)
                CharacterEntity.CollisionCategory.Egg;
            DynamicBody.CollidesWith = (Category)
                CharacterEntity.CollidesWith.Egg;
        }

        private void Death(float dt)
        {
            _deathTime += dt;
            if (_deathTime > DEATH_ANIM_TIME)
            {
                GameScreen.DestroyEgg(this);
            }
        }

        private void PlayNewEggAnimation()
        {
            //ClipAnim newAnim = null;
            //if (_animations.TryGetValue(CurrentState, out newAnim))
            //{
            //    if (newAnim != ClipInstance.CurrentAnim)
            //    {
            //        ClipInstance.Play(newAnim);
            //    }
            //}
        }

        private void InitialiseAnimations()
        {
            _animations[State.Idle] = _clip.AnimSet["bounce"];
            _animations[State.Death] = _clip.AnimSet["death"];
        }
    }
}