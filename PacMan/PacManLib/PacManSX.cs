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
        public const float AccelerometerDifference = 0.10f;
#endif

        public const int TitleHeight = 20;

        public const int TileWidth = 20;
        public const int TileHeight = 20;

        public const int CharacterWidth = 28;
        public const int CharacterHeight = 28;

        public const int GhostScore = 200;
        public const int RingScore = 10;
        public const int DotScore = 50;
        public const int FruitScoreMultiplier = 300;

        public const int StartBulletCost = 10;

        public const int PathTarget = 4;

        public const int GameCountdownInSeconds = 3;
        public const int FirstRoundGameCountdownInSeconds = 6;

        public const int LeaveJailInSeconds = 5;
        public const int GhostRespawnInSeconds = 5;
        public const int FruitDespawnInSeconds = 15;
        public const int FruitMinSpawnTimerInSeconds = 0;
        public const int FruitMaxSpawnTimerInSeconds = 30;
        public const int GodModeActiveInSeconds = 10;

        public const float BackgroundSoundVolume = 0.3f;
        public const float GodModeSoundVolume = 0.5f;

        #endregion

        #region Fields

#if WINDOWS_PHONE
        private Accelerometer accelerometer = null;
        private Vector2 accelCurrentValue = new Vector2();
        private bool accelActive = false;
        private Vector2 accelCalibration;
        private bool accelCalibrated = false;
        
        private bool accelInitialized = false;
        private float lastX = 0;
        private float lastY = 0;
#elif WINDOWS
        private KeyboardState previousKeyboardState;
#endif

        private bool ghostBullet = false;
        private bool fruitSpawned = false;
        private bool bulletAlive = false;
        private bool gameOver = false;
        private bool leaveJail = false;
        private bool gameStarted = false;
        
        private int fruitSpawnTime = -1;
        private int gameCountdown = PacManSX.FirstRoundGameCountdownInSeconds;
        private int dotsAndRingsLeft = 0;
        private int lives = 3;
        private int score = 0;
        private int ghostBullets = 0;
        private int bulletsFired = 0;

        private float leaveJailTimer = 0;
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
        private Texture2D ghostBulletTexture;
        private Texture2D BlackTexture;
        private Texture2D lifeTexture;
        private Texture2D ghostBulletIconTexture;

        private SoundEffectInstance godmodeInstance;
        private SoundEffectInstance chompInstance;
        private SoundEffectInstance backgroundSoundInstance;
        private SoundEffect soundGodMode;
        private SoundEffect soundChomp;
        private SoundEffect soundEatScore;
        private SoundEffect soundBullet;
        private SoundEffect soundWoodBarrier;
        private SoundEffect soundIronBarrier;
        private SoundEffect backgroundSound;

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
            this.ghostBulletTexture = this.GameManager.ContentManager.Load<Texture2D>("GhostBullet");
            this.lifeTexture = this.GameManager.ContentManager.Load<Texture2D>("Life");
            this.BlackTexture = this.GameManager.ContentManager.Load<Texture2D>("BlackTexture");
            this.fruitTileset = new Tileset(this.GameManager.ContentManager.Load<Texture2D>("Tiles/Fruits"),
                PacManSX.TileWidth, PacManSX.TileHeight);
            this.ghostBulletIconTexture = this.GameManager.ContentManager.Load<Texture2D>("Ghosts/AmmoGhost");

            this.soundEatScore = gameManager.ContentManager.Load<SoundEffect>("Sounds/coin");
            soundChomp = gameManager.ContentManager.Load<SoundEffect>("Sounds/chomp");
            soundGodMode = gameManager.ContentManager.Load<SoundEffect>("Sounds/godmode");
            this.soundBullet = gameManager.ContentManager.Load<SoundEffect>("Sounds/proj");
            this.soundWoodBarrier = gameManager.ContentManager.Load<SoundEffect>("Sounds/woodbox");
            this.soundIronBarrier = gameManager.ContentManager.Load<SoundEffect>("Sounds/ironbox");
            this.backgroundSound = gameManager.ContentManager.Load<SoundEffect>("Sounds/background");

            chompInstance = soundChomp.CreateInstance();
            godmodeInstance = soundGodMode.CreateInstance();
            godmodeInstance.Volume = PacManSX.GodModeSoundVolume;
            godmodeInstance.IsLooped = true;

            backgroundSoundInstance = backgroundSound.CreateInstance();
            backgroundSoundInstance.Volume = PacManSX.BackgroundSoundVolume;
            backgroundSoundInstance.IsLooped = true;
            backgroundSoundInstance.Play();

            // Create the tile map and load the first map.
            this.tileMap = new TileMap(gameManager);
            this.LoadMap(1);

            // Load all the spawns and initialize all the players if they exists.
            int i = 0;
            Point[] spawnCoords = this.tileMap.GetSpawns(SpawnPoint.Player, SpawnPoint.BlueGhost, SpawnPoint.GreenGhost, SpawnPoint.YellowGhost, SpawnPoint.PurpleGhost);

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.player = new Player(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]), Direction.Left,
                    this.GameManager.ContentManager.Load<Texture2D>("Pacman"), null, 
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight);
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.blueGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]), Direction.Right, true,
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/BlueGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GodMode"), 
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight);
                this.blueGhost.GhostAI += blueGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.greenGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]), Direction.Up, true,
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GreenGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight);
                this.greenGhost.GhostAI += greenGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.yellowGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]), Direction.Left, true,
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/YellowGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight);
                this.yellowGhost.GhostAI += yellowGhostAI;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.purpleGhost = new Ghost(this.GameManager, PacManSX.ConvertCellToPosition(spawnCoords[i]), Direction.Right, false,
                    this.GameManager.ContentManager.Load<Texture2D>("Ghosts/PurpleGhost"), this.GameManager.ContentManager.Load<Texture2D>("Ghosts/GodMode"),
                    PacManSX.CharacterWidth, PacManSX.CharacterHeight);
                this.purpleGhost.GhostAI += purpleGhostAI;
            }

