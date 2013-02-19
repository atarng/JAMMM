using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Actors;

namespace JAMMM
{
    public class Fish : Actor
    {
        public const float SCHOOLING_RANGE = 250.0f;
        public const float LEADER_RANGE = 500.0f;
        public const float MAX_RANGE = 500.0f;

        public const float MINIMUM_AVOIDANCE_SPEED = 20.0f;

        public const float PLAYER_RANGE = 300.0f;
        public const float THREAT_RADIUS = 250.0f;
        public const float SHARK_RANGE = 400.0f;

        private Penguin nearestPlayer;
        private Shark nearestShark;

        private Circle boundsChecker;

        private double randomX;
        private double randomY;

        private bool gettingIntoSchoolRange;
        private bool evading;

        private bool isLeader;
        public bool IsLeader
        {
            get { return isLeader; }
            set { isLeader = value; }
        }

        private bool isSchooling;
        public bool IsSchooling
        {
            get { return isSchooling; }
        }

        private Fish partner;

        private bool isPoweredUp;
        public bool IsPoweredUp { get; }

        public Rectangle schoolingBounds;

        public Fish() : base(0, 0, 32, 32, 16, 10) 
        {
            randomX = rnd.NextDouble() * 6.28;
            randomY = rnd.NextDouble() * 6.28;

            this.MaxVel = 400;
            this.MaxAcc = 500;

            this.isSchooling = false;
            this.isLeader = false;
            this.partner = null;
            this.isPoweredUp = false;

            schoolingBounds = Rectangle.Empty;

            this.gettingIntoSchoolRange = false;

            boundsChecker = new Circle(0, 0, 0);
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Dash, 
                SpriteManager.getTexture(Game1.FISH_SWIM), 4, true, 0.1f);
            deathAnimation = new Animation((Actor)this, AnimationType.Death, 
                SpriteManager.getTexture(Game1.FISH_DEATH), 8, false, 0.1f);
        }

        public override void spawnAt(Vector2 position, Powerup p)
        {
            base.spawnAt(position);

            if (p != null)
            {
                this.powerup = p;
                this.isPoweredUp = true;
                this.MaxVel = 999;
                this.MaxAcc = 1000;
            }
            else
            {
                this.isPoweredUp = false;
                this.MaxVel = 400;
                this.MaxAcc = 500;
            }

            changeState(state.Moving);

            randomX = rnd.NextDouble() * 2 - rnd.NextDouble();
            randomY = rnd.NextDouble() * 2 - rnd.NextDouble();

            this.isSchooling = false;
            this.isLeader = false;
            this.partner = null;
            this.gettingIntoSchoolRange = false;
        }

        public override void respawn()
        {
            base.respawn();

            this.isSchooling = false;
            this.isLeader = false;

            changeState(state.Moving);
        }
        
        public override void update(GameTime gameTime)
        {
            // update our process of getting into range
            updateSchoolRange();

            // update our delinquency status
            updateDelinquency();

            // try to avoid nearby threats
            evading = avoidThreats();

            if (!evading)
            {
                // if we're a leader or a delinquent, do random movement
                if (this.isLeader || (!this.isLeader && !this.isSchooling))
                {
                    acceleration.X = (float)Math.Cos(randomX + gameTime.TotalGameTime.TotalSeconds * 2)
                        * MaxAcc * ((float)randomX + 1.0f) / 6.28f;

                    acceleration.Y = (float)Math.Sin(randomY + gameTime.TotalGameTime.TotalSeconds * 2)
                        * MaxAcc * (float)randomY / 12.56f;
                }
                // if we're schooling, then just follow our partner
                else
                {
                    if (!this.gettingIntoSchoolRange)
                        this.acceleration = this.partner.acceleration;
                }
            }

            // if simulating our motion puts us out of bounds, then shoot us back in to bounds
            updateSchoolingInBounds((float)gameTime.ElapsedGameTime.TotalSeconds);

            // update our random variable
            if (rnd.NextDouble() > 0.99)
            {
                randomX = rnd.NextDouble() * 6.28;
                randomY = rnd.NextDouble() * 6.28 * 2;
            }

            if ((this.velocity.Length() / MaxVel) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble,
                    new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y + rnd.Next(-15, 15)),
                    new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.1f, -0.20f, 1, 0.5f, 10f);

