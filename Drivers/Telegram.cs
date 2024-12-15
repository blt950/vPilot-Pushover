using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Telegram : INotifier {

        // Init
        private static readonly HttpClient client = new HttpClient();
        private String settingTelegramBotToken = null;
        private String settingTelegramChatId = null;

        /*
         * 
         * Initilise the driver
         *
        */
        public void init( NotifierConfig config ) {
            this.settingTelegramBotToken = config.settingTelegramBotToken;
            this.settingTelegramChatId = config.settingTelegramChatId;
        }

        /*
         * 
         * Validate the configuration
         *
        */
        public Boolean hasValidConfig() {
            if (this.settingTelegramBotToken == null || this.settingTelegramChatId == null) {
                return false;
            }
            return true;
        }

        /*
         * 
         * Send Pushover message
         *
        */
        public async void sendMessage( String text, String title = "", int priority = 0 ) {

            // Construct the message for Telegram
            string telegramMessage = $"{title}\n\n{text}";
            // Prepare the Telegram API URL
            string telegramApiUrl = $"https://api.telegram.org/bot{settingTelegramBotToken}/sendMessage";

            // Create the form data for the POST request
            var values = new Dictionary<string, string>
            {
                { "chat_id", settingTelegramChatId },
                { "text", telegramMessage }
            };

            // Send the POST request to Telegram
            var response = await client.PostAsync(telegramApiUrl, new FormUrlEncodedContent(values));
            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
