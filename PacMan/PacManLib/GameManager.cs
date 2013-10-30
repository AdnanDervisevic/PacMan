#region File Description
    //////////////////////////////////////////////////////////////////////////
   // GameManager                                                          //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib
{
    /// <summary>
    /// Class containing everything needed by the game.
    /// </summary>
    public sealed class GameManager
    {
        #region Properties

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (this.SpriteBatch != null)
                    return this.SpriteBatch.GraphicsDevice;
                return null;
            }
        }

        /// <summary>
        /// Gets the spritebatch.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        public ContentManager ContentManager { get; private set; }

        /// <summary>
        /// Gets the height of the screen in pixels.
        /// </summary>
        public int ScreenWidth { get; private set; }

        /// <summary>
        /// Gets the width of the screen in pixels.
        /// </summary>
        public int ScreenHeight { get; private set; }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Constructs a new game manager.
        /// </summary>
        /// <param name="screenWidth">The width of the screen in pixels.</param>
        /// <param name="screenHeight">The height of the screen in pixels.</param>
        /// <param name="spriteBatch">The spritebatch that should be used by the game.</param>
        /// <param name="contentManager">The content manager that should be used by the game.</param>
        public GameManager(int screenWidth, int screenHeight, SpriteBatch spriteBatch, ContentManager contentManager)
        {
            this.ScreenWidth = screenWidth;
            this.ScreenHeight = screenHeight;
            this.SpriteBatch = spriteBatch;
            this.ContentManager = contentManager;
        }

        #endregion
    }
}