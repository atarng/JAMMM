using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JAMMM;

namespace JAMMM
{
    abstract class Particle
    {
        protected Vector2 location;
        protected Vector2 offset;
        protected Vector2 direction;  //unit vector
        protected float angle;        //angle of movement
        protected float life;         //current life
        protected float size;         //current scale
        protected float speed;
        protected float growth;       //growth factor
        protected float acceleration;
        protected float alpha;
        protected float dim;          //dimming alpha factor
        protected Color color;

        protected Animation animation;

        /// <summary>
        /// These are all of the animations for each actor. 
        /// Actors need to instantiate these within their
        /// load content method.
        /// </summary>
       
        public Particle()
        {
            life = 0;
        }

        public void set(Vector2 location, Vector2 offsets, float direction, float speed, float size, 
            float growth, float alphablend, float dim, float life, byte r = 255, byte g = 255, byte b = 255)
        {
            this.location = location;
            this.offset = offsets;
            this.life = life;
            this.size = size;
            this.growth = growth;
            this.alpha = alphablend;
            this.dim = dim;

            this.color.R = r;
            this.color.G = g;
            this.color.B = b;
            this.color.A = (byte)(255 * alphablend);

            this.angle = direction;
            this.speed = speed;
            this.direction = Vector2.Transform(new Vector2(1, 0), Matrix.CreateRotationZ(-direction));
        }

        public abstract void loadContent();
        public abstract void draw(GameTime gameTime, SpriteBatch batch);
        public abstract bool update(GameTime gameTime);

        protected bool checkDeath()
        {
            if (this.life < 0 || this.size < 0 || this.alpha < 0)
            {
                life = 0;
                return false;
            }
            return true;
        }

        public void die()
        {
            this.life = 0;
        }

        public bool isAlive()
        {
            if (life == 0)
                return false;
            return true;
        }

    }
}
