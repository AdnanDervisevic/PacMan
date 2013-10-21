using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PacManLib
{
    public sealed class FrameAnimation
    {
        #region Fields

        private List<Rectangle> sourceRectangles;
        private int index;
        private Texture2D texture;

        private float timer;
        private float frameLength;

        #endregion

        #region Properties

        public Texture2D Texture
        {
            get { return this.texture; }
        }

        /// <summary>
        /// Gets the currentIndex.
        /// </summary>
        public Rectangle CurrentSourceRectangle
        {
            get { return this.sourceRectangles[index]; }
        }

        public int FPS
        {
            set { this.frameLength = (float)Math.Max(1.0 / (float)value, .001f); }
        }

        #endregion

        public FrameAnimation(Texture2D texture, int frameWidth, int frameHeight)
        {
            this.timer = 0;
            this.index = 0;
            this.frameLength = .1f;
            this.sourceRectangles = new List<Rectangle>();
            this.texture = texture;

            int tilesInWidth = texture.Width / frameWidth;
            int tilesInHeight = texture.Height / frameHeight;

            for (int y = 0; y < tilesInHeight; y++)
                for (int x = 0; x < tilesInWidth; x++)
                    this.sourceRectangles.Add(new Rectangle(
                        x * frameWidth,
                        y * frameHeight,
                        frameWidth, frameHeight));
        }

        public void Update(GameTimerEventArgs gameTime)
        {
            if (this.sourceRectangles.Count > 2)
            {
                this.timer += (float)gameTime.ElapsedTime.TotalSeconds;

                if (this.timer > this.frameLength)
                {
                    this.timer = 0;
                    this.index = (this.index + 1) % this.sourceRectangles.Count;
                }
            }
        }

    }
}
