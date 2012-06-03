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
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Gnomic.Anim;

namespace Eggtastic
{
    public class EnemyEntity : CharacterEntity
    {
        private const float MOVEMENT_VARIANCE = 5f;
        private const float FAST_SPEED = 10.0f;
        private const float RETREAT_SPEED = 6.5f;
        private const float SLOW_SPEED = 2.5f;
        private const float TARGET_AGGRO_RADIUS = 300f;
        private const float RETREAT_DISTANCE = 200f;
        private const float DIRECTION_CHANGE_THRESHOLD = 0.2f;
        private const float UPDATE_HEADING_INTERVAL = 0.5f;
        private const float UPDATE_ATTACK_HEADING_INTERVAL = 0.1f;

        private static readonly float PHYSICS_RADIUS = 0.4f;
        private static readonly Vector2 PHYSICS_OFFSET = new Vector2(0.0f, -0.3f);        
        
        // Timings:
        private const float TIME_IN_AIMLESS = 2f;

        private Vector2 _heading;
        private AnimationDirection _animDirection;
        private float _aimlessTime = 0f;
        private float _headingTime = 0f;

        public float MovementSpeed { get; set; }
        public bool HitPlayer { get; set; }
        public bool HitEgg { get; set; }

        public enum State
        {
            WanderAimless,
            WanderTowards,
            Attacking,
            Retreating,
            BeingSuckedIn
        }

        public enum AnimState
        {
            Walk,
            Sucked
        }

        public struct AnimKey
        {
            AnimState AnimState;
            AnimationDirection Direction;

            public AnimKey(AnimState state, AnimationDirection direction)
            {
                AnimState = state;
                Direction = direction;
            }

            public AnimKey(State state, AnimationDirection direction)
            {
                switch (state)
                {
                    case EnemyEntity.State.BeingSuckedIn:
                        AnimState = EnemyEntity.AnimState.Sucked;
                        break;
                    case EnemyEntity.State.Retreating:
                    case EnemyEntity.State.WanderAimless:
                    case EnemyEntity.State.WanderTowards:
                    case EnemyEntity.State.Attacking:
                    default:
                        AnimState = EnemyEntity.AnimState.Walk;
                        break;
                }

                Direction = direction;
            }
        }

        private Dictionary<AnimKey, ClipAnim> _animations = new Dictionary<AnimKey, ClipAnim>();

        public State CurrentState { get; set; }
        public CharacterEntity Target { get; set; }

        private float _attackPlayerWeight;
        private float _attackEggWeight;

        public float AttackEggWeight
        {
            get { return _attackEggWeight; }
            set
            {
                _attackEggWeight = value;
                _attackPlayerWeight = 1f - _attackEggWeight;
            }
        }
        public float AttackPlayerWeight
        {
            get { return _attackPlayerWeight; }
            set
            {
                _attackPlayerWeight = value;
                _attackEggWeight = 1f - _attackPlayerWeight;
            }
        }

        protected override void SetupDynamics()
        {
            CreateCircularBody();
            DynamicBody.CollisionCategories = (Category)
                CharacterEntity.CollisionCategory.Enemy;
            DynamicBody.CollidesWith = (Category)
                CharacterEntity.CollidesWith.Enemy;
        }

        private bool TargetIsClose()
        {
            float distance = Math.Abs((Target.Position - Position).Length());
            if (distance < TARGET_AGGRO_RADIUS)
            {
                return true;
            }
            return false;
        }

        private bool TargetIsWithinRetreatDistance()
        {
            float distance = Math.Abs((Target.Position - Position).Length());
            if (distance < RETREAT_DISTANCE)
            {
                return true;
            }
            return false;
        }

        private void MoveTowards(Vector2 to)
        {
            Vector2 v = to * MovementSpeed;
            DynamicBody.ApplyForce(v);
        }

        private void WanderAimless(GameTime gameTime)
        {
            FindNearestTarget();
            
            _aimlessTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TargetIsClose())
            {
                SwitchToAttacking();
                return;
            }
            else if (_aimlessTime > TIME_IN_AIMLESS)
            {
                _aimlessTime = 0f;
                CurrentState = State.WanderTowards;
            }

            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_headingTime > UPDATE_HEADING_INTERVAL)
            {
                CalculateAimlessHeading();
                _headingTime = 0.0f;
            }
            
