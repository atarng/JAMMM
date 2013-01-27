using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using JAMMM.Actors;

namespace JAMMM
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private const int FISH_POOL_SIZE = 20;
        private const int SHARK_POOL_SIZE = 2;
        private List<Fish> fishPool;
        private List<Shark> sharkPool;
        private Dictionary<Actor, Actor> collisions;

        // this is our pool of players. we want to add a new player
        // to the list each time a new controller readies up
        private List<Tuple<Penguin, Spear>> players;

        //private AnimatedActorTest testActAnim;
        public static SpriteFont font;

        SoundEffect underwaterTheme;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fishPool = new List<Fish>();
            sharkPool = new List<Shark>();
            Random rng = new Random();
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool.Add(new Fish((float)(200 + rng.NextDouble() * 100), (float)(200 + rng.NextDouble() * 100), 10, 10, 10));
            //for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            //    sharkPool.Add(new Shark());
            sharkPool.Add(new Shark(150, 150, true));
            sharkPool.Add(new Shark(300, 300, false));


            //testActAnim = new AnimatedActorTest(100, 100, 10, 10, 10);
            //testAct = new Actor(100, 200, 10, 10, 10);

            players = new List<Tuple<Penguin, Spear>>();

            collisions = new Dictionary<Actor, Actor>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Peric");

            // load the content for the sprite manager
           
            SpriteManager.addTexture("Shark_Swim", Content.Load<Texture2D>("Sprites/Shark_Swim_80_48"));
            SpriteManager.addTexture("Shark_Eat", Content.Load<Texture2D>("Sprites/Shark_Eat_80_48"));
            SpriteManager.addTexture("Shark_Turn", Content.Load<Texture2D>("Sprites/Shark_Turn_80_48"));
            SpriteManager.addTexture("Shark_Death", Content.Load<Texture2D>("Sprites/Shark_Death_80_48"));
            

            SpriteManager.addTexture("Fish_Swim", Content.Load<Texture2D>("Sprites/Fish_Swim_16_16_Loop"));
            SpriteManager.addTexture("Fish_Death", Content.Load<Texture2D>("Sprites/Fish_Death_16_16"));            
            
            SpriteManager.addTexture("Kelp_Idle", Content.Load<Texture2D>("Sprites/Kelp_Idle"));
            
            underwaterTheme = Content.Load<SoundEffect>("Music/03-underwater");

            // tell each actor to load their content now that the sprite manager has its database
            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
                sharkPool[i].loadContent();
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool[i].loadContent();

            //testActAnim.loadContent();

            underwaterTheme.Play();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void Flock(List<Fish> _boids, GameTime gametime)
        {
            float maxSpeed = _boids[0].MaxVel;
            float maxAccel = _boids[0].MaxAcc;
            float desiredSeparation = 50.0f;
            float neighborRadius = 150.0f;
            float separationFactor = 20.0f;
            float alignmentFactor  = 10.0f;
            float cohesionFactor  = 10.0f;

            float elapsedTime = (float)gametime.ElapsedGameTime.TotalSeconds;            
            for (int i = 0; i < _boids.Count; i++)
            {
                float count = 0;
                float count0 = 0;
                Vector2 cohesion = new Vector2(0, 0);
                Vector2 separation = new Vector2(0, 0);
                Vector2 alignment = new Vector2(0, 0);                

                for (int j = 0; j < _boids.Count; j++)
                {
                    Vector2 vecTo = _boids[j].Position - _boids[i].Position;
                    if (vecTo.Length() < neighborRadius)
                    {
                        cohesion += _boids[j].Position;
                        alignment += _boids[j].Velocity;
                        count++;                        
                    }
                    if (vecTo.Length() > 0 && vecTo.Length() < desiredSeparation)
                    {                        
                        separation -= vecTo;
                        count0++;
                    }
                }

                if (count > 0)
                {
                    cohesion /= count;
                    cohesion = cohesion - _boids[i].Position;
                    alignment /= count;
                    if (alignment.Length() > maxAccel)
                    {
                        alignment.Normalize();
                        alignment *= maxAccel;
                    }
                }
                if (count0 > 0)
                {
                    separation /= count0;
                }
                if (cohesion.Length() > 0)
                {
                    float temp = cohesion.Length();
                    cohesion.Normalize();
                    if (temp < (desiredSeparation + neighborRadius) / 2.0f)
                    {
                        cohesion *= maxSpeed * (temp / neighborRadius);
                    }
                    else
                    {                        
                        cohesion *= maxSpeed;
                    }
                }
                _boids[i].acceleration = cohesion * cohesionFactor + separation * separationFactor + alignment * alignmentFactor;                


            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if (true)
            {
                for (int i = 0; i < FISH_POOL_SIZE; ++i)
                {
                    fishPool[i].update(gameTime);
                    Physics.applyMovement(fishPool[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
                }
            }
            else
            {
                Flock(fishPool, gameTime);
                for (int i = 0; i < FISH_POOL_SIZE; ++i)
                {
                    fishPool[i].update(null);
                    Physics.applyMovement(fishPool[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
                }
            }

            //testAct.update(gameTime);
            //testActAnim.update(gameTime);
            //Physics.applyMovement(testActAnim, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

            foreach (Shark s in sharkPool)
            {
                s.update(gameTime);
                Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, true);
            }

            collisions.Clear();
            for (int i = 0; i < SHARK_POOL_SIZE; i++)
            {
                for (int j = 0; j < SHARK_POOL_SIZE; j++)
                {
                    if (i != j && sharkPool[i].Bounds.isCollision(sharkPool[j].Bounds))
                    {
                        if( collisions.Count == 0 )
                            collisions.Add(sharkPool[i], sharkPool[j]);
                        else if( collisions[sharkPool[j]] != sharkPool[i] )
                            collisions.Add(sharkPool[i], sharkPool[j]);
                    }
                }
            }

            List<Actor> keyList = new List<Actor>(collisions.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                Physics.collide(collisions[keyList[i]],keyList[i]);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            
            Vector2 loc;
            loc.X = 50;
            loc.Y = 50;
            spriteBatch.Begin();
            spriteBatch.DrawString(Game1.font, "time" + gameTime.TotalGameTime.TotalSeconds, loc, Color.Black);
            spriteBatch.End();

            
            // TODO: Add your drawing code here
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool[i].draw(gameTime, spriteBatch);

            //testAct.draw(gameTime, spriteBatch);
            //testActAnim.draw(gameTime, spriteBatch);
            foreach( Shark s in sharkPool )
                s.draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
