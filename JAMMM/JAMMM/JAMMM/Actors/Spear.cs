using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class Spear : Actor
    {
        private int id;
        public int Id
        {
            get { return id; }
            set{id = value;}
        }

        public Penguin Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        private Penguin owner;

        public Spear() : base(0, 0, 0, 24, 20, 100)
        {
            MaxVel = 777;
        }

        protected override void onMoving()
        {
            changeAnimation(moveAnimation);
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Dash, 
                SpriteManager.getTexture("Spear"), 4, true, 0.1f);
        }

        public override void update(GameTime delta)
        {
            if (this.IsAlive)
            {
                base.update(delta);

                double time = delta.ElapsedGameTime.TotalMilliseconds;

                //if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(400, 500) || rnd.Next(1, 100) == 1)
                //    ParticleManager.Instance.createParticle(ParticleType.Bubble, 
                //        new Vector2(this.Position.X + rnd.Next(-15, 15), 
                //            this.Position.Y + rnd.Next(-15, 15)), new Vector2(0, 0), 
                //            3.14f / 2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

                currentAnimation.update(delta);
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            if (IsAlive)
            {
                if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(200, 500) || rnd.Next(1, 100) == 1)
                    ParticleManager.Instance.createParticle(ParticleType.Bubble, 
                        new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y 
                            + rnd.Next(-15, 15)), new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

                currentAnimation.draw(batch, this.Position, Color.White, SpriteEffects.FlipVertically, this.Rotation, 1.0f);

                if (printPhysics)
                    printPhys(batch);
            }
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);

            changeState(state.Moving);
        }

        public override void die()
        {
            base.die();

            if (owner != null)
                owner.spearAlive = false;
        }

        public void setSpawnParameters(Size s, int id, Penguin p)
        {
            this.owner = p;

            this.CurrentSize = s;
            this.id = id;

            acceleration = Vector2.Normalize(Physics.AngleToVector(p.Rotation)) * 50000F;
            velocity.X = p.Velocity.X;
            velocity.Y = p.Velocity.Y;
        }

        public void setSpawnParameters(Size s, int id, Penguin p, float rotation)
        {
            this.owner = p;

            this.CurrentSize = s;
            this.id = id;

            acceleration = Vector2.Normalize(Physics.AngleToVector(rotation)) * 50000F;
            velocity.X = p.Velocity.X;
            velocity.Y = p.Velocity.Y;
        }

        public void printPhys(SpriteBatch batch)
        {
            Color c = Color.Black;
            Vector2 loc = Position;
            Vector2 fontHeight;
            fontHeight.X = 0;
            fontHeight.Y = 14;

            //batch.DrawString(Game1.font, "Position " + Position, loc, c);
            //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "[>]", Bounds.center, c);
            batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
            batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);

            //batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public override void collideWith(Actor other)
        {
            if (other is Shark)
            {
                die();
            }
            else if (other is Penguin)
            {
                if (((Penguin)other).PowerupState != powerupstate.SpearDeflection)
                    die();
            }
            else if (other is Fish)
            {
                other.startDying();
            }
        }
    }
}
