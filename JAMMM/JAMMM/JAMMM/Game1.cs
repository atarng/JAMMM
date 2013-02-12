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

        //public const string PENGUIN_DEATH_SMALL = "Penguin_Death_Small";
        //public const string PENGUIN_DEATH_MEDIUM = "Penguin_Death_Med";
        //public const string PENGUIN_DEATH_LARGE = "Penguin_Death_Large";

        public const string FISH_SWIM = "Fish_Swim";
        public const string FISH_DEATH = "Fish_Death";

        public const string SHARK_SWIM = "Shark_Swim";
        public const string SHARK_EAT = "Shark_Eat";
        public const string SHARK_TURN = "Shark_Turn";
        public const string SHARK_DEATH = "Shark_Death";

        private const float EPSILON = 0.01f;

        private const int FISH_POOL_SIZE = 40;
        private const int SHARK_POOL_SIZE = 0;

        private const float SHARK_ATTACK_THRESHOLD = 300;
        private const float SHARK_SPAWN_CLOSENESS_THRESHOLD = 450;
        private const float SHARK_AGGRESS_THRESHOLD = 600;

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

        //private AnimatedActorTest testActAnim;
        public static SpriteFont font;

        public static int WINDOW_WIDTH = 1600;
        public static int WINDOW_HEIGHT = 900;

        // zooming related

        private const float slowZoom = 0.001f;
        private const float medZoom = 0.002f;

        private const float SHARK_RESPAWN_TIME = 1.0f;

        private const float BACKGROUND_FADE_DURATION = 1.0f;
        private Color backgroundFadeColor;
        private Color titleFadeColor;
        private Rectangle expandingView = Rectangle.Empty;
        private float fadeTime;

        private bool hasBeenCompletedOnce = false;
        
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

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            changeState(GameState.FindingPlayers);
            Random rnd = new Random();

            // set initial fish positions
            for (int i = 0; i < FISH_POOL_SIZE; ++i)
            {
                fishPool.Add(new Fish(0.0f, 0.0f));
            }

            // set initial shark position
            for (int i = 0; i < SHARK_POOL_SIZE; ++i)
            {
                sharkPool.Add(new Shark(0.0f, 0.0f));
                sharkRespawnTimes.Add(0.0f);
            }

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


            // tell each actor to load their content now that the sprite manager has its database
            foreach (Shark s in sharkPool)
                s.loadContent();
            foreach (Fish f in fishPool)
                f.loadContent();
            foreach (Spear s in spears)
                s.loadContent();

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

        protected void trySpear(Penguin a, int id) //TODO change the penguin
        {
            if (a.Fire)
            {
                a.Fire = false;
                //Spear projectile = new Spear(a.Position.X, a.Position.Y, a.CurrentSize);
                Spear projectile = new Spear(a.Position.X, a.Position.Y,a.CurrentSize, id);
                projectile.respawn();
                projectile.loadContent();
                projectile.acceleration = Vector2.Normalize(Physics.AngleToVector(a.Rotation)) * 50000F;
                projectile.velocity.X = a.Velocity.X;
                projectile.velocity.Y = a.Velocity.Y;
                spears.Add(projectile); //TODO change type to spear
                //projectilePool.Add(projectile);
            }
        }

        private Vector2 getRandomPositionInGameplayBounds()
        {
            Vector2 newPos = Vector2.Zero;

            newPos.X = (float)gameplayBoundaries.Left 
                + (float)(rng.NextDouble() * (double)(gameplayBoundaries.Width));

            newPos.Y = (float)gameplayBoundaries.Top
                + (float)(rng.NextDouble() * (double)(gameplayBoundaries.Height));

            return newPos;
        }

        private Vector2 getRandomPositionOnScreen()
        {
            Vector2 newPos = Vector2.Zero;

            newPos.X = (float)screenRectangle.Left
                + (float)(rng.NextDouble() * (double)(screenRectangle.Width));

            newPos.Y = (float)screenRectangle.Top
                + (float)(rng.NextDouble() * (double)(screenRectangle.Height));

            return newPos;
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
                    if (hasBeenCompletedOnce)
                    {
                        if (isPlayer1Connected) isPlayer1Ready = false;
                        if (isPlayer2Connected) isPlayer2Ready = false;
                        if (isPlayer3Connected) isPlayer3Ready = false;
                        if (isPlayer4Connected) isPlayer4Ready = false;

                        foreach (Penguin p in players)
                            p.setNewStartingPosition(getRandomPositionOnScreen());

                        foreach (Penguin p in players)
                            while (p.getBufferedRectangleBounds(0).Intersects(title.Bounds) &&
                                   !screenRectangle.Contains(p.getBufferedRectangleBounds(0)))
                                p.setNewStartingPosition(getRandomPositionOnScreen());
                    }

                    break;
                }
                case (GameState.TransitionIntoBattle):
                {
                    // initialize the fading variables
                    backgroundFadeColor.R = 255;
                    backgroundFadeColor.G = 255;
                    backgroundFadeColor.B = 255;
                    backgroundFadeColor.A = 50;

                    titleFadeColor = Color.White;

                    fadeTime = 0.0f;
                    
                    break;
                }
                case (GameState.Battle):
                {
                    // play the battle theme
                    if (battleTheme.State != SoundState.Playing)
                        battleTheme.Play();

                    camera.resetZoom();

                    foreach (Spear s in spears)
                        s.die();

                    // load content and spawn each player
                    foreach (Penguin p in players)
                    {
                        p.loadContent();
                        p.respawn();
                    }

                    // spawn some fishies
                    foreach (Fish p in fishPool)
                        p.spawnAt(getRandomPositionInGameplayBounds());

                    // spawn some sharkies
                    foreach (Shark s in sharkPool)
                        s.spawnAt(getRandomPositionInGameplayBounds());

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

        protected Boolean isOffScreen(Actor a)
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
                    for( int i = 0; i < players.Count; i++ )
                    {
                        players[i].update(gameTime);
                        Physics.applyMovement(players[i], (float)gameTime.ElapsedGameTime.TotalSeconds, true);
                        trySpear(players[i], i);
                        keepInBounds(players[i]);
                    }

                    // for each fishy, check if was alive last frame and is dead this one
                    // if that is the case, spawn a new fishy
                    foreach (Fish f in fishPool)
                    {
                        f.update(gameTime);
                        Physics.applyMovement(f, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                        if (!f.IsAlive)
                            f.spawnAt(getRandomPositionInGameplayBounds());

                        keepInBounds(f);
                    }

                    for (int i = 0; i < SHARK_POOL_SIZE; ++i)
                    {
                        Shark s = sharkPool.ElementAt(i);

                        s.update(gameTime);

                        if (s.CurrState != Actor.state.Dying)
                        {
                            tryToAggressTowardPlayers(s);
                            tryToAttackPlayers(s);
                        }

                        Physics.applyMovement(s, (float)gameTime.ElapsedGameTime.TotalSeconds, true);

                        if (!s.IsAlive)
                        {
                            sharkRespawnTimes[i] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (sharkRespawnTimes[i] >= SHARK_RESPAWN_TIME)
                            {
                                sharkRespawnTimes[i] = 0.0f;
                                
                                s.spawnAt(getRandomPositionInGameplayBounds());

                                while (isNearAPlayer(s))
                                    s.spawnAt(getRandomPositionInGameplayBounds());
                            }
                        }

                        keepInBounds(s);
                    }

                    // update the onscreen spears
                    for (int i = 0; i < spears.Count; i++)
                    {
                        Spear spear = spears[i];
                        spear.update(gameTime);
                        Physics.applyMovement(spear, (float)gameTime.ElapsedGameTime.TotalSeconds, false);

                        if (isOffScreen(spear))
                            spear.IsAlive = false;
                    }

                    // do collision detection and resolution
                    doCollisions();

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
                case (GameState.Victory) : 
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

        private void tryToAggressTowardPlayers(Shark s)
        {
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
            {
                s.CurrState = Actor.state.Pursuing;
                s.acceleration = vecTowardNearest;
            }
            else if (minDist > SHARK_AGGRESS_THRESHOLD)
            {
                s.CurrState = Actor.state.Moving;
            }
        }

        private void tryToAttackPlayers(Shark s)
        {
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
                s.CurrState = Actor.state.Dash;
            }
        }

        /// <summary>
        /// Do all of the collision detection for each pair
        /// for which we allow collisions.
        /// </summary>
        private void doCollisions()
        {
            //////COLLISIONS///////
            collisions.Clear();

            /*
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
            */

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
                        && spears[i].IsAlive && players[j].IsAlive && j != spears[i].Id  &&
                        !collisions.ContainsKey(spears[i]))
                        collisions.Add(spears[i], players[j]);
                }
            }

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
            }

            for (int i = 0; i < sharkPool.Count; ++i)
            {
                for (int j = 0; j < players.Count; ++j)
                {
                    if (sharkPool[i].Bounds.isCollision(players[j].Bounds)
                        && (sharkPool[i].IsAlive) && players[j].IsAlive &&
                        !collisions.ContainsKey(sharkPool[i]))
                        collisions.Add(sharkPool[i], players[j]);
                }
            }

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

        private void removeActor(Actor a)
        {
            if (a is Spear)
                spears.Remove((Spear)a);
            if (a is Shark)
                sharkPool.Remove((Shark)a);
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
                    players.Add(new Penguin(PlayerIndex.One, player1StartPosition, ""));
                    isPlayer1Connected = true;
                }
            }
            if (!isPlayer2Connected)
            {
                if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Two, player2StartPosition, "_r"));
                    isPlayer2Connected = true;
                }
                
            }
            if (!isPlayer3Connected)
            {
                if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Three, player3StartPosition, "_p"));
                    isPlayer3Connected = true;
                }
            }
            if (!isPlayer4Connected)
            {
                if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                {
                    players.Add(new Penguin(PlayerIndex.Four, player4StartPosition, "_g"));
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

        /// <summary>
        /// Checks if player 1 pressed start.
        /// </summary>
        private void TryToStartGame()
        {
            if (isPlayer1Ready && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                changeState(GameState.TransitionIntoBattle);
        }

        /// <summary>
        /// Checks if player 1 pressed start.
        /// </summary>
        private void TryToRestartGame()
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
                hasBeenCompletedOnce = true;
                changeState(GameState.FindingPlayers);
            }
        }

        /// <summary>
        /// Check if only 1 player is alive. If that is the case,
        /// then change state to victory.
        /// </summary>
        private void TryToEndGame()
        {
            int numAlive = 0;

            foreach (Penguin player in players)
                if (player.CurrState != Actor.state.Dying)
                    numAlive++;

            if (numAlive == 1)
                changeState(GameState.Victory);
        }

        /// <summary>
        /// Zooms in when the players are within an inner view
        /// rectangle for a certain duration. Zooms out when
        /// a player is going to pass through the border.
        /// </summary>
        private void updateCamera(GameTime gameTime)
        {
            TryZoomOut((float)gameTime.ElapsedGameTime.TotalSeconds);
            TryZoomIn((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// Find the player closest to the borders and zoom out by their
        /// velocity as long as they are moving in the direction of crossing
        /// that border and are within a certain limit ZOOM_BUFFER.
        /// </summary>
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

        /// <summary>
        /// If all of the players are within specific rectangles
        /// in the overall view ranges then zoom in slowly. 
        /// </summary>
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

        /// <summary>
        /// Gets the furthest penguin form the center of the screen.
        /// </summary>
        private Actor getFurthestPlayerFromCenter()
        {
            float maxDist = 0.0f;
            float currDist = 0.0f;
            Actor fActor = null;

            foreach (Penguin p in players)
            {
                currDist = camera.getDistanceFromViewCenter(p);

                if (currDist > maxDist)
                {
                    maxDist = currDist;
                    fActor = p;
                }
            }

            return fActor;
        }

        /// <summary>
        /// Gets the furthest player outside of the view
        /// boundaries.
        /// </summary>
        private Actor getFurthestPlayerOutOfBounds()
        {
            float maxDist = 0.0f;
            float currDist = 0.0f;
            Actor fActor = null;

            foreach (Penguin p in players)
            {
                currDist = camera.getDistanceOutsideBorders(p);

                if (currDist > maxDist)
                {
                    maxDist = currDist;
                    fActor = p;
                }
            }

            return fActor;
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
                    spriteBatch.Draw(background, screenRectangle, new Color(255, 255, 255, 50));
                    spriteBatch.Draw(title, titlePosition, Color.White);
                    //spriteBatch.DrawString(font, titleText, titlePosition, Color.WhiteSmoke);

                    // draw a penguin for each connected controller
                    if (isPlayer1Connected)
                        spriteBatch.Draw(playerPenguin, players[0].getStartingPosition(), playerPenguinRectangle, 
                            players[0].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer2Connected)
                        spriteBatch.Draw(playerPenguin, players[1].getStartingPosition(), playerPenguinRectangle,
                            players[1].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer3Connected)
                        spriteBatch.Draw(playerPenguin, players[2].getStartingPosition(), playerPenguinRectangle,
                            players[2].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer4Connected)
                        spriteBatch.Draw(playerPenguin, players[3].getStartingPosition(), playerPenguinRectangle,
                            players[3].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);

                    // draw a green ready text for each readied player
                    if (isPlayer1Ready)
                        spriteBatch.DrawString(font, readyText, player1ReadyTextPosition, Color.Green);
                    if (isPlayer2Ready)
                        spriteBatch.DrawString(font, readyText, player2ReadyTextPosition, Color.Green);
                    if (isPlayer3Ready)
                        spriteBatch.DrawString(font, readyText, player3ReadyTextPosition, Color.Green);
                    if (isPlayer4Ready)
                        spriteBatch.DrawString(font, readyText, player4ReadyTextPosition, Color.Green);

                    spriteBatch.End();

                    break;
                }
                case (GameState.TransitionIntoBattle):
                {
                    // draw background
                    GraphicsDevice.Clear(Color.DarkSeaGreen);

                    spriteBatch.Begin();

                    spriteBatch.Draw(background, expandingView, backgroundFadeColor);

                    spriteBatch.Draw(title, titlePosition, titleFadeColor);
                    //spriteBatch.DrawString(font, titleText, titlePosition, Color.WhiteSmoke);

                    // draw a penguin for each connected controller
                    if (isPlayer1Connected)
                        spriteBatch.Draw(playerPenguin, players[0].getStartingPosition(), playerPenguinRectangle,
                            players[0].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer2Connected)
                        spriteBatch.Draw(playerPenguin, players[1].getStartingPosition(), playerPenguinRectangle,
                            players[1].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer3Connected)
                        spriteBatch.Draw(playerPenguin, players[2].getStartingPosition(), playerPenguinRectangle,
                            players[2].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    if (isPlayer4Connected)
                        spriteBatch.Draw(playerPenguin, players[3].getStartingPosition(), playerPenguinRectangle,
                            players[3].color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);

                    spriteBatch.End();

                    break;
                }
                case (GameState.Battle):
                {
                    // draw background
                    GraphicsDevice.Clear(Color.Blue);
                    
                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend, null, null, null, null,
                        camera.getTransformation());
                    
                    //spriteBatch.Begin();

                    spriteBatch.Draw(background, camera.maxView, Color.White);

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

                    ParticleManager.Instance.draw(gameTime, spriteBatch);

                    spriteBatch.End();


                    spriteBatch.Begin();

                    // draw calories and player text for each player
                    if (isPlayer1Connected)
                    {
                        spriteBatch.DrawString(font, player1Text, player1TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player1CalorieTextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, players[0].Calories.ToString(), player1CalorieValuePosition, Color.Yellow);
                        // draw the calories themselves as a string right after that position
                        //spriteBatch.DrawString(font, players[0].Calories <= 0 ? "IS DEAD" : "Is Still Alive",
                        //    player1CalorieTextPosition, Color.WhiteSmoke);
                    }
                    if (isPlayer2Connected)
                    {
                        spriteBatch.DrawString(font, player2Text, player2TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player2CalorieTextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, players[1].Calories.ToString(), player2CalorieValuePosition, Color.Yellow);
                        // draw the calories themselves as a string right after that position
                        //spriteBatch.DrawString(font, players[1].Calories <= 0 ? "IS DEAD" : "Is Still Alive",
                        //    player2CalorieTextPosition, Color.WhiteSmoke);
                    }
                    if (isPlayer3Connected)
                    {
                        spriteBatch.DrawString(font, player3Text, player3TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player3CalorieTextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, players[2].Calories.ToString(), player3CalorieValuePosition, Color.Yellow);
                        // draw the calories themselves as a string right after that position
                        //spriteBatch.DrawString(font, players[2].Calories <= 0 ? "IS DEAD" : "Is Still Alive",
                        //    player3CalorieTextPosition, Color.WhiteSmoke);
                    }
                    if (isPlayer4Connected)
                    {
                        spriteBatch.DrawString(font, player4Text, player4TextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, caloriesLabelText, player4CalorieTextPosition, Color.WhiteSmoke);
                        spriteBatch.DrawString(font, players[3].Calories.ToString(), player4CalorieValuePosition, Color.Yellow);
                        // draw the calories themselves as a string right after that position
                        //spriteBatch.DrawString(font, players[3].Calories <= 0 ? "IS DEAD" : "Is Still Alive",
                        //    player4CalorieTextPosition, Color.WhiteSmoke);
                    }

                    spriteBatch.End();

                    break;
                }
                case (GameState.Victory):
                {
                    // draw victory screen splash
                    GraphicsDevice.Clear(Color.Blue);

                    spriteBatch.Begin();

                    // draw victory text (containing the player)
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
    }
}
