using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JAMMM
{
    public class Powerup
    {
        private float duration;
        public float Duration { get { return duration; } set { duration = value; } }

        public Powerup(float duration) 
        {
            this.duration = duration; 
        }
    }
}
