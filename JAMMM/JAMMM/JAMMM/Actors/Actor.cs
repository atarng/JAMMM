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
        private Circle bounds;
        private Vector2 acceleration;
        private Vector2 velocity;
        private Vector2 center;
        private float mass;

        private float accMax;
        private float accDashMax;
        private float velMax;

        public Actor()
        {
        }

        public void setCenter(Vector2 cen)
        {
            center = cen;
        }

        public Vector2 getCenter()
        {
            return center;
        }

        public void setVelocity(Vector2 vel)
        {
            velocity = vel;
        }

        public Vector2 getVelocity()
        {
            return velocity;
        }

        public void setAcceleration(Vector2 acc)
        {
            acceleration = acc;
        }

        public Vector2 getAcceleration()
        {
            return acceleration;
        }

        public void setMass(float mass)
        {
            this.mass = mass;
        }

        public float getMass()
        {
            return mass;
        }
    }
}
