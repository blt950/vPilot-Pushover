using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;

using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;

namespace vPilot_Pushover {
    public class Main : IPlugin {

        public static string version = "0.0.2";

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
        private Boolean settingSelcalEnabled = false;
        private Boolean settingHoppieEnabled = false;
        public String settingHoppieLogon = null;
        private String settingPushoverToken = null;
        private String settingPushoverUser = null;
        private String settingPushoverDevice = null;

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
                vPilot.NetworkDisconnected += OnNetworkDisconnectedHandler;
                if (settingPrivateEnabled) vPilot.PrivateMessageReceived += OnPrivateMessageReceivedHandler;
                if (settingRadioEnabled) vPilot.RadioMessageReceived += OnRadioMessageReceivedHandler;
                if (settingSelcalEnabled) vPilot.SelcalAlertReceived += OnSelcalAlertReceivedHandler;

                // Enable ACARS if Hoppie is enabled
                if (settingHoppieEnabled) {
                    acars = new Acars();
                    acars.init(this, settingHoppieLogon);
                }

                SendPushover($"Connected. Running version v{version}");
                SendDebug($"vPilot Pushover connected and enabled on v{version}");

                updateChecker();

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
        public async void SendPushover( String text, String title = "", int priority = 0 ) {

            var values = new Dictionary<string, string>
            {
                { "token", settingPushoverToken },
                { "user", settingPushoverUser },
                { "title",  title },
                { "message", text },
                { "priority", priority.ToString() },
                { "device", settingPushoverDevice != "" ? settingPushoverDevice : "" }
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

            if (settingHoppieEnabled) {
                acars.start();
            }
        }

        /*
         * 
         * Hook: Network disconnected
         *
        */
        private void OnNetworkDisconnectedHandler( object sender, EventArgs e ) {
            connectedCallsign = null;

            if (settingHoppieEnabled) {
                acars.stop();
            }
        }

        /*
         * 
         * Hook: Private Message
         *
        */
        private void OnPrivateMessageReceivedHandler( object sender, PrivateMessageReceivedEventArgs e ) {
            string from = e.From;
            string message = e.Message;

            SendPushover(message, from, 1);
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
                SendPushover(message, from, 1);
            }

        }

        /*
         * 
         * Hook: SELCAL Message
         *
        */
        private void OnSelcalAlertReceivedHandler( object sender, SelcalAlertReceivedEventArgs e ) {
            string from = e.From;
            SendPushover("SELCAL Alert", from, 1);
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

        /*
         * 
         * Check if is update available
         *
        */
        private async void updateChecker() {

            using (HttpClient httpClient = new HttpClient()) {
                try {
                    HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/blt950/vPilot-Pushover/main/version.txt");
                    if (response.IsSuccessStatusCode) {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent != ("v"+version)) {
                            await Task.Delay(5000);
                            SendDebug($"Update available. Latest version is {responseContent}");
                            SendPushover($"Update available. Latest version is {responseContent}. Download newest version at https://blt950.com", "vPilot Pushover Plugin");
                        }
                    } else {
                        SendDebug($"[Update Checker] HttpResponse request failed with status code: {response.StatusCode}");
                    }
                } catch (Exception ex) {
                    SendDebug($"[Update Checker] An HttpResponse error occurred: {ex.Message}");
                }
            }

        }


    }
}