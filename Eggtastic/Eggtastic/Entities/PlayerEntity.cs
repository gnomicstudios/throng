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
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Gnomic.Anim;

namespace Eggtastic
{
    public class PlayerEntity : CharacterEntity
    {
        public const float TARGET_SPEED = 10.0f;
        public const float PER_FRAME_FORCE_INCREMENT = 1.0f;
        private const float MOVEMENT_FORCE = 40f;
        private const float IDLE_SPEED_THRESHOLD = 0.2f;
        private const float DIRECTION_CHANGE_THRESHOLD = 0.3f;

        private const double EATING_EGG_DURATION = 0.9f;
        private const double EATING_ANIM_DURATION = 1.3f;
        private const float LUNGE_DURATION = 0.8f;
        
        private static readonly float PHYSICS_RADIUS = 0.5f;
        private static readonly Vector2 PHYSICS_OFFSET = new Vector2(0.0f, -0.45f);

        private Body _sensor;
        private float _sensorRotation;
        private AnimationDirection _animDirection;
        private Vector2 _movementDirection;
        private double _eatingTime;
        private bool _eggSpawned;
        private double _lungeTime;
        

        private float _baseForce;
        private float _targetSpeed = TARGET_SPEED;

        int _eatingSfxId;
        double[] _eatingSfxTimes = new double[] { 0.0, 0.4, 0.6, 0.9 };

        SoundEffect[] _sfxEat = new SoundEffect[4];
        SoundEffect _sfxChomp;
        SoundEffect _sfxSwallow;
        SoundEffect _sfxFart;
        SoundEffect _sfxPop;
        SoundEffect _sfxMove;
        SoundEffect _sfxSuck;

        SoundEffectInstance _sfxInstSuck;
        SoundEffectInstance _sfxInstMove;

        public State CurrentState { get; set; }
        public Body Sensor
        {
            get { return _sensor; }
        }

        public enum State
        {
            Idle,
            Moving,
            Lunging,
            Sucking,
            Eating
        }

        public struct AnimKey
        {
            State State;
            AnimationDirection Direction;

            public AnimKey(State state, AnimationDirection direction)
            {
                State = state;
                Direction = direction;
            }
        }

        Dictionary<AnimKey, ClipAnim> _animations = new Dictionary<AnimKey, ClipAnim>();

        public void PlayFartSound()
        {
            _sfxFart.Play();
        }

        private Vertices CreateTrapezoid(float lowerWidth, float upperWidth,
                                         float height)
        {
            Vertices trapezoid = new Vertices(4);
            trapezoid.Add(new Vector2(-(lowerWidth / 2f), 0f));
            trapezoid.Add(new Vector2(-(upperWidth / 2f), height));
            trapezoid.Add(new Vector2(upperWidth / 2f, height));
            trapezoid.Add(new Vector2(lowerWidth / 2f, 0f));
            trapezoid.Reverse();
            return trapezoid;
        }
        
        protected override void SetupDynamics()
        {
            CreateCircularBody();
            DynamicBody.CollisionCategories = (Category)
                CharacterEntity.CollisionCategory.Player;
            DynamicBody.CollidesWith = (Category)
                CharacterEntity.CollidesWith.Player;

            Vertices trapezoid = CreateTrapezoid(1f, 3f, 2.5f);

            _sensor = BodyFactory.CreatePolygon(GameScreen.World, trapezoid, 1f, ConvertUnits.ToSimUnits(GameScreen.ScreenCenter));
            _sensor.IsSensor = true;
            _sensor.Enabled = false;
            _sensor.BodyType = BodyType.Dynamic;
            _sensor.CollisionCategories = (Category)
                CollisionCategory.Sensor;
            _sensor.CollidesWith = (Category)
                CollidesWith.Sensor;
            _sensor.OnCollision += OnSensorCollision;
        }

        public bool OnSensorCollision(Fixture f1, Fixture f2, Contact contact)
        {
            EnemyEntity enemy = f2.Body.UserData as EnemyEntity;
            if (enemy != null)
            {
                if (enemy.CurrentState != EnemyEntity.State.BeingSuckedIn)
                {
                    enemy.CurrentState = EnemyEntity.State.BeingSuckedIn;
                    enemy.MovementSpeed = 0.3f;
                    enemy.Target = this;
                }
            }
            return true;
        }

