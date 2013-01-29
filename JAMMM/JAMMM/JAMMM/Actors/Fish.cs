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
        //(float x, float y, float offX, float offY, float radius, mass)
        public Fish(float x, float y) : base(x, y, 32, 32, 10, 10) 
        {
            
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
            
            acceleration.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2) * MaxAcc;
            //TODO MAKE COOLER
            acceleration.Y = 0;// (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * 4) * MaxAcc;
            
            currentAnimation.update(gameTime);
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
