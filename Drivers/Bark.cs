using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace vPilot_Pushover.Drivers {
    internal class Bark : HttpNotifierBase {

        private string _url;
        private string _key;
        private string _notificationGroup;

        protected override void Configure(NotifierConfig config) {
            _url = config.BarkUrl;
            _key = config.BarkKey;
            _notificationGroup = config.BarkNotificationGroup;
        }

        public override bool HasValidConfig() {
            return !string.IsNullOrWhiteSpace(_url) && !string.IsNullOrWhiteSpace(_key);
        }

        public override async Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification") {
            // Bark notification priority levels, obtained from Bark docs:
            // critical: Important alert, will ring even in silent mode
            // timeSensitiv: Time-sensitive notification, can display the notification in focus mode.
            // active: Default value, the system will immediately light up the screen to display the notification
            // passive: Only adds the notification to the notification list, will not light up the screen.

            string level = priority == -1 ? "passive"
                         : priority ==  1 ? "timeSensitive"
                         : priority ==  2 ? "critical"
                         : "active"; // defaults to "active", including priority = 0

            // Bark Request Parameters, see Bark docs for details:
            // https://bark.day.app/#/en-us/tutorial?id=request-parameters
            var values = new Dictionary<string, string> {
                { "title", title },
                { "body", text },
                { "level", level },
                { "group", _notificationGroup } // Groups notifications in iOS notification centre
            };

            // Prevents double-slashes if user leaves a trailing slash in the config URL
            await PostFormAsync($"{_url.TrimEnd('/')}/{_key}", values, source);
        }

        // Parse error text to improve user readability
        protected override string ExtractErrorDetail(string body) {
            // Ensure error message is not null, and trim whitespace
            string error = (body ?? "").Trim();
            
            // Extract the JSON "message" field if it exists
            var match = Regex.Match(error, "\"message\"\\s*:\\s*\"([^\"]*)\"");
            if (match.Success) error = match.Groups[1].Value ;

            if (error.Contains("failed to get device token")) {
                return $"The Bark server rejected your API Key ({_key}). Please verify your Bark API Key is correctly configured in vPilot-Pushover.ini.";
            }

            // Handle edge cases where API server is incorrectly configured
            // 1. Empty response
            if (error == "" || error == "{}") {
                return "The server returned an empty response. Please verify your Bark API URL is correctly configured in vPilot-Pushover.ini.";
            }

            // 2. HTML web pages, e.g. <!DOCTYPE html>
            if (error.StartsWith("<")) {
                return "The server returned a web page instead of an API response. Please verify your Bark API URL is correctly configured in vPilot-Pushover.ini.";
            }

            return error;
        }
    }
}