#if WINDOWS_PHONE
            // Initialize Windows phone stuff.
            TouchPanel.EnabledGestures = GestureType.DoubleTap;
            this.accelerometer = new Accelerometer();
            this.accelerometer.CurrentValueChanged += accelerometer_CurrentValueChanged;
#elif WINDOWS
            this.previousKeyboardState = Keyboard.GetState();
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
                        godmodeInstance.Stop();
                        backgroundSoundInstance.Play();
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
                        this.score += PacManSX.FruitScoreMultiplier * this.tileMap.Level * this.tileMap.Level;
                        this.fruitSpawnTime = rand.Next(PacManSX.FruitMinSpawnTimerInSeconds, PacManSX.FruitMaxSpawnTimerInSeconds);
                        this.fruitSpawnTimer = 0;
                        this.fruitSpawned = false;

                        if (this.chompInstance.State == SoundState.Stopped)
                            this.chompInstance.Play();
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

                if (
                    (this.blueGhost != null && this.blueGhost.InJail) ||
                    (this.greenGhost != null && this.greenGhost.InJail) ||
                    (this.yellowGhost != null && this.yellowGhost.InJail) &&
                    !this.leaveJail)
                {
                    this.leaveJailTimer += (float)elapsedGameTime.TotalSeconds;
                    if (this.leaveJailTimer >= PacManSX.LeaveJailInSeconds)
                    {
                        this.leaveJail = true;
                        this.leaveJailTimer = 0;
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

                    if (this.blueGhost != null && this.blueGhost.Alive && bulletBounds.Intersects(this.blueGhost.Bounds))
                        this.bulletAlive = false;

                    if (this.greenGhost != null && this.greenGhost.Alive && bulletBounds.Intersects(this.greenGhost.Bounds))
                        this.bulletAlive = false;

                    if (this.yellowGhost != null && this.yellowGhost.Alive && bulletBounds.Intersects(this.yellowGhost.Bounds))
                        this.bulletAlive = false;

                    if (this.purpleGhost != null && this.purpleGhost.Alive && bulletBounds.Intersects(this.purpleGhost.Bounds))
                        this.bulletAlive = false;

                    // Convert the center of the bullet to the bullet coordinates.
                    Point bulletCoords = PacManSX.ConvertPositionToCell(this.bulletPosition + new Vector2(10));
                    Tile bulletTile = this.tileMap.GetTile(bulletCoords);

                    // Check if the bullet tile is colliding with a wall or a type of barrier.
                    if (bulletTile.TileContent > TileContent.Path && bulletTile.TileContent < TileContent.WoodBarrier || !this.ghostBullet && bulletTile.TileContent == TileContent.ghostBarrier)
                        this.bulletAlive = false;
                    else if (bulletTile.TileContent == TileContent.WoodBarrier)
                    {
                        // Destroy the wood barrier if the bullet hits it.
                        this.bulletAlive = false;
                        this.tileMap.UpdateTile(bulletCoords, TileContent.Path);
                        this.soundWoodBarrier.Play();
                    }
                    else if (bulletTile.TileContent == TileContent.ghostBarrier && this.ghostBullet)
                    {
                        // Destroy the ghost barrier if the bullet hits it and it's a ghost bullet.
                        this.bulletAlive = false;
                        this.tileMap.UpdateTile(bulletCoords, TileContent.Path);
                        this.soundIronBarrier.Play();
                    }
                }
            }
            else
            {
#if WINDOWS_PHONE
                if (this.accelActive)
                {
                    try
                    {
                        this.accelerometer.Stop();
                        this.accelActive = false;
                    }
                    catch (Exception) { }
                }
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
                            this.gameOver = false;

                        this.fruitSpawnTime = rand.Next(PacManSX.FruitMinSpawnTimerInSeconds, PacManSX.FruitMaxSpawnTimerInSeconds);

                        this.gameStarted = true;
#if WINDOWS_PHONE
                        this.accelCalibrated = false;

                        if (!this.accelActive)
                        {
                            try
                            {
                                this.accelerometer.Start();
                                this.accelActive = true;
                            }
                            catch (Exception)
                            {
                                this.accelActive = false;
                            }
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
            {
                if (this.ghostBullet)
                    this.GameManager.SpriteBatch.Draw(this.ghostBulletTexture, this.bulletPosition, Color.White);
                else
                    this.GameManager.SpriteBatch.Draw(this.bulletTexture, this.bulletPosition, Color.White);
            }

            // Draw the fruit
            if (this.fruitSpawned)
                this.GameManager.SpriteBatch.Draw(this.fruitTileset.Texture, this.fruitBounds, this.fruitTileset.GetSourceRectangle(this.tileMap.Level - 1), Color.White);

            // Draw the GUI.
            this.GameManager.SpriteBatch.DrawString(this.font, "Level: " + this.tileMap.Level, this.levelPosition, Color.White);
            for (int i = 0; i < this.lives; i++)
                this.GameManager.SpriteBatch.Draw(this.lifeTexture, new Vector2(GameManager.ScreenWidth - lives * 20 - 4, 4) + new Vector2(20 * i, 0), Color.White);
            this.GameManager.SpriteBatch.DrawString(this.font, "Score: " + this.score, this.scorePosition, Color.White);

            this.GameManager.SpriteBatch.DrawString(this.font, "Ghost Bullets: " + this.ghostBullets + "x", new Vector2(this.GameManager.ScreenWidth - 400, 0), Color.White);
            this.GameManager.SpriteBatch.Draw(this.ghostBulletIconTexture, new Vector2(this.GameManager.ScreenWidth - 210, 2), Color.White);

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

#if WINDOWS_PHONE

        /// <summary>
        /// Method fired when we're navigating from the game page.
        /// </summary>
        public void StopSounds()
        {
            if (this.backgroundSoundInstance.State == SoundState.Playing)
                this.backgroundSoundInstance.Stop();

            if (this.godmodeInstance.State == SoundState.Playing)
                this.godmodeInstance.Stop();
        }

#endif

        #endregion

        #region Private Helpers

#if WINDOWS_PHONE

        /// <summary>
        /// Method fired when the accelerometer value has changed.
        /// </summary>
        private void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            if (!this.accelCalibrated)
            {
                this.accelCalibration.X = (float)e.SensorReading.Acceleration.X;
                this.accelCalibration.Y = (float)e.SensorReading.Acceleration.Y;
                this.accelCalibrated = true;
            }
            else
            {
                this.accelCurrentValue.X = (float)e.SensorReading.Acceleration.X;
                this.accelCurrentValue.Y = (float)e.SensorReading.Acceleration.Y;
            }
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
                    { 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 5, 1, 1, 1, 1, 6, 13, 5, 1, 1, 1, 1, 1, 1, 1, 6, 15, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 1, 6, 13, 5, 1, 1, 1, 1, 6, 13, 2 },
                    { 2, 13, 3, 1, 1, 1, 1, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 1, 1, 1, 1, 4, 13, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 15, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 15, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 14, 7, 1, 1, 1, 1, 8, 13, 5, 6, 13, 9, 13, 7, 1, 1, 1, 1, 6, 5, 1, 1, 1, 1, 8, 13, 9, 13, 5, 6, 13, 7, 1, 1, 1, 1, 8, 14, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 13, 2, 15, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 15, 2, 13, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 3, 1, 1, 1, 1, 1, 1, 6, 13, 2, 2, 13, 3, 1, 1, 1, 1, 8, 0, 3, 4, 0, 7, 1, 1, 1, 1, 4, 13, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 4 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 15, 13, 13, 13, 13, 13, 15, 17, 0, 17, 17, 0, 15, 13, 13, 13, 13, 15, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 5, 1, 1, 1, 6, 13, 5, 8, 18, 7, 6, 13, 5, 1, 1, 6, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 7, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 2, 0, 0, 0, 2, 13, 2, 17, 17, 17, 2, 13, 2, 0, 0, 2, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 8 },
                    { 0, 0, 0, 0, 0, 0, 0, 0, 15, 13, 13, 15, 3, 1, 1, 1, 4, 13, 3, 1, 1, 1, 4, 13, 3, 1, 1, 4, 15, 13, 13, 15, 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 7, 1, 1, 1, 1, 1, 1, 6, 13, 5, 6, 15, 0, 0, 0, 0, 0, 17, 11, 17, 0, 17, 11, 17, 0, 0, 0, 0, 15, 5, 6, 13, 5, 1, 1, 1, 1, 1, 1, 8 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 5, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 6 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 17, 0, 0, 17, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 2 },
                    { 2, 14, 3, 1, 1, 1, 1, 4, 13, 2, 2, 13, 3, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 4, 13, 2, 2, 13, 3, 1, 1, 1, 1, 4, 14, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 7, 1, 1, 1, 1, 1, 1, 4, 3, 1, 1, 1, 1, 1, 1, 8, 13, 7, 8, 13, 7, 1, 1, 1, 1, 1, 1, 4, 3, 1, 1, 1, 1, 1, 1, 8, 13, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4 },
                });

                this.tileMap.UpdateTile(new Point(20, 16), SpawnPoint.Player);
                this.tileMap.UpdateTile(new Point(19, 10), SpawnPoint.BlueGhost);
                this.tileMap.UpdateTile(new Point(20, 10), SpawnPoint.GreenGhost);
                this.tileMap.UpdateTile(new Point(21, 10), SpawnPoint.YellowGhost);
                this.tileMap.UpdateTile(new Point(20, 8), SpawnPoint.PurpleGhost);
                this.tileMap.UpdateTile(new Point(20, 12), SpawnPoint.Fruit);
            }
            else if (level == 2)
            {
                this.tileMap.LoadMap(level, new int[,]
                {
                    { 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 5, 1, 1, 1, 1, 6, 13, 5, 1, 1, 1, 1, 1, 1, 1, 6, 15, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 1, 6, 13, 5, 1, 1, 1, 1, 6, 13, 2 },
                    { 2, 13, 3, 1, 1, 1, 1, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 1, 1, 1, 1, 4, 13, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 15, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 15, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 14, 7, 1, 1, 1, 1, 8, 13, 5, 6, 13, 9, 13, 7, 1, 1, 1, 1, 6, 5, 1, 1, 1, 1, 8, 13, 9, 13, 5, 6, 13, 7, 1, 1, 1, 1, 8, 14, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 13, 2, 15, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 15, 2, 13, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 3, 1, 1, 1, 1, 1, 1, 6, 13, 2, 2, 13, 3, 1, 1, 1, 1, 8, 0, 3, 4, 0, 7, 1, 1, 1, 1, 4, 13, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 4 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 15, 13, 13, 13, 13, 13, 15, 17, 0, 17, 17, 0, 15, 13, 13, 13, 13, 15, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 5, 1, 1, 1, 6, 13, 5, 8, 18, 7, 6, 13, 5, 1, 1, 6, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 7, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 2, 0, 0, 0, 2, 13, 2, 17, 17, 17, 2, 13, 2, 0, 0, 2, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 8 },
                    { 0, 0, 0, 0, 0, 0, 0, 0, 15, 13, 13, 15, 3, 1, 1, 1, 4, 13, 3, 1, 1, 1, 4, 13, 3, 1, 1, 4, 15, 13, 13, 15, 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 7, 1, 1, 1, 1, 1, 1, 6, 13, 5, 6, 15, 0, 0, 0, 0, 0, 17, 12, 17, 0, 17, 12, 17, 0, 0, 0, 0, 15, 5, 6, 13, 5, 1, 1, 1, 1, 1, 1, 8 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 13, 2, 2, 13, 2, 0, 0, 0, 0, 0, 0, 0 },
                    { 5, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 1, 1, 6 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 17, 0, 0, 17, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 5, 6, 13, 5, 1, 1, 1, 1, 6, 13, 2 },
                    { 2, 14, 3, 1, 1, 1, 1, 4, 13, 2, 2, 13, 3, 1, 1, 1, 1, 4, 13, 3, 4, 13, 3, 1, 1, 1, 1, 4, 13, 2, 2, 13, 3, 1, 1, 1, 1, 4, 14, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 15, 2, 2, 15, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 2, 13, 7, 1, 1, 1, 1, 1, 1, 4, 3, 1, 1, 1, 1, 1, 1, 8, 13, 7, 8, 13, 7, 1, 1, 1, 1, 1, 1, 4, 3, 1, 1, 1, 1, 1, 1, 8, 13, 2 },
                    { 2, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 13, 13, 15, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 15, 2 },
                    { 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4 },
                });

                this.tileMap.UpdateTile(new Point(20, 16), SpawnPoint.Player);
                this.tileMap.UpdateTile(new Point(19, 10), SpawnPoint.BlueGhost);
                this.tileMap.UpdateTile(new Point(20, 10), SpawnPoint.GreenGhost);
                this.tileMap.UpdateTile(new Point(21, 10), SpawnPoint.YellowGhost);
                this.tileMap.UpdateTile(new Point(20, 8), SpawnPoint.PurpleGhost);
                this.tileMap.UpdateTile(new Point(20, 12), SpawnPoint.Fruit);
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
            {
                this.LoadMap(1);
                this.ghostBullets = 0;
                this.lives = 3;
                this.score = 0;
                this.gameCountdown = PacManSX.FirstRoundGameCountdownInSeconds;
            }
            else
                this.gameCountdown = PacManSX.GameCountdownInSeconds;

            int i = 0;
            Point[] spawnCoords = this.tileMap.GetSpawns(SpawnPoint.Player, SpawnPoint.BlueGhost, SpawnPoint.GreenGhost, SpawnPoint.YellowGhost, SpawnPoint.PurpleGhost);
            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.player.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.player.Direction = this.player.StartDirection;
                this.player.NextDirection = Direction.None;
                this.player.Alive = true;
                this.player.GodMode = false;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.blueGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.blueGhost.Direction = this.blueGhost.StartDirection;
                this.blueGhost.Alive = true;
                this.blueGhost.InJail = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.greenGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.greenGhost.Direction = this.greenGhost.StartDirection;
                this.greenGhost.Alive = true;
                this.greenGhost.InJail = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.yellowGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.yellowGhost.Direction = this.yellowGhost.StartDirection;
                this.yellowGhost.Alive = true;
                this.yellowGhost.InJail = true;
            }
            i++;

            if (spawnCoords[i].X >= 0 || spawnCoords[i].Y >= 0)
            {
                this.purpleGhost.Position = PacManSX.ConvertCellToPosition(spawnCoords[i]);
                this.purpleGhost.Direction = this.purpleGhost.StartDirection;
                this.purpleGhost.Alive = true;
                this.purpleGhost.InJail = false;
            }

            this.fruitSpawned = false;

            if (this.godmodeInstance.State == SoundState.Playing)
                this.godmodeInstance.Stop();

            // Start the game countdown.
            this.gameStarted = false;
            this.leaveJailTimer = 0;
            this.leaveJail = false;
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

            if (backgroundSoundInstance.State == SoundState.Playing)
                backgroundSoundInstance.Stop();

            //Play godmode music if not active
            if (godmodeInstance.State == SoundState.Stopped)
                godmodeInstance.Play();
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
        /// Blue matches cyan AI
        private Direction blueGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion, out Tile targetTile)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            targetTile = null;

            if (!this.blueGhost.InJail)
            {
                #region Cyan pathfinding tries to predict where the player will be in a few moments and thusly calculates what path it should take.

                if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
                {
                    Point predictedCoords;
                    predictedCoords.X = playerCoords.X;
                    predictedCoords.Y = playerCoords.Y;

                    // based on direction tries to estimate in front of the player by one tile to try and get slightly more accurate and predicting
                    if (player.Direction == Direction.Right)
                        predictedCoords.X = playerCoords.X + PacManSX.PathTarget;
                    else if (player.Direction == Direction.Left)
                        predictedCoords.X = playerCoords.X - PacManSX.PathTarget;
                    else if (player.Direction == Direction.Up)
                        predictedCoords.Y = playerCoords.Y + PacManSX.PathTarget;
                    else if (player.Direction == Direction.Down)
                        predictedCoords.Y = playerCoords.Y - PacManSX.PathTarget;

                    // calculates which path is closer purely based on distance between x and y of the player and ghost
                    int xDelta = Math.Abs((ghostCoords.X - predictedCoords.X));
                    int yDelta = Math.Abs((ghostCoords.Y - predictedCoords.Y));

                    // Checks where approximately the ghost is based on the x & y coordinates
                    if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                    {
                        // If closer in the x coordinate space than the y space
                        if (xDelta > yDelta)
                        {
                            // Move right!
                            direction = Direction.Right;

                            // If the player can hunt you down and kill you, you should really try and go the other way
                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            // Various statements trying to auto-correct the ghosts movement in case he runs into a wall or a dead end
                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        // Same exact code, except for the other direction
                        else
                        {
                            direction = Direction.Up;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Down;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Down;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Up;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                // If the blue is the only one in jail, leave.
                if (this.leaveJail && (this.greenGhost == null || !this.greenGhost.InJail) && (this.yellowGhost == null || !this.yellowGhost.InJail))
                {
                    if (!CanGhostMove(this.tileMap, ghostCoords, Direction.Up, ghost.InJail, out motion, out targetTile))
                    {
                        if (CanGhostMove(this.tileMap, ghostCoords, Direction.Right, ghost.InJail, out motion, out targetTile))
                            direction = Direction.Right;
                    }
                    else
                    {
                        direction = Direction.Up;
                        this.leaveJail = false;
                        this.blueGhost.InJail = false;
                    }
                }
                else return Direction.None;
            }


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
        /// Green matches red ai
        private Direction greenGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion, out Tile targetTile)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            targetTile = null;
            
            if (!this.greenGhost.InJail)
            {
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Up;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Down;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Down;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Up;

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                // If the blue is the only one in jail, leave.
                if (this.leaveJail)
                {
                    if (CanGhostMove(this.tileMap, ghostCoords, Direction.Up, ghost.InJail, out motion, out targetTile))
                    {
                        direction = Direction.Up;
                        this.leaveJail = false;
                        this.greenGhost.InJail = false;
                    }
                }
                else return Direction.None;
            }

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
        /// Yellow matches orange AI. Orange always was a bit slow...
        private Direction yellowGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion, out Tile targetTile)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            targetTile = null;

            if (!this.yellowGhost.InJail)
            {
                #region Ghost pathfinding for orange behaviour

                int randomNumber = rand.Next(0, 1);

                if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
                {
                    Point predictedCoords;
                    predictedCoords.X = playerCoords.X;
                    predictedCoords.Y = playerCoords.Y;

                    // based on direction tries to estimate in front of the player by one tile to try and get slightly more accurate and predicting
                    if (player.Direction == Direction.Right)
                        predictedCoords.X = playerCoords.X + PacManSX.PathTarget;
                    else if (player.Direction == Direction.Left)
                        predictedCoords.X = playerCoords.X - PacManSX.PathTarget;
                    else if (player.Direction == Direction.Up)
                        predictedCoords.Y = playerCoords.Y + PacManSX.PathTarget;
                    else if (player.Direction == Direction.Down)
                        predictedCoords.Y = playerCoords.Y - PacManSX.PathTarget;

                    int whichPathFinding = rand.Next(0, 1);

                    // calculates which path is closer purely based on distance between x and y of the player and ghost. Flips between them randomly
                    int xDelta = (whichPathFinding == 0) ? Math.Abs((ghostCoords.X - predictedCoords.X)) : Math.Abs((ghostCoords.X - playerCoords.X));
                    int yDelta = (whichPathFinding == 0) ? Math.Abs((ghostCoords.Y - predictedCoords.Y)) : Math.Abs((ghostCoords.Y - playerCoords.Y));

                    if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                    {
                        if (xDelta > yDelta)
                        {
                            direction = Direction.Right;

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Up;

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                                direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }
                        else
                        {
                            direction = Direction.Down;

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Right;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Down;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Down;

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Up;

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }

                        else
                        {
                            direction = Direction.Up;

                            // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                            direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                            if (player.GodMode)
                                direction = reverseMovement(direction);

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = Direction.Left;

                                // He has a 50% chance of actually getting the direction right. Orange wasn't brightest ghost of the lot, but he tried so very hard!
                                direction = (randomNumber % 1 == 0) ? direction : reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(direction);

                                    if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                    {
                                        direction = reverseMovement(blueGhost.Direction);
                                        PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                    }

                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                // If the blue is still in jail or null and green is not in jail then leave.
                if (this.leaveJail && (this.greenGhost == null || !this.greenGhost.InJail))
                {
                    if (!CanGhostMove(this.tileMap, ghostCoords, Direction.Up, ghost.InJail, out motion, out targetTile))
                    {
                        if (CanGhostMove(this.tileMap, ghostCoords, Direction.Left, ghost.InJail, out motion, out targetTile))
                            direction = Direction.Left;
                    }
                    else
                    {
                        direction = Direction.Up;
                        this.leaveJail = false;
                        this.yellowGhost.InJail = false;
                    }
                }
                else
                    return Direction.None;
            }

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
        /// Purple ghost matches "pink" pac-man ai
        private Direction purpleGhostAI(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion, out Tile targetTile)
        {
            Direction direction = ghost.Direction;
            motion = ghost.Motion;
            targetTile = null;

            #region Ghost pathfinding for pink behaviour

            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.RingTurn || ghostTile.TileContent == TileContent.DotTurn)
            {
                Point predictedCoords;
                predictedCoords.X = playerCoords.X;
                predictedCoords.Y = playerCoords.Y;

                // based on direction tries to estimate in front of the player by one tile to try and get slightly more accurate and predicting
                if (player.Direction == Direction.Right)
                    predictedCoords.X = playerCoords.X + PacManSX.PathTarget;
                else if (player.Direction == Direction.Left)
                    predictedCoords.X = playerCoords.X - PacManSX.PathTarget;
                else if (player.Direction == Direction.Up)
                    predictedCoords.Y = playerCoords.Y + PacManSX.PathTarget;
                else if (player.Direction == Direction.Down)
                    predictedCoords.Y = playerCoords.Y - PacManSX.PathTarget;

                int whichPathFinding = rand.Next(0,1);

                // calculates which path is closer purely based on distance between x and y of the player and ghost. Flips between them randomly
                int xDelta = (whichPathFinding == 0) ? Math.Abs((ghostCoords.X - predictedCoords.X)) : Math.Abs((ghostCoords.X - playerCoords.X));
                int yDelta = (whichPathFinding == 0) ? Math.Abs((ghostCoords.Y - predictedCoords.Y)) : Math.Abs((ghostCoords.Y - playerCoords.Y));

                if (ghostCoords.X <= playerCoords.X && ghostCoords.Y >= playerCoords.Y)
                {
                    if (xDelta > yDelta)
                    {
                        direction = Direction.Right;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Right;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Down;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Down;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Up;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
                                }

                            }
                        }
                    }

                    else
                    {
                        direction = Direction.Up;

                        if (player.GodMode)
                            direction = reverseMovement(direction);

                        if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                        {
                            direction = Direction.Left;

                            if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                            {
                                direction = reverseMovement(direction);

                                if (!PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile))
                                {
                                    direction = reverseMovement(blueGhost.Direction);
                                    PacManSX.CanGhostMove(this.tileMap, ghostCoords, direction, ghost.InJail, out motion, out targetTile);
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

            if (keyboardState.IsKeyDown(Keys.Space) && this.previousKeyboardState.IsKeyUp(Keys.Space))
                Fire();
            else if (keyboardState.IsKeyDown(Keys.LeftControl) && this.previousKeyboardState.IsKeyUp(Keys.LeftControl))
                TwoFingerFire();

            this.previousKeyboardState = keyboardState;
#elif WINDOWS_PHONE
            // If we're using Windows Phone use the Tap gesture to fire and the accelerometer to move.

            // If Gesture is available.
            if (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                if (gesture.GestureType == GestureType.DoubleTap)
                    if (gesture.Position.X > this.GameManager.ScreenWidth / 2)
                        Fire();
                    else
                        TwoFingerFire();
            }

            // If the accelerometer is active.
            if (this.accelActive)
            {
                float x = this.accelCurrentValue.X;
                float y = this.accelCurrentValue.Y;

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

                    // Set the lastX and lastY to the current X and Y.
                    lastX = x;
                    lastY = y;

                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        if (this.GameManager.Orientation == DisplayOrientation.LandscapeLeft)
                        {
                            if (x > this.accelCalibration.X + PacManSX.AccelerometerDifference)
                                direction = Direction.Up;
                            else if (x < this.accelCalibration.X - PacManSX.AccelerometerDifference)
                                direction = Direction.Down;
                        }
                        else
                        {
                            if (x > this.accelCalibration.X + PacManSX.AccelerometerDifference)
                                direction = Direction.Down;
                            else if (x < this.accelCalibration.X - PacManSX.AccelerometerDifference)
                                direction = Direction.Up;
                        }
                    }
                    else
                    {
                        if (this.GameManager.Orientation == DisplayOrientation.LandscapeLeft)
                        {
                            // Otherwise we're going Right or left.
                            if (y > this.accelCalibration.Y + PacManSX.AccelerometerDifference)
                                direction = Direction.Left;
                            else if (y < this.accelCalibration.Y - PacManSX.AccelerometerDifference)
                                direction = Direction.Right;
                        }
                        else
                        {
                            // Otherwise we're going Right or left.
                            if (y > this.accelCalibration.Y + PacManSX.AccelerometerDifference)
                                direction = Direction.Right;
                            else if (y < this.accelCalibration.Y - PacManSX.AccelerometerDifference)
                                direction = Direction.Left;
                        }
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
            if (!this.bulletAlive && this.score >= PacManSX.StartBulletCost * this.bulletsFired)
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
                this.ghostBullet = false;
                this.bulletsFired++;
                this.score -= this.bulletsFired * PacManSX.StartBulletCost;

                this.soundBullet.Play();
            }
        }

        /// <summary>
        /// Method for firing the gun with a two finger tap.
        /// </summary>
        private void TwoFingerFire()
        {
            // Only one bullet can be alive and you need atleast BulletCost in score..
            if (!this.bulletAlive && this.ghostBullets > 0 && this.score >= PacManSX.GhostScore)
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
                this.ghostBullet = true;
                this.ghostBullets--;
                this.score -= PacManSX.GhostScore;

                this.soundBullet.Play();
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
                    this.ghostBullets++;

                    if (chompInstance.State == SoundState.Stopped)
                        chompInstance.Play();
                }
                else if (this.greenGhost != null && this.greenGhost.Alive && this.greenGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.greenGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                    this.ghostBullets++;

                    if (chompInstance.State == SoundState.Stopped)
                        chompInstance.Play();
                }
                else if (this.yellowGhost != null && this.yellowGhost.Alive && this.yellowGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.yellowGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                    this.ghostBullets++;

                    if (chompInstance.State == SoundState.Stopped)
                        chompInstance.Play();
                }
                else if (this.purpleGhost != null && this.purpleGhost.Alive && this.purpleGhost.Bounds.Intersects(this.player.Bounds))
                {
                    this.purpleGhost.Alive = false;
                    this.score += PacManSX.GhostScore * scoreMultiplier;
                    this.ghostBullets++;

                    if (chompInstance.State == SoundState.Stopped)
                        chompInstance.Play();
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

                    if (chompInstance.State == SoundState.Stopped)
                        chompInstance.Play();

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
        public static bool CanGhostMove(TileMap tileMap, Point coords, Direction direction, bool inJail, out Vector2 motion, out Tile targetTile)
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
                || (targetTile.TileContent == TileContent.Door && inJail))
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

        #endregion
    }
}