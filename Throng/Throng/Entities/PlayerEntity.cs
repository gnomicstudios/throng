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

namespace Throng
{
    public class PlayerEntity : CharacterEntity
    {
        public const float TARGET_SPEED = 10.0f;
        public const float PER_FRAME_FORCE_INCREMENT = 1.0f;
        private float MOVEMENT_FORCE = Tweak.MOVEMENT_FORCE;
        private const float IDLE_SPEED_THRESHOLD = 0.2f;
        private const float DIRECTION_CHANGE_THRESHOLD = 0.3f;
        private const float ENEMY_MELEE_RANGE = 100.0f;
        private const double MELEE_ANIM_DURATION = 1.3f;
        private const float LUNGE_DURATION = 0.8f;
        
        private static readonly float PHYSICS_RADIUS = 0.5f;
        private static readonly Vector2 PHYSICS_OFFSET = new Vector2(0.0f, -0.45f);

        private Body _sensor;
        private float _sensorRotation;
        private AnimationDirection _animDirection;
        private Vector2 _movementDirection;
        private double _meleeTime;
        private double _lungeTime;

        private float _baseForce;
        private float _targetSpeed = TARGET_SPEED;

        //int _eatingSfxId;
        //double[] _eatingSfxTimes = new double[] { 0.0, 0.4, 0.6, 0.9 };

        //SoundEffect[] _sfxEat = new SoundEffect[4];
        //SoundEffect _sfxChomp;
        //SoundEffect _sfxSwallow;
        //SoundEffect _sfxFart;
        //SoundEffect _sfxPop;
        //SoundEffect _sfxMove;
        //SoundEffect _sfxSuck;

        //SoundEffectInstance _sfxInstSuck;
        //SoundEffectInstance _sfxInstMove;

        public Weapon CurrentWeapon { get; set; }

        public State CurrentState { get; set; }
        public Body Sensor
        {
            get { return _sensor; }
        }

        public enum State
        {
            Idle,
            Run,
            Attack,
        }

        public enum Weapon
        {
            Melee,
            Pistol
        }

        public struct AnimKey
        {
            State State;
            Weapon Weapon;
            AnimationDirection Direction;

            public AnimKey(State state, Weapon weapon, AnimationDirection direction)
            {
                State = state;
                Weapon = weapon;
                Direction = direction;
            }
        }

        Dictionary<AnimKey, ClipAnimSet> _animations = new Dictionary<AnimKey, ClipAnimSet>();

        public void PlayFartSound()
        {
            //_sfxFart.Play();
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

            Vertices trapezoid = CreateTrapezoid(1f, 3f * Tweak.MELEE_AREA_MULTIPLER, 2.5f * Tweak.MELEE_AREA_MULTIPLER);

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
            //EnemyEntity enemy = f2.Body.UserData as EnemyEntity;
            //if (enemy != null)
            //{
            //    if (enemy.CurrentState != EnemyEntity.State.Attack)
            //    {
            //        enemy.CurrentState = EnemyEntity.State.Attack;
            //        enemy.MovementSpeed = 0.3f;
            //        enemy.Target = this;
            //    }
            //}
            return true;
        }
        
