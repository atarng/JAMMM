using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class Shark : Actor
    {
        //public Shark() {}

        public Shark(float x, float y)
            : base(x, y, 40, 24, 20, 100)
        { }

        public override void processInput(){}

        public override void loadContent()
        {
            // need to create the animations
            moveAnimation = new Animation((Actor)this, AnimationType.Move,
                SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL), 4, true);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL), 1, true);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Shark_Eat"), 4, true, 0.3f);
            base.loadContent();
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);
            this.currentAnimation = moveAnimation;
            this.currentAnimation.play();
        }

        public override void update(GameTime gameTime)
        {
            double time = gameTime.ElapsedGameTime.TotalMilliseconds;

            acceleration.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2) * MaxAcc;
            acceleration.Y = 0;// (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * 4) * MaxAcc;

            dashAnimation.update(gameTime);
        }

        public override void handleAnimationComplete(Actor.AnimationType t)
        {
            if (t == Actor.AnimationType.Death)
            {
                base.die();
            }
        }

        /// <summary>
        /// How the penguin collides with other actors.
        /// </summary>
        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {
                IsAlive = false;

                AudioManager.getSound("Actor_Hit").Play();
                Random rnd = new Random();
                ParticleManager.Instance.createParticle(ParticleType.HitSpark, new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)), new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f, (float)rnd.NextDouble(), -(float)rnd.NextDouble()*3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);

            }
            else if (other is Penguin)
            {
                tryToEat();
            }
            else if (other is Shark)
            {
            }
            else if (other is Fish)
            {

            }
        }

        private void tryToEat() 
        {
        
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Begin();

            dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.None, this.Rotation, 1.0f);

            if (printPhysics)
            {
                Color c = Color.Black;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                //batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                batch.DrawString(Game1.font, "[>]", Bounds.center, c);
                batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                /*
                String s = "";
                switch (CurrState)
                {
                    case state.Dash:
                        s = "dash";
                        break;
                    case state.Dashing:
                        s = "dashing";
                        break;
                    case state.DashCooldown:
                        s = "dashcooldown";
                        break;
                    case state.DashReady:
                        s = "dashready";
                        break;
                }
                 */
                //batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;
                }*/
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                batch.DrawString(Game1.font, "Rotation " + Rotation, loc+= fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Position " + Position, loc, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
            }
            batch.End();
        }
    }
}
