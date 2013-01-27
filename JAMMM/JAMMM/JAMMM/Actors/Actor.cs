using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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


        public Actor()
        {
            this.Position = new Vector2();
            this.Velocity = new Vector2();
            this.Acceleration = new Vector2();
            rotation = 0;
        }

        /// <summary>
        /// Actors override this to determine what happens at
        /// the end of each animation. 
        /// </summary>
        public virtual void handleAnimationComplete(AnimationType t) { }
    }
}