            MoveTowards(_heading);
        }

        private void WanderTowards(GameTime gameTime)
        {
            FindNearestTarget();
            
            if (TargetIsClose())
            {
                SwitchToAttacking();
                return;
            }

            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_headingTime > UPDATE_HEADING_INTERVAL)
            {
                CalculateTowardsHeading();
                _headingTime = 0.0f;
            }
            
            MoveTowards(_heading);
        }

        private void Attacking(GameTime gameTime)
        {
            if (!Target.IsValid || HitEgg)
            {
                SwitchToWanderAimless();
                return;
            }
            else if (HitPlayer)
            {
                CurrentState = State.Retreating;
                MovementSpeed = RETREAT_SPEED;
                return;
            }

            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_headingTime > UPDATE_ATTACK_HEADING_INTERVAL)
            {
                CalculateApproachHeading();
                _headingTime = 0.0f;
            }
            
            MoveTowards(_heading);
        }

        private void Retreating(GameTime gameTime)
        {
            if (!TargetIsWithinRetreatDistance() || !Target.IsValid)
            {
                SwitchToWanderAimless();
                return;
            }

            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_headingTime > UPDATE_HEADING_INTERVAL)
            {
                CalculateRetreatHeading();
                _headingTime = 0.0f;
            }

            MoveTowards(_heading);
        }

        private void BeingSuckedIn(GameTime gameTime)
        {
            PlayNewEnemyAnimation();
            if (!IsPlayerSucking())
            {
                SwitchToRetreating();
            }

            const float SUCKED_FORC_MULTIPLIER = 40.0f;
            MoveTowards(_heading * SUCKED_FORC_MULTIPLIER);
            CalculateSuckedInHeading();
        }

        private void SwitchToAttacking()
        {
            CurrentState = State.Attacking;
            MovementSpeed = FAST_SPEED;
        }

        private void SwitchToRetreating()
        {
            CurrentState = State.Retreating;
            MovementSpeed = RETREAT_SPEED;
            CalculateRetreatHeading();
            PlayNewEnemyAnimation();
        }

        private void SwitchToWanderAimless()
        {
            CurrentState = State.WanderAimless;
            MovementSpeed = SLOW_SPEED;
            HitPlayer = false; HitEgg = false;
        }

        private bool IsPlayerSucking()
        {
            PlayerEntity player = Target as PlayerEntity;
            if (player != null)
            {
                return player.CurrentState == PlayerEntity.State.Sucking;
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (CurrentState)
            {
                case State.WanderAimless:
                    WanderAimless(gameTime);
                    break;
                case State.WanderTowards:
                    WanderTowards(gameTime);
                    break;
                case State.Attacking:
                    Attacking(gameTime);
                    break;
                case State.Retreating:
                    Retreating(gameTime);
                    break;
                case State.BeingSuckedIn:
                    BeingSuckedIn(gameTime);
                    break;
            }

            UpdateDirectionBasedOnVelocity();
        }

        public EnemyEntity(EggGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f)
        { }

        public EnemyEntity(EggGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f)
        { }

        public EnemyEntity(EggGameScreen gameScreen, Clip clip, Vector2 position,
                               Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f)
        { }

        public EnemyEntity(EggGameScreen gameScreen, Clip clip,
                            Vector2 position, Vector2 scale, float rotation)
            : base(gameScreen, clip, position, scale, rotation, PHYSICS_OFFSET, PHYSICS_RADIUS)
        {
            // Calculate what entity type to attack.
            CurrentState = State.WanderAimless;
            MovementSpeed = SLOW_SPEED;
            EntityType = Type.Enemy;
            HitPlayer = false;
            HitEgg = false;


            DynamicBody.OnCollision += CollisionHandler;
			FindNearestTarget();

            _animations[new AnimKey(AnimState.Walk, AnimationDirection.Left)] = clip.AnimSet["walk-left"];
            _animations[new AnimKey(AnimState.Walk, AnimationDirection.Right)] = clip.AnimSet["walk-right"];
            _animations[new AnimKey(AnimState.Walk, AnimationDirection.Up)] = clip.AnimSet["walk-up"];
            _animations[new AnimKey(AnimState.Walk, AnimationDirection.Down)] = clip.AnimSet["walk-down"];

            _animations[new AnimKey(AnimState.Sucked, AnimationDirection.Left)] = clip.AnimSet["sucked-right"];
            _animations[new AnimKey(AnimState.Sucked, AnimationDirection.Right)] = clip.AnimSet["sucked-left"];
            _animations[new AnimKey(AnimState.Sucked, AnimationDirection.Up)] = clip.AnimSet["sucked-down"];
            _animations[new AnimKey(AnimState.Sucked, AnimationDirection.Down)] = clip.AnimSet["sucked-up"];

            PlayNewEnemyAnimation();        
        }

        void PlayNewEnemyAnimation()
        {
            ClipAnim newAnim = null;
            if (_animations.TryGetValue(new AnimKey(CurrentState, _animDirection), out newAnim))
            {
                if (newAnim != ClipInstance.CurrentAnim)
                {
                     ClipInstance.Play(newAnim);
                }
            }
        }
        
        void UpdateDirectionBasedOnVelocity()
        {
            if (_heading.LengthSquared() > 0.001f)
            {
                // Get direction enemy is moving
                AnimationDirection newDirection = DirectionHelper.GetDirectionFromHeadingBiasHorizontal(_heading, 0.2f);
                if (newDirection == _animDirection)
                    return;

                if (newDirection == DirectionHelper.GetOppositeDirection(_animDirection))
                {
                    // If moving in the opposite direction to current facing, change instantly
                    _animDirection = newDirection;
                    PlayNewEnemyAnimation();
                }
                else
                {
                    // Only change when our heading when velocity starts moving slightly towards our heading
                    Vector2 velocity = DynamicBody.LinearVelocity;
                    if (velocity.LengthSquared() < 0.1f)
                        return;

                    // Avoid flickering 
                    Vector2 normalHeading = Vector2.Normalize(_heading); 
                    Vector2 normalVelocity = Vector2.Normalize(velocity);
                    float directionDotProduct = Vector2.Dot(normalHeading, normalVelocity);
                    if (directionDotProduct > DIRECTION_CHANGE_THRESHOLD)
                    {
                        _animDirection = newDirection;
                        PlayNewEnemyAnimation();
                    }
                }
            }
        }

        private bool CollisionHandler(Fixture f1, Fixture f2, Contact contact)
        {
            PlayerEntity player = f2.Body.UserData as PlayerEntity;
            EggEntity egg = f2.Body.UserData as EggEntity;
            if (player != null)
            {
                return CollisionWithPlayer(f1, f2, contact, player);
            }
            else if (egg != null)
            {
                return CollisionWithEgg(f1, f2, contact, egg);
            }
            return true;
        }

        private bool CollisionWithPlayer(Fixture f1, Fixture f2,
                                         Contact contact, PlayerEntity player)
        {
            if (player.EatEnemiesInRange())
            {
                // If this enemy was eaten, it will become invalid so return false to cancel the collision.
                return this.IsValid;
            }
            return true;
        }

        private bool CollisionWithEgg(Fixture f1, Fixture f2,
                                      Contact contact, EggEntity egg)
        {
            if (CurrentState == State.BeingSuckedIn)
                return false;

            HitEgg = true;
            egg.SwitchToDeathState();
            return true;
        }

        private void ApplyImpactImpulse(PlayerEntity player)
        {
            Vector2 toPlayer = 0.075f * (player.Position - Position);
            player.DynamicBody.ApplyLinearImpulse(toPlayer);
        }

        private void FindNearestTarget()
        {
            float min = 9999999f;
            foreach (EggEntity egg in GameScreen.Eggs)
            {
                float dist = Math.Abs(Vector2.Distance(egg.Position, Position));
                if (dist < min)
                {
                    Target = egg;
                    min = dist;
                }
            }

            // currently enemies only target eggs
            #if false
            foreach (GameEntity entity in GameScreen.ActiveEntities)
            {
                CharacterEntity character = entity as CharacterEntity;
                if (character != null)
                {
                    float dist = 0f;
                    switch (character.EntityType)
                    {
                        case Type.Player:
                        case Type.Egg:
                            dist = Vector2.Distance(character.Position, Position);
                            if (dist < min)
                            {
                                dist = min;
                                Target = character;
                            }
                            break;
                        case Type.Enemy:
                        default:
                            break;
                    }
                }
            }
            #endif
        }

        private void CalculateAimlessHeading()
        {
            if (Position.X < 0f || Position.Y < 0f)
            {
                _heading = GameScreen.ScreenCenter - _heading;
            }
            else if (Position.X > GameScreen.Viewport.Width ||
                     Position.Y > GameScreen.Viewport.Height)
            {
                _heading = -1f * (GameScreen.ScreenCenter - _heading);
            }
            else
            {
                float rx = (2f * (float)GameScreen.RandomNum.NextDouble()) - 1f;
                float ry = (2f * (float)GameScreen.RandomNum.NextDouble()) - 1f;
                _heading.X += rx * 0.25f; _heading.Y += ry * 0.25f;
            }
            _heading.Normalize();
        }

        private void CalculateTowardsHeading()
        {
            Vector2 to = Target.Position - Position; to.Normalize();
            to.X += (float)GameScreen.RandomNum.NextDouble() * 0.75f;
            to.Y += (float)GameScreen.RandomNum.NextDouble() * 0.75f;
            to.Normalize();
            _heading += to;
            _heading.Normalize();
        }

        private void CalculateApproachHeading()
        {
            Vector2 to = Target.Position - Position; to.Normalize();
            to.X += (float)GameScreen.RandomNum.NextDouble() * 0.5f;
            to.Y += (float)GameScreen.RandomNum.NextDouble() * 0.5f;
            to.Normalize();
            _heading = to;
        }

        private void CalculateRetreatHeading()
        {
            _heading = Position - Target.Position; _heading.Normalize();
            _heading.X += (float)GameScreen.RandomNum.NextDouble() * 2f;
            _heading.Y += (float)GameScreen.RandomNum.NextDouble() * 2f;
            _heading.Normalize();
        }

        private void CalculateSuckedInHeading()
        {
            PlayerEntity player = Target as PlayerEntity;
            _heading = 0.25f * (player.Position - Position);
			_heading.Normalize();
        }
    }
}