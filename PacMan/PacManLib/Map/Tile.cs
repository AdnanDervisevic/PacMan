#region File Description
    //////////////////////////////////////////////////////////////////////////
   // TileCell                                                             //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
#endregion End of Using Statements

namespace PacManLib.Map
{
    /// <summary>
    /// This class contains data about a single tile.
    /// </summary>
    public sealed class Tile
    {
        #region Properties

        /// <summary>
        /// Gets or sets the content of this tile.
        /// </summary>
        public TileContent TileContent { get; set; }

        /// <summary>
        /// Gets or sets if the player or a ghost should spawn on this tile.
        /// </summary>
        public SpawnPoint SpawnPoint { get; set; }

        #endregion
    }
}