using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAMMM.Actors;

namespace JAMMM.Powerups
{
    public class RapidFirePowerup : Powerup
    {
        public const float RAPID_FIRE_MELEE_COOLDOWN = 0.1f;
        public const float RAPID_FIRE_FIRE_COOLDOWN = 0.15f;

        public const int RAPID_FIRE_DAMAGE_SMALL  = 5;
        public const int RAPID_FIRE_DAMAGE_MEDIUM = 10;
        public const int RAPID_FIRE_DAMAGE_LARGE  = 15;

        public const int RAPID_FIRE_COST          = 3;

        public RapidFirePowerup(float duration = 10.0f) : base(duration) { }
    }
}
