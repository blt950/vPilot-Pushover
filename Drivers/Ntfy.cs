using System;
using System.Threading.Tasks;
namespace vPilot_Pushover.Drivers {
    internal class Ntfy : HttpNotifierBase {
        private string _url;
        private string _token;
        protected override void Configure(NotifierConfig config) {
            _url = config.NtfyUrl?.TrimEnd('/');
            _token = config.NtfyToken;
        }
        public override bool HasValidConfig() {
            return !string.IsNullOrWhiteSpace(_url);
        }
        public override async Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification") {
            var query = $"priority={MapPriority(priority)}";
            if (!string.IsNullOrWhiteSpace(title)) {
                query += $"&title={Uri.EscapeDataString(title)}";
            }
            await PostRawAsync($"{_url}?{query}", text, source, request => {
                if (!string.IsNullOrWhiteSpace(_token)) {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                }
            });
        }
        // Maps this project's Pushover-style priority (-2..2) onto ntfy's 1..5 scale.
        private static int MapPriority(int priority) {
            switch (priority) {
                case -2: return 1; // min
                case -1: return 2; // low
                case 0: return 3;  // default
                case 1: return 4;  // high
                case 2: return 5;  // max/urgent
                default: return 3;
            }
        }
    }
}