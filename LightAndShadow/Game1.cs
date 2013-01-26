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
using System.Threading;

namespace LightAndShadow
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        bool musicPlaying = false;
        int screenWidth, screenHeight;

        SpriteFont font;
        Song song1;
        Song song2;

        Player player1, player2, player3, player4, playerk;
        List<Bullet> bullets;
        List<Player> players;
        List<Enemy> enemies;
        List<Particle> particles;
        List<Bomb> bombs;
        List<Bullet> lasers;

        List<Object> MarkedForDelete;
        List<Enemy> SpawnChasers;

        const int STARTING_LIFE = 1100;
        const int STARTING_INVINCIBLE = 120;
        const int LIVESCOUNT = 5;
        int Lives;

        Random random;
        TimeSpan elapsedTime;

        bool gameOver = false;
        bool gameStart = false;
        Color color25, color50, color75;

        Texture2D blackBlend, blackBlendS, whiteBlend, whiteBlendS;
        Texture2D blackCircle;
        Texture2D gameOverText, gameOverBack;
        Texture2D ShmadowTitle;

        //RenderTarget2D sceneRenderTarget;
        //RenderTarget2D renderTarget1;
        //RenderTarget2D renderTarget2;

        int frameRate, frameCounter;
        int bigGuyCount = 0;
        int bigGuyMax = 40;

        float fadeIn = 0;
        int startScreenReloadTime = 300;

        int screenX;
        int screenY;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            #if WINDOWS
            screenX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenY = (int)((float)screenX / (16f / 9f));
            #endif
            #if XBOX
            this.graphics.IsFullScreen = true;
            screenX = 1280;
            screenY = 720;
            #endif

            this.graphics.PreferredBackBufferHeight = screenY;
            this.graphics.PreferredBackBufferWidth = screenX;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            screenWidth = screenX;
            screenHeight = screenY;

            // TODO: Add your initialization logic here
            player1 = new Player(screenHeight / 6, 1, STARTING_LIFE);
            player2 = new Player(2 * screenHeight / 6, 2, STARTING_LIFE);
            player3 = new Player(3 * screenHeight / 6, 3, STARTING_LIFE);
            player4 = new Player(4 * screenHeight / 6, 4, STARTING_LIFE);
            playerk = new Player(5 * screenHeight / 6, 5, STARTING_LIFE);
            bullets = new List<Bullet>();
            players = new List<Player>();
            enemies = new List<Enemy>();
            lasers = new List<Bullet>();
            particles = new List<Particle>();
            bombs = new List<Bomb>();

            MarkedForDelete = new List<Object>();
            SpawnChasers = new List<Enemy>();

            color25 = new Color(.25f, .25f, .25f, .25f);
            color50 = new Color(.50f, .50f, .50f, .50f);
            color75 = new Color(.75f, .75f, .75f, .75f);

            Lives = LIVESCOUNT;

            elapsedTime = TimeSpan.Zero;


            random = new Random();
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
            font = Content.Load<SpriteFont>("Text");


            song1 = Content.Load<Song>("Bossfinal");
            song2 = Content.Load<Song>("beno");

            blackBlend = Content.Load<Texture2D>("BlackBlend");
            blackCircle = Content.Load<Texture2D>("BlackCircle");
            whiteBlend = Content.Load<Texture2D>("WhiteBlend");

            blackBlendS = Content.Load<Texture2D>("BlackBlend9");
            whiteBlendS = Content.Load<Texture2D>("WhiteBlend9");

            gameOverText = Content.Load<Texture2D>("GameOver");
            gameOverBack = Content.Load<Texture2D>("GameOverBack");

            ShmadowTitle = Content.Load<Texture2D>("ShmadowTitle");

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

            if (GamePad.GetState(PlayerIndex.Two).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GamePad.GetState(PlayerIndex.Three).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GamePad.GetState(PlayerIndex.Four).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (startScreenReloadTime <= 0)
            {
                startScreenReloadTime = 300;
                gameOver = false;
                gameStart = false;
                players.Clear();
                player1 = new Player(screenHeight / 6, 1, STARTING_LIFE);
                player2 = new Player(2 * screenHeight / 6, 2, STARTING_LIFE);
                player3 = new Player(3 * screenHeight / 6, 3, STARTING_LIFE);
                player4 = new Player(4 * screenHeight / 6, 4, STARTING_LIFE);
                playerk = new Player(5 * screenHeight / 6, 5, STARTING_LIFE);
                fadeIn = 0;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                if (!players.Contains(player1))
                {
                    gameOver = false;
                    if (gameStart == false)
                    {
                        enemies.Clear();
                        bigGuyCount = 0;
                        gameStart = true;
                        Lives = LIVESCOUNT;
                    }
                    else
                    {
                        Lives += 2;
                    }
                    players.Add(player1);
                    bigGuyMax = 30 + 10 * players.Count;
                    Lives--;

                }
                else if (!player1.alive && Lives > 0)
                {
                    player1.alive = true;
                    player1.setMass(STARTING_LIFE);
                    player1.location = new Vector2(10, 4 * screenHeight / 6);
                    Lives--;
                    player1.invincible = STARTING_INVINCIBLE;
                }

            if (GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
                if (!players.Contains(player2))
                {
                    gameOver = false;
                    if (gameStart == false)
                    {
                        enemies.Clear();
                        bigGuyCount = 0;
                        gameStart = true;
                        Lives = LIVESCOUNT;
                    }
                    else
                    {
                        Lives += 2;
                    }
                    players.Add(player2);
                    player1.invincible = STARTING_INVINCIBLE;
                    bigGuyMax = 30 + 10 * players.Count;
                    player2.location = new Vector2(10, 4 * screenHeight / 6);
                    Lives--;
                }
                else if (!player2.alive && Lives > 0)
                {
                    player2.alive = true;
                    player2.setMass(STARTING_LIFE);
                    Lives--;
                    player1.invincible = STARTING_INVINCIBLE;
                }

            if (GamePad.GetState(PlayerIndex.Three).Buttons.Start == ButtonState.Pressed)
                if (!players.Contains(player3))
                {
                    gameOver = false;
                    if (gameStart == false)
                    {
                        enemies.Clear();
                        bigGuyCount = 0;
                        gameStart = true;
                        Lives = LIVESCOUNT;
                    }
                    else
                    {
                        Lives += 2;
                    }
                    players.Add(player3);
                    player1.invincible = STARTING_INVINCIBLE;
                    bigGuyMax = 30 + 10 * players.Count;
                    Lives--;
                }
                else if (!player3.alive && Lives > 0)
                {
                    player3.alive = true;
                    player3.setMass(STARTING_LIFE);
                    player1.invincible = STARTING_INVINCIBLE;
                    player3.location = new Vector2(10, 4 * screenHeight / 6);
                    Lives--;
                }

            if (GamePad.GetState(PlayerIndex.Four).Buttons.Start == ButtonState.Pressed)
                if (!players.Contains(player4))
                {
                    gameOver = false;
                    if (gameStart == false)
                    {
                        enemies.Clear();
                        bigGuyCount = 0;
                        gameStart = true;
                        Lives = 4;
                    }
                    else
                    {
                        Lives += 2;
                    }
                    player1.invincible = STARTING_INVINCIBLE;
                    players.Add(player4);
                    bigGuyMax = 30 + 10 * players.Count;
                    Lives--;
                }
                else if (!player4.alive && Lives > 0)
                {
                    player4.alive = true;
                    player4.setMass(STARTING_LIFE);
                    player4.location = new Vector2(10, 4 * screenHeight / 6);
                    Lives--;
                    player1.invincible = STARTING_INVINCIBLE;
                }

            player1.state = GamePad.GetState(PlayerIndex.One);
            player2.state = GamePad.GetState(PlayerIndex.Two);
            player3.state = GamePad.GetState(PlayerIndex.Three);
            player4.state = GamePad.GetState(PlayerIndex.Four);
            playerk.kstate = Keyboard.GetState();
            playerk.mstate = Mouse.GetState();

            //Keyboard
            #if WINDOWS
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (!players.Contains(playerk))
                {
                    gameOver = false;
                    if (gameStart == false)
                    {
                        enemies.Clear();
                        bigGuyCount = 0;
                        gameStart = true;
                        Lives = LIVESCOUNT;
                    }
                    players.Add(playerk);
                    bigGuyMax = 30 + 10 * players.Count;
                    playerk.mkb = true;
                    player1.invincible = STARTING_INVINCIBLE;
                    this.IsMouseVisible = true;
                    Lives--;
                }
                else if (!playerk.alive && Lives > 0)
                {
                    playerk.alive = true;
                    playerk.setMass(500);
                    playerk.location = new Vector2(10, 4 * screenHeight / 6);
                    Lives--;
                    player1.invincible = STARTING_INVINCIBLE;
                }
            }
            #endif
            foreach (Player p in players)
            {

                if (p.invincible > 0)
                {
                    p.invincible--;
                }
                if (!p.mkb)
                {
                    p.location.X += p.speed * p.state.ThumbSticks.Left.X;
                    p.location.Y -= p.speed * p.state.ThumbSticks.Left.Y;
                    p.vibration();
                }
                else
                {
                    if (p.kstate.IsKeyDown(Keys.A))
                        p.location.X -= p.speed;
                    if (p.kstate.IsKeyDown(Keys.D))
                        p.location.X += p.speed;
                    if (p.kstate.IsKeyDown(Keys.W))
                        p.location.Y -= p.speed;
                    if (p.kstate.IsKeyDown(Keys.S))
                        p.location.Y += p.speed;
                }

                if (p.location.X < 0)
                    p.location.X = 0;
                if (p.location.Y < 0)
                    p.location.Y = 0;
                if (p.location.X > screenWidth)
                    p.location.X = screenWidth;
                if (p.location.Y > screenHeight)
                    p.location.Y = screenHeight;

                if (!p.mkb)
                {
                    p.setDirection(p.state.ThumbSticks.Right.X, p.state.ThumbSticks.Right.Y);
                }
                else
                {
                    p.setDirection(p.mstate.X - p.location.X * ((float)screenX / 1600f), p.location.Y * ((float)screenX / 1600f) - p.mstate.Y);
                }

                int BULLETCOOLDOWN = 8;
                if (p.fireCooldown >= BULLETCOOLDOWN && (((p.state.ThumbSticks.Right.X != 0 || p.state.ThumbSticks.Right.Y != 0) || (p.mkb && p.mstate.LeftButton == ButtonState.Pressed)) && p.alive))
                {
                    bullets.Add(new Bullet(p.location, p.angle, 25-players.Count, p));
                    p.fireCooldown = 0;
                    p.changeMass(-(6 + bigGuyCount / bigGuyMax * 3));
                }

                //Right Trigger Pressed.    Determines if player can shoot bomb if alive and enough health
                if (((!p.mkb && p.state.Triggers.Right > 0) || (p.mkb && p.mstate.RightButton == ButtonState.Pressed)) && p.mass > p.bombCharge && p.alive)
                {
                    p.bombCharge += .5f;
                    p.changeMass( -(.5f+players.Count/2f));
                    foreach( Bomb b in bombs)
                    {
                        if (b.owner == p)
                        {
                            b.detonate = true;
                        }
                    }
                }

                //Right Trigger Released
                if (((!p.mkb && p.state.Triggers.Right == 0) || (p.mkb && p.mstate.RightButton == ButtonState.Released)) && p.bombCharge > 10 && p.alive)
                {
                    bombs.Add(new Bomb(p.location, p.angle, 5f, p, p.bombCharge / 2, p.bombCharge / 2));
                    p.bombCharge = 0;
                }

                //Left Trigger Pressed/Released for laser
                if (((p.state.Triggers.Left > 0) || (p.mkb && p.kstate.IsKeyDown(Keys.Space))) && !p.LTriggerPressed && p.laserCooldown >= 30 && p.alive)
                {
                    p.changeMass(-20-5*players.Count);
                    p.laserCooldown = 0;
                    p.LTriggerPressed = true;
                    lasers.Add(new Bullet(p.location, p, true));
                }

                if (p.state.Triggers.Left == 0)
                    p.LTriggerPressed = false;


                if (p.fireCooldown < BULLETCOOLDOWN)
                    p.fireCooldown += 1;
                if (p.laserCooldown < 30)
                    p.laserCooldown += 1;
                if ((int)p.mass <= 0 && p.alive)
                {
                    p.alive = false;
                    gameOver = Lives == 0;
                    fadeIn = 0;
                    foreach (Player p2 in players)
                    {
                        if (p2.alive)
                        {
                            gameOver = false;
                        }
                    }

                    if (gameOver)
                    {
                        //sound1.Play();
                    }

                    for (int i = 0; i < 50-players.Count*5; i++)
                    {
                        particles.Add(new Particle(p.location, random.Next(10, 30), (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(3, 15), random.Next(10, 50)));
                    }
                }
            }
            foreach (Particle p in particles)
            {
                p.life--;
                if (p.life == 0)
                {
                    MarkedForDelete.Add(p);
                }
                p.location.X += (float)Math.Cos(p.direction) * p.speed;
                p.location.Y -= (float)Math.Sin(p.direction) * p.speed;
            }
            DeleteMarked();

            foreach(Bullet b in bullets)
            {
                b.location.X += (float)Math.Cos(b.angle) * b.speed;
                b.location.Y -= (float)Math.Sin(b.angle) * b.speed;
                Rectangle tempRectB = new Rectangle((int)b.location.X - (int)b.size, (int)b.location.Y - (int)b.size, (int)b.size * 2, (int)b.size * 2);
                foreach(Enemy e in enemies)
                {
                    Rectangle tempRectE = new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2);
                    if (tempRectE.Intersects(tempRectB) && !e.chaser)
                    {
                        b.owner.changeMass(22-players.Count/2f);
                        int damage = 5000;
                        if (e.mass <= damage)
                        {
                            MarkedForDelete.Add(e);

                            for (int k = random.Next((int)e.radius)/2 + 3; k > 0; k--)
                            {

                                if (e.bigGuy)
                                {

                                    e.bigGuy = false;
                                    k += 5;
                                }

                                if (k > 25) k = 25;
                                particles.Add(new Particle(e.location, k, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                            }

                        }
                        e.changeMass(-damage);

                        MarkedForDelete.Add(b);

                        break;
                    }
                }

                foreach (Player p in players)
                {
                    if (p.playerNum != b.ownerNum)
                    {
                        Rectangle tempRectP = new Rectangle((int)p.location.X - (int)p.radius, (int)p.location.Y - (int)p.radius, (int)p.radius * 2, (int)p.radius * 2);
                        Rectangle tempRectP2 = new Rectangle((int)b.owner.location.X - (int)b.owner.radius, (int)b.owner.location.Y - (int)b.owner.radius, (int)b.owner.radius * 2, (int)b.owner.radius * 2);
                        if (tempRectB.Intersects(tempRectP) && !tempRectP2.Intersects(tempRectP))
                        {
                            p.changeMass(-25 - 5 * players.Count);
                            p.setVib(1f);
                            MarkedForDelete.Add(b);
                            //TODO: Add this back in
                            //for (int m = 0; m < 3; m++)
                            //    bullets.Add(new Bullet(p.location, p.angle + random.NextDouble() * 2 * (Math.PI / 10) - (Math.PI / 10), 20, p));

                            break;
                        }
                    }
                }

                if (b.location.X >= screenWidth || b.location.X <= 0 || b.location.Y >= screenHeight || b.location.X <= 0)
                {
                    MarkedForDelete.Add(b);
                }
            }
            DeleteMarked();

            foreach (Bomb b in bombs)
            {
                if (!b.detonate)
                {
                    b.location.X += (float)Math.Cos(b.angle) * b.speed;
                    b.location.Y -= (float)Math.Sin(b.angle) * b.speed;
                    Rectangle tempRectB = new Rectangle((int)b.location.X - (int)b.size / 2, (int)b.location.Y - (int)b.size / 2, (int)b.size, (int)b.size);
                    foreach(Enemy e in enemies)
                    {
                        Rectangle tempRectE = new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2);
                        if (tempRectE.Intersects(tempRectB))
                        {
                            b.owner.changeMass(10);
                            int damage = (int)(b.size * 100);
                            if (e.mass <= damage)
                            {
                                MarkedForDelete.Add(e);

                                for (int k = random.Next((int)e.radius)/2 + 3; k > 0; k--)
                                {

                                    if (e.bigGuy)
                                    {
                                        e.bigGuy = false;
                                        k += 5;
                                    }

                                    if (k > 25) k = 25;
                                    particles.Add(new Particle(e.location, k, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                                }
                            }
                            e.changeMass(-damage);
                            if (b.size >= 10)
                            {
                                //bombs.Add(new Bomb(b.location, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), b.speed / 2, b.owner, b.size / 3, b.eLife/3));
                                //bombs.Add(new Bomb(b.location, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), b.speed / 2, b.owner, b.size / 2, b.eLife / 2));
                                b.size /= 2;
                            }
                            else
                            {
                                MarkedForDelete.Add(b);
                            }
                            break;
                        }
                    }

                }
                else
                {
                    if (b.eLife > 0 && b.eLife > b.oLife / 2)
                    {
                        b.eLife -= .01f * b.oLife;
                        b.size += 0.2f * b.oLife;
                        if(random.Next(3) == 0)
                        particles.Add(new Particle(b.location, 10, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                        //particles.Add(new Particle(b.location, 10, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                    }
                    else if (b.eLife > 0 && b.eLife < b.oLife / 2)
                    {
                        b.eLife -= .02f * b.oLife;
                        b.size -= .3f * b.oLife;
                        //particles.Add(new Particle(b.location, 10, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                        //particles.Add(new Particle(b.location, 10, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                    }
                    else
                    {
                        b.detonate = false;
                        MarkedForDelete.Add(b);
                    }
                    Rectangle tempRectB = new Rectangle((int)b.location.X - (int)b.size, (int)b.location.Y - (int)b.size, (int)b.size * 2, (int)b.size * 2);
                    foreach( Enemy e in enemies)
                    {
                        Rectangle tempRectE = new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2);
                        if (tempRectE.Intersects(tempRectB))
                        {
                            b.owner.changeMass(8-players.Count);
                            int damage = (int)(b.eLife * 10);
                            if (e.mass <= damage)
                            {
                                MarkedForDelete.Add(e);

                                for (int k = 3; k > 0; k--)
                                {
                                   
                                    particles.Add(new Particle(e.location, k, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));
                                }
                            }
                            e.changeMass(-damage);
                        }
                    }
                }

            }
            DeleteMarked();

            foreach(Enemy e in enemies)
            {

                e.location.X -= e.hspeed;
                e.location.Y += e.vspeed;

                //If outside vertical borders, bounce!
                if (e.location.Y < 0 || e.location.Y > screenHeight)
                    e.vspeed *= -1;

                if (e.bigGuy && random.Next(200) <= bigGuyCount / 2)
                {
                    if (players.Count > 0)
                    {
                        Player p = players.ElementAt(random.Next(0, players.Count));
                        if (p.alive)
                        {

                            float xspeed = p.location.X < e.location.X ? 10 : -10;

                            float yspeed = -xspeed * (e.location.Y - p.location.Y) / (e.location.X - p.location.X);

                            yspeed = 15 * yspeed / (Math.Abs(xspeed) + Math.Abs(yspeed));
                            xspeed = 15 * xspeed / (Math.Abs(xspeed) + Math.Abs(yspeed));

                            SpawnChasers.Add( new Enemy(e.location.X, e.location.Y, xspeed, yspeed, 2000, false, true));
                        }
                    }
                }

                Rectangle tempRectE = new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2);
                foreach (Player p in players)
                {
                    if (p.alive)
                    {
                        Rectangle tempRectP = new Rectangle((int)p.location.X - (int)p.radius, (int)p.location.Y - (int)p.radius, (int)p.radius * 2, (int)p.radius * 2);
                        if (tempRectE.Intersects(tempRectP))
                        {
                            if (p.invincible == 0)
                            {
                                p.changeMass(-(e.mass / 25 + 10 * players.Count + bigGuyCount));
                            }
                            p.setVib(1f);
                            MarkedForDelete.Add(e);
                        }
                    }
                }
                if (e.location.X <= -e.radius * 2)
                {
                    MarkedForDelete.Add(e);
                }
                else if (e.location.X > screenWidth + e.radius && e.chaser)
                {
                    MarkedForDelete.Add(e);
                }
            }
            foreach (Enemy e in SpawnChasers)
                enemies.Add(e);
            SpawnChasers.Clear();

            if (enemies.Count < 25 + players.Count * 25 && random.Next(15) < players.Count+3)
            {
                bool Exists = false;
                foreach (Player p in players)
                    if (p.alive)
                        Exists = true;
                if (random.Next(2000) <= bigGuyCount + 10 * players.Count && Exists)
                {
                    if (bigGuyCount < bigGuyMax)
                        bigGuyCount++;

                    enemies.Add(new Enemy(screenWidth, random.Next(screenHeight), random.Next(players.Count + (bigGuyCount / 4), players.Count + (bigGuyCount / 3)), random.Next(-8, 8), random.Next(50, 100) * 1000, true, false));
                }
                else
                {
                    enemies.Add(new Enemy(screenWidth, random.Next(screenHeight), random.Next(players.Count + (bigGuyCount / 3), players.Count + 4 + (bigGuyCount / 2)), random.Next(-8, 8), random.Next(150, 900)+players.Count*50, false, false));
                }
            }
            DeleteMarked();

            foreach(Bullet l in lasers)
            {
                if (l.time == 0)
                {
                    Rectangle tempRectL = new Rectangle((int)l.location.X - (int)l.size, (int)l.location.Y - (int)l.size, (int)l.size * 1000, (int)l.size * 2);
                    foreach( Enemy e in enemies)
                    {
                        Rectangle tempRectE = new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2);
                        if (tempRectE.Intersects(tempRectL) && !e.chaser)
                        {
                            l.owner.changeMass(e.bigGuy ? 30 - players.Count * 2 : 20 - players.Count * 2);
                            
                            int damage = 5000;
                            damage = e.bigGuy ? 10000 : 5000;
                            

                            if (e.mass <= damage)
                            {
                                MarkedForDelete.Add(e);
                                for (int k = random.Next((int)e.radius)/2 + 3; k > 0; k--)
                                {
                                    if (e.bigGuy)
                                    {
                                        l.owner.changeMass(50-players.Count*2);
                                        e.bigGuy = false;
                                        k += 5;
                                    }
                                    if (k > 25) k = 25;
                                    particles.Add(new Particle(e.location, k, (float)(random.NextDouble() * 2 * MathHelper.Pi - MathHelper.Pi), random.Next(5, 20), random.Next(5, 20)));

                                }
                            }
                            e.changeMass(-damage);
                        }

                    }

                    Rectangle tempRectP;
                    foreach (Player p in players)
                    {
                        if (p.playerNum != l.ownerNum)
                        {
                            tempRectP = new Rectangle((int)p.location.X - (int)p.radius, (int)p.location.Y - (int)p.radius, (int)p.radius * 2, (int)p.radius * 2);
                            if (tempRectL.Intersects(tempRectP))
                            {
                                p.changeMass(-50 - 10 * players.Count);
                                p.setVib(1f);
                        
                                for (int m = 0; m < 6; m++)
                                    bullets.Add(new Bullet(p.location, p.angle + random.NextDouble() * 2 * (Math.PI / 10) - (Math.PI / 10), 20, p));

                                break;
                            }
                        }
                    }
                }

                l.time++;
                if (l.time > 10)
                    MarkedForDelete.Add(l);
            }
            DeleteMarked();



            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime >= TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }


            // TODO: Add your update logic here
            if(!musicPlaying){

                if (!MediaPlayer.State.Equals(MediaState.Playing) && MediaPlayer.GameHasControl)
                {   
                    MediaPlayer.Volume = .4f;
                    
                    //MediaPlayer.Play(song2);
                    MediaPlayer.Play(song1);

                    

                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.IsShuffled = true;
                    musicPlaying = true;

                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            GraphicsDevice.Clear(Color.Black);

            //Matrix scaleMatrix = Matrix.CreateScale((float)screenX / 1600f);//Force 16:9 Resolution

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, scaleMatrix);
            //spriteBatch.Begin(SpriteSortMode.Immediate);

            //*************
            //AURA
            //******///
            foreach (Player p in players)
            {
                if (p.alive)
                {
                    int scale = 75-10*players.Count;
                    int size = (int)(scale * p.radius);
                    spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - size / 2, (int)p.location.Y - size / 2, size, size), color50);
                    if(p.invincible > 0)
                    {
                        size = (int)((size / 3f)*(p.invincible/(float)STARTING_INVINCIBLE));
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - size / 2, (int)p.location.Y - size / 2, size, size), new Color(.5f, .6f, .5f, .05f));
                    }
                }
            }

            foreach (Enemy e in enemies)
            {
                if (e.bigGuy)
                {
                    int scale = 10;
                    int size = (int)(scale * e.radius);
                    spriteBatch.Draw(blackBlend, new Rectangle((int)(e.location.X - size / 2), (int)(e.location.Y - size / 2), size, size), color25);
                }
                else
                {
                    int scale = 20;
                    int size = (int)(scale * e.radius);
                    spriteBatch.Draw(blackBlend, new Rectangle((int)(e.location.X - size / 2), (int)(e.location.Y - size / 2), size, size), color25);
                }
            }

            foreach (Particle p in particles)
            {
                int scale = 20;
                int size = (int)(p.size * scale * p.life / 10);
                spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - size / 2, (int)p.location.Y - size / 2, size, size), color25);
            }

            foreach (Bullet b in bullets)
            {
                int scale = 50-players.Count;
                int size = (int)(scale * b.size);
                spriteBatch.Draw(whiteBlend, new Rectangle((int)(b.location.X - size / 2), (int)(b.location.Y - size / 2), size, size), color50);
            }

            foreach (Bomb b in bombs)
            {
                int scale = 20;
                int size = (int)(scale * b.size);
                spriteBatch.Draw(whiteBlend, new Rectangle((int)(b.location.X - size / 2), (int)(b.location.Y - size / 2), size, size), color25);
            }

            foreach (Bullet l in lasers)
            {
                int scale = 10;
                int size = (int)(scale * l.size);
                spriteBatch.Draw(whiteBlend, new Rectangle((int)(l.location.X - size), (int)(l.location.Y - size / 2), size * 10, size), color75);
            }

            //***MENU STUFF**//
            if (players.Count == 0 || !gameStart)
            {
                if (fadeIn < 1)
                    fadeIn += .005f;

                Color fadeColor = new Color(fadeIn, fadeIn, fadeIn, fadeIn);

                spriteBatch.Draw(ShmadowTitle, new Rectangle(screenWidth / 2 - 200, screenHeight / 2 - 200, 400, 400), fadeColor);
            }
            else if (gameOver)
            {
                if (startScreenReloadTime < 100)
                {
                    fadeIn -= .02f;
                }
                else if (fadeIn < 1)
                    fadeIn += .02f;

                startScreenReloadTime-=2;

                Color fadeColor = new Color(fadeIn, fadeIn, fadeIn, fadeIn);
                spriteBatch.Draw(gameOverBack, new Rectangle(screenWidth / 2 - 200, screenHeight / 2 - 200, 400, 400), fadeColor);
                spriteBatch.Draw(gameOverText, new Rectangle(screenWidth / 2 - 200, screenHeight / 2 - 200, 400, 400), fadeColor);
            }

            //****Physical Object STUFF**//
            foreach (Particle p in particles)
            {
                spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.size / 2, (int)p.location.Y - (int)p.size / 2, (int)p.size, (int)p.size), Color.White);
            }

            int t = 0;
            foreach (Player p in players)
            {
                if (p.alive)
                {
                    if (p.index == PlayerIndex.One)
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius - 1, (int)p.location.Y - (int)p.radius - 1, (int)p.radius * 2 + 2, (int)p.radius * 2 + 2), Color.Blue);
                    else if (p.index == PlayerIndex.Two)
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius - 1, (int)p.location.Y - (int)p.radius - 1, (int)p.radius * 2 + 2, (int)p.radius * 2 + 2), Color.Green);
                    else if (p.index == PlayerIndex.Three)
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius - 1, (int)p.location.Y - (int)p.radius - 1, (int)p.radius * 2 + 2, (int)p.radius * 2 + 2), Color.HotPink);
                    else if (p.index == PlayerIndex.Four)
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius - 1, (int)p.location.Y - (int)p.radius - 1, (int)p.radius * 2 + 2, (int)p.radius * 2 + 2), Color.Goldenrod);
                    else
                        spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius - 1, (int)p.location.Y - (int)p.radius - 1, (int)p.radius * 2 + 2, (int)p.radius * 2 + 2), Color.Purple);
                    t++;
                    spriteBatch.Draw(whiteBlend, new Rectangle((int)p.location.X - (int)p.radius, (int)p.location.Y - (int)p.radius, (int)p.radius * 2, (int)p.radius * 2), Color.White);
                }

            }

            foreach (Enemy e in enemies)
            {
                int scale = 25;
                int size = (int)(scale * e.radius);
                spriteBatch.Draw(blackBlend, new Rectangle((int)(e.location.X - size / 2), (int)(e.location.Y - size / 2), size, size), color25);
            }

            foreach (Enemy e in enemies)
            {   
                
                if (e.bigGuy)
                {
                    int scale = 60;
                    int size = (int)(2 * e.radius + scale);

                    spriteBatch.Draw(whiteBlend, new Rectangle((int)e.location.X - size / 2, (int)e.location.Y - size / 2, size, size), new Color(1f, 0.5f, 1f, 1f));
                }
            }
            foreach (Enemy e in enemies)
            {   
                if (e.chaser)
                {
                    int scale = 2;
                    int size = (int)(2 * e.radius + scale);
                    spriteBatch.Draw(whiteBlend, new Rectangle((int)e.location.X - size / 2, (int)e.location.Y - size / 2, size, size), Color.Red);
                }
            }

            foreach (Enemy e in enemies)
            {
                spriteBatch.Draw(blackBlend, new Rectangle((int)e.location.X - (int)e.radius, (int)e.location.Y - (int)e.radius, (int)e.radius * 2, (int)e.radius * 2), Color.Black);
            }

            foreach (Bullet b in bullets)
            {
                spriteBatch.Draw(whiteBlendS, new Rectangle((int)(b.location.X - b.size / 2), (int)(b.location.Y - b.size / 2), (int)b.size, (int)b.size), Color.White);
            }

            foreach (Bomb b in bombs)
            {
                int scale = 2;
                int size = (int)(b.size * scale);
                spriteBatch.Draw(whiteBlend, new Rectangle((int)(b.location.X - size / 2), (int)(b.location.Y - size / 2), size, size), Color.White);
            }

            foreach (Bullet l in lasers)
            {
                spriteBatch.Draw(whiteBlendS, new Rectangle((int)(l.location.X - l.size / 2), (int)(l.location.Y - l.size), (int)l.size * 300, (int)l.size * 2), Color.White);
            }
            if (Lives > 0)
            {
                spriteBatch.Draw(whiteBlendS, new Rectangle(0, 0, 36, 36), Color.White);
                spriteBatch.DrawString(font, Lives + "", new Vector2(15, 11), Color.Black);
            }

            //spriteBatch.DrawString(font, frameRate+"", new Vector2(), Color.White);

            spriteBatch.End();


            base.Draw(gameTime);
        }

        private void DeleteMarked()
        {
            foreach (Object m in MarkedForDelete)
            {
                if (m is Particle)
                {
                    particles.Remove((Particle)m);
                }
                else if (m is Bullet)
                {
                    if (((Bullet)m).isLaser == true)
                        lasers.Remove((Bullet)m);
                    else
                        bullets.Remove((Bullet)m);
                }
                else if (m is Enemy)
                {
                    enemies.Remove((Enemy)m);
                }
                else if (m is Bomb)
                {
                    bombs.Remove((Bomb)m);
                }
            }
            MarkedForDelete.Clear();
        }
    }
}
