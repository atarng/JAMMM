using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JAMMM.Powerups
{
    public class MultishotPowerup : Powerup
    {
        public const int MULTISHOT_SPEAR_COST = 10;

        public MultishotPowerup(float duration = 10.0f) : base(duration) { }
    }
}
