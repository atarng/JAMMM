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
            TransitionIntoBattle,
            Battle,
            Victory
        }



        #region GAME_CONSTANTS

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

        public const string PENGUIN_MOVE_SMALL = "Penguin_Move_Small";
        public const string PENGUIN_MOVE_MEDIUM = "Penguin_Move_Med";
        public const string PENGUIN_MOVE_LARGE = "Penguin_Move_Large";

        public const string PENGUIN_DASH_SMALL = "Penguin_Dash_Small";
        public const string PENGUIN_DASH_MEDIUM = "Penguin_Dash_Med";
        public const string PENGUIN_DASH_LARGE = "Penguin_Dash_Large";

        public const string PENGUIN_DEATH_SMALL = "Penguin_Death_Small";
        public const string PENGUIN_DEATH_MEDIUM = "Penguin_Death_Med";
        public const string PENGUIN_DEATH_LARGE = "Penguin_Death_Large";

        public const string FISH_SWIM = "Fish_Swim";
        public const string FISH_DEATH = "Fish_Death";

        public const string SHARK_SWIM = "Shark_Swim";
        public const string SHARK_EAT = "Shark_Eat";
        public const string SHARK_TURN = "Shark_Turn";
        public const string SHARK_DEATH = "Shark_Death";

        private const float EPSILON = 0.01f;

        private const int FISH_POOL_SIZE = 40;
        private const int SHARK_POOL_SIZE = 2;
        private const int SPEAR_POOL_SIZE = 50;

        private const float SHARK_ATTACK_THRESHOLD = 300;
        private const float SHARK_SPAWN_CLOSENESS_THRESHOLD = 450;
        private const float SHARK_AGGRESS_THRESHOLD = 600;

        public static int WINDOW_WIDTH = 1600;
        public static int WINDOW_HEIGHT = 900;

        private const float slowZoom = 0.001f;
        private const float medZoom = 0.002f;

        private const float SHARK_RESPAWN_TIME = 1.0f;

        private const float BACKGROUND_FADE_DURATION = 1.0f;

        #endregion

        #region GAME_VARIABLES

        Camera2D camera;

        private GameState currentGameState;

        GraphicsDeviceManager graphics;

        private SpriteBatch  spriteBatch;
        private Texture2D    playerPenguin;
        private Texture2D    background;
        private Texture2D    title;
        private Rectangle    playerPenguinRectangle;
        private Rectangle    screenRectangle;

        public Rectangle     gameplayBoundaries;

        private Vector2 player1StartPosition,
                        player2StartPosition,
                        player3StartPosition,
                        player4StartPosition;

        private Vector2 titlePosition;

        private Dictionary<Actor, Actor> collisions;

        private List<Fish>    fishPool;
        private List<Shark>   sharkPool;
        private List<float>   sharkRespawnTimes;
        private List<Penguin> players;
        private List<Spear>   spears;

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

        private Vector2 player1CalorieValuePosition,
                        player2CalorieValuePosition,
                        player3CalorieValuePosition,
                        player4CalorieValuePosition;

        private Vector2 player1VictoryTextPosition,
                        player2VictoryTextPosition,
                        player3VictoryTextPosition,
                        player4VictoryTextPosition;

        static readonly Random rng = new Random(DateTime.Now.Millisecond);

        SoundEffectInstance battleTheme;

        public static SpriteFont font;

        private Color backgroundFadeColor;
        private Color titleFadeColor;

        private Rectangle expandingView = Rectangle.Empty;

        private float fadeTime;

        private bool hasBeenCompletedOnce = false;

