using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Win32;

using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;


namespace vPilot_Pushover {
    public class Main : IPlugin {
        private IBroker _broker;
        private static readonly HttpClient client = new HttpClient();
        public string Name => "vPilot Pushover";
        public string connectedCallsign = null;
        private Acars acars;

        // Settings
        private Boolean SettingsLoaded = false;
        private IniFile SettingsFile;

        //@TODO: Cast the booleans dammit
        private Boolean settingPrivateEnabled = false;
        private Boolean settingRadioEnabled = false;
        private Boolean settingHoppieEnabled = false;
        private String settingHoppieLogon = null;
        private String settingPushoverToken = null;
        private String settingPushoverUser = null;

        public void Initialize( IBroker broker ) {
            _broker = broker;
            LoadSettings();

            if (SettingsLoaded) {
                // Subscribe to events according to settings
                _broker.NetworkConnected += OnNetworkConnectedHandler;
                if (settingPrivateEnabled) _broker.PrivateMessageReceived += OnPrivateMessageReceivedHandler;
                if (settingRadioEnabled) _broker.RadioMessageReceived += OnRadioMessageReceivedHandler;

                // Enable ACARS if Hoppie is enabled
                if (settingHoppieEnabled) {
                    acars = new Acars();
                    acars.init(this, settingHoppieLogon);
                }

                SendPushover("Connected");
                SendDebug("Connected");

            } else {
                SendDebug("vPilot Pushover plugin failed to load. Check your vPilot-Pushover.ini");
            }

        }

        public void SendDebug( String text ) {
            _broker.PostDebugMessage(text);
        }

        public async void SendPushover( String text ) {

            var values = new Dictionary<string, string>
            {
                { "token", settingPushoverToken },
                { "user", settingPushoverUser },
                { "message", text }
            };

            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new FormUrlEncodedContent(values));

            var responseString = await response.Content.ReadAsStringAsync();
            _broker.PostDebugMessage(responseString);

        }

        private void OnNetworkConnectedHandler( object sender, NetworkConnectedEventArgs e ) {
            connectedCallsign = e.Callsign;
            acars.SetCallsign(e.Callsign);
            _broker.PostDebugMessage($"VATSIM connected with callsign: {e.Callsign}");
        }

        private void OnPrivateMessageReceivedHandler( object sender, PrivateMessageReceivedEventArgs e ) {
            string from = e.From;
            string message = e.Message;

            SendPushover($"{from}: {message}");
        }

        private void OnRadioMessageReceivedHandler( object sender, RadioMessageReceivedEventArgs e ) {
            string from = e.From;
            string message = e.Message;

            if (message.Contains(connectedCallsign)) {
                SendPushover($"{from}: {message}");
            }

        }

        private void LoadSettings() {

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\vPilot");
            if (registryKey != null) {
                string vPilotPath = (string)registryKey.GetValue("Install_Dir");
                string configFile = vPilotPath + "\\Plugins\\vPilot-Pushover.ini";
                SettingsFile = new IniFile(configFile);

                // Set all values
                settingPushoverToken = SettingsFile.Read("ApiKey", "Pushover");
                settingPushoverUser = SettingsFile.Read("UserKey", "Pushover");
                settingHoppieEnabled = Boolean.Parse(SettingsFile.Read("Enabled", "Hoppie"));
                settingHoppieLogon = SettingsFile.Read("LogonCode", "Hoppie");
                settingPrivateEnabled = Boolean.Parse(SettingsFile.Read("Enabled", "RelayPrivate"));
                settingRadioEnabled = Boolean.Parse(SettingsFile.Read("Enabled", "RelayRadio"));

                // Validate values
                if (settingPushoverToken == null || settingPushoverUser == null) {
                    SendDebug("Pushover API key or user key not set. Check your vPilot-Pushover.ini");
                }

                if (settingHoppieEnabled && settingHoppieLogon == null) {
                    SendDebug("Hoppie logon code not set. Check your vPilot-Pushover.ini");
                }

                // Debug print
                SendDebug($"PushoverToken: {SettingsFile.Read("ApiKey", "Pushover")}");
                SendDebug($"PushoverUser: {SettingsFile.Read("UserKey", "Pushover")}");
                SendDebug($"HoppieEnabled: {SettingsFile.Read("Enabled", "Hoppie")}");
                SendDebug($"HoppieLogon: {SettingsFile.Read("LogonCode", "Hoppie")}");
                SendDebug($"PrivateEnabled: {SettingsFile.Read("Enabled", "RelayPrivate")}");
                SendDebug($"RadioEnabled: {SettingsFile.Read("Enabled", "RelayRadio")}");

                SettingsLoaded = true;

            } else {
                SendDebug("Registry key not found. Is vPilot installed correctly?");
            }

        }

    }
}