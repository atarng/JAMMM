using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightAndShadow
{
    class Bomb
    {
        public Vector2 location;
        public double angle;
        public Player owner;

        public float speed;
        public float size;

        public float eLife;
        public float oLife;
        public bool detonate;

        public Bomb(Vector2 location, double angle, float speed, Player owner, float size, float eLife)
        {
            this.angle = angle;
            this.location = location;
            this.speed = speed;
            this.owner = owner;
            this.size = size;
            this.oLife = this.eLife = eLife;
        }
    }
}
