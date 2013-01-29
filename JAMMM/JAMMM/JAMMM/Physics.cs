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
        private const float cr = 1.0F; //1 is elastic , 0 inelastic
        private const float uk = 0.990F; // 1 is frictionless .985
        //private const float uk = 0.5F; // coefficient of friction increase for more friction
        private const Double eps = 1; //epsilon
        private const float smallForce = 5;
        private const float accDecay = 0.985F; //1 is no decay
        //private static Vector2 smallForce = new Vector2(50,50);

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

        public static void separate(Actor a, Actor b)
        {
            float iDepth = a.Bounds.intersectionDepth(b.Bounds);

            Vector2 aVel = a.velocity;
            float aLen = aVel.Length();

            Vector2 bVel = b.velocity;
            float bLen = bVel.Length();

            // actor A is moving faster than actor B
            if (aLen > bLen)
            {
                Vector2 velNorm = aVel;
                velNorm.Normalize();

                a.Position -= velNorm * iDepth;
            }
            // actor B is moving faster than actor A
            else
            {
                Vector2 velNorm = bVel;
                velNorm.Normalize();

                b.Position -= velNorm * iDepth;
            }
        }

        public static void applyMovement(Actor a, float delta, Boolean applyFric )
        {
            Vector2 acc = a.Acceleration;
            Vector2 vel = a.Velocity;
            Vector2 pos = a.Position;
            
            //this is not used currently
            /*
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
            */

            //Vector2 initVelNormalize = vel; //initial velocity
            //if (!initVelNormalize.Equals(Vector2.Zero))
            //    initVelNormalize.Normalize();
            //if (a.CurrState == Actor.state.Dashing && !acc.Equals(Vector2.Zero))
            //    acc = a.MaxAccDash * Vector2.Normalize(acc);

            a.Velocity = acc * delta + vel;

            //if not dashing and sqrt then cap the magnitude
            if (a.CurrState != Actor.state.Dashing  && Math.Sqrt(magnitudeSquared(a.Velocity)) > a.MaxVel)
            {
                a.Velocity = vel = uk * a.Velocity;
            }
            else if( a.CurrState == Actor.state.Dashing && Math.Sqrt(magnitudeSquared(a.Velocity)) > a.MaxVelDash )
            {
                a.Velocity = a.MaxVelDash * Vector2.Normalize(a.Velocity);
            }

            //relative friction
            if (applyFric)
                a.Velocity = vel = uk * a.Velocity;

            //initial velocity is vel, final velocity is a.velocity
            //Vector2 finalVelNormalize = a.Velocity;
            //if (!finalVelNormalize.Equals(Vector2.Zero))
            //    finalVelNormalize.Normalize();

            /*
            Vector2 temp = a.Velocity + accFricNormalize * uk;
            temp.Normalize();
            if (applyFric && magnitudeSquared(temp) > magnitudeSquared(a.Velocity) )//temp.Equals(finalVelNormalize) )// && !acc.Equals(Vector2.Zero))
                a.Velocity += accFricNormalize * uk;
            
            */
            a.Position = pos = vel * delta + pos;

            //uncomment for boundry checks
            /*
            if (pos.X > 800)
            {
                pos.X = 800;
                vel.X *= -1; 
            }
            else if (pos.X < 0)
            {
                pos.X = 0;
                vel.X *= -1; 
            }
            if (pos.Y > 800)
            {
                pos.Y = 800;
                vel.Y *= -1; 
            }
            else if (pos.Y < 0)
            {
                pos.Y = 0;
                vel.Y *= -1; 
            }
            a.Position = pos;
            a.Velocity = vel;
              */  

            //Rotations
            //if ( !finalVelNormalize.Equals(Vector2.Zero))
            if (!acc.Equals(Vector2.Zero)) // turn direction to mirror controller
            {
                a.Rotation = VectorToAngle(a.Acceleration);
            }
            /*
            if (!acc.Equals(Vector2.Zero) && !finalVelNormalize.Equals(Vector2.Zero))
            {
                //float angle = (float)Math.Atan2(finalVelNormalize.Y - initVelNormalize.Y, finalVelNormalize.X - initVelNormalize.X);
                //float angle = (float) Math.Acos(Vector2.Dot(finalVelNormalize, initVelNormalize));
                //a.Rotation += angle;
                //if ( Math.Abs(a.Rotation - VectorToAngle(finalVelNormalize)) < Math.PI / 4)
                a.Rotation = VectorToAngle(finalVelNormalize);
            }
            */

            //update the bounds
            float s = (float)Math.Sin(a.Rotation);
            float c = (float)Math.Cos(a.Rotation);
            //rotate about the origin then add to position
            a.bounds.center.X = /*c * (a.Offset.X) - s * (a.Offset.Y) +*/ a.Position.X;
            a.bounds.center.Y = /* s * (a.Offset.X) + c * (a.Offset.Y) +*/ a.Position.Y;

            a.acceleration = accDecay * a.acceleration;

            //zero if too small
            if ( Math.Abs(a.Velocity.X) < eps) 
                a.velocity.X = 0;
            if ( Math.Abs(a.Velocity.Y) < eps)
                a.velocity.Y = 0;
            if ( Math.Abs(a.Acceleration.X) < eps)
                a.acceleration.X = 0;
            if ( Math.Abs(a.Acceleration.Y) < eps)
                a.acceleration.Y = 0;
       }

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static float magnitudeSquared(Vector2 v)
        {
            return v.X * v.X + v.Y * v.Y;
        }
    }

    public class Circle
    {
        public Vector2 center;
        public Vector2 Center
        {
            get { return center; }
            set
            {
                center = value;
            }
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
            Center = new Vector2(x, y);
        }

        public float intersectionDepth(Circle other)
        {
            float dx = center.X - other.center.X;
            float dy = center.Y - other.center.Y;

            Vector2 result = new Vector2(dx, dy);

            float distanceBetweenCenters = result.Length();

            float intersectionDepth = this.Radius + other.Radius - distanceBetweenCenters;

            return intersectionDepth;
        }

        public Boolean isCollision( Circle a )
        {
            float dx = center.X - a.center.X;
            float dy = center.Y - a.center.Y;
            float radii = radius + a.radius;
            if (dx * dx + dy * dy < radii * radii)
                return true;
            return false;
        }

    }
}
