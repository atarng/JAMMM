﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class AnimatedActorTest : Actor
    {
        public AnimatedActorTest(float x, float y, float offX, float offY, float radius) : base(x, y, offX, offY, radius, 1.0f)
        {
            this.MaxAcc = 400;
            this.MaxAccDash = 500;
            this.MaxVel = 500;

            this.Position = new Vector2(x, y);
            this.Offset = new Vector2(x, y);
            this.Bounds = new Circle(x + offX, y + offY, radius);
        }

        public override void processInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.IsConnected)
            {
                // then it is connected, and we can do stuff here
                acceleration.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                acceleration.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;
            }
        }

        public override void loadContent()
        {
            this.dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Shark_Eat"), 4, true, 0.2f);
            base.loadContent();
        }

        public override void update(GameTime gameTime)
        {
            if (!dashAnimation.IsPlaying)
                dashAnimation.play();

            processInput();
            dashAnimation.update(gameTime);
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Begin();

            dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.FlipHorizontally, this.Rotation, 1.0f);

            batch.End();
        }
    }
}
