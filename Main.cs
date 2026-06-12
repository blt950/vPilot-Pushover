using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Win32;

using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;

using vPilot_Pushover.Notifications;

namespace vPilot_Pushover {

    internal class PluginSettings {
        public string Driver { get; set; }
        public bool PrivateEnabled { get; set; }
        public bool RadioEnabled { get; set; }
        public bool SelcalEnabled { get; set; }
        public bool HoppieEnabled { get; set; }
        public bool DisconnectEnabled { get; set; }
        public string HoppieLogon { get; set; }
        public string PushoverToken { get; set; }
        public string PushoverUser { get; set; }
        public string PushoverDevice { get; set; }
        public string TelegramBotToken { get; set; }
        public string TelegramChatId { get; set; }
        public string GotifyUrl { get; set; }
        public string GotifyToken { get; set; }

        // Per-message-type priority, passed through to the driver.
        // Driver semantics: Pushover -2..2, Gotify 0..10, Telegram ignored.
        public int PrivatePriority { get; set; }
        public int RadioPriority { get; set; }
        public int SelcalPriority { get; set; }
        public int HoppiePriority { get; set; }
        public int DisconnectPriority { get; set; }
    }

    public class Main : IPlugin {

        public const string Version = "1.2.0";

        private static readonly HttpClient _httpClient = new HttpClient();

        // Driver factory — maps the ini Driver value to a constructor + config builder.
        private static readonly Dictionary<string, Func<PluginSettings, INotifier>> _driverFactories =
            new Dictionary<string, Func<PluginSettings, INotifier>>(StringComparer.OrdinalIgnoreCase) {
                {
                    "pushover",
                    s => {
                        var n = new Drivers.Pushover();
                        n.Initialize(new NotifierConfig {
                            PushoverToken = s.PushoverToken,
                            PushoverUser = s.PushoverUser,
                            PushoverDevice = s.PushoverDevice
                        });
                        return n;
                    }
                },
                {
                    "telegram",
                    s => {
                        var n = new Drivers.Telegram();
                        n.Initialize(new NotifierConfig {
                            TelegramBotToken = s.TelegramBotToken,
                            TelegramChatId = s.TelegramChatId
                        });
                        return n;
                    }
                },
                {
                    "gotify",
                    s => {
                        var n = new Drivers.Gotify();
                        n.Initialize(new NotifierConfig {
                            GotifyUrl = s.GotifyUrl,
                            GotifyToken = s.GotifyToken
                        });
                        return n;
                    }
                }
            };

        private IBroker _vPilot;
        private INotifier _notifier;
        private Acars _acars;
        private PluginSettings _settings;
        private bool _settingsLoaded;

        public string Name { get; } = "vPilot Pushover";

        // Exposed for Acars
        public string ConnectedCallsign { get; private set; }

        public void Initialize(IBroker broker) {
            _vPilot = broker;
            LoadSettings();

            if (!_settingsLoaded) {
                ReportLoadFailure($"{Name} plugin failed to load. Check your vPilot-Pushover.ini");
                return;
            }

            if (_settings.Driver == null || !_driverFactories.TryGetValue(_settings.Driver, out var factory)) {
                ReportLoadFailure("Driver not set correctly. Check your vPilot-Pushover.ini");
                return;
            }

            _notifier = factory(_settings);
            if (!_notifier.HasValidConfig()) {
                ReportLoadFailure($"{_settings.Driver} configuration is invalid. Check your vPilot-Pushover.ini");
                return;
            }

            SendDebug($"Driver set to {_settings.Driver}");

            // Subscribe to events according to settings
            _vPilot.NetworkConnected += OnNetworkConnected;
            _vPilot.NetworkDisconnected += OnNetworkDisconnected;
            if (_settings.PrivateEnabled) _vPilot.PrivateMessageReceived += OnPrivateMessageReceived;
            if (_settings.RadioEnabled) _vPilot.RadioMessageReceived += OnRadioMessageReceived;
            if (_settings.SelcalEnabled) _vPilot.SelcalAlertReceived += OnSelcalAlertReceived;

            // Enable ACARS if Hoppie is enabled
            if (_settings.HoppieEnabled) {
                _acars = new Acars();
                _acars.Initialize(this, _notifier, _settings.HoppieLogon, _settings.HoppiePriority);
            }

            _ = _notifier.SendMessageAsync($"Connected. Running version v{Version}");
            SendDebug($"{Name} connected and enabled on v{Version}");

            _ = CheckForUpdatesAsync();
        }

        public void SendDebug(string text) {
            _vPilot.PostDebugMessage(text);
        }

        private void ReportLoadFailure(string message) {
            SendDebug(message);
            LoadFailureNotifier.Show(Name, message);
        }

        private void OnNetworkConnected(object sender, NetworkConnectedEventArgs e) {
            ConnectedCallsign = e.Callsign;

            if (_settings.HoppieEnabled) {
                _acars.Start();
            }
        }

