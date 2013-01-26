using System;
using Microsoft.Xna.Framework;

namespace JAMMM
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                //vector test
                Vector2 v = new Vector2(-1, 0);
                Vector2 u = new Vector2(2, 0);

                /*
                Vector2 a = u + v;
                a = u - v;
                a = Vector2.mul(4,  u);
                a = Vector2.div(u,  4);

                Actor actA = new Actor();
                actA.Velocity = v;
                actA.Mass = 1;
                Actor actB = new Actor();
                actB.Velocity = u;
                actB.Mass = 1;

                Vector2[] vs = Physics.collide(actA, actB);
                 *                 */

                game.Run();
            }
        }
    }
#endif
}

