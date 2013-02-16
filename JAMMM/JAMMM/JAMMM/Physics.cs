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


        /// Calculate the time of closest approach of two moving circles.  Also determine if the circles collide.
        /// 
        /// Input:
        /// Pa - Position of circle A.
        /// Pb - Position of circle B.
        /// Va - Velocity of circle A.
        /// Vb - Velocity of circle B.
        /// Ra - Radius of circle A.
        /// Rb - Radius of circle B.
        /// 
        /// Returns:
        /// collision - Returns True if a collision occured, else False.
        /// The method returns the time to impact if collision=true, else it returns the time of closest approach.
        /// 
        /// Notes:
        /// This algorithm will work in any dimension.  Simply change the Vector2's to Vector3's to make this work
        /// for spheres.  You can also set the radii to 0 to work with points/rays.
        /// 
        public static float TimeOfClosestApproach(Vector2 Pa, Vector2 Pb, Vector2 Va, Vector2 Vb, float Ra, float Rb, out bool collision)
        {
            Vector2 Pab = Pa - Pb;
            Vector2 Vab = Va - Vb;
            float a = Vector2.Dot(Vab, Vab);
            float b = 2 * Vector2.Dot(Pab, Vab);
            float c = Vector2.Dot(Pab, Pab) - (Ra + Rb) * (Ra + Rb);

            // The quadratic discriminant.
            float discriminant = b * b - 4 * a * c;

            // Case 1:
            // If the discriminant is negative, then there are no real roots, so there is no collision.  The time of
            // closest approach is then given by the average of the imaginary roots, which is:  t = -b / 2a
            float t;
            if (discriminant < 0)
            {
                t = -b / (2 * a);
                collision = false;
            }
            else
            {
                // Case 2 and 3:
                // If the discriminant is zero, then there is exactly one real root, meaning that the circles just grazed each other.  If the 
                // discriminant is positive, then there are two real roots, meaning that the circles penetrate each other.  In that case, the
                // smallest of the two roots is the initial time of impact.  We handle these two cases identically.
                float t0 = (-b + (float)Math.Sqrt(discriminant)) / (2 * a);
                float t1 = (-b - (float)Math.Sqrt(discriminant)) / (2 * a);
                t = Math.Min(t0, t1);

                // We also have to check if the time to impact is negative.  If it is negative, then that means that the collision
                // occured in the past.  Since we're only concerned about future events, we say that no collision occurs if t < 0.
                if (t < 0)
                    collision = false;
                else
                    collision = true;
            }

            // Finally, if the time is negative, then set it to zero, because, again, we want this function to respond only to future events.
            if (t < 0)
                t = 0;

            return t;
        }

        public static void separate(Actor a, Actor b)
        {
            if (a.changeInPosition.Length() >
                b.changeInPosition.Length())
                a.Position -= a.changeInPosition;
            else
                b.Position -= b.changeInPosition;
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

            a.Velocity = acc * delta + a.MiscAcceleration * delta + vel;

            // cap the max speed for each state
            if (a.CurrState != Actor.state.Dashing  
                && Math.Sqrt(magnitudeSquared(a.Velocity)) > a.MaxVel && !a.IsBeingKnockedBack)
                a.Velocity = vel = a.MaxVel * Vector2.Normalize(a.Velocity);
            else if(a.CurrState == Actor.state.Dashing
                && Math.Sqrt(magnitudeSquared(a.Velocity)) > a.MaxVelDash && !a.IsBeingKnockedBack)
                a.Velocity = a.MaxVelDash * Vector2.Normalize(a.Velocity);
            
            // cap total max speed
            if (Math.Sqrt(magnitudeSquared(a.Velocity)) > Actor.MAX_POSSIBLE_SPEED)
                a.velocity = Actor.MAX_POSSIBLE_SPEED * Vector2.Normalize(a.Velocity);


            //relative friction
            if (applyFric)
                a.Velocity = vel = uk * a.Velocity;

            //a.Position = pos = vel * delta + pos;
            Vector2 dV = vel * delta;

            a.move(dV.X, dV.Y);

            //Rotations
            //if ( !finalVelNormalize.Equals(Vector2.Zero))
            if (!acc.Equals(Vector2.Zero)) // turn direction to mirror controller
            {
                a.Rotation = VectorToAngle(a.Acceleration);
            }

            //update the bounds
            float s = (float)Math.Sin(a.Rotation);
            float c = (float)Math.Cos(a.Rotation);
            //rotate about the origin then add to position
            a.bounds.center.X = /*c * (a.Offset.X) - s * (a.Offset.Y) +*/ a.Position.X;
            a.bounds.center.Y = /* s * (a.Offset.X) + c * (a.Offset.Y) +*/ a.Position.Y;

            if (a is Shark)
            {
                Shark sh = (Shark)a;

                sh.mouthPoint = sh.Bounds.center;
                sh.mouthPoint.X += 100;

                RotatePoint(sh.Bounds.center.X, sh.Bounds.center.Y, sh.Rotation, ref sh.mouthPoint);

                sh.mouthCircle.center = sh.mouthPoint;
            }

            if (a is Penguin)
            {
                Penguin p = (Penguin)a;

                p.spearPoint = p.Bounds.center;
                p.spearPoint.X += p.spearLength;

                RotatePoint(p.Bounds.center.X, p.Bounds.center.Y, p.Rotation, ref p.spearPoint);

                p.spearCircle.center = p.spearPoint;
            }

            a.acceleration     *= accDecay;
            a.MiscAcceleration *= accDecay;

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

        public static void RotatePoint(float cx, float cy, float angle, ref Vector2 p)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            // translate point back to origin:
            p.X -= cx;
            p.Y -= cy;

            // rotate point
            float xnew = p.X * c - p.Y * s;
            float ynew = p.X * s + p.Y * c;

            // translate point back:
            p.X = (xnew + cx);
            p.Y = (ynew + cy);
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

            Vector2 result = Vector2.Zero;

            result.X = dx;
            result.Y = dy;

            float distanceBetweenCenters = result.Length();

            float intersectionDepth = this.Radius + other.Radius - distanceBetweenCenters;

            return intersectionDepth;
        }

        public Boolean isCollision( Circle a )
        {
            float dx = center.X - a.center.X;
            float dy = center.Y - a.center.Y;

            float lengthSquared = dx * dx + dy * dy;

            float radii = radius + a.radius;

            float radiiSquared = radii * radii;

            if (lengthSquared <= radiiSquared)
                return true;

            return false;
        }

        public bool containsPoint(Vector2 p)
        {
            if ((p - center).Length() > radius)
                return false;

            return true;
        }
    }
}