        public void SpawnEgg()
        {
            const float SPAWN_DISTANCE = 10f;

            _eggSpawned = true;

            Vector2 direction = -1f * DirectionHelper.GetFacingFromDirection(_animDirection);
            if (_animDirection == AnimationDirection.Left ||
                _animDirection == AnimationDirection.Right)
            {
                direction.Y += (float)GameScreen.RandomNum.NextDouble() - 0.5f;
            }
            else if (_animDirection == AnimationDirection.Up ||
                     _animDirection == AnimationDirection.Down)
            {
                direction.X += (float)GameScreen.RandomNum.NextDouble() - 0.5f;
            }
            direction.Normalize();

            Vector2 spawn = Position + (SPAWN_DISTANCE * direction) + PHYSICS_OFFSET;

            EggEntity egg = GameScreen.AddEgg(spawn);
            egg.DynamicBody.ApplyLinearImpulse(direction);
        }

        public void ResetSounds()
        {
            StopLoopingSound(_sfxInstMove);
            StopLoopingSound(_sfxInstSuck);
        }

        public void StopLoopingSound(SoundEffectInstance sfxInst)
        {
            if (sfxInst != null && sfxInst.State == SoundState.Playing)
            {
                sfxInst.Stop();
            }
        }

        public void PlayLoopingSound(SoundEffect sfx, float volume, ref SoundEffectInstance sfxInst)
        {
            if (sfxInst == null || sfxInst.State != SoundState.Playing)
            {
                sfxInst = sfx.CreateInstance();
                sfxInst.IsLooped = true;
                sfxInst.Volume = volume;
                sfxInst.Play();
            }
        }

        public void SwitchToLunging()
        {
            if (_movementDirection != Vector2.Zero)
            {
                DynamicBody.ApplyLinearImpulse(20.0f * Vector2.Normalize(_movementDirection));
                CurrentState = State.Lunging;
                PlayNewCharacterAnimation();
                _lungeTime = 0;
            }
        }

        public void SwitchToSucking()
        {
            CurrentState = State.Sucking;
            PlayNewCharacterAnimation();
            _sensor.Enabled = true;

            PlayLoopingSound(_sfxSuck, 0.9f, ref _sfxInstSuck);
            StopLoopingSound(_sfxInstMove);
        }

        public void SwitchToMoving()
        {
            CurrentState = State.Moving;
            PlayNewCharacterAnimation();
            _sensor.Enabled = false;

            PlayLoopingSound(_sfxMove, 0.6f, ref _sfxInstMove);
            StopLoopingSound(_sfxInstSuck);
        }

        public void SwitchToIdle()
        {
            CurrentState = State.Idle;
            PlayNewCharacterAnimation();
            _sensor.Enabled = false;

            StopLoopingSound(_sfxInstMove);
            StopLoopingSound(_sfxInstSuck);
        }

        void PlayNewCharacterAnimation()
        {
            ClipAnim newAnim = null;
            if (_animations.TryGetValue(new AnimKey(CurrentState, _animDirection),
                                       out newAnim))
            {
                if (newAnim != ClipInstance.CurrentAnim)
                {
                    ClipInstance.Play(newAnim);
                }
            }
        }

        private void AnchorSensorToDynamicBody()
        {
            _sensor.Rotation = MathHelper.ToRadians(_sensorRotation);
            _sensor.Position = ConvertUnits.ToSimUnits(Position) + PHYSICS_OFFSET;
        }

        public void SwitchToEating()
        {
            _eatingSfxId = 0;
            _eggSpawned = false;
            CurrentState = State.Eating;
            PlayNewCharacterAnimation();
            StopLoopingSound(_sfxInstSuck);
        }

