#region File Description
    //////////////////////////////////////////////////////////////////////////
   // GamePage                                                             //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacManLib;
#endregion End of Using Statements

namespace PacMan
{
    public partial class GamePage : PhoneApplicationPage
    {
        #region Fields

        private PacManSX pacMan = null;
        private GameTimer timer = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new game page.
        /// </summary>
        public GamePage()
        {
            InitializeComponent();

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method runs when you navigate to the game page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Initializes the game.
            this.pacMan = new PacManSX(new GameManager(
                SharedGraphicsDeviceManager.Current.PreferredBackBufferWidth,
                SharedGraphicsDeviceManager.Current.PreferredBackBufferHeight,
                new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice), (Application.Current as App).Content));


            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Method runs when you navigate away from the game page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // This updates the entire game.
            this.pacMan.Update(e.ElapsedTime);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            // This draws the entire game.
            this.pacMan.Draw(e.ElapsedTime);
        }

        #endregion
    }
}