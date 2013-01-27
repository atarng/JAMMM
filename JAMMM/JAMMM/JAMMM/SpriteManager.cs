using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM
{
    /// <summary>
    /// Sprite Manager is really just a library to contain all of our
    /// sprites in the game and anyone can use this to get one. This 
    /// way we don't have any unnecessary extra loading of sprites. 
    /// There will be just one load content at the start of the game. 
    /// </summary>
    public class SpriteManager
    {
        private static Dictionary<string, Texture2D> database;

        public static void addTexture(string textureId, ref Texture2D texture)
        {
            database.Add(textureId, texture);
        }

        public static bool getTexture(string textureId, ref Texture2D tex)
        {
            if (!database.ContainsKey(textureId))
                return false;

            tex = database[textureId];

            return true;
        }
    }
}
