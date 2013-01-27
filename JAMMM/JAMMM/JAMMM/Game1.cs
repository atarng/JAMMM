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
        private Texture2D playerPenguin;
        private Rectangle playerPenguinRectangle;

        private Vector2 player1StartPosition,
                        player2StartPosition,
                        player3StartPosition,
                        player4StartPosition;

        private Vector2 titlePosition;
        private const string titleText = "Underwater Penguin Battle Royale!";
        private const string readyText = "Ready!";
        private const string player1Text = "Player 1";
        private const string player2Text = "Player 2";
        private const string player3Text = "Player 3";
        private const string player4Text = "Player 4";
        private const string player1VictoryText = "Player 1 Wins!";
        private const string player2VictoryText = "Player 2 Wins!";
        private const string player3VictoryText = "Player 3 Wins!";
        private const string player4VictoryText = "Player 4 Wins!";
        private const string caloriesLabelText = "Calories: ";

        private const int FISH_POOL_SIZE = 20;
        private const int SHARK_POOL_SIZE = 2;

        private Dictionary<Actor, Actor> collisions;

        private List<Fish> fishPool;
        private List<Shark> sharkPool;
        private List<Penguin> players;
        private List<Spear> spears;

        private bool isPlayer1Connected, isPlayer2Connected,
                     isPlayer3Connected, isPlayer4Connected;

        private bool isPlayer1Ready, isPlayer2Ready,
                     isPlayer3Ready, isPlayer4Ready;

        private Vector2 player1ReadyTextPosition,
                        player2ReadyTextPosition,
                        player3ReadyTextPosition,
                        player4ReadyTextPosition;

        private Vector2 player1TextPosition,
                        player2TextPosition,
                        player3TextPosition,
                        player4TextPosition;

        private Vector2 player1CalorieTextPosition,
                        player2CalorieTextPosition,
                        player3CalorieTextPosition,
                        player4CalorieTextPosition;

        private Vector2 player1VictoryTextPosition,
                        player2VictoryTextPosition,
                        player3VictoryTextPosition,
                        player4VictoryTextPosition;

        //private AnimatedActorTest testActAnim;
        public static SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fishPool = new List<Fish>();
            sharkPool = new List<Shark>();
            spears = new List<Spear>();

            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool.Add(new Fish());
            //for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            //    sharkPool.Add(new Shark());
            //sharkPool.Add(new Shark(150, 150, true));
            //sharkPool.Add(new Shark(300, 300, false));

            //testActAnim = new AnimatedActorTest(100, 100, 10, 10, 10);
            //testAct = new Actor(100, 200, 10, 10, 10);

            players = new List<Penguin>();

            collisions = new Dictionary<Actor, Actor>();

            isPlayer1Connected = false;
            isPlayer2Connected = false;
            isPlayer3Connected = false;
            isPlayer4Connected = false;

            isPlayer1Ready = false;
            isPlayer2Ready = false;
            isPlayer3Ready = false;
            isPlayer4Ready = false;

            graphics.PreferredBackBufferWidth = 1792;
            graphics.PreferredBackBufferHeight = 1024;

            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            player1StartPosition = new Vector2(width * 0.2f, height * 0.5f);
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
            changeState(GameState.FindingPlayers);

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
            playerPenguin = Content.Load<Texture2D>("Sprites/Penguin_Small_Image");

            // load the content for the sprite manager
            SpriteManager.addTexture("Shark_Swim", Content.Load<Texture2D>("Sprites/Shark_Swim_80_48"));
            SpriteManager.addTexture("Shark_Eat", Content.Load<Texture2D>("Sprites/Shark_Eat_80_48"));
            SpriteManager.addTexture("Shark_Turn", Content.Load<Texture2D>("Sprites/Shark_Turn_80_48"));
            SpriteManager.addTexture("Shark_Death", Content.Load<Texture2D>("Sprites/Shark_Death_80_48"));
            SpriteManager.addTexture("Fish_Swim", Content.Load<Texture2D>("Sprites/Fish_Swim_16_16_Loop"));
            SpriteManager.addTexture("Fish_Death", Content.Load<Texture2D>("Sprites/Fish_Death_16_16"));    
            SpriteManager.addTexture("Kelp_Idle", Content.Load<Texture2D>("Sprites/Kelp_Idle"));
            SpriteManager.addTexture("Penguin_Move_Small", Content.Load<Texture2D>("Sprites/Penguin_small_swim_18_16"));

            // tell each actor to load their content now that the sprite manager has its database
            foreach (Shark s in sharkPool)
                s.loadContent();
            foreach (Fish f in fishPool)
                f.loadContent();
            foreach (Spear s in spears)
                s.loadContent();

            //testActAnim.loadContent();

            // load content dependent initialization
            loadContentDependentInitialization();
        }

        /// <summary>
        /// This is for initializing variables and components that require
        /// all of the assets to be loaded first.
        /// </summary>
        private void loadContentDependentInitialization()
        {
            // set the title position relative to the viewport and font size
            titlePosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                            font.MeasureString(titleText).X / 2.0f,
                            graphics.PreferredBackBufferHeight * 0.1f);

            // set the text positions 
            player1ReadyTextPosition = new Vector2(player1StartPosition.X +
                (playerPenguin.Width) / 2.0f - font.MeasureString(readyText).X / 2.0f,
                player1StartPosition.Y + playerPenguin.Height + 5.0f);
            player2ReadyTextPosition = new Vector2(player2StartPosition.X +
                (playerPenguin.Width) - font.MeasureString(readyText).X / 2.0f,
                player2StartPosition.Y + playerPenguin.Height + 5.0f);
            player3ReadyTextPosition = new Vector2(player3StartPosition.X +
                (playerPenguin.Width) - font.MeasureString(readyText).X / 2.0f,
                player3StartPosition.Y + playerPenguin.Height + 5.0f);
            player4ReadyTextPosition = new Vector2(player4StartPosition.X +
                (playerPenguin.Width) - font.MeasureString(readyText).X / 2.0f,
                player4StartPosition.Y + playerPenguin.Height + 5.0f);

            // set the victory text positions
            player1VictoryTextPosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f - 
                                                     font.MeasureString(player1VictoryText).X / 2.0f,
                                                     graphics.PreferredBackBufferHeight / 2.0f - 
                                                     font.MeasureString(player1VictoryText).Y / 2.0f);
            player2VictoryTextPosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                                                     font.MeasureString(player2VictoryText).X / 2.0f,
                                                     graphics.PreferredBackBufferHeight / 2.0f -
                                                     font.MeasureString(player2VictoryText).Y / 2.0f);
            player3VictoryTextPosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                                                     font.MeasureString(player3VictoryText).X / 2.0f,
                                                     graphics.PreferredBackBufferHeight / 2.0f -
                                                     font.MeasureString(player3VictoryText).Y / 2.0f);
            player4VictoryTextPosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                                                     font.MeasureString(player4VictoryText).X / 2.0f,
                                                     graphics.PreferredBackBufferHeight / 2.0f -
                                                     font.MeasureString(player4VictoryText).Y / 2.0f);

            // set the player name text positions
            player1TextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.2f - 
                                              font.MeasureString(player1Text).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.01f);
            player2TextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.4f -
                                              font.MeasureString(player2Text).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.01f);
            player3TextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.6f -
                                              font.MeasureString(player3Text).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.01f);
            player4TextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.8f -
                                              font.MeasureString(player4Text).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.01f);

            // set the player calorie label text positions
            player1CalorieTextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.2f -
                                              font.MeasureString(caloriesLabelText).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player2CalorieTextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.4f -
                                              font.MeasureString(caloriesLabelText).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player3CalorieTextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.6f -
                                              font.MeasureString(caloriesLabelText).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player4CalorieTextPosition = new Vector2(graphics.PreferredBackBufferWidth * 0.8f -
                                              font.MeasureString(caloriesLabelText).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);

            // set the finding player penguin rectangle
            playerPenguinRectangle = new Rectangle(0, 0, playerPenguin.Width, playerPenguin.Height);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void Flock(List<Fish> _boids)
        {
            float neighborRadius = 30.0f;
            float separation = 1.0f;
            float alignment  = 1.0f;
            float cohesion  = 1.0f;
            List<Fish> boids = _boids;
            for (int i = 0; i < boids.Count; i++)
            {

            }
        }

        /// <summary>
        /// This is called any time we change states. This needs
        /// to do some initialization for that state likely. Just
        /// some logical initialization- no assets.
        /// </summary>
        private void changeState(GameState newState)
        {
            switch (newState)
            {
                case (GameState.FindingPlayers):
                {
                    // play finding players theme

                    break;
                }
                case (GameState.Battle):
                {
                    // play the battle theme

                    // load content and spawn each player
                    foreach (Penguin p in players)
                    {
                        p.loadContent();
                        p.respawn();
                    }

                    // spawn some fishies

                    // spawn some sharkies

                    break;
                }
                case (GameState.Victory):
                {
                    // play victory theme

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
                    // check controller connectivity for each player
                    TryToAddPlayers();

                    // check controller state to ready each player
                    TryToReadyPlayers();

                    // check controller state for player 1 to start the game
                    TryToStartGame();

                    break;
                }
                case (GameState.Battle):
                {
                    // do regular game logic updating
                    foreach (Penguin p in players)
                        p.update(gameTime);
                    
                    // for each fishy, check if was alive last frame and is dead this one
                    // if that is the case, spawn a new fishy

                    // do the same logic for the fishy for the sharks

                    // update the onscreen spears

                    // update any additional effects and stuff that needs it

                    // check if there is only 1 player left. if so, end the game
                    TryToEndGame();

                    break;
                }
                case (GameState.Victory) : 
                {
                    // check if player1 wants to restart the match
                    // TODO: We might want more options here like doing a new
                    // match altogether, but we have plenty of time to figure that out
                    TryToRestartGame();
                    break;
                }
            }

            // TODO: Add your update logic here
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
            {
                fishPool[i].update(gameTime);
                Physics.applyMovement(fishPool[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
            }

            //testAct.update(gameTime);
            //testActAnim.update(gameTime);
            //Physics.applyMovement(testActAnim, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

            foreach (Shark s in sharkPool)
            {
                s.update(gameTime);
                Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, true);
            }

            /*
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
             * */

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
                    players.Add(new Penguin(PlayerIndex.One, player1StartPosition));
                    isPlayer1Connected = true;
                }
            }
            if (!isPlayer2Connected)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Two, player2StartPosition));
                    isPlayer2Connected = true;
                }
            }
            if (!isPlayer3Connected)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Three, player3StartPosition));
                    isPlayer3Connected = true;
                }
            }
            if (!isPlayer4Connected)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Four, player4StartPosition));
                    isPlayer4Connected = true;
                }
            }
        }

        /// <summary>
        /// Readies up a player if they're pressing A on the corresponding controller.
        /// </summary>
        private void TryToReadyPlayers()
        {
            if (isPlayer1Connected && !isPlayer1Ready)
            {
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                {
                    isPlayer1Ready = true;
                }
            }
            if (isPlayer2Connected && !isPlayer2Ready)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.A))
                {
                    isPlayer2Ready = true;
                }
            }
            if (isPlayer3Connected && !isPlayer3Ready)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.A))
                {
                    isPlayer3Ready = true;
                }
            }
            if (isPlayer4Connected && !isPlayer4Ready)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.A))
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
        /// Check if only 1 player is alive. If that is the case,
        /// then change state to victory.
        /// </summary>
        private void TryToEndGame()
        {
            int numAlive = 0;

            foreach (Penguin player in players)
                if (player.IsAlive)
                    numAlive++;

            if (numAlive == 1)
                changeState(GameState.Victory);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                {
                    // draw the finding players screen
                    GraphicsDevice.Clear(Color.DarkSeaGreen);

                    spriteBatch.Begin();

                    // draw the title
                    spriteBatch.DrawString(font, titleText, titlePosition, Color.WhiteSmoke);

                    // draw a penguin for each connected controller
                    if (isPlayer1Connected)
                        spriteBatch.Draw(playerPenguin, player1StartPosition, playerPenguinRectangle, 
                            Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer2Connected)
                        spriteBatch.Draw(playerPenguin, player2StartPosition, playerPenguinRectangle,
                            Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer3Connected)
                        spriteBatch.Draw(playerPenguin, player3StartPosition, playerPenguinRectangle,
                            Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer4Connected)
                        spriteBatch.Draw(playerPenguin, player4StartPosition, playerPenguinRectangle,
                            Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);

                    // draw a green ready text for each readied player
                    if (isPlayer1Ready)
                        spriteBatch.DrawString(font, readyText, player1ReadyTextPosition, Color.Gold);
                    if (isPlayer2Ready)
                        spriteBatch.DrawString(font, readyText, player2ReadyTextPosition, Color.Gold);
                    if (isPlayer3Ready)
                        spriteBatch.DrawString(font, readyText, player3ReadyTextPosition, Color.Gold);
                    if (isPlayer4Ready)
                        spriteBatch.DrawString(font, readyText, player4ReadyTextPosition, Color.Gold);

                    spriteBatch.End();

                    break;
                }
                case (GameState.Battle):
                {
                    // draw background
                    GraphicsDevice.Clear(Color.Blue);

                    spriteBatch.Begin();

                    // draw calories and player text for each player
                    if (isPlayer1Connected)
                    {
                        spriteBatch.DrawString(font, player1Text, player1TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player1CalorieTextPosition, Color.WhiteSmoke);
                        // draw the calories themselves as a string right after that position
                    }
                    if (isPlayer2Connected)
                    {
                        spriteBatch.DrawString(font, player2Text, player2TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player2CalorieTextPosition, Color.WhiteSmoke);
                        // draw the calories themselves as a string right after that position
                    }
                    if (isPlayer3Connected)
                    {
                        spriteBatch.DrawString(font, player3Text, player3TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player3CalorieTextPosition, Color.WhiteSmoke);
                        // draw the calories themselves as a string right after that position
                    }
                    if (isPlayer4Connected)
                    {
                        spriteBatch.DrawString(font, player4Text, player4TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player4CalorieTextPosition, Color.WhiteSmoke);
                        // draw the calories themselves as a string right after that position
                    }

                    spriteBatch.End();

                    // draw each player
                    foreach (Penguin player in players)
                        player.draw(gameTime, spriteBatch);

                    // draw each fish
                    foreach (Fish fish in fishPool)
                        fish.draw(gameTime, spriteBatch);
                    
                    // draw each shark
                    foreach (Shark shark in sharkPool)
                        shark.draw(gameTime, spriteBatch);

                    // draw each spear
                    foreach (Spear spear in spears)
                        spear.draw(gameTime, spriteBatch);

                    break;
                }
                case (GameState.Victory):
                {
                    // draw victory screen splash
                    GraphicsDevice.Clear(Color.Cyan);

                    spriteBatch.Begin();

                    // draw victory text (containing the player)
                    if (players.Count > 0 && players[0].IsAlive)
                    {
                        spriteBatch.DrawString(font, player1VictoryText, player1VictoryTextPosition, Color.Gold);
                    }
                    else if (players.Count > 1 && players[1].IsAlive)
                    {
                        spriteBatch.DrawString(font, player2VictoryText, player2VictoryTextPosition, Color.Gold);
                    }
                    else if (players.Count > 2 && players[2].IsAlive)
                    {
                        spriteBatch.DrawString(font, player3VictoryText, player3VictoryTextPosition, Color.Gold);
                    }
                    else if (players.Count > 3 && players[3].IsAlive)
                    {
                        spriteBatch.DrawString(font, player4VictoryText, player4VictoryTextPosition, Color.Gold);
                    }

                    spriteBatch.End();

                    break;
                }
            }

            //for (int i = 0; i < FISH_POOL_SIZE; ++i)
            //    fishPool[i].draw(gameTime, spriteBatch);

            //testAct.draw(gameTime, spriteBatch);
            //testActAnim.draw(gameTime, spriteBatch);
            //foreach (Shark s in sharkPool)
            //    s.draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
