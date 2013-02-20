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

        public SpeedBoostPowerup(float duration = 10.0f) : base(duration) { }
    }
}
