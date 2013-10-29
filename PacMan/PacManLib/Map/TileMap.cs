#region File Description
    //////////////////////////////////////////////////////////////////////////
   // TileMap                                                              //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib.Map
{
    /// <summary>
    /// Class representing a single map.
    /// </summary>
    public sealed class TileMap
    {
        #region Fields

        private Texture2D doorTexture = null;
        private Tileset tileset = null;
        private Tile[,] map = null;
        private GameManager gameManager = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the level number.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets the width of the map.
        /// </summary>
        public int Width
        {
            get { return this.map.GetLength(1); }
        }

        /// <summary>
        /// Gets the height of the map.
        /// </summary>
        public int Height
        {
            get { return this.map.GetLength(0); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new map by a given multidimensional array.
        /// </summary>
        /// <param name="gameManager">The game manager.</param>
        /// <param name="map">The map.</param>
        public TileMap(GameManager gameManager)
        {
            this.gameManager = gameManager;
            this.tileset = new Tileset(
                this.gameManager.ContentManager.Load<Texture2D>("Tileset"),
                PacManSX.TileWidth, PacManSX.TileHeight);
            this.doorTexture = this.gameManager.ContentManager.Load<Texture2D>("Door");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a map.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="map">The map.</param>
        public void LoadMap(int level, int[,] map)
        {
            this.Level = level;

            this.map = new Tile[map.GetLength(0), map.GetLength(1)];
            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                    this.map[y, x] = new Tile()
                    {
                        TileContent = (TileContent)map[y, x]
                    };
        }

        /// <summary>
        /// Returns the amount of dots and rings on the map.
        /// </summary>
        /// <returns></returns>
        public int DotsAndRings()
        {
            int dotsAndRings = 0;

            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                    if (this.map[y, x].TileContent == TileContent.Dot || this.map[y, x].TileContent == TileContent.DotTurn
                        || this.map[y, x].TileContent == TileContent.Ring || this.map[y, x].TileContent == TileContent.RingTurn)
                        dotsAndRings++;

            return dotsAndRings;
        }

        /// <summary>
        /// Gets the Tile where the given spawnpoint are.
        /// </summary>
        /// <param name="spawnPoint">The spawn point.</param>
        /// <returns>Returns either the coords for the given spawn point or a negative coords.</returns>
        public Point GetSpawn(SpawnPoint spawnPoint)
        {
            for (int y = 0; y < this.map.GetLength(0); y++)
                for (int x = 0; x < this.map.GetLength(1); x++)
                    if (this.map[y, x].SpawnPoint == spawnPoint)
                        return new Point(x, y);

            return new Point(-1, -1);
        }

        /// <summary>
        /// Gets an array of spawn coords from an array of spawn points.
        /// </summary>
        /// <param name="spawnPoints">The array of spawn points.</param>
        /// <returns>The array of spawn coords.</returns>
        public Point[] GetSpawns(params SpawnPoint[] spawnPoints)
        {
            Point[] spawns = new Point[spawnPoints.GetLength(0)];
            for (int i = 0; i < spawns.GetLength(0); i++)
                spawns[i] = new Point(-1, -1);

            for (int y = 0; y < this.map.GetLength(0); y++)
                for (int x = 0; x < this.map.GetLength(1); x++)
                    for (int i = 0; i < spawnPoints.GetLength(0); i++)
                        if (this.map[y,x].SpawnPoint == spawnPoints[i])
                            spawns[i] = new Point(x, y);

            return spawns;
        }

        /// <summary>
        /// Gets the tile by its coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>The tile.</returns>
        public Tile GetTile(Point coordinates)
        {
            if (coordinates.Y < 0 || coordinates.X < 0 || coordinates.Y >= this.map.GetLength(0) || coordinates.X >= this.map.GetLength(1))
                return null;

            return this.map[coordinates.Y, coordinates.X];
        }

        /// <summary>
        /// Updates a single tile. This is used to destroy barriers etc.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        /// <param name="tileContent">The tile content.</param>
        public void UpdateTile(Point coordinates, TileContent tileContent)
        {
            if (coordinates.Y < 0 || coordinates.X < 0 || coordinates.Y >= this.map.GetLength(0) || coordinates.X >= this.map.GetLength(1))
                return;

            this.map[coordinates.Y, coordinates.X].TileContent = tileContent;
        }

        /// <summary>
        /// Updates a single tile. This is used to set spawn points.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        /// <param name="spawnPoint">The type of spawnpoint.</param>
        public void UpdateTile(Point coordinates, SpawnPoint spawnPoint)
        {
            if (coordinates.Y < 0 || coordinates.X < 0 || coordinates.Y >= this.map.GetLength(0) || coordinates.X >= this.map.GetLength(1))
                return;

            this.map[coordinates.Y, coordinates.X].SpawnPoint = spawnPoint;
        }

        /// <summary>
        /// Allows the game to draw itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last draw.</param>
        public void Draw(TimeSpan elapsedGameTime)
        {
            this.gameManager.SpriteBatch.Begin();

            for (int y = 0; y < this.map.GetLength(0); y++)
                for (int x = 0; x < this.map.GetLength(1); x++)
                {
                    if (this.map[y, x].TileContent > TileContent.Path && this.map[y, x].TileContent < TileContent.RingTurn)
                        this.gameManager.SpriteBatch.Draw(
                            this.tileset.Texture, new Vector2(x * PacManSX.TileWidth, y * PacManSX.TileHeight + PacManSX.TitleHeight),
                            this.tileset.GetSourceRectangle((int)this.map[y, x].TileContent - 1), Color.White);
                    else if (this.map[y, x].TileContent > TileContent.Dot && this.map[y, x].TileContent < TileContent.Turn)
                        this.gameManager.SpriteBatch.Draw(
                            this.tileset.Texture, new Vector2(x * PacManSX.TileWidth, y * PacManSX.TileHeight + PacManSX.TitleHeight),
                            this.tileset.GetSourceRectangle((int)this.map[y, x].TileContent - 3), Color.White);
                    else if (this.map[y, x].TileContent == TileContent.Door)
                        this.gameManager.SpriteBatch.Draw(this.doorTexture, new Vector2(x * PacManSX.TileWidth - 5, y * PacManSX.TileHeight + PacManSX.TitleHeight), Color.White);
                }

            this.gameManager.SpriteBatch.End();
        }

        #endregion
    }
}