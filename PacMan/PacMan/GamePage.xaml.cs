using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PacMan
{
    public partial class GamePage : PhoneApplicationPage
    {
        private PacManLib.PacMan pacMan;
        private GameTimer timer;

        public GamePage()
        {
            InitializeComponent();

            // Initializes the game and passes the content manager from the application.
            pacMan = new PacManLib.PacMan((Application.Current as App).Content);

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Initializes the game.
            pacMan.Initialize(new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice), 
                SharedGraphicsDeviceManager.Current.PreferredBackBufferWidth, SharedGraphicsDeviceManager.Current.PreferredBackBufferHeight);

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // This updates the entire game.
            pacMan.Update(e);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            // This draws the entire game.
            pacMan.Draw(e);
        }

        /// <summary>
        /// Event fired when the phone changes orientation, this is used to track whether we should adapt
        /// the controls for landscape left or right.
        /// </summary>
        /// <param name="e">The Orientation changed event arguments.</param>
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft)
            {
                if (this.pacMan != null && this.pacMan.EngineManager != null)
                    this.pacMan.EngineManager.Orientation = DisplayOrientation.LandscapeLeft;
            }
            else if (e.Orientation == PageOrientation.LandscapeRight)
            {
                if (this.pacMan != null && this.pacMan.EngineManager != null)
                    this.pacMan.EngineManager.Orientation = DisplayOrientation.LandscapeRight;
            }

            base.OnOrientationChanged(e);
        }
    }
}