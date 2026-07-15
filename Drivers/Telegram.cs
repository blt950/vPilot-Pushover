using System.Collections.Generic;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Telegram : HttpNotifierBase {

        private string _botToken;
        private string _chatId;

        protected override void Configure(NotifierConfig config) {
            _botToken = config.TelegramBotToken;
            _chatId = config.TelegramChatId;
        }

        public override bool HasValidConfig() {
            return !string.IsNullOrWhiteSpace(_botToken) && !string.IsNullOrWhiteSpace(_chatId);
        }

        public override async Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification") {
            var values = new Dictionary<string, string>
            {
                { "chat_id", _chatId },
                { "text", string.IsNullOrEmpty(title) ? text : $"{title}\n\n{text}" }
            };

            await PostFormAsync($"https://api.telegram.org/bot{_botToken}/sendMessage", values, source);
        }

    }
}
