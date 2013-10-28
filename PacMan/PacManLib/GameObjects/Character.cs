#region File Description
    //////////////////////////////////////////////////////////////////////////
   // Character                                                            //
  //                                                                      //
 // Copyright (C) Untitled. All Rights reserved.                         //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion End of Using Statements

namespace PacManLib.GameObjects
{
    public class Character
    {
        #region Fields

        private SpriteAnimation animation;
        private GameManager gameManager;

        private Vector2 origin;

        public Vector2 Position;

        #endregion

        #region Properties

        /// <summary>
        /// Determines if the character should be drawn.
        /// </summary>
        public bool Alive { get; set; }

        /// <summary>
        /// Gets or sets the speed of the character.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the motion of the character.
        /// </summary>
        public Vector2 Motion { get; set; }

        /// <summary>
        /// Gets or sets the direction the character is moving towards.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Gets the center of the character.
        /// </summary>
        public Vector2 Center
        {
            get { return this.Position + this.origin - new Vector2(4, 4); }
        }

        /// <summary>
        /// Gets the bounds of the character.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                Rectangle rect = this.animation.CurrentSourceRectangle;
                rect.X = (int)this.Position.X;
                rect.Y = (int)this.Position.Y;
                return rect;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new character.
        /// </summary>
        /// <param name="gameManager">The game manager.</param>
        /// <param name="origin">The origin of the character.</param>
        /// <param name="texture">The character spritesheet.</param>
        /// <param name="frameWidth">The width of a single frame.</param>
        /// <param name="frameHeight">The height of a single frame.</param>
        public Character(GameManager gameManager, Vector2 position, Texture2D texture, int frameWidth, int frameHeight)
        {
            this.Alive = true;
            this.gameManager = gameManager;
            this.Position = position;
            this.origin = new Vector2(frameWidth / 2, frameHeight / 2);
            this.animation = new SpriteAnimation(texture, frameWidth, frameHeight);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows the game to update itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last update.</param>
        public void Update(TimeSpan elapsedGameTime)
        {
            if (this.Alive)
                this.animation.Update(elapsedGameTime);
        }

        /// <summary>
        /// Allows the game to draw itself.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed time since the last draw.</param>
        public void Draw(TimeSpan elapsedGameTime)
        {
            if (!this.Alive)
                return;

            this.gameManager.SpriteBatch.Begin();

            if (this.Direction == Direction.Up)
                this.gameManager.SpriteBatch.Draw(this.animation.Texture, this.Center, this.animation.CurrentSourceRectangle, Color.White, 1.6f, this.origin, 1, SpriteEffects.None, 0);
            else if (this.Direction == Direction.Down)
                this.gameManager.SpriteBatch.Draw(this.animation.Texture, this.Center, this.animation.CurrentSourceRectangle, Color.White, 1.6f, this.origin, 1, SpriteEffects.FlipHorizontally, 0);
            else if (this.Direction == Direction.Right)
                this.gameManager.SpriteBatch.Draw(this.animation.Texture, this.Center, this.animation.CurrentSourceRectangle, Color.White, 0f, this.origin, 1, SpriteEffects.FlipHorizontally, 0);
            else if (this.Direction == Direction.Left)
                this.gameManager.SpriteBatch.Draw(this.animation.Texture, this.Center, this.animation.CurrentSourceRectangle, Color.White, 0f, this.origin, 1, SpriteEffects.None, 0);

            this.gameManager.SpriteBatch.End();
        }

        #endregion
    }
}