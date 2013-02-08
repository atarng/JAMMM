using System;
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
        public const int START_CALORIES = 100;
        public const int SMALL_SIZE_CALORIES_THRESHOLD = 1;
        public const int MED_SIZE_CALORIES_THRESHOLD   = 150;
        public const int MAX_SIZE_CALORIES_THRESHOLD   = 230;

        public const int NUMBER_BLINKS_ON_HIT          = 5;
        public const float BLINK_DURATION              = 0.1f;

        public const int SMALL_SIZE = 35;
        public const int MED_SIZE = 45;
        public const int LARGE_SIZE = 60;

        public const int SPEAR_SMALL_COST = 7;
        public const int SPEAR_MED_COST = 10;
        public const int SPEAR_LARGE_COST = 13;


        public const int DASH_SMALL_COST = 4;
        public const int DASH_MED_COST = 5;
        public const int DASH_LARGE_COST = 7;

        public const int SMALL_MASS = 100;
        public const int MEDIUM_MASS = 500;
        public const int LARGE_MASS = 1500;

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

        private string colorCode;

        /// <summary>
        /// Penguins take in a player num and a position
        /// and that's how they start on the map.
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="pos"></param>
        public Penguin(PlayerIndex playerIndex, Vector2 pos, string colorCode) 
            // going to need better values for the base
            : base(pos.X, pos.Y, 36, 32, SMALL_SIZE, SMALL_MASS)
        {
            this.colorCode        = colorCode;
            this.controller       = playerIndex;
            this.startingPosition = pos;
            this.Calories         = START_CALORIES;
            this.CurrentSize      = Size.Small;

            this.DashCost         = DASH_SMALL_COST;
            this.SpearCost        =  SPEAR_SMALL_COST;
 
            this.blinkTime        = 0.0f;
            this.numBlinks        = 0;
            this.isHit            = false;
            this.isBlink          = false;
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

                //if the acceleration is > max acc (means we were dashing)
                if (!(CurrState == state.Dashing &&
                    (Vector2.Normalize(accController) == Vector2.Normalize(Acceleration) || Vector2.Zero == accController)))
                {
                    Acceleration = accController;
                }

                /*
                //if controller acc is greater than old acc, set it to controller acc
                if (Physics.magnitudeSquared(accController) >= Physics.magnitudeSquared(Acceleration))
                    Acceleration = accController;
                else
                {
                    if( accController != Vector2.Zero )
                        Acceleration = Vector2.Normalize(accController) * (float)Math.Sqrt(Physics.magnitudeSquared(Acceleration));
                }
                */

                if ( this.calories > DashCost && gamePadState.IsButtonDown(Buttons.A) && !prevStateA)
                {
                    CurrState = state.Dash;
                    this.calories -= DashCost;
                    prevStateA = true;
                    currentAnimation = dashAnimation;
                    currentAnimation.play();
                    AudioManager.getSound("Actor_Dash").Play();
                }

                if (gamePadState.IsButtonUp(Buttons.A) && CurrState == state.DashReady)
                {
                    prevStateA = false;
                }
                
                if (gamePadState.Triggers.Right == 1 && FireTime <= 0)
                {
                    if (this.Calories > this.SpearCost)
                    {
                        AudioManager.getSound("Spear_Throw").Play();
                        FireTime = fireCooldown;
                        fire = true;
                        this.Calories -= this.SpearCost;
                    }
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
                SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL + colorCode), 4, true);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash,
                SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL + colorCode), 1, false);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL + colorCode), 1, false, 1.5f);
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
                //if (Acceleration.Equals(Vector2.Zero))
                //    Acceleration = Physics.AngleToVector(Rotation);
                //else
                //    Acceleration.Normalize();

                //velocity = acceleration * MaxVelDash;
                acceleration += acceleration * MaxAccDash;
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
                    currentAnimation = moveAnimation;
                    currentAnimation.play();
                }
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

                this.DashCost = DASH_LARGE_COST;
                this.SpearCost = SPEAR_LARGE_COST;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_LARGE + colorCode), 8);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_LARGE + colorCode), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_LARGE + colorCode), 1);
                currentAnimation.play();

                this.Bounds.Radius = LARGE_SIZE;

                this.Mass = LARGE_MASS;

                return;

            }
            // switch to medium
            else if (calories >= MED_SIZE_CALORIES_THRESHOLD && calories < MAX_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Medium)
            {
                this.CurrentSize = Size.Medium;

                this.DashCost = DASH_MED_COST;
                this.SpearCost = SPEAR_MED_COST;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_MEDIUM + colorCode), 4);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_MEDIUM + colorCode), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_MEDIUM + colorCode), 1);
                currentAnimation.play();

                this.Bounds.Radius = MED_SIZE;

                this.Mass = MEDIUM_MASS;

                return;
            }
            // switch to small
            else if (calories >= SMALL_SIZE_CALORIES_THRESHOLD && calories < MED_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Small)
            {
                this.CurrentSize = Size.Small;

                this.DashCost = DASH_SMALL_COST;
                this.SpearCost = SPEAR_SMALL_COST;

                // switch animations
                currentAnimation.stop();
                moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL + colorCode), 4);
                dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL + colorCode), 1);
                deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL + colorCode), 1);
                currentAnimation.play();

                this.Bounds.Radius = SMALL_SIZE;

                this.Mass = SMALL_MASS;

                return;
            }
        }

        /// <summary>
        /// If we're out of calories, go into the dying state
        /// and change our animation to the death animation.
        /// </summary>
        private void tryToDie()
        {
            if (this.calories <= 0 && this.CurrState != state.Dying)
            {

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
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble() , 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);


                AudioManager.getSound("Death_Penguin").Play();

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
                Color c;

                if (this.isBlink)
                    c = Color.Pink;
                else
                    c = Color.White;

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

                c = Color.White;
                batch.DrawString(Game1.font, "Calories: " + this.Calories, new Vector2(this.Position.X - 75, this.Position.Y + 60), c, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                if (printPhysics)
                    printPhys(batch);
            }
        }

        public void printPhys(SpriteBatch batch)
        {
                Color c = Color.White;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                //batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "[>]", Bounds.center, Color.Red);
                //batch.DrawString(Game1.font, "Cal " + Calories, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
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
               // batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "DashCost " + DashCost, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;f
                }*/
               // batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);

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
            this.Calories = START_CALORIES;
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
            if (t == AnimationType.Death)
                this.die();
        }

        /// <summary>
        /// How the penguin collides with other actors.
        /// </summary>
        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {
                //Set delay so we can't immediately fire a spear after being hit
                this.FireTime = fireCooldown * 1.5f;
                this.Fire = false;
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
                else if (other.CurrState == state.Dashing)
                {
                    if (CurrState != state.Dying)
                    {
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

                        this.calories -= SHARK_DAMAGE;

                        if (this.calories <= 0)
                            ((Shark)other).Calories += 100;

                        this.isHit = true;
                    }
                }
            }
            else if (other is Fish)
            {
                if (other.CurrState == state.Moving)
                {
                    AudioManager.getSound("Fish_Eat").Play();
                    this.calories += FISH_CALORIES;
                    other.startDying();

                    /*
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
                    */
                }
            }
        }
    }
}
