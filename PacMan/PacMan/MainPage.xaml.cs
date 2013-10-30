#region File Description
    //////////////////////////////////////////////////////////////////////////
   // MainPage                                                             //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

#region Using Statements
using System;
using System.Windows;
using Microsoft.Phone.Controls;
#endregion End of Using Statements

namespace PacMan
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Constructors

        /// <summary>
        /// Constructs a new main page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Helpers
        
        /// <summary>
        /// When the user clicks on the button the game page will load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        #endregion
    }
}