using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace vPilot_Pushover {

    internal class Acars {

        private static readonly Regex HoppieMessagePattern =
            new Regex(@"\{(\d+)\s(\w+)\s(\w+)\s\{([^\}]+)\}\}", RegexOptions.Compiled);
        private static readonly Regex DataPrefixPattern =
            new Regex(@"\/data\d\/\d+\/\d*\/.+\/", RegexOptions.Compiled);

        private const string HoppieBaseUrl = "http://www.hoppie.nl/acars/system/connect.html";
        private const double PollIntervalMs = 45 * 1000;

        private readonly Timer _hoppieTimer = new Timer();
        private readonly HashSet<string> _seenKeys = new HashSet<string>();

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

            _hoppieTimer.Elapsed += OnHoppieTimerElapsed;
            _hoppieTimer.Interval = PollIntervalMs;
        }

        public void Start() {
            _hoppieTimer.Start();
            _plugin.SendDebug("[ACARS] Starting ACARS");
            _ = FetchHoppieAsync();
        }

        public void Stop() {
            _hoppieTimer.Stop();
            _plugin.SendDebug("[ACARS] Stopping ACARS");
        }

        // async void is required by the ElapsedEventHandler signature; the actual work
        // lives in FetchHoppieAsync, which never throws.
        private async void OnHoppieTimerElapsed(object sender, ElapsedEventArgs e) {
            await FetchHoppieAsync();
        }

        private async Task FetchHoppieAsync() {
            string callsign = _plugin.ConnectedCallsign;
            if (callsign == null) {
                _plugin.SendDebug("[ACARS] FetchHoppie aborted due to missing callsign");
                return;
            }

            string url = $"{HoppieBaseUrl}?logon={_logon}&from={callsign}&type=peek&to=SERVER";
            _plugin.SendDebug($"[ACARS] Fetching Hoppie data with callsign {callsign}");

            try {
                HttpResponseMessage response = await Http.Client.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    ParseHoppie(await response.Content.ReadAsStringAsync());
                } else {
                    _plugin.SendDebug($"[ACARS] Hoppie request failed with status code: {response.StatusCode}");
                }
            } catch (Exception ex) {
                _plugin.SendDebug($"[ACARS] Hoppie request error: {ex.Message}");
            }
        }

        private void ParseHoppie(string response) {
            if (!response.StartsWith("ok", StringComparison.Ordinal)) {
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
                string message = DataPrefixPattern.Replace(match.Groups[4].Value, "").Replace("@", "");

                if (_cacheLoaded && !string.IsNullOrEmpty(message)) {
                    _ = _notifier.SendMessageAsync(message, $"{from} ({type.ToUpper()})", _priority, "ACARS message (Hoppie)");
                }

                _plugin.SendDebug($"[ACARS] Cached {key} with message: {message}");
            }

            _cacheLoaded = true;
        }

    }
}
