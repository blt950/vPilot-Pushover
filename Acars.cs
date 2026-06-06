using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;

namespace vPilot_Pushover {

    internal class HoppieMessage {
        public string Key { get; set; }
        public string From { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }

    internal class Acars {

        private static readonly Regex HoppieMessagePattern =
            new Regex(@"\{(\d+)\s(\w+)\s(\w+)\s\{([^\}]+)\}\}", RegexOptions.Compiled);
        private static readonly Regex DataPrefixPattern =
            new Regex(@"\/data\d\/\d+\/\d*\/.+\/", RegexOptions.Compiled);
        private static readonly Regex AtSignPattern =
            new Regex(@"@", RegexOptions.Compiled);

        private const string HoppieBaseUrl = "http://www.hoppie.nl/acars/system/connect.html";
        private const double PollIntervalMs = 45 * 1000;

        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly Timer _hoppieTimer = new Timer();
        private readonly HashSet<string> _seenKeys = new HashSet<string>();
        private readonly List<HoppieMessage> _cache = new List<HoppieMessage>();

        private Main _plugin;
        private INotifier _notifier;
        private string _logon;
        private int _priority;
        private bool _cacheLoaded;

        public void Initialize(Main main, INotifier notifier, string logon, int priority) {
            _plugin = main;
            _notifier = notifier;
            _logon = logon;
            _priority = priority;

            _hoppieTimer.Elapsed += FetchHoppie;
            _hoppieTimer.Interval = PollIntervalMs;
        }

        public void Start() {
            _hoppieTimer.Enabled = true;
            _plugin.SendDebug("[ACARS] Starting ACARS");
            FetchHoppie(null, null);
        }

        public void Stop() {
            _hoppieTimer.Enabled = false;
            _plugin.SendDebug("[ACARS] Stopping ACARS");
        }

        // Timer event handler — async void is required by the ElapsedEventHandler signature.
        private async void FetchHoppie(object source, ElapsedEventArgs e) {
            string callsign = _plugin.ConnectedCallsign;
            if (callsign == null) {
                _plugin.SendDebug("[ACARS] FetchHoppie aborted due to missing callsign");
                return;
            }

            string url = $"{HoppieBaseUrl}?logon={_logon}&from={callsign}&type=peek&to=SERVER";
            _plugin.SendDebug($"[ACARS] Fetching Hoppie data with callsign {callsign}");

            try {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    string body = await response.Content.ReadAsStringAsync();
                    ParseHoppie(body);
                } else {
                    _plugin.SendDebug($"[ACARS] HttpResponse request failed with status code: {response.StatusCode}");
                }
            } catch (Exception ex) {
                _plugin.SendDebug($"[ACARS] An HttpResponse error occurred: {ex.Message}");
            }
        }

        private void ParseHoppie(string response) {
            if (!response.StartsWith("ok")) {
                _plugin.SendDebug("[ACARS] okCheck Error: " + response);
                return;
            }

            foreach (Match match in HoppieMessagePattern.Matches(response)) {
                string key = match.Groups[1].Value;
                if (!_seenKeys.Add(key)) {
                    continue;
                }

                string from = match.Groups[2].Value;
                string type = match.Groups[3].Value;
                string message = match.Groups[4].Value;

                message = DataPrefixPattern.Replace(message, "");
                message = AtSignPattern.Replace(message, "");

                _cache.Add(new HoppieMessage {
                    Key = key,
                    From = from,
                    Type = type,
                    Message = message
                });

                if (_cacheLoaded && message != "") {
                    _ = _notifier.SendMessageAsync(message, $"{from} ({type.ToUpper()})", _priority);
                }

                _plugin.SendDebug($"[ACARS] Cached {key} with message: {message}");
            }

            _cacheLoaded = true;
        }

    }
}
