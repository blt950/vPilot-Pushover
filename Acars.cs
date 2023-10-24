using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;

namespace vPilot_Pushover {
    internal class Acars {

        // Init
        private static Main Plugin;
        private static List<Dictionary<string, string>> hoppieCache = new List<Dictionary<string, string>>();
        private static Boolean cacheLoaded = false;

        private readonly Timer hoppieTimer = new Timer();

        /*
         * 
         * Initilise the ACARS
         *
        */
        public void init( Main main, String logon ) {
            Plugin = main;

            hoppieTimer.Elapsed += new ElapsedEventHandler(fetchHoppie);
            hoppieTimer.Interval = 45 * 1000;

        }

        /*
         * 
         * Start the ACARS
         *
        */
        public void start() {
            hoppieTimer.Enabled = true;
            Plugin.sendDebug("[ACARS] Starting ACARS");
            fetchHoppie(null, null);
        }

        /*
         * 
         * Stop the ACARS
         *
        */
        public void stop() {
            hoppieTimer.Enabled = false;
            Plugin.sendDebug("[ACARS] Stopping ACARS");
        }

        /*
         * 
         * Fetch data from Hoppie API
         *
        */
        private async void fetchHoppie( object source, ElapsedEventArgs e ) {
            string baseUrl = "http://www.hoppie.nl/acars/system/connect.html";
            string logon = Plugin.settingHoppieLogon;
            string from = Plugin.connectedCallsign;
            string type = "peek";
            string to = "SERVER";

            if (Plugin.connectedCallsign != null) {
                using (HttpClient httpClient = new HttpClient()) {

                    // Build the complete URL with GET variables
                    string fullUrl = $"{baseUrl}?logon={logon}&from={from}&type={type}&to={to}";
                    Plugin.sendDebug($"[ACARS] Fetching Hoppie data with callsign {from}");

                    try {
                        HttpResponseMessage response = await httpClient.GetAsync(fullUrl);
                        if (response.IsSuccessStatusCode) {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            parseHoppie(responseContent);
                        } else {
                            Plugin.sendDebug($"[ACARS] HttpResponse request failed with status code: {response.StatusCode}");
                        }
                    } catch (Exception ex) {
                        Plugin.sendDebug($"[ACARS] An HttpResponse error occurred: {ex.Message}");
                    }

                }
            } else {
                Plugin.sendDebug($"[ACARS] fetchHoppie aborted due to missing callsign");
            }

        }

        /*
         * 
         * Parse the Hoppie response
         *
        */
        private void parseHoppie( String response ) {

            Boolean statusOk = response.StartsWith("ok");

            if (statusOk) {
                foreach (Match match in Regex.Matches(response, @"\{(\d+)\s(\w+)\s(\w+)\s\{([^\}]+)\}\}")) {
                    // Map the Regex groups
                    string key = match.Groups[1].Value;
                    string from = match.Groups[2].Value;
                    string type = match.Groups[3].Value;
                    string message = match.Groups[4].Value;

                    // Clean the messages
                    message = Regex.Replace(match.Groups[4].Value, @"\/data\d\/\d+\/\d*\/.+\/", "");
                    message = Regex.Replace(message, @"@", "");

                    // Check if key doesnt' exist, then add it
                    if (!hoppieCache.Exists(x => x["key"] == key)) {
                        // Create a dictionary for the current block and add the key-value pairs
                        Dictionary<string, string> dataDict = new Dictionary<string, string>
                        {
                            { "key", key },
                            { "from", from },
                            { "type", type },
                            { "message", message}
                        };

                        // Add the dictionary to the list
                        hoppieCache.Add(dataDict);

                        // Send the message to Pushover
                        if (cacheLoaded == true && message != "") {
                            Plugin.sendPushover(message, $"{from} ({type.ToUpper()})");
                        }

                        Plugin.sendDebug($"[ACARS] Cached {key} with message: {message}");

                    }


                }

                cacheLoaded = true;

            } else {
                Plugin.sendDebug("[ACARS] okCheck Error: " + response);
            }

        }

    }
}
