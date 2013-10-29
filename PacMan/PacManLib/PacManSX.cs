#region File Description
    //////////////////////////////////////////////////////////////////////////
   // PacManSX                                                             //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PacManLib.Map;
using PacManLib.GameObjects;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Devices.Sensors;
#endif
#endregion End of Using Statements

namespace PacManLib
{
    /// <summary>
    /// This is the main game class.
    /// </summary>
    public sealed class PacManSX
    {
        #region Consts
#if WINDOWS_PHONE
        public const float AccelerometerNoise = 0.2f;
#endif

        public const int TitleHeight = 20;

        public const int TileWidth = 20;
        public const int TileHeight = 20;

        public const int CharacterWidth = 28;
        public const int CharacterHeight = 28;

        public const int GhostScore = 200;
        public const int RingScore = 10;
        public const int DotScore = 50;
        public static readonly int[] FruitScore = 
        {
            100, 300, 500, 700, 1000, 2000, 3000, 5000
        };

        public const int BulletCost = 0;
        public const int IronBulletCost = 0;

        public const int FruitDespawnInSeconds = 30;
        public const int FruitMinSpawnTimerInSeconds = 0;
        public const int FruitMaxSpawnTimerInSeconds = 10;
        public const int GodModeActiveInSeconds = 10;

        #endregion

        #region Fields

#if WINDOWS_PHONE
        private Accelerometer accelerometer = null;
        private Vector3 accelReading = new Vector3();
        private bool accelActive = false;
        private bool accelInitialized = false;
        private float lastX = 0;
        private float lastY = 0;
#endif

        private bool ironBullet = false;
        private bool fruitSpawned = false;
        private bool bulletAlive = false;
        private bool gameOver = false;
        private bool gameStarted = false;
        
        private int fruitSpawnTime = -1;
        private int gameCountdown = 3;
        private int dotsAndRingsLeft = 0;
        private int lives = 3;
        private int score = 0;

        private float fruitSpawnTimer = 0;
        private float startGameTimer = 0;
        private float godModeTimer = 0;

        private Player player = null;
        private TileMap tileMap = null;
        private Tileset fruitTileset = null;

        private Random rand = null;

        private Ghost purpleGhost = null;
        private Ghost yellowGhost = null;
        private Ghost blueGhost = null;
        private Ghost greenGhost = null;

        private SpriteFont font = null;

        private Rectangle fruitBounds;

        private Vector2 levelPosition;
        private Vector2 scorePosition;
        private Vector2 bulletPosition;
        private Vector2 bulletMotion;

        private Texture2D bulletTexture;
        private Texture2D BlackTexture;
        private Texture2D lifeTexture;

        private SoundEffect soundGodMode;
        private SoundEffectInstance soundEngine;
        private SoundEffect soundChomp;
        private SoundEffect soundEatScore;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the game manager.
        /// </summary>
        public GameManager GameManager { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new PacMan Shooter Extreme object.
        /// </summary>
        /// <param name="gameManager">The game manager.</param>
        public PacManSX(GameManager gameManager)
        {
            this.GameManager = gameManager;
            this.font = this.GameManager.ContentManager.Load<SpriteFont>("Font");
            this.levelPosition = new Vector2(4, 0);
            this.scorePosition = new Vector2(120, 0);

            this.rand = new Random();

            this.bulletTexture = this.GameManager.ContentManager.Load<Texture2D>("Bullet");
            this.lifeTexture = this.GameManager.ContentManager.Load<Texture2D>("Life");
            this.BlackTexture = this.GameManager.ContentManager.Load<Texture2D>("BlackTexture");
            this.fruitTileset = new Tileset(this.GameManager.ContentManager.Load<Texture2D>("Tiles/Fruits"),
                PacManSX.TileWidth, PacManSX.TileHeight);

            // Create the tile map and load the first map.
            this.tileMap = new TileMap(gameManager);
            this.LoadMap(1);

            // Load all the spawns and initialize all the players if they exists.
            int i = 0;
            Point[] spawnCoords = this.tileMap.GetSpawns(SpawnPoint.Player, SpawnPoint.BlueGhost, SpawnPoint.GreenGhost, SpawnPoint.YellowGhost, SpawnPoint.PurpleGhost);

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.player = new Player(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]),
                    this.GameManager.ContentManager.Load<Texture2D>("Pacman"), this.GameManager.ContentManager.Load<Texture2D>("PacmanGodMode"), 
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight) { Direction = Direction.Right };
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.blueGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]),
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/BlueGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/BlueGhostGodMode"), 
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight) { Direction = Direction.Right };
                this.blueGhost.GhostAI += blueGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.greenGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]),
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GreenGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GreenGhostGodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight) { Direction = Direction.Right };
                this.greenGhost.GhostAI += greenGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.yellowGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]),
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/YellowGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/YellowGhostGodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight) { Direction = Direction.Right };
                this.yellowGhost.GhostAI += yellowGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.purpleGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]),
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/PurpleGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/PurpleGhostGodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight) { Direction = Direction.Right };
                this.purpleGhost.GhostAI += purpleGhostAI;
            }

