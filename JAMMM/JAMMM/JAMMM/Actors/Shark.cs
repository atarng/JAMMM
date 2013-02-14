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

        public const int SHARK_BASE_HEALTH = 100;

        public Vector2 mouthPoint;
        public Circle mouthCircle;

        public Shark() : base(0, 0, 160, 96, 60, 1500)
        {
            this.MaxAccDash = 1500;
            this.MaxVelDash = 1000;
            this.calories = SHARK_BASE_HEALTH;

            mouthCircle = new Circle(this.Bounds.center.X + 100, this.Bounds.center.Y, 35);
            mouthPoint = Vector2.Zero;
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Move,
                SpriteManager.getTexture(Game1.SHARK_SWIM), 2, true, 0.2f);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.SHARK_DEATH), 3, false, 0.3f);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash, 
                SpriteManager.getTexture(Game1.SHARK_EAT), 4, false, 0.4f);
            turnAnimation = new Animation((Actor)this, AnimationType.Turn, 
                SpriteManager.getTexture(Game1.SHARK_TURN), 3, false, 0.2f);
        }

        private void tryToDie()
        {
            if (this.calories <= 0 && this.CurrState != state.Dying)
                changeState(state.Dying);
        }

        public void attack()
        {
            changeState(state.Dash);
        }

        protected override void onDying()
        {
            changeAnimation(deathAnimation);
        }

        protected override void onMoving()
        {
            CurrTime = 0.0f;
            changeAnimation(moveAnimation);
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);

            this.calories = SHARK_BASE_HEALTH;

            resetBlink();

            changeState(state.Moving);
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

            if (this.CurrState == state.Moving)
            {
                acceleration.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5) * MaxAcc;
                acceleration.Y = 0;
            }
            else if (CurrState == state.Dash)
                changeState(state.Dashing);

            currentAnimation.update(gameTime);
        }

        protected override void onDash()
        {
            changeAnimation(dashAnimation);
        }

        protected override void onDashing()
        {
            acceleration.Normalize();

            acceleration += acceleration * MaxAccDash;

            CurrTime = DashTime;
        }

        public override void handleAnimationComplete(Actor.AnimationType t)
        {
            if (t == Actor.AnimationType.Dash)
                changeState(state.Moving);
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

                getHit();
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
                currentAnimation.draw(batch, this.Position,
                    healthColor, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
            else
                currentAnimation.draw(batch, this.Position,
                    healthColor, SpriteEffects.None, this.Rotation, 1.0f);

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
