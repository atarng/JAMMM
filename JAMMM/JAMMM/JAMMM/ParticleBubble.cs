using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Actors;

namespace JAMMM
{
    class ParticleBubble : Particle
    {

        public ParticleBubble() : base()
        {
            loadContent();
        }

        // WARNING, TEMPORARILY SET TO DASH & Shark_Eat ANIMATION FOR TESTING
        public override void loadContent()
        {
            this.animation = new Animation(Actor.AnimationType.Bubble, SpriteManager.getTexture("Fish_Death"), 4, true, 0.2f);
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Begin();

            animation.draw(batch, this.location + this.offset, Color.White * this.alpha, SpriteEffects.None, this.angle, this.size);

            batch.End();
        }

        public override bool update(GameTime gameTime)
        {

            this.location.X += this.speed * this.direction.X;
            this.location.Y += this.speed * this.direction.Y;

            animation.updateParticle(gameTime);
  
            this.size  -= this.growth * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.alpha -= this.dim * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.life  -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            return checkDeath();

        }

    }
}
