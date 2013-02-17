using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM
{
    public class Timer
    {

        public float elapsedsec = 0;

        public void Draw(SpriteBatch spriteBatch,SpriteFont font ,GraphicsDeviceManager graphics)
        {
            spriteBatch.DrawString(font, timestring(), new Vector2(graphics.PreferredBackBufferWidth * 0.05f,
                                                         graphics.PreferredBackBufferHeight * 0.01f), Color.Yellow);
        }


        public void Update(GameTime gametime)
        {
            elapsedsec += (float)gametime.ElapsedGameTime.TotalSeconds;
        }

        public String timestring()
        {
            return string.Format("{0}:{1:00}", (int)(elapsedsec / 60), ((int)(elapsedsec % 60)));
        }

        public int getTimer()
        {
            return (int)elapsedsec;
        }

      }
}
