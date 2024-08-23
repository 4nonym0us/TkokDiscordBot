using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.EntGaming.Dto;

namespace TkokDiscordBot.EntGaming
{
    //TODO: atm cient is singleton and it uses only one account.
    //Consider using account dispatcher, store cookies in cache and implement Polly policies to re-auth and refresh cookies for accounts
    public class EntClient : IDisposable
    {
        private readonly string _baseUrl = "https://entgaming.net";
        private readonly CookieContainer _cookieContainer;

        private readonly Regex _csrfTokenRegex =
            new Regex(@"<input type=\""hidden\"" name=\""sid\"" value=\""([a-f0-9]{32})\"" />", RegexOptions.Compiled);

        private readonly Thread _gameInfoUpdaterThread;

        private readonly Regex _gameNameHostedRegex = new Regex(@"the gamename is <b>(\S+)</b>", RegexOptions.Compiled);
        private readonly Regex _gameNameRegex = new Regex(@"<b>GAMENAME: (\S+)</b>", RegexOptions.Compiled);
        private readonly ISettings _settings;
        private LobbyStatus _gameInfo;

        public EntClient(ISettings settings)
        {
            Console.WriteLine(Guid.NewGuid() + "XXX");
            _settings = settings;
            _cookieContainer = new CookieContainer();

            _gameInfoUpdaterThread = new Thread(GameInfoUpdater);
            _gameInfoUpdaterThread.Start();
        }

        /// <summary>
        ///     Status of current Game or null (if game doesn't exist).
        /// </summary>
        public LobbyStatus GameInfo
        {
            get => _gameInfo;
            set
            {
                if (_gameInfo != value)
                {
                    _gameInfo = value;
                    OnPropertyChanged(nameof(GameInfo));
                }
            }
        }

        public void Dispose()
        {
            _gameInfoUpdaterThread.Abort();
        }

        private async void GameInfoUpdater()
        {
            while (true)
            {
                await UpdateBotStatus();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        /// <summary>
        ///     Hosts the game
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<GameHostedResponse> HostGame(string owner, string location)
        {
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentNullException(nameof(owner));
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException(nameof(location));
            if (!Regex.IsMatch(location, "^(atlanta|ny|la|europe|au|jp|sg)$"))
                throw new ArgumentException(nameof(location));

            await Login();

            using (var handler = new HttpClientHandler {CookieContainer = _cookieContainer})
            using (var client = new HttpClient(handler))
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("owner", owner),
                    new KeyValuePair<string, string>("map", _settings.EntMap),
                    new KeyValuePair<string, string>("location", location)
                });
                var response = await client.PostAsync(_baseUrl + "/link/host.php", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (_gameNameRegex.IsMatch(responseContent))
                {
                    GameInfo = new LobbyStatus(_gameNameRegex.Match(responseContent).Result("$1"));

                    OnPropertyChanged(nameof(GameInfo));
                    return new GameHostedResponse("New game was succesfully hosted.", GameInfo.GameName);
                }
                if (_gameNameHostedRegex.IsMatch(responseContent))
                {
                    GameInfo = new LobbyStatus(_gameNameHostedRegex.Match(responseContent).Result("$1"));

                    OnPropertyChanged(nameof(GameInfo));
                    return new GameHostedResponse("There is a game, which is beeing hosted.", GameInfo.GameName);
                }
                return new GameHostedResponse("Failed to host the game due to unknown error.");
            }
        }

        /// <summary>
        ///     Logs in and stores cookie in a <see cref="CookieContainer" />
        /// </summary>
        /// <returns></returns>
        public async Task Login()
        {
            using (var handler = new HttpClientHandler {CookieContainer = _cookieContainer})
            using (var client = new HttpClient(handler))
            {
                var tokenRequest = await client.GetStringAsync(_baseUrl + "/forum/ucp.php?mode=login");
                if (!_csrfTokenRegex.IsMatch(tokenRequest))
                    return;
                var csrfToken = _csrfTokenRegex.Match(tokenRequest).Result("$1");

                var content = new Collection<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", _settings.EntUsername),
                    new KeyValuePair<string, string>("password", _settings.EntPassword),
                    new KeyValuePair<string, string>("redirect", "./ucp.php?mode=login"),
                    new KeyValuePair<string, string>("sid", csrfToken),
                    new KeyValuePair<string, string>("redirect", "index.php"),
                    new KeyValuePair<string, string>("login", "Login")
                };
                var response = await client.PostAsync(_baseUrl + "/forum/ucp.php?mode=login",
                    new FormUrlEncodedContent(content));
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                if (!responseContent.Contains($"<span class=\"username\">{_settings.EntUsername}</span></a>"))
                    throw new Exception(); //EntAuthException
            }
        }

        /// <summary>
        ///     Uploads status of <see cref="GameInfo" />
        /// </summary>
        /// <returns></returns>
        protected async Task UpdateBotStatus()
        {
            using (var client = new HttpClient())
            {
                var response =
                    await client.GetStringAsync("https://entgaming.net/forum/games_fast.php?no-cache=" +
                                                Guid.NewGuid());
                string gameDataStr = null;
                if (GameInfo?.Id != null)
                    gameDataStr = response
                        .Split('\n')
                        .FirstOrDefault(r =>
                            r.IndexOf(GameInfo.Id.ToString(), StringComparison.OrdinalIgnoreCase) >= 0);
                else if (!string.IsNullOrEmpty(GameInfo?.GameName))
                    gameDataStr = response
                        .Split('\n')
                        .FirstOrDefault(r => r.IndexOf(GameInfo.GameName, StringComparison.OrdinalIgnoreCase) >= 0);

                if (gameDataStr == null)
                {
                    GameInfo = null;
                    return;
                }

                var gameData = gameDataStr.Split('|');
                GameInfo = new LobbyStatus(
                    gameData[5],
                    Convert.ToInt32(gameData[0]),
                    Convert.ToInt32(gameData[2]),
                    Convert.ToInt32(gameData[3]));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler GameInfoChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = GameInfoChanged;
            handler?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}