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

        private Actor testAct;
        private const int FISH_POOL_SIZE = 20;
        private const int SHARK_POOL_SIZE = 2;
        private List<Fish> fishPool;
        private List<Shark> sharkPool;

        // this is our pool of players. we want to add a new player
        // to the list each time a new controller readies up
        private List<Tuple<Penguin, Spear>> players;

        private AnimatedActorTest testActAnim;
        public static SpriteFont font;

        SoundEffect underwaterTheme;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fishPool = new List<Fish>();
            sharkPool = new List<Shark>();

            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool.Add(new Fish());
            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
                sharkPool.Add(new Shark());

            testActAnim = new AnimatedActorTest(100, 100, 10, 10, 10);

            players = new List<Tuple<Penguin, Spear>>();
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
            testAct = new Actor(100,100,10,10,10);
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
            SpriteManager.addTexture("Shark_Eat", Content.Load<Texture2D>("Sprites/Shark_Eat_80_48"));

            underwaterTheme = Content.Load<SoundEffect>("Music/03-underwater");

            // tell each actor to load their content now that the sprite manager has its database
            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
                sharkPool[i].loadContent();
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool[i].loadContent();

            testActAnim.loadContent();

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
            //testAct.update(gameTime);
            testActAnim.update(gameTime);
            Physics.applyMovement(testActAnim, (float)gameTime.ElapsedGameTime.TotalSeconds, true);
            //Physics.applyMovement(testAct, (float)gameTime.ElapsedGameTime.TotalSeconds, false);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //testAct.draw(gameTime, spriteBatch);
            testActAnim.draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
