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
        private const Vector2 zVec = new Vector2(0, 0);

        //http://en.wikipedia.org/wiki/Inelastic_collision
        public static Vector2[] collide(Actor a, Actor b)
        {
            Vector2 ua = a.getVelocity();
            Vector2 ub = b.getVelocity();
            float ma = a.getMass();
            float mb = b.getMass();

            //(cr * mb * (ub - ua) + ma * ua + mb * ua) / (ma + mb);
            Vector2 va = (cr * mb * (ub - ua) + ma * ua + mb * ua) / (ma + mb);
            Vector2 vb = (cr * ma * (ua - ub) + ma * ua + mb * ua) / (ma + mb);

            a.setVelocity(va);
            b.setVelocity(vb);

            Vector2[] res = { va, vb };
            return res;
        }

        //a = 
        //v = a*t + v
        //p = v*t + p
        public static Vector2 applyMovement(Actor a, float delta, Boolean applyFric )
        {

            Vector2 acc = a.getAcceleration();
            Vector2 vel = a.getVelocity();
            Vector2 pos = a.getCenter();

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
                    vel = zVec;
            }

            pos = vel * delta + pos;
            return pos;
        }
    }

    public class Circle
    {
        private float centerX;
        private float centerY;
        private float radius;

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
