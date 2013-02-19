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
        public const float RAPID_FIRE_FIRE_COOLDOWN = 0.1f;

        public RapidFirePowerup(float duration = 10.0f) : base(duration) { }

        protected override void onApply() {}

        protected override void onRemove() {}
    }
}
