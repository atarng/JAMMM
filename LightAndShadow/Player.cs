using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LightAndShadow
{
    class Player
    {
        public Vector2 location;
        public double angle;
        public double fireCooldown;
        public double laserCooldown;

        //Bomb stuff
        public float bombCharge;

        public int invincible;

        public float speed;

        public bool mkb = false;
        public float mass;
        public float radius;
        public GamePadState state;
        public KeyboardState kstate;
        public MouseState mstate;

        public bool alive;

        public int playerNum;
        public PlayerIndex index;
        public bool LTriggerPressed = false;
        public Player(float y, int playerNum, int life)
        {
            this.playerNum = playerNum;
            location = new Vector2(100, y);
            setMass(life);
            speed = 12;
            fireCooldown = 0;
            alive = true;
            this.invincible = 120;


            switch(playerNum){
                case 1: index = PlayerIndex.One; break;
                case 2: index = PlayerIndex.Two; break;
                case 3: index = PlayerIndex.Three; break;
                case 4: index = PlayerIndex.Four; break;
            }
            

        }

        public void setDirection(float x, float y)
        {
            angle = Math.Atan2(y, x);
        }

        public void setMass(float m)
        {
            mass = m;
            radius = (float)Math.Sqrt(m);
        }

        public void changeMass(float c)
        {
            setMass(mass + c);
        }

        float vib = 0;
        public void vibration(){
            if (playerNum != 5 && vib >= 0)
            {
                GamePad.SetVibration(index, vib, vib);
                vib -= .25f;
            }
            else if( playerNum != 5)
            {
                GamePad.SetVibration(index, 0, 0);
            }
        }

        public void setVib(float vib){
            this.vib = vib;
        }
    }
}