        public void SwitchToIdleOrMoving(State state)
        {
            Debug.Assert(state == State.Idle || state == State.Moving);
            CurrentState = state;
            PlayNewCharacterAnimation();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //_targetSpeed = TARGET_SPEED;
            switch (CurrentState)
            {
                case State.Eating:
                    {
                        _eatingTime += gameTime.ElapsedGameTime.TotalSeconds;
                        if (_eatingTime > EATING_ANIM_DURATION)
                        {
                            _eatingTime = 0f;
                            SwitchToIdleOrMoving(State.Moving);
                        }
                        else if (!_eggSpawned && _eatingTime > EATING_EGG_DURATION)
                        {
                            SpawnEgg();
                        }

                        while (_eatingSfxId < _eatingSfxTimes.Length &&
                            _eatingTime > _eatingSfxTimes[_eatingSfxId])
                        {
                            _sfxEat[_eatingSfxId].Play();
                            _eatingSfxId++;
                        }
                    }
                    break;
                case State.Lunging:
                    {
                        _lungeTime += gameTime.ElapsedGameTime.TotalSeconds;
                        //_targetSpeed = TARGET_SPEED * 2.0f;

                        if (_lungeTime > LUNGE_DURATION)
                        {
                            SwitchToMoving();
                        }
                    }
                    break;
            }

            float currentSpeed = DynamicBody.LinearVelocity.Length();
            if (currentSpeed < _targetSpeed)
            {
                _baseForce += PER_FRAME_FORCE_INCREMENT;
            }
            else if (currentSpeed > _targetSpeed)
            {
                _baseForce -= PER_FRAME_FORCE_INCREMENT;
            }
            
            Vector2 movementDirection = _baseForce * Vector2.UnitX;
            DynamicBody.ApplyForce(movementDirection);

            SwitchStateBasedOnSpeed();

            SetCurrentDirection();

            AnchorSensorToDynamicBody();
        }


        void SwitchStateBasedOnSpeed()
        {
            float speedSqr = DynamicBody.LinearVelocity.LengthSquared();
            switch (CurrentState)
            {
                case State.Idle:
                    if (_movementDirection != Vector2.Zero &&
                        speedSqr > IDLE_SPEED_THRESHOLD * IDLE_SPEED_THRESHOLD)
                    {
                        SwitchToMoving();
                    }
                    break;

                case State.Moving:
                    if (_movementDirection == Vector2.Zero &&
                        speedSqr < IDLE_SPEED_THRESHOLD * IDLE_SPEED_THRESHOLD)
                    {
                        SwitchToIdle();
                    }
                    break;
                default:
                    break;
            }
        }

        void SetCurrentDirection()
        {
            if (_movementDirection != Vector2.Zero)
            {
                // Get direction we are pushing
                AnimationDirection newDirection = DirectionHelper.GetDirectionFromHeading(_movementDirection);
                if (newDirection == _animDirection)
                    return;

                if (newDirection == DirectionHelper.GetOppositeDirection(_animDirection))
                {
                    // If pushing in the opposite direction to current facing, change instantly
                    _animDirection = newDirection;
                    PlayNewCharacterAnimation();
                }
                else
                {
                    // Only change when our heading when velocity starts moving slightly towards our heading
                    Vector2 velocity = DynamicBody.LinearVelocity;
                    if (velocity.LengthSquared() < 0.01f)
                        return;

                    Vector2 normalHeading = Vector2.Normalize(_movementDirection);
                    Vector2 normalVelocity = Vector2.Normalize(velocity);
                    float directionDotProduct = Vector2.Dot(normalHeading, normalVelocity);
                    if (directionDotProduct > DIRECTION_CHANGE_THRESHOLD)
                    {
                        _animDirection = newDirection;
                        PlayNewCharacterAnimation();
                    }
                }
            }
        }

        public void HandleKeyboardInput()
        {
            _movementDirection = Vector2.Zero;

            switch (CurrentState)
            {
                case State.Eating:
                    break;
                case State.Idle:
                case State.Moving:
                    {
                        if (Input.ButtonDownMapped((int)Controls.Up))
                        {
                            MoveUp();
                        }
                        if (Input.ButtonDownMapped((int)Controls.Down))
                        {
                            MoveDown();
                        }
                        if (Input.ButtonDownMapped((int)Controls.Left))
                        {
                            MoveLeft();
                        }
                        if (Input.ButtonDownMapped((int)Controls.Right))
                        {
                            MoveRight();
                        }
						if (Input.ButtonDownMapped((int)Controls.Suck))
                        {
                            SwitchToLunging();
                            //SwitchToSucking();
                            //goto case State.Sucking;
                        }
                        else if (_movementDirection != Vector2.Zero)
                        {
                            _movementDirection.Normalize();
                            DynamicBody.ApplyForce(_movementDirection * MOVEMENT_FORCE);
                        }
                        break;
                    }
                case State.Sucking:
                    {
						if (Input.ButtonDownMapped((int)Controls.Suck))
                        {
                            SuckIn();
                        }
                        else
                        {
                            SwitchToMoving();
                        }
                        break;
                    }
            }
        }

        public void SuckIn()
        {
            
        }

        public void MoveRight()
        {
            _sensorRotation = -90f;
            _movementDirection += new Vector2(1.0f, 0f);
        }

