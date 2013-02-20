using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JAMMM.Powerups
{
    public class SpearDeflectionPowerup : Powerup
    {
        public const int DEFLECTION_RADIUS = 75;

        public SpearDeflectionPowerup(float duration = 10.0f) : base(duration) { }
    }
}
