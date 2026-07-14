using System.Collections.Generic;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Gotify : HttpNotifierBase {

        private string _url;
        private string _token;

        protected override void Configure(NotifierConfig config) {
            _url = config.GotifyUrl;
            _token = config.GotifyToken;
        }

        public override bool HasValidConfig() {
            return !string.IsNullOrWhiteSpace(_url) && !string.IsNullOrWhiteSpace(_token);
        }

        public override async Task SendMessageAsync(string text, string title = "", int priority = 0) {
            var values = new Dictionary<string, string>
            {
                { "title", title },
                { "message", text },
                { "priority", priority.ToString() }
            };

            await PostFormAsync($"{_url}/message?token={_token}", values);
        }

    }
}