        public void MoveLeft()
        {
            _sensorRotation = 90f;
            _movementDirection += new Vector2(-1.0f, 0f);
        }

        public void MoveUp()
        {
            _sensorRotation = 180f;
            _movementDirection += new Vector2(0f, -1.0f);
        }

        public void MoveDown()
        {
            _sensorRotation = 0f;
            _movementDirection += new Vector2(0f, 1.0f);
        }

        public PlayerEntity(EggGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f)
        { }

        public PlayerEntity(EggGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f)
        { }

        public PlayerEntity(EggGameScreen gameScreen, Clip clip, Vector2 position,
                               Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f)
        { }

        public PlayerEntity(EggGameScreen gameScreen, Clip clip,
                            Vector2 position, Vector2 scale, float rotation)
            : base(gameScreen, clip, position, scale, rotation, PHYSICS_OFFSET, PHYSICS_RADIUS)
        {
            EntityType = Type.Player;
            
            _animDirection = AnimationDirection.Right;

            LoadSounds(gameScreen.Content);

            SetupAnimations(clip);

            SwitchToMoving();
         }

        private void LoadSounds(ContentManager content)
        {
            _sfxChomp = content.Load<SoundEffect>("Sounds/Chomp 2");
            _sfxSwallow = content.Load<SoundEffect>("Sounds/Swallow");
            _sfxFart = content.Load<SoundEffect>("Sounds/Fart 4");
            _sfxPop = content.Load<SoundEffect>("Sounds/Pop 2");
            _sfxMove = content.Load<SoundEffect>("Sounds/PlayerMove 1");
            _sfxSuck = content.Load<SoundEffect>("Sounds/Suck In Loop");

            _sfxEat[0] = _sfxChomp;
            _sfxEat[1] = _sfxSwallow;
            _sfxEat[2] = _sfxFart;
            _sfxEat[3] = _sfxPop;
        }

        private void SetupAnimations(Clip clip)
        {
            _animations[new AnimKey(State.Idle, AnimationDirection.Left)] = clip.AnimSet["idle-left"];
            _animations[new AnimKey(State.Idle, AnimationDirection.Right)] = clip.AnimSet["idle-right"];
            _animations[new AnimKey(State.Idle, AnimationDirection.Up)] = clip.AnimSet["idle-up"];
            _animations[new AnimKey(State.Idle, AnimationDirection.Down)] = clip.AnimSet["idle-down"];

            _animations[new AnimKey(State.Moving, AnimationDirection.Left)] = clip.AnimSet["walk-left"];
            _animations[new AnimKey(State.Moving, AnimationDirection.Right)] = clip.AnimSet["walk-right"];
            _animations[new AnimKey(State.Moving, AnimationDirection.Up)] = clip.AnimSet["walk-up"];
            _animations[new AnimKey(State.Moving, AnimationDirection.Down)] = clip.AnimSet["walk-down"];

            _animations[new AnimKey(State.Sucking, AnimationDirection.Left)] = clip.AnimSet["suck-left"];
            _animations[new AnimKey(State.Sucking, AnimationDirection.Right)] = clip.AnimSet["suck-right"];
            _animations[new AnimKey(State.Sucking, AnimationDirection.Up)] = clip.AnimSet["suck-up"];
            _animations[new AnimKey(State.Sucking, AnimationDirection.Down)] = clip.AnimSet["suck-down"];

            _animations[new AnimKey(State.Eating, AnimationDirection.Left)] = clip.AnimSet["eat-left"];
            _animations[new AnimKey(State.Eating, AnimationDirection.Right)] = clip.AnimSet["eat-right"];
            _animations[new AnimKey(State.Eating, AnimationDirection.Up)] = clip.AnimSet["eat-up"];
            _animations[new AnimKey(State.Eating, AnimationDirection.Down)] = clip.AnimSet["eat-down"];

            _animations[new AnimKey(State.Lunging, AnimationDirection.Left)] = clip.AnimSet["eat-left"];
            _animations[new AnimKey(State.Lunging, AnimationDirection.Right)] = clip.AnimSet["eat-right"];
            _animations[new AnimKey(State.Lunging, AnimationDirection.Up)] = clip.AnimSet["eat-up"];
            _animations[new AnimKey(State.Lunging, AnimationDirection.Down)] = clip.AnimSet["eat-down"];
        }

        public Vector2 GetHeading()
        {
            return _movementDirection;
        }
    }
}