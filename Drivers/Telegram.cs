using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Telegram : INotifier {

        private static readonly HttpClient _client = new HttpClient();

        private string _botToken;
        private string _chatId;

        public void Initialize(NotifierConfig config) {
            _botToken = config.TelegramBotToken;
            _chatId = config.TelegramChatId;
        }

        public bool HasValidConfig() {
            return _botToken != null && _chatId != null;
        }

        public async Task SendMessageAsync(string text, string title = "", int priority = 0) {
            string message = $"{title}\n\n{text}";
            string apiUrl = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var values = new Dictionary<string, string>
            {
                { "chat_id", _chatId },
                { "text", message }
            };

            using (var content = new FormUrlEncodedContent(values)) {
                var response = await _client.PostAsync(apiUrl, content);
                await response.Content.ReadAsStringAsync();
            }
        }

    }
}
