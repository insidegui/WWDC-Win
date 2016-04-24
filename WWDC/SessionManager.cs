using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Newtonsoft.Json;
using System.IO;

using Windows.Storage;

namespace WWDC
{
    public sealed class SessionManager
    {
        private static volatile SessionManager _sharedInstance;
        private static object _lock = new Object();

        private SessionManager() { }

        public static SessionManager SharedInstance
        {
            get
            {
                if (_sharedInstance == null)
                {
                    lock (_lock)
                    {
                        if (_sharedInstance == null) _sharedInstance = new SessionManager();
                    }
                }

                return _sharedInstance;
            }
        }

        public delegate void DidLoadSessionsCallback(string error = null);

        public List<Session> sessions = new List<Session>();

        private static string configurationURL = "http://wwdc.guilhermerambo.me/index.json";

        public async void LoadSessions(DidLoadSessionsCallback Callback)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(configurationURL) as HttpWebRequest;
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Received error response from config server");

                using (var reader = new StreamReader(response.GetResponseStream(), System.Text.UTF8Encoding.UTF8))
                {
                    var definition = new { sessions = "", url = "", wwdc_week = false };
                    string responseText = await reader.ReadToEndAsync();
                    var configuration = JsonConvert.DeserializeAnonymousType(responseText, definition);
                    DoLoadSessions(configuration.url, Callback);
                }
            }
            catch (Exception e)
            {
                Callback("Unable to download or parse configuration file.\n" + e.ToString());
            }
        }

        private async void DoLoadSessions(string sessionsURL, DidLoadSessionsCallback Callback)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(sessionsURL) as HttpWebRequest;
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Received error response from server");
                }
                using (var reader = new StreamReader(response.GetResponseStream(), System.Text.UTF8Encoding.UTF8))
                {
                    var definition = new { sessions = new List<Session>() };
                    string responseText = await reader.ReadToEndAsync();
                    var parsedResponse = JsonConvert.DeserializeAnonymousType(responseText, definition);
                    var sortedSessions = from session in parsedResponse.sessions
                                         orderby session.year descending, session.id ascending
                                         select session;
                    sessions = sortedSessions.ToList();

                    Callback();
                }
            }
            catch (Exception e)
            {
                Callback("Unable to download or parse sessions file.\n" + e.ToString());
            }
        }

        private ApplicationDataContainer settings = ApplicationData.Current.RoamingSettings;

        private string FavoriteKey(Session session)
        {
            return session.year + "-" + session.id + "-favorite";
        }

        public void SetFavorite(Session session, bool favorite)
        {
            settings.Values[FavoriteKey(session)] = favorite;
        }
        public bool GetFavorite(Session session)
        {
            var favorite = settings.Values[FavoriteKey(session)];
            if (favorite == null)
            {
                return false;
            } else
            {
                return (bool)favorite;
            }
        }

        private string PositionKey(Session session)
        {
            return session.year + "-" + session.id + "-position";
        }

        public void SetPosition(Session session, double position)
        {
            settings.Values[PositionKey(session)] = position;
        }
        public double GetPosition(Session session)
        {
            var position = settings.Values[PositionKey(session)];
            if (position == null)
            {
                return 0.0;
            } else
            {
                return (double)position;
            }
        }

        private string WatchedKey(Session session)
        {
            return session.year + "-" + session.id + "-watched";
        }

        public void SetWatched(Session session, bool watched)
        {
            settings.Values[WatchedKey(session)] = watched;
        }
        public bool GetWatched(Session session)
        {
            var watched = settings.Values[WatchedKey(session)];
            if (watched == null)
            {
                return false;
            } else
            {
                return (bool)watched;
            }
        }

    }
    
}