        public void ResetSounds()
        {
            //StopLoopingSound(_sfxInstMove);
            //StopLoopingSound(_sfxInstSuck);
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

        public void SwitchToAttacking()
        {
            CurrentState = State.Attack;
            PlayNewCharacterAnimation();
            //_sensor.Enabled = true;

            //PlayLoopingSound(_sfxSuck, 0.9f, ref _sfxInstSuck);
            //StopLoopingSound(_sfxInstMove);
        }

        public void SwitchToMoving()
        {
            CurrentState = State.Run;
            PlayNewCharacterAnimation();
            //_sensor.Enabled = false;

            //PlayLoopingSound(_sfxMove, 0.6f, ref _sfxInstMove);
            //StopLoopingSound(_sfxInstSuck);
        }

        public void SwitchToIdle()
        {
            CurrentState = State.Idle;
            PlayNewCharacterAnimation();
            //_sensor.Enabled = false;

            //StopLoopingSound(_sfxInstMove);
            //StopLoopingSound(_sfxInstSuck);
        }

        ClipAnimSet lastAnimSet = null;
        void PlayNewCharacterAnimation()
        {
            ClipAnimSet newAnimSet = null;
            if (_animations.TryGetValue(new AnimKey(CurrentState, CurrentWeapon, _animDirection),
                                       out newAnimSet))
            {
                if (newAnimSet != lastAnimSet)
                {
                    ClipInstance.Play(newAnimSet.GetNextAnim());
                    lastAnimSet = newAnimSet;
                }
            }
        }

        private void AnchorSensorToDynamicBody()
        {
            _sensor.Rotation = MathHelper.ToRadians(_sensorRotation);
            _sensor.Position = ConvertUnits.ToSimUnits(Position) + PHYSICS_OFFSET;
        }
        
        public void SwitchToIdleOrMoving(State state)
        {
            //Debug.Assert(state == State.Idle || state == State.Run);
            CurrentState = state;
            PlayNewCharacterAnimation();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            float currentSpeed = DynamicBody.LinearVelocity.Length();
            if (currentSpeed < _targetSpeed)
            {
                _baseForce += PER_FRAME_FORCE_INCREMENT;
            }
            else if (currentSpeed > _targetSpeed)
            {
                _baseForce -= PER_FRAME_FORCE_INCREMENT;
            }


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

                case State.Run:
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
            Tweak.Stats = "";
            Tweak.Stats += "_animDirection: " + _animDirection.ToString() + "\r\n";
            Tweak.Stats += "_movementDirection: " + _movementDirection.ToString() + "\r\n";
            
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
                //else
                //{
                //    // Only change when our heading when velocity starts moving slightly towards our heading
                //    Vector2 velocity = DynamicBody.LinearVelocity;
                //    if (velocity.LengthSquared() < 0.01f)
                //        return;

                //    Vector2 normalHeading = Vector2.Normalize(_movementDirection);
                //    Vector2 normalVelocity = Vector2.Normalize(velocity);
                //    float directionDotProduct = Vector2.Dot(normalHeading, normalVelocity);
                //    if (directionDotProduct > DIRECTION_CHANGE_THRESHOLD)
                //    {
                //        _animDirection = newDirection;
                //        PlayNewCharacterAnimation();
                //    }
                //}
            }
        }

