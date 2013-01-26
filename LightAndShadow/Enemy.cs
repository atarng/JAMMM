using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LightAndShadow
{
    class Enemy
    {
        public Vector2 location;

        public float hspeed;
        public float vspeed;
        public float radius;
        public float mass;
        public bool bigGuy;
        public bool chaser;

        public Enemy(float X, float Y, float hspd, float vspd, float mass, bool bigGuy, bool chaser)
        {
            setMass(mass);
            location = new Vector2(X, Y);
            this.hspeed = hspd;
            this.vspeed = vspd;
            this.bigGuy = bigGuy;
            this.chaser = chaser;
        }
        public void setMass(float m)
        {
            mass = m;
            radius = (float)Math.Sqrt(m);
        }
        public void changeMass(float c)
        {
            setMass(mass + c);
        }
    }
}
