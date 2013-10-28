#region File Description
    //////////////////////////////////////////////////////////////////////////
   // SpriteAnimation                                                      //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using PacManLib.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib
{
    /// <summary>
    /// Class representing a sprite animation.
    /// </summary>
    public sealed class SpriteAnimation : Tileset
    {
        #region Fields

        private int index;
        private float timer;
        private float frameLength;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current source rectangle.
        /// </summary>
        public Rectangle CurrentSourceRectangle
        {
            get { return this.GetSourceRectangle(index); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new sprite animation.
        /// </summary>
        /// <param name="texture">The spritesheet.</param>
        /// <param name="frameWidth">The width of a single frame.</param>
        /// <param name="frameHeight">The height of a single frame.</param>
        public SpriteAnimation(Texture2D texture, int frameWidth, int frameHeight)
            : base(texture, frameWidth, frameHeight)
        {
            this.timer = 0;
            this.index = 0;
            this.frameLength = .2f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows the animation to update itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last update.</param>
        public void Update(TimeSpan elapsedGameTime)
        {
            if (this.Count > 1)
            {
                this.timer += (float)elapsedGameTime.TotalSeconds;

                if (this.timer > this.frameLength)
                {
                    this.timer = 0;
                    this.index = (this.index + 1) % this.Count;
                }
            }
        }

        #endregion
    }
}