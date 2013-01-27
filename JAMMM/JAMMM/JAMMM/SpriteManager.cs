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
        private static Dictionary<string, Texture2D> database = new Dictionary<string, Texture2D>();

        public static void addTexture(string textureId, Texture2D texture)
        {
            database.Add(textureId, texture);
        }

        public static Texture2D getTexture(string textureId)
        {
            return database[textureId];
        }
    }
}
