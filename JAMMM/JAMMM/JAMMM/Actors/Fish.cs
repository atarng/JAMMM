﻿using System;
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
        private Animation dashAnimation;

        public Fish() 
        {
            this.MaxAcc = 400;
            this.MaxAccDash = 500;
            this.MaxVel = 500;

            this.Position = new Vector2(100, 100);
            this.Offset = new Vector2(8, 8);
            this.Bounds = new Circle(0 + this.Offset.X, 0 + this.Offset.Y, 8);
        }

        public Fish(float x, float y, float offX, float offY, float radius) 
        {
            this.MaxAcc = 400;
            this.MaxAccDash = 500;
            this.MaxVel = 500;

            this.Position = new Vector2(x, y);
            this.Offset = new Vector2(x, y);
            this.Bounds = new Circle(x + offX, y + offY, radius);

        }

        public override void loadContent()
        {
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Fish_Swim"), 2, true, 0.4f);
            base.loadContent();
        }

        public override void update(GameTime gameTime)
        {
            if (!dashAnimation.IsPlaying)
                dashAnimation.play();                        


            double time = gameTime.ElapsedGameTime.TotalMilliseconds;

            
            acceleration.X =  (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds*2) * MaxAcc;
            acceleration.Y = 0;

            dashAnimation.update(gameTime);
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {

            batch.Begin();

            if (Math.Abs(Rotation) > Math.PI / 2)
            {
                dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
            }
            else
            {
                dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.None, this.Rotation, 1.0f);
            }

            batch.End();
        }
    }
}
