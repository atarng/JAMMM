using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAMMM.Actors;
using Microsoft.Xna.Framework;

namespace JAMMM
{
    /// <summary>
    /// Physics should take actors and enact physics on them. 
    /// This means moving and colliding.
    /// </summary>
    public class Physics
    {
        private const float cr = 1; //1 is elastic , 0 inelastic
        private const float uk = 0.3F;

        //http://en.wikipedia.org/wiki/Inelastic_collision
        public static Vector2[] collide(Actor a, Actor b)
        {
            Vector2 ua = a.Velocity;
            Vector2 ub = b.Velocity;
            float ma = a.Mass;
            float mb = b.Mass;

            //(cr * mb * (ub - ua) + ma * ua + mb * ua) / (ma + mb);
            Vector2 va = (cr * mb * (ub - ua) + ma * ua + mb * ua) / (ma + mb);
            Vector2 vb = (cr * ma * (ua - ub) + ma * ua + mb * ua) / (ma + mb);

            a.Velocity = va;
            b.Velocity = vb;

            Vector2[] res = { va, vb };
            return res;
        }

        //a = 
        //v = a*t + v
        //p = v*t + p
        public static Vector2 applyMovement(Actor a, float delta, Boolean applyFric )
        {

            Vector2 acc = a.Acceleration;
            Vector2 vel = a.Velocity;
            Vector2 pos = a.Position;

            Vector2 accFric = -1 * vel * uk;
            Vector2 accFricNormalize = new Vector2(accFric.X, accFric.Y);
            accFricNormalize.Normalize();

            if (applyFric)
                acc = acc + accFric;

            vel = acc * delta + vel;

            if (applyFric) //set velocity to zero if the friction is making it go backwards
            {
                Vector2 velNormalize = new Vector2(vel.X, vel.Y);
                velNormalize.Normalize();
                if (velNormalize.Equals(accFricNormalize))
                {
                    vel.X = 0; vel.Y = 0;
                }
            }

            pos = vel * delta + pos;

            //update the bounds
            a.Bounds.CenterX = pos.X + a.Offset.X;
            a.Bounds.CenterY = pos.Y + a.Offset.Y;
            return pos;
        }
    }

    public class Circle
    {
        private float centerX;
        public float CenterX
        {
            get { return centerX; }
            set { centerX = value; }
        }

        private float centerY;
        public float CenterY
        {
            get { return centerY; }
            set { centerY = value; }
        }

        private float radius;
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public void move(float x, float y)
        {
            centerX = x;
            centerY = y;
        }

        public Boolean isCollision( Circle a )
        {
            float dx = centerX - a.centerX;
            float dy = centerY = a.centerY;
            float radii = radius + a.radius;
            if (dx * dx + dy * dy < radii * radii)
                return true;
            return false;
        }

    }
}
