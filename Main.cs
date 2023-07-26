using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Win32;

using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;

namespace vPilot_Pushover {
    public class Main : IPlugin {

        public static string version = "0.0.1";

        // Init
        private IBroker vPilot;
        private Acars acars;
        private static readonly HttpClient client = new HttpClient();

        public string Name { get; } = "vPilot Pushover";

        // Public variables
        public string connectedCallsign = null;

        // Settings
        private Boolean settingsLoaded = false;
        private IniFile settingsFile;

        private Boolean settingPrivateEnabled = false;
        private Boolean settingRadioEnabled = false;
        private Boolean settingHoppieEnabled = false;
        public String settingHoppieLogon = null;
        private String settingPushoverToken = null;
        private String settingPushoverUser = null;

        /*
         * 
         * Initilise the plugin
         *
        */
        public void Initialize( IBroker broker ) {
            vPilot = broker;
            LoadSettings();

            if (settingsLoaded) {
                // Subscribe to events according to settings
                vPilot.NetworkConnected += OnNetworkConnectedHandler;
                if (settingPrivateEnabled) vPilot.PrivateMessageReceived += OnPrivateMessageReceivedHandler;
                if (settingRadioEnabled) vPilot.RadioMessageReceived += OnRadioMessageReceivedHandler;

                // Enable ACARS if Hoppie is enabled
                if (settingHoppieEnabled) {
                    acars = new Acars();
                    acars.init(this, settingHoppieLogon);
                }

                SendPushover("Connected");
                SendDebug($"vPilot Pushover connected and enabled on v.{version}");

            } else {
                SendDebug("vPilot Pushover plugin failed to load. Check your vPilot-Pushover.ini");
            }

        }

        /*
         * 
         * Send debug message to vPilot
         *
        */
        public void SendDebug( String text ) {
            vPilot.PostDebugMessage(text);
        }

        /*
         * 
         * Send Pushover message
         *
        */
        public async void SendPushover( String text ) {

            var values = new Dictionary<string, string>
            {
                { "token", settingPushoverToken },
                { "user", settingPushoverUser },
                { "message", text }
            };

            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new FormUrlEncodedContent(values));
            var responseString = await response.Content.ReadAsStringAsync();

        }

        /*
         * 
         * Hook: Network connected
         *
        */
        private void OnNetworkConnectedHandler( object sender, NetworkConnectedEventArgs e ) {
            connectedCallsign = e.Callsign;
        }

        /*
         * 
         * Hook: Private Message
         *
        */
        private void OnPrivateMessageReceivedHandler( object sender, PrivateMessageReceivedEventArgs e ) {
            string from = e.From;
            string message = e.Message;

            SendPushover($"{from}: {message}");
        }

        /*
         * 
         * Hook: Radio Message
         *
        */
        private void OnRadioMessageReceivedHandler( object sender, RadioMessageReceivedEventArgs e ) {
            string from = e.From;
            string message = e.Message;

            if (message.Contains(connectedCallsign)) {
                SendPushover($"{from}: {message}");
            }

        }

        /*
         * 
         * Load plugin settings
         *
        */
        private void LoadSettings() {

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\vPilot");
            if (registryKey != null) {
                string vPilotPath = (string)registryKey.GetValue("Install_Dir");
                string configFile = vPilotPath + "\\Plugins\\vPilot-Pushover.ini";
                settingsFile = new IniFile(configFile);

                // Set all values
                settingPushoverToken = settingsFile.Read("ApiKey", "Pushover");
                settingPushoverUser = settingsFile.Read("UserKey", "Pushover");
                settingHoppieEnabled = Boolean.Parse(settingsFile.Read("Enabled", "Hoppie"));
                settingHoppieLogon = settingsFile.Read("LogonCode", "Hoppie");
                settingPrivateEnabled = Boolean.Parse(settingsFile.Read("Enabled", "RelayPrivate"));
                settingRadioEnabled = Boolean.Parse(settingsFile.Read("Enabled", "RelayRadio"));

                // Validate values
                if (settingPushoverToken == null || settingPushoverUser == null) {
                    SendDebug("Pushover API key or user key not set. Check your vPilot-Pushover.ini");
                }

                if (settingHoppieEnabled && settingHoppieLogon == null) {
                    SendDebug("Hoppie logon code not set. Check your vPilot-Pushover.ini");
                }

                // Debug print
                SendDebug($"PushoverToken: {settingsFile.Read("ApiKey", "Pushover")}");
                SendDebug($"PushoverUser: {settingsFile.Read("UserKey", "Pushover")}");
                SendDebug($"HoppieEnabled: {settingsFile.Read("Enabled", "Hoppie")}");
                SendDebug($"HoppieLogon: {settingsFile.Read("LogonCode", "Hoppie")}");
                SendDebug($"PrivateEnabled: {settingsFile.Read("Enabled", "RelayPrivate")}");
                SendDebug($"RadioEnabled: {settingsFile.Read("Enabled", "RelayRadio")}");

                settingsLoaded = true;

            } else {
                SendDebug("Registry key not found. Is vPilot installed correctly?");
            }

        }

    }
}