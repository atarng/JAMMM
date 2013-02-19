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

        private bool applied;
        private float timeAlive;

        protected Actor empowered;

        public Powerup(float duration) 
        {
            this.applied = false;
            this.duration = duration; 
        }

        public void update(float elapsedTime) 
        {
            if (!applied) return;

            timeAlive += elapsedTime;

            if (timeAlive >= duration)
                remove();
        }

        public void apply(Actor a)
        {
            this.empowered = a;
            this.timeAlive = 0.0f;
            this.applied = true;

            onApply();

            this.empowered.onPowerupApplication(this);
        }

        protected virtual void onApply() { }

        public virtual void remove()
        {
            onRemove();

            this.empowered.onPowerupRemoval(this);

            this.empowered = null;
            this.timeAlive = 0.0f;
            this.applied = false;
        }

        protected virtual void onRemove() { }
    }
}
