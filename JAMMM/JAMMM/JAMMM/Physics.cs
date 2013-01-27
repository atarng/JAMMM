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
        public const float cr = 1; //1 is elastic , 0 inelastic
        private const float uk = 0.985F; // 1 is frictionless
        //= 0.5F; // coefficient of friction increase for more friction
        private const Double eps = 1; //epsilon
        private static Vector2 fricForce = new Vector2(100,100);

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
            
            //this is not used currently
            Vector2 accFric = -1 * vel;
            Vector2 accFricNormalize = accFric;
            if (!accFricNormalize.Equals(Vector2.Zero))
                accFricNormalize.Normalize();
            else
            {
                accFricNormalize.X = (float) Math.Cos(a.Rotation);
                accFricNormalize.Y = (float)Math.Sin(a.Rotation);
                accFricNormalize.Normalize();
                accFricNormalize = -1 * accFricNormalize;
            }

            Vector2 initVelNormalize = vel; //initial velocity
            if (!initVelNormalize.Equals(Vector2.Zero))
                initVelNormalize.Normalize();

            //if (applyFric)
            //{
            //    acc = acc + -1 * vel * 0.5F;
            //}

            a.Velocity = acc * delta + vel;

            if (applyFric)
                a.Velocity = 0.985F * a.Velocity;
            
            a.Position = vel * delta + pos;

            //initial velocity is vel, final velocity is a.velocity
            Vector2 finalVelNormalize = a.Velocity;
            if (!finalVelNormalize.Equals(Vector2.Zero))
                finalVelNormalize.Normalize();

            //Rotations
            if ( !acc.Equals(Vector2.Zero) && !finalVelNormalize.Equals(Vector2.Zero))
            {
                //float angle = (float)Math.Atan2(finalVelNormalize.Y - initVelNormalize.Y, finalVelNormalize.X - initVelNormalize.X);
                //float angle = (float) Math.Acos(Vector2.Dot(finalVelNormalize, initVelNormalize));
                //a.Rotation += angle;
                a.Rotation = VectorToAngle(finalVelNormalize);
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

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

    }

    public class Circle
    {
        private Vector2 center;
        public Vector2 Center
        {
            get { return center; }
            set
            {
                center = value;
                CenterX = value.X;
                CenterY = value.Y;
            }
        }

        private float centerX;
        public float CenterX
        {
            get { return centerX; }
            set { centerX = value; center.X = value; }
        }

        private float centerY;
        public float CenterY
        {
            get { return centerY; }
            set { centerY = value; center.Y = value; }
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
