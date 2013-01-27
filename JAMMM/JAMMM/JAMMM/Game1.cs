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
        enum GameState
        {
            FindingPlayers,
            Battle,
            Victory
        }

        private GameState currentGameState;

        GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Vector2 player1StartPosition,
                        player2StartPosition,
                        player3StartPosition,
                        player4StartPosition;

        private const int FISH_POOL_SIZE = 20;
        private const int SHARK_POOL_SIZE = 2;
        private List<Fish> fishPool;
        private List<Shark> sharkPool;
        private Dictionary<Actor, Actor> collisions;

        // this is our pool of players. we want to add a new player
        // to the list each time a new controller readies up
        private List<Tuple<Penguin, Spear>> players;

        private bool isPlayer1Connected, isPlayer2Connected,
                     isPlayer3Connected, isPlayer4Connected;

        private bool isPlayer1Ready, isPlayer2Ready,
                     isPlayer3Ready, isPlayer4Ready;

        //private AnimatedActorTest testActAnim;
        public static SpriteFont font;

        SoundEffect underwaterTheme;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.ToggleFullScreen();
            Content.RootDirectory = "Content";

            fishPool = new List<Fish>();
            sharkPool = new List<Shark>();

            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool.Add(new Fish());
            //for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            //    sharkPool.Add(new Shark());
            sharkPool.Add(new Shark(150, 150, true));
            sharkPool.Add(new Shark(300, 300, false));

            //testActAnim = new AnimatedActorTest(100, 100, 10, 10, 10);
            //testAct = new Actor(100, 200, 10, 10, 10);

            players = new List<Tuple<Penguin, Spear>>();

            collisions = new Dictionary<Actor, Actor>();

            isPlayer1Connected = false;
            isPlayer2Connected = false;
            isPlayer3Connected = false;
            isPlayer4Connected = false;

            isPlayer1Ready = false;
            isPlayer2Ready = false;
            isPlayer3Ready = false;
            isPlayer4Ready = false;

            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            player1StartPosition = new Vector2(width * 0.2f,  height * 0.5f);
            player2StartPosition = new Vector2(width * 0.4f, height * 0.5f);
            player3StartPosition = new Vector2(width * 0.6f, height * 0.5f);
            player4StartPosition = new Vector2(width * 0.8f, height * 0.5f);
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
            this.currentGameState = GameState.FindingPlayers;

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

        /// <summary>
        /// This is called any time we change states. This needs
        /// to do some initialization for that state likely. Just
        /// some logical initialization- no assets.
        /// </summary>
        private void changeState(GameState newState)
        {
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                {
                    break;
                }
                case (GameState.Battle):
                {
                    // initialize each player? 

                    break;
                }
                case (GameState.Victory):
                {
                    // ???? something???

                    break;
                }
            }

            this.currentGameState = newState;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // the basic game logic outer switch
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers) :
                {
                    TryToAddPlayers();
                    TryToReadyPlayers();
                    TryToStartGame();
                    break;
                }
                case (GameState.Battle):
                {

                    break;
                }
                case (GameState.Victory) : 
                {
                    TryToRestartGame();
                    break;
                }
            }

            // TODO: Add your update logic here
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
        /// Tries to add players to the view.
        /// </summary>
        private void TryToAddPlayers()
        {
            if (!isPlayer1Connected)
            {
                if (GamePad.GetState(PlayerIndex.One).IsConnected)
                {
                    players.Add(new Tuple<Penguin, Spear>(new Penguin(PlayerIndex.One, player1StartPosition), new Spear()));
                    isPlayer1Connected = true;
                }
            }
            if (!isPlayer2Connected)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                {
                    players.Add(new Tuple<Penguin, Spear>(new Penguin(PlayerIndex.Two, player2StartPosition), new Spear()));
                    isPlayer2Connected = true;
                }
            }
            if (!isPlayer3Connected)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                {
                    players.Add(new Tuple<Penguin, Spear>(new Penguin(PlayerIndex.Three, player3StartPosition), new Spear()));
                    isPlayer3Connected = true;
                }
            }
            if (!isPlayer4Connected)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                {
                    players.Add(new Tuple<Penguin, Spear>(new Penguin(PlayerIndex.Four, player4StartPosition), new Spear()));
                    isPlayer4Connected = true;
                }
            }
        }

        /// <summary>
        /// Readies up a player.
        /// </summary>
        private void TryToReadyPlayers()
        {
            if (isPlayer1Connected && !isPlayer1Ready)
            {
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.BigButton))
                {
                    isPlayer1Ready = true;
                }
            }
            if (isPlayer2Connected && !isPlayer2Ready)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.BigButton))
                {
                    isPlayer2Ready = true;
                }
            }
            if (isPlayer3Connected && !isPlayer3Ready)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.BigButton))
                {
                    isPlayer3Ready = true;
                }
            }
            if (isPlayer4Connected && !isPlayer4Ready)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.BigButton))
                {
                    isPlayer4Ready = true;
                }
            }
        }

        /// <summary>
        /// Checks if player 1 pressed start.
        /// </summary>
        private void TryToStartGame()
        {
            if (isPlayer1Ready && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                changeState(GameState.Battle);
        }

        /// <summary>
        /// Checks if player 1 pressed start.
        /// </summary>
        private void TryToRestartGame()
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                changeState(GameState.Battle);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                {
                    // draw the finding players screen

                    // draw a penguin for each connected controller

                    // draw a green blip for each readied player

                    break;
                }
                case (GameState.Battle):
                {
                    // draw everything

                    break;
                }
                case (GameState.Victory):
                {
                    // draw victory screen splash

                    // draw victory text (containing the player)

                    break;
                }
            }

            //testAct.draw(gameTime, spriteBatch);
            //testActAnim.draw(gameTime, spriteBatch);
            foreach( Shark s in sharkPool )
                s.draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
