using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace JAMMM
{

    enum SoundType {
        Spear_Throw,
        Actor_Hit,
        Actor_Dash
        
    }




    /// <summary>
    /// AudioManager is a singleton controlling the usage
    /// of sounds and music within our game. It should abstract
    /// away all functionality and just allow desiring actors
    /// or the game to play sounds and music and ask what's playing.
    /// </summary>
    
    public class AudioManager
    {

        SoundEffect soundEffect;
        private static Dictionary<string, SoundEffect> database = new Dictionary<string, SoundEffect>();

        public static void addSound(string soundId, SoundEffect snd )      {
            database.Add(soundId, snd);
        }

        public static SoundEffect getSound(string soundId)
        {
            return database[soundId];
        }


    }
}
