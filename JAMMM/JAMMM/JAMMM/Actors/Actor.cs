﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Powerups;

namespace JAMMM
{
    /// <summary>
    /// Actors are the basic abstraction to reprsent all objects in
    /// our game. They will use composition to own a sprite and
    /// some physical state associated with player input and 
    /// the physics logic for our game. Each actor will differentiate
    /// its own logic for how it interacts with other actors in the
    /// game as well as how it responds to input.
    /// </summary>
    public class Actor
    {
        public const float BASE_SPEED = 250.0f;
        public const float BASE_ACCEL = 200.0f;

        public const int SPEAR_SMALL_DAMAGE = 20;
        public const int SPEAR_MED_DAMAGE = 30;
        public const int SPEAR_MAX_DAMAGE = 40;

        public const int SHARK_CALORIES = 100;
        public const int PENGUIN_CALORIES = 60;
        public const int FISH_CALORIES = 10;

        public const int SHARK_DAMAGE = 50;

        public const float BLINK_DURATION = 0.1f;
        public const int NUMBER_BLINKS_ON_HIT = 5;

        protected float knockbackTime = 0.0f;
        public const float KNOCKBACK_DURATION = 0.5f;

        public const float MAX_POSSIBLE_SPEED = 800.0f;

        public const int   MAX_HEALTH = 300;

        public const float MELEE_DISPLACEMENT = 50.0f;
        public const float SPEAR_DISPLACEMENT = 40.0f;

        public const float FIRE_COOLDOWN = 0.33F;
        public const float MELEE_COOLDOWN = 0.5F;

        public enum AnimationType
        {
            Idle,
            Move,
            Dash,
            Throw,
            Turn,
            Death,
            Bubble,
            HitSpark,
            Melee
        }

        public enum state
        {
            Moving,
            Turning,
            Dash,
            Dashing,
            DashCooldown,
            DashReady,
            Dying,
            MeleeAttack
        }

        public enum powerupstate
        {
            SpeedBoost,
            RapidFire,
            SharkRepellent,
            SpearDeflection,
            Chum,
            Multishot,
            None
        }

        protected powerupstate powerupState;
        public powerupstate PowerupState
        {
            get { return powerupState; }
        }

        protected float powerupTimer;

        #region Animations
        protected Animation currentAnimation;
        protected Animation dashAnimation;
        protected Animation moveAnimation;
        protected Animation idleAnimation;
        protected Animation deathAnimation;
        protected Animation throwAnimation;
        protected Animation turnAnimation;
        protected Animation meleeAnimation;
        #endregion

        public static bool printPhysics = false;
        static protected Random rnd = new Random();
        private bool removeMe;
        public bool RemoveMe
        {
            get { return removeMe; }
            set { removeMe = value; }
        }

        private state currState;
        public state CurrState
        {
            get { return currState; }
            set { currState = value; }
        }

        public Circle bounds;
        public Circle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public Vector2 acceleration;
        public Vector2 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public Vector2 miscAcceleration;
        public Vector2 MiscAcceleration
        {
            get { return miscAcceleration; }
            set { miscAcceleration = value; }
        }

        public Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set {   position = value;
                    this.Bounds = new Circle(this.position.X + Offset.X, this.position.Y + Offset.Y, this.Bounds.Radius);
            }
        }

        public Vector2 changeInPosition;

        private Vector2 offset;
        public Vector2 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private float mass;
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        private float maxAcc;
        public float MaxAcc
        {
            get { return maxAcc; }
            set { maxAcc = value; }
        }

        private float maxAccDash;
        public float MaxAccDash
        {
            get { return maxAccDash; }
            set { maxAccDash = value; }
        }

        private float maxVel;
        public float MaxVel
        {
            get { return maxVel; }
            set { maxVel = value; }
        }

        private float maxVelDash;
        public float MaxVelDash
        {
            get { return maxVelDash; }
            set { maxVelDash = value; }
        }

        private float dashTime;
        public float DashTime
        {
            get { return dashTime; }
            set { dashTime = value; }
        }

        private float dashCooldownTime;
        public float DashCooldownTime
        {
            get { return dashCooldownTime; }
            set { dashCooldownTime = value; }
        }

        private int dashCost;
        public int DashCost
        {
            get { return dashCost; }
            set { dashCost = value; }
        }

