using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    class TestActor : Actor
    {
        public TestActor(float x, float y, float offX, float offY, float radius)
        {
            this.MaxAcc = 250;
            this.MaxAccDash = 500;
            this.MaxVel = 500;

            this.Position = new Vector2(x,y);
            this.Offset = new Vector2(x, y);
            this.Bounds = new Circle(x + offX, y + offY, radius);
        }

        public void processInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.IsConnected)
            {
                // then it is connected, and we can do stuff here
                acceleration.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                acceleration.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;
            }
        }

        public void update(GameTime delta)
        {
            processInput();
            //Physics.applyMovement(this, delta.ElapsedGameTime.Seconds, false);
        }

        public void draw(GameTime delta, SpriteBatch batch)
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
            
             
            batch.DrawString(Game1.font, "Position " + Position, loc, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
            
            batch.End();
        }
    }
}
