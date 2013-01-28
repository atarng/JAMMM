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

        public const float fireCooldown = 0.5F;

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
        /// The number of lives this penguin has.
        /// </summary>
        private int lives;
        public int Lives
        {
            get { return lives; }
            set { lives = value; }
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

        /// <summary>
        /// Penguins take in a player num and a position
        /// and that's how they start on the map.
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="pos"></param>
        public Penguin(PlayerIndex playerIndex, Vector2 pos) 
            // going to need better values for the base
            : base(pos.X, pos.Y, 20, 20, 10, 100)
        {
            this.controller = playerIndex;
            this.startingPosition = pos;
        }

        public override void processInput()
        {
            GamePadState gamePadState = GamePad.GetState(controller);
            if (gamePadState.IsConnected)
            {
                // then it is connected, and we can do stuff here
                acceleration.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                acceleration.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;

                if (CurrState == state.DashReady && gamePadState.IsButtonDown(Buttons.A))
                {
                    CurrState = state.Dash;
                    CurrTime = DashTime;
                }
                
                if (gamePadState.Triggers.Right == 1 && fireTime <= 0)
                {
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
                SpriteManager.getTexture("Penguin_Move_Small"), 4, true);
        }

        public override void update(GameTime delta)
        {
            if (!this.IsAlive)
                return;

            Random rnd = new Random();

            if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble, new Vector2(this.Position.X + rnd.Next(-15,15), this.Position.Y + rnd.Next(-15,15)), new Vector2(0, 0), 3.14f/2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

            currentAnimation.update(delta);
            processInput();
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

            if (fireTime > 0)
            {
                fireTime -= (float)delta.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void draw(GameTime delta, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                batch.Begin();
                currentAnimation.draw(batch, this.Position, 
                    Color.White, SpriteEffects.None, this.Rotation, 1.0f);
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

                batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                batch.DrawString(Game1.font, "[>]", Bounds.center, c);
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

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;
                }*/
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
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
            this.lives--; 
        }

        /// <summary>
        /// Override to handle the current animation.
        /// </summary>
        public override void respawn()
        {
            base.respawn();
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
            {
                die();
            }
            else if (t == AnimationType.Throw || 
                     t == AnimationType.Dash)
            {
                currentAnimation = idleAnimation;
                currentAnimation.play();
            }
        }

        /// <summary>
        /// How the penguin collides with other actors.
        /// </summary>
        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {

            }
            else if (other is Penguin)
            {

            }
            else if (other is Shark)
            {

            }
            else if (other is Fish)
            {

            }
        }
    }
}
