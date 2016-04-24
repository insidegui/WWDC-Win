using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WWDC
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            SessionManager.SharedInstance.LoadSessions(new SessionManager.DidLoadSessionsCallback(DidLoadSessions));
        }

        private async void DidLoadSessions(string error)
        {
            if (error != null)
            {
                var popup = new MessageDialog(error, "Error loading sessions");
                await popup.ShowAsync();

                return;
            }

            foreach (var session in SessionManager.SharedInstance.sessions)
            {
                sessionsListView.Items.Add(session);
            }
        }

        private Session selectedSession = null;

        private void watchVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var mainFrame = Window.Current.Content as Frame;
            mainFrame.Navigate(typeof(VideoPlayer), selectedSession);
        }

        private void DidSelectSession(object sender, SelectionChangedEventArgs e)
        {
            Session selectedSession = e.AddedItems.First() as Session;
            if (selectedSession == null)
            {
                ShowEmptyState();
            } else
            {
                ShowSession(selectedSession);
            }
        }

        private void ShowEmptyState()
        {
            selectedSessionDescriptionTextBlock.Text = "";
            selectedSessionTitleTextBlock.Text = "No session selected";
            selectedSessionSummaryTextBlock.Text = "Select a session to see It here.";

            watchVideoButton.Visibility = Visibility.Collapsed;
            toggleFavoriteButton.Visibility = Visibility.Collapsed;
            toggleWatchedButton.Visibility = Visibility.Collapsed;
        }

        private void ShowSession(Session session)
        {
            watchVideoButton.Visibility = Visibility.Visible;
            toggleFavoriteButton.Visibility = Visibility.Visible;
            toggleWatchedButton.Visibility = Visibility.Visible;

            selectedSessionTitleTextBlock.Text = session.title;
            selectedSessionSummaryTextBlock.Text = "WWDC " + session.year + " | " + session.track + " | Session " + session.id;
            selectedSessionDescriptionTextBlock.Text = session.description;

            if (SessionManager.SharedInstance.GetFavorite(session))
            {
                toggleFavoriteButton.Tag = true;
                toggleFavoriteButton.Content = "Remove from Favorites";
            } else {
                toggleFavoriteButton.Tag = false;
                toggleFavoriteButton.Content = "Add to Favorites";
            }

            if (SessionManager.SharedInstance.GetWatched(session))
            {
                toggleWatchedButton.Tag = true;
                toggleWatchedButton.Content = "Mark as Unwatched";
            } else
            {
                toggleWatchedButton.Tag = false;
                toggleWatchedButton.Content = "Mark as Watched";
            }

            if (SessionManager.SharedInstance.GetPosition(session) > 0.0 && !SessionManager.SharedInstance.GetWatched(session))
            {
                watchVideoButton.Content = "Continue Watching";
            } else
            {
                watchVideoButton.Content = "Watch Video";
            }

            selectedSession = session;
        }

        private void toggleFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SharedInstance.SetFavorite(selectedSession, !(bool)toggleFavoriteButton.Tag);
            ShowSession(selectedSession);
        }

        private void toggleWatchedButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SharedInstance.SetWatched(selectedSession, !(bool)toggleWatchedButton.Tag);
            ShowSession(selectedSession);
        }
    }
}