#endregion



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fishPool  = new List<Fish>();
            sharkPool = new List<Shark>();
            spears    = new List<Spear>();
            sharkRespawnTimes = new List<float>();

            players = new List<Penguin>();

            collisions = new Dictionary<Actor, Actor>();

            camera = new Camera2D(this);

            isPlayer1Connected = false;
            isPlayer2Connected = false;
            isPlayer3Connected = false;
            isPlayer4Connected = false;

            isPlayer1Ready = false;
            isPlayer2Ready = false;
            isPlayer3Ready = false;
            isPlayer4Ready = false;

            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            screenRectangle = new Rectangle(0, 0, width, height);

            camera.move(new Vector2(width / 2.0f, height / 2.0f));

            player1StartPosition = new Vector2(width * 0.2f, height * 0.2f);
            player2StartPosition = new Vector2(width * 0.2f, height * 0.8f);
            player3StartPosition = new Vector2(width * 0.8f,  height * 0.2f);
            player4StartPosition = new Vector2(width * 0.8f,  height * 0.8f);
        }



        protected override void Initialize()
        {
            changeState(GameState.FindingPlayers);
            Random rnd = new Random();

            createObjectPools();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Peric");
            playerPenguin = Content.Load<Texture2D>("Sprites/Penguin_Small_Image");
            background = Content.Load<Texture2D>("Sprites/Background");
            title = Content.Load<Texture2D>("Sprites/title");

            AudioManager.addSound("Spear_Throw", Content.Load<SoundEffect>("Sounds/sound_5"));
            AudioManager.addSound("Actor_Dash", Content.Load<SoundEffect>("Sounds/sound_3"));
            AudioManager.addSound("Actor_Hit", Content.Load<SoundEffect>("Sounds/hit_3"));
            AudioManager.addSound("Fish_Eat", Content.Load<SoundEffect>("Sounds/hit_1"));
            AudioManager.addSound("Death_Penguin", Content.Load<SoundEffect>("Sounds/death_penguin"));
            AudioManager.addSound("Battle_Theme", Content.Load<SoundEffect>("Music/battletheme"));
            AudioManager.addSound("Ready_Sound", Content.Load<SoundEffect>("Sounds/ready"));
            battleTheme = AudioManager.getSound("Battle_Theme").CreateInstance();

            // load the content for the sprite manager
            SpriteManager.addTexture(SHARK_SWIM, Content.Load<Texture2D>("Sprites/Shark_Swim_80_48"));
            SpriteManager.addTexture(SHARK_EAT, Content.Load<Texture2D>("Sprites/Shark_Eat_80_48"));
            SpriteManager.addTexture(SHARK_TURN, Content.Load<Texture2D>("Sprites/Shark_Turn_80_48"));
            SpriteManager.addTexture(SHARK_DEATH, Content.Load<Texture2D>("Sprites/Shark_Death_80_48"));

            SpriteManager.addTexture(FISH_SWIM, Content.Load<Texture2D>("Sprites/Fish_Swim_16_16_Loop"));
            SpriteManager.addTexture(FISH_DEATH, Content.Load<Texture2D>("Sprites/Fish_Death_16_16"));
            SpriteManager.addTexture("Kelp_Idle", Content.Load<Texture2D>("Sprites/Kelp_Idle"));

            #region COLORCODEDPENGUINS
            SpriteManager.addTexture(PENGUIN_MOVE_SMALL, Content.Load<Texture2D>("Sprites/Penguin_small_swim_18_16"));
            SpriteManager.addTexture(PENGUIN_MOVE_MEDIUM, Content.Load<Texture2D>("Sprites/Penguin_med_swim_24_24"));
            SpriteManager.addTexture(PENGUIN_MOVE_LARGE, Content.Load<Texture2D>("Sprites/Penguin_fat_swim_32_32"));

            SpriteManager.addTexture(PENGUIN_DASH_SMALL, Content.Load<Texture2D>("Sprites/Penguin_small_dash_64_64"));
            SpriteManager.addTexture(PENGUIN_DASH_MEDIUM, Content.Load<Texture2D>("Sprites/Penguin_med_dash_96_96"));
            SpriteManager.addTexture(PENGUIN_DASH_LARGE, Content.Load<Texture2D>("Sprites/Penguin_fat_dash_128_128"));

            SpriteManager.addTexture(PENGUIN_DEATH_SMALL, Content.Load<Texture2D>("Sprites/Penguin_small_dead_64_64"));
            SpriteManager.addTexture(PENGUIN_DEATH_MEDIUM, Content.Load<Texture2D>("Sprites/Penguin_med_dead_96_96"));
            SpriteManager.addTexture(PENGUIN_DEATH_LARGE, Content.Load<Texture2D>("Sprites/Penguin_fat_dead_128_128"));

            SpriteManager.addTexture(PENGUIN_MOVE_SMALL + "_r", Content.Load<Texture2D>("Sprites/Penguin_small_swim_18_16" + "_r"));
            SpriteManager.addTexture(PENGUIN_MOVE_MEDIUM + "_r", Content.Load<Texture2D>("Sprites/Penguin_med_swim_24_24" + "_r"));
            SpriteManager.addTexture(PENGUIN_MOVE_LARGE + "_r", Content.Load<Texture2D>("Sprites/Penguin_fat_swim_32_32" + "_r"));

            SpriteManager.addTexture(PENGUIN_DASH_SMALL + "_r", Content.Load<Texture2D>("Sprites/Penguin_small_dash_64_64" + "_r"));
            SpriteManager.addTexture(PENGUIN_DASH_MEDIUM + "_r", Content.Load<Texture2D>("Sprites/Penguin_med_dash_96_96" + "_r"));
            SpriteManager.addTexture(PENGUIN_DASH_LARGE + "_r", Content.Load<Texture2D>("Sprites/Penguin_fat_dash_128_128" + "_r"));

            SpriteManager.addTexture(PENGUIN_DEATH_SMALL + "_r", Content.Load<Texture2D>("Sprites/Penguin_small_dead_64_64" + "_r"));
            SpriteManager.addTexture(PENGUIN_DEATH_MEDIUM + "_r", Content.Load<Texture2D>("Sprites/Penguin_med_dead_96_96" + "_r"));
            SpriteManager.addTexture(PENGUIN_DEATH_LARGE + "_r", Content.Load<Texture2D>("Sprites/Penguin_fat_dead_128_128" + "_r"));

            SpriteManager.addTexture(PENGUIN_MOVE_SMALL + "_p", Content.Load<Texture2D>("Sprites/Penguin_small_swim_18_16" + "_p"));
            SpriteManager.addTexture(PENGUIN_MOVE_MEDIUM + "_p", Content.Load<Texture2D>("Sprites/Penguin_med_swim_24_24" + "_p"));
            SpriteManager.addTexture(PENGUIN_MOVE_LARGE + "_p", Content.Load<Texture2D>("Sprites/Penguin_fat_swim_32_32" + "_p"));

            SpriteManager.addTexture(PENGUIN_DASH_SMALL + "_p", Content.Load<Texture2D>("Sprites/Penguin_small_dash_64_64" + "_p"));
            SpriteManager.addTexture(PENGUIN_DASH_MEDIUM + "_p", Content.Load<Texture2D>("Sprites/Penguin_med_dash_96_96" + "_p"));
            SpriteManager.addTexture(PENGUIN_DASH_LARGE + "_p", Content.Load<Texture2D>("Sprites/Penguin_fat_dash_128_128" + "_p"));

            SpriteManager.addTexture(PENGUIN_DEATH_SMALL + "_p", Content.Load<Texture2D>("Sprites/Penguin_small_dead_64_64" + "_p"));
            SpriteManager.addTexture(PENGUIN_DEATH_MEDIUM + "_p", Content.Load<Texture2D>("Sprites/Penguin_med_dead_96_96" + "_p"));
            SpriteManager.addTexture(PENGUIN_DEATH_LARGE + "_p", Content.Load<Texture2D>("Sprites/Penguin_fat_dead_128_128" + "_p"));

            SpriteManager.addTexture(PENGUIN_MOVE_SMALL + "_g", Content.Load<Texture2D>("Sprites/Penguin_small_swim_18_16" + "_g"));
            SpriteManager.addTexture(PENGUIN_MOVE_MEDIUM + "_g", Content.Load<Texture2D>("Sprites/Penguin_med_swim_24_24" + "_g"));
            SpriteManager.addTexture(PENGUIN_MOVE_LARGE + "_g", Content.Load<Texture2D>("Sprites/Penguin_fat_swim_32_32" + "_g"));

            SpriteManager.addTexture(PENGUIN_DASH_SMALL + "_g", Content.Load<Texture2D>("Sprites/Penguin_small_dash_64_64" + "_g"));
            SpriteManager.addTexture(PENGUIN_DASH_MEDIUM + "_g", Content.Load<Texture2D>("Sprites/Penguin_med_dash_96_96" + "_g"));
            SpriteManager.addTexture(PENGUIN_DASH_LARGE + "_g", Content.Load<Texture2D>("Sprites/Penguin_fat_dash_128_128" + "_g"));

            SpriteManager.addTexture(PENGUIN_DEATH_SMALL + "_g", Content.Load<Texture2D>("Sprites/Penguin_small_dead_64_64" + "_g"));
            SpriteManager.addTexture(PENGUIN_DEATH_MEDIUM + "_g", Content.Load<Texture2D>("Sprites/Penguin_med_dead_96_96" + "_g"));
            SpriteManager.addTexture(PENGUIN_DEATH_LARGE + "_g", Content.Load<Texture2D>("Sprites/Penguin_fat_dead_128_128" + "_g"));
            #endregion

            SpriteManager.addTexture("Particle_Bubble_1", Content.Load<Texture2D>("Sprites/PFX_Bubble_16_16"));
            SpriteManager.addTexture("Particle_Bubble_2", Content.Load<Texture2D>("Sprites/PFX_Bubble_24_24"));
            SpriteManager.addTexture("Particle_Bubble_3", Content.Load<Texture2D>("Sprites/PFX_Bubble_36_36"));
            SpriteManager.addTexture("Particle_Bubble_4", Content.Load<Texture2D>("Sprites/PFX_Bubble_56_56"));

            SpriteManager.addTexture("PFX_Beam", Content.Load<Texture2D>("Sprites/PFX_Beam"));
            SpriteManager.addTexture("PFX_FireSplosion", Content.Load<Texture2D>("Sprites/PFX_FireSplosion"));
            SpriteManager.addTexture("PFX_Burst", Content.Load<Texture2D>("Sprites/PFX_Burst"));

            SpriteManager.addTexture("Spear", Content.Load<Texture2D>("Sprites/spear_move_128_48"));

            foreach (Shark s in sharkPool)
                s.loadContent();
            foreach (Fish f in fishPool)
                f.loadContent();
            foreach (Spear s in spears)
                s.loadContent();

            // load content dependent initialization
            loadContentDependentInitialization();
        }

        protected override void UnloadContent() {}

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // the basic game logic outer switch
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                    {
                        // check controller connectivity for each player
                        TryToAddPlayers();

                        // check controller state to ready each player
                        TryToReadyPlayers();

                        // check controller state for player 1 to start the game
                        TryToStartGame();

                        break;
                    }
                case (GameState.TransitionIntoBattle):
                    {
                        fadeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (fadeTime >= BACKGROUND_FADE_DURATION)
                            changeState(GameState.Battle);
                        // update the fade color
                        else
                        {
                            float fadeRatio = (fadeTime / BACKGROUND_FADE_DURATION);

                            backgroundFadeColor.A = (byte)(50 + (int)(fadeRatio * 205));

                            titleFadeColor.A = (byte)(255 - fadeRatio * 255);
                            titleFadeColor.R = (byte)(255 - fadeRatio * 255);
                            titleFadeColor.G = (byte)(255 - fadeRatio * 255);
                            titleFadeColor.B = (byte)(255 - fadeRatio * 255);

                            expandingView.X = screenRectangle.X +
                                (int)((camera.maxView.X - screenRectangle.X) * fadeRatio);
                            expandingView.Y = screenRectangle.Y +
                                (int)((camera.maxView.Y - screenRectangle.Y) * fadeRatio);
                            expandingView.Width = screenRectangle.Width +
                                (int)((camera.maxView.Width - screenRectangle.Width) * fadeRatio);
                            expandingView.Height = screenRectangle.Height +
                                (int)((camera.maxView.Height - screenRectangle.Height) * fadeRatio);
                        }

                        break;
                    }
                case (GameState.Battle):
                    {
                        // do regular game logic updating each player
                        for (int i = 0; i < players.Count; i++)
                        {
                            players[i].update(gameTime);
                            Physics.applyMovement(players[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
                            TrySpear(players[i], i);
                            keepInBounds(players[i]);
                        }

                        // for each fishy, check if was alive last frame and is dead this one
                        // if that is the case, spawn a new fishy
                        foreach (Fish f in fishPool)
                        {
                            f.update(gameTime);
                            Physics.applyMovement(f, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                            if (!f.IsAlive)
                                f.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

                            keepInBounds(f);
                        }

                        for (int i = 0; i < SHARK_POOL_SIZE; ++i)
                        {
                            Shark s = sharkPool.ElementAt(i);

                            s.update(gameTime);

                            if (s.CurrState != Actor.state.Dying)
                            {
                                TryToAggressTowardPlayers(s);
                                TryToAttackPlayers(s);
                            }

                            Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                            if (!s.IsAlive)
                            {
                                sharkRespawnTimes[i] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                                if (sharkRespawnTimes[i] >= SHARK_RESPAWN_TIME)
                                {
                                    sharkRespawnTimes[i] = 0.0f;

                                    s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

                                    while (isNearAPlayer(s))
                                        s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));
                                }
                            }

                            keepInBounds(s);
                        }

                        // update spears
                        foreach (Spear s in spears)
                        {
                            s.update(gameTime);
                            Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, false);

                            if (s.IsAlive && isOffScreen(s))
                                s.die();
                        }

                        // collision detection and resolution
                        detectCollisions();
                        resolveCollisions();

                        // update particles
                        ParticleManager.Instance.update(gameTime);

                        // check sfx
                        if (battleTheme.State == SoundState.Stopped)
                            battleTheme.Play();

                        // check if there is only 1 player left. if so, end the game
                        TryToEndGame();

                        updateCamera(gameTime);
                        camera.updateBounds();

                        break;
                    }
                case (GameState.Victory):
                    {
                        // check if player1 wants to restart the match
                        // TODO: We might want more options here like doing a new
                        // match altogether, but we have plenty of time to figure that out
                        TryToRestartGame();
                        break;
                    }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                    {
                        GraphicsDevice.Clear(Color.DarkSeaGreen);

                        spriteBatch.Begin();
                        //
                        spriteBatch.Draw(background, screenRectangle, new Color(255, 255, 255, 50));
                        spriteBatch.Draw(title, titlePosition, Color.White);

                        foreach (Penguin p in players)
                            p.draw(gameTime, spriteBatch);

                        if (isPlayer1Ready)
                            spriteBatch.DrawString(font, readyText, player1ReadyTextPosition, Color.Green);
                        if (isPlayer2Ready)
                            spriteBatch.DrawString(font, readyText, player2ReadyTextPosition, Color.Green);
                        if (isPlayer3Ready)
                            spriteBatch.DrawString(font, readyText, player3ReadyTextPosition, Color.Green);
                        if (isPlayer4Ready)
                            spriteBatch.DrawString(font, readyText, player4ReadyTextPosition, Color.Green);
                        //
                        spriteBatch.End();

                        break;
                    }
                case (GameState.TransitionIntoBattle):
                    {
                        GraphicsDevice.Clear(Color.DarkSeaGreen);

                        spriteBatch.Begin();

                        spriteBatch.Draw(background, expandingView, backgroundFadeColor);

                        spriteBatch.Draw(title, titlePosition, titleFadeColor);

                        foreach (Penguin p in players)
                            p.draw(gameTime, spriteBatch);

                        spriteBatch.End();

                        break;
                    }
                case (GameState.Battle):
                    {
                        GraphicsDevice.Clear(Color.Blue);

                        spriteBatch.Begin(SpriteSortMode.BackToFront,
                            BlendState.AlphaBlend, null, null, null, null,
                            camera.getTransformation());

                        spriteBatch.Draw(background, camera.maxView, Color.White);

                        foreach (Penguin p in players)
                        {
                            p.draw(gameTime, spriteBatch);

                            if (p.CurrState != Actor.state.Dying &&
                                p.IsAlive)
                                spriteBatch.DrawString(Game1.font, "Calories: " + p.Calories,
                                new Vector2(p.Position.X - 75, p.Position.Y + 60),
                                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        }

                        foreach (Fish fish in fishPool)
                            fish.draw(gameTime, spriteBatch);

                        foreach (Shark s in sharkPool)
                        {
                            s.draw(gameTime, spriteBatch);

                            if (s.CurrState != Actor.state.Dying &&
                                s.IsAlive)
                                spriteBatch.DrawString(Game1.font, "Calories: " + s.Calories,
                                new Vector2(s.Position.X - 75, s.Position.Y + 100),
                                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        }

                        foreach (Spear spear in spears)
                            spear.draw(gameTime, spriteBatch);

                        ParticleManager.Instance.draw(gameTime, spriteBatch);

                        spriteBatch.End();

                        spriteBatch.Begin();

                        if (isPlayer1Connected)
                        {
                            spriteBatch.DrawString(font, player1Text, player1TextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, caloriesLabelText, player1CalorieTextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, players[0].Calories.ToString(), player1CalorieValuePosition, Color.Yellow);
                        }
                        if (isPlayer2Connected)
                        {
                            spriteBatch.DrawString(font, player2Text, player2TextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, caloriesLabelText, player2CalorieTextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, players[1].Calories.ToString(), player2CalorieValuePosition, Color.Yellow);
                        }
                        if (isPlayer3Connected)
                        {
                            spriteBatch.DrawString(font, player3Text, player3TextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, caloriesLabelText, player3CalorieTextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, players[2].Calories.ToString(), player3CalorieValuePosition, Color.Yellow);
                        }
                        if (isPlayer4Connected)
                        {
                            spriteBatch.DrawString(font, player4Text, player4TextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, caloriesLabelText, player4CalorieTextPosition, Color.WhiteSmoke);
                            spriteBatch.DrawString(font, players[3].Calories.ToString(), player4CalorieValuePosition, Color.Yellow);
                        }

                        spriteBatch.End();

                        break;
                    }
                case (GameState.Victory):
                    {
                        GraphicsDevice.Clear(Color.DarkSeaGreen);

                        spriteBatch.Begin();

                        if (players.Count > 0 && players[0].CurrState != Actor.state.Dying)
                        {
                            spriteBatch.DrawString(font, player1VictoryText, player1VictoryTextPosition, Color.Gold);
                        }
                        else if (players.Count > 1 && players[1].CurrState != Actor.state.Dying)
                        {
                            spriteBatch.DrawString(font, player2VictoryText, player2VictoryTextPosition, Color.Gold);
                        }
                        else if (players.Count > 2 && players[2].CurrState != Actor.state.Dying)
                        {
                            spriteBatch.DrawString(font, player3VictoryText, player3VictoryTextPosition, Color.Gold);
                        }
                        else if (players.Count > 3 && players[3].CurrState != Actor.state.Dying)
                        {
                            spriteBatch.DrawString(font, player4VictoryText, player4VictoryTextPosition, Color.Gold);
                        }

                        spriteBatch.End();

                        break;
                    }
            }

            base.Draw(gameTime);
        }



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
                (playerPenguin.Width) / 2.0f - font.MeasureString(readyText).X / 2.0f,
                player2StartPosition.Y + playerPenguin.Height + 5.0f);
            player3ReadyTextPosition = new Vector2(player3StartPosition.X +
                (playerPenguin.Width) / 2.0f - font.MeasureString(readyText).X / 2.0f,
                player3StartPosition.Y + playerPenguin.Height + 5.0f);
            player4ReadyTextPosition = new Vector2(player4StartPosition.X +
                (playerPenguin.Width) / 2.0f - font.MeasureString(readyText).X / 2.0f,
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

            // set the player calorie value positions
            player1CalorieValuePosition = new Vector2(graphics.PreferredBackBufferWidth * 0.2f +
                                              font.MeasureString(caloriesLabelText).X / 2.0f + 5.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player2CalorieValuePosition = new Vector2(graphics.PreferredBackBufferWidth * 0.4f +
                                              font.MeasureString(caloriesLabelText).X / 2.0f + 5.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player3CalorieValuePosition = new Vector2(graphics.PreferredBackBufferWidth * 0.6f +
                                              font.MeasureString(caloriesLabelText).X / 2.0f + 5.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);
            player4CalorieValuePosition = new Vector2(graphics.PreferredBackBufferWidth * 0.8f +
                                              font.MeasureString(caloriesLabelText).X / 2.0f + 5.0f,
                                              graphics.PreferredBackBufferHeight * 0.03f);

            // set the finding player penguin rectangle
            playerPenguinRectangle = new Rectangle(0, 0, playerPenguin.Width, playerPenguin.Height);

            camera.initialize();
            camera.updateBounds();
            gameplayBoundaries = camera.maxView;
        }



        private void createObjectPools()
        {
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
                fishPool.Add(new Fish());

            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            {
                sharkPool.Add(new Shark());
                sharkRespawnTimes.Add(0.0f);
            }

            for (int i = 0; i < SPEAR_POOL_SIZE; ++i)
                spears.Add(new Spear());
        }



        private void updateCamera(GameTime gameTime)
        {
            TryZoomOut((float)gameTime.ElapsedGameTime.TotalSeconds);
            TryZoomIn((float)gameTime.ElapsedGameTime.TotalSeconds);
        }



        private void changeState(GameState newState)
        {
            switch (newState)
            {
                case (GameState.FindingPlayers):
                {
                    onEnteringFindingPlayers();
                    break;
                }
                case (GameState.TransitionIntoBattle):
                {
                    onEnteringTransitionIntoBattle();
                    break;
                }
                case (GameState.Battle):
                {
                    onEnteringBattle();
                    break;
                }
                case (GameState.Victory):
                {
                    onEnteringVictory();
                    break;
                }
            }

            this.currentGameState = newState;
        }

        private void onEnteringFindingPlayers()
        {
            if (hasBeenCompletedOnce)
            {
                if (isPlayer1Connected) isPlayer1Ready = false;
                if (isPlayer2Connected) isPlayer2Ready = false;
                if (isPlayer3Connected) isPlayer3Ready = false;
                if (isPlayer4Connected) isPlayer4Ready = false;

                foreach (Penguin p in players)
                {
                    p.setNewStartingPosition(getRandomPositionWithinBounds(camera.spawnView));
                    p.resetProperties();
                }

                Rectangle titleBounds = title.Bounds;

                titleBounds.X = (int)titlePosition.X;
                titleBounds.Y = (int)titlePosition.Y;

                foreach (Penguin p in players)
                    while (p.getBufferedRectangleBounds(0).Intersects(titleBounds) ||
                           isNearAnotherPlayer(p))
                        p.setNewStartingPosition(getRandomPositionWithinBounds(camera.spawnView));

                if (isPlayer1Connected)
                {
                    player1ReadyTextPosition = players[0].Position;
                    player1ReadyTextPosition.Y += players[0].getBufferedRectangleBounds(0).Height / 2.0f;
                    player1ReadyTextPosition.X -= players[0].getBufferedRectangleBounds(0).Width / 2.0f;
                }
                if (isPlayer2Connected)
                {
                    player2ReadyTextPosition = players[1].Position;
                    player2ReadyTextPosition.Y += players[1].getBufferedRectangleBounds(0).Height / 2.0f;
                    player2ReadyTextPosition.X -= players[1].getBufferedRectangleBounds(0).Width / 2.0f;
                }
                if (isPlayer3Connected)
                {
                    player3ReadyTextPosition = players[2].Position;
                    player3ReadyTextPosition.Y += players[2].getBufferedRectangleBounds(0).Height / 2.0f;
                    player3ReadyTextPosition.X -= players[2].getBufferedRectangleBounds(0).Width / 2.0f;
                }
                if (isPlayer4Connected)
                {
                    player4ReadyTextPosition = players[3].Position;
                    player4ReadyTextPosition.Y += players[3].getBufferedRectangleBounds(0).Height / 2.0f;
                    player4ReadyTextPosition.X -= players[3].getBufferedRectangleBounds(0).Width / 2.0f;
                }
            }
        }

        private void onEnteringTransitionIntoBattle()
        {
            backgroundFadeColor.R = 255;
            backgroundFadeColor.G = 255;
            backgroundFadeColor.B = 255;
            backgroundFadeColor.A = 50;

            titleFadeColor = Color.White;

            fadeTime = 0.0f;
        }

        private void onEnteringBattle()
        {
            // play the battle theme
            if (battleTheme.State != SoundState.Playing)
                battleTheme.Play();

            // kill all the particles
            ParticleManager.Instance.killAllHitParticles();

            // zoom back to the default
            camera.resetZoom();

            // kill all the spears
            foreach (Spear s in spears)
                s.die();

            // respawn the players
            foreach (Penguin p in players)
                p.respawn();

            // spawn some fishies
            foreach (Fish p in fishPool)
                p.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

            // spawn some sharkies
            foreach (Shark s in sharkPool)
            {
                s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

                while (isNearAPlayer(s))
                    s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));
            }
        }

        private void onEnteringVictory()
        {

        }



        private Vector2 getRandomPositionWithinBounds(Rectangle bounds)
        {
            Vector2 newPos = Vector2.Zero;

            newPos.X = (float)bounds.Left
                + (float)(rng.NextDouble() * (double)(bounds.Width));

            newPos.Y = (float)bounds.Top
                + (float)(rng.NextDouble() * (double)(bounds.Height));

            return newPos;
        }

        private bool isOffScreen(Actor a)
        {
            Rectangle aBounds = a.getBufferedRectangleBounds(0);

            return !gameplayBoundaries.Contains(aBounds) &&
                   !gameplayBoundaries.Intersects(aBounds);
        }

        private void keepInBounds(Actor a)
        {
            Rectangle aBounds = a.getBufferedRectangleBounds(0);

            if (!gameplayBoundaries.Contains(aBounds))
            {
                // 1.) determine the wall(s) with which we are colliding
                // 3.) ensure position within bounds
                // 2.) zero velocity and acceleration along those axes
                if (aBounds.Left < gameplayBoundaries.Left)
                {
                    a.move((float)Math.Abs(gameplayBoundaries.Left - aBounds.Left) + EPSILON, 0.0f);

                    if (a.velocity.X < 0.0f)
                        a.velocity.X = 0.0f;

                    if (a.acceleration.X < 0.0f)
                        a.acceleration.X = 0.0f;
                }

                if (aBounds.Right > gameplayBoundaries.Right)
                {
                    a.move(-((float)Math.Abs(gameplayBoundaries.Right - aBounds.Right) + EPSILON), 0.0f);

                    if (a.velocity.X > 0.0f)
                        a.velocity.X = 0.0f;

                    if (a.acceleration.X > 0.0f)
                        a.acceleration.X = 0.0f;
                }

                if (aBounds.Top < gameplayBoundaries.Top)
                {
                    a.move(0.0f, (float)Math.Abs(gameplayBoundaries.Top - aBounds.Top) + EPSILON);

                    if (a.velocity.Y < 0.0f)
                        a.velocity.Y = 0.0f;

                    if (a.acceleration.Y < 0.0f)
                        a.acceleration.Y = 0.0f;
                }

                if (aBounds.Bottom > gameplayBoundaries.Bottom)
                {
                    a.move(0.0f, -((float)Math.Abs(gameplayBoundaries.Bottom - aBounds.Bottom) + EPSILON));

                    if (a.velocity.Y > 0.0f)
                        a.velocity.Y = 0.0f;

                    if (a.acceleration.Y > 0.0f)
                        a.acceleration.Y = 0.0f;
                }
            }
        }

        private bool isNearAPlayer(Actor a)
        {
            Vector2 aPos = a.Position;

            float minDist = 5000.0f;

            float currDist = 0.0f;

            foreach (Penguin p in players)
            {
                currDist = Vector2.Distance(p.Position, aPos);

                if (currDist < minDist)
                    minDist = currDist;
            }

            if (minDist <= SHARK_SPAWN_CLOSENESS_THRESHOLD)
                return true;

            return false;
        }

        private bool isNearAnotherPlayer(Actor a)
        {
            Vector2 aPos = a.Position;

            float minDist = 5000.0f;

            float currDist = 0.0f;

            foreach (Penguin p in players)
            {
                if (p == (Penguin)a) continue;

                currDist = Vector2.Distance(p.Position, aPos);

                if (currDist < minDist)
                    minDist = currDist;
            }

            if (minDist <= SHARK_SPAWN_CLOSENESS_THRESHOLD)
                return true;

            return false;
        }



        private void detectCollisions()
        {
            collisions.Clear();

            // player collisions
            for (int i = 0; i < players.Count; ++i)
            {
                for (int j = 0; j < players.Count; ++j)
                {
                    if (i != j && players[i].IsAlive && players[j].IsAlive &&
                        players[i].Bounds.isCollision(players[j].Bounds))
                    {
                        if (collisions.Count == 0)
                            collisions.Add(players[i], players[j]);
                        else if (collisions[players[j]] != players[i])
                            collisions.Add(players[i], players[j]);
                    }
                }
            }

            // spear collisions
            for (int i = 0; i < spears.Count; i++)
            {
                for (int j = 0; j < sharkPool.Count; j++)
                {
                    if (spears[i].Bounds.isCollision(sharkPool[j].Bounds) &&
                        spears[i].IsAlive && sharkPool[j].IsAlive &&
                        !collisions.ContainsKey(spears[i]))
                        collisions.Add(spears[i], sharkPool[j]);
                }

                for (int j = 0; j < players.Count; j++)
                {
                    if (spears[i].bounds.isCollision(players[j].Bounds)
                        && spears[i].IsAlive && players[j].IsAlive && j != spears[i].Id &&
                        !collisions.ContainsKey(spears[i]))
                        collisions.Add(spears[i], players[j]);
                }
            }

            // fish collisions
            for (int i = 0; i < fishPool.Count; ++i)
            {
                for (int j = 0; j < players.Count; ++j)
                {
                    if (fishPool[i].Bounds.isCollision(players[j].Bounds)
                        && (fishPool[i].CurrState != Actor.state.Dying) &&
                        (fishPool[i].IsAlive) && players[j].IsAlive &&
                        !collisions.ContainsKey(fishPool[i]))
                        collisions.Add(fishPool[i], players[j]);
                }

                for (int j = 0; j < sharkPool.Count; j++)
                {
                    if (fishPool[i].Bounds.isCollision(sharkPool[j].Bounds) &&
                        fishPool[i].IsAlive && sharkPool[j].IsAlive &&
                        !collisions.ContainsKey(fishPool[i]))
                        collisions.Add(fishPool[i], sharkPool[j]);
                }
            }

            // shark collisions
            for (int i = 0; i < sharkPool.Count; ++i)
            {
                for (int j = 0; j < players.Count; ++j)
                {
                    if (sharkPool[i].Bounds.isCollision(players[j].Bounds)
                        && (sharkPool[i].IsAlive) && players[j].IsAlive &&
                        !collisions.ContainsKey(sharkPool[i]))
                        collisions.Add(sharkPool[i], players[j]);
                }

                for (int j = 0; j < fishPool.Count; ++j)
                {
                    if (sharkPool[i].Bounds.isCollision(fishPool[j].Bounds)
                        && (sharkPool[i].IsAlive) && fishPool[j].IsAlive &&
                        !collisions.ContainsKey(sharkPool[i]))
                        collisions.Add(sharkPool[i], fishPool[j]);
                }
            }
        }

        private void resolveCollisions()
        {
            List<Actor> keyList = new List<Actor>(collisions.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                Actor a = collisions[keyList[i]];
                Actor b = keyList[i];

                a.collideWith(b);
                b.collideWith(a);

                if (a is Penguin && b is Penguin)
                {
                    Physics.separate(a, b);
                    Physics.collide(a, b);
                }
                else if (a is Penguin && b is Shark ||
                         a is Shark && b is Penguin)
                {
                    Physics.separate(a, b);
                    Physics.collide(a, b);
                }
            }
        }



        private void TrySpear(Penguin a, int id)
        {
            if (a.Fire)
            {
                a.Fire = false;

                foreach (Spear s in spears)
                {
                    if (!s.IsAlive)
                    {
                        s.setSpawnParameters(a.CurrentSize, id, a);
                        s.spawnAt(a.Position);
                        break;
                    }
                }
            }
        }

        private void TryToAggressTowardPlayers(Shark s)
        {
            if (s.CurrState != Actor.state.Moving)
                return;

            float minDist = 10000.0f;
            float currDist = 0.0f;
            Vector2 nearestPosition = Vector2.Zero;
            Vector2 distance = Vector2.Zero;
            Vector2 vecTowardNearest = Vector2.Zero;

            foreach (Penguin p in players)
            {
                if (!p.IsAlive)
                    continue;

                distance.X = p.Position.X - s.Position.X;
                distance.Y = p.Position.Y - s.Position.Y;

                currDist = distance.Length();

                if (currDist < minDist)
                {
                    minDist = currDist;
                    nearestPosition = p.Position;
                    vecTowardNearest = distance;
                }
            }

            // try to aggress toward the nearest player
            if (minDist <= SHARK_AGGRESS_THRESHOLD 
                && minDist > SHARK_ATTACK_THRESHOLD)
                s.acceleration = vecTowardNearest;
        }

        private void TryToAttackPlayers(Shark s)
        {
            if (s.CurrState != Actor.state.Moving)
                return;

            float minDist = 10000.0f;
            float currDist = 0.0f;
            Vector2 nearestPosition = Vector2.Zero;
            Vector2 distance = Vector2.Zero;
            Vector2 vecTowardNearest = Vector2.Zero;

            foreach (Penguin p in players)
            {
                if (!p.IsAlive)
                    continue;

                distance.X = p.Position.X - s.Position.X;
                distance.Y = p.Position.Y - s.Position.Y;

                currDist = distance.Length();

                if (currDist < minDist)
                {
                    minDist = currDist;
                    nearestPosition = p.Position;
                    vecTowardNearest = distance;
                }
            }

            // try to aggress toward the nearest player
            if (minDist <= SHARK_ATTACK_THRESHOLD
                && s.CurrState != Actor.state.Dashing
                && s.CurrState != Actor.state.Dash)
            {
                s.acceleration = vecTowardNearest;
                ((Shark)s).attack();
            }
        }

        private void TryToAddPlayers()
        {
            if (!isPlayer1Connected)
            {
                if (GamePad.GetState(PlayerIndex.One).IsConnected)
                {
                    Penguin newPlayer = 
                        new Penguin(PlayerIndex.One, player1StartPosition, "");

                    newPlayer.loadContent();
                    newPlayer.respawn();
                    newPlayer.pauseAnimation();

                    players.Add(newPlayer);

                    player1ReadyTextPosition = players[0].Position;
                    player1ReadyTextPosition.Y += players[0].getBufferedRectangleBounds(0).Height / 2.0f;
                    player1ReadyTextPosition.X -= players[0].getBufferedRectangleBounds(0).Width / 2.0f;

                    isPlayer1Connected = true;
                }
            }
            if (!isPlayer2Connected)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                {
                    Penguin newPlayer = 
                        new Penguin(PlayerIndex.Two, player2StartPosition, "_r");

                    newPlayer.loadContent();
                    newPlayer.respawn();
                    newPlayer.pauseAnimation();

                    players.Add(newPlayer);

                    player2ReadyTextPosition = players[1].Position;
                    player2ReadyTextPosition.Y += players[1].getBufferedRectangleBounds(0).Height / 2.0f;
                    player2ReadyTextPosition.X -= players[1].getBufferedRectangleBounds(0).Width / 2.0f;

                    isPlayer2Connected = true;
                }
                
            }
            if (!isPlayer3Connected)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                {
                    Penguin newPlayer = 
                        new Penguin(PlayerIndex.Three, player3StartPosition, "_p");

                    newPlayer.loadContent();
                    newPlayer.respawn();
                    newPlayer.pauseAnimation();

                    players.Add(newPlayer);

                    player3ReadyTextPosition = players[2].Position;
                    player3ReadyTextPosition.Y += players[2].getBufferedRectangleBounds(0).Height / 2.0f;
                    player3ReadyTextPosition.X -= players[2].getBufferedRectangleBounds(0).Width / 2.0f;

                    isPlayer3Connected = true;
                }
            }
            if (!isPlayer4Connected)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                {
                    Penguin newPlayer = 
                        new Penguin(PlayerIndex.Four, player4StartPosition, "_g");

                    newPlayer.loadContent();
                    newPlayer.respawn();
                    newPlayer.pauseAnimation();

                    players.Add(newPlayer);

                    player4ReadyTextPosition = players[3].Position;
                    player4ReadyTextPosition.Y += players[3].getBufferedRectangleBounds(0).Height / 2.0f;
                    player4ReadyTextPosition.X -= players[3].getBufferedRectangleBounds(0).Width / 2.0f;

                    isPlayer4Connected = true;
                }
            }
        }

        private void TryToReadyPlayers()
        {
            if (isPlayer1Connected && !isPlayer1Ready)
            {
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                {
                    isPlayer1Ready = true;
                    AudioManager.getSound("Ready_Sound").Play();
                }
            }
            if (isPlayer2Connected && !isPlayer2Ready)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.A) || Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    isPlayer2Ready = true;
                    AudioManager.getSound("Ready_Sound").Play();
                }
            }
            if (isPlayer3Connected && !isPlayer3Ready)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.A))
                {
                    isPlayer3Ready = true;
                    AudioManager.getSound("Ready_Sound").Play();
                }
            }
            if (isPlayer4Connected && !isPlayer4Ready)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.A))
                {
                    isPlayer4Ready = true;
                    AudioManager.getSound("Ready_Sound").Play();
                }
            }
        }

        private void TryToStartGame()
        {
            if (isPlayer1Ready && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                changeState(GameState.TransitionIntoBattle);
        }

        private void TryToRestartGame()
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
                hasBeenCompletedOnce = true;
                changeState(GameState.FindingPlayers);
            }
        }

        private void TryToEndGame()
        {
            int numAlive = 0;

            foreach (Penguin player in players)
                if (player.CurrState != Actor.state.Dying)
                    numAlive++;

            if (numAlive == 1)
                changeState(GameState.Victory);
        }

        private void TryZoomOut(float gameTime)
        {
            Actor furthestPlayer = null;

            Rectangle currView = camera.View;

            float minDist         = 500.0f,
                  minDistToLeft   = 0.0f,
                  minDistToRight  = 0.0f,
                  minDistToTop    = 0.0f,
                  minDistToBottom = 0.0f;

            float distToLeft = 0.0f,
                  distToRight = 0.0f,
                  distToTop = 0.0f,
                  distToBottom = 0.0f,
                  minActorDist = 0.0f;

            float dPF = 0.0f;

            Vector2 dP = Vector2.Zero;

            Rectangle aBounds = Rectangle.Empty;

            foreach (Penguin p in players)
            {
                aBounds = p.getBufferedRectangleBounds(0);

                distToLeft   = (float)Math.Abs(currView.Left   - aBounds.Left);
                distToRight  = (float)Math.Abs(currView.Right  - aBounds.Right);
                distToTop    = (float)Math.Abs(currView.Top    - aBounds.Top);
                distToBottom = (float)Math.Abs(currView.Bottom - aBounds.Bottom);

                minActorDist = Math.Min(Math.Min(distToLeft, distToRight), 
                                        Math.Min(distToTop, distToBottom));

                if (minActorDist < minDist)
                {
                    furthestPlayer  = p;
                    dP              = p.changeInPosition;
                    minDist         = minActorDist;

                    minDistToLeft   = distToLeft;
                    minDistToRight  = distToRight;
                    minDistToTop    = distToTop;
                    minDistToBottom = distToBottom;
                }
            }

            // this means we can zoom
            if (minDist <= Camera2D.ZOOM_BUFFER)
            {
                // crossing left boundary
                if (minDist == minDistToLeft)
                {
                    if (dP.X >= 0.0f) return;

                    dPF = Math.Abs(dP.X) + Camera2D.ZOOM_BUFFER - minDist;

                    camera.zoomOutByVelocity(dPF, true);
                }
                // crossing right boundary
                else if (minDist == minDistToRight)
                {
                    if (dP.X <= 0.0f) return;

                    dPF = Math.Abs(dP.X) + Camera2D.ZOOM_BUFFER - minDist;

                    camera.zoomOutByVelocity(dPF, true);
                }
                // crossing top boundary
                else if (minDist == minDistToTop)
                {
                    if (dP.Y >= 0.0f) return;

                    dPF = Math.Abs(dP.Y) + Camera2D.ZOOM_BUFFER - minDist;

                    camera.zoomOutByVelocity(dPF, false);
                }
                // crossing bottom boundary
                else
                {
                    if (dP.Y <= 0.0f) return;

                    dPF = Math.Abs(dP.Y) + Camera2D.ZOOM_BUFFER - minDist;

                    camera.zoomOutByVelocity(dPF, false);
                }
            }
        }

        private void TryZoomIn(float gameTime)
        {
            bool allInBoundsMedium = true,
                 allInBoundsSmall = true;

            Rectangle aBounds = Rectangle.Empty;

            foreach (Penguin p in players)
            {
                aBounds = p.getBufferedRectangleBounds(0);

                if (!camera.minView.Contains(aBounds))
                    allInBoundsSmall = false;
                if (!camera.medView.Contains(aBounds))
                    allInBoundsMedium = false;
            }

            // control zoom speed
            if (allInBoundsSmall)
            {
                if (camera.Zoom < 1.0f)
                {
                    camera.zoomIn(slowZoom);
                }
            }
            else if (allInBoundsMedium)
            {
                if (camera.Zoom < 0.75f)
                {
                    camera.zoomIn(medZoom);
                }
            }
        }
    }

    /**
protected void Flock(List<Fish> _boids, GameTime gametime)
{
    float maxSpeed = _boids[0].MaxVel;
    float maxAccel = _boids[0].MaxAcc;
    float desiredSeparation = 50.0f;
    float neighborRadius = 100.0f;
    float separationFactor = 30.0f;
    float alignmentFactor  = 4.0f;
    float cohesionFactor  = 1.0f;

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
            if (vecTo.Length() > 0 && vecTo.Length() < neighborRadius)
            {
                cohesion += _boids[j].Position;
                alignment += _boids[j].Velocity;
                count++;                        
            }
            if (vecTo.Length() > 0 && vecTo.Length() < desiredSeparation)
            {
                Vector2 temp = -vecTo;
                temp.Normalize();
                temp /= vecTo.Length();
                separation += temp;                        
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
            if ( temp < (desiredSeparation + neighborRadius) / 2.0f)
            {
                cohesion *= maxSpeed * (temp / neighborRadius);
            }
            else
            {                        
                cohesion *= maxSpeed;
            }
            cohesion -= _boids[i].velocity;
        }
        _boids[i].acceleration = cohesion * cohesionFactor + separation * separationFactor + alignment * alignmentFactor;                
    }
}
 * */
}
