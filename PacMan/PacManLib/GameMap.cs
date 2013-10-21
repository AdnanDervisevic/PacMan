using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacManLib.Map
{
    public sealed class GameMap
    {
        private Tileset tileset = null;
        private int tileWidth;
        private int tileHeight;
        private Tile[,] map = null;
        private EngineManager engineManager = null;

        /// <summary>
        /// Constructs a new map.
        /// </summary>
        /// <param name="map">The map layout.</param>
        public GameMap(EngineManager engineManager, int[,] map, int tileWidth, int tileHeight)
        {
            this.engineManager = engineManager;

            this.map = new Tile[map.GetLength(0), map.GetLength(1)];
            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    this.map[x, y] = new Tile() { ContentCode = (TileContent)map[x, y] };

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here
            this.tileset = new Tileset(this.engineManager.Content.Load<Texture2D>("tileset"), 40, 40);
        }

        #region Update & Draw

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
            // TODO: Add your draw code here
            this.engineManager.SpriteBatch.Begin();

            for (int y = 0; y < this.map.GetLength(0); y++)
            {
                for (int x = 0; x < this.map.GetLength(1); x++)
                {
                    this.engineManager.SpriteBatch.Draw(
                        this.tileset.Texture, new Vector2(x * this.tileWidth, y * this.tileHeight),
                        this.tileset.GetSourceRectangle((int)this.map[y, x].ContentCode), Color.White);
                }
            }

            this.engineManager.SpriteBatch.End();
        }

        #endregion

        /// <summary>
        /// Updates the tileContent for a specific tile.
        /// </summary>
        /// <param name="point">The tile position.</param>
        /// <param name="tileContent">The new tile content.</param>
        public void UpdateTile(Point point, TileContent tileContent)
        {
            this.map[point.Y, point.X].ContentCode = tileContent;
        }
    }
}