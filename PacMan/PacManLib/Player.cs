using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacManLib
{
    public enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }

    public class Player
    {
        private FrameAnimation animation;
        private EngineManager engineManager;

        public Vector2 Position;

        public float rotation = 0;

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Constructs a new Player.
        /// </summary>
        /// <param name="engineManager">The engine manager.</param>
        /// <param name="spritesheet">The spritesheet texture.</param>
        /// <param name="frameWidth">The width of a frame.</param>
        /// <param name="frameHeight">The height of a frame.</param>
        public Player(EngineManager engineManager, Texture2D spritesheet, int frameWidth, int frameHeight)
        {
            this.Position = new Vector2(40, 40);
            this.engineManager = engineManager;
            this.animation = new FrameAnimation(spritesheet, frameWidth, frameHeight);
        }

        /// <summary>
        /// Updates the animation.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTimerEventArgs gameTime)
        {
            animation.Update(gameTime);
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTimerEventArgs gameTime)
        {
            this.engineManager.SpriteBatch.Begin();

            if (this.Direction == Direction.Up)
                this.engineManager.SpriteBatch.Draw(this.animation.Texture, this.Position, this.animation.CurrentSourceRectangle, Color.White, 1.6f, new Vector2(40, 40), 1, SpriteEffects.None, 0);
            else if (this.Direction == Direction.Down)
                this.engineManager.SpriteBatch.Draw(this.animation.Texture, this.Position, this.animation.CurrentSourceRectangle, Color.White, 1.6f, new Vector2(40, 40), 1, SpriteEffects.FlipHorizontally, 0);
            else if (this.Direction == Direction.Right)
                this.engineManager.SpriteBatch.Draw(this.animation.Texture, this.Position, this.animation.CurrentSourceRectangle, Color.White, 0f, new Vector2(40, 40), 1, SpriteEffects.FlipHorizontally, 0);
            else if (this.Direction == Direction.Left)
                this.engineManager.SpriteBatch.Draw(this.animation.Texture, this.Position, this.animation.CurrentSourceRectangle, Color.White, 0f, new Vector2(40, 40), 1, SpriteEffects.None, 0);

            this.engineManager.SpriteBatch.End();
        }
    }
}