            currentAnimation.update(gameTime);
        }

        /// <summary>
        /// If there is a nearby leader in range, follow it.
        /// Otherwise, find the nearest fish. If it is in range,
        /// we're not following it or our partner is dead, and
        /// we're not a leader, then follow it. If that nearest
        /// fish is a delinquent, then make it a leader.
        /// </summary>
        public void TryToSchool(List<Fish> fishPool)
        {
            if (this.CurrState == state.Dying) return;

            float distToNearestLeader = 0.0f;
            Fish nearestLeader = getNearestFish(fishPool, ref distToNearestLeader, true);

            float distToNearestFish = 0.0f;
            Fish nearestFish = getNearestFish(fishPool, ref distToNearestFish, false);

            // if there is a nearby leader to follow and we're not already following it
            // and it is within range, then follow it. If we're a leader, then 
            // we are now a follower
            if (nearestLeader != null 
            && this.partner != nearestLeader
            && Actor.DistanceBetween(this, nearestLeader) < LEADER_RANGE)
            {
                this.isLeader = false;

                this.partner = nearestLeader;

                this.isSchooling = true;

                this.acceleration = 
                    Math.Abs(SCHOOLING_RANGE - distToNearestLeader) * this.DirectionTo(nearestLeader);

                this.gettingIntoSchoolRange = true;
            }
            // otherwise, if there is another fish to follow that we're not already following
            // or our partner is dead and we're not a leader and it is in range, then follow it. 
            // If it is a delinquent, make it a leader.
            else if (nearestFish != null 
                && (this.partner == null || 
                (this.partner != null && (!this.partner.IsAlive || this.partner.CurrState == state.Dying)))
                && !this.isLeader  && !nearestFish.isLeader && !nearestFish.IsSchooling
                && Actor.DistanceBetween(this, nearestFish) < SCHOOLING_RANGE)
            {
                this.partner = nearestFish;

                this.isSchooling = true;

                this.acceleration =
                    (SCHOOLING_RANGE - distToNearestFish) * this.DirectionTo(nearestFish);

                this.gettingIntoSchoolRange = true;
                nearestFish.IsLeader = true;
            }
        }

        /// <summary>
        /// Updates the school range variable.
        /// </summary>
        private void updateSchoolRange()
        {
            if (this.partner != null && this.gettingIntoSchoolRange)
            {
                if (Actor.DistanceBetween(this, this.partner) <=
                    SCHOOLING_RANGE)
                    this.gettingIntoSchoolRange = false;
                else if (Actor.DistanceBetween(this, this.partner) <=
                    MAX_RANGE && Actor.DistanceBetween(this, this.partner) >
                    SCHOOLING_RANGE)
                    this.acceleration += this.DirectionTo(this.partner) *
                        (Actor.DistanceBetween(this, this.partner) - SCHOOLING_RANGE);
                else
                    resetSchooling();
            }

            // if we go out of schooling range but not out of max range then
            // try to get back in
            if (this.partner != null && !this.gettingIntoSchoolRange &&
                Actor.DistanceBetween(this, this.partner) > SCHOOLING_RANGE &&
                Actor.DistanceBetween(this, this.partner) <= MAX_RANGE)
            {
                this.gettingIntoSchoolRange = true;
                this.acceleration += this.DirectionTo(this.partner) *
                        (Actor.DistanceBetween(this, this.partner) - SCHOOLING_RANGE);
            }
        }

        /// <summary>
        /// Become a delinquent if we get too far away
        /// from our school.
        /// </summary>
        private void updateDelinquency()
        {
            // if our partner becomes further than SCHOOLING_RANGE
            // when we're not trying to catch up to them then we become a delinquent.
            if (!this.gettingIntoSchoolRange &&
                this.partner != null &&
                !this.isLeader &&
                Actor.DistanceBetween(this, this.partner) > MAX_RANGE)
                resetSchooling();

            // if our partner dies or is dying we become a delinquent
            if (this.partner != null && 
                (!this.partner.IsAlive || this.partner.CurrState == state.Dying))
                resetSchooling();
        }

        /// <summary>
        /// If simulating our motion will put us out of bounds,
        /// then give us a push back inward. If we're a leader,
        /// then give us a huge push.
        /// </summary>
        private void updateSchoolingInBounds(float gameTime)
        {
            Vector2 currPos = this.Position;
            Vector2 destVel = this.velocity;
            Vector2 destPos = this.Position;
            Vector2 changingAcceleration = this.acceleration;
            Circle destBounds = this.Bounds;

            destVel = gameTime * changingAcceleration;
            destPos = destVel * gameTime + currPos;
            destBounds.center.X = destPos.X;
            destBounds.center.Y = destPos.Y;
        }

        /// <summary>
        /// Just reset all schooling variables.
        /// </summary>
        private void resetSchooling()
        {
            this.gettingIntoSchoolRange = false;
            this.partner                = null;
            this.isSchooling            = false;
            this.isLeader               = false;
        }

        /// <summary>
        /// If this is a leader fish it will try to evade players
        /// and sharks, allowing the other fish to follow suit and
        /// making it not quite as easy to catch fish.
        /// </summary>
        private bool avoidThreats()
        {
            // dead fish don't need to evade
            if (CurrState == state.Dying || !this.IsAlive)
                return false;

            if (nearestPlayer != null)
            {
                boundsChecker.center.X = nearestPlayer.Bounds.Center.X;
                boundsChecker.center.Y = nearestPlayer.Bounds.Center.Y;
                boundsChecker.Radius = PLAYER_RANGE;

                if (boundsChecker.containsPoint(this.Position))
                {
                    this.acceleration 
                        = nearestPlayer.DirectionTo(this) * this.MaxAcc;

                    return true;
                }
            }

            if (nearestShark != null)
            {
                boundsChecker.center.X = nearestShark.Bounds.Center.X;
                boundsChecker.center.Y = nearestShark.Bounds.Center.Y;
                boundsChecker.Radius = SHARK_RANGE;

                if (boundsChecker.containsPoint(this.Position))
                {
                    this.acceleration
                        = nearestShark.DirectionTo(this) * this.MaxAcc;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the nearest threatening actors for this fish 
        /// among the provided actors.
        /// </summary>
        public void SetNearestThreats(List<Penguin> players, List<Shark> sharks)
        {
            // dead finish don't need threats
            if (CurrState == state.Dying || !this.IsAlive)
                return;

            float   distBetween    = 0.0f;

            float   distToNearestP = 5000.0f;
            float   distToNearestS = 5000.0f;

            Penguin nearestP       = null;
            Shark   nearestS       = null;

            foreach (Penguin p in players)
            {
                if (!p.IsAlive || p.CurrState == state.Dying)
                    continue;

                distBetween = Actor.DistanceBetween(this, p);

                if (distBetween < distToNearestP)
                {
                    distToNearestP = distBetween;

                    nearestP = p;
                }
            }

            foreach (Shark s in sharks)
            {
                if (!s.IsAlive || s.CurrState == state.Dying)
                    continue;

                distBetween = Actor.DistanceBetween(this, s);

                if (distBetween < distToNearestS)
                {
                    distToNearestS = distBetween;

                    nearestS = s;
                }
            }

            if (nearestP != null)
                this.nearestPlayer = nearestP;

            if (nearestS != null)
                this.nearestShark = nearestS;
        }

        /// <summary>
        /// Get the nearest fish in the fish pool.
        /// </summary>
        private Fish getNearestFish(List<Fish> fishPool, ref float distance, bool wantsLeader)
        {
            float currDist = 0.0f;
            float minDist = 5000.0f;
            Fish nF = null;

            foreach (Fish f in fishPool)
            {
                if (f == this)
                    continue;

                if (!f.IsAlive)
                    continue;

                if (wantsLeader && !f.isLeader)
                    continue;

                currDist = (f.Position - this.Position).Length();

                if (currDist < minDist)
                {
                    minDist = currDist;
                    distance = currDist;
                    nF = f;
                }
            }

            return nF;
        }

        public override void startDying()
        {
            changeState(state.Dying);
        }

        protected override void onDying()
        {
            changeAnimation(deathAnimation);
        }

        protected override void onMoving()
        {
            changeAnimation(moveAnimation);
        }

        public override void handleAnimationComplete(Actor.AnimationType t)
        {
            if (t == Actor.AnimationType.Death)
                base.die();
        }

        public override void draw(GameTime gameTime, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                Color c = Color.White;

                // green for leaders
                if (this.isLeader)
                    c = Color.Green;
                // purple for delinquents
                else if (!this.isLeader && !this.isSchooling)
                    c = Color.Purple;
                // red for followers
                else
                    c = Color.Red;
                // blue for coward evaders
                if (this.evading)
                    c = Color.Blue;

                if (this.isPoweredUp)
                {
                    c.R = (byte)rnd.Next(256);
                    c.G = (byte)rnd.Next(256);
                    c.B = (byte)rnd.Next(256);
                    c.A = 255;
                }

                if (Math.Abs(Rotation) > Math.PI / 2)
                    currentAnimation.draw(batch, this.Position,
                        c, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
                else
                    currentAnimation.draw(batch, this.Position, 
                        c, SpriteEffects.None, this.Rotation, 1.0f);
            }
        }
    }
}
