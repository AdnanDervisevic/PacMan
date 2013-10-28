#region File Description
    //////////////////////////////////////////////////////////////////////////
   // Player                                                               //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib.GameObjects
{
    public sealed class Player : Character
    {
        #region Properties

        /// <summary>
        /// Gets or sets the next direction the player should take.
        /// </summary>
        public Direction NextDirection { get; set; }

        /// <summary>
        /// Gets or sets whether the player is in god mode.
        /// </summary>
        public bool GodMode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new player.
        /// </summary>
        /// <param name="gameManager">The game manager.</param>
        /// <param name="origin">The origin of the player.</param>
        /// <param name="texture">The players spritesheet.</param>
        /// <param name="frameWidth">The width of a single frame.</param>
        /// <param name="frameHeight">The height of a single frame.</param>
        public Player(GameManager gameManager, Vector2 position, Texture2D texture, int frameWidth, int frameHeight)
            : base(gameManager, position, texture, frameWidth, frameHeight)
        {
            this.Speed = 140;
        }

        #endregion
    }
}