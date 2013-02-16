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

        public const float MINIMUM_AVOIDANCE_SPEED = 20.0f;

        public const float PLAYER_RANGE = 500.0f;
        public const float SHARK_RANGE = 700.0f;

        private Penguin nearestPlayer;
        private Shark nearestShark;

        private double randomX;
        private double randomY;

        private bool isEvading;

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

        public Fish() : base(0, 0, 32, 32, 16, 10) 
        {
            randomX = rnd.NextDouble() * 6.28;
            randomY = rnd.NextDouble() * 6.28;

            this.MaxVel = 300;
            this.MaxAcc = 400;

            isEvading = false;
        }

        public override void loadContent()
        {
            moveAnimation = new Animation((Actor)this, AnimationType.Dash, 
                SpriteManager.getTexture(Game1.FISH_SWIM), 4, true, 0.1f);
            deathAnimation = new Animation((Actor)this, AnimationType.Death, 
                SpriteManager.getTexture(Game1.FISH_DEATH), 8, false, 0.1f);
        }

        public override void spawnAt(Vector2 position)
        {
            base.spawnAt(position);

            changeState(state.Moving);

            randomX = rnd.NextDouble() * 2 - rnd.NextDouble();
            randomY = rnd.NextDouble() * 2 - rnd.NextDouble();

            this.isSchooling = false;
            this.isLeader = false;
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
            if (!isEvading)
            {
                // following someone else
                if (this.isSchooling && !this.isLeader)
                {
                    if (this.partner != null
                        && (!this.partner.IsAlive || this.partner.CurrState == Actor.state.Dying))
                    {
                        this.isSchooling = false;
                        this.partner = null;

                        // do other behavior immediately instead of a 1 frame delay
                        update(gameTime);
                    }
                    else
                    {
                        float distBetween = (this.Position - this.partner.Position).Length();

                        if (distBetween > SCHOOLING_RANGE)
                        {
                            Vector2 dirToPartner = this.partner.Position - this.Position;
                            dirToPartner.Normalize();

                            // accelerate to close the schooling gap with the new
                            // fish you are following
                            this.acceleration = (distBetween - SCHOOLING_RANGE) * dirToPartner;
                        }
                        else
                            this.acceleration = this.partner.acceleration;
                    }
                }
                else
                {
                    acceleration.X = (float)Math.Cos(randomX + gameTime.TotalGameTime.TotalSeconds * 2)
                        * MaxAcc * ((float)randomX + 1.0f) / 6.28f;

                    acceleration.Y = (float)Math.Sin(randomY + gameTime.TotalGameTime.TotalSeconds * 2)
                        * MaxAcc * (float)randomY / 12.56f;

                    if (rnd.NextDouble() > 0.99)
                    {
                        randomX = rnd.NextDouble() * 6.28;
                        randomY = rnd.NextDouble() * 6.28 * 2;
                    }
                }
            }

            avoidNearestPredators((float)gameTime.ElapsedGameTime.TotalSeconds);

            if ((this.velocity.Length() / MaxVel) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble,
                    new Vector2(this.Position.X + rnd.Next(-15, 15), this.Position.Y + rnd.Next(-15, 15)),
                    new Vector2(0, 0), 3.14f / 2.0f, 0.9f, 0.1f, -0.20f, 1, 0.5f, 10f);

            currentAnimation.update(gameTime);
        }

        /// <summary>
        /// Simulates our movement for the current frame.
        /// If it will move us into attack range of a predator
        /// then rotate our acceleration for this frame until
        /// we will no longer endanger ourselves.
        /// </summary>
        private void avoidNearestPredators(float gameTime)
        {
            Vector2 currPos = this.Position;
            Vector2 destVel = this.velocity;
            Vector2 destPos = this.Position;

            destVel = gameTime * this.acceleration;
            destPos = destVel * gameTime + currPos;

            if (nearestShark != null)
            {

            }

            if (nearestPlayer != null)
            {

            }
        }

        /// <summary>
        /// If this is not currently schooling and not a leader,
        /// then find a fish nearby to follow. If we have a partner
        /// that is not a leader or no partner then try to find 
        /// a leader to follow nearby.
        /// </summary>
        public void TryToSchool(List<Fish> fishPool)
        {
            if (!this.isLeader && !this.isSchooling)
            {
                float dist = 5000.0f;
                Fish f = getNearestFish(fishPool, ref dist, false);

                if (f != null)
                {
                    if (dist < SCHOOLING_RANGE)
                    {
                        // make it a leader if it isn't one and isn't already
                        // schooling with other fish
                        if (!f.IsLeader && !f.IsSchooling)
                            f.IsLeader = true;

                        this.partner = f;
                        this.isSchooling = true;

                        Vector2 dirToPartner = f.Position - this.Position;
                        dirToPartner.Normalize();

                        // accelerate to close the schooling gap with the new
                        // fish you are following
                        this.MiscAcceleration = (SCHOOLING_RANGE - dist) * dirToPartner;
                    }
                }
            }
            else if ((this.partner != null &&
                        !this.partner.isLeader) || this.partner == null)
            {
                float dist = 5000.0f;
                Fish f = getNearestFish(fishPool, ref dist, true);

                if (f != null)
                {
                    if (dist < LEADER_RANGE)
                    {
                        this.partner = f;
                        this.isLeader = false;
                        this.isSchooling = true;

                        Vector2 dirToPartner = f.Position - this.Position;
                        dirToPartner.Normalize();

                        // accelerate to close the schooling gap with the new
                        // fish you are following
                        this.acceleration = (LEADER_RANGE - dist) * dirToPartner;
                    }
                }
            }
        }

        /// <summary>
        /// If this is a leader fish it will try to evade players
        /// and sharks, allowing the other fish to follow suit and
        /// making it not quite as easy to catch fish.
        /// </summary>
        public void TryToEvade(List<Penguin> players, List<Shark> sharks)
        {
            if (CurrState == state.Dying || !this.IsAlive)
                return;
            if (!isSchooling && !isLeader)
                return;

            float distBetween = 0.0f;
            Vector2 currCloser = Vector2.Zero;
            Vector2 fastestCloser = Vector2.Zero;
            Actor guyToAvoid = null;
            float minTimeToCollision = 5000.0f;
            Penguin nearestP = null;
            float distToNearestP = 5000.0f;
            float distToNearestS = 5000.0f;
            Shark nearestS = null;
            float currTimeToCollision = 0.0f;

            // for each player
            foreach (Penguin p in players)
            {
                if (!p.IsAlive || p.CurrState == state.Dying)
                    continue;

                distBetween = (p.Position - this.Position).Length();

                if (distBetween < distToNearestP)
                {
                    distToNearestP = distBetween;
                    nearestP = p;
                }

                // player is in range
                if (distBetween <= PLAYER_RANGE)
                {
                    // if the player will not collide with our school along
                    // his path, continue
                    bool willCollide = false;

                    currTimeToCollision = 
                        Physics.TimeOfClosestApproach(p.Position, this.Position, p.velocity, this.velocity,
                                                      p.Bounds.Radius, SCHOOLING_RANGE, out willCollide);

                    if (!willCollide)
                        continue;

                    // otherwise, accumulate
                    currCloser = p.velocity;

                    if (currTimeToCollision < minTimeToCollision)
                    {
                        minTimeToCollision = currTimeToCollision;
                        fastestCloser = currCloser;
                        guyToAvoid = p;
                    }
                }
            }

            // evade sharks
            foreach (Shark s in sharks)
            {
                if (!s.IsAlive || s.CurrState == state.Dying)
                    continue;

                distBetween = (s.Position - this.Position).Length();

                if (distBetween < distToNearestS)
                {
                    distToNearestS = distBetween;
                    nearestS = s;
                }

                if (distBetween <= SHARK_RANGE)
                {
                    // if the player will not collide with our school along
                    // his path, continue
                    bool willCollide = false;
                    float timeToCollision = 0.0f;

                    timeToCollision = Physics.TimeOfClosestApproach(s.Position, this.Position, s.velocity, this.velocity,
                                                                    s.Bounds.Radius, SCHOOLING_RANGE, out willCollide);

                    if (!willCollide)
                        continue;

                    // otherwise, accumulate
                    currCloser = s.velocity;

                    if (currTimeToCollision < minTimeToCollision)
                    {
                        minTimeToCollision = currTimeToCollision;
                        fastestCloser = currCloser;
                        guyToAvoid = s;
                    }
                }
            }

            if (nearestP != null)
                this.nearestPlayer = nearestP;
            if (nearestS != null)
                this.nearestShark = nearestS;

            // somebody will collide so we must evade
            if (guyToAvoid != null && minTimeToCollision < 0.167f)
            {
                Vector2 escapeDirection = this.Position - guyToAvoid.Position;
                escapeDirection.Normalize();

                this.Acceleration += 
                    MaxAcc * escapeDirection;

                this.isEvading = true;
            }
            else this.isEvading = false;
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

                if (this.isLeader)
                    c = Color.Red;

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