        public void HandleKeyboardInput()
        {
            _movementDirection = Vector2.Zero;
            if (Input.ButtonJustDownMapped((int)Controls.Select))
            {
                CurrentWeapon = CurrentWeapon == Weapon.Melee ? Weapon.Pistol : Weapon.Melee;
                SwitchToIdleOrMoving(CurrentState);
            }

            switch (CurrentState)
            {
                case State.Idle:
                case State.Run:
                    {
                        if (Input.ButtonDownMapped((int)Controls.Attack1))
                        {
                            SwitchToAttacking();
                            break;
                        }
	                        
                        _movementDirection = GetFacingDirectionFromControls();
                        if (_movementDirection != Vector2.Zero)
                        {
                            _sensorRotation = MathHelper.ToDegrees((float)Math.Atan2(_movementDirection.Y, _movementDirection.X) - MathHelper.PiOver2);
                            _movementDirection.Normalize();
                            DynamicBody.ApplyForce(_movementDirection * MOVEMENT_FORCE);
                        }

                        break;
                    }
                case State.Attack:
                    {
                        if (Input.ButtonDownMapped((int)Controls.Attack1))
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

        private Vector2 GetFacingDirectionFromControls()
        {
            Vector2 direction = Vector2.Zero;
#if WINDOWS
            if (Input.ButtonDownMapped((int)Controls.Up)) direction.Y -= 1.0f;
            if (Input.ButtonDownMapped((int)Controls.Down)) direction.Y += 1.0f;
            if (Input.ButtonDownMapped((int)Controls.Left)) direction.X -= 1.0f;
            if (Input.ButtonDownMapped((int)Controls.Right)) direction.X += 1.0f;
#else
            if (TouchTwinStick.leftStickMagnitude > 0.2f)
            {
                direction = Vector2.Normalize(TouchTwinStick.leftStickDirection);
                direction.Y = -direction.Y;
            }
#endif
            return direction;
        }

        public void SuckIn()
        {
            
        }
        
        public PlayerEntity(ThrongGameScreen gameScreen, Clip clip)
            : this(gameScreen, clip, new Vector2(), new Vector2(1f), 0.0f)
        { }

        public PlayerEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position)
            : this(gameScreen, clip, position, new Vector2(1f), 0.0f)
        { }

        public PlayerEntity(ThrongGameScreen gameScreen, Clip clip, Vector2 position,
                               Vector2 scale)
            : this(gameScreen, clip, position, scale, 0.0f)
        { }

        public PlayerEntity(ThrongGameScreen gameScreen, Clip clip,
                            Vector2 position, Vector2 scale, float rotation)
            : base(gameScreen, clip, position, scale, rotation, PHYSICS_OFFSET, PHYSICS_RADIUS)
        {
            EntityType = Type.Player;
            
            _animDirection = AnimationDirection.Right;

            LoadSounds(gameScreen.Content);

            SetupAnimations(clip);

            SwitchToIdle();
         }

        private void LoadSounds(ContentManager content)
        {
            //_sfxChomp = content.Load<SoundEffect>("Sounds/Chomp 2");
            //_sfxSwallow = content.Load<SoundEffect>("Sounds/Swallow");
            //_sfxFart = content.Load<SoundEffect>("Sounds/Fart 4");
            //_sfxPop = content.Load<SoundEffect>("Sounds/Pop 2");
            //_sfxMove = content.Load<SoundEffect>("Sounds/PlayerMove 1");
            //_sfxSuck = content.Load<SoundEffect>("Sounds/Suck In Loop");

            //_sfxEat[0] = _sfxChomp;
            //_sfxEat[1] = _sfxSwallow;
            //_sfxEat[2] = _sfxFart;
            //_sfxEat[3] = _sfxPop;
        }

        private void SetupAnimations(Clip clip)
        {
            _animations[new AnimKey(State.Idle, Weapon.Pistol, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("idle-left");
            _animations[new AnimKey(State.Idle, Weapon.Pistol, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("idle-right");

            _animations[new AnimKey(State.Run, Weapon.Pistol, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("run-left");
            _animations[new AnimKey(State.Run, Weapon.Pistol, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("run-right");

            _animations[new AnimKey(State.Attack, Weapon.Pistol, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("shoot-left");
            _animations[new AnimKey(State.Attack, Weapon.Pistol, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("shoot-right");

            _animations[new AnimKey(State.Idle, Weapon.Melee, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("idle_melee-left");
            _animations[new AnimKey(State.Idle, Weapon.Melee, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("idle_melee-right");

            _animations[new AnimKey(State.Run, Weapon.Melee, AnimationDirection.Left)] = clip.AnimSet.CreateSingleAnimSet("run_melee-left");
            _animations[new AnimKey(State.Run, Weapon.Melee, AnimationDirection.Right)] = clip.AnimSet.CreateSingleAnimSet("run_melee-right");

            _animations[new AnimKey(State.Attack, Weapon.Melee, AnimationDirection.Left)] = clip.AnimSet.CreateSubSet(true, "swingA-left", "swingB-left");
            _animations[new AnimKey(State.Attack, Weapon.Melee, AnimationDirection.Right)] = clip.AnimSet.CreateSubSet(true, "swingA-right", "swingB-right");
        }

        public Vector2 GetHeading()
        {
            return _movementDirection;
        }
    }
}