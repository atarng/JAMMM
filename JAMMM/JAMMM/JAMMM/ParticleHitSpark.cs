using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Actors;

namespace JAMMM
{
    class ParticleHitSpark : Particle
    {

        public static int type = 0;

        public ParticleHitSpark()
            : base()
        {
            loadContent();
        }

        public override void loadContent()
        {
            Random rnd = new Random();
            switch (type++ % 2)
            {
                case 0:
                    this.animation = new Animation(Actor.AnimationType.HitSpark, SpriteManager.getTexture("PFX_Burst"), 1, true, 0.2f);
                   
                    break;

                default:

                    this.animation = new Animation(Actor.AnimationType.HitSpark, SpriteManager.getTexture("PFX_Burst"), 1, true, 0.2f);
                    break;
            }

        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            animation.draw(batch, this.location + this.offset, Color.White * this.alpha, SpriteEffects.None, this.angle, this.size);
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
