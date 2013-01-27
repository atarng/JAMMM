using System;
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
        public AnimatedActorTest(float x, float y, float offX, float offY, float radius) 
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
            //dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Shark_Eat"), 4, true, 0.2f);
            //dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Fish_Swim"), 2, true, 0.4f);            
            //dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Fish_Death"), 8, true, 0.25f);
            //dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Shark_Turn"), 3, true, 0.5f);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Kelp_Idle"), 2, true, 0.5f);            
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
            Color c = Color.Black;
            Vector2 loc = Position;
            Vector2 fontHeight;
            fontHeight.X = 0;
            fontHeight.Y = 14;

            batch.DrawString(Game1.font, "Position " + Position, loc, c);
            batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
            batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
            batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);


            batch.DrawString(Game1.font, "Position " + Position, Bounds.Center, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 

            batch.End();

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
