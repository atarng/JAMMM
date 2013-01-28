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
            Death,
            Bubble
        }

        public enum state
        {
            Moving,
            Dash,
            Dashing,
            DashCooldown,
            DashReady,
            Dying
        }

        /// <summary>
        /// These are all of the animations for each actor. 
        /// Actors need to instantiate these within their
        /// load content method.
        /// </summary>
        #region Animations
        protected Animation currentAnimation;
        protected Animation dashAnimation;
        protected Animation moveAnimation;
        protected Animation idleAnimation;
        protected Animation deathAnimation;
        protected Animation throwAnimation;
        protected Animation turnAnimation;
        #endregion

        public static bool printPhysics = true;

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

        /// <summary>
        /// collision body offset
        /// </summary>
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

        private float currTime;
        public float CurrTime
        {
            get { return currTime; }
            set { currTime = value; }
        }
        /// <summary>
        /// rotation in radians
        /// </summary>
        private float rotation;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Whether or not this actor is alive.
        /// </summary>
        private bool isAlive;
        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }

        /// <summary>
        /// Setting the scale for this actor.
        /// </summary>
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

        /// <summary>
        /// The size of the penguin.
        /// </summary>
        private Size currentSize;
        public Size CurrentSize
        {
            get { return currentSize; }
            set { currentSize = value; }
        }

        protected Vector2 startingPosition;

        public virtual void die()
        {
            this.isAlive = false;
        }

        public virtual void startDying() {}

        public virtual void spawnAt(Vector2 position)
        {
            this.position = position;
            this.isAlive = true;
        }

        public virtual void respawn()
        {
            this.position = this.startingPosition;
            this.isAlive = true;
        }

        public virtual void processInput(){}

        public virtual void update(GameTime delta)
        {
            processInput();
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
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
                batch.End();
            }
        }

        public virtual void loadContent() { }

        public virtual void collideWith(Actor other) { }

        public Actor(float x, float y, float offX, float offY, float radius, float mass)
        {
            this.MaxAcc = 250;
            this.MaxAccDash = 15000;
            this.MaxVel = 200;
            this.MaxVelDash = 400;
            this.Mass = mass;
            this.dashTime = 1;
            this.dashCooldownTime = 3;
            CurrState = state.DashReady;

            this.Position = new Vector2(x,y);
            this.Offset = new Vector2(offX, offY);
            this.Bounds = new Circle(x + offX, y + offY, radius);
            this.startingPosition = new Vector2(x, y);
            this.isAlive = false;
        }

        /// <summary>
        /// Actors override this to determine what happens at
        /// the end of each animation. 
        /// </summary>
        public virtual void handleAnimationComplete(AnimationType t) { }
    }
}
