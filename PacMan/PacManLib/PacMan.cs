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

namespace PacManLib
{
    /// <summary>
    /// This is the main game class.
    /// </summary>
    public class PacMan
    {
        private SpriteBatch spriteBatch = null;
        private ContentManager contentManager = null;

        /// <summary>
        /// The sprite batch.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return this.spriteBatch; }
            set
            {
                if (this.spriteBatch == null)
                    this.spriteBatch = value;
            }
        }

        /// <summary>
        /// The content manager.
        /// </summary>
        public ContentManager Content
        {
            get { return this.contentManager; }
            set
            {
                if (this.contentManager == null)
                    this.contentManager = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PacMan(ContentManager content)
        {
            // TODO: Construct any child components here
            this.Content = content;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTimerEventArgs gameTime)
        {
            // TODO: Add your update code here
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTimerEventArgs gameTime)
        {
            spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
