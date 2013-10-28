#region File Description
    //////////////////////////////////////////////////////////////////////////
   // Tileset                                                              //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib.Map
{
    /// <summary>
    /// A class for storing source rectangles and a tileset texture.
    /// </summary>
    public class Tileset
    {
        #region Fields

        /// <summary>
        /// A list of source rectangles, rectangles are used to determine 
        /// what tile in the tileset should be drawn.
        /// </summary>
        private List<Rectangle> sourceRectangles = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tileset texture.
        /// </summary>
        public Texture2D Texture { get; private set; }

        /// <summary>
        /// Gets the number of source rectangles.
        /// </summary>
        public int Count
        {
            get { return this.sourceRectangles.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new tileset.
        /// </summary>
        /// <param name="texture">The tileset texture.</param>
        /// <param name="tileWidth">The width of a single tile.</param>
        /// <param name="tileHeight">The height of a single tile.</param>
        public Tileset(Texture2D texture, int tileWidth, int tileHeight)
        {
            this.Texture = texture;
            this.sourceRectangles = new List<Rectangle>();

            int tilesInWidth = texture.Width / tileWidth;
            int tilesInHeight = texture.Height / tileHeight;

            for (int y = 0; y < tilesInHeight; y++)
                for (int x = 0; x < tilesInWidth; x++)
                    this.sourceRectangles.Add(new Rectangle(
                        x * tileWidth, y * tileHeight,
                        tileWidth, tileHeight));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the source rectangle for the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The source rectangle for the given index.</returns>
        public Rectangle GetSourceRectangle(int index)
        {
            return this.sourceRectangles[index];
        }

        #endregion
    }
}