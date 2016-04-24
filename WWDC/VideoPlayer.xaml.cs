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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WWDC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayer : Page
    {
        public VideoPlayer()
        {
            this.InitializeComponent();
        }

        private DispatcherTimer controlsTimer = new DispatcherTimer();

        private Session session = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,0,0));

            session = e.Parameter as Session;
            UpdateUI();
        }

        private void ResetControlsTimer()
        {
            controlsTimer.Stop();
            controlsTimer = null;
            controlsTimer = new DispatcherTimer();
            controlsTimer.Interval = new TimeSpan(0, 0, 3);
            controlsTimer.Tick += ControlsTimerTick;
            controlsTimer.Start();
        }

        private bool needsRestorePosition = false;
        private bool restoredPosition = false;
        private double positionToRestore = 0.0;

        private void UpdateUI()
        {
            ResetControlsTimer();

            player.TransportControls.RequestedTheme = ElementTheme.Dark;
            player.Source = new Uri(session.download_hd);

            var position = SessionManager.SharedInstance.GetPosition(session);
            if (position > 0.0 && !SessionManager.SharedInstance.GetWatched(session))
            {
                positionToRestore = position;
                needsRestorePosition = true;
            }

            player.Play();
        }

        private void ControlsTimerTick(object sender, object args)
        {
            // mouse idle for 3 seconds, hide controls
            player.TransportControls.Visibility = Visibility.Collapsed;
            commandBar.Visibility = Visibility.Collapsed;
        }

        private void CloseVideo()
        {
            SaveCurrentPosition();
            player.Stop();
            player.IsFullWindow = false;
            Frame mainFrame = Window.Current.Content as Frame;
            mainFrame.GoBack();
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            CloseVideo();
        }

        private void PointerDidMove(object sender, PointerRoutedEventArgs e)
        {
            // show controls when mouse is moved
            player.TransportControls.Visibility = Visibility.Visible;
            commandBar.Visibility = Visibility.Visible;

            // restart timer to hide if the mouse is not moved again
            ResetControlsTimer();
        }

        private void DidFinishPlayback(object sender, RoutedEventArgs e)
        {
            SessionManager.SharedInstance.SetWatched(session, true);
        }

        private void SaveCurrentPosition()
        {
            SessionManager.SharedInstance.SetPosition(session, player.Position.TotalSeconds);

            // mark session as watched when player gets to 30 seconds from the end of the video
            var thresholdPosition = player.NaturalDuration.TimeSpan.TotalSeconds - 30.0;
            if (player.Position.TotalSeconds >= thresholdPosition)
            {
                SessionManager.SharedInstance.SetWatched(session, true);
            }
        }

        private void PlayerStateDidChange(object sender, RoutedEventArgs e)
        {
            if (player.CurrentState == MediaElementState.Playing && needsRestorePosition && !restoredPosition && player.CanSeek)
            {
                player.Position = TimeSpan.FromSeconds(positionToRestore);
                restoredPosition = true;
            }

            SaveCurrentPosition();
        }
    }
}
