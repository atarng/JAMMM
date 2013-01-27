﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class Shark : Actor
    {
        private Boolean useInput; //delete this later
        private Boolean fire;

        //public Shark() {}

        public Shark(float x, float y, Boolean useInput) : base(x, y, 40, 24, 20, 100)
        {
            this.useInput = useInput;
        }

        public override void processInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.IsConnected && useInput )
            {
                // then it is connected, and we can do stuff here
                acceleration.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                acceleration.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;

                if (gamePadState.Triggers.Right == 1)
                    fire = true;

                if (CurrState == state.DashReady && gamePadState.IsButtonDown(Buttons.A))
                {
                    CurrState = state.Dash;
                    CurrTime = DashTime;
                    if (Acceleration.Equals(Vector2.Zero))
                        Acceleration = Physics.AngleToVector(Rotation);
                }
                /* stop dashing
                else if (CurrState == state.Dashing && gamePadState.IsButtonUp(Buttons.A))
                {
                    CurrTime = DashCooldownTime;
                    CurrState = state.DashCooldown;
                }
                */
            }
        }

        public override void loadContent()
        {
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, SpriteManager.getTexture("Shark_Eat"), 4, true, 0.3f);
            base.loadContent();
        }

        public override void update(GameTime gameTime)
        {
            processInput();
            //ParticleManager.Instance.createParticle(ParticleType.Bubble, this.Position, new Vector2(0, 0), this.Rotation, -5, 1, 0, 1, 1.0f, 1.0f);

            dashAnimation.update(gameTime);
            switch(CurrState)
            {
                case state.Dash:
                    acceleration.Normalize();
                    acceleration = acceleration * MaxAccDash;
                    CurrTime = DashTime;
                    CurrState = state.Dashing;
                    break;
                case state.Dashing:
                    CurrTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if( CurrTime <= 0 )
                    {
                        CurrTime = DashCooldownTime;
                        CurrState = state.DashCooldown;
                    }
                    break;
                case state.DashCooldown:
                    CurrTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if( CurrTime <= 0 )
                    {
                        CurrState = state.DashReady;
                    }
                    break;
                default:
                    break;
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Begin();

            dashAnimation.draw(batch, this.Position, Color.White, SpriteEffects.None, this.Rotation, 1.0f);
            
            Color c = Color.Black;
            Vector2 loc = Position;
            Vector2 fontHeight;
            fontHeight.X = 0;
            fontHeight.Y = 14;

            batch.DrawString(Game1.font, "Position " + Position, loc, c);
            batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
            batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);

            String s = "";
            switch (CurrState)
            {
                case state.Dash:
                    s = "dash";
                    break;
                case state.Dashing:
                    s = "dashing";
                    break;
                case state.DashCooldown:
                    s = "dashcooldown";
                    break;
                case state.DashReady:
                    s = "dashready";
                    break;
            }
            batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
            if (fire)
            {
                batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                fire = false;
            }
            //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
            //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);


            //batch.DrawString(Game1.font, "Position " + Position, loc, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
             

            batch.End();
        }
    }
}