        private void OnNetworkDisconnected(object sender, EventArgs e) {
            ConnectedCallsign = null;

            if (_settings.HoppieEnabled) {
                _acars.Stop();
            }

            if (_settings.DisconnectEnabled) {
                _ = _notifier.SendMessageAsync("Disconnected from network", "vPilot", _settings.DisconnectPriority);
            }
        }

        private void OnPrivateMessageReceived(object sender, PrivateMessageReceivedEventArgs e) {
            _ = _notifier.SendMessageAsync(e.Message, e.From, _settings.PrivatePriority);
        }

        private void OnRadioMessageReceived(object sender, RadioMessageReceivedEventArgs e) {
            if (ConnectedCallsign != null && e.Message.Contains(ConnectedCallsign)) {
                _ = _notifier.SendMessageAsync(e.Message, e.From, _settings.RadioPriority);
            }
        }

        private void OnSelcalAlertReceived(object sender, SelcalAlertReceivedEventArgs e) {
            _ = _notifier.SendMessageAsync("SELCAL Alert", e.From, _settings.SelcalPriority);
        }

        private void LoadSettings() {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\vPilot")) {
                if (registryKey == null) {
                    SendDebug("Registry key not found. Is vPilot installed correctly?");
                    return;
                }

                string vPilotPath = (string)registryKey.GetValue("Install_Dir");
                string configFile = vPilotPath + @"\Plugins\vPilot-Pushover.ini";
                var ini = new IniFile(configFile);

                _settings = new PluginSettings {
                    Driver = ini.Read("Driver", "General", null),
                    PushoverToken = ini.Read("ApiKey", "Pushover", null),
                    PushoverUser = ini.Read("UserKey", "Pushover", null),
                    PushoverDevice = ini.Read("Device", "Pushover", null),
                    HoppieEnabled = ParseBool(ini.Read("Enabled", "Hoppie", null)),
                    HoppieLogon = ini.Read("LogonCode", "Hoppie", null),
                    PrivateEnabled = ParseBool(ini.Read("Enabled", "RelayPrivate", null)),
                    RadioEnabled = ParseBool(ini.Read("Enabled", "RelayRadio", null)),
                    SelcalEnabled = ParseBool(ini.Read("Enabled", "RelaySelcal", null)),
                    TelegramBotToken = ini.Read("BotToken", "Telegram", null),
                    TelegramChatId = ini.Read("ChatId", "Telegram", null),
                    DisconnectEnabled = ParseBool(ini.Read("Enabled", "Disconnect", null)),
                    GotifyUrl = ini.Read("Url", "Gotify", null),
                    GotifyToken = ini.Read("Token", "Gotify", null),
                    PrivatePriority = ParseInt(ini.Read("Priority", "RelayPrivate", null), 1),
                    RadioPriority = ParseInt(ini.Read("Priority", "RelayRadio", null), 1),
                    SelcalPriority = ParseInt(ini.Read("Priority", "RelaySelcal", null), 1),
                    HoppiePriority = ParseInt(ini.Read("Priority", "Hoppie", null), 0),
                    DisconnectPriority = ParseInt(ini.Read("Priority", "Disconnect", null), 1)
                };

                // Notify if Hoppie is enabled but LogonCode is missing, and disable ACARS to prevent errors.
                if (_settings.HoppieEnabled && string.IsNullOrWhiteSpace(_settings.HoppieLogon)) {
                    _settings.HoppieEnabled = false;
                    ReportLoadFailure(
                        "Hoppie is enabled but the LogonCode is missing. ACARS has been disabled. " +
                        "Set LogonCode in vPilot-Pushover.ini under [Hoppie].");
                }

                _settingsLoaded = true;
            }
        }

        private static bool ParseBool(string value) {
            return value != null && bool.TryParse(value, out bool result) && result;
        }

        private static int ParseInt(string value, int defaultValue) {
            return value != null && int.TryParse(value, out int result) ? result : defaultValue;
        }

        private async Task CheckForUpdatesAsync() {
            try {
                HttpResponseMessage response = await _httpClient.GetAsync(
                    "https://raw.githubusercontent.com/blt950/vPilot-Pushover/main/version.txt");

                if (!response.IsSuccessStatusCode) {
                    SendDebug($"[Update Checker] HttpResponse request failed with status code: {response.StatusCode}");
                    return;
                }

                string latest = (await response.Content.ReadAsStringAsync()).Trim();
                if (latest != Version) {
                    await Task.Delay(5000);
                    SendDebug($"Update available. Latest version is v{latest}");
                    await _notifier.SendMessageAsync(
                        $"Update available. Latest version is v{latest}. Download newest version at https://blt950.com",
                        $"{Name} Plugin");
                }
            } catch (Exception ex) {
                SendDebug($"[Update Checker] An HttpResponse error occurred: {ex.Message}");
            }
        }

    }
}
