﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class Penguin : Actor
    {
        public const int SHARK_CALORIES                = 100;
        public const int PENGUIN_CALORIES              = 60;
        public const int FISH_CALORIES                 = 15;

        public const int SHARK_DAMAGE                  = 100;

        public const int SPEAR_SMALL_DAMAGE            = 20;
        public const int SPEAR_MED_DAMAGE              = 40;
        public const int SPEAR_MAX_DAMAGE              = 60;

        public const int SMALL_SIZE_CALORIES_THRESHOLD = 1;
        public const int MED_SIZE_CALORIES_THRESHOLD   = 200;
        public const int MAX_SIZE_CALORIES_THRESHOLD   = 300;

        public const int NUMBER_BLINKS_ON_HIT          = 5;
        public const float BLINK_DURATION              = 0.1f;

        public const int SMALL_SIZE = 25;
        public const int MED_SIZE = 45;
        public const int LARGE_SIZE = 60;

        public const float fireCooldown                = 0.5F;
        //Change Dash Constants in Actor Contructor!

        /// <summary>
        /// for game to query if this actor has fired
        /// </summary>
        private Boolean fire;
        public Boolean Fire
        {
            get { return fire; }
            set { fire = value; }
        }

        private float fireTime;
        public float FireTime
        {
            get { return fireTime; }
            set { fireTime = value; }
        }

        /// <summary>
        /// The penguin's health.
        /// </summary>
        private int calories;
        public int Calories
        {
            get { return calories; }
            set { calories = value; }
        }

        /// <summary>
        /// The controller for this player.
        /// </summary>
        private PlayerIndex controller;
        public PlayerIndex Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        private bool prevStateA = false;

        /// <summary>
        /// How long to blink for when we're hit.
        /// </summary>
        private float blinkTime;
        private float numBlinks;
        private bool  isHit;
        private bool  isBlink;

        /// <summary>
        /// Penguins take in a player num and a position
        /// and that's how they start on the map.
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="pos"></param>
        public Penguin(PlayerIndex playerIndex, Vector2 pos) 
            // going to need better values for the base
            : base(pos.X, pos.Y, 36, 32, SMALL_SIZE, 100)
        {
            this.controller       = playerIndex;
            this.startingPosition = pos;
            this.calories         = 100;
            this.CurrentSize      = Size.Small;

            this.blinkTime       = 0.0f;
            this.numBlinks       = 0;
            this.isHit           = false;
            this.isBlink         = false;
        }

        public override void processInput()
        {
            if (this.CurrState == state.Dying)
                return;

            GamePadState gamePadState = GamePad.GetState(controller);
            if (gamePadState.IsConnected)
            {
                // then it is connected, and we can do stuff here
                //acceleration.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                //acceleration.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;
                
                Vector2 accController = Acceleration;
                accController.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                accController.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;

                //if controller acc is greater than old acc, set it to controller acc
                if (Physics.magnitudeSquared(accController) >= Physics.magnitudeSquared(Acceleration))
                    Acceleration = accController;
                else
                {
                    if( accController != Vector2.Zero )
                        Acceleration = Vector2.Normalize(accController) * (float)Math.Sqrt(Physics.magnitudeSquared(Acceleration));
                }

                if ( this.calories > DashCost && gamePadState.IsButtonDown(Buttons.A) && !prevStateA)
                {
                    CurrState = state.Dash;
                    this.calories -= DashCost;
                    prevStateA = true;
                    currentAnimation = dashAnimation;
                    currentAnimation.play();
                    AudioManager.getSound("Actor_Dash").Play();
                }

                if (gamePadState.IsButtonUp(Buttons.A))
                {
                    prevStateA = false;
                }
                
                if (gamePadState.Triggers.Right == 1 && fireTime <= 0)
                {
                    AudioManager.getSound("Spear_Throw").Play();
                    fireTime = fireCooldown;
                    fire = true;
                }
                /*
                                // stop dashing
               else if (CurrState == state.Dashing && gamePadState.IsButtonUp(Buttons.A))
               {
                   CurrTime = DashCooldownTime;
                   CurrState = state.DashCooldown;
               } 
               */
            }

            KeyboardState kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.Space) && fireTime <= 0)
            {
                fireTime = fireCooldown;
                fire = true;
            }

            if (kbState.IsKeyDown(Keys.LeftShift))
            {
                CurrState = state.Dash;
                CurrTime = DashTime;
            }

            if (kbState.IsKeyDown(Keys.W))
                acceleration.Y = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.A))
                acceleration.X = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.D))
                acceleration.X = MaxAcc;
            if (kbState.IsKeyDown(Keys.S))
                acceleration.Y = MaxAcc;
        }

        public override void loadContent()
        {
            // need to create the animations
            moveAnimation = new Animation((Actor)this, AnimationType.Move, 
                SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL), 4, true);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash,
                SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL), 1, true);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL), 1, true);
        }

        public override void update(GameTime delta)
        {
            if (!this.IsAlive)
                return;

            tryToGrow();
            tryToDie();
            tryToBlink(delta);

            if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble, 
                    new Vector2(this.Position.X + rnd.Next(-15,15), this.Position.Y + rnd.Next(-15,15)), 
                    new Vector2(0, 0), 3.14f/2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

            currentAnimation.update(delta);
            processInput();
            if (CurrState == state.Dash)
            {
                if (Acceleration.Equals(Vector2.Zero))
                    Acceleration = Physics.AngleToVector(Rotation);
                else
                    Acceleration.Normalize();

                //velocity = acceleration * MaxVelDash;
                acceleration = acceleration * MaxAccDash;
                CurrTime     = DashTime;
                CurrState    = state.Dashing;

                currentAnimation = dashAnimation;
                currentAnimation.play();
            }
            else if (CurrState == state.Dashing)
            {
                //CurrState = state.DashReady;
                //currentAnimation = moveAnimation;
                //currentAnimation.play();

                CurrTime -= (float)delta.ElapsedGameTime.TotalSeconds;
                if (CurrTime <= 0)
                {
                    CurrState = state.DashReady;
                }
            }
            else if (CurrState == state.DashReady)
            {
                currentAnimation = moveAnimation;
                currentAnimation.play();
            }

            /*
            switch (CurrState) //deal with dash
            {
                case state.Dash:
                    if (Acceleration.Equals(Vector2.Zero))
                        Acceleration = Physics.AngleToVector(Rotation);
                    else
                        acceleration.Normalize();

                    acceleration = acceleration * MaxAccDash;
                    CurrTime = DashTime;
                    CurrState = state.Dashing;
                    break;
                case state.Dashing:
                    CurrTime -= (float)delta.ElapsedGameTime.TotalSeconds;
                    if (CurrTime <= 0)
                    {
                        CurrTime = DashCooldownTime;
                        CurrState = state.DashCooldown;
                    }
                    break;
                case state.DashCooldown:
                    CurrTime -= (float)delta.ElapsedGameTime.TotalSeconds;
                    if (CurrTime <= 0)
                    {
                        CurrState = state.DashReady;
                        
                    }
                    break;
                default:
                    break;
            }
            */

            if (fireTime > 0)
            {
                fireTime -= (float)delta.ElapsedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Switch between animations dependent on our size if we can.
        /// </summary>
        private void tryToGrow()
        {
            // switch to big
            if (calories >= MAX_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Large)
            {
                this.CurrentSize = Size.Large;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_LARGE), 8);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_LARGE), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_LARGE), 1);
                currentAnimation.play();

                this.Bounds.Radius = LARGE_SIZE;

                return;

            }
            // switch to medium
            else if (calories >= MED_SIZE_CALORIES_THRESHOLD && calories < MAX_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Medium)
            {
                this.CurrentSize = Size.Medium;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_MEDIUM), 4);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_MEDIUM), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_MEDIUM), 1);
                currentAnimation.play();

                this.Bounds.Radius = MED_SIZE;

                return;
            }
            // switch to small
            else if (calories >= SMALL_SIZE_CALORIES_THRESHOLD && calories < MED_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Small)
            {
                this.CurrentSize = Size.Small;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL), 4);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL), 1);
                currentAnimation.play();

                this.Bounds.Radius = SMALL_SIZE;

                return;
            }
        }

        /// <summary>
        /// If we're out of calories, go into the dying state
        /// and change our animation to the death animation.
        /// </summary>
        private void tryToDie()
        {
            if (this.calories <= 0)
            {
                currentAnimation = deathAnimation;
                currentAnimation.play();
                this.CurrState = state.Dying;
            }
        }

        /// <summary>
        /// Try to blink if we're being hit.
        /// </summary>
        private void tryToBlink(GameTime gameTime)
        {
            if (this.isHit)
            {
                this.blinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.blinkTime > BLINK_DURATION)
                {
                    this.blinkTime -= BLINK_DURATION;

                    // switch to blinking off
                    if (isBlink)
                    {
                        this.isBlink = false;
                    }
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
                            this.isBlink = false;
                            this.blinkTime = 0.0f;
                            this.numBlinks = 0;
                        }
                    }
                }
            }
        }

        public override void draw(GameTime delta, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                batch.Begin();
                Color c;

                if (this.isBlink)
                {
                    c = Color.Red;
                }
                else
                {
                    c = Color.White;
                }

                if (Math.Abs(Rotation) > Math.PI / 2)
                {
                    currentAnimation.draw(batch, this.Position,
                        c, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
                }
                else
                {
                    currentAnimation.draw(batch, this.Position,
                        c, SpriteEffects.None, this.Rotation, 1.0f);
                }

                if (printPhysics)
                    printPhys(batch);
                batch.End();
            }
        }

        public void printPhys(SpriteBatch batch)
        {
                Color c = Color.Black;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                //batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                batch.DrawString(Game1.font, "[>]", Bounds.center, Color.Red);
                //batch.DrawString(Game1.font, "Cal " + Calories, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                String s = "";
                switch (CurrState)
                {
                    case state.Dash:
                        s = "dash";
                        break;
                    case state.Dashing:
                        s = "dashing";
                        break;
                    case state.DashCooldown:
                        s = "dashcooldown";
                        break;
                    case state.DashReady:
                        s = "dashready";
                        break;
                }
                batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "DashCost " + DashCost, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;
                }*/
               // batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Position " + Position, loc, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
        }

        /// <summary>
        /// Override to handle lives
        /// </summary>
        public override void die()
        {
            base.die();
        }

        /// <summary>
        /// Override to handle the current animation.
        /// </summary>
        public override void respawn()
        {
            base.respawn();
            this.isHit = false;
            this.isBlink = false;
            this.numBlinks = 0;
            this.Calories = 100;
            this.CurrState = state.Moving;
            this.currentAnimation = moveAnimation;
            currentAnimation.play();
        }

        /// <summary>
        /// Actors override this to determine what happens at
        /// the end of each animation. 
        /// </summary>
        public override void handleAnimationComplete(AnimationType t) 
        {

        }

        /// <summary>
        /// How the penguin collides with other actors.
        /// </summary>
        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {
                // take damage based on the spear's owner's size
                if (other.CurrentSize == Size.Large)
                {
                    AudioManager.getSound("Actor_Hit").Play();
                    this.calories -= SPEAR_MAX_DAMAGE;
                }
                else if (other.CurrentSize == Size.Medium)
                {
                    this.calories -= SPEAR_MED_DAMAGE;
                }
                else if (other.CurrentSize == Size.Small)
                {
                    this.calories -= SPEAR_SMALL_DAMAGE;
                }

                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);

                AudioManager.getSound("Actor_Hit").Play();

                this.isHit = true;
                this.blinkTime = 0.0f; 
                this.numBlinks = 0;
            }
            else if (other is Shark)
            {
                // gain health
                if (other.CurrState == state.Dying)
                {
                    this.calories += SHARK_CALORIES;
                    other.die();
                }
                // take damage
                else
                {
                    this.calories -= SHARK_DAMAGE;
                    this.isHit = true;
                }
            }
            else if (other is Fish)
            {
                if (other.CurrState == state.Moving)
                {
                    AudioManager.getSound("Fish_Eat").Play();
                    this.calories += FISH_CALORIES;
                    other.startDying();
                }
            }
        }
    }
}
