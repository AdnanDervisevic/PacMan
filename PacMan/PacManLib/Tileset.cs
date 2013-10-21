using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PacManLib
{
    public sealed class Tileset
    {
        private List<Rectangle> sourceRectanglels = new List<Rectangle>();
        private Texture2D texture;
        private int tileWidth;
        private int tileHeight;

        /// <summary>
        /// Gets the Texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return this.texture; }
        }

        /// <summary>
        /// Constructs a new spritesheet.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="tileWidth">The width of a tile.</param>
        /// <param name="tileHeight">The height of a tile.</param>
        public Tileset(Texture2D texture, int tileWidth, int tileHeight)
        {
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.texture = texture;

            int tilesInWidth = texture.Width / this.tileWidth;
            int tilesInHeight = texture.Height / this.tileHeight;

            for (int y = 0; y < tilesInHeight; y++)
                for (int x = 0; x < tilesInWidth; x++)
                    this.sourceRectanglels.Add(new Rectangle(
                        x * tileWidth,
                        y * tileHeight,
                        tileWidth, tileHeight));
        }

        /// <summary>
        /// Get the source rectangle for the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The source rectangle for the given index.</returns>
        public Rectangle GetSourceRectangle(int index)
        {
            return this.sourceRectanglels[index];
        }

    }
}
