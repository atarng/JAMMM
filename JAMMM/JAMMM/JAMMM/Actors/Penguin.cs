using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM.Actors
{
    public class Penguin : Actor
    {
        public const int START_CALORIES = 100;

        public const int SMALL_SIZE_CALORIES_THRESHOLD = 1;
        public const int MED_SIZE_CALORIES_THRESHOLD   = 150;
        public const int MAX_SIZE_CALORIES_THRESHOLD   = 230;

        public const int SMALL_SIZE = 22;
        public const int MED_SIZE   = 33;
        public const int LARGE_SIZE = 54;

        private const int MELEE_DAMAGE_SMALL = 5;
        private const int MELEE_DAMAGE_MEDIUM = 10;
        private const int MELEE_DAMAGE_LARGE = 15;

        private const float KNOCKBACK_SMALL = 400.0f;
        private const float KNOCKBACK_MEDIUM = 500.0f;
        private const float KNOCKBACK_LARGE = 600.0f;
        private const float SHARK_KNOCKBACK = 150.0f;

        public const int SPEAR_SMALL_COST = 5;
        public const int SPEAR_MED_COST   = 10;
        public const int SPEAR_LARGE_COST = 15;

        public const int DASH_SMALL_COST = 0;
        public const int DASH_MED_COST   = 1;
        public const int DASH_LARGE_COST = 3;

        public const int MELEE_SMALL_COST = 0;
        public const int MELEE_MED_COST   = 1;
        public const int MELEE_LARGE_COST = 2;

        public const int SPEAR_LENGTH_SMALL  = 26;
        public const int SPEAR_RADIUS_SMALL  = 8;
        public const int SPEAR_LENGTH_MEDIUM = 40;
        public const int SPEAR_RADIUS_MEDIUM = 10;
        public const int SPEAR_LENGTH_LARGE  = 64;
        public const int SPEAR_RADIUS_LARGE  = 14;

        public const int SMALL_MASS = 100;
        public const int MEDIUM_MASS = 500;
        public const int LARGE_MASS = 1500;

        private const float DASH_SPEED_SMALL  = 500.0f;
        private const float DASH_ACCEL_SMALL  = 550.0f;
        private const float DASH_SPEED_MEDIUM = 400.0f;
        private const float DASH_ACCEL_MEDIUM = 450.0f;
        private const float DASH_SPEED_LARGE  = 300.0f;
        private const float DASH_ACCEL_LARGE  = 350.0f;

        private const float MAX_SPEAR_LENGTH  = 100.0f;

        private float meleeCooldown;
        public float MeleeCooldown
        {
            get { return meleeCooldown; }
            set { meleeCooldown = value; }
        }

        private float fireCooldown;
        public float FireCooldown
        {
            get { return FireCooldown; }
            set { FireCooldown = value; }
        }

        private float knockbackAmount;
        public float KnockbackAmount
        {
            get { return knockbackAmount; }
        }

        private int meleeDamage;
        public int MeleeDamage
        {
            get { return meleeDamage; }
        }

        public int spearLength;

        public bool spearAlive; 

        private Boolean fire;
        public Boolean Fire
        {
            get { return fire; }
            set { fire = value; }
        }

        private float fireTime;
        public float FireTime
        {
            get { return fireTime; }
            set { fireTime = value; }
        }

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

        private PlayerIndex controller;
        public PlayerIndex Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        private bool prevStateA = false;

        private string colorCode;

        public Vector2 spearPoint;
        public Circle spearCircle;

        private float meleeTime;
        private bool canShowMeleeSpark;
        private bool canMelee;

        public Color color;

        public Circle spearDeflectorAura;

        public Penguin(PlayerIndex playerIndex, Vector2 pos, string colorCode) 
            : base(pos.X, pos.Y, 36, 32, SMALL_SIZE, SMALL_MASS)
        {
            if (colorCode == "")
                color = Color.Blue;
            else if (colorCode == "_r")
                color = Color.Red;
            else if (colorCode == "_p")
                color = Color.Purple;
            else
                color = Color.Green;

            this.colorCode        = colorCode;
            this.controller       = playerIndex;
            this.startingPosition = pos;
            this.Calories         = START_CALORIES;
            this.CurrentSize      = Size.Small;

            this.DashCost         = DASH_SMALL_COST;
            this.SpearCost        = SPEAR_SMALL_COST;
            this.MeleeCost        = MELEE_SMALL_COST;

            this.meleeCooldown    = MELEE_COOLDOWN;
            this.fireCooldown     = FIRE_COOLDOWN;

            this.isHit = false;
            this.canMelee = true;
            meleeTime = 0.0f;
            resetBlink();

            spearPoint = Vector2.Zero;
            spearCircle = new Circle(this.Bounds.center.X + 60, this.Bounds.center.Y, 50);
            spearDeflectorAura = new Circle(0, 0, 75);
        }

        public override void processInput()
        {
            if (this.CurrState == state.Dying)
                return;

            GamePadState gamePadState = GamePad.GetState(controller);

            if (gamePadState.IsConnected)
            {
                Vector2 accController = Acceleration;

                accController.X = gamePadState.ThumbSticks.Left.X * MaxAcc;
                accController.Y = -1 * gamePadState.ThumbSticks.Left.Y * MaxAcc;

                //if the acceleration is > max acc (means we were dashing)
                if (!(CurrState == state.Dashing &&
                    (Vector2.Normalize(accController) == Vector2.Normalize(Acceleration) 
                    || Vector2.Zero == accController)))
                    Acceleration = accController;

                if ( this.calories > DashCost 
                  && gamePadState.IsButtonDown(Buttons.A) && !prevStateA && 
                    CurrState != state.MeleeAttack)
                    changeState(state.Dash);

                if (gamePadState.IsButtonUp(Buttons.A) 
                    && (CurrState == state.DashReady ||
                        CurrState == state.Moving))
                    prevStateA = false;

                if (gamePadState.IsButtonDown(Buttons.X) &&
                    canMelee && CurrState != state.MeleeAttack && !spearAlive)
                {
                    if (CurrState == state.Dash ||
                        CurrState == state.Dashing)
                    {
                        CurrTime = 0.0f;
                        prevStateA = false;
                    }

                    changeState(state.MeleeAttack);
                }
                
                if (gamePadState.Triggers.Right == 1 && FireTime <= 0)
                {
                    if (this.Calories > this.SpearCost &&
                        this.CurrState != state.MeleeAttack)
                    {
                        AudioManager.getSound("Spear_Throw").Play();
                        FireTime = fireCooldown;
                        fire = true;
                        this.Calories -= this.SpearCost;
                    }
                }
            }

            KeyboardState kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.Space) && fireTime <= 0)
            {
                fireTime = fireCooldown;
                fire = true;
            }

            if (kbState.IsKeyDown(Keys.LeftShift) 
                && this.CurrState != state.Dashing 
                && this.CurrState != state.Dash)
                changeState(state.Dash);

            if (kbState.IsKeyDown(Keys.F)
                && canMelee && this.CurrState != state.Dashing
                && this.CurrState != state.Dash)
                changeState(state.MeleeAttack);

            if (kbState.IsKeyDown(Keys.W))
                acceleration.Y = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.A))
                acceleration.X = -1 * MaxAcc;
            if (kbState.IsKeyDown(Keys.D))
                acceleration.X = MaxAcc;
            if (kbState.IsKeyDown(Keys.S))
                acceleration.Y = MaxAcc;
        }

        public override void loadContent()
        {
            // need to create the animations
            moveAnimation = new Animation((Actor)this, AnimationType.Move, 
                SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL + colorCode), 4, true);
            dashAnimation = new Animation((Actor)this, AnimationType.Dash,
                SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL + colorCode), 1, false);
            deathAnimation = new Animation((Actor)this, AnimationType.Death,
                SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL + colorCode), 1, false, 1.5f);
            meleeAnimation = new Animation((Actor)this, AnimationType.Melee,
                SpriteManager.getTexture(Game1.PENGUIN_MELEE_SMALL + colorCode), 4, false);  
        }

        public override void update(GameTime delta)
        {
            if (!this.IsAlive) return;

            updatePowerupState(delta);

            tryToGrow();

            tryToDie();

            tryToBlink(delta);

            if ((this.velocity.Length() / MaxVelDash) * 100 > rnd.Next(1, 700) || rnd.Next(1, 100) == 1)
                ParticleManager.Instance.createParticle(ParticleType.Bubble, 
                    new Vector2(this.Position.X + rnd.Next(-15,15), this.Position.Y + rnd.Next(-15,15)), 
                    new Vector2(0, 0), 3.14f/2.0f, 0.9f, 0.4f, -0.20f, 1, 0.5f, 10f);

            currentAnimation.update(delta);

            processInput();

            if (CurrState == state.Dash)
                changeState(state.Dashing);
            else if (CurrState == state.Dashing)
            {
                CurrTime -= (float)delta.ElapsedGameTime.TotalSeconds;

                if (CurrTime <= 0)
                    changeState(state.DashReady);
            }

            if (meleeTime > 0)
            {
                meleeTime -= (float)delta.ElapsedGameTime.TotalSeconds;

                float meleeRatio = 1.0f - (meleeTime / meleeCooldown);

                this.spearCircle.Radius = (int)(MAX_SPEAR_LENGTH * meleeRatio);

                if (meleeTime <= 0)
                {
                    meleeTime = 0.0f;
                    canMelee = true;
                }
            }

            if (fireTime > 0)
                fireTime -= (float)delta.ElapsedGameTime.TotalSeconds;

            if (knockbackTime > 0.0f)
            {
                knockbackTime -= (float)delta.ElapsedGameTime.TotalSeconds;

                if (knockbackTime <= 0.0f)
                {
                    knockbackTime = 0.0f;
                    isBeingKnockedBack = false;
                }
            }
        }

        protected override void onDash()         
        {
            this.calories -= DashCost;

            prevStateA = true;

            changeAnimation(dashAnimation);

            AudioManager.getSound("Actor_Dash").Play();
        }

        protected override void onDashing()      
        {
            if (Acceleration.Equals(Vector2.Zero))
                Acceleration = Physics.AngleToVector(Rotation);
            else
                Acceleration.Normalize();

            acceleration += acceleration * MaxAccDash;

            CurrTime = DashTime;
        }

        protected override void onDashReady()    
        {
            CurrTime = 0.0f;

            changeState(state.Moving);
        }

        protected override void onMoving()
        {
            changeAnimation(moveAnimation);
        }

        protected override void onDying()        
        {
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
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);
            ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 5, 1, 1 + (float)rnd.NextDouble(), 2f);

            AudioManager.getSound("Death_Penguin").Play();
            changeAnimation(deathAnimation);
        }
        
        protected override void onMeleeAttack()  
        {
            this.calories -= MeleeCost;

            canMelee = false;

            meleeTime = meleeCooldown;

            changeAnimation(meleeAnimation);

            canShowMeleeSpark = true;

            AudioManager.getSound("Spear_Throw").Play();
        }

        private void tryToGrow()
        {
            if (calories >= MAX_SIZE_CALORIES_THRESHOLD   && this.CurrentSize != Size.Large)
                becomeLarge();
            else if (calories >= MED_SIZE_CALORIES_THRESHOLD 
                && calories < MAX_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Medium)
                becomeMedium();
            else if (calories >= SMALL_SIZE_CALORIES_THRESHOLD 
                && calories < MED_SIZE_CALORIES_THRESHOLD && this.CurrentSize != Size.Small)
                becomeSmall();
        }

        private void becomeSmall()
        {
            this.CurrentSize = Size.Small;

            this.DashCost = DASH_SMALL_COST;
            this.SpearCost = SPEAR_SMALL_COST;
            this.MeleeCost = MELEE_SMALL_COST;
            this.meleeDamage = MELEE_DAMAGE_SMALL;
            this.knockbackAmount = KNOCKBACK_SMALL;
            this.spearLength = SPEAR_LENGTH_SMALL;
            this.spearCircle.Radius = SPEAR_RADIUS_SMALL;

            if (this.powerupState != powerupstate.SpeedBoost)
            {
                this.MaxAccDash = DASH_ACCEL_SMALL;
                this.MaxVelDash = DASH_SPEED_SMALL;
            }

            moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_SMALL + colorCode), 4);
            dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_SMALL + colorCode), 1);
            deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_SMALL + colorCode), 1);
            meleeAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MELEE_SMALL + colorCode), 4);

            this.Bounds.Radius = SMALL_SIZE;

            this.Mass = SMALL_MASS;
        }

        private void becomeMedium()
        {
            this.CurrentSize = Size.Medium;

            this.DashCost = DASH_MED_COST;
            this.SpearCost = SPEAR_MED_COST;
            this.MeleeCost = MELEE_MED_COST;
            this.meleeDamage = MELEE_DAMAGE_MEDIUM;
            this.knockbackAmount = KNOCKBACK_MEDIUM;
            this.spearLength = SPEAR_LENGTH_MEDIUM;
            this.spearCircle.Radius = SPEAR_RADIUS_MEDIUM;

            if (this.powerupState != powerupstate.SpeedBoost)
            {
                this.MaxAccDash = DASH_ACCEL_MEDIUM;
                this.MaxVelDash = DASH_SPEED_MEDIUM;
            }

            moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_MEDIUM + colorCode), 4);
            dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_MEDIUM + colorCode), 1);
            deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_MEDIUM + colorCode), 1);
            meleeAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MELEE_MEDIUM + colorCode), 4);

            this.Bounds.Radius = MED_SIZE;

            this.Mass = MEDIUM_MASS;
        }

        private void becomeLarge()
        {
            this.CurrentSize = Size.Large;

            this.DashCost = DASH_LARGE_COST;
            this.SpearCost = SPEAR_LARGE_COST;
            this.MeleeCost = MELEE_LARGE_COST;
            this.meleeDamage = MELEE_DAMAGE_LARGE;
            this.knockbackAmount = KNOCKBACK_LARGE;
            this.spearLength = SPEAR_LENGTH_LARGE;
            this.spearCircle.Radius = SPEAR_RADIUS_LARGE;

            if (this.powerupState != powerupstate.SpeedBoost)
            {
                this.MaxAccDash = DASH_ACCEL_LARGE;
                this.MaxVelDash = DASH_SPEED_LARGE;
            }

            moveAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MOVE_LARGE + colorCode), 8);
            dashAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DASH_LARGE + colorCode), 1);
            deathAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_DEATH_LARGE + colorCode), 1);
            meleeAnimation.replaceSpriteSheet(SpriteManager.getTexture(Game1.PENGUIN_MELEE_LARGE + colorCode), 4);

            this.Bounds.Radius = LARGE_SIZE;

            this.Mass = LARGE_MASS;
        }

        private void tryToDie()
        {
            if (this.calories <= 0
                && this.CurrState != state.Dying)
            {
                changeState(state.Dying);
            }
        }

        public override void die()
        {
            base.die();
        }

        /// <summary>
        /// Checks if the player can throw a spear
        /// and respawns an existing dead spear
        /// with new physical settings as a result.
        /// </summary>
        public void TrySpear(int id, List<Spear> spearPool)
        {
            if (this.Fire)
            {
                this.Fire = false;

                foreach (Spear s in spearPool)
                {
                    if (!s.IsAlive)
                    {
                        s.setSpawnParameters(this.CurrentSize, id, this);
                        s.spawnAt(this.Position);
                        spearAlive = true;
                        break;
                    }
                }
            }
        }

        public void resetProperties()
        {
            this.isHit = false;

            this.Calories = START_CALORIES;

            resetBlink();

            resetPowerupState();

            becomeSmall();

            resetPhysics();

            changeState(state.DashReady);
        }

        public override void applyPowerup(Powerup p)
        {
            resetPowerupState();

            powerupTimer = p.Duration;

            if (p is Powerups.SpeedBoostPowerup)
            {
                this.powerupState = powerupstate.SpeedBoost;

                this.MaxVel = Powerups.SpeedBoostPowerup.SPEED_BOOST_SPEED;
                this.MaxVelDash = Powerups.SpeedBoostPowerup.SPEED_BOOST_DASH_SPEED;
                this.MaxAcc = Powerups.SpeedBoostPowerup.SPEED_BOOST_ACCEL;
                this.MaxAccDash = Powerups.SpeedBoostPowerup.SPEED_BOOST_DASH_ACCEL;
            }
            else if (p is Powerups.RapidFirePowerup)
            {
                this.powerupState = powerupstate.RapidFire;

                this.meleeCooldown = Powerups.RapidFirePowerup.RAPID_FIRE_MELEE_COOLDOWN;
                this.fireCooldown = Powerups.RapidFirePowerup.RAPID_FIRE_FIRE_COOLDOWN;
                this.SpearCost = Powerups.RapidFirePowerup.RAPID_FIRE_COST;
            }
            else if (p is Powerups.SharkRepellentPowerup)
            {
                this.powerupState = powerupstate.SharkRepellent;
            }
            else if (p is Powerups.SpearDeflectionPowerup)
            {
                this.powerupState = powerupstate.SpearDeflection;
            }

            AudioManager.getSound("Power_Up").Play();
        }

        protected override void resetPowerupState()
        {
            this.meleeCooldown = MELEE_COOLDOWN;
            this.fireCooldown  = FIRE_COOLDOWN;

            if (this.CurrentSize == Size.Large)
            {
                this.MaxAccDash = DASH_ACCEL_LARGE;
                this.MaxVelDash = DASH_SPEED_LARGE;
                this.SpearCost = SPEAR_LARGE_COST;
            }
            else if (this.CurrentSize == Size.Medium)
            {
                this.MaxAccDash = DASH_ACCEL_MEDIUM;
                this.MaxVelDash = DASH_SPEED_MEDIUM;
                this.SpearCost = SPEAR_MED_COST;
            }
            else
            {
                this.MaxAccDash = DASH_ACCEL_SMALL;
                this.MaxVelDash = DASH_SPEED_SMALL;
                this.SpearCost = SPEAR_SMALL_COST;
            }

            this.MaxAcc     = BASE_ACCEL;
            this.MaxVel     = BASE_SPEED;

            this.powerupTimer = 0.0f;
            this.powerupState = powerupstate.None;
        }

        public override void draw(GameTime delta, SpriteBatch batch)
        {
            if (this.IsAlive)
            {
                Color c;

                if (this.isBlink)
                    c = Color.Pink;
                else
                {
                    Vector2 auraPosition = this.Position;

                    auraPosition.X -= 75;
                    auraPosition.Y -= 75;

                    switch (this.powerupState)
                    {
                        case powerupstate.SpeedBoost:
                        {
                            c = Color.Yellow;
                            break;
                        }
                        case powerupstate.RapidFire:
                        {
                            c = Color.SandyBrown;
                            break;
                        }
                        case powerupstate.SharkRepellent:
                        {
                            c = Color.Purple;
                            batch.Draw(SpriteManager.getTexture(Game1.SHARKREPELLENT_AURA), 
                                auraPosition, Color.White);
                            break;
                        }
                        case powerupstate.SpearDeflection:
                        {
                            c = Color.LightBlue;
                            batch.Draw(SpriteManager.getTexture(Game1.SPEARDEFLECTION_AURA), 
                                auraPosition, Color.White);
                            break;
                        }
                        default:
                        {
                            c = Color.White;
                            break;
                        }
                    }
                }

                // spawn some particles on the last frame of the melee attack animation
                if (currentAnimation == meleeAnimation &&
                    meleeAnimation.FrameIndex == 3 && canShowMeleeSpark)
                {
                    ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                        new Vector2(spearPoint.X, spearPoint.Y + 10),
                        new Vector2(0, 0), this.Rotation, 0.0f,
                        0.9f, 1.5f, 0.8f, 1, 0.3f, this.color.R, this.color.G, this.color.B);

                    canShowMeleeSpark = false;
                }

                if (Math.Abs(Rotation) > Math.PI / 2)
                    currentAnimation.draw(batch, this.Position,
                        c, SpriteEffects.FlipVertically, this.Rotation, 1.0f);
                else
                    currentAnimation.draw(batch, this.Position,
                        c, SpriteEffects.None, this.Rotation, 1.0f);

                if (printPhysics)
                    printPhys(batch);
            }
        }

        public void printPhys(SpriteBatch batch)
        {
                Color c = Color.White;
                Vector2 loc = Position;
                Vector2 fontHeight;
                fontHeight.X = 0;
                fontHeight.Y = 14;

                //batch.DrawString(Game1.font, "Position " + Position, loc, c);
                //batch.DrawString(Game1.font, "Center " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "[>]", Bounds.center, Color.Red);
                //batch.DrawString(Game1.font, "Cal " + Calories, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                string s = "";
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
               // batch.DrawString(Game1.font, "Dash " + s, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "DashCost " + DashCost, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Bounds " + Bounds.Center, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Offset " + Offset.X + " " + Offset.Y, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Mass " + Mass, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Radi " + Bounds.Radius, loc += fontHeight, c);
                /*if (fire)
                {
                    batch.DrawString(Game1.font, "FIRE", loc += fontHeight, c);
                    fire = false;f
                }*/
               // batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c);
               // batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c);

                //batch.DrawString(Game1.font, "Rotation " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);

                //batch.DrawString(Game1.font, "Velocity " + Velocity, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Accleration " + Acceleration, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
                //batch.DrawString(Game1.font, "Rot " + Rotation, loc += fontHeight, c, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0); 
        }

        public override void respawn()
        {
            base.respawn();

            resetProperties();
        }

        public override void handleAnimationComplete(AnimationType t) 
        {
            if (t == AnimationType.Death)
                die();
            else if (t == AnimationType.Melee)
                changeState(state.Moving);
        }

        public override void collideWith(Actor other)
        {
            if (other is Spear)
            {
                if (this.powerupState == powerupstate.SpearDeflection)
                {
                    other.acceleration = Vector2.Zero;

                    Vector2 reflectingVector = this.DirectionTo(other);
                    float reflectingRotation = Physics.VectorToAngle(reflectingVector);

                    other.velocity
                        = other.MaxVel * reflectingVector;

                    ((Spear)other).Owner = this;

                    other.Rotation = reflectingRotation;

                    AudioManager.getSound("Ding").Play();
                }
                else
                {
                    // Set delay so we can't immediately fire a spear after being hit
                    this.FireTime = fireCooldown * 1.5f;
                    this.Fire = false;

                    // take damage based on the spear's owner's size
                    if (other.CurrentSize == Size.Large)
                    {
                        AudioManager.getSound("Actor_Hit").Play();

                        if (((Spear)other).Owner.powerupState == powerupstate.RapidFire)
                            this.calories -= Powerups.RapidFirePowerup.RAPID_FIRE_DAMAGE_LARGE;
                        else
                            this.calories -= SPEAR_MAX_DAMAGE;
                    }
                    else if (other.CurrentSize == Size.Medium)
                    {
                        if (((Spear)other).Owner.powerupState == powerupstate.RapidFire)
                            this.calories -= Powerups.RapidFirePowerup.RAPID_FIRE_DAMAGE_MEDIUM;
                        else
                            this.calories -= SPEAR_MED_DAMAGE;
                    }
                    else if (other.CurrentSize == Size.Small)
                    {
                        if (((Spear)other).Owner.powerupState == powerupstate.RapidFire)
                            this.calories -= Powerups.RapidFirePowerup.RAPID_FIRE_DAMAGE_SMALL;
                        else
                            this.calories -= SPEAR_SMALL_DAMAGE;
                    }

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
                    ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                        new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                        new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                        (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);

                    getHit();

                    isBeingKnockedBack = true;
                    knockbackTime = KNOCKBACK_DURATION;

                    // get physics rotation to p's rotation, 
                    // normalize that, and magnify by the amount
                    Vector2 pRotation = Physics.AngleToVector(other.Rotation);

                    pRotation.Normalize();

                    // give us an acceleration in that direction
                    this.Position += pRotation * Actor.SPEAR_DISPLACEMENT;
                }
            }
            else if (other is Shark)
            {
                if (!other.Bounds.isCollision(this.Bounds))
                    return;

                // gain health
                if (other.CurrState == state.Dying)
                {
                    if (other.Powerup != null)
                        this.calories += 2 * SHARK_CALORIES;
                    else
                        this.calories += SHARK_CALORIES;

                    other.die();
                }
                // take damage
                else if (other.CurrState == state.Dashing)
                {
                    // we must not be dying and we must be colliding with
                    // the mouth of the shark
                    if (CurrState != state.Dying &&
                        this.Bounds.isCollision(((Shark)other).mouthCircle))
                    {
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
                        ParticleManager.Instance.createParticle(ParticleType.HitSpark,
                            new Vector2(this.Position.X + rnd.Next(-20, 20), this.Position.Y + rnd.Next(-20, 20)),
                            new Vector2(0, 0), (float)(rnd.NextDouble() * 6.29f), 0.1f,
                            (float)rnd.NextDouble(), -(float)rnd.NextDouble() * 3, 1, 1 + (float)rnd.NextDouble() * 2f, 1f);

                        getHit();

                        this.calories -= SHARK_DAMAGE;
                    }
                }
            }
            else if (other is Fish)
            {
                if (other.CurrState == state.Moving)
                {
                    AudioManager.getSound("Fish_Eat").Play();
                    this.calories += FISH_CALORIES;
                    other.startDying();

                    if (other.Powerup != null)
                        applyPowerup(other.Powerup);
                }
            }
            else if (other is Penguin)
            {
                Penguin p = (Penguin)other;

                if (p.CurrState == state.MeleeAttack && 
                    this.Bounds.isCollision(p.spearCircle) &&
                    !isBeingKnockedBack)
                {
                    isBeingKnockedBack = true;
                    knockbackTime = KNOCKBACK_DURATION;

                    getHit();

                    this.calories -= p.MeleeDamage;
                        
                    // get physics rotation to p's rotation, 
                    // normalize that, and magnify by the amount
                    Vector2 pRotation = Physics.AngleToVector(p.Rotation);
                        
                    pRotation.Normalize();

                    // give us an acceleration in that direction
                    this.velocity += pRotation * p.KnockbackAmount;
                    this.Position += pRotation * (Actor.MELEE_DISPLACEMENT + p.calories / 10.0f);
                }

            }
        }
    }
}