#if WINDOWS_PHONE
            // Initialize Windows phone stuff.
            TouchPanel.EnabledGestures = GestureType.Tap;
            this.accelerometer = new Accelerometer();
            this.accelerometer.CurrentValueChanged += accelerometer_CurrentValueChanged;
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows the game to update itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last update.</param>
        public void Update(TimeSpan elapsedGameTime)
        {
            // Only update the game if it has started.
            if (gameStarted)
            {
                // Check if the player has eaten all the dots and rings.
                if (this.dotsAndRingsLeft == 0)
                {
                    // Then load the new map and reset all the positions.
                    LoadMap(this.tileMap.Level + 1);
                    ResetPositions();
                }

                // Check if player is in godmode, then start the coutdown and remove godmode after PacManSX.GodModeActiveInSeconds.
                if (this.player.GodMode)
                {
                    this.godModeTimer += (float)elapsedGameTime.TotalSeconds;

                    if (this.godModeTimer >= PacManSX.GodModeActiveInSeconds)
                    {
                        this.player.GodMode = false;
                        this.godModeTimer = 0;
                    }
                }

                // Check if we should spawn a fruit.
                if (!this.fruitSpawned && this.fruitSpawnTime >= 0)
                {
                    this.fruitSpawnTimer += (float)elapsedGameTime.TotalSeconds;
                    if (this.fruitSpawnTimer >= this.fruitSpawnTime)
                    {
                        // Spawn the fruit.
                        Point fruitSpawnCoords = this.tileMap.GetSpawn(SpawnPoint.Fruit);
                        if (fruitSpawnCoords.X >= 0 && fruitSpawnCoords.Y >= 0)
                        {
                            Vector2 fruitPos = PacManSX.ConvertCellToPosition(fruitSpawnCoords);
                            this.fruitBounds = new Rectangle(
                                (int)fruitPos.X, (int)fruitPos.Y, 
                                PacManSX.TileWidth, PacManSX.TileHeight);

                            // Create a new SpawnTime and reset the timer.
                            this.fruitSpawnTime = PacManSX.FruitDespawnInSeconds;
                            this.fruitSpawnTimer = 0;
                            this.fruitSpawned = true;
                        }
                        else
                        {
                            // If spawnCoords are out of bound then just don't spawn any fruit.
                            this.fruitSpawnTime = -1;
                        }
                    }
                }
                else if (this.fruitSpawned && this.fruitSpawnTime == PacManSX.FruitDespawnInSeconds)
                {
                    if (this.player.Bounds.Intersects(this.fruitBounds))
                    {
                        int fruitIndex = this.tileMap.Level - 1 % PacManSX.FruitScore.GetLength(0);
                        this.score += PacManSX.FruitScore[fruitIndex];
                        this.fruitSpawnTime = rand.Next(PacManSX.FruitMinSpawnTimerInSeconds, PacManSX.FruitMaxSpawnTimerInSeconds);
                        this.fruitSpawnTimer = 0;
                        this.fruitSpawned = false;
                    }
                    else
                    {
                        // Or if the fruit is spawned and the fruit spawn time is over 0 then despawn it after FruitSpawnTime
                        this.fruitSpawnTimer += (float)elapsedGameTime.TotalSeconds;
                        if (this.fruitSpawnTimer >= this.fruitSpawnTime)
                        {
                            // Despawn the fruit.
                            this.fruitSpawnTime = rand.Next(PacManSX.FruitMinSpawnTimerInSeconds, PacManSX.FruitMaxSpawnTimerInSeconds);
                            this.fruitSpawnTimer = 0;
                            this.fruitSpawned = false;
                        }
                    }
                }

                // Updates the player and movement.
                if (this.player != null)
                    this.player.Update(elapsedGameTime);

                if (this.blueGhost != null)
                    this.blueGhost.Update(elapsedGameTime);

                if (this.greenGhost != null)
                    this.greenGhost.Update(elapsedGameTime);

                if (this.yellowGhost != null)
                    this.yellowGhost.Update(elapsedGameTime);

                if (this.purpleGhost != null)
                    this.purpleGhost.Update(elapsedGameTime);

                // If the player is alive then handle the movement.
                if (this.player != null && this.player.Alive)
                    PlayerMovement(elapsedGameTime);

                // Update the movement for all ghosts.
                if (this.blueGhost != null)
                    this.blueGhost.Movement(elapsedGameTime, player, tileMap);

                if (this.greenGhost != null)
                    this.greenGhost.Movement(elapsedGameTime, player, tileMap);

                if (this.yellowGhost != null)
                    this.yellowGhost.Movement(elapsedGameTime, player, tileMap);

                if (this.purpleGhost != null)
                    this.purpleGhost.Movement(elapsedGameTime, player, tileMap);

                // Check for Player Ghost hitbox.
                PlayerGhostHitbox();

                // Check for bullet hitbox.
                if (this.bulletAlive && this.bulletMotion != Vector2.Zero)
                {
                    // Normalize the bullet motion and move the bullet.
                    this.bulletMotion.Normalize();

                    this.bulletPosition.X += (float)Math.Round((this.bulletMotion * this.player.Speed * 1.5f * (float)elapsedGameTime.TotalSeconds).X);
                    this.bulletPosition.Y += (float)Math.Round((this.bulletMotion * this.player.Speed * 1.5f * (float)elapsedGameTime.TotalSeconds).Y);

                    // Calculate the bounds of the bullet and check if it intersects with a ghost.
                    Rectangle bulletBounds = new Rectangle((int)this.bulletPosition.X, (int)this.bulletPosition.Y, 20, 20);
                    if (bulletBounds.Intersects(this.purpleGhost.Bounds))
                        this.bulletAlive = false;

                    // Convert the center of the bullet to the bullet coordinates.
                    Point bulletCoords = PacManSX.ConvertPositionToCell(this.bulletPosition + new Vector2(10));
                    Tile bulletTile = this.tileMap.GetTile(bulletCoords);

                    // Check if the bullet tile is colliding with a wall or a type of barrier.
                    if (bulletTile.TileContent > TileContent.Path && bulletTile.TileContent < TileContent.WoodBarrier || !this.ironBullet && bulletTile.TileContent == TileContent.IronBarrier)
                        this.bulletAlive = false;
                    else if (bulletTile.TileContent == TileContent.WoodBarrier)
                    {
                        // Destroy the wood barrier if the bullet hits it.
                        this.bulletAlive = false;
                        this.tileMap.UpdateTile(bulletCoords, TileContent.Path);
                    }
                    else if (bulletTile.TileContent == TileContent.IronBarrier && this.ironBullet)
                    {
                        // Destroy the iron barrier if the bullet hits it and it's a iron bullet.
                        this.bulletAlive = false;
                        this.tileMap.UpdateTile(bulletCoords, TileContent.Path);
                    }
                }
            }
            else
            {
#if WINDOWS_PHONE
                try
                {
                    this.accelerometer.Stop();
                    this.accelActive = false;
                }
                catch (Exception) { }
#endif
                this.startGameTimer += (float)elapsedGameTime.TotalSeconds;

                if (this.startGameTimer >= 1)
                {
                    // Decrease the countdown by one.
                    this.gameCountdown--;

                    // If we've reached -1 then start the game.
                    if (this.gameCountdown == -1)
                    {
                        // Reset the lives and score if it's game over.
                        if (this.gameOver)
                        {
                            this.gameOver = false;
                            this.lives = 3;
                            this.score = 0;
                        }

                        this.fruitSpawnTime = rand.Next(PacManSX.FruitMinSpawnTimerInSeconds, PacManSX.FruitMaxSpawnTimerInSeconds);

                        this.gameStarted = true;
#if WINDOWS_PHONE
                        try
                        {
                            this.accelerometer.Start();
                            this.accelActive = true;
                        }
                        catch (Exception)
                        {
                            this.accelActive = false;
                        }
#endif
                    }

                    this.startGameTimer = 0;
                }
            }
        }
        
        /// <summary>
        /// Allows the game to draw itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last draw.</param>
        public void Draw(TimeSpan elapsedGameTime)
        {
            // Clear the screen to black and draw the map and player.
            this.GameManager.SpriteBatch.GraphicsDevice.Clear(new Color(68, 68, 68));

            this.tileMap.Draw(elapsedGameTime);
            if (this.player != null)
                this.player.Draw(elapsedGameTime, this.player.GodMode);

            // Draws all the ghosts.
            if (this.blueGhost != null)
                this.blueGhost.Draw(elapsedGameTime, this.player.GodMode);

            if (this.greenGhost != null)
                this.greenGhost.Draw(elapsedGameTime, this.player.GodMode);

            if (this.yellowGhost != null)
                this.yellowGhost.Draw(elapsedGameTime, this.player.GodMode);

            if (this.purpleGhost != null)
                this.purpleGhost.Draw(elapsedGameTime, this.player.GodMode);

            this.GameManager.SpriteBatch.Begin();

            // Draw the bullet
            if (this.bulletAlive)
                this.GameManager.SpriteBatch.Draw(this.bulletTexture, this.bulletPosition, Color.White);

            // Draw the fruit
            if (this.fruitSpawned)
                this.GameManager.SpriteBatch.Draw(this.fruitTileset.Texture, this.fruitBounds, this.fruitTileset.GetSourceRectangle(this.tileMap.Level - 1), Color.White);

            // Draw the GUI.
            this.GameManager.SpriteBatch.DrawString(this.font, "Level: " + this.tileMap.Level, this.levelPosition, Color.White);
            for (int i = 0; i < this.lives; i++)
                this.GameManager.SpriteBatch.Draw(this.lifeTexture, new Vector2(GameManager.ScreenWidth - lives * 20 - 4, 4) + new Vector2(20 * i, 0), Color.White);
            this.GameManager.SpriteBatch.DrawString(this.font, "Score: " + this.score, this.scorePosition, Color.White);
            this.GameManager.SpriteBatch.End();
            
            // If the game has not started then draw a countdown.
            if (!this.gameStarted)
            {
                this.GameManager.SpriteBatch.Begin();
                this.GameManager.SpriteBatch.Draw(this.BlackTexture, new Rectangle(0, 0, this.GameManager.ScreenWidth, this.GameManager.ScreenHeight), new Color(0, 0, 0, 120));

                Vector2 textSize = this.font.MeasureString("Starts in " + this.gameCountdown);
                Vector2 textCenter = new Vector2(this.GameManager.ScreenWidth / 2, this.GameManager.ScreenHeight / 2);
                this.GameManager.SpriteBatch.DrawString(this.font, "Starts in " + this.gameCountdown, textCenter - (textSize / 2), Color.White);

                // If it's game over then draw a "Game Over!" text.
                if (this.gameOver)
                {
                    textSize = this.font.MeasureString("Game Over!");
                    textCenter = new Vector2(this.GameManager.ScreenWidth / 2, this.GameManager.ScreenHeight / 2 - 50);
                    this.GameManager.SpriteBatch.DrawString(this.font, "Game Over!", textCenter - (textSize / 2), Color.Yellow);
                }

                this.GameManager.SpriteBatch.End();
            }
        }

        #endregion

        #region Private Helpers

