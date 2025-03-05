using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers
{
    internal class Gotify : INotifier
    {

        // Init
        private static readonly HttpClient client = new HttpClient();
        private String settingGotifyUrl = null;
        private String settingGotifyToken = null;

        /*
         * 
         * Initilise the driver
         *
        */
        public void init(NotifierConfig config)
        {
            this.settingGotifyUrl = config.settingGotifyUrl;
            this.settingGotifyToken = config.settingGotifyToken;
        }

        /*
         * 
         * Validate the configuration
         *
        */
        public Boolean hasValidConfig()
        {
            if (this.settingGotifyUrl == null || this.settingGotifyToken == null)
            {
                return false;
            }
            return true;
        }

        /*
         * 
         * Send Pushover message
         *
        */

        public async void sendMessage(String text, String title = "", int priority = 0)
        {
            var values = new Dictionary<string, string>
            {
                { "title",  title },
                { "message", text },
                { "priority", priority.ToString() }
            };

            var response = await client.PostAsync(this.settingGotifyUrl + "/message?token=" + this.settingGotifyToken, new FormUrlEncodedContent(values));
            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
