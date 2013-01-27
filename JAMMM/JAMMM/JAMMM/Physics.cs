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
        private const float uk = 0.5F; // coefficient of friction increase for more friction
        private const Double eps = 1; //epsilon

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

        public static void applyMovement(Actor a, float delta, Boolean applyFric )
        {
            Vector2 acc = a.Acceleration;
            Vector2 vel = a.Velocity;
            Vector2 pos = a.Position;
            
            //Vector2 accFric = -1 * vel * uk;
            Vector2 accFricNormalize = accFric;
            if( !accFricNormalize.Equals(Vector2.Zero))
                accFricNormalize.Normalize();

            Vector2 initVelNormalize = vel; //initial velocity
            if (!initVelNormalize.Equals(Vector2.Zero))
                initVelNormalize.Normalize();

            //if (applyFric)
            //    acc = acc + accFric;

            a.Velocity = acc * delta + vel;

            /*
            if (applyFric) //set velocity to zero if the friction is making it go backwards
            {
                if (initVelNormalize.Equals(accFricNormalize) && !initVelNormalize.Equals(Vector2.Zero))
                {
                    a.Velocity = Vector2.Zero;
                }
            }
             */


            if (applyFric)
                vel = 0.95F * vel;
            
            a.Position = vel * delta + pos;

            //Rotations
            //initial velocity is vel, final velocity is a.velocity
            Vector2 finalVelNormalize = a.Velocity;
            if (!finalVelNormalize.Equals(Vector2.Zero))
                finalVelNormalize.Normalize();
            
            if (!finalVelNormalize.Equals(Vector2.Zero) && !initVelNormalize.Equals(Vector2.Zero))
            {
                //float angle = (float)Math.Atan2(finalVelNormalize.Y - initVelNormalize.Y, finalVelNormalize.X - initVelNormalize.X);
                float angle = (float) Math.Acos(Vector2.Dot(finalVelNormalize, initVelNormalize));
                a.Rotation += angle;
            }

            //update the bounds
            a.Bounds.CenterX = pos.X + a.Offset.X;
            a.Bounds.CenterY = pos.Y + a.Offset.Y;

            //zero if too small
            if ( Math.Abs(a.Velocity.X) < eps)
                a.velocity.X = 0;
            if ( Math.Abs(a.Velocity.Y) < eps)
                a.velocity.Y = 0;
            if ( Math.Abs(a.Acceleration.X) < eps)
                a.acceleration.X = 0;
            if ( Math.Abs(a.Acceleration.X) < eps)
                a.acceleration.X = 0;
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

        public Circle(float x, float y, float radius)
        {
            this.Radius = radius;
            this.CenterX = x;
            this.CenterY = y;
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
