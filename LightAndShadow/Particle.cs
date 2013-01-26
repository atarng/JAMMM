using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LightAndShadow
{
    class Particle
    {
        public Vector2 location;
        public float direction;
        public float life;
        public float size;
        public float speed;

        public Particle(Vector2 location, float size, float direction, int speed, int life)
        {
            this.location = location;
            this.direction = direction;
            this.life = life;
            this.size = size;
            this.speed = speed;
        }
    }
}
