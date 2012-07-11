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

namespace Throng
{
    public class EnemyEntity : CharacterEntity
    {
        private const float MOVEMENT_VARIANCE = 5f;
        private const float FAST_SPEED = 10.0f;
        private const float RETREAT_SPEED = 6.5f;
        private const float SLOW_SPEED = 2.5f;
        private const float TARGET_ATTACK_RADIUS = 100f;
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
        private float _health = 100.0f;
        private float _stateTransitionCountdown = -1.0f;

        public float MovementSpeed { get; set; }
        public bool HitPlayer { get; set; }
        public bool HitEgg { get; set; }
        public float Health { get { return _health; } }

        public enum State
        {
            WanderAimless,
            WanderTowards,
            Retreat,
            Attack,
            Hit,
            Death
        }

        public enum AnimState
        {
            Idle,
            Run,
            Attack,
            Hit,
            Death
        }

        public struct AnimKey
        {
            public AnimState State;
            public AnimationDirection Direction;

            public AnimKey(AnimState state, AnimationDirection direction)
            {
                State = state;
                Direction = direction;
            }
        }

        private Dictionary<AnimKey, ClipAnimSet> _animations = new Dictionary<AnimKey, ClipAnimSet>();

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

        private AnimState GetAnimState()
        {
            switch (CurrentState)
            {
                case State.WanderAimless:
                    return AnimState.Run;
                case State.WanderTowards:
                    return AnimState.Run;
                case State.Attack:
                    return AnimState.Attack;
                case State.Retreat:
                    return AnimState.Run;
                case State.Hit:
                    return AnimState.Hit;
                case State.Death:
                    return AnimState.Death;
                default:
                    return  AnimState.Idle;
            }
        }

        private bool TargetIsClose()
        {
            float distance = Math.Abs((Target.Position - Position).Length());
            if (distance < TARGET_ATTACK_RADIUS)
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
            _stateTransitionCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_stateTransitionCountdown <= 0.0f)
            {
                CalculateTowardsHeading();
                if (TargetIsClose())
                {
                    SwitchToAttacking();
                }
                else
                {
                    SwitchToWanderAimless();
                }
            }
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

        private void SwitchToAttacking()
        {
            CurrentState = State.Attack;
            MovementSpeed = 0.0f;
            _stateTransitionCountdown = PlayNewEnemyAnimation();
        }

        private void SwitchToRetreating()
        {
            CurrentState = State.Retreat;
            MovementSpeed = RETREAT_SPEED;
            CalculateRetreatHeading();
            PlayNewEnemyAnimation();
        }

        private void SwitchToWanderAimless()
        {
            CurrentState = State.Retreat;
            MovementSpeed = SLOW_SPEED;
            HitPlayer = false; HitEgg = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Tweak.PermanentlyFreezeEnemies)
            {
                return;
            }
            
            _headingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (CurrentState)
            {
                case State.WanderAimless:
                    WanderAimless(gameTime);
                    break;
                case State.WanderTowards:
                    WanderTowards(gameTime);
                    break;
                case State.Attack:
                    Attacking(gameTime);
                    break;
                case State.Retreat:
                    Retreating(gameTime);
                    break;
                case State.Hit:
                    Hit(gameTime);
                    break;
                case State.Death:
                    Death(gameTime);
                    break;
            }

            UpdateDirectionBasedOnVelocity();
        }

        public EnemyEntity(ThrongGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f)
        { }

        public EnemyEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f)
        { }

        public EnemyEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position,
                               Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f)
        { }

        public EnemyEntity(ThrongGameScreen gameScreen, Clip clip,
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

            _animations[new AnimKey(AnimState.Idle, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("idle-left");
            _animations[new AnimKey(AnimState.Idle, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("right-right");

            _animations[new AnimKey(AnimState.Run, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("run-right");
            _animations[new AnimKey(AnimState.Run, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("run-left");

            _animations[new AnimKey(AnimState.Attack, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("attack-right");
            _animations[new AnimKey(AnimState.Attack, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("attack-left");

            _animations[new AnimKey(AnimState.Hit, AnimationDirection.Right)] = clip.AnimSet.CreateSubSet(true, "hitA-right", "hitB-right", "hitC-right");
            _animations[new AnimKey(AnimState.Hit, AnimationDirection.Left)] = clip.AnimSet.CreateSubSet(true, "hitA-left", "hitB-left", "hitC-left");

            _animations[new AnimKey(AnimState.Death, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("death-right");
            _animations[new AnimKey(AnimState.Death, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("death-left");

            PlayNewEnemyAnimation();        
        }

        ClipAnimSet currentAnimSet = null;
        float PlayNewEnemyAnimation()
        {
            ClipAnimSet newAnimSet = null;
            AnimKey key = new AnimKey(GetAnimState(), _animDirection);
            if (_animations.TryGetValue(key, out newAnimSet))
            {
                if (newAnimSet != currentAnimSet)
                {
                    bool isLooping = key.State == AnimState.Idle || key.State == AnimState.Run || key.State == AnimState.Attack;
                    ClipAnim newAnim = newAnimSet.GetNextAnim();
                    if (newAnim == null)
                    {
                        throw new InvalidOperationException("Could not find Enemy animation for " + key.State.ToString() + "-" + key.Direction.ToString());
                    }
                    else
                    {
                        ClipInstance.Play(newAnim, isLooping);
                    }
                    currentAnimSet = newAnimSet;
                }
                return ClipInstance.CurrentAnim.DurationInSecondsRemaining;
            }
            return 0.0f;
        }
        
        void UpdateDirectionBasedOnVelocity()
        {
            if (_heading.LengthSquared() > 0.001f)
            {
                // Get direction enemy is moving
                AnimationDirection newDirection = DirectionHelper.GetDirectionFromHeading(_heading);
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
            if (player != null)
            {
                return CollisionWithPlayer(f1, f2, contact, player);
            }
            return true;
        }

        private bool CollisionWithPlayer(Fixture f1, Fixture f2,
                                         Contact contact, PlayerEntity player)
        {
            //if (player.EatEnemiesInRange())
            //{
            //    // If this enemy was eaten, it will become invalid so return false to cancel the collision.
            //    return this.IsValid;
            //}
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

            // currently enemies only target players
            #if false
            foreach (EggEntity egg in GameScreen.Eggs)
            {
                float dist = Math.Abs(Vector2.Distance(egg.Position, Position));
                if (dist < min)
                {
                    Target = egg;
                    min = dist;
                }
            }
            #endif

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

        public void DoDamage(float hp)
        {
            if (CurrentState != State.Hit)
            {
                _health -= hp;
                if (_health > 0.0f)
                {
                    SwitchToHit();
                }
                else
                {
                    SwitchToDeath();
                }
            }
        }


        private void SwitchToHit()
        {
            CurrentState = State.Hit;
            _stateTransitionCountdown = PlayNewEnemyAnimation();

        }
        private void Hit(GameTime gameTime)
        {
            _stateTransitionCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_stateTransitionCountdown <= 0.0f)
            {
                SwitchToRetreating();
            }
        }

        private void SwitchToDeath()
        {
            CurrentState = State.Death;
            _stateTransitionCountdown = PlayNewEnemyAnimation() + 2.0f;
        }
        private void Death(GameTime gameTime)
        {
            _stateTransitionCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_stateTransitionCountdown <= 0.0f)
            {
                GameScreen.DestroyEnemy(this);
            }
        }
    }
}