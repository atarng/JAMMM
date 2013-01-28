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
        //public Penguin.Size size;

        //(float x, float y, float offX, float offY, float radius, mass)
        //public Spear(float x, float y) : base(x, y, 0, 24, 10, 100) {
        //    MaxVel = 500;
        //}

        private int id;
        public int Id
        {
            get { return id; }
            set{id = value;}
        }

        public Spear(float x, float y, Size s, int id)
            : base(x, y, 0, 24, 20, 100)
        {
            MaxVel = 500;
            this.CurrentSize = s;
            this.id = id;
        }

        public override void loadContent()
        {
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Spear"), 4, true, 0.4f);
            base.loadContent();
        }

        public override void update(GameTime delta)
        {
             base.update(delta);
             double time = delta.ElapsedGameTime.TotalMilliseconds;
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Begin();

            dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
            if (printPhysics)
                printPhys(batch);
            batch.End();
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
            batch.DrawString(Game1.font, "[>]", Bounds.center, c);
            batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
            batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);

            batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        /// <summary>
        /// How the penguin collides with other actors.
        /// </summary>
        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {

            }
            else if (other is Penguin)
            {
                IsAlive = false;
            }
            else if (other is Shark)
            {
                IsAlive = false;
            }
            else if (other is Fish)
            {

            }
        }
    }
}