#if WINDOWS_PHONE

        /// <summary>
        /// Method fired when the accelerometer value has changed.
        /// </summary>
        private void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            this.accelReading.X = (float)e.SensorReading.Acceleration.X;
            this.accelReading.Y = (float)e.SensorReading.Acceleration.Y;
            this.accelReading.Z = (float)e.SensorReading.Acceleration.Z;
        }

#endif

        /// <summary>
        /// Helper for loading a map.
        /// </summary>
        private void LoadMap(int level)
        {
            if (level == 1)
            {
                this.tileMap.LoadMap(level, new int[,]
                {
                    { 5, 1, 8, 0, 7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6 },
                    { 2, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 17, 2 },
                    { 2, 0, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 8, 0, 7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 1, 8, 17, 7, 1, 1, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 10, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 16, 0, 0, 0, 0, 16, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 10 },
                    { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 5, 1, 1, 6, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0 },
                    { 9, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 9 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 3, 1, 1, 4, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 0, 2, 16, 0, 0, 0, 0, 16, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 1, 1, 1, 1, 1, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2 },
                    { 2, 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4, 0, 2 },
                    { 2, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 17, 2 },
                    { 3, 1, 8, 0, 7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4 }
                });

                this.tileMap.UpdateTile(new Point(14, 17), SpawnPoint.Fruit);
                this.tileMap.UpdateTile(new Point(1, 1), SpawnPoint.Player);
                this.tileMap.UpdateTile(new Point(21, 1), SpawnPoint.BlueGhost);
                this.tileMap.UpdateTile(new Point(23, 1), SpawnPoint.GreenGhost);
                this.tileMap.UpdateTile(new Point(25, 1), SpawnPoint.YellowGhost);
                this.tileMap.UpdateTile(new Point(27, 1), SpawnPoint.PurpleGhost);
            }

            this.dotsAndRingsLeft = this.tileMap.DotsAndRings();
        }

        /// <summary>
        /// Method for reseting all the positions, this is used when the player dies or when the entering the next level.
        /// </summary>
        private void ResetPositions()
        {
            // If it's game over, reload the map.
            if (this.gameOver)
                this.LoadMap(1);

            int i = 0;
            Point[] spawnCoords = this.tileMap.GetSpawns(SpawnPoint.Player, SpawnPoint.BlueGhost, SpawnPoint.GreenGhost, SpawnPoint.YellowGhost, SpawnPoint.PurpleGhost);
            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.player.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.player.Direction = Direction.Right;
                this.player.Alive = true;
                this.player.GodMode = false;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.blueGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.blueGhost.Direction = Direction.Right;
                this.blueGhost.Alive = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.greenGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.greenGhost.Direction = Direction.Right;
                this.greenGhost.Alive = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.yellowGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.yellowGhost.Direction = Direction.Right;
                this.yellowGhost.Alive = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.purpleGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.purpleGhost.Direction = Direction.Right;
                this.purpleGhost.Alive = true;
            }

            this.fruitSpawned = false;

            // Start the game countdown.
            this.gameCountdown = 3;
            this.gameStarted = false;
        }

        /// <summary>
        /// Method runs when the player has walked over a dot tile.
        /// </summary>
        private void PlayerHasEatenDot(Tile tile)
        {
            // Change the tile to either a turn or path tile.
            if (tile.TileContent == TileContent.DotTurn)
                tile.TileContent = TileContent.Turn;
            else if (tile.TileContent == TileContent.Dot)
                tile.TileContent = TileContent.Path;

            // Add the score you get from a dot.
            score += DotScore;
            this.dotsAndRingsLeft--;

            // Turn on godMode.
            player.GodMode = true;
            this.godModeTimer = 0;

            //Play godmode music
            soundGodMode = GameManager.ContentManager.Load<SoundEffect>("Sounds/god_mode");
            soundGodMode.Play();
            soundEngine = soundGodMode.CreateInstance();
        }

        /// <summary>
        /// Method runs when the player has walked over a ring tile.
        /// </summary>
        private void PlayerHasEatenRing(Tile tile)
        {
            // Change the tile to either a turn or path tile.
            if (tile.TileContent == TileContent.RingTurn)
                tile.TileContent = TileContent.Turn;
            else if (tile.TileContent == TileContent.Ring)
                tile.TileContent = TileContent.Path;

            // Add the score you get from a ring.
            score += RingScore;
            this.dotsAndRingsLeft--;

            // Sound for walking over a coin/ring
            soundEatScore = GameManager.ContentManager.Load<SoundEffect>("Sounds/coin");
            soundEatScore.Play();
        }

        /// <summary>
        /// Method for handling the AI for the blue ghost.
        /// </summary>
        /// <param name="ghost">The ghost object.</param>
        /// <param name="ghostTile">The tile the ghost stands on.</param>
        /// <param name="ghostCoords">The tile coordinates of this ghost.</param>
        /// <param name="playerCoords">The tile coordinates of the player.</param>
        /// <param name="motion">The motion the ghost should move in.</param>
        /// <returns>Returns the direction the ghost should move in.</returns>
        private Direction blueGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            Tile targetTile = null;

            #region Ghost pathfinding for red behaviour

            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
            {
                int xDelta = Math.Abs((ghostCoords.X - playerCoords.X));
                int yDelta = Math.Abs((ghostCoords.Y - playerCoords.Y));

                if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X <= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }
            }

            #endregion

            return direction;
        }

        /// <summary>
        /// Method for handling the AI for the green ghost.
        /// </summary>
        /// <param name="ghost">The ghost object.</param>
        /// <param name="ghostTile">The tile the ghost stands on.</param>
        /// <param name="ghostCoords">The tile coordinates of this ghost.</param>
        /// <param name="playerCoords">The tile coordinates of the player.</param>
        /// <param name="motion">The motion the ghost should move in.</param>
        /// <returns>Returns the direction the ghost should move in.</returns>
        private Direction greenGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            Tile targetTile = null;

            #region Ghost pathfinding for red behaviour

            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
            {
                int xDelta = Math.Abs((ghostCoords.X - playerCoords.X));
                int yDelta = Math.Abs((ghostCoords.Y - playerCoords.Y));

                if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X <= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }
            }

            #endregion

            return direction;
        }

        /// <summary>
        /// Method for handling the AI for the yellow ghost.
        /// </summary>
        /// <param name="ghost">The ghost object.</param>
        /// <param name="ghostTile">The tile the ghost stands on.</param>
        /// <param name="ghostCoords">The tile coordinates of this ghost.</param>
        /// <param name="playerCoords">The tile coordinates of the player.</param>
        /// <param name="motion">The motion the ghost should move in.</param>
        /// <returns>Returns the direction the ghost should move in.</returns>
        private Direction yellowGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            Tile targetTile = null;

            #region Ghost pathfinding for red behaviour

            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
            {
                int xDelta = Math.Abs((ghostCoords.X - playerCoords.X));
                int yDelta = Math.Abs((ghostCoords.Y - playerCoords.Y));

                if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X <= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }
            }

            #endregion

            return direction;
        }

        /// <summary>
        /// Method for handling the AI for the purple ghost.
        /// </summary>
        /// <param name="ghost">The ghost object.</param>
        /// <param name="ghostTile">The tile the ghost stands on.</param>
        /// <param name="ghostCoords">The tile coordinates of this ghost.</param>
        /// <param name="playerCoords">The tile coordinates of the player.</param>
        /// <param name="motion">The motion the ghost should move in.</param>
        /// <returns>Returns the direction the ghost should move in.</returns>
        private Direction purpleGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            Tile targetTile = null;

            #region Ghost pathfinding for red behaviour

            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
            {
                int xDelta = Math.Abs((ghostCoords.X - playerCoords.X));
                int yDelta = Math.Abs((ghostCoords.Y - playerCoords.Y));

                if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X <= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y <= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }

                else if (ghostCoords.X >= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Left;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, out motion, out targetTile);
                                }

                            }
                        }
                    }
                }
            }

            #endregion

            return direction;
        }

        /// <summary>
        /// Returns the reverse direction from a given direction.
        /// </summary>
        /// <param name="direction">The direction you want to reverse.</param>
        /// <returns>The reversed direction.</returns>
        private Direction reverseMovement(Direction direction)
        {
            if (direction == Direction.Up)
                return Direction.Down;
            
            if(direction == Direction.Down)
                return Direction.Up;
            
            if(direction == Direction.Left)
                return Direction.Right;

            if(direction == Direction.Right)
                return Direction.Left;
            else
                return Direction.None;
        }

        /// <summary>
        /// Method for handling movement and input.
        /// </summary>
        private void PlayerMovement(TimeSpan elapsedGameTime)
        {
            // If the player goes outside the playfield, teleport the player to the other side.
            if (this.player.Position.X >= 772)
                this.player.Position.X = 0;
            else if (this.player.Position.X <= 0)
                this.player.Position.X = 772;

            if (this.player.Position.Y >= 452)
                this.player.Position.Y = PacManSX.TitleHeight;
            else if (this.player.Position.Y <= 0 + PacManSX.TitleHeight)
                this.player.Position.Y = 452;

            KeyboardState keyboardState = Keyboard.GetState();

            Direction direction = this.player.NextDirection;
            Vector2 motion = this.player.Motion;

            // Converts the center of the player to the players tile coordinates.
            Point playerCoords = PacManSX.ConvertPositionToCell(this.player.Center);
            Tile playerTile = tileMap.GetTile(playerCoords); // Get the tile the player is located at.
            Tile targetTile = null;

            // If we're running on windows, use the keyboard WASD to move and Space to shoot.
#if WINDOWS
            // Check for input, should we change direction?
            if (keyboardState.IsKeyDown(Keys.W))
                direction = Direction.Up;
            else if (keyboardState.IsKeyDown(Keys.S))
                direction = Direction.Down;
            else if (keyboardState.IsKeyDown(Keys.A))
                direction = Direction.Left;
            else if (keyboardState.IsKeyDown(Keys.D))
                direction = Direction.Right;

            if (keyboardState.IsKeyDown(Keys.Space))
                Fire();
#elif WINDOWS_PHONE
            // If we're using Windows Phone use the Tap gesture to fire and the accelerometer to move.

            int touchCount = TouchPanel.GetState().Count;
            // If Gesture is available.
            while (TouchPanel.IsGestureAvailable)
            {
                // If we've more than one touch location.
                if (touchCount > 1)
                {
                    // Check for two finger tap.
                    GestureSample firstGesture = TouchPanel.ReadGesture();
                    GestureSample secondGesture = TouchPanel.ReadGesture();

                    if (firstGesture.GestureType == GestureType.Tap && secondGesture.GestureType == GestureType.Tap)
                        TwoFingerFire();
                }
                else if (touchCount == 1)
                {
                    // Read it.
                    GestureSample gesture = TouchPanel.ReadGesture();

                    // If it's a Tap gesture then fire a bullet.
                    if (gesture.GestureType == GestureType.Tap)
                        Fire();
                }
            }

            // If the accelerometer is active.
            if (this.accelActive)
            {
                // Get the accelerometer values.
                float x = this.accelReading.X;
                float y = this.accelReading.Y;

                // Check if it's the first time we're here.
                if (!this.accelInitialized)
                {
                    // Initialize lastX and lastY.
                    lastX = x;
                    lastY = y;
                    this.accelInitialized = true;
                }
                else
                {
                    // If it's not the first time calculate deltaX and deltaY.
                    float deltaX = lastX - x;
                    float deltaY = lastY - y;

                    // Filter out small movements.
                    if (deltaX > -PacManSX.AccelerometerNoise && deltaX < PacManSX.AccelerometerNoise)
                        deltaX = 0;

                    if (deltaY > -PacManSX.AccelerometerNoise && deltaY < PacManSX.AccelerometerNoise)
                        deltaY = 0;

                    // Set the lastX and lastY to the current X and Y.
                    lastX = x;
                    lastY = y;

                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        // If ABS(deltaX) is larget then ABS(deltaY) then we're going either up or down.
                        if (deltaX < 0)
                            direction = Direction.Up;
                        else if (deltaX > 0)
                            direction = Direction.Down;
                    }
                    else
                    {
                        // Otherwise we're going Right or left.
                        if (deltaY < 0)
                            direction = Direction.Right;
                        else if (deltaY > 0)
                            direction = Direction.Left;
                    }
                }
            }
