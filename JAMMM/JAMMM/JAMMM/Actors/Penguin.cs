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
            : base(pos.X, pos.Y, 20, 20, 10)
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

                if (gamePadState.Triggers.Right > 0.75)
                {
                    //fire
                }
            }

            /*
            KeyboardState kbState = Keyboard.GetState();
            if( kbState.IsKeyDown(Keys.W))
                acceleration.Y = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.A))
                acceleration.X = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.D))
                acceleration.X = MaxAcc;
            if (kbState.IsKeyDown(Keys.S))
                acceleration.Y = MaxAcc;
            */
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

            ParticleManager.Instance.createParticle(ParticleType.Bubble, this.Position, new Vector2(0, 0), 3.14f/2.0f, 5, 1, 0, 1, 1.0f, 1.0f);


            currentAnimation.update(delta);
            
            processInput();
        }

        public override void draw(GameTime delta, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                batch.Begin();
                currentAnimation.draw(batch, this.Position, 
                    Color.White, SpriteEffects.None, this.Rotation, 1.0f);
                batch.End();
            }
            /*
            Boolean printPhysics = true;
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
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
                batch.End();
            }
             * */
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