        private int meleeCost;
        public int MeleeCost
        {
            get { return meleeCost; }
            set { meleeCost = value; }
        }

        private int spearCost;
        public int SpearCost
        {
            get { return spearCost; }
            set { spearCost = value; }
        }

        private float currTime;
        public float CurrTime
        {
            get { return currTime; }
            set { currTime = value; }
        }
        
        private float rotation;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        private bool isAlive;
        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }

        private float scale;
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public enum Size
        {
            Small = 0,
            Medium = 1,
            Large = 2
        }

        protected float blinkTime;
        protected float numBlinks;
        protected bool isHit;
        public bool IsHit
        {
            get { return isHit; }
        }
        protected bool isBlink;
        public bool IsBlink
        {
            get { return isBlink; }
        }

        protected Powerup powerup;
        public Powerup Powerup
        {
            get { return powerup; }
            set { powerup = value; }
        }

        protected bool isBeingKnockedBack;
        public bool IsBeingKnockedBack
        {
            get { return isBeingKnockedBack; }
        }

        private Size currentSize;
        public Size CurrentSize
        {
            get { return currentSize; }
            set { currentSize = value; }
        }

        protected Vector2 startingPosition;

        public virtual void move(float x, float y)
        {
            this.changeInPosition.X = x;
            this.changeInPosition.Y = y;

            this.position.X += x;
            this.position.Y += y;
        }

        public virtual void die()
        {
            this.isAlive = false;
        }

        public virtual void startDying() {}

        public void setNewStartingPosition(Vector2 pos)
        {
            this.startingPosition = pos;
            this.position = this.startingPosition;
        }

        public Vector2 getStartingPosition()
        {
            return this.startingPosition;
        }

        public void resetPhysics()
        {
            this.acceleration = Vector2.Zero;
            this.miscAcceleration = Vector2.Zero;
            this.velocity = Vector2.Zero;
            this.rotation = 0.0f;
        }

        public void pauseAnimation()
        {
            currentAnimation.pause();
        }

        public void resumeAnimation()
        {
            currentAnimation.play();
        }

        protected virtual void changeState(state newState) 
        {
            switch (newState)
            {
                case state.Dash:
                {
                    onDash();
                    break;
                }
                case state.DashCooldown:
                {
                    onDashCooldown();
                    break;
                }
                case state.Dashing:
                {
                    onDashing();
                    break;
                }
                case state.DashReady:
                {
                    onDashReady();
                    break;
                }
                case state.Dying:
                {
                    onDying();
                    break;
                }
                case state.Moving:
                {
                    onMoving();
                    break;
                }
                case state.Turning:
                {
                    onTurning();
                    break;
                }
                case state.MeleeAttack:
                {
                    onMeleeAttack();
                    break;
                }
            }

            this.currState = newState;
        }

        protected virtual void onDash()           { }
        protected virtual void onDashCooldown()   { }
        protected virtual void onDashing()        { }
        protected virtual void onDashReady()      { }
        protected virtual void onDying()          { }
        protected virtual void onMoving()         { }
        protected virtual void onTurning()        { }
        protected virtual void onMeleeAttack()    { }

        public virtual void onPowerupApplication(Powerup p) { }
        public virtual void onPowerupRemoval(Powerup p) { }

        /// <summary>
        /// Returns a float representing the distance between
        /// this actor and the other actor.
        /// </summary>
        public static float DistanceBetween(Actor a, Actor b)
        {
            return Vector2.Distance(a.Position, b.Position);
        }

        /// <summary>
        /// Returns a normalized vector pointing to the other actor
        /// from ourselves.
        /// </summary>
        public Vector2 DirectionTo(Actor other)
        {
            return Vector2.Normalize(other.position - this.Position);
        }

        protected void changeAnimation(Animation newAnimation)
        {
            if (currentAnimation != null)
                currentAnimation.stop();

            newAnimation.stop();

            currentAnimation = newAnimation;

            currentAnimation.play();
        }

        /// <summary>
        /// This handles application of a powerup which triggers changePowerupState
        /// and resetPowerupState as well as resetting the timer
        /// </summary>
        public virtual void applyPowerup(Powerup p) {}

        /// <summary>
        /// Resets all of the state associated with the previous powerup.
        /// </summary>
        protected virtual void resetPowerupState() {}

        protected virtual void resetBlink()
        {
            this.isBlink = false;
            this.numBlinks = 0;
            this.blinkTime = 0.0f;
        }

        protected virtual void getHit()
        {
            AudioManager.getSound("Actor_Hit").Play();

            resetBlink();

            this.isHit = true;
        }

        protected virtual void tryToBlink(GameTime gameTime)
        {
            if (this.isHit)
            {
                this.blinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.blinkTime >= BLINK_DURATION)
                {
                    this.blinkTime -= BLINK_DURATION;

                    // switch to blinking off
                    if (isBlink)
                        this.isBlink = false;
                    // switch to blinking on 
                    else
                    {
                        this.isBlink = true;
                        this.numBlinks++;

                        // end the blink animation if we surpassed the maximum
                        // number of blinks
                        if (this.numBlinks > NUMBER_BLINKS_ON_HIT)
                        {
                            this.isHit = false;
                            resetBlink();
                        }
                    }
                }
            }
        }

        public virtual void spawnAt(Vector2 position)
        {
            this.Position = position;
            this.isAlive = true;
        }

        public virtual void respawn()
        {
            this.position = this.startingPosition;
            this.isAlive = true;
        }

        public virtual void processInput(){}

        public void boundaryCheck()
        {
            
            if (this.Position.X > Game1.WINDOW_WIDTH)
            {
                this.Position = new Vector2(Game1.WINDOW_WIDTH, this.Position.Y);
            }
            if (this.Position.X < 0)
            {
                this.Position = new Vector2(0, this.Position.Y);
            }
            if (this.Position.Y > Game1.WINDOW_HEIGHT)
            {
                this.Position = new Vector2(this.Position.X, Game1.WINDOW_HEIGHT);
            }
            if (this.Position.Y < 0)
            {
                this.Position = new Vector2(this.Position.X, 0);
            }
        }

        public virtual Rectangle getBufferedRectangleBounds(int buffer)
        {
            if (currentAnimation != null)
            {
                Rectangle result = currentAnimation.getCurrentFrame();

                result.Width  = result.Right - result.Left + buffer;
                result.Height = result.Bottom - result.Top + buffer;
                result.X      = (int)position.X - buffer;
                result.Y      = (int)position.Y - buffer;

                return result;
            }

            return Rectangle.Empty;
        }

        public virtual Rectangle getScaledRectangleBounds(float horScale, float verScale)
        {
            if (currentAnimation != null)
            {
                Rectangle result = currentAnimation.getCurrentFrame();

                result.Width = result.Right - result.Left + 2;
                result.Height = result.Bottom - result.Top + 2;

                result.X = (int)position.X;
                result.Y = (int)position.Y;

                result.Inflate((int)(result.Width * horScale),
                               (int)(result.Height * verScale));

                return result;
            }

            return Rectangle.Empty;
        }

        public virtual void update(GameTime delta)
        {
            processInput();
        }

        protected void updatePowerupState(GameTime delta)
        {
            if (powerupTimer > 0.0f)
            {
                powerupTimer -= (float)delta.ElapsedGameTime.TotalSeconds;

                if (powerupTimer <= 0.0f)
                    resetPowerupState();
            }
        }

        public virtual void draw(GameTime delta, SpriteBatch batch)
        {
            if (printPhysics)
            {
                batch.Begin();
                Color c = Color.Black;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                batch.DrawString(Game1.font, "Position " + Position, loc, c);
                batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);


                batch.DrawString(Game1.font, "Position " + Position, Bounds.Center, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
                batch.End();
            }
        }

        public virtual void loadContent() { }

        public virtual void collideWith(Actor other) { }

        public Actor(float x, float y, float offX, float offY, float radius, float mass)
        {
            this.MaxAcc = BASE_ACCEL;
            this.MaxAccDash = 200;
            this.MaxVel = BASE_SPEED;
            this.MaxVelDash = 300;
            this.dashTime = 0.25f;
            this.dashCost = 1;
            this.spearCost = 1;

            this.Mass = mass;
            this.dashCooldownTime = 3; //this doesnt do anything
            CurrState = state.DashReady;

            this.Bounds = new Circle(x + offX, y + offY, radius);
            this.Position = new Vector2(x,y);
            this.Offset = new Vector2(offX, offY);

            this.startingPosition = new Vector2(x, y);
            this.isAlive = false;
        }

        public virtual void handleAnimationComplete(AnimationType t) { }
    }
}
