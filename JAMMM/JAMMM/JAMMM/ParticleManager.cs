﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JAMMM;

namespace JAMMM
{

    public enum ParticleType
    {
        Bubble
    }

    /// <summary>
    /// ParticleManager is a singleton creating and managing particles
    /// </summary>
    class ParticleManager
    {
        private static ParticleManager instance;

        private const int BUBBLE_POOL_SIZE = 2000;

        private int bubblePoolIndex = 0;

        private List<ParticleBubble> bubblePool;

        private ParticleManager()
        {
            bubblePool = new List<ParticleBubble>();
            for (int i = 0; i < BUBBLE_POOL_SIZE; i++)
                bubblePool.Add(new ParticleBubble());


            for (int i = 0; i < BUBBLE_POOL_SIZE; ++i)
                bubblePool[i].loadContent();

        }

        public static ParticleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ParticleManager();
                }
                return instance;
            }
        }


        public void update(GameTime gameTime)
        {
            foreach (ParticleBubble a in bubblePool)
                if(a.isAlive())
                    a.update(gameTime);

        }


        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (ParticleBubble a in bubblePool)
                if(a.isAlive())
                    a.draw(gameTime, spriteBatch);
        }

        public void createParticle(ParticleType type, Vector2 location, Vector2 offsets, float direction, float speed, float size, float growth, float alphablend, float dim, float life)
        {
            switch(type){
                case ParticleType.Bubble:

                    bubblePool[bubblePoolIndex].set(location, offsets, direction, speed, size, growth, alphablend, dim, life);
                    if (++bubblePoolIndex >= BUBBLE_POOL_SIZE)
                        bubblePoolIndex = 0;
                    break;
                default:
                    break;
            }
        }

    }
}