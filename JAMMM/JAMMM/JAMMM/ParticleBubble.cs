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

        public static int type = 0;

        public ParticleBubble() : base()
        {
            loadContent();
        }

        // WARNING, TEMPORARILY SET TO DASH & Shark_Eat ANIMATION FOR TESTING
        public override void loadContent()
        {
            Random rnd = new Random();
            switch (type++ % 4)
            {
                case 0:
                   
                    this.animation = new Animation(Actor.AnimationType.Bubble, SpriteManager.getTexture("Particle_Bubble_1"), 1, true, 0.2f);
                    break;
                case 1:
                 
                    this.animation = new Animation(Actor.AnimationType.Bubble, SpriteManager.getTexture("Particle_Bubble_2"), 1, true, 0.2f);
                    break;
                case 2:
                    
                    this.animation = new Animation(Actor.AnimationType.Bubble, SpriteManager.getTexture("Particle_Bubble_3"), 1, true, 0.2f);
                    break;
                default:
               
                    this.animation = new Animation(Actor.AnimationType.Bubble, SpriteManager.getTexture("Particle_Bubble_4"), 1, true, 0.2f);
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
