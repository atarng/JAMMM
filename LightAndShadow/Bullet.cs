using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightAndShadow
{
    class Bullet
    {
        public Vector2 location;
        public double angle;
        public Player owner;
        public int ownerNum;

        public float speed;
        public float size;
        public int time = 0;

        public bool isLaser = false;
        public Bullet(Vector2 location, double angle, float speed, Player owner)
        {
            this.angle = angle;
            this.location = location;
            this.speed = speed;
            this.owner = owner;
            this.ownerNum = owner.playerNum;
            this.size = 10;
        }

        public Bullet(Vector2 location, Player owner, bool amILaser)
        {
         
            this.location = location;
            this.owner = owner;
            this.ownerNum = owner.playerNum;
            this.size = 20;
            this.isLaser = amILaser;
        }
    }
}