#endif

            // Check if the tile is a turn or path tile.
            if (playerTile.TileContent == TileContent.Turn || playerTile.TileContent == TileContent.Path
                || playerTile.TileContent >= TileContent.Ring && playerTile.TileContent <= TileContent.DotTurn)
            {
                // Convert the cell to a position.
                Vector2 playerTilePosition = PacManSX.ConvertCellToPosition(playerCoords);

                // Check if the player is right ontop of the tile.
                if (playerTilePosition == this.player.Position)
                {
                    // Check if the player can move in that direction
                    if (CanPlayerMove(playerCoords, direction, out motion, out targetTile))
                    {
                        this.player.Motion = motion;
                        this.player.Direction = direction;
                        this.player.NextDirection = direction;
                    }
                    else
                    {
                        // If the player can't move in that direction then check if the player can move in the old direction.
                        if (CanPlayerMove(playerCoords, this.player.Direction, out motion, out targetTile))
                        {
                            this.player.NextDirection = direction;
                            this.player.Motion = motion;
                        }
                    }
                    // If the player can't move in the old direction or the new then just stand still.
                }
                else
                {
                    // If the player is not right ontop of the tile then continue to move.
                    if (this.player.Direction == Direction.Up)
                    {
                        // Did we change direction to the opposite direction, then move in the opposite direction.
                        if (direction == Direction.Down)
                        {
                            motion.Y = 1;
                            this.player.Direction = direction;
                        }
                        else
                            motion.Y = -1;
                    }
                    else if (this.player.Direction == Direction.Down)
                    {
                        // Did we change direction to the opposite direction, then move in the opposite direction.
                        if (direction == Direction.Up)
                        {
                            motion.Y = -1;
                            this.player.Direction = direction;
                        }
                        else
                            motion.Y = 1;
                    }
                    else if (this.player.Direction == Direction.Left)
                    {
                        // Did we change direction to the opposite direction, then move in the opposite direction.
                        if (direction == Direction.Right)
                        {
                            motion.X = 1;
                            this.player.Direction = direction;
                        }
                        else
                            motion.X = -1;
                    }
                    else if (this.player.Direction == Direction.Right)
                    {
                        // Did we change direction to the opposite direction, then move in the opposite direction.
                        if (direction == Direction.Left)
                        {
                            motion.X = -1;
                            this.player.Direction = direction;
                        }
                        else
                            motion.X = 1;
                    }

                    // Update next direction.
                    this.player.NextDirection = direction;
                }

                // If the player walks over a ring or dot, run the PlayerHasEatenRing or PlayerHasEatenDot method.
                if (targetTile != null)
                {
                    if (targetTile.TileContent == TileContent.Ring || targetTile.TileContent == TileContent.RingTurn)
                        PlayerHasEatenRing(targetTile);
                    else if (targetTile.TileContent == TileContent.Dot || targetTile.TileContent == TileContent.DotTurn)
                        PlayerHasEatenDot(targetTile);
                }
            }

            // Check if we should move.
            if (motion != Vector2.Zero)
            {
                // Normalize the motion vector and move the player.
                motion.Normalize();

                this.player.Position.X += (float)Math.Round((motion * this.player.Speed * (float)elapsedGameTime.TotalSeconds).X);
                this.player.Position.Y += (float)Math.Round((motion * this.player.Speed * (float)elapsedGameTime.TotalSeconds).Y);
            }
        }

        /// <summary>
        /// Method for firing the gun :!
        /// </summary>
        private void Fire()
        {
            // Only one bullet can be alive and you need atleast BulletCost in score..
            if (!this.bulletAlive && this.score >= PacManSX.BulletCost)
            {
                this.bulletMotion = this.player.Motion;

                // Aim the projectile in the same direction the player is moving.
                if (this.player.Direction == Direction.Up)
                    this.bulletPosition = this.player.Center + new Vector2(-10, -27);
                else if (this.player.Direction == Direction.Down)
                    this.bulletPosition = this.player.Center + new Vector2(-10, 7);
                else if (this.player.Direction == Direction.Left)
                    this.bulletPosition = this.player.Center + new Vector2(-27, -8);
                else if (this.player.Direction == Direction.Right)
                    this.bulletPosition = this.player.Center + new Vector2(7, -8);

                this.bulletAlive = true;
                this.ironBullet = false;
                this.score -= PacManSX.BulletCost;
            }
        }

        /// <summary>
        /// Method for firing the gun with a two finger tap.
        /// </summary>
        private void TwoFingerFire()
        {
            // Only one bullet can be alive and you need atleast BulletCost in score..
            if (!this.bulletAlive && this.score >= PacManSX.IronBulletCost)
            {
                this.bulletMotion = this.player.Motion;

                // Aim the projectile in the same direction the player is moving.
                if (this.player.Direction == Direction.Up)
                    this.bulletPosition = this.player.Center + new Vector2(-10, -27);
                else if (this.player.Direction == Direction.Down)
                    this.bulletPosition = this.player.Center + new Vector2(-10, 7);
                else if (this.player.Direction == Direction.Left)
                    this.bulletPosition = this.player.Center + new Vector2(-27, -8);
                else if (this.player.Direction == Direction.Right)
                    this.bulletPosition = this.player.Center + new Vector2(7, -8);

                this.bulletAlive = true;
                this.ironBullet = true;
                this.score -= PacManSX.BulletCost;
            }
        }

        /// <summary>
        /// Helper for checking if someone can move in a given direction.
        /// </summary>
        /// <param name="coords">The coordinates of the tile.</param>
        /// <param name="direction">The direction to be checked.</param>
        /// <param name="motion">If the player couldn't move then it's set to (0, 0); otherwise it's set to the direction.</param>
        /// <returns>True if the player can move in that direction; otherwise false.</returns>
        private bool CanPlayerMove(Point coords, Direction direction, out Vector2 motion, out Tile targetTile)
        {
            motion = Vector2.Zero;
            targetTile = null;
            Point target = new Point();

            // Set the target tile depending on the direction.
            if (direction == Direction.Up)
            {
                target = new Point(0, -1);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.Y--;
            }
            else if (direction == Direction.Down)
            {
                target = new Point(0, 1);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.Y++;
            }
            else if (direction == Direction.Left)
            {
                target = new Point(-1, 0);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.X--;
            }
            else if (direction == Direction.Right)
            {
                target = new Point(1, 0);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.X++;
            }
            else
                return false;

            // Get the tile at the target.
            targetTile = tileMap.GetTile(target);

            if (targetTile == null)
            {
                motion = Vector2.Zero;
                targetTile = null;
                return false;
            }

            // If the player can move then return true.
            if (targetTile.TileContent == TileContent.Path || targetTile.TileContent == TileContent.Turn
                || targetTile.TileContent >= TileContent.Ring && targetTile.TileContent <= TileContent.DotTurn)
                return true;

            // else set motion to (0, 0) and return false.
            motion = Vector2.Zero;
            targetTile = null;
            return false;
        }

        /// <summary>
        /// Method handling the collision detection between the player and a ghost.
        /// </summary>
        private void PlayerGhostHitbox()
        {
            // If the player is not alive then don't check for hitbox.
            if (!this.player.Alive)
                return;

            // Check if the player is in god mode.
            if (this.player.GodMode)
            {
                int scoreMultiplier = 1;
                if (this.blueGhost != null && !this.blueGhost.Alive)
                    scoreMultiplier *= 2;
                if (this.greenGhost != null && !this.greenGhost.Alive)
                    scoreMultiplier *= 2;
                if (this.yellowGhost != null && !this.yellowGhost.Alive)
                    scoreMultiplier *= 2;
                if (this.purpleGhost != null && !this.purpleGhost.Alive)
                    scoreMultiplier *= 2;

                // If he is in godmode then he should be able to eat the ghost.
                // If the player and a ghost collides, kill the ghost and recieve score.
                if (this.blueGhost != null && this.blueGhost.Alive && this.blueGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.blueGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                }
                else if (this.greenGhost != null && this.greenGhost.Alive && this.greenGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.greenGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                }
                else if (this.yellowGhost != null && this.yellowGhost.Alive && this.yellowGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.yellowGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                }
                else if (this.purpleGhost != null && this.purpleGhost.Alive && this.purpleGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.purpleGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                }
            }
            else
            {
                // If the player and a ghost collides, remove a life from the player and kill it.
                if (this.blueGhost != null && this.blueGhost.Alive && this.blueGhost.Bounds.Intersects(this.player.Bounds) ||
                    this.greenGhost != null && this.greenGhost.Alive && this.greenGhost.Bounds.Intersects(this.player.Bounds) ||
                    this.yellowGhost != null && this.yellowGhost.Alive && this.yellowGhost.Bounds.Intersects(this.player.Bounds) ||
                    this.purpleGhost != null && this.purpleGhost.Alive && this.purpleGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.lives--;
                    this.player.Alive = false;

                    soundChomp = GameManager.ContentManager.Load<SoundEffect>("Sounds/chomp");
                    soundChomp.Play();

                    // Set game over to true if the player doesn't have any lives left.
                    if (this.lives == 0)
                        this.gameOver = true;
                    
                    // Reset all the posiitons.
                    this.ResetPositions();
                }
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Helper for checking if someone can move in a given direction.
        /// </summary>
        /// <param name="coords">The coordinates of the tile.</param>
        /// <param name="direction">The direction to be checked.</param>
        /// <param name="motion">If the player couldn't move then it's set to (0, 0); otherwise it's set to the direction.</param>
        /// <returns>True if the player can move in that direction; otherwise false.</returns>
        public static bool CanGhostMove(TileMap tileMap, Point coords, Direction direction, out Vector2 motion, out Tile targetTile)
        {
            motion = Vector2.Zero;
            targetTile = null;
            Point target = new Point();

            // Set the target tile depending on the direction.
            if (direction == Direction.Up)
            {
                target = new Point(0, -1);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.Y--;
            }
            else if (direction == Direction.Down)
            {
                target = new Point(0, 1);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.Y++;
            }
            else if (direction == Direction.Left)
            {
                target = new Point(-1, 0);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.X--;
            }
            else if (direction == Direction.Right)
            {
                target = new Point(1, 0);
                target.X += coords.X;
                target.Y += coords.Y;
                motion.X++;
            }
            else
                return false;

            // Get the tile at the target.
            targetTile = tileMap.GetTile(target);

            if (targetTile == null)
            {
                motion = Vector2.Zero;
                targetTile = null;
                return false;
            }

            // If the player can move then return true.
            if (targetTile.TileContent == TileContent.Path || targetTile.TileContent == TileContent.Turn
                || targetTile.TileContent >= TileContent.Ring && targetTile.TileContent <= TileContent.DotTurn
                || targetTile.TileContent == TileContent.Door)
                return true;

            // else set motion to (0, 0) and return false.
            motion = Vector2.Zero;
            targetTile = null;
            return false;
        }

        /// <summary>
        /// Converts a pixel to the tile coordinates.
        /// </summary>
        /// <param name="position">The pixel position.</param>
        /// <returns>The tile cell coordinates.</returns>
        public static Point ConvertPositionToCell(Vector2 position)
        {
            return new Point(
                (int)(position.X / (float)TileWidth),
                (int)((position.Y - PacManSX.TitleHeight) / (float)TileHeight));
        }

        /// <summary>
        /// Converts a tile coordinate to the position of the tile.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static Vector2 ConvertCellToPosition(Point cell)
        {
            return new Vector2(cell.X * PacManSX.TileWidth, cell.Y * PacManSX.TileHeight + PacManSX.TileHeight);
        }

        /// <summary>
        /// Creates a rectangle around a specific tile cell.
        /// </summary>
        /// <param name="tileCoordinates">The tile cell.</param>
        /// <returns>The rectangle around the given tile cell point.</returns>
        public static Rectangle CreateRectForTile(Point tileCoordinates)
        {
            return new Rectangle(
                tileCoordinates.X * TileWidth, 
                tileCoordinates.Y * TileHeight,
                TileWidth, TileHeight);
        }

        #endregion
    }
}