using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

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
        /// <summary>
        /// Actors use this enum to determine which animation 
        /// is which handleAnimationComplete.
        /// </summary>
        public enum AnimationType
        {
            Idle,
            Move,
            Dash,
            Throw,
            Turn,
            Death
        }

        private Circle bounds;
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
            set { position = value; }
        }

        private Vector2 offset; ///collision body offset
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

        private float rotation;///radians
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public virtual void processInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
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

        public virtual void update(GameTime delta)
        {
            processInput();
            //Physics.applyMovement(this, delta.ElapsedGameTime.Seconds, false);
        }

        public virtual void draw(GameTime delta, SpriteBatch batch)
        {
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
        }

        public Actor(float x, float y, float offX, float offY, float radius)
        {
            this.MaxAcc = 250;
            this.MaxAccDash = 500;
            this.MaxVel = 500;

            this.Position = new Vector2(x,y);
            this.Offset = new Vector2(x, y);
            this.Bounds = new Circle(x + offX, y + offY, radius);
        }

        public Actor()
        {
            this.Position = new Vector2();
            this.Velocity = new Vector2();
            this.Acceleration = new Vector2();
            rotation = 0;
        }

        public virtual void loadContent() { }

        /// <summary>
        /// Actors override this to determine what happens at
        /// the end of each animation. 
        /// </summary>
        public virtual void handleAnimationComplete(AnimationType t) { }
    }
}
