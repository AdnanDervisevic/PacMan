#region File Description
    //////////////////////////////////////////////////////////////////////////
   // Ghost                                                                //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacManLib.Map;
#endregion End of Using Statements

namespace PacManLib.GameObjects
{
    public sealed class Ghost : Character
    {
        #region Fields

        private Vector2 SpawnPosition;
        private float respawnTimer = 0;

        #endregion

        #region Events

        /// <summary>
        /// The delegate used by the Ghost AI event.
        /// </summary>
        /// <param name="ghost">The ghost.</param>
        /// <returns>Returns the direction the ghost should move in.</returns>
        public delegate Direction GhostAIEventHandler(Ghost ghost, Tile ghostTile, Point ghostCoords, Point playerCoords, out Vector2 motion);
    
        /// <summary>
        /// The GhostAI event, this event is fired when the ghosts AI should calculate a direction.
        /// </summary>
        public event GhostAIEventHandler GhostAI;

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
        public Ghost(GameManager gameManager, Vector2 position, Direction startDirection, Texture2D texture, Texture2D godModeTexture, int frameWidth, int frameHeight)
            : base(gameManager, position, startDirection, texture, godModeTexture, frameWidth, frameHeight)
        {
            this.SpawnPosition = position;
            this.Speed = 120;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows the game to update itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last update.</param>
        public override void Update(TimeSpan elapsedGameTime)
        {
            // If the ghost is dead then wait X seconds and revive it at the spawn location.
            if (!this.Alive)
            {
                respawnTimer += (float)elapsedGameTime.TotalSeconds;

                if (respawnTimer >= PacManSX.GhostRespawnInSeconds)
                {
                    this.Alive = true;
                    this.Position = this.SpawnPosition;
                    this.respawnTimer = 0;
                }
            }

            base.Update(elapsedGameTime);
        }

        /// <summary>
        /// Method handling the movement for this ghost.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last update.</param>
        public void Movement(TimeSpan elapsedGameTime, Player player, TileMap tileMap)
        {
            if (!this.Alive)
                return;

            // Player coordinates for targeting
            Point playerCoords = PacManSX.ConvertPositionToCell(player.Center);

            Tile targetTile = null;

            Direction direction = this.Direction;
            Vector2 motion = this.Motion;

            // Converts the center of the ghost to the ghost's tile coordinates.
            Point ghostCoords = PacManSX.ConvertPositionToCell(this.Center);
            Tile ghostTile = tileMap.GetTile(ghostCoords); // Get the tile the ghost is located at.

            // Check if the tile is a turn or path tile.
            if (ghostTile.TileContent == TileContent.Turn || ghostTile.TileContent == TileContent.Path
                || ghostTile.TileContent >= TileContent.Ring && ghostTile.TileContent <= TileContent.DotTurn
                || ghostTile.TileContent == TileContent.Door)
            {
                // Convert the cell to a position.

                Vector2 ghostTilePosition = PacManSX.ConvertCellToPosition(ghostCoords);

                // Check if the ghost is right ontop of the tile.
                if (ghostTilePosition == this.Position)
                {
                    // Run the ghost AI.
                    if (this.GhostAI != null)
                    {
                        direction = this.GhostAI(this, ghostTile, ghostCoords, playerCoords, out motion);
                    }

                    if (PacManSX.CanGhostMove(tileMap, ghostCoords, direction, out motion, out targetTile))
                    {
                        this.Motion = motion;
                        this.Direction = direction;
                    }
                    else
                    {
                        // If the ghost can't move in that direction then check if the player can move in the old direction.
                        if (PacManSX.CanGhostMove(tileMap, ghostCoords, this.Direction, out motion, out targetTile))
                        {
                            this.Motion = motion;
                        }
                    }
                }
                else
                {
                    // If the ghost is not right ontop of the tile then continue to move.
                    if (this.Direction == Direction.Up)
                        motion.Y = -1;
                    else if (this.Direction == Direction.Down)
                        motion.Y = 1;
                    else if (this.Direction == Direction.Left)
                        motion.X = -1;
                    else if (this.Direction == Direction.Right)
                        motion.X = 1;
                }
            }

            // Check if we should move.
            if (motion != Vector2.Zero)
            {
                // Normalize the motion vector and move the ghost.
                motion.Normalize();

                this.Position.X += (float)Math.Round((motion * this.Speed * (float)elapsedGameTime.TotalSeconds).X);
                this.Position.Y += (float)Math.Round((motion * this.Speed * (float)elapsedGameTime.TotalSeconds).Y);
            }
        }

        #endregion
    }
}