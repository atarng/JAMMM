using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM
{
    public class Fish : Actor
    {

        private double randomX;
        private double randomY;

        //(float x, float y, float offX, float offY, float radius, mass)
        public Fish(float x, float y) : base(x, y, 32, 32, 10, 10) 
        {
            randomX = rnd.NextDouble() * 6.28;
            randomY = rnd.NextDouble() * 6.28;
            this.MaxVel = 100;
            this.MaxAcc = 200;
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture(Game1.FISH_SWIM), 4, true, 0.1f);
            deathAnimation = new Animation((Actor)this, AnimationType.Death, SpriteManager.getTexture(Game1.FISH_DEATH), 8, false, 0.1f);
            currentAnimation = moveAnimation;
            base.loadContent();
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);
            this.currentAnimation = moveAnimation;
            this.currentAnimation.play();
            this.CurrState = state.Moving;
            randomX = rnd.NextDouble() * 2 - rnd.NextDouble();
            randomY = rnd.NextDouble() * 2 - rnd.NextDouble();
        }

        public override void respawn()
        {
            base.respawn();
            this.currentAnimation = moveAnimation;
            this.currentAnimation.play();
            this.CurrState = state.Moving;
        }
        
        public override void update(GameTime gameTime)
        {
            double time = gameTime.ElapsedGameTime.TotalMilliseconds;

            acceleration.X = (float)Math.Cos(randomX + gameTime.TotalGameTime.TotalSeconds * 2) * MaxAcc * ((float)randomX + 1.0f) / 6.28f;
            acceleration.Y = (float)Math.Sin(randomY + gameTime.TotalGameTime.TotalSeconds * 2) * MaxAcc * (float)randomY / 12.56f;


            if (rnd.NextDouble() > 0.99)
            {
                randomX = rnd.NextDouble() * 6.28;
                randomY = rnd.NextDouble() * 6.28 * 2;
            }

            currentAnimation.update(gameTime);

            boundaryCheck();

            if ((this.velocity.Length() / MaxVel) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble,
                    new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y + rnd.Next(-15, 15)),
                    new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.1f, -0.20f, 1, 0.5f, 10f);
        }

        public override void startDying()
        {
            this.CurrState = state.Dying;
            currentAnimation = deathAnimation;
            currentAnimation.play();
        }

        public override void handleAnimationComplete(Actor.AnimationType t)
        {
            if (t == Actor.AnimationType.Death)
            {
                base.die();
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                batch.Begin();

                if (Math.Abs(Rotation) > Math.PI / 2)
                {
                    currentAnimation.draw(batch, this.Position, Color.White, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
                }
                else
                {
                    currentAnimation.draw(batch, this.Position, Color.White, SpriteEffects.None, this.Rotation, 1.0f);
                }

                batch.End();
            }
        }
    }
}
