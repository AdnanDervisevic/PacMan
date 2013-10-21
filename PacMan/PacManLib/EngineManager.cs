using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PacManLib
{
    public sealed class EngineManager
    {
        private int screenWidth = 0;
        private int screenHeight = 0;
        private SpriteBatch spriteBatch = null;
        private ContentManager contentManager = null;

        /// <summary>
        /// Gets or sets the Orientation, should never be set manually.
        /// </summary>
        public DisplayOrientation Orientation { get; set; }

        /// <summary>
        /// The graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (this.spriteBatch != null)
                    return this.spriteBatch.GraphicsDevice;

                return null;
            }
        }

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
        }

        /// <summary>
        /// Gets the screen width in pixels.
        /// </summary>
        public int ScreenWidth
        {
            get { return this.screenWidth; }
            set
            {
                if (this.screenWidth == 0)
                    this.screenWidth = value;
            }
        }

        /// <summary>
        /// Gets the screen height in pixels.
        /// </summary>
        public int ScreenHeight
        {
            get { return this.screenHeight; }
            set
            {
                if (this.screenHeight == 0)
                    this.screenHeight = value;
            }
        }

        /// <summary>
        /// Constructs a new engine manager.
        /// </summary>
        public EngineManager(ContentManager content)
        {
            this.contentManager = content;
        }
    }
}
