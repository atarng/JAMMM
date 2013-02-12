using System;
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
        private int calories;
        public int Calories
        {
            get { return calories; }
            set { calories = value; }
        }

        private float blinkTime;
        private float numBlinks;
        private bool isHit;
        private bool isBlink;
        public const int NUMBER_BLINKS_ON_HIT = 3;
        public const float BLINK_DURATION = 0.1f;

        public Shark(float x, float y) 
            : base(x, y, 40, 24, 44, 1500)
        {
            this.MaxAccDash = 800;
            this.calories = 100;
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Move,
                SpriteManager.getTexture(Game1.SHARK_SWIM), 2, true, 0.2f);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.SHARK_DEATH), 3, false, 0.3f);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, 
                SpriteManager.getTexture(Game1.SHARK_EAT), 4, false, 0.3f);
            turnAnimation = new Animation((Actor)this, AnimationType.Turn, 
                SpriteManager.getTexture(Game1.SHARK_TURN), 3, false, 0.2f);
        }

        private void tryToBlink(GameTime gameTime)
        {
            if (this.isHit)
            {
                this.blinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.blinkTime > BLINK_DURATION)
                {
                    this.blinkTime -= BLINK_DURATION;

                    // switch to blinking off
                    if (isBlink)
                    {
                        this.isBlink = false;
                    }
                    // switch to blinking on 
                    else
                    {
                        this.isBlink = true;
                        this.numBlinks++;

                        // end the blink animation if we surpassed the maximum
                        // number of blinks
                        if (this.numBlinks > NUMBER_BLINKS_ON_HIT)
                        {
                            this.isHit = false;
                            this.isBlink = false;
                            this.blinkTime = 0.0f;
                            this.numBlinks = 0;
                        }
                    }
                }
            }
        }

        private void tryToDie()
        {
            if (this.calories <= 0 && this.CurrState != state.Dying)
            {
                currentAnimation = deathAnimation;
                currentAnimation.play();
                this.CurrState = state.Dying;
            }
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);
            this.calories = 100;
            this.currentAnimation = moveAnimation;
            this.currentAnimation.play();
            this.CurrState = state.Moving;
            this.isHit = false;
            this.isBlink = false;
            this.numBlinks = 0;
            this.Mass = 1;
        }

        public override void update(GameTime gameTime)
        {
            double time = gameTime.ElapsedGameTime.TotalMilliseconds;

            if(IsAlive)
                if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                    ParticleManager.Instance.createParticle(ParticleType.Bubble,
                        new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y + rnd.Next(-15, 15)),
                        new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);


            tryToBlink(gameTime);
            tryToDie();

            if (this.CurrState == state.Moving || this.CurrState == state.Turning)
            {
                acceleration.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5) * MaxAcc;
                acceleration.Y = 0;
            }
            else if (CurrState == state.Dash)
            {
                acceleration.Normalize();
                acceleration = acceleration * MaxAccDash;
                CurrTime = DashTime;
                this.CurrState = state.Dashing;

                currentAnimation = dashAnimation;
                currentAnimation.play();
            }
            else if (CurrState == state.Dashing)
            {
                CurrTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (CurrTime <= 0)
                {
                    this.CurrState = state.Moving;
                    currentAnimation = moveAnimation;
                    currentAnimation.play();
                }
            }

            boundaryCheck();
            currentAnimation.update(gameTime);
        }

        public override void handleAnimationComplete(Actor.AnimationType t)
        {
            if (t == Actor.AnimationType.Dash ||
                t == Actor.AnimationType.Turn)
            {
                this.CurrState = state.Moving;
                currentAnimation = moveAnimation;
                currentAnimation.play();
            }
        }

        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {
                if (other.CurrentSize == Size.Small)
                    this.calories -= SPEAR_SMALL_DAMAGE;
                else if (other.CurrentSize == Size.Medium)
                    this.calories -= SPEAR_MED_DAMAGE;
                else
                    this.calories -= SPEAR_MAX_DAMAGE;

                AudioManager.getSound("Actor_Hit").Play();
                Random rnd = new Random();

                ParticleManager.Instance.createParticle(ParticleType.HitSpark, 
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)), 
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f, 
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble()*3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
                ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                    new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                    new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                    (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);


                this.isHit = true;
                this.blinkTime = 0.0f;
                this.numBlinks = 0;
            }
            else if (other is Fish)
            {
                if (other.CurrState == state.Moving)
                {
                    AudioManager.getSound("Fish_Eat").Play();
                    this.calories += FISH_CALORIES;
                    other.startDying();
                }
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            if (!IsAlive)
                return;

            Color healthColor;

            if (this.Calories < 50 || this.isBlink)
                healthColor = Color.Red;
            else
                healthColor = Color.White;

            if (Math.Abs(Rotation) > Math.PI / 2)
            {
                currentAnimation.draw(batch, this.Position,
                    healthColor, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
            }
            else
            {
                currentAnimation.draw(batch, this.Position,
                    healthColor, SpriteEffects.None, this.Rotation, 1.0f);
            }

            if (printPhysics)
            {
                Color c = Color.Black;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                //batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                batch.DrawString(Game1.font, "[>]", Bounds.center, c);
                batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                /*
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
                 */
                //batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;
                }*/
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                batch.DrawString(Game1.font, "Rotation " + Rotation, loc+= fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Position " + Position, loc, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
            }
        }
    }
}
