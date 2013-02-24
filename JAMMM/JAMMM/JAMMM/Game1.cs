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
using JAMMM.Powerups;

namespace JAMMM
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        enum GameState
        {
            FindingPlayers,
            TransitionIntoBattle,
            Battle,
            Victory
        }

        #region GAME_BALANCING_CONSTANTS
        private const int POWERUP_RARITY                    = 8;
        private const int POWERUP_TIME                      = 7;

        private const float SPEAR_DEFLECTION_DURATION       = 10.5f;
        private const float SHARK_REPELLENT_DURATION        = 14.0f;
        private const float SPEED_BOOST_DURATION            = 7.0f;
        private const float RAPID_FIRE_DURATION             = 7.0f;
        private const float MULTI_SHOT_DURATION             = 7.0f;
        private const float CHUM_DURATION                   = 14.0f;

        private const int FISH_POOL_SIZE                    = 60;
        private const int SHARK_POOL_SIZE                   = 0;
        private const int SPEAR_POOL_SIZE                   = 500;

        private const float SHARK_SPAWN_CLOSENESS_THRESHOLD = 350;
        private const float SHARK_RESPAWN_TIME              = 5.0f;

        public const int TIME_EVENT_SHARK                   = 3000;
        #endregion

        #region GAME_CONSTANTS

        private const int MAX_NUM_PLAYERS = 4;

        private const string titleText = "Underwater Penguin Battle Royale!";
        private const string readyText = "Ready!";

        private string[] colorCodes = new string[MAX_NUM_PLAYERS];

        private string[] playerTexts = new string[MAX_NUM_PLAYERS];

        private const string caloriesLabelText = "Calories: ";

        public const string PENGUIN_MOVE_SMALL = "Penguin_Move_Small";
        public const string PENGUIN_MOVE_MEDIUM = "Penguin_Move_Med";
        public const string PENGUIN_MOVE_LARGE = "Penguin_Move_Large";

        public const string PENGUIN_MELEE_SMALL = "Penguin_Melee_Small";
        public const string PENGUIN_MELEE_MEDIUM = "Penguin_Melee_Med";
        public const string PENGUIN_MELEE_LARGE = "Penguin_Melee_Large";

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

        public const string POWERUP_SPEEDBOOST = "Speed_Boost";
        public const string POWERUP_RAPIDFIRE = "Rapid_Fire";
        public const string POWERUP_SHARKREPELLENT = "Shark_Repellent";
        public const string POWERUP_SPEARDEFLECTION = "Spear_Deflection";
        public const string POWERUP_CHUM = "Chum";
        public const string POWERUP_MULTISHOT = "Multi_Shot";
        public const string CHUM_AURA = "Chum_Aura";
        public const string SHARKREPELLENT_AURA = "Shark_Repellent_Aura";
        public const string SPEARDEFLECTION_AURA = "Spear_Deflection_Aura";

        private const float EPSILON = 0.01f;

        private const float BACKGROUND_FADE_DURATION = 1.0f;

        public static int WINDOW_WIDTH = 1600;
        public static int WINDOW_HEIGHT = 900;

        private const int HEALTH_BAR_HEIGHT = 22;
        private const int HEALTH_BAR_WIDTH = 100;
        private const int HEALTH_BAR_THICKNESS = 2;

        private Color POWERUP_COLOR;

        #endregion

        #region GAME_VARIABLES
        Camera2D camera;
        private GameState     currentGameState;

        GraphicsDeviceManager graphics;

        private SpriteBatch   spriteBatch;
        private Texture2D     background;
        private Texture2D     title;
        private Rectangle     screenRectangle;

        public Rectangle      gameplayBoundaries;
        
        private Vector2[] playerStartPositions   = new Vector2[MAX_NUM_PLAYERS];
        private Vector2[] playerPowerupPositions = new Vector2[MAX_NUM_PLAYERS];
        private HealthBar[] healthBars           = new HealthBar[MAX_NUM_PLAYERS];

        private bool[] isPlayerConnected = new bool[MAX_NUM_PLAYERS];
        private bool[] isPlayerReady = new bool[MAX_NUM_PLAYERS];

        private Vector2[] playerReadyTextPositions = new Vector2[MAX_NUM_PLAYERS];
        private Vector2[] playerTextPositions = new Vector2[MAX_NUM_PLAYERS];
        private Vector2[] playerCalorieTextPositions = new Vector2[MAX_NUM_PLAYERS];
        private Vector2[] playerCalorieValuePositions = new Vector2[MAX_NUM_PLAYERS];
        private Vector2[] playerVictoryTextPositions = new Vector2[MAX_NUM_PLAYERS];

        private Vector2                         titlePosition;

        private Dictionary<Actor, List<Actor>> collisions;

        private List<Fish>                     fishPool;
        private List<Shark>                    sharkPool;
        private List<float>                    sharkRespawnTimes;
        private List<Penguin>                  players;
        private List<Spear>                    spears;

        private SpeedBoostPowerup       speedBoost;
        private RapidFirePowerup        rapidFire;
        private SharkRepellentPowerup   sharkRepellent;
        private SpearDeflectionPowerup  spearDeflection;
        private ChumPowerup             chum;
        private MultishotPowerup        multishot;

        static readonly Random rng = new Random(DateTime.Now.Millisecond);
        SoundEffectInstance         battleTheme;

        public static SpriteFont    font;
        public static SpriteFont    bigFont;

        private Color               backgroundFadeColor;
        private Color               titleFadeColor;

        private Rectangle           expandingView = Rectangle.Empty;
        private Rectangle           powerupRectangle;

        private float               fadeTime;

        private bool                hasBeenCompletedOnce = false;

        private Timer               timer1;

        private int                 numExtraSharks;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            POWERUP_COLOR = new Color(255, 255, 255, 200);

            fishPool  = new List<Fish>();
            sharkPool = new List<Shark>();
            spears    = new List<Spear>();
            sharkRespawnTimes = new List<float>();

            players = new List<Penguin>();

            collisions = new Dictionary<Actor, List<Actor>>();

            camera = new Camera2D(this);

            for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
            {
                isPlayerConnected[i] = false;
                isPlayerReady[i] = false;
                playerTexts[i] = "Player " + i.ToString();
            }

            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            screenRectangle = new Rectangle(0, 0, width, height);

            camera.move(new Vector2(width / 2.0f, height / 2.0f));

            playerStartPositions[0] = new Vector2(width * 0.2f, height * 0.2f);
            playerStartPositions[1] = new Vector2(width * 0.2f, height * 0.8f);
            playerStartPositions[2] = new Vector2(width * 0.8f, height * 0.2f);
            playerStartPositions[3] = new Vector2(width * 0.8f, height * 0.8f);

            colorCodes[0] = "";
            colorCodes[1] = "_r";
            colorCodes[2] = "_p";
            colorCodes[3] = "_g";

            powerupRectangle = new Rectangle(0, 0, 48, 48);
        }

        #region INIT_GAME

        protected override void Initialize()
        {
            changeState(GameState.FindingPlayers);
            Random rnd = new Random();

            createObjectPools();

            initializeHealthBars();

            base.Initialize();
        }

        private void initializeHealthBars()
        {
            List<Color> colors = new List<Color>();
            colors.Add(Color.Yellow);
            colors.Add(Color.Green);
            colors.Add(Color.Blue);

            for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
            {
                healthBars[i] = new HealthBar(3, 100, 
                    HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT, HEALTH_BAR_THICKNESS);
                healthBars[i].setColors(colors);
                healthBars[i].updateHealth(100);
            }
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Peric");
            bigFont = Content.Load<SpriteFont>("Miramonte");
            background = Content.Load<Texture2D>("Sprites/Background");
            title = Content.Load<Texture2D>("Sprites/title");

            AudioManager.addSound("Spear_Throw", Content.Load<SoundEffect>("Sounds/sound_5"));
            AudioManager.addSound("Actor_Dash", Content.Load<SoundEffect>("Sounds/sound_3"));
            AudioManager.addSound("Actor_Hit", Content.Load<SoundEffect>("Sounds/hit_3"));
            AudioManager.addSound("Fish_Eat", Content.Load<SoundEffect>("Sounds/hit_1"));
            AudioManager.addSound("Power_Up", Content.Load<SoundEffect>("Sounds/powerup"));
            AudioManager.addSound("Ding", Content.Load<SoundEffect>("Sounds/ding"));
            AudioManager.addSound("Chum_Fart", Content.Load<SoundEffect>("Sounds/chumfart"));

            AudioManager.addSound("Death_Penguin", Content.Load<SoundEffect>("Sounds/death_penguin"));
            AudioManager.addSound("Battle_Theme", Content.Load<SoundEffect>("Music/battletheme"));
            AudioManager.addSound("Ready_Sound", Content.Load<SoundEffect>("Sounds/ready"));

            battleTheme = AudioManager.getSound("Battle_Theme").CreateInstance();

            // load the content for the sprite manager
            SpriteManager.addTexture(POWERUP_SPEEDBOOST, Content.Load<Texture2D>("Sprites/powerup_speedboost"));
            SpriteManager.addTexture(POWERUP_RAPIDFIRE, Content.Load<Texture2D>("Sprites/powerup_rapidfire"));
            SpriteManager.addTexture(POWERUP_SHARKREPELLENT, Content.Load<Texture2D>("Sprites/powerup_sharkrepellent"));
            SpriteManager.addTexture(POWERUP_SPEARDEFLECTION, Content.Load<Texture2D>("Sprites/powerup_speardeflection"));
            SpriteManager.addTexture(POWERUP_CHUM, Content.Load<Texture2D>("Sprites/powerup_chum"));
            SpriteManager.addTexture(POWERUP_MULTISHOT, Content.Load<Texture2D>("Sprites/powerup_multishot"));

            SpriteManager.addTexture(CHUM_AURA, Content.Load<Texture2D>("Sprites/chum_arrow"));

            SpriteManager.addTexture(SHARKREPELLENT_AURA, Content.Load<Texture2D>("Sprites/shark_repellent"));
            SpriteManager.addTexture(SPEARDEFLECTION_AURA, Content.Load<Texture2D>("Sprites/spear_deflector"));

            SpriteManager.addTexture(SHARK_SWIM, Content.Load<Texture2D>("Sprites/Shark_Swim_80_48"));
            SpriteManager.addTexture(SHARK_EAT, Content.Load<Texture2D>("Sprites/Shark_Eat_80_48"));
            SpriteManager.addTexture(SHARK_TURN, Content.Load<Texture2D>("Sprites/Shark_Turn_80_48"));
            SpriteManager.addTexture(SHARK_DEATH, Content.Load<Texture2D>("Sprites/Shark_Death_80_48"));

            SpriteManager.addTexture(FISH_SWIM, Content.Load<Texture2D>("Sprites/Fish_Swim_16_16_Loop"));
            SpriteManager.addTexture(FISH_DEATH, Content.Load<Texture2D>("Sprites/Fish_Death_16_16"));
            SpriteManager.addTexture("Kelp_Idle", Content.Load<Texture2D>("Sprites/Kelp_Idle"));

            #region COLORCODEDPENGUINS
            SpriteManager.addTexture(PENGUIN_MELEE_SMALL, Content.Load<Texture2D>("Sprites/penguin_small_melee"));
            SpriteManager.addTexture(PENGUIN_MELEE_MEDIUM, Content.Load<Texture2D>("Sprites/penguin_med_melee"));
            SpriteManager.addTexture(PENGUIN_MELEE_LARGE, Content.Load<Texture2D>("Sprites/penguin_fat_melee"));

            SpriteManager.addTexture(PENGUIN_MELEE_SMALL + "_r", Content.Load<Texture2D>("Sprites/penguin_small_melee" + "_r"));
            SpriteManager.addTexture(PENGUIN_MELEE_MEDIUM + "_r", Content.Load<Texture2D>("Sprites/penguin_med_melee" + "_r"));
            SpriteManager.addTexture(PENGUIN_MELEE_LARGE + "_r", Content.Load<Texture2D>("Sprites/penguin_fat_melee" + "_r"));

            SpriteManager.addTexture(PENGUIN_MELEE_SMALL + "_p", Content.Load<Texture2D>("Sprites/penguin_small_melee" + "_p"));
            SpriteManager.addTexture(PENGUIN_MELEE_MEDIUM + "_p", Content.Load<Texture2D>("Sprites/penguin_med_melee" + "_p"));
            SpriteManager.addTexture(PENGUIN_MELEE_LARGE + "_p", Content.Load<Texture2D>("Sprites/penguin_fat_melee" + "_p"));

            SpriteManager.addTexture(PENGUIN_MELEE_SMALL + "_g", Content.Load<Texture2D>("Sprites/penguin_small_melee" + "_g"));
            SpriteManager.addTexture(PENGUIN_MELEE_MEDIUM + "_g", Content.Load<Texture2D>("Sprites/penguin_med_melee" + "_g"));
            SpriteManager.addTexture(PENGUIN_MELEE_LARGE + "_g", Content.Load<Texture2D>("Sprites/penguin_fat_melee" + "_g"));

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
            foreach (HealthBar hb in healthBars)
                hb.loadContent(GraphicsDevice);

            // load content dependent initialization
            loadContentDependentInitialization();
        }

        /// <summary>
        /// The positions of the hardcoded text locations for each of the things
        /// on the battle screen and the finding players screen need to have
        /// textures loaded before they're able to be positioned accurately.
        /// 
        /// The camera also needs the graphics device to be available 
        /// before initialization which does not occur until after
        /// base.LoadContent().
        /// </summary>
        private void loadContentDependentInitialization()
        {
            titlePosition = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                            font.MeasureString(titleText).X / 2.0f,
                            graphics.PreferredBackBufferHeight * 0.1f);

            initializePlayerUIPositions();

            camera.initialize();
            camera.updateBounds();
            gameplayBoundaries = camera.maxView;
        }

        private void initializePlayerUIPositions()
        {
            // initialize all the UI positions for each player
            for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
            {
                playerVictoryTextPositions[i] = new Vector2(graphics.PreferredBackBufferWidth / 2.0f -
                                                     bigFont.MeasureString(playerTexts[i] + " Wins!").X / 2.0f,
                                                     graphics.PreferredBackBufferHeight / 2.0f -
                                                     bigFont.MeasureString(playerTexts[i] + " Wins!").Y / 2.0f);

                playerTextPositions[i] = new Vector2(graphics.PreferredBackBufferWidth * ((i + 1) * 0.2f) -
                                              bigFont.MeasureString(playerTexts[i]).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.92f);

                playerPowerupPositions[i] = new Vector2(playerTextPositions[i].X - 12 - powerupRectangle.Width,
                                                 playerTextPositions[i].Y);

                playerCalorieTextPositions[i] = new Vector2(graphics.PreferredBackBufferWidth * ((i + 1) * 0.2f) -
                                              bigFont.MeasureString(caloriesLabelText).X / 2.0f,
                                              graphics.PreferredBackBufferHeight * 0.95f);

                playerCalorieValuePositions[i] = new Vector2(graphics.PreferredBackBufferWidth * ((i + 1) * 0.2f) +
                                              bigFont.MeasureString(caloriesLabelText).X / 2.0f + 5.0f,
                                              graphics.PreferredBackBufferHeight * 0.95f);
            }
        }

        /// <summary>
        /// Creates the object pools for each kind of object
        /// other than the players. For now, the players are
        /// hard-coded.
        /// 
        /// There should never be a new call while the game is
        /// running and not loading because finding memory
        /// on the heap is slow. Thus, we must recycle all of
        /// our objects that can be. 
        /// 
        /// If enough programming effort is used, all objects
        /// in a game can be recycled.
        /// </summary>
        private void createObjectPools()
        {
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
            {
                fishPool.Add(new Fish());
                collisions.Add(fishPool[i], new List<Actor>());
            }

            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            {
                sharkPool.Add(new Shark());
                sharkRespawnTimes.Add(0.0f);
                collisions.Add(sharkPool[i], new List<Actor>());
            }

            for (int i = 0; i < SPEAR_POOL_SIZE; ++i)
            {
                spears.Add(new Spear());
                collisions.Add(spears[i], new List<Actor>());
            }

            speedBoost      = new SpeedBoostPowerup(SPEED_BOOST_DURATION);
            rapidFire       = new RapidFirePowerup(RAPID_FIRE_DURATION);
            sharkRepellent  = new SharkRepellentPowerup(SHARK_REPELLENT_DURATION);
            spearDeflection = new SpearDeflectionPowerup(SPEAR_DEFLECTION_DURATION);
            chum            = new ChumPowerup(CHUM_DURATION);
            multishot       = new MultishotPowerup(MULTI_SHOT_DURATION);
        }

        #endregion

        #region UPDATING_CURRENT_GAME_STATE

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
                        UpdateFindingPlayers(gameTime);
                        break;
                    }
                case (GameState.TransitionIntoBattle):
                    {
                        UpdateTransitionIntoBattle(gameTime);
                        break;
                    }
                case (GameState.Battle):
                    {
                        UpdateBattle(gameTime);
                        break;
                    }
                case (GameState.Victory):
                    {
                        UpdateVictory(gameTime);
                        break;
                    }
            }

            base.Update(gameTime);
        }

        private void UpdateFindingPlayers(GameTime gameTime)
        {
            // check controller connectivity for each player
            TryToAddPlayers();

            // check controller state to ready each player
            TryToReadyPlayers();

            // check controller state for player 1 to start the game
            TryToStartGame();
        }

        private void UpdateTransitionIntoBattle(GameTime gameTime)
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
        }

        private void UpdateBattle(GameTime gameTime)
        {
            timer1.Update(gameTime);

            Rectangle currRect = Rectangle.Empty;
            Vector2 currHBPos    = Vector2.Zero;

            // do regular game logic updating each player
            for (int i = 0; i < players.Count; i++)
            {
                players[i].update(gameTime);
                Physics.applyMovement(players[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
                healthBars[i].updateHealth(players[i].Calories);

                currRect = players[i].getBufferedRectangleBounds(0);
                currHBPos.X = players[i].Position.X - (float)healthBars[i].totalWidth / 2.0f;
                currHBPos.Y = players[i].Position.Y + (float)currRect.Height / 2.0f + 10;
                healthBars[i].updatePosition(currHBPos.X, currHBPos.Y);

                players[i].TrySpear(i, spears);
                keepInBounds(players[i]);
            }

            // for each fishy, check if was alive last frame and is dead this one
            // if that is the case, spawn a new fishy
            foreach (Fish f in fishPool)
            {
                // try to school
                f.TryToSchool(fishPool);

                // set threats
                f.SetNearestThreats(players, sharkPool);

                // update state by deciding what to do given the above
                f.update(gameTime);

                Physics.applyMovement(f, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                // if we're dead, instantly respawn in a random location in gameplay bounds
                if (!f.IsAlive)
                {
                    int timeForPowerUp = rng.Next(POWERUP_RARITY);

                    if (timeForPowerUp == POWERUP_TIME)
                    {
                        Powerup p = null;
                        int whichPowerup = rng.Next(6);

                        int colorCoded = rng.Next(4);

                        if (whichPowerup == 0)
                            p = rapidFire;
                        else if (whichPowerup == 1)
                            p = speedBoost;
                        else if (whichPowerup == 2)
                            p = sharkRepellent;
                        else if (whichPowerup == 3)
                            p = spearDeflection;
                        else if (whichPowerup == 4)
                            p = chum;
                        else if (whichPowerup == 5)
                            p = multishot;

                        if (colorCoded == 0)
                            f.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries), p, true);
                        else
                            f.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries), p, false);
                    }
                    else
                        f.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));
                }

                keepInBounds(f);
            }

            // spawn new sharks
            if (isNewSharkReady() && numPlayersAlive() > 1)
            {
                numExtraSharks++;
                sharkPool.Add(new Shark());
                sharkRespawnTimes.Add(0.0f);
                Shark babyShark = sharkPool[(SHARK_POOL_SIZE + numExtraSharks) - 1];
                collisions.Add(babyShark, new List<Actor>());
                //load new content
                babyShark.loadContent();
                //spawn
                (babyShark).spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

                while (isNearAPlayer(sharkPool[(SHARK_POOL_SIZE + numExtraSharks) - 1]))
                    babyShark.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

            }

            for (int i = 0; i < (SHARK_POOL_SIZE + numExtraSharks); ++i)
            {
                Shark s = sharkPool.ElementAt(i);

                s.update(gameTime);

                if (s.CurrState != Actor.state.Dying)
                {
                    s.TryToAggressTowardPlayers(s, players);
                    s.TryToAttackPlayers(s, players);
                }

                Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                if (!s.IsAlive)
                {
                    if (numPlayersAlive() > 1)
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
        }

        private void UpdateVictory(GameTime gameTime)
        {
            // check if player1 wants to restart the match
            // TODO: We might want more options here like doing a new
            // match altogether, but we have plenty of time to figure that out
            TryToRestartGame();
        }

        /// <summary>
        /// The camera zooms in and out automatically depending on
        /// where players are on screen.
        /// </summary>
        private void updateCamera(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            camera.TryZoomOut(delta, players);
            camera.TryZoomIn(delta, players);
        }

        #endregion

        #region DRAWING_CURRENT_GAME_STATE

        protected override void Draw(GameTime gameTime)
        {
            switch (this.currentGameState)
            {
                case (GameState.FindingPlayers):
                    {
                        DrawFindingPlayers(gameTime);
                        break;
                    }
                case (GameState.TransitionIntoBattle):
                    {
                        DrawTransitionIntoBattle(gameTime);
                        break;
                    }
                case (GameState.Battle):
                    {
                        DrawBattle(gameTime);
                        break;
                    }
                case (GameState.Victory):
                    {
                        DrawVictory(gameTime);
                        break;
                    }
            }

            base.Draw(gameTime);
        }

        private void DrawFindingPlayers(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSeaGreen);

            spriteBatch.Begin();
            {
                // draw the background
                spriteBatch.Draw(background, screenRectangle, new Color(255, 255, 255, 50));

                // draw title
                spriteBatch.Draw(title, titlePosition, Color.White);

                // draw each player
                foreach (Penguin p in players)
                    p.draw(gameTime, spriteBatch);

                // draw ready text
                for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
                    if (isPlayerReady[i])
                        spriteBatch.DrawString(font, readyText,
                            playerReadyTextPositions[i], Color.Green);
            }
            spriteBatch.End();
        }

        private void DrawTransitionIntoBattle(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSeaGreen);

            spriteBatch.Begin();
            {
                // draw expanding background fade animation
                spriteBatch.Draw(background, expandingView, backgroundFadeColor);

                // draw the title fading
                spriteBatch.Draw(title, titlePosition, titleFadeColor);

                // draw each player
                foreach (Penguin p in players)
                    p.draw(gameTime, spriteBatch);
            }
            spriteBatch.End();
        }

        private void DrawBattle(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend, null, null, null, null,
                camera.getTransformation());

            spriteBatch.Draw(background, camera.maxView, Color.White);

            ParticleManager.Instance.draw(gameTime, spriteBatch);

            foreach (Fish fish in fishPool)
                fish.draw(gameTime, spriteBatch);

            foreach (Shark shark in sharkPool)
                shark.draw(gameTime, spriteBatch);

            for (int i = 0; i < players.Count; ++i)
                if (players[i].IsAlive)
                    healthBars[i].draw(spriteBatch, players[i].IsBlink);

            for (int i = 0; i < players.Count; ++i)
                players[i].draw(gameTime, spriteBatch);

            foreach (Spear spear in spears)
                spear.draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();

            for (int i = 0; i < players.Count; ++i)
            {
                Penguin p = players.ElementAt(i);

                powerupRectangle.X = (int)playerPowerupPositions[i].X;
                powerupRectangle.Y = (int)playerPowerupPositions[i].Y;

                switch (p.PowerupState)
                {
                    case Actor.powerupstate.SpeedBoost:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_SPEEDBOOST), 
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    case Actor.powerupstate.RapidFire:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_RAPIDFIRE), 
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    case Actor.powerupstate.SharkRepellent:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_SHARKREPELLENT), 
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    case Actor.powerupstate.SpearDeflection:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_SPEARDEFLECTION), 
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    case Actor.powerupstate.Chum:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_CHUM),
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    case Actor.powerupstate.Multishot:
                    {
                        spriteBatch.Draw(SpriteManager.getTexture(POWERUP_MULTISHOT),
                            powerupRectangle, POWERUP_COLOR);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }

            timer1.Draw(spriteBatch, bigFont, graphics);

            for (int i = 0; i < players.Count; ++i)
            {
                spriteBatch.DrawString(bigFont, playerTexts[i], playerTextPositions[i], players[i].color);
                spriteBatch.DrawString(bigFont, caloriesLabelText, playerCalorieTextPositions[i], Color.WhiteSmoke);
                spriteBatch.DrawString(bigFont, players[i].Calories.ToString(), playerCalorieValuePositions[i], Color.Yellow);
            }

            spriteBatch.End();
        }

        private void DrawVictory(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSeaGreen);

            spriteBatch.Begin();
            {
                // draw victory text
                for (int i = 0; i < players.Count; ++i)
                    if (players[i].IsAlive && players[i].CurrState != Actor.state.Dying)
                        spriteBatch.DrawString(bigFont, playerTexts[i] + " Wins!",
                            playerVictoryTextPositions[i], Color.Gold);
            }
            spriteBatch.End();
        }

        #endregion

        #region CHANGING_STATES

        /// <summary>
        /// This is the function called to change the 
        /// overall game state whenever it is done. This
        /// in turn invokes the onEnteringState functions
        /// for each game state.
        /// </summary>
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

        /// <summary>
        /// Upon entering the finding players screen,
        /// if the game has already been completed,
        /// we will give the players a new random 
        /// position on screen and adjust their text
        /// labels accordingly.
        /// </summary>
        private void onEnteringFindingPlayers()
        {
            if (hasBeenCompletedOnce)
            {
                for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
                {
                    if (isPlayerConnected[i]) 
                        isPlayerReady[i] = false;
                }

                foreach (Penguin p in players)
                {
                    p.setNewStartingPosition(getRandomPositionWithinBounds(camera.spawnView));
                    p.respawn();
                }

                Rectangle titleBounds = title.Bounds;

                titleBounds.X = (int)titlePosition.X;
                titleBounds.Y = (int)titlePosition.Y;

                foreach (Penguin p in players)
                    while (p.getBufferedRectangleBounds(0).Intersects(titleBounds) ||
                           isNearAnotherPlayer(p))
                        p.setNewStartingPosition(getRandomPositionWithinBounds(camera.spawnView));


                Rectangle currPlayerBounds;
                for (int i = 0; i < players.Count; ++i)
                {
                    currPlayerBounds = players[i].getBufferedRectangleBounds(0);

                    playerReadyTextPositions[i] = players[i].Position;
                    playerReadyTextPositions[i].X -= currPlayerBounds.Width / 2.0f;
                    playerReadyTextPositions[i].Y += currPlayerBounds.Height / 2.0f;
                }
            }
        }

        /// <summary>
        /// The backgroundFadeColor is initialized 
        /// and the fade variables are reset so we 
        /// can fade smoothely between the two 
        /// version of the background.
        /// </summary>
        private void onEnteringTransitionIntoBattle()
        {
            // add collision pairs for each player that exists
            foreach (Penguin p in players)
            {
                if (!collisions.ContainsKey(p))
                    collisions.Add(p, new List<Actor>());
            }

            backgroundFadeColor.R = 255;
            backgroundFadeColor.G = 255;
            backgroundFadeColor.B = 255;
            backgroundFadeColor.A = 50;

            titleFadeColor = Color.White;

            fadeTime = 0.0f;
        }

        /// <summary>
        /// The battletheme is restarted if it is no longer
        /// playing. The particle manager kills off all 
        /// particles that are not bubbles. The camera resets
        /// back into the original zoom. Any leftover spears
        /// are killed. Fishies and sharkies and players
        /// are respawned.
        /// </summary>
        private void onEnteringBattle()
        {
            //initialize the timer
            timer1 = new Timer();

            //initialize timebased constants
            numExtraSharks = 0;

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
            {
                p.schoolingBounds = gameplayBoundaries;
                p.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));
            }

            // spawn some sharkies
            foreach (Shark s in sharkPool)
            {
                s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));

                while (isNearAPlayer(s))
                    s.spawnAt(getRandomPositionWithinBounds(gameplayBoundaries));
            }
        }

        /// <summary>
        /// TODO: Something.
        /// </summary>
        private void onEnteringVictory()
        {
            cleanExtraSharks();
        }

        #endregion

        #region HELPER_FUNCTIONS

        private void cleanExtraSharks()
        {
            while (sharkPool.Count > SHARK_POOL_SIZE)
                sharkPool.RemoveAt(sharkPool.Count - 1);
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

            return !camera.View.Contains(aBounds) &&
                   !camera.View.Intersects(aBounds);
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

                    //if (a.acceleration.X < 0.0f)
                    //    a.acceleration.X = 0.0f;
                    a.acceleration.X = -(a.acceleration.X);
                }

                if (aBounds.Right > gameplayBoundaries.Right)
                {
                    a.move(-((float)Math.Abs(gameplayBoundaries.Right - aBounds.Right) + EPSILON), 0.0f);

                    if (a.velocity.X > 0.0f)
                        a.velocity.X = 0.0f;

                    //if (a.acceleration.X > 0.0f)
                    //    a.acceleration.X = 0.0f;
                    a.acceleration.X = -(a.acceleration.X);
                }

                if (aBounds.Top < gameplayBoundaries.Top)
                {
                    a.move(0.0f, (float)Math.Abs(gameplayBoundaries.Top - aBounds.Top) + EPSILON);

                    if (a.velocity.Y < 0.0f)
                        a.velocity.Y = 0.0f;

                    //if (a.acceleration.Y < 0.0f)
                    //    a.acceleration.Y = 0.0f;
                    a.acceleration.Y = -(a.acceleration.Y);
                }

                if (aBounds.Bottom > gameplayBoundaries.Bottom)
                {
                    a.move(0.0f, -((float)Math.Abs(gameplayBoundaries.Bottom - aBounds.Bottom) + EPSILON));

                    if (a.velocity.Y > 0.0f)
                        a.velocity.Y = 0.0f;

                    //if (a.acceleration.Y > 0.0f)
                    //    a.acceleration.Y = 0.0f;
                    a.acceleration.Y = -(a.acceleration.Y);
                }
            }
        }

        private void wrapAround(Actor a)
        {
            Rectangle aBounds = a.getBufferedRectangleBounds(0);

            if (!gameplayBoundaries.Contains(aBounds))
            {
                // 1.) determine the wall(s) with which we are colliding
                // 2.) if we are completely outside on that wall, then 
                //     reflect to the other side of the stage.
                // off the right side
                if (aBounds.Left >= gameplayBoundaries.Right)
                {
                    a.move(-(float)(gameplayBoundaries.Width + (aBounds.Left - gameplayBoundaries.Right)), 
                                           0.0f);
                }
                // off the left side
                if (aBounds.Right <= gameplayBoundaries.Left)
                {
                    a.move((float)(gameplayBoundaries.Width + (gameplayBoundaries.Left - aBounds.Right)),
                       0.0f);
                }
                // off the bottom side
                if (aBounds.Top >= gameplayBoundaries.Bottom)
                {
                    a.move(0.0f, 
                        -(float)((aBounds.Top - gameplayBoundaries.Bottom) + gameplayBoundaries.Height));

                }
                // off the top side
                if (aBounds.Bottom <= gameplayBoundaries.Top)
                {
                    a.move(0.0f, 
                        (float)((gameplayBoundaries.Top - aBounds.Bottom) + gameplayBoundaries.Height));
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

        private bool isNearAnotherPlayer(Penguin a)
        {
            Vector2 aPos = a.Position;

            float minDist = 5000.0f;

            float currDist = 0.0f;

            foreach (Penguin p in players)
            {
                if (p == a) continue;

                currDist = Vector2.Distance(p.Position, aPos);

                if (currDist < minDist)
                    minDist = currDist;
            }

            if (minDist <= SHARK_SPAWN_CLOSENESS_THRESHOLD)
                return true;

            return false;
        }

        private int numPlayersAlive()
        {
            int numPlayersAlive = 0;

            foreach (Penguin player in players)
                if (player.CurrState != Actor.state.Dying)
                    numPlayersAlive++;

            return numPlayersAlive;
        }

        public bool isNewSharkReady()
        {
            if (timer1.getTimer() > (TIME_EVENT_SHARK) * (numExtraSharks + 1))
            {
                return true;
            }
            else return false;
        }

        #endregion

        #region PHYSICS

        private void detectCollisions()
        {
            // clear potential collisions
            foreach (List<Actor> pairs in collisions.Values)
                pairs.Clear();

            #region PLAYERS

            // player collisions
            for (int i = 0; i < players.Count; ++i)
            {
                // players with players
                for (int j = 0; j < players.Count; ++j)
                {
                    if (i == j || !players[i].IsAlive || !players[j].IsAlive)
                        continue;

                    if (players[i].Bounds.isCollision(players[j].Bounds) || 
                        (players[i].Bounds.isCollision(players[j].spearCircle) 
                        && players[j].CurrState == Actor.state.MeleeAttack))
                    {
                        if (!collisions[players[i]].Contains(players[j]) && 
                            !collisions[players[j]].Contains(players[i]))
                            collisions[players[i]].Add(players[j]);
                    }
                }
            }

            #endregion

            #region SPEARS

            // spear collisions
            for (int i = 0; i < spears.Count; i++)
            {
                if (!spears[i].IsAlive) continue;

                // spears with sharks
                for (int j = 0; j < sharkPool.Count; j++)
                {
                    if (!sharkPool[j].IsAlive) continue;

                    if (spears[i].Bounds.isCollision(sharkPool[j].Bounds))
                    {
                        if (!collisions[spears[i]].Contains(sharkPool[j]) &&
                            !collisions[sharkPool[j]].Contains(spears[i]))
                            collisions[spears[i]].Add(sharkPool[j]);
                    }
                }

                // spears with players
                for (int j = 0; j < players.Count; j++)
                {
                    if (players[j] == spears[i].Owner || !players[j].IsAlive) continue;

                    if (spears[i].bounds.isCollision(players[j].Bounds) || 
                        (spears[i].bounds.isCollision(players[j].spearDeflectorAura) &&
                        players[j].PowerupState == Actor.powerupstate.SpearDeflection))
                    {
                        if (!collisions[spears[i]].Contains(players[j]) &&
                            !collisions[players[j]].Contains(spears[i]))
                            collisions[spears[i]].Add(players[j]);
                    }
                }
            }

            #endregion

            #region FISH

            // fish collisions
            for (int i = 0; i < fishPool.Count; ++i)
            {
                if (!fishPool[i].IsAlive)
                    continue;
                
                // fish with players
                for (int j = 0; j < players.Count; ++j)
                {
                    if (!players[j].IsAlive || players[j].CurrState == Actor.state.Dying)
                        continue;

                    if (fishPool[i].Bounds.isCollision(players[j].Bounds))
                    {
                        if (!collisions[fishPool[i]].Contains(players[j]) &&
                            !collisions[players[j]].Contains(fishPool[i]))
                            collisions[fishPool[i]].Add(players[j]);
                    }
                }

                // fish with sharks
                for (int j = 0; j < sharkPool.Count; j++)
                {
                    if (!sharkPool[j].IsAlive) continue;

                    if (fishPool[i].Bounds.isCollision(sharkPool[j].Bounds))
                    {
                        if (!collisions[fishPool[i]].Contains(sharkPool[j]) &&
                            !collisions[sharkPool[j]].Contains(fishPool[i]))
                            collisions[fishPool[i]].Add(sharkPool[j]);
                    }
                }

                // fish with fish
                for (int j = 0; j < fishPool.Count; ++j)
                {
                    if (i == j || !fishPool[j].IsAlive || fishPool[j].IsPoweredUp || fishPool[i].IsPoweredUp)
                        continue;

                    if (fishPool[i].Bounds.isCollision(fishPool[j].Bounds))
                    {
                        if (!collisions[fishPool[i]].Contains(fishPool[j]) && 
                            !collisions[fishPool[j]].Contains(fishPool[i]))
                            collisions[fishPool[i]].Add(fishPool[j]);
                    }
                }
            }

            #endregion

            #region SHARKS

            // shark collisions
            for (int i = 0; i < sharkPool.Count; ++i)
            {
                if (!sharkPool[i].IsAlive) continue;

                // sharks with players
                for (int j = 0; j < players.Count; ++j)
                {
                    if (!players[j].IsAlive)
                        continue;

                    if (sharkPool[i].Bounds.isCollision(players[j].Bounds) || 
                        (sharkPool[i].Bounds.isCollision(players[j].spearCircle) 
                        && players[j].CurrState == Actor.state.MeleeAttack))
                    {
                        if (!collisions[sharkPool[i]].Contains(players[j]))
                            collisions[sharkPool[i]].Add(players[j]);
                    }
                }

                // sharks with fishies
                for (int j = 0; j < fishPool.Count; ++j)
                {
                    if (!fishPool[j].IsAlive) continue;

                    if (sharkPool[i].Bounds.isCollision(fishPool[j].Bounds))
                    {
                        if (!collisions[sharkPool[i]].Contains(fishPool[j]) &&
                            !collisions[fishPool[j]].Contains(sharkPool[i]))
                            collisions[sharkPool[i]].Add(fishPool[j]);
                    }
                }
            }

            #endregion
        }

        private void resolveCollisions()
        {
            foreach (Actor collider in collisions.Keys)
            {
                foreach (Actor collidee in collisions[collider])
                {
                    collider.collideWith(collidee);
                    collidee.collideWith(collider);

                    // if it is one of the pairs that physically collides, 
                    // do so. 
                    if ((collider is Penguin && collidee is Penguin && 
                         collider.Bounds.isCollision(collidee.Bounds)) ||
                        (collider is Penguin && collidee is Shark &&
                        collider.Bounds.isCollision(collidee.Bounds))||
                        (collider is Shark && collidee is Penguin &&
                        collider.Bounds.isCollision(collidee.Bounds)) ||
                        collider is Fish && collidee is Fish)
                    {
                        Physics.separate(collider, collidee);
                        Physics.collide(collider, collidee);
                    }
                }
            }
        }

        #endregion

        #region GAME_STATE_TRANSITION_LOGIC

        private void TryToAddPlayers()
        {
            for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
            {
                if (!isPlayerConnected[i])
                {
                    if (GamePad.GetState((PlayerIndex)i).IsConnected)
                    {
                        Penguin newPlayer = 
                            new Penguin((PlayerIndex)i, playerStartPositions[i], colorCodes[i]);

                        newPlayer.loadContent();
                        newPlayer.respawn();
                        newPlayer.pauseAnimation();

                        players.Add(newPlayer);

                        playerReadyTextPositions[i] = players[i].Position;
                        playerReadyTextPositions[i].Y += players[i].getBufferedRectangleBounds(0).Height / 2.0f;
                        playerReadyTextPositions[i].X -= players[i].getBufferedRectangleBounds(0).Width / 2.0f;

                        isPlayerConnected[i] = true;
                    }
                }
            }
        }

        private void TryToReadyPlayers()
        {
            for (int i = 0; i < MAX_NUM_PLAYERS; ++i)
            {
                if (isPlayerConnected[i] && !isPlayerReady[i])
                {
                    if (GamePad.GetState(players[i].Controller).IsButtonDown(Buttons.A))
                    {
                        isPlayerReady[i] = true;
                        AudioManager.getSound("Ready_Sound").Play();
                    }
                }
            }
        }

        private void TryToStartGame()
        {
            // if any player isn't ready but connected then we cannot start
            for (int i = 1; i < MAX_NUM_PLAYERS; ++i)
                if (isPlayerConnected[i] && !isPlayerReady[i])
                    return;

            // otherwise, just check to see if start is being pressed
            if (isPlayerReady[0] && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
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
            int numLivePlayers = numPlayersAlive();
            
            // huehuehue
            //foreach (Shark s in sharkPool)
            //    if (s.CurrState != Actor.state.Dying)
            //        numAlive++;

            if ((numAlive == 0 && numLivePlayers == 1) ||
               (numLivePlayers == 0))
                changeState(GameState.Victory);
        }

        #endregion
    }
}
