using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAMMM.Actors;

namespace JAMMM.Powerups
{
    public class SpeedBoostPowerup : Powerup
    {
        public const float SPEED_BOOST_SPEED      = 800.0f;
        public const float SPEED_BOOST_DASH_SPEED = 1000.0f;
        public const float SPEED_BOOST_ACCEL      = 1000.0f;
        public const float SPEED_BOOST_DASH_ACCEL = 1500.0f;

        private float maxVel, maxVelDash, maxAcc, maxAccDash;

        public SpeedBoostPowerup(float duration = 10.0f) : base(duration) { }

        protected override void onApply() 
        {
            maxVel      = this.empowered.MaxVel;
            maxVelDash  = this.empowered.MaxVelDash;
            maxAcc      = this.empowered.MaxAcc;
            maxAccDash  = this.empowered.MaxAccDash;

            this.empowered.MaxVel     = SPEED_BOOST_SPEED;
            this.empowered.MaxVelDash = SPEED_BOOST_DASH_SPEED;
            this.empowered.MaxAcc     = SPEED_BOOST_ACCEL;
            this.empowered.MaxAccDash = SPEED_BOOST_DASH_ACCEL;
        }

        protected override void onRemove() 
        {
            this.empowered.MaxVel     = maxVel;
            this.empowered.MaxVelDash = maxVelDash;
            this.empowered.MaxAcc     = maxAcc;
            this.empowered.MaxAccDash = maxAccDash;
        }
    }
}
