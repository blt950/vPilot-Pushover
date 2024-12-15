using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Pushover : INotifier {

        // Init
        private static readonly HttpClient client = new HttpClient();
        private String settingPushoverToken = null;
        private String settingPushoverUser = null;
        private String settingPushoverDevice = null;

        /*
         * 
         * Initilise the driver
         *
        */
        public void init( NotifierConfig config ) {
            this.settingPushoverToken = config.settingPushoverToken;
            this.settingPushoverUser = config.settingPushoverUser;
            this.settingPushoverDevice = config.settingPushoverDevice;
        }

        /*
         * 
         * Validate the configuration
         *
        */
        public Boolean hasValidConfig() {
            if (this.settingPushoverToken == null || this.settingPushoverUser == null) {
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
            var values = new Dictionary<string, string>
            {
                { "token", this.settingPushoverToken },
                { "user", this.settingPushoverUser },
                { "title",  title },
                { "message", text },
                { "priority", priority.ToString() },
                { "device", this.settingPushoverDevice != "" ? this.settingPushoverDevice : "" }
            };

            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new FormUrlEncodedContent(values));
            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
