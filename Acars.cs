using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;

namespace vPilot_Pushover {
    internal class Acars {

        public static List<Dictionary<string, string>> HoppieCache = new List<Dictionary<string, string>>();
        public static Boolean CacheLoaded = false;
        public static Main _main;

        public string _callsign;
        public string _logon;

        public void init( Main main, String logon ) {
            _main = main;
            _logon = logon;

            Timer hoppieTimer = new Timer();
            hoppieTimer.Elapsed += new ElapsedEventHandler(FetchHoppie);
            hoppieTimer.Interval = 45 * 1000;
            hoppieTimer.Enabled = true;

            // Run it manually once
            FetchHoppie(null, null);
        }

        public void SetCallsign( String callsign ) {
            _callsign = callsign;
        }

        async void FetchHoppie( object source, ElapsedEventArgs e ) {
            string baseUrl = "http://www.hoppie.nl/acars/system/connect.html";
            string logon = _logon;
            string from = _callsign;
            string type = "peek";
            string to = "SERVER";

            if (_callsign != null) {
                using (HttpClient httpClient = new HttpClient()) {
                    // Build the complete URL with GET variables
                    string fullUrl = $"{baseUrl}?logon={logon}&from={from}&type={type}&to={to}";
                    _main.SendDebug("[ACARS] Fetching Hoppie data");

                    try {
                        HttpResponseMessage response = await httpClient.GetAsync(fullUrl);
                        if (response.IsSuccessStatusCode) {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            ParseHoppie(responseContent);
                        } else {
                            _main.SendDebug($"[ACARS] HttpResponse request failed with status code: {response.StatusCode}");
                        }
                    } catch (Exception ex) {
                        _main.SendDebug($"[ACARS] An HttpResponse error occurred: {ex.Message}");
                    }
                }
            } else {
                _main.SendDebug($"[ACARS] FetchHoppie aborted due to missing callsign");
            }

        }

        void ParseHoppie( String response ) {

            Boolean statusOk = response.StartsWith("ok");

            if (statusOk) {
                foreach (Match match in Regex.Matches(response, @"\{(\d+)\s(\w+)\s(\w+)\s\{([^\}]+)\}\}")) {
                    // Initate the Regex groups
                    string key = match.Groups[1].Value;
                    string from = match.Groups[2].Value;
                    string type = match.Groups[3].Value;
                    string message = match.Groups[4].Value;

                    // Clean the messages
                    message = Regex.Replace(match.Groups[4].Value, @"\/data\d\/\d+\/\/.+\/", "");
                    message = Regex.Replace(message, @"@", "");

                    // Check if key doesnt' exist, then add it
                    if (!HoppieCache.Exists(x => x["key"] == key)) {
                        // Create a dictionary for the current block and add the key-value pairs
                        Dictionary<string, string> dataDict = new Dictionary<string, string>
                        {
                            { "key", key },
                            { "from", from },
                            { "type", type },
                            { "message", message}
                        };

                        // Add the dictionary to the list
                        HoppieCache.Add(dataDict);

                        // Send the message to Pushover
                        if (CacheLoaded == true && message != "") {
                            _main.SendPushover($"[{type.ToUpper()}] {from}: {message}");
                        }
                    }


                }

                CacheLoaded = true;

            } else {
                _main.SendDebug("[ACARS] okCheck Error: " + response);
            }

        }

    }
}
