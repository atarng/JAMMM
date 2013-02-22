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
        public  const   int SHARK_BASE_HEALTH       = 200;
        private const float SHARK_ATTACK_THRESHOLD  = 400;
        private const float SHARK_AGGRESS_THRESHOLD = 1200;
        private const float PENGUIN_KNOCKBACK       = 100.0f;
        private const float SHARK_DASH_SPEED        = 450.0f;
        private const float SHARK_DASH_ACCELERATION = 1500.0f;
        private const float SHARK_SPEED             = 300.0f;
        private const float SHARK_ACCELERATION      = 400.0f;
        private const int   SHARK_FISH_CALORIES     = 5;

        public Vector2 mouthPoint;
        public Circle mouthCircle;

        private int calories;
        public int Calories
        {
            get { return calories; }
            set 
            {
                calories = value; 

                if (calories > MAX_HEALTH)
                    calories = MAX_HEALTH;
            }
        }

        public Shark() : base(0, 0, 160, 96, 60, 20)
        {
            this.MaxAccDash = SHARK_DASH_ACCELERATION;
            this.MaxVelDash = SHARK_DASH_SPEED;
            this.calories = SHARK_BASE_HEALTH;
            this.DashTime = 0.5f;

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

            this.isHit = false;
            resetBlink();

            resetPowerupState();

            changeState(state.Moving);
        }

        public override void applyPowerup(Powerup p)
        {
            if (p is Powerups.SpeedBoostPowerup)
            {
                resetPowerupState();

                powerupTimer = p.Duration;

                this.powerupState = powerupstate.SpeedBoost;

                this.MaxVel = Powerups.SpeedBoostPowerup.SPEED_BOOST_SPEED;
                this.MaxVelDash = Powerups.SpeedBoostPowerup.SPEED_BOOST_DASH_SPEED;
                this.MaxAcc = Powerups.SpeedBoostPowerup.SPEED_BOOST_ACCEL;
                this.MaxAccDash = Powerups.SpeedBoostPowerup.SPEED_BOOST_DASH_ACCEL;

                AudioManager.getSound("Power_Up").Play();
            }
        }

        protected override void resetPowerupState()
        {
            this.MaxAccDash = SHARK_DASH_ACCELERATION;
            this.MaxVelDash = SHARK_DASH_SPEED;
            this.MaxVel = SHARK_SPEED;
            this.MaxAcc = SHARK_ACCELERATION;

            this.powerupTimer = 0.0f;
            this.powerupState = powerupstate.None;
        }

        public override void update(GameTime gameTime)
        {
            double time = gameTime.ElapsedGameTime.TotalMilliseconds;

            if(IsAlive)
                if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                    ParticleManager.Instance.createParticle(ParticleType.Bubble,
                        new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y + rnd.Next(-15, 15)),
                        new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

            updatePowerupState(gameTime);

            tryToBlink(gameTime);

            tryToDie();

            if (this.CurrState == state.Moving)
            {
                acceleration.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5) * MaxAcc;
                acceleration.Y = 0;
            }
            else if (CurrState == state.Dash)
                changeState(state.Dashing);

            if (knockbackTime > 0.0f)
            {
                knockbackTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (knockbackTime <= 0.0f)
                {
                    knockbackTime = 0.0f;
                    isBeingKnockedBack = false;
                }
            }

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

        /// <summary>
        /// Try to move toward one of the players passed in.
        /// </summary>
        public void TryToAggressTowardPlayers(Shark s, List<Penguin> players)
        {
            if (s.CurrState != Actor.state.Moving)
                return;

            float minDist = 10000.0f;
            float currDist = 0.0f;
            Vector2 nearestPosition = Vector2.Zero;
            Vector2 distance = Vector2.Zero;
            Vector2 vecTowardNearest = Vector2.Zero;

            float chumDist = 10000.0f;

            foreach (Penguin p in players)
            {
                if (!p.IsAlive)
                    continue;
                if (p.PowerupState == powerupstate.SharkRepellent)
                    continue;

                distance.X = p.Position.X - s.Position.X;
                distance.Y = p.Position.Y - s.Position.Y;

                currDist = distance.Length();

                // make chum have priority
                if (p.PowerupState == powerupstate.Chum)
                {
                    if (currDist < chumDist)
                    {
                        chumDist = currDist;
                        nearestPosition = p.Position;
                        vecTowardNearest = distance;
                        continue;
                    }
                }

                if (currDist < minDist && chumDist >= 10000.0f)
                {
                    minDist = currDist;
                    nearestPosition = p.Position;
                    vecTowardNearest = distance;
                }
            }

            // LOL GO AFTER NO MATTER WHAT
            if (chumDist < 10000.0f)
            {
                s.acceleration = vecTowardNearest;
                return;
            }

            // try to aggress toward the nearest player
            if (minDist <= SHARK_AGGRESS_THRESHOLD
                && minDist > SHARK_ATTACK_THRESHOLD)
                s.acceleration = vecTowardNearest;
        }

        /// <summary>
        /// Try to attack one of the players in the list passed in.
        /// </summary>
        public void TryToAttackPlayers(Shark s, List<Penguin> players)
        {
            if (s.CurrState != Actor.state.Moving)
                return;

            float minDist = 10000.0f;
            float currDist = 0.0f;
            Vector2 nearestPosition = Vector2.Zero;
            Vector2 distance = Vector2.Zero;
            Vector2 vecTowardNearest = Vector2.Zero;

            float chumDist = 10000.0f;

            foreach (Penguin p in players)
            {
                if (!p.IsAlive)
                    continue;
                if (p.PowerupState == powerupstate.SharkRepellent)
                    continue;

                distance.X = p.Position.X - s.Position.X;
                distance.Y = p.Position.Y - s.Position.Y;

                currDist = distance.Length();

                // make chum have priority
                if (p.PowerupState == powerupstate.Chum)
                {
                    if (currDist < chumDist)
                    {
                        chumDist = currDist;
                        nearestPosition = p.Position;
                        vecTowardNearest = distance;
                        continue;
                    }
                }

                if (currDist < minDist)
                {
                    minDist = currDist;
                    nearestPosition = p.Position;
                    vecTowardNearest = distance;
                }
            }

            // LOL GO AFTER NO MATTER WHAT
            if (chumDist <= SHARK_ATTACK_THRESHOLD && s.CurrState != Actor.state.Dashing
                && s.CurrState != Actor.state.Dash)
            {
                s.acceleration = vecTowardNearest;
                ((Shark)s).attack();
                return;
            }

            // try to aggress toward the nearest player
            if (minDist <= SHARK_ATTACK_THRESHOLD
                && s.CurrState != Actor.state.Dashing
                && s.CurrState != Actor.state.Dash && chumDist >= 10000.0f)
            {
                s.acceleration = vecTowardNearest;
                ((Shark)s).attack();
            }
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

                isBeingKnockedBack = true;
                knockbackTime = KNOCKBACK_DURATION;

                // get physics rotation to p's rotation, 
                // normalize that, and magnify by the amount
                Vector2 pRotation = Physics.AngleToVector(other.Rotation);

                pRotation.Normalize();

                this.Position += pRotation * Actor.SPEAR_DISPLACEMENT;

                getHit();
            }
            else if (other is Fish)
            {
                if (other.CurrState == state.Moving &&
                    this.CurrState != state.Dying &&
                    this.IsAlive)
                {
                    AudioManager.getSound("Fish_Eat").Play();
                    this.calories += SHARK_FISH_CALORIES;
                    other.startDying();

                    if (other.Powerup != null)
                        applyPowerup(other.Powerup);
                }
            }
            else if (other is Penguin)
            {
                Penguin p = (Penguin)other;

                if (p.CurrState == state.MeleeAttack &&
                    this.Bounds.isCollision(p.spearCircle) 
                    && !isBeingKnockedBack)
                {
                    isBeingKnockedBack = true;
                    knockbackTime = KNOCKBACK_DURATION;

                    getHit();

                    this.calories -= p.MeleeDamage;

                    // get physics rotation to p's rotation, 
                    // normalize that, and magnify by the amount
                    Vector2 pRotation = Physics.AngleToVector(p.Rotation);

                    pRotation.Normalize();

                    this.velocity = pRotation * p.KnockbackAmount;
                    this.acceleration = Vector2.Zero;
                    this.Position += pRotation * Actor.MELEE_DISPLACEMENT;
                }
                else if (other.CurrState == state.Dying)
                {
                    this.calories += PENGUIN_CALORIES;
                    other.die();
                }
                else if (this.Bounds.isCollision(other.Bounds))
                    this.acceleration = Vector2.Zero;
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            if (!IsAlive)
                return;

            Color healthColor;

            if (this.isBlink)
                healthColor = Color.Gray;
            else
            {
                if (this.powerupState == powerupstate.SpeedBoost)
                    healthColor = Color.Yellow;
                else
                    healthColor = Color.White;
            }

            if (this.calories < 50)
                healthColor = Color.Orange;

            if (this.CurrState == state.Dying)
                healthColor = Color.Red;

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
