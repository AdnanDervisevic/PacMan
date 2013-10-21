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
using Microsoft.Xna.Framework.Input.Touch;

namespace PacManLib
{
    /// <summary>
    /// This is the main game class.
    /// </summary>
    public class PacMan
    {
        private GameMap gameMap = null;

        /// <summary>
        /// Gets the engine manager.
        /// </summary>
        public EngineManager EngineManager { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PacMan(ContentManager content)
        {
            // Constructs the engine manager.
            this.EngineManager = new EngineManager(content);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            // Initializes the spritebatch and screen width and height.
            this.EngineManager.SpriteBatch = spriteBatch;
            this.EngineManager.ScreenWidth = screenWidth;
            this.EngineManager.ScreenHeight = screenHeight;

            this.gameMap = new GameMap(this.EngineManager, new int[,]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1 },
                { 1, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            }, 40, 40);

            this.gameMap.Initialize();
        }

        #region Update & Draw

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTimerEventArgs gameTime)
        {
            // TODO: Add your update code here

            // Adds gesture capabilities to the game.
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Hold;
            if (TouchPanel.IsGestureAvailable)
            {
                GestureSample sample = TouchPanel.ReadGesture();

                switch (sample.GestureType)
                {
                    // If we detected a Tap then fire the OnTap method.
                    case GestureType.Tap:
                        OnTap(sample);
                        break;
                }
            }
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTimerEventArgs gameTime)
        {
            // Clears the backbuffer.
            this.EngineManager.SpriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
            this.gameMap.Draw(gameTime);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method that runs every time you tap on the screen.
        /// </summary>
        /// <param name="sample">The Gesture sample.</param>
        private void OnTap(GestureSample sample)
        {
        }

        #endregion
    }
}
